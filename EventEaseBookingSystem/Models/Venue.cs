using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventEaseBookingSystem.Models;
using Microsoft.AspNetCore.Http; // Needed for IFormFile

public class Venue
{
    [Key]
    public int VenueId { get; set; }

    [Required]
    [StringLength(50)]
    public string VenueName { get; set; }

    [Required]
    [StringLength(50)]
    public string Location { get; set; }

    [Required]
    public int Capacity { get; set; }

    public string? ImageUrl { get; set; }   // for ImageUrl

    [NotMapped]
    public IFormFile? ImageFile { get; set; } // For uploading images

    // Navigation Property for Events
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();  // Initialize as empty list

    // Constructor
    public Venue()
    {
        VenueName = string.Empty;  // Initialize with empty string
        Location = string.Empty;  // Initialize with empty string
    }
}
