---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 3046022100dc9dbe40f59ab37575b9f3c48a2239d7b0c3797ca64c617ac3d86653c0a3c9ad022100f56ddb32802c070b786b8d754da413d31cdce37bc1502059532837f84eb3b574
    ReservedCode2: 304502203dc0f7fbbaf46a3796a072a9c832a413b1961de3178f089416d0a81a02fe73d602210090dd4e6af6a98f73e78f40047e72c02841680a5790dadf660721eb0a7b8182a5
---

# 中望CAD动态块查寻插件单元测试用例

## 概述

本文档详细描述了为中望CAD动态块查寻插件创建的单元测试用例。该测试套件使用NUnit测试框架和Moq模拟框架，涵盖了插件的所有核心功能模块。

## 测试架构

### 测试项目结构

```
ZWDynLookup.Tests/
├── ZWDynLookup.Tests.csproj          # 测试项目配置文件
├── Commands/                          # 命令层测试
│   ├── BParameterCommandTests.cs     # BPARAMETER命令测试
│   └── BActionToolCommandTests.cs    # BACTIONTOOL命令测试
├── Services/                          # 服务层测试
│   ├── LookupParameterServiceTests.cs # 查寻参数服务测试
│   └── LookupActionServiceTests.cs    # 查寻动作服务测试
├── Models/                            # 模型层测试
│   ├── LookupTableDataTests.cs       # 查寻表数据模型测试
│   └── ParameterPropertyTests.cs     # 参数特性模型测试
├── Helpers/                           # 助手类测试
│   └── BlockReferenceHelperTests.cs  # 块引用助手测试
├── TestData/                         # 测试数据目录
└── README.md                         # 本说明文档
```

### 测试框架和依赖

- **NUnit 3.14.0**: 测试框架
- **NUnit3TestAdapter 4.5.0**: Visual Studio测试适配器
- **Moq 4.20.70**: 模拟框架
- **Castle.Core 5.1.1**: 核心组件库

## 测试覆盖范围

### 1. 命令层测试 (Commands)

#### BParameterCommandTests.cs
测试BPARAMETER命令的K选项（查寻参数）功能：

- **基本功能测试**
  - 正常执行流程测试
  - 编辑器和文档为空的情况处理
  - 非块编辑器环境检测

- **参数创建测试**
  - 有效参数创建
  - 参数名称和夹点数验证
  - 异常情况处理

- **用户交互测试**
  - 参数属性对话框测试
  - 用户取消操作处理
  - 参数验证逻辑

- **关键字处理测试**
  - 名称、标签、说明、选项板关键字处理
  - 递归调用验证
  - 退出关键字处理

- **边界条件测试**
  - 异常情况处理
  - 快速创建功能测试

#### BActionToolCommandTests.cs
测试BACTIONTOOL命令的L选项（查寻动作）功能：

- **基本功能测试**
  - 正常执行流程测试
  - 环境验证和错误处理

- **命令选项测试**
  - 查寻动作、标准动作、管理选择集选项
  - 关键字处理和模式切换

- **查寻动作流程测试**
  - 参数选择、对象选择流程
  - 查寻值设置和验证
  - 动作创建和错误处理

- **动作管理测试**
  - 选择集管理功能
  - 查寻值设置和管理
  - 动作属性配置

### 2. 服务层测试 (Services)

#### LookupParameterServiceTests.cs
测试查寻参数服务功能：

- **参数查找测试**
  - 参数搜索和选择
  - 用户交互处理
  - 异常情况处理

- **参数验证测试**
  - 参数对象有效性验证
  - 参数名称格式验证
  - 属性完整性检查

- **参数操作测试**
  - 属性获取和更新
  - 参数删除和克隆
  - 统计信息获取

- **高级功能测试**
  - 参数搜索和过滤
  - 批量操作处理
  - 性能边界测试

#### LookupActionServiceTests.cs
测试查寻动作服务功能：

- **动作查找测试**
  - 动作搜索和识别
  - 选择集处理
  - 错误处理机制

- **动作验证测试**
  - 动作对象有效性
  - 关联参数验证
  - 属性完整性检查

- **查寻值管理测试**
  - 查寻值获取和设置
  - 数据验证和转换
  - 批量更新功能

- **动作执行测试**
  - 查寻动作执行逻辑
  - 值验证和处理
  - 异常情况处理

### 3. 模型层测试 (Models)

#### LookupTableDataTests.cs
测试查寻表数据模型：

- **基本属性测试**
  - 表名、列、行管理
  - 数据类型验证
  - 状态管理

- **列管理测试**
  - 列添加、删除、更新
  - 列名唯一性验证
  - 数据类型约束

- **行管理测试**
  - 行数据CRUD操作
  - 数据验证和约束
  - 批量操作支持

- **数据验证测试**
  - 完整性和一致性验证
  - 约束条件检查
  - 错误处理

- **序列化测试**
  - JSON序列化/反序列化
  - CSV导入/导出
  - 数据完整性保持

#### ParameterPropertyTests.cs
测试参数特性模型：

- **基本属性测试**
  - 属性名称和显示名称
  - 属性类型和值
  - 描述和格式化

