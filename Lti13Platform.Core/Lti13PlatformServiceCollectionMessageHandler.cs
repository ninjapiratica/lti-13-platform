using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.Core
{
    public class Lti13PlatformServiceCollectionMessageHandler(IServiceCollection baseCollection, string messageType) : Lti13PlatformBuilder(baseCollection)
    {
        public Lti13PlatformServiceCollectionMessageHandler Extend<T, U>()
            where T : class
            where U : Populator<T>
        {
            base.ExtendLti13Message<T, U>(messageType);

            return this;
        }
    }
}
