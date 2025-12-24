using System.Diagnostics.CodeAnalysis;
using Aiursoft.CommandFramework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.NetworkTest.Handlers;

namespace Aiursoft.NetworkTest;

[ExcludeFromCodeCoverage]
internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await new NestedCommandApp()
            .WithGlobalOptions(CommonOptionsProvider.VerboseOption)
            .WithFeature(new QualityHandler())
            .RunAsync(args);
    }
}