- **数据类型测试**
  - 不同类型的值验证
  - 类型转换处理
  - 类型约束检查

- **验证规则测试**
  - 必需字段验证
  - 数值范围检查
  - 允许值列表

- **高级功能测试**
  - 属性克隆和复制
  - 值格式化和转换
  - 性能边界测试

### 4. 助手类测试 (Helpers)

#### BlockReferenceHelperTests.cs
测试块引用助手功能：

- **块引用查找测试**
  - 块引用搜索和识别
  - 选择集处理
  - 环境验证

- **块定义管理测试**
  - 块定义查找和验证
  - 块创建和插入
  - 变换操作

- **属性管理测试**
  - 属性获取和设置
  - 属性验证和转换
  - 批量操作支持

- **动态块测试**
  - 动态块识别
  - 动态属性操作
  - 属性值设置和获取

## 测试执行方法

### 1. 使用Visual Studio运行测试

1. 打开解决方案文件 `ZWDynLookup.sln`
2. 确保测试项目 `ZWDynLookup.Tests` 设置为启动项目
3. 在"测试资源管理器"中运行所有测试
4. 查看测试结果和覆盖率报告

### 2. 使用命令行运行测试

```bash
# 在测试项目目录下执行
dotnet test

# 生成测试覆盖率报告
dotnet test --collect:"XPlat Code Coverage"

# 运行特定测试类
dotnet test --filter "BParameterCommandTests"

# 运行特定测试方法
dotnet test --filter "BParameterCommandTests.Execute_正常执行流程_应成功创建参数"
```

### 3. 持续集成测试

在CI/CD管道中集成测试：

```yaml
# GitHub Actions 示例
- name: Run Unit Tests
  run: |
    cd ZWDynLookup.Tests
    dotnet test --configuration Release --logger "trx;LogFileName=results.trx"
```

## 测试最佳实践

### 1. 测试命名规范

- 使用描述性的测试方法名称
- 遵循"测试方法_条件_预期结果"命名模式
- 使用中文测试名称便于理解和维护

### 2. 测试数据管理

- 使用测试夹具(SetUp/TearDown)管理测试数据
- 创建独立的测试数据避免测试间依赖
- 使用模拟对象隔离外部依赖

### 3. 断言策略

- 使用具体的断言验证预期结果
- 避免过度断言，保持测试的专注性
- 提供清晰的错误消息

### 4. 异常处理测试

- 测试正常流程的同时测试异常情况
- 验证异常消息和日志记录
- 确保系统不会因异常而崩溃

## 性能测试

### 边界条件测试

- **大量数据处理**: 测试处理大量参数、动作、块引用
- **内存使用**: 监控内存泄漏和资源释放
- **响应时间**: 确保用户交互的及时响应

### 示例性能测试

```csharp
[Test]
public void FindLookupParameters_边界测试_大量参数()
{
    // 创建大量测试数据
    var largeDataset = CreateLargeParameterSet(10000);
    
    var startTime = DateTime.Now;
    var result = _service.FindLookupParameters(largeDataset);
    var endTime = DateTime.Now;
    
    Assert.IsTrue(result.Count > 0);
    Assert.IsTrue((endTime - startTime).TotalSeconds < 5); // 5秒内完成
}
```

## 扩展测试

### 1. 添加新测试

当添加新功能时，请：

1. 在相应的测试类中添加测试方法
2. 遵循现有的测试命名和结构
3. 覆盖正常流程、异常情况和边界条件
4. 更新测试文档

### 2. 测试覆盖率

- 目标代码覆盖率 ≥ 80%
- 关键功能覆盖率 ≥ 90%
- 定期检查和更新测试用例

### 3. 集成测试

除了单元测试外，建议添加：

- 端到端测试
- 性能基准测试
- 兼容性测试

## 常见问题解决

### 1. 测试失败排查

- 检查模拟对象的设置是否正确
- 验证测试数据的一致性
- 确认异常情况的预期行为

### 2. 性能问题

- 使用性能分析工具定位瓶颈
- 优化测试数据的创建和清理
- 考虑并行测试执行

### 3. 维护成本

- 定期重构测试代码
- 提取公共测试辅助方法
- 使用测试数据工厂模式

## 总结

该单元测试套件为中望CAD动态块查寻插件提供了全面的测试覆盖，确保代码质量和功能正确性。通过遵循测试最佳实践，可以持续维护和改进测试用例，为项目的长期发展提供可靠的质量保障。

---

**测试文件列表**:
1. `ZWDynLookup.Tests.csproj` - 测试项目配置
2. `Commands/BParameterCommandTests.cs` - BPARAMETER命令测试
3. `Commands/BActionToolCommandTests.cs` - BACTIONTOOL命令测试  
4. `Services/LookupParameterServiceTests.cs` - 查寻参数服务测试
5. `Services/LookupActionServiceTests.cs` - 查寻动作服务测试
6. `Models/LookupTableDataTests.cs` - 查寻表数据模型测试
7. `Models/ParameterPropertyTests.cs` - 参数特性模型测试
8. `Helpers/BlockReferenceHelperTests.cs` - 块引用助手测试

**总计**: 8个测试文件，约4500行测试代码，覆盖了插件的所有核心功能模块。