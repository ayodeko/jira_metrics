using MetricsServer.Controllers;
using MetricsServer.Models;
using System.Collections.Generic;

namespace MetricsServer.Akka.Messages
{
    /// <summary>
    /// Contains messages that are not specific to any single metric calculation.
    /// </summary>
    public static class GeneralMessages
    {
        // --- Commands ---

        /// <summary>
        /// Command to fetch all relevant issues from the Jira API.
        /// </summary>
        public record FetchJiraIssues(JiraCredentialsRequest Credentials);


        // --- Events ---

        /// <summary>
        /// Event published when Jira issues have been successfully fetched.
        /// </summary>
        public record JiraIssuesFetched(IReadOnlyList<Issue> Issues, JiraCredentialsRequest Credentials);

        /// <summary>
        /// Event published when any part of a calculation process fails.
        /// </summary>
        public record CalculationFailed(string Reason);
    }
}