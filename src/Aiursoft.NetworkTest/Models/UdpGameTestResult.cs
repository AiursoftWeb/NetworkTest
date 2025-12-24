


namespace Aiursoft.NetworkTest.Models;

public class PacketResult
{
    public int SequenceId { get; set; }
    public long SendTimestamp { get; set; }
    public long RecvTimestamp { get; set; }
    public bool IsLost { get; set; }
    public long Rtt { get; set; }
}

public class UdpGameTestResult
{
    public string TargetHost { get; set; } = string.Empty;
    public int TargetPort { get; set; }
    public List<PacketResult> PacketResults { get; set; } = new();
    
    public int SentCount => PacketResults.Count;
    public int LostCount => PacketResults.Count(p => p.IsLost);
    public double LossRate => SentCount == 0 ? 0 : (double)LostCount / SentCount * 100;
    
    public double AvgJitter { get; set; }
    public double FinalScore { get; set; }
    public string ScoreFormula { get; set; } = string.Empty;
    public string LossGrade { get; set; } = string.Empty;
    public string JitterGrade { get; set; } = string.Empty;
    
    public double AvgLatency => PacketResults.Any(p => !p.IsLost) 
        ? PacketResults.Where(p => !p.IsLost).Average(p => p.Rtt) 
        : 0;
        
    public double MaxLatency => PacketResults.Any(p => !p.IsLost) 
        ? PacketResults.Where(p => !p.IsLost).Max(p => p.Rtt) 
        : 0;
        
    public double MinLatency => PacketResults.Any(p => !p.IsLost) 
        ? PacketResults.Where(p => !p.IsLost).Min(p => p.Rtt) 
        : 0;
}
