using System.Threading.Tasks;
using System.Collections.Generic;
using MetricsServer.Models;

namespace MetricsServer.Services
{
    public interface IMetricsProvider
    {
        Task<string> GetIssuesAsync(string jql, int maxResults = 50);
        Task<List<Issue>> GetIssuesAsync();
        Task<double> CalculateLeadTimeAsync(string? startDate = null, string? endDate = null);
        Task<double> CalculateCycleTimeAsync(string? startDate = null, string? endDate = null);
        Task<int> CalculateThroughputAsync(string? startDate = null, string? endDate = null);
        Task<int> CalculateWorkInProgressAsync(string? startDate = null, string? endDate = null);
        Task<double> CalculateReopenRateAsync(string? startDate = null, string? endDate = null);
        Task<List<Issue>> FetchIssuesAsync();
        
        // New methods for chart data
        Task<TimeInStatusData> GetTimeInStatusDataAsync(string? startDate = null, string? endDate = null);
        Task<TrendData> GetTrendDataAsync(string? startDate = null, string? endDate = null);
        Task<CumulativeFlowData> GetCumulativeFlowDataAsync(string? startDate = null, string? endDate = null);
        Task<CumulativeFlowData> GetCumulativeFlowAsync();
        Task<TimeInStatusData> GetTimeInStatusAsync();
        Task<TrendData> GetTrendAsync();
        Task InitializeAsync();
        Task<(List<JiraMetricsProvider.IssueTableRow> Issues, int Total)> GetIssueTableRowsAsync(int page = 1, int pageSize = 20, string? startDate = null, string? endDate = null);
    }

    // Data models for the new endpoints
    public class TimeInStatusData
    {
        public List<StatusDuration> StatusDurations { get; set; } = new List<StatusDuration>();
    }

    public class StatusDuration
    {
        public string Status { get; set; } = string.Empty;
        public double AverageDays { get; set; }
    }

    public class TrendData
    {
        public List<MetricTrend> LeadTime { get; set; } = new List<MetricTrend>();
        public List<MetricTrend> CycleTime { get; set; } = new List<MetricTrend>();
        public List<MetricTrend> Throughput { get; set; } = new List<MetricTrend>();
    }

    public class MetricTrend
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }

    public class CumulativeFlowData
    {
        public List<FlowPoint> DataPoints { get; set; } = new List<FlowPoint>();
    }

    public class FlowPoint
    {
        public DateTime Date { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
    }
} 