using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ZWDynLookup.Models;

namespace ZWDynLookup.Service
{
    /// <summary>
    /// 图形更新服务
    /// 负责处理查寻参数变化时的图形更新、缩放调整和位置计算
    /// </summary>
    public class GraphicsUpdateService
    {
        private static GraphicsUpdateService _instance;
        private readonly Dictionary<ObjectId, GraphicsUpdateInfo> _updateQueue = new Dictionary<ObjectId, GraphicsUpdateInfo>();
        private readonly object _lockObject = new object();

        public static GraphicsUpdateService Instance => _instance ??= new GraphicsUpdateService();

        /// <summary>
        /// 更新图形
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="parameters">新参数</param>
        /// <returns>是否成功</returns>
        public bool UpdateGraphics(BlockReference blockReference, Dictionary<string, string> parameters = null)
        {
            try
            {
                if (blockReference == null || blockReference.IsDisposed)
                    return false;

                // 获取当前参数
                var currentParameters = parameters ?? BlockReferenceHelper.Instance.GetLookupParameters(blockReference);

                // 创建更新信息
                var updateInfo = new GraphicsUpdateInfo
                {
                    BlockId = blockReference.Id,
                    NewParameters = currentParameters,
                    Timestamp = DateTime.Now
                };

                // 添加到更新队列
                lock (_lockObject)
                {
                    _updateQueue[blockReference.Id] = updateInfo;
                }

                // 立即执行更新
                ExecuteUpdate(blockReference, updateInfo);

                return true;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新图形失败: {ex.Message}");
                return false;
            }
        }

