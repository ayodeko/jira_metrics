using Akka.Actor;
using Akka.Hosting;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using MetricsServer.Akka.Services;
using MetricsServer.Akka.Actors;
using MetricsServer.Akka.Messages;
using MetricsServer.Controllers;
using Moq;
using Xunit;
using static MetricsServer.Akka.Messages.LeadTimeMessages;

namespace MetricsServer.Akka.Tests
{
    /// <summary>
    /// AkkaMetricsService Tests demonstrating:
    /// - Service layer testing with mocked actor references
    /// - Dependency injection testing
    /// - Error handling and timeout scenarios
    /// - Message transformation and response mapping
    /// </summary>
    public class AkkaMetricsServiceTests : TestKit
    {
        private readonly Mock<IRequiredActor<MetricsCoordinatorActor>> _mockRequiredActor;
        private readonly Mock<IActorRef> _mockCoordinatorActor;
        private readonly AkkaMetricsService _akkaMetricsService;
        private readonly JiraCredentialsRequest _testCredentials;

        public AkkaMetricsServiceTests()
        {
            _mockRequiredActor = new Mock<IRequiredActor<MetricsCoordinatorActor>>();
            _mockCoordinatorActor = new Mock<IActorRef>();
            _testCredentials = new JiraCredentialsRequest
            {
                BaseUrl = "https://test.atlassian.net",
                Email = "test@example.com",
                ApiToken = "test-token"
            };

            // Setup mock actor reference
            _mockRequiredActor.Setup(x => x.ActorRef).Returns(_mockCoordinatorActor.Object);
            
            // Create service under test
            _akkaMetricsService = new AkkaMetricsService(_mockRequiredActor.Object);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldReturnLeadTimeResult_WhenActorRespondsWithSuccess")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldReturnLeadTimeResult_WhenActorRespondsWithSuccess()
        {
            // Arrange
            var expectedLeadTime = 18.5;
            var leadTimeCalculatedMessage = new LeadTimeCalculated(expectedLeadTime);

            // Setup mock to return success response
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(leadTimeCalculatedMessage);

            // Act
            var result = await _akkaMetricsService.GetLeadTime(_testCredentials);

            // Assert
            Assert.NotNull(result);
            
            // Verify the result is an anonymous object with leadTime property
            var resultType = result.GetType();
            var leadTimeProperty = resultType.GetProperty("leadTime");
            Assert.NotNull(leadTimeProperty);
            
            var actualLeadTime = (double)leadTimeProperty.GetValue(result);
            Assert.Equal(expectedLeadTime, actualLeadTime);

            // Verify actor was called with correct message
            _mockCoordinatorActor.Verify(x => x.Ask(
                It.Is<CalculateLeadTime>(msg => msg.Credentials == _testCredentials),
                It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldReturnErrorResult_WhenActorRespondsWithFailure")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldReturnErrorResult_WhenActorRespondsWithFailure()
        {
            // Arrange
            var expectedErrorMessage = "Calculation failed due to invalid data";
            var calculationFailedMessage = new GeneralMessages.CalculationFailed(expectedErrorMessage);

            // Setup mock to return failure response
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(calculationFailedMessage);

            // Act
            var result = await _akkaMetricsService.GetLeadTime(_testCredentials);

            // Assert
            Assert.NotNull(result);
            
            // Verify the result is an anonymous object with error property
            var resultType = result.GetType();
            var errorProperty = resultType.GetProperty("error");
            Assert.NotNull(errorProperty);
            
            var actualError = (string)errorProperty.GetValue(result);
            Assert.Equal(expectedErrorMessage, actualError);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldReturnErrorResult_WhenActorThrowsException")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldReturnErrorResult_WhenActorThrowsException()
        {
            // Arrange
            var expectedErrorMessage = "Actor system error";
            
            // Setup mock to throw exception
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ThrowsAsync(new InvalidOperationException(expectedErrorMessage));

            // Act
            var result = await _akkaMetricsService.GetLeadTime(_testCredentials);

            // Assert
            Assert.NotNull(result);
            
            // Verify the result is an anonymous object with error property
            var resultType = result.GetType();
            var errorProperty = resultType.GetProperty("error");
            Assert.NotNull(errorProperty);
            
            var actualError = (string)errorProperty.GetValue(result);
            Assert.Contains(expectedErrorMessage, actualError);
            Assert.Contains("Service error", actualError);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldReturnErrorResult_WhenActorTimesOut")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldReturnErrorResult_WhenActorTimesOut()
        {
            // Arrange
            // Setup mock to simulate timeout
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ThrowsAsync(new TimeoutException("Actor request timed out"));

            // Act
            var result = await _akkaMetricsService.GetLeadTime(_testCredentials);

            // Assert
            Assert.NotNull(result);
            
            // Verify the result is an anonymous object with error property
            var resultType = result.GetType();
            var errorProperty = resultType.GetProperty("error");
            Assert.NotNull(errorProperty);
            
            var actualError = (string)errorProperty.GetValue(result);
            Assert.Contains("timed out", actualError.ToLower());
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldReturnUnknownErrorResult_WhenActorReturnsUnexpectedMessage")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldReturnUnknownErrorResult_WhenActorReturnsUnexpectedMessage()
        {
            // Arrange
            var unexpectedMessage = "Unexpected response type";
            
            // Setup mock to return unexpected message type
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(unexpectedMessage);

            // Act
            var result = await _akkaMetricsService.GetLeadTime(_testCredentials);

            // Assert
            Assert.NotNull(result);
            
            // Verify the result is an anonymous object with error property
            var resultType = result.GetType();
            var errorProperty = resultType.GetProperty("error");
            Assert.NotNull(errorProperty);
            
            var actualError = (string)errorProperty.GetValue(result);
            Assert.Equal("An unknown error occurred", actualError);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldUseCorrectTimeout_WhenSendingMessageToActor")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldUseCorrectTimeout_WhenSendingMessageToActor()
        {
            // Arrange
            var expectedTimeout = TimeSpan.FromSeconds(30);
            var leadTimeCalculatedMessage = new LeadTimeCalculated(15.0);

            // Setup mock to return success response
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(leadTimeCalculatedMessage);

            // Act
            await _akkaMetricsService.GetLeadTime(_testCredentials);

            // Assert
            // Verify actor was called with the expected timeout
            _mockCoordinatorActor.Verify(x => x.Ask(
                It.IsAny<CalculateLeadTime>(),
                It.Is<TimeSpan>(timeout => timeout == expectedTimeout)), Times.Once);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldHandleNullCredentials_AndReturnErrorResult")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldHandleNullCredentials_AndReturnErrorResult()
        {
            // Arrange
            JiraCredentialsRequest? nullCredentials = null;

            // Act
            var result = await _akkaMetricsService.GetLeadTime(nullCredentials!);

            // Assert
            Assert.NotNull(result);
            
            // Verify the result is an anonymous object with error property
            var resultType = result.GetType();
            var errorProperty = resultType.GetProperty("error");
            Assert.NotNull(errorProperty);
            
            var actualError = (string)errorProperty.GetValue(result);
            Assert.Contains("Service error", actualError);
        }

        [Fact(DisplayName = "AkkaMetricsService_GetLeadTime_ShouldHandleMultipleConcurrentRequests_AndProcessThemIndependently")]
        public async Task AkkaMetricsService_GetLeadTime_ShouldHandleMultipleConcurrentRequests_AndProcessThemIndependently()
        {
            // Arrange
            var expectedLeadTimes = new[] { 10.0, 20.0, 30.0 };
            var callCount = 0;

            // Setup mock to return different values for each call
            _mockCoordinatorActor.Setup(x => x.Ask(It.IsAny<CalculateLeadTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(() => new LeadTimeCalculated(expectedLeadTimes[callCount++ % expectedLeadTimes.Length]));

            // Act - Send multiple concurrent requests
            var tasks = new List<Task<object>>();
            for (int i = 0; i < 6; i++)
            {
                tasks.Add(_akkaMetricsService.GetLeadTime(_testCredentials));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(6, results.Length);
            
            // Verify all results are valid
            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                Assert.NotNull(result);
                
                var resultType = result.GetType();
                var leadTimeProperty = resultType.GetProperty("leadTime");
                Assert.NotNull(leadTimeProperty);
                
                var actualLeadTime = (double)leadTimeProperty.GetValue(result);
                Assert.Contains(actualLeadTime, expectedLeadTimes);
            }

            // Verify actor was called 6 times
            _mockCoordinatorActor.Verify(x => x.Ask(
                It.IsAny<CalculateLeadTime>(),
                It.IsAny<TimeSpan>()), Times.Exactly(6));
        }
    }
} 