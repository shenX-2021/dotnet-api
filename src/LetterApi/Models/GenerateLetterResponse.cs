namespace LetterApi.Models;

/// <summary>
/// POST /letter/generate 响应体
/// </summary>
public class GenerateLetterResponse
{
    public string TaskId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
