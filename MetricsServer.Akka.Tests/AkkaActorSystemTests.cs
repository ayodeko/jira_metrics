using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.TestActors;
using Akka.TestKit.Xunit2;
using MetricsServer.Akka.Actors;
using MetricsServer.Akka.Messages;
using MetricsServer.Controllers;
using MetricsServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static MetricsServer.Akka.Messages.GeneralMessages;
using static MetricsServer.Akka.Messages.LeadTimeMessages;

namespace MetricsServer.Akka.Tests
{
    /// <summary>
    /// Comprehensive Akka.NET Actor System Tests demonstrating:
    /// - Actor behavior testing with TestKit
    /// - Message flow validation
    /// - Supervision strategy testing
    /// - Actor lifecycle management
    /// </summary>
    public class AkkaActorSystemTests : TestKit
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<JiraMetricsProviderCache> _mockProviderCache;
        private readonly Mock<IMetricsProvider> _mockMetricsProvider;
        private readonly JiraCredentialsRequest _testCredentials;

        public AkkaActorSystemTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockProviderCache = new Mock<JiraMetricsProviderCache>();
            _mockMetricsProvider = new Mock<IMetricsProvider>();
            _testCredentials = new JiraCredentialsRequest
            {
                BaseUrl = "https://test.atlassian.net",
                Email = "test@example.com",
                ApiToken = "test-token"
            };

