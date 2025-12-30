using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWDynLookup.Models;
using ZWDynLookup.Service;

namespace ZWDynLookup.Tests.Performance
{
    /// <summary>
    /// 大型查寻表性能测试
    /// 测试不同规模查寻表的性能表现
    /// </summary>
    [TestClass]
    public class LargeLookupTableTests : PerformanceTests
    {
        private LookupTableData _testData;
        private LookupTableService _lookupService;

        [TestInitialize]
        public void TestInitialize()
        {
            _lookupService = new LookupTableService();
        }

        #region 查寻表加载性能测试

        /// <summary>
        /// 测试小型查寻表加载性能（100行）
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestSmallTableLoad_Performance()
        {
            // Arrange
            int size = 100;
            _testData = GenerateTestData(size);
            int iterations = _benchmarkConfig?.Iterations ?? 10;

            // Act
            var executionTimes = new List<double>();
            var memoryUsages = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var (execTime, memoryUsed) = MeasureExecutionTimeAndMemory(() =>
                {
                    _lookupService.LoadLookupTable(_testData);
                });
                
                executionTimes.Add(execTime);
                memoryUsages.Add(memoryUsed);
            }

            // Assert
            var avgExecutionTime = executionTimes.Average();
            var avgMemoryUsage = memoryUsages.Average();
            
            RecordResult("SmallTableLoad", avgExecutionTime, (long)avgMemoryUsage, size);
            
            Assert.IsTrue(avgExecutionTime <= _benchmarkConfig.Thresholds.SmallTableResponseTime,
                $"小型查寻表加载时间 {avgExecutionTime:F2}ms 超过了阈值 {_benchmarkConfig.Thresholds.SmallTableResponseTime}ms");
            
            Assert.IsTrue(avgMemoryUsage <= _benchmarkConfig.Thresholds.MaxMemoryUsage / 10,
                $"内存使用 {avgMemoryUsage} bytes 过大");
        }

        /// <summary>
        /// 测试中型查寻表加载性能（1000行）
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestMediumTableLoad_Performance()
        {
            // Arrange
            int size = 1000;
            _testData = GenerateTestData(size);
            int iterations = _benchmarkConfig?.Iterations ?? 10;

            // Act
            var executionTimes = new List<double>();
            var memoryUsages = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var (execTime, memoryUsed) = MeasureExecutionTimeAndMemory(() =>
                {
                    _lookupService.LoadLookupTable(_testData);
                });
                
                executionTimes.Add(execTime);
                memoryUsages.Add(memoryUsed);
            }

            // Assert
            var avgExecutionTime = executionTimes.Average();
            var avgMemoryUsage = memoryUsages.Average();
            
            RecordResult("MediumTableLoad", avgExecutionTime, (long)avgMemoryUsage, size);
            
