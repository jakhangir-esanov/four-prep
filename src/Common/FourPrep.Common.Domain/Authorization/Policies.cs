namespace FourPrep.Common.Domain.Authorization;

public static class Policies
{
    public const string Student = "StudentOnly";
    public const string Parent = "ParentOnly";
    public const string StudentOrParent = "StudentOrParentOnly";
    public const string VerifiedEmail = "VerifiedEmailOnly";
    public const string Staff = "StaffOnly";
    public const string Mentor = "MentorOnly";
    public const string Teacher = "TeacherOnly";
    public const string MentorAdmin = "MentorAdminOnly";
    public const string ContentAdmin = "ContentAdminOnly";
    public const string SuperAdmin = "SuperAdminOnly";
}
