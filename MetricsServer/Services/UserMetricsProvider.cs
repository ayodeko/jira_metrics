using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsServer.Models;
using System.Text.Json;

namespace MetricsServer.Services
{
    public class UserMetricsProvider
    {
        private readonly IMetricsProvider _jiraMetricsProvider;
        private List<TeamTableRow>? _cachedTeamRows;
        private string? _lastDataHash;
        private readonly object _lock = new object();

        public UserMetricsProvider(IMetricsProvider jiraMetricsProvider)
        {
            _jiraMetricsProvider = jiraMetricsProvider;
        }

        public async Task<List<TeamTableRow>> GetTeamTableRowsAsync(string? startDate = null, string? endDate = null)
        {
            // Get current issues
            var issues = await _jiraMetricsProvider.GetIssuesAsync();
            if (issues == null) return new List<TeamTableRow>();

            // Filter by date range if provided
            DateTime? start = null;
            DateTime? end = null;
            if (!string.IsNullOrEmpty(startDate)) {
                DateTime tmp;
                if (DateTime.TryParse(startDate, out tmp)) start = tmp;
            }
            if (!string.IsNullOrEmpty(endDate)) {
                DateTime tmp;
                if (DateTime.TryParse(endDate, out tmp)) end = tmp;
            }
            var filteredIssues = issues.Where(issue => {
                if (issue.Fields.TryGetProperty("created", out var createdProp)) {
                    DateTime created;
                    if (DateTime.TryParse(createdProp.GetString(), out created)) {
                        if (start.HasValue && created < start.Value) return false;
                        if (end.HasValue && created > end.Value) return false;
                        return true;
                    }
                }
                return false;
            }).ToList();

            // Create a hash of the current data to detect changes
            var currentDataHash = CreateDataHash(filteredIssues);

            lock (_lock)
            {
                // Check if we have cached results and the data hasn't changed
                if (_cachedTeamRows != null && _lastDataHash == currentDataHash)
                {
                    return _cachedTeamRows;
                }

                // Data has changed or no cache, recalculate
                _lastDataHash = currentDataHash;
                _cachedTeamRows = CalculateTeamMetrics(filteredIssues);
                return _cachedTeamRows;
            }
        }

        private string CreateDataHash(List<Issue> issues)
        {
            // Create a simple hash based on issue count and last modified issue
            var lastModified = issues
                .Where(i => i.Fields.TryGetProperty("updated", out var updated) && DateTime.TryParse(updated.GetString(), out var dt))
                .Select(i => DateTime.Parse(i.Fields.GetProperty("updated").GetString() ?? DateTime.MinValue.ToString()))
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            return $"{issues.Count}_{lastModified:yyyyMMddHHmmss}";
        }

        private List<TeamTableRow> CalculateTeamMetrics(List<Issue> issues)
        {
            var userDict = new Dictionary<string, TeamTableRow>();

            foreach (var issue in issues)
            {
                if (!issue.Fields.TryGetProperty("assignee", out var assignee) || assignee.ValueKind != JsonValueKind.Object)
                    continue;

                var accountId = assignee.TryGetProperty("accountId", out var idProp) ? idProp.GetString() : null;
                if (string.IsNullOrEmpty(accountId)) continue;

                if (!userDict.TryGetValue(accountId, out var row))
                {
                    row = new TeamTableRow
                    {
                        AccountId = accountId,
                        DisplayName = (assignee.TryGetProperty("displayName", out var nameProp) ? nameProp.GetString() : "") ?? string.Empty,
                        Email = (assignee.TryGetProperty("emailAddress", out var emailProp) ? emailProp.GetString() : "") ?? string.Empty,
                        AvatarUrl = (assignee.TryGetProperty("avatarUrls", out var avatarProp) && avatarProp.TryGetProperty("48x48", out var avatar48) ? avatar48.GetString() : "") ?? string.Empty,
                        IssuesAssigned = 0,
                        IssuesCompleted = 0,
                        AverageLeadTime = 0,
                        AverageCycleTime = 0
                    };
                    userDict[accountId] = row;
                }

                row.IssuesAssigned++;

                // Completed if resolutiondate exists
                if (issue.Fields.TryGetProperty("resolutiondate", out var doneProp) && doneProp.GetString() != null)
                {
                    row.IssuesCompleted++;
                    // Lead/Cycle time
                    if (issue.Fields.TryGetProperty("created", out var createdProp) &&
                        DateTime.TryParse(createdProp.GetString(), out var createdDt) &&
                        DateTime.TryParse(doneProp.GetString(), out var doneDt))
                    {
                        var days = (doneDt - createdDt).TotalDays;
                        row.AverageLeadTime += days;
                        row.AverageCycleTime += days;
                    }
                }
            }

            // Finalize averages
            foreach (var row in userDict.Values)
            {
                if (row.IssuesCompleted > 0)
                {
                    row.AverageLeadTime = Math.Round(row.AverageLeadTime / row.IssuesCompleted, 2);
                    row.AverageCycleTime = Math.Round(row.AverageCycleTime / row.IssuesCompleted, 2);
                }
            }

            return userDict.Values.ToList();
        }

        public void ClearCache()
        {
            lock (_lock)
            {
                _cachedTeamRows = null;
                _lastDataHash = null;
            }
        }
    }
} 