namespace NP.Lti13Platform.Core.Constants
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
    }

    /// <summary>
    /// Whenever a platform specifies a sub-role, by best practice it should also include the associated principal role.
    /// <see href="https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles"/>
    /// </summary>
    public static class Lti13ContextSubRoles
    {
        public const string Administrator_Administrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Administrator";
        public const string Administrator_Developer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Developer";
        public const string Administrator_ExternalDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalDeveloper";
        public const string Administrator_ExternalSupport = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalSupport";
        public const string Administrator_ExternalSystemAdministrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalSystemAdministrator";
        public const string Administrator_Support = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Support";
        public const string Administrator_SystemAdministrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#SystemAdministrator";

        public const string ContentDeveloper_ContentDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ContentDeveloper";
        public const string ContentDeveloper_ContentExpert = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ContentExpert";
        public const string ContentDeveloper_ExternalContentExpert = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ExternalContentExpert";
        public const string ContentDeveloper_Librarian = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#Librarian";

        public const string Instructor_ExternalInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#ExternalInstructor";
        public const string Instructor_Grader = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#Grader";
        public const string Instructor_GuestInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#GuestInstructor";
        public const string Instructor_Lecturer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#Lecturer";
        public const string Instructor_PrimaryInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#PrimaryInstructor";
        public const string Instructor_SecondaryInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#SecondaryInstructor";
        public const string Instructor_TeachingAssistant = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistant";
        public const string Instructor_TeachingAssistantGroup = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantGroup";
        public const string Instructor_TeachingAssistantOffering = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantOffering";
        public const string Instructor_TeachingAssistantSection = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantSection";
        public const string Instructor_TeachingAssistantSectionAssociation = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantSectionAssociation";
        public const string Instructor_TeachingAssistantTemplate = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantTemplate";

        public const string Learner_ExternalLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#ExternalLearner";
        public const string Learner_GuestLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#GuestLearner";
        public const string Learner_Instructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#Instructor";
        public const string Learner_Learner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#Learner";
        public const string Learner_NonCreditLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#NonCreditLearner";

        public const string Mentor_Advisor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Advisor";
        public const string Mentor_Auditor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Auditor";
        public const string Mentor_ExternalAdvisor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalAdvisor";
        public const string Mentor_ExternalAuditor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalAuditor";
        public const string Mentor_ExternalLearningFacilitator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalLearningFacilitator";
        public const string Mentor_ExternalMentor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalMentor";
        public const string Mentor_ExternalReviewer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalReviewer";
        public const string Mentor_ExternalTutor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalTutor";
        public const string Mentor_LearningFacilitator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#LearningFacilitator";
        public const string Mentor_Mentor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Mentor";
        public const string Mentor_Reviewer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Reviewer";
        public const string Mentor_Tutor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Tutor";

        public const string Manager_AreaManager = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#AreaManager";
        public const string Manager_CourseCoordinator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#CourseCoordinator";
        public const string Manager_ExternalObserver = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#ExternalObserver";
        public const string Manager_Manager = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#Manager";
        public const string Manager_Observer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#Observer";

        public const string Member_Member = "http://purl.imsglobal.org/vocab/lis/v2/membership/Member#Member";

        public const string Officer_Chair = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Chair";
        public const string Officer_Communications = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Communications";
        public const string Officer_Secretary = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Secretary";
        public const string Officer_Treasurer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Treasurer";
        public const string Officer_ViceChair = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Vice-Chair";
    }
}
