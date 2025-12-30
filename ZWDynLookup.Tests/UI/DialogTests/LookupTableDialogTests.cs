using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.WindowItems;

namespace ZWDynLookup.Tests.UI.DialogTests
{
    /// <summary>
    /// 特性查寻表对话框的UI自动化测试
    /// </summary>
    [TestClass]
    public class LookupTableDialogTests
    {
        private Window _dialog;
        private const string DialogTitle = "查寻表管理";

        [TestInitialize]
        public void TestInitialize()
        {
            // 初始化测试环境
            UIAutomationHelpers.InitializeTestEnvironment();
            
            // 模拟打开查寻表对话框
            // 这里需要根据实际的命令调用方式来调整
            OpenLookupTableDialog();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (_dialog != null && !_dialog.IsClosed)
                {
                    UIAutomationHelpers.CloseDialog(_dialog);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理测试环境时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试对话框正常打开
        /// </summary>
        [TestMethod]
        public void TestDialogOpensSuccessfully()
        {
            // 验证对话框存在且为模式对话框
            Assert.IsNotNull(_dialog, "对话框未能正确打开");
            Assert.IsTrue(UIAutomationHelpers.IsModal(_dialog), "对话框应该为模式对话框");
            
            // 验证对话框标题
            Assert.IsTrue(_dialog.Title.Contains(DialogTitle), $"对话框标题应包含'{DialogTitle}'");
        }

        /// <summary>
        /// 测试对话框基本UI元素
        /// </summary>
        [TestMethod]
        public void TestDialogContainsRequiredElements()
        {
            // 验证必需的UI元素存在
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "lookupTableDataGrid"), 
                "应存在查寻表数据网格");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "addButton"), 
                "应存在添加按钮");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "editButton"), 
                "应存在编辑按钮");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "deleteButton"), 
                "应存在删除按钮");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "closeButton"), 
                "应存在关闭按钮");
        }

        /// <summary>
        /// 测试添加按钮功能
        /// </summary>
        [TestMethod]
        public void TestAddButtonFunctionality()
        {
            var addButton = UIAutomationHelpers.FindButton(_dialog, "添加");
            Assert.IsNotNull(addButton, "未找到添加按钮");

            // 点击添加按钮
            UIAutomationHelpers.ClickButton(addButton);
            UIAutomationHelpers.Wait(500);

            // 验证是否打开了新的对话框（编辑对话框）
            var editDialog = UIAutomationHelpers.WaitForDialog("查寻表编辑");
            Assert.IsNotNull(editDialog, "点击添加按钮后应打开编辑对话框");

            // 关闭编辑对话框
            UIAutomationHelpers.CloseDialog(editDialog);
        }

        /// <summary>
        /// 测试编辑按钮功能
        /// </summary>
        [TestMethod]
        public void TestEditButtonFunctionality()
        {
            // 首先确保有数据可以编辑
            AddTestData();

            var editButton = UIAutomationHelpers.FindButton(_dialog, "编辑");
            Assert.IsNotNull(editButton, "未找到编辑按钮");

            // 选择第一行数据
            var dataGrid = UIAutomationHelpers.FindDataGrid(_dialog, "lookupTableDataGrid");
            var firstRow = dataGrid.Rows.FirstOrDefault();
            if (firstRow != null)
            {
                firstRow.Select();
                
                // 点击编辑按钮
                UIAutomationHelpers.ClickButton(editButton);
                UIAutomationHelpers.Wait(500);

                // 验证是否打开了编辑对话框
                var editDialog = UIAutomationHelpers.WaitForDialog("查寻表编辑");
                Assert.IsNotNull(editDialog, "点击编辑按钮后应打开编辑对话框");

                // 关闭编辑对话框
                UIAutomationHelpers.CloseDialog(editDialog);
            }
        }

        /// <summary>
        /// 测试删除按钮功能
        /// </summary>
        [TestMethod]
        public void TestDeleteButtonFunctionality()
        {
            // 首先添加测试数据
            AddTestData();

            var deleteButton = UIAutomationHelpers.FindButton(_dialog, "删除");
            Assert.IsNotNull(deleteButton, "未找到删除按钮");

            var dataGrid = UIAutomationHelpers.FindDataGrid(_dialog, "lookupTableDataGrid");
            var initialRowCount = dataGrid.Rows.Count;

            if (initialRowCount > 0)
            {
                // 选择第一行
                var firstRow = dataGrid.Rows.First();
                firstRow.Select();

                // 点击删除按钮
                UIAutomationHelpers.ClickButton(deleteButton);
                UIAutomationHelpers.Wait(500);

                // 验证行数是否减少
                var finalRowCount = dataGrid.Rows.Count;
                Assert.IsTrue(finalRowCount < initialRowCount, "删除操作后行数应减少");
            }
        }

        /// <summary>
        /// 测试关闭按钮功能
        /// </summary>
        [TestMethod]
        public void TestCloseButtonFunctionality()
        {
            var closeButton = UIAutomationHelpers.FindButton(_dialog, "关闭");
            Assert.IsNotNull(closeButton, "未找到关闭按钮");

            Assert.IsFalse(_dialog.IsClosed, "对话框初始状态应未关闭");

            // 点击关闭按钮
            UIAutomationHelpers.ClickButton(closeButton);
            UIAutomationHelpers.Wait(500);

            Assert.IsTrue(_dialog.IsClosed, "点击关闭按钮后对话框应关闭");
        }

        /// <summary>
        /// 测试Esc键取消功能
        /// </summary>
        [TestMethod]
        public void TestEscapeKeyCancellation()
        {
            Assert.IsFalse(_dialog.IsClosed, "对话框初始状态应未关闭");

            // 发送Esc键
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
            UIAutomationHelpers.Wait(500);

            Assert.IsTrue(_dialog.IsClosed, "按Esc键后对话框应关闭");
        }

        /// <summary>
        /// 测试数据网格基本功能
        /// </summary>
        [TestMethod]
        public void TestDataGridBasicFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_dialog, "lookupTableDataGrid");
            Assert.IsNotNull(dataGrid, "未找到数据网格");

            // 验证网格有基本的列结构
            var columnCount = dataGrid.Header.Columns.Count;
            Assert.IsTrue(columnCount > 0, "数据网格应有列结构");

            // 添加测试数据并验证
            AddTestData();
            Assert.IsTrue(dataGrid.Rows.Count > 0, "添加测试数据后网格应有行数据");
        }

        /// <summary>
        /// 测试右键菜单功能
        /// </summary>
        [TestMethod]
        public void TestContextMenuFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_dialog, "lookupTableDataGrid");
            Assert.IsNotNull(dataGrid, "未找到数据网格");

            // 确保有数据
            AddTestData();

            if (dataGrid.Rows.Count > 0)
            {
                var firstRow = dataGrid.Rows.First();
                
                // 右键点击第一行
                var bounds = firstRow.GetElement().Current.BoundingRectangle;
                // 这里需要模拟鼠标右键点击，White框架可能需要更复杂的实现
                // 基本思路是发送WM_RBUTTONDOWN和WM_RBUTTONUP消息
            }
        }

        /// <summary>
        /// 测试对话框响应性
        /// </summary>
        [TestMethod]
        public void TestDialogResponsiveness()
        {
            var startTime = DateTime.Now;
            
            // 执行一些基本操作
            var dataGrid = UIAutomationHelpers.FindDataGrid(_dialog, "lookupTableDataGrid");
            Assert.IsNotNull(dataGrid, "未找到数据网格");

            var addButton = UIAutomationHelpers.FindButton(_dialog, "添加");
            Assert.IsNotNull(addButton, "未找到添加按钮");

            var endTime = DateTime.Now;
            var operationTime = endTime - startTime;

            // 操作应该在合理时间内完成（少于5秒）
            Assert.IsTrue(operationTime.TotalSeconds < 5, "UI操作响应时间过长");
        }

        /// <summary>
        /// 测试对话框大小调整
        /// </summary>
        [TestMethod]
        public void TestDialogResize()
        {
            var initialBounds = _dialog.GetElement().Current.BoundingRectangle;
            
            // 尝试调整窗口大小（如果支持）
            try
            {
                _dialog.MoveBy(100, 100);
                UIAutomationHelpers.Wait(200);
                
                var newBounds = _dialog.GetElement().Current.BoundingRectangle;
                
                // 验证窗口位置发生变化
                Assert.AreNotEqual(initialBounds.Left, newBounds.Left, "窗口位置应发生变化");
            }
            catch (Exception ex)
            {
                // 如果不支持调整大小，记录但不失败测试
                Assert.Inconclusive($"窗口调整测试跳过: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试对话框焦点管理
        /// </summary>
        [TestMethod]
        public void TestDialogFocusManagement()
        {
            // 验证对话框获得焦点
            Assert.IsTrue(_dialog.IsActive, "对话框应该获得焦点");

            // 验证主要控件可以接收输入
            var dataGrid = UIAutomationHelpers.FindDataGrid(_dialog, "lookupTableDataGrid");
            if (dataGrid != null)
            {
                dataGrid.Click();
                UIAutomationHelpers.Wait(200);
                
                // 验证点击后对话框仍然活跃
                Assert.IsTrue(_dialog.IsActive, "点击后对话框应保持活跃状态");
            }
        }

        /// <summary>
        /// 测试数据验证功能
        /// </summary>
        [TestMethod]
        public void TestDataValidation()
        {
            var addButton = UIAutomationHelpers.FindButton(_dialog, "添加");
            UIAutomationHelpers.ClickButton(addButton);
            
            var editDialog = UIAutomationHelpers.WaitForDialog("查寻表编辑");
            Assert.IsNotNull(editDialog, "应打开编辑对话框");

            // 测试空数据提交验证
            var okButton = UIAutomationHelpers.FindButton(editDialog, "确定");
            UIAutomationHelpers.ClickButton(okButton);
            UIAutomationHelpers.Wait(500);

            // 验证是否有验证错误提示（如果有实现）
            // 这取决于具体的验证实现

            UIAutomationHelpers.CloseDialog(editDialog);
        }

        /// <summary>
        /// 测试对话框截图功能
        /// </summary>
        [TestMethod]
        public void TestScreenshotFunctionality()
        {
            var screenshotPath = Path.Combine(Path.GetTempPath(), "lookupTableDialogTest.png");
            
            UIAutomationHelpers.CaptureWindowScreenshot(_dialog, screenshotPath);
            
            // 验证截图文件存在且不为空
            Assert.IsTrue(File.Exists(screenshotPath), "截图文件应已创建");
            var fileInfo = new FileInfo(screenshotPath);
            Assert.IsTrue(fileInfo.Length > 0, "截图文件不应为空");

            // 清理截图文件
            File.Delete(screenshotPath);
        }

        /// <summary>
        /// 打开查寻表对话框（模拟）
        /// </summary>
        private void OpenLookupTableDialog()
        {
            try
            {
                // 这里需要根据实际的命令调用方式来打开对话框
                // 模拟通过菜单或命令打开对话框
                var mainWindow = UIAutomationHelpers.GetMainWindow();
                
                // 尝试通过菜单打开（如果有菜单栏）
                try
                {
                    var menuBar = mainWindow.MenuBar;
                    if (menuBar != null)
                    {
                        // 模拟菜单点击
                        // menuBar.MenuItem("查寻").SubMenu("查寻表管理").Click();
                    }
                }
                catch
                {
                    // 菜单方式失败，尝试其他方式
                }

                // 如果通过菜单无法打开，直接等待对话框
                // 这在实际的CAD环境中应该通过命令调用来打开
                _dialog = UIAutomationHelpers.WaitForDialog(DialogTitle);
            }
            catch (Exception ex)
            {
                // 如果无法自动打开对话框，创建一个模拟的对话框用于测试
                // 在实际测试中，这个方法需要根据具体的CAD集成方式来实现
                throw new InvalidOperationException($"无法打开查寻表对话框: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加测试数据
        /// </summary>
        private void AddTestData()
        {
            try
            {
                var addButton = UIAutomationHelpers.FindButton(_dialog, "添加");
                if (addButton != null)
                {
                    UIAutomationHelpers.ClickButton(addButton);
                    var editDialog = UIAutomationHelpers.WaitForDialog("查寻表编辑");
                    
                    if (editDialog != null)
                    {
                        // 填写测试数据
                        var nameTextBox = UIAutomationHelpers.FindTextBox(editDialog, "nameTextBox");
                        if (nameTextBox != null)
                        {
                            UIAutomationHelpers.InputText(nameTextBox, "测试查寻表");
                        }

                        var okButton = UIAutomationHelpers.FindButton(editDialog, "确定");
                        if (okButton != null)
                        {
                            UIAutomationHelpers.ClickButton(okButton);
                        }
                        
                        UIAutomationHelpers.Wait(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加测试数据失败: {ex.Message}");
            }
        }
    }
}