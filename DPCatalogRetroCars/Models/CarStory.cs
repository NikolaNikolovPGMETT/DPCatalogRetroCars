using System.ComponentModel.DataAnnotations;

namespace DPCatalogRetroCars.Models;

public class CarStory
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    
    public string Brand { get; set; } = string.Empty;

    [Required, StringLength(100)]
    
    public string Model { get; set; } = string.Empty;

    [StringLength(100)]
    
    public string? Generation { get; set; }

    [Range(1886, 2100)]
    
    public int Year { get; set; }

    [StringLength(100)]
   
    public string? Engine { get; set; }

    [StringLength(100)]
    
    public string? Color { get; set; }
    

    [Display(Name = "Състояние при придобиване")]
    [Required, StringLength(2000)]
    public string AcquisitionState { get; set; } = string.Empty;

    [Display(Name = "История на реставрацията")]
    [Required, StringLength(4000)]
    public string RestorationHistory { get; set; } = string.Empty;

    [Display(Name = "Краен резултат")]
    [Required, StringLength(2000)]
    public string FinalResult { get; set; } = string.Empty;

    [Display(Name = "Основно изображение URL")]
    [Url]
    public string? ImageUrl { get; set; }

    [Display(Name = "Видео URL")]
    [Url]
    public string? VideoUrl { get; set; }

    [Display(Name = "Локация")]
    [StringLength(200)]
    public string? Location { get; set; }

    
    public StoryStatus StoryStatus { get; set; } = StoryStatus.Pending;

    
    public RestorationStatus RestorationStatus { get; set; } = RestorationStatus.Current;

    [Display(Name = "QR код URL")]
    [Url]
    public string? QrCodeUrl { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}