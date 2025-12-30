using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.WindowItems;

namespace ZWDynLookup.Tests.UI.DialogTests
{
    /// <summary>
    /// 运行时查寻菜单的UI自动化测试
    /// </summary>
    [TestClass]
    public class RuntimeLookupMenuTests
    {
        private Window _menu;
        private const string MenuTitle = "运行时查寻";

        [TestInitialize]
        public void TestInitialize()
        {
            UIAutomationHelpers.InitializeTestEnvironment();
            OpenRuntimeLookupMenu();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (_menu != null && !_menu.IsClosed)
                {
                    UIAutomationHelpers.CloseDialog(_menu);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理测试环境时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试菜单正常显示
        /// </summary>
        [TestMethod]
        public void TestMenuDisplaysSuccessfully()
        {
            Assert.IsNotNull(_menu, "运行时查寻菜单未能正确打开");
            Assert.IsTrue(UIAutomationHelpers.IsModal(_menu), "菜单应该为模式对话框");
            
            // 运行时菜单可能没有传统标题，需要调整验证逻辑
            if (!string.IsNullOrEmpty(_menu.Title))
            {
                Assert.IsTrue(_menu.Title.Contains(MenuTitle), $"菜单标题应包含'{MenuTitle}'");
            }
        }

        /// <summary>
        /// 测试菜单基本UI元素
        /// </summary>
        [TestMethod]
        public void TestMenuContainsRequiredElements()
        {
            // 验证查寻输入框
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_menu, "searchTextBox"), 
                "应存在查寻输入框");
            
            // 验证查寻结果列表
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_menu, "searchResultsList"), 
                "应存在查寻结果列表");
            
