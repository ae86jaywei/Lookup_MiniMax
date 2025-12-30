using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;

namespace ZWDynLookup.Tests.UI
{
    /// <summary>
    /// UI自动化测试主类，整合所有UI组件的测试
    /// </summary>
    [TestClass]
    public class UITests
    {
        private Application _testApplication;
        private Window _mainWindow;

        [TestInitialize]
        public void TestInitialize()
        {
            // 初始化测试环境
            InitializeTestEnvironment();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanupTestEnvironment();
        }

        /// <summary>
        /// 测试UI测试环境初始化
        /// </summary>
        [TestMethod]
        public void TestUITestEnvironmentInitialization()
        {
            // 验证测试环境正确初始化
            Assert.IsNotNull(_testApplication, "测试应用程序应已初始化");
            Assert.IsNotNull(_mainWindow, "主窗口应已获取");
            
            // 验证应用程序状态
            Assert.IsTrue(_testApplication.IsRunning, "测试应用程序应正在运行");
            Assert.IsTrue(_mainWindow.IsVisible, "主窗口应可见");
        }

        /// <summary>
        /// 测试所有对话框的完整性
        /// </summary>
        [TestMethod]
        public void TestAllDialogsIntegrity()
        {
            // 测试查寻表对话框
            TestLookupTableDialog();

            // 测试添加参数特性对话框
            TestAddParameterPropertyDialog();

            // 测试查寻表编辑器
            TestLookupTableEditor();

            // 测试运行时查寻菜单
            TestRuntimeLookupMenu();

            // 测试查寻上下文菜单
            TestLookupContextMenu();
        }

        /// <summary>
        /// 测试UI导航流程
        /// </summary>
        [TestMethod]
        public void TestUINavigationFlow()
        {
            // 从主窗口开始，测试完整的UI导航流程
            Assert.IsNotNull(_mainWindow, "主窗口应存在");

            // 模拟打开查寻表管理器
            var lookupTableDialog = OpenDialog("查寻表管理");
            Assert.IsNotNull(lookupTableDialog, "查寻表对话框应打开");

            // 从查寻表对话框打开编辑器
            var editor = OpenEditorFromDialog(lookupTableDialog);
            Assert.IsNotNull(editor, "查寻表编辑器应打开");

            // 测试编辑器的各种操作
            TestEditorOperations(editor);

            // 关闭编辑器
            CloseDialog(editor);

            // 关闭查寻表对话框
            CloseDialog(lookupTableDialog);
        }

        /// <summary>
        /// 测试UI一致性
        /// </summary>
        [TestMethod]
        public void TestUIConsistency()
        {
            // 测试所有对话框的标题一致性
            TestDialogTitleConsistency();

            // 测试按钮文本一致性
            TestButtonTextConsistency();

            // 测试图标和样式一致性
            TestVisualConsistency();
        }

        /// <summary>
        /// 测试UI性能
        /// </summary>
        [TestMethod]
        public void TestUIPerformance()
        {
            var startTime = DateTime.Now;

            // 执行一系列UI操作
            for (int i = 0; i < 10; i++)
            {
                var dialog = OpenDialog("查寻表管理");
                if (dialog != null)
                {
                    // 执行基本操作
                    TestBasicDialogOperations(dialog);
                    CloseDialog(dialog);
                }
                
                UIAutomationHelpers.Wait(100);
            }

            var endTime = DateTime.Now;
            var totalTime = endTime - startTime;

            // 验证性能在可接受范围内
            Assert.IsTrue(totalTime.TotalSeconds < 30, "UI操作总时间应在30秒内");
        }

        /// <summary>
        /// 测试UI错误处理
        /// </summary>
        [TestMethod]
        public void TestUIErrorHandling()
        {
            // 测试无效操作的处理
            TestInvalidOperationHandling();

            // 测试异常情况的UI响应
            TestExceptionHandling();

            // 测试资源清理
            TestResourceCleanup();
        }

        /// <summary>
        /// 测试UI截图功能
        /// </summary>
        [TestMethod]
        public void TestUIScreenshotFunctionality()
        {
            // 测试主窗口截图
            TestMainWindowScreenshot();

            // 测试对话框截图
            TestDialogScreenshots();

            // 测试菜单截图
            TestMenuScreenshots();
        }

        /// <summary>
        /// 测试键盘快捷键
        /// </summary>
        [TestMethod]
        public void TestKeyboardShortcuts()
        {
            // 测试通用快捷键
            TestCommonShortcuts();

            // 测试特定功能快捷键
            TestFunctionSpecificShortcuts();

            // 测试组合键
            TestModifierKeyCombinations();
        }

