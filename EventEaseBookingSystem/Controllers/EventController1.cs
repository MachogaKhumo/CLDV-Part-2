using EventEaseBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class EventController : Controller
{
    private readonly AppDbContext _context;

    public EventController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Event
    public async Task<IActionResult> Index()
    {
        var events = await _context.Event.Include(e => e.Venue).ToListAsync();
        Console.WriteLine($"Total Events Loaded: {events.Count}");
        return View(events);
    }

    // GET: Event/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var eventItem = await _context.Event
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(m => m.EventId == id);
        if (eventItem == null) return NotFound();

        return View(eventItem);
    }

    // GET: Event/Create
    public IActionResult Create()
    {
        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
        return View();
    }

    // POST: Event/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,VenueId,Description")] Event eventItem)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(eventItem);
                Console.WriteLine($"Saving Event: {eventItem.EventName}, {eventItem.EventDate}, {eventItem.Description}, {eventItem.VenueId}");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Save Error: " + ex.Message);
            }
        }

        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine("Validation Error: " + error.ErrorMessage);
        }

        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", eventItem.VenueId);
        return View(eventItem);
    }

    // GET: Event/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var eventItem = await _context.Event
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(m => m.EventId == id);
        if (eventItem == null) return NotFound();

        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", eventItem.VenueId);
        return View(eventItem);
    }

    // POST: Event/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,VenueId,Description")] Event eventItem)
    {
        if (id != eventItem.EventId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Update Error: " + ex.Message);
            }
        }

        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", eventItem.VenueId);
        return View(eventItem);
    }

    // GET: Event/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var eventItem = await _context.Event
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(m => m.EventId == id);
        if (eventItem == null) return NotFound();

        return View(eventItem);
    }

    // POST: Event/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var eventItem = await _context.Event.FindAsync(id);
        if (eventItem == null) return NotFound();

        try
        {
            _context.Event.Remove(eventItem);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Database Delete Error: " + ex.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool EventExists(int id)
    {
        return _context.Event.Any(e => e.EventId == id);
    }
}
