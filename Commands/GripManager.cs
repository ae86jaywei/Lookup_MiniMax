using System;
using System.Collections.Generic;
using System.Linq;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// 夹点管理器
    /// 负责创建、管理和维护查寻参数的夹点
    /// </summary>
    public class GripManager
    {
        private readonly Document _document;
        private readonly Database _database;
        private readonly Editor _editor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">CAD文档</param>
        public GripManager(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _database = document.Database ?? throw new ArgumentNullException(nameof(document));
            _editor = document.Editor ?? throw new ArgumentNullException(nameof(document));
        }

        /// <summary>
        /// 创建夹点
        /// </summary>
        /// <param name="location">夹点位置</param>
        /// <param name="type">夹点类型</param>
        /// <param name="size">夹点大小</param>
        /// <returns>创建的夹点</returns>
        public Grip CreateGrip(Point3d location, GripType type, double size = 3.0)
        {
            try
            {
                PluginEntry.Log($"创建夹点: {type} at {location}");

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

                    // 创建夹点对象
                    var grip = new Grip
                    {
                        Location = location,
                        Type = type,
                        Size = size,
                        IsVisible = true,
                        IsEnabled = true,
                        Color = GetGripColor(type)
                    };

                    // 创建夹点几何图形
                    CreateGripGeometry(transaction, currentBlockRecord, grip);

                    // 设置夹点属性
                    SetGripProperties(transaction, currentBlockRecord, grip);

                    transaction.Commit();
                    
                    PluginEntry.Log($"夹点创建成功: {type}");
                    return grip;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建夹点失败: {ex.Message}");
                throw new InvalidOperationException($"创建夹点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建夹点几何图形
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void CreateGripGeometry(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            try
            {
                // 根据夹点类型创建不同的几何图形
                switch (grip.Type)
                {
                    case GripType.Position:
                        CreatePositionGrip(transaction, blockRecord, grip);
                        break;
                    case GripType.Linear:
                        CreateLinearGrip(transaction, blockRecord, grip);
                        break;
                    case GripType.Angular:
                        CreateAngularGrip(transaction, blockRecord, grip);
                        break;
                    case GripType.Radius:
                        CreateRadiusGrip(transaction, blockRecord, grip);
                        break;
                    default:
                        CreateDefaultGrip(transaction, blockRecord, grip);
                        break;
                }

                PluginEntry.Log($"夹点几何图形已创建: {grip.Type}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建夹点几何图形失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建位置夹点
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void CreatePositionGrip(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            // 创建位置夹点（圆形）
            var circle = new Circle(grip.Location, Vector3d.ZAxis, grip.Size / 2);
            circle.ColorIndex = 2; // 黄色
            circle.LayerId = _database.Clayer;

            blockRecord.AppendEntity(circle);
            transaction.AddNewlyCreatedDBObject(circle, true);

            grip.GeometryObjects.Add(circle);
        }

        /// <summary>
        /// 创建线性夹点
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void CreateLinearGrip(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            // 创建线性夹点（方形）
            var size = grip.Size;
            var halfSize = size / 2;

            var line1 = new Line(
                grip.Location.Add(new Vector3d(-halfSize, -halfSize, 0)),
                grip.Location.Add(new Vector3d(halfSize, halfSize, 0))
            );
            var line2 = new Line(
                grip.Location.Add(new Vector3d(-halfSize, halfSize, 0)),
                grip.Location.Add(new Vector3d(halfSize, -halfSize, 0))
            );

            line1.ColorIndex = 2; // 黄色
            line2.ColorIndex = 2; // 黄色
            line1.LayerId = _database.Clayer;
            line2.LayerId = _database.Clayer;

            blockRecord.AppendEntity(line1);
            blockRecord.AppendEntity(line2);
            transaction.AddNewlyCreatedDBObject(line1, true);
            transaction.AddNewlyCreatedDBObject(line2, true);

            grip.GeometryObjects.Add(line1);
            grip.GeometryObjects.Add(line2);
        }

        /// <summary>
        /// 创建角度夹点
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void CreateAngularGrip(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            // 创建角度夹点（弧形）
            var arc = new Arc(grip.Location, grip.Size / 2, 0, Math.PI * 1.5);
            arc.ColorIndex = 2; // 黄色
            arc.LayerId = _database.Clayer;

            blockRecord.AppendEntity(arc);
            transaction.AddNewlyCreatedDBObject(arc, true);

            grip.GeometryObjects.Add(arc);
        }

        /// <summary>
        /// 创建半径夹点
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void CreateRadiusGrip(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            // 创建半径夹点（圆形带半径线）
            var circle = new Circle(grip.Location, Vector3d.ZAxis, grip.Size / 3);
            circle.ColorIndex = 2; // 黄色
            circle.LayerId = _database.Clayer;

            // 添加半径指示线
            var line = new Line(
                grip.Location.Add(new Vector3d(grip.Size / 3, 0, 0)),
                grip.Location.Add(new Vector3d(grip.Size / 2, 0, 0))
            );
            line.ColorIndex = 2; // 黄色
            line.LayerId = _database.Clayer;

            blockRecord.AppendEntity(circle);
            blockRecord.AppendEntity(line);
            transaction.AddNewlyCreatedDBObject(circle, true);
            transaction.AddNewlyCreatedDBObject(line, true);

            grip.GeometryObjects.Add(circle);
            grip.GeometryObjects.Add(line);
        }

        /// <summary>
        /// 创建默认夹点
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void CreateDefaultGrip(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            // 创建默认夹点（方形）
            var size = grip.Size;
            var halfSize = size / 2;

            var rectangle = new Line[4]
            {
                new Line(grip.Location.Add(new Vector3d(-halfSize, -halfSize, 0)), grip.Location.Add(new Vector3d(halfSize, -halfSize, 0))),
                new Line(grip.Location.Add(new Vector3d(halfSize, -halfSize, 0)), grip.Location.Add(new Vector3d(halfSize, halfSize, 0))),
                new Line(grip.Location.Add(new Vector3d(halfSize, halfSize, 0)), grip.Location.Add(new Vector3d(-halfSize, halfSize, 0))),
                new Line(grip.Location.Add(new Vector3d(-halfSize, halfSize, 0)), grip.Location.Add(new Vector3d(-halfSize, -halfSize, 0)))
            };

            foreach (var line in rectangle)
            {
                line.ColorIndex = 2; // 黄色
                line.LayerId = _database.Clayer;

                blockRecord.AppendEntity(line);
                transaction.AddNewlyCreatedDBObject(line, true);

                grip.GeometryObjects.Add(line);
            }
        }

        /// <summary>
        /// 获取夹点颜色
        /// </summary>
        /// <param name="type">夹点类型</param>
        /// <returns>颜色索引</returns>
        private int GetGripColor(GripType type)
        {
            return type switch
            {
                GripType.Position => 2,      // 黄色
                GripType.Linear => 3,        // 绿色
                GripType.Angular => 4,       // 青色
                GripType.Radius => 5,        // 蓝色
                _ => 2                       // 默认黄色
            };
        }

        /// <summary>
        /// 设置夹点属性
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="blockRecord">块记录</param>
        /// <param name="grip">夹点对象</param>
        private void SetGripProperties(Transaction transaction, BlockTableRecord blockRecord, Grip grip)
        {
            try
            {
                // 为每个几何对象添加扩展数据
                foreach (var geometryObject in grip.GeometryObjects)
                {
                    if (geometryObject is Entity entity)
                    {
                        var xData = entity.XData;
                        xData.Add(new TypedValue(1000, "GRIP"));
                        xData.Add(new TypedValue(1001, grip.Type.ToString()));
                        xData.Add(new TypedValue(1040, (double)grip.Type));
                        xData.Add(new TypedValue(1041, grip.Size));
                        xData.Add(new TypedValue(1042, grip.IsVisible ? 1.0 : 0.0));
                        xData.Add(new TypedValue(1043, grip.IsEnabled ? 1.0 : 0.0));
                    }
                }

                PluginEntry.Log($"夹点属性已设置: {grip.Type}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置夹点属性失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 查找所有夹点
        /// </summary>
        /// <returns>夹点列表</returns>
        public List<Grip> FindAllGrips()
        {
            try
            {
                var grips = new List<Grip>();

                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    // 获取当前块表记录
                    var blockTable = transaction.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    var currentBlockRecord = transaction.GetObject(blockTable[BlockTableRecord.CurrentSpace], 
                        OpenMode.ForRead) as BlockTableRecord;

                    if (currentBlockRecord == null)
                        return grips;

                    // 遍历块记录中的所有实体，查找夹点
                    foreach (ObjectId entityId in currentBlockRecord)
                    {
                        try
                        {
                            var entity = transaction.GetObject(entityId, OpenMode.ForRead) as Entity;
                            if (entity != null && IsGrip(entity))
                            {
                                var grip = CreateGripFromEntity(entity);
                                if (grip != null)
                                {
                                    grips.Add(grip);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            PluginEntry.Log($"处理夹点实体时出错: {ex.Message}");
                        }
                    }

                    transaction.Commit();
                }

                PluginEntry.Log($"找到 {grips.Count} 个夹点");
                return grips;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"查找夹点失败: {ex.Message}");
                throw new InvalidOperationException($"查找夹点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 判断是否为夹点
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否为夹点</returns>
        private bool IsGrip(Entity entity)
        {
            try
            {
                if (entity?.XData == null || entity.XData.Count == 0)
                    return false;

                // 检查扩展数据是否包含夹点标识
                foreach (TypedValue value in entity.XData)
                {
                    if (value.TypeCode == 1000 && value.Value.ToString() == "GRIP")
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"判断夹点失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从实体创建夹点对象
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>夹点对象</returns>
        private Grip CreateGripFromEntity(Entity entity)
        {
            try
            {
                var grip = new Grip();
                grip.GeometryObjects.Add(entity);

                // 解析扩展数据
                foreach (TypedValue value in entity.XData)
                {
                    switch (value.TypeCode)
                    {
                        case 1001: // 夹点类型
                            if (Enum.TryParse<GripType>(value.Value.ToString(), out var type))
                            {
                                grip.Type = type;
                            }
                            break;
                        case 1040: // 类型值
                            grip.Type = (GripType)(int)(double)value.Value;
                            break;
                        case 1041: // 大小
                            grip.Size = (double)value.Value;
                            break;
                        case 1042: // 可见性
                            grip.IsVisible = ((double)value.Value) > 0.5;
                            break;
                        case 1043: // 启用状态
                            grip.IsEnabled = ((double)value.Value) > 0.5;
                            break;
                    }
                }

                // 设置位置（基于实体的几何中心）
                grip.Location = GetEntityCenter(entity);

                return grip;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"从实体创建夹点失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取实体的中心点
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>中心点</returns>
        private Point3d GetEntityCenter(Entity entity)
        {
            try
            {
                return entity.Bounds?.CenterPoint ?? entity.Position;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取实体中心点失败: {ex.Message}");
                return Point3d.Origin;
            }
        }

        /// <summary>
        /// 更新夹点位置
        /// </summary>
        /// <param name="grip">夹点对象</param>
        /// <param name="newLocation">新位置</param>
        public void UpdateGripLocation(Grip grip, Point3d newLocation)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    grip.Location = newLocation;

                    // 更新所有几何对象的位置
                    foreach (var geometryObject in grip.GeometryObjects)
                    {
                        if (geometryObject is Entity entity)
                        {
                            entity.UpgradeOpen();
                            
                            // 根据几何类型移动对象
                            switch (entity)
                            {
                                case Circle circle:
                                    circle.Center = newLocation;
                                    break;
                                case Arc arc:
                                    arc.Center = newLocation;
                                    break;
                                case Line line:
                                    var offset = newLocation - GetEntityCenter(entity);
                                    line.TransformBy(Matrix3d.Displacement(offset));
                                    break;
                            }
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log($"夹点位置已更新: {newLocation}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新夹点位置失败: {ex.Message}");
                throw new InvalidOperationException($"更新夹点位置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 设置夹点可见性
        /// </summary>
        /// <param name="grip">夹点对象</param>
        /// <param name="visible">是否可见</param>
        public void SetGripVisibility(Grip grip, bool visible)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    grip.IsVisible = visible;

                    // 更新所有几何对象的可见性
                    foreach (var geometryObject in grip.GeometryObjects)
                    {
                        if (geometryObject is Entity entity)
                        {
                            entity.UpgradeOpen();
                            entity.Visible = visible;
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log($"夹点可见性已设置: {visible}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置夹点可见性失败: {ex.Message}");
                throw new InvalidOperationException($"设置夹点可见性失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 启用或禁用夹点
        /// </summary>
        /// <param name="grip">夹点对象</param>
        /// <param name="enabled">是否启用</param>
        public void SetGripEnabled(Grip grip, bool enabled)
        {
            try
            {
                grip.IsEnabled = enabled;

                // 更新扩展数据中的启用状态
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    foreach (var geometryObject in grip.GeometryObjects)
                    {
                        if (geometryObject is Entity entity)
                        {
                            entity.UpgradeOpen();
                            var xData = entity.XData;

                            // 查找并更新启用状态值
                            for (int i = 0; i < xData.Count; i++)
                            {
                                if (xData[i].TypeCode == 1043)
                                {
                                    xData[i] = new TypedValue(1043, enabled ? 1.0 : 0.0);
                                    break;
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log($"夹点启用状态已设置: {enabled}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置夹点启用状态失败: {ex.Message}");
                throw new InvalidOperationException($"设置夹点启用状态失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除夹点
        /// </summary>
        /// <param name="grip">要删除的夹点</param>
        public void DeleteGrip(Grip grip)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    // 删除所有几何对象
                    foreach (var geometryObject in grip.GeometryObjects)
                    {
                        if (geometryObject is Entity entity && !entity.IsDisposed)
                        {
                            entity.UpgradeOpen();
                            entity.Erase();
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log("夹点已删除");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除夹点失败: {ex.Message}");
                throw new InvalidOperationException($"删除夹点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除多个夹点
        /// </summary>
        /// <param name="grips">要删除的夹点列表</param>
        public void DeleteGrips(List<Grip> grips)
        {
            try
            {
                if (grips == null || grips.Count == 0)
                    return;

                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    var deletedCount = 0;

                    foreach (var grip in grips)
                    {
                        try
                        {
                            foreach (var geometryObject in grip.GeometryObjects)
                            {
                                if (geometryObject is Entity entity && !entity.IsDisposed)
                                {
                                    entity.UpgradeOpen();
                                    entity.Erase();
                                }
                            }
                            deletedCount++;
                        }
                        catch (System.Exception ex)
                        {
                            PluginEntry.Log $"删除单个夹点时出错: {ex.Message}");
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log($"已删除 {deletedCount} 个夹点");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除多个夹点失败: {ex.Message}");
                throw new InvalidOperationException($"删除多个夹点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除所有夹点
        /// </summary>
        public void DeleteAllGrips()
        {
            try
            {
                var allGrips = FindAllGrips();
                DeleteGrips(allGrips);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除所有夹点失败: {ex.Message}");
                throw new InvalidOperationException($"删除所有夹点失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 高亮夹点
        /// </summary>
        /// <param name="grip">要高亮的夹点</param>
        /// <param name="highlight">是否高亮</param>
        public void HighlightGrip(Grip grip, bool highlight = true)
        {
            try
            {
                using (var transaction = _database.TransactionManager.StartTransaction())
                {
                    foreach (var geometryObject in grip.GeometryObjects)
                    {
                        if (geometryObject is Entity entity)
                        {
                            entity.UpgradeOpen();
                            if (highlight)
                            {
                                entity.Highlight();
                            }
                            else
                            {
                                entity.Unhighlight();
                            }
                        }
                    }

                    transaction.Commit();
                    PluginEntry.Log($"夹点高亮状态已设置: {highlight}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置夹点高亮失败: {ex.Message}");
                throw new InvalidOperationException($"设置夹点高亮失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取夹点统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        public GripStatistics GetGripStatistics()
        {
            try
            {
                var allGrips = FindAllGrips();
                var stats = new GripStatistics();

                stats.TotalGrips = allGrips.Count;

                foreach (var grip in allGrips)
                {
                    stats.GripTypeCounts[grip.Type]++;
                    if (grip.IsVisible) stats.VisibleGrips++;
                    if (grip.IsEnabled) stats.EnabledGrips++;
                }

                PluginEntry.Log($"夹点统计: 总数={stats.TotalGrips}, 可见={stats.VisibleGrips}, 启用={stats.EnabledGrips}");
                return stats;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取夹点统计失败: {ex.Message}");
                return new GripStatistics();
            }
        }
    }

    /// <summary>
    /// 夹点类
    /// </summary>
    public class Grip
    {
        /// <summary>
        /// 夹点位置
        /// </summary>
        public Point3d Location { get; set; }

        /// <summary>
        /// 夹点类型
        /// </summary>
        public GripType Type { get; set; }

        /// <summary>
        /// 夹点大小
        /// </summary>
        public double Size { get; set; } = 3.0;

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 夹点颜色
        /// </summary>
        public int Color { get; set; } = 2;

        /// <summary>
        /// 几何对象列表
        /// </summary>
        public List<Entity> GeometryObjects { get; set; } = new List<Entity>();

        /// <summary>
        /// 夹点ID
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => GeometryObjects.Count > 0 && 
                              GeometryObjects.Any(obj => obj != null && !obj.IsDisposed);
    }

    /// <summary>
    /// 夹点类型枚举
    /// </summary>
    public enum GripType
    {
        /// <summary>
        /// 位置夹点
        /// </summary>
        Position = 0,

        /// <summary>
        /// 线性夹点
        /// </summary>
        Linear = 1,

        /// <summary>
        /// 角度夹点
        /// </summary>
        Angular = 2,

        /// <summary>
        /// 半径夹点
        /// </summary>
        Radius = 3,

        /// <summary>
        /// XY夹点
        /// </summary>
        XY = 4
    }

    /// <summary>
    /// 夹点统计信息
    /// </summary>
    public class GripStatistics
    {
        /// <summary>
        /// 夹点总数
        /// </summary>
        public int TotalGrips { get; set; }

        /// <summary>
        /// 可见夹点数
        /// </summary>
        public int VisibleGrips { get; set; }

        /// <summary>
        /// 启用夹点数
        /// </summary>
        public int EnabledGrips { get; set; }

        /// <summary>
        /// 各类夹点数量统计
        /// </summary>
        public Dictionary<GripType, int> GripTypeCounts { get; set; } = new Dictionary<GripType, int>();

        /// <summary>
        /// 可见夹点比例
        /// </summary>
        public double VisibleRatio => TotalGrips > 0 ? (double)VisibleGrips / TotalGrips : 0.0;

        /// <summary>
        /// 启用夹点比例
        /// </summary>
        public double EnabledRatio => TotalGrips > 0 ? (double)EnabledGrips / TotalGrips : 0.0;
    }
}