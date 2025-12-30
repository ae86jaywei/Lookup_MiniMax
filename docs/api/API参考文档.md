---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 3045022100aa3071a91f9c9dd40ba300fede9d5550712aa4899127b616ff050556b1f80cd102201c533cbfbb6f65672fd715c0aa966e486a1199feefbe58a56deb0e7430a25467
    ReservedCode2: 3044022060889c8ca59d86fac5faea48b7f10e923b77d6cb796589910ec7ea1188e4e19e02201f974f9c82e6a174ec3fae7721e82b1e68b3ebb94f41696aeb8abd23251064c4
---

# 中望CAD动态块查寻插件API参考文档

## 目录
1. [API概述](#api概述)
2. [核心类库](#核心类库)
3. [命令API](#命令api)
4. [数据模型API](#数据模型api)
5. [服务接口API](#服务接口api)
6. [UI组件API](#ui组件api)
7. [扩展开发API](#扩展开发api)
8. [最佳实践](#最佳实践)

---

## API概述

### API架构
中望CAD动态块查寻插件采用分层架构设计，提供清晰的API接口：

```
┌─────────────────────────────────────────┐
│              用户界面层 (UI)              │
├─────────────────────────────────────────┤
│              业务逻辑层 (Commands)        │
├─────────────────────────────────────────┤
│              服务管理层 (Services)       │
├─────────────────────────────────────────┤
│              数据访问层 (Models)         │
├─────────────────────────────────────────┤
│              CAD核心层 (ZwCAD API)      │
└─────────────────────────────────────────┘
```

### 命名空间结构
```csharp
namespace ZWDynLookup
{
    // 核心类和接口
    namespace Commands          // 命令实现
    namespace Models           // 数据模型
    namespace Service         // 服务接口
    namespace UI             // 用户界面
    namespace Extensions     // 扩展功能
}
```

### 版本兼容性
- **当前API版本**: 1.0.0
- **支持的CAD版本**: 中望CAD 2020+
- **.NET Framework要求**: 4.8+
- **向后兼容性**: 保持主版本号不变时的向后兼容

---

## 核心类库

### PluginEntry - 插件主入口

#### 类定义
```csharp
/// <summary>
/// 中望CAD动态块查寻插件主入口类
/// </summary>
[Guid("A1B2C3D4-E5F6-7890-ABCD-123456789012")]
public class PluginEntry : IExtensionApplication
```

#### 主要属性
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `LogFilePath` | `string` | 日志文件完整路径 |
| `CurrentVersion` | `string` | 插件当前版本 |

#### 主要方法

##### Initialize() - 插件初始化
```csharp
/// <summary>
/// 插件初始化
/// </summary>
public void Initialize()
```

**功能说明**: 插件加载时自动调用，执行初始化操作

**实现示例**:
```csharp
public void Initialize()
{
    try
    {
        Log("插件初始化开始");
        
        // 创建日志目录
        var logDir = Path.GetDirectoryName(LogFilePath);
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        // 注册命令
        RegisterCommands();

        // 初始化插件界面
        InitializeUI();

        Log("插件初始化完成");
    }
    catch (System.Exception ex)
    {
        Log($"插件初始化失败: {ex.Message}");
        MessageBox.Show($"插件初始化失败: {ex.Message}", 
            "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

##### Terminate() - 插件卸载
```csharp
/// <summary>
/// 插件卸载
/// </summary>
public void Terminate()
```

**功能说明**: 插件卸载时自动调用，清理资源

##### GetActiveDocument() - 获取当前文档
```csharp
/// <summary>
/// 获取当前活动的CAD文档
/// </summary>
/// <returns>CAD文档对象</returns>
public static Document? GetActiveDocument()
```

**返回值**: `Document?` - 当前活动的CAD文档，失败时返回null

**使用示例**:
```csharp
var doc = PluginEntry.GetActiveDocument();
if (doc != null)
{
    var editor = doc.Editor;
    editor.WriteMessage("当前文档获取成功");
}
```

##### GetEditor() - 获取编辑器对象
```csharp
/// <summary>
/// 获取CAD编辑器
/// </summary>
/// <returns>CAD编辑器对象</returns>
public static Editor? GetEditor()
```

**返回值**: `Editor?` - CAD编辑器对象，失败时返回null

##### Log() - 写入日志
```csharp
/// <summary>
/// 写入日志
/// </summary>
/// <param name="message">日志消息</param>
public static void Log(string message)
```

**参数说明**:
- `message`: 要写入的日志消息

**使用示例**:
```csharp
PluginEntry.Log("用户执行了查寻参数创建操作");
```

### CommandMethodService - 命令管理服务

#### 类定义
```csharp
/// <summary>
/// 命令注册管理服务
/// </summary>
public static class CommandMethodService
```

#### 主要方法

##### RegisterCommand() - 注册命令
```csharp
/// <summary>
/// 注册命令
/// </summary>
/// <param name="commandName">命令名称</param>
/// <param name="commandType">命令类型</param>
/// <param name="displayName">显示名称</param>
/// <param name="commandLineName">命令行名称</param>
/// <param name="description">命令描述</param>
public static void RegisterCommand(string commandName, Type commandType, 
    string displayName, string commandLineName, string description)
```

**参数说明**:
- `commandName`: 唯一命令标识符
- `commandType`: 实现命令的类类型
- `displayName`: 用户界面显示的名称
- `commandLineName`: 命令行输入的名称
- `description`: 命令功能描述

**使用示例**:
```csharp
CommandMethodService.RegisterCommand("ZWBPARAMETER", typeof(BParameterCommand), 
    "创建查寻参数", "ZWBPARAMETER", "创建动态块查寻参数");
```

##### UnregisterCommands() - 注销所有命令
```csharp
/// <summary>
/// 注销所有命令
/// </summary>
public static void UnregisterCommands()
```

##### GetRegisteredCommands() - 获取已注册命令
```csharp
/// <summary>
/// 获取已注册的命令列表
/// </summary>
/// <returns>命令名称列表</returns>
public static List<string> GetRegisteredCommands()
```

**返回值**: `List<string>` - 已注册的命令名称列表

##### ExecuteCommand() - 执行命令
```csharp
/// <summary>
/// 执行指定命令
/// </summary>
/// <param name="commandName">命令名称</param>
/// <param name="parameters">命令参数</param>
/// <returns>命令执行结果</returns>
public static CommandResult ExecuteCommand(string commandName, object parameters = null)
```

**参数说明**:
- `commandName`: 要执行的命令名称
- `parameters`: 命令执行参数（可选）

**返回值**: `CommandResult` - 命令执行结果对象

#### CommandResult 类
```csharp
/// <summary>
/// 命令执行结果
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
    public Exception Exception { get; set; }
    public DateTime ExecuteTime { get; set; }
}
```

---

## 命令API

### BParameterCommand - 查寻参数命令

#### 类定义
```csharp
/// <summary>
/// BPARAMETER命令主类，实现BPARAMETER K选项功能
/// </summary>
[CommandMethod("ZWBPARAMETER")]
public class BParameterCommand : ICommand
```

#### 主要方法

##### Execute() - 主执行方法
```csharp
/// <summary>
/// 执行查寻参数创建命令
/// </summary>
[CommandMethod("ZWBPARAMETER")]
public void Execute()
```

**功能说明**: 主要的命令入口点，处理用户交互和参数创建流程

**实现流程**:
1. 检查当前环境（必须在块编辑器中）
2. 显示命令选项
3. 处理用户输入
4. 调用相应的处理方法

**使用示例**:
```csharp
var parameterCommand = new BParameterCommand();
parameterCommand.Execute();
```

##### ExecuteLookupParameterFlow() - 查寻参数创建流程
```csharp
/// <summary>
/// 执行查寻参数创建流程
/// </summary>
private void ExecuteLookupParameterFlow()
```

**功能说明**: 处理查寻参数创建的完整流程

**流程步骤**:
1. 初始化参数属性对话框
2. 获取用户输入的参数属性
3. 验证参数属性
4. 创建查寻参数
5. 设置参数夹点
6. 更新数据库

##### ShowParameterPropertiesDialog() - 显示参数属性对话框
```csharp
/// <summary>
/// 显示参数属性设置对话框
/// </summary>
/// <param name="parameter">参数对象</param>
/// <returns>对话框执行结果</returns>
private DialogResult ShowParameterPropertiesDialog(ParameterInfo parameter)
```

**参数说明**:
- `parameter`: 参数信息对象

**返回值**: `DialogResult` - 对话框执行结果（OK/Cancel）

##### ValidateParameter() - 验证参数
```csharp
/// <summary>
/// 验证参数属性
/// </summary>
/// <param name="parameter">参数信息</param>
/// <returns>验证结果</returns>
private ValidationResult ValidateParameter(ParameterInfo parameter)
```

**参数说明**:
- `parameter`: 要验证的参数信息

**返回值**: `ValidationResult` - 验证结果对象

### BActionToolCommand - 查寻动作命令

#### 类定义
```csharp
/// <summary>
/// BACTIONTOOL命令实现类
/// </summary>
[CommandMethod("ZWBACTIONTOOL")]
public class BActionToolCommand : ICommand
```

#### 主要方法

##### Execute() - 主执行方法
```csharp
/// <summary>
/// 执行BACTIONTOOL命令
/// </summary>
[CommandMethod("ZWBACTIONTOOL")]
public void Execute()
```

**功能说明**: 处理BACTIONTOOL命令的主入口，支持多种选项

##### ExecuteLookupActionFlow() - 查寻动作创建流程
```csharp
/// <summary>
/// 执行查寻动作创建流程
/// </summary>
private void ExecuteLookupActionFlow()
```

**功能说明**: 处理查寻动作创建的完整流程

**流程步骤**:
1. 选择查寻参数
2. 选择受动作影响的对象
3. 设置查寻值
4. 配置动作属性
5. 创建查寻动作

##### SetupLookupValues() - 设置查寻值
```csharp
/// <summary>
/// 设置查寻值
/// </summary>
/// <param name="action">动作对象</param>
private void SetupLookupValues(LookupAction action)
```

**参数说明**:
- `action`: 目标动作对象

##### ShowCommandOptions() - 显示命令选项
```csharp
/// <summary>
/// 显示命令选项
/// </summary>
private void ShowCommandOptions()
```

**功能说明**: 在命令行显示可用的命令选项

**选项说明**:
- `L` - 查寻动作
- `S` - 标准动作  
- `M` - 管理选择集

### LookupTableCommand - 查寻表管理命令

#### 类定义
```csharp
/// <summary>
/// 查寻表管理命令
/// </summary>
[CommandMethod("ZWBLOOKUPTABLE")]
public class LookupTableCommand : ICommand
```

#### 主要方法

##### Execute() - 主执行方法
```csharp
/// <summary>
/// 执行查寻表管理命令
/// </summary>
[CommandMethod("ZWBLOOKUPTABLE")]
public void Execute()
```

##### OpenLookupTableManager() - 打开查寻表管理器
```csharp
/// <summary>
/// 打开查寻表管理器
/// </summary>
private void OpenLookupTableManager()
```

##### CreateLookupTable() - 创建查寻表
```csharp
/// <summary>
/// 创建新的查寻表
/// </summary>
/// <param name="tableData">查寻表数据</param>
/// <returns>创建结果</returns>
private CreateResult CreateLookupTable(LookupTableData tableData)
```

**参数说明**:
- `tableData`: 查寻表数据对象

**返回值**: `CreateResult` - 创建操作结果

##### EditLookupTable() - 编辑查寻表
```csharp
/// <summary>
/// 编辑现有查寻表
/// </summary>
/// <param name="tableId">查寻表ID</param>
private void EditLookupTable(ObjectId tableId)
```

**参数说明**:
- `tableId`: 要编辑的查寻表对象ID

### PropertiesCommand - 特性管理命令

#### 类定义
```csharp
/// <summary>
/// 特性管理命令
/// </summary>
[CommandMethod("ZWBPROPERTIES")]
public class PropertiesCommand : ICommand
```

#### 主要方法

##### Execute() - 主执行方法
```csharp
/// <summary>
/// 执行特性管理命令
/// </summary>
[CommandMethod("ZWBPROPERTIES")]
public void Execute()
```

##### OpenPropertiesManager() - 打开特性管理器
```csharp
/// <summary>
/// 打开特性管理器
/// </summary>
private void OpenPropertiesManager()
```

##### AddProperty() - 添加特性
```csharp
/// <summary>
/// 添加新特性
/// </summary>
/// <param name="parameterId">参数ID</param>
/// <param name="property">特性对象</param>
/// <returns>添加结果</returns>
private AddResult AddProperty(ObjectId parameterId, ParameterProperty property)
```

##### RemoveProperty() - 删除特性
```csharp
/// <summary>
/// 删除特性
/// </summary>
/// <param name="parameterId">参数ID</param>
/// <param name="propertyName">特性名称</param>
/// <returns>删除结果</returns>
private RemoveResult RemoveProperty(ObjectId parameterId, string propertyName)
```

---

## 数据模型API

### LookupTableData - 查寻表数据模型

#### 类定义
```csharp
/// <summary>
/// 查寻表数据结构
/// </summary>
[Serializable]
public class LookupTableData
{
    public string TableName { get; set; }
    public string ActionName { get; set; }
    public List<ParameterProperty> Properties { get; set; }
    public ObjectId? BlockId { get; set; }
    public ObjectId? ActionId { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime ModifyTime { get; set; }
}
```

#### 属性说明
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `TableName` | `string` | 查寻表名称 |
| `ActionName` | `string` | 关联的动作名称 |
| `Properties` | `List<ParameterProperty>` | 特性列表 |
| `BlockId` | `ObjectId?` | 关联的块ID |
| `ActionId` | `ObjectId?` | 关联的动作ID |
| `CreateTime` | `DateTime` | 创建时间 |
| `ModifyTime` | `DateTime` | 修改时间 |

#### 主要方法

##### Validate() - 验证查寻表数据
```csharp
/// <summary>
/// 验证查寻表数据
/// </summary>
/// <returns>验证结果</returns>
public ValidationResult Validate()
```

**返回值**: `ValidationResult` - 验证结果，包含错误和警告信息

##### ToXml() - 序列化为XML
```csharp
/// <summary>
/// 序列化为XML格式
/// </summary>
/// <returns>XML字符串</returns>
public string ToXml()
```

**返回值**: `string` - 序列化的XML字符串

##### FromXml() - 从XML反序列化
```csharp
/// <summary>
/// 从XML反序列化
/// </summary>
/// <param name="xml">XML字符串</param>
/// <returns>查寻表数据对象</returns>
public static LookupTableData FromXml(string xml)
```

**参数说明**:
- `xml`: XML格式的字符串

**返回值**: `LookupTableData` - 反序列化后的对象

##### Clone() - 克隆对象
```csharp
/// <summary>
/// 克隆查寻表数据
/// </summary>
/// <returns>克隆的对象</returns>
public LookupTableData Clone()
```

**返回值**: `LookupTableData` - 克隆的新对象

**使用示例**:
```csharp
// 创建查寻表数据
var lookupData = new LookupTableData
{
    TableName = "SizeTable",
    ActionName = "SizeAction",
    Properties = new List<ParameterProperty>
    {
        new ParameterProperty 
        { 
            PropertyName = "Size", 
            PropertyValue = "Small", 
            DisplayValue = "小号" 
        },
        new ParameterProperty 
        { 
            PropertyName = "Size", 
            PropertyValue = "Large", 
            DisplayValue = "大号" 
        }
    }
};

// 验证数据
var validation = lookupData.Validate();
if (validation.IsValid)
{
    // 保存到文件
    string xml = lookupData.ToXml();
    File.WriteAllText("lookuptable.xml", xml);
}
```

### ParameterProperty - 参数特性模型

#### 类定义
```csharp
/// <summary>
/// 参数特性数据模型
/// </summary>
[Serializable]
public class ParameterProperty
{
    public string PropertyName { get; set; }
    public string PropertyValue { get; set; }
    public string DisplayValue { get; set; }
    public PropertyType PropertyType { get; set; }
    public bool IsInput { get; set; }
    public bool IsLookup { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> Attributes { get; set; }
}
```

#### 属性说明
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `PropertyName` | `string` | 特性名称 |
| `PropertyValue` | `string` | 特性值 |
| `DisplayValue` | `string` | 显示值 |
| `PropertyType` | `PropertyType` | 特性类型 |
| `IsInput` | `bool` | 是否为输入特性 |
| `IsLookup` | `bool` | 是否为查寻特性 |
| `Description` | `string` | 特性描述 |
| `Attributes` | `Dictionary<string, object>` | 自定义属性 |

#### PropertyType 枚举
```csharp
/// <summary>
/// 特性类型枚举
/// </summary>
public enum PropertyType
{
    Text,           // 文本类型
    Number,         // 数值类型
    Boolean,        // 布尔类型
    DateTime,       // 日期时间类型
    Enum,           // 枚举类型
    Custom          // 自定义类型
}
```

#### 主要方法

##### Validate() - 验证特性
```csharp
/// <summary>
/// 验证特性数据
/// </summary>
/// <returns>验证结果</returns>
public ValidationResult Validate()
```

##### GetFormattedValue() - 获取格式化值
```csharp
/// <summary>
/// 获取格式化后的显示值
/// </summary>
/// <param name="format">格式字符串</param>
/// <returns>格式化后的值</returns>
public string GetFormattedValue(string format = null)
```

**参数说明**:
- `format`: 可选的格式字符串

**返回值**: `string` - 格式化后的显示值

##### ToKeyValuePair() - 转换为键值对
```csharp
/// <summary>
/// 转换为键值对
/// </summary>
/// <returns>键值对</returns>
public KeyValuePair<string, string> ToKeyValuePair()
```

**返回值**: `KeyValuePair<string, string>` - 键值对对象

**使用示例**:
```csharp
// 创建输入特性
var inputProperty = new ParameterProperty
{
    PropertyName = "Size",
    PropertyValue = "Medium",
    DisplayValue = "中号",
    PropertyType = PropertyType.Text,
    IsInput = true,
    IsLookup = false,
    Description = "尺寸输入特性"
};

// 创建查寻特性
var lookupProperty = new ParameterProperty
{
    PropertyName = "Price",
    PropertyValue = "100",
    DisplayValue = "￥100",
    PropertyType = PropertyType.Number,
    IsInput = false,
    IsLookup = true,
    Description = "价格查寻特性"
};

// 验证特性
var validation1 = inputProperty.Validate();
var validation2 = lookupProperty.Validate();

if (validation1.IsValid && validation2.IsValid)
{
    Console.WriteLine("特性验证通过");
}
```

### ParameterInfo - 参数信息模型

#### 类定义
```csharp
/// <summary>
/// 查寻参数信息
/// </summary>
public class ParameterInfo
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public Point3d Position { get; set; }
    public int GripCount { get; set; }
    public bool ShowOnPalette { get; set; }
    public ParameterType Type { get; set; }
    public ObjectId? ParameterId { get; set; }
}
```

#### ParameterType 枚举
```csharp
/// <summary>
/// 参数类型枚举
/// </summary>
public enum ParameterType
{
    Lookup,         // 查寻参数
    Linear,         // 线性参数
    Polar,          // 极坐标参数
    XY,             // XY参数
    Rotation,       // 旋转参数
    Flip,           // 翻转参数
    Visibility      // 可见性参数
}
```

---

## 服务接口API

### ILookupParameterService - 查寻参数服务接口

#### 接口定义
```csharp
/// <summary>
/// 查寻参数服务接口
/// </summary>
public interface ILookupParameterService
{
    // 参数管理
    Task<ParameterInfo> CreateParameterAsync(ParameterCreationData data);
    Task<bool> UpdateParameterAsync(ObjectId parameterId, ParameterInfo data);
    Task<bool> DeleteParameterAsync(ObjectId parameterId);
    Task<ParameterInfo> GetParameterAsync(ObjectId parameterId);
    Task<List<ParameterInfo>> GetParametersByBlockAsync(ObjectId blockId);
    
    // 参数验证
    Task<ValidationResult> ValidateParameterAsync(ParameterInfo parameter);
    Task<bool> IsParameterNameUniqueAsync(string name, ObjectId blockId);
    
    // 参数统计
    Task<ParameterStatistics> GetStatisticsAsync(ObjectId blockId);
}
```

#### ParameterCreationData 类
```csharp
/// <summary>
/// 参数创建数据
/// </summary>
public class ParameterCreationData
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public Point3d Position { get; set; }
    public int GripCount { get; set; }
    public bool ShowOnPalette { get; set; }
    public ObjectId BlockId { get; set; }
}
```

#### ParameterStatistics 类
```csharp
/// <summary>
/// 参数统计信息
/// </summary>
public class ParameterStatistics
{
    public int TotalParameters { get; set; }
    public int LookupParameters { get; set; }
    public int LinearParameters { get; set; }
    public int OtherParameters { get; set; }
    public int ActiveParameters { get; set; }
    public int InvalidParameters { get; set; }
}
```

#### 使用示例
```csharp
public class LookupParameterServiceImpl : ILookupParameterService
{
    private readonly Database _database;
    
    public async Task<ParameterInfo> CreateParameterAsync(ParameterCreationData data)
    {
        return await Task.Run(() =>
        {
            var creator = new LookupParameterCreator(_database);
            return creator.CreateParameter(data.Name, data.Label, data.Description);
        });
    }
    
    public async Task<bool> IsParameterNameUniqueAsync(string name, ObjectId blockId)
    {
        return await Task.Run(() =>
        {
            var manager = new ParameterPointManager(_database);
            return manager.FindParameterByName(name) == null;
        });
    }
}
```

### ILookupActionService - 查寻动作服务接口

#### 接口定义
```csharp
/// <summary>
/// 查寻动作服务接口
/// </summary>
public interface ILookupActionService
{
    // 动作管理
    Task<LookupAction> CreateActionAsync(ActionCreationData data);
    Task<bool> UpdateActionAsync(ObjectId actionId, LookupAction data);
    Task<bool> DeleteActionAsync(ObjectId actionId);
    Task<LookupAction> GetActionAsync(ObjectId actionId);
    Task<List<LookupAction>> GetActionsByParameterAsync(ObjectId parameterId);
    
    // 动作执行
    Task<bool> ExecuteActionAsync(ObjectId actionId, string lookupValue);
    Task<List<string>> GetAvailableValuesAsync(ObjectId actionId);
    
    // 动作关联
    Task<bool> AssociateParameterAsync(ObjectId actionId, ObjectId parameterId);
    Task<bool> AssociateObjectsAsync(ObjectId actionId, ObjectIdCollection objects);
}
```

#### ActionCreationData 类
```csharp
/// <summary>
/// 动作创建数据
/// </summary>
public class ActionCreationData
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public Point3d Position { get; set; }
    public ObjectId ParameterId { get; set; }
    public ObjectIdCollection TargetObjects { get; set; }
    public List<string> LookupValues { get; set; }
    public string DefaultValue { get; set; }
}
```

#### LookupAction 类
```csharp
/// <summary>
/// 查寻动作对象
/// </summary>
public class LookupAction
{
    public string ActionName { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public Point3d Position { get; set; }
    public ObjectId ParameterId { get; set; }
    public ObjectIdCollection AssociatedObjects { get; set; }
    public List<string> LookupValues { get; set; }
    public string DefaultValue { get; set; }
    public bool IsActive { get; set; }
    public ObjectId ActionId { get; set; }
}
```

### ILookupTableService - 查寻表服务接口

#### 接口定义
```csharp
/// <summary>
/// 查寻表服务接口
/// </summary>
public interface ILookupTableService
{
    // 查寻表管理
    Task<LookupTableData> CreateTableAsync(LookupTableCreationData data);
    Task<bool> UpdateTableAsync(ObjectId tableId, LookupTableData data);
    Task<bool> DeleteTableAsync(ObjectId tableId);
    Task<LookupTableData> GetTableAsync(ObjectId tableId);
    Task<List<LookupTableData>> GetTablesByBlockAsync(ObjectId blockId);
    
    // 查寻表操作
    Task<string> LookupValueAsync(ObjectId tableId, string inputValue);
    Task<List<LookupResult>> BatchLookupAsync(ObjectId tableId, List<string> inputValues);
    
    // 查寻表验证
    Task<ValidationResult> ValidateTableAsync(LookupTableData table);
    Task<bool> HasCircularReferenceAsync(ObjectId tableId);
}
```

#### LookupTableCreationData 类
```csharp
/// <summary>
/// 查寻表创建数据
/// </summary>
public class LookupTableCreationData
{
    public string TableName { get; set; }
    public string ActionName { get; set; }
    public List<ParameterProperty> Properties { get; set; }
    public ObjectId BlockId { get; set; }
    public ObjectId ActionId { get; set; }
}
```

#### LookupResult 类
```csharp
/// <summary>
/// 查寻结果
/// </summary>
public class LookupResult
{
    public string InputValue { get; set; }
    public string OutputValue { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public TimeSpan LookupTime { get; set; }
}
```

---

## UI组件API

### ParameterPropertiesDialog - 参数属性对话框

#### 类定义
```csharp
/// <summary>
/// 参数属性设置对话框
/// </summary>
public partial class ParameterPropertiesDialog : Form
```

#### 主要属性
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `ParameterName` | `string` | 参数名称 |
| `ParameterLabel` | `string` | 参数标签 |
| `ParameterDescription` | `string` | 参数描述 |
| `GripCount` | `int` | 夹点数量 |
| `ShowOnPalette` | `bool` | 是否在选项板显示 |
| `Position` | `Point3d` | 参数位置 |

#### 主要方法

##### ShowDialog() - 显示对话框
```csharp
/// <summary>
/// 显示参数属性对话框
/// </summary>
/// <param name="parameter">初始参数信息</param>
/// <returns>对话框结果</returns>
public DialogResult ShowDialog(ParameterInfo parameter)
```

**参数说明**:
- `parameter`: 初始参数信息，可为空

**返回值**: `DialogResult` - 对话框执行结果

##### ValidateInput() - 验证输入
```csharp
/// <summary>
/// 验证用户输入
/// </summary>
/// <returns>验证结果</returns>
public ValidationResult ValidateInput()
```

**返回值**: `ValidationResult` - 验证结果对象

##### GetParameterInfo() - 获取参数信息
```csharp
/// <summary>
/// 获取对话框中的参数信息
/// </summary>
/// <returns>参数信息对象</returns>
public ParameterInfo GetParameterInfo()
```

**返回值**: `ParameterInfo` - 包含用户输入的参数信息

#### 事件
| 事件名 | 说明 |
|--------|------|
| `ParameterChanged` | 参数信息改变时触发 |
| `ValidationError` | 验证错误时触发 |
| `OkClicked` | 确定按钮点击时触发 |

#### 使用示例
```csharp
// 创建并显示参数属性对话框
var dialog = new ParameterPropertiesDialog();

// 设置初始参数信息
var initialParameter = new ParameterInfo
{
    Name = "SizeParameter",
    Label = "尺寸参数",
    Description = "控制对象尺寸的参数",
    GripCount = 1,
    ShowOnPalette = true
};

var result = dialog.ShowDialog(initialParameter);

if (result == DialogResult.OK)
{
    // 获取用户输入的参数信息
    var parameterInfo = dialog.GetParameterInfo();
    
    // 创建参数
    var creator = new LookupParameterCreator(database);
    creator.CreateParameter(parameterInfo.Name, parameterInfo.Label, parameterInfo.Description);
}
```

### ActionPropertiesDialog - 动作属性对话框

#### 类定义
```csharp
/// <summary>
/// 动作属性设置对话框
/// </summary>
public partial class ActionPropertiesDialog : Form
```

#### 主要属性
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `ActionName` | `string` | 动作名称 |
| `ActionLabel` | `string` | 动作标签 |
| `DefaultValue` | `string` | 默认值 |
| `LookupValues` | `List<string>` | 查寻值列表 |
| `SelectedObjects` | `int` | 选中对象数量 |
| `EnableAction` | `bool` | 是否启用动作 |
| `ShowLabel` | `bool` | 是否显示标签 |

#### 主要方法

##### ShowDialog() - 显示对话框
```csharp
/// <summary>
/// 显示动作属性对话框
/// </summary>
/// <param name="action">初始动作信息</param>
/// <returns>对话框结果</returns>
public DialogResult ShowDialog(LookupAction action)
```

##### AddLookupValue() - 添加查寻值
```csharp
/// <summary>
/// 添加查寻值
/// </summary>
/// <param name="value">查寻值</param>
public void AddLookupValue(string value)
```

**参数说明**:
- `value`: 要添加的查寻值

##### RemoveLookupValue() - 移除查寻值
```csharp
/// <summary>
/// 移除查寻值
/// </summary>
/// <param name="value">要移除的查寻值</param>
public void RemoveLookupValue(string value)
```

#### 使用示例
```csharp
// 创建动作属性对话框
var dialog = new ActionPropertiesDialog();

// 设置初始动作信息
var initialAction = new LookupAction
{
    ActionName = "SizeAction",
    Label = "尺寸动作",
    DefaultValue = "Medium",
    LookupValues = new List<string> { "Small", "Medium", "Large" },
    EnableAction = true,
    ShowLabel = true
};

var result = dialog.ShowDialog(initialAction);

if (result == DialogResult.OK)
{
    // 获取动作信息并创建动作
    var actionCreator = new LookupActionCreator(database);
    actionCreator.CreateLookupAction(dialog.ActionName, dialog.ActionLabel, "动作描述");
}
```

### LookupTableManagerDialog - 查寻表管理器对话框

#### 类定义
```csharp
/// <summary>
/// 查寻表管理器对话框
/// </summary>
public partial class LookupTableManagerDialog : Form
```

#### 主要属性
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `CurrentTable` | `LookupTableData` | 当前查寻表 |
| `Tables` | `List<LookupTableData>` | 查寻表列表 |
| `SelectedTableIndex` | `int` | 选中的表索引 |

#### 主要方法

##### LoadTables() - 加载查寻表
```csharp
/// <summary>
/// 加载所有查寻表
/// </summary>
public void LoadTables()
```

##### SaveTable() - 保存查寻表
```csharp
/// <summary>
/// 保存当前查寻表
/// </summary>
/// <returns>保存结果</returns>
public bool SaveTable()
```

##### CreateNewTable() - 创建新表
```csharp
/// <summary>
/// 创建新的查寻表
/// </summary>
public void CreateNewTable()
```

##### DeleteTable() - 删除查寻表
```csharp
/// <summary>
/// 删除当前查寻表
/// </summary>
/// <returns>删除结果</returns>
public bool DeleteTable()
```

#### 事件
| 事件名 | 说明 |
|--------|------|
| `TableSelected` | 选择查寻表时触发 |
| `TableModified` | 查寻表修改时触发 |
| `TableSaved` | 查寻表保存时触发 |

### PropertiesManagerDialog - 特性管理器对话框

#### 类定义
```csharp
/// <summary>
/// 特性管理器对话框
/// </summary>
public partial class PropertiesManagerDialog : Form
```

#### 主要属性
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `CurrentParameter` | `ParameterInfo` | 当前参数 |
| `Properties` | `List<ParameterProperty>` | 特性列表 |
| `PropertyTypes` | `List<PropertyType>` | 可用特性类型 |

#### 主要方法

##### LoadProperties() - 加载特性
```csharp
/// <summary>
/// 加载指定参数的特性
/// </summary>
/// <param name="parameterId">参数ID</param>
public void LoadProperties(ObjectId parameterId)
```

##### AddProperty() - 添加特性
```csharp
/// <summary>
/// 添加新特性
/// </summary>
/// <param name="property">特性对象</param>
public void AddProperty(ParameterProperty property)
```

##### UpdateProperty() - 更新特性
```csharp
/// <summary>
/// 更新特性
/// </summary>
/// <param name="index">特性索引</param>
/// <param name="property">新特性对象</param>
public void UpdateProperty(int index, ParameterProperty property)
```

##### RemoveProperty() - 移除特性
```csharp
/// <summary>
/// 移除特性
/// </summary>
/// <param name="index">特性索引</param>
public void RemoveProperty(int index)
```

#### 使用示例
```csharp
// 创建特性管理器对话框
var dialog = new PropertiesManagerDialog();

// 加载参数特性
var parameterId = /* 获取参数ID */;
dialog.LoadProperties(parameterId);

// 添加新特性
var newProperty = new ParameterProperty
{
    PropertyName = "Color",
    PropertyValue = "Red",
    DisplayValue = "红色",
    PropertyType = PropertyType.Text,
    IsInput = true,
    IsLookup = false,
    Description = "颜色特性"
};

dialog.AddProperty(newProperty);

// 显示对话框
dialog.ShowDialog();
```

---

## 扩展开发API

### IPluginExtension - 插件扩展接口

#### 接口定义
```csharp
/// <summary>
/// 插件扩展接口
/// </summary>
public interface IPluginExtension
{
    // 扩展信息
    string ExtensionName { get; }
    string Version { get; }
    string Description { get; }
    string Author { get; }
    
    // 生命周期
    Task<bool> InitializeAsync(IPluginContext context);
    Task<bool> ShutdownAsync();
    
    // 功能扩展
    IEnumerable<ICommand> GetCommands();
    IEnumerable<IService> GetServices();
    IEnumerable<IUIComponent> GetUIComponents();
    
    // 配置
    Task<ExtensionConfiguration> GetConfigurationAsync();
    Task<bool> SetConfigurationAsync(ExtensionConfiguration configuration);
}
```

#### IPluginContext 接口
```csharp
/// <summary>
/// 插件上下文接口
/// </summary>
public interface IPluginContext
{
    // 核心服务
    ICommandService CommandService { get; }
    IUIService UIService { get; }
    IConfigurationService ConfigurationService { get; }
    ILoggingService LoggingService { get; }
    
    // CAD上下文
    Application Application { get; }
    Document ActiveDocument { get; }
    Editor Editor { get; }
    Database Database { get; }
    
    // 插件服务
    IServiceProvider ServiceProvider { get; }
}
```

### 创建自定义命令扩展

#### 自定义命令类
```csharp
/// <summary>
/// 自定义查寻命令示例
/// </summary>
[CommandMethod("MYCUSTOMLOOKUP")]
public class MyCustomLookupCommand : ICommand
{
    private readonly IPluginContext _context;
    
    public MyCustomLookupCommand(IPluginContext context)
    {
        _context = context;
    }
    
    public string CommandName => "MYCUSTOMLOOKUP";
    public string DisplayName => "我的自定义查寻";
    public string Description => "自定义查寻功能";
    
    public void Execute()
    {
        try
        {
            // 获取当前文档
            var doc = _context.ActiveDocument;
            if (doc == null) return;
            
            // 执行自定义逻辑
            var result = ExecuteCustomLookup(doc);
            
            // 显示结果
            _context.Editor.WriteMessage($"查寻完成: {result}");
        }
        catch (Exception ex)
        {
            _context.LoggingService.LogError($"执行自定义命令失败: {ex.Message}");
        }
    }
    
    private string ExecuteCustomLookup(Document doc)
    {
        // 实现自定义查寻逻辑
        return "自定义查寻结果";
    }
    
    public void Terminate()
    {
        // 清理资源
    }
}
```

#### 扩展插件实现
```csharp
/// <summary>
/// 自定义扩展插件示例
/// </summary>
public class MyCustomExtension : IPluginExtension
{
    public string ExtensionName => "MyCustomExtension";
    public string Version => "1.0.0";
    public string Description => "我的自定义扩展";
    public string Author => "开发者姓名";
    
    private IPluginContext _context;
    
    public async Task<bool> InitializeAsync(IPluginContext context)
    {
        _context = context;
        
        // 注册自定义命令
        var commands = GetCommands();
        foreach (var command in commands)
        {
            await _context.CommandService.RegisterCommandAsync(command);
        }
        
        // 注册服务
        var services = GetServices();
        foreach (var service in services)
        {
            _context.ServiceProvider.RegisterService(service);
        }
        
        return true;
    }
    
    public async Task<bool> ShutdownAsync()
    {
        // 清理资源
        return await Task.FromResult(true);
    }
    
    public IEnumerable<ICommand> GetCommands()
    {
        yield return new MyCustomLookupCommand(_context);
    }
    
    public IEnumerable<IService> GetServices()
    {
        yield return new MyCustomService();
    }
    
    public IEnumerable<IUIComponent> GetUIComponents()
    {
        yield return new MyCustomUIComponent();
    }
    
    public async Task<ExtensionConfiguration> GetConfigurationAsync()
    {
        return await _context.ConfigurationService.GetConfigurationAsync<ExtensionConfiguration>();
    }
    
    public async Task<bool> SetConfigurationAsync(ExtensionConfiguration configuration)
    {
        return await _context.ConfigurationService.SetConfigurationAsync(configuration);
    }
}
```

### 自定义服务实现

#### 服务接口定义
```csharp
/// <summary>
/// 自定义查寻服务接口
/// </summary>
public interface IMyCustomLookupService
{
    Task<string> ExecuteLookupAsync(string input);
    Task<List<string>> GetSuggestionsAsync(string input);
    Task<bool> ValidateInputAsync(string input);
}
```

#### 服务实现
```csharp
/// <summary>
/// 自定义查寻服务实现
/// </summary>
public class MyCustomLookupService : IMyCustomLookupService
{
    private readonly IPluginContext _context;
    private readonly Dictionary<string, string> _lookupTable;
    
    public MyCustomLookupService(IPluginContext context)
    {
        _context = context;
        _lookupTable = new Dictionary<string, string>
        {
            { "small", "小号" },
            { "medium", "中号" },
            { "large", "大号" }
        };
    }
    
    public async Task<string> ExecuteLookupAsync(string input)
    {
        return await Task.Run(() =>
        {
            if (_lookupTable.ContainsKey(input.ToLower()))
            {
                return _lookupTable[input.ToLower()];
            }
            
            return "未知值";
        });
    }
    
    public async Task<List<string>> GetSuggestionsAsync(string input)
    {
        return await Task.Run(() =>
        {
            return _lookupTable.Keys
                .Where(key => key.StartsWith(input.ToLower()))
                .ToList();
        });
    }
    
    public async Task<bool> ValidateInputAsync(string input)
    {
        return await Task.Run(() =>
        {
            return !string.IsNullOrWhiteSpace(input) && 
                   input.Length <= 50 &&
                   input.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        });
    }
}
```

### 自定义UI组件实现

#### UI组件接口
```csharp
/// <summary>
/// 自定义UI组件接口
/// </summary>
public interface IMyCustomUIComponent : IUIComponent
{
    string ComponentName { get; }
    Control CreateControl();
    void OnDataChanged(object data);
}
```

#### UI组件实现
```csharp
/// <summary>
/// 自定义UI组件实现
/// </summary>
public class MyCustomUIComponent : IMyCustomUIComponent
{
    public string ComponentName => "MyCustomLookupControl";
    
    public Control CreateControl()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.LightBlue
        };
        
        var textBox = new TextBox
        {
            Name = "lookupInput",
            Dock = DockStyle.Top,
            PlaceholderText = "输入查寻值..."
        };
        
        var button = new Button
        {
            Name = "lookupButton",
            Text = "查寻",
            Dock = DockStyle.Top,
            Height = 30
        };
        
        var resultLabel = new Label
        {
            Name = "resultLabel",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5)
        };
        
        // 绑定事件
        button.Click += (sender, e) =>
        {
            var input = textBox.Text;
            var service = GetLookupService();
            var result = service.ExecuteLookup(input);
            resultLabel.Text = $"结果: {result}";
        };
        
        panel.Controls.Add(resultLabel);
        panel.Controls.Add(button);
        panel.Controls.Add(textBox);
        
        return panel;
    }
    
    public void OnDataChanged(object data)
    {
        // 处理数据变化
        if (data is string lookupValue)
        {
            var service = GetLookupService();
            var result = service.ExecuteLookup(lookupValue);
            // 更新UI显示
        }
    }
    
    private IMyCustomLookupService GetLookupService()
    {
        // 从服务提供程序获取服务实例
        return /* 获取服务实例 */;
    }
}
```

---

## 最佳实践

### API使用规范

#### 1. 异步编程模式
```csharp
// ✅ 推荐：使用异步方法
public async Task<ParameterInfo> CreateParameterAsync(ParameterCreationData data)
{
    return await Task.Run(() =>
    {
        var creator = new LookupParameterCreator(_database);
        return creator.CreateParameter(data.Name, data.Label, data.Description);
    });
}

// ❌ 避免：使用同步方法进行长时间操作
public ParameterInfo CreateParameter(ParameterCreationData data)
{
    // 长时间操作会阻塞UI线程
    var creator = new LookupParameterCreator(_database);
    return creator.CreateParameter(data.Name, data.Label, data.Description);
}
```

#### 2. 错误处理
```csharp
// ✅ 推荐：完整的错误处理
public async Task<bool> CreateParameterAsync(ParameterCreationData data)
{
    try
    {
        // 参数验证
        var validation = await ValidateParameterDataAsync(data);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        // 创建参数
        var creator = new LookupParameterCreator(_database);
        var parameter = await creator.CreateParameterAsync(data);
        
        // 记录成功日志
        PluginEntry.Log($"参数创建成功: {parameter.Name}");
        
        return true;
    }
    catch (ValidationException ex)
    {
        PluginEntry.Log($"参数验证失败: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        PluginEntry.Log($"参数创建失败: {ex.Message}");
        throw new PluginException("参数创建失败，请重试", ex);
    }
}
```

#### 3. 资源管理
```csharp
// ✅ 推荐：正确的资源管理
public void ProcessParameters()
{
    var creator = new LookupParameterCreator(_database);
    
    try
    {
        // 使用using语句确保资源释放
        using (var transaction = _database.TransactionManager.StartTransaction())
        {
            // 执行操作
            var parameter = creator.CreateParameter("Test", "测试", "测试参数");
            
            transaction.Commit();
        }
    }
    finally
    {
        // 清理资源
        creator?.Dispose();
    }
}

// ❌ 避免：资源泄漏
public void ProcessParameters()
{
    var creator = new LookupParameterCreator(_database);
    var transaction = _database.TransactionManager.StartTransaction();
    
    // 如果出现异常，transaction不会被正确释放
    var parameter = creator.CreateParameter("Test", "测试", "测试参数");
    transaction.Commit();
    // 缺少资源清理
}
```

#### 4. 配置管理
```csharp
// ✅ 推荐：类型安全的配置管理
public class PluginConfiguration
{
    public int MaxParameters { get; set; } = 50;
    public bool EnableLogging { get; set; } = true;
    public string LogLevel { get; set; } = "Info";
    public List<string> SupportedLanguages { get; set; } = new List<string> { "zh-CN", "en-US" };
}

public class ConfigurationService
{
    private readonly string _configPath;
    
    public async Task<PluginConfiguration> LoadConfigurationAsync()
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(_configPath))
            {
                return new PluginConfiguration(); // 返回默认配置
            }
            
            try
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<PluginConfiguration>(json);
            }
            catch (Exception ex)
            {
                PluginEntry.Log($"配置加载失败，使用默认配置: {ex.Message}");
                return new PluginConfiguration();
            }
        });
    }
    
    public async Task SaveConfigurationAsync(PluginConfiguration config)
    {
        await Task.Run(() =>
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            File.WriteAllText(_configPath, json);
        });
    }
}
```

#### 5. 线程安全
```csharp
// ✅ 推荐：线程安全的单例模式
public sealed class ParameterManager
{
    private static readonly Lazy<ParameterManager> _instance = 
        new Lazy<ParameterManager>(() => new ParameterManager());
    
