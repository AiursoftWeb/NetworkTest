
namespace Aiursoft.NetworkTest.Services;

public class DomesticLatencyTestService : LatencyTestServiceBase
{
    public override string TestName => "China Products Latency";

    protected override string HttpClientName => "DomesticQualityTest";

    protected override List<(string Name, string Url)> Endpoints { get; } = new()
    {
        ("Xiaomi (MIUI)", "http://connect.rom.miui.com/generate_204"),
        ("Huawei (EMUI)", "http://connectivitycheck.platform.hicloud.com/generate_204"),
        ("Vivo Check", "http://wifi.vivo.com.cn/generate_204"),
        ("Aliyun DNS", "https://dns.alidns.com/resolve?name=www.taobao.com&type=1"),
        ("Tencent DNS", "https://doh.pub/resolve?name=www.qq.com"),
        ("360 DNS", "https://doh.360.cn/resolve?name=www.360.cn"),
        ("Baidu CDN", "https://www.baidu.com/favicon.ico"),
        ("Bilibili CDN", "https://i0.hdslb.com/bfs/face/member/noface.jpg"),
        ("JD (Jingdong)", "https://www.jd.com/favicon.ico"), // 京东 CDN，非常稳
        ("NetEase (163)", "https://www.163.com/favicon.ico"), // 网易门户，老牌稳定
        ("Sogou", "https://www.sogou.com/favicon.ico"), // 搜狗，解析速度通常很快
    };

    public DomesticLatencyTestService(
        IHttpClientFactory httpClientFactory,
        TableRenderer tableRenderer)
        : base(httpClientFactory, tableRenderer)
    {
    }
}
