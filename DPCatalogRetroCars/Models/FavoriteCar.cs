namespace DPCatalogRetroCars.Models;

public class FavoriteCar
{
    public int Id { get; set; }
    public int CarStoryId { get; set; }
    public CarStory? CarStory { get; set; }
    public string UserId { get; set; } = string.Empty;
}