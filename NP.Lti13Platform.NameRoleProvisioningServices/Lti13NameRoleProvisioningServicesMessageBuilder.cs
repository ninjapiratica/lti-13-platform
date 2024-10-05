using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public class Lti13NameRoleProvisioningServicesMessageBuilder(string messageType, Lti13PlatformBuilder platformBuilder) : Lti13NameRoleProvisioningServicesBuilder(platformBuilder)
    {
        public Lti13NameRoleProvisioningServicesMessageBuilder Extend<T, U>()
            where T : class
            where U : Populator<T>
        {
            base.ExtendMessage<T, U>(messageType);

            return this;
        }
    }
}
