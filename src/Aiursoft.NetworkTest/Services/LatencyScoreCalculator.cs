namespace Aiursoft.NetworkTest.Services;

/// <summary>
/// Provides logarithmic scoring algorithms for network latency testing.
/// Uses exponential decay model with unified baselines for fair comparison regardless of user location.
/// </summary>
public static class LatencyScoreCalculator
{
    /// <summary>
    /// Unified baselines for all latency tests (both China and Global products)
    /// </summary>
    private const double MinLatencyBaseline = 10.0;  // 10ms for minimum latency
    private const double AvgLatencyBaseline = 30.0;  // 30ms for average latency
    
    /// <summary>
    /// Decay coefficient for minimum latency scoring.
    /// Calculated so that 10ms = 100pt, 100ms = 50pt
    /// k = ln(0.5) / -90 ≈ 0.0077
    /// </summary>
    private const double MinLatencyDecayCoefficient = 0.0077;
    
    /// <summary>
    /// Decay coefficient for average latency scoring.
    /// Calculated so that 30ms = 100pt, 150ms = 50pt
    /// k = ln(0.5) / -120 ≈ 0.0058
    /// </summary>
    private const double AvgLatencyDecayCoefficient = 0.0058;
    
    /// <summary>
    /// Divisor for stability scoring based on standard deviation.
    /// Calculated so that 0ms = 100pt, 90ms = 50pt
    /// divisor = 90 / ln(2) ≈ 129.9
    /// </summary>
    private const double StabilityDivisor = 129.9;

    /// <summary>
    /// Calculates a latency score using logarithmic decay model.
    /// Formula: 100 × e^(-coefficient × (latency - baselineLatency))
    /// </summary>
    /// <param name="latency">The measured latency in milliseconds</param>
    /// <param name="baselineLatency">The baseline latency for 100 points</param>
    /// <param name="decayCoefficient">The decay coefficient to use</param>
    /// <returns>Score from 0-100</returns>
    private static double CalculateLatencyScore(double latency, double baselineLatency, double decayCoefficient)
    {
        if (latency <= 0) return 0;
        
        var exponent = -decayCoefficient * (latency - baselineLatency);
        var score = 100.0 * Math.Exp(exponent);
        
        // Cap at 100 points maximum
        return Math.Min(100.0, score);
    }

    /// <summary>
    /// Calculates minimum latency score using unified baseline (10ms, coefficient 0.0077)
    /// </summary>
    public static double CalculateMinLatencyScore(double minLatency)
    {
        return CalculateLatencyScore(minLatency, MinLatencyBaseline, MinLatencyDecayCoefficient);
    }

    /// <summary>
    /// Calculates average latency score using unified baseline (30ms, coefficient 0.0058)
    /// </summary>
    public static double CalculateAvgLatencyScore(double avgLatency)
    {
        return CalculateLatencyScore(avgLatency, AvgLatencyBaseline, AvgLatencyDecayCoefficient);
    }

    /// <summary>
    /// Calculates a stability score based on standard deviation of latencies.
    /// Lower standard deviation (more stable) results in higher score.
    /// Formula: 100 × e^(-standardDeviation / 129.9)
    /// Unified baseline: 0ms = 100pt, 90ms = 50pt
    /// </summary>
    /// <param name="standardDeviation">The standard deviation of latencies</param>
    /// <returns>Stability score from 0-100</returns>
    public static double CalculateStabilityScore(double standardDeviation)
    {
        if (standardDeviation < 0) return 0;
        
        var exponent = -standardDeviation / StabilityDivisor;
        return 100.0 * Math.Exp(exponent);
    }

    /// <summary>
    /// Calculates an intelligent failure penalty that excludes the most failing endpoint
    /// and applies a hidden tolerance to avoid unfair scoring from minor failures.
    /// </summary>
    /// <param name="failuresByEndpoint">List of failure counts for each endpoint</param>
    /// <returns>Penalty points to deduct from score</returns>
    public static double CalculateSmartFailurePenalty(List<int> failuresByEndpoint)
    {
        if (failuresByEndpoint.Count == 0) return 0;
        
        // Find and exclude the endpoint with most failures (likely server issue, not network quality)
        var maxFailures = failuresByEndpoint.Max();
        var remainingFailures = failuresByEndpoint.Sum() - maxFailures;
        
        // Apply hidden tolerance: 10 points = 5 failures × 2 points/failure
        // This prevents minor fluctuations from affecting the score
        const int hiddenTolerancePoints = 10;
        const int pointsPerFailure = 2;
        
        var totalPenalty = remainingFailures * pointsPerFailure;
        var actualPenalty = Math.Max(0, totalPenalty - hiddenTolerancePoints);
        
        return actualPenalty;
    }

    /// <summary>
    /// Calculates a comprehensive score considering minimum latency, average latency, and stability.
    /// Weights: Min latency 40%, Average latency 40%, Stability 20%
    /// Uses unified baselines for fair comparison regardless of location.
    /// </summary>
    /// <param name="minLatency">Minimum latency observed (reflects best link quality)</param>
    /// <param name="avgLatency">Average latency (reflects overall performance)</param>
    /// <param name="standardDeviation">Standard deviation of latencies (reflects stability)</param>
    /// <param name="failuresByEndpoint">List of failure counts for each endpoint</param>
    /// <returns>Final comprehensive score from 0-100</returns>
    public static double CalculateComprehensiveScore(
        double minLatency,
        double avgLatency,
        double standardDeviation,
        List<int> failuresByEndpoint)
    {
        // Calculate individual scores with unified baselines
        var minLatencyScore = CalculateLatencyScore(minLatency, MinLatencyBaseline, MinLatencyDecayCoefficient);
        var avgLatencyScore = CalculateLatencyScore(avgLatency, AvgLatencyBaseline, AvgLatencyDecayCoefficient);
        var stabilityScore = CalculateStabilityScore(standardDeviation);

        // Weighted combination: 40% min + 40% avg + 20% stability
        var weightedScore = (minLatencyScore * 0.4) + (avgLatencyScore * 0.4) + (stabilityScore * 0.2);

        // Apply smart failure penalty (excludes worst endpoint, applies hidden tolerance)
        var failurePenalty = CalculateSmartFailurePenalty(failuresByEndpoint);
        var finalScore = weightedScore - failurePenalty;

        // Ensure score is within 0-100 range
        return Math.Max(0, Math.Min(100, finalScore));
    }
}