            Assert.IsTrue(avgExecutionTime <= _benchmarkConfig.Thresholds.MediumTableResponseTime,
                $"中型查寻表加载时间 {avgExecutionTime:F2}ms 超过了阈值 {_benchmarkConfig.Thresholds.MediumTableResponseTime}ms");
        }

        /// <summary>
        /// 测试大型查寻表加载性能（10000行）
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestLargeTableLoad_Performance()
        {
            // Arrange
            int size = 10000;
            _testData = GenerateTestData(size);
            int iterations = _benchmarkConfig?.Iterations ?? 10;

            // Act
            var executionTimes = new List<double>();
            var memoryUsages = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var (execTime, memoryUsed) = MeasureExecutionTimeAndMemory(() =>
                {
                    _lookupService.LoadLookupTable(_testData);
                });
                
                executionTimes.Add(execTime);
                memoryUsages.Add(memoryUsed);
            }

            // Assert
            var avgExecutionTime = executionTimes.Average();
            var avgMemoryUsage = memoryUsages.Average();
            
            RecordResult("LargeTableLoad", avgExecutionTime, (long)avgMemoryUsage, size);
            
            Assert.IsTrue(avgExecutionTime <= _benchmarkConfig.Thresholds.LargeTableResponseTime,
                $"大型查寻表加载时间 {avgExecutionTime:F2}ms 超过了阈值 {_benchmarkConfig.Thresholds.LargeTableResponseTime}ms");
        }

        /// <summary>
        /// 测试超大型查寻表加载性能（100000行）
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestExtraLargeTableLoad_Performance()
        {
            // Arrange
            int size = 100000;
            _testData = GenerateTestData(size);
            int iterations = _benchmarkConfig?.Iterations ?? 5; // 减少迭代次数以节省时间

            // Act
            var executionTimes = new List<double>();
            var memoryUsages = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var (execTime, memoryUsed) = MeasureExecutionTimeAndMemory(() =>
                {
                    _lookupService.LoadLookupTable(_testData);
                });
                
                executionTimes.Add(execTime);
                memoryUsages.Add(memoryUsed);
            }

            // Assert
            var avgExecutionTime = executionTimes.Average();
            var avgMemoryUsage = memoryUsages.Average();
            
            RecordResult("ExtraLargeTableLoad", avgExecutionTime, (long)avgMemoryUsage, size);
            
            Assert.IsTrue(avgExecutionTime <= _benchmarkConfig.Thresholds.ExtraLargeTableResponseTime,
                $"超大型查寻表加载时间 {avgExecutionTime:F2}ms 超过了阈值 {_benchmarkConfig.Thresholds.ExtraLargeTableResponseTime}ms");
        }

        #endregion

        #region 查寻性能测试

        /// <summary>
        /// 测试查寻操作性能
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestLookupOperations_Performance()
        {
            // Arrange
            var sizes = new[] { 100, 1000, 10000, 100000 };
            
            foreach (var size in sizes)
            {
                _testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(_testData);
                
                var testKeys = new List<string>();
                for (int i = 0; i < Math.Min(100, size); i += size / 100 + 1)
                {
                    testKeys.Add($"ID_{i}");
                }

                // Act
                var executionTimes = new List<double>();
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 10; i++)
                {
                    foreach (var key in testKeys)
                    {
                        var execTime = MeasureExecutionTime(() =>
                        {
                            var result = _lookupService.LookupValue(key);
                        });
                        executionTimes.Add(execTime);
                    }
                }

                // Assert
                var avgExecutionTime = executionTimes.Average();
                RecordResult($"LookupOperations_{size}", avgExecutionTime, 0, size);
                
                // 查寻操作应该很快
                Assert.IsTrue(avgExecutionTime < 10, 
                    $"查寻操作平均时间 {avgExecutionTime:F2}ms 对于数据大小 {size} 过大");
            }
        }

        /// <summary>
        /// 测试批量查寻性能
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestBatchLookup_Performance()
        {
            // Arrange
            int size = 10000;
            _testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(_testData);
            
            var batchSizes = new[] { 10, 50, 100, 500, 1000 };
            
            foreach (var batchSize in batchSizes)
            {
                var keys = new List<string>();
                for (int i = 0; i < batchSize; i++)
                {
                    keys.Add($"ID_{i % (size / 100)}");
                }

                // Act
                var executionTime = MeasureExecutionTime(() =>
                {
                    var results = _lookupService.BatchLookup(keys);
                });

                // Assert
                RecordResult($"BatchLookup_{batchSize}", executionTime, 0, size);
                
                // 批量查寻时间应该与批量大小成线性关系
                var expectedMaxTime = batchSize * 0.1; // 每个查寻不超过0.1ms
                Assert.IsTrue(executionTime <= expectedMaxTime,
                    $"批量查寻 {batchSize} 个项目时间 {executionTime:F2}ms 超过预期 {expectedMaxTime:F2}ms");
            }
        }

        #endregion

        #region 查寻表更新性能测试

        /// <summary>
        /// 测试查寻表更新性能
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestTableUpdate_Performance()
        {
            // Arrange
            var sizes = new[] { 100, 1000, 10000 };
            
            foreach (var size in sizes)
            {
                _testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(_testData);
                
                var updateData = new Dictionary<string, object>
                {
                    ["ID_0"] = 0,
                    ["Value_0"] = 999.99,
                    ["ID_1"] = 1,
                    ["Value_1"] = 888.88
                };

                // Act
                var executionTime = MeasureExecutionTime(() =>
                {
                    _lookupService.UpdateLookupTable(updateData);
                });

                // Assert
                RecordResult($"TableUpdate_{size}", executionTime, 0, size);
                
                // 更新操作应该在合理时间内完成
                var maxUpdateTime = size < 1000 ? 50 : size < 10000 ? 200 : 1000;
                Assert.IsTrue(executionTime <= maxUpdateTime,
                    $"更新 {size} 行数据时间 {executionTime:F2}ms 超过阈值 {maxUpdateTime}ms");
            }
        }

        /// <summary>
        /// 测试增量更新性能
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestIncrementalUpdate_Performance()
        {
            // Arrange
            int size = 10000;
            _testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(_testData);
            
            var updateSizes = new[] { 10, 100, 1000 };
            
            foreach (var updateSize in updateSizes)
            {
                var updateData = new Dictionary<string, object>();
                for (int i = 0; i < updateSize; i++)
                {
                    updateData[$"ID_{i}"] = i;
                    updateData[$"Value_{i}"] = i * 2.0;
                }

                // Act
                var executionTime = MeasureExecutionTime(() =>
                {
                    _lookupService.IncrementalUpdate(updateData);
                });

                // Assert
                RecordResult($"IncrementalUpdate_{updateSize}", executionTime, 0, size);
                
                // 增量更新应该很快
                Assert.IsTrue(executionTime < updateSize * 0.5,
                    $"增量更新 {updateSize} 行数据时间 {executionTime:F2}ms 过大");
            }
        }

        #endregion

        #region 并发性能测试

        /// <summary>
        /// 测试多线程查寻性能
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestConcurrentLookup_Performance()
        {
            // Arrange
            int size = 10000;
            _testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(_testData);
            
            var threadCounts = new[] { 2, 5, 10, 20 };
            int operationsPerThread = 100;

            foreach (var threadCount in threadCounts)
            {
                var tasks = new List<Task>();
                var stopwatch = Stopwatch.StartNew();

                // Act
                for (int t = 0; t < threadCount; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            var key = $"ID_{i % (size / 100)}";
                            var result = _lookupService.LookupValue(key);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                stopwatch.Stop();

                var totalOperations = threadCount * operationsPerThread;
                var avgTimePerOperation = stopwatch.Elapsed.TotalMilliseconds / totalOperations;

                // Assert
                RecordResult($"ConcurrentLookup_{threadCount}", stopwatch.Elapsed.TotalMilliseconds, 0, size);
                
                // 并发查寻的平均时间应该合理
                Assert.IsTrue(avgTimePerOperation < 1.0,
                    $"并发查寻平均操作时间 {avgTimePerOperation:F4}ms 过大（{threadCount}线程）");
            }
        }

        /// <summary>
        /// 测试并发更新性能
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestConcurrentUpdate_Performance()
        {
            // Arrange
            int size = 10000;
            _testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(_testData);
            
            var threadCounts = new[] { 2, 5, 10 };
            int updatesPerThread = 50;

            foreach (var threadCount in threadCounts)
            {
                var tasks = new List<Task>();
                var stopwatch = Stopwatch.StartNew();

                // Act
                for (int t = 0; t < threadCount; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < updatesPerThread; i++)
                        {
                            var updateData = new Dictionary<string, object>
                            {
                                [$"ID_{i}"] = i,
                                [$"Value_{i}"] = i * 3.0
                            };
                            _lookupService.IncrementalUpdate(updateData);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                stopwatch.Stop();

                // Assert
                RecordResult($"ConcurrentUpdate_{threadCount}", stopwatch.Elapsed.TotalMilliseconds, 0, size);
                
                // 并发更新应该能正确处理
                Assert.IsTrue(stopwatch.Elapsed.TotalMilliseconds < threadCount * updatesPerThread * 10,
                    $"并发更新执行时间 {stopwatch.Elapsed.TotalMilliseconds:F2}ms 过大");
            }
        }

        #endregion

        #region 内存管理性能测试

        /// <summary>
        /// 测试大查寻表的内存使用
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestMemoryUsage_LargeTable()
        {
            // Arrange
            var sizes = new[] { 10000, 50000, 100000 };

            foreach (var size in sizes)
            {
                // Act
                var (executionTime, memoryUsed) = MeasureExecutionTimeAndMemory(() =>
                {
                    _testData = GenerateTestData(size);
                    _lookupService.LoadLookupTable(_testData);
                });

                // Assert
                RecordResult($"MemoryUsage_{size}", executionTime, memoryUsed, size);
                
                // 检查内存使用是否合理（不超过数据大小的100倍）
                var maxExpectedMemory = size * 100;
                Assert.IsTrue(memoryUsed < maxExpectedMemory,
                    $"内存使用 {memoryUsed} bytes 对于 {size} 行数据过大");
            }
        }

        /// <summary>
        /// 测试内存泄漏
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void TestMemoryLeaks()
        {
            // Arrange
            long memoryBefore = GC.GetTotalMemory(true);
            int iterations = 100;
            int size = 1000;

            // Act - 重复创建和销毁查寻表
            for (int i = 0; i < iterations; i++)
            {
                var data = GenerateTestData(size);
                _lookupService.LoadLookupTable(data);
                _lookupService.ClearTable();
                
                // 强制垃圾回收
                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfter = GC.GetTotalMemory(false);
            long memoryDifference = memoryAfter - memoryBefore;

            // Assert
            RecordResult("MemoryLeaks", 0, memoryDifference, size * iterations);
            
            // 内存增长应该很小（小于10MB）
            Assert.IsTrue(memoryDifference < 10 * 1024 * 1024,
                $"检测到内存泄漏，增长了 {memoryDifference} bytes");
        }

        #endregion
    }
}