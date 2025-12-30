using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// 查寻动作创建器
    /// 负责创建和管理查寻动作的各个组成部分
    /// </summary>
    public class LookupActionCreator
    {
        private readonly string _actionType = "LookupAction";
        private readonly Color _actionColor = Color.Green;
        private readonly Color _labelColor = Color.Yellow;
        private readonly Color _selectionColor = Color.Cyan;

        /// <summary>
        /// 创建查寻动作
        /// </summary>
        /// <param name="parameterId">参数对象ID</param>
        /// <param name="selectionSet">选择集</param>
        /// <param name="actionName">动作名称</param>
        /// <param name="description">动作描述</param>
        /// <param name="lookupValues">查寻值列表</param>
        /// <returns>创建的动作ID</returns>
        public ObjectId CreateLookupAction(ObjectId parameterId, List<ObjectId> selectionSet, 
                                         string actionName, string description, List<string> lookupValues)
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return ObjectId.Null;

                ObjectId actionId = ObjectId.Null;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    // 获取当前块表记录
                    var blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable == null) return ObjectId.Null;

                    var currentBlockRecord = transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (currentBlockRecord == null) return ObjectId.Null;

                    // 获取参数位置
                    var parameter = transaction.GetObject(parameterId, OpenMode.ForRead) as DBPoint;
                    if (parameter == null) return ObjectId.Null;

                    // 创建动作图标
                    var actionPoint = CreateActionIcon(transaction, currentBlockRecord, parameter.Position);
                    if (actionPoint != null)
                    {
                        actionId = actionPoint.ObjectId;

                        // 创建动作标签
                        CreateActionLabel(transaction, currentBlockRecord, actionPoint.Position, actionName);

                        // 设置动作属性
                        SetActionProperties(transaction, actionPoint, actionName, description, parameterId, lookupValues);

                        // 关联选择集
                        AssociateSelectionSet(transaction, actionPoint, selectionSet);

                        // 创建查寻表关联
                        CreateLookupTableAssociation(transaction, actionPoint, lookupValues);

                        PluginEntry.Log($"查寻动作创建成功: {actionName}");
                    }

                    transaction.Commit();
                }

                return actionId;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建查寻动作失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建动作图标
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="blockRecord">块表记录</param>
        /// <param name="parameterPosition">参数位置</param>
        /// <returns>动作图标对象</returns>
        private DBPoint CreateActionIcon(Transaction transaction, BlockTableRecord blockRecord, Point3d parameterPosition)
        {
            try
            {
                // 在参数旁边创建动作图标
                var actionPosition = parameterPosition.Add(new Vector3d(15, 0, 0));
                var actionPoint = new DBPoint(actionPosition);
                
                // 设置颜色和样式
                actionPoint.ColorIndex = 3; // 绿色
                actionPoint.Size = 3.0; // 图标大小
                actionPoint.XData.Add(new TypedValue(1000, "ACTION_ICON"));
                actionPoint.XData.Add(new TypedValue(1001, _actionType));

                blockRecord.AppendEntity(actionPoint);
                transaction.AddNewlyCreatedDBObject(actionPoint, true);

                return actionPoint;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建动作图标失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 创建动作标签
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="blockRecord">块表记录</param>
        /// <param name="actionPosition">动作位置</param>
        /// <param name="actionName">动作名称</param>
        private void CreateActionLabel(Transaction transaction, BlockTableRecord blockRecord, Point3d actionPosition, string actionName)
        {
            try
            {
                // 创建标签文字
                var labelPosition = actionPosition.Add(new Vector3d(0, 8, 0));
                var labelText = new DBText();
                labelText.Position = labelPosition;
                labelText.TextString = $"查找_{actionName}";
                labelText.Height = 2.5;
                labelText.ColorIndex = 2; // 黄色
                labelText.HorizontalMode = TextHorizontalMode.Left;
                labelText.VerticalMode = TextVerticalMode.Baseline;
                labelText.WidthFactor = 0.8;

                // 设置标签属性
                labelText.XData.Add(new TypedValue(1000, "ACTION_LABEL"));
                labelText.XData.Add(new TypedValue(1001, actionName));
                labelText.XData.Add(new TypedValue(1002, _actionType));

                blockRecord.AppendEntity(labelText);
                transaction.AddNewlyCreatedDBObject(labelText, true);

                // 创建连接线
                CreateConnectionLine(transaction, blockRecord, actionPosition, labelPosition);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建动作标签失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建连接线
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="blockRecord">块表记录</param>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        private void CreateConnectionLine(Transaction transaction, BlockTableRecord blockRecord, Point3d startPoint, Point3d endPoint)
        {
            try
            {
                var line = new Line(startPoint, endPoint);
                line.ColorIndex = 4; // 青色
                line.LinetypeScale = 0.5;

                line.XData.Add(new TypedValue(1000, "ACTION_CONNECTION"));
                line.XData.Add(new TypedValue(1001, _actionType));

                blockRecord.AppendEntity(line);
                transaction.AddNewlyCreatedDBObject(line, true);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建连接线失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置动作属性
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作对象</param>
        /// <param name="name">动作名称</param>
        /// <param name="description">动作描述</param>
        /// <param name="parameterId">关联的参数ID</param>
        /// <param name="lookupValues">查寻值列表</param>
        private void SetActionProperties(Transaction transaction, DBPoint action, string name, string description, 
                                       ObjectId parameterId, List<string> lookupValues)
        {
            try
            {
                // 设置基本属性
                action.XData.Add(new TypedValue(1000, "ACTION"));
                action.XData.Add(new TypedValue(1001, name));
                action.XData.Add(new TypedValue(1002, description));
                action.XData.Add(new TypedValue(1040, 2.0)); // 动作类型标识（查寻动作）
                action.XData.Add(new TypedValue(1041, parameterId.OldId)); // 关联的参数ID

                // 设置查寻动作特有属性
                action.XData.Add(new TypedValue(1042, lookupValues.Count)); // 查寻值数量
                action.XData.Add(new TypedValue(1043, 1.0)); // 动作启用状态

                // 存储查寻值到扩展字典
                StoreLookupValues(transaction, action, lookupValues);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置动作属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 存储查寻值
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作对象</param>
        /// <param name="lookupValues">查寻值列表</param>
        private void StoreLookupValues(Transaction transaction, DBPoint action, List<string> lookupValues)
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

                // 存储查寻值
                var lookupXrecord = new Xrecord();
                var data = new ResultBuffer();
                data.Add(new TypedValue(1000, "LOOKUP_VALUES"));

                foreach (var value in lookupValues)
                {
                    data.Add(new TypedValue(1001, value));
                }

                lookupXrecord.Data = data;
                dict.SetAt("LookupValues", lookupXrecord);
                transaction.AddNewlyCreatedDBObject(lookupXrecord, true);

                PluginEntry.Log($"查寻值已存储，数量: {lookupValues.Count}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"存储查寻值失败: {ex.Message}");
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
                var extensionDict = action.ExtensionDictionary;
                if (extensionDict.IsNull)
                {
                    action.CreateExtensionDictionary();
                    extensionDict = action.ExtensionDictionary;
                }

                var dict = transaction.GetObject(extensionDict, OpenMode.ForWrite) as DBDictionary;
                if (dict == null) return;

                // 存储选择集信息
                var selectionXrecord = new Xrecord();
                var data = new ResultBuffer();
                
                data.Add(new TypedValue(1000, "SELECTION_SET"));
                foreach (var objId in selectionSet)
                {
                    data.Add(new TypedValue(1070, (int)objId.OldId));
                }

                selectionXrecord.Data = data;
                dict.SetAt("SelectionSet", selectionXrecord);
                transaction.AddNewlyCreatedDBObject(selectionXrecord, true);

                PluginEntry.Log($"动作选择集已关联，包含 {selectionSet.Count} 个对象");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"关联选择集失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建查寻表关联
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作对象</param>
        /// <param name="lookupValues">查寻值列表</param>
        private void CreateLookupTableAssociation(Transaction transaction, DBPoint action, List<string> lookupValues)
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

                // 创建查寻表记录
                var tableXrecord = new Xrecord();
                var data = new ResultBuffer();
                
                data.Add(new TypedValue(1000, "LOOKUP_TABLE"));
                data.Add(new TypedValue(1040, lookupValues.Count)); // 查寻值数量

                for (int i = 0; i < lookupValues.Count; i++)
                {
                    data.Add(new TypedValue(1001 + i, lookupValues[i]));
                    data.Add(new TypedValue(1070 + i, i)); // 索引
                }

                tableXrecord.Data = data;
                dict.SetAt("LookupTable", tableXrecord);
                transaction.AddNewlyCreatedDBObject(tableXrecord, true);

                PluginEntry.Log($"查寻表关联已创建，包含 {lookupValues.Count} 个值");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建查寻表关联失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 触发查寻动作
        /// </summary>
        /// <param name="actionId">动作ID</param>
        /// <param name="selectedValue">选中的查寻值</param>
        /// <returns>是否执行成功</returns>
        public bool TriggerLookupAction(ObjectId actionId, string selectedValue)
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return false;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    var action = transaction.GetObject(actionId, OpenMode.ForRead) as DBPoint;
                    if (action == null) return false;

                    // 获取关联的选择集
                    var selectionSet = GetAssociatedSelectionSet(transaction, action);
                    if (selectionSet.Count == 0) return false;

                    // 执行动作逻辑
                    ExecuteActionLogic(transaction, action, selectionSet, selectedValue);

                    transaction.Commit();
                    PluginEntry.Log($"查寻动作已触发: {selectedValue}");
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"触发查寻动作失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取关联的选择集
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作对象</param>
        /// <returns>选择集对象ID列表</returns>
        private List<ObjectId> GetAssociatedSelectionSet(Transaction transaction, DBPoint action)
        {
            var selectionSet = new List<ObjectId>();

            try
            {
                if (!action.ExtensionDictionary.IsNull)
                {
                    var dict = transaction.GetObject(action.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                    if (dict?.Contains("SelectionSet") == true)
                    {
                        var selectionXrecord = transaction.GetObject(dict.GetAt("SelectionSet"), OpenMode.ForRead) as Xrecord;
                        if (selectionXrecord?.Data != null)
                        {
                            foreach (var typedValue in selectionXrecord.Data)
                            {
                                if (typedValue.TypeCode == 1070) // ObjectId类型
                                {
                                    var objectId = new ObjectId(new IntPtr((int)typedValue.Value));
                                    selectionSet.Add(objectId);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取关联选择集失败: {ex.Message}");
            }

            return selectionSet;
        }

        /// <summary>
        /// 执行动作逻辑
        /// </summary>
        /// <param name="transaction">数据库事务</param>
        /// <param name="action">动作对象</param>
        /// <param name="selectionSet">选择集</param>
        /// <param name="selectedValue">选中的查寻值</param>
        private void ExecuteActionLogic(Transaction transaction, DBPoint action, List<ObjectId> selectionSet, string selectedValue)
        {
            try
            {
                foreach (var objId in selectionSet)
                {
                    var entity = transaction.GetObject(objId, OpenMode.ForWrite) as Entity;
                    if (entity != null)
                    {
                        // 根据查寻值修改对象属性
                        ApplyLookupValueToEntity(entity, selectedValue);
                    }
                }

                // 更新显示
                var editor = PluginEntry.GetEditor();
                editor?.Regen();
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"执行动作逻辑失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将查寻值应用到实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="lookupValue">查寻值</param>
        private void ApplyLookupValueToEntity(Entity entity, string lookupValue)
        {
            try
            {
                // 这里实现具体的查寻逻辑
                // 例如：根据查寻值修改对象的颜色、线型、尺寸等属性
                
                // 示例：根据查寻值修改颜色
                if (entity is DBPoint point)
                {
                    switch (lookupValue.ToLower())
                    {
                        case "红色":
                            point.ColorIndex = 1;
                            break;
                        case "绿色":
                            point.ColorIndex = 3;
                            break;
                        case "蓝色":
                            point.ColorIndex = 5;
                            break;
                        default:
                            point.ColorIndex = 7; // 默认颜色
                            break;
                    }
                }
                else if (entity is DBText text)
                {
                    text.TextString = lookupValue;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"应用查寻值到实体失败: {ex.Message}");
            }
        }
    }
}