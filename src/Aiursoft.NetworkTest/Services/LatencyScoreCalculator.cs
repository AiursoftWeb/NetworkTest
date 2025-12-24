namespace Aiursoft.NetworkTest.Services;

/// <summary>
/// Provides logarithmic scoring algorithms for network latency testing.
/// Uses exponential decay model to provide more reasonable scoring across different latency ranges.
/// </summary>
public static class LatencyScoreCalculator
{
    /// <summary>
    /// Decay coefficient for the logarithmic scoring model.
    /// This value ensures the scoring curve matches expected benchmarks (e.g., 30ms=100, 40ms=90, 50ms=81).
    /// </summary>
    private const double DecayCoefficient = 0.0105;

    /// <summary>
    /// Calculates a latency score using logarithmic decay model.
    /// Formula: 100 × e^(-k × (latency - baselineLatency))
    /// </summary>
    /// <param name="latency">The measured latency in milliseconds</param>
    /// <param name="baselineLatency">The baseline latency for 100 points (30ms for domestic, 70ms for international)</param>
    /// <returns>Score from 0-100</returns>
    public static double CalculateLatencyScore(double latency, double baselineLatency)
    {
        if (latency <= 0) return 0;
        
        var exponent = -DecayCoefficient * (latency - baselineLatency);
        var score = 100.0 * Math.Exp(exponent);
        
        // Cap at 100 points maximum
        return Math.Min(100.0, score);
    }

    /// <summary>
    /// Calculates a stability score based on standard deviation of latencies.
    /// Lower standard deviation (more stable) results in higher score.
    /// Formula: 100 × e^(-standardDeviation / 60)
    /// </summary>
    /// <param name="standardDeviation">The standard deviation of latencies</param>
    /// <returns>Stability score from 0-100</returns>
    public static double CalculateStabilityScore(double standardDeviation)
    {
        if (standardDeviation < 0) return 0;
        
        // Use divisor of 60 instead of 20 for more forgiving scoring
        // This ensures even networks with high jitter get some points
        var exponent = -standardDeviation / 60.0;
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
        if (failuresByEndpoint == null || failuresByEndpoint.Count == 0) return 0;
        
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
    /// </summary>
    /// <param name="minLatency">Minimum latency observed (reflects best link quality)</param>
    /// <param name="avgLatency">Average latency (reflects overall performance)</param>
    /// <param name="standardDeviation">Standard deviation of latencies (reflects stability)</param>
    /// <param name="minBaselineLatency">Baseline latency for minimum latency scoring (stricter)</param>
    /// <param name="avgBaselineLatency">Baseline latency for average latency scoring</param>
    /// <param name="failuresByEndpoint">List of failure counts for each endpoint</param>
    /// <returns>Final comprehensive score from 0-100</returns>
    public static double CalculateComprehensiveScore(
        double minLatency,
        double avgLatency,
        double standardDeviation,
        double minBaselineLatency,
        double avgBaselineLatency,
        List<int> failuresByEndpoint)
    {
        // Calculate individual scores with separate baselines
        var minLatencyScore = CalculateLatencyScore(minLatency, minBaselineLatency);
        var avgLatencyScore = CalculateLatencyScore(avgLatency, avgBaselineLatency);
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
