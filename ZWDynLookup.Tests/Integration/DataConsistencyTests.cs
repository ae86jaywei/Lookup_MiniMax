using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZWDynLookup.Core;
using ZWDynLookup.Core.Models;
using ZWDynLookup.Core.Services;
using ZWDynLookup.Tests.Integration.Helpers;

namespace ZWDynLookup.Tests.Integration
{
    /// <summary>
    /// 数据一致性测试
    /// 验证系统在各种场景下的数据一致性
    /// </summary>
    public class DataConsistencyTests : IDisposable
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IParameterService _parameterService;
        private readonly IActionService _actionService;
        private readonly IDataConsistencyService _consistencyService;
        private readonly IIntegrationTestHelper _testHelper;

        public DataConsistencyTests()
        {
            _lookupTableService = new LookupTableService();
            _parameterService = new ParameterService();
            _actionService = new ActionService();
            _consistencyService = new DataConsistencyService();
            _testHelper = new IntegrationTestHelper();
        }

        [Fact]
        public async Task CrossReferenceConsistency_ShouldBeMaintained()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var parameterId = await _testHelper.CreateTestParameterAsync(lookupTableId);
            var actionId = await _testHelper.CreateTestActionAsync(parameterId);

            // Act - 验证交叉引用一致性
            var consistencyReport = await _consistencyService.ValidateCrossReferencesAsync();

