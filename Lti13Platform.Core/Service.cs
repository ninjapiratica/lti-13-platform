namespace NP.Lti13Platform.Core
{
    public class Service
    {
        private const string LTI_VERSION = "1.3.0";

        public IEnumerable<(string Key, object Value)> GetClaims<T>(
            T message,
            string deploymentId,
            Lti13RolesClaim roles,
            Lti13Context? context = null,
            Lti13PlatformClaim? platform = null,
            Lti13RoleScopeMentorClaim? roleScopeMentor = null,
            Lti13LaunchPresentationClaim? launchPresentation = null,
            Lti13CustomClaim? custom = null)
            where T : ILti13Message
        {
            yield return ("https://purl.imsglobal.org/spec/lti/claim/message_type", T.MessageType);
            yield return ("https://purl.imsglobal.org/spec/lti/claim/version", LTI_VERSION);
            yield return ("https://purl.imsglobal.org/spec/lti/claim/deployment_id", deploymentId);

            foreach (var claim1 in new ILti13Claim?[] { message, roles, context, platform, roleScopeMentor, launchPresentation, custom })
            {
                if (claim1 != null)
                {
                    foreach (var claim in claim1.GetClaims())
                    {
                        yield return claim;
                    }
                }
            }
        }
    }

    public class Lti13Client
    {
        public required string Id { get; set; }

        public required string OidcInitiationUrl { get; set; }

        public required IEnumerable<string> RedirectUris { get; set; }

        public Jwks? Jwks { get; set; }
    }

    public class JwtPublicKey : Jwks
    {
        public required string PublicKey { get; set; }
    }

    public class JwksUri : Jwks
    {
        public required string Uri { get; set; }
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

        public IEnumerable<(string Key, object Value)> GetClaims()
        {
            var dict = new Dictionary<string, object>();

            if (Id != null) dict.Add("id", Id);
            if (Types.Any()) dict.Add("type", Types);
            if (Label != null) dict.Add("label", Label);
            if (Title != null) dict.Add("title", Title);

            if (dict.Count > 0)
            {
                yield return ("https://purl.imsglobal.org/spec/lti/claim/context", dict);
            }
        }
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

        public IEnumerable<(string Key, object Value)> GetClaims()
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

                yield return ("https://purl.imsglobal.org/spec/lti/claim/tool_platform", dict);
            }
        }
    }

    public class Lti13RolesClaim : ILti13Claim
    {
        public List<string> Roles { get; set; } = [];

        public IEnumerable<(string Key, object Value)> GetClaims()
        {
            yield return ("https://purl.imsglobal.org/spec/lti/claim/roles", Roles);
        }
    }

    public class Lti13RoleScopeMentorClaim : ILti13Claim
    {
        public List<string> UserIds { get; set; } = [];

        public IEnumerable<(string Key, object Value)> GetClaims()
        {
            if (UserIds.Count > 0)
            {
                yield return ("https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor", UserIds);
            }
        }
    }

    public class Lti13LaunchPresentationClaim : ILti13Claim
    {
        public string? Document_Target { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }
        public string? Return_Url { get; set; }
        public string? Locale { get; set; }

        public IEnumerable<(string Key, object Value)> GetClaims()
        {
            var dict = new Dictionary<string, object>();

            if (Document_Target != null) dict.Add("document_target", Document_Target);
            if (Height != null) dict.Add("height", Height.GetValueOrDefault());
            if (Width != null) dict.Add("width", Width.GetValueOrDefault());
            if (Return_Url != null) dict.Add("return_url", Return_Url);
            if (Locale != null) dict.Add("locale", Locale);

            if (dict.Count > 0)
            {
                yield return ("https://purl.imsglobal.org/spec/lti/claim/launch_presentation", dict);
            }
        }
    }

    public class Lti13CustomClaim : ILti13Claim
    {
        public Dictionary<string, string> CustomClaims { get; set; } = [];

        public IEnumerable<(string Key, object Value)> GetClaims()
        {
            if (CustomClaims.Count != 0)
            {
                yield return ("http://imsglobal.org/custom", CustomClaims);
            }
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

    public class Lti13ResourceLink
    {
        public required string Id { get; set; }

        public required string ContextId { get; set; }
    }
}
