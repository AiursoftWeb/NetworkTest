
using System.Net;
using System.Net.Sockets;
using Aiursoft.NetworkTest.Models;
using Microsoft.Extensions.Logging;

namespace Aiursoft.NetworkTest.Services;

public class UdpGameReliabilityTestService(
    ILogger<UdpGameReliabilityTestService> logger) : ITestService
{
    private static readonly string[] StunCandidates = 
    {
    "stun.qq.com:3478",
    "stun.miwifi.com:3478", 
    "stun.bjtelecom.net:3478", 
    "stun.cloudflare.com:3478",
    "stun.l.google.com:19302",
    "stun1.l.google.com:19302",
    "stun2.l.google.com:19302",
    "global.stun.twilio.com:3478"
    };

    public string TestName => "UDP Game Reliability";

    public async Task<double> RunTestAsync(bool verbose = false)
    {
        var result = await RunDetailedTestAsync();
        return result.FinalScore;
    }

    public async Task<UdpGameTestResult> RunDetailedTestAsync()
    {
        var target = await IdentifyBestStunServer();
        var result = new UdpGameTestResult
        {
            TargetHost = $"{target.Hostname} ({target.EndPoint.Address})",
            TargetPort = target.EndPoint.Port
        };

        using var client = new UdpClient(AddressFamily.InterNetwork);
        client.Client.ReceiveTimeout = 1000;
        
        logger.LogInformation($"Starting UDP Game Reliability Test to {target.Hostname} ({target.EndPoint})...");

        for (int i = 0; i < 300; i++)
        {
            var packet = new PacketResult { SequenceId = i, SendTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
            var request = BuildStunRequest(i);

            try
            {
                await client.SendAsync(request, request.Length, target.EndPoint);
                
                // We use a tight loop or just `Receive` with timeout. 
                // Since `SendAsync` is non-blocking, we need to be careful.
                // But specifically for this test, we want to simulate a game loop.
                // A game doesn't wait for the packet back before rendering the next frame, but here we need to measure RTT.
                // We can use the synchronous Receive with timeout for simplicity as per requirement pseudo-code.
                
                // However, Receive returns a remoteIP. We should check if transaction ID matches if we were doing full STUN parsing.
                // For simplicity and speed as per requirements, we assume the next packet is the response.
                // But to be robust against out-of-order, we should parse the Transaction ID.
                
                // Let's implement a simple receive loop with timeout
                var receiveTask = ReceiveWithTimeout(client, i);
                if (await Task.WhenAny(receiveTask, Task.Delay(1000)) == receiveTask)
                {
                    var response = await receiveTask;
                    if (response != null)
                    {
                        packet.RecvTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        packet.Rtt = packet.RecvTimestamp - packet.SendTimestamp;
                        packet.IsLost = false;
                        logger.LogDebug($"Packet {i} received. RTT: {packet.Rtt}ms");
                    }
                    else
                    {
                        packet.IsLost = true;
                        logger.LogWarning($"Packet {i} received null/empty response.");
                    }
                }
                else
                {
                    packet.IsLost = true;
                    logger.LogWarning($"Packet {i} timed out (1000ms).");
                }
            }
            catch (Exception ex)
            {
                packet.IsLost = true;
                logger.LogError(ex,($"Packet {i} failed with exception."));
            }

            result.PacketResults.Add(packet);
            
            // Sleep 50ms (Simulate 100Hz tick rate)
            await Task.Delay(10);
        }

        CalculateMetrics(result);
        return result;
    }

    private async Task<(IPEndPoint EndPoint, string Hostname)> IdentifyBestStunServer()
    {
        var candidates = new List<(IPEndPoint EndPoint, string Hostname)>();

        // 1. Resolve all candidates in parallel (Resolve ALL IPs)
        var resolveTasks = StunCandidates.Select(async candidate => 
        {
            try 
            {
                var parts = candidate.Split(':');
                var host = parts[0];
                var port = int.Parse(parts[1]);
                var eps = await ResolveAllIPv4(host, port);
                return eps.Select(ep => (ep, host, candidate));
            }
            catch
            {
                return Enumerable.Empty<(IPEndPoint, string, string)>();
            }
        });

        var resolvedGroups = await Task.WhenAll(resolveTasks);
        foreach (var group in resolvedGroups)
        {
            // Limit to 4 IPs per hostname to avoid flooding the table
            foreach (var (ep, host, original) in group.Take(4))
            {
                candidates.Add((ep, original));
            }
        }

        if (candidates.Count == 0)
        {
            throw new Exception("Could not resolve any STUN servers.");
        }
        
        logger.LogInformation($"Resolved {candidates.Count} STUN endpoints. Benchmarking (10 packets each)...");

        // 2. Measure latency for all candidates in parallel
        var benchmarkTasks = candidates.Select(async candidate => 
        {
            var result = await MeasureLatencyAsync(candidate.EndPoint, 10);
            return (Candidate: candidate, Result: result);
        }).ToList();

        var benchmarkResults = await Task.WhenAll(benchmarkTasks);
        
        // 3. Output Table
        Console.WriteLine("\n=== STUN Server Benchmark Results ===");
        Console.WriteLine(string.Format("{0,-35} | {1,-15} | {2,-10} | {3,-10}", "Server", "IP Endpoint", "Avg RTT", "Loss"));
        Console.WriteLine(new string('-', 80));

        foreach (var item in benchmarkResults.OrderBy(x => x.Result.LossRate).ThenBy(x => x.Result.AvgRtt))
        {
             var rttDisplay = item.Result.AvgRtt < 9999 ? $"{item.Result.AvgRtt:F1} ms" : "Timeout";
             var lossDisplay = $"{item.Result.LossRate * 100:F0}%";
             Console.WriteLine(string.Format("{0,-35} | {1,-15} | {2,-10} | {3,-10}", 
                item.Candidate.Hostname, 
                item.Candidate.EndPoint, 
                rttDisplay, 
                lossDisplay));
        }
        Console.WriteLine();

        // 4. Select Best
        // Prefer 0% loss, then lowest RTT
        var best = benchmarkResults
            .Where(x => x.Result.LossRate < 0.2) // Filter out high loss > 20%
            .OrderBy(x => x.Result.AvgRtt)
            .FirstOrDefault();

        if (best.Candidate.EndPoint == null)
        {
             // Fallback to lowest loss even if RTT is high
             best = benchmarkResults.OrderBy(x => x.Result.LossRate).FirstOrDefault();
             if (best.Candidate.EndPoint == null) // All failed 100%
             {
                 throw new Exception("All STUN servers failed benchmark.");
             }
        }

        logger.LogInformation($"Selected Best Server: {best.Candidate.Hostname} ({best.Result.AvgRtt:F1}ms)");
        return best.Candidate;
    }

    private async Task<(double AvgRtt, double LossRate)> MeasureLatencyAsync(IPEndPoint target, int packetsCount)
    {
        using var client = new UdpClient(AddressFamily.InterNetwork);
        client.Client.ReceiveTimeout = 1000;
        
        int received = 0;
        long totalRtt = 0;

        for(int i=0; i<packetsCount; i++)
        {
            try 
            {
                var data = BuildStunRequest(i);
                var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await client.SendAsync(data, data.Length, target);
                
                var receiveTask = client.ReceiveAsync();
                if (await Task.WhenAny(receiveTask, Task.Delay(800)) == receiveTask) // 800ms timeout per ping
                {
                     await receiveTask;
                     var rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start;
                     totalRtt += rtt;
                     received++;
                }
            }
            catch {}
            // Small delay between pings
            await Task.Delay(5);
        }

        double loss = 1.0 - ((double)received / packetsCount);
        double avg = received > 0 ? (double)totalRtt / received : 99999;
        return (avg, loss);
    }

    private async Task<List<IPEndPoint>> ResolveAllIPv4(string host, int port)
    {
        try 
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            return addresses
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => new IPEndPoint(a, port))
                .ToList();
        }
        catch
        {
            // Logging would be good practice but we might not want to spam logs here
            return new List<IPEndPoint>();
        }
    }

    private async Task<byte[]?> ReceiveWithTimeout(UdpClient client, int expectedId)
    {
        try 
        {
             // In a real STUN client we would parse the headers.
             // We wrapped ReceiveAsync in Task.WhenAny in the caller, but here we just await it.
             
             var result = await client.ReceiveAsync();
             
             // Basic STUN validation: Length >= 20
             if (result.Buffer.Length >= 20)
             {
                 // Transaction ID starts at offset 8 (12 bytes).
                 // We stored sequence ID in the first 4 bytes (8,9,10,11).
                 // Check if it matches expectedId
                 var receivedId = BitConverter.ToInt32(result.Buffer, 8);
                 if (receivedId == expectedId)
                 {
                     return result.Buffer;
                 }
             }
             return null;

        } 
        catch 
        { 
            return null; 
        }
    }

    private byte[] BuildStunRequest(int transId)
    {
        // 0x0001: Binding Request
        // 0x0000: Message Length (0)
        // 0x2112A442: Magic Cookie
        // Transaction ID (12 bytes)
        var packet = new byte[20];
        packet[0] = 0x00; packet[1] = 0x01; // Type
        packet[2] = 0x00; packet[3] = 0x00; // Length
        packet[4] = 0x21; packet[5] = 0x12; packet[6] = 0xA4; packet[7] = 0x42; // Magic Cookie
        
        // Transaction ID (12 bytes)
        // Generate random bytes for the whole ID first to avoid "all-zero" patterns that firewalls drop
        Random.Shared.NextBytes(packet.AsSpan(8, 12));

        // Embed sequence ID in the first 4 bytes for tracking
        var seqBytes = BitConverter.GetBytes(transId);
        packet[8] = seqBytes[0];
        packet[9] = seqBytes[1];
        packet[10] = seqBytes[2];
        packet[11] = seqBytes[3];
        
        return packet;
    }

    private void CalculateMetrics(UdpGameTestResult result)
    {
        // 1. Jitter Calculation (RFC 3550)
        var validPackets = result.PacketResults.Where(r => !r.IsLost).OrderBy(r => r.SequenceId).ToList();
        double totalJitter = 0;
        for (int k = 0; k < validPackets.Count - 1; k++)
        {
            long rttDiff = Math.Abs(validPackets[k].Rtt - validPackets[k + 1].Rtt);
            totalJitter += rttDiff;
        }
        result.AvgJitter = (validPackets.Count > 1) ? (totalJitter / (validPackets.Count - 1)) : 0;

        // 2. Latency Score (Base Score)
        // Logarithmic decay: 10ms -> 100, 50ms -> 70, 90ms -> 50, 130ms -> 40, 170ms -> 35
        // Formula: 100 - 22 * ln(Latency / 10)
        double avgLatency = result.AvgLatency;
        double latencyScore = 100;
        if (avgLatency > 10)
        {
            latencyScore = 100 - 22.0 * Math.Log(avgLatency / 10.0);
        }
        latencyScore = Math.Clamp(latencyScore, 0, 100);

        // 3. Jitter Multiplier
        // Penalty: Multiplier = 1 - (Jitter / 100)
        // 1ms -> 0.99, 10ms -> 0.90, 50ms -> 0.50
        double jitterMultiplier = Math.Max(0, 1.0 - (result.AvgJitter / 100.0));

        // 4. Loss Penalty (Cruel Deduction)
        // Each 1% loss deducts 10 points. 10% loss -> -100 points.
        double lossRatePercent = result.LossRate * 100;
        double lossPenalty = lossRatePercent * 10;

        // 5. Final Calculation
        result.FinalScore = (latencyScore * jitterMultiplier) - lossPenalty;
        result.FinalScore = Math.Max(0, result.FinalScore); // No negative scores

        // 6. Generate Formula String
        result.ScoreFormula = $"Score = (LatencyScore[{latencyScore:F1}] * JitterMult[{jitterMultiplier:F2}]) - LossPenalty[{lossPenalty:F1}]";
        
        // Debug
        // logger.LogInformation($"Scoring: Latency={avgLatency:F1}ms->{latencyScore:F1}, Jitter={result.AvgJitter:F1}ms->x{jitterMultiplier:F2}, Loss={lossRatePercent:F1}%->-{lossPenalty:F1}");
    }
}
