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
public interface ILti13DataService
    : ILti13CoreDataService,
    ILti13ResourceLinkMessageDataService,
    ILti13DeepLinkingRequestDataService,
    ILti13DeepLinkingResponseDataService,
    ILti13NameRoleProvisioningDataService,
    ILti13AssignmentGradeDataService
{ }