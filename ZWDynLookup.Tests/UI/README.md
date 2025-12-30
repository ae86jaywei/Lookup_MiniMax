---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 30450220557a96bf6d69d4bb82f3de42eba78b2c31844687ad81208054d6eef4a322073d022100e7dca6dd0f0127c0fa3253097c90dd1096b873d97e23c8254edd17f2185b9f0a
    ReservedCode2: 30460221008529152e7be14b0dd91ee3e31c63fa8f0140c1a14ba26ae2c4097171a6effa81022100d9cbe46fa16b615784fcb915e6db2eb913d7cc8df4e9b39e1118c610d4dfcfdf
---

# UI自动化测试说明

## 概述

ZWDynLookup插件的UI自动化测试套件使用White框架和Microsoft Test Framework构建，专注于验证所有用户界面组件的交互逻辑和功能完整性。

## 项目结构

```
ZWDynLookup.Tests/UI/
├── UIAutomationHelpers.cs              # UI自动化辅助类
├── GlobalTestSettings.cs              # 全局测试设置
├── UITests.cs                         # UI测试主类
├── DialogTests/                       # 对话框测试
│   ├── LookupTableDialogTests.cs      # 查寻表管理对话框测试
│   ├── AddParameterPropertyDialogTests.cs # 添加参数特性对话框测试
│   ├── LookupTableEditorTests.cs      # 查寻表编辑器测试
│   ├── RuntimeLookupMenuTests.cs      # 运行时查寻菜单测试
│   └── LookupContextMenuTests.cs      # 查寻上下文菜单测试
└── TestScreenshots/                   # 测试快照
    └── expected_ui_snapshots.md       # UI快照说明文档
```

## 测试覆盖范围

### 1. 查寻表管理对话框 (LookupTableDialogTests)
- ✅ 对话框正常打开和关闭
- ✅ 必需UI元素验证
- ✅ 添加、编辑、删除按钮功能
- ✅ 数据网格基本功能
- ✅ 行选择和操作
- ✅ 键盘导航支持
- ✅ 数据验证逻辑
- ✅ 截图功能测试

### 2. 添加参数特性对话框 (AddParameterPropertyDialogTests)
- ✅ 对话框显示和布局
- ✅ 表单输入验证
- ✅ 特性类型下拉框功能
- ✅ 特性名称和值输入
- ✅ 描述输入功能
- ✅ 确定/取消按钮操作
- ✅ 特殊字符处理
- ✅ 长文本输入支持

### 3. 查寻表编辑器 (LookupTableEditorTests)
- ✅ 编辑器打开和布局
- ✅ 数据网格编辑功能
- ✅ 添加/删除行操作
- ✅ 单元格编辑验证
- ✅ 保存/取消操作
- ✅ 数据验证和处理
- ✅ 多行编辑支持
- ✅ 上下文菜单功能

### 4. 运行时查寻菜单 (RuntimeLookupMenuTests)
- ✅ 菜单显示和定位
- ✅ 查寻输入功能
- ✅ 实时查寻结果
- ✅ 结果选择和确认
- ✅ 键盘导航支持
- ✅ 无结果状态处理
- ✅ 快捷键提示显示
- ✅ 菜单响应性测试

### 5. 查寻上下文菜单 (LookupContextMenuTests)
- ✅ 上下文菜单显示
- ✅ 菜单项功能和状态
- ✅ 导航和选择操作
- ✅ 分隔线和布局
- ✅ 快捷键显示验证
- ✅ 悬停效果测试
- ✅ 多级菜单支持
- ✅ 菜单项状态管理

## 环境要求

### 软件依赖
- Visual Studio 2019/2022 或 JetBrains Rider
- .NET Framework 4.7.2 或更高版本
- 中望CAD 2020 或更高版本
- White框架 (TestStack.White)
- MSTest Framework

### 中望CAD环境
- 必须安装并运行中望CAD
- 插件已正确加载
- 具有必要的权限访问CAD窗口

## 测试执行

