using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Runtime;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// BPROPERTIES命令 - 管理参数特性
    /// </summary>
    public class PropertiesCommand
    {
        private List<ParameterProperty> _availableProperties = new List<ParameterProperty>();
        private List<ParameterProperty> _selectedProperties = new List<ParameterProperty>();

        /// <summary>
        /// 执行BPROPERTIES命令
        /// </summary>
        [CommandMethod("ZWBPROPERTIES")]
        public void Execute()
        {
            try
            {
                PluginEntry.Log("开始执行BPROPERTIES命令");

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

                // 选择查寻动作
                if (!SelectLookupAction(editor))
                {
                    PluginEntry.Log("用户取消命令");
                    return;
                }

                // 加载可用特性
                LoadAvailableProperties();

                // 显示特性管理对话框
                using (var dialog = new PropertiesManagerDialog(_availableProperties, _selectedProperties))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _selectedProperties = dialog.GetSelectedProperties();
                        
                        // 应用特性更改
                        ApplyPropertyChanges();
                    }
                }

                editor.Regen();
                PluginEntry.Log("BPROPERTIES命令执行完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"BPROPERTIES命令执行失败: {ex.Message}");
                MessageBox.Show($"命令执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 选择查寻动作
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否成功选择</returns>
        private bool SelectLookupAction(Editor editor)
        {
            try
            {
                var promptSelectionOptions = new PromptSelectionOptions();
                promptSelectionOptions.MessageForAdding = "\n选择查寻动作: ";
                promptSelectionOptions.AllowDuplicates = false;
                promptSelectionOptions.AllowSubSelections = false;
                promptSelectionOptions.SinglePickInSpace = true;

                // 设置过滤器，只选择查寻动作
                var filterList = new SelectionFilter(
                    new TypedValue[] 
                    {
                        new TypedValue(1000, "ACTION"),
                        new TypedValue(1040, 2.0) // 查寻动作类型标识
                    }
                );
                promptSelectionOptions.Filter = filterList;

                var result = editor.GetSelection(promptSelectionOptions);
                if (result.Status == PromptStatus.OK)
                {
                    var selectedEntities = result.Value.GetObjectIds();
                    if (selectedEntities.Count > 0)
                    {
                        // 存储选中的查寻动作ID
                        SelectedActionId = selectedEntities[0];
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"选择查寻动作失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载可用特性
        /// </summary>
        private void LoadAvailableProperties()
        {
            try
            {
                _availableProperties.Clear();
                _selectedProperties.Clear();

                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    // 获取当前块定义中的参数
                    var currentBlockRecord = GetCurrentBlockRecord(transaction);
                    if (currentBlockRecord == null) return;

                    var parameters = GetBlockParameters(currentBlockRecord);
                    
                    // 为每个参数创建特性
                    foreach (var parameter in parameters)
                    {
                        var properties = CreatePropertiesForParameter(parameter);
                        _availableProperties.AddRange(properties);
                    }

                    // 加载已选中的特性
                    LoadSelectedProperties(transaction);
                    
                    transaction.Commit();
                }

                PluginEntry.Log($"已加载 {_availableProperties.Count} 个可用特性");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"加载可用特性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前块记录
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <returns>块表记录</returns>
        private BlockTableRecord? GetCurrentBlockRecord(Transaction transaction)
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return null;

                var blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable == null) return null;

                return transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], OpenMode.ForRead) as BlockTableRecord;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取当前块记录失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取块中的参数
        /// </summary>
        /// <param name="blockRecord">块表记录</param>
        /// <returns>参数列表</returns>
        private List<Entity> GetBlockParameters(BlockTableRecord blockRecord)
        {
            var parameters = new List<Entity>();

            try
            {
                foreach (ObjectId entityId in blockRecord)
                {
                    var entity = transaction.GetObject(entityId, OpenMode.ForRead) as Entity;
                    if (entity == null) continue;

                    // 检查是否是参数
                    if (IsParameter(entity))
                    {
                        parameters.Add(entity);
                    }
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取块参数失败: {ex.Message}");
            }

            return parameters;
        }

        /// <summary>
        /// 检查是否是参数
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否是参数</returns>
        private bool IsParameter(Entity entity)
        {
            try
            {
                var xdata = entity.XData;
                if (xdata == null) return false;

                foreach (TypedValue value in xdata)
                {
                    if (value.TypeCode == 1000 && value.Value.ToString() == "PARAMETER")
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"检查参数失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 为参数创建特性
        /// </summary>
        /// <param name="parameter">参数实体</param>
        /// <returns>特性列表</returns>
        private List<ParameterProperty> CreatePropertiesForParameter(Entity parameter)
        {
            var properties = new List<ParameterProperty>();

            try
            {
                var xdata = parameter.XData;
                if (xdata == null) return properties;

                string parameterName = "";
                string parameterLabel = "";

                foreach (TypedValue value in xdata)
                {
                    switch (value.TypeCode)
                    {
                        case 1001: // 参数名称
                            parameterName = value.Value.ToString();
                            break;
                        case 1002: // 参数标签
                            parameterLabel = value.Value.ToString();
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(parameterName))
                {
                    // 创建输入特性
                    var inputProperty = new ParameterProperty
                    {
                        Name = parameterName,
                        DisplayName = parameterLabel,
                        Type = PropertyType.Input,
                        Value = parameterName,
                        Description = $"输入特性: {parameterLabel}"
                    };
                    properties.Add(inputProperty);

                    // 创建查寻特性
                    var lookupProperty = new ParameterProperty
                    {
                        Name = $"{parameterName}_Lookup",
                        DisplayName = $"{parameterLabel}_查寻",
                        Type = PropertyType.Lookup,
                        Value = parameterName,
                        Description = $"查寻特性: {parameterLabel}"
                    };
                    properties.Add(lookupProperty);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建参数特性失败: {ex.Message}");
            }

            return properties;
        }

        /// <summary>
        /// 加载已选中的特性
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        private void LoadSelectedProperties(Transaction transaction)
        {
            try
            {
                if (SelectedActionId == ObjectId.Null) return;

                var action = transaction.GetObject(SelectedActionId, OpenMode.ForRead) as Entity;
                if (action == null) return;

                var lookupTable = ExtractLookupTableData(action);
                if (lookupTable != null)
                {
                    _selectedProperties.AddRange(lookupTable.Properties);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"加载已选中特性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 提取查寻表数据
        /// </summary>
        /// <param name="action">动作实体</param>
        /// <returns>查寻表数据</returns>
        private LookupTableData ExtractLookupTableData(Entity action)
        {
            try
            {
                if (action.ExtensionDictionary.IsNull) return null;

                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return null;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    var dict = transaction.GetObject(action.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                    if (dict == null || !dict.Contains("LookupTable")) return null;

                    var tableRecord = transaction.GetObject(dict.GetAt("LookupTable"), OpenMode.ForRead) as Xrecord;
                    if (tableRecord?.Data == null) return null;

                    var lookupTable = new LookupTableData();
                    
                    foreach (TypedValue value in tableRecord.Data)
                    {
                        switch (value.TypeCode)
                        {
                            case 1001: // 动作名称
                                lookupTable.ActionName = value.Value.ToString();
                                break;
                            case 1002: // 特性名称
                                var property = new ParameterProperty
                                {
                                    Name = value.Value.ToString(),
                                    Type = PropertyType.Lookup
                                };
                                lookupTable.Properties.Add(property);
                                break;
                            case 1003: // 显示值
                                if (lookupTable.Properties.Count > 0)
                                {
                                    lookupTable.Properties[lookupTable.Properties.Count - 1].DisplayValue = value.Value.ToString();
                                }
                                break;
                            case 1041: // 特性值
                                if (lookupTable.Properties.Count > 0)
                                {
                                    lookupTable.Properties[lookupTable.Properties.Count - 1].Value = value.Value.ToString();
                                }
                                break;
                        }
                    }

                    transaction.Commit();
                    return lookupTable;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提取查寻表数据失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 应用特性更改
        /// </summary>
        private void ApplyPropertyChanges()
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null || SelectedActionId == ObjectId.Null) return;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    var action = transaction.GetObject(SelectedActionId, OpenMode.ForWrite) as Entity;
                    if (action == null) return;

                    // 更新扩展字典中的特性数据
                    UpdatePropertyData(transaction, action, _selectedProperties);

                    transaction.Commit();
                }

                PluginEntry.Log($"特性更改已应用，共 {_selectedProperties.Count} 个特性");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"应用特性更改失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 更新特性数据
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作实体</param>
        /// <param name="properties">特性列表</param>
        private void UpdatePropertyData(Transaction transaction, Entity action, List<ParameterProperty> properties)
        {
            try
            {
                var extensionDict = action.ExtensionDictionary;
                if (extensionDict.IsNull)
                {
                    action.CreateExtensionDictionary();
                    extensionDict = action.ExtensionDictionary;
                }

                var dict = transaction.GetObject(extensionDict, OpenMode.ForWrite) as DBDictionary;
                if (dict == null) return;

                // 存储特性数据
                var propertiesRecord = new Xrecord();
                var data = new ResultBuffer();

                data.Add(new TypedValue(1000, "PARAMETER_PROPERTIES"));
                data.Add(new TypedValue(1070, properties.Count)); // 特性数量

                foreach (var property in properties)
                {
                    data.Add(new TypedValue(1001, property.Name));
                    data.Add(new TypedValue(1002, property.DisplayName));
                    data.Add(new TypedValue(1003, property.Type.ToString()));
                    data.Add(new TypedValue(1004, property.Value));
                    data.Add(new TypedValue(1005, property.DisplayValue));
                }

                propertiesRecord.Data = data;

                // 清除旧数据
                if (dict.Contains("ParameterProperties"))
                {
                    dict.Remove("ParameterProperties");
                }

                dict.SetAt("ParameterProperties", propertiesRecord);
                transaction.AddNewlyCreatedDBObject(propertiesRecord, true);

                PluginEntry.Log($"特性数据已更新，数量: {properties.Count}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新特性数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 选中的查寻动作ID
        /// </summary>
        private ObjectId SelectedActionId { get; set; } = ObjectId.Null;

        /// <summary>
        /// 数据库事务对象（用于临时访问）
        /// </summary>
        private Transaction? transaction { get; set; }
    }
}