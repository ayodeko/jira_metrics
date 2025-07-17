using Akka.Actor;
using MetricsServer.Services;
using System;
using static MetricsServer.Akka.Messages.GeneralMessages;
using static MetricsServer.Akka.Messages.LeadTimeMessages;

namespace MetricsServer.Akka.Actors
{
    public class LeadTimeActor : ReceiveActor
    {
        private readonly IMetricsProvider _metricsProvider;

        public LeadTimeActor(IMetricsProvider metricsProvider)
        {
            _metricsProvider = metricsProvider;

            ReceiveAsync<CalculateLeadTimeWithData>(async msg =>
            {
                try
                {
                    var leadTime = await _metricsProvider.CalculateLeadTimeAsync();
                    Sender.Tell(new LeadTimeCalculated(leadTime));
                }
                catch (Exception ex)
                {
                    Sender.Tell(new CalculationFailed($"Calculation failed: {ex.Message}"));
                }
            });
        }
    }
}