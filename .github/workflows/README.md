---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 3045022078a63ad218f5d52593341a711cb90732f1b8162313962134d4290792aea432df02210084a28eb46d901ded3186ce52641a5252e59cd80d83f1ea6211f1ef755332c6ab
    ReservedCode2: 304502210097c06d20ed705f050e37c70fa471e5a251edecb5f97573efcdbb32a0730be33402201b910f706e9af28b920231895d0ac03b5e07dc5a57c223e13fbfb8b7ecb31eb9
---

# GitHub Actions 测试工作流说明

## 📋 概述

这个GitHub Actions工作流为中望CAD动态块查寻插件提供完整的CI/CD流程，包括自动化测试、代码质量检查、安全扫描和发布准备。

## 🏗️ 工作流结构

### 触发条件
- **代码推送**: `main` 和 `develop` 分支
- **Pull Request**: 提交到 `main` 和 `develop` 分支
- **定时执行**: 每天UTC 2:00 (北京时间10:00)

### 工作流程阶段

#### 1. **代码质量检查** (`code-quality`)
- **运行环境**: Ubuntu Latest
- **主要任务**:
  - SonarCloud代码质量分析
  - 静态代码分析
  - 依赖安全检查
  - 代码复杂度分析

#### 2. **Windows环境测试** (`test-windows`)
- **运行环境**: Windows Latest
- **测试矩阵**:
  - 配置: Debug, Release
  - 框架: .NET Framework 4.8
- **测试类型**:
  - 单元测试 (BParameterCommandTests, BActionToolCommandTests)
  - 集成测试 (IntegrationTests, CommandWorkflowTests)
  - 性能测试 (PerformanceTests, LargeLookupTableTests)
  - 代码覆盖率分析

#### 3. **UI自动化测试** (`test-ui`)
- **运行环境**: Windows Latest
- **依赖**: 需要Windows环境
- **测试内容**:
  - 所有对话框UI交互测试
  - 模拟CAD环境运行
  - UI自动化验证

#### 4. **安全扫描** (`security-scan`)
- **运行环境**: Ubuntu Latest
- **检查项目**:
  - 依赖漏洞扫描
  - CodeQL安全分析
  - 过期依赖检查
  - 已知漏洞检测

#### 5. **许可证检查** (`license-check`)
- **运行环境**: Ubuntu Latest
- **检查内容**:
  - 第三方库许可证扫描
  - 兼容性验证
  - 许可证合规性检查

#### 6. **发布准备** (`prepare-release`)
- **触发条件**: main分支推送
- **任务内容**:
  - 自动版本号生成
  - 发布版本构建
  - 测试验证
  - 创建GitHub Release
  - 打包发布文件

#### 7. **结果通知** (`notify-results`)
- **任务**: 汇总所有测试结果
- **输出**: GitHub Step Summary
- **内容**: 测试状态、覆盖率、错误信息

## 🔧 环境变量配置

### 必需的环境变量
```yaml
env:
  DOTNET_VERSION: '6.0.x'           # .NET SDK版本
  DOTNET_FRAMEWORK_VERSION: 'net48' # 目标框架
  PROJECT_NAME: 'ZWDynLookup'       # 项目名称
  SOLUTION_NAME: 'ZWDynLookup.sln'  # 解决方案文件
  TEST_PROJECT_NAME: 'ZWDynLookup.Tests'
  COVERAGE_THRESHOLD: 85            # 代码覆盖率阈值
```

### GitHub Secrets配置

需要在GitHub仓库的Settings > Secrets中配置以下密钥：

#### 必需的Secrets
```
SONAR_TOKEN        # SonarCloud访问令牌
```

#### 可选的Secrets
```
CODECOV_TOKEN      # Codecov令牌 (如果使用Codecov)
NUGET_API_KEY      # NuGet发布API密钥 (如果发布到NuGet)
```

## 🚀 使用指南

### 1. 启用工作流

1. 将工作流文件复制到 `.github/workflows/` 目录
2. 推送到GitHub仓库
3. 在GitHub Actions页面查看工作流执行

