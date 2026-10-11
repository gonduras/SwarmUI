using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("test_file.txt", "test_filetxt")]
        [InlineData("C:\\test\\file.txt", "C/test/filetxt")]
        [InlineData("test//file.txt", "test/filetxt")]
        [InlineData("/test/file.txt", "test/filetxt")]
        [InlineData("con", "con_")]
        [InlineData("test/con", "test/con_")]
        [InlineData("prn", "prn_")]
        [InlineData("aux", "aux_")]
        [InlineData("nul", "nul_")]
        [InlineData("com1", "com1_")]
        [InlineData("lpt1", "lpt1_")]
        [InlineData("test<file", "testfile")]
        [InlineData("test>file", "testfile")]
        [InlineData("test|file", "testfile")]
        [InlineData("test?file", "testfile")]
        [InlineData("test*file", "testfile")]
        [InlineData("test:file", "testfile")]
        [InlineData("test\"file", "testfile")]
        [InlineData("test​file", "testfile")] // non-breaking zero-width space
        public void StrictFilenameClean_CleansCorrectly(string input, string expected)
        {
            string result = Utilities.StrictFilenameClean(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(10.23, 0.1, 10.2)]
        [InlineData(10.26, 0.1, 10.3)]
        [InlineData(10.234, 0.01, 10.23)]
        [InlineData(10.236, 0.01, 10.24)]
        [InlineData(-10.23, 0.1, -10.2)]
        [InlineData(100.0, 64.0, 128.0)]
        [InlineData(10.0, 5.0, 10.0)]
        public void RoundToPrecision_RoundsCorrectly(double val, double prec, double expected)
        {
            double result = Utilities.RoundToPrecision(val, prec);
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData("C:/test", "file.txt", "C:/test/file.txt")]
        [InlineData("C:/test/", "file.txt", "C:/test/file.txt")]
        [InlineData("C:/test", "/absolute/file.txt", "/absolute/file.txt")]
        [InlineData("C:/test", "D:/absolute/file.txt", "D:/absolute/file.txt")]
        [InlineData("C:/test", "\\\\server\\share\\file.txt", "\\\\server\\share\\file.txt")]
        [InlineData("C:\\test", "file.txt", "C:\\test/file.txt")] // Mixed slash
        [InlineData("C:\\test", "sub\\file.txt", "C:\\test/sub\\file.txt")] // Window slashes
        public void CombinePathWithAbsolute_CombinesCorrectly(string pathA, string pathB, string expected)
        {
            string result = Utilities.CombinePathWithAbsolute(pathA, pathB);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CombinePathWithAbsolute_ThreeParams_CombinesCorrectly()
        {
            string result = Utilities.CombinePathWithAbsolute("C:/test", "sub", "file.txt");
            Assert.Equal("C:/test/sub/file.txt", result);
        }
    }
}
