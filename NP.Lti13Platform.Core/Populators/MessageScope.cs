using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Populators
{
    public record MessageScope(UserScope UserScope, Tool Tool, Deployment Deployment, Context? Context, ResourceLink? ResourceLink, string? MessageHint);

    public record UserScope(User User, User? ActualUser, bool IsAnonymous);
}
