using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.TabItems;
using TestStack.White.UIItems.TreeItems;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.WindowsAPI;

namespace ZWDynLookup.Tests.UI
{
    /// <summary>
    /// UI自动化测试辅助类，提供通用的UI操作和验证方法
    /// </summary>
    public static class UIAutomationHelpers
    {
        private static Application _application;
        private static Window _mainWindow;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan ShortTimeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// 初始化中望CAD UI测试环境
        /// </summary>
        /// <param name="cadProcessName">CAD进程名称</param>
        public static void InitializeTestEnvironment(string cadProcessName = "zwcad")
        {
            try
            {
                // 查找中望CAD应用程序
                var processes = System.Diagnostics.Process.GetProcessesByName(cadProcessName);
                if (processes.Length == 0)
                {
                    // 尝试其他可能的进程名称
                    var possibleNames = new[] { "zwcad", "ZWCAD", "zwcadpro" };
                    foreach (var name in possibleNames)
                    {
                        processes = System.Diagnostics.Process.GetProcessesByName(name);
                        if (processes.Length > 0) break;
                    }
                    
                    if (processes.Length == 0)
                    {
                        throw new InvalidOperationException($"未找到中望CAD进程: {cadProcessName}");
                    }
                }

                _application = Application.Attach(processes[0].Id);
                
                // 获取主窗口 - 优先查找中望CAD窗口
                var windows = _application.GetWindows();
                _mainWindow = windows.FirstOrDefault(w => 
                    w.Title.Contains("ZWCAD") || 
                    w.Title.Contains("中望CAD") ||
                    w.Title.Contains("ZhongWang") ||
                    w.Title.Contains("Zhongwang")
                ) ?? windows.FirstOrDefault();

                if (_mainWindow == null)
                {
                    throw new InvalidOperationException("未找到中望CAD主窗口");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"初始化中望CAD UI测试环境失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清理测试环境
        /// </summary>
        public static void CleanupTestEnvironment()
        {
            try
            {
                if (_application != null)
                {
                    _application.Dispose();
                    _application = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理测试环境时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 等待对话框出现
        /// </summary>
        /// <param name="dialogTitle">对话框标题</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>对话框窗口</returns>
        public static Window WaitForDialog(string dialogTitle, TimeSpan? timeout = null)
        {
            var waitTimeout = timeout ?? DefaultTimeout;
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < waitTimeout)
            {
                try
                {
                    var windows = _application.GetWindows();
                    var dialog = windows.FirstOrDefault(w => w.Title.Contains(dialogTitle));
                    if (dialog != null && dialog.IsModal)
                    {
                        return dialog;
                    }
                }
                catch
                {
                    // 忽略查找过程中的异常
                }

                Thread.Sleep(100);
            }

            throw new TimeoutException($"等待对话框 '{dialogTitle}' 超时");
        }

        /// <summary>
        /// 查找UI元素
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>UI项</returns>
        public static IUIItem FindElementById(Window window, string automationId)
        {
            return window.Get(SearchCriteria.ByAutomationId(automationId));
        }

        /// <summary>
        /// 查找按钮
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>按钮</returns>
        public static Button FindButton(Window window, string buttonText)
        {
            return window.Get<Button>(SearchCriteria.ByText(buttonText));
        }

        /// <summary>
        /// 查找文本框
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>文本框</returns>
        public static TextBox FindTextBox(Window window, string automationId)
        {
            return window.Get<TextBox>(SearchCriteria.ByAutomationId(automationId));
        }

        /// <summary>
        /// 查找下拉框
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>下拉框</returns>
        public static ComboBox FindComboBox(Window window, string automationId)
        {
            return window.Get<ComboBox>(SearchCriteria.ByAutomationId(automationId));
        }

        /// <summary>
        /// 查找数据网格
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>数据网格</returns>
        public static DataGrid FindDataGrid(Window window, string automationId)
        {
            return window.Get<DataGrid>(SearchCriteria.ByAutomationId(automationId));
        }

        /// <summary>
        /// 输入文本到文本框
        /// </summary>
        /// <param name="textBox">文本框</param>
        /// <param name="text">文本</param>
        public static void InputText(TextBox textBox, string text)
        {
            textBox.Click();
            textBox.SetValue(text);
        }

        /// <summary>
        /// 从下拉框选择项
        /// </summary>
        /// <param name="comboBox">下拉框</param>
        /// <param name="itemText">项文本</param>
        public static void SelectComboBoxItem(ComboBox comboBox, string itemText)
        {
            comboBox.Select(itemText);
        }

        /// <summary>
        /// 点击按钮
        /// </summary>
        /// <param name="button">按钮</param>
        public static void ClickButton(Button button)
        {
            button.Click();
        }

        /// <summary>
        /// 等待指定时间
        /// </summary>
        /// <param name="milliseconds">毫秒数</param>
        public static void Wait(int milliseconds = 1000)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// 模拟键盘输入
        /// </summary>
        /// <param name="keys">按键</param>
        public static void SendKeys(KeyboardInput.SpecialKeys keys)
        {
            KeyboardInput.KeyPress(keys);
        }

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">文本</param>
        public static void SendText(string text)
        {
            foreach (char c in text)
            {
                KeyboardInput.KeyIn(c.ToString());
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// 验证元素是否存在
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="automationId">AutomationId</param>
        /// <returns>是否存在</returns>
        public static bool ElementExists(Window window, string automationId)
        {
            try
            {
                window.Get(SearchCriteria.ByAutomationId(automationId));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证元素文本内容
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="expectedText">期望文本</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyText(IUIItem element, string expectedText)
        {
            try
            {
                var actualText = element.GetElement().Current.Name;
                return actualText.Equals(expectedText, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 截取窗口截图
        /// </summary>
        /// <param name="window">窗口</param>
        /// <param name="filePath">文件路径</param>
        public static void CaptureWindowScreenshot(Window window, string filePath)
        {
            try
            {
                var bounds = window.GetElement().Current.BoundingRectangle;
                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                    }
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"截取截图失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证对话框模式状态
        /// </summary>
        /// <param name="window">窗口</param>
        /// <returns>是否为模式对话框</returns>
        public static bool IsModal(Window window)
        {
            return window.IsModal;
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        /// <param name="window">窗口</param>
        public static void CloseDialog(Window window)
        {
            window.Close();
        }

        /// <summary>
        /// 获取当前主窗口
        /// </summary>
        /// <returns>主窗口</returns>
        public static Window GetMainWindow()
        {
            return _mainWindow;
        }

        /// <summary>
        /// 获取当前应用程序
        /// </summary>
        /// <returns>应用程序</returns>
        public static Application GetApplication()
        {
            return _application;
        }
    }
}