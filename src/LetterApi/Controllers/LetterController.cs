using LetterApi.Models;
using LetterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LetterApi.Controllers;

[ApiController]
[Route("letter")]
public class LetterController : ControllerBase
{
    private readonly ILogger<LetterController> _logger;
    private readonly IDocumentService _documentService;
    private readonly ITaskManager _taskManager;

    public LetterController(
        ILogger<LetterController> logger,
        IDocumentService documentService,
        ITaskManager taskManager)
    {
        _logger = logger;
        _documentService = documentService;
        _taskManager = taskManager;
    }

    /// <summary>
    /// 异步生成文档：提交任务后返回 taskId，通过 /letter/status 轮询状态
    /// </summary>
    [HttpPost("generate")]
    public IActionResult Generate([FromBody] GenerateLetterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateFileName))
            return BadRequest(new { error = "templateFileName is required" });

        var taskId = _taskManager.CreateTask();

        // 后台执行文档处理
        _ = Task.Run(() =>
        {
            try
            {
                _taskManager.SetProcessing(taskId);
                var result = _documentService.ProcessDocument(request);
                var outputFileName = Path.GetFileNameWithoutExtension(request.TemplateFileName);
                _taskManager.SetCompleted(taskId, outputFileName, result, request.OutputFormat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task {TaskId} failed", taskId);
                _taskManager.SetFailed(taskId, ex.Message);
            }
        });

        return Accepted(new GenerateLetterResponse
        {
            TaskId = taskId,
            Message = "Document generation started"
        });
    }

    /// <summary>
    /// 查询任务状态
    /// </summary>
    [HttpGet("status/{taskId}")]
    public IActionResult Status(string taskId)
    {
        var status = _taskManager.GetStatus(taskId);
        if (status.State == TaskState.Failed && status.ErrorMessage == "Task not found")
            return NotFound(status);

        return Ok(status);
    }

    /// <summary>
    /// 下载已完成的文档
    /// </summary>
    [HttpGet("download/{taskId}")]
    public IActionResult Download(string taskId)
    {
        var result = _taskManager.GetResult(taskId);
        if (result == null)
            return NotFound(new { error = "Result not found or task not completed" });

        var (data, contentType, fileName) = result.Value;
        var ext = contentType switch
        {
            "application/pdf" => ".pdf",
            "image/png" => ".png",
            "text/html" => ".html",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            _ => ".pdf"
        };

        return File(data, contentType, $"{fileName}{ext}");
    }
}
