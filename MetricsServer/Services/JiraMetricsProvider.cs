using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MetricsServer.Models;

namespace MetricsServer.Services
{
    public class JiraMetricsProvider : IMetricsProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _email;
        private readonly string _apiToken;
        private readonly ILogger<JiraMetricsProvider> _logger;
        private List<Issue>? _issues;
        private List<Sprint>? _sprints;
        private List<Epic>? _epics;
        private bool _initialized = false;
        private readonly object _lock = new object();

        public JiraMetricsProvider(IConfiguration configuration, ILogger<JiraMetricsProvider> logger)
        {
            _baseUrl = configuration["Jira:BaseUrl"] ?? string.Empty;
            _email = configuration["Jira:Email"] ?? string.Empty;
            _apiToken = configuration["Jira:ApiToken"] ?? string.Empty;
            _logger = logger;

            _httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            InitializeAsync().GetAwaiter().GetResult();
        }

        public JiraMetricsProvider(string baseUrl, string email, string apiToken, ILogger<JiraMetricsProvider> logger)
        {
            _baseUrl = baseUrl;
            _email = email;
            _apiToken = apiToken;
            _logger = logger;

            _httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                _initialized = true;
            }
            try
            {
                _logger.LogInformation("Authenticating with Jira and fetching issues at startup...");
                string jql = "ORDER BY created DESC";
                var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100&expand=changelog&fields=*all";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("issues", out var issuesArray))
                {
                    _issues = new List<Issue>();
                    foreach (var issue in issuesArray.EnumerateArray())
                    {
                        _issues.Add(new Issue(issue));
                    }
                    _logger.LogInformation($"Fetched {_issues.Count} issues from Jira.");
                }
                else
                {
                    _logger.LogWarning("No 'issues' property found in Jira response.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating or fetching issues from Jira.");
                throw; // Rethrow to ensure initialization failure is known
            }
        }

