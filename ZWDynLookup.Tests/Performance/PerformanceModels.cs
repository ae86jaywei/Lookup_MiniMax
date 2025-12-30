using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZWDynLookup.Tests.Performance
{
    /// <summary>
    /// 性能测试配置
    /// </summary>
    public class BenchmarkConfig
    {
        [JsonPropertyName("TestSizes")]
        public Dictionary<string, int> TestSizes { get; set; }

        [JsonPropertyName("Thresholds")]
        public PerformanceThresholds Thresholds { get; set; }

        [JsonPropertyName("Iterations")]
        public int Iterations { get; set; }

        [JsonPropertyName("WarmupIterations")]
        public int WarmupIterations { get; set; }
    }

    /// <summary>
    /// 性能阈值配置
    /// </summary>
    public class PerformanceThresholds
    {
        [JsonPropertyName("SmallTableResponseTime")]
        public double SmallTableResponseTime { get; set; }

        [JsonPropertyName("MediumTableResponseTime")]
        public double MediumTableResponseTime { get; set; }

        [JsonPropertyName("LargeTableResponseTime")]
        public double LargeTableResponseTime { get; set; }

        [JsonPropertyName("ExtraLargeTableResponseTime")]
        public double ExtraLargeTableResponseTime { get; set; }

        [JsonPropertyName("MaxMemoryUsage")]
        public long MaxMemoryUsage { get; set; }

        [JsonPropertyName("MaxConcurrentUsers")]
        public int MaxConcurrentUsers { get; set; }
    }

    /// <summary>
    /// 性能测试结果
    /// </summary>
    public class PerformanceTestResults
    {
        [JsonPropertyName("StartTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("EndTime")]
        public DateTime? EndTime { get; set; }

        [JsonPropertyName("TotalTests")]
        public int TotalTests { get; set; }

        [JsonPropertyName("PassedTests")]
        public int PassedTests { get; set; }

        [JsonPropertyName("FailedTests")]
        public int FailedTests { get; set; }

        [JsonPropertyName("Results")]
        public List<TestResult> Results { get; set; } = new List<TestResult>();

        [JsonPropertyName("Configuration")]
        public BenchmarkConfig Configuration { get; set; }

        [JsonPropertyName("Summary")]
        public TestSummary Summary { get; set; } = new TestSummary();

        public double GetPassRate()
        {
            return TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;
        }

        public double GetAverageExecutionTime()
        {
            return Results.Count > 0 ? Results.Average(r => r.ExecutionTime) : 0;
        }

        public double GetMaxExecutionTime()
        {
            return Results.Count > 0 ? Results.Max(r => r.ExecutionTime) : 0;
        }

        public double GetMinExecutionTime()
        {
            return Results.Count > 0 ? Results.Min(r => r.ExecutionTime) : 0;
        }

        public long GetTotalMemoryUsed()
        {
            return Results.Sum(r => r.MemoryUsed);
        }

        public long GetAverageMemoryUsed()
        {
            return Results.Count > 0 ? Results.Average(r => r.MemoryUsed) : 0;
        }
    }

    /// <summary>
    /// 单个测试结果
    /// </summary>
    public class TestResult
    {
        [JsonPropertyName("TestName")]
        public string TestName { get; set; }

        [JsonPropertyName("ExecutionTime")]
        public double ExecutionTime { get; set; }

        [JsonPropertyName("MemoryUsed")]
        public long MemoryUsed { get; set; }

        [JsonPropertyName("DataSize")]
        public int DataSize { get; set; }

        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("Passed")]
        public bool Passed { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("Metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        [JsonPropertyName("PerformanceRating")]
        public string PerformanceRating { get; set; }

        public string GetPerformanceRating()
        {
            if (ExecutionTime < 50) return "优秀";
            if (ExecutionTime < 100) return "良好";
            if (ExecutionTime < 500) return "一般";
            return "需要优化";
        }
    }

    /// <summary>
    /// 测试摘要
    /// </summary>
    public class TestSummary
    {
        [JsonPropertyName("TotalExecutionTime")]
        public double TotalExecutionTime { get; set; }

        [JsonPropertyName("PeakMemoryUsage")]
        public long PeakMemoryUsage { get; set; }

        [JsonPropertyName("AverageResponseTime")]
        public double AverageResponseTime { get; set; }

        [JsonPropertyName("PerformanceScore")]
        public double PerformanceScore { get; set; }

        [JsonPropertyName("OverallRating")]
        public string OverallRating { get; set; }

        [JsonPropertyName("Bottlenecks")]
        public List<string> Bottlenecks { get; set; } = new List<string>();

        [JsonPropertyName("Recommendations")]
        public List<string> Recommendations { get; set; } = new List<string>();

        [JsonPropertyName("TestEnvironment")]
        public TestEnvironment Environment { get; set; } = new TestEnvironment();
    }

    /// <summary>
    /// 测试环境信息
    /// </summary>
    public class TestEnvironment
    {
        [JsonPropertyName("OperatingSystem")]
        public string OperatingSystem { get; set; }

        [JsonPropertyName("Processor")]
        public string Processor { get; set; }

        [JsonPropertyName("TotalMemory")]
        public long TotalMemory { get; set; }

        [JsonPropertyName("DotNetVersion")]
        public string DotNetVersion { get; set; }

        [JsonPropertyName("ProcessorCount")]
        public int ProcessorCount { get; set; }

        [JsonPropertyName("Is64BitProcess")]
        public bool Is64BitProcess { get; set; }

        [JsonPropertyName("MachineName")]
        public string MachineName { get; set; }

        [JsonPropertyName("UserName")]
        public string UserName { get; set; }
    }

    /// <summary>
    /// 性能测试指标
    /// </summary>
    public class PerformanceMetrics
    {
        [JsonPropertyName("Latency")]
        public LatencyMetrics Latency { get; set; } = new LatencyMetrics();

        [JsonPropertyName("Throughput")]
        public ThroughputMetrics Throughput { get; set; } = new ThroughputMetrics();

        [JsonPropertyName("ResourceUsage")]
        public ResourceUsageMetrics ResourceUsage { get; set; } = new ResourceUsageMetrics();

        [JsonPropertyName("Scalability")]
        public ScalabilityMetrics Scalability { get; set; } = new ScalabilityMetrics();
    }

    /// <summary>
    /// 延迟指标
    /// </summary>
    public class LatencyMetrics
    {
        [JsonPropertyName("AverageLatency")]
        public double AverageLatency { get; set; }

        [JsonPropertyName("MedianLatency")]
        public double MedianLatency { get; set; }

        [JsonPropertyName("P95Latency")]
        public double P95Latency { get; set; }

        [JsonPropertyName("P99Latency")]
        public double P99Latency { get; set; }

        [JsonPropertyName("MinLatency")]
        public double MinLatency { get; set; }

        [JsonPropertyName("MaxLatency")]
        public double MaxLatency { get; set; }

        [JsonPropertyName("LatencyDistribution")]
        public Dictionary<string, double> LatencyDistribution { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// 吞吐量指标
    /// </summary>
    public class ThroughputMetrics
    {
        [JsonPropertyName("OperationsPerSecond")]
        public double OperationsPerSecond { get; set; }

        [JsonPropertyName("RequestsPerSecond")]
        public double RequestsPerSecond { get; set; }

        [JsonPropertyName("DataProcessedPerSecond")]
        public double DataProcessedPerSecond { get; set; }

        [JsonPropertyName("ConcurrentUsers")]
        public int ConcurrentUsers { get; set; }

        [JsonPropertyName("PeakThroughput")]
        public double PeakThroughput { get; set; }

        [JsonPropertyName("AverageThroughput")]
        public double AverageThroughput { get; set; }
    }

    /// <summary>
    /// 资源使用指标
    /// </summary>
    public class ResourceUsageMetrics
    {
        [JsonPropertyName("CPUUsage")]
        public double CPUUsage { get; set; }

        [JsonPropertyName("MemoryUsage")]
        public long MemoryUsage { get; set; }

        [JsonPropertyName("PeakMemoryUsage")]
        public long PeakMemoryUsage { get; set; }

        [JsonPropertyName("GCCollections")]
        public int GCCollections { get; set; }

        [JsonPropertyName("ThreadCount")]
        public int ThreadCount { get; set; }

        [JsonPropertyName("HandleCount")]
        public int HandleCount { get; set; }
    }

    /// <summary>
    /// 可扩展性指标
    /// </summary>
    public class ScalabilityMetrics
    {
        [JsonPropertyName("LinearScalability")]
        public bool LinearScalability { get; set; }

        [JsonPropertyName("ScalabilityFactor")]
        public double ScalabilityFactor { get; set; }

        [JsonPropertyName("BottleneckIdentified")]
        public string BottleneckIdentified { get; set; }

        [JsonPropertyName("ScalabilityTestResults")]
        public Dictionary<int, double> ScalabilityTestResults { get; set; } = new Dictionary<int, double>();
    }

    /// <summary>
    /// 性能测试报告
    /// </summary>
    public class PerformanceReport
    {
        [JsonPropertyName("ReportMetadata")]
        public ReportMetadata Metadata { get; set; } = new ReportMetadata();

        [JsonPropertyName("ExecutiveSummary")]
        public ExecutiveSummary Summary { get; set; } = new ExecutiveSummary();

        [JsonPropertyName("TestResults")]
        public PerformanceTestResults Results { get; set; } = new PerformanceTestResults();

        [JsonPropertyName("PerformanceMetrics")]
        public PerformanceMetrics Metrics { get; set; } = new PerformanceMetrics();

        [JsonPropertyName("Analysis")]
        public PerformanceAnalysis Analysis { get; set; } = new PerformanceAnalysis();

        [JsonPropertyName("Recommendations")]
        public List<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }

    /// <summary>
    /// 报告元数据
    /// </summary>
    public class ReportMetadata
    {
        [JsonPropertyName("ReportId")]
        public string ReportId { get; set; }

        [JsonPropertyName("GeneratedAt")]
        public DateTime GeneratedAt { get; set; }

        [JsonPropertyName("Version")]
        public string Version { get; set; }

        [JsonPropertyName("Format")]
        public string Format { get; set; }

        [JsonPropertyName("GeneratedBy")]
        public string GeneratedBy { get; set; }
    }

    /// <summary>
    /// 执行摘要
    /// </summary>
    public class ExecutiveSummary
    {
        [JsonPropertyName("OverallScore")]
        public double OverallScore { get; set; }

        [JsonPropertyName("PerformanceGrade")]
        public string PerformanceGrade { get; set; }

        [JsonPropertyName("KeyFindings")]
        public List<string> KeyFindings { get; set; } = new List<string>();

        [JsonPropertyName("CriticalIssues")]
        public List<string> CriticalIssues { get; set; } = new List<string>();

        [JsonPropertyName("SuccessCriteria")]
        public Dictionary<string, bool> SuccessCriteria { get; set; } = new Dictionary<string, bool>();
    }

    /// <summary>
    /// 性能分析
    /// </summary>
    public class PerformanceAnalysis
    {
        [JsonPropertyName("PerformanceTrends")]
        public Dictionary<string, List<double>> Trends { get; set; } = new Dictionary<string, List<double>>();

        [JsonPropertyName("Anomalies")]
        public List<string> Anomalies { get; set; } = new List<string>();

        [JsonPropertyName("OptimizationOpportunities")]
        public List<string> OptimizationOpportunities { get; set; } = new List<string>();

        [JsonPropertyName("RiskAssessment")]
        public RiskAssessment RiskAssessment { get; set; } = new RiskAssessment();
    }

    /// <summary>
    /// 风险评估
    /// </summary>
    public class RiskAssessment
    {
        [JsonPropertyName("HighRiskAreas")]
        public List<string> HighRiskAreas { get; set; } = new List<string>();

        [JsonPropertyName("MediumRiskAreas")]
        public List<string> MediumRiskAreas { get; set; } = new List<string>();

        [JsonPropertyName("LowRiskAreas")]
        public List<string> LowRiskAreas { get; set; } = new List<string>();

        [JsonPropertyName("MitigationStrategies")]
        public Dictionary<string, string> MitigationStrategies { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// 优化建议
    /// </summary>
    public class Recommendation
    {
        [JsonPropertyName("Priority")]
        public string Priority { get; set; }

        [JsonPropertyName("Category")]
        public string Category { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("Impact")]
        public string Impact { get; set; }

        [JsonPropertyName("Effort")]
        public string Effort { get; set; }

        [JsonPropertyName("Implementation")]
        public string Implementation { get; set; }
    }
}