using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking.Services;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;

namespace NP.Lti13Platform
{
    public interface IDataService : ICoreDataService, IDeepLinkingDataService, INameRoleProvisioningDataService, IAssignmentGradeDataService { }
}