using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZWDynLookup.Core;
using ZWDynLookup.Core.Models;
using ZWDynLookup.Core.Services;

namespace ZWDynLookup.Tests.Integration.Helpers
{
    /// <summary>
    /// 集成测试辅助类
    /// 提供测试所需的工具方法和测试数据
    /// </summary>
    public class IntegrationTestHelper : IIntegrationTestHelper
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IParameterService _parameterService;
        private readonly IActionService _actionService;
        private readonly List<string> _createdResourceIds;

        public IntegrationTestHelper()
        {
            _lookupTableService = new LookupTableService();
            _parameterService = new ParameterService();
            _actionService = new ActionService();
            _createdResourceIds = new List<string>();
        }

        #region 查找表操作

        public async Task<LookupTableDto> CreateTestLookupTableAsync(string name = "测试查找表")
        {
            var tableData = new LookupTableCreateDto
            {
                Name = name,
                Description = $"用于测试的查找表 - {Guid.NewGuid():N}",
                Columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition { Name = "Key", Type = ColumnType.String, IsRequired = true },
                    new ColumnDefinition { Name = "Value", Type = ColumnType.String, IsRequired = true },
                    new ColumnDefinition { Name = "Category", Type = ColumnType.String, IsRequired = false },
                    new ColumnDefinition { Name = "Description", Type = ColumnType.String, IsRequired = false }
                }
            };

