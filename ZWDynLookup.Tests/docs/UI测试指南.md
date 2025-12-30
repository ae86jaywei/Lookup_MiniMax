---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 30450220329bce038f841f70e26cc7e3b950b279aeb6d58ff14ca1319c21b7846deee25a022100db87e251e80bcf0ecafa5f9cf3d777a23a9c27b609ee3dd87de8c0ee4fc11d9e
    ReservedCode2: 3046022100c185dca61342ae2902e82b30a8b9a50dbc0af064840e2b7ff3617a08537ba5a3022100971f963d96a01e982386c1cfa53eff3d0ee003f91b09b4949b8b04d09dd3c712
---

# UI自动化测试开发指南

## 概述

UI自动化测试是确保ZWDynLookup插件用户界面功能正确性和用户体验质量的重要手段。本指南详细说明了WPF UI自动化测试的框架选择、测试策略、实现方法和最佳实践。

## 测试框架选择

### 1. 推荐框架组合

#### 主要框架: White
- **优势**: 与WPF深度集成，支持复杂UI操作
- **适用场景**: WPF应用程序的自动化测试
- **版本**: 最新稳定版本

```xml
<PackageReference Include="White" Version="0.13.0" />
<PackageReference Include="TestStack.White" Version="0.13.3" />
<PackageReference Include="TestStack.White.ScreenObjects" Version="0.13.3" />
```

#### 辅助框架: FlaUI
- **优势**: 现代化UI自动化框架，支持UIA3
- **适用场景**: 需要更精确UI控制的场景

```xml
<PackageReference Include="FlaUI.Core" Version="4.0.0" />
<PackageReference Include="FlaUI.UIA3" Version="4.0.0" />
<PackageReference Include="FlaUI.Chromium" Version="4.0.0" />
```

### 2. 测试工具集成

```xml
<!-- 截图工具 -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />

<!-- 图像比较 -->
<PackageReference Include="OpenCvSharp4" Version="4.8.0.20230708" />
<PackageReference Include="AForge.Imaging" Version="2.2.5" />
```

## UI测试架构

### 1. 页面对象模式 (Page Object Pattern)

```csharp
public abstract class UIPageBase : IDisposable
{
    protected Application Application { get; }
    protected Window MainWindow { get; }
    protected ILogger Logger { get; }
    
    protected UIPageBase(Application application)
    {
        Application = application;
        MainWindow = application.GetWindowByTitle("AutoCAD");
        Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UIPageBase>();
    }
    
    protected TElement GetElement<TElement>(SearchCriteria criteria) where TElement : UIItem
    {
        try
        {
            return MainWindow.Get<TElement>(criteria);
        }
        catch (Exception ex)
        {
            Logger.LogError($"获取元素失败: {ex.Message}");
            throw;
        }
    }
    
    protected void WaitForElement(SearchCriteria criteria, TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        while (DateTime.Now - startTime < timeout)
        {
            try
            {
                MainWindow.Get<UIItem>(criteria);
                return;
            }
            catch
            {
                Thread.Sleep(100);
            }
        }
        throw new TimeoutException($"等待元素超时: {criteria}");
    }
    
    public virtual void Dispose()
    {
        // 页面清理逻辑
    }
}
```

### 2. 查寻表对话框页面对象

