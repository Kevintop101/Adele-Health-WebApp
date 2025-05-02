namespace Adele_Health_App.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Glucose { get; set; }
        public string MealTiming { get; set; }
        public string Notes { get; set; }
    }
}