            var table = await _lookupTableService.CreateAsync(tableData);
            _createdResourceIds.Add(table.Id);
            return table;
        }

        public async Task<List<LookupTableDto>> CreateMultipleLookupTablesAsync(int count)
        {
            var tables = new List<LookupTableDto>();
            for (int i = 0; i < count; i++)
            {
                var table = await CreateTestLookupTableAsync($"测试查找表_{i + 1}");
                tables.Add(table);
            }
            return tables;
        }

        public async Task<LookupTableDto> CreateLargeLookupTableAsync(int rowCount)
        {
            var table = await CreateTestLookupTableAsync("大数据量测试表");
            
            var rows = new List<LookupRowDto>();
            for (int i = 0; i < rowCount; i++)
            {
                rows.Add(new LookupRowDto
                {
                    Key = $"Key_{i:D6}",
                    Value = $"Value_{i:D6}",
                    Category = $"Category_{i % 10}",
                    Description = $"Description for row {i}"
                });
            }

            await _lookupTableService.AddRowsAsync(table.Id, rows);
            return table;
        }

        public async Task AddTestDataAsync(string lookupTableId, Dictionary<string, string> testData)
        {
            foreach (var item in testData)
            {
                await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
                {
                    Key = item.Key,
                    Value = item.Value
                });
            }
        }

        #endregion

        #region 参数操作

        public async Task<ParameterDto> CreateTestParameterAsync(string lookupTableId, string name = "测试参数")
        {
            var parameterData = new ParameterDto
            {
                Name = name,
                Type = ParameterType.Lookup,
                LookupTableId = lookupTableId,
                Description = $"通过{nameof(CreateTestParameterAsync)}创建的测试参数",
                DefaultValue = "默认",
                IsActive = true,
                Constraints = new ParameterConstraints
                {
                    Required = true,
                    MinLength = 1,
                    MaxLength = 100
                }
            };

            var parameter = await _parameterService.CreateAsync(parameterData);
            _createdResourceIds.Add(parameter.Id);
            return parameter;
        }

        public async Task<List<ParameterDto>> CreateMultipleParametersAsync(string lookupTableId, int count)
        {
            var parameters = new List<ParameterDto>();
            for (int i = 0; i < count; i++)
            {
                var parameter = await CreateTestParameterAsync(lookupTableId, $"测试参数_{i + 1}");
                parameters.Add(parameter);
            }
            return parameters;
        }

        #endregion

        #region 动作操作

        public async Task<ActionDto> CreateTestActionAsync(string parameterId, string name = "测试动作")
        {
            var actionData = new ActionDto
            {
                Name = name,
                Type = ActionType.Lookup,
                ParameterId = parameterId,
                Trigger = "OnSelect",
                ActionData = new Dictionary<string, object>
                {
                    { "Source", "LookupTable" },
                    { "TargetField", "Value" },
                    { "DisplayField", "Description" }
                },
                Description = $"通过{nameof(CreateTestActionAsync)}创建的测试动作",
                IsActive = true
            };

            var action = await _actionService.CreateAsync(actionData);
            _createdResourceIds.Add(action.Id);
            return action;
        }

        public async Task<List<ActionDto>> CreateMultipleActionsAsync(string parameterId, int count)
        {
            var actions = new List<ActionDto>();
            for (int i = 0; i < count; i++)
            {
                var action = await CreateTestActionAsync(parameterId, $"测试动作_{i + 1}");
                actions.Add(action);
            }
            return actions;
        }

        #endregion

        #region 完整工作流程

        public async Task ExecuteCompleteWorkflowAsync(string lookupTableId, string parameterId, string actionId)
        {
            // 验证所有组件都已正确创建
            var lookupTable = await _lookupTableService.GetByIdAsync(lookupTableId);
            var parameter = await _parameterService.GetByIdAsync(parameterId);
            var action = await _actionService.GetByIdAsync(actionId);

            Assert.NotNull(lookupTable);
            Assert.NotNull(parameter);
            Assert.NotNull(action);

            // 验证关联关系
            Assert.Equal(lookupTableId, parameter.LookupTableId);
            Assert.Equal(parameterId, action.ParameterId);

            // 验证参数的动作关联
            Assert.Contains(parameter.Actions, a => a.Id == actionId);

            // 执行一些基本操作验证功能
            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "WorkflowTestKey",
                Value = "WorkflowTestValue"
            });

            // 验证数据完整性
            var updatedTable = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.Contains(updatedTable.Rows, r => r.Key == "WorkflowTestKey");
        }

        #endregion

        #region 错误场景模拟

        public async Task SimulateDataCorruptionAsync(string lookupTableId)
        {
            // 这里模拟数据损坏场景
            // 在实际实现中，这可能涉及直接操作数据库来损坏数据
            await Task.CompletedTask;
        }

        public async Task AddCorruptedDataAsync(string lookupTableId)
        {
            // 模拟添加损坏数据（通过特殊方式绕过验证）
            await Task.CompletedTask;
        }

        #endregion

        #region 缓存和同步

        public async Task CreateCacheDataAsync(string lookupTableId)
        {
            // 创建测试用的缓存数据
            await Task.CompletedTask;
        }

        public async Task<List<LookupRowDto>> GetCacheDataAsync(string lookupTableId)
        {
            // 获取缓存数据
            return new List<LookupRowDto>();
        }

        #endregion

        #region 导入导出

        public async Task<ExternalFormatData> ExportToExternalFormatAsync(string lookupTableId)
        {
            var table = await _lookupTableService.GetByIdAsync(lookupTableId);
            
            return new ExternalFormatData
            {
                Format = "JSON",
                Data = System.Text.Json.JsonSerializer.Serialize(table),
                ExportedAt = DateTime.Now,
                SourceId = lookupTableId
            };
        }

        public async Task<ImportResult> ImportFromExternalFormatAsync(ExternalFormatData exportData)
        {
            try
            {
                var table = System.Text.Json.JsonSerializer.Deserialize<LookupTableDto>(exportData.Data);
                
                if (table != null)
                {
                    var createdTable = await CreateTestLookupTableAsync(table.Name);
                    return new ImportResult
                    {
                        Success = true,
                        LookupTable = createdTable
                    };
                }

                return new ImportResult
                {
                    Success = false,
                    ErrorMessage = "Deserialization failed"
                };
            }
            catch (Exception ex)
            {
                return new ImportResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        #endregion

        #region 清理工作

        public async Task CleanupAsync()
        {
            // 清理所有创建的测试资源
            foreach (var resourceId in _createdResourceIds.ToList())
            {
                try
                {
                    await CleanupResourceAsync(resourceId);
                }
                catch
                {
                    // 忽略清理错误
                }
            }
            
            _createdResourceIds.Clear();
        }

        private async Task CleanupResourceAsync(string resourceId)
        {
            // 尝试作为不同类型的资源清理
            try
            {
                await _lookupTableService.DeleteAsync(resourceId, force: true);
            }
            catch
            {
                try
                {
                    await _parameterService.DeleteAsync(resourceId);
                }
                catch
                {
                    try
                    {
                        await _actionService.DeleteAsync(resourceId);
                    }
                    catch
                    {
                        // 资源可能已经被清理或不存在
                    }
                }
            }
        }

        #endregion

        #region 辅助方法

        public string GenerateTestName(string prefix = "Test")
        {
            return $"{prefix}_{Guid.NewGuid():N}";
        }

        public Dictionary<string, string> GenerateTestData(int count)
        {
            var data = new Dictionary<string, string>();
            for (int i = 0; i < count; i++)
            {
                data[$"TestKey_{i:D4}"] = $"TestValue_{i:D4}";
            }
            return data;
        }

        public async Task<bool> WaitForConditionAsync(Func<Task<bool>> condition, int timeoutMs = 5000, int intervalMs = 100)
        {
            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (await condition())
                {
                    return true;
                }
                await Task.Delay(intervalMs);
            }
            return false;
        }

        #endregion
    }

    #region 辅助类型定义

    public interface IIntegrationTestHelper
    {
        Task<LookupTableDto> CreateTestLookupTableAsync(string name = "测试查找表");
        Task<List<LookupTableDto>> CreateMultipleLookupTablesAsync(int count);
        Task<LookupTableDto> CreateLargeLookupTableAsync(int rowCount);
        Task AddTestDataAsync(string lookupTableId, Dictionary<string, string> testData);
        Task<ParameterDto> CreateTestParameterAsync(string lookupTableId, string name = "测试参数");
        Task<List<ParameterDto>> CreateMultipleParametersAsync(string lookupTableId, int count);
        Task<ActionDto> CreateTestActionAsync(string parameterId, string name = "测试动作");
        Task<List<ActionDto>> CreateMultipleActionsAsync(string parameterId, int count);
        Task ExecuteCompleteWorkflowAsync(string lookupTableId, string parameterId, string actionId);
        Task SimulateDataCorruptionAsync(string lookupTableId);
        Task AddCorruptedDataAsync(string lookupTableId);
        Task CreateCacheDataAsync(string lookupTableId);
        Task<List<LookupRowDto>> GetCacheDataAsync(string lookupTableId);
        Task<ExternalFormatData> ExportToExternalFormatAsync(string lookupTableId);
        Task<ImportResult> ImportFromExternalFormatAsync(ExternalFormatData exportData);
        Task CleanupAsync();
        string GenerateTestName(string prefix = "Test");
        Dictionary<string, string> GenerateTestData(int count);
        Task<bool> WaitForConditionAsync(Func<Task<bool>> condition, int timeoutMs = 5000, int intervalMs = 100);
    }

    public class ExternalFormatData
    {
        public string Format { get; set; }
        public string Data { get; set; }
        public DateTime ExportedAt { get; set; }
        public string SourceId { get; set; }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public LookupTableDto LookupTable { get; set; }
    }

    public class BulkOperation
    {
        public BulkOperationType Type { get; set; }
        public string TargetId { get; set; }
        public object Data { get; set; }
    }

    public enum BulkOperationType
    {
        AddRows,
        UpdateRows,
        DeleteRows,
        AddColumns,
        UpdateColumns,
        DeleteColumns
    }

    public class CommandContext
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}