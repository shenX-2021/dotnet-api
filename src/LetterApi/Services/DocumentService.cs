using Aspose.Words;
using Aspose.Words.Saving;
using LetterApi.BookmarkHandlers;
using LetterApi.Models;

namespace LetterApi.Services;

public interface IDocumentService
{
    /// <summary>
    /// 加载模板、执行 Mail Merge、处理 Bookmark 操作、转换输出格式
    /// </summary>
    byte[] ProcessDocument(GenerateLetterRequest request);
}

public class DocumentService : IDocumentService
{
    private readonly ILogger<DocumentService> _logger;
    private readonly IEnumerable<IBookmarkHandler> _handlers;
    private readonly string _templatesPath;

    public DocumentService(
        ILogger<DocumentService> logger,
        IEnumerable<IBookmarkHandler> handlers,
        IConfiguration configuration)
    {
        _logger = logger;
        _handlers = handlers;
        _templatesPath = configuration["LetterApi:TemplatesPath"]
            ?? throw new InvalidOperationException("LetterApi:TemplatesPath is not configured");
    }

    public byte[] ProcessDocument(GenerateLetterRequest request)
    {
        var templatePath = Path.Combine(_templatesPath, request.TemplateFileName);
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template not found: {request.TemplateFileName}", templatePath);

        var doc = new Document(templatePath);

        // 1. Mail Merge
        if (request.MergeFields.Count > 0)
        {
            doc.MailMerge.Execute(request.MergeFields.Keys.ToArray(), request.MergeFields.Values.ToArray());
        }

        // 2. 处理 Bookmark 操作
        foreach (var (bookmarkName, action) in request.Bookmarks)
        {
            var handler = _handlers.FirstOrDefault(h => h.Type.Equals(action.Type, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
            {
                _logger.LogWarning("No handler found for bookmark type '{Type}', bookmark '{Name}' skipped", action.Type, bookmarkName);
                continue;
            }

            try
            {
                handler.Handle(doc, bookmarkName, action.Options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling bookmark '{Name}' with type '{Type}'", bookmarkName, action.Type);
                throw;
            }
        }

        // 3. 转换输出
        using var ms = new MemoryStream();
        var saveOptions = GetSaveOptions(request.OutputFormat);
        doc.Save(ms, saveOptions);
        return ms.ToArray();
    }

    private static SaveOptions GetSaveOptions(OutputFormat format) => format switch
    {
        OutputFormat.Pdf => new PdfSaveOptions(),
        OutputFormat.Png => new ImageSaveOptions(SaveFormat.Png),
        OutputFormat.Html => new HtmlSaveOptions(),
        OutputFormat.Docx => new OoxmlSaveOptions(SaveFormat.Docx),
        _ => new PdfSaveOptions()
    };
}
