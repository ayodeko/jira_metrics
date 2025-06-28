namespace MetricsServer.Models
{
    public class TeamTableRow
    {
        public string AccountId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int IssuesAssigned { get; set; }
        public int IssuesCompleted { get; set; }
        public double AverageLeadTime { get; set; }
        public double AverageCycleTime { get; set; }
    }
} 