### 1. 环境准备
```bash
# 启动中望CAD
# 加载ZWDynLookup插件
# 确保插件功能正常工作
```

### 2. 运行UI测试
```bash
# 运行所有UI测试
dotnet test --filter Category=UI

# 运行特定对话框测试
dotnet test --filter "Category=DialogTests"

# 运行快照测试
dotnet test --filter Category=UI_Snapshot

# 运行性能测试
dotnet test --filter Category=UI_Performance
```

### 3. 生成测试报告
```bash
# 生成详细测试报告
dotnet test --logger:"trx;LogFileName=UI_Test_Results.trx"

# 生成覆盖率报告
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## 测试配置

### 测试超时设置
```csharp
// 在GlobalTestSettings.cs中配置
public const int DefaultTimeoutSeconds = 30;
```

### 截图设置
```csharp
// 截图自动保存到指定目录
public const string ScreenshotDirectory = "TestScreenshots";
```

### 中望CAD进程配置
```csharp
// 在UIAutomationHelpers.cs中配置
private static readonly string[] CadProcessNames = { "zwcad", "ZWCAD", "zwcadpro" };
```

## 常见问题解决

### 1. 测试环境初始化失败
**症状**: `InvalidOperationException: 未找到中望CAD进程`
**解决方案**:
- 确保中望CAD正在运行
- 检查进程名称配置 (zwcad, ZWCAD, zwcadpro)
- 验证插件已正确加载
- 以管理员权限运行测试

### 2. UI元素识别失败
**症状**: `ElementNotFoundException: 未找到指定元素`
**解决方案**:
- 检查AutomationId设置
- 验证元素可见性
- 增加等待时间
- 检查UI布局是否改变

### 3. 对话框无法打开
**症状**: `TimeoutException: 等待对话框超时`
**解决方案**:
- 验证CAD命令路径
- 检查权限设置
- 手动测试对话框打开
- 调整超时时间

### 4. 截图功能异常
**症状**: 截图文件为空或生成失败
**解决方案**:
- 检查文件系统权限
- 验证截图目录存在
- 检查显示器配置
- 更新截图工具

## 测试最佳实践

### 1. 测试编写规范
- 使用描述性的测试方法名称
- 每个测试方法只测试一个功能点
- 包含适当的注释和文档
- 使用适当的断言和验证

### 2. 稳定性考虑
- 添加适当的等待时间
- 处理异步操作
- 清理测试资源
- 避免测试间的依赖关系

### 3. 性能优化
- 复用测试应用程序实例
- 合理设置超时时间
- 避免不必要的操作
- 使用批量操作

### 4. 维护性
- 保持测试代码简洁
- 提取公共操作到辅助方法
- 定期更新快照文件
- 跟踪和修复失败的测试

## 持续集成

### GitHub Actions配置
```yaml
name: UI Tests
on: [push, pull_request]
jobs:
  ui-tests:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 6.0.x
    - name: Install dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: UI Tests
      run: dotnet test --no-build --filter Category=UI --logger:"trx"
```

### 报告生成
- 自动生成测试结果报告
- 截图对比分析
- 性能指标统计
- 失败用例追踪

## 扩展开发

### 添加新的UI测试
1. 在DialogTests目录创建新的测试类
2. 继承基本测试模式
3. 实现必要的测试方法
4. 更新主测试配置

### 集成外部工具
- 使用图像识别进行视觉验证
- 集成性能监控工具
- 添加无障碍测试支持
- 支持多语言环境测试

## 支持和反馈

如遇到测试问题或需要技术支持，请：

1. 查看现有的测试文档
2. 检查已知的常见问题
3. 运行调试版本的测试
4. 收集错误日志和截图
5. 联系开发团队

## 版本历史

- v1.0.0: 初始UI测试框架实现
- v1.1.0: 添加快照测试功能
- v1.2.0: 完善性能测试覆盖
- v1.3.0: 增加无障碍测试支持

---

**注意**: UI测试需要在真实的中望CAD环境中运行，确保测试结果的准确性和可靠性。如果无法安装中望CAD，可以使用模拟模式进行测试验证。