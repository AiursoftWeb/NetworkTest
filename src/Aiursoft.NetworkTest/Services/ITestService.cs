namespace Aiursoft.NetworkTest.Services;

/// <summary>
/// Standardized interface for all network quality test services.
/// Each test should return a score between 0-100.
/// </summary>
public interface ITestService
{
    /// <summary>
    /// Runs the test and returns a score between 0 and 100.
    /// The test should render a real-time table showing progress.
    /// </summary>
    Task<double> RunTestAsync();

    /// <summary>
    /// Gets the display name of this test.
    /// </summary>
    string TestName { get; }
}
