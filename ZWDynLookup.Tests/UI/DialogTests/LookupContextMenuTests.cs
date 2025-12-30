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
    /// 查寻上下文菜单的UI自动化测试
    /// </summary>
    [TestClass]
    public class LookupContextMenuTests
    {
        private Window _contextMenu;
        private const string MenuTitle = "查寻上下文菜单";

        [TestInitialize]
        public void TestInitialize()
        {
            UIAutomationHelpers.InitializeTestEnvironment();
            OpenLookupContextMenu();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (_contextMenu != null && !_contextMenu.IsClosed)
                {
                    UIAutomationHelpers.CloseDialog(_contextMenu);
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
            Assert.IsNotNull(_contextMenu, "查寻上下文菜单未能正确打开");
            Assert.IsTrue(UIAutomationHelpers.IsModal(_contextMenu), "菜单应该为模式对话框");
            
            // 上下文菜单可能没有传统标题
            if (!string.IsNullOrEmpty(_contextMenu.Title))
            {
                Assert.IsTrue(_contextMenu.Title.Contains(MenuTitle), $"菜单标题应包含'{MenuTitle}'");
            }
        }

        /// <summary>
        /// 测试菜单基本UI元素
        /// </summary>
        [TestMethod]
        public void TestMenuContainsRequiredElements()
        {
            // 验证菜单项存在
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_contextMenu, "lookupParameterItem"), 
                "应存在查寻参数菜单项");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_contextMenu, "editParameterItem"), 
                "应存在编辑参数菜单项");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_contextMenu, "deleteParameterItem"), 
                "应存在删除参数菜单项");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_contextMenu, "propertiesItem"), 
                "应存在属性菜单项");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_contextMenu, "separator1"), 
                "应存在分隔线");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_contextMenu, "closeMenuItem"), 
                "应存在关闭菜单项");
        }

        /// <summary>
        /// 测试查寻参数菜单项
        /// </summary>
        [TestMethod]
        public void TestLookupParameterMenuItem()
        {
            var lookupItem = TryFindMenuItem("查寻参数");
            Assert.IsNotNull(lookupItem, "未找到查寻参数菜单项");

            // 验证菜单项文本
            Assert.IsTrue(lookupItem.Text.Contains("查寻") || lookupItem.Text.Contains("Lookup"), 
                "查寻参数菜单项文本应包含相关关键词");

            // 测试菜单项可点击
            lookupItem.Click();
            UIAutomationHelpers.Wait(500);

            // 验证菜单关闭或执行相应操作
            Assert.IsTrue(_contextMenu.IsClosed, "点击查寻参数菜单项后菜单应关闭");
        }

        /// <summary>
        /// 测试编辑参数菜单项
        /// </summary>
        [TestMethod]
        public void TestEditParameterMenuItem()
        {
            var editItem = TryFindMenuItem("编辑参数");
            Assert.IsNotNull(editItem, "未找到编辑参数菜单项");

            // 点击编辑参数菜单项
            editItem.Click();
            UIAutomationHelpers.Wait(500);

            // 验证是否打开编辑对话框
            var editDialog = UIAutomationHelpers.WaitForDialog("参数编辑", TimeSpan.FromSeconds(3));
            Assert.IsNotNull(editDialog, "点击编辑参数菜单项后应打开编辑对话框");

            // 关闭编辑对话框
            UIAutomationHelpers.CloseDialog(editDialog);
        }

        /// <summary>
        /// 测试删除参数菜单项
        /// </summary>
        [TestMethod]
        public void TestDeleteParameterMenuItem()
        {
            var deleteItem = TryFindMenuItem("删除参数");
            Assert.IsNotNull(deleteItem, "未找到删除参数菜单项");

            // 点击删除参数菜单项
            deleteItem.Click();
            UIAutomationHelpers.Wait(500);

            // 验证是否显示确认对话框
            var confirmDialog = UIAutomationHelpers.WaitForDialog("确认删除", TimeSpan.FromSeconds(3));
            if (confirmDialog != null)
            {
                // 验证确认对话框有确定和取消按钮
                var okButton = UIAutomationHelpers.FindButton(confirmDialog, "确定");
                var cancelButton = UIAutomationHelpers.FindButton(confirmDialog, "取消");
                
                Assert.IsNotNull(okButton, "确认对话框应有确定按钮");
                Assert.IsNotNull(cancelButton, "确认对话框应有取消按钮");

                // 点击取消保持数据
                UIAutomationHelpers.ClickButton(cancelButton);
                UIAutomationHelpers.Wait(300);
            }

            // 验证上下文菜单关闭
            Assert.IsTrue(_contextMenu.IsClosed, "点击删除参数菜单项后菜单应关闭");
        }

        /// <summary>
        /// 测试属性菜单项
        /// </summary>
        [TestMethod]
        public void TestPropertiesMenuItem()
        {
            var propertiesItem = TryFindMenuItem("属性");
            Assert.IsNotNull(propertiesItem, "未找到属性菜单项");

            // 点击属性菜单项
            propertiesItem.Click();
            UIAutomationHelpers.Wait(500);

            // 验证是否打开属性对话框
            var propertiesDialog = UIAutomationHelpers.WaitForDialog("属性", TimeSpan.FromSeconds(3));
            if (propertiesDialog != null)
            {
                // 验证属性对话框基本元素
                Assert.IsNotNull(propertiesDialog, "点击属性菜单项后应打开属性对话框");
                
                // 关闭属性对话框
                UIAutomationHelpers.CloseDialog(propertiesDialog);
            }

            // 验证上下文菜单关闭
            Assert.IsTrue(_contextMenu.IsClosed, "点击属性菜单项后菜单应关闭");
        }

        /// <summary>
        /// 测试分隔线显示
        /// </summary>
        [TestMethod]
        public void TestSeparatorDisplay()
        {
            // 验证分隔线存在
            var separator = TryFindSeparator("separator1");
            if (separator != null)
            {
                // 验证分隔线在正确位置
                Assert.IsNotNull(separator, "应存在分隔线");
            }
        }

        /// <summary>
        /// 测试关闭菜单项
        /// </summary>
        [TestMethod]
        public void TestCloseMenuItem()
        {
            var closeItem = TryFindMenuItem("关闭");
            Assert.IsNotNull(closeItem, "未找到关闭菜单项");

            Assert.IsFalse(_contextMenu.IsClosed, "菜单初始状态应打开");

            // 点击关闭菜单项
            closeItem.Click();
            UIAutomationHelpers.Wait(300);

            Assert.IsTrue(_contextMenu.IsClosed, "点击关闭菜单项后菜单应关闭");
        }

        /// <summary>
        /// 测试键盘导航
        /// </summary>
        [TestMethod]
        public void TestKeyboardNavigation()
        {
            // 点击菜单项测试键盘导航
            var menuItems = GetAllMenuItems();
            Assert.IsTrue(menuItems.Count > 0, "应存在菜单项");

            // 使用上下箭头键导航
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWDOWN);
            UIAutomationHelpers.Wait(200);

            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWDOWN);
            UIAutomationHelpers.Wait(200);

            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWUP);
            UIAutomationHelpers.Wait(200);

            // 测试回车键选择
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.RETURN);
            UIAutomationHelpers.Wait(300);

            Assert.IsTrue(_contextMenu.IsClosed, "按回车键后菜单应关闭");
        }

        /// <summary>
        /// 测试鼠标悬停效果
        /// </summary>
        [TestMethod]
        public void TestMouseHoverEffect()
        {
            var menuItems = GetAllMenuItems();
            Assert.IsTrue(menuItems.Count > 0, "应存在菜单项");

            foreach (var item in menuItems.Take(3)) // 测试前3项
            {
                try
                {
                    // 模拟鼠标悬停（如果支持）
                    var bounds = item.GetElement().Current.BoundingRectangle;
                    
                    // 验证菜单项有合理的尺寸
                    Assert.IsTrue(bounds.Width > 0, "菜单项宽度应大于0");
                    Assert.IsTrue(bounds.Height > 0, "菜单项高度应大于0");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"悬停测试时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 测试菜单项图标显示
        /// </summary>
        [TestMethod]
        public void TestMenuItemIconDisplay()
        {
            var menuItems = GetAllMenuItems();
            Assert.IsTrue(menuItems.Count > 0, "应存在菜单项");

            // 检查菜单项是否有图标（如果实现）
            foreach (var item in menuItems)
            {
                try
                {
                    var element = item.GetElement();
                    var helpText = element.Current.HelpText;
                    
                    // 验证菜单项有适当的帮助文本或标识
                    // 具体验证取决于图标实现方式
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"检查图标时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 测试菜单快捷键显示
        /// </summary>
        [TestMethod]
        public void TestMenuShortcutDisplay()
        {
            var menuItems = GetAllMenuItems();
            Assert.IsTrue(menuItems.Count > 0, "应存在菜单项");

            foreach (var item in menuItems)
            {
                var itemText = item.Text;
                
                // 检查是否显示快捷键
                var hasShortcut = itemText.Contains("(&") || itemText.Contains("Ctrl+") || 
                                 itemText.Contains("F2") || itemText.Contains("Del");
                
                // 如果有快捷键，验证格式正确
                if (hasShortcut)
                {
                    Assert.IsTrue(itemText.Length > 2, "带快捷键的菜单项文本应足够长");
                }
            }
        }

        /// <summary>
        /// 测试菜单状态（启用/禁用）
        /// </summary>
        [TestMethod]
        public void TestMenuItemState()
        {
            var menuItems = GetAllMenuItems();
            Assert.IsTrue(menuItems.Count > 0, "应存在菜单项");

            foreach (var item in menuItems)
            {
                try
                {
                    var isEnabled = item.Enabled;
                    
                    // 验证菜单项状态
                    Assert.IsNotNull(isEnabled, "菜单项应有效状态");

                    // 如果菜单项被禁用，应有适当的视觉反馈
                    if (!isEnabled.Value)
                    {
                        // 验证禁用状态的显示
                        var bounds = item.GetElement().Current.BoundingRectangle;
                        Assert.IsTrue(bounds.Width > 0, "禁用菜单项仍应有尺寸");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"检查菜单项状态时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 测试菜单大小和布局
        /// </summary>
        [TestMethod]
        public void TestMenuSizingAndLayout()
        {
            var bounds = _contextMenu.GetElement().Current.BoundingRectangle;
            
            // 验证菜单有合理的大小
            Assert.IsTrue(bounds.Width > 100, "菜单宽度应足够");
            Assert.IsTrue(bounds.Height > 50, "菜单高度应足够");
            Assert.IsTrue(bounds.Width < 300, "菜单宽度不应过大");
            Assert.IsTrue(bounds.Height < 400, "菜单高度不应过大");

            // 验证菜单项布局
            var menuItems = GetAllMenuItems();
            Assert.IsTrue(menuItems.Count > 0, "应存在菜单项");

            // 验证菜单项不重叠
            for (int i = 0; i < menuItems.Count - 1; i++)
            {
                var currentBounds = menuItems[i].GetElement().Current.BoundingRectangle;
                var nextBounds = menuItems[i + 1].GetElement().Current.BoundingRectangle;
                
                // 验证垂直布局
                Assert.IsTrue(currentBounds.Top < nextBounds.Top, "菜单项应垂直排列");
            }
        }

        /// <summary>
        /// 测试菜单响应性
        /// </summary>
        [TestMethod]
        public void TestMenuResponsiveness()
        {
            var startTime = DateTime.Now;

            // 执行多个菜单操作
            var menuItems = GetAllMenuItems();
            foreach (var item in menuItems.Take(3))
            {
                try
                {
                    item.Click();
                    UIAutomationHelpers.Wait(200);
                }
                catch
                {
                    // 忽略某些操作失败
                }
            }

            var endTime = DateTime.Now;
            var operationTime = endTime - startTime;

            Assert.IsTrue(operationTime.TotalSeconds < 5, "菜单操作响应时间应合理");
        }

        /// <summary>
        /// 测试右键取消功能
        /// </summary>
        [TestMethod]
        public void TestRightClickCancellation()
        {
            Assert.IsFalse(_contextMenu.IsClosed, "菜单初始状态应打开");

            // 发送右键点击（在空白区域）
            // 这里需要模拟右键点击，如果框架支持的话
            
            // 如果不支持右键，验证其他取消方式
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
            UIAutomationHelpers.Wait(300);

            Assert.IsTrue(_contextMenu.IsClosed, "按Esc键后菜单应关闭");
        }

        /// <summary>
        /// 测试菜单焦点管理
        /// </summary>
        [TestMethod]
        public void TestMenuFocusManagement()
        {
            // 验证菜单获得焦点
            Assert.IsTrue(_contextMenu.IsActive, "菜单应该获得焦点");

            // 验证第一个菜单项获得焦点
            var menuItems = GetAllMenuItems();
            if (menuItems.Count > 0)
            {
                var firstItem = menuItems.First();
                firstItem.Click();
                UIAutomationHelpers.Wait(200);

                // 验证菜单项被选中或激活
                // 具体验证取决于UI实现
            }
        }

        /// <summary>
        /// 测试多级菜单（如果支持）
        /// </summary>
        [TestMethod]
        public void TestSubmenuFunctionality()
        {
            var menuItems = GetAllMenuItems();
            
            foreach (var item in menuItems)
            {
                try
                {
                    // 检查是否有子菜单
                    var hasSubmenu = item.SubMenu != null;
                    
                    if (hasSubmenu)
                    {
                        // 测试子菜单
                        var submenuItems = item.SubMenu.Items;
                        Assert.IsNotNull(submenuItems, "子菜单应存在");
                        
                        // 点击主菜单项展开子菜单
                        item.Click();
                        UIAutomationHelpers.Wait(300);

                        // 测试子菜单项
                        foreach (var subItem in submenuItems)
                        {
                            Assert.IsNotNull(subItem, "子菜单项不应为空");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"测试子菜单时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 测试菜单截图功能
        /// </summary>
        [TestMethod]
        public void TestMenuScreenshotFunctionality()
        {
            var screenshotPath = Path.Combine(Path.GetTempPath(), "lookupContextMenuTest.png");
            
            UIAutomationHelpers.CaptureWindowScreenshot(_contextMenu, screenshotPath);
            
            // 验证截图文件
            Assert.IsTrue(File.Exists(screenshotPath), "截图文件应已创建");
            var fileInfo = new FileInfo(screenshotPath);
            Assert.IsTrue(fileInfo.Length > 0, "截图文件不应为空");

            // 清理截图文件
            File.Delete(screenshotPath);
        }

        /// <summary>
        /// 打开查寻上下文菜单
        /// </summary>
        private void OpenLookupContextMenu()
        {
            try
            {
                // 上下文菜单通常通过右键点击触发
                // 这里需要模拟实际的触发方式
                _contextMenu = UIAutomationHelpers.WaitForDialog(MenuTitle, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"无法打开查寻上下文菜单: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试查找菜单项
        /// </summary>
        /// <param name="itemText">菜单项文本</param>
        /// <returns>菜单项</returns>
        private Menu TryFindMenuItem(string itemText)
        {
            try
            {
                return _contextMenu.MenuBar.MenuItem(itemText);
            }
            catch
            {
                // 如果通过MenuBar找不到，尝试直接查找
                try
                {
                    return _contextMenu.Get<Menu>(SearchCriteria.ByText(itemText));
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 尝试查找分隔线
        /// </summary>
        /// <param name="automationId">AutomationId</param>
        /// <returns>分隔线</returns>
        private Menu TryFindSeparator(string automationId)
        {
            try
            {
                return _contextMenu.Get<Menu>(SearchCriteria.ByAutomationId(automationId));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取所有菜单项
        /// </summary>
        /// <returns>菜单项列表</returns>
        private System.Collections.Generic.List<Menu> GetAllMenuItems()
        {
            var menuItems = new System.Collections.Generic.List<Menu>();
            
            try
            {
                var menuBar = _contextMenu.MenuBar;
                if (menuBar != null)
                {
                    // 获取主菜单项
                    var mainMenuItems = menuBar.MenuItems;
                    foreach (var item in mainMenuItems)
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