using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.NameRoleProvisioningServices;

namespace NP.Lti13Platform
{
    public interface IDataService : ICoreDataService, IDeepLinkingDataService, INameRoleProvisioningServicesDataService, IAssignmentGradeDataService { }
}