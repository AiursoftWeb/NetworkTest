using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Tests;

[TestClass]
public class ConnectivityTestResultTests
{
    [TestMethod]
    public void AllProperties_CanBeInitialized()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "IPv4 Connectivity",
            SuccessfulEndpoints = 3,
            TotalEndpoints = 4,
            Score = 75.0,
            DetectedIP = "203.0.113.5",
            HasPublicIP = true,
            Notes = "Good connectivity detected"
        };

        // Assert
        Assert.AreEqual("IPv4 Connectivity", result.TestName);
        Assert.AreEqual(3, result.SuccessfulEndpoints);
        Assert.AreEqual(4, result.TotalEndpoints);
        Assert.AreEqual(75.0, result.Score);
        Assert.AreEqual("203.0.113.5", result.DetectedIP);
        Assert.IsTrue(result.HasPublicIP);
        Assert.AreEqual("Good connectivity detected", result.Notes);
    }

    [TestMethod]
    public void Notes_DefaultsToEmptyString()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Test",
            SuccessfulEndpoints = 0,
            TotalEndpoints = 1,
            Score = 0.0
        };

        // Assert
        Assert.IsNotNull(result.Notes);
        Assert.AreEqual(string.Empty, result.Notes);
    }

    [TestMethod]
    public void DetectedIP_CanBeNull()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Test",
            SuccessfulEndpoints = 0,
            TotalEndpoints = 1,
            Score = 0.0,
            DetectedIP = null
        };

        // Assert
        Assert.IsNull(result.DetectedIP);
    }

    [TestMethod]
    public void HasPublicIP_CanBeFalse()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Test",
            SuccessfulEndpoints = 0,
            TotalEndpoints = 1,
            Score = 0.0,
            HasPublicIP = false
        };

        // Assert
        Assert.IsFalse(result.HasPublicIP);
    }

    [TestMethod]
    public void SuccessfulEndpoints_CanBeZero()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Failed Test",
            SuccessfulEndpoints = 0,
            TotalEndpoints = 5,
            Score = 0.0
        };

        // Assert
        Assert.AreEqual(0, result.SuccessfulEndpoints);
        Assert.AreEqual(5, result.TotalEndpoints);
    }

    [TestMethod]
    public void SuccessfulEndpoints_CanEqualTotalEndpoints()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Perfect Test",
            SuccessfulEndpoints = 5,
            TotalEndpoints = 5,
            Score = 100.0
        };

        // Assert
        Assert.AreEqual(5, result.SuccessfulEndpoints);
        Assert.AreEqual(5, result.TotalEndpoints);
        Assert.AreEqual(100.0, result.Score);
    }

    [TestMethod]
    public void Score_CanBeZero()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Test",
            SuccessfulEndpoints = 0,
            TotalEndpoints = 1,
            Score = 0.0
        };

        // Assert
        Assert.AreEqual(0.0, result.Score);
    }

    [TestMethod]
    public void Score_CanBeDecimal()
    {
        // Arrange & Act
        var result = new ConnectivityTestResult
        {
            TestName = "Test",
            SuccessfulEndpoints = 3,
            TotalEndpoints = 4,
            Score = 75.5
        };

        // Assert
        Assert.AreEqual(75.5, result.Score);
    }
}
