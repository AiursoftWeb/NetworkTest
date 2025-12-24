using Aiursoft.NetworkTest.Services;

namespace Aiursoft.NetworkTest.Tests;

[TestClass]
public class LatencyScoreCalculatorTests
{
    [TestMethod]
    public void CalculateMinLatencyScore_With10Ms_Returns100Points()
    {
        // Arrange
        var latency = 10.0;

        // Act
        var score = LatencyScoreCalculator.CalculateMinLatencyScore(latency);

        // Assert
        Assert.AreEqual(100.0, score, 0.1, "10ms should yield 100 points");
    }

    [TestMethod]
    public void CalculateMinLatencyScore_With100Ms_ReturnsApproximately50Points()
    {
        // Arrange
        var latency = 100.0;

        // Act
        var score = LatencyScoreCalculator.CalculateMinLatencyScore(latency);

        // Assert
        Assert.AreEqual(50.0, score, 1.0, "100ms should yield approximately 50 points");
    }

    [TestMethod]
    public void CalculateMinLatencyScore_WithZero_ReturnsZero()
    {
        // Arrange
        var latency = 0.0;

        // Act
        var score = LatencyScoreCalculator.CalculateMinLatencyScore(latency);

        // Assert
        Assert.AreEqual(0.0, score, "0ms should yield 0 points");
    }

    [TestMethod]
    public void CalculateMinLatencyScore_WithVeryLowLatency_ReturnsCappedAt100()
    {
        // Arrange
        var latency = 1.0;

        // Act
        var score = LatencyScoreCalculator.CalculateMinLatencyScore(latency);

        // Assert
        Assert.IsTrue(score <= 100.0, "Score should not exceed 100");
        Assert.IsTrue(score >= 99.0, "Very low latency should yield near-maximum score");
    }

    [TestMethod]
    public void CalculateAvgLatencyScore_With30Ms_Returns100Points()
    {
        // Arrange
        var latency = 30.0;

        // Act
        var score = LatencyScoreCalculator.CalculateAvgLatencyScore(latency);

        // Assert
        Assert.AreEqual(100.0, score, 0.1, "30ms should yield 100 points");
    }

    [TestMethod]
    public void CalculateAvgLatencyScore_With150Ms_ReturnsApproximately50Points()
    {
        // Arrange
        var latency = 150.0;

        // Act
        var score = LatencyScoreCalculator.CalculateAvgLatencyScore(latency);

        // Assert
        Assert.AreEqual(50.0, score, 1.0, "150ms should yield approximately 50 points");
    }

    [TestMethod]
    public void CalculateAvgLatencyScore_WithZero_ReturnsZero()
    {
        // Arrange
        var latency = 0.0;

        // Act
        var score = LatencyScoreCalculator.CalculateAvgLatencyScore(latency);

        // Assert
        Assert.AreEqual(0.0, score, "0ms should yield 0 points");
    }

    [TestMethod]
    public void CalculateStabilityScore_WithZeroStdDev_Returns100Points()
    {
        // Arrange
        var stdDev = 0.0;

        // Act
        var score = LatencyScoreCalculator.CalculateStabilityScore(stdDev);

        // Assert
        Assert.AreEqual(100.0, score, 0.1, "0ms std dev should yield 100 points");
    }

    [TestMethod]
    public void CalculateStabilityScore_With90MsStdDev_ReturnsApproximately50Points()
    {
        // Arrange
        var stdDev = 90.0;

        // Act
        var score = LatencyScoreCalculator.CalculateStabilityScore(stdDev);

        // Assert
        Assert.AreEqual(50.0, score, 1.0, "90ms std dev should yield approximately 50 points");
    }

    [TestMethod]
    public void CalculateStabilityScore_WithNegativeValue_ReturnsZero()
    {
        // Arrange
        var stdDev = -10.0;

        // Act
        var score = LatencyScoreCalculator.CalculateStabilityScore(stdDev);

        // Assert
        Assert.AreEqual(0.0, score, "Negative std dev should yield 0 points");
    }

    [TestMethod]
    public void CalculateSmartFailurePenalty_WithNoFailures_ReturnsZero()
    {
        // Arrange
        var failures = new List<int>();

        // Act
        var penalty = LatencyScoreCalculator.CalculateSmartFailurePenalty(failures);

        // Assert
        Assert.AreEqual(0.0, penalty, "No failures should yield no penalty");
    }

