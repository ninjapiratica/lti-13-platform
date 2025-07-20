using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.Services;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;

namespace NP.Lti13Platform;

/// <summary>
/// Provides a contract for services that handle LTI 1.3 data operations, including core data management, deep linking, name and role provisioning, and assignment grading functionalities.
/// </summary>
/// <remarks>
/// This interface combines multiple LTI 1.3 related services, allowing implementations to manage various aspects of LTI 1.3 interactions in a unified manner. It extends the capabilities of <see cref="ILti13CoreDataService"/>, <see cref="ILti13DeepLinkingDataService"/>, <see cref="ILti13NameRoleProvisioningDataService"/>, and <see cref="ILti13AssignmentGradeDataService"/>.
/// </remarks>
public interface ILti13DataService : ILti13CoreDataService, ILti13DeepLinkingDataService, ILti13NameRoleProvisioningDataService, ILti13AssignmentGradeDataService { }