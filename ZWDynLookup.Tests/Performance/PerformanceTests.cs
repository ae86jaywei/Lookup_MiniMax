using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWDynLookup.Models;
using ZWDynLookup.Service;
using System.Threading;

namespace ZWDynLookup.Tests.Performance
{
    /// <summary>
    /// 性能测试主文件
    /// 包含查寻表系统的综合性能测试
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        private static BenchmarkConfig _benchmarkConfig;
        private static PerformanceTestResults _testResults;
        private static readonly string ConfigPath = "/workspace/CAD开发/ZWDynLookup.Tests/Performance/BenchmarkConfig.json";
        
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // 加载性能测试配置
            LoadBenchmarkConfig();
            _testResults = new PerformanceTestResults();
        }

        /// <summary>
        /// 加载性能测试配置文件
        /// </summary>
        private static void LoadBenchmarkConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string configJson = File.ReadAllText(ConfigPath);
                    _benchmarkConfig = JsonSerializer.Deserialize<BenchmarkConfig>(configJson);
                }
                else
                {
                    // 创建默认配置
                    _benchmarkConfig = new BenchmarkConfig
                    {
                        TestSizes = new Dictionary<string, int>
                        {
                            { "Small", 100 },
                            { "Medium", 1000 },
                            { "Large", 10000 },
                            { "ExtraLarge", 100000 }
                        },
                        Thresholds = new PerformanceThresholds
                        {
                            SmallTableResponseTime = 100,
                            MediumTableResponseTime = 500,
                            LargeTableResponseTime = 2000,
                            ExtraLargeTableResponseTime = 5000,
                            MaxMemoryUsage = 1024 * 1024 * 1024, // 1GB
                            MaxConcurrentUsers = 100
                        },
                        Iterations = 10,
                        WarmupIterations = 3
                    };
                    SaveBenchmarkConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}");
                _benchmarkConfig = CreateDefaultConfig();
            }
        }

        /// <summary>
        /// 保存性能测试配置
        /// </summary>
        private static void SaveBenchmarkConfig()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string configJson = JsonSerializer.Serialize(_benchmarkConfig, options);
                File.WriteAllText(ConfigPath, configJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        private static BenchmarkConfig CreateDefaultConfig()
        {
            return new BenchmarkConfig
            {
                TestSizes = new Dictionary<string, int>
                {
                    { "Small", 100 },
                    { "Medium", 1000 },
                    { "Large", 10000 },
                    { "ExtraLarge", 100000 }
                },
                Thresholds = new PerformanceThresholds
                {
                    SmallTableResponseTime = 100,
                    MediumTableResponseTime = 500,
                    LargeTableResponseTime = 2000,
                    ExtraLargeTableResponseTime = 5000,
                    MaxMemoryUsage = 1024 * 1024 * 1024,
                    MaxConcurrentUsers = 100
                },
                Iterations = 10,
                WarmupIterations = 3
            };
        }

        #region 数据生成方法

        /// <summary>
        /// 生成测试用的查寻表数据
        /// </summary>
        /// <param name="size">数据大小</param>
        /// <returns>查寻表数据</returns>
        protected LookupTableData GenerateTestData(int size)
        {
            var data = new LookupTableData
            {
                TableName = $"PerformanceTestTable_{size}",
                Data = new Dictionary<string, object>()
            };

            for (int i = 0; i < size; i++)
            {
                var row = new Dictionary<string, object>
                {
                    ["ID"] = i,
                    ["Name"] = $"Item_{i}",
                    ["Value"] = i * 1.5,
                    ["Description"] = $"Description for item {i}",
                    ["Category"] = $"Category_{i % 10}",
                    ["Status"] = i % 2 == 0 ? "Active" : "Inactive",
                    ["Timestamp"] = DateTime.Now.AddDays(-i),
                    ["Priority"] = i % 5 + 1
                };

                foreach (var kvp in row)
                {
                    data.Data[$"{kvp.Key}_{i}"] = kvp.Value;
                }
            }

            return data;
        }

        /// <summary>
        /// 记录测试结果
        /// </summary>
        /// <param name="testName">测试名称</param>
        /// <param name="executionTime">执行时间</param>
        /// <param name="memoryUsed">使用的内存</param>
        /// <param name="dataSize">数据大小</param>
        protected void RecordResult(string testName, double executionTime, long memoryUsed, int dataSize)
        {
            var result = new TestResult
            {
                TestName = testName,
                ExecutionTime = executionTime,
                MemoryUsed = memoryUsed,
                DataSize = dataSize,
                Timestamp = DateTime.Now,
                Passed = IsPerformanceAcceptable(testName, executionTime, dataSize)
            };

            _testResults.Results.Add(result);
        }

        /// <summary>
        /// 检查性能是否可接受
        /// </summary>
        /// <param name="testName">测试名称</param>
        /// <param name="executionTime">执行时间</param>
        /// <param name="dataSize">数据大小</param>
        /// <returns>是否通过性能测试</returns>
        private bool IsPerformanceAcceptable(string testName, double executionTime, int dataSize)
        {
            if (dataSize < 1000)
                return executionTime <= _benchmarkConfig.Thresholds.SmallTableResponseTime;
            else if (dataSize < 10000)
                return executionTime <= _benchmarkConfig.Thresholds.MediumTableResponseTime;
            else if (dataSize < 100000)
                return executionTime <= _benchmarkConfig.Thresholds.LargeTableResponseTime;
            else
                return executionTime <= _benchmarkConfig.Thresholds.ExtraLargeTableResponseTime;
        }

        /// <summary>
        /// 测量方法执行时间
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <returns>执行时间（毫秒）</returns>
        protected double MeasureExecutionTime(Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// 测量方法执行时间和内存使用
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <returns>执行时间和内存使用的元组</returns>
        protected (double executionTime, long memoryUsed) MeasureExecutionTimeAndMemory(Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();
            
            action();
            
            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            return (stopwatch.Elapsed.TotalMilliseconds, memoryAfter - memoryBefore);
        }

        /// <summary>
        /// 清理测试资源
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 生成性能报告
        /// </summary>
        [ClassCleanup]
        public static void GeneratePerformanceReport()
        {
            try
            {
                var reportPath = "/workspace/CAD开发/ZWDynLookup.Tests/Performance/TestReports/performance_report.md";
                var report = GenerateReportContent();
                File.WriteAllText(reportPath, report);
                Console.WriteLine($"性能报告已生成: {reportPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成报告失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成报告内容
        /// </summary>
        /// <returns>报告内容</returns>
        private static string GenerateReportContent()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("# 查寻表性能测试报告");
            report.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            report.AppendLine("## 测试概要");
            report.AppendLine($"总测试数: {_testResults.Results.Count}");
            report.AppendLine($"通过测试数: {_testResults.Results.Count(r => r.Passed)}");
            report.AppendLine($"失败测试数: {_testResults.Results.Count(r => !r.Passed)}");
            report.AppendLine($"通过率: {_testResults.Results.Count(r => r.Passed) * 100.0 / _testResults.Results.Count:F1}%");
            report.AppendLine();

            report.AppendLine("## 详细测试结果");
            report.AppendLine("| 测试名称 | 数据大小 | 执行时间(ms) | 内存使用(bytes) | 状态 | 时间戳 |");
            report.AppendLine("|----------|----------|--------------|-----------------|------|--------|");

            foreach (var result in _testResults.Results)
            {
                var status = result.Passed ? "✅ 通过" : "❌ 失败";
                report.AppendLine($"| {result.TestName} | {result.DataSize} | {result.ExecutionTime:F2} | {result.MemoryUsed} | {status} | {result.Timestamp:HH:mm:ss} |");
            }

            report.AppendLine();
            report.AppendLine("## 性能分析");
            
            var bySize = _testResults.Results.GroupBy(r => r.DataSize)
                .Select(g => new { Size = g.Key, AvgTime = g.Average(r => r.ExecutionTime), AvgMemory = g.Average(r => r.MemoryUsed) });
            
            foreach (var group in bySize)
            {
                report.AppendLine($"### 数据大小: {group.Size} 行");
                report.AppendLine($"- 平均响应时间: {group.AvgTime:F2} ms");
                report.AppendLine($"- 平均内存使用: {group.AvgMemory:F0} bytes");
                report.AppendLine();
            }

            return report.ToString();
        }
    }
}