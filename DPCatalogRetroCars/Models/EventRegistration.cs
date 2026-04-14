namespace DPCatalogRetroCars.Models;

public class EventRegistration
{
    public int Id { get; set; }
    public int RetroEventId { get; set; }
    public RetroEvent? RetroEvent { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime RegisteredOnUtc { get; set; } = DateTime.UtcNow;
}
