using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using Newtonsoft.Json.Linq;
using SwarmUI.Accounts;
using SwarmUI.Core;
using SwarmUI.Media;
using SwarmUI.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using SwarmUI.Text2Image;

namespace SwarmUI.WebAPI;

[API.APIClass("API routes related to image history and image management.")]
public static class ImageAPI
{
    public static void Register()
    {
        API.RegisterAPICall(AddImageToHistory, true, Permissions.BasicImageGeneration);
        API.RegisterAPICall(ListImages, false, Permissions.ViewImageHistory);
        API.RegisterAPICall(ToggleImageStarred, true, Permissions.UserStarImages);
        API.RegisterAPICall(OpenImageFolder, true, Permissions.LocalImageFolder);
        API.RegisterAPICall(DeleteImage, true, Permissions.UserDeleteImage);
    }

    [API.APIDescription("Takes an image and stores it directly in the user's history.\nBehaves identical to GenerateText2Image but never queues a generation.",
        """
            "images":
            [
                {
                    "image": "View/local/raw/2024-01-02/0304-a photo of a cat-etc-1.png", // the image file path, GET this path to read the image content
                    "batch_index": "0", // which image index within the batch this is
                    "metadata": "{ ... }" // image metadata string, usually a JSON blob stringified. Not guaranteed to be.
                }
            ]
        """)]
    public static async Task<JObject> AddImageToHistory(Session session,
        [API.APIParameter("Data URL of the image to save.")] string image,
        [API.APIParameter("Raw mapping of input should contain general T2I parameters (see listing on Generate tab of main interface) to values, eg `{ \"prompt\": \"a photo of a cat\", \"model\": \"OfficialStableDiffusion/sd_xl_base_1.0\", \"steps\": 20, ... }`. Note that this is the root raw map, ie all params go on the same level as `images`, `session_id`, etc.")] JObject rawInput)
    {
        ImageFile img = ImageFile.FromDataString(image);
        T2IParamInput user_input;
        rawInput.Remove("image");
        try
        {
            user_input = T2IAPI.RequestToParams(session, rawInput);
        }
        catch (SwarmReadableErrorException ex)
        {
            return new() { ["error"] = ex.Message };
        }
        user_input.ApplySpecialLogic();
        Logs.Info($"User {session.User.UserID} stored an image to history.");
        (Task<MediaFile> imgTask, string metadata) = user_input.SourceSession.ApplyMetadata(img, user_input, 1);
        T2IEngine.ImageOutput outputImage = new() { File = img as Image, ActualFileTask = imgTask };
        (string path, _) = session.SaveImage(outputImage, 0, user_input, metadata);
        return new() { ["images"] = new JArray() { new JObject() { ["image"] = path, ["batch_index"] = "0", ["request_id"] = $"{user_input.UserRequestId}", ["metadata"] = metadata } } };
    }

    public static HashSet<string> HistoryExtensions = [.. MediaType.TypesByExtension.Keys, "html"];

    public enum ImageHistorySortMode { Name, Date }

