using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform
{
    public class Service(IOptionsMonitor<Lti13PlatformConfig> config, LinkGenerator linkGenerator, HttpContextAccessor httpContextAccessor)
    {
        public Uri GetDeepLinkInitiationUrl(Tool tool, string deploymentId, string? contextId, string? userId = null, DeepLinkSettings? deepLinkSettings = null, LaunchPresentation? launchPresentation = null)
            => GetUrl(Lti13MessageType.LtiDeepLinkingRequest, tool, deploymentId, tool.DeepLinkUrl, contextId, userId, Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deepLinkSettings))), launchPresentation);

        public Uri GetResourceLinkInitiationUrl(Tool tool, LtiResourceLinkContentItem resourceLink, string? userId = null, LaunchPresentation? launchPresentation = null)
            => GetUrl(Lti13MessageType.LtiResourceLinkRequest, tool, resourceLink.DeploymentId, string.IsNullOrWhiteSpace(resourceLink.Url) ? tool.OidcInitiationUrl : resourceLink.Url, resourceLink.ContextId, userId, resourceLink.Id, launchPresentation);

        public Uri GetUrl(string messageType, Tool tool, string deploymentId, string targetLinkUri, string? contextId = null, string? userId = null, string? messageHint = null, LaunchPresentation? launchPresentation = null)
        {
            var launchPresentationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(launchPresentation)));

            var builder = new UriBuilder(tool.OidcInitiationUrl);

            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("iss", config.CurrentValue.Issuer);
            query.Add("login_hint", userId);
            query.Add("target_link_uri", targetLinkUri);
            query.Add("client_id", tool.ClientId.ToString());
            query.Add("lti_message_hint", $"{messageType}|{deploymentId}|{contextId}|{launchPresentationString}|{messageHint}");
            query.Add("lti_deployment_id", deploymentId);
            builder.Query = query.ToString();

            return builder.Uri;
        }

        public ServiceEndpoints? GetServiceEndpoints(string? contextId, string? lineItemId, ServicePermissions permissions)
        {
            if (!permissions.Scopes.Any() || string.IsNullOrWhiteSpace(contextId) || httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            return new ServiceEndpoints
            {
                Scopes = permissions.Scopes.ToList(),
                LineItemsUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_LINE_ITEMS, new { contextId = contextId }),
                LineItemUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_LINE_ITEM, new { contextId = contextId, lineItemId = lineItemId }),
            };
        }
    }

    public static class Lti13MessageType
    {
        public const string LtiResourceLinkRequest = "LtiResourceLinkRequest";
        public const string LtiDeepLinkingRequest = "LtiDeepLinkingRequest";
    }

    public interface IDataService
    {
        Task<Tool?> GetToolAsync(string clientId);
        Task<Deployment?> GetDeploymentAsync(string deploymentId);
        Task<Context?> GetContextAsync(string contextId);

        Task<IEnumerable<string>> GetRolesAsync(string userId, Context? context);
        Task<IEnumerable<string>> GetMentoredUserIdsAsync(string userId, Context? context);
        Task<User?> GetUserAsync(string userId);

        Task SaveContentItemsAsync(IEnumerable<ContentItem> contentItems);
        Task<T?> GetContentItemAsync<T>(string contentItemId) where T : ContentItem;

        Task<ServiceToken?> GetServiceTokenRequestAsync(string id);
        Task SaveServiceTokenRequestAsync(ServiceToken serviceToken);

        Task<IEnumerable<SecurityKey>> GetPublicKeysAsync();
        Task<SecurityKey> GetPrivateKeyAsync();

        Task<PartialList<LineItem>> GetLineItemsAsync(string contextId, int pageIndex, int limit, string? resourceId, string? resourceLinkId, string? tag);

        Task SaveLineItemAsync(LineItem lineItem);
        Task<LineItem?> GetLineItemAsync(string lineItemId);
        Task DeleteLineItemAsync(string lineItemId);

        Task<PartialList<Result>> GetLineItemResultsAsync(string contextId, string lineItemId, int pageIndex, int limit, string? userId);
        Task SaveLineItemResultAsync(Result result);

        // TODO: Figure out custom
    }

    public interface IPlatformService
    {
        Task<Platform?> GetPlatformAsync(string clientId);
    }

    public class PartialList<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
    }

    public class JwtPublicKey : Jwks
    {
        public required string PublicKey { get; set; }

        public override Task<IEnumerable<SecurityKey>> GetKeysAsync()
        {
            return Task.FromResult<IEnumerable<SecurityKey>>([new JsonWebKey(PublicKey)]);
        }
    }

    public class JwksUri : Jwks
    {
        private static readonly HttpClient httpClient = new();

        public required string Uri { get; set; }

        public override async Task<IEnumerable<SecurityKey>> GetKeysAsync()
        {
            var httpResponse = await httpClient.GetAsync(Uri);
            var result = await httpResponse.Content.ReadFromJsonAsync<JsonWebKeySet>();

            if (result != null)
            {
                return result.Keys;
            }

            return [];
        }
    }

    public abstract class Jwks
    {
        /// <summary>
        /// Create an instance of Jwks using the provided key or uri.
        /// </summary>
        /// <param name="keyOrUri">The public key or JWKS uri to use.</param>
        /// <returns>An instance of Jwks depending on the type of string provided.</returns>
        static Jwks Create(string keyOrUri) => Uri.IsWellFormedUriString(keyOrUri, UriKind.Absolute) ?
                new JwksUri { Uri = keyOrUri } :
                new JwtPublicKey { PublicKey = keyOrUri };

        public abstract Task<IEnumerable<SecurityKey>> GetKeysAsync();

        public static implicit operator Jwks(string keyOrUri) => Create(keyOrUri);
    }

    public static class Lti13ContextTypes
    {
        public const string CourseTemplate = "http://purl.imsglobal.org/vocab/lis/v2/course#CourseTemplate";
        public const string CourseOffering = "http://purl.imsglobal.org/vocab/lis/v2/course#CourseOffering";
        public const string CourseSection = "http://purl.imsglobal.org/vocab/lis/v2/course#CourseSection";
        public const string Group = "http://purl.imsglobal.org/vocab/lis/v2/course#Group";
    }

    public static class Lti13SystemRoles
    {
        // Core Roles
        public const string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/system/person#Administrator";
        public const string None = "http://purl.imsglobal.org/vocab/lis/v2/system/person#None";

        // Non-Core Roles
        public const string AccountAdmin = "http://purl.imsglobal.org/vocab/lis/v2/system/person#AccountAdmin";
        public const string Creator = "http://purl.imsglobal.org/vocab/lis/v2/system/person#Creator";
        public const string SysAdmin = "http://purl.imsglobal.org/vocab/lis/v2/system/person#SysAdmin";
        public const string SysSupport = "http://purl.imsglobal.org/vocab/lis/v2/system/person#SysSupport";
        public const string User = "http://purl.imsglobal.org/vocab/lis/v2/system/person#User";

        // LTI Launch Only
        public const string TestUser = "http://purl.imsglobal.org/vocab/lti/system/person#TestUser";
    }

    public static class Lti13InstitutionRoles
    {
        // Core Roles
        public const string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Administrator";
        public const string Faculty = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Faculty";
        public const string Guest = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Guest";
        public const string None = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#None";
        public const string Other = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Other";
        public const string Staff = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Staff";
        public const string Student = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Student";

        // Non-Core Roles
        public const string Alumni = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Alumni";
        public const string Instructor = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Instructor";
        public const string Learner = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Learner";
        public const string Member = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Member";
        public const string Mentor = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Mentor";
        public const string Observer = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Observer";
        public const string ProspectiveStudent = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#ProspectiveStudent";
    }

    public static class Lti13ContextRoles
    {
        // Core Roles
        public const string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/membership#Administrator";
        public const string ContentDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership#ContentDeveloper";
        public const string Instructor = "http://purl.imsglobal.org/vocab/lis/v2/membership#Instructor";
        public const string Learner = "http://purl.imsglobal.org/vocab/lis/v2/membership#Learner";
        public const string Mentor = "http://purl.imsglobal.org/vocab/lis/v2/membership#Mentor";

        // Non-Core Roles
        public const string Manager = "http://purl.imsglobal.org/vocab/lis/v2/membership#Manager";
        public const string Member = "http://purl.imsglobal.org/vocab/lis/v2/membership#Member";
        public const string Officer = "http://purl.imsglobal.org/vocab/lis/v2/membership#Officer";

        // Sub Roles exist (not currently implemented)
        // https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles
    }

    /// <summary>
    /// Used for DeepLinking accept_types
    /// </summary>
    public static class Lti13DeepLinkingTypes
    {
        public const string Link = "link";
        public const string File = "file";
        public const string Html = "html";
        public const string LtiResourceLink = "ltiResourceLink";
        public const string Image = "image";
    }

    public static class Lti13PresentationTargetDocuments
    {
        public const string Embed = "embed";
        public const string Window = "window";
        public const string Iframe = "iframe";
    }
}