```csharp
public class LookupTableDialogPage : UIPageBase
{
    private const string DialogTitle = "查寻表管理器";
    
    // 页面元素
    public Button AddButton => GetElement<Button>(SearchCriteria.ByAutomationId("AddButton"));
    public Button EditButton => GetElement<Button>(SearchCriteria.ByAutomationId("EditButton"));
    public Button DeleteButton => GetElement<Button>(SearchCriteria.ByAutomationId("DeleteButton"));
    public DataGrid LookupTableGrid => GetElement<DataGrid>(SearchCriteria.ByAutomationId("LookupTableGrid"));
    public TextBox TableNameTextBox => GetElement<TextBox>(SearchCriteria.ByAutomationId("TableNameTextBox"));
    public ComboBox ParameterComboBox => GetElement<ComboBox>(SearchCriteria.ByAutomationId("ParameterComboBox"));
    
    public LookupTableDialogPage(Application application) : base(application) { }
    
    public void WaitForDialog()
    {
        WaitForElement(SearchCriteria.ByText(DialogTitle), TimeSpan.FromSeconds(10));
    }
    
    public void AddNewTable(string tableName, string[] parameters)
    {
        Logger.LogInformation($"添加新表格: {tableName}");
        
        // 点击添加按钮
        AddButton.Click();
        
        // 等待对话框打开
        var addDialog = Application.GetWindowByTitle("添加查寻表", InitializeOption.NoCache);
        
        // 填写表格名称
        var nameTextBox = addDialog.Get<TextBox>(SearchCriteria.ByAutomationId("TableNameTextBox"));
        nameTextBox.Text = tableName;
        
        // 添加参数
        var parametersList = addDialog.Get<ListBox>(SearchCriteria.ByAutomationId("ParametersList"));
        foreach (var param in parameters)
        {
            var addParamButton = addDialog.Get<Button>(SearchCriteria.ByText("添加参数"));
            addParamButton.Click();
            
            var paramNameTextBox = addDialog.Get<TextBox>(SearchCriteria.ByAutomationId("ParameterNameTextBox"));
            paramNameTextBox.Text = param;
            
            var okButton = addDialog.Get<Button>(SearchCriteria.ByText("确定"));
            okButton.Click();
        }
        
        // 保存表格
        var saveButton = addDialog.Get<Button>(SearchCriteria.ByText("保存"));
        saveButton.Click();
        
        // 等待对话框关闭
        addDialog.WaitTill(() => !addDialog.IsCurrentlyAvailable, TimeSpan.FromSeconds(5));
    }
    
    public void EditTable(string oldTableName, string newTableName)
    {
        Logger.LogInformation($"编辑表格: {oldTableName} -> {newTableName}");
        
        // 选择表格
        SelectTable(oldTableName);
        
        // 点击编辑按钮
        EditButton.Click();
        
        // 修改名称
        TableNameTextBox.Text = newTableName;
        
        // 保存修改
        var saveButton = GetElement<Button>(SearchCriteria.ByText("保存"));
        saveButton.Click();
    }
    
    public void SelectTable(string tableName)
    {
        Logger.LogInformation($"选择表格: {tableName}");
        
        var tableItem = LookupTableGrid.GetCell(0, 0); // 假设表格名称在第一列
        tableItem.Click();
        
        // 验证选择
        var selectedTable = GetSelectedTableName();
        Assert.AreEqual(tableName, selectedTable, $"表格选择失败: 期望 {tableName}, 实际 {selectedTable}");
    }
    
    private string GetSelectedTableName()
    {
        try
        {
            var selectedCell = LookupTableGrid.GetCell(0, 0);
            return selectedCell.Text;
        }
        catch
        {
            return string.Empty;
        }
    }
    
    public List<string> GetAvailableTables()
    {
        var tables = new List<string>();
        
        try
        {
            var rows = LookupTableGrid.Rows;
            foreach (var row in rows)
            {
                var tableName = row.Cells[0].Text;
                if (!string.IsNullOrEmpty(tableName))
                {
                    tables.Add(tableName);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"获取表格列表失败: {ex.Message}");
        }
        
        return tables;
    }
}
```

## 核心UI测试实现

### 1. 查寻表管理UI测试

