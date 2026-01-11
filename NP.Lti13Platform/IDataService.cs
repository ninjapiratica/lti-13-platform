using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.Services;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;

namespace NP.Lti13Platform;

/// <summary>
/// Defines a contract for services that provide data access and operations for LTI 1.3 workflows, including core, resource link, deep linking, names and roles provisioning, and assignment and grade services.
/// </summary>
/// <remarks>This interface aggregates multiple LTI 1.3 service interfaces to support comprehensive LTI 1.3 integration scenarios.
/// Implementations should ensure that all required LTI 1.3 data operations are available through this unified service.</remarks>
public interface IDataService
    : IRequiredDataService,
    IMessageHandlerDataService
{ }

/// <summary>
/// Defines the required set of services for handling LTI 1.3 data operations, including core, name and role provisioning, and assignment and grade services.
/// </summary>
/// <remarks>This interface aggregates the essential LTI 1.3 data service contracts that an implementation must provide to support full LTI 1.3 integration.
/// It is intended for use in scenarios where all standard LTI 1.3 data services are required together.</remarks>
public interface IRequiredDataService
    : ICoreDataService,
    IDeepLinkingResponseDataService,
    INameRoleProvisioningDataService,
    IAssignmentGradeDataService
{ }

/// <summary>
/// Defines a contract for data services that handle LTI 1.3 message processing, including resource link messages and deep linking requests and responses.
/// </summary>
/// <remarks>This interface aggregates multiple LTI 1.3 message data service interfaces, enabling implementations to support all required message types for LTI 1.3 integrations.
/// It is intended for use in scenarios where a unified data service is needed to manage different LTI 1.3 message flows.</remarks>
public interface IMessageHandlerDataService
    : ILtiResourceLinkMessageDataService,
    IDeepLinkingRequestDataService
{ }