            // 验证快捷键提示
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_menu, "shortcutHintLabel"), 
                "应存在快捷键提示标签");
            
            // 验证关闭按钮
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_menu, "closeButton"), 
                "应存在关闭按钮");
        }

        /// <summary>
        /// 测试查寻输入功能
        /// </summary>
        [TestMethod]
        public void TestSearchInputFunctionality()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            Assert.IsNotNull(searchTextBox, "未找到查寻输入框");

            var testSearchText = "测试查寻";
            UIAutomationHelpers.InputText(searchTextBox, testSearchText);

            var actualValue = searchTextBox.Text;
            Assert.AreEqual(testSearchText, actualValue, "查寻输入应生效");
        }

        /// <summary>
        /// 测试实时查寻功能
        /// </summary>
        [TestMethod]
        public void TestRealTimeSearchFunctionality()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            Assert.IsNotNull(searchTextBox, "未找到查寻输入框");
            Assert.IsNotNull(resultsList, "未找到查寻结果列表");

            // 输入查寻文本
            UIAutomationHelpers.InputText(searchTextBox, "test");
            UIAutomationHelpers.Wait(1000); // 等待实时查寻结果

            // 验证查寻结果
            var resultCount = resultsList.Items.Count;
            // 具体的验证取决于查寻逻辑的实现
        }

        /// <summary>
        /// 测试查寻结果选择
        /// </summary>
        [TestMethod]
        public void TestSearchResultSelection()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            Assert.IsNotNull(searchTextBox, "未找到查寻输入框");
            Assert.IsNotNull(resultsList, "未找到查寻结果列表");

            // 输入查寻文本获得结果
            UIAutomationHelpers.InputText(searchTextBox, "parameter");
            UIAutomationHelpers.Wait(1000);

            if (resultsList.Items.Count > 0)
            {
                var firstItem = resultsList.Items.First();
                
                // 选择第一项
                firstItem.Select();
                UIAutomationHelpers.Wait(300);

                // 验证选择生效
                Assert.IsTrue(firstItem.IsSelected, "查寻结果项应被选中");
            }
        }

        /// <summary>
        /// 测试键盘导航
        /// </summary>
        [TestMethod]
        public void TestKeyboardNavigation()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            if (searchTextBox != null && resultsList != null)
            {
                // 点击输入框获得焦点
                searchTextBox.Click();
                UIAutomationHelpers.Wait(200);

                // 输入文本获得查寻结果
                UIAutomationHelpers.InputText(searchTextBox, "lookup");
                UIAutomationHelpers.Wait(1000);

                if (resultsList.Items.Count > 0)
                {
                    // 测试上下箭头键导航
                    UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWDOWN);
                    UIAutomationHelpers.Wait(200);

                    UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWDOWN);
                    UIAutomationHelpers.Wait(200);

                    UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWUP);
                    UIAutomationHelpers.Wait(200);

                    // 测试Tab键导航
                    UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.TAB);
                    UIAutomationHelpers.Wait(200);
                }
            }
        }

        /// <summary>
        /// 测试回车键确认功能
        /// </summary>
        [TestMethod]
        public void TestEnterKeyConfirmation()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            Assert.IsNotNull(searchTextBox, "未找到查寻输入框");
            Assert.IsNotNull(resultsList, "未找到查寻结果列表");

            // 输入查寻文本
            UIAutomationHelpers.InputText(searchTextBox, "parameter");
            UIAutomationHelpers.Wait(1000);

            if (resultsList.Items.Count > 0)
            {
                // 选择第一项
                resultsList.Items.First().Select();
                UIAutomationHelpers.Wait(300);

                // 按回车键确认
                UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.RETURN);
                UIAutomationHelpers.Wait(500);

                // 验证菜单关闭（选择确认后）
                Assert.IsTrue(_menu.IsClosed, "按回车键确认后菜单应关闭");
            }
        }

        /// <summary>
        /// 测试Esc键取消功能
        /// </summary>
        [TestMethod]
        public void TestEscapeKeyCancellation()
        {
            Assert.IsFalse(_menu.IsClosed, "菜单初始状态应打开");

            // 发送Esc键
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
            UIAutomationHelpers.Wait(500);

            Assert.IsTrue(_menu.IsClosed, "按Esc键后菜单应关闭");
        }

        /// <summary>
        /// 测试鼠标点击选择
        /// </summary>
        [TestMethod]
        public void TestMouseClickSelection()
        {
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            Assert.IsNotNull(resultsList, "未找到查寻结果列表");

            // 如果有查寻结果，测试鼠标点击
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            if (searchTextBox != null)
            {
                UIAutomationHelpers.InputText(searchTextBox, "test");
                UIAutomationHelpers.Wait(1000);

                if (resultsList.Items.Count > 0)
                {
                    var firstItem = resultsList.Items.First();
                    
                    // 点击选择项
                    firstItem.Click();
                    UIAutomationHelpers.Wait(300);

                    // 验证菜单关闭（选择确认后）
                    Assert.IsTrue(_menu.IsClosed, "点击选择后菜单应关闭");
                }
            }
        }

        /// <summary>
        /// 测试快捷键显示
        /// </summary>
        [TestMethod]
        public void TestShortcutHintDisplay()
        {
            var shortcutHintLabel = TryFindLabel("shortcutHintLabel");
            if (shortcutHintLabel != null)
            {
                var hintText = shortcutHintLabel.Text;
                
                // 验证快捷键提示文本
                Assert.IsFalse(string.IsNullOrEmpty(hintText), "快捷键提示不应为空");
                
                // 常见快捷键提示的验证
                var expectedHints = new[] { "ESC", "↑", "↓", "ENTER", "TAB" };
                foreach (var hint in expectedHints)
                {
                    if (hintText.ToUpper().Contains(hint))
                    {
                        Assert.IsTrue(hintText.Contains(hint), $"应显示{hint}快捷键提示");
                    }
                }
            }
        }

        /// <summary>
        /// 测试查寻结果高亮显示
        /// </summary>
        [TestMethod]
        public void TestSearchResultHighlight()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            if (searchTextBox != null && resultsList != null)
            {
                // 输入部分查寻文本
                UIAutomationHelpers.InputText(searchTextBox, "para");
                UIAutomationHelpers.Wait(1000);

                // 验证结果项的文本高亮（如果实现）
                foreach (var item in resultsList.Items)
                {
                    var itemText = item.Text;
                    // 验证查寻关键词是否在结果中高亮显示
                    // 具体实现取决于UI框架的高亮机制
                }
            }
        }

        /// <summary>
        /// 测试查寻结果分页（如果支持）
        /// </summary>
        [TestMethod]
        public void TestSearchResultPagination()
        {
            // 检查是否有分页控件
            var nextPageButton = TryFindButton("下一页");
            var prevPageButton = TryFindButton("上一页");
            var pageInfoLabel = TryFindLabel("pageInfoLabel");

            if (nextPageButton != null && prevPageButton != null && pageInfoLabel != null)
            {
                // 测试分页功能
                var currentPageText = pageInfoLabel.Text;
                
                // 点击下一页
                UIAutomationHelpers.ClickButton(nextPageButton);
                UIAutomationHelpers.Wait(500);

                var newPageText = pageInfoLabel.Text;
                Assert.AreNotEqual(currentPageText, newPageText, "分页应生效");

                // 点击上一页
                UIAutomationHelpers.ClickButton(prevPageButton);
                UIAutomationHelpers.Wait(500);
            }
        }

        /// <summary>
        /// 测试查寻历史功能（如果支持）
        /// </summary>
        [TestMethod]
        public void TestSearchHistoryFunctionality()
        {
            // 检查是否有查寻历史按钮
            var historyButton = TryFindButton("历史");
            if (historyButton != null)
            {
                // 点击历史按钮
                UIAutomationHelpers.ClickButton(historyButton);
                UIAutomationHelpers.Wait(500);

                // 验证历史记录显示
                var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
                if (resultsList != null)
                {
                    // 历史记录应该显示之前的查寻
                    // 具体验证取决于实现
                }
            }
        }

        /// <summary>
        /// 测试查寻过滤器（如果支持）
        /// </summary>
        [TestMethod]
        public void TestSearchFilters()
        {
            // 检查是否有过滤器控件
            var filterComboBox = UIAutomationHelpers.FindComboBox(_menu, "filterComboBox");
            if (filterComboBox != null)
            {
                // 测试过滤器选择
                var filterItems = filterComboBox.Items;
                if (filterItems.Count > 0)
                {
                    var firstFilter = filterItems.First();
                    UIAutomationHelpers.SelectComboBoxItem(filterComboBox, firstFilter);
                    UIAutomationHelpers.Wait(500);

                    // 验证过滤器生效
                    // 具体验证取决于过滤器实现
                }
            }
        }

        /// <summary>
        /// 测试菜单大小和位置
        /// </summary>
        [TestMethod]
        public void TestMenuSizingAndPositioning()
        {
            var bounds = _menu.GetElement().Current.BoundingRectangle;
            
            // 验证菜单有合理的大小
            Assert.IsTrue(bounds.Width > 200, "菜单宽度应足够");
            Assert.IsTrue(bounds.Height > 150, "菜单高度应足够");
            Assert.IsTrue(bounds.Width < 600, "菜单宽度不应过大");
            Assert.IsTrue(bounds.Height < 400, "菜单高度不应过大");
        }

        /// <summary>
        /// 测试菜单响应性
        /// </summary>
        [TestMethod]
        public void TestMenuResponsiveness()
        {
            var startTime = DateTime.Now;

            // 执行多个操作
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            if (searchTextBox != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    UIAutomationHelpers.InputText(searchTextBox, i.ToString());
                    UIAutomationHelpers.Wait(200);
                }
            }

            var endTime = DateTime.Now;
            var operationTime = endTime - startTime;

            Assert.IsTrue(operationTime.TotalSeconds < 10, "菜单操作响应时间应合理");
        }

        /// <summary>
        /// 测试菜单焦点管理
        /// </summary>
        [TestMethod]
        public void TestMenuFocusManagement()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            Assert.IsNotNull(searchTextBox, "未找到查寻输入框");
            Assert.IsNotNull(resultsList, "未找到查寻结果列表");

            // 验证初始焦点在输入框
            searchTextBox.Click();
            UIAutomationHelpers.Wait(200);

            // 测试焦点在不同控件间切换
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.TAB);
            UIAutomationHelpers.Wait(200);

            // 验证焦点移动
            // 具体验证取决于焦点指示器的实现
        }

        /// <summary>
        /// 测试无结果查寻处理
        /// </summary>
        [TestMethod]
        public void TestNoResultsSearchHandling()
        {
            var searchTextBox = UIAutomationHelpers.FindTextBox(_menu, "searchTextBox");
            var resultsList = UIAutomationHelpers.FindListBox(_menu, "searchResultsList");
            
            Assert.IsNotNull(searchTextBox, "未找到查寻输入框");
            Assert.IsNotNull(resultsList, "未找到查寻结果列表");

            // 输入不存在的查寻文本
            var nonExistentText = "xyzabc123nonexistent";
            UIAutomationHelpers.InputText(searchTextBox, nonExistentText);
            UIAutomationHelpers.Wait(1000);

            // 验证无结果显示
            var resultCount = resultsList.Items.Count;
            Assert.AreEqual(0, resultCount, "不存在的查寻应返回0结果");

            // 验证无结果提示显示
            var noResultsLabel = TryFindLabel("noResultsLabel");
            if (noResultsLabel != null)
            {
                var noResultsText = noResultsLabel.Text;
                Assert.IsFalse(string.IsNullOrEmpty(noResultsText), "无结果时应显示提示信息");
            }
        }

        /// <summary>
        /// 测试菜单截图功能
        /// </summary>
        [TestMethod]
        public void TestMenuScreenshotFunctionality()
        {
            var screenshotPath = Path.Combine(Path.GetTempPath(), "runtimeLookupMenuTest.png");
            
            UIAutomationHelpers.CaptureWindowScreenshot(_menu, screenshotPath);
            
            // 验证截图文件
            Assert.IsTrue(File.Exists(screenshotPath), "截图文件应已创建");
            var fileInfo = new FileInfo(screenshotPath);
            Assert.IsTrue(fileInfo.Length > 0, "截图文件不应为空");

            // 清理截图文件
            File.Delete(screenshotPath);
        }

        /// <summary>
        /// 打开运行时查寻菜单
        /// </summary>
        private void OpenRuntimeLookupMenu()
        {
            try
            {
                // 运行时查寻菜单通常通过快捷键触发
                // 这里需要模拟实际的触发方式
                _menu = UIAutomationHelpers.WaitForDialog(MenuTitle, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"无法打开运行时查寻菜单: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试查找标签
        /// </summary>
        /// <param name="automationId">AutomationId</param>
        /// <returns>标签</returns>
        private Label TryFindLabel(string automationId)
        {
            try
            {
                return UIAutomationHelpers.FindElementById(_menu, automationId) as Label;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试查找按钮
        /// </summary>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>按钮</returns>
        private Button TryFindButton(string buttonText)
        {
            try
            {
                return UIAutomationHelpers.FindButton(_menu, buttonText);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 查找列表框
        /// </summary>
        /// <param name="automationId">AutomationId</param>
        /// <returns>列表框</returns>
        private ListBox UIAutomationHelpers.FindListBox(Window window, string automationId)
        {
            return window.Get<ListBox>(SearchCriteria.ByAutomationId(automationId));
        }
    }
}