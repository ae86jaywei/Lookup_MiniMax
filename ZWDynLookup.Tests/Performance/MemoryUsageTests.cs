using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWDynLookup.Models;
using ZWDynLookup.Service;

namespace ZWDynLookup.Tests.Performance
{
    /// <summary>
    /// 内存使用性能测试
    /// 测试查寻表系统的内存使用情况
    /// </summary>
    [TestClass]
    public class MemoryUsageTests : PerformanceTests
    {
        private LookupTableService _lookupService;
        private long _baselineMemory;

        [TestInitialize]
        public void TestInitialize()
        {
            _lookupService = new LookupTableService();
            _baselineMemory = GC.GetTotalMemory(true);
        }

        #region 基础内存使用测试

        /// <summary>
        /// 测试空查寻表的内存占用
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestEmptyTable_MemoryUsage()
        {
            // Arrange
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore = GC.GetTotalMemory(true);

            // Act
            _lookupService.LoadLookupTable(new LookupTableData());
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfter = GC.GetTotalMemory(false);
            long memoryUsed = memoryAfter - memoryBefore;

            // Assert
            RecordResult("EmptyTableMemory", 0, memoryUsed, 0);
            
            // 空查寻表内存占用应该很小（小于1MB）
            Assert.IsTrue(memoryUsed < 1024 * 1024,
                $"空查寻表内存占用 {memoryUsed} bytes 过大");
        }

        /// <summary>
        /// 测试不同数据大小的内存占用
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestDataSize_MemoryUsage()
        {
            // Arrange
            var sizes = new[] { 100, 1000, 10000, 50000, 100000 };

            foreach (var size in sizes)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long memoryBefore = GC.GetTotalMemory(true);

                // Act
                var testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(testData);
                
                // 执行一些操作
                for (int i = 0; i < Math.Min(100, size); i += 10)
                {
                    var key = $"ID_{i}";
                    var result = _lookupService.LookupValue(key);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                long memoryAfter = GC.GetTotalMemory(false);
                long memoryUsed = memoryAfter - memoryBefore;

                // Assert
                RecordResult($"DataSizeMemory_{size}", 0, memoryUsed, size);
                
                // 内存使用应该与数据大小成合理比例
                var bytesPerRow = (double)memoryUsed / size;
                Assert.IsTrue(bytesPerRow > 10 && bytesPerRow < 1000,
                    $"每行数据内存使用 {bytesPerRow:F2} bytes 不合理（总内存: {memoryUsed} bytes, 行数: {size}）");
            }
        }

        #endregion

        #region 对象创建内存测试

        /// <summary>
        /// 测试对象创建对内存的影响
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestObjectCreation_MemoryImpact()
        {
            // Arrange
            var sizes = new[] { 1000, 5000, 10000 };
            int objectCreationCount = 1000;

            foreach (var size in sizes)
            {
                // 测试创建对象前
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                long memoryBeforeObjects = GC.GetTotalMemory(true);

                // Act - 创建大量对象
                var objects = new List<object>();
                for (int i = 0; i < objectCreationCount; i++)
                {
                    var data = GenerateTestData(Math.Min(100, size));
                    var service = new LookupTableService();
                    service.LoadLookupTable(data);
                    objects.Add(new { Data = data, Service = service });
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                long memoryAfterObjects = GC.GetTotalMemory(false);
                long memoryUsedByObjects = memoryAfterObjects - memoryBeforeObjects;

                // Assert
                RecordResult($"ObjectCreation_{size}", 0, memoryUsedByObjects, objectCreationCount);
                
                // 对象创建应该消耗合理内存
                var avgMemoryPerObject = (double)memoryUsedByObjects / objectCreationCount;
                Assert.IsTrue(avgMemoryPerObject < 10000,
                    $"平均每个对象内存使用 {avgMemoryPerObject:F2} bytes 过大");
            }
        }

        /// <summary>
        /// 测试字典对象的内存效率
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestDictionary_MemoryEfficiency()
        {
            // Arrange
            int size = 10000;
            var testData = GenerateTestData(size);

            // 测试字典的不同实现
            var implementations = new Dictionary<string, Func<LookupTableData, object>>
            {
                ["Dictionary"] = data => new Dictionary<string, object>(data.Data),
                ["ConcurrentDictionary"] = data => new System.Collections.Concurrent.ConcurrentDictionary<string, object>(data.Data)
            };

            foreach (var implementation in implementations)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long memoryBefore = GC.GetTotalMemory(true);

                // Act
                var dict = implementation.Value(testData);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                long memoryAfter = GC.GetTotalMemory(false);
                long memoryUsed = memoryAfter - memoryBefore;

                // Assert
                RecordResult($"DictionaryMemory_{implementation.Key}", 0, memoryUsed, size);
                
                // 字典内存使用应该合理
                var bytesPerEntry = (double)memoryUsed / size;
                Assert.IsTrue(bytesPerEntry < 500,
                    $"{implementation.Key} 每条目内存使用 {bytesPerEntry:F2} bytes 过大");
            }
        }

