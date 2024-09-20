using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Extensions;
using NP.Lti13Platform.Models;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform
{
    public static class AuthenticationHandler
    {
        private const string OPENID = "openid";
        private const string ID_TOKEN = "id_token";
        private const string FORM_POST = "form_post";
        private const string NONE = "none";
        private const string INVALID_SCOPE = "invalid_scope";
        private const string INVALID_REQUEST = "invalid_request";
        private const string INVALID_CLIENT = "invalid_client";
        private const string INVALID_GRANT = "invalid_grant";
        private const string UNAUTHORIZED_CLIENT = "unauthorized_client";
        private const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";
        private const string LTI_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#lti_message_hint-login-parameter";
        private const string SCOPE_REQUIRED = "scope must be 'openid'.";
        private const string RESPONSE_TYPE_REQUIRED = "response_type must be 'id_token'.";
        private const string RESPONSE_MODE_REQUIRED = "response_mode must be 'form_post'.";
        private const string PROMPT_REQUIRED = "prompt must be 'none'.";
        private const string NONCE_REQUIRED = "nonce is required.";
        private const string CLIENT_ID_REQUIRED = "client_id is required.";
        private const string UNKNOWN_CLIENT_ID = "client_id is unknown";
        private const string UNKNOWN_REDIRECT_URI = "redirect_uri is unknown";
        private const string LTI_MESSAGE_HINT_INVALID = "lti_message_hint is invalid";
        private const string LOGIN_HINT_REQUIRED = "login_hint is required";
        private const string USER_CLIENT_MISMATCH = "client is not authorized for user";
        private const string DEPLOYMENT_CLIENT_MISMATCH = "deployment is not for client";
        private const string UNKNOWN_MESSAGE_PARAMETERS = "unknown message parameters";

        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };
        private static readonly JsonSerializerOptions LTI_MESSAGE_JSON_SERIALIZER_OPTIONS = new() { TypeInfoResolver = new LtiMessageTypeResolver(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } };

        internal static async Task<IResult> HandleAsync(
            IServiceProvider serviceProvider,
            IDataService dataService,
            AuthenticationRequest request)
        {
            /* https://datatracker.ietf.org/doc/html/rfc6749#section-5.2 */
            /* https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request */

            if (request.Scope != OPENID)
            {
                return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            if (request.Response_Type != ID_TOKEN)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = RESPONSE_TYPE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            if (request.Response_Mode != FORM_POST)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = RESPONSE_MODE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            if (request.Prompt != NONE)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = PROMPT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            if (string.IsNullOrWhiteSpace(request.Nonce))
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = NONCE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            if (string.IsNullOrWhiteSpace(request.Login_Hint))
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LOGIN_HINT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            if (string.IsNullOrWhiteSpace(request.Client_Id))
            {
                return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            var tool = await dataService.GetToolByClientIdAsync(request.Client_Id);

            if (tool == null)
            {
                return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = UNKNOWN_CLIENT_ID, Error_Uri = AUTH_SPEC_URI });
            }

            if (!tool.RedirectUrls.Contains(request.Redirect_Uri))
            {
                return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = UNKNOWN_REDIRECT_URI, Error_Uri = AUTH_SPEC_URI });
            }

            var userId = request.Login_Hint;
            var user = await dataService.GetUserAsync(userId);

            if (user == null)
            {
                return Results.BadRequest(new { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH });
            }

            if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint) ||
                request.Lti_Message_Hint.Split('|', 5, StringSplitOptions.RemoveEmptyEntries) is not [var messageTypeString, var deploymentId, var contextId, var launchPresentationString, var messageString])
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_INVALID, Error_Uri = LTI_SPEC_URI });
            }

            var deployment = await dataService.GetDeploymentAsync(deploymentId);

            if (deployment?.ToolId != tool.ClientId)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_CLIENT_MISMATCH, Error_Uri = AUTH_SPEC_URI });
            }

            var messageHandler = serviceProvider.GetKeyedService<IMessageHandler>(messageTypeString) ?? throw new NotImplementedException($"LTI Message Type {messageTypeString} does not have a registered message handler.");
            var context = !string.IsNullOrWhiteSpace(contextId) ? await dataService.GetContextAsync(contextId) : null;

            var launchPresentation = string.IsNullOrWhiteSpace(launchPresentationString) ? null : JsonSerializer.Deserialize<LaunchPresentation>(Convert.FromBase64String(launchPresentationString));

            var ltiMessage = await messageHandler.HandleAsync(tool, deployment, user, context, launchPresentation, messageString, request);

            if (ltiMessage == null)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = UNKNOWN_MESSAGE_PARAMETERS, Error_Uri = AUTH_SPEC_URI });
            }

            var privateKey = await dataService.GetPrivateKeyAsync();

            var token = new JsonWebTokenHandler().CreateToken(
                JsonSerializer.Serialize(ltiMessage, LTI_MESSAGE_JSON_SERIALIZER_OPTIONS),
                new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256) { CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY });

            return Results.Content(@$"<!DOCTYPE html>
            <html>
            <body>
            <form method=""post"" action=""{request.Redirect_Uri}"">
            <input type=""hidden"" name=""id_token"" value=""{token}""/>
            {(!string.IsNullOrWhiteSpace(request.State) ? @$"<input type=""hidden"" name=""state"" value=""{request.State}"" />" : null)}
            </form>
            <script type=""text/javascript"">
            document.getElementsByTagName('form')[0].submit();
            </script>
            </body>
            </html>", MediaTypeNames.Text.Html);
        }
    }

    public class LtiMessage
    {
        [JsonPropertyName("iss")]
        public required string Issuer { get; set; }

        [JsonPropertyName("aud")]
        public required string Audience { get; set; }

        [JsonPropertyName("exp")]
        public long ExpirationDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public required DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("iat")]
        public long IssuedDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public required DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("nonce")]
        public required string Nonce { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
        public required string MessageType { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
        public string LtiVersion { get; } = "1.3.0";

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
        public required string DeploymentId { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/roles")]
        public required IEnumerable<string> Roles { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor")]
        public IEnumerable<string>? RoleScopeMentor { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/context")]
        public MessageContext? Context { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/tool_platform")]
        public Platform? Platform { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/launch_presentation")]
        public LaunchPresentation? LaunchPresentation { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-ags/claim/endpoint")]
        public LineItemServiceEndpoints? ServiceEndpoints { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-nrps/claim/namesroleservice")]
        public NamesRoleService? NamesRoleService { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
        public IDictionary<string, string>? Custom { get; set; }

        [JsonPropertyName("sub")]
        public string? Subject { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("middle_name")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("preferred_username")]
        public string? PreferredUsername { get; set; }

        [JsonPropertyName("profile")]
        public string? Profile { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool? EmailVerified { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("birthdate")]
        public DateOnly? Birthdate { get; set; }

        [JsonPropertyName("zoneinfo")]
        public string? TimeZone { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("phone_number_verified")]
        public bool? PhoneNumberVerified { get; set; }

        [JsonPropertyName("address")]
        public AddressClaim? Address { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAtUnix => !UpdatedAt.HasValue ? null : new DateTimeOffset(UpdatedAt.GetValueOrDefault()).ToUnixTimeSeconds();

        public void SetUser(User user, UserPermissions permissions)
        {
            if (user == null)
            {
                return;
            }

            Subject = user.Id;

            Address = user.Address == null || !permissions.Address ? null : new AddressClaim
            {
                Country = permissions.AddressCountry ? user.Address.Country : null,
                Formatted = permissions.AddressFormatted ? user.Address.Formatted : null,
                Locality = permissions.AddressLocality ? user.Address.Locality : null,
                PostalCode = permissions.AddressPostalCode ? user.Address.PostalCode : null,
                Region = permissions.AddressRegion ? user.Address.Region : null,
                StreetAddress = permissions.AddressStreetAddress ? user.Address.StreetAddress : null
            };

            Birthdate = permissions.Birthdate ? user.Birthdate : null;
            Email = permissions.Email ? user.Email : null;
            EmailVerified = permissions.EmailVerified ? user.EmailVerified : null;
            FamilyName = permissions.FamilyName ? user.FamilyName : null;
            Gender = permissions.Gender ? user.Gender : null;
            GivenName = permissions.GivenName ? user.GivenName : null;
            Locale = permissions.Locale ? user.Locale : null;
            MiddleName = permissions.MiddleName ? user.MiddleName : null;
            Name = permissions.Name ? user.Name : null;
            Nickname = permissions.Nickname ? user.Nickname : null;
            PhoneNumber = permissions.PhoneNumber ? user.PhoneNumber : null;
            PhoneNumberVerified = permissions.PhoneNumberVerified ? user.PhoneNumberVerified : null;
            Picture = permissions.Picture ? user.Picture : null;
            PreferredUsername = permissions.PreferredUsername ? user.PreferredUsername : null;
            Profile = permissions.Profile ? user.Profile : null;
            UpdatedAt = permissions.UpdatedAt ? user.UpdatedAt : null;
            Website = permissions.Website ? user.Website : null;
            TimeZone = permissions.TimeZone ? user.TimeZone : null;
        }

        public void SetCustom(IDictionary<string, string>? custom, Context? context, User? user, LtiResourceLinkContentItem? resourceLink, Attempt? attempt, LineItem? lineItem, Grade? grade, CustomPermissions permissions)
        {
            Custom = custom?.ToDictionary();

            if (Custom == null)
            {
                return;
            }

            foreach (var kvp in Custom.Where(kvp => kvp.Value.StartsWith('$')))
            {
                // TODO: missing values
                // TODO: ActualUser
                Custom[kvp.Key] = kvp.Key switch
                {
                    Lti13UserVariables.Id when permissions.UserId => user?.Id,
                    Lti13UserVariables.Image when permissions.UserImage => user?.ImageUrl,
                    Lti13UserVariables.Username when permissions.UserUsername => user?.Username,
                    Lti13UserVariables.Org when permissions.UserOrg => user != null ? string.Join(',', user.Orgs) : string.Empty,
                    Lti13UserVariables.ScopeMentor when permissions.UserScopeMentor => RoleScopeMentor != null ? string.Join(',', RoleScopeMentor) : string.Empty,
                    Lti13UserVariables.GradeLevelsOneRoster when permissions.UserGradeLevelsOneRoster => user != null ? string.Join(',', user.OneRosterGrades) : string.Empty,

                    Lti13ContextVariables.Id when permissions.ContextId => context?.Id,
                    Lti13ContextVariables.Org when permissions.ContextOrg => context != null ? string.Join(',', context.Orgs) : string.Empty,
                    Lti13ContextVariables.Type when permissions.ContextType => context != null ? string.Join(',', context.Types) : string.Empty,
                    Lti13ContextVariables.Label when permissions.ContextLabel => context?.Label,
                    Lti13ContextVariables.Title when permissions.ContextTitle => context?.Title,
                    Lti13ContextVariables.SourcedId when permissions.ContextSourcedId => context?.SourcedId,
                    Lti13ContextVariables.IdHistory when permissions.ContextIdHistory => context != null ? string.Join(',', context.ClonedIdHistory) : string.Empty,
                    Lti13ContextVariables.GradeLevelsOneRoster when permissions.ContextGradeLevelsOneRoster => context != null ? string.Join(',', context.OneRosterGrades) : string.Empty,

                    Lti13ResourceLinkVariables.Id when permissions.ResourceLinkId => resourceLink?.Id,
                    Lti13ResourceLinkVariables.Title when permissions.ResourceLinkTitle => resourceLink?.Title,
                    Lti13ResourceLinkVariables.Description when permissions.ResourceLinkDescription => resourceLink?.Text,
                    Lti13ResourceLinkVariables.AvailableStartDateTime when permissions.ResourceLinkAvailableStartDateTime => resourceLink?.Available?.StartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableUserStartDateTime when permissions.ResourceLinkAvailableUserStartDateTime => attempt?.AvailableStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableEndDateTime when permissions.ResourceLinkAvailableEndDateTime => resourceLink?.Available?.EndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableUserEndDateTime when permissions.ResourceLinkAvailableUserEndDateTime => attempt?.AvailableEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionStartDateTime when permissions.ResourceLinkSubmissionStartDateTime => resourceLink?.Submission?.StartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionUserStartDateTime when permissions.ResourceLinkSubmissionUserStartDateTime => attempt?.SubmisstionStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionEndDateTime when permissions.ResourceLinkSubmissionEndDateTime => resourceLink?.Submission?.EndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionUserEndDateTime when permissions.ResourceLinkSubmissionUserEndDateTime => attempt?.SubmissionEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.LineItemReleaseDateTime when permissions.ResourceLinkLineItemReleaseDateTime => lineItem?.GradesReleasedDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.LineItemUserReleaseDateTime when permissions.ResourceLinkLineItemUserReleaseDateTime => grade?.ReleaseDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.IdHistory when permissions.ResourceLinkIdHistory => resourceLink != null ? string.Join(',', resourceLink.ClonedIdHistory) : string.Empty,

                    Lti13ToolPlatformVariables.ProductFamilyCode when permissions.ToolPlatformProductFamilyCode => Platform?.ProductFamilyCode,
                    Lti13ToolPlatformVariables.Version when permissions.ToolPlatformProductVersion => Platform?.Version,
                    Lti13ToolPlatformVariables.InstanceGuid when permissions.ToolPlatformProductInstanceGuid => Platform?.Guid,
                    Lti13ToolPlatformVariables.InstanceName when permissions.ToolPlatformProductInstanceName => Platform?.Name,
                    Lti13ToolPlatformVariables.InstanceDescription when permissions.ToolPlatformProductInstanceDescription => Platform?.Description,
                    Lti13ToolPlatformVariables.InstanceUrl when permissions.ToolPlatformProductInstanceUrl => Platform?.Url,
                    Lti13ToolPlatformVariables.InstanceContactEmail when permissions.ToolPlatformProductInstanceContactEmail => Platform?.ContactEmail,
                    _ => kvp.Value
                } ?? string.Empty;
            }
        }

        public void SetLineItemServiceEndpoints(LineItemServiceEndpoints? serviceEndpoints, ServicePermissions permissions)
        {
            ServiceEndpoints = permissions.LineItemScopes.Any() ? serviceEndpoints : null;
        }

        public void SetNamesRoleService(NamesRoleService? namesRoleService, ServicePermissions permissions)
        {
            NamesRoleService = permissions.AllowNameRoleProvisioningService ? namesRoleService : null;
        }

        public void SetLaunchPresentation(LaunchPresentation? launchPresentation)
        {
            LaunchPresentation = launchPresentation;
        }

        public void SetPlatform(Platform? platform)
        {
            Platform = platform;
        }

        public void SetContext(Context? context)
        {
            Context = context == null ? null : new MessageContext
            {
                Id = context.Id,
                Label = context.Label,
                Title = context.Title,
                Types = context.Types
            };
        }

        public void SetRoleScopeMentor(IEnumerable<string>? roleScopeMentor)
        {
            RoleScopeMentor = roleScopeMentor?.ToList();
        }
    }

    internal class LtiResourceLinkMessage : LtiMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/target_link_uri")]
        public required string TargetLinkUri { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/resource_link")]
        public required ResourceLink ResourceLink { get; set; }

        //new Claim("https://purl.imsglobal.org/spec/lti/claim/lis", "") // https://www.imsglobal.org/spec/lti/v1p3/#learning-information-services-lis-claim
    }

    internal class LtiDeepLinkingMessage : LtiMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings")]
        public required DeepLinkSettings DeepLinkSettings { get; set; }
    }

    internal class ResourceLink
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    public class DeepLinkSettings
    {
        [JsonPropertyName("deep_link_return_url")]
        public required string DeepLinkReturnUrl { get; set; }

        [JsonPropertyName("accept_types")]
        public required IEnumerable<string> AcceptTypes { get; set; }

        [JsonPropertyName("accept_presentation_document_targets")]
        public required IEnumerable<string> AcceptPresentationDocumentTargets { get; set; }

        [JsonPropertyName("accept_media_types")]
        public string? AcceptMediaTypesSerialized => AcceptMediaTypes == null ? null : string.Join(",", AcceptMediaTypes);

        [JsonIgnore]
        public IEnumerable<string>? AcceptMediaTypes { get; set; }

        [JsonPropertyName("accept_multiple")]
        public bool? AcceptMultiple { get; set; }

        [JsonPropertyName("accept_lineitem")]
        public bool? AcceptLineItem { get; set; }

        [JsonPropertyName("auto_create")]
        public bool? AutoCreate { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }

    public class AddressClaim
    {
        [JsonPropertyName("formatted")]
        public string? Formatted { get; set; }

        [JsonPropertyName("street_address")]
        public string? StreetAddress { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }

    public class MessageContext
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("type")]
        public IEnumerable<string> Types { get; set; } = [];
    }

    public class Platform
    {
        [JsonPropertyName("guid")]
        public required string Guid { get; set; }

        [JsonPropertyName("contact_email")]
        public string? ContactEmail { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("product_family_code")]
        public string? ProductFamilyCode { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    public class LaunchPresentation
    {
        /// <summary>
        /// <see cref="Lti13PresentationTargetDocuments"/> has the list of possible values.
        /// </summary>
        [JsonPropertyName("document_target")]
        public string? DocumentTarget { get; set; }

        [JsonPropertyName("height")]
        public double? Height { get; set; }

        [JsonPropertyName("width")]
        public double? Width { get; set; }

        [JsonPropertyName("return_url")]
        public string? ReturnUrl { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }
    }

    public class LineItemServiceEndpoints
    {
        [JsonPropertyName("scope")]
        public required IEnumerable<string> Scopes { get; set; }

        [JsonPropertyName("lineitems")]
        public string? LineItemsUrl { get; set; }

        [JsonPropertyName("lineitem")]
        public string? LineItemUrl { get; set; }
    }

    public class NamesRoleService
    {
        [JsonPropertyName("context_memberships_url")]
        public required string ContextMembershipUrl { get; set; }

        [JsonPropertyName("service_versions")]
        public required IEnumerable<string> ServiceVersions { get; set; }
    }

    public interface IMessageHandler
    {
        Task<LtiMessage?> HandleAsync(Tool tool, Deployment deployment, User user, Context? context, LaunchPresentation? launchPresentation, string message, AuthenticationRequest request);
    }

    internal class DeepLinkingMessageHandler(
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<Lti13PlatformConfig> config,
        IDataService dataService,
        IPlatformService platformService,
        Service service) : IMessageHandler
    {
        public async Task<LtiMessage?> HandleAsync(Tool tool, Deployment deployment, User user, Context? context, LaunchPresentation? launchPresentation, string message, AuthenticationRequest request)
        {
            var deepLinkSettings = string.IsNullOrWhiteSpace(message) ? null : JsonSerializer.Deserialize<DeepLinkSettings>(Convert.FromBase64String(message));

            var roles = context != null ? await dataService.GetRolesAsync(user.Id, context) : [];

            var ltiMessage = new LtiDeepLinkingMessage
            {
                Audience = tool.ClientId,
                DeploymentId = deployment.Id,
                IssuedDate = DateTime.UtcNow,
                Issuer = config.CurrentValue.Issuer,
                MessageType = Lti13MessageType.LtiDeepLinkingRequest,
                Nonce = request.Nonce!,
                Roles = roles,
                ExpirationDate = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds),
                DeepLinkSettings = new DeepLinkSettings
                {
                    AcceptPresentationDocumentTargets = new[] { deepLinkSettings?.AcceptPresentationDocumentTargets, config.CurrentValue.DeepLink.AcceptPresentationDocumentTargets }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                    AcceptTypes = new[] { deepLinkSettings?.AcceptTypes, config.CurrentValue.DeepLink.AcceptTypes }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                    DeepLinkReturnUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, RouteNames.DEEP_LINKING_RESPONSE, new { contextId = context?.Id }) ?? string.Empty,
                    AcceptLineItem = new[] { deepLinkSettings?.AcceptLineItem, config.CurrentValue.DeepLink.AcceptLineItem }.FirstOrDefault(x => x.HasValue),
                    AcceptMediaTypes = new[] { deepLinkSettings?.AcceptMediaTypes, config.CurrentValue.DeepLink.AcceptMediaTypes }.FirstOrDefault(x => x != null && x.Any()),
                    AcceptMultiple = new[] { deepLinkSettings?.AcceptMultiple, config.CurrentValue.DeepLink.AcceptMultiple }.FirstOrDefault(x => x.HasValue),
                    AutoCreate = new[] { deepLinkSettings?.AutoCreate, config.CurrentValue.DeepLink.AutoCreate }.FirstOrDefault(x => x.HasValue),
                    Data = deepLinkSettings?.Data,
                    Text = deepLinkSettings?.Text,
                    Title = deepLinkSettings?.Title,
                }
            };

            ltiMessage.SetContext(context);
            ltiMessage.SetLaunchPresentation(launchPresentation);
            ltiMessage.SetPlatform(await platformService.GetPlatformAsync(tool.ClientId));
            ltiMessage.SetRoleScopeMentor(roles.Contains(Lti13ContextRoles.Mentor) && context != null ? await dataService.GetMentoredUserIdsAsync(user.Id, context) : null);
            ltiMessage.SetLineItemServiceEndpoints(service.GetServiceEndpoints(context?.Id, null, tool.ServicePermissions), tool.ServicePermissions);
            ltiMessage.SetUser(user, tool.UserPermissions);
            ltiMessage.SetNamesRoleService(service.GetNamesRoleService(context?.Id, tool.ServicePermissions), tool.ServicePermissions);
            ltiMessage.SetCustom(tool.Custom.Merge(deployment.Custom), context, user, null, null, null, null, tool.CustomPermissions);

            return ltiMessage;
        }
    }

    internal class ResourceLinkMessageHandler(
        IOptionsMonitor<Lti13PlatformConfig> config,
        IDataService dataService,
        IPlatformService platformService,
        Service service) : IMessageHandler
    {
        public async Task<LtiMessage?> HandleAsync(Tool tool, Deployment deployment, User user, Context? context, LaunchPresentation? launchPresentation, string message, AuthenticationRequest request)
        {
            var resourceLinkId = message;
            var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(resourceLinkId);

            if (resourceLink == null || resourceLink.DeploymentId != deployment.Id || resourceLink.ContextId != null && context != null && context.Id != resourceLink.ContextId)
            {
                return null;
            }

            Attempt? attempt = null;
            LineItem? lineItem = null;
            Grade? grade = null;

            if (context != null)
            {
                attempt = await dataService.GetAttemptAsync(context.Id, resourceLinkId, user.Id);

                var lineItems = await dataService.GetLineItemsAsync(context.Id, 0, 1, null, resourceLinkId, null);

                if (lineItems.TotalItems == 1)
                {
                    lineItem = lineItems.Items.FirstOrDefault();

                    grade = (await dataService.GetGradesAsync(context.Id, lineItem!.Id, 0, 1, user.Id))?.Items.FirstOrDefault();
                }
            }

            var roles = context != null ? await dataService.GetRolesAsync(user.Id, context) : [];

            var ltiMessage = new LtiResourceLinkMessage
            {
                Audience = tool.ClientId,
                DeploymentId = deployment.Id,
                IssuedDate = DateTime.UtcNow,
                Issuer = config.CurrentValue.Issuer,
                MessageType = Lti13MessageType.LtiResourceLinkRequest,
                Nonce = request.Nonce!,
                Roles = roles,
                ExpirationDate = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds),
                ResourceLink = new ResourceLink
                {
                    Id = resourceLink.Id,
                    Description = resourceLink.Text,
                    Title = resourceLink.Title
                },
                TargetLinkUri = string.IsNullOrWhiteSpace(resourceLink.Url) ? tool.LaunchUrl : resourceLink.Url,
            };

            ltiMessage.SetContext(context);
            ltiMessage.SetLaunchPresentation(launchPresentation);
            ltiMessage.SetPlatform(await platformService.GetPlatformAsync(tool.ClientId));
            ltiMessage.SetRoleScopeMentor(roles.Contains(Lti13ContextRoles.Mentor) && context != null ? await dataService.GetMentoredUserIdsAsync(user.Id, context) : null);
            ltiMessage.SetLineItemServiceEndpoints(service.GetServiceEndpoints(context?.Id, lineItem?.Id, tool.ServicePermissions), tool.ServicePermissions);
            ltiMessage.SetUser(user, tool.UserPermissions);
            ltiMessage.SetNamesRoleService(service.GetNamesRoleService(context?.Id, tool.ServicePermissions), tool.ServicePermissions);
            ltiMessage.SetCustom(tool.Custom.Merge(deployment.Custom).Merge(resourceLink.Custom), context, user, resourceLink, attempt, lineItem, grade, tool.CustomPermissions);

            return ltiMessage;
        }
    }
}

