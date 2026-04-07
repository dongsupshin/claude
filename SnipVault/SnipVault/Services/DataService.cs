using System.IO;
using System.Text.Json;
using SnipVault.Models;

namespace SnipVault.Services;

/// <summary>
/// Handles persistence of snippets and settings using local JSON files.
/// Data is stored in %APPDATA%/SnipVault/ by default.
/// </summary>
public class DataService
{
    private static readonly string DefaultAppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnipVault");

    private readonly string _appDataFolder;
    private readonly string _snippetsFile;
    private readonly string _settingsFile;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<Snippet> _snippets = new();
    private AppSettings _settings = new();

    public IReadOnlyList<Snippet> Snippets => _snippets.AsReadOnly();
    public AppSettings Settings => _settings;

    /// <summary>Supported language labels for syntax tagging.</summary>
    public static readonly string[] SupportedLanguages =
    {
        "Plain Text", "C#", "JavaScript", "TypeScript", "Python",
        "HTML", "CSS", "JSON", "XML", "SQL", "Bash", "PowerShell",
        "Java", "C++", "Go", "Rust", "YAML", "Markdown", "Other"
    };

    public DataService() : this(DefaultAppDataFolder) { }

    /// <summary>Constructor with custom folder path (for testing).</summary>
    public DataService(string folder)
    {
        _appDataFolder = folder;
        _snippetsFile = Path.Combine(_appDataFolder, "snippets.json");
        _settingsFile = Path.Combine(_appDataFolder, "settings.json");
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_appDataFolder))
            Directory.CreateDirectory(_appDataFolder);
    }

    /// <summary>
    /// Returns a de-duplicated sorted list of all categories,
    /// always including "General".
    /// </summary>
    public List<string> GetCategories()
    {
        var cats = _snippets.Select(s => s.Category).Distinct().ToList();
        if (!cats.Contains("General")) cats.Insert(0, "General");
        return cats.OrderBy(c => c == "General" ? "" : c).ToList();
    }

    // ── Settings ──────────────────────────────────────────────

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFile))
            {
                var json = await File.ReadAllTextAsync(_settingsFile);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }
        return _settings;
    }

    public async Task SaveSettingsAsync()
    {
        var json = JsonSerializer.Serialize(_settings, JsonOptions);
        await File.WriteAllTextAsync(_settingsFile, json);
    }

    // ── Snippets ──────────────────────────────────────────────

    public async Task LoadSnippetsAsync()
    {
        try
        {
            if (File.Exists(_snippetsFile))
            {
                var json = await File.ReadAllTextAsync(_snippetsFile);
                _snippets = JsonSerializer.Deserialize<List<Snippet>>(json, JsonOptions) ?? new();
            }
        }
        catch
        {
            _snippets = new();
        }
    }

    public async Task SaveSnippetsAsync()
    {
        var json = JsonSerializer.Serialize(_snippets, JsonOptions);
        await File.WriteAllTextAsync(_snippetsFile, json);
    }

    public async Task AddSnippetAsync(Snippet snippet)
    {
        _snippets.Insert(0, snippet);
        await SaveSnippetsAsync();
    }

    public async Task UpdateSnippetAsync(Snippet updated)
    {
        var index = _snippets.FindIndex(s => s.Id == updated.Id);
        if (index >= 0)
        {
            updated.ModifiedAt = DateTime.Now;
            _snippets[index] = updated;
            await SaveSnippetsAsync();
        }
    }

    public async Task DeleteSnippetAsync(string id)
    {
        _snippets.RemoveAll(s => s.Id == id);
        await SaveSnippetsAsync();
    }

    public async Task IncrementCopyCountAsync(string id)
    {
        var snippet = _snippets.Find(s => s.Id == id);
        if (snippet != null)
        {
            snippet.CopyCount++;
            await SaveSnippetsAsync();
        }
    }

    // ── Search & Filter ───────────────────────────────────────

    public List<Snippet> Search(string query, string? categoryFilter = null, bool? favoritesOnly = null)
    {
        var results = _snippets.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
            results = results.Where(s => s.Category == categoryFilter);

        if (favoritesOnly == true)
            results = results.Where(s => s.IsFavorite);

        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(s =>
                s.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Category.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Language.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return results
            .OrderByDescending(s => s.IsPinned)
            .ThenByDescending(s => s.ModifiedAt)
            .ToList();
    }

    // ── Import / Export ───────────────────────────────────────

    public async Task<string> ExportToJsonAsync()
    {
        return JsonSerializer.Serialize(_snippets, JsonOptions);
    }

    public async Task<int> ImportFromJsonAsync(string json)
    {
        try
        {
            var imported = JsonSerializer.Deserialize<List<Snippet>>(json, JsonOptions);
            if (imported == null || imported.Count == 0) return 0;

            int count = 0;
            foreach (var snippet in imported)
            {
                if (!_snippets.Any(s => s.Id == snippet.Id))
                {
                    _snippets.Add(snippet);
                    count++;
                }
            }

            await SaveSnippetsAsync();
            return count;
        }
        catch
        {
            return 0;
        }
    }
}
