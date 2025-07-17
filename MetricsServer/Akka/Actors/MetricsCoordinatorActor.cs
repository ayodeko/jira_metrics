using Akka.Actor;
using MetricsServer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using MetricsServer.Controllers;
using static MetricsServer.Akka.Messages.GeneralMessages;
using static MetricsServer.Akka.Messages.LeadTimeMessages;

namespace MetricsServer.Akka.Actors
{
    public class MetricsCoordinatorActor : ReceiveActor
    {
        private readonly IServiceProvider _serviceProvider;
        private IActorRef? _originalSender;

        public MetricsCoordinatorActor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
            Receive<CalculateLeadTime>(HandleCalculateLeadTime);
            Receive<JiraIssuesFetched>(HandleJiraIssuesFetched);
            Receive<LeadTimeCalculated>(HandleLeadTimeCalculated);
            Receive<CalculationFailed>(HandleCalculationFailed);
            Receive<ReceiveTimeout>(HandleReceiveTimeout);
        }

        private void HandleCalculateLeadTime(CalculateLeadTime msg)
        {
            _originalSender = Sender;
            
            var providerCache = _serviceProvider.GetRequiredService<JiraMetricsProviderCache>();
            var apiActor = Context.ActorOf(Props.Create(() => new JiraApiActor(providerCache)));
            apiActor.Tell(new FetchJiraIssues(msg.Credentials));
            
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
        }

        private void HandleJiraIssuesFetched(JiraIssuesFetched jiraData)
        {
            var providerCache = _serviceProvider.GetRequiredService<JiraMetricsProviderCache>();
            var provider = providerCache.GetOrCreateAsync(
                jiraData.Credentials.BaseUrl, 
                jiraData.Credentials.Email, 
                jiraData.Credentials.ApiToken).Result;
            
            var calculatorActor = Context.ActorOf(Props.Create(() => new LeadTimeActor(provider)));
            calculatorActor.Tell(new CalculateLeadTimeWithData(jiraData.Issues));
        }

        private void HandleLeadTimeCalculated(LeadTimeCalculated result)
        {
            if (_originalSender != null)
            {
                _originalSender.Tell(result);
            }
            
            Context.SetReceiveTimeout(null);
            _originalSender = null;
        }

        private void HandleCalculationFailed(CalculationFailed failure)
        {
            if (_originalSender != null)
            {
                _originalSender.Tell(failure);
            }
            
            Context.SetReceiveTimeout(null);
            _originalSender = null;
        }

        private void HandleReceiveTimeout(ReceiveTimeout timeout)
        {
            if (_originalSender != null)
            {
                _originalSender.Tell(new CalculationFailed("Operation timed out"));
            }
            
            _originalSender = null;
        }
    }
}