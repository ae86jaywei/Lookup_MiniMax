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
    /// 响应时间性能测试
    /// 测试查寻表系统的响应时间表现
    /// </summary>
    [TestClass]
    public class ResponseTimeTests : PerformanceTests
    {
        private LookupTableService _lookupService;
        private List<string> _testKeys;

        [TestInitialize]
        public void TestInitialize()
        {
            _lookupService = new LookupTableService();
            _testKeys = new List<string>();
        }

        #region 查寻响应时间测试

        /// <summary>
        /// 测试单个查寻响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestSingleLookup_ResponseTime()
        {
            // Arrange
            var sizes = new[] { 100, 1000, 10000, 100000 };
            
            foreach (var size in sizes)
            {
                var testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(testData);
                
                var testKey = $"ID_{size / 2}";

                // Act - 测量多个样本的响应时间
                var responseTimes = new List<double>();
                
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 20; i++)
                {
                    var responseTime = MeasureExecutionTime(() =>
                    {
                        var result = _lookupService.LookupValue(testKey);
                    });
                    responseTimes.Add(responseTime);
                }

                // Assert
                var avgResponseTime = responseTimes.Average();
                var minResponseTime = responseTimes.Min();
                var maxResponseTime = responseTimes.Max();
                var p95ResponseTime = GetPercentile(responseTimes, 95);
                var p99ResponseTime = GetPercentile(responseTimes, 99);
                
                RecordResult($"SingleLookup_{size}", avgResponseTime, 0, size);
                
                // 验证响应时间阈值
                Assert.IsTrue(avgResponseTime <= GetThreshold(size),
                    $"平均响应时间 {avgResponseTime:F2}ms 超过阈值 {GetThreshold(size)}ms (数据大小: {size})");
                
                Assert.IsTrue(p95ResponseTime <= avgResponseTime * 2,
                    $"95%响应时间 {p95ResponseTime:F2}ms 过高 (平均: {avgResponseTime:F2}ms)");
                
                Assert.IsTrue(maxResponseTime <= avgResponseTime * 5,
                    $"最大响应时间 {maxResponseTime:F2}ms 过高 (平均: {avgResponseTime:F2}ms)");
            }
        }

        /// <summary>
        /// 测试批量查寻响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestBatchLookup_ResponseTime()
        {
            // Arrange
            int size = 10000;
            var testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(testData);
            
            var batchSizes = new[] { 10, 50, 100, 500, 1000 };
            
            foreach (var batchSize in batchSizes)
            {
                var keys = new List<string>();
                for (int i = 0; i < batchSize; i++)
                {
                    keys.Add($"ID_{i % (size / 100)}");
                }

                // Act
                var responseTimes = new List<double>();
                
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 10; i++)
                {
                    var responseTime = MeasureExecutionTime(() =>
                    {
                        var results = _lookupService.BatchLookup(keys);
                    });
                    responseTimes.Add(responseTime);
                }

                // Assert
                var avgResponseTime = responseTimes.Average();
                var avgTimePerLookup = avgResponseTime / batchSize;
                
                RecordResult($"BatchLookup_{batchSize}", avgResponseTime, 0, size);
                
                // 批量查寻的平均时间应该合理
                Assert.IsTrue(avgTimePerLookup < 1.0,
                    $"批量查寻平均每项时间 {avgTimePerLookup:F4}ms 过大 (批量大小: {batchSize})");
                
                Assert.IsTrue(avgResponseTime < batchSize * 2,
                    $"批量查寻总时间 {avgResponseTime:F2}ms 过大 (批量大小: {batchSize})");
            }
        }

        /// <summary>
        /// 测试复杂查寻响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestComplexLookup_ResponseTime()
        {
            // Arrange
            var sizes = new[] { 1000, 10000 };
            
            foreach (var size in sizes)
            {
                var testData = GenerateComplexTestData(size);
                _lookupService.LoadLookupTable(testData);
                
                var criteria = new Dictionary<string, object>
                {
                    ["Category"] = "Category_5",
                    ["Status"] = "Active",
                    ["Priority"] = 3
                };

                // Act
                var responseTimes = new List<double>();
                
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 15; i++)
                {
                    var responseTime = MeasureExecutionTime(() =>
                    {
                        var results = _lookupService.ComplexLookup(criteria);
                    });
                    responseTimes.Add(responseTime);
                }

                // Assert
                var avgResponseTime = responseTimes.Average();
                
                RecordResult($"ComplexLookup_{size}", avgResponseTime, 0, size);
                
                // 复杂查寻应该仍然在合理时间内
                var threshold = GetThreshold(size) * 2; // 复杂查寻可以有更长阈值
                Assert.IsTrue(avgResponseTime <= threshold,
                    $"复杂查寻平均响应时间 {avgResponseTime:F2}ms 超过阈值 {threshold}ms");
            }
        }

        #endregion

        #region 查寻表操作响应时间测试

        /// <summary>
        /// 测试查寻表加载响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestTableLoad_ResponseTime()
        {
            // Arrange
            var sizes = new[] { 100, 1000, 10000, 50000, 100000 };
            
            foreach (var size in sizes)
            {
                // Act
                var responseTimes = new List<double>();
                
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 5; i++)
                {
                    var responseTime = MeasureExecutionTime(() =>
                    {
                        var testData = GenerateTestData(size);
                        _lookupService.LoadLookupTable(testData);
                    });
                    responseTimes.Add(responseTime);
                }

                // Assert
                var avgResponseTime = responseTimes.Average();
                var maxResponseTime = responseTimes.Max();
                
                RecordResult($"TableLoad_{size}", avgResponseTime, 0, size);
                
                // 验证加载时间阈值
                Assert.IsTrue(avgResponseTime <= GetLoadThreshold(size),
                    $"查寻表加载平均时间 {avgResponseTime:F2}ms 超过阈值 {GetLoadThreshold(size)}ms");
                
                Assert.IsTrue(maxResponseTime <= avgResponseTime * 3,
                    $"最大加载时间 {maxResponseTime:F2}ms 过高 (平均: {avgResponseTime:F2}ms)");
            }
        }

        /// <summary>
        /// 测试查寻表更新响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestTableUpdate_ResponseTime()
        {
            // Arrange
            var sizes = new[] { 1000, 10000 };
            var updateSizes = new[] { 10, 100, 1000 };
            
            foreach (var size in sizes)
            {
                var testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(testData);
                
                foreach (var updateSize in updateSizes)
                {
                    var updateData = new Dictionary<string, object>();
                    for (int i = 0; i < updateSize; i++)
                    {
                        updateData[$"ID_{i}"] = i;
                        updateData[$"Value_{i}"] = i * 2.0;
                    }

                    // Act
                    var responseTime = MeasureExecutionTime(() =>
                    {
                        _lookupService.UpdateLookupTable(updateData);
                    });

                    // Assert
                    RecordResult($"TableUpdate_{size}_{updateSize}", responseTime, 0, size);
                    
                    // 更新时间应该与更新大小成比例
                    var maxExpectedTime = updateSize * 0.5; // 每项不超过0.5ms
                    Assert.IsTrue(responseTime <= maxExpectedTime,
                        $"更新 {updateSize} 项数据时间 {responseTime:F2}ms 超过预期 {maxExpectedTime}ms");
                }
            }
        }

        /// <summary>
        /// 测试查寻表搜索响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestTableSearch_ResponseTime()
        {
            // Arrange
            var sizes = new[] { 10000, 50000 };
            
            foreach (var size in sizes)
            {
                var testData = GenerateTestData(size);
                _lookupService.LoadLookupTable(testData);
                
                var searchTerms = new[] { "Item_5000", "Category_3", "Active", "999" };

                foreach (var searchTerm in searchTerms)
                {
                    // Act
                    var responseTimes = new List<double>();
                    
                    for (int i = 0; i < _benchmarkConfig?.Iterations ?? 10; i++)
                    {
                        var responseTime = MeasureExecutionTime(() =>
                        {
                            var results = _lookupService.SearchTable(searchTerm);
                        });
                        responseTimes.Add(responseTime);
                    }

                    // Assert
                    var avgResponseTime = responseTimes.Average();
                    
                    RecordResult($"TableSearch_{size}_{searchTerm}", avgResponseTime, 0, size);
                    
                    // 搜索时间应该合理
                    Assert.IsTrue(avgResponseTime < 100,
                        $"搜索 '{searchTerm}' 平均时间 {avgResponseTime:F2}ms 过大");
                }
            }
        }

        #endregion

        #region UI操作响应时间测试

        /// <summary>
        /// 测试UI渲染响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestUIRender_ResponseTime()
        {
            // Arrange
            var sizes = new[] { 100, 1000, 10000 };
            
            foreach (var size in sizes)
            {
                var testData = GenerateTestData(size);

                // Act - 模拟UI渲染操作
                var responseTimes = new List<double>();
                
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 10; i++)
                {
                    var responseTime = MeasureExecutionTime(() =>
                    {
                        RenderMockUI(testData);
                    });
                    responseTimes.Add(responseTime);
                }

                // Assert
                var avgResponseTime = responseTimes.Average();
                
                RecordResult($"UIRender_{size}", avgResponseTime, 0, size);
                
                // UI渲染应该在合理时间内完成
                var threshold = size < 1000 ? 50 : size < 10000 ? 200 : 500;
                Assert.IsTrue(avgResponseTime <= threshold,
                    $"UI渲染平均时间 {avgResponseTime:F2}ms 超过阈值 {threshold}ms (数据大小: {size})");
            }
        }

        /// <summary>
        /// 测试用户交互响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestUserInteraction_ResponseTime()
        {
            // Arrange
            int size = 10000;
            var testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(testData);
            
            var interactions = new[]
            {
                ("Selection", (Action)(() => _lookupService.SelectItem("ID_5000"))),
                ("Filter", (Action)(() => _lookupService.FilterData("Category_5"))),
                ("Sort", (Action)(() => _lookupService.SortData("Value"))),
                ("Refresh", (Action)(() => _lookupService.RefreshTable()))
            };

            foreach (var (interactionName, interaction) in interactions)
            {
                // Act
                var responseTimes = new List<double>();
                
                for (int i = 0; i < _benchmarkConfig?.Iterations ?? 15; i++)
                {
                    var responseTime = MeasureExecutionTime(interaction);
                    responseTimes.Add(responseTime);
                }

                // Assert
                var avgResponseTime = responseTimes.Average();
                
                RecordResult($"UserInteraction_{interactionName}", avgResponseTime, 0, size);
                
                // 用户交互应该在50ms内响应
                Assert.IsTrue(avgResponseTime <= 50,
                    $"用户交互 '{interactionName}' 平均时间 {avgResponseTime:F2}ms 超过50ms");
            }
        }

        #endregion

        #region 并发响应时间测试

        /// <summary>
        /// 测试并发查寻响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestConcurrentLookup_ResponseTime()
        {
            // Arrange
            int size = 10000;
            var testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(testData);
            
            var threadCounts = new[] { 2, 5, 10, 20 };
            int operationsPerThread = 50;

            foreach (var threadCount in threadCounts)
            {
                var tasks = new List<Task<double>>();
                
                // Act
                for (int t = 0; t < threadCount; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            var key = $"ID_{i % (size / 100)}";
                            var result = _lookupService.LookupValue(key);
                        }
                        
                        stopwatch.Stop();
                        return stopwatch.Elapsed.TotalMilliseconds;
                    }));
                }

                var threadTimes = Task.WhenAll(tasks).Result;
                var totalTime = threadTimes.Max();
                var avgTimePerThread = threadTimes.Average();
                var avgTimePerOperation = totalTime / (threadCount * operationsPerThread);

                // Assert
                RecordResult($"ConcurrentLookup_{threadCount}", totalTime, 0, size);
                
                // 并发查寻的平均操作时间应该合理
                Assert.IsTrue(avgTimePerOperation < 1.0,
                    $"并发查寻平均操作时间 {avgTimePerOperation:F4}ms 过大 ({threadCount}线程)");
                
                Assert.IsTrue(totalTime < threadCount * operationsPerThread * 2,
                    $"并发查寻总时间 {totalTime:F2}ms 过大 ({threadCount}线程)");
            }
        }

        /// <summary>
        /// 测试并发更新响应时间
        /// </summary>
        [TestMethod]
        [TestCategory("ResponseTime")]
        public void TestConcurrentUpdate_ResponseTime()
        {
            // Arrange
            int size = 10000;
            var testData = GenerateTestData(size);
            _lookupService.LoadLookupTable(testData);
            
            var threadCounts = new[] { 2, 5, 10 };
            int updatesPerThread = 20;

            foreach (var threadCount in threadCounts)
            {
                var tasks = new List<Task<double>>();
                
                // Act
                for (int t = 0; t < threadCount; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        
                        for (int i = 0; i < updatesPerThread; i++)
                        {
                            var updateData = new Dictionary<string, object>
                            {
                                [$"ID_{threadId * updatesPerThread + i}"] = threadId * updatesPerThread + i,
                                [$"Value_{threadId * updatesPerThread + i}"] = i * 1.5
                            };
                            _lookupService.IncrementalUpdate(updateData);
                        }
                        
                        stopwatch.Stop();
                        return stopwatch.Elapsed.TotalMilliseconds;
                    }));
                }

                var threadTimes = Task.WhenAll(tasks).Result;
                var totalTime = threadTimes.Max();

                // Assert
                RecordResult($"ConcurrentUpdate_{threadCount}", totalTime, 0, size);
                
                // 并发更新时间应该合理
                var expectedMaxTime = threadCount * updatesPerThread * 2; // 每项更新不超过2ms
                Assert.IsTrue(totalTime < expectedMaxTime,
                    $"并发更新总时间 {totalTime:F2}ms 超过预期 {expectedMaxTime}ms ({threadCount}线程)");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 生成复杂测试数据
        /// </summary>
        private LookupTableData GenerateComplexTestData(int size)
        {
            var data = new LookupTableData
            {
                TableName = $"ComplexTestTable_{size}",
                Data = new Dictionary<string, object>()
            };

            for (int i = 0; i < size; i++)
            {
                var row = new Dictionary<string, object>
                {
                    ["ID"] = i,
                    ["Name"] = $"ComplexItem_{i}_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    ["Value"] = i * Math.PI,
                    ["Description"] = $"Complex description for item {i} with additional metadata",
                    ["Category"] = $"Category_{i % 20}",
                    ["Status"] = i % 3 == 0 ? "Active" : i % 3 == 1 ? "Inactive" : "Pending",
                    ["Timestamp"] = DateTime.Now.AddDays(-i),
                    ["Priority"] = (i % 7) + 1,
                    ["Score"] = (i * 1.23456789) % 100,
                    ["Tags"] = $"tag{i % 10},tag{i % 20},special",
                    ["Metadata"] = new { Key1 = $"value1_{i}", Key2 = i * 2, Key3 = true }
                };

                foreach (var kvp in row)
                {
                    data.Data[$"{kvp.Key}_{i}"] = kvp.Value;
                }
            }

            return data;
        }

        /// <summary>
        /// 获取响应时间阈值
        /// </summary>
        private double GetThreshold(int dataSize)
        {
            if (dataSize < 1000)
                return _benchmarkConfig.Thresholds.SmallTableResponseTime;
            else if (dataSize < 10000)
                return _benchmarkConfig.Thresholds.MediumTableResponseTime;
            else if (dataSize < 100000)
                return _benchmarkConfig.Thresholds.LargeTableResponseTime;
            else
                return _benchmarkConfig.Thresholds.ExtraLargeTableResponseTime;
        }

        /// <summary>
        /// 获取加载时间阈值
        /// </summary>
        private double GetLoadThreshold(int dataSize)
        {
            // 加载时间可以比查询时间长一些
            return GetThreshold(dataSize) * 2;
        }

        /// <summary>
        /// 计算百分位数
        /// </summary>
        private double GetPercentile(List<double> values, double percentile)
        {
            var sortedValues = values.OrderBy(x => x).ToList();
            var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
            return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
        }

        /// <summary>
        /// 模拟UI渲染
        /// </summary>
        private void RenderMockUI(LookupTableData data)
        {
            // 模拟UI渲染操作
            var rows = data.Data.Keys.Count / 7; // 假设每行7个字段
            
            // 模拟表格渲染
            for (int i = 0; i < Math.Min(rows, 1000); i++)
            {
                var rowData = new Dictionary<string, object>();
                for (int j = 0; j < 7; j++)
                {
                    var key = $"Field{j}_{i}";
                    rowData[key] = data.Data.ContainsKey(key) ? data.Data[key] : null;
                }
                
                // 模拟控件创建
                var controls = new List<object>();
                foreach (var kvp in rowData)
                {
                    controls.Add(new { Name = kvp.Key, Value = kvp.Value });
                }
            }
        }

        #endregion
    }
}