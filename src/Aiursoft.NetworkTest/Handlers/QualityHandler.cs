using Aiursoft.CommandFramework.Framework;

namespace Aiursoft.NetworkTest.Handlers;

public class QualityHandler : NavigationCommandHandlerBuilder
{
    protected override string Name => "quality";

    protected override string Description => "Network quality testing commands.";

    protected override CommandHandlerBuilder[] GetSubCommandHandlers() =>
    [
        new DomesticLatencyHandler(),
        new InternationalLatencyHandler(),
        new IPv6ConnectivityHandler(),
        new AllTestsHandler()
        // Future tests will be added here:
        // new DomesticSpeedHandler(),
        // new DomesticPacketLossHandler(),
        // new InternationalSpeedHandler(),
        // new InternationalPacketLossHandler()
    ];
}
