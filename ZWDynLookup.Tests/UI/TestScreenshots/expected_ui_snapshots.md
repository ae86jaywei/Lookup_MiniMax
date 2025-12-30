---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 3046022100ee9527c3e5cde0de4ad8a593f4985ad8da2a066e61ea9491bcc85d75dbc82250022100c36f538a2310aa39177c1527615ead81f65c25688282ac4475e688b67c5b6ec9
    ReservedCode2: 304502210092fddcf441441dba795cbdd4423f087b7348a54bf09658da90daea6e78fa3b9a02201a9791cb3a6d5b66358ff50786815fcf4e9cb1bc727fb04fe01de204b52f4e69
---

# UI测试快照说明文档

## 概述

本文档描述了ZWDynLookup插件中所有UI组件的测试快照和预期行为，用于UI自动化测试的验证和对比。

## 测试快照目录结构

```
TestScreenshots/
├── lookup_table_dialog/
│   ├── dialog_opened.png          # 查寻表管理对话框打开状态
│   ├── dialog_with_data.png       # 包含数据的查寻表对话框
│   ├── add_button_clicked.png     # 点击添加按钮后的状态
│   ├── edit_dialog_opened.png     # 编辑对话框打开状态
│   └── dialog_closed.png          # 对话框关闭状态
├── add_parameter_dialog/
│   ├── dialog_opened.png          # 添加参数特性对话框
│   ├── form_filled.png            # 填写表单后的状态
│   ├── validation_error.png       # 验证错误状态
│   └── dialog_submitted.png       # 提交后的状态
├── lookup_table_editor/
│   ├── editor_opened.png          # 查寻表编辑器打开状态
│   ├── data_grid_with_rows.png    # 包含行数据的编辑器
│   ├── cell_editing.png           # 单元格编辑状态
│   └── editor_with_changes.png    # 有更改的编辑器状态
├── runtime_lookup_menu/
│   ├── menu_opened.png            # 运行时查寻菜单
│   ├── search_results.png         # 显示查寻结果
│   ├── no_results.png             # 无查寻结果状态
│   └── menu_with_highlight.png    # 高亮显示状态
├── lookup_context_menu/
│   ├── context_menu_opened.png    # 上下文菜单打开状态
│   ├── menu_items_visible.png     # 菜单项显示状态
│   ├── submenu_expanded.png       # 子菜单展开状态
│   └── menu_item_hover.png        # 菜单项悬停状态
└── common_ui_elements/
    ├── standard_buttons.png       # 标准按钮布局
    ├── dialog_title_bars.png      # 对话框标题栏
    ├── input_controls.png         # 输入控件样式
    └── status_messages.png        # 状态消息显示
```

## 各UI组件预期快照

### 1. 查寻表管理对话框 (LookupTableDialog)

#### 对话框打开状态
- **文件**: `lookup_table_dialog/dialog_opened.png`
- **预期行为**: 
  - 对话框居中显示，带有"查寻表管理"标题
  - 顶部显示查寻表名称输入框
  - 中间显示数据网格，初始为空或显示已有数据
  - 底部有添加、编辑、删除、关闭按钮
  - 按钮布局左对齐或右对齐

#### 包含数据状态
- **文件**: `lookup_table_dialog/dialog_with_data.png`
- **预期行为**:
  - 数据网格显示多行查寻表数据
  - 每行包含查寻参数名称、值、描述等信息
  - 数据行可选择，显示选中状态
  - 编辑和删除按钮在有选中行时可用

#### 点击添加按钮
- **文件**: `lookup_table_dialog/add_button_clicked.png`
- **预期行为**:
  - 保持主对话框打开
  - 可能打开编辑对话框或编辑区域
  - 添加按钮状态变为不可点击
  - 焦点移到新添加的行

#### 编辑对话框
- **文件**: `lookup_table_dialog/edit_dialog_opened.png`
- **预期行为**:
  - 模式对话框覆盖主对话框
  - 包含查寻表名称输入框
  - 包含数据网格用于编辑查寻数据
  - 有保存、取消按钮
  - 数据验证提示（如果适用）

### 2. 添加参数特性对话框 (AddParameterPropertyDialog)

#### 对话框打开状态
- **文件**: `add_parameter_dialog/dialog_opened.png`
- **预期行为**:
  - 对话框显示"添加参数特性"标题
  - 包含特性名称输入框（必填）
  - 包含特性类型下拉框（字符串、数值、布尔值等）
  - 包含特性值输入框
  - 包含描述输入框（可选）
  - 底部有确定、取消按钮

