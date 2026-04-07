using Xunit;
using SnipVault.Models;
using SnipVault.Services;

namespace SnipVault.Tests;

public class DataServiceTests : IDisposable
{
    private readonly string _testFolder;
    private readonly DataService _svc;

    public DataServiceTests()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), $"SnipVault_Test_{Guid.NewGuid():N}");
        _svc = new DataService(_testFolder);
    }

    public void Dispose()
    {
        try { Directory.Delete(_testFolder, true); } catch { }
    }

    [Fact]
    public async Task Settings_SaveAndLoad_RoundTrips()
    {
        _svc.Settings.DefaultCategory = "TestCat";
        _svc.Settings.MaxRecentSnippets = 99;
        await _svc.SaveSettingsAsync();

        var fresh = new DataService(_testFolder);
        var loaded = await fresh.LoadSettingsAsync();

        Assert.Equal("TestCat", loaded.DefaultCategory);
        Assert.Equal(99, loaded.MaxRecentSnippets);
    }

    [Fact]
    public async Task Settings_DefaultValues_AreJsonSafe()
    {
        // Ensure no NaN/Infinity that would crash System.Text.Json
        await _svc.SaveSettingsAsync();
        var fresh = new DataService(_testFolder);
        var loaded = await fresh.LoadSettingsAsync();

        Assert.Equal(960, loaded.WindowWidth);
        Assert.Equal(640, loaded.WindowHeight);
        Assert.True(loaded.ShowGuideOnStartup);
    }

    [Fact]
    public async Task AddSnippet_AppearsInList()
    {
        var snippet = new Snippet { Title = "Hello", Content = "World" };
        await _svc.AddSnippetAsync(snippet);

        Assert.Single(_svc.Snippets);
        Assert.Equal("Hello", _svc.Snippets[0].Title);
    }

    [Fact]
    public async Task AddSnippet_PersistsAfterReload()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "Persistent", Content = "Data" });

        var fresh = new DataService(_testFolder);
        await fresh.LoadSnippetsAsync();

        Assert.Single(fresh.Snippets);
        Assert.Equal("Persistent", fresh.Snippets[0].Title);
    }

    [Fact]
    public async Task UpdateSnippet_ChangesContent()
    {
        var snippet = new Snippet { Title = "Original" };
        await _svc.AddSnippetAsync(snippet);

        var updated = snippet.Clone();
        updated.Title = "Updated";
        await _svc.UpdateSnippetAsync(updated);

        Assert.Equal("Updated", _svc.Snippets[0].Title);
    }

    [Fact]
    public async Task DeleteSnippet_RemovesFromList()
    {
        var snippet = new Snippet { Title = "ToDelete" };
        await _svc.AddSnippetAsync(snippet);
        Assert.Single(_svc.Snippets);

        await _svc.DeleteSnippetAsync(snippet.Id);
        Assert.Empty(_svc.Snippets);
    }

    [Fact]
    public async Task IncrementCopyCount_Works()
    {
        var snippet = new Snippet { Title = "CopyMe" };
        await _svc.AddSnippetAsync(snippet);
        Assert.Equal(0, _svc.Snippets[0].CopyCount);

        await _svc.IncrementCopyCountAsync(snippet.Id);
        Assert.Equal(1, _svc.Snippets[0].CopyCount);
    }

    [Fact]
    public async Task Search_ByTitle()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "Alpha Code", Content = "abc" });
        await _svc.AddSnippetAsync(new Snippet { Title = "Beta Script", Content = "def" });

        var results = _svc.Search("alpha");
        Assert.Single(results);
        Assert.Equal("Alpha Code", results[0].Title);
    }

    [Fact]
    public async Task Search_ByContent()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "S1", Content = "hello world" });
        await _svc.AddSnippetAsync(new Snippet { Title = "S2", Content = "goodbye" });

        var results = _svc.Search("world");
        Assert.Single(results);
        Assert.Equal("S1", results[0].Title);
    }

    [Fact]
    public async Task Search_ByCategoryFilter()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "S1", Category = "Work" });
        await _svc.AddSnippetAsync(new Snippet { Title = "S2", Category = "Personal" });

        var results = _svc.Search("", "Work");
        Assert.Single(results);
        Assert.Equal("S1", results[0].Title);
    }

    [Fact]
    public async Task Search_FavoritesOnly()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "Fav", IsFavorite = true });
        await _svc.AddSnippetAsync(new Snippet { Title = "NotFav", IsFavorite = false });

        var results = _svc.Search("", null, true);
        Assert.Single(results);
        Assert.Equal("Fav", results[0].Title);
    }

    [Fact]
    public async Task Search_PinnedAppearsFirst()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "Normal", IsPinned = false });
        await _svc.AddSnippetAsync(new Snippet { Title = "Pinned", IsPinned = true });

        var results = _svc.Search("");
        Assert.Equal("Pinned", results[0].Title);
    }

    [Fact]
    public void GetCategories_AlwaysIncludesGeneral()
    {
        var cats = _svc.GetCategories();
        Assert.Contains("General", cats);
    }

    [Fact]
    public async Task GetCategories_IncludesSnippetCategories()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "S1", Category = "Work" });
        await _svc.AddSnippetAsync(new Snippet { Title = "S2", Category = "Personal" });

        var cats = _svc.GetCategories();
        Assert.Contains("Work", cats);
        Assert.Contains("Personal", cats);
        Assert.Contains("General", cats);
    }

    [Fact]
    public async Task ExportImport_RoundTrips()
    {
        await _svc.AddSnippetAsync(new Snippet { Title = "Export1", Content = "data1" });
        await _svc.AddSnippetAsync(new Snippet { Title = "Export2", Content = "data2" });

        var json = await _svc.ExportToJsonAsync();

        // Import into a fresh service
        var fresh = new DataService(Path.Combine(Path.GetTempPath(), $"SnipVault_Test2_{Guid.NewGuid():N}"));
        var count = await fresh.ImportFromJsonAsync(json);

        Assert.Equal(2, count);
        Assert.Equal(2, fresh.Snippets.Count);

        try { Directory.Delete(Path.GetDirectoryName(fresh.Snippets.ToString())!, true); } catch { }
    }

    [Fact]
    public async Task Import_NoDuplicates()
    {
        var snippet = new Snippet { Title = "UniqueOne" };
        await _svc.AddSnippetAsync(snippet);

        var json = await _svc.ExportToJsonAsync();
        var count = await _svc.ImportFromJsonAsync(json);

        Assert.Equal(0, count); // Already exists
        Assert.Single(_svc.Snippets);
    }

    [Fact]
    public async Task Import_InvalidJson_ReturnsZero()
    {
        var count = await _svc.ImportFromJsonAsync("not json at all");
        Assert.Equal(0, count);
    }

    [Fact]
    public void Snippet_Clone_IsDeepCopy()
    {
        var original = new Snippet { Title = "Orig", Content = "Data", IsFavorite = true };
        var clone = original.Clone();

        clone.Title = "Changed";
        Assert.Equal("Orig", original.Title);
        Assert.Equal("Changed", clone.Title);
        Assert.Equal(original.Id, clone.Id);
    }

    [Fact]
    public async Task LoadSnippets_EmptyFile_ReturnsEmptyList()
    {
        await _svc.LoadSnippetsAsync();
        Assert.Empty(_svc.Snippets);
    }

    [Fact]
    public async Task LoadSettings_EmptyFile_ReturnsDefaults()
    {
        var settings = await _svc.LoadSettingsAsync();
        Assert.True(settings.ShowGuideOnStartup);
        Assert.Equal("General", settings.DefaultCategory);
    }
}
