using Aspose.Words;

namespace LetterApi.BookmarkHandlers;

/// <summary>
/// Bookmark 处理器接口 —— 策略模式核心
/// 实现此接口即可扩展新的 Bookmark 操作类型，无需修改已有代码
/// </summary>
public interface IBookmarkHandler
{
    /// <summary>
    /// 处理器类型标识，用于请求中的 BookmarkAction.Type 匹配
    /// </summary>
    string Type { get; }

    /// <summary>
    /// 处理指定 Bookmark 节点
    /// </summary>
    /// <param name="doc">当前文档对象</param>
    /// <param name="bookmarkName">Bookmark 名称</param>
    /// <param name="options">请求中传递的参数</param>
    void Handle(Document doc, string bookmarkName, Dictionary<string, object> options);
}
