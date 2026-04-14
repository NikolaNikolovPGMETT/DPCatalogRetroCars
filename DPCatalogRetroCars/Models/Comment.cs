using System.ComponentModel.DataAnnotations;

namespace DPCatalogRetroCars.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public int CarStoryId { get; set; }

    public CarStory? CarStory { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsHidden { get; set; }

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
}
