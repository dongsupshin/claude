using System.Collections.ObjectModel;
using System.Windows;
using SnipVault.Helpers;
using SnipVault.Models;
using SnipVault.Services;

namespace SnipVault.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly DataService _dataService;

    private string _searchQuery = "";
    private string _selectedCategory = "All";
    private bool _favoritesOnly;
    private Snippet? _selectedSnippet;
    private bool _isEditing;
    private string _statusMessage = "Ready";

    // Editor fields
    private string _editTitle = "";
    private string _editContent = "";
    private string _editCategory = "General";
    private string _editLanguage = "Plain Text";
    private bool _editIsFavorite;

    public MainViewModel(DataService dataService)
    {
        _dataService = dataService;
        FilteredSnippets = new ObservableCollection<Snippet>();
        Categories = new ObservableCollection<string> { "All" };

        NewSnippetCommand = new RelayCommand(NewSnippet);
        SaveSnippetCommand = new RelayCommand(async () => await SaveSnippetAsync());
        CancelEditCommand = new RelayCommand(CancelEdit);
        DeleteSnippetCommand = new RelayCommand(async () => await DeleteSnippetAsync());
        CopyToClipboardCommand = new RelayCommand(async () => await CopyToClipboardAsync());
        ToggleFavoriteCommand = new RelayCommand(async () => await ToggleFavoriteAsync());
        TogglePinCommand = new RelayCommand(async () => await TogglePinAsync());
        ExportCommand = new RelayCommand(async () => await ExportAsync());
        ImportCommand = new RelayCommand(async () => await ImportAsync());
        ShowGuideCommand = new RelayCommand(() => GuideRequested?.Invoke());
        SearchCommand = new RelayCommand(RefreshList);
    }

    // ── Collections ───────────────────────────────────────────

    public ObservableCollection<Snippet> FilteredSnippets { get; }
    public ObservableCollection<string> Categories { get; }
    public static string[] Languages => DataService.SupportedLanguages;

    // ── Search / Filter ───────────────────────────────────────

    public string SearchQuery
    {
        get => _searchQuery;
        set { if (SetProperty(ref _searchQuery, value)) RefreshList(); }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { if (SetProperty(ref _selectedCategory, value)) RefreshList(); }
    }

    public bool FavoritesOnly
    {
        get => _favoritesOnly;
        set { if (SetProperty(ref _favoritesOnly, value)) RefreshList(); }
    }

    // ── Selection ─────────────────────────────────────────────

    public Snippet? SelectedSnippet
    {
        get => _selectedSnippet;
        set
        {
            if (SetProperty(ref _selectedSnippet, value) && value != null && !IsEditing)
                LoadSnippetToEditor(value);
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool HasSelection => SelectedSnippet != null || IsEditing;

    // ── Editor Properties ─────────────────────────────────────

    public string EditTitle
    {
        get => _editTitle;
        set => SetProperty(ref _editTitle, value);
    }

    public string EditContent
    {
        get => _editContent;
        set => SetProperty(ref _editContent, value);
    }

    public string EditCategory
    {
        get => _editCategory;
        set => SetProperty(ref _editCategory, value);
    }

    public string EditLanguage
    {
        get => _editLanguage;
        set => SetProperty(ref _editLanguage, value);
    }

    public bool EditIsFavorite
    {
        get => _editIsFavorite;
        set => SetProperty(ref _editIsFavorite, value);
    }

    // ── Commands ──────────────────────────────────────────────

    public RelayCommand NewSnippetCommand { get; }
    public RelayCommand SaveSnippetCommand { get; }
    public RelayCommand CancelEditCommand { get; }
    public RelayCommand DeleteSnippetCommand { get; }
    public RelayCommand CopyToClipboardCommand { get; }
    public RelayCommand ToggleFavoriteCommand { get; }
    public RelayCommand TogglePinCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand ShowGuideCommand { get; }
    public RelayCommand SearchCommand { get; }

    public event Action? GuideRequested;
    public event Func<string, string, string?, Task<string?>>? FileDialogRequested;

    // ── Initialization ────────────────────────────────────────

    public void Initialize()
    {
        RefreshCategories();
        RefreshList();
        StatusMessage = $"{_dataService.Snippets.Count} snippets loaded";
    }

    // ── List Management ───────────────────────────────────────

    public void RefreshList()
    {
        var results = _dataService.Search(
            SearchQuery,
            SelectedCategory == "All" ? null : SelectedCategory,
            FavoritesOnly ? true : null);

        FilteredSnippets.Clear();
        foreach (var s in results)
            FilteredSnippets.Add(s);
    }

    private void RefreshCategories()
    {
        Categories.Clear();
        Categories.Add("All");
        foreach (var cat in _dataService.GetCategories())
            Categories.Add(cat);
    }

    // ── Editor ────────────────────────────────────────────────

    private void LoadSnippetToEditor(Snippet snippet)
    {
        EditTitle = snippet.Title;
        EditContent = snippet.Content;
        EditCategory = snippet.Category;
        EditLanguage = snippet.Language;
        EditIsFavorite = snippet.IsFavorite;
        OnPropertyChanged(nameof(HasSelection));
    }

    private void NewSnippet()
    {
        SelectedSnippet = null;
        EditTitle = "";
        EditContent = "";
        EditCategory = _dataService.Settings.DefaultCategory;
        EditLanguage = _dataService.Settings.DefaultLanguage;
        EditIsFavorite = false;
        IsEditing = true;
        OnPropertyChanged(nameof(HasSelection));
        StatusMessage = "Creating new snippet...";
    }

    private async Task SaveSnippetAsync()
    {
        if (string.IsNullOrWhiteSpace(EditTitle))
        {
            StatusMessage = "Please enter a title";
            return;
        }

        if (SelectedSnippet != null && !IsEditing)
        {
            // Update existing
            var updated = SelectedSnippet.Clone();
            updated.Title = EditTitle.Trim();
            updated.Content = EditContent;
            updated.Category = string.IsNullOrWhiteSpace(EditCategory) ? "General" : EditCategory.Trim();
            updated.Language = EditLanguage;
            updated.IsFavorite = EditIsFavorite;
            await _dataService.UpdateSnippetAsync(updated);
            StatusMessage = $"Updated: {updated.Title}";
        }
        else
        {
            // Create new
            var snippet = new Snippet
            {
                Title = EditTitle.Trim(),
                Content = EditContent,
                Category = string.IsNullOrWhiteSpace(EditCategory) ? "General" : EditCategory.Trim(),
                Language = EditLanguage,
                IsFavorite = EditIsFavorite
            };
            await _dataService.AddSnippetAsync(snippet);
            StatusMessage = $"Created: {snippet.Title}";
        }

        IsEditing = false;
        RefreshCategories();
        RefreshList();
    }

    private void CancelEdit()
    {
        IsEditing = false;
        if (SelectedSnippet != null)
            LoadSnippetToEditor(SelectedSnippet);
        else
            ClearEditor();
        StatusMessage = "Edit cancelled";
    }

    private void ClearEditor()
    {
        EditTitle = "";
        EditContent = "";
        EditCategory = "General";
        EditLanguage = "Plain Text";
        EditIsFavorite = false;
    }

    private async Task DeleteSnippetAsync()
    {
        if (SelectedSnippet == null) return;

        if (_dataService.Settings.ConfirmBeforeDelete)
        {
            var result = MessageBox.Show(
                $"Delete \"{SelectedSnippet.Title}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
        }

        var title = SelectedSnippet.Title;
        await _dataService.DeleteSnippetAsync(SelectedSnippet.Id);
        SelectedSnippet = null;
        ClearEditor();
        RefreshCategories();
        RefreshList();
        OnPropertyChanged(nameof(HasSelection));
        StatusMessage = $"Deleted: {title}";
    }

    // ── Clipboard ─────────────────────────────────────────────

    private async Task CopyToClipboardAsync()
    {
        if (SelectedSnippet == null) return;
        try
        {
            Clipboard.SetText(SelectedSnippet.Content);
            await _dataService.IncrementCopyCountAsync(SelectedSnippet.Id);
            StatusMessage = $"Copied: {SelectedSnippet.Title} (×{SelectedSnippet.CopyCount})";
        }
        catch
        {
            StatusMessage = "Failed to copy to clipboard";
        }
    }

    // ── Toggle Actions ────────────────────────────────────────

    private async Task ToggleFavoriteAsync()
    {
        if (SelectedSnippet == null) return;
        var updated = SelectedSnippet.Clone();
        updated.IsFavorite = !updated.IsFavorite;
        await _dataService.UpdateSnippetAsync(updated);
        EditIsFavorite = updated.IsFavorite;
        RefreshList();
        StatusMessage = updated.IsFavorite ? "Added to favorites" : "Removed from favorites";
    }

    private async Task TogglePinAsync()
    {
        if (SelectedSnippet == null) return;
        var updated = SelectedSnippet.Clone();
        updated.IsPinned = !updated.IsPinned;
        await _dataService.UpdateSnippetAsync(updated);
        RefreshList();
        StatusMessage = updated.IsPinned ? "Pinned to top" : "Unpinned";
    }

    // ── Import / Export ───────────────────────────────────────

    private async Task ExportAsync()
    {
        var json = await _dataService.ExportToJsonAsync();
        try
        {
            Clipboard.SetText(json);
            StatusMessage = $"Exported {_dataService.Snippets.Count} snippets to clipboard (JSON)";
        }
        catch
        {
            StatusMessage = "Export failed";
        }
    }

    private async Task ImportAsync()
    {
        try
        {
            var json = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(json))
            {
                StatusMessage = "Clipboard is empty — copy JSON data first";
                return;
            }

            var count = await _dataService.ImportFromJsonAsync(json);
            RefreshCategories();
            RefreshList();
            StatusMessage = count > 0 ? $"Imported {count} snippets" : "No new snippets found in clipboard";
        }
        catch
        {
            StatusMessage = "Import failed — invalid JSON format";
        }
    }
}