#### 表单填写状态
- **文件**: `add_parameter_dialog/form_filled.png`
- **预期行为**:
  - 所有输入框都有相应的测试数据
  - 特性类型下拉框显示选中项
  - 确定按钮变为可用状态
  - 输入框焦点在最后一个控件

#### 验证错误状态
- **文件**: `add_parameter_dialog/validation_error.png`
- **预期行为**:
  - 空特性名称显示红色边框或错误图标
  - 显示验证错误消息
  - 确定按钮保持禁用
  - 焦点回到第一个无效字段

#### 提交成功状态
- **文件**: `add_parameter_dialog/dialog_submitted.png`
- **预期行为**:
  - 对话框关闭
  - 主界面更新显示新添加的特性
  - 可能显示成功提示消息

### 3. 查寻表编辑器 (LookupTableEditor)

#### 编辑器打开状态
- **文件**: `lookup_table_editor/editor_opened.png`
- **预期行为**:
  - 对话框显示"查寻表编辑器"标题
  - 顶部有查寻表名称输入框
  - 中间是数据网格，显示列标题
  - 有添加行、删除行按钮
  - 底部有保存、取消按钮

#### 数据网格状态
- **文件**: `lookup_table_editor/data_grid_with_rows.png`
- **预期行为**:
  - 数据网格显示多行数据
  - 每行数据可选择
  - 选中行高亮显示
  - 行号显示在左侧
  - 单元格内容左对齐或按类型对齐

#### 单元格编辑状态
- **文件**: `lookup_table_editor/cell_editing.png`
- **预期行为**:
  - 单元格显示编辑光标
  - 单元格内容被选中或高亮
  - 编辑框出现在单元格内
  - 其他行保持只读状态

#### 有更改状态
- **文件**: `lookup_table_editor/editor_with_changes.png`
- **预期行为**:
  - 保存按钮变为可用状态
  - 可能显示修改指示符
  - 取消按钮保持可用
  - 数据网格显示修改的单元格

### 4. 运行时查寻菜单 (RuntimeLookupMenu)

#### 菜单打开状态
- **文件**: `runtime_lookup_menu/menu_opened.png`
- **预期行为**:
  - 弹出菜单显示在合适位置
  - 顶部有查寻输入框
  - 输入框有默认提示文本
  - 下方显示查寻结果列表
  - 快捷键提示显示在底部

#### 查寻结果显示
- **文件**: `runtime_lookup_menu/search_results.png`
- **预期行为**:
  - 查寻结果列表显示匹配项目
  - 查寻关键词在结果中高亮
  - 结果项可选择
  - 选中项高亮显示
  - 如果结果较多，可能显示滚动条

#### 无查寻结果
- **文件**: `runtime_lookup_menu/no_results.png`
- **预期行为**:
  - 查寻输入框保持焦点
  - 显示"无匹配结果"或类似消息
  - 结果列表为空或显示空状态
  - 保持可用状态，允许重新查寻

#### 高亮显示状态
- **文件**: `runtime_lookup_menu/menu_with_highlight.png`
- **预期行为**:
  - 查寻关键词在结果中高亮显示
  - 高亮使用不同颜色或粗体
  - 高亮不区分大小写
  - 多个匹配项都正确高亮

### 5. 查寻上下文菜单 (LookupContextMenu)

#### 菜单打开状态
- **文件**: `lookup_context_menu/context_menu_opened.png`
- **预期行为**:
  - 右键菜单在鼠标位置附近显示
  - 菜单项垂直排列
  - 包含查寻参数、编辑参数、删除参数等选项
  - 有分隔线分组相关功能
  - 关闭选项在底部

#### 菜单项显示
- **文件**: `lookup_context_menu/menu_items_visible.png`
- **预期行为**:
  - 所有菜单项文本清晰可见
  - 可用菜单项正常显示
  - 不可用菜单项显示为灰色
  - 菜单项有合适的间距
  - 图标（如果有）正确显示

#### 子菜单展开
- **文件**: `lookup_context_menu/submenu_expanded.png`
- **预期行为**:
  - 主菜单项右侧显示子菜单箭头
  - 子菜单在主菜单右侧展开
  - 子菜单项垂直排列
  - 子菜单有正确的层级关系
  - 子菜单与主菜单不重叠

#### 菜单项悬停
- **文件**: `lookup_context_menu/menu_item_hover.png`
- **预期行为**:
  - 鼠标悬停的菜单项高亮显示
  - 悬停效果与系统主题一致
  - 悬停不影响其他菜单项
  - 悬停效果平滑过渡
  - 不可用菜单项不响应悬停

