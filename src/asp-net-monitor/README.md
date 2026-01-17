# FeatBit .NET Server Monitor - ASP.NET Core API

一个集成了 FeatBit .NET Server SDK 的 ASP.NET Core Web API 项目。

## 功能特性

- ✅ 完整的 FeatBit SDK 集成与依赖注入
- ✅ 支持所有功能标志类型（布尔、字符串、整数、浮点数、JSON）
- ✅ RESTful API 端点
- ✅ **Scalar UI** 文档（.NET 10 最佳实践，替代 Swagger）
- ✅ OpenAPI 3.0 支持
- ✅ XML 文档注释自动集成
- ✅ 事件追踪支持
- ✅ 配置文件管理（appsettings.json）

## 快速开始

### 1. 配置 FeatBit

编辑 `appsettings.json` 文件，设置你的 FeatBit 环境密钥：

```json
{
  "FeatBit": {
    "EnvSecret": "你的环境密钥",
    "StreamingUri": "wss://app-eval.featbit.co",
    "EventUri": "https://app-eval.featbit.co",
    "StartWaitTimeSeconds": 3,
    "DisableEvents": false
  }
}
```

### 2. 还原依赖并运行

```bash
dotnet restore
dotnet run
```

应用将启动在 `https://localhost:5001` (或 `http://localhost:5000`)

### 3. 访问 Scalar UI

打开浏览器访问：`https://localhost:5001/scalar/v1`

> **注意**：根据 .NET 10 最佳实践，我们使用 **Scalar** 而不是 Swagger 作为 API 文档界面。Scalar 提供了更现代、更美观的交互式文档体验。

## API 端点

### 1. 获取用户功能配置
```
GET /api/features/user-config?userId=user123
```

返回用户的所有功能标志配置（布尔、字符串、整数、浮点数、JSON）。

**示例响应：**
```json
{
  "userKey": "user123",
  "features": {
    "newUIEnabled": true,
    "theme": "dark",
    "maxItems": 20,
    "discountRate": 0.15,
    "config": "{\"key\":\"value\"}"
  }
}
```

### 2. 检查单个功能标志
```
GET /api/features/check/{flagKey}?userId=user123
```

检查特定功能标志是否启用。

**示例响应：**
```json
{
  "flagKey": "new-feature",
  "userKey": "user123",
  "isEnabled": true,
  "timestamp": "2026-01-17T10:30:00Z"
}
```

### 3. 获取功能标志详细信息
```
GET /api/features/feature-detail/{flagKey}?userId=user123
```

获取功能标志的详细评估信息。

**示例响应：**
```json
{
  "flagKey": "new-feature",
  "userKey": "user123",
  "value": true,
  "reason": "TargetMatch",
  "kind": "BoolVariation"
}
```

### 4. 追踪自定义事件
```
POST /api/features/track-event
Content-Type: application/json

{
  "userId": "user123",
  "eventName": "purchase-completed",
  "numericValue": 99.99
}
```

用于 A/B 测试的事件追踪。

### 5. 检查 SDK 状态
```
GET /api/features/status
```

检查 FeatBit SDK 的初始化状态。

## 项目结构

```
asp-net-monitor/
├── Controllers/
│   └── FeaturesController.cs    # API 控制器
├── Program.cs                     # 应用入口点
├── DotNetServerMonitor.csproj    # 项目文件
├── appsettings.json              # 生产配置
└── appsettings.Development.json  # 开发配置
```

## 高级用法

### 创建带自定义属性的用户

```csharp
var user = FbUser.Builder("user-123")
    .Name("张三")
    .Custom("role", "admin")
    .Custom("subscription", "premium")
    .Custom("country", "CN")
    .Build();

var isEnabled = _fbClient.BoolVariation("premium-feature", user, defaultValue: false);
```

### 追踪转化事件

```csharp
// 追踪带数值的事件（例如收入）
_fbClient.Track(user, "purchase-completed", numericValue: 99.99);

// 追踪简单事件
_fbClient.Track(user, "button-clicked");
```

### 获取评估详情

```csharp
var detail = _fbClient.BoolVariationDetail("flag-key", user, defaultValue: false);

Console.WriteLine($"值: {detail.Value}");
Console.WriteLine($"原因: {detail.Reason}");
Console.WriteLine($"类型: {detail.Kind}");
```

## 环境变量配置

你也可以使用环境变量来配置 FeatBit：

```bash
export FeatBit__EnvSecret="你的环境密钥"
export FeatBit__StreamingUri="wss://app-eval.featbit.co"
export FeatBit__EventUri="https://app-eval.featbit.co"
```

## 依赖项

- .NET 8.0
- FeatBit.ServerSdk 1.2.9
- Microsoft.AspNetCore.OpenApi 8.0.0
- Scalar.AspNetCore 1.2.42 (现代化的 OpenAPI UI，替代 Swagger)

## 更多资源

- [FeatBit 官方文档](https://docs.featbit.co)
- [FeatBit .NET SDK GitHub](https://github.com/featbit/featbit-dotnet-sdk)
- [FeatBit 云平台](https://app.featbit.co)

## 许可证

MIT
