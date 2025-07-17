using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MetricsServer.Services
{
    public class JiraMetricsProviderCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<JiraMetricsProviderCache> _logger;

        public JiraMetricsProviderCache(IMemoryCache cache, ILoggerFactory loggerFactory, ILogger<JiraMetricsProviderCache> logger)
        {
            _cache = cache;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public async Task<JiraMetricsProvider> GetOrCreateAsync(string baseUrl, string email, string apiToken, bool forceRefresh = false)
        {
            var cacheKey = CreateCacheKey(baseUrl, email, apiToken);
            
            _logger.LogInformation($"CACHE: Request for credentials - BaseUrl: '{baseUrl}', Email: '{email}', ApiToken: '{apiToken?.Substring(0, Math.Min(10, apiToken?.Length ?? 0))}...', CacheKey: {cacheKey}");

            if (forceRefresh)
            {
                _cache.Remove(cacheKey);
            }

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                _logger.LogInformation($"Creating new JiraMetricsProvider for key: {cacheKey}");
                _logger.LogInformation($"CACHE: Storing credentials - BaseUrl: '{baseUrl}', Email: '{email}', ApiToken: '{apiToken?.Substring(0, Math.Min(10, apiToken?.Length ?? 0))}...'");
                entry.SlidingExpiration = TimeSpan.FromMinutes(30); // Cache for 30 minutes of inactivity

                var provider = new JiraMetricsProvider(baseUrl, email, apiToken, _loggerFactory.CreateLogger<JiraMetricsProvider>());
                await provider.InitializeAsync();
                
                return provider;
            });
        }

        private string CreateCacheKey(string baseUrl, string email, string apiToken)
        {
            // Create a consistent key for the same credentials
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{baseUrl}:{email}:{apiToken}"));
            return $"JiraProvider_{Convert.ToBase64String(hashBytes)}";
        }
    }
} 