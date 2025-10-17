# S7 PLC 数据监控系统

这是一个基于 ASP.NET Core 6 和 S7 协议的 PLC 数据读取演示项目。

## 功能特性

- ✅ **S7 协议通信**：支持通过以太网 TCP/IP 连接西门子 S7 系列 PLC
- ✅ **GPS 数据读取**：读取 GPS 经度、纬度和设备 ID
- ✅ **实时监控**：Web 界面实时显示 PLC 数据
- ✅ **连接管理**：支持连接/断开 PLC
- ✅ **手动读取**：支持手动读取指定 DB 块的 Real 和 Int 值
- ✅ **现代化 UI**：使用 Bootstrap 5 的响应式界面

## 项目结构

```
S7Demo/
├── Controllers/
│   ├── HomeController.cs          # 主页控制器
│   └── PlcController.cs           # PLC API控制器
├── Models/
│   └── DataModels.cs              # 数据模型
├── Services/
│   └── S7PlcService.cs            # S7通信服务
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml           # 主页面
│   │   ├── Privacy.cshtml         # 隐私政策
│   │   └── Error.cshtml           # 错误页面
│   └── Shared/
│       └── _Layout.cshtml         # 布局模板
├── Program.cs                      # 程序入口
├── appsettings.json               # 配置文件
└── S7Demo.csproj                  # 项目文件
```

## 配置说明

### PLC 连接配置 (appsettings.json)

```json
{
  "PlcConfig": {
    "IpAddress": "192.168.0.1", // PLC IP地址
    "Rack": 0, // 机架号
    "Slot": 1, // 槽位号 (S7-1200/1500通常是1)
    "DataBlockNumber": 1, // 数据块编号
    "TimeoutMs": 5000 // 连接超时时间(毫秒)
  }
}
```

### 数据块布局

根据您的图片示例，假设 DB1 的数据布局如下：

| 偏移 | 变量名    | 数据类型 | 描述     |
| ---- | --------- | -------- | -------- |
| DBD0 | GPS_lon   | Real     | GPS 经度 |
| DBD4 | Gps_lat   | Real     | GPS 纬度 |
| DBW8 | Device_ID | Int      | 设备 ID  |

## 运行项目

### 新电脑环境要求

#### 必需环境

- **.NET 6 SDK**: [下载地址](https://dotnet.microsoft.com/download/dotnet/6.0)
- **操作系统**: Windows 10/11, Linux, 或 macOS
- **内存**: 至少 4GB RAM
- **磁盘空间**: 至少 2GB 可用空间

#### 可选工具

- **Visual Studio 2022** 或 **Visual Studio Code**: 用于代码编辑
- **Postman**: 用于 API 测试
- **PLC 模拟器**: 用于测试 S7 协议通信

### 快速安装

#### Windows 用户

```bash
# 1. 安装 .NET 6 SDK (从官网下载)
# 2. 运行安装脚本
install.bat
```

#### Linux/macOS 用户

```bash
# 1. 安装 .NET 6 SDK
# 2. 运行安装脚本
chmod +x install.sh
./install.sh
```

#### 手动安装步骤

### 1. 安装依赖

```bash
dotnet restore
```

### 2. 配置 PLC 连接

编辑 `appsettings.json` 文件，设置正确的 PLC IP 地址和其他参数。

> 📖 **详细配置说明**: 请参考 [配置说明.md](./配置说明.md) 文件，其中包含了所有配置参数的详细说明和常见问题解决方案。

### 3. 运行项目

```bash
dotnet run
```

### 4. 访问应用

打开浏览器访问：`https://localhost:5001` 或 `http://localhost:5000`

## API 接口

### 获取 GPS 数据

```
GET /api/plc/gps
```

### 检查连接状态

```
GET /api/plc/status
```

### 连接 PLC

```
POST /api/plc/connect
```

### 断开 PLC 连接

```
POST /api/plc/disconnect
```

### 读取 Real 值

```
GET /api/plc/read/real/{dbNumber}/{startByte}
```

### 读取 Int 值

```
GET /api/plc/read/int/{dbNumber}/{startByte}
```

### 写入 Real 值

```
POST /api/plc/write/real/{dbNumber}/{startByte}
Body: {value}
```

## 使用说明

1. **连接 PLC**：点击"连接"按钮连接到 PLC
2. **查看数据**：连接成功后，GPS 数据会自动显示
3. **手动读取**：使用"手动读取测试"区域读取特定 DB 块的值
4. **刷新数据**：点击"刷新数据"按钮更新最新数据

## 支持的 PLC 型号

- S7-200
- S7-300
- S7-400
- S7-1200
- S7-1500

## 技术栈

- **后端**：ASP.NET Core 6
- **S7 通信**：S7netplus 库
- **前端**：Bootstrap 5 + JavaScript
- **图标**：Font Awesome 6

## 注意事项

1. 确保 PLC 和运行应用的计算机在同一网络中
2. 检查 PLC 的 IP 地址和端口设置
3. 确认 PLC 的数据块布局与代码中的偏移量匹配
4. 某些 PLC 可能需要特殊的网络配置

## 故障排除

### 连接失败

- 检查 PLC IP 地址是否正确
- 确认网络连通性（ping PLC IP）
- 检查 PLC 的机架号和槽位号设置

### 数据读取失败

- 确认数据块编号和偏移量正确
- 检查数据类型是否匹配
- 确认 PLC 中确实存在对应的数据

## 许可证

本项目仅用于学习和演示目的。
