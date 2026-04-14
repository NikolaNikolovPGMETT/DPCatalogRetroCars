using DPCatalogRetroCars.Data;
using DPCatalogRetroCars.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DPCatalogRetroCars.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public EventsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var upcoming = await _db.RetroEvents
            .AsNoTracking()
            .Where(e => e.StartsOnUtc >= DateTime.UtcNow.AddDays(-1))
            .OrderBy(e => e.StartsOnUtc)
            .ToListAsync();

        return View(upcoming);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var retroEvent = await _db.RetroEvents
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (retroEvent is null)
        {
            return NotFound();
        }

        ViewBag.IsRegistered = User.Identity?.IsAuthenticated == true
            && retroEvent.Registrations.Any(r => r.UserId == _userManager.GetUserId(User));

        return View(retroEvent);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new RetroEvent
    {
        StartsOnUtc = DateTime.UtcNow.AddDays(7),
        EndsOnUtc = DateTime.UtcNow.AddDays(7).AddHours(6)
    });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RetroEvent retroEvent)
    {
        if (!ModelState.IsValid)
        {
            return View(retroEvent);
        }

        _db.RetroEvents.Add(retroEvent);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var retroEvent = await _db.RetroEvents.FindAsync(id);
        return retroEvent is null ? NotFound() : View(retroEvent);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RetroEvent updated)
    {
        if (id != updated.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(updated);
        }

        _db.RetroEvents.Update(updated);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = updated.Id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var exists = await _db.EventRegistrations.AnyAsync(r => r.RetroEventId == id && r.UserId == userId);
        if (!exists)
        {
            _db.EventRegistrations.Add(new EventRegistration { RetroEventId = id, UserId = userId });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
