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
    /// BLOOKUPTABLE命令 - 管理查寻表
    /// </summary>
    public class LookupTableCommand
    {
        private List<LookupTableData> _lookupTables = new List<LookupTableData>();

        /// <summary>
        /// 执行BLOOKUPTABLE命令
        /// </summary>
        [CommandMethod("ZWBLOOKUPTABLE")]
        public void Execute()
        {
            try
            {
                PluginEntry.Log("开始执行BLOOKUPTABLE命令");

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

                // 加载现有的查寻表
                LoadExistingLookupTables();

                // 显示查寻表管理对话框
                using (var dialog = new LookupTableManagerDialog(_lookupTables))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _lookupTables = dialog.GetLookupTables();
                        
                        // 应用查寻表更改
                        ApplyLookupTableChanges();
                    }
                }

                editor.Regen();
                PluginEntry.Log("BLOOKUPTABLE命令执行完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"BLOOKUPTABLE命令执行失败: {ex.Message}");
                MessageBox.Show($"命令执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载现有的查寻表
        /// </summary>
        private void LoadExistingLookupTables()
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                _lookupTables.Clear();

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    // 扫描块表记录中的查寻表数据
                    var blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable == null) return;

                    foreach (ObjectId blockId in blockTable)
                    {
                        var blockRecord = transaction.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                        if (blockRecord == null) continue;

                        foreach (ObjectId entityId in blockRecord)
                        {
                            var entity = transaction.GetObject(entityId, OpenMode.ForRead) as Entity;
                            if (entity == null) continue;

                            // 检查是否是查寻动作
                            if (IsLookupAction(entity))
                            {
                                var lookupTable = ExtractLookupTableData(entity);
                                if (lookupTable != null)
                                {
                                    _lookupTables.Add(lookupTable);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }

                PluginEntry.Log($"已加载 {_lookupTables.Count} 个查寻表");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"加载查寻表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否是查寻动作
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否是查寻动作</returns>
        private bool IsLookupAction(Entity entity)
        {
            try
            {
                var xdata = entity.XData;
                if (xdata == null) return false;

                foreach (TypedValue value in xdata)
                {
                    if (value.TypeCode == 1000 && value.Value.ToString() == "ACTION")
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"检查查寻动作失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 提取查寻表数据
        /// </summary>
        /// <param name="action">动作实体</param>
        /// <returns>查寻表数据</returns>
        private LookupTableData? ExtractLookupTableData(Entity action)
        {
            try
            {
                var xdata = action.XData;
                if (xdata == null) return null;

                var lookupTable = new LookupTableData();
                string? actionName = null;
                string? description = null;
                ObjectId? parameterId = null;

                foreach (TypedValue value in xdata)
                {
                    switch (value.TypeCode)
                    {
                        case 1001: // 动作名称
                            actionName = value.Value.ToString();
                            break;
                        case 1002: // 描述
                            description = value.Value.ToString();
                            break;
                        case 1041: // 关联参数ID
                            parameterId = new ObjectId(new IntPtr((int)value.Value));
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(actionName))
                {
                    lookupTable.ActionName = actionName;
                    lookupTable.Description = description ?? "";
                    lookupTable.ParameterId = parameterId ?? ObjectId.Null;
                    lookupTable.ActionId = action.ObjectId;

                    // 提取选择集
                    lookupTable.SelectionSet = ExtractSelectionSet(action);

                    return lookupTable;
                }

                return null;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提取查寻表数据失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 提取选择集
        /// </summary>
        /// <param name="action">动作实体</param>
        /// <returns>选择集对象ID列表</returns>
        private List<ObjectId> ExtractSelectionSet(Entity action)
        {
            var selectionSet = new List<ObjectId>();

            try
            {
                if (!action.ExtensionDictionary.IsNull)
                {
                    var doc = PluginEntry.GetActiveDocument();
                    if (doc?.Database == null) return selectionSet;

                    using (var transaction = doc.Database.TransactionManager.StartTransaction())
                    {
                        var dict = transaction.GetObject(action.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                        if (dict != null && dict.Contains("SelectionSet"))
                        {
                            var selectionRecord = transaction.GetObject(dict.GetAt("SelectionSet"), OpenMode.ForRead) as Xrecord;
                            if (selectionRecord?.Data != null)
                            {
                                foreach (TypedValue value in selectionRecord.Data)
                                {
                                    if (value.TypeCode == 1070) // 对象ID
                                    {
                                        selectionSet.Add(new ObjectId(new IntPtr((int)value.Value)));
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提取选择集失败: {ex.Message}");
            }

            return selectionSet;
        }

        /// <summary>
        /// 应用查寻表更改
        /// </summary>
        private void ApplyLookupTableChanges()
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (var lookupTable in _lookupTables)
                    {
                        if (lookupTable.IsModified)
                        {
                            ApplyLookupTableModification(transaction, lookupTable);
                        }
                    }

                    transaction.Commit();
                }

                PluginEntry.Log("查寻表更改已应用");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"应用查寻表更改失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 应用单个查寻表修改
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="lookupTable">查寻表数据</param>
        private void ApplyLookupTableModification(Transaction transaction, LookupTableData lookupTable)
        {
            try
            {
                var action = transaction.GetObject(lookupTable.ActionId, OpenMode.ForWrite) as Entity;
                if (action == null) return;

                // 更新动作属性
                var xdata = action.XData;
                if (xdata != null)
                {
                    // 重新设置扩展数据
                    action.XData.Dispose();
                    action.XData = new ResultBuffer();

                    action.XData.Add(new TypedValue(1000, "ACTION"));
                    action.XData.Add(new TypedValue(1001, lookupTable.ActionName));
                    action.XData.Add(new TypedValue(1002, lookupTable.Description));
                    action.XData.Add(new TypedValue(1040, 2.0)); // 动作类型标识
                    action.XData.Add(new TypedValue(1041, lookupTable.ParameterId.OldId));
                }

                // 更新查寻表数据
                UpdateLookupTableData(transaction, action, lookupTable);

                PluginEntry.Log($"查寻表修改已应用: {lookupTable.ActionName}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"应用查寻表修改失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新查寻表数据
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作实体</param>
        /// <param name="lookupTable">查寻表数据</param>
        private void UpdateLookupTableData(Transaction transaction, Entity action, LookupTableData lookupTable)
        {
            try
            {
                // 创建或更新扩展字典
                var extensionDict = action.ExtensionDictionary;
                if (extensionDict.IsNull)
                {
                    action.CreateExtensionDictionary();
                    extensionDict = action.ExtensionDictionary;
                }

                var dict = transaction.GetObject(extensionDict, OpenMode.ForWrite) as DBDictionary;
                if (dict == null) return;

                // 存储查寻表数据
                var tableRecord = new Xrecord();
                var data = new ResultBuffer();

                data.Add(new TypedValue(1000, "LOOKUP_TABLE"));
                data.Add(new TypedValue(1001, lookupTable.ActionName));

                // 添加查寻特性数据
                foreach (var property in lookupTable.Properties)
                {
                    data.Add(new TypedValue(1002, property.Name));
                    data.Add(new TypedValue(1003, property.DisplayValue));
                    data.Add(new TypedValue(1041, property.Value));
                }

                tableRecord.Data = data;

                // 清除旧数据
                if (dict.Contains("LookupTable"))
                {
                    dict.Remove("LookupTable");
                }

                dict.SetAt("LookupTable", tableRecord);
                transaction.AddNewlyCreatedDBObject(tableRecord, true);

                PluginEntry.Log($"查寻表数据已更新: {lookupTable.ActionName}，特性数量: {lookupTable.Properties.Count}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新查寻表数据失败: {ex.Message}");
            }
        }
    }
}