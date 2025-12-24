namespace Aiursoft.NetworkTest.Services;

public class InternationalLatencyTestService : LatencyTestServiceBase
{
    public override string TestName => "Global Products Latency";

    protected override string HttpClientName => "InternationalQualityTest";

    protected override List<(string Name, string Url)> Endpoints { get; } = new()
    {
        // 纯文本/极简返回，适合做 HTTP 连通性测试
        ("Android (Gstatic)", "http://connectivitycheck.gstatic.com/generate_204"), // 安卓原生检测，不同于 google.com，走的是 gstatic CDN
        ("Cloudflare CP", "http://cp.cloudflare.com/"), // 也是 204，很多旁路检测工具喜欢用这个
        ("Cloudflare Trace", "https://www.cloudflare.com/cdn-cgi/trace"),
        ("Google Gen204", "https://www.google.com/generate_204"),
        ("MS Connect Test", "http://www.msftconnecttest.com/connecttest.txt"),
        ("Apple Captive", "http://captive.apple.com/hotspot-detect.html"),
        ("AWS CheckIP", "https://checkip.amazonaws.com"),
        ("Firefox Detect", "http://detectportal.firefox.com/success.txt"),
    };

    public InternationalLatencyTestService(
        IHttpClientFactory httpClientFactory,
        TableRenderer tableRenderer)
        : base(httpClientFactory, tableRenderer)
    {
    }
}
