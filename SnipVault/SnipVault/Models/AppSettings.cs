namespace SnipVault.Models;

/// <summary>
/// Application settings persisted as JSON. No NaN or Infinity values.
/// </summary>
public class AppSettings
{
    public bool ShowGuideOnStartup { get; set; } = true;
    public string DefaultCategory { get; set; } = "General";
    public string DefaultLanguage { get; set; } = "Plain Text";
    public bool ConfirmBeforeDelete { get; set; } = true;
    public bool CopyOnSingleClick { get; set; } = false;
    public string Theme { get; set; } = "Dark";
    public int MaxRecentSnippets { get; set; } = 50;
    public double WindowWidth { get; set; } = 960;
    public double WindowHeight { get; set; } = 640;
}
