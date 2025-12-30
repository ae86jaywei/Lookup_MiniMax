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
    /// 查找表完整性测试
    /// 验证查找表数据的完整性和一致性
    /// </summary>
    public class LookupTableIntegrityTests : IDisposable
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IIntegrationTestHelper _testHelper;

        public LookupTableIntegrityTests()
        {
            _lookupTableService = new LookupTableService();
            _testHelper = new IntegrationTestHelper();
        }

        [Fact]
        public async Task CreateLookupTable_ShouldHaveCompleteStructure()
        {
            // Arrange
            var tableData = new LookupTableCreateDto
            {
                Name = "完整性测试表",
                Description = "用于测试数据完整性的查找表",
                Columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition { Name = "Key", Type = ColumnType.String, IsRequired = true },
                    new ColumnDefinition { Name = "Value", Type = ColumnType.String, IsRequired = true },
                    new ColumnDefinition { Name = "Category", Type = ColumnType.String, IsRequired = false }
                }
            };

            // Act
            var lookupTable = await _lookupTableService.CreateAsync(tableData);

            // Assert
            Assert.NotNull(lookupTable);
            Assert.NotNull(lookupTable.Id);
            Assert.Equal(tableData.Name, lookupTable.Name);
            Assert.Equal(tableData.Columns.Count, lookupTable.Columns.Count);
            Assert.True(lookupTable.CreatedAt <= DateTime.Now);
        }

        [Fact]
        public async Task AddLookupTableRows_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var testRows = new List<LookupRowDto>
            {
                new LookupRowDto { Key = "TestKey1", Value = "TestValue1", Category = "Category1" },
                new LookupRowDto { Key = "TestKey2", Value = "TestValue2", Category = "Category2" },
                new LookupRowDto { Key = "TestKey3", Value = "TestValue3", Category = "Category1" }
            };

            // Act
            foreach (var row in testRows)
            {
                await _lookupTableService.AddRowAsync(lookupTableId, row);
            }

            // Assert
            var updatedTable = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.NotNull(updatedTable);
            Assert.Equal(testRows.Count, updatedTable.Rows.Count);
            
            // 验证数据完整性
            foreach (var row in testRows)
            {
                Assert.Contains(updatedTable.Rows, r => r.Key == row.Key && r.Value == row.Value);
            }
        }

        [Fact]
        public async Task UpdateLookupTableRow_ShouldPreserveIntegrity()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var originalRow = await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto 
            { 
                Key = "OriginalKey", 
                Value = "OriginalValue" 
            });

            // Act
            var updatedRow = await _lookupTableService.UpdateRowAsync(lookupTableId, "OriginalKey", new LookupRowDto 
            { 
                Key = "UpdatedKey", 
                Value = "UpdatedValue" 
            });

            // Assert
            Assert.NotNull(updatedRow);
            Assert.Equal("UpdatedKey", updatedRow.Key);
            Assert.Equal("UpdatedValue", updatedRow.Value);
            
            // 验证原始数据已不存在
            var table = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.DoesNotContain(table.Rows, r => r.Key == "OriginalKey");
        }

        [Fact]
        public async Task DeleteLookupTableRow_ShouldMaintainConsistency()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var testRows = new List<string>();
            
            for (int i = 1; i <= 5; i++)
            {
                var row = await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto 
                { 
                    Key = $"Key{i}", 
                    Value = $"Value{i}" 
                });
                testRows.Add($"Key{i}");
            }

            // Act - 删除中间的行
            await _lookupTableService.DeleteRowAsync(lookupTableId, "Key3");

            // Assert
            var table = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.Equal(4, table.Rows.Count);
            Assert.DoesNotContain(table.Rows, r => r.Key == "Key3");
            
            // 验证其他行仍然存在
            Assert.Contains(table.Rows, r => r.Key == "Key1");
            Assert.Contains(table.Rows, r => r.Key == "Key2");
            Assert.Contains(table.Rows, r => r.Key == "Key4");
            Assert.Contains(table.Rows, r => r.Key == "Key5");
        }

        [Fact]
        public async Task LookupTableSearch_ShouldReturnAccurateResults()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // 添加测试数据
            var testData = new Dictionary<string, string>
            {
                { "Apple", "苹果" },
                { "Banana", "香蕉" },
                { "Orange", "橙子" },
                { "Grape", "葡萄" }
            };

            foreach (var item in testData)
            {
                await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto 
                { 
                    Key = item.Key, 
                    Value = item.Value 
                });
            }

            // Act - 搜索测试
            var appleResults = await _lookupTableService.SearchAsync(lookupTableId, "Apple");
            var fruitResults = await _lookupTableService.SearchAsync(lookupTableId, "果");

            // Assert
            Assert.Single(appleResults);
            Assert.Equal("Apple", appleResults[0].Key);
            Assert.Equal("苹果", appleResults[0].Value);
            
            Assert.Equal(3, fruitResults.Count);
            Assert.All(fruitResults, result => Assert.Contains("果", result.Value));
        }

        [Fact]
        public async Task BulkOperations_ShouldMaintainIntegrity()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var bulkData = new List<LookupRowDto>();
            
            // 生成大量测试数据
            for (int i = 1; i <= 1000; i++)
            {
                bulkData.Add(new LookupRowDto 
                { 
                    Key = $"BulkKey_{i:D4}", 
                    Value = $"BulkValue_{i:D4}" 
                });
            }

            // Act - 批量添加
            await _lookupTableService.AddRowsAsync(lookupTableId, bulkData);

            // Assert - 验证完整性
            var table = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.Equal(1000, table.Rows.Count);
            
            // 随机验证几个数据点
            var randomKeys = new[] { "BulkKey_0001", "BulkKey_0500", "BulkKey_1000" };
            foreach (var key in randomKeys)
            {
                Assert.Contains(table.Rows, r => r.Key == key);
            }
        }

        [Fact]
        public async Task InvalidData_ShouldBeRejected()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();

            // Act & Assert - 测试各种无效数据
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto { Key = "", Value = "Value" }));
            
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto { Key = null, Value = "Value" }));
            
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto { Key = "ValidKey", Value = null }));
        }

        [Fact]
        public async Task DataValidation_ShouldEnforceConstraints()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            
            // Act - 添加有效数据
            var validRow = await _lookupTableService.AddRowAsync(lookupTableId, new LookupRowDto 
            { 
                Key = "ValidKey", 
                Value = "ValidValue" 
            });

            // Assert
            Assert.NotNull(validRow);
            Assert.True(validRow.CreatedAt <= DateTime.Now);
            Assert.True(validRow.UpdatedAt >= validRow.CreatedAt);
        }

        [Fact]
        public async Task DataBackupAndRestore_ShouldPreserveIntegrity()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var testData = new List<LookupRowDto>
            {
                new LookupRowDto { Key = "BackupKey1", Value = "BackupValue1" },
                new LookupRowDto { Key = "BackupKey2", Value = "BackupValue2" }
            };

            foreach (var row in testData)
            {
                await _lookupTableService.AddRowAsync(lookupTableId, row);
            }

            // 创建备份
            var backup = await _lookupTableService.CreateBackupAsync(lookupTableId);
            Assert.NotNull(backup);

            // Act - 修改数据
            await _lookupTableService.UpdateRowAsync(lookupTableId, "BackupKey1", new LookupRowDto 
            { 
                Key = "ModifiedKey", 
                Value = "ModifiedValue" 
            });

            // 恢复备份
            await _lookupTableService.RestoreFromBackupAsync(lookupTableId, backup.Id);

            // Assert - 验证数据已恢复
            var restoredTable = await _lookupTableService.GetByIdAsync(lookupTableId);
            Assert.Contains(restoredTable.Rows, r => r.Key == "BackupKey1" && r.Value == "BackupValue1");
            Assert.Contains(restoredTable.Rows, r => r.Key == "BackupKey2" && r.Value == "BackupValue2");
            Assert.DoesNotContain(restoredTable.Rows, r => r.Key == "ModifiedKey");
        }

        public void Dispose()
        {
            _testHelper.CleanupAsync().Wait();
        }
    }
}