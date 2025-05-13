using EventEaseBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class BookingController : Controller
{
    private readonly AppDbContext _context;

    public BookingController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Booking with search
    public async Task<IActionResult> Index(string searchString)
    {
        var bookingsQuery = _context.Booking
            .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            bookingsQuery = bookingsQuery.Where(b =>
                (b.Event != null && b.Event.EventName.Contains(searchString)) ||
                (b.Event != null && b.Event.Venue != null && b.Event.Venue.VenueName.Contains(searchString)) ||
                b.BookingDate.ToString().Contains(searchString));
        }

        ViewData["CurrentFilter"] = searchString;
        return View(await bookingsQuery.ToListAsync());
    }

    // GET: Booking/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Booking
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(m => m.BookingId == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

    // GET: Booking/Create
    public IActionResult Create()
    {
        SetupCreateViewData();
        return View();
    }

    // POST: Booking/Create (Updated version)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventId == booking.EventId);

        if (selectedEvent == null)
        {
            ModelState.AddModelError("", "Selected event not found.");
            SetupCreateViewData(booking.EventId, booking.VenueId);
            return View(booking);
        }

        // Check manually for double booking
        var conflict = await _context.Booking
            .Include(b => b.Event)
            .AnyAsync(b => b.VenueId == booking.VenueId &&
                           b.Event.EventDate.Date == selectedEvent.EventDate.Date);

        if (conflict)
        {
            ModelState.AddModelError("", "This venue is already booked for that date.");
            SetupCreateViewData(booking.EventId, booking.VenueId);
            return View(booking);
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
                SetupCreateViewData(booking.EventId, booking.VenueId);
                return View(booking);
            }
        }

        SetupCreateViewData(booking.EventId, booking.VenueId);
        return View(booking);
    }

    // Other methods remain unchanged...

    private bool BookingExists(int id)
    {
        return _context.Booking.Any(e => e.BookingId == id);
    }

    private void SetupCreateViewData(int? eventId = null, int? venueId = null)
    {
        ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", eventId);
        ViewBag.VenueId = new SelectList(_context.Venue, "VenueId", "VenueName", venueId);
    }
}
