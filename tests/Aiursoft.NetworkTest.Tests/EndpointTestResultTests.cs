using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Tests;

[TestClass]
public class EndpointTestResultTests
{
    [TestMethod]
    public void MinLatency_WithMultipleLatencies_ReturnsMinimum()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 50, 75, 200 }
        };

        // Act
        var minLatency = result.MinLatency;

        // Assert
        Assert.AreEqual(50, minLatency, "Should return minimum latency");
    }

    [TestMethod]
    public void MinLatency_WithEmptyLatencies_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double>()
        };

        // Act
        var minLatency = result.MinLatency;

        // Assert
        Assert.AreEqual(0, minLatency, "Empty latencies should return 0");
    }

    [TestMethod]
    public void MaxLatency_WithMultipleLatencies_ReturnsMaximum()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 50, 75, 200 }
        };

        // Act
        var maxLatency = result.MaxLatency;

        // Assert
        Assert.AreEqual(200, maxLatency, "Should return maximum latency");
    }

    [TestMethod]
    public void MaxLatency_WithEmptyLatencies_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double>()
        };

        // Act
        var maxLatency = result.MaxLatency;

        // Assert
        Assert.AreEqual(0, maxLatency, "Empty latencies should return 0");
    }

    [TestMethod]
    public void AverageLatency_WithMultipleLatencies_ReturnsCorrectAverage()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 200, 300 }
        };

        // Act
        var avgLatency = result.AverageLatency;

        // Assert
        Assert.AreEqual(200, avgLatency, "Should return correct average");
    }

    [TestMethod]
    public void AverageLatency_WithEmptyLatencies_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double>()
        };

        // Act
        var avgLatency = result.AverageLatency;

        // Assert
        Assert.AreEqual(0, avgLatency, "Empty latencies should return 0");
    }

    [TestMethod]
    public void MedianLatency_WithOddCountLatencies_ReturnsMiddleValue()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 300, 200 }
        };

        // Act
        var median = result.MedianLatency;

        // Assert
        Assert.AreEqual(200, median, "Should return middle value for odd count");
    }

    [TestMethod]
    public void MedianLatency_WithEvenCountLatencies_ReturnsAverageOfMiddleTwo()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 200, 300, 400 }
        };

        // Act
        var median = result.MedianLatency;

        // Assert
        Assert.AreEqual(250, median, "Should return average of two middle values");
    }

    [TestMethod]
    public void MedianLatency_WithEmptyLatencies_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double>()
        };

        // Act
        var median = result.MedianLatency;

        // Assert
        Assert.AreEqual(0, median, "Empty latencies should return 0");
    }

    [TestMethod]
    public void MedianLatency_WithSingleLatency_ReturnsThatValue()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 150 }
        };

        // Act
        var median = result.MedianLatency;

        // Assert
        Assert.AreEqual(150, median, "Single latency should return itself");
    }

    [TestMethod]
    public void PercentageOver100Ms_CalculatesCorrectly()
    {
        // Arrange - 2 out of 4 are over 100ms
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 50, 150, 75, 200 }
        };

        // Act
        var percentage = result.PercentageOver100Ms;

        // Assert
        Assert.AreEqual(50.0, percentage, 0.01, "Should be 50%");
    }

    [TestMethod]
    public void PercentageOver100Ms_WithEmptyLatencies_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double>()
        };

        // Act
        var percentage = result.PercentageOver100Ms;

        // Assert
        Assert.AreEqual(0.0, percentage, "Empty latencies should return 0%");
    }

    [TestMethod]
    public void PercentageOver200Ms_CalculatesCorrectly()
    {
        // Arrange - 1 out of 4 is over 200ms
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 50, 150, 200, 250 }
        };

        // Act
        var percentage = result.PercentageOver200Ms;

        // Assert
        Assert.AreEqual(25.0, percentage, 0.01, "Should be 25%");
    }

    [TestMethod]
    public void PercentageOver300Ms_CalculatesCorrectly()
    {
        // Arrange - 1 out of 5 is over 300ms
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 50, 150, 250, 300, 350 }
        };

        // Act
        var percentage = result.PercentageOver300Ms;

        // Assert
        Assert.AreEqual(20.0, percentage, 0.01, "Should be 20%");
    }

    [TestMethod]
    public void StandardDeviation_WithUniformValues_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 100, 100, 100 }
        };

        // Act
        var stdDev = result.StandardDeviation;

        // Assert
        Assert.AreEqual(0.0, stdDev, 0.01, "Uniform values should have 0 std dev");
    }

    [TestMethod]
    public void StandardDeviation_WithVariedValues_CalculatesCorrectly()
    {
        // Arrange - Simple case with known std dev
        // Values: 10, 20, 30. Mean = 20. Variance = ((100+0+100)/3) = 66.67. StdDev = 8.165
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 10, 20, 30 }
        };

        // Act
        var stdDev = result.StandardDeviation;

        // Assert
        Assert.AreEqual(8.165, stdDev, 0.01, "Should calculate correct std dev");
    }

    [TestMethod]
    public void StandardDeviation_WithEmptyLatencies_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double>()
        };

        // Act
        var stdDev = result.StandardDeviation;

        // Assert
        Assert.AreEqual(0.0, stdDev, "Empty latencies should return 0");
    }

    [TestMethod]
    public void StandardDeviation_WithSingleValue_ReturnsZero()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100 }
        };

        // Act
        var stdDev = result.StandardDeviation;

        // Assert
        Assert.AreEqual(0.0, stdDev, 0.01, "Single value should have 0 std dev");
    }

    [TestMethod]
    public void FailedCount_IsStoredCorrectly()
    {
        // Arrange
        var result = new EndpointTestResult
        {
            EndpointName = "Test",
            Url = "https://example.com",
            Latencies = new List<double> { 100, 200 },
            FailedCount = 5
        };

        // Act
        var failedCount = result.FailedCount;

        // Assert
        Assert.AreEqual(5, failedCount, "Failed count should be stored correctly");
    }

    [TestMethod]
    public void AllProperties_CanBeInitialized()
    {
        // Arrange & Act
        var result = new EndpointTestResult
        {
            EndpointName = "Google DNS",
            Url = "https://dns.google",
            Latencies = new List<double> { 10, 20, 30 },
            FailedCount = 2
        };

        // Assert
        Assert.AreEqual("Google DNS", result.EndpointName);
        Assert.AreEqual("https://dns.google", result.Url);
        Assert.AreEqual(3, result.Latencies.Count);
        Assert.AreEqual(2, result.FailedCount);
    }
}
