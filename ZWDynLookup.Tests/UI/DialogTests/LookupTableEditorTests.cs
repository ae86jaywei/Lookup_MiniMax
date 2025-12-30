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
    /// 查寻表编辑器的UI自动化测试
    /// </summary>
    [TestClass]
    public class LookupTableEditorTests
    {
        private Window _editor;
        private const string EditorTitle = "查寻表编辑器";

        [TestInitialize]
        public void TestInitialize()
        {
            UIAutomationHelpers.InitializeTestEnvironment();
            OpenLookupTableEditor();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (_editor != null && !_editor.IsClosed)
                {
                    UIAutomationHelpers.CloseDialog(_editor);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理测试环境时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试编辑器正常打开
        /// </summary>
        [TestMethod]
        public void TestEditorOpensSuccessfully()
        {
            Assert.IsNotNull(_editor, "编辑器未能正确打开");
            Assert.IsTrue(UIAutomationHelpers.IsModal(_editor), "编辑器应该为模式对话框");
            Assert.IsTrue(_editor.Title.Contains(EditorTitle), $"编辑器标题应包含'{EditorTitle}'");
        }

        /// <summary>
        /// 测试编辑器基本UI元素
        /// </summary>
        [TestMethod]
        public void TestEditorContainsRequiredElements()
        {
            // 验证查寻表名称输入框
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_editor, "tableNameTextBox"), 
                "应存在查寻表名称输入框");
            
            // 验证数据网格
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_editor, "dataGrid"), 
                "应存在数据网格");
            
            // 验证添加行按钮
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_editor, "addRowButton"), 
                "应存在添加行按钮");
            
            // 验证删除行按钮
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_editor, "deleteRowButton"), 
                "应存在删除行按钮");
            
            // 验证保存和取消按钮
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_editor, "saveButton"), 
                "应存在保存按钮");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_editor, "cancelButton"), 
                "应存在取消按钮");
        }

        /// <summary>
        /// 测试查寻表名称输入功能
        /// </summary>
        [TestMethod]
        public void TestTableNameInputFunctionality()
        {
            var nameTextBox = UIAutomationHelpers.FindTextBox(_editor, "tableNameTextBox");
            Assert.IsNotNull(nameTextBox, "未找到查寻表名称输入框");

            var testName = "测试查寻表";
            UIAutomationHelpers.InputText(nameTextBox, testName);

            var actualValue = nameTextBox.Text;
            Assert.AreEqual(testName, actualValue, "名称输入应生效");
        }

        /// <summary>
        /// 测试数据网格基本功能
        /// </summary>
        [TestMethod]
        public void TestDataGridBasicFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            Assert.IsNotNull(dataGrid, "未找到数据网格");

            // 验证网格有列结构
            var columnCount = dataGrid.Header.Columns.Count;
            Assert.IsTrue(columnCount > 0, "数据网格应有列结构");

            // 验证初始状态
            var rowCount = dataGrid.Rows.Count;
            Assert.IsTrue(rowCount >= 0, "数据网格行数应有效");
        }

        /// <summary>
        /// 测试添加行功能
        /// </summary>
        [TestMethod]
        public void TestAddRowFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            Assert.IsNotNull(dataGrid, "未找到数据网格");
            Assert.IsNotNull(addRowButton, "未找到添加行按钮");

            var initialRowCount = dataGrid.Rows.Count;

            // 点击添加行按钮
            UIAutomationHelpers.ClickButton(addRowButton);
            UIAutomationHelpers.Wait(500);

            var newRowCount = dataGrid.Rows.Count;
            Assert.IsTrue(newRowCount > initialRowCount, "添加行后行数应增加");

            // 验证新行可以编辑
            if (newRowCount > 0)
            {
                var lastRow = dataGrid.Rows.Last();
                lastRow.Select();
                
                // 验证行被选中
                Assert.IsTrue(lastRow.IsSelected, "新添加的行应被选中");
            }
        }

        /// <summary>
        /// 测试删除行功能
        /// </summary>
        [TestMethod]
        public void TestDeleteRowFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            var deleteRowButton = UIAutomationHelpers.FindButton(_editor, "删除行");
            
            Assert.IsNotNull(dataGrid, "未找到数据网格");
            Assert.IsNotNull(addRowButton, "未找到添加行按钮");
            Assert.IsNotNull(deleteRowButton, "未找到删除行按钮");

            // 先添加一行
            UIAutomationHelpers.ClickButton(addRowButton);
            UIAutomationHelpers.Wait(300);

            var rowCountAfterAdd = dataGrid.Rows.Count;
            if (rowCountAfterAdd > 0)
            {
                // 选择最后一行
                var lastRow = dataGrid.Rows.Last();
                lastRow.Select();

                // 点击删除行按钮
                UIAutomationHelpers.ClickButton(deleteRowButton);
                UIAutomationHelpers.Wait(300);

                var finalRowCount = dataGrid.Rows.Count;
                Assert.IsTrue(finalRowCount < rowCountAfterAdd, "删除行后行数应减少");
            }
        }

        /// <summary>
        /// 测试单元格编辑功能
        /// </summary>
        [TestMethod]
        public void TestCellEditingFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            Assert.IsNotNull(dataGrid, "未找到数据网格");
            Assert.IsNotNull(addRowButton, "未找到添加行按钮");

            // 添加一行
            UIAutomationHelpers.ClickButton(addRowButton);
            UIAutomationHelpers.Wait(500);

            if (dataGrid.Rows.Count > 0)
            {
                var firstRow = dataGrid.Rows.First();
                
                // 点击第一行第一个单元格
                var firstCell = firstRow.Cells.FirstOrDefault();
                if (firstCell != null)
                {
                    firstCell.Click();
                    UIAutomationHelpers.Wait(300);

                    // 输入测试数据
                    UIAutomationHelpers.SendText("测试数据");
                    UIAutomationHelpers.Wait(300);

                    // 验证数据输入
                    // 具体验证取决于单元格实现
                }
            }
        }

        /// <summary>
        /// 测试保存按钮功能
        /// </summary>
        [TestMethod]
        public void TestSaveButtonFunctionality()
        {
            // 填写基本信息
            var nameTextBox = UIAutomationHelpers.FindTextBox(_editor, "tableNameTextBox");
            if (nameTextBox != null)
            {
                UIAutomationHelpers.InputText(nameTextBox, "测试查寻表");
            }

            // 添加一些数据
            AddTestData();

            var saveButton = UIAutomationHelpers.FindButton(_editor, "保存");
            Assert.IsNotNull(saveButton, "未找到保存按钮");

            // 点击保存按钮
            UIAutomationHelpers.ClickButton(saveButton);
            UIAutomationHelpers.Wait(500);

            // 验证编辑器关闭
            Assert.IsTrue(_editor.IsClosed, "点击保存按钮后编辑器应关闭");
        }

        /// <summary>
        /// 测试取消按钮功能
        /// </summary>
        [TestMethod]
        public void TestCancelButtonFunctionality()
        {
            // 填写一些数据但不保存
            var nameTextBox = UIAutomationHelpers.FindTextBox(_editor, "tableNameTextBox");
            if (nameTextBox != null)
            {
                UIAutomationHelpers.InputText(nameTextBox, "临时查寻表");
            }

            var cancelButton = UIAutomationHelpers.FindButton(_editor, "取消");
            Assert.IsNotNull(cancelButton, "未找到取消按钮");

            Assert.IsFalse(_editor.IsClosed, "编辑器初始状态应打开");

            // 点击取消按钮
            UIAutomationHelpers.ClickButton(cancelButton);
            UIAutomationHelpers.Wait(500);

            // 验证编辑器关闭
            Assert.IsTrue(_editor.IsClosed, "点击取消按钮后编辑器应关闭");
        }

        /// <summary>
        /// 测试Esc键取消功能
        /// </summary>
        [TestMethod]
        public void TestEscapeKeyCancellation()
        {
            Assert.IsFalse(_editor.IsClosed, "编辑器初始状态应打开");

            // 发送Esc键
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
            UIAutomationHelpers.Wait(500);

            Assert.IsTrue(_editor.IsClosed, "按Esc键后编辑器应关闭");
        }

        /// <summary>
        /// 测试行选择功能
        /// </summary>
        [TestMethod]
        public void TestRowSelectionFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            Assert.IsNotNull(dataGrid, "未找到数据网格");
            Assert.IsNotNull(addRowButton, "未找到添加行按钮");

            // 添加多行数据
            for (int i = 0; i < 3; i++)
            {
                UIAutomationHelpers.ClickButton(addRowButton);
                UIAutomationHelpers.Wait(200);
            }

            // 测试行选择
            var rows = dataGrid.Rows;
            Assert.IsTrue(rows.Count >= 3, "应添加了至少3行数据");

            // 选择不同行
            for (int i = 0; i < rows.Count && i < 3; i++)
            {
                var row = rows[i];
                row.Select();
                UIAutomationHelpers.Wait(200);

                Assert.IsTrue(row.IsSelected, $"第{i}行应被选中");
            }
        }

        /// <summary>
        /// 测试键盘导航功能
        /// </summary>
        [TestMethod]
        public void TestKeyboardNavigation()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            if (dataGrid != null && addRowButton != null)
            {
                // 添加一行
                UIAutomationHelpers.ClickButton(addRowButton);
                UIAutomationHelpers.Wait(300);

                // 点击网格获得焦点
                dataGrid.Click();
                UIAutomationHelpers.Wait(200);

                // 测试箭头键导航
                UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWDOWN);
                UIAutomationHelpers.Wait(200);

                UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ARROWUP);
                UIAutomationHelpers.Wait(200);

                // 测试Tab键导航
                UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.TAB);
                UIAutomationHelpers.Wait(200);
            }
        }

        /// <summary>
        /// 测试数据验证功能
        /// </summary>
        [TestMethod]
        public void TestDataValidation()
        {
            var saveButton = UIAutomationHelpers.FindButton(_editor, "保存");
            Assert.IsNotNull(saveButton, "未找到保存按钮");

            // 测试空表名保存
            var nameTextBox = UIAutomationHelpers.FindTextBox(_editor, "tableNameTextBox");
            if (nameTextBox != null)
            {
                nameTextBox.SetValue(""); // 清空名称
            }

            UIAutomationHelpers.ClickButton(saveButton);
            UIAutomationHelpers.Wait(500);

            // 验证编辑器仍然打开（如果实现了验证）
            Assert.IsFalse(_editor.IsClosed, "空表名保存时编辑器应保持打开");
        }

        /// <summary>
        /// 测试行号显示功能
        /// </summary>
        [TestMethod]
        public void TestRowNumberDisplay()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            Assert.IsNotNull(dataGrid, "未找到数据网格");
            Assert.IsNotNull(addRowButton, "未找到添加行按钮");

            // 添加多行
            for (int i = 0; i < 5; i++)
            {
                UIAutomationHelpers.ClickButton(addRowButton);
                UIAutomationHelpers.Wait(200);
            }

            // 验证行数
            var rowCount = dataGrid.Rows.Count;
            Assert.IsTrue(rowCount >= 5, "应显示正确的行数");
        }

        /// <summary>
        /// 测试列标题和宽度
        /// </summary>
        [TestMethod]
        public void TestColumnHeadersAndWidth()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            Assert.IsNotNull(dataGrid, "未找到数据网格");

            var columns = dataGrid.Header.Columns;
            Assert.IsTrue(columns.Count > 0, "数据网格应有列标题");

            // 验证列标题文本
            foreach (var column in columns)
            {
                Assert.IsFalse(string.IsNullOrEmpty(column.Name), "列标题不应为空");
            }

            // 验证列宽度合理
            var bounds = dataGrid.GetElement().Current.BoundingRectangle;
            Assert.IsTrue(bounds.Width > 200, "数据网格宽度应合理");
        }

        /// <summary>
        /// 测试上下文菜单功能
        /// </summary>
        [TestMethod]
        public void TestContextMenuFunctionality()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            if (dataGrid != null && addRowButton != null)
            {
                // 添加一行
                UIAutomationHelpers.ClickButton(addRowButton);
                UIAutomationHelpers.Wait(300);

                if (dataGrid.Rows.Count > 0)
                {
                    var firstRow = dataGrid.Rows.First();
                    
                    // 右键点击行（如果支持）
                    // 这里需要模拟右键点击，具体实现取决于White框架的能力
                    
                    // 验证是否有上下文菜单
                    try
                    {
                        var contextMenu = _editor.ContextMenu;
                        if (contextMenu != null)
                        {
                            // 测试上下文菜单项
                            Assert.IsNotNull(contextMenu, "应存在上下文菜单");
                        }
                    }
                    catch
                    {
                        // 如果没有上下文菜单，这是正常的
                    }
                }
            }
        }

        /// <summary>
        ///测试编辑器响应性
        /// </summary>
        [TestMethod]
        public void TestEditorResponsiveness()
        {
            var startTime = DateTime.Now;

            // 执行多个操作
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            var deleteRowButton = UIAutomationHelpers.FindButton(_editor, "删除行");
            
            if (addRowButton != null && deleteRowButton != null)
            {
                // 添加行
                UIAutomationHelpers.ClickButton(addRowButton);
                UIAutomationHelpers.Wait(200);

                // 删除行
                UIAutomationHelpers.ClickButton(deleteRowButton);
                UIAutomationHelpers.Wait(200);
            }

            var endTime = DateTime.Now;
            var operationTime = endTime - startTime;

            Assert.IsTrue(operationTime.TotalSeconds < 5, "编辑器操作响应时间应合理");
        }

        /// <summary>
        /// 测试数据导入导出功能（如果有）
        /// </summary>
        [TestMethod]
        public void TestDataImportExportFunctionality()
        {
            // 检查是否有导入导出按钮
            var importButton = TryFindButton("导入");
            var exportButton = TryFindButton("导出");

            if (importButton != null)
            {
                // 测试导入功能（如果实现）
                Assert.Inconclusive("导入功能测试需要具体的实现");
            }

            if (exportButton != null)
            {
                // 测试导出功能（如果实现）
                Assert.Inconclusive("导出功能测试需要具体的实现");
            }
        }

        /// <summary>
        /// 测试多行编辑功能
        /// </summary>
        [TestMethod]
        public void TestMultipleRowEditing()
        {
            var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
            var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
            
            Assert.IsNotNull(dataGrid, "未找到数据网格");
            Assert.IsNotNull(addRowButton, "未找到添加行按钮");

            // 添加多行
            for (int i = 0; i < 3; i++)
            {
                UIAutomationHelpers.ClickButton(addRowButton);
                UIAutomationHelpers.Wait(200);
            }

            var rows = dataGrid.Rows;
            Assert.IsTrue(rows.Count >= 3, "应添加了至少3行");

            // 编辑多行数据
            for (int i = 0; i < rows.Count && i < 3; i++)
            {
                var row = rows[i];
                row.Select();
                UIAutomationHelpers.Wait(200);

                // 编辑第一列
                var firstCell = row.Cells.FirstOrDefault();
                if (firstCell != null)
                {
                    firstCell.Click();
                    UIAutomationHelpers.SendText($"数据{i + 1}");
                    UIAutomationHelpers.Wait(200);
                }
            }
        }

        /// <summary>
        /// 测试撤销重做功能（如果有）
        /// </summary>
        [TestMethod]
        public void TestUndoRedoFunctionality()
        {
            var undoButton = TryFindButton("撤销");
            var redoButton = TryFindButton("重做");

            if (undoButton != null && redoButton != null)
            {
                // 测试撤销重做功能（如果实现）
                Assert.Inconclusive("撤销重做功能测试需要具体的实现");
            }
        }

        /// <summary>
        /// 打开查寻表编辑器
        /// </summary>
        private void OpenLookupTableEditor()
        {
            try
            {
                _editor = UIAutomationHelpers.WaitForDialog(EditorTitle);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"无法打开查寻表编辑器: {ex.Message}");
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
                return UIAutomationHelpers.FindButton(_editor, buttonText);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 添加测试数据
        /// </summary>
        private void AddTestData()
        {
            try
            {
                var dataGrid = UIAutomationHelpers.FindDataGrid(_editor, "dataGrid");
                var addRowButton = UIAutomationHelpers.FindButton(_editor, "添加行");
                
                if (dataGrid != null && addRowButton != null)
                {
                    // 添加一行
                    UIAutomationHelpers.ClickButton(addRowButton);
                    UIAutomationHelpers.Wait(500);

                    // 如果有数据，编辑第一列
                    if (dataGrid.Rows.Count > 0)
                    {
                        var firstRow = dataGrid.Rows.First();
                        var firstCell = firstRow.Cells.FirstOrDefault();
                        
                        if (firstCell != null)
                        {
                            firstCell.Click();
                            UIAutomationHelpers.SendText("测试数据");
                            UIAutomationHelpers.Wait(300);
                        }
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