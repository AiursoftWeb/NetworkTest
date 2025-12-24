using Aiursoft.CommandFramework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.NetworkTest.Handlers;

namespace Aiursoft.NetworkTest.Tests;

[TestClass]
public class IntegrationTests
{
    private readonly NestedCommandApp _program = new NestedCommandApp()
        .WithGlobalOptions(CommonOptionsProvider.VerboseOption)
        .WithFeature(new QualityHandler());

    [TestMethod]
    public async Task InvokeHelp()
    {
        var result = await _program.TestRunAsync(["--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.Contains("quality", result.StdOut);
    }

    [TestMethod]
    public async Task InvokeVersion()
    {
        var result = await _program.TestRunAsync(["--version"]);
        Assert.AreEqual(0, result.ProgramReturn);
    }

    [TestMethod]
    public async Task InvokeUnknown()
    {
        var result = await _program.TestRunAsync(["--wtf"]);
        Assert.AreEqual(1, result.ProgramReturn);
    }

    [TestMethod]
    public async Task InvokeQualityHelp()
    {
        var result = await _program.TestRunAsync(["quality", "--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.Contains("domestic-latency", result.StdOut);
        Assert.Contains("international-latency", result.StdOut);
        Assert.Contains("all", result.StdOut);
    }

    [TestMethod]
    public async Task InvokeDomesticLatencyHelp()
    {
        var result = await _program.TestRunAsync(["quality", "domestic-latency", "--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.Contains("Test domestic", result.StdOut);
    }

    [TestMethod]
    public async Task InvokeInternationalLatencyHelp()
    {
        var result = await _program.TestRunAsync(["quality", "international-latency", "--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.Contains("Test international", result.StdOut);
    }

    [TestMethod]
    public async Task InvokeNATTraversalHelp()
    {
        var result = await _program.TestRunAsync(["quality", "nat-traversal", "--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.Contains("NAT type", result.StdOut);
        Assert.Contains("P2P", result.StdOut);
    }
}
