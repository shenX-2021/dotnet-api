namespace LetterApi.Models;

public enum TaskState
{
    Pending,
    Processing,
    Completed,
    Failed
}

/// <summary>
/// GET /letter/status/{taskId} 响应体
/// </summary>
public class TaskStatusResponse
{
    public string TaskId { get; set; } = string.Empty;
    public TaskState State { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DownloadUrl { get; set; }
}
