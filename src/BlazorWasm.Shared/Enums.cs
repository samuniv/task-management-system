namespace BlazorWasm.Shared.Enums;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Review = 2,
    Done = 3,
    Cancelled = 4
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