```csharp
[TestClass]
public class LookupTableDialogTests : UITestBase
{
    public LookupTableDialogTests() : base() { }
    
    [TestInitialize]
    public void TestSetup()
    {
        base.TestSetup();
        
        // 启动查寻表对话框
        LaunchLookupTableDialog();
    }
    
    [TestMethod]
    public void AddNewLookupTable_WithValidData_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var page = new LookupTableDialogPage(Application);
        var tableName = $"UITest_Table_{Guid.NewGuid():N}";
        var parameters = new[] { "Width", "Height", "Length" };
        
        // Act
        page.WaitForDialog();
        page.AddNewTable(tableName, parameters);
        
        // Assert
        var availableTables = page.GetAvailableTables();
        Assert.IsTrue(availableTables.Contains(tableName), 
                     $"新创建的表格 {tableName} 未在列表中找到");
        
        // 验证表格内容
        page.SelectTable(tableName);
        VerifyTableParameters(parameters);
    }
    
    [TestMethod]
    public void EditLookupTable_ModifyName_ShouldUpdateSuccessfully()
    {
        // Arrange
        var page = new LookupTableDialogPage(Application);
        var oldTableName = "TestTable";
        var newTableName = "UpdatedTestTable";
        
        // 确保测试表格存在
        EnsureTestTableExists(oldTableName);
        
        // Act
        page.EditTable(oldTableName, newTableName);
        
        // Assert
        var availableTables = page.GetAvailableTables();
        Assert.IsFalse(availableTables.Contains(oldTableName), 
                      $"旧表格名称 {oldTableName} 仍然存在");
        Assert.IsTrue(availableTables.Contains(newTableName), 
                     $"新表格名称 {newTableName} 未找到");
    }
    
    [TestMethod]
    public void DeleteLookupTable_ConfirmDeletion_ShouldRemoveFromList()
    {
        // Arrange
        var page = new LookupTableDialogPage(Application);
        var tableName = "ToDeleteTable";
        
        EnsureTestTableExists(tableName);
        
        // Act
        page.SelectTable(tableName);
        page.DeleteButton.Click();
        
        // 确认删除
        ConfirmDialogAction("是");
        
        // Assert
        var availableTables = page.GetAvailableTables();
        Assert.IsFalse(availableTables.Contains(tableName), 
                      $"表格 {tableName} 未被删除");
    }
    
    [TestMethod]
    public void LookupTableOperations_PerformanceTest_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var page = new LookupTableDialogPage(Application);
        var stopwatch = Stopwatch.StartNew();
        
        // Act - 执行一系列UI操作
        var operations = new Action[]
        {
            () => page.AddNewTable($"PerfTest_{Guid.NewGuid():N}", new[] { "Param1", "Param2" }),
            () => page.SelectTable(GetFirstAvailableTable()),
            () => page.EditTable(GetFirstAvailableTable(), $"Edited_{Guid.NewGuid():N}")
        };
        
        foreach (var operation in operations)
        {
            operation();
        }
        
        stopwatch.Stop();
        
        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, 
                     $"UI操作耗时过长: {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

### 2. 参数属性对话框测试

```csharp
[TestClass]
public class ParameterPropertiesDialogTests : UITestBase
{
    [TestMethod]
    public void AddParameter_WithValidData_ShouldAddSuccessfully()
    {
        // Arrange
        LaunchParameterPropertiesDialog();
        var dialog = Application.GetWindowByTitle("参数属性");
        
        // Act
        var nameTextBox = dialog.Get<TextBox>(SearchCriteria.ByAutomationId("ParameterNameTextBox"));
        nameTextBox.Text = "TestParameter";
        
        var typeComboBox = dialog.Get<ComboBox>(SearchCriteria.ByAutomationId("ParameterTypeComboBox"));
        typeComboBox.Select("Double");
        
        var defaultValueTextBox = dialog.Get<TextBox>(SearchCriteria.ByAutomationId("DefaultValueTextBox"));
        defaultValueTextBox.Text = "10.0";
        
        var okButton = dialog.Get<Button>(SearchCriteria.ByText("确定"));
        okButton.Click();
        
        // Assert
        // 验证参数已添加（可以通过数据库查询或界面刷新来验证）
        Thread.Sleep(1000); // 等待UI更新
        
        var parameterAdded = VerifyParameterExists("TestParameter");
        Assert.IsTrue(parameterAdded, "参数添加失败");
    }
    
    [TestMethod]
    public void ParameterValidation_InvalidData_ShouldShowErrorMessage()
    {
        // Arrange
        LaunchParameterPropertiesDialog();
        var dialog = Application.GetWindowByTitle("参数属性");
        
        // Act - 输入无效数据
        var nameTextBox = dialog.Get<TextBox>(SearchCriteria.ByAutomationId("ParameterNameTextBox"));
        nameTextBox.Text = ""; // 空名称
        
        var okButton = dialog.Get<Button>(SearchCriteria.ByText("确定"));
        okButton.Click();
        
        // Assert - 验证错误消息
        var errorDialog = Application.GetWindowByTitle("验证错误", InitializeOption.NoCache);
        Assert.IsNotNull(errorDialog, "应该显示验证错误对话框");
        
        var errorMessage = errorDialog.Get<Label>(SearchCriteria.ByAutomationId("ErrorMessageLabel"));
        Assert.IsTrue(errorMessage.Text.Contains("参数名称不能为空"), 
                     "错误消息内容不正确");
        
        // 关闭错误对话框
        var okErrorButton = errorDialog.Get<Button>(SearchCriteria.ByText("确定"));
        okErrorButton.Click();
    }
}
```

### 3. 右键菜单测试

```csharp
[TestClass]
public class ContextMenuTests : UITestBase
{
    [TestMethod]
    public void LookupContextMenu_ShowOnRightClick_ShouldDisplayMenuItems()
    {
        // Arrange
        var page = new LookupTableDialogPage(Application);
        
        // 在AutoCAD绘图区域进行右键点击
        var drawingArea = GetDrawingArea();
        
        // Act
        drawingArea.RightClick();
        
        // Assert - 验证右键菜单显示
        var contextMenu = Application.GetWindowByTitle("查寻菜单", InitializeOption.NoCache);
        Assert.IsNotNull(contextMenu, "右键菜单未显示");
        
        // 验证菜单项
        var menuItems = contextMenu.GetItems();
        Assert.IsTrue(menuItems.Any(item => item.Text.Contains("查寻值")), 
                     "查寻值菜单项未找到");
        Assert.IsTrue(menuItems.Any(item => item.Text.Contains("管理查寻表")), 
                     "管理查寻表菜单项未找到");
    }
    
