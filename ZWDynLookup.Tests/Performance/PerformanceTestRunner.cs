using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWDynLookup.Tests.Performance;

namespace ZWDynLookup.Tests.Performance
{
    /// <summary>
    /// 性能测试运行器
    /// 协调和执行所有性能测试
    /// </summary>
    public class PerformanceTestRunner
    {
        private static BenchmarkConfig _config;
        private static PerformanceTestResults _results;
        private static readonly string ReportDirectory = "/workspace/CAD开发/ZWDynLookup.Tests/Performance/TestReports/";

        /// <summary>
        /// 运行所有性能测试
        /// </summary>
        public static async Task<PerformanceTestResults> RunAllTests()
        {
            Console.WriteLine("=== 查寻表性能测试开始 ===");
            Console.WriteLine($"开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            LoadConfiguration();
            InitializeResults();
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // 运行各类测试
                await RunLargeLookupTableTests();
                await RunMemoryUsageTests();
                await RunResponseTimeTests();
                await RunConcurrencyTests();
                
                stopwatch.Stop();
                
                // 生成报告
                await GenerateReports(stopwatch.Elapsed);
                
                Console.WriteLine($"=== 性能测试完成 ===");
                Console.WriteLine($"总耗时: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
                Console.WriteLine($"通过测试: {_results.PassedTests}/{_results.TotalTests}");
                Console.WriteLine($"失败测试: {_results.FailedTests}/{_results.TotalTests}");
                
                return _results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试执行失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 运行大型查寻表测试
        /// </summary>
        private static async Task RunLargeLookupTableTests()
        {
            Console.WriteLine("\n--- 大型查寻表性能测试 ---");
            
            var testInstance = new LargeLookupTableTests();
            var testMethods = typeof(LargeLookupTableTests)
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
                .ToList();

            foreach (var method in testMethods)
            {
                try
                {
                    Console.WriteLine($"执行测试: {method.Name}");
                    
                    // 执行测试初始化
                    testInstance.TestInitialize();
                    
                    // 测量执行时间
                    var testStopwatch = Stopwatch.StartNew();
                    method.Invoke(testInstance, null);
                    testStopwatch.Stop();
                    
                    // 记录结果
                    _results.TotalTests++;
                    _results.PassedTests++;
                    
                    Console.WriteLine($"  ✓ 完成 ({testStopwatch.Elapsed.TotalSeconds:F2}s)");
                }
                catch (Exception ex)
                {
                    _results.TotalTests++;
                    _results.FailedTests++;
                    Console.WriteLine($"  ✗ 失败: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }

        /// <summary>
        /// 运行内存使用测试
        /// </summary>
        private static async Task RunMemoryUsageTests()
        {
            Console.WriteLine("\n--- 内存使用性能测试 ---");
            
            var testInstance = new MemoryUsageTests();
            var testMethods = typeof(MemoryUsageTests)
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
                .ToList();

            foreach (var method in testMethods)
            {
                try
                {
                    Console.WriteLine($"执行测试: {method.Name}");
                    
                    // 执行测试初始化
                    testInstance.TestInitialize();
                    
                    // 测量执行时间
                    var testStopwatch = Stopwatch.StartNew();
                    method.Invoke(testInstance, null);
                    testStopwatch.Stop();
                    
                    // 记录结果
                    _results.TotalTests++;
                    _results.PassedTests++;
                    
                    Console.WriteLine($"  ✓ 完成 ({testStopwatch.Elapsed.TotalSeconds:F2}s)");
                }
                catch (Exception ex)
                {
                    _results.TotalTests++;
                    _results.FailedTests++;
                    Console.WriteLine($"  ✗ 失败: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }

        /// <summary>
        /// 运行响应时间测试
        /// </summary>
        private static async Task RunResponseTimeTests()
        {
            Console.WriteLine("\n--- 响应时间性能测试 ---");
            
            var testInstance = new ResponseTimeTests();
            var testMethods = typeof(ResponseTimeTests)
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
                .ToList();

            foreach (var method in testMethods)
            {
                try
                {
                    Console.WriteLine($"执行测试: {method.Name}");
                    
                    // 执行测试初始化
                    testInstance.TestInitialize();
                    
                    // 测量执行时间
                    var testStopwatch = Stopwatch.StartNew();
                    method.Invoke(testInstance, null);
                    testStopwatch.Stop();
                    
                    // 记录结果
                    _results.TotalTests++;
                    _results.PassedTests++;
                    
                    Console.WriteLine($"  ✓ 完成 ({testStopwatch.Elapsed.TotalSeconds:F2}s)");
                }
                catch (Exception ex)
                {
                    _results.TotalTests++;
                    _results.FailedTests++;
                    Console.WriteLine($"  ✗ 失败: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }

        /// <summary>
        /// 运行并发测试
        /// </summary>
        private static async Task RunConcurrencyTests()
        {
            Console.WriteLine("\n--- 并发性能测试 ---");
            
            // 并发测试集成在大查寻表测试中
            var testInstance = new LargeLookupTableTests();
            testInstance.TestInitialize();
            
            try
            {
                var concurrentMethod = typeof(LargeLookupTableTests)
                    .GetMethod("TestConcurrentLookup_Performance");
                
                if (concurrentMethod != null)
                {
                    Console.WriteLine("执行测试: TestConcurrentLookup_Performance");
                    var testStopwatch = Stopwatch.StartNew();
                    concurrentMethod.Invoke(testInstance, null);
                    testStopwatch.Stop();
                    
                    _results.TotalTests++;
                    _results.PassedTests++;
                    Console.WriteLine($"  ✓ 完成 ({testStopwatch.Elapsed.TotalSeconds:F2}s)");
                }
            }
            catch (Exception ex)
            {
                _results.TotalTests++;
                _results.FailedTests++;
                Console.WriteLine($"  ✗ 失败: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// 加载测试配置
        /// </summary>
        private static void LoadConfiguration()
        {
            try
            {
                var configPath = "/workspace/CAD开发/ZWDynLookup.Tests/Performance/BenchmarkConfig.json";
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    _config = JsonSerializer.Deserialize<BenchmarkConfig>(configJson);
                    Console.WriteLine($"已加载配置文件: {configPath}");
                }
                else
                {
                    Console.WriteLine("配置文件不存在，使用默认配置");
                    _config = CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}，使用默认配置");
                _config = CreateDefaultConfig();
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

        /// <summary>
        /// 初始化测试结果
        /// </summary>
        private static void InitializeResults()
        {
            _results = new PerformanceTestResults
            {
                StartTime = DateTime.Now,
                Configuration = _config
            };
        }

        /// <summary>
        /// 生成测试报告
        /// </summary>
        private static async Task GenerateReports(TimeSpan totalExecutionTime)
        {
            Console.WriteLine("\n--- 生成测试报告 ---");
            
            EnsureReportDirectory();
            
            // 生成Markdown报告
            await GenerateMarkdownReport(totalExecutionTime);
            
            // 生成CSV报告
            await GenerateCsvReport();
            
            // 生成JSON报告
            await GenerateJsonReport();
            
            // 生成HTML报告
            await GenerateHtmlReport();
            
            Console.WriteLine($"报告已生成到: {ReportDirectory}");
        }

        /// <summary>
        /// 确保报告目录存在
        /// </summary>
        private static void EnsureReportDirectory()
        {
            if (!Directory.Exists(ReportDirectory))
            {
                Directory.CreateDirectory(ReportDirectory);
            }
        }

        /// <summary>
        /// 生成Markdown格式报告
        /// </summary>
        private static async Task GenerateMarkdownReport(TimeSpan totalExecutionTime)
        {
            var reportPath = Path.Combine(ReportDirectory, "performance_report.md");
            var report = GenerateMarkdownContent(totalExecutionTime);
            
            await File.WriteAllTextAsync(reportPath, report);
            Console.WriteLine($"Markdown报告: {reportPath}");
        }

        /// <summary>
        /// 生成CSV格式报告
        /// </summary>
        private static async Task GenerateCsvReport()
        {
            var reportPath = Path.Combine(ReportDirectory, "performance_results.csv");
            var csv = GenerateCsvContent();
            
            await File.WriteAllTextAsync(reportPath, csv);
            Console.WriteLine($"CSV报告: {reportPath}");
        }

        /// <summary>
        /// 生成JSON格式报告
        /// </summary>
        private static async Task GenerateJsonReport()
        {
            var reportPath = Path.Combine(ReportDirectory, "performance_results.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_results, options);
            
            await File.WriteAllTextAsync(reportPath, json);
            Console.WriteLine($"JSON报告: {reportPath}");
        }

        /// <summary>
        /// 生成HTML格式报告
        /// </summary>
        private static async Task GenerateHtmlReport()
        {
            var reportPath = Path.Combine(ReportDirectory, "performance_report.html");
            var html = GenerateHtmlContent();
            
            await File.WriteAllTextAsync(reportPath, html);
            Console.WriteLine($"HTML报告: {reportPath}");
        }

        /// <summary>
        /// 生成Markdown报告内容
        /// </summary>
        private static string GenerateMarkdownContent(TimeSpan totalExecutionTime)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("# 查寻表性能测试报告");
            report.AppendLine();
            report.AppendLine($"**生成时间**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**总执行时间**: {totalExecutionTime.TotalSeconds:F2} 秒");
            report.AppendLine();
            
            // 测试概要
            report.AppendLine("## 测试概要");
            report.AppendLine();
            report.AppendLine($"- **总测试数**: {_results.TotalTests}");
            report.AppendLine($"- **通过测试**: {_results.PassedTests}");
            report.AppendLine($"- **失败测试**: {_results.FailedTests}");
            report.AppendLine($"- **通过率**: {_results.PassedTests * 100.0 / _results.TotalTests:F1}%");
            report.AppendLine();
            
            // 性能阈值
            report.AppendLine("## 性能阈值");
            report.AppendLine();
            report.AppendLine("| 数据规模 | 响应时间阈值 |");
            report.AppendLine("|----------|--------------|");
            report.AppendLine($"| 小型(< 1K行) | {_config.Thresholds.SmallTableResponseTime} ms |");
            report.AppendLine($"| 中型(1K-10K行) | {_config.Thresholds.MediumTableResponseTime} ms |");
            report.AppendLine($"| 大型(10K-100K行) | {_config.Thresholds.LargeTableResponseTime} ms |");
            report.AppendLine($"| 超大型(> 100K行) | {_config.Thresholds.ExtraLargeTableResponseTime} ms |");
            report.AppendLine();
            
            // 详细结果
            report.AppendLine("## 详细测试结果");
            report.AppendLine();
            report.AppendLine("| 测试名称 | 数据大小 | 执行时间(ms) | 内存使用(bytes) | 状态 | 时间戳 |");
            report.AppendLine("|----------|----------|--------------|-----------------|------|--------|");
            
            foreach (var result in _results.Results)
            {
                var status = result.Passed ? "✅ 通过" : "❌ 失败";
                report.AppendLine($"| {result.TestName} | {result.DataSize:N0} | {result.ExecutionTime:F2} | {result.MemoryUsed:N0} | {status} | {result.Timestamp:HH:mm:ss} |");
            }
            
            report.AppendLine();
            
            // 性能分析
            report.AppendLine("## 性能分析");
            report.AppendLine();
            
            var bySize = _results.Results
                .GroupBy(r => r.DataSize)
                .Select(g => new { 
                    Size = g.Key, 
                    Count = g.Count(),
                    AvgTime = g.Average(r => r.ExecutionTime), 
                    AvgMemory = g.Average(r => r.MemoryUsed),
                    MaxTime = g.Max(r => r.ExecutionTime),
                    MinTime = g.Min(r => r.ExecutionTime)
                })
                .OrderBy(x => x.Size);
            
            foreach (var group in bySize)
            {
                report.AppendLine($"### 数据规模: {group.Size:N0} 行 ({group.Count} 个测试)");
                report.AppendLine();
                report.AppendLine($"- **平均响应时间**: {group.AvgTime:F2} ms");
                report.AppendLine($"- **最大响应时间**: {group.MaxTime:F2} ms");
                report.AppendLine($"- **最小响应时间**: {group.MinTime:F2} ms");
                report.AppendLine($"- **平均内存使用**: {group.AvgMemory:N0} bytes");
                report.AppendLine();
            }
            
            // 结论和建议
            report.AppendLine("## 结论和建议");
            report.AppendLine();
            
            var failedTests = _results.Results.Where(r => !r.Passed).ToList();
            if (failedTests.Any())
            {
                report.AppendLine("### 需要改进的测试");
                report.AppendLine();
                foreach (var failed in failedTests)
                {
                    report.AppendLine($"- **{failed.TestName}**: {failed.ExecutionTime:F2} ms (数据大小: {failed.DataSize:N0})");
                }
                report.AppendLine();
            }
            
            var passedRate = _results.PassedTests * 100.0 / _results.TotalTests;
            if (passedRate >= 90)
            {
                report.AppendLine("✅ **整体性能表现良好**");
            }
            else if (passedRate >= 70)
            {
                report.AppendLine("⚠️ **整体性能需要优化**");
            }
            else
            {
                report.AppendLine("❌ **性能存在严重问题，需要立即优化**");
            }
            
            return report.ToString();
        }

        /// <summary>
        /// 生成CSV报告内容
        /// </summary>
        private static string GenerateCsvContent()
        {
            var csv = new System.Text.StringBuilder();
            
            // CSV头部
            csv.AppendLine("TestName,DataSize,ExecutionTime,MemoryUsed,Passed,Timestamp");
            
            // 数据行
            foreach (var result in _results.Results)
            {
                csv.AppendLine($"{result.TestName},{result.DataSize},{result.ExecutionTime:F2},{result.MemoryUsed},{result.Passed},{result.Timestamp:yyyy-MM-dd HH:mm:ss}");
            }
            
            return csv.ToString();
        }

        /// <summary>
        /// 生成HTML报告内容
        /// </summary>
        private static string GenerateHtmlContent()
        {
            var html = new System.Text.StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"utf-8\">");
            html.AppendLine("    <title>查寻表性能测试报告</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        .header { background: #f5f5f5; padding: 20px; border-radius: 5px; }");
            html.AppendLine("        .summary { display: flex; gap: 20px; margin: 20px 0; }");
            html.AppendLine("        .summary-item { background: #e3f2fd; padding: 15px; border-radius: 5px; text-align: center; }");
            html.AppendLine("        .passed { color: #4caf50; }");
            html.AppendLine("        .failed { color: #f44336; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("        th { background-color: #f2f2f2; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // 标题
            html.AppendLine("    <div class=\"header\">");
            html.AppendLine("        <h1>查寻表性能测试报告</h1>");
            html.AppendLine($"        <p>生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine("    </div>");
            
            // 概要
            html.AppendLine("    <div class=\"summary\">");
            html.AppendLine($"        <div class=\"summary-item\"><h3>总测试</h3><p>{_results.TotalTests}</p></div>");
            html.AppendLine($"        <div class=\"summary-item passed\"><h3>通过</h3><p>{_results.PassedTests}</p></div>");
            html.AppendLine($"        <div class=\"summary-item failed\"><h3>失败</h3><p>{_results.FailedTests}</p></div>");
            html.AppendLine($"        <div class=\"summary-item\"><h3>通过率</h3><p>{_results.PassedTests * 100.0 / _results.TotalTests:F1}%</p></div>");
            html.AppendLine("    </div>");
            
            // 结果表格
            html.AppendLine("    <table>");
            html.AppendLine("        <tr><th>测试名称</th><th>数据大小</th><th>执行时间(ms)</th><th>内存使用(bytes)</th><th>状态</th></tr>");
            
            foreach (var result in _results.Results)
            {
                var status = result.Passed ? "✅ 通过" : "❌ 失败";
                var statusClass = result.Passed ? "passed" : "failed";
                html.AppendLine($"        <tr><td>{result.TestName}</td><td>{result.DataSize:N0}</td><td>{result.ExecutionTime:F2}</td><td>{result.MemoryUsed:N0}</td><td class=\"{statusClass}\">{status}</td></tr>");
            }
            
            html.AppendLine("    </table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
    }
}