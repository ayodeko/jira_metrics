using MetricsServer.Controllers;
using MetricsServer.Models;

namespace MetricsServer.Akka.Messages;

public class LeadTimeMessages
{
    // --- Commands ---

    /// <summary>
    /// Top-level command to initiate a lead time calculation.
    /// </summary>
    public record CalculateLeadTime(JiraCredentialsRequest Credentials);

    /// <summary>
    /// Command to a worker actor to calculate lead time from a given set of issues.
    /// </summary>
    public record CalculateLeadTimeWithData(IReadOnlyList<Issue> Issues);


    // --- Events ---

    /// <summary>
    // Event published when the lead time has been successfully calculated.
    /// </summary>
    public record LeadTimeCalculated(double LeadTime);
}