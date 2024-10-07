using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    internal record MessageType(string Name, HashSet<Type> Interfaces);

    public class Lti13NameRoleProvisioningServicesBuilder(Lti13PlatformBuilder platformBuilder)
    {
        private static readonly Dictionary<string, MessageType> MessageTypes = [];
        internal static readonly Dictionary<MessageType, Type> LtiMessageTypes = [];

        public Lti13NameRoleProvisioningServicesBuilder ExtendMessage<T, U>(string messageType)
            where T : class
            where U : Populator<T>
        {
            var tType = typeof(T);
            List<Type> interfaceTypes = [tType, .. tType.GetInterfaces()];

            foreach (var interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsInterface)
                {
                    throw new Exception("T must be an interface");
                }

                if (interfaceType.GetMethods().Any(m => !m.IsSpecialName))
                {
                    throw new Exception("Interfaces may only have properties.");
                }
            }

            if (!MessageTypes.TryGetValue(messageType, out var mt))
            {
                AddMessage(messageType);
                mt = MessageTypes[messageType];
            }

            interfaceTypes.ForEach(t => mt.Interfaces.Add(t));
            platformBuilder.Services.AddKeyedTransient<Populator, U>(mt);

            return this;
        }

        public Lti13NameRoleProvisioningServicesMessageBuilder AddMessage(string messageType)
        {
            if (!MessageTypes.TryGetValue(messageType, out var mt))
            {
                mt = new MessageType(messageType, []);
                MessageTypes.Add(messageType, mt);
            }

            platformBuilder.Services.AddKeyedTransient(mt, (sp, obj) =>
            {
                return Activator.CreateInstance(LtiMessageTypes[mt])!;
            });

            return new Lti13NameRoleProvisioningServicesMessageBuilder(messageType, platformBuilder);
        }

        static internal void CreateTypes()
        {
            if (LtiMessageTypes.Count == 0)
            {
                foreach (var messageType in MessageTypes.Select(mt => mt.Value))
                {
                    var type = TypeGenerator.CreateType(messageType.Name, messageType.Interfaces, typeof(NameRoleProvisioningMessage));
                    NameRoleProvisioningMessageTypeResolver.AddDerivedType(type);
                    LtiMessageTypes.TryAdd(messageType, type);
                }
            }
        }
    }
}
