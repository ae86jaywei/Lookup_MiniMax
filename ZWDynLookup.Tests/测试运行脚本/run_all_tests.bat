@echo off
setlocal EnableDelayedExpansion

:: ZWDynLookup 测试运行脚本 (Windows)
:: 用途：自动化执行所有测试套件并生成报告

echo ============================================
echo ZWDynLookup 自动化测试运行脚本 (Windows)
echo ============================================
echo.

:: 设置环境变量
set "SOLUTION_DIR=%~dp0..\"
set "TEST_PROJECT=%SOLUTION_DIR%ZWDynLookup.Tests\ZWDynLookup.Tests.csproj"
set "BUILD_CONFIGURATION=Release"
set "TEST_RESULTS_DIR=%SOLUTION_DIR%TestResults"
set "COVERAGE_DIR=%SOLUTION_DIR%CoverageResults"
set "LOG_DIR=%SOLUTION_DIR%Logs"
set "REPORTS_DIR=%SOLUTION_DIR%TestReports"

:: 创建输出目录
if not exist "%TEST_RESULTS_DIR%" mkdir "%TEST_RESULTS_DIR%"
if not exist "%COVERAGE_DIR%" mkdir "%COVERAGE_DIR%"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"
if not exist "%REPORTS_DIR%" mkdir "%REPORTS_DIR%"

:: 设置时间戳
for /f "tokens=1-3 delims=:. " %%a in ("%time%") do set "timestamp=%%a%%b%%c"
for /f "tokens=1-3 delims=-" %%a in ("%date%") do set "datestamp=%%a%%b%%c"
set "LOG_FILE=%LOG_DIR%\test-execution-%datestamp%-%timestamp%.log"

echo [开始] ZWDynLookup 测试执行
echo 时间: %date% %time%
echo 日志文件: %LOG_FILE%
echo.

:: 检查 .NET 环境
echo [检查] .NET 环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [错误] 未找到 .NET SDK，请先安装 .NET 8.0
    goto :error
)
echo [信息] .NET 环境正常
echo.

:: 检查项目文件
echo [检查] 项目文件...
if not exist "%TEST_PROJECT%" (
    echo [错误] 测试项目文件不存在: %TEST_PROJECT%
    goto :error
)
echo [信息] 项目文件正常
echo.

:: 清理之前的测试结果
echo [清理] 清理之前的测试结果...
if exist "%TEST_RESULTS_DIR%" rmdir /s /q "%TEST_RESULTS_DIR%"
if exist "%COVERAGE_DIR%" rmdir /s /q "%COVERAGE_DIR%"
mkdir "%TEST_RESULTS_DIR%"
mkdir "%COVERAGE_DIR%"
echo [信息] 清理完成
echo.

:: 开始测试执行
echo ============================================
echo 开始执行测试套件
echo ============================================

:: 1. 单元测试
echo [1/4] 执行单元测试...
echo 开始时间: %time%
echo.

dotnet test "%TEST_PROJECT%" ^
    --configuration %BUILD_CONFIGURATION% ^
    --filter "Category=Unit" ^
    --logger "trx;LogFileName=%TEST_RESULTS_DIR%\unit-tests.trx" ^
    --collect:"XPlat Code Coverage" ^
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura ^
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.OutputPath=%COVERAGE_DIR% ^
    >> "%LOG_FILE%" 2>&1

if errorlevel 1 (
    echo [警告] 单元测试执行失败
) else (
    echo [信息] 单元测试执行成功
)
echo.

:: 2. 集成测试
echo [2/4] 执行集成测试...
echo 开始时间: %time%
echo.

dotnet test "%TEST_PROJECT%" ^
    --configuration %BUILD_CONFIGURATION% ^
    --filter "Category=Integration" ^
    --logger "trx;LogFileName=%TEST_RESULTS_DIR%\integration-tests.trx" ^
    >> "%LOG_FILE%" 2>&1

if errorlevel 1 (
    echo [警告] 集成测试执行失败
) else (
    echo [信息] 集成测试执行成功
)
echo.

:: 3. UI测试
echo [3/4] 执行UI测试...
echo 开始时间: %time%
echo.

dotnet test "%TEST_PROJECT%" ^
    --configuration %BUILD_CONFIGURATION% ^
    --filter "Category=UI" ^
    --logger "trx;LogFileName=%TEST_RESULTS_DIR%\ui-tests.trx" ^
    --collect:"XPlat Code Coverage" ^
    >> "%LOG_FILE%" 2>&1

if errorlevel 1 (
    echo [警告] UI测试执行失败
) else (
    echo [信息] UI测试执行成功
)
echo.