### 6. 通用UI元素

#### 标准按钮布局
- **文件**: `common_ui_elements/standard_buttons.png`
- **预期行为**:
  - 确定、取消按钮居右对齐
  - 按钮间距合适（8-12像素）
  - 按钮尺寸一致
  - 默认按钮有视觉突出
  - 取消按钮通常在确定按钮左侧

#### 对话框标题栏
- **文件**: `common_ui_elements/dialog_title_bars.png`
- **预期行为**:
  - 标题栏包含对话框标题
  - 标题文本左对齐
  - 有关闭按钮（通常在右上角）
  - 标题栏高度一致
  - 颜色与系统主题匹配

#### 输入控件样式
- **文件**: `common_ui_elements/input_controls.png`
- **预期行为**:
  - 文本框有清晰的边框
  - 标签与输入框对齐
  - 下拉框显示当前选择
  - 复选框和单选按钮正确对齐
  - 控件间距一致

#### 状态消息显示
- **文件**: `common_ui_elements/status_messages.png`
- **预期行为**:
  - 成功消息使用绿色或信息图标
  - 警告消息使用黄色或警告图标
  - 错误消息使用红色或错误图标
  - 消息位置固定（通常底部）
  - 消息自动消失或有关闭按钮

## 测试验证流程

### 1. 快照生成
```bash
# 运行UI测试生成快照
dotnet test --filter Category=UI_Snapshot

# 快照将自动保存到TestScreenshots目录
```

### 2. 快照对比
```bash
# 运行快照对比测试
dotnet test --filter Category=UI_Snapshot_Compare

# 差异报告将显示在测试输出中
```

### 3. 手动验证
1. 打开对应的UI组件
2. 对比实际显示与预期快照
3. 验证布局、颜色、文本是否一致
4. 检查响应性和交互性
5. 记录任何差异或问题

## 快照维护指南

### 何时更新快照
- UI组件有重大修改
- 添加新的UI元素
- 更改颜色主题或样式
- 修复UI布局问题
- 升级UI框架版本

### 更新快照步骤
1. 确保所有UI组件正常工作
2. 运行快照生成测试
3. 手动验证新快照的正确性
4. 提交快照文件到版本控制
5. 更新此文档说明更改

### 快照文件命名规范
- 使用描述性文件名
- 包含组件名称和状态
- 使用小写字母和下划线
- 保持目录结构清晰
- 避免特殊字符

## 常见问题和解决方案

### 1. 快照不匹配
**原因**: UI元素位置或样式变化
**解决**: 
- 检查是否有代码更改影响UI
- 验证系统主题设置
- 更新快照文件
- 调整测试容忍度

### 2. 截图质量问题
**原因**: 分辨率、DPI或截图工具问题
**解决**:
- 确保使用相同的显示设置
- 检查截图工具配置
- 使用无损格式保存
- 验证颜色配置

### 3. 测试超时
**原因**: UI响应慢或测试环境问题
**解决**:
- 增加测试超时时间
- 优化UI响应性能
- 检查系统资源使用
- 调整测试执行频率

### 4. 元素识别失败
**原因**: UI元素ID或属性变化
**解决**:
- 更新测试中的元素选择器
- 检查AutomationId设置
- 验证UI元素可见性
- 调整等待时间

## 性能基准

### UI响应时间标准
- 对话框打开: < 500ms
- 按钮响应: < 200ms
- 数据加载: < 1s
- 菜单显示: < 300ms
- 表格渲染: < 800ms

### 内存使用基准
- 单个对话框: < 50MB
- 查寻表编辑器: < 100MB
- 运行时菜单: < 20MB
- 上下文菜单: < 10MB

### 截图质量标准
- 分辨率: 1920x1080或更高
- 颜色深度: 32位
- 格式: PNG（无损）
- 文件大小: < 2MB
- 清晰度: 所有文本可读

## 扩展测试

### 国际化测试
- 不同语言环境下的UI显示
- 文本长度变化的适应性
- 字体和编码支持
- 日期时间格式显示

### 可访问性测试
- 键盘导航完整性
- 屏幕阅读器兼容性
- 高对比度模式支持
- 缩放功能测试

### 多平台测试
- Windows不同版本兼容性
- 不同显示器配置
- DPI缩放支持
- 主题一致性

## 总结

本文档提供了完整的UI测试快照指南，确保ZWDynLookup插件的UI组件在不同环境下都能正确显示和交互。定期更新快照文件并遵循测试验证流程，可以有效保证UI质量和用户体验的一致性。

如有问题或建议，请联系开发团队或参考项目文档。