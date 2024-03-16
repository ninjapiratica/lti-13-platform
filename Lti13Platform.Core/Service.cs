using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform
{
    public class Service(IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config)
    {
        private const string LTI_VERSION = "1.3.0";

        /// <summary>
        /// Gets the LTI specific claims that can be added to the base OpenID user claims.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="deploymentId"></param>
        /// <param name="message"></param>
        /// <param name="roles"></param>
        /// <param name="context"></param>
        /// <param name="platform"></param>
        /// <param name="roleScopeMentor"></param>
        /// <param name="launchPresentation"></param>
        /// <param name="custom"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetClaims(
            Lti13MessageType messageType,
            string deploymentId,
            ILti13Message message,
            Lti13RolesClaim roles,
            Lti13Context? context = null,
            Lti13PlatformClaim? platform = null,
            Lti13RoleScopeMentorClaim? roleScopeMentor = null,
            Lti13LaunchPresentationClaim? launchPresentation = null,
            Lti13CustomClaim? custom = null)
        {
            IEnumerable<KeyValuePair<string, object>> claims = new Dictionary<string, object>
            {
                { "https://purl.imsglobal.org/spec/lti/claim/message_type", messageType.ToString() },
                { "https://purl.imsglobal.org/spec/lti/claim/version", LTI_VERSION },
                { "https://purl.imsglobal.org/spec/lti/claim/deployment_id", deploymentId },
            };

            foreach (var ltiClaim in new ILti13Claim?[] { message, roles, context, platform, roleScopeMentor, launchPresentation, custom })
            {
                if (ltiClaim != null)
                {
                    claims = claims.Concat(ltiClaim.GetClaims());
                }
            }

            return claims.ToDictionary(c => c.Key, c => c.Value);
        }

        public Uri GetDeepLinkInitiationUrl(
            Lti13Client client,
            Lti13Deployment deployment,
            Lti13Context context,
            string userId,
            string? title = default,
            string? text = default,
            string? data = default,
            string? documentTarget = default,
            double? height = default,
            double? width = default,
            string? locale = default)
        {
            var builder = new UriBuilder(client.OidcInitiationUrl);

            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("iss", config.CurrentValue.Issuer);
            query.Add("login_hint", userId);
            query.Add("target_link_uri", client.DeepLinkUri);
            query.Add("client_id", client.Id);
            query.Add("lti_message_hint", $"{Lti13MessageType.LtiDeepLinkingRequest}!{CreateLaunchPresentationHint(documentTarget, height, width, locale)}!{context.Id},{Base64Encode(title)},{Base64Encode(text)},{Base64Encode(data)}");
            query.Add("lti_deployment_id", deployment.Id);
            builder.Query = query.ToString();

            return builder.Uri;
        }

        public Uri GetResourceLinkInitiationUrl(
            Lti13Client client,
            Lti13Deployment deployment,
            Lti13ResourceLink resourceLink,
            string userId,
            string? documentTarget = default,
            double? height = default,
            double? width = default,
            string? locale = default,
            string? returnUrl = default)
        {
            var builder = new UriBuilder(client.OidcInitiationUrl);

            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("iss", config.CurrentValue.Issuer);
            query.Add("login_hint", userId);
            query.Add("target_link_uri", GetResourceLinkUrl(resourceLink, client));
            query.Add("client_id", client.Id);
            query.Add("lti_message_hint", $"{Lti13MessageType.LtiResourceLinkRequest}!{CreateLaunchPresentationHint(documentTarget, height, width, locale, returnUrl)}!{resourceLink.Id}");
            query.Add("lti_deployment_id", deployment.Id);
            builder.Query = query.ToString();

            return builder.Uri;
        }

        public async Task<(ILti13Message?, Lti13Context?)> ParseResourceLinkRequestHintAsync(string hint)
        {
            var resourceLink = await dataService.GetResourceLinkAsync(hint);
            if (resourceLink == null)
            {
                return (null, null);
            }

            var context = await dataService.GetContextAsync(resourceLink.ContextId);
            if (context == null)
            {
                return (null, null);
            }

            var deployment = await dataService.GetDeploymentAsync(context.DeploymentId);
            if (deployment == null)
            {
                return (null, null);
            }

            var client = await dataService.GetClientAsync(deployment.ClientId);
            if (client == null)
            {
                return (null, null);
            }

            return (new LtiResourceLinkRequestMessage
            {
                Resource_Link_Id = resourceLink.Id,
                Target_Link_Uri = GetResourceLinkUrl(resourceLink, client),
                Resource_Link_Description = resourceLink.Description,
                Resource_Link_Title = resourceLink.Title
            },
            context);
        }

        private string GetResourceLinkUrl(Lti13ResourceLink resourceLink, Lti13Client client) => string.IsNullOrWhiteSpace(resourceLink.Url) ? client.LaunchUri : resourceLink.Url!;

        public async Task<(ILti13Message?, Lti13Context?)> ParseDeepLinkRequestHintAsync(string hint)
        {
            if (hint.Split(',', 4) is not [var contextId, var title, var text, var data])
            {
                return (null, null);
            }

            var context = await dataService.GetContextAsync(contextId);
            if (context == null)
            {
                return (null, null);
            }

            return (new LtiDeepLinkingRequestMessage
            {
                Accept_LineItem = config.CurrentValue.DeepLink.AcceptLineItem,
                Accept_Media_Types = config.CurrentValue.DeepLink.AcceptMediaTypes,
                Accept_Multiple = config.CurrentValue.DeepLink.AcceptMultiple,
                Accept_Presentation_Document_Targets = config.CurrentValue.DeepLink.AcceptPresentationDocumentTargets,
                Accept_Types = config.CurrentValue.DeepLink.AcceptTypes,
                Auto_Create = config.CurrentValue.DeepLink.AutoCreate,
                Data = Base64Decode(data),
                Deep_Link_Return_Url = config.CurrentValue.DeepLink.ReturnUrl,
                Text = Base64Decode(text),
                Title = Base64Decode(title)
            },
            context);
        }

        public Lti13LaunchPresentationClaim ParseLaunchPresentationHint(string hint)
        {
            var parts = hint.Split([','], 5);
            return new Lti13LaunchPresentationClaim
            {
                Document_Target = parts.Length > 0 ? parts[0] : null,
                Height = parts.Length > 1 && double.TryParse(parts[1], out var h) ? h : null,
                Width = parts.Length > 2 && double.TryParse(parts[2], out var w) ? w : null,
                Locale = parts.Length > 3 ? parts[3] : null,
                Return_Url = parts.Length > 4 ? Base64Decode(parts[4]) : null,
            };
        }

        public string CreateLaunchPresentationHint(string? documentTarget = default, double? height = default, double? width = default, string? locale = default, string? returnUrl = default) =>
            $"{documentTarget},{height},{width},{locale},{Base64Encode(returnUrl)}";

        private string? Base64Decode(string? input) => input == null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(input));
        private string? Base64Encode(string? input) => input == null ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }

    public enum Lti13MessageType
    {
        Unknown = 0,
        LtiResourceLinkRequest = 1,
        LtiDeepLinkingRequest = 2
    }

    public interface IDataService
    {
        Task<Lti13Client?> GetClientAsync(string clientId);
        Task<Lti13Deployment?> GetDeploymentAsync(string deploymentId);
        Task<Lti13Context?> GetContextAsync(string contextId);
        Task<Lti13ResourceLink?> GetResourceLinkAsync(string resourceLinkId);
        Task<IEnumerable<string>> GetRolesAsync(string userId, Lti13Client client, Lti13Context? context);
        Task<IEnumerable<string>> GetMentoredUserIdsAsync(string userId, Lti13Client client, Lti13Context? context);
        Task<Lti13OpenIdUser?> GetUserAsync(Lti13Client client, string userId);
        Task SaveContentItemsAsync(IEnumerable<ContentItem> contentItems);
        // TODO: Figure out custom
    }

    public class Lti13Client
    {
        public required string Id { get; set; }

        public required string OidcInitiationUrl { get; set; }

        public required string DeepLinkUri { get; set; }

        public required string LaunchUri { get; set; }

        public IEnumerable<string> RedirectUris => new[] { DeepLinkUri, LaunchUri }.Where(x => x != null).Select(x => x!);

        public Jwks? Jwks { get; set; }
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

            return Enumerable.Empty<SecurityKey>();
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

    public class Lti13Deployment
    {
        public required string Id { get; set; }

        public required string ClientId { get; set; }
    }

    public class Lti13Context : ILti13Claim
    {
        /// <summary>
        /// Max Length 255 characters
        /// Case sensitive
        /// </summary>
        public required string Id { get; set; }

        public required string DeploymentId { get; set; }

        public string? Label { get; set; }

        public string? Title { get; set; }

        public IEnumerable<string> Types { get; set; } = Enumerable.Empty<string>();

        public IDictionary<string, object> GetClaims()
        {
            var dict = new Dictionary<string, object>();

            if (Id != null) dict.Add("id", Id);
            if (Types.Any()) dict.Add("type", JsonSerializer.SerializeToElement(Types));
            if (Label != null) dict.Add("label", Label);
            if (Title != null) dict.Add("title", Title);

            if (dict.Count > 0)
            {
                return new Dictionary<string, object>
                {
                    { "https://purl.imsglobal.org/spec/lti/claim/context", dict }
                };
            }

            return new Dictionary<string, object>();
        }
    }

    public class Lti13ResourceLink
    {
        public required string Id { get; set; }

        public required string ContextId { get; set; }

        public string? Url { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }
    }

    public class Lti13PlatformClaim : ILti13Claim
    {
        public required string Guid { get; set; }
        public string? Contact_Email { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Product_Family_Code { get; set; }
        public string? Version { get; set; }

        public IDictionary<string, object> GetClaims()
        {
            if (Guid != null)
            {
                var dict = new Dictionary<string, object>
                {
                    { "guid", Guid }
                };

                if (Contact_Email != null) dict.Add("contact_email", Contact_Email);
                if (Description != null) dict.Add("description", Description);
                if (Name != null) dict.Add("name", Name);
                if (Url != null) dict.Add("url", Url);
                if (Product_Family_Code != null) dict.Add("product_family_code", Product_Family_Code);
                if (Version != null) dict.Add("version", Version);

                return new Dictionary<string, object>
                {
                    { "https://purl.imsglobal.org/spec/lti/claim/tool_platform", dict }
                };
            }

            return new Dictionary<string, object>();
        }
    }

    public class Lti13RolesClaim : ILti13Claim
    {
        public IEnumerable<string> Roles { get; set; } = [];

        public IDictionary<string, object> GetClaims()
        {
            return new Dictionary<string, object> { { "https://purl.imsglobal.org/spec/lti/claim/roles", JsonSerializer.SerializeToElement(Roles) } };
        }
    }

    public class Lti13RoleScopeMentorClaim : ILti13Claim
    {
        public IEnumerable<string> UserIds { get; set; } = [];

        public IDictionary<string, object> GetClaims()
        {
            var userIds = UserIds.ToList();
            if (userIds.Count > 0)
            {
                return new Dictionary<string, object> { { "https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor", JsonSerializer.SerializeToElement(UserIds) } };
            }

            return new Dictionary<string, object>();
        }
    }

    public class Lti13LaunchPresentationClaim : ILti13Claim
    {
        /// <summary>
        /// <see cref="Lti13PresentationTargetDocuments"/> has the list of possible values.
        /// </summary>
        public string? Document_Target { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }
        public string? Return_Url { get; set; }
        public string? Locale { get; set; }

        public IDictionary<string, object> GetClaims()
        {
            var dict = new Dictionary<string, object>();

            if (Document_Target != null) dict.Add("document_target", Document_Target);
            if (Height != null) dict.Add("height", Height.GetValueOrDefault());
            if (Width != null) dict.Add("width", Width.GetValueOrDefault());
            if (Return_Url != null) dict.Add("return_url", Return_Url);
            if (Locale != null) dict.Add("locale", Locale);

            if (dict.Count > 0)
            {
                return new Dictionary<string, object> { { "https://purl.imsglobal.org/spec/lti/claim/launch_presentation", dict } };
            }

            return new Dictionary<string, object>();
        }
    }

    public class Lti13CustomClaim : ILti13Claim
    {
        public Dictionary<string, string> CustomClaims { get; set; } = [];

        public IDictionary<string, object> GetClaims()
        {
            if (CustomClaims.Count != 0)
            {
                return new Dictionary<string, object> { { "http://imsglobal.org/custom", CustomClaims } };
            }

            return new Dictionary<string, object>();
        }
    }

    public static class Lti13ContextTypes
    {
        public static readonly string CourseTemplate = "http://purl.imsglobal.org/vocab/lis/v2/course#CourseTemplate";
        public static readonly string CourseOffering = "http://purl.imsglobal.org/vocab/lis/v2/course#CourseOffering";
        public static readonly string CourseSection = "http://purl.imsglobal.org/vocab/lis/v2/course#CourseSection";
        public static readonly string Group = "http://purl.imsglobal.org/vocab/lis/v2/course#Group";
    }

    public static class Lti13SystemRoles
    {
        // Core Roles
        public static readonly string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/system/person#Administrator";
        public static readonly string None = "http://purl.imsglobal.org/vocab/lis/v2/system/person#None";

        // Non-Core Roles
        public static readonly string AccountAdmin = "http://purl.imsglobal.org/vocab/lis/v2/system/person#AccountAdmin";
        public static readonly string Creator = "http://purl.imsglobal.org/vocab/lis/v2/system/person#Creator";
        public static readonly string SysAdmin = "http://purl.imsglobal.org/vocab/lis/v2/system/person#SysAdmin";
        public static readonly string SysSupport = "http://purl.imsglobal.org/vocab/lis/v2/system/person#SysSupport";
        public static readonly string User = "http://purl.imsglobal.org/vocab/lis/v2/system/person#User";

        // LTI Launch Only
        public static readonly string TestUser = "http://purl.imsglobal.org/vocab/lti/system/person#TestUser";
    }

    public static class Lti13InstitutionRoles
    {
        // Core Roles
        public static readonly string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Administrator";
        public static readonly string Faculty = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Faculty";
        public static readonly string Guest = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Guest";
        public static readonly string None = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#None";
        public static readonly string Other = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Other";
        public static readonly string Staff = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Staff";
        public static readonly string Student = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Student";

        // Non-Core Roles
        public static readonly string Alumni = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Alumni";
        public static readonly string Instructor = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Instructor";
        public static readonly string Learner = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Learner";
        public static readonly string Member = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Member";
        public static readonly string Mentor = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Mentor";
        public static readonly string Observer = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Observer";
        public static readonly string ProspectiveStudent = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#ProspectiveStudent";
    }

    public static class Lti13ContextRoles
    {
        // Core Roles
        public static readonly string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/membership#Administrator";
        public static readonly string ContentDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership#ContentDeveloper";
        public static readonly string Instructor = "http://purl.imsglobal.org/vocab/lis/v2/membership#Instructor";
        public static readonly string Learner = "http://purl.imsglobal.org/vocab/lis/v2/membership#Learner";
        public static readonly string Mentor = "http://purl.imsglobal.org/vocab/lis/v2/membership#Mentor";

        // Non-Core Roles
        public static readonly string Manager = "http://purl.imsglobal.org/vocab/lis/v2/membership#Manager";
        public static readonly string Member = "http://purl.imsglobal.org/vocab/lis/v2/membership#Member";
        public static readonly string Officer = "http://purl.imsglobal.org/vocab/lis/v2/membership#Officer";

        // Sub Roles exist (not currently implemented)
        // https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles
    }

    /// <summary>
    /// Used for DeepLinking accept_types
    /// </summary>
    public static class Lti13DeepLinkingTypes
    {
        public static readonly string Link = "link";
        public static readonly string File = "file";
        public static readonly string Html = "html";
        public static readonly string LtiResourceLink = "ltiResourceLink";
        public static readonly string Image = "image";
    }

    public static class Lti13PresentationTargetDocuments
    {
        public static readonly string Embed = "embed";
        public static readonly string Window = "window";
        public static readonly string Iframe = "iframe";
    }
}
