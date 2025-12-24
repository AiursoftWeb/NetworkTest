using Aiursoft.NetworkTest.Models;
using Aiursoft.NetworkTest.Services;

namespace Aiursoft.NetworkTest.Tests;

[TestClass]
public class TableRendererTests
{
    [TestMethod]
    public void TruncateString_WithShortString_ReturnsOriginal()
    {
        // Arrange
        var renderer = new TableRenderer();
        var value = "Short";
        var maxLength = 20;

        // Act - Use reflection to call private method
        var method = typeof(TableRenderer).GetMethod("TruncateString",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(renderer, new object[] { value, maxLength }) as string;

        // Assert
        Assert.AreEqual("Short", result, "Short string should not be truncated");
    }

    [TestMethod]
    public void TruncateString_WithLongString_TruncatesAndAddsEllipsis()
    {
        // Arrange
        var renderer = new TableRenderer();
        var value = "This is a very long string that should be truncated";
        var maxLength = 20;

        // Act
        var method = typeof(TableRenderer).GetMethod("TruncateString",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(renderer, new object[] { value, maxLength }) as string;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(20, result.Length, "Result should be exactly maxLength");
        Assert.IsTrue(result.EndsWith(".."), "Truncated string should end with ..");
    }

    [TestMethod]
    public void TruncateString_WithExactLength_ReturnsOriginal()
    {
        // Arrange
        var renderer = new TableRenderer();
        var value = "ExactlyTwentyChars!!";
        var maxLength = 20;

        // Act
        var method = typeof(TableRenderer).GetMethod("TruncateString",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(renderer, new object[] { value, maxLength }) as string;

        // Assert
        Assert.AreEqual("ExactlyTwentyChars!!", result, "Exact length string should not be truncated");
    }

    [TestMethod]
    public void GetLatencyColor_UnderThirty_ReturnsGreen()
    {
        // Arrange
        var renderer = new TableRenderer();
        var latency = 25.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetLatencyColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { latency });

        // Assert
        Assert.AreEqual(ConsoleColor.Green, result);
    }

    [TestMethod]
    public void GetLatencyColor_Between30And50_ReturnsDarkGreen()
    {
        // Arrange
        var renderer = new TableRenderer();
        var latency = 40.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetLatencyColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { latency });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkGreen, result);
    }

    [TestMethod]
    public void GetLatencyColor_Between50And100_ReturnsYellow()
    {
        // Arrange
        var renderer = new TableRenderer();
        var latency = 75.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetLatencyColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { latency });

