using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.Services;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;

namespace NP.Lti13Platform
{
    public interface ILti13DataService : ILti13CoreDataService, ILti13DeepLinkingDataService, ILti13NameRoleProvisioningDataService, ILti13AssignmentGradeDataService { }
}