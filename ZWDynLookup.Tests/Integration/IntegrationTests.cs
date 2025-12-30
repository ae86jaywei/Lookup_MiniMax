using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ZWDynLookup.Core;
using ZWDynLookup.Core.Models;
using ZWDynLookup.Core.Services;
using ZWDynLookup.Tests.Integration.Helpers;

namespace ZWDynLookup.Tests.Integration
{
    /// <summary>
    /// 集成测试主入口点
    /// 验证整个动态查找表系统的端到端功能
    /// </summary>
    public class IntegrationTests : IDisposable
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IParameterService _parameterService;
        private readonly IActionService _actionService;
        private readonly IIntegrationTestHelper _testHelper;

        public IntegrationTests()
        {
            _lookupTableService = new LookupTableService();
            _parameterService = new ParameterService();
            _actionService = new ActionService();
            _testHelper = new IntegrationTestHelper();
        }

        [Fact]
        public async Task CompleteLookupTableWorkflow_ShouldSucceed()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var parameterId = await _testHelper.CreateTestParameterAsync(lookupTableId);
            var actionId = await _testHelper.CreateTestActionAsync(parameterId);

            // Act & Assert - 完整工作流程测试
            await _testHelper.ExecuteCompleteWorkflowAsync(lookupTableId, parameterId, actionId);
        }

        [Fact]
        public async Task MultipleLookupTableOperations_ShouldMaintainConsistency()
        {
            // Arrange
            var lookupTables = await _testHelper.CreateMultipleLookupTablesAsync(5);

            // Act
            var operations = new List<Func<Task>>
            {
                () => _lookupTableService.UpdateAsync(lookupTables[0].Id, new LookupTableUpdateDto { Name = "Updated Table 1" }),
                () => _lookupTableService.AddRowAsync(lookupTables[1].Id, new LookupRowDto { Key = "NewKey", Value = "NewValue" }),
                () => _lookupTableService.DeleteRowAsync(lookupTables[2].Id, "ExistingKey"),
                () => _lookupTableService.AddColumnAsync(lookupTables[3].Id, "NewColumn"),
                () => _parameterService.CreateParameterAsync(lookupTables[4].Id, new ParameterDto { Name = "TestParam", Type = ParameterType.Lookup })
            };

            // 并行执行操作
            await Task.WhenAll(operations.Select(op => op()));

            // Assert - 验证所有操作后数据一致性
            foreach (var table in lookupTables)
            {
                var updatedTable = await _lookupTableService.GetByIdAsync(table.Id);
                Assert.NotNull(updatedTable);
            }
        }

        [Fact]
        public async Task ErrorRecoveryScenario_ShouldRestoreData()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var originalData = await _lookupTableService.GetByIdAsync(lookupTableId);
            
            // 模拟数据损坏场景
            await _testHelper.SimulateDataCorruptionAsync(lookupTableId);
            
            // Act - 恢复操作
            await _lookupTableService.RestoreFromBackupAsync(lookupTableId);
            
            // Assert - 验证数据恢复
            var restoredData = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.Equal(originalData.Name, restoredData.Name);
            Assert.Equal(originalData.Rows.Count, restoredData.Rows.Count);
        }

        [Fact]
        public async Task LargeScaleLookupTable_ShouldHandleEfficiently()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateLargeLookupTableAsync(10000);
            
            // Act - 测试大数据量下的性能
            var startTime = DateTime.Now;
            var results = await _lookupTableService.SearchAsync(lookupTableId, "Key_5000");
            var endTime = DateTime.Now;
            
            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.True((endTime - startTime).TotalMilliseconds < 1000, "大数据量查询应该在1秒内完成");
        }

        [Fact]
        public async Task ConcurrentAccess_ShouldNotCorruptData()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // Act - 模拟并发访问
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto 
                        { 
                            Key = $"ConcurrentKey_{i}_{j}", 
                            Value = $"ConcurrentValue_{i}_{j}" 
                        });
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
            
            // Assert - 验证最终数据完整性
            var finalTable = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.NotNull(finalTable);
            Assert.True(finalTable.Rows.Count >= 1000, "应该有足够的行数据");
        }

        [Fact]
        public async Task IntegrationWithExternalSystems_ShouldWork()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var exportData = await _testHelper.ExportToExternalFormatAsync(lookupTableId);
            
            // Act
            var importResult = await _testHelper.ImportFromExternalFormatAsync(exportData);
            
            // Assert
            Assert.True(importResult.Success);
            Assert.NotNull(importResult.LookupTable);
        }

        public void Dispose()
        {
            // 清理测试数据
            _testHelper.CleanupAsync().Wait();
        }
    }
}