using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace CamaroesPOS.Tests;

public class LocalizationTests
{
    private static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [Fact]
    public void FrenchAndEnglishDictionariesHaveIdenticalKeys()
    {
        var frPath = Path.Combine(RepoRoot, "CamaroesPOS.UI.Avalonia", "Resources", "Langs", "Strings.fr.axaml");
        var enPath = Path.Combine(RepoRoot, "CamaroesPOS.UI.Avalonia", "Resources", "Langs", "Strings.en.axaml");

        Assert.True(File.Exists(frPath), $"Missing French dictionary at {frPath}");
        Assert.True(File.Exists(enPath), $"Missing English dictionary at {enPath}");

        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var frKeys = XDocument.Load(frPath).Descendants().Select(e => e.Attribute(x + "Key")?.Value).Where(k => k != null).ToHashSet();
        var enKeys = XDocument.Load(enPath).Descendants().Select(e => e.Attribute(x + "Key")?.Value).Where(k => k != null).ToHashSet();

        Assert.NotEmpty(frKeys);
        Assert.Equal(frKeys, enKeys);
    }
}