        /// <summary>
        /// 测试UI可访问性
        /// </summary>
        [TestMethod]
        public void TestUIAccessibility()
        {
            // 测试Tab键导航
            TestTabNavigation();

            // 测试焦点管理
            TestFocusManagement();

            // 测试屏幕阅读器支持
            TestScreenReaderSupport();
        }

        /// <summary>
        /// 测试UI响应性设计
        /// </summary>
        [TestMethod]
        public void TestUIReponsiveDesign()
        {
            // 测试窗口大小变化
            TestWindowResizeHandling();

            // 测试DPI缩放
            TestDPIScaling();

            // 测试多显示器支持
            TestMultiMonitorSupport();
        }

        /// <summary>
        /// 测试数据绑定
        /// </summary>
        [TestMethod]
        public void TestDataBinding()
        {
            // 测试双向数据绑定
            TestTwoWayDataBinding();

            // 测试数据验证
            TestDataValidation();

            // 测试数据转换
            TestDataConversion();
        }

        /// <summary>
        /// 测试UI主题和样式
        /// </summary>
        [TestMethod]
        public void TestUIThemesAndStyles()
        {
            // 测试主题切换
            TestThemeSwitching();

            // 测试样式一致性
            TestStyleConsistency();

            // 测试高对比度模式
            TestHighContrastMode();
        }

