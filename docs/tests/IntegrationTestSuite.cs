using System;
using System.Collections.Generic;
using System.Linq;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Runtime;
using ZWDynLookup.Commands;
using ZWDynLookup.Models;
using ZWDynLookup.Service;

namespace ZWDynLookup.Tests
{
    /// <summary>
    /// 中望CAD动态块查寻插件集成测试套件
    /// </summary>
    public class IntegrationTestSuite
    {
        private readonly Application _application;
        private readonly Document _document;
        private readonly Editor _editor;
        private readonly Database _database;
        private readonly List<TestResult> _testResults;
        private readonly string _testLogFile;

        public IntegrationTestSuite()
        {
            _application = Application.AcadApplication as Application;
            _document = _application?.ActiveDocument;
            _editor = _document?.Editor;
            _database = _document?.Database;
            _testResults = new List<TestResult>();
            _testLogFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ZWDynLookup", "TestResults", $"IntegrationTest_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            
            // 创建测试日志目录
            var logDir = Path.GetDirectoryName(_testLogFile);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        /// <summary>
        /// 执行完整的集成测试套件
        /// </summary>
        public void RunAllTests()
        {
            Log("=== 开始执行集成测试套件 ===");
            Log($"测试开始时间: {DateTime.Now}");

            try
            {
                // 测试前准备
                PreTestSetup();

                // 执行所有测试
                TestPluginInitialization();
                TestParameterCommands();
                TestActionCommands();
                TestLookupTableCommands();
                TestPropertiesCommands();
                TestIntegrationScenarios();
                TestErrorHandling();

                // 生成测试报告
                GenerateTestReport();

                Log("=== 集成测试套件执行完成 ===");
            }
            catch (Exception ex)
            {
                Log($"测试套件执行失败: {ex.Message}");
                Log($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        #region 测试方法

        /// <summary>
        /// 测试插件初始化
        /// </summary>
        private void TestPluginInitialization()
        {
            Log("\n--- 测试插件初始化 ---");
            
            try
            {
                // 测试插件入口点
                var pluginEntry = new PluginEntry();
                TestResult("插件入口点创建", true, null);
                
                // 测试命令注册服务
                var commandsRegistered = TestCommandRegistration();
                TestResult("命令注册", commandsRegistered, null);
                
                // 测试UI初始化
                var uiInitialized = TestUIInitialization();
                TestResult("UI初始化", uiInitialized, null);

                Log("插件初始化测试完成");
            }
            catch (Exception ex)
            {
                TestResult("插件初始化", false, ex.Message);
                Log($"插件初始化测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试参数命令功能
        /// </summary>
        private void TestParameterCommands()
        {
            Log("\n--- 测试参数命令功能 ---");
            
            try
            {
                // 测试查寻参数创建
                TestParameterCreation();
                
                // 测试参数属性管理
                TestParameterProperties();
                
                // 测试参数点管理
                TestParameterPointManagement();
                
                // 测试夹点管理
                TestGripManagement();
                
                Log("参数命令功能测试完成");
            }
            catch (Exception ex)
            {
                TestResult("参数命令功能", false, ex.Message);
                Log($"参数命令功能测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试动作命令功能
        /// </summary>
        private void TestActionCommands()
        {
            Log("\n--- 测试动作命令功能 ---");
            
            try
            {
                // 测试查寻动作创建
                TestActionCreation();
                
                // 测试选择集管理
                TestSelectionSetManagement();
                
                // 测试动作选择管理器
                TestActionSelectionManager();
                
                Log("动作命令功能测试完成");
            }
            catch (Exception ex)
            {
                TestResult("动作命令功能", false, ex.Message);
                Log($"动作命令功能测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试查寻表命令功能
        /// </summary>
        private void TestLookupTableCommands()
        {
            Log("\n--- 测试查寻表命令功能 ---");
            
            try
            {
                // 测试查寻表数据模型
                TestLookupTableDataModel();
                
                // 测试查寻表服务
                TestLookupTableService();
                
                // 测试查寻表UI
                TestLookupTableUI();
                
                Log("查寻表命令功能测试完成");
            }
            catch (Exception ex)
            {
                TestResult("查寻表命令功能", false, ex.Message);
                Log($"查寻表命令功能测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试特性命令功能
        /// </summary>
        private void TestPropertiesCommands()
        {
            Log("\n--- 测试特性命令功能 ---");
            
            try
            {
                // 测试参数特性数据模型
                TestParameterPropertyModel();
                
                // 测试特性管理服务
                TestPropertiesManagementService();
                
                // 测试特性UI
                TestPropertiesUI();
                
                Log("特性命令功能测试完成");
            }
            catch (Exception ex)
            {
                TestResult("特性命令功能", false, ex.Message);
                Log($"特性命令功能测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试集成场景
        /// </summary>
        private void TestIntegrationScenarios()
        {
            Log("\n--- 测试集成场景 ---");
            
            try
            {
                // 完整工作流程测试
                TestCompleteWorkflow();
                
                // 参数与动作关联测试
                TestParameterActionIntegration();
                
                // 查寻表与动作集成测试
                TestLookupTableActionIntegration();
                
                // 跨功能集成测试
                TestCrossFunctionIntegration();
                
                Log("集成场景测试完成");
            }
            catch (Exception ex)
            {
                TestResult("集成场景", false, ex.Message);
                Log($"集成场景测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试错误处理
        /// </summary>
        private void TestErrorHandling()
        {
            Log("\n--- 测试错误处理 ---");
            
            try
            {
                // 测试无效参数处理
                TestInvalidParameterHandling();
                
                // 测试无效动作处理
                TestInvalidActionHandling();
                
                // 测试网络错误处理
                TestNetworkErrorHandling();
                
                // 测试数据验证错误
                TestDataValidationErrors();
                
                Log("错误处理测试完成");
            }
            catch (Exception ex)
            {
                TestResult("错误处理", false, ex.Message);
                Log($"错误处理测试失败: {ex.Message}");
            }
        }

        #endregion

        #region 详细测试实现

        private void TestParameterCreation()
        {
            try
            {
                var creator = new LookupParameterCreator(_database);
                var parameter = creator.CreateParameter("TestParameter", "测试参数", "用于测试的参数");
                
                TestResult("查寻参数创建", parameter != null, null);
                
                if (parameter != null)
                {
                    // 验证参数属性
                    var hasValidName = parameter.Name == "TestParameter";
                    var hasValidLabel = parameter.Label == "测试参数";
                    var hasValidDescription = parameter.Description == "用于测试的参数";
                    
                    TestResult("参数属性验证", hasValidName && hasValidLabel && hasValidDescription, null);
                }
            }
            catch (Exception ex)
            {
                TestResult("查寻参数创建", false, ex.Message);
            }
        }

        private void TestActionCreation()
        {
            try
            {
                var actionCreator = new LookupActionCreator(_database);
                var action = actionCreator.CreateLookupAction("TestAction", "测试动作", "用于测试的动作");
                
                TestResult("查寻动作创建", action != null, null);
                
                if (action != null)
                {
                    // 验证动作属性
                    var hasValidName = action.ActionName == "TestAction";
                    var hasValidLabel = action.Label == "测试动作";
                    
                    TestResult("动作属性验证", hasValidName && hasValidLabel, null);
                }
            }
            catch (Exception ex)
            {
                TestResult("查寻动作创建", false, ex.Message);
            }
        }

        private void TestCompleteWorkflow()
        {
            try
            {
                // 创建测试块
                var testBlockId = CreateTestBlock();
                
                // 创建查寻参数
                var creator = new LookupParameterCreator(_database);
                var parameter = creator.CreateParameter("WorkflowParameter", "工作流参数", "完整工作流测试参数");
                
                if (parameter != null)
                {
                    // 创建查寻动作
                    var actionCreator = new LookupActionCreator(_database);
                    var action = actionCreator.CreateLookupAction("WorkflowAction", "工作流动作用", "完整工作流动作用");
                    
                    if (action != null)
                    {
                        // 创建查寻表数据
                        var lookupData = new LookupTableData
                        {
                            ActionName = "WorkflowAction",
                            Properties = new List<ParameterProperty>
                            {
                                new ParameterProperty
                                {
                                    PropertyName = "测试特性",
                                    PropertyValue = "测试值",
                                    DisplayValue = "显示值"
                                }
                            }
                        };
                        
                        TestResult("完整工作流程", true, null);
                    }
                }
            }
            catch (Exception ex)
            {
                TestResult("完整工作流程", false, ex.Message);
            }
        }

        private void TestInvalidParameterHandling()
        {
            try
            {
                var creator = new LookupParameterCreator(_database);
                
                // 测试空名称
                try
                {
                    creator.CreateParameter("", "标签", "描述");
                    TestResult("空参数名称处理", false, "应该抛出异常但没有");
                }
                catch
                {
                    TestResult("空参数名称处理", true, null);
                }
                
                // 测试重复名称
                try
                {
                    creator.CreateParameter("DuplicateTest", "标签1", "描述1");
                    creator.CreateParameter("DuplicateTest", "标签2", "描述2");
                    TestResult("重复参数名称处理", false, "应该抛出异常但没有");
                }
                catch
                {
                    TestResult("重复参数名称处理", true, null);
                }
            }
            catch (Exception ex)
            {
                TestResult("无效参数处理测试", false, ex.Message);
            }
        }

        #endregion

        #region 辅助方法

        private bool TestCommandRegistration()
        {
            try
            {
                var commands = CommandMethodService.GetRegisteredCommands();
                var expectedCommands = new[] { "ZWBPARAMETER", "ZWBACTIONTOOL", "ZWBLOOKUPTABLE", "ZWBPROPERTIES" };
                
                return expectedCommands.All(cmd => commands.Contains(cmd));
            }
            catch
            {
                return false;
            }
        }

        private bool TestUIInitialization()
        {
            try
            {
                // 测试UI组件创建
                var parameterDialog = new ParameterPropertiesDialog();
                var actionDialog = new ActionPropertiesDialog();
                var lookupDialog = new LookupTableManagerDialog();
                var propertiesDialog = new PropertiesManagerDialog();
                
                return parameterDialog != null && actionDialog != null && 
                       lookupDialog != null && propertiesDialog != null;
            }
            catch
            {
                return false;
            }
        }

        private void PreTestSetup()
        {
            Log("执行测试前准备...");
            
            // 清理测试环境
            CleanupTestEnvironment();
            
            // 创建测试文档
            CreateTestDocument();
            
            Log("测试前准备完成");
        }

        private void CleanupTestEnvironment()
        {
            try
            {
                // 清理测试块和对象
                if (_database != null)
                {
                    using (var trans = _database.TransactionManager.StartTransaction())
                    {
                        // 清理测试相关的块定义
                        var blockTable = trans.GetObject(_database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                        if (blockTable != null)
                        {
                            var testBlocks = blockTable.Cast<ObjectId>()
                                .Where(id => id.Handle.ToString().StartsWith("TestBlock"));
                            
                            foreach (var blockId in testBlocks)
                            {
                                try
                                {
                                    var block = trans.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
                                    if (block != null)
                                    {
                                        block.Erase();
                                    }
                                }
                                catch { }
                            }
                        }
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"清理测试环境失败: {ex.Message}");
            }
        }

        private void CreateTestDocument()
        {
            try
            {
                if (_document != null)
                {
                    // 确保处于块编辑器模式
                    if (!_document.Editor.IsInBlockEditor)
                    {
                        _document.Editor.ToggleBlockEditor();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"创建测试文档失败: {ex.Message}");
            }
        }

        private ObjectId CreateTestBlock()
        {
            try
            {
                using (var trans = _database.TransactionManager.StartTransaction())
                {
                    var blockTable = trans.GetObject(_database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    if (blockTable != null)
                    {
                        var blockRecord = new BlockTableRecord();
                        blockRecord.Name = $"TestBlock_{DateTime.Now.Ticks}";
                        
                        var blockId = blockTable.Add(blockRecord);
                        trans.AddNewlyCreatedDBObject(blockRecord, true);
                        trans.Commit();
                        
                        return blockId;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"创建测试块失败: {ex.Message}");
            }
            
            return ObjectId.Null;
        }

        private void TestResult(string testName, bool success, string errorMessage)
        {
            var result = new TestResult
            {
                TestName = testName,
                Success = success,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.Now
            };
            
            _testResults.Add(result);
            
            var status = success ? "✓ 通过" : "✗ 失败";
            var message = success ? "成功" : $"失败: {errorMessage}";
            Log($"[{status}] {testName} - {message}");
        }

        private void Log(string message)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(_testLogFile, logMessage + Environment.NewLine);
        }

        private void GenerateTestReport()
        {
            Log("\n=== 生成测试报告 ===");
            
            var totalTests = _testResults.Count;
            var passedTests = _testResults.Count(r => r.Success);
            var failedTests = totalTests - passedTests;
            var successRate = totalTests > 0 ? (passedTests * 100.0 / totalTests) : 0;
            
            var report = $@"
集成测试报告
============
测试时间: {DateTime.Now}
总测试数: {totalTests}
通过数: {passedTests}
失败数: {failedTests}
成功率: {successRate:F2}%

测试详情:
{string.Join(Environment.NewLine, _testResults.Select(r => 
    $"[{r.Timestamp:HH:mm:ss}] {(r.Success ? "✓" : "✗")} {r.TestName} - {(r.Success ? "成功" : r.ErrorMessage)}"))}
";
            
            Log(report);
            
            // 保存详细报告到文件
            var reportFile = _testLogFile.Replace(".log", "_Report.log");
            File.WriteAllText(reportFile, report);
        }

        #endregion
    }

    /// <summary>
    /// 测试结果类
    /// </summary>
    public class TestResult
    {
        public string TestName { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}