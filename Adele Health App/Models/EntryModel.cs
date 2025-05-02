using System.Collections.Generic;

namespace Adele_Health_App.Models
{
    public class EntryModel
    {
        public int Id { get; set; }
        public string Time { get; set; }
        public string Reading { get; set; }
        public string MealTiming { get; set; }
        public string Date { get; set; }
        public string GlucoseResponse { get; set; }
        public string Notes { get; set; }
        public List<MeasurementModel> Measurements { get; set; } = new();
    }
}