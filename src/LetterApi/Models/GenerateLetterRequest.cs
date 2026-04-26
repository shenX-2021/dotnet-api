using System.Text.Json.Serialization;

namespace LetterApi.Models;

/// <summary>
/// POST /letter/generate 请求体
/// </summary>
public class GenerateLetterRequest
{
    /// <summary>
    /// 模板文件名（位于 TemplatesPath 目录下）
    /// </summary>
    public string TemplateFileName { get; set; } = string.Empty;

    /// <summary>
    /// Mail Merge 字段键值对
    /// </summary>
    public Dictionary<string, string> MergeFields { get; set; } = new();

    /// <summary>
    /// Bookmark 操作配置：key 为 Bookmark 名称，value 为操作定义
    /// </summary>
    public Dictionary<string, BookmarkAction> Bookmarks { get; set; } = new();

    /// <summary>
    /// 输出格式，默认 pdf
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Pdf;
}

/// <summary>
/// 单个 Bookmark 的操作定义
/// </summary>
public class BookmarkAction
{
    /// <summary>
    /// 处理器类型标识，对应注册的 IBookmarkHandler（如 "image"、"qrcode"）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 传递给处理器的参数，不同处理器所需参数不同
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new();
}

public enum OutputFormat
{
    Pdf,
    Png,
    Html,
    Docx
}
