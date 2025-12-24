using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Services;

public class TableRenderer
{
    public void RenderTestResultsTable(List<EndpointTestResult> results)
    {
        if (results.Count == 0)
        {
            Console.WriteLine("No test results available.");
            return;
        }

        // Print table header
        Console.WriteLine();
        Console.WriteLine("| {0,-15} | {1,-10} | {2,-10} | {3,-10} | {4,-10} | {5,-10} | {6,-10} | {7,-10} |",
            "Service", "Min (ms)", "Avg (ms)", "StdDev", "Max (ms)", ">100ms %", ">200ms %", ">300ms %");
        Console.WriteLine("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|",
            new string('-', 17), new string('-', 12), new string('-', 12), new string('-', 12),
            new string('-', 12), new string('-', 12), new string('-', 12), new string('-', 12));

        // Print each result
        foreach (var result in results)
        {
            RenderResultRow(result);
        }

        Console.WriteLine();
    }

    public void RenderResultRow(EndpointTestResult result)
    {
        Console.Write("| {0,-15} | ", TruncateString(result.EndpointName, 15));

        // Min latency
        RenderColoredLatency(result.MinLatency, 10);
        Console.Write(" | ");

        // Average latency
        RenderColoredLatency(result.AverageLatency, 10);
        Console.Write(" | ");

        // Standard deviation
        RenderColoredLatency(result.StandardDeviation, 10);
        Console.Write(" | ");

        // Max latency
        RenderColoredLatency(result.MaxLatency, 10);
        Console.Write(" | ");

        // Percentage thresholds
        RenderColoredPercentage(result.PercentageOver100Ms, 10);
        Console.Write(" | ");

        RenderColoredPercentage(result.PercentageOver200Ms, 10);
        Console.Write(" | ");

        RenderColoredPercentage(result.PercentageOver300Ms, 10);
        Console.Write(" |");

        Console.WriteLine();
    }

    private void RenderColoredLatency(double latency, int width)
    {
        var color = GetLatencyColor(latency);
        Console.ForegroundColor = color;
        Console.Write(latency.ToString("F2").PadLeft(width));
        Console.ResetColor();
    }

    private void RenderColoredPercentage(double percentage, int width)
    {
        var color = GetPercentageColor(percentage);
        Console.ForegroundColor = color;
        Console.Write(percentage.ToString("F1").PadLeft(width));
        Console.ResetColor();
    }

    private ConsoleColor GetLatencyColor(double latency)
    {
        return latency switch
        {
            < 30 => ConsoleColor.Green,
            < 50 => ConsoleColor.DarkGreen,
            < 100 => ConsoleColor.Yellow,
            < 150 => ConsoleColor.DarkYellow,
            < 200 => ConsoleColor.Magenta,
            < 300 => ConsoleColor.DarkMagenta,
            < 500 => ConsoleColor.Red,
            _ => ConsoleColor.DarkRed
        };
    }

    private ConsoleColor GetPercentageColor(double percentage)
    {
        return percentage switch
        {
            0 => ConsoleColor.Green,
            < 10 => ConsoleColor.DarkGreen,
            < 25 => ConsoleColor.Yellow,
            < 50 => ConsoleColor.DarkYellow,
            < 75 => ConsoleColor.Magenta,
            < 90 => ConsoleColor.Red,
            _ => ConsoleColor.DarkRed
        };
    }

    public void RenderScoreSummary(string testName, double score)
    {
        Console.WriteLine();
        Console.Write($"{testName} Score: ");

        var color = GetScoreColor(score);
        Console.ForegroundColor = color;
        Console.Write($"{score:F2}");
        Console.ResetColor();
        Console.WriteLine(" / 100");
        Console.WriteLine();
    }

    public void RenderOverallScoreSummary(Dictionary<string, double> testScores, double overallScore)
    {
        Console.WriteLine();
        Console.WriteLine("=== Network Quality Test Summary ===");
        Console.WriteLine();
        Console.WriteLine("| {0,-30} | {1,-10} |", "Test Name", "Score");
        Console.WriteLine("|{0}|{1}|", new string('-', 32), new string('-', 12));

        foreach (var (testName, score) in testScores)
        {
            Console.Write("| {0,-30} | ", testName);
            RenderColoredScore(score, 10);
            Console.WriteLine(" |");
        }

        Console.WriteLine("|{0}|{1}|", new string('-', 32), new string('-', 12));
        Console.Write("| {0,-30} | ", "Overall Score");
        RenderColoredScore(overallScore, 10);
        Console.WriteLine(" |");
        Console.WriteLine();
    }

    private void RenderColoredScore(double score, int width)
    {
        var color = GetScoreColor(score);
        Console.ForegroundColor = color;
        Console.Write(score.ToString("F2").PadLeft(width));
        Console.ResetColor();
    }

    private ConsoleColor GetScoreColor(double score)
    {
        return score switch
        {
            >= 90 => ConsoleColor.Green,
            >= 80 => ConsoleColor.DarkGreen,
            >= 70 => ConsoleColor.Yellow,
            >= 60 => ConsoleColor.DarkYellow,
            >= 50 => ConsoleColor.Magenta,
            >= 40 => ConsoleColor.DarkMagenta,
            >= 30 => ConsoleColor.Red,
            >= 20 => ConsoleColor.DarkRed,
            >= 10 => ConsoleColor.Gray,
            _ => ConsoleColor.DarkGray
        };
    }

