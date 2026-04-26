using Aspose.Words;
using QRCoder;

namespace LetterApi.BookmarkHandlers;

/// <summary>
/// 根据 Bookmark 插入二维码
/// Options:
///   - content: 二维码内容（必填）
///   - pixelsPerModule: 每模块像素数，默认 10
///   - width: 图片宽度（pt），可选
///   - height: 图片高度（pt），可选
/// </summary>
public class QrCodeBookmarkHandler : IBookmarkHandler
{
    public string Type => "qrcode";

    public void Handle(Document doc, string bookmarkName, Dictionary<string, object> options)
    {
        var bookmark = doc.Range.Bookmarks[bookmarkName];
        if (bookmark == null) return;

        var content = options["content"]?.ToString()
            ?? throw new ArgumentException("qrcode handler requires 'content' option");

        var pixelsPerModule = options.TryGetValue("pixelsPerModule", out var ppm)
            ? int.Parse(ppm?.ToString() ?? "10")
            : 10;

        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(pixelsPerModule);

        var builder = new DocumentBuilder(doc);
        builder.MoveToBookmark(bookmarkName);

        var shape = builder.InsertImage(qrBytes);

        if (options.TryGetValue("width", out var w) && double.TryParse(w?.ToString(), out var width))
            shape.Width = width;
        if (options.TryGetValue("height", out var h) && double.TryParse(h?.ToString(), out var height))
            shape.Height = height;

        bookmark.Text = "";
    }
}
