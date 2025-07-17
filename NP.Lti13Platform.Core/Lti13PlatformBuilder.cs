using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.Core
{
    internal record MessageType(string Name, HashSet<Type> Interfaces);

    /// <summary>
    /// A builder for configuring LTI 1.3 platform services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    public partial class Lti13PlatformBuilder(IServiceCollection services)
    {
        private static readonly HashSet<Type> GlobalInterfaces = [];
        private static readonly HashSet<Type> GlobalPopulators = [];
        private static readonly Dictionary<string, MessageType> MessageTypes = [];
        private static readonly Dictionary<string, Type> LtiMessageTypes = [];

        /// <summary>
        /// Extends an existing LTI 1.3 message type with additional interfaces and a populator.
        /// </summary>
        /// <typeparam name="T">The type of the LTI message to extend.</typeparam>
        /// <typeparam name="U">The type of the populator to use.</typeparam>
        /// <param name="messageType">The message type to extend. If null, the extension applies to all message types.</param>
        /// <returns>The <see cref="Lti13PlatformBuilder"/>.</returns>
        public Lti13PlatformBuilder ExtendLti13Message<T, U>(string? messageType = null)
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

            if (string.IsNullOrWhiteSpace(messageType))
            {
                var uType = typeof(U);
                GlobalPopulators.Add(uType);

                interfaceTypes.ForEach(t => GlobalInterfaces.Add(t));

                foreach (var mt in MessageTypes.Values)
                {
                    interfaceTypes.ForEach(t => mt.Interfaces.Add(t));

                    services.AddKeyedTransient<Populator, U>(mt.Name);
                };
            }
            else
            {
                if (!MessageTypes.TryGetValue(messageType, out var mt))
                {
                    MessageTypes.TryAdd(messageType, new MessageType(messageType, [.. GlobalInterfaces]));

                    foreach (var globalPopulator in GlobalPopulators)
                    {
                        services.AddKeyedTransient(typeof(Populator), messageType, globalPopulator);
                    }

                    services.AddKeyedTransient(messageType, (sp, obj) =>
                    {
                        return (LtiMessage)Activator.CreateInstance(LtiMessageTypes[messageType])!;
                    });

                    mt = MessageTypes[messageType];
                }

                interfaceTypes.ForEach(t => mt.Interfaces.Add(t));
                services.AddKeyedTransient<Populator, U>(messageType);
            }

            return this;
        }

        internal static void CreateTypes()
        {
            if (LtiMessageTypes.Count == 0)
            {
                foreach (var messageType in MessageTypes.Select(mt => mt.Value))
                {
                    var type = TypeGenerator.CreateType(messageType.Name, messageType.Interfaces, typeof(LtiMessage));
                    LtiMessageTypeResolver.AddDerivedType(type);
                    LtiMessageTypes.TryAdd(messageType.Name, type);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where LTI 1.3 services are configured.
        /// </summary>
        public IServiceCollection Services => services;
    }
}
