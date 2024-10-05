namespace NP.Lti13Platform.Core
{
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

}
