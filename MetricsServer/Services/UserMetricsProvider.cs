using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsServer.Models;

namespace MetricsServer.Services
{
    public class UserMetricsProvider
    {
        private readonly IMetricsProvider _jiraMetricsProvider;
        public UserMetricsProvider(IMetricsProvider jiraMetricsProvider)
        {
            _jiraMetricsProvider = jiraMetricsProvider;
        }

        public async Task<List<TeamTableRow>> GetTeamTableRowsAsync()
        {
            var issues = await _jiraMetricsProvider.FetchIssuesAsync();
            if (issues == null) return new List<TeamTableRow>();

            var userDict = new Dictionary<string, TeamTableRow>();

            foreach (var issue in issues)
            {
                if (!issue.Fields.TryGetProperty("assignee", out var assignee) || assignee.ValueKind != System.Text.Json.JsonValueKind.Object)
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
    }
} 