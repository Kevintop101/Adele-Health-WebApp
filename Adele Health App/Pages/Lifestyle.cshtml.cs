using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Adele_Health_App.Models;
using System.Threading.Tasks;
using Adele_Health_App.Areas.Identity.Data;

namespace Adele_Health_App.Pages
{
    public class LifestyleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdeleHealthAppUser> _userManager;

        public LifestyleModel(ApplicationDbContext context, UserManager<AdeleHealthAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)] public string Date { get; set; }
        [BindProperty(SupportsGet = true)] public string Time { get; set; }
        [BindProperty(SupportsGet = true)] public string Glucose { get; set; }
        [BindProperty(SupportsGet = true)] public string MealTiming { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedGlucose { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedExercise { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedNutrition { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedMedication { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedHydration { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedSleep { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedStress { get; set; }
        [BindProperty(SupportsGet = true)] public string AddedTags { get; set; }
        [BindProperty(SupportsGet = true)] public string Notes { get; set; }

        public void OnGet()
        {
            // Prefill logic happens automatically through SupportsGet query binding.
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);

            var entry = new LifestyleEntry
            {
                UserId = userId,
                Date = Date,
                Time = Time,
                Glucose = Glucose,
                MealTiming = MealTiming,
                SelectedGlucose = SelectedGlucose,
                SelectedExercise = SelectedExercise,
                SelectedNutrition = SelectedNutrition,
                SelectedMedication = SelectedMedication,
                SelectedHydration = SelectedHydration,
                SelectedSleep = SelectedSleep,
                SelectedStress = SelectedStress,
                AddedTags = AddedTags,
                Notes = Notes
            };

            _context.LifestyleEntries.Add(entry);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Lifestyle", new
            {
                date = Date,
                time = Time,
                glucose = Glucose,
                mealTiming = MealTiming,
                selectedGlucose = SelectedGlucose,
                selectedExercise = SelectedExercise,
                selectedNutrition = SelectedNutrition,
                selectedMedication = SelectedMedication,
                selectedHydration = SelectedHydration,
                selectedSleep = SelectedSleep,
                selectedStress = SelectedStress,
                addedTags = AddedTags,
                notes = Notes
            });
        }
    }
}
