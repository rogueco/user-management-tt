using Microsoft.Extensions.Logging;

namespace UserManagement.UnitTests.MockExtensions;

public static class MockLoggerExtensions
{
    public static Mock<ILogger<T>> VerifyLogLevelWasCalled<T>(this Mock<ILogger<T>> logger, LogLevel logLevel, Times times)
    {
        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times,
            $"Expected a {logLevel} log entry {times}");

        return logger;
    }
}
