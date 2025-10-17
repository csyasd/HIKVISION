#!/bin/bash

echo "========================================"
echo "S7 PLC Demo 项目环境安装脚本"
echo "========================================"
echo

echo "1. 检查 .NET 6 SDK 是否已安装..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 6 SDK 未安装"
    echo "请先下载并安装 .NET 6 SDK: https://dotnet.microsoft.com/download/dotnet/6.0"
    echo "安装完成后重新运行此脚本"
    exit 1
else
    echo "✅ .NET 6 SDK 已安装"
    dotnet --version
fi

echo
echo "2. 还原项目依赖包..."
if ! dotnet restore; then
    echo "❌ 依赖包还原失败"
    exit 1
else
    echo "✅ 依赖包还原成功"
fi

echo
echo "3. 编译项目..."
if ! dotnet build; then
    echo "❌ 项目编译失败"
    exit 1
else
    echo "✅ 项目编译成功"
fi

echo
echo "4. 检查配置文件..."
if [ ! -f "appsettings.json" ]; then
    echo "❌ 配置文件 appsettings.json 不存在"
    exit 1
else
    echo "✅ 配置文件存在"
fi

echo
echo "========================================"
echo "🎉 环境安装完成！"
echo "========================================"
echo
echo "运行项目命令: dotnet run"
echo "访问地址: http://localhost:5000"
echo
echo "请先配置 appsettings.json 中的PLC连接参数"
echo "详细配置说明请查看: 配置说明.md"
echo