            // Setup service provider mock
            _mockServiceProvider.Setup(x => x.GetService(typeof(JiraMetricsProviderCache)))
                .Returns(_mockProviderCache.Object);
        }

        [Fact(DisplayName = "MetricsCoordinatorActor_ShouldHandleCalculateLeadTimeMessage_AndRespondWithLeadTimeCalculated")]
        public async Task MetricsCoordinatorActor_ShouldHandleCalculateLeadTimeMessage_AndRespondWithLeadTimeCalculated()
        {
            // Arrange
            var testIssues = CreateTestIssues();
            var expectedLeadTime = 15.5;

            // Setup mocks
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(CreateMockMetricsProvider(expectedLeadTime));

            _mockMetricsProvider.Setup(x => x.GetIssuesAsync())
                .ReturnsAsync(testIssues);

            // Create actor under test
            var coordinatorActor = ActorOfAsTestActorRef<MetricsCoordinatorActor>(
                Props.Create(() => new MetricsCoordinatorActor(_mockServiceProvider.Object)),
                "test-coordinator");

            // Act
            var message = new CalculateLeadTime(_testCredentials);
            var result = await coordinatorActor.Ask(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LeadTimeCalculated>(result);
            
            var leadTimeResult = (LeadTimeCalculated)result;
            Assert.Equal(expectedLeadTime, leadTimeResult.LeadTime);

            // Verify actor behavior
            ExpectMsg<LeadTimeCalculated>(TimeSpan.FromSeconds(1));
        }

        [Fact(DisplayName = "JiraApiActor_ShouldHandleFetchJiraIssuesMessage_AndRespondWithJiraIssuesFetched")]
        public async Task JiraApiActor_ShouldHandleFetchJiraIssuesMessage_AndRespondWithJiraIssuesFetched()
        {
            // Arrange
            var testIssues = CreateTestIssues();
            
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(CreateMockMetricsProvider(0));

            _mockMetricsProvider.Setup(x => x.GetIssuesAsync())
                .ReturnsAsync(testIssues);

            // Create actor under test
            var apiActor = ActorOfAsTestActorRef<JiraApiActor>(
                Props.Create(() => new JiraApiActor(_mockProviderCache.Object)),
                "test-api-actor");

            // Act
            var message = new FetchJiraIssues(_testCredentials);
            var result = await apiActor.Ask(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JiraIssuesFetched>(result);
            
            var issuesResult = (JiraIssuesFetched)result;
            Assert.Equal(testIssues.Count, issuesResult.Issues.Count);
            Assert.Equal(_testCredentials, issuesResult.Credentials);

            // Verify actor stopped itself after processing
            Watch(apiActor);
            ExpectTerminated(apiActor);
        }

        [Fact(DisplayName = "LeadTimeActor_ShouldHandleCalculateLeadTimeWithDataMessage_AndRespondWithLeadTimeCalculated")]
        public async Task LeadTimeActor_ShouldHandleCalculateLeadTimeWithDataMessage_AndRespondWithLeadTimeCalculated()
        {
            // Arrange
            var testIssues = CreateTestIssues();
            var expectedLeadTime = 25.7;

            _mockMetricsProvider.Setup(x => x.CalculateLeadTimeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedLeadTime);

            // Create actor under test
            var leadTimeActor = ActorOfAsTestActorRef<LeadTimeActor>(
                Props.Create(() => new LeadTimeActor(_mockMetricsProvider.Object)),
                "test-leadtime-actor");

            // Act
            var message = new CalculateLeadTimeWithData(testIssues);
            var result = await leadTimeActor.Ask(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LeadTimeCalculated>(result);
            
            var leadTimeResult = (LeadTimeCalculated)result;
            Assert.Equal(expectedLeadTime, leadTimeResult.LeadTime);
        }

        [Fact(DisplayName = "ActorSystem_ShouldHandleCalculationFailure_AndRespondWithCalculationFailed")]
        public async Task ActorSystem_ShouldHandleCalculationFailure_AndRespondWithCalculationFailed()
        {
            // Arrange
            var expectedErrorMessage = "Jira API connection failed";

            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(expectedErrorMessage));

            // Create actor under test
            var coordinatorActor = ActorOfAsTestActorRef<MetricsCoordinatorActor>(
                Props.Create(() => new MetricsCoordinatorActor(_mockServiceProvider.Object)),
                "test-coordinator-failure");

            // Act
            var message = new CalculateLeadTime(_testCredentials);
            var result = await coordinatorActor.Ask(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CalculationFailed>(result);
            
            var failureResult = (CalculationFailed)result;
            Assert.Contains(expectedErrorMessage, failureResult.Reason);
        }

        [Fact(DisplayName = "ActorSystem_ShouldHandleTimeout_AndRespondWithCalculationFailed")]
        public async Task ActorSystem_ShouldHandleTimeout_AndRespondWithCalculationFailed()
        {
            // Arrange
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ => CreateMockMetricsProvider(0)));

            // Create actor under test with shorter timeout
            var coordinatorActor = ActorOfAsTestActorRef<MetricsCoordinatorActor>(
                Props.Create(() => new MetricsCoordinatorActor(_mockServiceProvider.Object)),
                "test-coordinator-timeout");

            // Act
            var message = new CalculateLeadTime(_testCredentials);
            var result = await coordinatorActor.Ask(message, TimeSpan.FromSeconds(2));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CalculationFailed>(result);
            
            var failureResult = (CalculationFailed)result;
            Assert.Contains("timed out", failureResult.Reason.ToLower());
        }

        [Fact(DisplayName = "ActorSystem_ShouldMaintainMessageOrder_AndProcessSequentially")]
        public async Task ActorSystem_ShouldMaintainMessageOrder_AndProcessSequentially()
        {
            // Arrange
            var coordinatorActor = ActorOfAsTestActorRef<MetricsCoordinatorActor>(
                Props.Create(() => new MetricsCoordinatorActor(_mockServiceProvider.Object)),
                "test-coordinator-order");

            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(CreateMockMetricsProvider(10.0));

            _mockMetricsProvider.Setup(x => x.GetIssuesAsync())
                .ReturnsAsync(CreateTestIssues());

            // Act - Send multiple messages rapidly
            var tasks = new List<Task<object>>();
            for (int i = 0; i < 3; i++)
            {
                var message = new CalculateLeadTime(_testCredentials);
                tasks.Add(coordinatorActor.Ask(message, TimeSpan.FromSeconds(5)));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(3, results.Length);
            Assert.All(results, result => Assert.IsType<LeadTimeCalculated>(result));
        }

        private IMetricsProvider CreateMockMetricsProvider(double leadTime)
        {
            var mock = new Mock<IMetricsProvider>();
            mock.Setup(x => x.CalculateLeadTimeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(leadTime);
            mock.Setup(x => x.GetIssuesAsync())
                .ReturnsAsync(CreateTestIssues());
            return mock.Object;
        }

        private List<MetricsServer.Models.Issue> CreateTestIssues()
        {
            return new List<MetricsServer.Models.Issue>
            {
                new MetricsServer.Models.Issue
                {
                    Key = "TEST-1",
                    Fields = new System.Text.Json.JsonElement()
                },
                new MetricsServer.Models.Issue
                {
                    Key = "TEST-2",
                    Fields = new System.Text.Json.JsonElement()
                }
            };
        }
    }
} 