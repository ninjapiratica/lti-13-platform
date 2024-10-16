using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Lti13Platform.Core.Constants
{
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

    public static class Lti13ActualUserVariables
    {
        /// <summary>
        /// user.id message property value; this may not be their real ID if they are masquerading as another user.
        /// </summary>
        public const string Id = "$ActualUser.id";

        /// <summary>
        /// user.image message property value.
        /// </summary>
        public const string Image = "$ActualUser.image";

        /// <summary>
        /// Username by which the message sender knows the user (typically, the name a user logs in with).
        /// </summary>
        public const string Username = "$ActualUser.username";

        /// <summary>
        /// One or more URIs describing the user's organizational properties (for example, an ldap:// URI).
        /// By best practice, message senders should separate multiple URIs by commas.
        /// </summary>
        public const string Org = "$ActualUser.org";

        /// <summary>
        /// role_scope_mentor message property value.
        /// </summary>
        public const string ScopeMentor = "$ActualUser.scope.mentor";

        /// <summary>
        /// A comma-separated list of grade(s) for which the user is enrolled.
        /// The permitted vocabulary is from the 'grades' field utilized in OneRoster Users.
        /// </summary>
        public const string GradeLevelsOneRoster = "$ActualUser.gradeLevels.oneRoster";
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

    public static class LisActualPersonVariables
    {
        /// <summary>
        /// XPath for value from LIS database: personRecord/sourcedId
        /// (lis_person.sourcedid property)
        /// </summary>
        public const string SourcedId = "$ActualPerson.sourcedId";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/formname/[formnameType/instanceValue/text="Full"]/formattedName/text
        /// (lis_person.name_full property)
        /// </summary>
        public const string NameFull = "$ActualPerson.name.full";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Family"]/instanceValue/text
        /// (lis_person.name_family property)
        /// </summary>
        public const string NameFamily = "$ActualPerson.name.family";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Given"]/instanceValue/text
        /// (lis_person.name_given property)
        /// </summary>
        public const string NameGiven = "$ActualPerson.name.given";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Middle"]/instanceValue/text
        /// </summary>
        public const string NameMiddle = "$ActualPerson.name.middle";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Prefix"]/instanceValue/text
        /// </summary>
        public const string NamePrefix = "$ActualPerson.name.prefix";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/name/partName[instanceName/text="Suffix"]/instanceValue/text
        /// </summary>
        public const string NameSuffix = "$ActualPerson.name.suffix";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/demographics/gender/instanceValue/text
        /// </summary>
        public const string Gender = "$ActualPerson.gender";

        /// <summary>
        /// No XPath available (N/A)
        /// </summary>
        public const string GenderPronouns = "$ActualPerson.gender.pronouns";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]/addressPart/nameValuePair/[instanceName/text="NonFieldedStreetAddress1"]/instanceValue/text
        /// </summary>
        public const string AddressStreet1 = "$ActualPerson.address.street1";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]/addressPart/nameValuePair[instanceName/text="NonFieldedStreetAddress2"]/instanceValue/text
        /// </summary>
        public const string AddressStreet2 = "$ActualPerson.address.street2";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="NonFieldedStreetAddress3"]/instanceValue/text
        /// </summary>
        public const string AddressStreet3 = "$ActualPerson.address.street3";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="NonFieldedStreetAddress4"]/instanceValue/
        /// </summary>
        public const string AddressStreet4 = "$ActualPerson.address.street4";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Locality"]/instanceValue/text
        /// </summary>
        public const string AddressLocality = "$ActualPerson.address.locality";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred "]addressPart/nameValuePair/[instanceName/text="Statepr"]/instanceValue/text
        /// </summary>
        public const string AddressStatepr = "$ActualPerson.address.statepr";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Country"]/instanceValue/text
        /// </summary>
        public const string AddressCountry = "$ActualPerson.address.country";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Postcode"]/instanceValue/text
        /// </summary>
        public const string AddressPostcode = "$ActualPerson.address.postcode";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/address/[addressType/instanceValue/text="Preferred"]addressPart/nameValuePair/[instanceName/text="Timezone"]/instanceValue/text
        /// </summary>
        public const string AddressTimezone = "$ActualPerson.address.timezone";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Mobile"]/contactInfoValue/text
        /// </summary>
        public const string PhoneMobile = "$ActualPerson.phone.mobile";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Telephone_Primary"]/contactinfoValue/text
        /// </summary>
        public const string PhonePrimary = "$ActualPerson.phone.primary";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo [contactinfoType/instanceValue/text="Telephone_Home"]/contactinfoValue/text
        /// </summary>
        public const string PhoneHome = "$ActualPerson.phone.home";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo [contactinfoType/instanceValue/text="Telephone_Work"]/contactinfoValue /text
        /// </summary>
        public const string PhoneWork = "$ActualPerson.phone.work";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Email_Primary"]/contactinfoValue/text
        /// (lis.person_contact_email_primary property)
        /// </summary>
        public const string EmailPrimary = "$ActualPerson.email.primary";

        /// <summary>
        /// XPath for value from LIS database: person/contactinfo[contactinfoType/instanceValue/text="Email_Personal"]/contactinfoValue/text
        /// </summary>
        public const string EmailPersonal = "$ActualPerson.email.personal";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="Web-Address"]/contactinfoValue/text
        /// </summary>
        public const string Webaddress = "$ActualPerson.webaddress";

        /// <summary>
        /// XPath for value from LIS database: personRecord/person/contactinfo[contactinfoType/instanceValue/text="SMS"]/contactinfoValue/text
        /// </summary>
        public const string Sms = "$ActualPerson.sms";
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
