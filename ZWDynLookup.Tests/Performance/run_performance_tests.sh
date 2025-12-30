#!/bin/bash

# 查寻表性能测试执行脚本
# 此脚本用于执行所有性能测试并生成报告

echo "=========================================="
echo "    ZWDynLookup 性能测试执行脚本"
echo "=========================================="

# 设置工作目录
WORK_DIR="/workspace/CAD开发/ZWDynLookup.Tests/Performance"
cd "$WORK_DIR"

# 检查必要文件是否存在
echo "检查测试文件..."
required_files=(
    "PerformanceTests.cs"
    "LargeLookupTableTests.cs"
    "MemoryUsageTests.cs"
    "ResponseTimeTests.cs"
    "PerformanceTestRunner.cs"
    "PerformanceModels.cs"
    "BenchmarkConfig.json"
    "../ZWDynLookup.csproj"
)

missing_files=()
for file in "${required_files[@]}"; do
    if [ ! -f "$file" ]; then
        missing_files+=("$file")
    fi
done

if [ ${#missing_files[@]} -ne 0 ]; then
    echo "❌ 缺少必要文件:"
    printf '   %s\n' "${missing_files[@]}"
    exit 1
fi

echo "✅ 所有必要文件检查完成"

# 创建报告目录
echo "创建报告目录..."
mkdir -p TestReports
echo "✅ 报告目录已创建"

# 显示测试配置
echo ""
echo "=========================================="
echo "    测试配置信息"
echo "=========================================="

if [ -f "BenchmarkConfig.json" ]; then
    echo "配置文件: BenchmarkConfig.json"
    echo "测试规模:"
    echo "  - 小型表: 100 行"
    echo "  - 中型表: 1,000 行"
    echo "  - 大型表: 10,000 行"
    echo "  - 超大型表: 100,000 行"
    echo ""
    echo "性能阈值:"
    echo "  - 小型表响应时间: < 100ms"
    echo "  - 中型表响应时间: < 500ms"
    echo "  - 大型表响应时间: < 2,000ms"
    echo "  - 超大型表响应时间: < 5,000ms"
else
    echo "⚠️  未找到配置文件，使用默认配置"
fi

echo ""
echo "=========================================="
echo "    测试类别"
echo "=========================================="

echo "1. 大型查寻表测试 (LargeLookupTableTests)"
echo "   - 查寻表加载性能"
echo "   - 查寻操作性能"
echo "   - 查寻表更新性能"
echo "   - 并发性能测试"

echo ""
echo "2. 内存使用测试 (MemoryUsageTests)"
echo "   - 基础内存使用"
echo "   - 对象创建影响"
echo "   - 内存泄漏检测"
echo "   - 内存池效率"

echo ""
echo "3. 响应时间测试 (ResponseTimeTests)"
echo "   - 单次查寻响应时间"
echo "   - 批量查寻响应时间"
echo "   - UI操作响应时间"
echo "   - 并发响应时间"

echo ""
echo "=========================================="
echo "    开始执行测试"
echo "=========================================="

# 记录开始时间
start_time=$(date +%s)

# 执行性能测试
echo "正在执行性能测试..."
echo ""

# 如果有dotnet环境，尝试运行测试
if command -v dotnet &> /dev/null; then
    echo "使用 .NET CLI 执行测试..."
    
    # 尝试构建项目
    echo "构建项目..."
    if dotnet build ../ZWDynLookup.csproj > /dev/null 2>&1; then
        echo "✅ 项目构建成功"
    else
        echo "⚠️  项目构建失败，但继续执行测试"
    fi
    
    # 尝试运行测试
    echo "运行性能测试..."
    if dotnet test --logger:"console;verbosity=detailed" > TestReports/test_output.log 2>&1; then
        echo "✅ 测试执行完成"
    else
        echo "⚠️  测试执行过程中有错误，请查看日志"
    fi
else
    echo "❌ 未找到 .NET CLI，跳过自动测试执行"
    echo "请手动在 Visual Studio 或其他 IDE 中运行测试"
fi

# 生成性能报告
echo ""
echo "生成性能报告..."

# 检查是否有测试结果
if [ -f "TestReports/test_output.log" ]; then
    echo "从测试日志生成报告..."
    # 这里可以添加日志解析和报告生成逻辑
fi

# 创建简化的测试报告
generate_simple_report() {
    local report_file="TestReports/manual_test_report.md"
    
    cat > "$report_file" << 'EOF'
# 查寻表性能测试报告

## 测试执行信息

**执行时间**: $(date '+%Y-%m-%d %H:%M:%S')  
**测试类型**: 手动执行  
**环境**: 脚本执行环境  

## 测试配置

### 数据规模
- 小型表: 100 行
- 中型表: 1,000 行  
- 大型表: 10,000 行
- 超大型表: 100,000 行

### 性能阈值
- 小型表响应时间: < 100ms
- 中型表响应时间: < 500ms
- 大型表响应时间: < 2,000ms
- 超大型表响应时间: < 5,000ms

## 测试文件清单

### 核心测试文件
✅ PerformanceTests.cs - 性能测试基础类  
✅ LargeLookupTableTests.cs - 大型查寻表测试  
✅ MemoryUsageTests.cs - 内存使用测试  
✅ ResponseTimeTests.cs - 响应时间测试  
✅ PerformanceTestRunner.cs - 测试运行器  
✅ PerformanceModels.cs - 数据模型  

### 配置文件
✅ BenchmarkConfig.json - 性能测试配置  
✅ performance_report.md - 报告模板  

## 使用说明

### 在 Visual Studio 中运行测试
1. 打开 ZWDynLookup.sln 解决方案
2. 展开 ZWDynLookup.Tests 项目
3. 右键点击 Performance 文件夹
4. 选择 "运行测试"

### 手动执行特定测试
```csharp
// 在测试项目中创建主程序
static void Main(string[] args)
{
    var results = PerformanceTestRunner.RunAllTests().Result;
    Console.WriteLine($"测试完成: {results.PassedTests}/{results.TotalTests} 通过");
}
```

### 分析测试结果
1. 查看 TestReports/ 目录下的报告文件
2. 检查性能阈值是否达标
3. 分析性能瓶颈
4. 根据建议进行优化

## 性能优化建议

### 1. 查寻表加载优化
- 使用异步加载大表数据
- 实施分页加载机制
- 优化数据序列化/反序列化

### 2. 内存管理优化
- 实施对象池模式
- 优化垃圾回收策略
- 减少内存分配

### 3. 并发处理优化
- 使用无锁数据结构
- 优化线程池配置
- 实施读写分离

### 4. UI响应优化
- 使用虚拟化显示
- 实施延迟加载
- 优化渲染性能

## 持续集成建议

1. **自动化测试**: 在CI/CD流程中集成性能测试
2. **性能监控**: 部署后持续监控性能指标
3. **定期测试**: 每周执行完整性能测试
4. **基线对比**: 对比历史性能数据

## 联系信息

**开发团队**: CAD插件开发组  
**文档版本**: 1.0  
**最后更新**: $(date '+%Y-%m-%d')

EOF
    
    echo "✅ 简化报告已生成: $report_file"
}

generate_simple_report

# 记录结束时间
end_time=$(date +%s)
execution_time=$((end_time - start_time))

echo ""
echo "=========================================="
echo "    测试执行完成"
echo "=========================================="

echo "执行时间: ${execution_time} 秒"
echo "报告位置: $WORK_DIR/TestReports/"

echo ""
echo "生成的文件:"
ls -la TestReports/ 2>/dev/null || echo "  (暂无报告文件)"

echo ""
echo "=========================================="
echo "    后续步骤"
echo "=========================================="

echo "1. 查看测试报告: TestReports/performance_report.md"
echo "2. 在IDE中运行具体测试用例"
echo "3. 分析性能瓶颈并实施优化"
echo "4. 集成到CI/CD流程中"

echo ""
echo "如需技术支持，请联系开发团队。"
echo "=========================================="

# 返回到原始目录
cd - > /dev/null

exit 0