### 2. 配置SonarCloud

1. 在 [SonarCloud](https://sonarcloud.io/) 创建账户
2. 导入GitHub仓库
3. 获取项目密钥和令牌
4. 在GitHub Secrets中添加 `SONAR_TOKEN`

### 3. 查看测试结果

#### 工作流执行页面
- GitHub仓库 > Actions > 选择工作流执行

#### 测试报告
- TestResults/ 目录下的 .trx 文件
- 代码覆盖率报告
- 性能测试结果

#### 质量分析
- SonarCloud分析结果
- 安全扫描报告
- 许可证合规性报告

## 📊 质量指标

### 代码覆盖率要求
- **目标覆盖率**: ≥ 85%
- **单元测试**: 核心业务逻辑 100%覆盖
- **集成测试**: 关键工作流程 90%覆盖
- **UI测试**: 主要界面组件 80%覆盖

### 性能基准
- **小型查寻表** (< 1,000行): < 100ms
- **中型查寻表** (1,000-10,000行): < 500ms  
- **大型查寻表** (10,000-100,000行): < 2,000ms

### 安全要求
- **高危漏洞**: 0个
- **中危漏洞**: ≤ 3个 (需要计划修复)
- **过期依赖**: 及时更新

## 🔧 自定义配置

### 修改测试配置

编辑工作流文件中的环境变量：
```yaml
env:
  COVERAGE_THRESHOLD: 90  # 提高覆盖率要求
  DOTNET_VERSION: '7.0.x' # 升级.NET版本
```

### 添加新的测试类型

在 `test-windows` job 中添加新的测试步骤：
```yaml
- name: 运行新测试类型
  shell: pwsh
  run: |
    dotnet test ${{ env.TEST_PROJECT_NAME }}/${{ env.TEST_PROJECT_NAME }}.csproj `
      --configuration Release `
      --framework ${{ env.DOTNET_FRAMEWORK_VERSION }} `
      --filter "Category=NewTestCategory" `
      --logger trx
```

### 跳过特定测试

在测试命令中添加 `--filter` 参数：
```bash
dotnet test --filter "Category!=SlowTest"
```

## 🐛 故障排除

### 常见问题

#### 1. 测试失败
- 检查代码覆盖率是否达标
- 验证单元测试是否正确通过
- 查看具体的失败日志

#### 2. 构建失败
- 确认所有依赖都已正确配置
- 检查MSBuild版本兼容性
- 验证项目文件路径

#### 3. SonarCloud分析失败
- 验证 `SONAR_TOKEN` 是否正确配置
- 检查项目权限设置
- 确认SonarCloud项目配置

### 调试技巧

#### 启用详细日志
在测试命令中添加 `--verbosity detailed`：
```yaml
dotnet test --verbosity detailed --logger trx
```

#### 单独运行特定测试
```bash
# 只运行单元测试
dotnet test --filter "Category=UnitTest"

# 只运行特定测试类
dotnet test --filter "BParameterCommandTests"
```

#### 本地复现测试
```bash
# 安装依赖
dotnet restore

# 构建项目
dotnet build

# 运行所有测试
dotnet test

# 运行特定类型测试
dotnet test --filter "Category=Integration"
```

## 📈 性能优化

### 缓存策略
- NuGet包缓存: 加速依赖还原
- 测试结果缓存: 避免重复执行

### 并行执行
- 多配置矩阵并行测试
- 独立job并行运行

### 条件执行
- 仅在特定条件下执行耗时测试
- 失败时跳过非关键步骤

## 🔄 持续改进

### 定期更新
- .NET SDK版本升级
- 依赖包版本更新
- GitHub Actions版本更新

### 质量提升
- 增加新的质量检查项
- 优化测试覆盖率
- 完善性能基准

### 自动化增强
- 添加自动修复建议
- 智能错误诊断
- 预测性分析

---

**总结**: 这个GitHub Actions工作流提供了完整的CI/CD流程，确保代码质量、安全性和可维护性，为中望CAD动态块查寻插件的持续开发提供了强有力的保障。