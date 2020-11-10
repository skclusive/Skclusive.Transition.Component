using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Skclusive.Core.Component;
using Skclusive.Script.DomHelpers;

namespace Skclusive.Transition.Component
{
    public static class TransitionExtension
    {
        public static void TryAddTransitionServices(this IServiceCollection services, ICoreConfig config)
        {
            services.TryAddDomHelpersServices(config);
        }
    }
}
