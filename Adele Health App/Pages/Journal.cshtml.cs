using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Adele_Health_App.Models;
using Adele_Health_App.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Adele_Health_App.Pages
{
    public class JournalModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdeleHealthAppUser> _userManager;

        public JournalModel(ApplicationDbContext context, UserManager<AdeleHealthAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)] public string Filter { get; set; }
        [BindProperty(SupportsGet = true)] public string Start { get; set; }
        [BindProperty(SupportsGet = true)] public string End { get; set; }

        public List<EntrySectionModel> EntrySections { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            // Get all entries for the current user
            var entries = _context.LifestyleEntries
                .Where(e => e.UserId == userId)
                .ToList();

            // Parse all valid entries with a proper date
            var parsedEntries = entries
                .Where(e => DateTime.TryParse(e.Date, out _))
                .Select(e => new
                {
                    Entry = e,
                    ParsedDate = DateTime.Parse(e.Date)
                })
                .ToList();

            // Apply date filter
            DateTime now = DateTime.Today;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;

            if (Filter == "custom" && DateTime.TryParse(Start, out var customStart) && DateTime.TryParse(End, out var customEnd))
            {
                startDate = customStart.Date;
                endDate = customEnd.Date;
            }
            else
            {
                switch (Filter?.ToLower())
                {
                    case "today":
                        startDate = now;
                        endDate = now;
                        break;
                    case "week":
                        startDate = now.AddDays(-7);
                        break;
                    case "1m":
                        startDate = now.AddMonths(-1);
                        break;
                    case "3m":
                        startDate = now.AddMonths(-3);
                        break;
                    case "1yr":
                        startDate = now.AddYears(-1);
                        break;
                }
            }

            var filtered = parsedEntries
                .Where(e => e.ParsedDate >= startDate && e.ParsedDate <= endDate)
                .GroupBy(e => e.ParsedDate.Date)
                .OrderByDescending(g => g.Key);

            foreach (var group in filtered)
            {
                string dateText = group.Key == now ? $"Today, {group.Key:MMM dd, yyyy}"
                                : group.Key == now.AddDays(-1) ? $"Yesterday, {group.Key:MMM dd, yyyy}"
                                : group.Key.ToString("MMM dd, yyyy");

                var entryModels = group.Select(e => new EntryModel
                {
                    Id = e.Entry.Id,
                    Time = ConvertTimeTo12Hour(e.Entry.Time),
                    Reading = e.Entry.Glucose,
                    MealTiming = e.Entry.MealTiming,
                    Date = group.Key.ToString("MMM dd, yyyy"),
                    GlucoseResponse = e.Entry.SelectedGlucose,
                    Notes = e.Entry.Notes,
                    Measurements = new List<MeasurementModel>
                    {
                        CreateMeasurement("Exercise", e.Entry.SelectedExercise, e.Entry.AddedTags, new[] { "Walking", "Running", "Yoga", "Gym", "Cycling" }),
                        CreateMeasurement("Nutrition", e.Entry.SelectedNutrition, e.Entry.AddedTags, new[] { "Chicken", "Beef", "Steak", "French Fries" }),
                        CreateMeasurement("Medication", e.Entry.SelectedMedication, e.Entry.AddedTags, new[] { "Insulin", "Metformin", "Advil" }),
                        CreateMeasurement("Hydration", e.Entry.SelectedHydration, e.Entry.AddedTags, new[] { "Coke", "Sprite", "Water", "Gatorade", "Coffee", "Tea" }),
                        CreateMeasurement("Sleep", e.Entry.SelectedSleep, e.Entry.AddedTags, new[] { "Nap", "Deep Sleep", "Interrupted" }),
                        CreateMeasurement("Stress", e.Entry.SelectedStress, e.Entry.AddedTags, new[] { "Work", "Family", "Finance", "Health" })
                    }
                }).ToList();

                EntrySections.Add(new EntrySectionModel
                {
                    DateText = dateText,
                    Entries = entryModels
                });
            }
        }

        public async Task<IActionResult> OnPostUpdateEntryAsync(int id, string time, string reading, string mealTiming)
        {
            var userId = _userManager.GetUserId(User);
            var entry = await _context.LifestyleEntries.FindAsync(id);

            if (entry == null || entry.UserId != userId)
            {
                return NotFound();
            }

            entry.Time = time;
            entry.Glucose = reading;
            entry.MealTiming = mealTiming;

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        private string ConvertTimeTo12Hour(string militaryTime)
        {
            if (TimeSpan.TryParse(militaryTime, out var parsed))
            {
                return DateTime.Today.Add(parsed).ToString("h:mm tt", CultureInfo.InvariantCulture);
            }
            return militaryTime;
        }

        private MeasurementModel CreateMeasurement(string name, string value, string tagsCsv, string[] validTags)
        {
            var tagList = new List<string>();
            if (!string.IsNullOrWhiteSpace(tagsCsv))
            {
                tagList = tagsCsv.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => validTags.Contains(t, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }

            return new MeasurementModel
            {
                Name = name,
                Value = value ?? "",
                Tags = tagList
            };
        }
    }
}