using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NuExt.System.Tests;

[TestFixture]
[TestOf(typeof(IOUtils))]
public class IOUtils_GetRelativePathTests
{
    private readonly bool _isWindows = PlatformInformation.IsWindows;
    private readonly char _directorySeparator = Path.DirectorySeparatorChar;

    #region Basic Scenarios

    [Test]
    public void GetRelativePath_FromDirectoryToItself_ReturnsCurrentDirectory()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp";
        string fullPath = @"C:\Projects\MyApp";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo(IOUtils.ThisDirectory));
    }

    [Test]
    public void GetRelativePath_FromDirectoryToChildFile_ReturnsRelativePath()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp";
        string fullPath = @"C:\Projects\MyApp\src\Program.cs";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo($"src{_directorySeparator}Program.cs"));
    }

    [Test]
    public void GetRelativePath_FromDirectoryToSiblingDirectory_ReturnsParentRefPath()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp\src";
        string fullPath = @"C:\Projects\MyApp\tests";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo($"..{_directorySeparator}tests"));
    }

    #endregion

    #region Platform-Specific Paths

    [Test]
    public void GetRelativePath_WindowsStylePaths_ComputesCorrectly()
    {
        // Skip on non-Windows platforms
        if (!_isWindows)
        {
            Assert.Ignore("Windows-specific test");
            return;
        }

        // Arrange
        string directory = @"C:\alpha\beta";
        string fullPath = @"C:\alpha\gamma\delta.txt";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo($"..{_directorySeparator}gamma{_directorySeparator}delta.txt"));
    }


    [Test]
    public void GetRelativePath_WindowsNetworkPaths_HandlesUNC()
    {
        // Skip on non-Windows platforms
        if (!_isWindows)
        {
            Assert.Ignore("Windows UNC test");
            return;
        }

        // Arrange
        string directory = @"\\server\share\documents";
        string fullPath = @"\\server\share\documents\reports\Q1.pdf";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        // This will likely fail with current implementation - good test case
        Assert.That(result, Is.EqualTo($"reports{_directorySeparator}Q1.pdf"));
    }

    #endregion

    #region Edge Cases

    [TestCase(@"a\b", @"a\b\c\d", ExpectedResult = @"c\d")]
    [TestCase(@"a\b\c", @"a\b", ExpectedResult = @"..")]
    [TestCase(@"a\b", @"a\c\d", ExpectedResult = @"..\c\d")]
    [TestCase(@"a", @"a", ExpectedResult = @".")]
    [TestCase(@"a\b", @"a\b\c", ExpectedResult = "c")]
    [TestCase(@"a\b", @"a\c", ExpectedResult = @"..\c")]
    [TestCase(@"a\b\c", @"a\d", ExpectedResult = @"..\..\d")]
    [TestCase(@"a", @"b", ExpectedResult = @"..\b")]
    public string GetRelativePath_RelativePathVariations(string dir, string path)
    {
        var bclPath = Path.GetRelativePath(dir, path);
        Assert.That(IOUtils.GetRelativePath(dir, path), Is.EqualTo(bclPath));
        return IOUtils.GetRelativePath(dir, path);
    }

    [Test]
    public void GetRelativePath_FromRootToFile_ReturnsPathFromRoot()
    {
        // Arrange
        string root = _isWindows ? @"C:\" : "/";
        string fullPath = _isWindows ? @"C:\Windows\system.ini" : "/etc/hosts";

        // Act
        string result = IOUtils.GetRelativePath(root, fullPath);

        // Assert
        string expected = _isWindows ? $"Windows{_directorySeparator}system.ini" : $"etc{_directorySeparator}hosts";
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetRelativePath_NoCommonRoot_ReturnsFullPath()
    {
        // Skip on Unix as this is Windows-specific scenario
        if (!_isWindows)
        {
            Assert.Ignore("Windows drive letter test");
            return;
        }

        // Arrange
        string directory = @"C:\Projects\App1";
        string fullPath = @"D:\Data\file.txt";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo(fullPath));
    }

    [Test]
    public void GetRelativePath_EmptyRelativeResult_ReturnsCurrentDirectory()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp\src";
        string fullPath = @"C:\Projects\MyApp\src";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo(IOUtils.ThisDirectory));
    }

    #endregion

    #region Parameterized Tests

    [TestCase(@"C:\A\B", @"C:\A\B\C", ExpectedResult = "C")]
    [TestCase(@"C:\A\B", @"C:\A\B\C\D", ExpectedResult = @"C\D")]
    [TestCase(@"C:\A\B", @"C:\A", ExpectedResult = "..")]
    [TestCase(@"C:\A\B", @"C:\", ExpectedResult = @"..\..")]
    [TestCase(@"C:\", @"C:\W", ExpectedResult = "W")]
    public string GetRelativePath_WindowsParameterized_ReturnsExpected(string directory, string fullPath)
    {
        if (!_isWindows)
        {
            Assert.Ignore("Windows-specific test");
            return string.Empty;
        }

        // Act & Assert
        return IOUtils.GetRelativePath(directory, fullPath);
    }

    #endregion

    #region Special Character Handling

    [Test]
    public void GetRelativePath_PathsWithParentDirectoryRef_NormalizesCorrectly()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp\src\features";
        string fullPath = @"C:\Projects\MyApp\src\..\tests\unit.cs";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        // This tests if the method normalizes paths before comparison
        Assert.That(result, Is.EqualTo($"..{_directorySeparator}..{_directorySeparator}tests{_directorySeparator}unit.cs"));
    }

    [Test]
    public void GetRelativePath_PathsWithCurrentDirectoryRef_HandlesCorrectly()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp";
        string fullPath = @"C:\Projects\MyApp\.\.\src\.\Program.cs";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        Assert.That(result, Is.EqualTo($"src{_directorySeparator}Program.cs"));
    }

    #endregion

    #region Case Sensitivity Tests

    [Test]
    public void GetRelativePath_SamePathDifferentCase_RespectsPlatformCaseSensitivity()
    {
        // Arrange
        string directory = @"C:\Projects\MyApp";
        string fullPathSameCase = @"C:\Projects\MyApp\subdir\file.txt";
        string fullPathDiffCaseDir = @"C:\Projects\MYAPP\subdir\file.txt";

        // Act
        string resultSame = IOUtils.GetRelativePath(directory, fullPathSameCase);
        string resultDiffDir = IOUtils.GetRelativePath(directory, fullPathDiffCaseDir);

        // Assert
        if (IOUtils.IsCaseSensitivePlatform)
        {
            // (Unix)
            Assert.That(resultDiffDir, Is.Not.EqualTo(resultSame));
        }
        else
        {
            // (Windows)
            Assert.That(resultDiffDir, Is.EqualTo(resultSame));
        }
    }

    [Test]
    public void GetRelativePath_DifferentCaseInFileName_ReturnsDifferentStrings()
    {
        // Arrange:
        string directory = @"C:\Projects\MyApp";
        string fullPathLower = @"C:\Projects\MyApp\readme.txt";
        string fullPathUpper = @"C:\Projects\MyApp\README.TXT";

        // Act
        string resultLower = IOUtils.GetRelativePath(directory, fullPathLower);
        string resultUpper = IOUtils.GetRelativePath(directory, fullPathUpper);

        // Assert
        Assert.That(resultUpper, Is.Not.EqualTo(resultLower));

        if (!IOUtils.IsCaseSensitivePlatform)
        {
            string combinedLower = Path.Combine(directory, resultLower);
            string combinedUpper = Path.Combine(directory, resultUpper);
            Assert.That(IOUtils.PathEquals(combinedLower, combinedUpper), Is.True);
        }
    }

    #endregion

    #region Input Validation Tests

    [Test]
    public void GetRelativePath_DirectoryIsNullOrEmpty_ThrowsArgumentException()
    {
        // Arrange
        string validPath = @"C:\test";

        // Act & Assert
        Assert.That(() => IOUtils.GetRelativePath(null!, validPath),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("directory"));

        Assert.That(() => IOUtils.GetRelativePath("", validPath),
            Throws.ArgumentException);
    }

    [Test]
    public void GetRelativePath_FullPathIsNullOrEmpty_ThrowsArgumentException()
    {
        // Arrange
        string validPath = @"C:\test";

        // Act & Assert
        Assert.That(() => IOUtils.GetRelativePath(validPath, null!),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("fullPath"));

        Assert.That(() => IOUtils.GetRelativePath(validPath, ""),
            Throws.ArgumentException);
    }

    [Test]
    public void GetRelativePath_RelativePathsAsInput_ReturnsFullPath()
    {
        // Arrange
        string directory = @"projects\myapp";
        string fullPath = @"projects\myapp\src\file.cs";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert

        string bclResult = Path.GetRelativePath(directory, fullPath);
        Assert.That(result, Is.EqualTo(bclResult));

        Assert.That(result, Is.EqualTo(@"src\file.cs"));
    }

#endregion

    #region Mixed Path Separators

    [Test]
    public void GetRelativePath_MixedSeparators_NormalizesOutput()
    {
        if (!_isWindows)
        {
            Assert.Ignore("Windows-specific path separator test");
            return;
        }

        // Arrange - mixing forward and backslashes
        string directory = @"C:\Projects/MyApp";
        string fullPath = @"C:\Projects\MyApp/src\Program.cs";

        // Act
        string result = IOUtils.GetRelativePath(directory, fullPath);

        // Assert
        // Should normalize to platform-specific separator
        Assert.That(result, Is.EqualTo($"src{_directorySeparator}Program.cs"));
    }

    #endregion
}
