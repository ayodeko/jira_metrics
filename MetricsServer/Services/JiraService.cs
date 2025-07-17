using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace MetricsServer.Services
{
    public class JiraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _email;
        private readonly string _apiToken;

        public JiraService(IConfiguration configuration)
        {
            _baseUrl = configuration["Jira:BaseUrl"] ?? string.Empty;
            _email = configuration["Jira:Email"] ?? string.Empty;
            _apiToken = configuration["Jira:ApiToken"] ?? string.Empty;

            _httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public JiraService(string baseUrl, string email, string apiToken)
        {
            _baseUrl = baseUrl;
            _email = email;
            _apiToken = apiToken;

            _httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetIssuesAsync(string jql, int maxResults = 50)
        {
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults={maxResults}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAllIssuesAsync(string jql, int maxResultsPerPage = 100)
        {
            // First, get the total number of issues (and the first page)
            var url0 = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&startAt=0&maxResults=100&expand=changelog&fields=*all";
            var response0 = await _httpClient.GetAsync(url0);
            response0.EnsureSuccessStatusCode();
            var json0 = await response0.Content.ReadAsStringAsync();
            var doc0 = JsonDocument.Parse(json0);
            int total = 0;
            if (doc0.RootElement.TryGetProperty("total", out var totalElement))
                total = totalElement.GetInt32();
            var allIssues = new List<JsonElement>();
            if (doc0.RootElement.TryGetProperty("issues", out var issuesArray0))
                allIssues.AddRange(issuesArray0.EnumerateArray());

            // If total <= 100, we're done
            if (total <= 100)
            {
                var result0 = new
                {
                    expand = "changelog",
                    startAt = 0,
                    maxResults = allIssues.Count,
                    total = allIssues.Count,
                    issues = allIssues
                };
                return JsonSerializer.Serialize(result0);
            }

            // Otherwise, fire off up to 9 more requests in parallel (for up to 1000 issues)
            var tasks = new List<Task<string>>();
            for (int i = 1; i < 10; i++)
            {
                int startAt = i * 100;
                if (startAt >= total) break;
                var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&startAt={startAt}&maxResults=100&expand=changelog&fields=*all";
                tasks.Add(_httpClient.GetStringAsync(url));
            }
            var results = await Task.WhenAll(tasks);
            foreach (var json in results)
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("issues", out var issuesArray))
                {
                    allIssues.AddRange(issuesArray.EnumerateArray());
                }
            }
            // Reconstruct the response with all issues
            var result = new
            {
                expand = "changelog",
                startAt = 0,
                maxResults = allIssues.Count,
                total = allIssues.Count,
                issues = allIssues
            };
            return JsonSerializer.Serialize(result);
        }
    }
} 