        /// <parameter name="blockReference">块引用</parameter>
        /// <parameter name="parameterName">参数名</parameter>
        /// <parameter name="parameterValue">参数值</parameter>
        /// <returns>是否成功</returns>
        public bool UpdateParameterValue(BlockReference blockReference, string parameterName, string parameterValue)
        {
            try
            {
                if (blockReference == null || blockReference.IsDisposed)
                    return false;

                // 获取当前参数
                var currentParameters = BlockReferenceHelper.Instance.GetLookupParameters(blockReference);
                
                // 更新指定参数
                if (currentParameters.ContainsKey(parameterName))
                {
                    currentParameters[parameterName] = parameterValue;
                }
                else
                {
                    currentParameters[parameterName] = parameterValue;
                }

                // 更新图形
                return UpdateGraphics(blockReference, currentParameters);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新参数值失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 计算新的缩放比例
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="parameters">参数</param>
        /// <returns>新的缩放比例</returns>
        public Scale3d CalculateNewScale(BlockReference blockReference, Dictionary<string, string> parameters)
        {
            try
            {
                var currentScale = blockReference.ScaleFactors;
                var newScale = currentScale;

                // 根据参数计算新的缩放比例
                foreach (var kvp in parameters)
                {
                    if (IsScaleParameter(kvp.Key))
                    {
                        var scaleValue = ParseScaleValue(kvp.Value);
                        if (scaleValue > 0)
                        {
                            newScale = new Scale3d(scaleValue);
                        }
                    }
                }

                return newScale;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n计算新缩放比例失败: {ex.Message}");
                return blockReference.ScaleFactors;
            }
        }

        /// <summary>
        /// 计算新的位置
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="parameters">参数</param>
        /// <returns>新的位置</returns>
        public Point3d CalculateNewPosition(BlockReference blockReference, Dictionary<string, string> parameters)
        {
            try
            {
                var currentPosition = blockReference.Position;
                var newPosition = currentPosition;

                // 根据参数计算新位置
                foreach (var kvp in parameters)
                {
                    if (IsPositionParameter(kvp.Key))
                    {
                        var offset = ParsePositionOffset(kvp.Value);
                        if (offset != Vector3d.Zero)
                        {
                            newPosition = currentPosition + offset;
                        }
                    }
                }

                return newPosition;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n计算新位置失败: {ex.Message}");
                return blockReference.Position;
            }
        }

        /// <summary>
        /// 重置到默认状态
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>是否成功</returns>
        public bool ResetToDefault(BlockReference blockReference)
        {
            try
            {
                if (blockReference == null || blockReference.IsDisposed)
                    return false;

                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return false;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var blockRef = trans.GetObject(blockReference.Id, OpenMode.ForWrite) as BlockReference;
                    if (blockRef != null)
                    {
                        // 重置到默认缩放
                        blockRef.ScaleFactors = new Scale3d(1.0);
                        
                        // 重置属性到默认值
                        ResetAttributesToDefault(blockRef);
                        
                        // 重置动态属性
                        ResetDynamicProperties(blockRef);
                        
                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n重置到默认状态失败: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 平滑更新图形
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="targetParameters">目标参数</param>
        /// <param name="duration">动画持续时间（毫秒）</param>
        /// <returns>是否成功</returns>
        public bool SmoothUpdateGraphics(BlockReference blockReference, Dictionary<string, string> targetParameters, int duration = 500)
        {
            try
            {
                if (blockReference == null || blockReference.IsDisposed)
                    return false;

                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return false;

                // 获取当前参数
                var currentParameters = BlockReferenceHelper.Instance.GetLookupParameters(blockReference);

                // 开始平滑更新（在实际实现中可能需要使用定时器）
                StartSmoothUpdate(blockReference, currentParameters, targetParameters, duration);

                return true;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n平滑更新图形失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新多个块的图形
        /// </summary>
        /// <param name="blockReferences">块引用列表</param>
        /// <param name="parameters">参数</param>
        /// <returns>成功更新的数量</returns>
        public int UpdateMultipleBlocks(IEnumerable<BlockReference> blockReferences, Dictionary<string, string> parameters)
        {
            var successCount = 0;

            try
            {
                foreach (var blockRef in blockReferences)
                {
                    if (UpdateGraphics(blockRef, parameters))
                    {
                        successCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n批量更新图形失败: {ex.Message}");
            }

            return successCount;
        }

        /// <summary>
        /// 获取更新状态
        /// </summary>
        /// <param name="blockId">块ID</param>
        /// <returns>更新状态</returns>
        public GraphicsUpdateStatus GetUpdateStatus(ObjectId blockId)
        {
            lock (_lockObject)
            {
                if (_updateQueue.TryGetValue(blockId, out var updateInfo))
                {
                    return new GraphicsUpdateStatus
                    {
                        IsUpdating = true,
                        LastUpdateTime = updateInfo.Timestamp,
                        PendingParameters = updateInfo.NewParameters
                    };
                }
                else
                {
                    return new GraphicsUpdateStatus
                    {
                        IsUpdating = false
                    };
                }
            }
        }

        /// <summary>
        /// 清理更新队列
        /// </summary>
        public void ClearUpdateQueue()
        {
            lock (_lockObject)
            {
                _updateQueue.Clear();
            }
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        private void ExecuteUpdate(BlockReference blockReference, GraphicsUpdateInfo updateInfo)
        {
            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var blockRef = trans.GetObject(blockReference.Id, OpenMode.ForWrite) as BlockReference;
                    if (blockRef != null)
                    {
                        // 计算新的变换
                        var newScale = CalculateNewScale(blockRef, updateInfo.NewParameters);
                        var newPosition = CalculateNewPosition(blockRef, updateInfo.NewParameters);

                        // 应用变换
                        ApplyTransform(blockRef, newPosition, newScale);

                        // 更新属性
                        UpdateAttributes(blockRef, updateInfo.NewParameters);

                        // 更新动态属性
                        UpdateDynamicProperties(blockRef, updateInfo.NewParameters);

                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n执行图形更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用变换
        /// </summary>
        private void ApplyTransform(BlockReference blockRef, Point3d newPosition, Scale3d newScale)
        {
            try
            {
                // 更新位置
                blockRef.Position = newPosition;
                
                // 更新缩放
                blockRef.ScaleFactors = newScale;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n应用变换失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新属性
        /// </summary>
        private void UpdateAttributes(BlockReference blockRef, Dictionary<string, string> parameters)
        {
            try
            {
                foreach (var kvp in parameters)
                {
                    if (IsAttributeParameter(kvp.Key))
                    {
                        BlockReferenceHelper.Instance.SetAttributeValue(blockRef, kvp.Key, kvp.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新动态属性
        /// </summary>
        private void UpdateDynamicProperties(BlockReference blockRef, Dictionary<string, string> parameters)
        {
            try
            {
                foreach (DynamicBlockReferenceProperty property in blockRef.DynamicBlockReferencePropertyCollection)
                {
                    if (parameters.ContainsKey(property.PropertyName))
                    {
                        var value = parameters[property.PropertyName];
                        try
                        {
                            property.Value = Convert.ChangeType(value, property.Value.GetType());
                        }
                        catch
                        {
                            // 如果转换失败，尝试直接设置字符串值
                            property.Value = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新动态属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重置属性到默认值
        /// </summary>
        private void ResetAttributesToDefault(BlockReference blockRef)
        {
            try
            {
                var attributes = BlockReferenceHelper.Instance.GetVisibleAttributes(blockRef);
                foreach (var attr in attributes)
                {
                    var attrRef = blockRef.AttributeCollection.GetObject(attr.ObjectId) as AttributeReference;
                    if (attrRef != null)
                    {
                        // 这里需要根据实际的默认值定义来重置
                        // 示例：设置为空或默认文本
                        attrRef.TextString = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n重置属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重置动态属性
        /// </summary>
        private void ResetDynamicProperties(BlockReference blockRef)
        {
            try
            {
                foreach (DynamicBlockReferenceProperty property in blockRef.DynamicBlockReferencePropertyCollection)
                {
                    // 设置为默认值（这里需要根据具体的动态块属性定义）
                    if (property.PropertyType == DynamicBlockReferencePropertyType.String)
                    {
                        property.Value = "";
                    }
                    else if (property.PropertyType == DynamicBlockReferencePropertyType.Distance)
                    {
                        property.Value = 0.0;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n重置动态属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 开始平滑更新
        /// </summary>
        private void StartSmoothUpdate(BlockReference blockRef, Dictionary<string, string> currentParameters, 
                                     Dictionary<string, string> targetParameters, int duration)
        {
            // 这里可以实现平滑动画效果
            // 简化实现：直接更新到目标状态
            UpdateGraphics(blockRef, targetParameters);
        }

        /// <summary>
        /// 检查是否为缩放参数
        /// </summary>
        private bool IsScaleParameter(string parameterName)
        {
            return parameterName.Contains("scale", StringComparison.OrdinalIgnoreCase) ||
                   parameterName.Contains("缩放", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 检查是否为位置参数
        /// </summary>
        private bool IsPositionParameter(string parameterName)
        {
            return parameterName.Contains("position", StringComparison.OrdinalIgnoreCase) ||
                   parameterName.Contains("offset", StringComparison.OrdinalIgnoreCase) ||
                   parameterName.Contains("位置", StringComparison.OrdinalIgnoreCase) ||
                   parameterName.Contains("偏移", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 检查是否为属性参数
        /// </summary>
        private bool IsAttributeParameter(string parameterName)
        {
            return parameterName.StartsWith("@", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析缩放值
        /// </summary>
        private double ParseScaleValue(string value)
        {
            if (double.TryParse(value, out var scale))
            {
                return Math.Max(0.1, Math.Min(10.0, scale)); // 限制在合理范围内
            }
            return 1.0;
        }

        /// <summary>
        /// 解析位置偏移
        /// </summary>
        private Vector3d ParsePositionOffset(string value)
        {
            try
            {
                // 解析类似 "10,20" 或 "10,20,0" 的格式
                var parts = value.Split(',');
                if (parts.Length >= 2)
                {
                    var x = double.TryParse(parts[0], out var xVal) ? xVal : 0;
                    var y = double.TryParse(parts[1], out var yVal) ? yVal : 0;
                    var z = parts.Length > 2 && double.TryParse(parts[2], out var zVal) ? zVal : 0;
                    return new Vector3d(x, y, z);
                }
            }
            catch
            {
                // 忽略解析错误
            }
            return Vector3d.Zero;
        }
    }

    /// <summary>
    /// 图形更新信息
    /// </summary>
    public class GraphicsUpdateInfo
    {
        public ObjectId BlockId { get; set; }
        public Dictionary<string, string> NewParameters { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 图形更新状态
    /// </summary>
    public class GraphicsUpdateStatus
    {
        public bool IsUpdating { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Dictionary<string, string> PendingParameters { get; set; }
    }
}