    [TestMethod]
    public void CalculateSmartFailurePenalty_WithinHiddenTolerance_ReturnsZero()
    {
        // Arrange - 5 failures total, but within tolerance (10 points / 2 points per failure = 5)
        var failures = new List<int> { 2, 2, 1 };

        // Act
        var penalty = LatencyScoreCalculator.CalculateSmartFailurePenalty(failures);

        // Assert
        Assert.AreEqual(0.0, penalty, "Failures within tolerance should yield no penalty");
    }

    [TestMethod]
    public void CalculateSmartFailurePenalty_ExcludesMostFailingEndpoint()
    {
        // Arrange - One endpoint has 10 failures (excluded), others have 2 each
        var failures = new List<int> { 10, 2, 2 };

        // Act  
        var penalty = LatencyScoreCalculator.CalculateSmartFailurePenalty(failures);

        // Assert - Only 4 failures count (2+2), within tolerance, so no penalty
        Assert.AreEqual(0.0, penalty, "Most failing endpoint should be excluded");
    }

    [TestMethod]
    public void CalculateSmartFailurePenalty_AboveTolerance_AppliesPenalty()
    {
        // Arrange - 20 failures on worst endpoint (excluded), 10 on others (counted)
        var failures = new List<int> { 20, 5, 5 };

        // Act
        var penalty = LatencyScoreCalculator.CalculateSmartFailurePenalty(failures);

        // Assert - 10 failures counted, 20 points total, minus 10 tolerance = 10 penalty
        Assert.AreEqual(10.0, penalty, "Failures above tolerance should apply penalty");
    }

    [TestMethod]
    public void CalculateComprehensiveScore_PerfectConditions_Returns100()
    {
        // Arrange
        var minLatency = 10.0;
        var avgLatency = 30.0;
        var stdDev = 0.0;
        var failures = new List<int>();

        // Act
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            minLatency, avgLatency, stdDev, failures);

        // Assert
        Assert.AreEqual(100.0, score, 0.1, "Perfect conditions should yield 100 points");
    }

    [TestMethod]
    public void CalculateComprehensiveScore_MediocreConditions_ReturnsLowerScore()
    {
        // Arrange
        var minLatency = 100.0;  // ~50 points
        var avgLatency = 150.0;  // ~50 points
        var stdDev = 90.0;       // ~50 points
        var failures = new List<int>();

        // Act
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            minLatency, avgLatency, stdDev, failures);

        // Assert
        // 50*0.4 + 50*0.4 + 50*0.2 = 50
        Assert.AreEqual(50.0, score, 2.0, "Mediocre conditions should yield ~50 points");
    }

    [TestMethod]
    public void CalculateComprehensiveScore_WithFailures_AppliesPenalty()
    {
        // Arrange
        var minLatency = 10.0;
        var avgLatency = 30.0;
        var stdDev = 0.0;
        var failures = new List<int> { 50, 10, 10 }; // Worst excluded, 20 counted

        // Act
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            minLatency, avgLatency, stdDev, failures);

        // Assert
        // Base 100, minus penalty: 20*2 - 10 tolerance = 30 penalty
        Assert.AreEqual(70.0, score, 1.0, "Failures should reduce score");
    }

    [TestMethod]
    public void CalculateComprehensiveScore_NeverBelowZero()
    {
        // Arrange
        var minLatency = 1000.0;
        var avgLatency = 1000.0;
        var stdDev = 500.0;
        var failures = new List<int> { 100, 100, 100 };

        // Act
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            minLatency, avgLatency, stdDev, failures);

        // Assert
        Assert.IsTrue(score >= 0.0, "Score should never be negative");
    }

    [TestMethod]
    public void CalculateComprehensiveScore_NeverAbove100()
    {
        // Arrange
        var minLatency = 1.0;
        var avgLatency = 1.0;
        var stdDev = 0.0;
        var failures = new List<int>();

        // Act
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            minLatency, avgLatency, stdDev, failures);

        // Assert
        Assert.IsTrue(score <= 100.0, "Score should never exceed 100");
    }

    [TestMethod]
    public void CalculateComprehensiveScore_WeightingIsCorrect()
    {
        // Arrange - Test that weights are 40%, 40%, 20%
        var minLatency = 10.0;   // 100 points
        var avgLatency = 1000.0; // ~0 points
        var stdDev = 0.0;        // 100 points
        var failures = new List<int>();

        // Act
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            minLatency, avgLatency, stdDev, failures);

        // Assert
        // Expected: 100*0.4 + 0*0.4 + 100*0.2 = 60
        Assert.AreEqual(60.0, score, 5.0, "Weighting should be 40-40-20");
    }
}
