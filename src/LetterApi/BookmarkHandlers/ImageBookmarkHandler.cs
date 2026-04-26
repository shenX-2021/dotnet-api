using Aspose.Words;
using Aspose.Words.Drawing;

namespace LetterApi.BookmarkHandlers;

/// <summary>
/// 根据 Bookmark 插入图片
/// Options:
///   - source: "base64" | "url" | "file"
///   - data: base64 编码的图片数据（source=base64 时）
///   - url: 图片 URL（source=url 时）
///   - path: 图片文件路径（source=file 时，相对于 TemplatesPath）
///   - width: 图片宽度（pt），可选
///   - height: 图片高度（pt），可选
/// </summary>
public class ImageBookmarkHandler : IBookmarkHandler
{
    public string Type => "image";

    public void Handle(Document doc, string bookmarkName, Dictionary<string, object> options)
    {
        var bookmark = doc.Range.Bookmarks[bookmarkName];
        if (bookmark == null) return;

        var source = options.GetValueOrDefault("source", "base64")?.ToString() ?? "base64";
        byte[] imageBytes;

        imageBytes = source switch
        {
            "base64" => Convert.FromBase64String(options["data"]?.ToString() ?? ""),
            "file" => File.ReadAllBytes(options["path"]?.ToString() ?? ""),
            _ => throw new ArgumentException($"Unsupported image source: {source}")
        };

        var builder = new DocumentBuilder(doc);
        builder.MoveToBookmark(bookmarkName);

        var shape = builder.InsertImage(imageBytes);

        if (options.TryGetValue("width", out var w) && double.TryParse(w?.ToString(), out var width))
            shape.Width = width;
        if (options.TryGetValue("height", out var h) && double.TryParse(h?.ToString(), out var height))
            shape.Height = height;

        bookmark.Text = "";
    }
}