        // Assert
        Assert.AreEqual(ConsoleColor.Yellow, result);
    }

    [TestMethod]
    public void GetLatencyColor_Between100And150_ReturnsDarkYellow()
    {
        // Arrange
        var renderer = new TableRenderer();
        var latency = 125.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetLatencyColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { latency });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkYellow, result);
    }

    [TestMethod]
    public void GetLatencyColor_Over500_ReturnsDarkRed()
    {
        // Arrange
        var renderer = new TableRenderer();
        var latency = 600.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetLatencyColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { latency });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkRed, result);
    }

    [TestMethod]
    public void GetPercentageColor_Zero_ReturnsGreen()
    {
        // Arrange
        var renderer = new TableRenderer();
        var percentage = 0.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetPercentageColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { percentage });

        // Assert
        Assert.AreEqual(ConsoleColor.Green, result);
    }

    [TestMethod]
    public void GetPercentageColor_UnderTen_ReturnsDarkGreen()
    {
        // Arrange
        var renderer = new TableRenderer();
        var percentage = 5.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetPercentageColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { percentage });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkGreen, result);
    }

    [TestMethod]
    public void GetPercentageColor_Over90_ReturnsDarkRed()
    {
        // Arrange
        var renderer = new TableRenderer();
        var percentage = 95.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetPercentageColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { percentage });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkRed, result);
    }

    [TestMethod]
    public void GetScoreColor_Above90_ReturnsGreen()
    {
        // Arrange
        var renderer = new TableRenderer();
        var score = 95.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetScoreColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { score });

        // Assert
        Assert.AreEqual(ConsoleColor.Green, result);
    }

    [TestMethod]
    public void GetScoreColor_Between80And90_ReturnsDarkGreen()
    {
        // Arrange
        var renderer = new TableRenderer();
        var score = 85.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetScoreColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { score });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkGreen, result);
    }

    [TestMethod]
    public void GetScoreColor_Between70And80_ReturnsYellow()
    {
        // Arrange
        var renderer = new TableRenderer();
        var score = 75.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetScoreColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { score });

        // Assert
        Assert.AreEqual(ConsoleColor.Yellow, result);
    }

    [TestMethod]
    public void GetScoreColor_Under10_ReturnsDarkGray()
    {
        // Arrange
        var renderer = new TableRenderer();
        var score = 5.0;

        // Act
        var method = typeof(TableRenderer).GetMethod("GetScoreColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (ConsoleColor?)method?.Invoke(renderer, new object[] { score });

        // Assert
        Assert.AreEqual(ConsoleColor.DarkGray, result);
    }

    [TestMethod]
    public void RenderTestResultsTable_WithEmptyResults_OutputsNoResultsMessage()
    {
        // Arrange
        var renderer = new TableRenderer();
        var results = new List<EndpointTestResult>();

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderTestResultsTable(results);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("No test results available"), "Should show no results message");
    }

    [TestMethod]
    public void RenderTestResultsTable_WithResults_OutputsTable()
    {
        // Arrange
        var renderer = new TableRenderer();
        var results = new List<EndpointTestResult>
        {
            new EndpointTestResult
            {
                EndpointName = "Test Service",
                Url = "https://example.com",
                Latencies = new List<double> { 10, 20, 30 }
            }
        };

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderTestResultsTable(results);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("Service"), "Should contain header");
        Assert.IsTrue(output.Contains("Min (ms)"), "Should contain column header");
        Assert.IsTrue(output.Contains("Test Service"), "Should contain service name");
    }

    [TestMethod]
    public void RenderConnectivityTestsTable_WithEmptyResults_OutputsNoResultsMessage()
    {
        // Arrange
        var renderer = new TableRenderer();
        var results = new List<ConnectivityTestResult>();

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderConnectivityTestsTable(results);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("No connectivity test results available"), "Should show no results message");
    }

    [TestMethod]
    public void RenderConnectivityTestsTable_WithResults_OutputsTable()
    {
        // Arrange
        var renderer = new TableRenderer();
        var results = new List<ConnectivityTestResult>
        {
            new ConnectivityTestResult
            {
                TestName = "IPv4 Test",
                SuccessfulEndpoints = 3,
                TotalEndpoints = 4,
                Score = 75.0,
                Notes = "Good connectivity"
            }
        };

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderConnectivityTestsTable(results);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("IPv4 Test"), "Should contain test name");
        Assert.IsTrue(output.Contains("3/4"), "Should contain status");
    }

    [TestMethod]
    public void RenderScoreSummary_OutputsScoreWithFormatting()
    {
        // Arrange
        var renderer = new TableRenderer();

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderScoreSummary("Test Name", 85.5);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("Test Name Score"), "Should contain test name");
        Assert.IsTrue(output.Contains("85.50"), "Should contain formatted score");
        Assert.IsTrue(output.Contains("100"), "Should contain max score");
    }

    [TestMethod]
    public void RenderOverallScoreSummary_OutputsCompleteTable()
    {
        // Arrange
        var renderer = new TableRenderer();
        var testScores = new Dictionary<string, double>
        {
            { "China Products", 85.0 },
            { "Global Products", 75.0 }
        };

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderOverallScoreSummary(testScores, 80.0);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("Network Quality Test Summary"), "Should contain summary header");
        Assert.IsTrue(output.Contains("China Products"), "Should contain test name");
        Assert.IsTrue(output.Contains("Overall Score"), "Should contain overall score row");
    }

    [TestMethod]
    public void RenderNATTestResults_OutputsCompleteTable()
    {
        // Arrange
        var renderer = new TableRenderer();
        var result = new NATTestResult
        {
            NATType = NATType.FullCone,
            LocalIP = "192.168.1.100",
            MappedPublicIP = "203.0.113.5",
            MappedPublicPort = 12345,
            BehindNAT = true,
            UPnPAvailable = true,
            UPnPTestedPort = 8080,
            BaseScore = 90.0,
            UPnPBonus = 10.0
        };

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderNATTestResults(result);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("NAT Type"), "Should contain NAT Type row");
        Assert.IsTrue(output.Contains("Full Cone NAT"), "Should contain NAT description");
        Assert.IsTrue(output.Contains("192.168.1.100"), "Should contain local IP");
        Assert.IsTrue(output.Contains("203.0.113.5"), "Should contain public IP");
        Assert.IsTrue(output.Contains("UPnP Port Mapping"), "Should contain UPnP row");
        Assert.IsTrue(output.Contains("P2P Connectivity"), "Should contain P2P row");
        Assert.IsTrue(output.Contains("Final Score"), "Should contain final score");
    }

    [TestMethod]
    public void RenderNATTestResults_WithoutUPnP_DoesNotShowBonus()
    {
        // Arrange
        var renderer = new TableRenderer();
        var result = new NATTestResult
        {
            NATType = NATType.Symmetric,
            BaseScore = 0.0,
            UPnPAvailable = false,
            UPnPBonus = 0.0
        };

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        renderer.RenderNATTestResults(result);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(output.Contains("Not available"), "Should show UPnP not available");
    }
}
