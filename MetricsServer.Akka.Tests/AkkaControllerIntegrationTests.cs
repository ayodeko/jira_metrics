using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MetricsServer.Akka.Services;
using MetricsServer.Controllers;
using MetricsServer.Services;
using Moq;
using Xunit;

namespace MetricsServer.Akka.Tests
{
    /// <summary>
    /// Controller Integration Tests demonstrating:
    /// - Controller integration with Akka.NET services
    /// - Feature flag testing (UseActorModel configuration)
    /// - Dependency injection testing in controllers
    /// - Error handling and response mapping
    /// - Traditional vs Actor-based approach switching
    /// </summary>
    public class AkkaControllerIntegrationTests
    {
        private readonly Mock<ILogger<MetricsController>> _mockLogger;
        private readonly Mock<JiraMetricsProviderCache> _mockProviderCache;
        private readonly Mock<AkkaMetricsService> _mockAkkaService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly MetricsController _controller;
        private readonly MetricsRequest _testRequest;

        public AkkaControllerIntegrationTests()
        {
            _mockLogger = new Mock<ILogger<MetricsController>>();
            _mockProviderCache = new Mock<JiraMetricsProviderCache>();
            _mockAkkaService = new Mock<AkkaMetricsService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _testRequest = new MetricsRequest
            {
                Credentials = new JiraCredentialsRequest
                {
                    BaseUrl = "https://test.atlassian.net",
                    Email = "test@example.com",
                    ApiToken = "test-token"
                },
                StartDate = "2024-01-01",
                EndDate = "2024-12-31"
            };

            // Create controller under test
            _controller = new MetricsController(
                _mockLogger.Object,
                _mockProviderCache.Object,
                _mockAkkaService.Object,
                _mockConfiguration.Object);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldUseAkkaService_WhenUseActorModelIsTrue")]
        public async Task MetricsController_GetLeadTime_ShouldUseAkkaService_WhenUseActorModelIsTrue()
        {
            // Arrange
            var expectedLeadTime = 22.5;
            var expectedResult = new { leadTime = expectedLeadTime };

            // Setup configuration to enable actor model
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(true);

            // Setup Akka service to return success
            _mockAkkaService.Setup(x => x.GetLeadTime(_testRequest.Credentials))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetLeadTime(_testRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedResult, okResult.Value);

            // Verify Akka service was called
            _mockAkkaService.Verify(x => x.GetLeadTime(_testRequest.Credentials), Times.Once);
            
            // Verify traditional service was NOT called
            _mockProviderCache.Verify(x => x.GetOrCreateAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldUseTraditionalService_WhenUseActorModelIsFalse")]
        public async Task MetricsController_GetLeadTime_ShouldUseTraditionalService_WhenUseActorModelIsFalse()
        {
            // Arrange
            var expectedLeadTime = 18.3;
            var mockProvider = new Mock<IMetricsProvider>();

            // Setup configuration to disable actor model
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(false);

            // Setup traditional service
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockProvider.Object);

            mockProvider.Setup(x => x.CalculateLeadTimeAsync(
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedLeadTime);

            // Act
            var result = await _controller.GetLeadTime(_testRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            
            var okResult = (OkObjectResult)result;
            var resultValue = okResult.Value;
            
            // Verify the result is an anonymous object with leadTime property
            var resultType = resultValue.GetType();
            var leadTimeProperty = resultType.GetProperty("leadTime");
            Assert.NotNull(leadTimeProperty);
            
            var actualLeadTime = (double)leadTimeProperty.GetValue(resultValue);
            Assert.Equal(expectedLeadTime, actualLeadTime);

            // Verify traditional service was called
            _mockProviderCache.Verify(x => x.GetOrCreateAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // Verify Akka service was NOT called
            _mockAkkaService.Verify(x => x.GetLeadTime(It.IsAny<JiraCredentialsRequest>()), Times.Never);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldReturnErrorResult_WhenAkkaServiceReturnsError")]
        public async Task MetricsController_GetLeadTime_ShouldReturnErrorResult_WhenAkkaServiceReturnsError()
        {
            // Arrange
            var expectedError = "Calculation failed due to invalid credentials";
            var errorResult = new { error = expectedError };

            // Setup configuration to enable actor model
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(true);

            // Setup Akka service to return error
            _mockAkkaService.Setup(x => x.GetLeadTime(_testRequest.Credentials))
                .ReturnsAsync(errorResult);

            // Act
            var result = await _controller.GetLeadTime(_testRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            
            var okResult = (OkObjectResult)result;
            Assert.Equal(errorResult, okResult.Value);

            // Verify Akka service was called
            _mockAkkaService.Verify(x => x.GetLeadTime(_testRequest.Credentials), Times.Once);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldReturn500Error_WhenTraditionalServiceThrowsException")]
        public async Task MetricsController_GetLeadTime_ShouldReturn500Error_WhenTraditionalServiceThrowsException()
        {
            // Arrange
            var expectedErrorMessage = "Jira API connection failed";

            // Setup configuration to disable actor model
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(false);

            // Setup traditional service to throw exception
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(expectedErrorMessage));

            // Act
            var result = await _controller.GetLeadTime(_testRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);
            
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            
            // Verify the result is an anonymous object with error property
            var resultValue = objectResult.Value;
            var resultType = resultValue.GetType();
            var errorProperty = resultType.GetProperty("error");
            Assert.NotNull(errorProperty);
            
            var actualError = (string)errorProperty.GetValue(resultValue);
            Assert.Equal("Failed to calculate lead time", actualError);

            // Verify logger was called
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldLogInformation_WhenUsingActorModel")]
        public async Task MetricsController_GetLeadTime_ShouldLogInformation_WhenUsingActorModel()
        {
            // Arrange
            var expectedResult = new { leadTime = 15.0 };

            // Setup configuration to enable actor model
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(true);

            // Setup Akka service
            _mockAkkaService.Setup(x => x.GetLeadTime(_testRequest.Credentials))
                .ReturnsAsync(expectedResult);

            // Act
            await _controller.GetLeadTime(_testRequest);

            // Assert
            // Verify information was logged about using Akka.NET
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Akka.NET actors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldLogInformation_WhenUsingTraditionalApproach")]
        public async Task MetricsController_GetLeadTime_ShouldLogInformation_WhenUsingTraditionalApproach()
        {
            // Arrange
            var mockProvider = new Mock<IMetricsProvider>();

            // Setup configuration to disable actor model
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(false);

            // Setup traditional service
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockProvider.Object);

            mockProvider.Setup(x => x.CalculateLeadTimeAsync(
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(12.5);

            // Act
            await _controller.GetLeadTime(_testRequest);

            // Assert
            // Verify information was logged about using traditional approach
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("traditional approach")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldHandleNullRequest_AndReturnBadRequest")]
        public async Task MetricsController_GetLeadTime_ShouldHandleNullRequest_AndReturnBadRequest()
        {
            // Arrange
            MetricsRequest? nullRequest = null;

            // Act
            var result = await _controller.GetLeadTime(nullRequest!);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Request cannot be null", badRequestResult.Value);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldHandleNullCredentials_AndReturnBadRequest")]
        public async Task MetricsController_GetLeadTime_ShouldHandleNullCredentials_AndReturnBadRequest()
        {
            // Arrange
            var requestWithNullCredentials = new MetricsRequest
            {
                Credentials = null!,
                StartDate = "2024-01-01",
                EndDate = "2024-12-31"
            };

            // Act
            var result = await _controller.GetLeadTime(requestWithNullCredentials);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Credentials cannot be null", badRequestResult.Value);
        }

        [Fact(DisplayName = "MetricsController_GetLeadTime_ShouldHandleConfigurationDefault_WhenUseActorModelNotSet")]
        public async Task MetricsController_GetLeadTime_ShouldHandleConfigurationDefault_WhenUseActorModelNotSet()
        {
            // Arrange
            var mockProvider = new Mock<IMetricsProvider>();

            // Setup configuration to return default value (false)
            _mockConfiguration.Setup(x => x.GetValue<bool>("UseActorModel", false))
                .Returns(false);

            // Setup traditional service
            _mockProviderCache.Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockProvider.Object);

            mockProvider.Setup(x => x.CalculateLeadTimeAsync(
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(20.0);

            // Act
            var result = await _controller.GetLeadTime(_testRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            // Verify traditional service was called (default behavior)
            _mockProviderCache.Verify(x => x.GetOrCreateAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // Verify Akka service was NOT called
            _mockAkkaService.Verify(x => x.GetLeadTime(It.IsAny<JiraCredentialsRequest>()), Times.Never);
        }
    }
} 