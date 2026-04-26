using System.Collections.Concurrent;
using LetterApi.Models;

namespace LetterApi.Services;

public interface ITaskManager
{
    /// <summary>创建新任务并返回 taskId</summary>
    string CreateTask();

    /// <summary>获取任务状态</summary>
    TaskStatusResponse GetStatus(string taskId);

    /// <summary>标记任务处理中</summary>
    void SetProcessing(string taskId);

    /// <summary>标记任务完成，保存结果文件</summary>
    void SetCompleted(string taskId, string fileName, byte[] data, OutputFormat format);

    /// <summary>标记任务失败</summary>
    void SetFailed(string taskId, string errorMessage);

    /// <summary>获取结果文件数据，未完成返回 null</summary>
    (byte[] Data, string ContentType, string FileName)? GetResult(string taskId);
}

public class InMemoryTaskManager : ITaskManager
{
    private readonly ConcurrentDictionary<string, TaskInfo> _tasks = new();
    private readonly string _outputPath;

    public InMemoryTaskManager(IConfiguration configuration)
    {
        _outputPath = configuration["LetterApi:OutputPath"] ?? "/app/output";
        Directory.CreateDirectory(_outputPath);
    }

    public string CreateTask()
    {
        var taskId = Guid.NewGuid().ToString("N");
        _tasks[taskId] = new TaskInfo { TaskId = taskId, State = TaskState.Pending, CreatedAt = DateTime.UtcNow };
        return taskId;
    }

    public TaskStatusResponse GetStatus(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var info))
            return new TaskStatusResponse { TaskId = taskId, State = TaskState.Failed, ErrorMessage = "Task not found" };

        var response = new TaskStatusResponse
        {
            TaskId = taskId,
            State = info.State,
            ErrorMessage = info.ErrorMessage
        };

        if (info.State == TaskState.Completed)
            response.DownloadUrl = $"/letter/download/{taskId}";

        return response;
    }

    public void SetProcessing(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var info))
            info.State = TaskState.Processing;
    }

    public void SetCompleted(string taskId, string fileName, byte[] data, OutputFormat format)
    {
        if (!_tasks.TryGetValue(taskId, out var info)) return;

        // 持久化到文件系统，避免大文件占用内存
        var ext = GetFileExtension(format);
        var outputFileName = $"{taskId}{ext}";
        var outputPath = Path.Combine(_outputPath, outputFileName);
        File.WriteAllBytes(outputPath, data);

        info.State = TaskState.Completed;
        info.OutputPath = outputPath;
        info.FileName = fileName;
        info.ContentType = GetContentType(format);
    }

    public void SetFailed(string taskId, string errorMessage)
    {
        if (_tasks.TryGetValue(taskId, out var info))
        {
            info.State = TaskState.Failed;
            info.ErrorMessage = errorMessage;
        }
    }

    public (byte[] Data, string ContentType, string FileName)? GetResult(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var info))
            return null;

        if (info.State != TaskState.Completed || info.OutputPath == null)
            return null;

        if (!File.Exists(info.OutputPath))
            return null;

        return (File.ReadAllBytes(info.OutputPath), info.ContentType!, info.FileName!);
    }

    private static string GetFileExtension(OutputFormat format) => format switch
    {
        OutputFormat.Pdf => ".pdf",
        OutputFormat.Png => ".png",
        OutputFormat.Html => ".html",
        OutputFormat.Docx => ".docx",
        _ => ".pdf"
    };

    private static string GetContentType(OutputFormat format) => format switch
    {
        OutputFormat.Pdf => "application/pdf",
        OutputFormat.Png => "image/png",
        OutputFormat.Html => "text/html",
        OutputFormat.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/pdf"
    };

    private class TaskInfo
    {
        public string TaskId { get; set; } = string.Empty;
        public TaskState State { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OutputPath { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
