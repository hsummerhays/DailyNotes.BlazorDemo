namespace DailyNotes.Shared;

public static class DomainConstants
{
    public static class TaskStatus
    {
        public const string Todo = "todo";
        public const string InProgress = "in-progress";
        public const string Done = "done";
    }

    public static class Visibility
    {
        public const string Private = "private";
        public const string Public = "public";
    }

    public static class Proficiency
    {
        public const string Learning = "learning";
        public const string Beginner = "beginner";
        public const string Novice = "novice";
        public const string Intermediate = "intermediate";
        public const string Advanced = "advanced";
        public const string Expert = "expert";
    }

    public static class Role
    {
        public const string Admin = "admin";
    }
}
