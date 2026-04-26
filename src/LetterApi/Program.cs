using Aspose.Words;
using LetterApi.BookmarkHandlers;
using LetterApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 注册控制器和服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 注册文档服务
builder.Services.AddSingleton<IDocumentService, DocumentService>();
builder.Services.AddSingleton<ITaskManager, InMemoryTaskManager>();

// 注册所有 Bookmark 处理器（策略模式 —— 新增处理器只需在此注册）
builder.Services.AddSingleton<IBookmarkHandler, ImageBookmarkHandler>();
builder.Services.AddSingleton<IBookmarkHandler, QrCodeBookmarkHandler>();

var app = builder.Build();

// 获取应用执行目录
var appBasePath = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

// 确保模板和输出目录存在
var templatesPath = Path.Combine(appBasePath, "templates");
var outputPath = Path.Combine(appBasePath, "output");
var licensePath = Path.Combine(appBasePath, "Aspose.Words.lic");
Directory.CreateDirectory(templatesPath);
Directory.CreateDirectory(outputPath);

// 初始化 Aspose License
InitializeAsposeLicense(licensePath);

app.MapControllers();
app.Run();

static void InitializeAsposeLicense(string licensePath)
{
    if (File.Exists(licensePath))
    {
        try
        {
            var license = new License();
            license.SetLicense(licensePath);
            Console.WriteLine($"Aspose license loaded from: {licensePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load Aspose license: {ex.Message} — running in evaluation mode");
        }
    }
    else
    {
        Console.WriteLine("Aspose license not found — running in evaluation mode");
    }
}
