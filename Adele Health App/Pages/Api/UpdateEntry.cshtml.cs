using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Adele_Health_App.Areas.Identity.Data;
using Adele_Health_App.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace Adele_Health_App.Pages.Api
{
    public class UpdateEntryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdeleHealthAppUser> _userManager;

        public UpdateEntryModel(ApplicationDbContext context, UserManager<AdeleHealthAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostAsync(int id, string time, string reading, string mealTiming)
        {
            Console.WriteLine($"Received POST - id: {id}, time: {time}, reading: {reading}, mealTiming: {mealTiming}");

            var userId = _userManager.GetUserId(User);
            Console.WriteLine($"User ID: {userId}");

            var entry = _context.LifestyleEntries.FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (entry == null)
            {
                Console.WriteLine("Entry not found or does not belong to user.");
                return new JsonResult(new { success = false, message = "Entry not found" });
            }

            entry.Time = time;
            entry.Glucose = reading;
            entry.MealTiming = mealTiming;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Save successful.");
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save failed: " + ex.Message);
                return new JsonResult(new { success = false, message = "Exception during save" });
            }
        }
    }
}
