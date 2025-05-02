using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Adele_Health_App.Models;
using System.Threading.Tasks;
using Adele_Health_App.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace Adele_Health_App.Pages
{
    [Authorize]
    public class LoggingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdeleHealthAppUser> _userManager;

        public LoggingModel(ApplicationDbContext context, UserManager<AdeleHealthAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty] public string Date { get; set; }
        [BindProperty] public string Time { get; set; }
        [BindProperty] public string Glucose { get; set; }
        [BindProperty] public string MealTiming { get; set; }
        [BindProperty] public string Notes { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);

            var log = new LogEntry
            {
                UserId = userId,
                Date = Date,
                Time = Time,
                Glucose = Glucose,
                MealTiming = MealTiming,
                Notes = Notes
            };

            _context.LogEntries.Add(log);
            await _context.SaveChangesAsync();

            // Send values to Lifestyle page
            TempData["Date"] = Date;
            TempData["Time"] = Time;
            TempData["Glucose"] = Glucose;
            TempData["MealTiming"] = MealTiming;
            TempData["Notes"] = Notes;

            return RedirectToPage("/Lifestyle");
        }
    }
}
