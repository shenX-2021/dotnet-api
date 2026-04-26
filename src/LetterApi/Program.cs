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

// 初始化 Aspose License
InitializeAsposeLicense(app.Configuration);

// 确保模板和输出目录存在
var templatesPath = app.Configuration["LetterApi:TemplatesPath"] ?? "/app/templates";
var outputPath = app.Configuration["LetterApi:OutputPath"] ?? "/app/output";
Directory.CreateDirectory(templatesPath);
Directory.CreateDirectory(outputPath);

app.MapControllers();
app.Run();

static void InitializeAsposeLicense(IConfiguration configuration)
{
    var licensePath = configuration["LetterApi:AsposeLicensePath"];
    if (!string.IsNullOrWhiteSpace(licensePath) && File.Exists(licensePath))
    {
        var license = new License();
        license.SetLicense(licensePath);
        Console.WriteLine($"Aspose license loaded from: {licensePath}");
    }
    else
    {
        Console.WriteLine("Aspose license not configured — running in evaluation mode");
    }
}
