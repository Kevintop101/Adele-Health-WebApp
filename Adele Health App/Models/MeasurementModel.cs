namespace Adele_Health_App.Models
{
    public class MeasurementModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}