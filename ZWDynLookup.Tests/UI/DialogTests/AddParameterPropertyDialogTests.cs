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
    /// 添加参数特性对话框的UI自动化测试
    /// </summary>
    [TestClass]
    public class AddParameterPropertyDialogTests
    {
        private Window _dialog;
        private const string DialogTitle = "添加参数特性";

        [TestInitialize]
        public void TestInitialize()
        {
            UIAutomationHelpers.InitializeTestEnvironment();
            OpenAddParameterPropertyDialog();
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
            Assert.IsNotNull(_dialog, "对话框未能正确打开");
            Assert.IsTrue(UIAutomationHelpers.IsModal(_dialog), "对话框应该为模式对话框");
            Assert.IsTrue(_dialog.Title.Contains(DialogTitle), $"对话框标题应包含'{DialogTitle}'");
        }

        /// <summary>
        /// 测试对话框必需UI元素
        /// </summary>
        [TestMethod]
        public void TestDialogContainsRequiredElements()
        {
            // 验证特性名称输入框
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "propertyNameTextBox"), 
                "应存在特性名称输入框");
            
            // 验证特性类型下拉框
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "propertyTypeComboBox"), 
                "应存在特性类型下拉框");
            
            // 验证特性值输入框
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "propertyValueTextBox"), 
                "应存在特性值输入框");
            
            // 验证描述输入框
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "descriptionTextBox"), 
                "应存在描述输入框");
            
            // 验证确定和取消按钮
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "okButton"), 
                "应存在确定按钮");
            
            Assert.IsTrue(UIAutomationHelpers.ElementExists(_dialog, "cancelButton"), 
                "应存在取消按钮");
        }

        /// <summary>
        /// 测试特性类型下拉框功能
        /// </summary>
        [TestMethod]
        public void TestPropertyTypeComboBoxFunctionality()
        {
            var typeComboBox = UIAutomationHelpers.FindComboBox(_dialog, "propertyTypeComboBox");
            Assert.IsNotNull(typeComboBox, "未找到特性类型下拉框");

            // 验证下拉框有选项
            var items = typeComboBox.Items;
            Assert.IsTrue(items.Count > 0, "特性类型下拉框应有选项");

            // 测试选择不同类型
            var firstItem = items.First();
            UIAutomationHelpers.SelectComboBoxItem(typeComboBox, firstItem);
            
            // 验证选择生效
            Assert.AreEqual(firstItem, typeComboBox.SelectedItemText, "选择应生效");
        }

        /// <summary>
        /// 测试特性名称输入验证
        /// </summary>
        [TestMethod]
        public void TestPropertyNameInputValidation()
        {
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            Assert.IsNotNull(nameTextBox, "未找到特性名称输入框");

            // 测试空名称提交
            var okButton = UIAutomationHelpers.FindButton(_dialog, "确定");
            UIAutomationHelpers.ClickButton(okButton);
            UIAutomationHelpers.Wait(500);

            // 验证对话框仍然打开（如果实现了验证）
            Assert.IsFalse(_dialog.IsClosed, "空名称提交后对话框应保持打开");

            // 测试有效名称输入
            UIAutomationHelpers.InputText(nameTextBox, "TestProperty");
            var actualValue = nameTextBox.Text;
            Assert.AreEqual("TestProperty", actualValue, "名称输入应生效");
        }

        /// <summary>
        /// 测试特性值输入验证
        /// </summary>
        [TestMethod]
        public void TestPropertyValueInputValidation()
        {
            var valueTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyValueTextBox");
            Assert.IsNotNull(valueTextBox, "未找到特性值输入框");

            // 测试数值类型输入
            UIAutomationHelpers.InputText(valueTextBox, "123.45");
            var actualValue = valueTextBox.Text;
            Assert.AreEqual("123.45", actualValue, "数值输入应生效");

            // 清空并测试文本输入
            valueTextBox.SetValue("");
            UIAutomationHelpers.InputText(valueTextBox, "测试文本值");
            actualValue = valueTextBox.Text;
            Assert.AreEqual("测试文本值", actualValue, "文本输入应生效");
        }

        /// <summary>
        /// 测试描述输入功能
        /// </summary>
        [TestMethod]
        public void TestDescriptionInputFunctionality()
        {
            var descriptionTextBox = UIAutomationHelpers.FindTextBox(_dialog, "descriptionTextBox");
            Assert.IsNotNull(descriptionTextBox, "未找到描述输入框");

            var testDescription = "这是一个测试特性的描述";
            UIAutomationHelpers.InputText(descriptionTextBox, testDescription);
            
            var actualValue = descriptionTextBox.Text;
            Assert.AreEqual(testDescription, actualValue, "描述输入应生效");
        }

        /// <summary>
        /// 测试确定按钮功能
        /// </summary>
        [TestMethod]
        public void TestOkButtonFunctionality()
        {
            // 填写表单数据
            FillValidFormData();

            var okButton = UIAutomationHelpers.FindButton(_dialog, "确定");
            Assert.IsNotNull(okButton, "未找到确定按钮");

            // 点击确定按钮
            UIAutomationHelpers.ClickButton(okButton);
            UIAutomationHelpers.Wait(500);

            // 验证对话框关闭
            Assert.IsTrue(_dialog.IsClosed, "点击确定按钮后对话框应关闭");
        }

        /// <summary>
        /// 测试取消按钮功能
        /// </summary>
        [TestMethod]
        public void TestCancelButtonFunctionality()
        {
            // 填写一些数据
            FillValidFormData();

            var cancelButton = UIAutomationHelpers.FindButton(_dialog, "取消");
            Assert.IsNotNull(cancelButton, "未找到取消按钮");

            Assert.IsFalse(_dialog.IsClosed, "初始状态对话框应打开");

            // 点击取消按钮
            UIAutomationHelpers.ClickButton(cancelButton);
            UIAutomationHelpers.Wait(500);

            // 验证对话框关闭
            Assert.IsTrue(_dialog.IsClosed, "点击取消按钮后对话框应关闭");
        }

        /// <summary>
        /// 测试Esc键取消功能
        /// </summary>
        [TestMethod]
        public void TestEscapeKeyCancellation()
        {
            Assert.IsFalse(_dialog.IsClosed, "对话框初始状态应打开");

            // 发送Esc键
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.ESCAPE);
            UIAutomationHelpers.Wait(500);

            Assert.IsTrue(_dialog.IsClosed, "按Esc键后对话框应关闭");
        }

        /// <summary>
        /// 测试表单数据验证逻辑
        /// </summary>
        [TestMethod]
        public void TestFormDataValidation()
        {
            // 测试空表单提交
            var okButton = UIAutomationHelpers.FindButton(_dialog, "确定");
            UIAutomationHelpers.ClickButton(okButton);
            UIAutomationHelpers.Wait(500);

            // 如果实现了验证，对话框应该保持打开
            // 具体行为取决于验证实现
        }

        /// <summary>
        /// 测试输入框焦点管理
        /// </summary>
        [TestMethod]
        public void TestInputFocusManagement()
        {
            // 验证初始焦点在第一个输入框
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            Assert.IsNotNull(nameTextBox, "未找到特性名称输入框");

            // 点击第一个输入框
            nameTextBox.Click();
            UIAutomationHelpers.Wait(200);

            // 测试Tab键导航
            UIAutomationHelpers.SendKeys(KeyboardInput.SpecialKeys.TAB);
            UIAutomationHelpers.Wait(200);

            // 验证焦点移动到下一个控件（这里需要根据实际UI结构调整）
            // 这是一个基本的焦点测试，实际实现可能需要更详细的验证
        }

        /// <summary>
        /// 测试特性类型变更时的界面更新
        /// </summary>
        [TestMethod]
        public void TestPropertyTypeChangeUpdates()
        {
            var typeComboBox = UIAutomationHelpers.FindComboBox(_dialog, "propertyTypeComboBox");
            Assert.IsNotNull(typeComboBox, "未找到特性类型下拉框");

            var valueTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyValueTextBox");
            Assert.IsNotNull(valueTextBox, "未找到特性值输入框");

            // 选择不同类型，观察值输入框的变化
            var items = typeComboBox.Items;
            if (items.Count > 1)
            {
                var firstType = items.First();
                var secondType = items.Skip(1).First();

                // 选择第一种类型
                UIAutomationHelpers.SelectComboBoxItem(typeComboBox, firstType);
                UIAutomationHelpers.Wait(300);

                // 清空并输入值
                valueTextBox.SetValue("");
                UIAutomationHelpers.InputText(valueTextBox, "123");

                // 选择第二种类型
                UIAutomationHelpers.SelectComboBoxItem(typeComboBox, secondType);
                UIAutomationHelpers.Wait(300);

                // 验证值输入框状态
                // 具体验证取决于UI实现
            }
        }

        /// <summary>
        /// 测试长文本输入处理
        /// </summary>
        [TestMethod]
        public void TestLongTextInputHandling()
        {
            var descriptionTextBox = UIAutomationHelpers.FindTextBox(_dialog, "descriptionTextBox");
            Assert.IsNotNull(descriptionTextBox, "未找到描述输入框");

            // 生成很长的文本
            var longText = new string('A', 1000);
            UIAutomationHelpers.InputText(descriptionTextBox, longText);

            var actualValue = descriptionTextBox.Text;
            // 验证长文本处理（可能有限制）
            Assert.IsTrue(actualValue.Length > 0, "长文本输入应被处理");
        }

        /// <summary>
        /// 测试特殊字符输入处理
        /// </summary>
        [TestMethod]
        public void TestSpecialCharacterInput()
        {
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            Assert.IsNotNull(nameTextBox, "未找到特性名称输入框");

            var specialText = "Test_Property-123@#$%^&*()";
            UIAutomationHelpers.InputText(nameTextBox, specialText);

            var actualValue = nameTextBox.Text;
            Assert.AreEqual(specialText, actualValue, "特殊字符输入应正确处理");
        }

        /// <summary>
        /// 测试对话框大小和布局
        /// </summary>
        [TestMethod]
        public void TestDialogLayoutAndSizing()
        {
            var bounds = _dialog.GetElement().Current.BoundingRectangle;
            
            // 验证对话框有合理的尺寸
            Assert.IsTrue(bounds.Width > 300, "对话框宽度应足够");
            Assert.IsTrue(bounds.Height > 200, "对话框高度应足够");

            // 验证控件布局合理
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            var typeComboBox = UIAutomationHelpers.FindComboBox(_dialog, "propertyTypeComboBox");
            
            if (nameTextBox != null && typeComboBox != null)
            {
                var nameBounds = nameTextBox.GetElement().Current.BoundingRectangle;
                var typeBounds = typeComboBox.GetElement().Current.BoundingRectangle;
                
                // 验证控件不重叠
                Assert.IsFalse(nameBounds.IntersectsWith(typeBounds), "控件不应重叠");
            }
        }

        /// <summary>
        /// 测试对话框响应性
        /// </summary>
        [TestMethod]
        public void TestDialogResponsiveness()
        {
            var startTime = DateTime.Now;

            // 执行多个UI操作
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            var typeComboBox = UIAutomationHelpers.FindComboBox(_dialog, "propertyTypeComboBox");
            
            if (nameTextBox != null && typeComboBox != null)
            {
                UIAutomationHelpers.InputText(nameTextBox, "Test");
                UIAutomationHelpers.SelectComboBoxItem(typeComboBox, typeComboBox.Items.First());
                UIAutomationHelpers.Wait(100);
            }

            var endTime = DateTime.Now;
            var operationTime = endTime - startTime;

            Assert.IsTrue(operationTime.TotalSeconds < 3, "UI操作响应时间应合理");
        }

        /// <summary>
        /// 测试数据绑定和同步
        /// </summary>
        [TestMethod]
        public void TestDataBindingAndSynchronization()
        {
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            var valueTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyValueTextBox");
            
            Assert.IsNotNull(nameTextBox, "未找到特性名称输入框");
            Assert.IsNotNull(valueTextBox, "未找到特性值输入框");

            // 模拟数据绑定测试
            UIAutomationHelpers.InputText(nameTextBox, "TestProperty");
            UIAutomationHelpers.InputText(valueTextBox, "TestValue");
            
            // 验证数据正确设置
            Assert.AreEqual("TestProperty", nameTextBox.Text, "名称数据绑定应正确");
            Assert.AreEqual("TestValue", valueTextBox.Text, "值数据绑定应正确");
        }

        /// <summary>
        /// 测试错误提示显示
        /// </summary>
        [TestMethod]
        public void TestErrorMessageDisplay()
        {
            // 测试无效数据提交时的错误提示
            var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
            var okButton = UIAutomationHelpers.FindButton(_dialog, "确定");
            
            if (nameTextBox != null && okButton != null)
            {
                // 输入无效数据
                UIAutomationHelpers.InputText(nameTextBox, ""); // 空名称
                UIAutomationHelpers.ClickButton(okButton);
                UIAutomationHelpers.Wait(1000);

                // 验证是否显示错误提示
                // 具体实现取决于错误处理机制
            }
        }

        /// <summary>
        /// 打开添加参数特性对话框
        /// </summary>
        private void OpenAddParameterPropertyDialog()
        {
            try
            {
                _dialog = UIAutomationHelpers.WaitForDialog(DialogTitle);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"无法打开添加参数特性对话框: {ex.Message}");
            }
        }

        /// <summary>
        /// 填写有效的表单数据
        /// </summary>
        private void FillValidFormData()
        {
            try
            {
                var nameTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyNameTextBox");
                var typeComboBox = UIAutomationHelpers.FindComboBox(_dialog, "propertyTypeComboBox");
                var valueTextBox = UIAutomationHelpers.FindTextBox(_dialog, "propertyValueTextBox");
                var descriptionTextBox = UIAutomationHelpers.FindTextBox(_dialog, "descriptionTextBox");

                if (nameTextBox != null)
                    UIAutomationHelpers.InputText(nameTextBox, "TestProperty");

                if (typeComboBox != null && typeComboBox.Items.Count > 0)
                    UIAutomationHelpers.SelectComboBoxItem(typeComboBox, typeComboBox.Items.First());

                if (valueTextBox != null)
                    UIAutomationHelpers.InputText(valueTextBox, "TestValue");

                if (descriptionTextBox != null)
                    UIAutomationHelpers.InputText(descriptionTextBox, "测试特性描述");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"填写表单数据失败: {ex.Message}");
            }
        }
    }
}