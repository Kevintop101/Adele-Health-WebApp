using Microsoft.AspNetCore.Mvc;
using Adele_Health_App.Models;
using Adele_Health_App.Areas.Identity.Data;

namespace Adele_Health_App.Pages.Api
{
    [Route("Api/[controller]")]
    [ApiController]
    public class UpdateEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UpdateEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Post(
            [FromForm] int id,
            [FromForm] string time,
            [FromForm] string reading,
            [FromForm] string mealTiming,
            [FromForm] string? notes // make this nullable
        )
        {
            var entry = _context.LifestyleEntries.FirstOrDefault(e => e.Id == id);
            if (entry == null)
            {
                return NotFound(new { success = false, message = "Entry not found" });
            }

            // Update fields
            entry.Time = time;
            entry.Glucose = reading;
            entry.MealTiming = mealTiming;
            entry.Notes = notes ?? "";

            _context.SaveChanges();

            return Ok(new { success = true });
        }
    }
}
