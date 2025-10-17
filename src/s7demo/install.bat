@echo off
echo ========================================
echo S7 PLC Demo 项目环境安装脚本
echo ========================================
echo.

echo 1. 检查 .NET 6 SDK 是否已安装...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET 6 SDK 未安装
    echo 请先下载并安装 .NET 6 SDK: https://dotnet.microsoft.com/download/dotnet/6.0
    echo 安装完成后重新运行此脚本
    pause
    exit /b 1
) else (
    echo ✅ .NET 6 SDK 已安装
    dotnet --version
)

echo.
echo 2. 还原项目依赖包...
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ 依赖包还原失败
    pause
    exit /b 1
) else (
    echo ✅ 依赖包还原成功
)

echo.
echo 3. 编译项目...
dotnet build
if %errorlevel% neq 0 (
    echo ❌ 项目编译失败
    pause
    exit /b 1
) else (
    echo ✅ 项目编译成功
)

echo.
echo 4. 检查配置文件...
if not exist "appsettings.json" (
    echo ❌ 配置文件 appsettings.json 不存在
    pause
    exit /b 1
) else (
    echo ✅ 配置文件存在
)

echo.
echo ========================================
echo 🎉 环境安装完成！
echo ========================================
echo.
echo 运行项目命令: dotnet run
echo 访问地址: http://localhost:5000
echo.
echo 请先配置 appsettings.json 中的PLC连接参数
echo 详细配置说明请查看: 配置说明.md
echo.
pause
