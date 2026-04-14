using System.ComponentModel.DataAnnotations;

namespace DPCatalogRetroCars.Models;

public class Rating
{
    public int Id { get; set; }

    [Required]
    public int CarStoryId { get; set; }

    public CarStory? CarStory { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Value { get; set; }
}
