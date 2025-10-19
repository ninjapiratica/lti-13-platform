namespace NP.Lti13Platform.Core.Constants;

/// <summary>
/// Defines the LTI 1.3 system roles.
/// </summary>
public static class Lti13SystemRoles
{
    // Core Roles

    /// <summary>
    /// Administrator system role.
    /// </summary>
    public static readonly string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/system/person#Administrator";
    /// <summary>
    /// None system role.
    /// </summary>
    public static readonly string None = "http://purl.imsglobal.org/vocab/lis/v2/system/person#None";

    // Non-Core Roles

    /// <summary>
    /// AccountAdmin system role.
    /// </summary>
    public static readonly string AccountAdmin = "http://purl.imsglobal.org/vocab/lis/v2/system/person#AccountAdmin";
    /// <summary>
    /// Creator system role.
    /// </summary>
    public static readonly string Creator = "http://purl.imsglobal.org/vocab/lis/v2/system/person#Creator";
    /// <summary>
    /// SysAdmin system role.
    /// </summary>
    public static readonly string SysAdmin = "http://purl.imsglobal.org/vocab/lis/v2/system/person#SysAdmin";
    /// <summary>
    /// SysSupport system role.
    /// </summary>
    public static readonly string SysSupport = "http://purl.imsglobal.org/vocab/lis/v2/system/person#SysSupport";
    /// <summary>
    /// User system role.
    /// </summary>
    public static readonly string User = "http://purl.imsglobal.org/vocab/lis/v2/system/person#User";

    // LTI Launch Only

    /// <summary>
    /// TestUser system role. Should be used only for LTI launches and only in conjunction with a 'real' role.
    /// <see href="https://www.imsglobal.org/spec/lti/v1p3/#lti-vocabulary-for-system-roles"/>.
    /// </summary>
    public static readonly string TestUser = "http://purl.imsglobal.org/vocab/lti/system/person#TestUser";
}

/// <summary>
/// Defines the LTI 1.3 institution roles.
/// </summary>
public static class Lti13InstitutionRoles
{
    // Core Roles

    /// <summary>
    /// Administrator institution role.
    /// </summary>
    public static readonly string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Administrator";
    /// <summary>
    /// Faculty institution role.
    /// </summary>
    public static readonly string Faculty = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Faculty";
    /// <summary>
    /// Guest institution role.
    /// </summary>
    public static readonly string Guest = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Guest";
    /// <summary>
    /// None institution role.
    /// </summary>
    public static readonly string None = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#None";
    /// <summary>
    /// Other institution role.
    /// </summary>
    public static readonly string Other = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Other";
    /// <summary>
    /// Staff institution role.
    /// </summary>
    public static readonly string Staff = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Staff";
    /// <summary>
    /// Student institution role.
    /// </summary>
    public static readonly string Student = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Student";

    // Non-Core Roles

    /// <summary>
    /// Alumni institution role.
    /// </summary>
    public static readonly string Alumni = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Alumni";
    /// <summary>
    /// Instructor institution role.
    /// </summary>
    public static readonly string Instructor = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Instructor";
    /// <summary>
    /// Learner institution role.
    /// </summary>
    public static readonly string Learner = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Learner";
    /// <summary>
    /// Member institution role.
    /// </summary>
    public static readonly string Member = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Member";
    /// <summary>
    /// Mentor institution role.
    /// </summary>
    public static readonly string Mentor = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Mentor";
    /// <summary>
    /// Observer institution role.
    /// </summary>
    public static readonly string Observer = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#Observer";
    /// <summary>
    /// ProspectiveStudent institution role.
    /// </summary>
    public static readonly string ProspectiveStudent = "http://purl.imsglobal.org/vocab/lis/v2/institution/person#ProspectiveStudent";
}

/// <summary>
/// Defines the LTI 1.3 context roles.
/// </summary>
public static class Lti13ContextRoles
{
    // Core Roles

