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
    /// 动作选择管理器
    /// 负责管理查寻动作的参数选择和拾取功能
    /// </summary>
    public class ActionSelectionManager
    {
        private List<ObjectId> _selectedParameters = new List<ObjectId>();
        private List<ObjectId> _selectedEntities = new List<ObjectId>();
        private SelectionMode _currentMode = SelectionMode.Parameter;
        private Point3d _selectionCenter = Point3d.Origin;
        private double _selectionRadius = 10.0;

        /// <summary>
        /// 选择模式枚举
        /// </summary>
        public enum SelectionMode
        {
            Parameter,      // 选择参数
            Entity,         // 选择实体
            Multiple        // 多选模式
        }

        /// <summary>
        /// 获取当前选择的参数
        /// </summary>
        /// <returns>选中的参数对象ID列表</returns>
        public List<ObjectId> GetSelectedParameters()
        {
            return new List<ObjectId>(_selectedParameters);
        }

        /// <summary>
        /// 获取当前选择的实体
        /// </summary>
        /// <returns>选中的实体对象ID列表</returns>
        public List<ObjectId> GetSelectedEntities()
        {
            return new List<ObjectId>(_selectedEntities);
        }

        /// <summary>
        /// 设置选择模式
        /// </summary>
        /// <param name="mode">选择模式</param>
        public void SetSelectionMode(SelectionMode mode)
        {
            _currentMode = mode;
            PluginEntry.Log($"选择模式已设置为: {mode}");
        }

        /// <summary>
        /// 开始参数选择
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否成功选择</returns>
        public bool StartParameterSelection(Editor editor)
        {
            try
            {
                _selectedParameters.Clear();
                SetSelectionMode(SelectionMode.Parameter);

                var promptSelectionOptions = CreateParameterSelectionOptions();
                var result = editor.GetSelection(promptSelectionOptions);

                if (result.Status == PromptStatus.OK)
                {
                    _selectedParameters.AddRange(result.Value.GetObjectIds());
                    UpdateSelectionCenter(result.Value);
                    return true;
                }
                else if (result.Status == PromptStatus.Keyword)
                {
                    return HandleParameterKeyword(editor, result.StringResult);
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"参数选择失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 开始实体选择
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否成功选择</returns>
        public bool StartEntitySelection(Editor editor)
        {
            try
            {
                _selectedEntities.Clear();
                SetSelectionMode(SelectionMode.Entity);

                var promptSelectionOptions = CreateEntitySelectionOptions();
                var result = editor.GetSelection(promptSelectionOptions);

                if (result.Status == PromptStatus.OK)
                {
                    _selectedEntities.AddRange(result.Value.GetObjectIds());
                    UpdateSelectionCenter(result.Value);
                    return true;
                }
                else if (result.Status == PromptStatus.Keyword)
                {
                    return HandleEntityKeyword(editor, result.StringResult);
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"实体选择失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建参数选择选项
        /// </summary>
        /// <returns>选择选项</returns>
        private PromptSelectionOptions CreateParameterSelectionOptions()
        {
            var options = new PromptSelectionOptions
            {
                MessageForAdding = "\n选择查寻参数 [窗口(W)/交叉(C)/上一个(P)]: ",
                AllowDuplicates = false,
                AllowSubSelections = true,
                SinglePickInSpace = false,
                RejectPaperSpaceEntities = true
            };

            // 设置过滤器，只选择查寻参数
            var filterList = new SelectionFilter(new TypedValue[] 
            {
                new TypedValue(1000, "PARAMETER"),
                new TypedValue(1040, 1.0) // 查寻参数类型标识
            });
            options.Filter = filterList;

            return options;
        }

        /// <summary>
        /// 创建实体选择选项
        /// </summary>
        /// <returns>选择选项</returns>
        private PromptSelectionOptions CreateEntitySelectionOptions()
        {
            var options = new PromptSelectionOptions
            {
                MessageForAdding = "\n选择要受动作影响的对象 [窗口(W)/交叉(C)/上一个(P)/添加(A)]: ",
                AllowDuplicates = false,
                AllowSubSelections = true,
                SinglePickInSpace = false,
                RejectPaperSpaceEntities = true
            };

            // 排除查寻参数和动作对象
            var filterList = new SelectionFilter(new TypedValue[] 
            {
                new TypedValue(-4, "<NOT"),
                new TypedValue(1000, "PARAMETER"),
                new TypedValue(1000, "ACTION"),
                new TypedValue(1000, "ACTION_ICON"),
                new TypedValue(-4, "NOT>")
            });
            options.Filter = filterList;

            return options;
        }

        /// <summary>
        /// 处理参数选择关键字
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="keyword">关键字</param>
        /// <returns>是否成功选择</returns>
        private bool HandleParameterKeyword(Editor editor, string keyword)
        {
            switch (keyword.ToUpper())
            {
                case "W": // 窗口选择
                    return WindowSelection(editor, true);
                case "C": // 交叉选择
                    return WindowSelection(editor, false);
                case "P": // 上一个选择集
                    return PreviousSelection(editor, true);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理实体选择关键字
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="keyword">关键字</param>
        /// <returns>是否成功选择</returns>
        private bool HandleEntityKeyword(Editor editor, string keyword)
        {
            switch (keyword.ToUpper())
            {
                case "W": // 窗口选择
                    return WindowSelection(editor, true);
                case "C": // 交叉选择
                    return WindowSelection(editor, false);
                case "P": // 上一个选择集
                    return PreviousSelection(editor, false);
                case "A": // 添加模式
                    return AddSelection(editor);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 窗口选择
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="windowSelect">是否窗口选择（false为交叉选择）</param>
        /// <returns>是否成功选择</returns>
        private bool WindowSelection(Editor editor, bool windowSelect)
        {
            try
            {
                var point1 = editor.GetPoint("\n指定第一个角点: ");
                if (point1.Status != PromptStatus.OK) return false;

                var point2 = editor.GetPoint("\n指定对角点: ");
                if (point2.Status != PromptStatus.OK) return false;

                var selectionFilter = _currentMode == SelectionMode.Parameter ? 
                    new SelectionFilter(new TypedValue[] { new TypedValue(1000, "PARAMETER") }) :
                    null;

                var selectionResult = windowSelect ?
                    editor.SelectWindow(point1.Value, point2.Value, selectionFilter) :
                    editor.SelectCrossingWindow(point1.Value, point2.Value, selectionFilter);

                if (selectionResult.Status == PromptStatus.OK)
                {
                    var objectIds = selectionResult.Value.GetObjectIds();
                    if (_currentMode == SelectionMode.Parameter)
                    {
                        _selectedParameters.AddRange(objectIds);
                    }
                    else
                    {
                        _selectedEntities.AddRange(objectIds);
                    }
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"窗口选择失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上一个选择集
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="parametersOnly">是否只选择参数</param>
        /// <returns>是否成功选择</returns>
        private bool PreviousSelection(Editor editor, bool parametersOnly)
        {
            try
            {
                var previousSelection = editor.GetPreviousSelectionSet();
                if (previousSelection.Status == PromptStatus.OK)
                {
                    var objectIds = previousSelection.Value.GetObjectIds();
                    if (parametersOnly)
                    {
                        _selectedParameters.AddRange(objectIds);
                    }
                    else
                    {
                        _selectedEntities.AddRange(objectIds);
                    }
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"上一个选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 添加选择
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>是否成功选择</returns>
        private bool AddSelection(Editor editor)
        {
            try
            {
                var options = _currentMode == SelectionMode.Parameter ?
                    CreateParameterSelectionOptions() :
                    CreateEntitySelectionOptions();

                options.MessageForAdding = "\n选择要添加的对象: ";

                var result = editor.GetSelection(options);
                if (result.Status == PromptStatus.OK)
                {
                    var objectIds = result.Value.GetObjectIds();
                    if (_currentMode == SelectionMode.Parameter)
                    {
                        _selectedParameters.AddRange(objectIds);
                    }
                    else
                    {
                        _selectedEntities.AddRange(objectIds);
                    }
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"添加选择失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新选择中心点
        /// </summary>
        /// <param name="selectionResult">选择结果</param>
        private void UpdateSelectionCenter(SelectionSet selectionResult)
        {
            try
            {
                if (selectionResult.Count > 0)
                {
                    var objectIds = selectionResult.GetObjectIds();
                    CalculateSelectionCenter(objectIds);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"更新选择中心点失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 计算选择集中心点
        /// </summary>
        /// <param name="objectIds">对象ID列表</param>
        private void CalculateSelectionCenter(List<ObjectId> objectIds)
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                double sumX = 0, sumY = 0, sumZ = 0;
                int count = 0;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (var objId in objectIds)
                    {
                        try
                        {
                            var entity = transaction.GetObject(objId, OpenMode.ForRead) as Entity;
                            if (entity != null)
                            {
                                var bounds = entity.Bounds;
                                if (bounds.HasValue)
                                {
                                    var center = (bounds.Value.MinPoint + bounds.Value.MaxPoint) / 2;
                                    sumX += center.X;
                                    sumY += center.Y;
                                    sumZ += center.Z;
                                    count++;
                                }
                            }
                        }
                        catch { /* 忽略单个对象错误 */ }
                    }
                }

                if (count > 0)
                {
                    _selectionCenter = new Point3d(sumX / count, sumY / count, sumZ / count);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"计算选择中心点失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            _selectedParameters.Clear();
            _selectedEntities.Clear();
            PluginEntry.Log("选择已清除");
        }

        /// <summary>
        /// 获取选择信息
        /// </summary>
        /// <returns>选择信息字符串</returns>
        public string GetSelectionInfo()
        {
            var info = $"参数: {_selectedParameters.Count}, 实体: {_selectedEntities.Count}";
            if (_selectedParameters.Count > 0 || _selectedEntities.Count > 0)
            {
                info += $", 中心: ({_selectionCenter.X:F1}, {_selectionCenter.Y:F1})";
            }
            return info;
        }

        /// <summary>
        /// 验证选择有效性
        /// </summary>
        /// <returns>验证结果</returns>
        public SelectionValidationResult ValidateSelection()
        {
            var result = new SelectionValidationResult();

            if (_selectedParameters.Count == 0)
            {
                result.Errors.Add("未选择任何查寻参数");
            }

            if (_selectedEntities.Count == 0)
            {
                result.Errors.Add("未选择任何动作对象");
            }

            if (_selectedParameters.Count == 0 && _selectedEntities.Count == 0)
            {
                result.IsValid = false;
            }
            else
            {
                result.IsValid = true;
                result.Warnings.Add($"已选择 {_selectedParameters.Count} 个参数，{_selectedEntities.Count} 个对象");
            }

            return result;
        }

        /// <summary>
        /// 高亮显示选择的对象
        /// </summary>
        /// <param name="highlight">是否高亮</param>
        public void HighlightSelection(bool highlight)
        {
            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    // 高亮参数
                    foreach (var paramId in _selectedParameters)
                    {
                        try
                        {
                            var entity = transaction.GetObject(paramId, OpenMode.ForRead) as Entity;
                            if (entity != null)
                            {
                                if (highlight)
                                    entity.Highlight();
                                else
                                    entity.Unhighlight();
                            }
                        }
                        catch { /* 忽略单个对象错误 */ }
                    }

                    // 高亮实体
                    foreach (var entityId in _selectedEntities)
                    {
                        try
                        {
                            var entity = transaction.GetObject(entityId, OpenMode.ForRead) as Entity;
                            if (entity != null)
                            {
                                if (highlight)
                                    entity.Highlight();
                                else
                                    entity.Unhighlight();
                            }
                        }
                        catch { /* 忽略单个对象错误 */ }
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"高亮选择失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 选择验证结果
    /// </summary>
    public class SelectionValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public string GetMessage()
        {
            var messages = new List<string>();
            
            if (Errors.Count > 0)
            {
                messages.AddRange(Errors);
            }
            
            if (Warnings.Count > 0)
            {
                messages.AddRange(Warnings);
            }

            return string.Join("\n", messages);
        }
    }
}