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
    /// 查寻标签管理器
    /// 负责管理查寻标签的创建、显示、更新和删除
    /// </summary>
    public class LookupTagManager
    {
        private static LookupTagManager _instance;
        private readonly List<LookupTag> _activeTags = new List<LookupTag>();

        public static LookupTagManager Instance => _instance ??= new LookupTagManager();

        /// <summary>
        /// 创建查寻标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="tagValue">标签值</param>
        /// <param name="tagType">标签类型</parameter>
        /// <returns>创建的标签</returns>
        public LookupTag CreateTag(BlockReference blockReference, string tagName, string tagValue, TagType tagType)
        {
            var tag = new LookupTag
            {
                Id = Guid.NewGuid(),
                Name = tagName,
                Value = tagValue,
                Type = tagType,
                BlockReferenceId = blockReference.Id,
                IsVisible = true,
                CreatedTime = DateTime.Now
            };

            _activeTags.Add(tag);
            UpdateTagDisplay(blockReference);
            return tag;
        }

        /// <summary>
        /// 显示查寻标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        public void ShowTags(BlockReference blockReference)
        {
            var tags = GetTagsForBlock(blockReference);
            foreach (var tag in tags.Where(t => t.IsVisible))
            {
                DisplayTag(blockReference, tag);
            }
        }

        /// <summary>
        /// 隐藏查寻标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        public void HideTags(BlockReference blockReference)
        {
            var tags = GetTagsForBlock(blockReference);
            foreach (var tag in tags)
            {
                HideTag(blockReference, tag);
            }
        }

        /// <summary>
        /// 更新标签显示
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="newValue">新值</param>
        public void UpdateTagValue(BlockReference blockReference, string tagName, string newValue)
        {
            var tag = GetTag(blockReference, tagName);
            if (tag != null)
            {
                tag.Value = newValue;
                tag.LastModified = DateTime.Now;
                RefreshTagDisplay(blockReference, tag);
            }
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="tagName">标签名称</param>
        public void RemoveTag(BlockReference blockReference, string tagName)
        {
            var tag = GetTag(blockReference, tagName);
            if (tag != null)
            {
                RemoveTagFromDisplay(blockReference, tag);
                _activeTags.Remove(tag);
            }
        }

        /// <summary>
        /// 获取块的标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>标签列表</returns>
        public List<LookupTag> GetTagsForBlock(BlockReference blockReference)
        {
            return _activeTags.Where(t => t.BlockReferenceId == blockReference.Id).ToList();
        }

        /// <summary>
        /// 获取指定标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="tagName">标签名称</param>
        /// <returns>标签</returns>
        public LookupTag GetTag(BlockReference blockReference, string tagName)
        {
            return _activeTags.FirstOrDefault(t => 
                t.BlockReferenceId == blockReference.Id && t.Name == tagName);
        }

        /// <summary>
        /// 显示所有标签
        /// </summary>
        public void ShowAllTags()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            using (var trans = document.Database.TransactionManager.StartTransaction())
            {
                foreach (var tag in _activeTags.Where(t => t.IsVisible))
                {
                    var blockReference = trans.GetObject(tag.BlockReferenceId, OpenMode.ForRead) as BlockReference;
                    if (blockReference != null)
                    {
                        DisplayTag(blockReference, tag);
                    }
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 隐藏所有标签
        /// </summary>
        public void HideAllTags()
        {
            foreach (var tag in _activeTags)
            {
                tag.IsVisible = false;
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document != null)
                {
                    using (var trans = document.Database.TransactionManager.StartTransaction())
                    {
                        var blockReference = trans.GetObject(tag.BlockReferenceId, OpenMode.ForRead) as BlockReference;
                        if (blockReference != null)
                        {
                            HideTag(blockReference, tag);
                        }
                        trans.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// 重命名标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="oldName">旧名称</param>
        /// <param name="newName">新名称</param>
        public void RenameTag(BlockReference blockReference, string oldName, string newName)
        {
            var tag = GetTag(blockReference, oldName);
            if (tag != null)
            {
                tag.Name = newName;
                RefreshTagDisplay(blockReference, tag);
            }
        }

        private void UpdateTagDisplay(BlockReference blockReference)
        {
            var tags = GetTagsForBlock(blockReference);
            foreach (var tag in tags.Where(t => t.IsVisible))
            {
                DisplayTag(blockReference, tag);
            }
        }

        private void DisplayTag(BlockReference blockReference, LookupTag tag)
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            try
            {
                var position = CalculateTagPosition(blockReference, tag);
                var displayText = $"{tag.Name}: {tag.Value}";

                // 创建文本实体
                var textEntity = new DBText
                {
                    Position = position,
                    TextString = displayText,
                    Height = 2.5,
                    ColorIndex = 1, // 红色
                    LayerId = GetOrCreateTagLayer(document.Database)
                };

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var space = trans.GetObject(blockReference.BlockTableRecord, OpenMode.ForWrite) as BlockTableRecord;
                    space.AppendEntity(textEntity);
                    trans.AddNewlyCreatedDBObject(textEntity, true);
                    trans.Commit();
                }

                // 记录显示的实体
                tag.DisplayedEntities.Add(textEntity.Id);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示标签失败: {ex.Message}");
            }
        }

        private void HideTag(BlockReference blockReference, LookupTag tag)
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            try
            {
                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    foreach (var entityId in tag.DisplayedEntities)
                    {
                        var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                        if (entity != null)
                        {
                            entity.Erase();
                        }
                    }
                    trans.Commit();
                }

                tag.DisplayedEntities.Clear();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n隐藏标签失败: {ex.Message}");
            }
        }

        private void RefreshTagDisplay(BlockReference blockReference, LookupTag tag)
        {
            HideTag(blockReference, tag);
            if (tag.IsVisible)
            {
                DisplayTag(blockReference, tag);
            }
        }

        private void RemoveTagFromDisplay(BlockReference blockReference, LookupTag tag)
        {
            HideTag(blockReference, tag);
        }

        private Point3d CalculateTagPosition(BlockReference blockReference, LookupTag tag)
        {
            // 计算标签显示位置
            var basePosition = blockReference.Position;
            var offsetX = 10.0;
            var offsetY = 10.0;

            // 根据标签类型调整位置
            switch (tag.Type)
            {
                case TagType.Action:
                    offsetY = 15.0;
                    break;
                case TagType.Parameter:
                    offsetY = 20.0;
                    break;
                case TagType.Lookup:
                    offsetY = 25.0;
                    break;
            }

            return new Point3d(basePosition.X + offsetX, basePosition.Y + offsetY, basePosition.Z);
        }

        private ObjectId GetOrCreateTagLayer(Database db)
        {
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var layers = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                
                if (layers.Has("LOOKUP_TAG_LAYER"))
                {
                    return layers["LOOKUP_TAG_LAYER"];
                }

                // 创建标签图层
                var newLayer = new LayerTableRecord
                {
                    Name = "LOOKUP_TAG_LAYER",
                    Color = 1, // 红色
                    IsPlottable = false
                };

                trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                layers.Add(newLayer);
                trans.AddNewlyCreatedDBObject(newLayer, true);
                trans.Commit();

                return newLayer.Id;
            }
        }

        /// <summary>
        /// 清理所有标签
        /// </summary>
        public void ClearAllTags()
        {
            HideAllTags();
            _activeTags.Clear();
        }
    }

    /// <summary>
    /// 查寻标签
    /// </summary>
    public class LookupTag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public TagType Type { get; set; }
        public ObjectId BlockReferenceId { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastModified { get; set; }
        public List<ObjectId> DisplayedEntities { get; set; } = new List<ObjectId>();
    }

    /// <summary>
    /// 标签类型
    /// </summary>
    public enum TagType
    {
        Action,    // 动作
        Parameter, // 参数
        Lookup     // 查寻
    }
}