            // Assert
            Assert.True(consistencyReport.IsConsistent);
            Assert.Empty(consistencyReport.Inconsistencies);
        }

        [Fact]
        public async Task OrphanedData_ShouldBeDetected()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var parameterId = await _testHelper.CreateTestParameterAsync(lookupTableId);

            // 模拟孤立数据 - 删除查找表但不删除相关参数
            await _lookupTableService.DeleteAsync(lookupTableId);

            // Act - 检测孤立数据
            var orphanedDataReport = await _consistencyService.DetectOrphanedDataAsync();

            // Assert
            Assert.NotNull(orphanedDataReport);
            Assert.Contains(orphanedDataReport.OrphanedParameters, p => p.Id == parameterId);
        }

        [Fact]
        public async Task DataIntegrityConstraints_ShouldBeEnforced()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // Act & Assert - 验证各种完整性约束
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _lookupTableService.DeleteAsync(lookupTableId, force: false));

            // 添加一些依赖数据
            var parameterId = await _testHelper.CreateTestParameterAsync(lookupTableId);
            
            // 现在应该仍然无法删除（因为有依赖）
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _lookupTableService.DeleteAsync(lookupTableId, force: false));

            // 强制删除应该成功
            var result = await _lookupTableService.DeleteAsync(lookupTableId, force: true);
            Assert.True(result);
        }

        [Fact]
        public async Task ConcurrentModifications_ShouldNotCorruptData()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var initialData = await _lookupTableService.GetByIdAsync(lookupTableId);
            var modificationCount = 100;
            var semaphore = new SemaphoreSlim(10, 10); // 限制并发数

            // Act - 并发修改数据
            var tasks = new List<Task>();
            for (int i = 0; i < modificationCount; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var row = await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
                        {
                            Key = $"ConcurrentKey_{index}",
                            Value = $"ConcurrentValue_{index}"
                        });
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert - 验证数据完整性
            var finalData = await _lookupTableService.GetByIdAsync(lookupTableId);
            
            // 验证基本完整性
            Assert.NotNull(finalData);
            Assert.Equal(initialData.Name, finalData.Name);
            Assert.True(finalData.Rows.Count >= modificationCount);
            
            // 验证数据无重复
            var keys = finalData.Rows.Select(r => r.Key).ToList();
            Assert.Equal(keys.Count, keys.Distinct().Count());
        }

        [Fact]
        public async Task TransactionalConsistency_ShouldBeGuaranteed()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();

            // Act - 使用事务进行复杂操作
            var transaction = await _consistencyService.BeginTransactionAsync();
            
            try
            {
                // 在事务中添加多个相关数据
                await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
                {
                    Key = "TransactionKey1",
                    Value = "TransactionValue1"
                }, transaction);

                await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
                {
                    Key = "TransactionKey2",
                    Value = "TransactionValue2"
                }, transaction);

                // 模拟错误
                throw new Exception("Simulated error");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
            }

            // Assert - 验证事务回滚
            var finalData = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.DoesNotContain(finalData.Rows, r => r.Key == "TransactionKey1");
            Assert.DoesNotContain(finalData.Rows, r => r.Key == "TransactionKey2");
        }

        [Fact]
        public async Task DataVersioning_ShouldTrackChanges()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();

            // Act - 进行多次修改并记录版本
            var versions = new List<DataVersion>();
            
            // 第一次修改
            var version1 = await _consistencyService.CreateVersionAsync(lookupTableId, "Initial version");
            versions.Add(version1);
            
            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "VersionKey1",
                Value = "VersionValue1"
            });

            // 第二次修改
            var version2 = await _consistencyService.CreateVersionAsync(lookupTableId, "After first modification");
            versions.Add(version2);

            // 验证版本历史
            var versionHistory = await _consistencyService.GetVersionHistoryAsync(lookupTableId);

            // Assert
            Assert.Equal(2, versionHistory.Count);
            Assert.True(versionHistory[0].CreatedAt < versionHistory[1].CreatedAt);
        }

        [Fact]
        public async Task DataRecovery_ShouldRestoreCorrectState()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // 添加初始数据
            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "RecoveryKey1",
                Value = "RecoveryValue1"
            });

            var initialVersion = await _consistencyService.CreateVersionAsync(lookupTableId, "Initial state");

            // 添加更多数据
            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "RecoveryKey2",
                Value = "RecoveryValue2"
            });

            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "RecoveryKey3",
                Value = "RecoveryValue3"
            });

            // Act - 恢复到初始版本
            await _consistencyService.RestoreToVersionAsync(lookupTableId, initialVersion.Id);

            // Assert
            var restoredData = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.Single(restoredData.Rows);
            Assert.Contains(restoredData.Rows, r => r.Key == "RecoveryKey1");
            Assert.DoesNotContain(restoredData.Rows, r => r.Key == "RecoveryKey2");
            Assert.DoesNotContain(restoredData.Rows, r => r.Key == "RecoveryKey3");
        }

        [Fact]
        public async Task ReferentialIntegrity_ShouldBeMaintained()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var parameterId = await _testHelper.CreateTestParameterAsync(lookupTableId);
            var actionId = await _testHelper.CreateTestActionAsync(parameterId);

            // Act - 验证引用完整性
            var integrityCheck = await _consistencyService.CheckReferentialIntegrityAsync();

            // Assert
            Assert.True(integrityCheck.IsValid);
            
            // 验证所有引用都有效
            var parameter = await _parameterService.GetByIdAsync(parameterId);
            Assert.Equal(lookupTableId, parameter.LookupTableId);

            var action = await _actionService.GetByIdAsync(actionId);
            Assert.Equal(parameterId, action.ParameterId);
        }

        [Fact]
        public async Task DataValidationRules_ShouldBeConsistent()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();

            // Act - 测试各种数据验证规则
            var validationResults = new List<ValidationResult>();

            // 测试有效数据
            validationResults.Add(await _consistencyService.ValidateDataAsync(lookupTableId, new LookupRowDto
            {
                Key = "ValidKey",
                Value = "ValidValue"
            }));

            // 测试无效数据
            validationResults.Add(await _consistencyService.ValidateDataAsync(lookupTableId, new LookupRowDto
            {
                Key = "", // 空键
                Value = "ValidValue"
            }));

            validationResults.Add(await _consistencyService.ValidateDataAsync(lookupTableId, new LookupRowDto
            {
                Key = "ValidKey",
                Value = null // 空值
            }));

            // Assert
            Assert.True(validationResults[0].IsValid);
            Assert.False(validationResults[1].IsValid);
            Assert.False(validationResults[2].IsValid);
        }

        [Fact]
        public async Task DataCleanup_ShouldRemoveInvalidEntries()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // 添加一些有效数据
            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "ValidKey1",
                Value = "ValidValue1"
            });

            await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto
            {
                Key = "ValidKey2",
                Value = "ValidValue2"
            });

            // 模拟添加无效数据（通过直接数据库操作）
            await _testHelper.AddCorruptedDataAsync(lookupTableId);

            // Act - 清理无效数据
            var cleanupResult = await _consistencyService.CleanupInvalidDataAsync(lookupTableId);

            // Assert
            Assert.True(cleanupResult.Success);
            Assert.True(cleanupResult.RemovedCount > 0);

            var cleanedData = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.All(cleanedData.Rows, row => 
            {
                Assert.NotNull(row.Key);
                Assert.NotEmpty(row.Key);
                Assert.NotNull(row.Value);
            });
        }

        [Fact]
        public async Task DataSynchronization_ShouldMaintainConsistency()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // 创建缓存数据
            await _testHelper.CreateCacheDataAsync(lookupTableId);

            // Act - 同步数据
            var syncResult = await _consistencyService.SynchronizeDataAsync(lookupTableId);

            // Assert
            Assert.True(syncResult.Success);
            Assert.True(syncResult.SynchronizedItems > 0);

            // 验证缓存和数据库一致性
            var dbData = await _lookupTableService.GetByIdAsync(lookupTableId);
            var cacheData = await _testHelper.GetCacheDataAsync(lookupTableId);
            
            Assert.Equal(dbData.Rows.Count, cacheData.Count);
        }

        [Fact]
        public async Task BulkOperations_ShouldMaintainConsistency()
        {
            // Arrange
            var lookupTableIds = await _testHelper.CreateMultipleLookupTablesAsync(5);
            var bulkOperations = new List<BulkOperation>();

            // 创建批量操作
            for (int i = 0; i < lookupTableIds.Count; i++)
            {
                bulkOperations.Add(new BulkOperation
                {
                    Type = BulkOperationType.AddRows,
                    TargetId = lookupTableIds[i].Id,
                    Data = new List<LookupRowDto>
                    {
                        new LookupRowDto { Key = $"BulkKey_{i}_1", Value = $"BulkValue_{i}_1" },
                        new LookupRowDto { Key = $"BulkKey_{i}_2", Value = $"BulkValue_{i}_2" }
                    }
                });
            }

            // Act - 执行批量操作
            var bulkResult = await _consistencyService.ExecuteBulkOperationsAsync(bulkOperations);

            // Assert
            Assert.True(bulkResult.Success);
            Assert.Equal(bulkOperations.Count, bulkResult.ProcessedOperations);

            // 验证所有操作都成功
            foreach (var tableId in lookupTableIds.Select(t => t.Id))
            {
                var table = await _lookupTableService.GetByIdAsync(tableId);
                Assert.True(table.Rows.Count >= 2);
            }
        }

        public void Dispose()
        {
            _testHelper.CleanupAsync().Wait();
        }
    }
}