    [TestMethod]
    public void RuntimeLookupMenu_PerformLookup_ShouldReturnResult()
    {
        // Arrange
        var contextMenu = GetVisibleContextMenu();
        
        // Act
        var lookupItem = contextMenu.GetMenuItemByText("查寻值");
        lookupItem.Click();
        
        // 等待查寻结果
        Thread.Sleep(2000);
        
        // Assert
        var resultDialog = Application.GetWindowByTitle("查寻结果", InitializeOption.NoCache);
        Assert.IsNotNull(resultDialog, "查寻结果对话框未显示");
        
        var resultText = resultDialog.Get<Label>(SearchCriteria.ByAutomationId("ResultTextBlock"));
        Assert.IsFalse(string.IsNullOrEmpty(resultText.Text), "查寻结果为空");
    }
}
```

## 截图和视觉验证

### 1. 截图工具

```csharp
public class ScreenshotCapture
{
    private readonly string _screenshotPath;
    private readonly ILogger _logger;
    
    public ScreenshotCapture(string screenshotPath)
    {
        _screenshotPath = screenshotPath;
        Directory.CreateDirectory(screenshotPath);
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<ScreenshotCapture>();
    }
    
    public void CaptureWindow(Window window, string testName, string stepName)
    {
        var fileName = $"{testName}_{stepName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var filePath = Path.Combine(_screenshotPath, fileName);
        
        try
        {
            var bitmap = window.GetBitmap();
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            
            _logger.LogInformation($"截图已保存: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"截图失败: {ex.Message}");
        }
    }
    
    public void CaptureScreen(string testName, string stepName)
    {
        var fileName = $"{testName}_{stepName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var filePath = Path.Combine(_screenshotPath, fileName);
        
        try
        {
            using var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, 
                Screen.PrimaryScreen.Bounds.Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            _logger.LogInformation($"全屏截图已保存: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"全屏截图失败: {ex.Message}");
        }
    }
}
```

### 2. 视觉验证

```csharp
public class VisualVerification
{
    private readonly ScreenshotCapture _screenshotCapture;
    private readonly string _baselinePath;
    
    public VisualVerification(ScreenshotCapture screenshotCapture, string baselinePath)
    {
        _screenshotCapture = screenshotCapture;
        _baselinePath = baselinePath;
    }
    
    public bool VerifyWindowLayout(Window window, string testName, double tolerance = 0.1)
    {
        var currentScreenshot = CaptureWindowAsBitmap(window);
        var baselineScreenshotPath = Path.Combine(_baselinePath, $"{testName}_baseline.png");
        
        if (!File.Exists(baselineScreenshotPath))
        {
            // 保存基准图像
            currentScreenshot.Save(baselineScreenshotPath);
            _screenshotCapture._logger.LogWarning($"基准图像已创建: {baselineScreenshotPath}");
            return true;
        }
        
        // 加载基准图像
        var baselineScreenshot = new Bitmap(baselineScreenshotPath);
        
        // 比较图像
        var similarity = CalculateImageSimilarity(currentScreenshot, baselineScreenshot);
        var isMatch = similarity >= (1 - tolerance);
        
        _screenshotCapture._logger.LogInformation($"视觉验证结果: 相似度 {similarity:P2}, 通过: {isMatch}");
        
        return isMatch;
    }
    
    private Bitmap CaptureWindowAsBitmap(Window window)
    {
        var bounds = window.Bounds;
        var bitmap = new Bitmap(bounds.Width, bounds.Height);
        
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
        
        return bitmap;
    }
    
    private double CalculateImageSimilarity(Bitmap img1, Bitmap img2)
    {
        if (img1.Size != img2.Size)
        {
            img2 = new Bitmap(img2, img1.Size);
        }
        
        int matchingPixels = 0;
        int totalPixels = img1.Width * img1.Height;
        
        for (int x = 0; x < img1.Width; x += 4) // 采样比较提高性能
        {
            for (int y = 0; y < img1.Height; y += 4)
            {
                var color1 = img1.GetPixel(x, y);
                var color2 = img2.GetPixel(x, y);
                
                if (Math.Abs(color1.R - color2.R) < 30 &&
                    Math.Abs(color1.G - color2.G) < 30 &&
                    Math.Abs(color1.B - color2.B) < 30)
                {
                    matchingPixels++;
                }
            }
        }
        
        return (double)matchingPixels / (totalPixels / 16); // 调整采样数量
    }
}
```

## 数据驱动UI测试

### 1. 参数化UI测试

```csharp
[TestClass]
public class DataDrivenUITests : UITestBase
{
    [DataTestMethod]
    [CsvDataSource("TestData/UI/TestScenarios.csv")]
    public void ParameterCreation_WithVariousDataTypes_ShouldHandleCorrectly(
        string testName, string parameterType, string defaultValue, string expectedBehavior)
    {
        // Arrange
        LaunchParameterPropertiesDialog();
        var dialog = Application.GetWindowByTitle("参数属性");
        
        var nameTextBox = dialog.Get<TextBox>(SearchCriteria.ByAutomationId("ParameterNameTextBox"));
        var typeComboBox = dialog.Get<ComboBox>(SearchCriteria.ByAutomationId("ParameterTypeComboBox"));
        var valueTextBox = dialog.Get<TextBox>(SearchCriteria.ByAutomationId("DefaultValueTextBox"));
        
        // Act
        nameTextBox.Text = $"TestParam_{testName}";
        typeComboBox.Select(parameterType);
        valueTextBox.Text = defaultValue;
        
        var okButton = dialog.Get<Button>(SearchCriteria.ByText("确定"));
        okButton.Click();
        
        // Assert
        Thread.Sleep(1000);
        
        if (expectedBehavior == "Success")
        {
            VerifyParameterCreationSuccess($"TestParam_{testName}");
        }
        else if (expectedBehavior == "ValidationError")
        {
            VerifyValidationErrorDisplayed();
        }
    }
}
```

### 2. 测试场景数据

```csv
testName,parameterType,defaultValue,expectedBehavior
ValidDouble,Double,10.5,Success
ValidInteger,Integer,42,Success
ValidString,String,HelloWorld,Success
EmptyName,,10.5,ValidationError
InvalidDouble,String,invalid,ValidationError
NegativeValue,Double,-5.0,Success
ZeroValue,Integer,0,Success
```

## 异常处理和恢复

### 1. UI异常处理

```csharp
public class UIExceptionHandler
{
    private readonly ILogger _logger;
    private readonly ScreenshotCapture _screenshotCapture;
    
    public UIExceptionHandler(ScreenshotCapture screenshotCapture)
    {
        _screenshotCapture = screenshotCapture;
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UIExceptionHandler>();
    }
    
    public void HandleUIException(Exception ex, string context)
    {
        _logger.LogError(ex, $"UI异常发生 - 上下文: {context}");
        
        // 截图保存异常状态
        try
        {
            _screenshotCapture.CaptureScreen($"Error_{context}", DateTime.Now.ToString("HHmmss"));
        }
        catch
        {
            // 截图失败不影响异常处理
        }
        
        // 尝试恢复UI状态
        TryRecoverUIState(context);
        
        // 根据异常类型决定是否重新抛出
        if (IsRecoverableException(ex))
        {
            _logger.LogWarning($"异常已恢复: {ex.Message}");
        }
        else
        {
            throw new UITestException($"无法恢复的UI异常: {ex.Message}", ex);
        }
    }
    
    private bool IsRecoverableException(Exception ex)
    {
        return ex is ElementNotAvailableException || 
               ex is TimeoutException || 
               ex is AutomationException;
    }
    
    private void TryRecoverUIState(string context)
    {
        try
        {
            // 关闭可能的对话框
            CloseAllDialogs();
            
            // 重新聚焦主窗口
            FocusMainWindow();
            
            // 清理可能的UI状态
            ResetUIState();
        }
        catch (Exception recoveryEx)
        {
            _logger.LogError(recoveryEx, "UI状态恢复失败");
        }
    }
}
```

### 2. 测试重试机制

```csharp
public class UITestRetry
{
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;
    
    public UITestRetry(int maxRetries = 3, TimeSpan? retryDelay = null)
    {
        _maxRetries = maxRetries;
        _retryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
    }
    
    public T ExecuteWithRetry<T>(Func<T> testAction, string testName)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                return testAction();
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                Console.WriteLine($"测试 {testName} 第 {attempt} 次尝试失败: {ex.Message}");
                Thread.Sleep(_retryDelay);
                
                // 尝试恢复状态
                RecoverTestState();
            }
        }
        
