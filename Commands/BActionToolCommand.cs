using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// BACTIONTOOL命令 - 创建查寻动作
    /// 支持L选项（查寻动作）的完整实现
    /// </summary>
    public class BActionToolCommand
    {
        private List<ObjectId> _selectedParameters = new List<ObjectId>();
        private List<ObjectId> _selectionSet = new List<ObjectId>();
        private string _actionName = "LookupAction";
        private string _description = "查寻动作描述";
        private List<string> _lookupValues = new List<string>();
        private ActionSelectionManager _selectionManager = new ActionSelectionManager();
        private SelectionSetManager _selectionSetManager = new SelectionSetManager();
        private LookupActionCreator _actionCreator = new LookupActionCreator();
        private bool _isLookupMode = false;

        /// <summary>
        /// 执行BACTIONTOOL命令
        /// </summary>
        [CommandMethod("ZWBACTIONTOOL")]
        public void Execute()
        {
            try
            {
                PluginEntry.Log("开始执行BACTIONTOOL命令");

                var editor = PluginEntry.GetEditor();
                if (editor == null)
                {
                    MessageBox.Show("无法获取CAD编辑器", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var doc = PluginEntry.GetActiveDocument();
                if (doc == null)
                {
                    MessageBox.Show("没有活动的CAD文档", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 检查是否在块编辑器中
                if (!IsInBlockEditor())
                {
                    MessageBox.Show("请在块编辑器中执行此命令", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 显示命令选项
                if (!ShowCommandOptions(editor))
                {
                    PluginEntry.Log("用户取消命令");
                    return;
                }

                if (_isLookupMode)
                {
                    // 执行查寻动作创建流程
                    ExecuteLookupActionFlow(editor);
                }
                else
                {
                    // 执行标准动作创建流程
                    ExecuteStandardActionFlow(editor);
                }

                editor.Regen();
                PluginEntry.Log("BACTIONTOOL命令执行完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"BACTIONTOOL命令执行失败: {ex.Message}");
                MessageBox.Show($"命令执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 显示命令选项
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否继续执行</returns>
        private bool ShowCommandOptions(Editor editor)
        {
            try
            {
                var promptOptions = new PromptKeywordOptions("\n选择命令选项 [查寻动作(L)/标准动作(S)/管理选择集(M)]: ");
                promptOptions.Keywords.Add("Lookup", "L", "查寻动作");
                promptOptions.Keywords.Add("Standard", "S", "标准动作");
                promptOptions.Keywords.Add("Manage", "M", "管理选择集");
                promptOptions.Keywords.Default = "Lookup";

                var result = editor.GetKeywords(promptOptions);
                if (result.Status == PromptStatus.OK)
                {
                    switch (result.StringResult.ToUpper())
                    {
                        case "L":
                        case "LOOKUP":
                            _isLookupMode = true;
                            PluginEntry.Log("选择查寻动作模式");
                            return true;
                        case "S":
                        case "STANDARD":
                            _isLookupMode = false;
                            PluginEntry.Log("选择标准动作模式");
                            return true;
                        case "M":
                        case "MANAGE":
                            return ManageSelectionSets();
                        default:
                            _isLookupMode = true; // 默认查寻动作
                            return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示命令选项失败: {ex.Message}");
                _isLookupMode = true; // 默认查寻动作
                return true;
            }
        }

        /// <summary>
        /// 执行查寻动作创建流程
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        private void ExecuteLookupActionFlow(Editor editor)
        {
            try
            {
                PluginEntry.Log("开始查寻动作创建流程");

                // 选择查寻参数
                if (!_selectionManager.StartParameterSelection(editor))
                {
                    PluginEntry.Log("参数选择失败或取消");
                    return;
                }
                _selectedParameters = _selectionManager.GetSelectedParameters();

                // 选择动作对象
                if (!_selectionManager.StartEntitySelection(editor))
                {
                    PluginEntry.Log("对象选择失败或取消");
                    return;
                }
                _selectionSet = _selectionManager.GetSelectedEntities();

                // 设置查寻值
                if (!SetupLookupValues())
                {
                    PluginEntry.Log("查寻值设置失败");
                    return;
                }

                // 设置动作属性
                if (!PromptActionProperties())
                {
                    PluginEntry.Log("动作属性设置失败或取消");
                    return;
                }

                // 创建查寻动作
                CreateLookupActions();

                PluginEntry.Log("查寻动作创建流程完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"查寻动作创建流程失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 执行标准动作创建流程
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        private void ExecuteStandardActionFlow(Editor editor)
        {
            try
            {
                PluginEntry.Log("开始标准动作创建流程");

                // 选择查寻参数
                if (!SelectLookupParameters(editor))
                {
                    PluginEntry.Log("参数选择失败或取消");
                    return;
                }

                // 创建动作选择集
                if (!CreateActionSelectionSet(editor))
                {
                    PluginEntry.Log("对象选择失败或取消");
                    return;
                }

                // 提示用户设置动作属性
                if (!PromptActionProperties())
                {
                    return;
                }

                // 创建查寻动作
                CreateLookupAction();

                PluginEntry.Log("标准动作创建流程完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"标准动作创建流程失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 管理选择集
        /// </summary>
        /// <returns>是否成功</returns>
        private bool ManageSelectionSets()
        {
            try
            {
                var result = _selectionSetManager.ShowSelectionSetManagerDialog();
                return result == DialogResult.OK;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"管理选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 设置查寻值
        /// </summary>
        /// <returns>是否设置成功</returns>
        private bool SetupLookupValues()
        {
            try
            {
                var inputDialog = new LookupValuesInputDialog();
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    _lookupValues = inputDialog.LookupValues;
                    return _lookupValues.Count > 0;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置查寻值失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建查寻动作（多个参数）
        /// </summary>
        private void CreateLookupActions()
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    var createdActions = new List<ObjectId>();

                    // 为每个选中的参数创建查寻动作
                    foreach (var parameterId in _selectedParameters)
                    {
                        try
                        {
                            var actionId = _actionCreator.CreateLookupAction(
                                parameterId, 
                                _selectionSet, 
                                _actionName, 
                                _description, 
                                _lookupValues
                            );
                            
                            if (actionId != ObjectId.Null)
                            {
                                createdActions.Add(actionId);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            PluginEntry.Log($"为参数 {parameterId} 创建查寻动作失败: {ex.Message}");
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log($"查寻动作创建完成，共创建 {createdActions.Count} 个动作");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建查寻动作失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 检查是否在块编辑器中
        /// </summary>
        /// <returns>是否在块编辑器中</returns>
        private bool IsInBlockEditor()
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return false;

                var currentSpace = doc.Database.CurrentSpaceId;
                var blockTable = doc.Database.BlockTableId;
                
                return currentSpace != blockTable;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"检查块编辑器状态失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 选择查寻参数
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否成功选择</returns>
        private bool SelectLookupParameters(Editor editor)
        {
            try
            {
                _selectedParameters.Clear();

                var promptSelectionOptions = new PromptSelectionOptions();
                promptSelectionOptions.MessageForAdding = "\n选择查寻参数或 [多个(M)]: ";
                promptSelectionOptions.AllowDuplicates = false;
                promptSelectionOptions.AllowSubSelections = true;
                promptSelectionOptions.SinglePickInSpace = true;

                // 设置过滤器，只选择查寻参数
                var filterList = new SelectionFilter(
                    new TypedValue[] 
                    {
                        new TypedValue(1000, "PARAMETER"),
                        new TypedValue(1040, 1.0) // 查寻参数类型标识
                    }
                );
                promptSelectionOptions.Filter = filterList;

                var result = editor.GetSelection(promptSelectionOptions);
                if (result.Status == PromptStatus.OK)
                {
                    _selectedParameters.AddRange(result.Value.GetObjectIds());
                    return true;
                }
                else if (result.Status == PromptStatus.Keyword)
                {
                    return HandleSelectionKeyword(editor, result.StringResult);
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"选择查寻参数失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 处理选择关键字
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="keyword">关键字</param>
        /// <returns>是否成功选择</returns>
        private bool HandleSelectionKeyword(Editor editor, string keyword)
        {
            try
            {
                if (keyword.ToUpper() == "M")
                {
                    // 多个选择
                    var promptSelectionOptions = new PromptSelectionOptions();
                    promptSelectionOptions.MessageForAdding = "\n选择查寻参数: ";

                    var filterList = new SelectionFilter(
                        new TypedValue[] 
                        {
                            new TypedValue(1000, "PARAMETER"),
                            new TypedValue(1040, 1.0)
                        }
                    );
                    promptSelectionOptions.Filter = filterList;
                    promptSelectionOptions.AllowDuplicates = true;
                    promptSelectionOptions.AllowSubSelections = true;

                    var result = editor.GetSelection(promptSelectionOptions);
                    if (result.Status == PromptStatus.OK)
                    {
                        _selectedParameters.AddRange(result.Value.GetObjectIds());
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"处理选择关键字失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建动作选择集
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否成功创建</returns>
        private bool CreateActionSelectionSet(Editor editor)
        {
            try
            {
                _selectionSet.Clear();

                var promptSelectionOptions = new PromptSelectionOptions();
                promptSelectionOptions.MessageForAdding = "\n选择要受动作影响的对象: ";
                promptSelectionOptions.AllowDuplicates = false;
                promptSelectionOptions.AllowSubSelections = true;

                var result = editor.GetSelection(promptSelectionOptions);
                if (result.Status == PromptStatus.OK)
                {
                    _selectionSet.AddRange(result.Value.GetObjectIds());
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建动作选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 提示用户设置动作属性
        /// </summary>
        /// <returns>是否继续执行</returns>
        private bool PromptActionProperties()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return false;

                // 显示动作属性设置对话框
                using (var dialog = new ActionPropertiesDialog())
                {
                    dialog.ActionName = _actionName;
                    dialog.Description = _description;
                    dialog.SelectedParameters = _selectedParameters.Count;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _actionName = dialog.ActionName;
                        _description = dialog.Description;
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示动作属性失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建查寻动作
        /// </summary>
        private void CreateLookupAction()
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    // 获取当前块表记录
                    var blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable == null) return;

                    var currentBlockRecord = transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (currentBlockRecord == null) return;

                    // 为每个选中的参数创建查寻动作
                    foreach (var parameterId in _selectedParameters)
                    {
                        CreateLookupActionForParameter(transaction, currentBlockRecord, parameterId);
                    }

                    transaction.Commit();
                    PluginEntry.Log($"查寻动作已创建: {_actionName}，参数数量: {_selectedParameters.Count}，对象数量: {_selectionSet.Count}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建查寻动作失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 为指定参数创建查寻动作
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="blockRecord">块表记录</param>
        /// <param name="parameterId">参数对象ID</param>
        private void CreateLookupActionForParameter(Transaction transaction, BlockTableRecord blockRecord, ObjectId parameterId)
        {
            try
            {
                // 获取参数对象
                var parameter = transaction.GetObject(parameterId, OpenMode.ForRead) as DBPoint;
                if (parameter == null) return;

                // 创建动作图标
                var actionPoint = new DBPoint(parameter.Position.Add(new Vector3d(10, 0, 0)));
                actionPoint.ColorIndex = 3; // 绿色
                blockRecord.AppendEntity(actionPoint);
                transaction.AddNewlyCreatedDBObject(actionPoint, true);

                // 创建动作标签
                CreateActionLabel(transaction, blockRecord, actionPoint.Position, _actionName);

                // 设置动作属性
                SetActionProperties(actionPoint, _actionName, _description, parameterId);

                // 关联选择集
                AssociateSelectionSet(transaction, actionPoint, _selectionSet);

                PluginEntry.Log($"为参数 {parameterId} 创建查寻动作: {_actionName}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"为参数创建查寻动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建动作标签
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="blockRecord">块表记录</param>
        /// <param name="position">位置</param>
        /// <param name="actionName">动作名称</param>
        private void CreateActionLabel(Transaction transaction, BlockTableRecord blockRecord, Point3d position, string actionName)
        {
            try
            {
                // 创建标签文字
                var labelText = new DBText();
                labelText.Position = position.Add(new Vector3d(5, 5, 0));
                labelText.TextString = $"查找_{actionName}";
                labelText.Height = 2.5;
                labelText.ColorIndex = 2; // 黄色
                labelText.HorizontalMode = TextHorizontalMode.Left;
                labelText.VerticalMode = TextVerticalMode.Baseline;

                blockRecord.AppendEntity(labelText);
                transaction.AddNewlyCreatedDBObject(labelText, true);

                // 设置标签属性
                labelText.XData.Add(new TypedValue(1000, "ACTION_LABEL"));
                labelText.XData.Add(new TypedValue(1001, actionName));
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建动作标签失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置动作属性
        /// </summary>
        /// <param name="action">动作对象</param>
        /// <param name="name">动作名称</param>
        /// <param name="description">描述</param>
        /// <param name="parameterId">关联的参数ID</param>
        private void SetActionProperties(DBPoint action, string name, string description, ObjectId parameterId)
        {
            try
            {
                action.XData.Add(new TypedValue(1000, "ACTION"));
                action.XData.Add(new TypedValue(1001, name));
                action.XData.Add(new TypedValue(1002, description));
                action.XData.Add(new TypedValue(1040, 2.0)); // 动作类型标识（查寻动作）
                action.XData.Add(new TypedValue(1041, parameterId.OldId)); // 关联的参数ID

                PluginEntry.Log($"动作属性已设置: {name}, {description}, 关联参数: {parameterId}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置动作属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 关联选择集
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作对象</param>
        /// <param name="selectionSet">选择集</param>
        private void AssociateSelectionSet(Transaction transaction, DBPoint action, List<ObjectId> selectionSet)
        {
            try
            {
                // 将选择集信息存储到扩展字典中
                var extensionDict = action.ExtensionDictionary;
                if (extensionDict.IsNull)
                {
                    action.CreateExtensionDictionary();
                    extensionDict = action.ExtensionDictionary;
                }

                var dict = transaction.GetObject(extensionDict, OpenMode.ForWrite) as DBDictionary;
                if (dict == null) return;

                // 存储选择集信息
                var selectionRecord = new Xrecord();
                var data = new ResultBuffer();
                
                // 添加选择集对象ID列表
                data.Add(new TypedValue(1000, "SELECTION_SET"));
                foreach (var objId in selectionSet)
                {
                    data.Add(new TypedValue(1070, (int)objId.OldId));
                }

                selectionRecord.Data = data;
                dict.SetAt("SelectionSet", selectionRecord);
                transaction.AddNewlyCreatedDBObject(selectionRecord, true);

                PluginEntry.Log($"动作选择集已关联，包含 {selectionSet.Count} 个对象");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"关联选择集失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 查寻值输入对话框
    /// </summary>
    public class LookupValuesInputDialog : Form
    {
        private TextBox _valuesTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private Label _instructionLabel;
        
        public List<string> LookupValues { get; private set; } = new List<string>();

        public LookupValuesInputDialog()
        {
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            this.Text = "设置查寻值";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 说明标签
            _instructionLabel = new Label
            {
                Text = "请输入查寻值，每行一个值:",
                Location = new Point(20, 20),
                Size = new Size(350, 40),
                AutoSize = false
            };
            this.Controls.Add(_instructionLabel);

            // 查寻值输入框
            _valuesTextBox = new TextBox
            {
                Location = new Point(20, 70),
                Size = new Size(350, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = true,
                WordWrap = false
            };
            _valuesTextBox.TextChanged += ValuesTextBox_TextChanged;
            this.Controls.Add(_valuesTextBox);

            // 预设值按钮
            var presetButton = new Button
            {
                Text = "预设值(&P)",
                Location = new Point(20, 230),
                Size = new Size(75, 23)
            };
            presetButton.Click += PresetButton_Click;
            this.Controls.Add(presetButton);

            // 确定按钮
            _okButton = new Button
            {
                Text = "确定(&O)",
                Location = new Point(200, 230),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            // 取消按钮
            _cancelButton = new Button
            {
                Text = "取消(&C)",
                Location = new Point(285, 230),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            // 设置接受和取消按钮
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            // 加载默认示例
            LoadDefaultValues();
        }

        private void LoadDefaultValues()
        {
            _valuesTextBox.Text = "选项1\n选项2\n选项3\n自定义选项";
        }

        private void ValuesTextBox_TextChanged(object sender, EventArgs e)
        {
            var text = _valuesTextBox.Text.Trim();
            _okButton.Enabled = !string.IsNullOrEmpty(text) && 
                               text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length > 0;
        }

        private void PresetButton_Click(object sender, EventArgs e)
        {
            var presetDialog = new PresetValuesDialog();
            if (presetDialog.ShowDialog() == DialogResult.OK)
            {
                _valuesTextBox.Text = string.Join("\n", presetDialog.SelectedValues);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var text = _valuesTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                LookupValues = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(line => !string.IsNullOrWhiteSpace(line))
                                 .Select(line => line.Trim())
                                 .ToList();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    /// <summary>
    /// 预设值选择对话框
    /// </summary>
    public class PresetValuesDialog : Form
    {
        private ListBox _presetListBox;
        private Button _okButton;
        private Button _cancelButton;
        
        public List<string> SelectedValues { get; private set; } = new List<string>();

        public PresetValuesDialog()
        {
            InitializeDialog();
            LoadPresetValues();
        }

        private void InitializeDialog()
        {
            this.Text = "选择预设查寻值";
            this.Size = new Size(300, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var label = new Label
            {
                Text = "选择预设的查寻值:",
                Location = new Point(20, 20),
                Size = new Size(200, 23)
            };
            this.Controls.Add(label);

            _presetListBox = new ListBox
            {
                Location = new Point(20, 50),
                Size = new Size(250, 200),
                SelectionMode = SelectionMode.MultiExtended
            };
            this.Controls.Add(_presetListBox);

            _okButton = new Button
            {
                Text = "确定(&O)",
                Location = new Point(100, 260),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "取消(&C)",
                Location = new Point(185, 260),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);
        }

        private void LoadPresetValues()
        {
            _presetListBox.Items.AddRange(new object[]
            {
                "是/否",
                "开/关", 
                "真/假",
                "启用/禁用",
                "显示/隐藏",
                "类型A/类型B/类型C",
                "选项1/选项2/选项3/选项4",
                "大/中/小",
                "高/中/低",
                "红/绿/蓝",
                "1/2/3/4/5",
                "第一季度/第二季度/第三季度/第四季度",
                "状态1/状态2/状态3/状态4/状态5"
            });
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            SelectedValues = _presetListBox.SelectedItems.Cast<string>().ToList();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}