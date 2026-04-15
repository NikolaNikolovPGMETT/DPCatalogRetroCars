using DPCatalogRetroCars.Data;
using DPCatalogRetroCars.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DPCatalogRetroCars.Controllers;

public class CarsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public CarsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? brand, RestorationStatus? restorationStatus)
    {
        var query = _db.CarStories
            .AsNoTracking()
            .Where(c => c.StoryStatus == StoryStatus.Approved);

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(c => c.Brand.Contains(brand));
        }

        if (restorationStatus.HasValue)
        {
            query = query.Where(c => c.RestorationStatus == restorationStatus.Value);
        }

        ViewBag.Brand = brand;
        ViewBag.RestorationStatus = restorationStatus;
        return View(await query.OrderByDescending(c => c.CreatedOnUtc).ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var car = await _db.CarStories
            .Include(c => c.Comments.Where(x => !x.IsHidden))
            .Include(c => c.Ratings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car is null)
        {
            return NotFound();
        }

        if (car.StoryStatus != StoryStatus.Approved && !User.IsInRole("Admin") && car.OwnerId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        ViewBag.AverageRating = car.Ratings.Any() ? car.Ratings.Average(r => r.Value) : 0;
        ViewBag.UserRating = await GetCurrentUserRatingAsync(id);
        ViewBag.IsFavorite = await IsCurrentUserFavoriteAsync(id);
        return View(car);
    }

    [Authorize]
    public IActionResult Create() => View(new CarStory { Year = DateTime.UtcNow.Year });

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CarStory story)
    {
        story.OwnerId = _userManager.GetUserId(User)!;
        ModelState.Remove(nameof(CarStory.OwnerId));
        if (!ModelState.IsValid)
        {
            return View(story);
        }

        
        story.StoryStatus = StoryStatus.Pending;
        _db.CarStories.Add(story);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(MyStories));
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var story = await _db.CarStories.FindAsync(id);
        if (story is null)
        {
            return NotFound();
        }

        if (!CanManage(story))
        {
            return Forbid();
        }

        return View(story);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CarStory updated)
    {
        ModelState.Remove(nameof(CarStory.OwnerId));
        if (id != updated.Id)
        {
            return BadRequest();
        }

        var story = await _db.CarStories.FindAsync(id);
        if (story is null)
        {
            return NotFound();
        }

        if (!CanManage(story))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(updated);
        }

        story.Brand = updated.Brand;
        story.Model = updated.Model;
        story.Generation = updated.Generation;
        story.Year = updated.Year;
        story.Engine = updated.Engine;
        story.Color = updated.Color;
        story.AcquisitionState = updated.AcquisitionState;
        story.RestorationHistory = updated.RestorationHistory;
        story.FinalResult = updated.FinalResult;
        story.ImageUrl = updated.ImageUrl;
        story.VideoUrl = updated.VideoUrl;
        story.Location = updated.Location;
        story.QrCodeUrl = updated.QrCodeUrl;
        story.RestorationStatus = updated.RestorationStatus;

        if (!User.IsInRole("Admin"))
        {
            story.StoryStatus = StoryStatus.Pending;
        }
        else
        {
            story.StoryStatus = updated.StoryStatus;
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = story.Id });
    }

    [Authorize]
    public async Task<IActionResult> MyStories()
    {
        var userId = _userManager.GetUserId(User)!;
        var stories = await _db.CarStories
            .Where(c => c.OwnerId == userId)
            .OrderByDescending(c => c.CreatedOnUtc)
            .ToListAsync();

        return View(stories);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int carId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return RedirectToAction(nameof(Details), new { id = carId });
        }

        _db.Comments.Add(new Comment
        {
            CarStoryId = carId,
            Content = content,
            UserId = _userManager.GetUserId(User)!
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = carId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int carId, int value)
    {
        value = Math.Clamp(value, 1, 5);
        var userId = _userManager.GetUserId(User)!;

        var rating = await _db.Ratings.FirstOrDefaultAsync(r => r.CarStoryId == carId && r.UserId == userId);
        if (rating is null)
        {
            _db.Ratings.Add(new Rating { CarStoryId = carId, UserId = userId, Value = value });
        }
        else
        {
            rating.Value = value;
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = carId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int carId)
    {
        var userId = _userManager.GetUserId(User)!;
        var favorite = await _db.FavoriteCars.FirstOrDefaultAsync(f => f.CarStoryId == carId && f.UserId == userId);

        if (favorite is null)
        {
            _db.FavoriteCars.Add(new FavoriteCar { CarStoryId = carId, UserId = userId });
        }
        else
        {
            _db.FavoriteCars.Remove(favorite);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = carId });
    }

    [Authorize]
    public async Task<IActionResult> Favorites()
    {
        var userId = _userManager.GetUserId(User)!;
        var cars = await _db.FavoriteCars
            .Where(f => f.UserId == userId)
            .Include(f => f.CarStory)
            .Select(f => f.CarStory!)
            .Where(c => c.StoryStatus == StoryStatus.Approved)
            .ToListAsync();

        return View(cars);
    }

    private bool CanManage(CarStory story)
        => User.IsInRole("Admin") || story.OwnerId == _userManager.GetUserId(User);

    private async Task<int?> GetCurrentUserRatingAsync(int carId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return null;
        }

        var userId = _userManager.GetUserId(User)!;
        return await _db.Ratings.Where(r => r.CarStoryId == carId && r.UserId == userId).Select(r => (int?)r.Value).FirstOrDefaultAsync();
    }

    private async Task<bool> IsCurrentUserFavoriteAsync(int carId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        var userId = _userManager.GetUserId(User)!;
        return await _db.FavoriteCars.AnyAsync(f => f.CarStoryId == carId && f.UserId == userId);
    }
}