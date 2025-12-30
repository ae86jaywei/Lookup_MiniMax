using System;
using System.Collections.Generic;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// 查寻参数创建器
    /// 负责创建和管理查寻参数对象
    /// </summary>
    public class LookupParameterCreator
    {
        private readonly Document _document;
        private readonly Database _database;
        private readonly Editor _editor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">CAD文档</param>
        public LookupParameterCreator(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _database = document.Database ?? throw new ArgumentNullException(nameof(document));
            _editor = document.Editor ?? throw new ArgumentNullException(nameof(document));
        }

        /// <summary>
        /// 创建查寻参数
        /// </summary>
        /// <param name="location">参数位置</param>
        /// <param name="name">参数名称</param>
        /// <param name="label">标签</param>
        /// <param name="description">描述</param>
        /// <param name="gripCount">夹点数量</param>
        /// <param name="showPalette">是否显示选项板</param>
        /// <returns>创建的查寻参数对象</returns>
        public LookupParameter CreateParameter(Point3d location, string name, string label, 
            string description, int gripCount, bool showPalette)
        {
            try
            {
                PluginEntry.Log($"开始创建查寻参数: {name} at {location}");

                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    // 获取当前块表记录
                    var blockTable = transaction.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable == null)
                        throw new InvalidOperationException("无法获取块表");

                    var currentBlockRecord = transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], 
                        OpenMode.ForWrite) as BlockTableRecord;
                    if (currentBlockRecord == null)
                        throw new InvalidOperationException("无法获取当前块记录");

                    // 创建查寻参数对象
                    var lookupParameter = new LookupParameter
                    {
                        Name = name,
                        Label = label,
                        Description = description,
                        Location = location,
                        GripCount = gripCount,
                        ShowPalette = showPalette
                    };

                    // 创建参数几何图形
                    CreateParameterGeometry(transaction, currentBlockRecord, lookupParameter);

                    // 设置参数属性
                    SetParameterProperties(transaction, currentBlockRecord, lookupParameter);

                    // 创建夹点
                    CreateGrips(transaction, currentBlockRecord, lookupParameter);

                    transaction.Commit();
                    
                    PluginEntry.Log($"查寻参数创建成功: {name}");
                    return lookupParameter;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建查寻参数失败: {ex.Message}");
                throw new InvalidOperationException($"创建查寻参数失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建参数几何图形
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="parameter">查寻参数</param>
        private void CreateParameterGeometry(Transaction transaction, BlockTableRecord blockRecord, 
            LookupParameter parameter)
        {
            try
            {
                // 创建参数点
                var point = new DBPoint(parameter.Location);
                point.ColorIndex = 3; // 绿色
                point.LayerId = _database.Clayer;

                blockRecord.AppendEntity(point);
                transaction.AddNewlyCreatedDBObject(point, true);

                parameter.ParameterPoint = point;

                // 创建参数标签
                if (!string.IsNullOrEmpty(parameter.Label))
                {
                    CreateParameterLabel(transaction, blockRecord, parameter);
                }

                PluginEntry.Log($"参数几何图形已创建: {parameter.Name}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建参数几何图形失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建参数标签
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="parameter">查寻参数</param>
        private void CreateParameterLabel(Transaction transaction, BlockTableRecord blockRecord, 
            LookupParameter parameter)
        {
            try
            {
                // 计算标签位置（在参数点上方偏移）
                var labelPosition = parameter.Location.Add(new Vector3d(0, 10, 0));

                // 创建标签文字
                var mText = new MText();
                mText.Contents = parameter.Label;
                mText.Location = labelPosition;
                mText.Height = 2.5;
                mText.ColorIndex = 4; // 青色
                mText.LayerId = _database.Clayer;

                blockRecord.AppendEntity(mText);
                transaction.AddNewlyCreatedDBObject(mText, true);

                parameter.LabelText = mText;

                PluginEntry.Log($"参数标签已创建: {parameter.Label}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建参数标签失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 设置参数属性
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="parameter">查寻参数</param>
        private void SetParameterProperties(Transaction transaction, BlockTableRecord blockRecord, 
            LookupParameter parameter)
        {
            try
            {
                if (parameter.ParameterPoint == null) return;

                var xData = parameter.ParameterPoint.XData;

                // 添加扩展数据
                xData.Add(new TypedValue(1000, "LOOKUP_PARAMETER"));
                xData.Add(new TypedValue(1001, parameter.Name));
                xData.Add(new TypedValue(1002, parameter.Label));
                xData.Add(new TypedValue(1003, parameter.Description));
                xData.Add(new TypedValue(1040, parameter.GripCount));
                xData.Add(new TypedValue(1041, parameter.ShowPalette ? 1.0 : 0.0));
                xData.Add(new TypedValue(1070, DateTime.Now.ToFileTime())); // 创建时间戳

                PluginEntry.Log($"参数属性已设置: {parameter.Name}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置参数属性失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建夹点
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="parameter">查寻参数</param>
        private void CreateGrips(Transaction transaction, BlockTableRecord blockRecord, 
            LookupParameter parameter)
        {
            try
            {
                var gripManager = new GripManager(_document);
                
                if (parameter.GripCount > 0)
                {
                    // 创建主夹点（位置夹点）
                    var mainGrip = gripManager.CreateGrip(parameter.Location, GripType.Position);
                    parameter.Grips.Add(mainGrip);
                }

                PluginEntry.Log($"参数夹点已创建，数量: {parameter.Grips.Count}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建参数夹点失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 更新查寻参数
        /// </summary>
        /// <param name="parameter">要更新的参数</param>
        /// <param name="newName">新名称</param>
        /// <param name="newLabel">新标签</param>
        /// <param name="newDescription">新描述</param>
        public void UpdateParameter(LookupParameter parameter, string newName, string newLabel, string newDescription)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    parameter.Name = newName;
                    parameter.Label = newLabel;
                    parameter.Description = newDescription;

                    // 更新扩展数据
                    UpdateParameterXData(parameter);

                    // 更新标签文字
                    UpdateParameterLabel(parameter);

                    transaction.Commit();
                    PluginEntry.Log($"查寻参数已更新: {parameter.Name}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新查寻参数失败: {ex.Message}");
                throw new InvalidOperationException($"更新查寻参数失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新参数扩展数据
        /// </summary>
        /// <param name="parameter">参数对象</param>
        private void UpdateParameterXData(LookupParameter parameter)
        {
            try
            {
                if (parameter.ParameterPoint?.XData == null) return;

                var xData = parameter.ParameterPoint.XData;
                
                // 清除现有的参数扩展数据
                for (int i = xData.Count - 1; i >= 0; i--)
                {
                    if (xData[i].TypeCode == 1000 && xData[i].Value.ToString() == "LOOKUP_PARAMETER")
                    {
                        xData.RemoveAt(i);
                        break;
                    }
                }

                // 重新添加扩展数据
                SetParameterProperties(null, null, parameter);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新参数扩展数据失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 更新参数标签
        /// </summary>
        /// <param name="parameter">参数对象</param>
        private void UpdateParameterLabel(LookupParameter parameter)
        {
            try
            {
                if (parameter.LabelText != null)
                {
                    parameter.LabelText.Contents = parameter.Label;
                }
                else if (!string.IsNullOrEmpty(parameter.Label))
                {
                    // 如果标签文字对象不存在但有标签内容，创建新的标签
                    using (var transaction = _database.TransactionManager.StartTransaction())
                    {
                        var blockTable = transaction.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var currentBlockRecord = transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], 
                            OpenMode.ForWrite) as BlockTableRecord;
                        
                        CreateParameterLabel(transaction, currentBlockRecord, parameter);
                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新参数标签失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 删除查寻参数
        /// </summary>
        /// <param name="parameter">要删除的参数</param>
        public void DeleteParameter(LookupParameter parameter)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    // 删除参数点
                    if (parameter.ParameterPoint != null && !parameter.ParameterPoint.IsDisposed)
                    {
                        parameter.ParameterPoint.Erase();
                    }

                    // 删除标签文字
                    if (parameter.LabelText != null && !parameter.LabelText.IsDisposed)
                    {
                        parameter.LabelText.Erase();
                    }

                    // 删除夹点
                    var gripManager = new GripManager(_document);
                    gripManager.DeleteGrips(parameter.Grips);

                    transaction.Commit();
                    PluginEntry.Log($"查寻参数已删除: {parameter.Name}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除查寻参数失败: {ex.Message}");
                throw new InvalidOperationException($"删除查寻参数失败: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// 查寻参数类
    /// </summary>
    public class LookupParameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 参数位置
        /// </summary>
        public Point3d Location { get; set; }

        /// <summary>
        /// 夹点数量
        /// </summary>
        public int GripCount { get; set; }

        /// <summary>
        /// 是否显示选项板
        /// </summary>
        public bool ShowPalette { get; set; }

        /// <summary>
        /// 参数点对象
        /// </summary>
        public DBPoint ParameterPoint { get; set; }

        /// <summary>
        /// 标签文字对象
        /// </summary>
        public MText LabelText { get; set; }

        /// <summary>
        /// 夹点列表
        /// </summary>
        public List<Grip> Grips { get; set; } = new List<Grip>();

        /// <summary>
        /// 参数ID
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public ParameterType Type => ParameterType.Lookup;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(Name) && 
                              ParameterPoint != null && 
                              !ParameterPoint.IsDisposed;
    }

    /// <summary>
    /// 参数类型枚举
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// 查寻参数
        /// </summary>
        Lookup = 0,

        /// <summary>
        /// 线性参数
        /// </summary>
        Linear = 1,

        /// <summary>
        /// 角度参数
        /// </summary>
        Angular = 2,

        /// <summary>
        /// 半径参数
        /// </summary>
        Radius = 3,

        /// <summary>
        /// XY参数
        /// </summary>
        XY = 4
    }
}