        #endregion

        #region 内存泄漏测试

        /// <summary>
        /// 测试查寻表操作的内存泄漏
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestLookupTable_MemoryLeaks()
        {
            // Arrange
            int size = 5000;
            int iterations = 200;
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long initialMemory = GC.GetTotalMemory(true);

            // Act - 重复执行查寻表操作
            for (int i = 0; i < iterations; i++)
            {
                var testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(testData);
                
                // 执行查寻操作
                for (int j = 0; j < 50; j++)
                {
                    var key = $"ID_{j % (size / 10)}";
                    var result = _lookupService.LookupValue(key);
                }
                
                _lookupService.ClearTable();
                
                // 定期强制垃圾回收
                if (i % 50 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long finalMemory = GC.GetTotalMemory(false);
            long memoryGrowth = finalMemory - initialMemory;

            // Assert
            RecordResult("LookupTableMemoryLeaks", 0, memoryGrowth, size * iterations);
            
            // 内存增长应该在合理范围内（小于50MB）
            Assert.IsTrue(memoryGrowth < 50 * 1024 * 1024,
                $"检测到查寻表内存泄漏，增长了 {memoryGrowth} bytes");
        }

        /// <summary>
        /// 测试UI组件内存泄漏
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestUIComponents_MemoryLeaks()
        {
            // Arrange
            int size = 2000;
            int componentCount = 100;
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long initialMemory = GC.GetTotalMemory(true);

            // Act - 创建和销毁UI组件
            for (int i = 0; i < componentCount; i++)
            {
                var testData = GenerateTestData(size);
                
                // 模拟UI组件操作
                var menu = CreateMockMenu(testData);
                var dialog = CreateMockDialog(testData);
                var editor = CreateMockEditor(testData);
                
                // 清理
                CleanupMenu(menu);
                CleanupDialog(dialog);
                CleanupEditor(editor);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long finalMemory = GC.GetTotalMemory(false);
            long memoryGrowth = finalMemory - initialMemory;

            // Assert
            RecordResult("UIComponentsMemoryLeaks", 0, memoryGrowth, componentCount * size);
            
            // UI组件内存增长应该在合理范围内（小于20MB）
            Assert.IsTrue(memoryGrowth < 20 * 1024 * 1024,
                $"检测到UI组件内存泄漏，增长了 {memoryGrowth} bytes");
        }

        #endregion

        #region 内存池和缓存测试

        /// <summary>
        /// 测试内存池效率
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestMemoryPool_Efficiency()
        {
            // Arrange
            int size = 10000;
            int poolSize = 100;
            int iterations = 50;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryWithPool = 0;
            long memoryWithoutPool = 0;

            // Act - 测试使用内存池
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long beforeWithPool = GC.GetTotalMemory(true);
            var pool = new MemoryPool(poolSize);
            
            for (int i = 0; i < iterations; i++)
            {
                var buffer = pool.Rent();
                var testData = GenerateTestData(size);
                FillBuffer(buffer, testData);
                pool.Return(buffer);
            }
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            memoryWithPool = GC.GetTotalMemory(false) - beforeWithPool;

            // Act - 测试不使用内存池
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long beforeWithoutPool = GC.GetTotalMemory(true);
            
            for (int i = 0; i < iterations; i++)
            {
                var testData = GenerateTestData(size);
                var buffer = new byte[size * 100]; // 模拟缓冲区分配
                FillBuffer(buffer, testData);
            }
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            memoryWithoutPool = GC.GetTotalMemory(false) - beforeWithoutPool;

            // Assert
            RecordResult("MemoryPoolEfficiency", 0, memoryWithoutPool - memoryWithPool, iterations);
            
            // 内存池应该减少内存使用
            Assert.IsTrue(memoryWithPool < memoryWithoutPool,
                $"内存池没有减少内存使用 (池: {memoryWithPool}, 无池: {memoryWithoutPool})");
        }

        /// <summary>
        /// 测试查寻结果缓存
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void TestLookupCache_MemoryUsage()
        {
            // Arrange
            int size = 10000;
            int cacheSize = 1000;
            int testIterations = 100;
            
            var testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(testData);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryBefore = GC.GetTotalMemory(true);

            // Act - 启用缓存并进行重复查寻
            _lookupService.EnableCache(cacheSize);
            
            var keys = new List<string>();
            for (int i = 0; i < testIterations; i++)
            {
                keys.Add($"ID_{i % (size / 10)}");
            }

            // 第一次查寻（填充缓存）
            foreach (var key in keys)
            {
                var result = _lookupService.LookupValue(key);
            }

            // 第二次查寻（使用缓存）
            foreach (var key in keys)
            {
                var result = _lookupService.LookupValue(key);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfter = GC.GetTotalMemory(false);
            long memoryUsed = memoryAfter - memoryBefore;

            // Assert
            RecordResult("LookupCacheMemory", 0, memoryUsed, testIterations);
            
            // 缓存内存使用应该合理
            var memoryPerCacheEntry = (double)memoryUsed / Math.Min(cacheSize, keys.Count);
            Assert.IsTrue(memoryPerCacheEntry < 1000,
                $"每个缓存条目内存使用 {memoryPerCacheEntry:F2} bytes 过大");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建模拟菜单
        /// </summary>
        private object CreateMockMenu(LookupTableData data)
        {
            return new
            {
                Data = data,
                Items = new List<object> { "Item1", "Item2", "Item3" },
                SelectedIndex = 0
            };
        }

        /// <summary>
        /// 创建模拟对话框
        /// </summary>
        private object CreateMockDialog(LookupTableData data)
        {
            return new
            {
                Data = data,
                Controls = new Dictionary<string, object>
                {
                    ["TextBox1"] = "Test",
                    ["ComboBox1"] = "Option1",
                    ["CheckBox1"] = true
                },
                IsVisible = false
            };
        }

        /// <summary>
        /// 创建模拟编辑器
        /// </summary>
        private object CreateMockEditor(LookupTableData data)
        {
            return new
            {
                Data = data,
                Grid = new object[100, 10], // 模拟数据网格
                Selection = new List<int> { 0, 1, 2 },
                IsDirty = false
            };
        }

        /// <summary>
        /// 清理菜单
        /// </summary>
        private void CleanupMenu(object menu)
        {
            // 模拟清理操作
        }

        /// <summary>
        /// 清理对话框
        /// </summary>
        private void CleanupDialog(object dialog)
        {
            // 模拟清理操作
        }

        /// <summary>
        /// 清理编辑器
        /// </summary>
        private void CleanupEditor(object editor)
        {
            // 模拟清理操作
        }

        /// <summary>
        /// 内存池类
        /// </summary>
        private class MemoryPool
        {
            private readonly Queue<byte[]> _pool;
            private readonly int _poolSize;

            public MemoryPool(int poolSize)
            {
                _poolSize = poolSize;
                _pool = new Queue<byte[]>();
                
                for (int i = 0; i < poolSize; i++)
                {
                    _pool.Enqueue(new byte[1024]); // 1KB 缓冲区
                }
            }

            public byte[] Rent()
            {
                if (_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
                return new byte[1024];
            }

            public void Return(byte[] buffer)
            {
                if (_pool.Count < _poolSize)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    _pool.Enqueue(buffer);
                }
            }
        }

        /// <summary>
        /// 填充缓冲区
        /// </summary>
        private void FillBuffer(byte[] buffer, LookupTableData data)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            
            if (buffer.Length >= bytes.Length)
            {
                Array.Copy(bytes, buffer, bytes.Length);
            }
        }

        #endregion
    }
}