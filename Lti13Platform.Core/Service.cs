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
    public class Service(IOptionsMonitor<Lti13PlatformConfig> config, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
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

        public LineItemServiceEndpoints? GetServiceEndpoints(string? contextId, string? lineItemId, ServicePermissions permissions)
        {
            if (!permissions.LineItemScopes.Any() || string.IsNullOrWhiteSpace(contextId) || httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            return new LineItemServiceEndpoints
            {
                Scopes = permissions.LineItemScopes.ToList(),
                LineItemsUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_LINE_ITEMS, new { contextId = contextId }),
                LineItemUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_LINE_ITEM, new { contextId = contextId, lineItemId = lineItemId }),
            };
        }

        public NamesRoleService? GetNamesRoleService(string? contextId, ServicePermissions permissions)
        {
            if (!permissions.AllowNameRoleProvisioningService || string.IsNullOrWhiteSpace(contextId) || httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            return new NamesRoleService
            {
                ContextMembershipUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_MEMBERSHIPS, new { contextId = contextId })!,
                ServiceVersions = ["2.0"]
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
        Task<Tool?> GetToolAsync(string toolId);
        Task<Tool?> GetToolByClientIdAsync(string clientId);
        Task<Deployment?> GetDeploymentAsync(string deploymentId);
        Task<Context?> GetContextAsync(string contextId);

        Task<IEnumerable<string>> GetRolesAsync(string userId, Context? context);
        Task<IEnumerable<string>> GetMentoredUserIdsAsync(string userId, Context? context);
        Task<User?> GetUserAsync(string userId);
        Task<PartialList<User>> GetUsersAsync(IEnumerable<string> userIds, DateTime? asOfDate = null);

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

        Task<PartialList<Grade>> GetGradesAsync(string contextId, string lineItemId, int pageIndex, int limit, string? userId);
        Task SaveGradeAsync(Grade result);

        Task<PartialList<Membership>> GetMembershipsAsync(string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null);
        Task<Attempt?> GetAttemptAsync(string contextId, string resourceLinkId, string userId);

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

        // TODO: implement sub-roles
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

    // BELOW ARE THE LTI 1.3 VARIABLES

    public static class Lti13UserVariables
    {
        /// <summary>
        /// user.id message property value; this may not be their real ID if they are masquerading as another user.
        /// </summary>
        public const string Id = "$User.id";

        /// <summary>
        /// user.image message property value.
        /// </summary>
        public const string Image = "$User.image";

        /// <summary>
        /// Username by which the message sender knows the user (typically, the name a user logs in with).
        /// </summary>
        public const string Username = "$User.username";

        /// <summary>
        /// One or more URIs describing the user's organizational properties (for example, an ldap:// URI).
        /// By best practice, message senders should separate multiple URIs by commas.
        /// </summary>
        public const string Org = "$User.org";

        /// <summary>
        /// role_scope_mentor message property value.
        /// </summary>
        public const string ScopeMentor = "$User.scope.mentor";

        /// <summary>
        /// A comma-separated list of grade(s) for which the user is enrolled.
        /// The permitted vocabulary is from the 'grades' field utilized in OneRoster Users.
        /// </summary>
        public const string GradeLevelsOneRoster = "$User.gradeLevels.oneRoster";
    }

    public static class Lti13ContextVariables
    {
        /// <summary>
        /// (Context.id property)
        /// </summary>
        public const string Id = "$Context.id";

        /// <summary>
        /// A URI describing the context's organizational properties; for example, an ldap:// URI.
        /// By best practice, message senders should separate URIs using commas.
        /// </summary>
        public const string Org = "$Context.org";

        /// <summary>
        /// (context.type property)
        /// </summary>
        public const string Type = "$Context.type";

        /// <summary>
        /// (context.label property)
        /// </summary>
        public const string Label = "$Context.label";

        /// <summary>
        /// (context.title property)
        /// </summary>
        public const string Title = "$Context.title";

        /// <summary>
        /// The sourced ID of the context.
        /// </summary>
        public const string SourcedId = "$Context.sourcedId";

        /// <summary>
        /// A comma-separated list of URL-encoded context ID values representing previous copies of the context;
        /// the ID of most recent copy should appear first in the list followed by any earlier IDs in reverse chronological order.
        /// If the context was created from scratch, not as a copy of an existing context, then this variable should have an empty value.
        /// </summary>
        public const string IdHistory = "$Context.id.history";

        /// <summary>
        /// A comma-separated list of grade(s) for which the context is attended.
        /// The permitted vocabulary is from the grades field utilized in OneRoster Classes.
        /// </summary>
        public const string GradeLevelsOneRoster = "$Context.gradeLevels.oneRoster";
    }

    public static class Lti13ResourceLinkVariables
    {
        /// <summary>
        /// (ResourceLink.id property)
        /// </summary>
        public const string Id = "$ResourceLink.id";

        /// <summary>
        /// (ResourceLink.title property)
        /// </summary>
        public const string Title = "$ResourceLink.title";

        /// <summary>
        /// (ResourceLink.description property)
        /// </summary>
        public const string Description = "$ResourceLink.description";

        /// <summary>
        /// The ISO 8601 date and time when this resource is available for learners to access.
        /// </summary>
        public const string AvailableStartDateTime = "$ResourceLink.available.startDateTime";

        /// <summary>
        /// The ISO 8601 date and time when this resource is available for the current user to access.
        /// This date overrides that of ResourceLink.available.startDateTime.
        /// A value of an empty string indicates that the date for the resource should be used.
        /// </summary>
        public const string AvailableUserStartDateTime = "$ResourceLink.available.user.startDateTime";

        /// <summary>
        /// The ISO 8601 date and time when this resource ceases to be available for learners to access.
        /// </summary>
        public const string AvailableEndDateTime = "$ResourceLink.available.endDateTime";

        /// <summary>
        /// The ISO 8601 date and time when this resource ceases to be available for the current user to access.
        /// This date overrides that of ResourceLink.available.endDateTime.
        /// A value of an empty string indicates that the date for the resource should be used.
        /// </summary>
        public const string AvailableUserEndDateTime = "$ResourceLink.available.user.endDateTime";

        /// <summary>
        /// The ISO 8601 date and time when this resource can start receiving submissions.
        /// </summary>
        public const string SubmissionStartDateTime = "$ResourceLink.submission.startDateTime";

        /// <summary>
        /// The ISO 8601 date and time when the current user can submit to the resource.
        /// This date overrides that of ResourceLink.submission.startDateTime.
        /// A value of an empty string indicates that the date for the resource should be used.
        /// </summary>
        public const string SubmissionUserStartDateTime = "$ResourceLink.submission.user.startDateTime";

        /// <summary>
        /// The ISO 8601 date and time when this resource stops accepting submissions.
        /// </summary>
        public const string SubmissionEndDateTime = "$ResourceLink.submission.endDateTime";

        /// <summary>
        /// The ISO 8601 date and time when the current user stops being able to submit to the resource.
        /// This date overrides that of ResourceLink.submission.endDateTime.
        /// A value of an empty string indicates that the date for the resource should be used.
        /// </summary>
        public const string SubmissionUserEndDateTime = "$ResourceLink.submission.user.endDateTime";

        /// <summary>
        /// The ISO 8601 date and time set when the grades for the associated line item can be released to learner.
        /// </summary>
        public const string LineItemReleaseDateTime = "$ResourceLink.lineitem.releaseDateTime";

        /// <summary>
        /// The ISO 8601 date and time set when the current user's grade for the associated line item can be released to the user.
        /// This date overrides that of ResourceLink.lineitem.releaseDateTime.
        /// A value of an empty string indicates that the date for the resource should be used.
        /// </summary>
        public const string LineItemUserReleaseDateTime = "$ResourceLink.lineitem.user.releaseDateTime";

        /// <summary>
        /// A comma-separated list of URL-encoded resource link ID values representing the ID of the link from a previous copy of the context;
        /// the most recent copy should appear first in the list followed by any earlier IDs in reverse chronological order.
        /// If the link was first added to the current context then this variable should have an empty value.
        /// </summary>
        public const string IdHistory = "$ResourceLink.id.history";
    }

    public static class Lti13ToolPlatformVariables
    {
        /// <summary>
        /// Corresponds to the tool_platform.product_family_code property.
        /// </summary>
        public const string ProductFamilyCode = "$ToolPlatform.productFamilyCode";

        /// <summary>
        /// Corresponds to the tool_platform.version property.
        /// </summary>
        public const string Version = "$ToolPlatform.version";

        /// <summary>
        /// Corresponds to the tool_platform.instance_guid property.
        /// </summary>
        public const string InstanceGuid = "$ToolPlatformInstance.guid";

        /// <summary>
        /// Corresponds to the tool_platform.instance_name property.
        /// </summary>
        public const string InstanceName = "$ToolPlatformInstance.name";

        /// <summary>
        /// Corresponds to the tool_platform.instance_description property.
        /// </summary>
        public const string InstanceDescription = "$ToolPlatformInstance.description";

        /// <summary>
        /// Corresponds to the tool_platform.instance_url property.
        /// </summary>
        public const string InstanceUrl = "$ToolPlatformInstance.url";

        /// <summary>
        /// Corresponds to the tool_platform.instance_contact_email property.
        /// </summary>
        public const string InstanceContactEmail = "$ToolPlatformInstance.contactEmail";
    }

    // BELOW ARE THE LIS VARIABLES

    // TODO: ActualPerson
    public static class LisPersonVariables
    {
        /// <summary>
        /// XPath for value from LIS database: personRecord/sourcedId
        /// (lis_person.sourcedid property)
        /// </summary>
        public const string SourcedId = "$Person.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/formname/[formnameType/instanceValue/text="Full"]/formattedName/text
        /// (lis_person.name_full property)
        /// </summary>
        public const string NameFull = "$Person.name.full";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Family"]/instanceValue/text
        /// (lis_person.name_family property)
        /// </summary>
        public const string NameFamily = "$Person.name.family";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Given"]/instanceValue/text
        /// (lis_person.name_given property)
        /// </summary>
        public const string NameGiven = "$Person.name.given";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Middle"]/instanceValue/text
        /// </summary>
        public const string NameMiddle = "$Person.name.middle";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Prefix"]/instanceValue/text
        /// </summary>
        public const string NamePrefix = "$Person.name.prefix";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Suffix"]/instanceValue/text
        /// </summary>
        public const string NameSuffix = "$Person.name.suffix";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/demographics/gender/instanceValue/text
        /// </summary>
        public const string Gender = "$Person.gender";

        /// <summary>
        /// No XPath available (N/A)
        /// </summary>
        public const string GenderPronouns = "$Person.gender.pronouns";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]/addressPart/nameValuePair/[instanceName/text="NonFieldedStreetAddress1"]/instanceValue/text
        /// </summary>
        public const string AddressStreet1 = "$Person.address.street1";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]/addressPart/nameValuePair[instanceName/text="NonFieldedStreetAddress2"]/instanceValue/text
        /// </summary>
        public const string AddressStreet2 = "$Person.address.street2";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="NonFieldedStreetAddress3"]/instanceValue/text
        /// </summary>
        public const string AddressStreet3 = "$Person.address.street3";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="NonFieldedStreetAddress4"]/instanceValue/
        /// </summary>
        public const string AddressStreet4 = "$Person.address.street4";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Locality"]/instanceValue/text
        /// </summary>
        public const string AddressLocality = "$Person.address.locality";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred "]addressPart/nameValuePair/[instanceName/text="Statepr"]/instanceValue/text
        /// </summary>
        public const string AddressStatepr = "$Person.address.statepr";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Country"]/instanceValue/text
        /// </summary>
        public const string AddressCountry = "$Person.address.country";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Postcode"]/instanceValue/text
        /// </summary>
        public const string AddressPostcode = "$Person.address.postcode";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Timezone"]/instanceValue/text
        /// </summary>
        public const string AddressTimezone = "$Person.address.timezone";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Mobile"]/contactInfoValue/text
        /// </summary>
        public const string PhoneMobile = "$Person.phone.mobile";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Telephone_Primary"]/contactinfoValue/text
        /// </summary>
        public const string PhonePrimary = "$Person.phone.primary";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo [contactinfoType/instanceValue/text="Telephone_Home"]/contactinfoValue/text
        /// </summary>
        public const string PhoneHome = "$Person.phone.home";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo [contactinfoType/instanceValue/text="Telephone_Work"]/contactinfoValue /text
        /// </summary>
        public const string PhoneWork = "$Person.phone.work";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Email_Primary"]/contactinfoValue/text
        /// (lis.person_contact_email_primary property)
        /// </summary>
        public const string EmailPrimary = "$Person.email.primary";

        /// <summary>
        /// XPath for value from LIS database: person/contactinfo[contactinfoType/instanceValue/text="Email_Personal"]/contactinfoValue/text
        /// </summary>
        public const string EmailPersonal = "$Person.email.personal";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Web-Address"]/contactinfoValue/text
        /// </summary>
        public const string Webaddress = "$Person.webaddress";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="SMS"]/contactinfoValue/text
        /// </summary>
        public const string Sms = "$Person.sms";
    }

    public static class LisCourseVariables
    {
        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/sourcedId
        /// </summary>
        public const string SourcedId = "$CourseTemplate.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/courseTemplate/label/textString
        /// </summary>
        public const string Label = "$CourseTemplate.label";

        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/courseTemplate/title/textString
        /// </summary>
        public const string Title = "$CourseTemplate.title";

        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/courseTemplate/catalogDescription/shortDescription
        /// </summary>
        public const string ShortDescription = "$CourseTemplate.shortDescription";

        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/courseTemplate/catalogDescription/longDescription
        /// </summary>
        public const string LongDescription = "$CourseTemplate.longDescription";

        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/courseTemplate/courseNumber/textString
        /// </summary>
        public const string CourseNumber = "$CourseTemplate.courseNumber";

        /// <summary>
        /// XPath for value from LIS database: courseTemplateRecord/courseTemplate/defaultCredits/textString
        /// </summary>
        public const string Credits = "$CourseTemplate.credits";
    }

    public static class LisCourseOfferingVariables
    {
        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/sourcedId
        /// (lis_course_offering_sourcedid property)
        /// </summary>
        public const string SourcedId = "$CourseOffering.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/label
        /// </summary>
        public const string Label = "$CourseOffering.label";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/title
        /// </summary>
        public const string Title = "$CourseOffering.title";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/catalogDescription/shortDescription
        /// </summary>
        public const string ShortDescription = "$CourseOffering.shortDescription";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/catalogDescription/longDescription
        /// </summary>
        public const string LongDescription = "$CourseOffering.longDescription";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/courseNumber/textString
        /// </summary>
        public const string CourseNumber = "$CourseOffering.courseNumber";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/defaultCredits/textString
        /// </summary>
        public const string Credits = "$CourseOffering.credits";

        /// <summary>
        /// XPath for value from LIS database: courseOfferingRecord/courseOffering/defaultCredits/textString
        /// </summary>
        public const string AcademicSession = "$CourseOffering.academicSession";
    }

    public static class LisCourseSectionVariables
    {
        /// <summary>
        /// XPath for value from LIS database: courseSection/sourcedId
        /// (lis_course_section_sourcedid property)
        /// </summary>
        public const string SourcedId = "$CourseSection.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/label
        /// </summary>
        public const string Label = "$CourseSection.label";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/title
        /// </summary>
        public const string Title = "$CourseSection.title";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/catalogDescription/shortDescription
        /// </summary>
        public const string ShortDescription = "$CourseSection.shortDescription";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/catalogDescription/longDescription
        /// </summary>
        public const string LongDescription = "$CourseSection.longDescription";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/courseNumber/textString
        /// </summary>
        public const string CourseNumber = "$CourseSection.courseNumber";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/defaultCredits/textString
        /// </summary>
        public const string Credits = "$CourseSection.credits";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/maxNumberofStudents
        /// </summary>
        public const string MaxNumberOfStudents = "$CourseSection.maxNumberOfStudents";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/numberofStudents
        /// </summary>
        public const string NumberOfStudents = "$CourseSection.numberOfStudents";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/org[type/textString="Dept"]/orgName/textString
        /// </summary>
        public const string Dept = "$CourseSection.dept";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/timeFrame/begin
        /// </summary>
        public const string TimeFrameBegin = "$CourseSection.timeFrame.begin";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/timeFrame/end
        /// </summary>
        public const string TimeFrameEnd = "$CourseSection.timeFrame.end";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/enrollControl/enrollAccept
        /// </summary>
        public const string EnrollControlAccept = "$CourseSection.enrollControl.accept";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/enrollControl/enrollAllowed
        /// </summary>
        public const string EnrollControlAllowed = "$CourseSection.enrollControl.allowed";

        /// <summary>
        /// XPath for value from LIS database: courseSectionRecord/courseSection/dataSource
        /// </summary>
        public const string DataSource = "$CourseSection.dataSource";

        /// <summary>
        /// XPath for value from LIS database: createCourseSectionFromCourseSectionRequest/sourcedId
        /// </summary>
        public const string SourceSectionId = "$CourseSection.sourceSectionId";
    }

    public static class LisGroupVariables
    {
        /// <summary>
        /// XPath for value from LIS database: groupRecord/sourcedId
        /// </summary>
        public const string SourcedId = "$Group.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/groupType/scheme/textString
        /// </summary>
        public const string Scheme = "$Group.scheme";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/groupType/typevalue/textString
        /// </summary>
        public const string Typevalue = "$Group.typevalue";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/groupType/typevalue/level/textString
        /// </summary>
        public const string Level = "$Group.level";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/email
        /// </summary>
        public const string Email = "$Group.email";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/url
        /// </summary>
        public const string Url = "$Group.url";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/timeframe/begin
        /// </summary>
        public const string TimeFrameBegin = "$Group.timeFrame.begin";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/timeframe/end
        /// </summary>
        public const string TimeFrameEnd = "$Group.timeFrame.end";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/enrollControl/enrollAccept
        /// </summary>
        public const string EnrollControlAccept = "$Group.enrollControl.accept";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/enrollControl/enrollAllowed
        /// </summary>
        public const string EnrollControlEnd = "$Group.enrollControl.end";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/description/shortDescription
        /// </summary>
        public const string ShortDescription = "$Group.shortDescription";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/description/longDescription
        /// </summary>
        public const string LongDescription = "$Group.longDescription";

        /// <summary>
        /// XPath for value from LIS database: groupRecord/group/relationship[relation="Parent"]/sourcedId
        /// </summary>
        public const string ParentId = "$Group.parentId";
    }

    public static class LisMembershipVariables
    {
        /// <summary>
        /// XPath for value from LIS database: membershipRecord/sourcedId
        /// </summary>
        public const string SourcedId = "$Membership.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: membershipRecord/membership/collectionSourcedId
        /// </summary>
        public const string CollectionSourcedid = "$Membership.collectionSourcedid";

        /// <summary>
        /// XPath for value from LIS database: membershipRecord/membership/memnber/personSourcedId
        /// </summary>
        public const string PersonSourcedId = "$Membership.personSourcedId";

        /// <summary>
        /// XPath for value from LIS database: membershipRecord/membership/member/role/status
        /// </summary>
        public const string Status = "$Membership.status";

        /// <summary>
        /// XPath for value from LIS database: membershipRecord/membership/member/role/roleType
        /// (roles property)
        /// </summary>
        public const string Role = "$Membership.role";

        /// <summary>
        /// XPath for value from LIS database: membershipRecord/membership/member/role/dateTime
        /// </summary>
        public const string CreatedTimestamp = "$Membership.createdTimestamp";

        /// <summary>
        /// XPath for value from LIS database: membershipRecord/membership/member/role/dataSource
        /// </summary>
        public const string DataSource = "$Membership.dataSource";

        /// <summary>
        /// Property: role_scope_mentor
        /// </summary>
        public const string RoleScopeMentor = "$Membership.role.scope.mentor";
    }

    public static class LisMessageVariables
    {
        /// <summary>
        /// URL for returning the user to the platform (for example, the launch_presentation.return_url property).
        /// </summary>
        public const string ReturnUrl = "$Message.returnUrl";

        /// <summary>
        /// Corresponds to the launch_presentation.document_target property.
        /// </summary>
        public const string DocumentTarget = "$Message.documentTarget";

        /// <summary>
        /// Corresponds to the launch_presentation.height property.
        /// </summary>
        public const string Height = "$Message.height";

        /// <summary>
        /// Corresponds to the launch_presentation.width property.
        /// </summary>
        public const string Width = "$Message.width";

        /// <summary>
        /// Corresponds to the launch_presentation.locale property.
        /// </summary>
        public const string Locale = "$Message.locale";
    }
}
