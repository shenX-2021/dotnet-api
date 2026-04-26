FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/LetterApi/LetterApi.csproj LetterApi/
RUN dotnet restore LetterApi/LetterApi.csproj

COPY src/LetterApi/ LetterApi/
RUN dotnet publish LetterApi/LetterApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 安装 Aspose.Words 在 Linux 上所需的字体
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    fonts-dejavu-core \
    fonts-noto-cjk \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# 创建模板和输出目录
RUN mkdir -p /app/templates /app/output

# 暴露端口
EXPOSE 3000

# License 文件通过卷挂载，路径通过环境变量配置
ENV ASPNETCORE_URLS=http://+:3000
ENV LetterApi__TemplatesPath=/app/templates
ENV LetterApi__OutputPath=/app/output
ENV LetterApi__AsposeLicensePath=/app/license/Aspose.Words.lic

ENTRYPOINT ["dotnet", "LetterApi.dll"]