    /// <summary>
    /// Administrator context role.
    /// </summary>
    public static readonly string Administrator = "http://purl.imsglobal.org/vocab/lis/v2/membership#Administrator";
    /// <summary>
    /// ContentDeveloper context role.
    /// </summary>
    public static readonly string ContentDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership#ContentDeveloper";
    /// <summary>
    /// Instructor context role.
    /// </summary>
    public static readonly string Instructor = "http://purl.imsglobal.org/vocab/lis/v2/membership#Instructor";
    /// <summary>
    /// Learner context role.
    /// </summary>
    public static readonly string Learner = "http://purl.imsglobal.org/vocab/lis/v2/membership#Learner";
    /// <summary>
    /// Mentor context role.
    /// </summary>
    public static readonly string Mentor = "http://purl.imsglobal.org/vocab/lis/v2/membership#Mentor";

    // Non-Core Roles

    /// <summary>
    /// Manager context role.
    /// </summary>
    public static readonly string Manager = "http://purl.imsglobal.org/vocab/lis/v2/membership#Manager";
    /// <summary>
    /// Member context role.
    /// </summary>
    public static readonly string Member = "http://purl.imsglobal.org/vocab/lis/v2/membership#Member";
    /// <summary>
    /// Officer context role.
    /// </summary>
    public static readonly string Officer = "http://purl.imsglobal.org/vocab/lis/v2/membership#Officer";
}

/// <summary>
/// Defines the LTI 1.3 context sub-roles.
/// Whenever a platform specifies a sub-role, by best practice it should also include the associated principal role.
/// <see href="https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles"/>
/// </summary>
public static class Lti13ContextSubRoles
{
    /// <summary>
    /// Administrator sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_Administrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Administrator";
    /// <summary>
    /// Developer sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_Developer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Developer";
    /// <summary>
    /// ExternalDeveloper sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_ExternalDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalDeveloper";
    /// <summary>
    /// ExternalSupport sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_ExternalSupport = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalSupport";
    /// <summary>
    /// ExternalSystemAdministrator sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_ExternalSystemAdministrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#ExternalSystemAdministrator";
    /// <summary>
    /// Support sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_Support = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#Support";
    /// <summary>
    /// SystemAdministrator sub-role of Administrator.
    /// </summary>
    public static readonly string Administrator_SystemAdministrator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Administrator#SystemAdministrator";

    /// <summary>
    /// ContentDeveloper sub-role of ContentDeveloper.
    /// </summary>
    public static readonly string ContentDeveloper_ContentDeveloper = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ContentDeveloper";
    /// <summary>
    /// ContentExpert sub-role of ContentDeveloper.
    /// </summary>
    public static readonly string ContentDeveloper_ContentExpert = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ContentExpert";
    /// <summary>
    /// ExternalContentExpert sub-role of ContentDeveloper.
    /// </summary>
    public static readonly string ContentDeveloper_ExternalContentExpert = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#ExternalContentExpert";
    /// <summary>
    /// Librarian sub-role of ContentDeveloper.
    /// </summary>
    public static readonly string ContentDeveloper_Librarian = "http://purl.imsglobal.org/vocab/lis/v2/membership/ContentDeveloper#Librarian";

    /// <summary>
    /// ExternalInstructor sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_ExternalInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#ExternalInstructor";
    /// <summary>
    /// Grader sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_Grader = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#Grader";
    /// <summary>
    /// GuestInstructor sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_GuestInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#GuestInstructor";
    /// <summary>
    /// Lecturer sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_Lecturer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#Lecturer";
    /// <summary>
    /// PrimaryInstructor sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_PrimaryInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#PrimaryInstructor";
    /// <summary>
    /// SecondaryInstructor sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_SecondaryInstructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#SecondaryInstructor";
    /// <summary>
    /// TeachingAssistant sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_TeachingAssistant = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistant";
    /// <summary>
    /// TeachingAssistantGroup sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_TeachingAssistantGroup = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantGroup";
    /// <summary>
    /// TeachingAssistantOffering sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_TeachingAssistantOffering = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantOffering";
    /// <summary>
    /// TeachingAssistantSection sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_TeachingAssistantSection = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantSection";
    /// <summary>
    /// TeachingAssistantSectionAssociation sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_TeachingAssistantSectionAssociation = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantSectionAssociation";
    /// <summary>
    /// TeachingAssistantTemplate sub-role of Instructor.
    /// </summary>
    public static readonly string Instructor_TeachingAssistantTemplate = "http://purl.imsglobal.org/vocab/lis/v2/membership/Instructor#TeachingAssistantTemplate";

