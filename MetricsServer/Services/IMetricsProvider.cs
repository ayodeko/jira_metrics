using System.Threading.Tasks;
using System.Collections.Generic;
using MetricsServer.Models;

namespace MetricsServer.Services
{
    public interface IMetricsProvider
    {
        Task<string> GetIssuesAsync(string jql, int maxResults = 50);
        Task<double> CalculateLeadTimeAsync();
        Task<double> CalculateCycleTimeAsync();
        Task<int> CalculateThroughputAsync();
        Task<int> CalculateWorkInProgressAsync();
        Task<double> CalculateReopenRateAsync();
        Task<List<Issue>> FetchIssuesAsync();
        
        // New methods for chart data
        Task<TimeInStatusData> GetTimeInStatusDataAsync();
        Task<TrendData> GetTrendDataAsync();
        Task<CumulativeFlowData> GetCumulativeFlowDataAsync();
        Task<CumulativeFlowData> GetCumulativeFlowAsync();
        Task<TimeInStatusData> GetTimeInStatusAsync();
        Task<TrendData> GetTrendAsync();
        Task InitializeAsync();
        Task<(List<JiraMetricsProvider.IssueTableRow> Issues, int Total)> GetIssueTableRowsAsync(int page = 1, int pageSize = 20);
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