    private static JObject GetListAPIInternal(Session session, string rawPath, string root, HashSet<string> extensions, Func<string, bool> isAllowed, int depth, ImageHistorySortMode sortBy, bool sortReverse)
    {
        int maxInHistory = session.User.Settings.MaxImagesInHistory;
        int maxScanned = session.User.Settings.MaxImagesScannedInHistory;
        Logs.Verbose($"User {session.User.UserID} wants to list images in '{rawPath}', maxDepth={depth}, sortBy={sortBy}, reverse={sortReverse}, maxInHistory={maxInHistory}, maxScanned={maxScanned}");
        long timeStart = Environment.TickCount64;
        int limit = sortBy == ImageHistorySortMode.Name ? maxInHistory : Math.Max(maxInHistory, maxScanned);
        (string path, string consoleError, string userError) = WebServer.CheckFilePath(root, rawPath);
        path = UserImageHistoryHelper.GetRealPathFor(session.User, path, root: root);
        if (consoleError is not null)
        {
            Logs.Error(consoleError);
            return new JObject() { ["error"] = userError };
        }
        try
        {
            ConcurrentDictionary<string, string> dirsConc = [];
            ConcurrentDictionary<string, string> finalDirs = [];
            ConcurrentDictionary<string, Task> tasks = [];
            void addDirs(string dir, int subDepth)
            {
                tasks.TryAdd(dir, Utilities.RunCheckedTask(() =>
                {
                    if (dir.EndsWith('/'))
                    {
                        dir = dir[..^1];
                    }
                    if (dir != "")
                    {
                        (subDepth == 0 ? finalDirs : dirsConc).TryAdd(dir, dir);
                    }
                    if (subDepth > 0)
                    {
                        string actualPath = $"{path}/{dir}";
                        actualPath = UserImageHistoryHelper.GetRealPathFor(session.User, actualPath, root: root);
                        if (!Directory.Exists(actualPath))
                        {
                            return;
                        }
                        IEnumerable<string> subDirs = Directory.EnumerateDirectories(actualPath).Select(Path.GetFileName).OrderDescending();
                        foreach (string subDir in subDirs)
                        {
                            string subPath = dir == "" ? subDir : $"{dir}/{subDir}";
                            if (isAllowed(subPath))
                            {
                                addDirs(subPath, subDepth - 1);
                            }
                        }
                    }
                }, "t2i getlist add dir"));
            }
            addDirs("", depth);
            string rawRefPath = Path.GetRelativePath(root, path).Replace('\\', '/');
            if (!rawRefPath.EndsWith('/'))
            {
                rawRefPath += '/';
            }
            if (rawRefPath == "./")
            {
                rawRefPath = "";
            }
            foreach (string specialFolder in UserImageHistoryHelper.SharedSpecialFolders.Keys)
            {
                if (specialFolder.StartsWith(rawRefPath))
                {
                    addDirs(specialFolder[rawRefPath.Length..], 1);
                }
            }
            while (tasks.Any(t => !t.Value.IsCompleted))
            {
                Task.WaitAll([.. tasks.Values]);
            }
            List<string> dirs = [.. dirsConc.Keys.OrderDescending()];
            if (sortReverse)
            {
                dirs.Reverse();
            }
            ConcurrentDictionary<int, List<ImageHistoryHelper>> filesConc = [];
            bool starNoFolders = session.User.Settings.StarNoFolders;
            int id = 0;
            int remaining = limit;
            void sortList(List<ImageHistoryHelper> list)
            {
                if (sortBy == ImageHistorySortMode.Name)
                {
                    list.Sort((a, b) => b.Name.CompareTo(a.Name));
                }
                else if (sortBy == ImageHistorySortMode.Date)
                {
                    list.Sort((a, b) => b.Metadata.FileTime.CompareTo(a.Metadata.FileTime));
                }
                if (sortReverse)
                {
                    list.Reverse();
                }
            }
            Parallel.ForEach(dirs.Append(""), new ParallelOptions() { MaxDegreeOfParallelism = 5, CancellationToken = Program.GlobalProgramCancel }, folder =>
            {
                int localId = Interlocked.Increment(ref id);
                int localLimit = Interlocked.CompareExchange(ref remaining, 0, 0);
                if (localLimit <= 0)
                {
                    return;
                }
                string prefix = folder == "" ? "" : folder + "/";
                string actualPath = $"{path}/{prefix}";
                actualPath = UserImageHistoryHelper.GetRealPathFor(session.User, actualPath, root: root);
                if (!Directory.Exists(actualPath))
                {
                    return;
                }
                List<string> subFiles = [.. Directory.EnumerateFiles(actualPath).Take(localLimit)];
                IEnumerable<string> newFileNames = subFiles.Where(isAllowed).Where(f => extensions.Contains(f.AfterLast('.')) && !f.EndsWith(".swarmpreview.jpg") && !f.EndsWith(".swarmpreview.webp")).Select(f => f.Replace('\\', '/'));
                List<ImageHistoryHelper> localFiles = [.. newFileNames.Select(f => new ImageHistoryHelper(prefix + f.AfterLast('/'), OutputMetadataTracker.GetMetadataFor(f, root, starNoFolders))).Where(f => f.Metadata is not null)];
                int leftOver = Interlocked.Add(ref remaining, -localFiles.Count);
                sortList(localFiles);
                filesConc.TryAdd(localId, localFiles);
                if (leftOver <= 0)
                {
                    return;
                }
            });
            List<ImageHistoryHelper> files = [.. filesConc.Values.SelectMany(f => f).Take(limit)];
            HashSet<string> included = [.. files.Select(f => f.Name)];
            for (int i = 0; i < files.Count; i++)
            {
                if (!files[i].Name.StartsWith("Starred/"))
                {
                    string starPath = $"Starred/{(session.User.Settings.StarNoFolders ? files[i].Name.Replace("/", "") : files[i].Name)}";
                    if (included.Contains(starPath))
                    {
                        files[i] = files[i] with { Name = null };
                    }
                }
            }
            files = [.. files.Where(f => f.Name is not null)];
            sortList(files);
            long timeEnd = Environment.TickCount64;
            Logs.Verbose($"Listed {files.Count} images in {(timeEnd - timeStart) / 1000.0:0.###} seconds.");
            return new JObject()
            {
                ["folders"] = JToken.FromObject(dirs.Union(finalDirs.Keys).ToList()),
                ["files"] = JToken.FromObject(files.Take(maxInHistory).Select(f => new JObject() { ["src"] = f.Name, ["metadata"] = f.Metadata.Metadata }).ToList())
            };
        }
        catch (Exception ex)
        {
            if (ex is FileNotFoundException || ex is DirectoryNotFoundException || ex is PathTooLongException)
            {
                return new JObject() { ["error"] = "404, path not found." };
            }
            else
            {
                Logs.Error($"Error reading file list: {ex.ReadableString()}");
                return new JObject() { ["error"] = "Error reading file list." };
            }
        }
    }

