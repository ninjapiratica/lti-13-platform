using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
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

        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

        internal static async Task<IResult> HandleAsync(
            LinkGenerator linkGenerator,
            HttpContext httpContext,
            IOptionsMonitor<Lti13PlatformConfig> config,
            Service service,
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
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = UNAUTHORIZED_CLIENT, Error_Uri = LTI_SPEC_URI });
            }

            _ = Enum.TryParse(messageTypeString, out Lti13MessageType messageType);

            var deployment = await dataService.GetDeploymentAsync(deploymentId);

            if (deployment?.ClientId != tool.ClientId)
            {
                throw new Exception();
            }

            var context = !string.IsNullOrWhiteSpace(contextId) ? await dataService.GetContextAsync(contextId) : null;
            var roles = context != null ? await dataService.GetRolesAsync(user.Id, context) : [];

            var serviceEndpoints = context == null ? null : new ServiceEndpoints
            {
                Scopes = tool.Scopes,
                LineItemsUrl = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { contextId = contextId })
            };

            LtiMessage ltiMessage;

            switch (messageType)
            {
                case Lti13MessageType.LtiResourceLinkRequest:
                    var resourceLinkId = messageString;
                    var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(resourceLinkId);

                    if (resourceLink == null || resourceLink.ContextId != null && context != null && context.Id != resourceLink.ContextId || resourceLink.DeploymentId != deploymentId)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_INVALID, Error_Uri = LTI_SPEC_URI });
                    }

                    if (serviceEndpoints != null)
                    {
                        var lineItems = await dataService.GetLineItemsAsync(contextId, 0, 1, null, resourceLinkId, null);

                        if (lineItems.TotalItems == 1)
                        {
                            serviceEndpoints.LineItemUrl = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId = contextId, lineItemId = lineItems.Items.First().Id });
                        }
                    }

                    ltiMessage = new LtiResourceLinkMessage
                    {
                        Audience = request.Client_Id,
                        ExpirationDate = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds),
                        IssuedDate = DateTime.UtcNow,
                        Issuer = config.CurrentValue.Issuer,
                        Nonce = request.Nonce,
                        Subject = user.Id,
                        DeploymentId = deploymentId,
                        Roles = roles,
                        ResourceLink = new ResourceLink
                        {
                            Id = resourceLink.Id,
                            Description = resourceLink.Text,
                            Title = resourceLink.Title
                        },
                        TargetLinkUri = string.IsNullOrWhiteSpace(resourceLink.Url) ? tool.LaunchUrl : resourceLink.Url,
                    };
                    break;
                case Lti13MessageType.LtiDeepLinkingRequest:
                    var deepLinkSettings = string.IsNullOrWhiteSpace(messageString) ? null : JsonSerializer.Deserialize<DeepLinkSettings>(Convert.FromBase64String(messageString));

                    ltiMessage = new LtiDeepLinkingMessage
                    {
                        Audience = request.Client_Id,
                        ExpirationDate = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds),
                        IssuedDate = DateTime.UtcNow,
                        Issuer = config.CurrentValue.Issuer,
                        Nonce = request.Nonce,
                        Subject = user.Id,
                        DeploymentId = deploymentId,
                        Roles = roles,
                        DeepLinkSettings = new DeepLinkSettings
                        {
                            AcceptPresentationDocumentTargets = new[] { deepLinkSettings?.AcceptPresentationDocumentTargets, config.CurrentValue.DeepLink.AcceptPresentationDocumentTargets }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                            AcceptTypes = new[] { deepLinkSettings?.AcceptTypes, config.CurrentValue.DeepLink.AcceptTypes }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                            DeepLinkReturnUrl = linkGenerator.GetUriByName(httpContext, RouteNames.DEEP_LINKING_RESPONSE, new { contextId = contextId }) ?? string.Empty,
                            AcceptLineItem = new[] { deepLinkSettings?.AcceptLineItem, config.CurrentValue.DeepLink.AcceptLineItem }.FirstOrDefault(x => x.HasValue),
                            AcceptMediaTypes = new[] { deepLinkSettings?.AcceptMediaTypes, config.CurrentValue.DeepLink.AcceptMediaTypes }.FirstOrDefault(x => x != null && x.Any()),
                            AcceptMultiple = new[] { deepLinkSettings?.AcceptMultiple, config.CurrentValue.DeepLink.AcceptMultiple }.FirstOrDefault(x => x.HasValue),
                            AutoCreate = new[] { deepLinkSettings?.AutoCreate, config.CurrentValue.DeepLink.AutoCreate }.FirstOrDefault(x => x.HasValue),
                            Data = deepLinkSettings?.Data,
                            Text = deepLinkSettings?.Text,
                            Title = deepLinkSettings?.Title,
                        }
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }

            ltiMessage.Context = context == null ? null : new MessageContext
            {
                Id = contextId,
                Label = context.Label,
                Title = context.Title,
                Types = context.Types
            };

            // TODO: figure out custom replacements
            ltiMessage.Custom = null;

            ltiMessage.LaunchPresentation = string.IsNullOrWhiteSpace(launchPresentationString) ? null : JsonSerializer.Deserialize<LaunchPresentation>(Convert.FromBase64String(launchPresentationString));

            ltiMessage.Platform = config.CurrentValue.Platform;

            ltiMessage.RoleScopeMentor = roles.Contains(Lti13ContextRoles.Mentor) && context != null ? await dataService.GetMentoredUserIdsAsync(user.Id, context) : null;

            ltiMessage.ServiceEndpoints = serviceEndpoints;

            ltiMessage.Address = user.Address == null || !tool.UserPermissions.Address ? null : new AddressClaim
            {
                Country = tool.UserPermissions.AddressCountry ? user.Address.Country : null,
                Formatted = tool.UserPermissions.AddressFormatted ? user.Address.Formatted : null,
                Locality = tool.UserPermissions.AddressLocality ? user.Address.Locality : null,
                PostalCode = tool.UserPermissions.AddressPostalCode ? user.Address.PostalCode : null,
                Region = tool.UserPermissions.AddressRegion ? user.Address.Region : null,
                StreetAddress = tool.UserPermissions.AddressStreetAddress ? user.Address.StreetAddress : null
            };
            ltiMessage.Birthdate = tool.UserPermissions.Birthdate ? user.Birthdate : null;
            ltiMessage.Email = tool.UserPermissions.Email ? user.Email : null;
            ltiMessage.EmailVerified = tool.UserPermissions.EmailVerified ? user.EmailVerified : null;
            ltiMessage.FamilyName = tool.UserPermissions.FamilyName ? user.FamilyName : null;
            ltiMessage.Gender = tool.UserPermissions.Gender ? user.Gender : null;
            ltiMessage.GivenName = tool.UserPermissions.GivenName ? user.GivenName : null;
            ltiMessage.Locale = tool.UserPermissions.Locale ? user.Locale : null;
            ltiMessage.MiddleName = tool.UserPermissions.MiddleName ? user.MiddleName : null;
            ltiMessage.Name = tool.UserPermissions.Name ? user.Name : null;
            ltiMessage.Nickname = tool.UserPermissions.Nickname ? user.Nickname : null;
            ltiMessage.PhoneNumber = tool.UserPermissions.PhoneNumber ? user.PhoneNumber : null;
            ltiMessage.PhoneNumberVerified = tool.UserPermissions.PhoneNumberVerified ? user.PhoneNumberVerified : null;
            ltiMessage.Picture = tool.UserPermissions.Picture ? user.Picture : null;
            ltiMessage.PreferredUsername = tool.UserPermissions.PreferredUsername ? user.PreferredUsername : null;
            ltiMessage.Profile = tool.UserPermissions.Profile ? user.Profile : null;
            ltiMessage.UpdatedAt = tool.UserPermissions.UpdatedAt ? user.UpdatedAt : null;
            ltiMessage.Website = tool.UserPermissions.Website ? user.Website : null;
            ltiMessage.TimeZone = tool.UserPermissions.TimeZone ? user.TimeZone : null;

            var privateKey = await dataService.GetPrivateKeyAsync();

            var token = new JsonWebTokenHandler().CreateToken(
                JsonSerializer.Serialize(ltiMessage, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } }),
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

    [JsonDerivedType(typeof(LtiResourceLinkMessage))]
    [JsonDerivedType(typeof(LtiDeepLinkingMessage))]
    internal abstract class LtiMessage(Lti13MessageType messageType)
    {
        [JsonPropertyName("iss")]
        public required string Issuer { get; set; }

        [JsonPropertyName("sub")]
        public required string Subject { get; set; }

        [JsonPropertyName("aud")]
        public required string Audience { get; set; }

        [JsonPropertyName("exp")]
        public long ExpirationDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("iat")]
        public long IssuedDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("nonce")] 
        public required string Nonce { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
        public Lti13MessageType MessageType { get; } = messageType;

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
        public ServiceEndpoints? ServiceEndpoints { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
        public Dictionary<string, string>? Custom { get; set; }

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
        public DateTime? UpdatedAt { get; set; } // Convert to UNIX timestamp

        [JsonPropertyName("updated_at")]
        public long? UpdatedAtUnix => !UpdatedAt.HasValue ? null : new DateTimeOffset(UpdatedAt.Value).ToUnixTimeSeconds();
    }

    internal class LtiResourceLinkMessage() : LtiMessage(Lti13MessageType.LtiResourceLinkRequest)
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/target_link_uri")]
        public required string TargetLinkUri { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/resource_link")]
        public required ResourceLink ResourceLink { get; set; }

        //new Claim("https://purl.imsglobal.org/spec/lti/claim/lis", "") // https://www.imsglobal.org/spec/lti/v1p3/#learning-information-services-lis-claim
    }

    internal class LtiDeepLinkingMessage() : LtiMessage(Lti13MessageType.LtiDeepLinkingRequest)
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

    internal class AddressClaim
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

    internal class MessageContext
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

    internal class ServiceEndpoints
    {
        [JsonPropertyName("scope")]
        public required IEnumerable<string> Scopes { get; set; }

        [JsonPropertyName("lineitems")]
        public string? LineItemsUrl { get; set; }

        [JsonPropertyName("lineitem")]
        public string? LineItemUrl { get; set; }
    }
}