    public static ParameterManager Instance => _instance.Value;
    
    private readonly ConcurrentDictionary<ObjectId, ParameterInfo> _parameters;
    private readonly object _lockObject = new object();
    
    private ParameterManager()
    {
        _parameters = new ConcurrentDictionary<ObjectId, ParameterInfo>();
    }
    
    public void AddParameter(ObjectId id, ParameterInfo parameter)
    {
        _parameters.AddOrUpdate(id, parameter, (key, oldValue) => parameter);
    }
    
    public ParameterInfo GetParameter(ObjectId id)
    {
        _parameters.TryGetValue(id, out var parameter);
        return parameter;
    }
    
    public void RemoveParameter(ObjectId id)
    {
        _parameters.TryRemove(id, out _);
    }
}
```

### 性能优化建议

#### 1. 缓存策略
```csharp
// ✅ 推荐：使用缓存提高性能
public class LookupTableCache
{
    private readonly MemoryCache _cache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    
    public LookupTableCache()
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100 // 限制缓存项数量
        });
    }
    
    public async Task<LookupTableData> GetLookupTableAsync(ObjectId tableId)
    {
        var cacheKey = $"LookupTable_{tableId.Handle}";
        
        if (_cache.TryGetValue(cacheKey, out LookupTableData cachedTable))
        {
            return cachedTable;
        }
        
        // 从数据库加载
        var table = await LoadLookupTableFromDatabaseAsync(tableId);
        
        // 缓存结果
        _cache.Set(cacheKey, table, _defaultExpiration);
        
        return table;
    }
    
    public void InvalidateCache(ObjectId tableId)
    {
        var cacheKey = $"LookupTable_{tableId.Handle}";
        _cache.Remove(cacheKey);
    }
}
```

#### 2. 批处理操作
```csharp
// ✅ 推荐：批处理操作提高效率
public class BatchOperationService
{
    public async Task<BatchResult> CreateParametersBatchAsync(List<ParameterCreationData> parameters)
    {
        var results = new List<ParameterCreationResult>();
        var errors = new List<string>();
        
        await Task.Run(() =>
        {
            using (var transaction = _database.TransactionManager.StartTransaction())
            {
                try
                {
                    var creator = new LookupParameterCreator(_database);
                    
                    // 并行处理（注意CAD API的线程要求）
                    var parallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    };
                    
                    Parallel.ForEach(parameters, parallelOptions, (param, state) =>
                    {
                        try
                        {
                            // 在CAD环境中需要串行处理
                            lock (_database)
                            {
                                var parameter = creator.CreateParameter(param.Name, param.Label, param.Description);
                                results.Add(new ParameterCreationResult 
                                { 
                                    Success = true, 
                                    Parameter = parameter 
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (errors)
                            {
                                errors.Add($"创建参数 {param.Name} 失败: {ex.Message}");
                            }
                        }
                    });
                    
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Abort();
                    errors.Add($"批处理操作失败: {ex.Message}");
                }
            }
        });
        
        return new BatchResult
        {
            SuccessCount = results.Count,
            ErrorCount = errors.Count,
            Errors = errors,
            Results = results
        };
    }
}
```

### 测试指南

#### 1. 单元测试示例
```csharp
[TestFixture]
public class LookupParameterCreatorTests
{
    private Database _database;
    private LookupParameterCreator _creator;
    
    [SetUp]
    public void Setup()
    {
        // 创建测试数据库
        _database = new Database();
        _creator = new LookupParameterCreator(_database);
    }
    
    [Test]
    public void CreateParameter_ValidData_ReturnsValidParameter()
    {
        // Arrange
        var parameterData = new ParameterCreationData
        {
            Name = "TestParameter",
            Label = "测试参数",
            Description = "测试描述"
        };
        
        // Act
        var result = _creator.CreateParameter(parameterData.Name, parameterData.Label, parameterData.Description);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(parameterData.Name, result.Name);
        Assert.AreEqual(parameterData.Label, result.Label);
        Assert.AreEqual(parameterData.Description, result.Description);
    }
    
    [Test]
    public void CreateParameter_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var emptyName = "";
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            _creator.CreateParameter(emptyName, "Label", "Description");
        });
    }
    
    [Test]
    public void CreateParameter_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var name = "DuplicateParameter";
        _creator.CreateParameter(name, "Label1", "Description1");
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _creator.CreateParameter(name, "Label2", "Description2");
        });
    }
    
    [TearDown]
    public void TearDown()
    {
        _database?.Dispose();
    }
}
```

#### 2. 集成测试示例
```csharp
[TestFixture]
public class IntegrationTests
{
    private Application _application;
    private Document _document;
    
    [SetUp]
    public async Task Setup()
    {
        // 启动测试CAD应用
        _application = new Application();
        
        // 创建测试文档
        _document = _application.DocumentManager.Add("Test.dwg");
        _document.Database.SaveAs("Test.dwg", true, DwgVersion.AC1015, _application.SystemVariable);
    }
    
    [Test]
    public async Task CreateLookupParameter_Integration_Success()
    {
        // Arrange
        var command = new BParameterCommand();
        var parameterData = new ParameterCreationData
        {
            Name = "IntegrationParameter",
            Label = "集成测试参数",
            Description = "用于集成测试的参数"
        };
        
        // Act
        var result = await command.CreateParameterAsync(parameterData);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Parameter);
        Assert.AreEqual(parameterData.Name, result.Parameter.Name);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        // 清理测试文档
        if (_document != null)
        {
            _document.CloseAndDiscard();
        }
        
        // 关闭CAD应用
        _application?.Quit();
    }
}
```

---

**API版本：** 1.0.0  
**最后更新：** 2024年12月  
**维护者：** 中望CAD动态块查寻插件开发团队