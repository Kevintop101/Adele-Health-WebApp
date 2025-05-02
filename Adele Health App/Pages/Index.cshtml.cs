using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Adele_Health_App.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Adele_Health_App.Models;
namespace Adele_Health_App.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SignInManager<AdeleHealthAppUser> _signInManager;
        private readonly UserManager<AdeleHealthAppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        public List<EntrySectionModel> EntrySections { get; set; } = new();
        public string UserDataString { get; set; } = string.Empty; 
        [BindProperty]
        public string UserMessage { get; set; }
        public string AIResponse { get; set; }
        public List<ChatMessage> ChatHistory { get; set; } = new();

        [BindProperty]
        public AdeleHealthAppUser? CurrentUser { get; private set; }
        public string? LastGlucoseReading { get; set; }
        public string MostRecentEntry { get; private set; } = string.Empty;
        public IndexModel(
            ILogger<IndexModel> logger,
            SignInManager<AdeleHealthAppUser> signInManager,
            UserManager<AdeleHealthAppUser> userManager,
            ApplicationDbContext context,
            HttpClient httpClient
        )
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);

            var allEntries = await _context.LifestyleEntries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Time)
                .ToListAsync();

            if (allEntries.Any())
            {
                UserDataString = string.Join("\n", allEntries.Select(entry =>
                    $"Date: {entry.Date} | Time: {DateTime.ParseExact(entry.Time, "HH:mm", CultureInfo.InvariantCulture).ToString("hh:mm tt")} | Glucose: {entry.Glucose} | Notes: {entry.Notes}"));
            }
            if (!string.IsNullOrWhiteSpace(UserMessage))
            {
                var historyJson = HttpContext.Session.GetString("ChatHistory");
                if (!string.IsNullOrEmpty(historyJson))
                {
                    ChatHistory = JsonSerializer.Deserialize<List<ChatMessage>>(historyJson);
                }

                ChatHistory.Add(new ChatMessage { Role = "user", Content = UserMessage });

                AIResponse = await GetChatGptResponse();

                ChatHistory.Add(new ChatMessage { Role = "assistant", Content = AIResponse });

                HttpContext.Session.SetString("ChatHistory", JsonSerializer.Serialize(ChatHistory));
            }

            return Page();
        }

        public async Task OnGetAsync()
        {
            HttpContext.Session.Remove("ChatHistory");
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("No user is currently logged in.");
            }
            else
            {
                _logger.LogInformation($"User {user.FirstName} {user.LastName} is logged in.");

                var lastEntry = await _context.LifestyleEntries
                    .Where(e => e.UserId == user.Id && e.Glucose != null)
                    .OrderByDescending(e => e.Date)
                    .ThenByDescending(e => e.Time)
                    .FirstOrDefaultAsync();

                LastGlucoseReading = lastEntry?.Glucose ?? "None";
            }

            var historyJson = HttpContext.Session.GetString("ChatHistory");
            if (!string.IsNullOrEmpty(historyJson))
            {
                ChatHistory = JsonSerializer.Deserialize<List<ChatMessage>>(historyJson);
            }
        }

        private async Task<string> GetChatGptResponse()
        {
            // api key that you need to change, key is in readme file
            var apiKey = "";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // this is a feature that needs to be improved, i've hard coded this to find terms a user may use to bring their readings up
            var lastUserMessage = ChatHistory.LastOrDefault(m => m.Role == "user")?.Content.ToLower();
            var userRequestedReadings = lastUserMessage != null &&
                (lastUserMessage.Contains("glucose")
                || lastUserMessage.Contains("reading")
                || lastUserMessage.Contains("readings")
                || lastUserMessage.Contains("blood sugar")
                || lastUserMessage.Contains("sugar level")
                || lastUserMessage.Contains("my levels"));

            string readingsData = "";

            if (userRequestedReadings)
            {
                var userId = _userManager.GetUserId(User);
                var allEntries = await _context.LifestyleEntries
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.Date)
                    .ThenByDescending(e => e.Time)
                    .ToListAsync();

                if (allEntries.Any())
                {
                    readingsData = "**Here are your past glucose readings:**\n" +
                        string.Join("\n", allEntries.Select(entry =>
                        $"Date: {entry.Date} | Time: {DateTime.ParseExact(entry.Time, "HH:mm", CultureInfo.InvariantCulture).ToString("hh:mm tt")} | Glucose: {entry.Glucose} | Notes: {entry.Notes}"));
                }
            }

                var body = new
                {
                    model = "gpt-4.1-nano",
                    messages = new List<object>
                {
                    new { role = "system", content = "You are a helpful assistant that provides diabetes tracking insights based on user data. Answer user questions clearly, avoiding strict medical advice." }
                }
                            .Concat(userRequestedReadings && !string.IsNullOrEmpty(readingsData)
                                ? new List<object> { new { role = "system", content = readingsData } }
                                : new List<object>())
                            .Concat(ChatHistory.Select(m => new { role = m.Role, content = m.Content }))
                            .ToList()
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(responseString);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    .Trim();
        }


        public class ChatMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }
}
