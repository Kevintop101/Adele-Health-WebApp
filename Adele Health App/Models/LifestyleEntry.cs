
namespace Adele_Health_App.Models
{
    public class LifestyleEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string? Date { get; set; }
        public string? Time { get; set; }
        public string? Glucose { get; set; }
        public string? MealTiming { get; set; }

        public string? SelectedGlucose { get; set; }
        public string? SelectedExercise { get; set; }
        public string? SelectedNutrition { get; set; }
        public string? SelectedMedication { get; set; }
        public string? SelectedHydration { get; set; }
        public string? SelectedSleep { get; set; }
        public string? SelectedStress { get; set; }

        public string? AddedTags { get; set; }
        public string? Notes { get; set; }
    }
}
