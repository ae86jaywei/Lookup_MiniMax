#!/bin/bash

# ZWDynLookup 测试运行脚本 (Linux)
# 用途：自动化执行所有测试套件并生成报告

set -e  # 遇到错误立即退出

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 日志函数
log_info() {
    echo -e "${BLUE}[信息]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[成功]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[警告]${NC} $1"
}

log_error() {
    echo -e "${RED}[错误]${NC} $1"
}

# 显示横幅
echo "============================================"
echo "ZWDynLookup 自动化测试运行脚本 (Linux)"
echo "============================================"
echo

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# 设置环境变量
SOLUTION_DIR="$PROJECT_DIR"
TEST_PROJECT="$SOLUTION_DIR/ZWDynLookup.Tests/ZWDynLookup.Tests.csproj"
BUILD_CONFIGURATION="Release"
TEST_RESULTS_DIR="$SOLUTION_DIR/TestResults"
COVERAGE_DIR="$SOLUTION_DIR/CoverageResults"
LOG_DIR="$SOLUTION_DIR/Logs"
REPORTS_DIR="$SOLUTION_DIR/TestReports"

# 创建输出目录
mkdir -p "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"
mkdir -p "$LOG_DIR"
mkdir -p "$REPORTS_DIR"

# 设置时间戳
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/test-execution-$TIMESTAMP.log"

log_info "开始 ZWDynLookup 测试执行"
log_info "时间: $(date)"
log_info "日志文件: $LOG_FILE"
echo

# 检查 .NET 环境
log_info "检查 .NET 环境..."
if ! command -v dotnet &> /dev/null; then
    log_error "未找到 .NET SDK，请先安装 .NET 8.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
log_info ".NET 环境正常，版本: $DOTNET_VERSION"
echo

# 检查项目文件
log_info "检查项目文件..."
if [[ ! -f "$TEST_PROJECT" ]]; then
    log_error "测试项目文件不存在: $TEST_PROJECT"
    exit 1
fi
log_info "项目文件正常"
echo