:: 4. 性能测试
echo [4/4] 执行性能测试...
echo 开始时间: %time%
echo.

dotnet test "%TEST_PROJECT%" ^
    --configuration %BUILD_CONFIGURATION% ^
    --filter "Category=Performance" ^
    --logger "trx;LogFileName=%TEST_RESULTS_DIR%\performance-tests.trx" ^
    >> "%LOG_FILE%" 2>&1

if errorlevel 1 (
    echo [警告] 性能测试执行失败
) else (
    echo [信息] 性能测试执行成功
)
echo.

:: 5. 代码覆盖率分析
echo [分析] 代码覆盖率...
if exist "%COVERAGE_DIR%\coverage.cobertura.xml" (
    echo [信息] 覆盖率数据文件存在，开始分析...
    
    :: 使用 ReportGenerator 生成HTML报告
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.9 >> "%LOG_FILE%" 2>&1
    
    reportgenerator -reports:"%COVERAGE_DIR%\*.cobertura.xml" ^
                   -targetdir:"%COVERAGE_DIR%\html" ^
                   -reporttypes:HTML;HTMLSummary ^
                   >> "%LOG_FILE%" 2>&1
    
    if errorlevel 1 (
        echo [警告] 覆盖率报告生成失败
    ) else (
        echo [信息] 覆盖率报告生成成功
        echo 报告位置: %COVERAGE_DIR%\html\index.html
    )
) else (
    echo [警告] 未找到覆盖率数据文件
)
echo.

:: 6. 生成综合测试报告
echo [报告] 生成综合测试报告...
powershell -Command "& '%~dp0generate-test-report.ps1' -TestResultsPath '%TEST_RESULTS_DIR%' -OutputPath '%REPORTS_DIR%\test-report.html' -CoveragePath '%COVERAGE_DIR%'" >> "%LOG_FILE%" 2>&1

if errorlevel 1 (
    echo [警告] 综合测试报告生成失败
) else (
    echo [信息] 综合测试报告生成成功
    echo 报告位置: %REPORTS_DIR%\test-report.html
)
echo.

:: 7. 检查测试结果
echo ============================================
echo 测试结果统计
echo ============================================

:: 统计测试结果
set /a totalTests=0
set /a passedTests=0
set /a failedTests=0

for %%f in ("%TEST_RESULTS_DIR%\*.trx") do (
    echo 分析测试结果文件: %%~nxf
    powershell -Command "& 'Get-TestResultsStats' -TrxFile '%%f'" >> "%LOG_FILE%" 2>&1
    
    :: 这里可以添加更详细的TRX文件解析逻辑
)

echo.
echo 测试执行完成时间: %time%
echo.

:: 8. 结果摘要
echo ============================================
echo 测试执行摘要
echo ============================================

echo 测试结果文件位置: %TEST_RESULTS_DIR%
echo 覆盖率报告位置: %COVERAGE_DIR%\html\index.html
echo 综合报告位置: %REPORTS_DIR%\test-report.html
echo 日志文件位置: %LOG_FILE%
echo.

:: 9. 生成执行摘要
echo [摘要] 生成执行摘要...
powershell -Command "& '%~dp0generate-execution-summary.ps1' -LogFile '%LOG_FILE%' -OutputPath '%REPORTS_DIR%\execution-summary.txt'" >> "%LOG_FILE%" 2>&1

:: 10. 清理临时文件
echo [清理] 清理临时文件...
:: 这里可以添加清理逻辑

echo.
echo ============================================
echo 测试执行完成！
echo ============================================

:: 检查是否有测试失败
findstr /c:"Failed" "%LOG_FILE%" >nul
if errorlevel 1 (
    echo [成功] 所有测试执行完成，无失败用例
    set EXIT_CODE=0
) else (
    echo [注意] 部分测试执行失败，请查看详细报告
    set EXIT_CODE=1
)

:: 显示报告链接
echo.
echo 快速访问链接:
echo - 测试报告: %REPORTS_DIR%\test-report.html
echo - 覆盖率报告: %COVERAGE_DIR%\html\index.html
echo - 执行日志: %LOG_FILE%
echo.

if "%PAUSE_ON_COMPLETE%"=="true" (
    echo 按任意键继续...
    pause >nul
)

exit /b %EXIT_CODE%

:error
echo.
echo ============================================
echo 测试执行失败！
echo ============================================
echo 错误信息已记录到: %LOG_FILE%
echo.

if "%PAUSE_ON_ERROR%"=="true" (
    echo 按任意键退出...
    pause >nul
)

exit /b 1