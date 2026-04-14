using System.ComponentModel.DataAnnotations;

namespace DPCatalogRetroCars.Models;

public class RetroEvent
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateTime StartsOnUtc { get; set; }

    [Required]
    public DateTime EndsOnUtc { get; set; }

    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