# 清理之前的测试结果
log_info "清理之前的测试结果..."
rm -rf "$TEST_RESULTS_DIR"/*
rm -rf "$COVERAGE_DIR"/*
mkdir -p "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"
log_info "清理完成"
echo

# 函数：执行测试
run_test() {
    local test_category=$1
    local test_filter=$2
    local output_file=$3
    local description=$4
    
    log_info "[$test_category] $description"
    log_info "开始时间: $(date '+%H:%M:%S')"
    echo
    
    # 执行测试
    if dotnet test "$TEST_PROJECT" \
        --configuration "$BUILD_CONFIGURATION" \
        --filter "$test_filter" \
        --logger "trx;LogFileName=$TEST_RESULTS_DIR/$output_file" \
        --collect:"XPlat Code Coverage" \
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.OutputPath="$COVERAGE_DIR" \
        >> "$LOG_FILE" 2>&1; then
        log_success "$description 执行成功"
        return 0
    else
        log_warning "$description 执行失败"
        return 1
    fi
}

# 开始测试执行
echo "============================================"
echo "开始执行测试套件"
echo "============================================"

# 1. 单元测试
run_test "1/4" "Category=Unit" "unit-tests.trx" "执行单元测试"

# 2. 集成测试  
run_test "2/4" "Category=Integration" "integration-tests.trx" "执行集成测试"

# 3. UI测试
run_test "3/4" "Category=UI" "ui-tests.trx" "执行UI测试"

# 4. 性能测试
run_test "4/4" "Category=Performance" "performance-tests.trx" "执行性能测试"

# 5. 代码覆盖率分析
log_info "[分析] 代码覆盖率..."
if [[ -f "$COVERAGE_DIR/coverage.cobertura.xml" ]]; then
    log_info "覆盖率数据文件存在，开始分析..."
    
    # 安装 ReportGenerator（如果未安装）
    if ! command -v reportgenerator &> /dev/null; then
        log_info "安装 ReportGenerator..."
        dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.9 >> "$LOG_FILE" 2>&1
    fi
    
    # 生成覆盖率报告
    if reportgenerator -reports:"$COVERAGE_DIR/*.cobertura.xml" \
                      -targetdir:"$COVERAGE_DIR/html" \
                      -reporttypes:HTML;HTMLSummary \
                      >> "$LOG_FILE" 2>&1; then
        log_success "覆盖率报告生成成功"
        log_info "报告位置: $COVERAGE_DIR/html/index.html"
    else
        log_warning "覆盖率报告生成失败"
    fi
else
    log_warning "未找到覆盖率数据文件"
fi
echo

# 6. 生成综合测试报告
log_info "[报告] 生成综合测试报告..."
if [[ -f "$SCRIPT_DIR/generate-test-report.ps1" ]]; then
    # 如果有PowerShell脚本，尝试使用
    if command -v pwsh &> /dev/null; then
        pwsh -Command "& '$SCRIPT_DIR/generate-test-report.ps1' -TestResultsPath '$TEST_RESULTS_DIR' -OutputPath '$REPORTS_DIR/test-report.html' -CoveragePath '$COVERAGE_DIR'" >> "$LOG_FILE" 2>&1
    else
        log_warning "未找到 PowerShell，跳过综合报告生成"
    fi
fi

# 生成简单的HTML报告（备用）
if [[ ! -f "$REPORTS_DIR/test-report.html" ]]; then
    generate_simple_html_report
fi

if [[ -f "$REPORTS_DIR/test-report.html" ]]; then
    log_success "综合测试报告生成成功"
    log_info "报告位置: $REPORTS_DIR/test-report.html"
else
    log_warning "综合测试报告生成失败"
fi
echo

# 7. 生成执行摘要
log_info "[摘要] 生成执行摘要..."
if [[ -f "$SCRIPT_DIR/generate-execution-summary.ps1" ]] && command -v pwsh &> /dev/null; then
    pwsh -Command "& '$SCRIPT_DIR/generate-execution-summary.ps1' -LogFile '$LOG_FILE' -OutputPath '$REPORTS_DIR/execution-summary.txt'" >> "$LOG_FILE" 2>&1
fi

# 生成基本执行摘要
generate_basic_summary
echo

# 8. 显示测试结果统计
echo "============================================"
echo "测试结果统计"
echo "============================================"

# 分析TRX文件
analyze_test_results

echo
log_info "测试执行完成时间: $(date '+%H:%M:%S')"
echo

# 9. 结果摘要
echo "============================================"
echo "测试执行摘要"
echo "============================================"

echo "测试结果文件位置: $TEST_RESULTS_DIR"
echo "覆盖率报告位置: $COVERAGE_DIR/html/index.html"
echo "综合报告位置: $REPORTS_DIR/test-report.html"
echo "执行日志位置: $LOG_FILE"
echo

echo "快速访问链接:"
echo "- 测试报告: file://$REPORTS_DIR/test-report.html"
echo "- 覆盖率报告: file://$COVERAGE_DIR/html/index.html"
echo "- 执行日志: $LOG_FILE"
echo

# 检查测试结果
if grep -q "Failed" "$LOG_FILE"; then
    log_warning "部分测试执行失败，请查看详细报告"
    EXIT_CODE=1
else
    log_success "所有测试执行完成，无失败用例"
    EXIT_CODE=0
fi

# 如果设置了暂停变量，等待用户输入
if [[ "$PAUSE_ON_COMPLETE" == "true" ]]; then
    echo "按 Enter 键继续..."
    read
fi

exit $EXIT_CODE

# 辅助函数：生成简单HTML报告
generate_simple_html_report() {
    cat > "$REPORTS_DIR/test-report.html" << EOF
<!DOCTYPE html>
<html>
<head>
    <title>ZWDynLookup 测试报告</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        .pass { color: green; }
        .fail { color: red; }
        .summary { background-color: #e7f3ff; padding: 15px; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>ZWDynLookup 测试报告</h1>
    <div class="summary">
        <h2>执行信息</h2>
        <p>生成时间: $(date)</p>
        <p>日志文件: $LOG_FILE</p>
    </div>
    
    <h2>测试文件</h2>
    <ul>
EOF

    for trx_file in "$TEST_RESULTS_DIR"/*.trx; do
        if [[ -f "$trx_file" ]]; then
            filename=$(basename "$trx_file")
            echo "        <li><a href=\"../TestResults/$filename\">$filename</a></li>" >> "$REPORTS_DIR/test-report.html"
        fi
    done

    cat >> "$REPORTS_DIR/test-report.html" << EOF
    </ul>
    
    <h2>覆盖率报告</h2>
    <p><a href="../CoverageResults/html/index.html">查看详细覆盖率报告</a></p>
    
    <h2>日志文件</h2>
    <p><a href="../Logs/$(basename "$LOG_FILE")">查看执行日志</a></p>
</body>
</html>
EOF
}

# 辅助函数：生成基本执行摘要
generate_basic_summary() {
    cat > "$REPORTS_DIR/execution-summary.txt" << EOF
ZWDynLookup 测试执行摘要
============================================
执行时间: $(date)
日志文件: $LOG_FILE

测试结果目录: $TEST_RESULTS_DIR
覆盖率报告目录: $COVERAGE_DIR
综合报告目录: $REPORTS_DIR

测试套件:
- 单元测试 (Category=Unit)
- 集成测试 (Category=Integration)  
- UI测试 (Category=UI)
- 性能测试 (Category=Performance)

详细结果请查看:
- 综合报告: $REPORTS_DIR/test-report.html
- 覆盖率报告: $COVERAGE_DIR/html/index.html
- 执行日志: $LOG_FILE
EOF
}

# 辅助函数：分析测试结果
analyze_test_results() {
    local total_tests=0
    local passed_tests=0
    local failed_tests=0
    
    for trx_file in "$TEST_RESULTS_DIR"/*.trx; do
        if [[ -f "$trx_file" ]]; then
            filename=$(basename "$trx_file")
            log_info "分析测试结果文件: $filename"
            
            # 这里可以添加更详细的TRX文件解析逻辑
            # 目前只是统计文件数量
            ((total_tests++))
        fi
    done
    
    echo "找到 $total_tests 个测试结果文件"
    
    # 如果有覆盖率数据，显示覆盖率信息
    if [[ -f "$COVERAGE_DIR/coverage.cobertura.xml" ]]; then
        log_info "覆盖率数据文件存在"
    fi
}

# 错误处理
trap 'log_error "脚本执行过程中发生错误，退出码: $?"' ERR

# 如果没有参数，显示帮助信息
if [[ $# -eq 0 ]]; then
    echo "使用方法:"
    echo "  $0 [选项]"
    echo
    echo "选项:"
    echo "  --unit-only      仅运行单元测试"
    echo "  --integration-only  仅运行集成测试"
    echo "  --ui-only        仅运行UI测试"
    echo "  --performance-only  仅运行性能测试"
    echo "  --no-coverage    跳过覆盖率分析"
    echo "  --pause          执行完成后暂停"
    echo "  --help           显示此帮助信息"
    echo
    echo "环境变量:"
    echo "  PAUSE_ON_COMPLETE=true  执行完成后暂停"
    echo "  PAUSE_ON_ERROR=true     发生错误时暂停"
fi

# 解析命令行参数
UNIT_ONLY=false
INTEGRATION_ONLY=false
UI_ONLY=false
PERFORMANCE_ONLY=false
SKIP_COVERAGE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --unit-only)
            UNIT_ONLY=true
            shift
            ;;
        --integration-only)
            INTEGRATION_ONLY=true
            shift
            ;;
        --ui-only)
            UI_ONLY=true
            shift
            ;;
        --performance-only)
            PERFORMANCE_ONLY=true
            shift
            ;;
        --no-coverage)
            SKIP_COVERAGE=true
            shift
            ;;
        --pause)
            PAUSE_ON_COMPLETE=true
            shift
            ;;
        --help)
            echo "帮助信息请查看脚本开头的说明"
            exit 0
            ;;
        *)
            log_error "未知参数: $1"
            exit 1
            ;;
    esac
done

# 如果指定了特定测试类型，重新执行
if [[ "$UNIT_ONLY" == true ]] || [[ "$INTEGRATION_ONLY" == true ]] || [[ "$UI_ONLY" == true ]] || [[ "$PERFORMANCE_ONLY" == true ]]; then
    echo "执行指定类型的测试..."
    # 这里可以添加针对特定测试类型的执行逻辑
fi