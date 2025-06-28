using Microsoft.AspNetCore.Mvc;
using MetricsServer.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MetricsServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;
        private readonly JiraMetricsProviderCache _providerCache;

        public MetricsController(
            ILogger<MetricsController> logger,
            JiraMetricsProviderCache providerCache)
        {
            _logger = logger;
            _providerCache = providerCache;
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] JiraCredentialsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.BaseUrl, request.Email, request.ApiToken);
                return Ok(new { message = "Connection successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return BadRequest(new { message = "Invalid credentials or connection failed" });
            }
        }

        [HttpPost("leadtime")]
        public async Task<IActionResult> GetLeadTime([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var leadTime = await provider.CalculateLeadTimeAsync();
                return Ok(new { leadTime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating lead time");
                return StatusCode(500, new { error = "Failed to calculate lead time" });
            }
        }

        [HttpPost("cycletime")]
        public async Task<IActionResult> GetCycleTime([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var cycleTime = await provider.CalculateCycleTimeAsync();
                return Ok(new { cycleTime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cycle time");
                return StatusCode(500, new { error = "Failed to calculate cycle time" });
            }
        }

        [HttpPost("throughput")]
        public async Task<IActionResult> GetThroughput([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var throughput = await provider.CalculateThroughputAsync();
                return Ok(new { throughput });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating throughput");
                return StatusCode(500, new { error = "Failed to calculate throughput" });
            }
        }

        [HttpPost("wip")]
        public async Task<IActionResult> GetWorkInProgress([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var workInProgress = await provider.CalculateWorkInProgressAsync();
                return Ok(new { workInProgress });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating work in progress");
                return StatusCode(500, new { error = "Failed to calculate work in progress" });
            }
        }

        [HttpPost("reopenrate")]
        public async Task<IActionResult> GetReopenRate([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var reopenRate = await provider.CalculateReopenRateAsync();
                return Ok(new { reopenRate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating reopen rate");
                return StatusCode(500, new { error = "Failed to calculate reopen rate" });
            }
        }

        [HttpPost("timeinstatus")]
        public async Task<IActionResult> GetTimeInStatus([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var timeInStatus = await provider.GetTimeInStatusDataAsync();
                return Ok(timeInStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time in status data");
                return StatusCode(500, new { error = "Failed to get time in status data" });
            }
        }

        [HttpPost("trend")]
        public async Task<IActionResult> GetTrend([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var trend = await provider.GetTrendDataAsync();
                return Ok(trend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trend data");
                return StatusCode(500, new { error = "Failed to get trend data" });
            }
        }

        [HttpPost("cumulativeflow")]
        public async Task<IActionResult> GetCumulativeFlow([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var flow = await provider.GetCumulativeFlowDataAsync();
                return Ok(flow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cumulative flow data");
                return StatusCode(500, new { error = "Failed to get cumulative flow data" });
            }
        }

        [HttpPost("issuetable")]
        public async Task<IActionResult> GetIssueTableRows([FromBody] IssueTableRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var (issues, total) = await provider.GetIssueTableRowsAsync(request.Page, request.PageSize);
                return Ok(new { total, issues });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting issue table rows");
                return StatusCode(500, new { error = "Failed to get issue table rows" });
            }
        }

        [HttpPost("teamtable")]
        public async Task<IActionResult> GetTeamTableRows([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken);
                var userMetricsProvider = new UserMetricsProvider(provider);
                var rows = await userMetricsProvider.GetTeamTableRowsAsync();
                return Ok(rows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team table rows");
                return StatusCode(500, new { error = "Failed to get team table rows" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshMetrics([FromBody] MetricsRequest request)
        {
            try
            {
                var provider = await _providerCache.GetOrCreateAsync(request.Credentials.BaseUrl, request.Credentials.Email, request.Credentials.ApiToken, true);
                return Ok(new { message = "Metrics refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing metrics");
                return StatusCode(500, new { error = "Failed to refresh metrics" });
            }
        }
    }

    public class JiraCredentialsRequest
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
    }

    public class MetricsRequest
    {
        public JiraCredentialsRequest Credentials { get; set; } = new();
    }

    public class IssueTableRequest : MetricsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
} 