    private string TruncateString(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 2) + "..";
    }

    public void RenderConnectivityTestsTable(List<ConnectivityTestResult> results)
    {
        if (results.Count == 0)
        {
            Console.WriteLine("No connectivity test results available.");
            return;
        }

        // Print table header
        Console.WriteLine();
        Console.WriteLine("| {0,-20} | {1,-10} | {2,-10} | {3,-50} |",
            "Test", "Status", "Score", "Notes");
        Console.WriteLine("|{0}|{1}|{2}|{3}|",
            new string('-', 22), new string('-', 12), new string('-', 12), new string('-', 52));

        // Print each result
        foreach (var result in results)
        {
            RenderConnectivityResultRow(result);
        }

        Console.WriteLine();
    }

    private void RenderConnectivityResultRow(ConnectivityTestResult result)
    {
        Console.Write("| {0,-20} | ", TruncateString(result.TestName, 20));

        // Status
        var statusText = $"{result.SuccessfulEndpoints}/{result.TotalEndpoints}";
        var statusColor = result.SuccessfulEndpoints >= 2 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.ForegroundColor = statusColor;
        Console.Write(statusText.PadLeft(10));
        Console.ResetColor();
        Console.Write(" | ");

        // Score
        RenderColoredScore(result.Score, 10);
        Console.Write(" | ");

        // Notes
        var notesColor = result.HasPublicIP ? ConsoleColor.Green : ConsoleColor.Yellow;
        Console.ForegroundColor = notesColor;
        Console.Write(TruncateString(result.Notes, 50).PadRight(50));
        Console.ResetColor();
        Console.Write(" |");

        Console.WriteLine();
    }

    public void RenderNATTestResults(NATTestResult result)
    {
        Console.WriteLine();
        Console.WriteLine("| {0,-25} | {1,-50} | {2,-10} |", "Test Item", "Result", "Score");
        Console.WriteLine("|{0}|{1}|{2}|",
            new string('-', 27), new string('-', 52), new string('-', 12));

        // NAT Type
        Console.Write("| {0,-25} | ", "NAT Type");
        var natColor = result.NATType switch
        {
            NATType.OpenInternet => ConsoleColor.Green,
            NATType.FullCone => ConsoleColor.Green,
            NATType.RestrictedCone => ConsoleColor.Yellow,
            NATType.PortRestrictedCone => ConsoleColor.DarkYellow,
            NATType.Symmetric => ConsoleColor.Red,
            NATType.UDPBlocked => ConsoleColor.Red,
            _ => ConsoleColor.Gray
        };
        Console.ForegroundColor = natColor;
        Console.Write(TruncateString(result.NATTypeDescription, 50).PadRight(50));
        Console.ResetColor();
        Console.Write(" | ");
        RenderColoredScore(result.BaseScore, 10);
        Console.WriteLine(" |");

        // Local IP
        Console.WriteLine("| {0,-25} | {1,-50} | {2,10} |",
            "Local IP",
            TruncateString(result.LocalIP ?? "N/A", 50),
            "");

        // Public IP
        var publicIPDisplay = result.MappedPublicIP != null
            ? $"{result.MappedPublicIP}:{result.MappedPublicPort}"
            : "N/A";
        Console.WriteLine("| {0,-25} | {1,-50} | {2,10} |",
            "Public IP (via STUN)",
            TruncateString(publicIPDisplay, 50),
            "");

        // Behind NAT
        Console.Write("| {0,-25} | ", "Behind NAT");
        var natStatusColor = result.BehindNAT ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.ForegroundColor = natStatusColor;
        Console.Write((result.BehindNAT ? "Yes" : "No").PadRight(50));
        Console.ResetColor();
        Console.WriteLine(" | {0,10} |", "");

        // UPnP
        Console.Write("| {0,-25} | ", "UPnP Port Mapping");
        var upnpColor = result.UPnPAvailable ? ConsoleColor.Green : ConsoleColor.DarkGray;
        var upnpText = result.UPnPAvailable
            ? $"Available (tested port {result.UPnPTestedPort})"
            : "Not available";
        Console.ForegroundColor = upnpColor;
        Console.Write(TruncateString(upnpText, 50).PadRight(50));
        Console.ResetColor();
        Console.Write(" | ");
        if (result.UPnPBonus > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"+{result.UPnPBonus:F0}".PadLeft(10));
            Console.ResetColor();
        }
        else
        {
            Console.Write("".PadLeft(10));
        }
        Console.WriteLine(" |");

        // P2P Capability
        Console.Write("| {0,-25} | ", "P2P Connectivity");
        var p2pColor = result.NATType switch
        {
            NATType.OpenInternet or NATType.FullCone => ConsoleColor.Green,
            NATType.RestrictedCone => ConsoleColor.DarkGreen,
            NATType.PortRestrictedCone when result.UPnPAvailable => ConsoleColor.Yellow,
            NATType.PortRestrictedCone => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Red
        };
        Console.ForegroundColor = p2pColor;
        Console.Write(TruncateString(result.P2PCapability, 50).PadRight(50));
        Console.ResetColor();
        Console.WriteLine(" | {0,10} |", "");

        // Separator
        Console.WriteLine("|{0}|{1}|{2}|",
            new string('-', 27), new string('-', 52), new string('-', 12));

        // Final Score
        Console.Write("| {0,-25} | {1,-50} | ", "Final Score", "");
        RenderColoredScore(result.Score, 10);
        Console.WriteLine(" |");

        Console.WriteLine();
    }

}
