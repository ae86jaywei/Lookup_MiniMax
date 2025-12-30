using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ZWDynLookup.Models;

namespace ZWDynLookup.Service
{
    /// <summary>
    /// 块引用助手
    /// 提供块引用的辅助功能，包括查寻参数获取、属性设置、动态块操作等
    /// </summary>
    public class BlockReferenceHelper
    {
        private static BlockReferenceHelper _instance;

        public static BlockReferenceHelper Instance => _instance ??= new BlockReferenceHelper();

        /// <summary>
        /// 获取块的查寻参数
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>查寻参数字典</returns>
        public Dictionary<string, string> GetLookupParameters(BlockReference blockReference)
        {
            var parameters = new Dictionary<string, string>();

            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return parameters;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    // 获取块定义
                    var blockTable = trans.GetObject(document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null && blockTable.Has(blockReference.Name))
                    {
                        var blockDef = trans.GetObject(blockTable[blockReference.Name], OpenMode.ForRead) as BlockTableRecord;
                        if (blockDef != null)
                        {
                            // 解析动态块的参数
                            ParseDynamicBlockParameters(blockDef, blockReference, parameters);
                        }
                    }
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取查寻参数失败: {ex.Message}");
            }

            return parameters;
        }

        /// <summary>
        /// 设置块的查寻参数
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="parameters">参数字典</param>
        /// <returns>是否成功</returns>
        public bool SetLookupParameters(BlockReference blockReference, Dictionary<string, string> parameters)
        {
            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return false;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var blockTable = trans.GetObject(document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null && blockTable.Has(blockReference.Name))
                    {
                        var blockDef = trans.GetObject(blockTable[blockReference.Name], OpenMode.ForRead) as BlockTableRecord;
                        if (blockDef != null)
                        {
                            // 设置动态块参数
                            SetDynamicBlockParameters(blockDef, blockReference, parameters);
                        }
                    }
                    trans.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n设置查寻参数失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取块的可见属性
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>属性列表</returns>
        public List<AttributeReference> GetVisibleAttributes(BlockReference blockReference)
        {
            var attributes = new List<AttributeReference>();

            try
            {
                if (blockReference.AttributeCollection == null) return attributes;

                foreach (ObjectId attId in blockReference.AttributeCollection)
                {
                    var attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                    if (attRef != null && !attRef.IsInvisible)
                    {
                        attributes.Add(attRef);
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取可见属性失败: {ex.Message}");
            }

            return attributes;
        }

        /// <summary>
        /// 设置块属性值
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="tagName">属性标签名</param>
        /// <param name="value">属性值</param>
        /// <returns>是否成功</returns>
        public bool SetAttributeValue(BlockReference blockReference, string tagName, string value)
        {
            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return false;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var modified = false;

                    foreach (ObjectId attId in blockReference.AttributeCollection)
                    {
                        var attRef = trans.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                        if (attRef != null && attRef.Tag.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        {
                            attRef.TextString = value;
                            modified = true;
                            break;
                        }
                    }

                    trans.Commit();
                    return modified;
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n设置属性值失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取块的所有夹点
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>夹点列表</returns>
        public List<Point3d> GetBlockGrips(BlockReference blockReference)
        {
            var grips = new List<Point3d>();

            try
            {
                // 获取主夹点（插入点）
                grips.Add(blockReference.Position);

                // 获取动态块夹点
                if (blockReference.IsDynamicBlock)
                {
                    foreach (DynamicBlockReferenceProperty property in blockReference.DynamicBlockReferencePropertyCollection)
                    {
                        if (property.Grips.Count > 0)
                        {
                            grips.AddRange(property.Grips);
                        }
                    }
                }

                // 获取属性夹点
                foreach (var attribute in GetVisibleAttributes(blockReference))
                {
                    if (attribute.Bounds.HasValue)
                    {
                        var bounds = attribute.Bounds.Value;
                        grips.Add(bounds.MinPoint);
                        grips.Add(bounds.MaxPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取块夹点失败: {ex.Message}");
            }

            return grips;
        }

        /// <summary>
        /// 检查块是否包含指定的查寻参数
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="parameterName">参数名</param>
        /// <returns>是否包含</returns>
        public bool HasLookupParameter(BlockReference blockReference, string parameterName)
        {
            try
            {
                var parameters = GetLookupParameters(blockReference);
                return parameters.ContainsKey(parameterName);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n检查查寻参数失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取块的查寻表数据
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>查寻表数据</returns>
        public LookupTableData GetLookupTableData(BlockReference blockReference)
        {
            try
            {
                var lookupTableData = new LookupTableData
                {
                    BlockName = blockReference.Name,
                    BlockId = blockReference.Id,
                    Parameters = GetLookupParameters(blockReference),
                    Attributes = GetVisibleAttributes(blockReference)
                        .ToDictionary(att => att.Tag, att => att.TextString)
                };

                return lookupTableData;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取查寻表数据失败: {ex.Message}");
                return new LookupTableData();
            }
        }

        /// <summary>
        /// 更新块的位置和缩放
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="newPosition">新位置</param>
        /// <param name="newScale">新缩放</param>
        /// <returns>是否成功</returns>
        public bool UpdateBlockTransform(BlockReference blockReference, Point3d newPosition, Scale3d newScale)
        {
            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return false;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var blockRef = trans.GetObject(blockReference.Id, OpenMode.ForWrite) as BlockReference;
                    if (blockRef != null)
                    {
                        blockRef.Position = newPosition;
                        blockRef.ScaleFactors = newScale;
                        
                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新块变换失败: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 复制块属性到另一个块
        /// </summary>
        /// <param name="sourceBlock">源块</param>
        /// <param name="targetBlock">目标块</param>
        /// <returns>是否成功</returns>
        public bool CopyAttributes(BlockReference sourceBlock, BlockReference targetBlock)
        {
            try
            {
                var sourceAttributes = GetVisibleAttributes(sourceBlock);
                var targetAttributes = GetVisibleAttributes(targetBlock);

                foreach (var sourceAtt in sourceAttributes)
                {
                    var targetAtt = targetAttributes.FirstOrDefault(t => 
                        t.Tag.Equals(sourceAtt.Tag, StringComparison.OrdinalIgnoreCase));
                    
                    if (targetAtt != null)
                    {
                        SetAttributeValue(targetBlock, targetAtt.Tag, sourceAtt.TextString);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n复制块属性失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取块的边界框
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>边界框</returns>
        public Extents3d GetBlockBounds(BlockReference blockReference)
        {
            try
            {
                var bounds = new Extents3d();
                var firstPoint = true;

                // 获取所有几何图元
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return bounds;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var blockTable = trans.GetObject(document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null && blockTable.Has(blockReference.Name))
                    {
                        var blockDef = trans.GetObject(blockTable[blockReference.Name], OpenMode.ForRead) as BlockTableRecord;
                        if (blockDef != null)
                        {
                            foreach (ObjectId entityId in blockDef)
                            {
                                var entity = trans.GetObject(entityId, OpenMode.ForRead) as Entity;
                                if (entity != null && entity.Bounds.HasValue)
                                {
                                    var entityBounds = entity.Bounds.Value;
                                    if (firstPoint)
                                    {
                                        bounds = entityBounds;
                                        firstPoint = false;
                                    }
                                    else
                                    {
                                        bounds.AddExtents(entityBounds);
                                    }
                                }
                            }
                        }
                    }
                    trans.Commit();
                }

                return bounds;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取块边界框失败: {ex.Message}");
                return new Extents3d();
            }
        }

        /// <summary>
        /// 验证块是否支持查寻功能
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>验证结果</returns>
        public bool ValidateLookupSupport(BlockReference blockReference)
        {
            try
            {
                // 检查是否为动态块
                if (!blockReference.IsDynamicBlock)
                {
                    return false;
                }

                // 检查是否包含动态属性
                if (blockReference.DynamicBlockReferencePropertyCollection.Count == 0)
                {
                    return false;
                }

                // 检查参数数量
                var parameterCount = GetLookupParameters(blockReference).Count;
                return parameterCount > 0;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n验证查寻支持失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 解析动态块参数
        /// </summary>
        private void ParseDynamicBlockParameters(BlockTableRecord blockDef, BlockReference blockReference, Dictionary<string, string> parameters)
        {
            try
            {
                foreach (ObjectId entityId in blockDef)
                {
                    var entity = blockDef.Database.TransactionManager.GetObject(entityId, OpenMode.ForRead) as Entity;
                    if (entity == null) continue;

                    // 处理不同类型的动态块元素
                    ProcessDynamicBlockEntity(entity, blockReference, parameters);
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n解析动态块参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理动态块实体
        /// </summary>
        private void ProcessDynamicBlockEntity(Entity entity, BlockReference blockReference, Dictionary<string, string> parameters)
        {
            try
            {
                // 这里需要根据具体的动态块实现来处理不同类型的参数
                // 例如：距离参数、角度参数、字符串参数等
                
                // 示例：处理字符串参数
                if (entity is AttributeReference attrRef)
                {
                    parameters[attrRef.Tag] = attrRef.TextString;
                }
                
                // 示例：处理动态属性
                foreach (DynamicBlockReferenceProperty property in blockReference.DynamicBlockReferencePropertyCollection)
                {
                    if (property.PropertyType == DynamicBlockReferencePropertyType.String)
                    {
                        parameters[property.PropertyName] = property.Value?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n处理动态块实体失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置动态块参数
        /// </summary>
        private void SetDynamicBlockParameters(BlockTableRecord blockDef, BlockReference blockReference, Dictionary<string, string> parameters)
        {
            try
            {
                // 设置属性值
                foreach (var kvp in parameters)
                {
                    SetAttributeValue(blockReference, kvp.Key, kvp.Value);
                }

                // 设置动态属性
                foreach (DynamicBlockReferenceProperty property in blockReference.DynamicBlockReferencePropertyCollection)
                {
                    if (parameters.ContainsKey(property.PropertyName))
                    {
                        var value = parameters[property.PropertyName];
                        property.Value = Convert.ChangeType(value, property.Value.GetType());
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n设置动态块参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            // 清理资源（如果需要）
        }
    }
}