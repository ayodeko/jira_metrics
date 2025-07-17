using Akka.Actor;
using MetricsServer.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static MetricsServer.Akka.Messages.GeneralMessages;

namespace MetricsServer.Akka.Actors
{
    public class JiraApiActor : ReceiveActor
    {
        private readonly JiraMetricsProviderCache _providerCache;

        public JiraApiActor(JiraMetricsProviderCache providerCache)
        {
            _providerCache = providerCache;

            ReceiveAsync<FetchJiraIssues>(async msg =>
            {
                try
                {
                    var provider = await _providerCache.GetOrCreateAsync(
                        msg.Credentials.BaseUrl, 
                        msg.Credentials.Email, 
                        msg.Credentials.ApiToken);
                    
                    var allIssues = await provider.GetIssuesAsync();
                    Sender.Tell(new JiraIssuesFetched(allIssues, msg.Credentials));
                }
                catch (Exception ex)
                {
                    Sender.Tell(new CalculationFailed($"Failed to fetch data from Jira: {ex.Message}"));
                }
                finally
                {
                    Context.Stop(Self);
                }
            });
        }
    }
}