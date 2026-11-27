using System.IO;

namespace NuExt.System.Tests;

public class IOUtilsTests
{
    [Test]
    public void SmartEndTrimTest()
    {
        var originalFileName = "This name too long.txt";
        var fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This name.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This name.txt"));

        originalFileName = "This_name_too_long.txt";
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));

        originalFileName = "This-name-too-long.txt";
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This-name.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This-name.txt"));

        originalFileName = "This@name@too@long.txt";
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This@nam.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This@name.txt"));
        fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This@name@.txt"));
    }

    [Test]
    public void ClearFileNameTest()
    {
        var originalFileName = "This:name:too:long.txt";
        var fileName = IOUtils.SanitizeFileName(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));

        originalFileName = "This?name?too?long.txt";
        fileName = IOUtils.SanitizeFileName(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));

        originalFileName = "This*name*too*long.txt";
        fileName = IOUtils.SanitizeFileName(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This_name.txt"));

        originalFileName = "This@name@too@long.txt";
        fileName = IOUtils.SanitizeFileName(originalFileName, 12);
        Assert.That(fileName, Is.EqualTo("This@nam.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 13);
        Assert.That(fileName, Is.EqualTo("This@name.txt"));
        fileName = IOUtils.SanitizeFileName(originalFileName, 14);
        Assert.That(fileName, Is.EqualTo("This@name@.txt"));
    }

    [Test]
    public void ShortenPathForDisplayEmptyTest()
    {
        Assert.That(IOUtils.ShortenPathForDisplay(string.Empty, 10), Is.EqualTo(string.Empty));
        Assert.That(IOUtils.ShortenPathForDisplay(null!, 10), Is.Null);
        Assert.That(IOUtils.ShortenPathForDisplay(string.Empty, 10, '\\'), Is.EqualTo(string.Empty));
        Assert.That(IOUtils.ShortenPathForDisplay(null!, 10, '/'), Is.Null);
    }

    [Test]
    public void ShortenPathForDisplayInvalidSeparatorTest()
    {
        Assert.That(() => IOUtils.ShortenPathForDisplay(@"C:\dir\file.txt", 5, '|'), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 0, @"C:\Users\me\Documents\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", -1, @"C:\Users\me\Documents\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 41, @"C:\Users\me\Documents\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 1000, @"C:\Users\me\Documents\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 40, @"C:\...\me\Documents\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 30, @"C:\...\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 26, @"C:\...\projects\report.pdf")]
    [TestCase(@"C:\Users\me\Documents\projects\report.pdf", 25, @"C:\...\report.pdf")]
    [TestCase(@"C:\a\b\c\d\e\file.txt", 20, @"C:\...\d\e\file.txt")]
    [TestCase(@"C:\file.txt", 5, @"C:\...\file.txt")]
    [TestCase(@"\\server\share\folder\file.txt", 25, @"\\server\share\...\file.txt")]
    [TestCase(@"C:\Users\user\Documents\Secretariat\new-competition.skrx", 50, @"C:\...\Documents\Secretariat\new-competition.skrx")]
    [TestCase(@"C:\Users\me\Documents\projects\", 30, @"C:\...\me\Documents\projects")]
    [TestCase("report.pdf", 3, "report.pdf")]
    public void ShortenPathForDisplayWindowsTest(string path, int preferredLength, string expected)
    {
        Assert.That(IOUtils.ShortenPathForDisplay(path, preferredLength, '\\'), Is.EqualTo(expected));
    }

    [TestCase(@"/home/user/Documents/projects/report.pdf", 30, "/.../projects/report.pdf")]
    [TestCase(@"/usr/share/doc/very-long-name-here.txt", 20, "/.../very-long-name-here.txt")]
    [TestCase(@"/home/user/file.txt", 12, "/.../file.txt")]
    [TestCase("/file.txt", 5, "/.../file.txt")]
    [TestCase("/home/user/Documents/projects/", 29, "/.../user/Documents/projects")]
    [TestCase("home/user/file.txt", 10, ".../file.txt")]
    [TestCase("report.pdf", 3, "report.pdf")]
    public void ShortenPathForDisplayUnixTest(string path, int preferredLength, string expected)
    {
        Assert.That(IOUtils.ShortenPathForDisplay(path, preferredLength, '/'), Is.EqualTo(expected));
    }

    [Test]
    public void ShortenPathForDisplayCurrentPlatformTest()
    {
        string separator = IOUtils.DirectorySeparatorChar.ToString();
        string root = Path.GetPathRoot(Path.GetTempPath())!;
        string name = "long-file-name.txt";
        string path = Path.Combine(root, "Users", "someone", "Documents", "Projects", name);

        // The overload without a separator uses the platform one; the rules stay the same.
        Assert.That(IOUtils.ShortenPathForDisplay(path, 0), Is.EqualTo(path));
        Assert.That(IOUtils.ShortenPathForDisplay(path, path.Length), Is.EqualTo(path));
        Assert.That(IOUtils.ShortenPathForDisplay(path, 40), Is.EqualTo(root + "..." + separator + "Projects" + separator + name));

        // A long name is kept whole, so the result exceeds the limit.
        string longName = new string('n', 60) + ".txt";
        string overLimit = IOUtils.ShortenPathForDisplay(Path.Combine(root, "a", longName), 20);
        Assert.That(overLimit, Is.EqualTo(root + "..." + separator + longName));
        Assert.That(overLimit.Length, Is.GreaterThan(20));
    }

    [Test]
    public void ShortenPathForDisplayKeepsSegmentsWholeTest()
    {
        (string Path, char Separator)[] cases =
        [
            (@"C:\Users\user\Documents\Secretariat\regional_competitions_taekwondo_poomsae_12_14.skrx", '\\'),
            (@"c:\Users\user\Documents\Secretariat\new competition.skrx", '\\'),
            (@"C:\a\b\c\d\e\f\g\h\i\j\file.txt", '\\'),
            (@"\\server\share\folder\sub\file.txt", '\\'),
            (@"\\?\C:\very\long\path\file.txt", '\\'),
            (@"C:\file.txt", '\\'),
            (@"C:\Users\me\", '\\'),
            ("file.txt", '\\'),
            ("file.txt", '/'),
            ("/home/user/Documents/projects/report.pdf", '/'),
            ("/usr/share/doc/very-long-name-here.txt", '/'),
            ("/file.txt", '/'),
            ("/home/user/", '/'),
            ("home/user/file.txt", '/'),
            ("/home/user/Documents/report on competitions.txt", '/')
        ];
        int[] limits = [0, 1, 5, 10, 20, 30, 40, 60, 80, 120, 1000];

        foreach ((string path, char separator) in cases)
        {
            foreach (int limit in limits)
            {
                string result = IOUtils.ShortenPathForDisplay(path, limit, separator);
                string context = $"path={path}, separator={separator}, limit={limit}";

                if (limit <= 0 || path.Length <= limit)
                {
                    Assert.That(result, Is.EqualTo(path), context);
                    continue;
                }

                if (result.Length == path.Length)
                {
                    Assert.That(result, Is.EqualTo(path), context);
                    continue;
                }

                string[] pathSegments = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
                string name = pathSegments[pathSegments.Length - 1];

                // The file or directory name is never shortened.
                Assert.That(result, Does.EndWith(name), context);

                int ellipsisIndex = result.IndexOf("...", StringComparison.Ordinal);
                Assert.That(ellipsisIndex, Is.GreaterThanOrEqualTo(0), context);

                // Whatever precedes the ellipsis is a real prefix of the path.
                Assert.That(path.StartsWith(result.Substring(0, ellipsisIndex), StringComparison.Ordinal), Is.True, context);

                // Every shown segment is a whole segment of the original path, never a cut one.
                foreach (string segment in result.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (segment == "...")
                    {
                        continue;
                    }
                    Assert.That(pathSegments, Has.Member(segment), context);
                }

                // Exceeding the limit is allowed only when the name alone does not fit: no directories then.
                if (result.Length > limit)
                {
                    Assert.That(result.Substring(ellipsisIndex + 4), Is.EqualTo(name), context);
                }
            }
        }
    }
}