        public async Task<List<Issue>> FetchIssuesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching issues from Jira...");
                string jql = "ORDER BY created DESC";
                var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100&expand=changelog&fields=*all";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var issuesList = new List<Issue>();
                if (doc.RootElement.TryGetProperty("issues", out var issuesArray))
                {
                    foreach (var issue in issuesArray.EnumerateArray())
                    {
                        issuesList.Add(new Issue(issue));
                    }
                    _logger.LogInformation($"Fetched {issuesList.Count} issues from Jira.");
                }
                else
                {
                    _logger.LogWarning("No 'issues' property found in Jira response.");
                }
                _issues = issuesList;
                return _issues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching issues from Jira in FetchIssuesAsync.");
                _issues = new List<Issue>();
                return _issues;
            }
        }

        private async Task FetchSprintsAsync()
        {
            // TODO: Implement actual Jira API call
            _sprints = new List<Sprint>();
        }

        private async Task FetchEpicsAsync()
        {
            // TODO: Implement actual Jira API call
            _epics = new List<Epic>();
        }

        public async Task<string> GetIssuesAsync(string jql, int maxResults = 50)
        {
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults={maxResults}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public Task<double> CalculateLeadTimeAsync()
        {
            try
            {
                _logger.LogInformation($"Total issues fetched: {_issues?.Count ?? 0}");
                var leadTimes = new List<double>();
                foreach (var issue in _issues ?? new List<Issue>())
                {
                    if (issue.Fields.TryGetProperty("status", out var status) &&
                        status.TryGetProperty("statusCategory", out var statusCategory) &&
                        statusCategory.TryGetProperty("name", out var categoryName) &&
                        string.Equals(categoryName.GetString(), "Done", StringComparison.OrdinalIgnoreCase))
                    {
                        if (issue.Fields.TryGetProperty("created", out var createdProp) &&
                            issue.Fields.TryGetProperty("resolutiondate", out var resolvedProp))
                        {
                            var createdStr = createdProp.GetString() ?? string.Empty;
                            var resolvedStr = resolvedProp.GetString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(createdStr) && !string.IsNullOrEmpty(resolvedStr) &&
                                DateTime.TryParse(createdStr, out var created) &&
                                DateTime.TryParse(resolvedStr, out var resolved))
                            {
                                var days = (resolved - created).TotalDays;
                                leadTimes.Add(days);
                                string key = issue.Key ?? "<no-key>";
                                string summary = (issue.Fields.TryGetProperty("summary", out var s) ? s.GetString() : "<no-summary>") ?? "<no-summary>";
                                _logger.LogInformation($"LeadTime Issue: {key} - {summary} | Created: {created}, Resolved: {resolved}, Days: {days}");
                            }
                        }
                    }
                }
                double avg = leadTimes.Count > 0 ? leadTimes.Average() : 0.0;
                _logger.LogInformation($"Lead Time Calculation: {leadTimes.Count} issues, Average: {avg} days");
                return Task.FromResult(Math.Round(avg, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Lead Time");
                return Task.FromResult(0.0);
            }
        }

        public Task<double> CalculateCycleTimeAsync()
        {
            try
            {
                _logger.LogInformation($"Total issues fetched: {_issues?.Count ?? 0}");
                var cycleTimes = new List<double>();
                foreach (var issue in _issues ?? new List<Issue>())
                {
                    if (issue.Fields.TryGetProperty("status", out var status) &&
                        status.TryGetProperty("statusCategory", out var statusCategory) &&
                        statusCategory.TryGetProperty("name", out var categoryName) &&
                        string.Equals(categoryName.GetString(), "Done", StringComparison.OrdinalIgnoreCase))
                    {
                        if (issue.Fields.TryGetProperty("created", out var createdProp) &&
                            issue.Fields.TryGetProperty("resolutiondate", out var resolvedProp))
                        {
                            var createdStr = createdProp.GetString() ?? string.Empty;
                            var resolvedStr = resolvedProp.GetString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(createdStr) && !string.IsNullOrEmpty(resolvedStr) &&
                                DateTime.TryParse(createdStr, out var created) &&
                                DateTime.TryParse(resolvedStr, out var resolved))
                            {
                                var days = (resolved - created).TotalDays;
                                cycleTimes.Add(days);
                                string key = issue.Key ?? "<no-key>";
                                string summary = (issue.Fields.TryGetProperty("summary", out var s) ? s.GetString() : "<no-summary>") ?? "<no-summary>";
                                _logger.LogInformation($"CycleTime Issue: {key} - {summary} | Created: {created}, Resolved: {resolved}, Days: {days}");
                            }
                        }
                    }
                }
                double avg = cycleTimes.Count > 0 ? cycleTimes.Average() : 0.0;
                avg = Math.Round(avg, 2);
                _logger.LogInformation($"Cycle Time Calculation: {cycleTimes.Count} issues, Average: {avg} days");
                return Task.FromResult(avg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Cycle Time");
                return Task.FromResult(0.0);
            }
        }

        public Task<int> CalculateThroughputAsync()
        {
            try
            {
                _logger.LogInformation($"Total issues fetched: {_issues?.Count ?? 0}");
                var throughputIssues = _issues?.Where(issue =>
                {
                    if (issue.Fields.TryGetProperty("status", out var status) &&
                        status.TryGetProperty("statusCategory", out var statusCategory) &&
                        statusCategory.TryGetProperty("name", out var categoryName) &&
                        categoryName.GetString() != null)
                    {
                        bool isDone = string.Equals(categoryName.GetString(), "Done", StringComparison.OrdinalIgnoreCase);
                        if (isDone)
                        {
                            string key = issue.Key ?? "<no-key>";
                            string summary = (issue.Fields.TryGetProperty("summary", out var s) ? s.GetString() : "<no-summary>") ?? "<no-summary>";
                            _logger.LogInformation($"Throughput Issue: {key} - {summary}");
                        }
                        return isDone;
                    }
                    return false;
                }).ToList() ?? new List<Issue>();
                _logger.LogInformation($"Throughput Calculation: {throughputIssues.Count} issues completed out of {_issues?.Count ?? 0} total.");
                return Task.FromResult(throughputIssues.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Throughput");
                return Task.FromResult(0);
            }
        }

        public Task<int> CalculateWorkInProgressAsync()
        {
            try
            {
                _logger.LogInformation($"Total issues fetched: {_issues?.Count ?? 0}");
                var wipIssues = _issues?.Where(issue =>
                {
                    if (issue.Fields.TryGetProperty("status", out var status) &&
                        status.TryGetProperty("statusCategory", out var statusCategory) &&
                        statusCategory.TryGetProperty("name", out var categoryName) &&
                        categoryName.GetString() != null)
                    {
                        bool isWip = !string.Equals(categoryName.GetString(), "Done", StringComparison.OrdinalIgnoreCase);
                        if (isWip)
                        {
                            string key = issue.Key ?? "<no-key>";
                            string summary = (issue.Fields.TryGetProperty("summary", out var s) ? s.GetString() : "<no-summary>") ?? "<no-summary>";
                            _logger.LogInformation($"WIP Issue: {key} - {summary}");
                        }
                        return isWip;
                    }
                    return false;
                }).ToList() ?? new List<Issue>();
                _logger.LogInformation($"WIP Calculation: {wipIssues.Count} issues in progress out of {_issues?.Count ?? 0} total.");
                return Task.FromResult(wipIssues.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Work In Progress");
                return Task.FromResult(0);
            }
        }

        public Task<double> CalculateReopenRateAsync()
        {
            try
            {
                _logger.LogInformation($"Total issues fetched: {_issues?.Count ?? 0}");
                int reopenedCount = 0;
                int totalDone = 0;
                foreach (var issue in _issues ?? new List<Issue>())
                {
                    if (issue.Fields.TryGetProperty("status", out var status) &&
                        status.TryGetProperty("statusCategory", out var statusCategory) &&
                        statusCategory.TryGetProperty("name", out var categoryName) &&
                        categoryName.GetString() != null &&
                        string.Equals(categoryName.GetString(), "Done", StringComparison.OrdinalIgnoreCase))
                    {
                        totalDone++;
                        // Check changelog for multiple transitions to 'Done'
                        if (issue.Changelog.TryGetProperty("histories", out var histories))
                        {
                            int doneTransitions = 0;
                            foreach (var history in histories.EnumerateArray())
                            {
                                if (history.TryGetProperty("items", out var items))
                                {
                                    foreach (var item in items.EnumerateArray())
                                    {
                                        if (item.TryGetProperty("field", out var field) &&
                                            item.TryGetProperty("toString", out var toString) &&
                                            field.GetString() == "status" &&
                                            toString.GetString() != null &&
                                            string.Equals(toString.GetString(), "Done", StringComparison.OrdinalIgnoreCase))
                                        {
                                            doneTransitions++;
                                        }
                                    }
                                }
                            }
                            if (doneTransitions > 1)
                            {
                                reopenedCount++;
                                string key = issue.Key ?? "<no-key>";
                                string summary = (issue.Fields.TryGetProperty("summary", out var s) ? s.GetString() : "<no-summary>") ?? "<no-summary>";
                                _logger.LogInformation($"Reopened Issue: {key} - {summary} | Done transitions: {doneTransitions}");
                            }
                        }
                    }
                }
                double rate = totalDone > 0 ? (double)reopenedCount / totalDone * 100.0 : 0.0;
                _logger.LogInformation($"Reopen Rate Calculation: {reopenedCount} reopened out of {totalDone} done issues. Rate: {rate}%");
                return Task.FromResult(Math.Round(rate, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Reopen Rate");
                return Task.FromResult(0.0);
            }
        }

        public Task<TimeInStatusData> GetTimeInStatusDataAsync()
        {
            try
            {
                var result = new TimeInStatusData();
                var statusDurations = new Dictionary<string, List<double>>();

                int totalIssues = _issues?.Count ?? 0;
                int issuesWithChangelog = 0;
                int totalHistories = 0;
                int totalTransitions = 0;
                int skippedDurations = 0;

                foreach (var issue in _issues ?? new List<Issue>())
                {
                    bool foundTransition = false;
                    if (issue.Changelog.TryGetProperty("histories", out var histories))
                    {
                        issuesWithChangelog++;
                        totalHistories += histories.GetArrayLength();
                        DateTime? lastStatusChange = null;
                        string? currentStatus = null;

                        // Sort histories by 'created' ascending
                        var sortedHistories = histories.EnumerateArray()
                            .OrderBy(h => DateTime.Parse(h.GetProperty("created").GetString()!));

                        foreach (var history in sortedHistories)
                        {
                            if (history.TryGetProperty("created", out var created) &&
                                history.TryGetProperty("items", out var items))
                            {
                                foreach (var item in items.EnumerateArray())
                                {
                                    if (item.TryGetProperty("field", out var field) &&
                                        field.GetString() == "status")
                                    {
                                        foundTransition = true;
                                        totalTransitions++;
                                        if (lastStatusChange.HasValue && currentStatus != null)
                                        {
                                            var prev = lastStatusChange.Value;
                                            var curr = DateTime.Parse(created.GetString()!);
                                            var duration = (curr - prev).TotalDays;
                                            if (duration > 0)
                                            {
                                                if (!statusDurations.ContainsKey(currentStatus!))
                                                {
                                                    statusDurations[currentStatus!] = new List<double>();
                                                }
                                                statusDurations[currentStatus!].Add(duration);
                                            }
                                            else
                                            {
                                                skippedDurations++;
                                                _logger.LogWarning($"Skipping negative or zero duration for status '{currentStatus}' in issue due to out-of-order or duplicate transitions. prev={prev}, curr={curr}, duration={duration}");
                                            }
                                        }
                                        lastStatusChange = DateTime.Parse(created.GetString()!);
                                        if (item.TryGetProperty("toString", out var toString))
                                        {
                                            currentStatus = toString.GetString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!foundTransition)
                    {
                        _logger.LogInformation($"No status transitions found for issue {issue.Key}");
                    }
                }

                foreach (var status in statusDurations)
                {
                    result.StatusDurations.Add(new StatusDuration
                    {
                        Status = status.Key,
                        AverageDays = status.Value.Any() ? status.Value.Average() : 0
                    });
                }

                _logger.LogInformation($"TimeInStatus: Total issues: {totalIssues}, Issues with changelog: {issuesWithChangelog}, Total histories: {totalHistories}, Total status transitions: {totalTransitions}, Skipped durations: {skippedDurations}, Statuses with durations: {result.StatusDurations.Count}");
                if (result.StatusDurations.Count == 0)
                {
                    _logger.LogWarning("TimeInStatus: No status durations found. Check if issues have changelogs and status transitions.");
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Time in Status data");
                return Task.FromResult(new TimeInStatusData());
            }
        }

        public async Task<TrendData> GetTrendDataAsync()
        {
            try
            {
                var result = new TrendData();
                var issues = _issues ?? new List<Issue>();
                if (issues.Count == 0)
                {
                    await FetchIssuesAsync();
                    issues = _issues ?? new List<Issue>();
                }
                _logger.LogInformation($"TrendData: Starting calculation with {issues.Count} total issues.");

                // Filter for issues with resolution dates
                var resolvedIssues = issues
                    .Where(issue => issue.Fields.TryGetProperty("resolutiondate", out var resolved) && resolved.ValueKind != JsonValueKind.Null)
                    .ToList();
                _logger.LogInformation($"TrendData: Found {resolvedIssues.Count} issues with a resolution date.");

                if (resolvedIssues.Count == 0)
                {
                    _logger.LogWarning("TrendData: No issues with a resolution date found. Returning empty trend data.");
                    return result;
                }

                // Group issues by week of resolution
                var grouped = resolvedIssues
                    .Select(issue => {
                        try
                        {
                            var createdDate = DateTime.Parse(issue.Fields.GetProperty("created").GetString()!);
                            var resolvedDate = DateTime.Parse(issue.Fields.GetProperty("resolutiondate").GetString()!);
                            var week = System.Globalization.ISOWeek.GetWeekOfYear(resolvedDate);
                            var year = resolvedDate.Year;
                            var leadTime = (resolvedDate - createdDate).TotalDays;
                            return new { year, week, createdDate, resolvedDate, leadTime, issue.Key };
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"TrendData: Could not parse dates for issue {issue.Key}. Skipping. Error: {ex.Message}");
                            return null;
                        }
                    })
                    .Where(x => x != null)
                    .GroupBy(x => new { x!.year, x!.week })
                    .OrderBy(g => g.Key.year).ThenBy(g => g.Key.week);

                _logger.LogInformation($"TrendData: Grouped resolved issues into {grouped.Count()} weekly groups.");

                foreach (var group in grouped)
                {
                    var weekLabel = $"{group.Key.year}-W{group.Key.week:D2}";
                    var avgLeadTime = group.Average(x => x.leadTime);
                    var avgCycleTime = avgLeadTime; // For now, same as lead time
                    var throughput = group.Count();
                    result.LeadTime.Add(new MetricTrend { Date = new DateTime(group.Key.year, 1, 1).AddDays((group.Key.week - 1) * 7), Value = Math.Round(avgLeadTime, 2) });
                    result.CycleTime.Add(new MetricTrend { Date = new DateTime(group.Key.year, 1, 1).AddDays((group.Key.week - 1) * 7), Value = Math.Round(avgCycleTime, 2) });
                    result.Throughput.Add(new MetricTrend { Date = new DateTime(group.Key.year, 1, 1).AddDays((group.Key.week - 1) * 7), Value = throughput });
                }

                _logger.LogInformation($"TrendData: Processed {result.LeadTime.Count} data points for the trend chart.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Trend data");
                return new TrendData();
            }
        }

        public async Task<CumulativeFlowData> GetCumulativeFlowDataAsync()
        {
            try
            {
                var result = new CumulativeFlowData();
                var issues = _issues ?? new List<Issue>();
                if (!issues.Any()) return result;

                var startDate = DateTime.Now.AddDays(-30).Date;
                var endDate = DateTime.Now.Date;

                // Create a list of all possible statuses from the issues
                var allStatuses = issues
                    .Select(i => i.Fields.TryGetProperty("status", out var s) && s.TryGetProperty("name", out var n) ? n.GetString() : null)
                    .Where(s => s != null)
                    .Distinct()
                    .ToList();

                // Create a map of issue key to its status transitions
                var issueStatusTransitions = new Dictionary<string, List<(DateTime Date, string Status)>>();

                foreach (var issue in issues)
                {
                    var key = issue.Key;
                    if (key == null) continue;
                    issueStatusTransitions[key] = new List<(DateTime, string)>();
                    if (issue.Changelog.TryGetProperty("histories", out var histories))
                    {
                        foreach (var history in histories.EnumerateArray())
                        {
                            if (history.TryGetProperty("created", out var created) &&
                                DateTime.TryParse(created.GetString(), out var date))
                            {
                                foreach (var item in history.GetProperty("items").EnumerateArray())
                                {
                                    if (item.TryGetProperty("field", out var field) &&
                                        field.GetString() == "status" &&
                                        item.TryGetProperty("toString", out var toString))
                                    {
                                        var status = toString.GetString();
                                        if (status != null)
                                        {
                                            issueStatusTransitions[key].Add((date, status));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (var day = startDate; day <= endDate; day = day.AddDays(1))
                {
                    var dailyCounts = new Dictionary<string, int>();
                    foreach (var status in allStatuses)
                    {
                        if (status != null) dailyCounts[status] = 0;
                    }

                    foreach (var issueKey in issueStatusTransitions.Keys)
                    {
                        var lastKnownStatus = issueStatusTransitions[issueKey]
                            .Where(t => t.Date <= day)
                            .OrderByDescending(t => t.Date)
                            .FirstOrDefault();

                        if (lastKnownStatus != default && lastKnownStatus.Status != null)
                        {
                            dailyCounts[lastKnownStatus.Status]++;
                        }
                    }

                    result.DataPoints.Add(new FlowPoint { Date = day, StatusCounts = dailyCounts });
                }

                _logger.LogInformation($"CumulativeFlow: Processed {result.DataPoints.Count} days for CFD chart.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Cumulative Flow data");
                return new CumulativeFlowData();
            }
        }

        public Task<TrendData> GetTrendAsync()
        {
            return GetTrendDataAsync();
        }

        public Task<double> GetLeadTimeAsync()
        {
            return CalculateLeadTimeAsync();
        }

        public Task<double> GetCycleTimeAsync()
        {
            return CalculateCycleTimeAsync();
        }

        public Task<int> GetThroughputAsync()
        {
            return CalculateThroughputAsync();
        }

        public Task<int> GetWorkInProgressAsync()
        {
            return CalculateWorkInProgressAsync();
        }

        public Task<double> GetReopenRateAsync()
        {
            return CalculateReopenRateAsync();
        }

        public async Task<List<Issue>> GetIssuesAsync()
        {
            if (_issues == null)
            {
                await FetchIssuesAsync();
            }
            return _issues ?? new List<Issue>();
        }

        public Task<CumulativeFlowData> GetCumulativeFlowAsync()
        {
            return GetCumulativeFlowDataAsync();
        }

        public Task<TimeInStatusData> GetTimeInStatusAsync()
        {
            return GetTimeInStatusDataAsync();
        }

        public class IssueTableRow
        {
            public string Key { get; set; } = string.Empty;
            public string Summary { get; set; } = string.Empty;
            public string Created { get; set; } = string.Empty;
            public string Done { get; set; } = string.Empty;
            public string LeadTime { get; set; } = string.Empty;
            public string CycleTime { get; set; } = string.Empty;
            public string Sprint { get; set; } = string.Empty;
        }

        public async Task<(List<IssueTableRow> Issues, int Total)> GetIssueTableRowsAsync(int page = 1, int pageSize = 20)
        {
            if (_issues == null)
            {
                await FetchIssuesAsync();
            }
            var allIssues = _issues ?? new List<Issue>();
            // Sort by resolution date descending
            var sorted = allIssues.OrderByDescending(issue =>
                issue.Fields.TryGetProperty("resolutiondate", out var done) && DateTime.TryParse(done.GetString(), out var doneDt)
                    ? doneDt : DateTime.MinValue).ToList();
            var total = sorted.Count;
            var paged = sorted.Skip((page - 1) * pageSize).Take(pageSize);
            var result = new List<IssueTableRow>();
            foreach (var issue in paged)
            {
                var row = new IssueTableRow();
                row.Key = issue.Key ?? string.Empty;
                if (issue.Fields.TryGetProperty("summary", out var summary))
                    row.Summary = summary.GetString() ?? string.Empty;
                if (issue.Fields.TryGetProperty("created", out var created))
                    row.Created = DateTime.TryParse(created.GetString(), out var createdDt) ? createdDt.ToString("yyyy-MM-dd") : created.GetString() ?? string.Empty;
                if (issue.Fields.TryGetProperty("resolutiondate", out var done))
                    row.Done = DateTime.TryParse(done.GetString(), out var doneDt) ? doneDt.ToString("yyyy-MM-dd") : done.GetString() ?? string.Empty;
                // LeadTime and CycleTime: (resolutiondate - created) in days
                string createdStr = row.Created;
                string doneStr = row.Done;
                if (!string.IsNullOrEmpty(createdStr) && !string.IsNullOrEmpty(doneStr)
                    && DateTime.TryParse(createdStr, out var createdDt2)
                    && DateTime.TryParse(doneStr, out var doneDt2))
                {
                    var days = (doneDt2 - createdDt2).TotalDays;
                    row.LeadTime = days.ToString("0.##");
                    row.CycleTime = days.ToString("0.##");
                }
                else
                {
                    row.LeadTime = "";
                    row.CycleTime = "";
                }
                // Sprint: try fields.sprint.name or fields.customfield_10021[0].name (common Jira sprint field)
                string sprintName = string.Empty;
                if (issue.Fields.TryGetProperty("sprint", out var sprint) && sprint.ValueKind != JsonValueKind.Null)
                    sprintName = sprint.GetString() ?? string.Empty;
                row.Sprint = sprintName;
                result.Add(row);
            }
            return (result, total);
        }
    }
} 