    public record struct ImageHistoryHelper(string Name, OutputMetadataTracker.OutputMetadataEntry Metadata);

    [API.APIDescription("Gets a list of images in a saved image history folder.",
        """
            "folders": ["Folder1", "Folder2"],
            "files":
            [
                {
                    "src": "path/to/image.jpg",
                    "metadata": "some-metadata" // usually a JSON blob encoded as a string. Not guaranteed.
                }
            ]
        """)]
    public static async Task<JObject> ListImages(Session session,
        [API.APIParameter("The folder path to start the listing in. Use an empty string for root.")] string path,
        [API.APIParameter("Maximum depth (number of recursive folders) to search.")] int depth,
        [API.APIParameter("What to sort the list by - `Name` or `Date`.")] string sortBy = "Name",
        [API.APIParameter("If true, the sorting should be done in reverse.")] bool sortReverse = false)
    {
        if (!Enum.TryParse(sortBy, true, out ImageHistorySortMode sortMode))
        {
            return new JObject() { ["error"] = $"Invalid sort mode '{sortBy}'." };
        }
        string root = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, session.User.OutputDirectory);
        return GetListAPIInternal(session, path, root, HistoryExtensions, f => true, depth, sortMode, sortReverse);
    }

    [API.APIDescription("Open an image folder in the file explorer. Used for local users directly.", "\"success\": true")]
    public static async Task<JObject> OpenImageFolder(Session session,
        [API.APIParameter("The path to the image to show in the image folder.")] string path)
    {
        string origPath = path;
        string root = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, session.User.OutputDirectory);
        (path, string consoleError, string userError) = WebServer.CheckFilePath(root, path);
        if (consoleError is not null)
        {
            Logs.Error(consoleError);
            return new JObject() { ["error"] = userError };
        }
        path = UserImageHistoryHelper.GetRealPathFor(session.User, path, root: root);
        if (!File.Exists(path))
        {
            Logs.Warning($"User {session.User.UserID} tried to open image path '{origPath}' which maps to '{path}', but cannot as the image does not exist.");
            return new JObject() { ["error"] = "That file does not exist, cannot open." };
        }
        ProcessStartInfo psi = new();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            psi.FileName = "explorer.exe";
            psi.ArgumentList.Add($"/select,{Path.GetFullPath(path)}");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            psi.FileName = "xdg-open";
            psi.ArgumentList.Add(Path.GetDirectoryName(Path.GetFullPath(path)));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            psi.FileName = "open";
            psi.ArgumentList.Add("-R");
            psi.ArgumentList.Add(Path.GetFullPath(path));
        }
        else
        {
            Logs.Warning("Cannot open image path on unrecognized OS type.");
            return new JObject() { ["error"] = "Cannot open image folder on this OS." };
        }
        Process.Start(psi);
        return new JObject() { ["success"] = true };
    }

    public static string[] DeletableFileExtensions = [".txt", ".metadata.js", ".swarm.json", ".swarmpreview.jpg", ".swarmpreview.webp"];

    [API.APIDescription("Delete an image from history.", "\"success\": true")]
    public static async Task<JObject> DeleteImage(Session session,
        [API.APIParameter("The path to the image to delete.")] string path)
    {
        string origPath = path;
        string root = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, session.User.OutputDirectory);
        (path, string consoleError, string userError) = WebServer.CheckFilePath(root, path);
        if (consoleError is not null)
        {
            Logs.Error(consoleError);
            return new JObject() { ["error"] = userError };
        }
        path = UserImageHistoryHelper.GetRealPathFor(session.User, path, root: root);
        if (!File.Exists(path))
        {
            Logs.Warning($"User {session.User.UserID} tried to delete image path '{origPath}' which maps to '{path}', but cannot as the image does not exist.");
            return new JObject() { ["error"] = "That file does not exist, cannot delete." };
        }
        string standardizedPath = Path.GetFullPath(path);
        Session.RecentlyBlockedFilenames[standardizedPath] = standardizedPath;
        Action<string> deleteFile = Program.ServerSettings.Paths.RecycleDeletedImages ? Utilities.SendFileToRecycle : File.Delete;
        deleteFile(path);
        string fileBase = path.BeforeLast('.');
        foreach (string str in DeletableFileExtensions)
        {
            string altFile = $"{fileBase}{str}";
            if (File.Exists(altFile))
            {
                deleteFile(altFile);
            }
        }
        OutputMetadataTracker.RemoveMetadataFor(path);
        return new JObject() { ["success"] = true };
    }

    [API.APIDescription("Toggle whether an image is starred or not.", "\"new_state\": true")]
    public static async Task<JObject> ToggleImageStarred(Session session,
        [API.APIParameter("The path to the image to star.")] string path)
    {
        bool wasStar = false;
        path = path.Replace('\\', '/').Trim('/');
        if (path.StartsWith("Starred/"))
        {
            wasStar = true;
            path = path["Starred/".Length..];
        }
        string origPath = path;
        string root = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, session.User.OutputDirectory);
        (path, string consoleError, string userError) = WebServer.CheckFilePath(root, path);
        if (consoleError is not null)
        {
            Logs.Error(consoleError);
            return new JObject() { ["error"] = userError };
        }
        path = UserImageHistoryHelper.GetRealPathFor(session.User, path, root: root);
        string pathBeforeDot = path.BeforeLast('.');
        string starPath = $"Starred/{(session.User.Settings.StarNoFolders ? origPath.Replace("/", "") : origPath)}";
        (starPath, _, _) = WebServer.CheckFilePath(root, starPath);
        starPath = UserImageHistoryHelper.GetRealPathFor(session.User, starPath, root: root);
        string starBeforeDot = starPath.BeforeLast('.');
        if (!File.Exists(path))
        {
            if (wasStar && File.Exists(starPath))
            {
                Logs.Debug($"User {session.User.UserID} un-starred '{path}' without a raw, moving back to raw");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.Move(starPath, path);
                foreach (string ext in DeletableFileExtensions)
                {
                    if (File.Exists($"{starBeforeDot}{ext}"))
                    {
                        File.Move($"{starBeforeDot}{ext}", $"{pathBeforeDot}{ext}");
                    }
                }
                OutputMetadataTracker.RemoveMetadataFor(path);
                OutputMetadataTracker.RemoveMetadataFor(starPath);
                return new JObject() { ["new_state"] = false };
            }
            Logs.Warning($"User {session.User.UserID} tried to star image path '{origPath}' which maps to '{path}', but cannot as the image does not exist.");
            return new JObject() { ["error"] = "That file does not exist, cannot star." };
        }
        if (File.Exists(starPath))
        {
            Logs.Debug($"User {session.User.UserID} un-starred '{path}'");
            File.Delete(starPath);
            foreach (string ext in DeletableFileExtensions)
            {
                if (File.Exists($"{starBeforeDot}{ext}"))
                {
                    File.Delete($"{starBeforeDot}{ext}");
                }
            }
            OutputMetadataTracker.RemoveMetadataFor(path);
            OutputMetadataTracker.RemoveMetadataFor(starPath);
            return new JObject() { ["new_state"] = false };
        }
        else
        {
            Logs.Debug($"User {session.User.UserID} starred '{path}'");
            Directory.CreateDirectory(Path.GetDirectoryName(starPath));
            File.Copy(path, starPath);
            foreach (string ext in DeletableFileExtensions)
            {
                if (File.Exists($"{pathBeforeDot}{ext}"))
                {
                    File.Copy($"{pathBeforeDot}{ext}", $"{starBeforeDot}{ext}");
                }
            }
            OutputMetadataTracker.RemoveMetadataFor(path);
            OutputMetadataTracker.RemoveMetadataFor(starPath);
            return new JObject() { ["new_state"] = true };
        }
    }


}