    /// <summary>
    /// ExternalLearner sub-role of Learner.
    /// </summary>
    public static readonly string Learner_ExternalLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#ExternalLearner";
    /// <summary>
    /// GuestLearner sub-role of Learner.
    /// </summary>
    public static readonly string Learner_GuestLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#GuestLearner";
    /// <summary>
    /// Instructor sub-role of Learner.
    /// </summary>
    public static readonly string Learner_Instructor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#Instructor";
    /// <summary>
    /// Learner sub-role of Learner.
    /// </summary>
    public static readonly string Learner_Learner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#Learner";
    /// <summary>
    /// NonCreditLearner sub-role of Learner.
    /// </summary>
    public static readonly string Learner_NonCreditLearner = "http://purl.imsglobal.org/vocab/lis/v2/membership/Learner#NonCreditLearner";

    /// <summary>
    /// Advisor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_Advisor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Advisor";
    /// <summary>
    /// Auditor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_Auditor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Auditor";
    /// <summary>
    /// ExternalAdvisor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_ExternalAdvisor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalAdvisor";
    /// <summary>
    /// ExternalAuditor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_ExternalAuditor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalAuditor";
    /// <summary>
    /// ExternalLearningFacilitator sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_ExternalLearningFacilitator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalLearningFacilitator";
    /// <summary>
    /// ExternalMentor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_ExternalMentor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalMentor";
    /// <summary>
    /// ExternalReviewer sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_ExternalReviewer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalReviewer";
    /// <summary>
    /// ExternalTutor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_ExternalTutor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#ExternalTutor";
    /// <summary>
    /// LearningFacilitator sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_LearningFacilitator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#LearningFacilitator";
    /// <summary>
    /// Mentor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_Mentor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Mentor";
    /// <summary>
    /// Reviewer sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_Reviewer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Reviewer";
    /// <summary>
    /// Tutor sub-role of Mentor.
    /// </summary>
    public static readonly string Mentor_Tutor = "http://purl.imsglobal.org/vocab/lis/v2/membership/Mentor#Tutor";

    /// <summary>
    /// AreaManager sub-role of Manager.
    /// </summary>
    public static readonly string Manager_AreaManager = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#AreaManager";
    /// <summary>
    /// CourseCoordinator sub-role of Manager.
    /// </summary>
    public static readonly string Manager_CourseCoordinator = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#CourseCoordinator";
    /// <summary>
    /// ExternalObserver sub-role of Manager.
    /// </summary>
    public static readonly string Manager_ExternalObserver = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#ExternalObserver";
    /// <summary>
    /// Manager sub-role of Manager.
    /// </summary>
    public static readonly string Manager_Manager = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#Manager";
    /// <summary>
    /// Observer sub-role of Manager.
    /// </summary>
    public static readonly string Manager_Observer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Manager#Observer";

    /// <summary>
    /// Member sub-role of Member.
    /// </summary>
    public static readonly string Member_Member = "http://purl.imsglobal.org/vocab/lis/v2/membership/Member#Member";

    /// <summary>
    /// Chair sub-role of Officer.
    /// </summary>
    public static readonly string Officer_Chair = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Chair";
    /// <summary>
    /// Communications sub-role of Officer.
    /// </summary>
    public static readonly string Officer_Communications = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Communications";
    /// <summary>
    /// Secretary sub-role of Officer.
    /// </summary>
    public static readonly string Officer_Secretary = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Secretary";
    /// <summary>
    /// Treasurer sub-role of Officer.
    /// </summary>
    public static readonly string Officer_Treasurer = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Treasurer";
    /// <summary>
    /// Vice-Chair sub-role of Officer.
    /// </summary>
    public static readonly string Officer_ViceChair = "http://purl.imsglobal.org/vocab/lis/v2/membership/Officer#Vice-Chair";
}