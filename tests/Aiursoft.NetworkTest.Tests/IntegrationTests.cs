using Aiursoft.CommandFramework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.NetworkTest.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        Assert.IsTrue(result.StdOut.Contains("quality"));
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
        Assert.IsTrue(result.StdOut.Contains("domestic-latency"));
        Assert.IsTrue(result.StdOut.Contains("international-latency"));
        Assert.IsTrue(result.StdOut.Contains("all"));
    }

    [TestMethod]
    public async Task InvokeDomesticLatencyHelp()
    {
        var result = await _program.TestRunAsync(["quality", "domestic-latency", "--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.IsTrue(result.StdOut.Contains("Test domestic"));
    }

    [TestMethod]
    public async Task InvokeInternationalLatencyHelp()
    {
        var result = await _program.TestRunAsync(["quality", "international-latency", "--help"]);
        Assert.AreEqual(0, result.ProgramReturn);
        Assert.IsTrue(result.StdOut.Contains("Test international"));
    }
}
