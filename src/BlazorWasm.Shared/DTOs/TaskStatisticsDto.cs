namespace BlazorWasm.Shared.DTOs;

public class TaskStatisticsDto
{
    public TaskStatusStatsDto StatusStats { get; set; } = new();
    public TaskPriorityStatsDto PriorityStats { get; set; } = new();
    public TaskOverviewStatsDto OverviewStats { get; set; } = new();
    public List<TaskCompletionTrendDto> CompletionTrend { get; set; } = new();
}

public class TaskStatusStatsDto
{
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Done { get; set; }
    public int Cancelled { get; set; }
}

public class TaskPriorityStatsDto
{
    public int Low { get; set; }
    public int Medium { get; set; }
    public int High { get; set; }
    public int Critical { get; set; }
}

public class TaskOverviewStatsDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TasksDueToday { get; set; }
    public int TasksDueThisWeek { get; set; }
    public double CompletionRate { get; set; }
}

public class TaskCompletionTrendDto
{
    public DateTime Date { get; set; }
    public int CompletedTasks { get; set; }
    public int CreatedTasks { get; set; }
}
