using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

        public async Task<string> GetIssuesAsync(string jql, int maxResults = 50)
        {
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults={maxResults}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
} 