        throw new UITestException($"测试 {testName} 在 {_maxRetries} 次重试后仍然失败");
    }
    
    private void RecoverTestState()
    {
        try
        {
            // 重新初始化测试环境
            ResetTestEnvironment();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"状态恢复失败: {ex.Message}");
        }
    }
}
```

## CI/CD集成

### 1. 无头模式测试

```csharp
public class HeadlessUITestRunner
{
    public static void RunHeadlessTests(string testDllPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"test {testDllPath} --logger trx --results-directory ./TestResults",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(processStartInfo);
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            throw new TestExecutionException($"UI测试执行失败，退出码: {process.ExitCode}");
        }
    }
}
```

### 2. 测试报告集成

```csharp
public class UITestReportGenerator
{
    public static void GenerateHtmlReport(string testResultsPath, string outputPath)
    {
        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("<!DOCTYPE html>");
        reportBuilder.AppendLine("<html><head><title>UI测试报告</title>");
        reportBuilder.AppendLine("<style>");
        reportBuilder.AppendLine("table { border-collapse: collapse; }");
        reportBuilder.AppendLine("th, td { border: 1px solid black; padding: 8px; }");
        reportBuilder.AppendLine(".passed { background-color: #d4edda; }");
        reportBuilder.AppendLine(".failed { background-color: #f8d7da; }");
        reportBuilder.AppendLine("</style></head><body>");
        
        reportBuilder.AppendLine("<h1>UI自动化测试报告</h1>");
        reportBuilder.AppendLine($"<p>生成时间: {DateTime.Now}</p>");
        
        // 添加测试结果表格
        reportBuilder.AppendLine("<h2>测试结果</h2>");
        reportBuilder.AppendLine("<table><tr><th>测试名称</th><th>状态</th><th>执行时间</th><th>错误信息</th></tr>");
        
        // 从TRX文件解析结果
        var testResults = ParseTestResults(testResultsPath);
        foreach (var result in testResults)
        {
            var statusClass = result.Outcome == "Passed" ? "passed" : "failed";
            reportBuilder.AppendLine($"<tr class='{statusClass}'>");
            reportBuilder.AppendLine($"<td>{result.TestName}</td>");
            reportBuilder.AppendLine($"<td>{result.Outcome}</td>");
            reportBuilder.AppendLine($"<td>{result.Duration}</td>");
            reportBuilder.AppendLine($"<td>{result.ErrorMessage}</td>");
            reportBuilder.AppendLine("</tr>");
        }
        
        reportBuilder.AppendLine("</table></body></html>");
        
        File.WriteAllText(outputPath, reportBuilder.ToString());
    }
}
```

## 最佳实践

### 1. 测试稳定性优化
- **适当的等待策略**: 使用智能等待而不是固定延迟
- **元素定位优化**: 使用稳定的标识符进行元素定位
- **测试数据隔离**: 确保每个测试使用独立的测试数据

### 2. 维护性考虑
- **页面对象模式**: 使用页面对象模式减少代码重复
- **配置驱动**: 将测试配置外部化，便于维护
- **日志记录**: 详细的日志记录有助于问题定位

### 3. 性能优化
- **并行测试执行**: 合理使用并行测试提高效率
- **资源清理**: 及时清理测试资源，防止内存泄漏
- **智能等待**: 避免不必要的等待时间

---

*本指南为ZWDynLookup项目提供了全面的UI自动化测试框架，确保用户界面功能的稳定性和用户体验质量。*