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
        private const string INVALID_CLIENT_ID = "client_id is invalid";
        private const string UNKNOWN_REDIRECT_URI = "redirect_uri is unknown";
        private const string LTI_MESSAGE_HINT_INVALID = "lti_message_hint is invalid";
        private const string LOGIN_HINT_REQUIRED = "login_hint is required";
        private const string USER_CLIENT_MISMATCH = "client is not authorized for user";
        private const string DEPLOYMENT_CLIENT_MISMATCH = "deployment is not for client";

        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };
        private static readonly JsonSerializerOptions LTI_MESSAGE_JSON_SERIALIZER_OPTIONS = new() { TypeInfoResolver = new LtiMessageTypeResolver(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } };

        internal static async Task<IResult> HandleAsync(
            IServiceProvider serviceProvider,
            LinkGenerator linkGenerator,
            HttpContext httpContext,
            IOptionsMonitor<Lti13PlatformConfig> config,
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

            var tool = await dataService.GetToolAsync(request.Client_Id);

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

            if (deployment?.ClientId != tool.ClientId)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_CLIENT_MISMATCH, Error_Uri = AUTH_SPEC_URI });
            }

            var messageHandler = serviceProvider.GetKeyedService<IMessageHandler>(messageTypeString) ?? throw new NotImplementedException($"LTI Message Type {messageTypeString} does not have a registered message handler.");
            var context = !string.IsNullOrWhiteSpace(contextId) ? await dataService.GetContextAsync(contextId) : null;

            var launchPresentation = string.IsNullOrWhiteSpace(launchPresentationString) ? null : JsonSerializer.Deserialize<LaunchPresentation>(Convert.FromBase64String(launchPresentationString));

            var ltiMessage = await messageHandler.HandleAsync(tool, deployment, user, context, launchPresentation, messageString, request);

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
        public LtiMessage(string messageType, string issuer, string audience, string nonce, string deploymentId, IEnumerable<string> roles)
        {
            MessageType = messageType;
            Issuer = issuer;
            Audience = audience;
            Nonce = nonce;
            DeploymentId = deploymentId;
            Roles = roles;
        }

        [JsonPropertyName("iss")]
        public string Issuer { get; set; }

        [JsonPropertyName("aud")]
        public string Audience { get; set; }

        [JsonPropertyName("exp")]
        public long ExpirationDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("iat")]
        public long IssuedDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
        public string MessageType { get; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
        public string LtiVersion { get; } = "1.3.0";

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
        public string DeploymentId { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/roles")]
        public IEnumerable<string> Roles { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor")]
        public IEnumerable<string>? RoleScopeMentor { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/context")]
        public MessageContext? Context { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/tool_platform")]
        public Platform? Platform { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/launch_presentation")]
        public LaunchPresentation? LaunchPresentation { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-ags/claim/endpoint")]
        public ServiceEndpoints? ServiceEndpoints { get; set; }

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
        public long? UpdatedAtUnix => !UpdatedAt.HasValue ? null : new DateTimeOffset(UpdatedAt.Value).ToUnixTimeSeconds();

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

        public async Task SetCustomAsync(IDictionary<string, string>? custom, CustomPermissions permissions)
        {
            // TODO: Figure out the replacements
            Custom = custom;

            await Task.CompletedTask;
        }

        public void SetServiceEndpoints(string? lineItemsUrl, string? lineItemUrl, ServicePermissions permissions)
        {
            ServiceEndpoints = !permissions.Scopes.Any() || (string.IsNullOrWhiteSpace(lineItemsUrl) && string.IsNullOrWhiteSpace(lineItemUrl)) ? null : new ServiceEndpoints
            {
                Scopes = permissions.Scopes.ToList(),
                LineItemsUrl = lineItemsUrl,
                LineItemUrl = lineItemUrl
            };
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

    internal class LtiResourceLinkMessage(string issuer, string audience, string nonce, string deploymentId, IEnumerable<string> roles) : LtiMessage(Lti13MessageType.LtiResourceLinkRequest, issuer, audience, nonce, deploymentId, roles)
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/target_link_uri")]
        public required string TargetLinkUri { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/resource_link")]
        public required ResourceLink ResourceLink { get; set; }

        //new Claim("https://purl.imsglobal.org/spec/lti/claim/lis", "") // https://www.imsglobal.org/spec/lti/v1p3/#learning-information-services-lis-claim
    }

    internal class LtiDeepLinkingMessage(string issuer, string audience, string nonce, string deploymentId, IEnumerable<string> roles) : LtiMessage(Lti13MessageType.LtiDeepLinkingRequest, issuer, audience, nonce, deploymentId, roles)
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

    public class ServiceEndpoints
    {
        [JsonPropertyName("scope")]
        public required IEnumerable<string> Scopes { get; set; }

        [JsonPropertyName("lineitems")]
        public string? LineItemsUrl { get; set; }

        [JsonPropertyName("lineitem")]
        public string? LineItemUrl { get; set; }
    }

    public interface IMessageHandler
    {
        Task<LtiMessage> HandleAsync(Tool tool, Deployment deployment, User user, Context? context, LaunchPresentation? launchPresentation, string message, AuthenticationRequest request);
    }

    internal class DeepLinkingMessageHandler(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService) : IMessageHandler
    {
        public async Task<LtiMessage> HandleAsync(Tool tool, Deployment deployment, User user, Context? context, LaunchPresentation? launchPresentation, string message, AuthenticationRequest request)
        {
            var httpContext = httpContextAccessor.HttpContext!;
            var deepLinkSettings = string.IsNullOrWhiteSpace(message) ? null : JsonSerializer.Deserialize<DeepLinkSettings>(Convert.FromBase64String(message));

            var roles = context != null ? await dataService.GetRolesAsync(user.Id, context) : [];

            var ltiMessage = new LtiDeepLinkingMessage(config.CurrentValue.Issuer, tool.ClientId, request.Nonce!, deployment.Id, roles)
            {
                ExpirationDate = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds),
                DeepLinkSettings = new DeepLinkSettings
                {
                    AcceptPresentationDocumentTargets = new[] { deepLinkSettings?.AcceptPresentationDocumentTargets, config.CurrentValue.DeepLink.AcceptPresentationDocumentTargets }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                    AcceptTypes = new[] { deepLinkSettings?.AcceptTypes, config.CurrentValue.DeepLink.AcceptTypes }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                    DeepLinkReturnUrl = linkGenerator.GetUriByName(httpContext, RouteNames.DEEP_LINKING_RESPONSE, new { contextId = context?.Id }) ?? string.Empty,
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
            await ltiMessage.SetCustomAsync(tool.Custom.Merge(deployment.Custom), tool.CustomPermissions);
            ltiMessage.SetLaunchPresentation(launchPresentation);
            ltiMessage.SetPlatform(config.CurrentValue.Platform);
            ltiMessage.SetRoleScopeMentor(roles.Contains(Lti13ContextRoles.Mentor) && context != null ? await dataService.GetMentoredUserIdsAsync(user.Id, context) : null);
            ltiMessage.SetServiceEndpoints(
                context == null ? null : linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, RouteNames.GET_LINE_ITEMS, new { contextId = context.Id }),
                null,
                tool.ServicePermissions);
            ltiMessage.SetUser(user, tool.UserPermissions);

            return ltiMessage;
        }
    }

    internal class ResourceLinkMessageHandler(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService) : IMessageHandler
    {
        public async Task<LtiMessage> HandleAsync(Tool tool, Deployment deployment, User user, Context? context, LaunchPresentation? launchPresentation, string message, AuthenticationRequest request)
        {
            var resourceLinkId = message;
            var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(resourceLinkId);

            // TODO: Figure this out
            //if (resourceLink == null || resourceLink.ContextId != null && context != null && context.Id != resourceLink.ContextId || resourceLink.DeploymentId != deploymentId)
            //{
            //    return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_INVALID, Error_Uri = LTI_SPEC_URI });
            //}

            string? lineItemsUrl = null,
                lineItemUrl = null;

            if (context != null)
            {
                lineItemsUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, RouteNames.GET_LINE_ITEMS, new { contextId = context.Id });

                var lineItems = await dataService.GetLineItemsAsync(context.Id, 0, 1, null, resourceLinkId, null);

                if (lineItems.TotalItems == 1)
                {
                    lineItemUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, RouteNames.GET_LINE_ITEM, new { contextId = context.Id, lineItemId = lineItems.Items.First().Id });
                }
            }

            var roles = context != null ? await dataService.GetRolesAsync(user.Id, context) : [];

            var ltiMessage = new LtiResourceLinkMessage(config.CurrentValue.Issuer, tool.ClientId, request.Nonce!, deployment.Id, roles)
            {
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
            await ltiMessage.SetCustomAsync(tool.Custom.Merge(deployment.Custom).Merge(resourceLink.Custom), tool.CustomPermissions);
            ltiMessage.SetLaunchPresentation(launchPresentation);
            ltiMessage.SetPlatform(config.CurrentValue.Platform);
            ltiMessage.SetRoleScopeMentor(roles.Contains(Lti13ContextRoles.Mentor) && context != null ? await dataService.GetMentoredUserIdsAsync(user.Id, context) : null);
            ltiMessage.SetServiceEndpoints(
                lineItemsUrl,
                lineItemUrl,
                tool.ServicePermissions);
            ltiMessage.SetUser(user, tool.UserPermissions);

            return ltiMessage;
        }
    }
}

