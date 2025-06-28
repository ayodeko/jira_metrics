namespace MetricsServer.Models
{
    public class TrendData
    {
        public string[] Labels { get; set; } = Array.Empty<string>();
        public TrendDataset[] Datasets { get; set; } = Array.Empty<TrendDataset>();
    }

    public class TrendDataset
    {
        public string Label { get; set; } = string.Empty;
        public int[] Data { get; set; } = Array.Empty<int>();
    }
} 