namespace NP.Lti13Platform.Core.Constants;

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
}

/// <summary>
/// Whenever a platform specifies a sub-role, by best practice it should also include the associated principal role.
/// <see href="https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles"/>
/// </summary>
public static class Lti13ContextSubRoles
{
    public static readonly string Administrator_Administrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Administrator";
    public static readonly string Administrator_Developer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Developer";
    public static readonly string Administrator_ExternalDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalDeveloper";
    public static readonly string Administrator_ExternalSupport = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalSupport";
    public static readonly string Administrator_ExternalSystemAdministrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalSystemAdministrator";
    public static readonly string Administrator_Support = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Support";
    public static readonly string Administrator_SystemAdministrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#SystemAdministrator";

    public static readonly string ContentDeveloper_ContentDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ContentDeveloper";
    public static readonly string ContentDeveloper_ContentExpert = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ContentExpert";
    public static readonly string ContentDeveloper_ExternalContentExpert = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ExternalContentExpert";
    public static readonly string ContentDeveloper_Librarian = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#Librarian";

    public static readonly string Instructor_ExternalInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#ExternalInstructor";
    public static readonly string Instructor_Grader = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#Grader";
    public static readonly string Instructor_GuestInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#GuestInstructor";
    public static readonly string Instructor_Lecturer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#Lecturer";
    public static readonly string Instructor_PrimaryInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#PrimaryInstructor";
    public static readonly string Instructor_SecondaryInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#SecondaryInstructor";
    public static readonly string Instructor_TeachingAssistant = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistant";
    public static readonly string Instructor_TeachingAssistantGroup = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantGroup";
    public static readonly string Instructor_TeachingAssistantOffering = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantOffering";
    public static readonly string Instructor_TeachingAssistantSection = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantSection";
    public static readonly string Instructor_TeachingAssistantSectionAssociation = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantSectionAssociation";
    public static readonly string Instructor_TeachingAssistantTemplate = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantTemplate";

    public static readonly string Learner_ExternalLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#ExternalLearner";
    public static readonly string Learner_GuestLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#GuestLearner";
    public static readonly string Learner_Instructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#Instructor";
    public static readonly string Learner_Learner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#Learner";
    public static readonly string Learner_NonCreditLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#NonCreditLearner";

    public static readonly string Mentor_Advisor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Advisor";
    public static readonly string Mentor_Auditor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Auditor";
    public static readonly string Mentor_ExternalAdvisor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalAdvisor";
    public static readonly string Mentor_ExternalAuditor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalAuditor";
    public static readonly string Mentor_ExternalLearningFacilitator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalLearningFacilitator";
    public static readonly string Mentor_ExternalMentor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalMentor";
    public static readonly string Mentor_ExternalReviewer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalReviewer";
    public static readonly string Mentor_ExternalTutor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalTutor";
    public static readonly string Mentor_LearningFacilitator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#LearningFacilitator";
    public static readonly string Mentor_Mentor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Mentor";
    public static readonly string Mentor_Reviewer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Reviewer";
    public static readonly string Mentor_Tutor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Tutor";

    public static readonly string Manager_AreaManager = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#AreaManager";
    public static readonly string Manager_CourseCoordinator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#CourseCoordinator";
    public static readonly string Manager_ExternalObserver = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#ExternalObserver";
    public static readonly string Manager_Manager = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#Manager";
    public static readonly string Manager_Observer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#Observer";

    public static readonly string Member_Member = "http://purl.imsglobal.org/vocab/lis/v2/membership/Member#Member";

    public static readonly string Officer_Chair = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Chair";
    public static readonly string Officer_Communications = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Communications";
    public static readonly string Officer_Secretary = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Secretary";
    public static readonly string Officer_Treasurer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Treasurer";
    public static readonly string Officer_ViceChair = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Vice-Chair";
}
