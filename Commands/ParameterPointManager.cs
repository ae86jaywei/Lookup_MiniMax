using System;
using System.Collections.Generic;
using System.Linq;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// 参数点管理器
    /// 负责管理查寻参数点的创建、查找、更新和删除
    /// </summary>
    public class ParameterPointManager
    {
        private readonly Document _document;
        private readonly Database _database;
        private readonly Editor _editor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">CAD文档</param>
        public ParameterPointManager(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _database = document.Database ?? throw new ArgumentNullException(nameof(document));
            _editor = document.Editor ?? throw new ArgumentNullException(nameof(document));
        }

        /// <summary>
        /// 创建参数点
        /// </summary>
        /// <param name="location">位置</param>
        /// <param name="name">参数名称</param>
        /// <returns>创建的参数点</returns>
        public DBPoint CreateParameterPoint(Point3d location, string name)
        {
            try
            {
                PluginEntry.Log($"创建参数点: {name} at {location}");

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

                    // 创建参数点
                    var point = new DBPoint(location);
                    point.ColorIndex = 3; // 绿色
                    point.LayerId = _database.Clayer;

                    // 设置参数点的扩展数据
                    SetParameterPointXData(point, name);

                    currentBlockRecord.AppendEntity(point);
                    transaction.AddNewlyCreatedDBObject(point, true);

                    transaction.Commit();
                    PluginEntry.Log($"参数点创建成功: {name}");
                    return point;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建参数点失败: {ex.Message}");
                throw new InvalidOperationException($"创建参数点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 设置参数点扩展数据
        /// </summary>
        /// <param name="point">参数点</param>
        /// <param name="name">参数名称</param>
        private void SetParameterPointXData(DBPoint point, string name)
        {
            try
            {
                var xData = point.XData;
                xData.Add(new TypedValue(1000, "PARAMETER_POINT"));
                xData.Add(new TypedValue(1001, name));
                xData.Add(new TypedValue(1040, 1.0)); // 参数点类型标识
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置参数点扩展数据失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 查找所有参数点
        /// </summary>
        /// <returns>参数点列表</returns>
        public List<DBPoint> FindAllParameterPoints()
        {
            try
            {
                var parameterPoints = new List<DBPoint>();

                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    // 获取当前块表记录
                    var blockTable = transaction.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    var currentBlockRecord = transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], 
                        OpenMode.ForRead) as BlockTableRecord;

                    if (currentBlockRecord == null)
                        return parameterPoints;

                    // 遍历块记录中的所有实体
                    foreach (ObjectId entityId in currentBlockRecord)
                    {
                        try
                        {
                            var entity = transaction.GetObject(entityId, OpenMode.ForRead) as DBPoint;
                            if (entity != null && IsParameterPoint(entity))
                            {
                                parameterPoints.Add(entity);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            PluginEntry.Log($"处理实体时出错: {ex.Message}");
                        }
                    }

                    transaction.Commit();
                }

                PluginEntry.Log($"找到 {parameterPoints.Count} 个参数点");
                return parameterPoints;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"查找参数点失败: {ex.Message}");
                throw new InvalidOperationException($"查找参数点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 判断是否为参数点
        /// </summary>
        /// <param name="point">点对象</param>
        /// <returns>是否为参数点</returns>
        private bool IsParameterPoint(DBPoint point)
        {
            try
            {
                if (point?.XData == null || point.XData.Count == 0)
                    return false;

                // 检查扩展数据是否包含参数点标识
                foreach (TypedValue value in point.XData)
                {
                    if (value.TypeCode == 1000 && value.Value.ToString() == "PARAMETER_POINT")
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"判断参数点失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 根据名称查找参数点
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>参数点列表</returns>
        public List<DBPoint> FindParameterPointsByName(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("参数名称不能为空", nameof(name));

                var allPoints = FindAllParameterPoints();
                var matchedPoints = new List<DBPoint>();

                foreach (var point in allPoints)
                {
                    if (GetParameterName(point) == name)
                    {
                        matchedPoints.Add(point);
                    }
                }

                PluginEntry.Log($"找到 {matchedPoints.Count} 个名为 '{name}' 的参数点");
                return matchedPoints;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"根据名称查找参数点失败: {ex.Message}");
                throw new InvalidOperationException($"根据名称查找参数点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据位置查找参数点
        /// </summary>
        /// <param name="location">位置</param>
        /// <param name="tolerance">容差</param>
        /// <returns>参数点列表</returns>
        public List<DBPoint> FindParameterPointsByLocation(Point3d location, double tolerance = 1.0)
        {
            try
            {
                var allPoints = FindAllParameterPoints();
                var matchedPoints = new List<DBPoint>();

                foreach (var point in allPoints)
                {
                    if (point.Position.DistanceTo(location) <= tolerance)
                    {
                        matchedPoints.Add(point);
                    }
                }

                PluginEntry.Log($"找到 {matchedPoints.Count} 个位置在容差范围内的参数点");
                return matchedPoints;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"根据位置查找参数点失败: {ex.Message}");
                throw new InvalidOperationException($"根据位置查找参数点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取参数点的名称
        /// </summary>
        /// <param name="point">参数点</param>
        /// <returns>参数名称</returns>
        public string GetParameterName(DBPoint point)
        {
            try
            {
                if (point?.XData == null || point.XData.Count == 0)
                    return string.Empty;

                foreach (TypedValue value in point.XData)
                {
                    if (value.TypeCode == 1001)
                    {
                        return value.Value.ToString();
                    }
                }

                return string.Empty;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取参数点名称失败: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取参数点的完整信息
        /// </summary>
        /// <param name="point">参数点</param>
        /// <returns>参数信息</returns>
        public ParameterInfo GetParameterInfo(DBPoint point)
        {
            try
            {
                if (point?.XData == null)
                    return null;

                var info = new ParameterInfo();

                foreach (TypedValue value in point.XData)
                {
                    switch (value.TypeCode)
                    {
                        case 1001: // 参数名称
                            info.Name = value.Value.ToString();
                            break;
                        case 1002: // 参数标签
                            info.Label = value.Value.ToString();
                            break;
                        case 1003: // 参数描述
                            info.Description = value.Value.ToString();
                            break;
                        case 1040: // 参数类型
                            info.Type = (int)value.Value;
                            break;
                    }
                }

                info.Location = point.Position;
                info.Id = point.ObjectId;
                info.IsParameterPoint = IsParameterPoint(point);

                return info;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取参数点信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 更新参数点位置
        /// </summary>
        /// <param name="point">参数点</param>
        /// <param name="newLocation">新位置</param>
        public void UpdateParameterPointLocation(DBPoint point, Point3d newLocation)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    point.UpgradeOpen();
                    point.Position = newLocation;

                    transaction.Commit();
                    PluginEntry.Log($"参数点位置已更新: {newLocation}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新参数点位置失败: {ex.Message}");
                throw new InvalidOperationException($"更新参数点位置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新参数点名称
        /// </summary>
        /// <param name="point">参数点</param>
        /// <param name="newName">新名称</param>
        public void UpdateParameterPointName(DBPoint point, string newName)
        {
            try
            {
                if (string.IsNullOrEmpty(newName))
                    throw new ArgumentException("参数名称不能为空", nameof(newName));

                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    point.UpgradeOpen();

                    // 清除现有的名称扩展数据
                    var xData = point.XData;
                    for (int i = xData.Count - 1; i >= 0; i--)
                    {
                        if (xData[i].TypeCode == 1001)
                        {
                            xData.RemoveAt(i);
                        }
                    }

                    // 添加新的名称
                    xData.Add(new TypedValue(1001, newName));

                    transaction.Commit();
                    PluginEntry.Log($"参数点名称已更新: {newName}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新参数点名称失败: {ex.Message}");
                throw new InvalidOperationException($"更新参数点名称失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除参数点
        /// </summary>
        /// <param name="point">要删除的参数点</param>
        public void DeleteParameterPoint(DBPoint point)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    point.UpgradeOpen();
                    point.Erase();

                    transaction.Commit();
                    PluginEntry.Log("参数点已删除");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除参数点失败: {ex.Message}");
                throw new InvalidOperationException($"删除参数点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除所有参数点
        /// </summary>
        public void DeleteAllParameterPoints()
        {
            try
            {
                var allPoints = FindAllParameterPoints();
                var deletedCount = 0;

                foreach (var point in allPoints)
                {
                    try
                    {
                        DeleteParameterPoint(point);
                        deletedCount++;
                    }
                    catch (System.Exception ex)
                    {
                        PluginEntry.Log($"删除参数点时出错: {ex.Message}");
                    }
                }

                PluginEntry.Log($"已删除 {deletedCount} 个参数点");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除所有参数点失败: {ex.Message}");
                throw new InvalidOperationException($"删除所有参数点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 验证参数点的有效性
        /// </summary>
        /// <param name="point">参数点</param>
        /// <returns>是否有效</returns>
        public bool ValidateParameterPoint(DBPoint point)
        {
            try
            {
                if (point == null || point.IsDisposed)
                    return false;

                if (!IsParameterPoint(point))
                    return false;

                // 检查必需的属性
                var info = GetParameterInfo(point);
                return info != null && !string.IsNullOrEmpty(info.Name);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"验证参数点失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取参数点的统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        public ParameterStatistics GetParameterStatistics()
        {
            try
            {
                var allPoints = FindAllParameterPoints();
                var stats = new ParameterStatistics();

                stats.TotalParameterPoints = allPoints.Count;

                foreach (var point in allPoints)
                {
                    try
                    {
                        var info = GetParameterInfo(point);
                        if (info != null)
                        {
                            stats.NamedParameters++;
                            if (!string.IsNullOrEmpty(info.Label))
                                stats.LabeledParameters++;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        PluginEntry.Log($"处理参数点统计时出错: {ex.Message}");
                    }
                }

                PluginEntry.Log($"参数点统计: 总数={stats.TotalParameterPoints}, 已命名={stats.NamedParameters}, 已标记={stats.LabeledParameters}");
                return stats;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取参数统计失败: {ex.Message}");
                return new ParameterStatistics();
            }
        }
    }

    /// <summary>
    /// 参数信息类
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 参数标签
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 参数位置
        /// </summary>
        public Point3d Location { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 对象ID
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// 是否为参数点
        /// </summary>
        public bool IsParameterPoint { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 参数统计信息
    /// </summary>
    public class ParameterStatistics
    {
        /// <summary>
        /// 参数点总数
        /// </summary>
        public int TotalParameterPoints { get; set; }

        /// <summary>
        /// 已命名参数数量
        /// </summary>
        public int NamedParameters { get; set; }

        /// <summary>
        /// 已标记参数数量
        /// </summary>
        public int LabeledParameters { get; set; }

        /// <summary>
        /// 有效参数比例
        /// </summary>
        public double ValidParameterRatio => TotalParameterPoints > 0 ? 
            (double)NamedParameters / TotalParameterPoints : 0.0;

        /// <summary>
        /// 已标记比例
        /// </summary>
        public double LabeledRatio => NamedParameters > 0 ? 
            (double)LabeledParameters / NamedParameters : 0.0;
    }
}