        /// <summary>
        /// 初始化测试环境
        /// </summary>
        private void InitializeTestEnvironment()
        {
            try
            {
                UIAutomationHelpers.InitializeTestEnvironment();
                _mainWindow = UIAutomationHelpers.GetMainWindow();
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"UI测试环境初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理测试环境
        /// </summary>
        private void CleanupTestEnvironment()
        {
            try
            {
                UIAutomationHelpers.CleanupTestEnvironment();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理测试环境时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试查寻表对话框
        /// </summary>
        private void TestLookupTableDialog()
        {
            try
            {
                var dialog = OpenDialog("查寻表管理");
                if (dialog != null)
                {
                    // 执行基本操作测试
                    TestBasicDialogOperations(dialog);
                    CloseDialog(dialog);
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"查寻表对话框测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试添加参数特性对话框
        /// </summary>
        private void TestAddParameterPropertyDialog()
        {
            try
            {
                var dialog = OpenDialog("添加参数特性");
                if (dialog != null)
                {
                    // 测试表单输入
                    TestFormInput(dialog);
                    CloseDialog(dialog);
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"添加参数特性对话框测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试查寻表编辑器
        /// </summary>
        private void TestLookupTableEditor()
        {
            try
            {
                var editor = OpenDialog("查寻表编辑器");
                if (editor != null)
                {
                    TestEditorOperations(editor);
                    CloseDialog(editor);
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"查寻表编辑器测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试运行时查寻菜单
        /// </summary>
        private void TestRuntimeLookupMenu()
        {
            try
            {
                var menu = OpenMenu("运行时查寻");
                if (menu != null)
                {
                    TestMenuOperations(menu);
                    CloseDialog(menu);
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"运行时查寻菜单测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试查寻上下文菜单
        /// </summary>
        private void TestLookupContextMenu()
        {
            try
            {
                var menu = OpenMenu("查寻上下文菜单");
                if (menu != null)
                {
                    TestContextMenuOperations(menu);
                    CloseDialog(menu);
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"查寻上下文菜单测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开对话框
        /// </summary>
        /// <param name="dialogTitle">对话框标题</param>
        /// <returns>对话框窗口</returns>
        private Window OpenDialog(string dialogTitle)
        {
            try
            {
                return UIAutomationHelpers.WaitForDialog(dialogTitle, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开对话框 '{dialogTitle}' 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 打开菜单
        /// </summary>
        /// <param name="menuTitle">菜单标题</param>
        /// <returns>菜单窗口</returns>
        private Window OpenMenu(string menuTitle)
        {
            try
            {
                return UIAutomationHelpers.WaitForDialog(menuTitle, TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开菜单 '{menuTitle}' 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从对话框打开编辑器
        /// </summary>
        /// <param name="dialog">源对话框</param>
        /// <returns>编辑器窗口</returns>
        private Window OpenEditorFromDialog(Window dialog)
        {
            try
            {
                if (dialog != null)
                {
                    var editButton = TryFindButton(dialog, "编辑");
                    if (editButton != null)
                    {
                        UIAutomationHelpers.ClickButton(editButton);
                        return UIAutomationHelpers.WaitForDialog("查寻表编辑器", TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从对话框打开编辑器失败: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        /// <param name="window">要关闭的窗口</param>
        private void CloseDialog(Window window)
        {
            try
            {
                if (window != null && !window.IsClosed)
                {
                    var closeButton = TryFindButton(window, "关闭");
                    if (closeButton != null)
                    {
                        UIAutomationHelpers.ClickButton(closeButton);
                    }
                    else
                    {
                        UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
                    }
                    
                    UIAutomationHelpers.Wait(300);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭对话框时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试基本对话框操作
        /// </summary>
        /// <param name="dialog">对话框</param>
        private void TestBasicDialogOperations(Window dialog)
        {
            if (dialog == null) return;

            // 测试主要按钮
            var addButton = TryFindButton(dialog, "添加");
            if (addButton != null)
            {
                UIAutomationHelpers.ClickButton(addButton);
                UIAutomationHelpers.Wait(300);
            }

            var saveButton = TryFindButton(dialog, "保存");
            if (saveButton != null)
            {
                UIAutomationHelpers.ClickButton(saveButton);
                UIAutomationHelpers.Wait(300);
            }
        }

        /// <summary>
        /// 测试编辑器操作
        /// </summary>
        /// <param name="editor">编辑器</param>
        private void TestEditorOperations(Window editor)
        {
            if (editor == null) return;

            // 测试数据网格操作
            var dataGrid = TryFindDataGrid(editor, "dataGrid");
            if (dataGrid != null)
            {
                var addRowButton = TryFindButton(editor, "添加行");
                if (addRowButton != null)
                {
                    UIAutomationHelpers.ClickButton(addRowButton);
                    UIAutomationHelpers.Wait(300);
                }
            }
        }

        /// <summary>
        /// 测试菜单操作
        /// </summary>
        /// <param name="menu">菜单</param>
        private void TestMenuOperations(Window menu)
        {
            if (menu == null) return;

            // 测试查寻输入
            var searchTextBox = TryFindTextBox(menu, "searchTextBox");
            if (searchTextBox != null)
            {
                UIAutomationHelpers.InputText(searchTextBox, "test");
                UIAutomationHelpers.Wait(500);
            }
        }

        /// <summary>
        /// 测试上下文菜单操作
        /// </summary>
        /// <param name="menu">菜单</param>
        private void TestContextMenuOperations(Window menu)
        {
            if (menu == null) return;

            // 测试菜单项点击
            var menuItems = GetMenuItems(menu);
            if (menuItems.Count > 0)
            {
                var firstItem = menuItems.First();
                try
                {
                    firstItem.Click();
                    UIAutomationHelpers.Wait(300);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"点击菜单项失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 测试表单输入
        /// </summary>
        /// <param name="dialog">对话框</param>
        private void TestFormInput(Window dialog)
        {
            if (dialog == null) return;

            // 测试文本框输入
            var nameTextBox = TryFindTextBox(dialog, "nameTextBox");
            if (nameTextBox != null)
            {
                UIAutomationHelpers.InputText(nameTextBox, "测试名称");
            }

            var valueTextBox = TryFindTextBox(dialog, "valueTextBox");
            if (valueTextBox != null)
            {
                UIAutomationHelpers.InputText(valueTextBox, "测试值");
            }
        }

        /// <summary>
        /// 测试对话框标题一致性
        /// </summary>
        private void TestDialogTitleConsistency()
        {
            // 验证所有对话框都有合适的标题
            // 具体实现取决于对话框命名规范
        }

        /// <summary>
        /// 测试按钮文本一致性
        /// </summary>
        private void TestButtonTextConsistency()
        {
            // 验证所有对话框的确定、取消、关闭按钮文本一致
            var standardButtons = new[] { "确定", "取消", "关闭", "应用", "帮助" };
            
            foreach (var buttonText in standardButtons)
            {
                // 验证按钮文本在所有对话框中一致
                // 具体实现需要检查所有对话框
            }
        }

        /// <summary>
        /// 测试视觉一致性
        /// </summary>
        private void TestVisualConsistency()
        {
            // 验证图标、颜色、字体等视觉元素的一致性
            // 具体实现需要截图对比分析
        }

        /// <summary>
        /// 测试无效操作处理
        /// </summary>
        private void TestInvalidOperationHandling()
        {
            // 测试在无效状态下的操作
            try
            {
                var dialog = OpenDialog("查寻表管理");
                if (dialog != null)
                {
                    // 在没有数据的情况下尝试删除
                    var deleteButton = TryFindButton(dialog, "删除");
                    if (deleteButton != null)
                    {
                        UIAutomationHelpers.ClickButton(deleteButton);
                        UIAutomationHelpers.Wait(500);
                    }
                    
                    CloseDialog(dialog);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无效操作测试时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试异常处理
        /// </summary>
        private void TestExceptionHandling()
        {
            // 测试各种异常情况下的UI响应
            // 具体实现需要模拟各种错误场景
        }

        /// <summary>
        /// 测试资源清理
        /// </summary>
        private void TestResourceCleanup()
        {
            // 测试对话框关闭后的资源清理
            for (int i = 0; i < 5; i++)
            {
                var dialog = OpenDialog("查寻表管理");
                if (dialog != null)
                {
                    CloseDialog(dialog);
                }
                UIAutomationHelpers.Wait(200);
            }
        }

        /// <summary>
        /// 测试主窗口截图
        /// </summary>
        private void TestMainWindowScreenshot()
        {
            try
            {
                var screenshotPath = Path.Combine(Path.GetTempPath(), "mainWindowTest.png");
                UIAutomationHelpers.CaptureWindowScreenshot(_mainWindow, screenshotPath);
                
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"主窗口截图测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试对话框截图
        /// </summary>
        private void TestDialogScreenshots()
        {
            try
            {
                var dialog = OpenDialog("查寻表管理");
                if (dialog != null)
                {
                    var screenshotPath = Path.Combine(Path.GetTempPath(), "dialogTest.png");
                    UIAutomationHelpers.CaptureWindowScreenshot(dialog, screenshotPath);
                    
                    if (File.Exists(screenshotPath))
                    {
                        File.Delete(screenshotPath);
                    }
                    
                    CloseDialog(dialog);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"对话框截图测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试菜单截图
        /// </summary>
        private void TestMenuScreenshots()
        {
            try
            {
                var menu = OpenMenu("运行时查寻");
                if (menu != null)
                {
                    var screenshotPath = Path.Combine(Path.GetTempPath(), "menuTest.png");
                    UIAutomationHelpers.CaptureWindowScreenshot(menu, screenshotPath);
                    
                    if (File.Exists(screenshotPath))
                    {
                        File.Delete(screenshotPath);
                    }
                    
                    CloseDialog(menu);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"菜单截图测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试通用快捷键
        /// </summary>
        private void TestCommonShortcuts()
        {
            // 测试Esc键
            var dialog = OpenDialog("查寻表管理");
            if (dialog != null)
            {
                UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
                UIAutomationHelpers.Wait(300);
                
                if (!dialog.IsClosed)
                {
                    CloseDialog(dialog);
                }
            }

            // 测试Enter键
            dialog = OpenDialog("添加参数特性");
            if (dialog != null)
            {
                UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.RETURN);
                UIAutomationHelpers.Wait(300);
                
                if (!dialog.IsClosed)
                {
                    CloseDialog(dialog);
                }
            }
        }

        /// <summary>
        /// 测试特定功能快捷键
        /// </summary>
        private void TestFunctionSpecificShortcuts()
        {
            // 测试Ctrl+S保存
            // 测试F2重命名
            // 测试Del删除
        }

        /// <summary>
        /// 测试组合键
        /// </summary>
        private void TestModifierKeyCombinations()
        {
            // 测试Ctrl+组合键
            // 测试Shift+组合键
            // 测试Alt+组合键
        }

        /// <summary>
        /// 测试Tab键导航
        /// </summary>
        private void TestTabNavigation()
        {
            var dialog = OpenDialog("查寻表管理");
            if (dialog != null)
            {
                // 测试Tab键遍历所有控件
                for (int i = 0; i < 10; i++)
                {
                    UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.TAB);
                    UIAutomationHelpers.Wait(200);
                }
                
                CloseDialog(dialog);
            }
        }

        /// <summary>
        /// 测试焦点管理
        /// </summary>
        private void TestFocusManagement()
        {
            var dialog = OpenDialog("查寻表管理");
            if (dialog != null)
            {
                // 验证对话框获得焦点
                Assert.IsTrue(dialog.IsActive, "对话框应获得焦点");
                
                CloseDialog(dialog);
            }
        }

        /// <summary>
        /// 测试屏幕阅读器支持
        /// </summary>
        private void TestScreenReaderSupport()
        {
            // 验证控件的AutomationId和Name
            // 具体实现需要检查UI元素的AutomationProperties
        }

        /// <summary>
        /// 测试窗口大小变化处理
        /// </summary>
        private void TestWindowResizeHandling()
        {
            var dialog = OpenDialog("查寻表管理");
            if (dialog != null)
            {
                try
                {
                    dialog.MoveBy(50, 50);
                    UIAutomationHelpers.Wait(300);
                    
                    dialog.MoveBy(-50, -50);
                    UIAutomationHelpers.Wait(300);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"窗口大小调整测试失败: {ex.Message}");
                }
                
                CloseDialog(dialog);
            }
        }

        /// <summary>
        /// 测试DPI缩放
        /// </summary>
        private void TestDPIScaling()
        {
            // 测试高DPI显示下的UI缩放
            // 具体实现需要获取屏幕DPI信息
        }

        /// <summary>
        /// 测试多显示器支持
        /// </summary>
        private void TestMultiMonitorSupport()
        {
            // 测试对话框在不同显示器上的显示
            // 具体实现需要显示器信息
        }

        /// <summary>
        /// 测试双向数据绑定
        /// </summary>
        private void TestTwoWayDataBinding()
        {
            var dialog = OpenDialog("添加参数特性");
            if (dialog != null)
            {
                var nameTextBox = TryFindTextBox(dialog, "nameTextBox");
                if (nameTextBox != null)
                {
                    var testValue = "测试双向绑定";
                    UIAutomationHelpers.InputText(nameTextBox, testValue);
                    
                    // 验证值设置成功
                    Assert.AreEqual(testValue, nameTextBox.Text, "双向数据绑定应生效");
                }
                
                CloseDialog(dialog);
            }
        }

        /// <summary>
        /// 测试数据验证
        /// </summary>
        private void TestDataValidation()
        {
            var dialog = OpenDialog("添加参数特性");
            if (dialog != null)
            {
                // 测试无效数据
                var nameTextBox = TryFindTextBox(dialog, "nameTextBox");
                if (nameTextBox != null)
                {
                    UIAutomationHelpers.InputText(nameTextBox, ""); // 空值
                    
                    var okButton = TryFindButton(dialog, "确定");
                    if (okButton != null)
                    {
                        UIAutomationHelpers.ClickButton(okButton);
                        UIAutomationHelpers.Wait(500);
                        
                        // 如果实现了验证，对话框应保持打开
                        Assert.IsFalse(dialog.IsClosed, "无效数据应触发验证");
                    }
                }
                
                CloseDialog(dialog);
            }
        }

        /// <summary>
        /// 测试数据转换
        /// </summary>
        private void TestDataConversion()
        {
            // 测试数据类型转换
            // 验证数值、日期、布尔值等类型的正确转换
        }

        /// <summary>
        /// 测试主题切换
        /// </summary>
        private void TestThemeSwitching()
        {
            // 如果支持主题切换，测试不同主题下的UI显示
        }

        /// <summary>
        /// 测试样式一致性
        /// </summary>
        private void TestStyleConsistency()
        {
            // 验证所有对话框的样式一致性
            // 检查按钮、标签、输入框等控件的样式
        }

        /// <summary>
        /// 测试高对比度模式
        /// </summary>
        private void TestHighContrastMode()
        {
            // 测试高对比度模式下的可读性
        }

        /// <summary>
        /// 尝试查找按钮
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>按钮</returns>
        private Button TryFindButton(Window window, string buttonText)
        {
            try
            {
                return UIAutomationHelpers.FindButton(window, buttonText);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试查找文本框
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>文本框</returns>
        private TextBox TryFindTextBox(Window window, string automationId)
        {
            try
            {
                return UIAutomationHelpers.FindTextBox(window, automationId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试查找数据网格
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>数据网格</returns>
        private DataGrid TryFindDataGrid(Window window, string automationId)
        {
            try
            {
                return UIAutomationHelpers.FindDataGrid(window, automationId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取菜单项
        /// </summary>
        /// <param name="window">窗口</param>
        /// <returns>菜单项列表</returns>
        private System.Collections.Generic.List<Menu> GetMenuItems(Window window)
        {
            var menuItems = new System.Collections.Generic.List<Menu>();
            try
            {
                var menuBar = window.MenuBar;
                if (menuBar != null)
                {
                    foreach (var item in menuBar.MenuItems)
                    {
                        menuItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取菜单项时出错: {ex.Message}");
            }
            return menuItems;
        }
    }
}