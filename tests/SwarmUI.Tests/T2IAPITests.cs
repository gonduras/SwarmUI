using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Accounts;
using System.Linq;

namespace SwarmUI.Tests;

public class T2IAPITests
{
    [Fact]
    public void TestRegister_RegistersExpectedEndpoints()
    {
        // Act
        T2IAPI.Register();

        // Assert
        Assert.True(API.APIHandlers.ContainsKey("generatetext2image"));
        Assert.True(API.APIHandlers.ContainsKey("generatetext2imagews"));
        Assert.True(API.APIHandlers.ContainsKey("addimagetohistory"));
        Assert.True(API.APIHandlers.ContainsKey("listimages"));
        Assert.True(API.APIHandlers.ContainsKey("toggleimagestarred"));
        Assert.True(API.APIHandlers.ContainsKey("openimagefolder"));
        Assert.True(API.APIHandlers.ContainsKey("deleteimage"));
        Assert.True(API.APIHandlers.ContainsKey("listt2iparams"));
        Assert.True(API.APIHandlers.ContainsKey("triggerrefresh"));

        // Verify some properties of the registered endpoints
        Assert.Equal(Permissions.BasicImageGeneration, API.APIHandlers["generatetext2image"].Permission);
        Assert.Equal(Permissions.BasicImageGeneration, API.APIHandlers["generatetext2imagews"].Permission);
        Assert.Equal(Permissions.BasicImageGeneration, API.APIHandlers["addimagetohistory"].Permission);
        Assert.Equal(Permissions.ViewImageHistory, API.APIHandlers["listimages"].Permission);
        Assert.Equal(Permissions.UserStarImages, API.APIHandlers["toggleimagestarred"].Permission);
        Assert.Equal(Permissions.LocalImageFolder, API.APIHandlers["openimagefolder"].Permission);
        Assert.Equal(Permissions.UserDeleteImage, API.APIHandlers["deleteimage"].Permission);
        Assert.Equal(Permissions.FundamentalGenerateTabAccess, API.APIHandlers["listt2iparams"].Permission);
        Assert.Equal(Permissions.FundamentalGenerateTabAccess, API.APIHandlers["triggerrefresh"].Permission);

        Assert.True(API.APIHandlers["generatetext2image"].IsUserUpdate);
        Assert.True(API.APIHandlers["generatetext2imagews"].IsUserUpdate);
        Assert.True(API.APIHandlers["addimagetohistory"].IsUserUpdate);
        Assert.False(API.APIHandlers["listimages"].IsUserUpdate);
        Assert.True(API.APIHandlers["toggleimagestarred"].IsUserUpdate);
        Assert.True(API.APIHandlers["openimagefolder"].IsUserUpdate);
        Assert.True(API.APIHandlers["deleteimage"].IsUserUpdate);
        Assert.False(API.APIHandlers["listt2iparams"].IsUserUpdate);
        Assert.True(API.APIHandlers["triggerrefresh"].IsUserUpdate);
    }
}
