using Akka.Actor;
using Akka.Hosting;
using MetricsServer.Controllers;
using System;
using System.Threading.Tasks;
using MetricsServer.Akka.Actors;
using MetricsServer.Akka.Messages;
using static MetricsServer.Akka.Messages.LeadTimeMessages;

namespace MetricsServer.Akka.Services
{
    public class AkkaMetricsService
    {
        private readonly IActorRef _coordinator;

        public AkkaMetricsService(IRequiredActor<MetricsCoordinatorActor> requiredActor)
        {
            _coordinator = requiredActor.ActorRef;
        }

        public async Task<object> GetLeadTime(JiraCredentialsRequest credentials)
        {
            try
            {
                // Create the message to send to the coordinator
                var message = new CalculateLeadTime(credentials);
                
                // Use the Ask pattern to send message and wait for response
                var result = await _coordinator.Ask(message, TimeSpan.FromSeconds(30));
                
                // Pattern match on the result to handle success or failure
                return result switch
                {
                    LeadTimeCalculated success => new { leadTime = success.LeadTime },
                    GeneralMessages.CalculationFailed failure => new { error = failure.Reason },
                    _ => new { error = "An unknown error occurred" }
                };
            }
            catch (Exception ex)
            {
                // Handle any exceptions (like timeout)
                return new { error = $"Service error: {ex.Message}" };
            }
        }
    }
}