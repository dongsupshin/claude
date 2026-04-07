namespace SnipVault.Models;

/// <summary>
/// Represents a single saved text or code snippet.
/// </summary>
public class Snippet
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "Untitled";
    public string Content { get; set; } = "";
    public string Category { get; set; } = "General";
    public string Language { get; set; } = "Plain Text";
    public bool IsFavorite { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public int CopyCount { get; set; }

    /// <summary>
    /// Creates a deep copy of this snippet (for editing without mutating the original).
    /// </summary>
    public Snippet Clone() => new()
    {
        Id = Id,
        Title = Title,
        Content = Content,
        Category = Category,
        Language = Language,
        IsFavorite = IsFavorite,
        IsPinned = IsPinned,
        CreatedAt = CreatedAt,
        ModifiedAt = ModifiedAt,
        CopyCount = CopyCount
    };
}
