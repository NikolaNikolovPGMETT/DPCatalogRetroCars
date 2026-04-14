using DPCatalogRetroCars.Data;
using DPCatalogRetroCars.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DPCatalogRetroCars.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.PendingStories = await _db.CarStories.Where(s => s.StoryStatus == StoryStatus.Pending).CountAsync();
        ViewBag.HiddenComments = await _db.Comments.Where(c => c.IsHidden).CountAsync();
        return View(await _db.CarStories.OrderByDescending(c => c.CreatedOnUtc).Take(20).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStoryStatus(int id, StoryStatus status)
    {
        var story = await _db.CarStories.FindAsync(id);
        if (story is null)
        {
            return NotFound();
        }

        story.StoryStatus = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Comments()
    {
        var comments = await _db.Comments.OrderByDescending(c => c.CreatedOnUtc).Take(200).ToListAsync();
        return View(comments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCommentVisibility(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment is null)
        {
            return NotFound();
        }

        comment.IsHidden = !comment.IsHidden;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Comments));
    }
}
