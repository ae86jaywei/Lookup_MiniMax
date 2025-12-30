using System;
using System.ComponentModel;
using System.Windows;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using ZWDynLookup.UI;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// BPARAMETER命令 - 创建查寻参数
    /// 实现K选项（查寻参数）功能
    /// </summary>
    public class BParameterCommand
    {
        private string _parameterName = "LookupParameter";
        private string _label = "标签";
        private string _description = "查寻参数描述";
        private bool _showPalette = true;
        private int _gripCount = 1;

        private LookupParameterCreator _parameterCreator;
        private ParameterPointManager _pointManager;
        private GripManager _gripManager;
        private LookupParameter _currentParameter;

        /// <summary>
        /// 执行BPARAMETER命令
        /// </summary>
        [CommandMethod("ZWBPARAMETER")]
        public void Execute()
        {
            try
            {
                PluginEntry.Log("开始执行BPARAMETER命令");

                var editor = PluginEntry.GetEditor();
                if (editor == null)
                {
                    editor?.WriteMessage("\n错误：无法获取CAD编辑器");
                    return;
                }

                var doc = PluginEntry.GetActiveDocument();
                if (doc == null)
                {
                    editor.WriteMessage("\n错误：没有活动的CAD文档");
                    return;
                }

                // 初始化管理器
                InitializeManagers(doc);

                // 检查是否在块编辑器中
                if (!IsInBlockEditor(doc))
                {
                    editor.WriteMessage("\n错误：请在块编辑器中执行此命令");
                    return;
                }

                // 执行主循环
                ExecuteMainLoop(editor);

                PluginEntry.Log("BPARAMETER命令执行完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"BPARAMETER命令执行失败: {ex.Message}");
                var editor = PluginEntry.GetEditor();
                editor?.WriteMessage($"\n错误：命令执行失败 - {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化管理器
        /// </summary>
        /// <param name="document">CAD文档</param>
        private void InitializeManagers(Document document)
        {
            _parameterCreator = new LookupParameterCreator(document);
            _pointManager = new ParameterPointManager(document);
            _gripManager = new GripManager(document);
        }

        /// <summary>
        /// 执行主循环
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        private void ExecuteMainLoop(Editor editor)
        {
            bool continueLoop = true;

            while (continueLoop)
            {
                // 提示用户设置参数属性
                if (!PromptParameterProperties())
                {
                    break;
                }

                // 提示用户指定参数位置
                var point = PromptParameterLocation(editor);
                if (point == null)
                {
                    editor.WriteMessage("\n命令已取消");
                    break;
                }

                try
                {
                    // 创建查寻参数
                    _currentParameter = _parameterCreator.CreateParameter(
                        point.Value, 
                        _parameterName, 
                        _label, 
                        _description, 
                        _gripCount, 
                        _showPalette
                    );

                    editor.Regen();
                    editor.WriteMessage($"\n查寻参数 '{_parameterName}' 已创建成功");
                    
                    // 询问是否继续创建更多参数
                    var continueResult = editor.GetKeyword("\n是否创建另一个查寻参数？[是(Y)/否(N)] ", "Y N");
                    if (continueResult.Status != PromptStatus.OK || 
                        (continueResult.StringResult != "Y" && continueResult.StringResult != "y"))
                    {
                        continueLoop = false;
                    }
                }
                catch (System.Exception ex)
                {
                    editor.WriteMessage($"\n创建查寻参数失败: {ex.Message}");
                    break;
                }
            }
        }

        /// <summary>
        /// 检查是否在块编辑器中
        /// </summary>
        /// <param name="document">CAD文档</param>
        /// <returns>是否在块编辑器中</returns>
        private bool IsInBlockEditor(Document document)
        {
            try
            {
                if (document?.Database == null) return false;

                // 检查当前空间是否为块表记录
                var currentSpace = document.Database.CurrentSpaceId;
                var blockTable = document.Database.BlockTableId;
                
                return currentSpace != blockTable;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"检查块编辑器状态失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 提示用户设置参数属性
        /// </summary>
        /// <returns>是否继续执行</returns>
        private bool PromptParameterProperties()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return false;

                // 使用WPF对话框显示参数属性设置
                var result = ParameterPropertiesDialog.ShowDialog(
                    null, // 没有父窗口
                    _parameterName,
                    _label,
                    _description,
                    _showPalette,
                    _gripCount
                );

                if (result.Success && result.IsValidationPassed)
                {
                    _parameterName = result.ParameterName;
                    _label = result.Label;
                    _description = result.Description;
                    _showPalette = result.ShowPalette;
                    _gripCount = result.GripCount;
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示参数属性失败: {ex.Message}");
                editor.WriteMessage($"\n错误：显示参数属性对话框失败 - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 提示用户指定参数位置
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <returns>参数位置点</returns>
        private Point3d? PromptParameterLocation(Editor editor)
        {
            try
            {
                // 显示当前参数设置
                editor.WriteMessage($"\n当前设置: 名称='{_parameterName}', 标签='{_label}', 夹点数={_gripCount}");

                var promptPointOptions = new PromptPointOptions("\n指定查寻参数位置或 [名称(N)/标签(L)/说明(D)/选项板(P)/退出(X)]:");
                promptPointOptions.Keywords.Add("名称", "N", "N");
                promptPointOptions.Keywords.Add("标签", "L", "L");
                promptPointOptions.Keywords.Add("说明", "D", "D");
                promptPointOptions.Keywords.Add("选项板", "P", "P");
                promptPointOptions.Keywords.Add("退出", "X", "X");

                var result = editor.GetPoint(promptPointOptions);
                if (result.Status == PromptStatus.OK)
                {
                    return result.Value;
                }
                else if (result.Status == PromptStatus.Keyword)
                {
                    // 处理关键字输入
                    return HandleKeywordInput(editor, result.StringResult);
                }

                return null;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示参数位置失败: {ex.Message}");
                editor.WriteMessage($"\n错误：提示参数位置失败 - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 处理关键字输入
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="keyword">关键字</param>
        /// <returns>参数位置点</returns>
        private Point3d? HandleKeywordInput(Editor editor, string keyword)
        {
            try
            {
                switch (keyword.ToUpper())
                {
                    case "N": // 名称
                        PromptParameterName();
                        return PromptParameterLocation(editor);

                    case "L": // 标签
                        PromptParameterLabel();
                        return PromptParameterLocation(editor);

                    case "D": // 说明
                        PromptParameterDescription();
                        return PromptParameterLocation(editor);

                    case "P": // 选项板
                        PromptPaletteOptions();
                        return PromptParameterLocation(editor);

                    case "X": // 退出
                        editor.WriteMessage("\n命令已取消");
                        return null;

                    default:
                        editor.WriteMessage($"\n未知关键字: {keyword}");
                        return PromptParameterLocation(editor);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"处理关键字输入失败: {ex.Message}");
                editor.WriteMessage($"\n错误：处理关键字输入失败 - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 提示参数名称
        /// </summary>
        private void PromptParameterName()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return;

                var promptStringOptions = new PromptStringOptions("\n输入参数名称: ");
                promptStringOptions.DefaultValue = _parameterName;

                var result = editor.GetString(promptStringOptions);
                if (result.Status == PromptStatus.OK)
                {
                    _parameterName = result.StringResult;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示参数名称失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 提示参数标签
        /// </summary>
        private void PromptParameterLabel()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return;

                var promptStringOptions = new PromptStringOptions("\n输入参数标签: ");
                promptStringOptions.DefaultValue = _label;

                var result = editor.GetString(promptStringOptions);
                if (result.Status == PromptStatus.OK)
                {
                    _label = result.StringResult;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示参数标签失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 提示参数说明
        /// </summary>
        private void PromptParameterDescription()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return;

                var promptStringOptions = new PromptStringOptions("\n输入参数说明: ");
                promptStringOptions.DefaultValue = _description;

                var result = editor.GetString(promptStringOptions);
                if (result.Status == PromptStatus.OK)
                {
                    _description = result.StringResult;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示参数说明失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 提示选项板选项
        /// </summary>
        private void PromptPaletteOptions()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return;

                var promptIntegerOptions = new PromptIntegerOptions("\n输入夹点数 [0/1]: ");
                promptIntegerOptions.DefaultValue = _gripCount;
                promptIntegerOptions.LowerLimit = 0;
                promptIntegerOptions.UpperLimit = 1;

                var result = editor.GetInteger(promptIntegerOptions);
                if (result.Status == PromptStatus.OK)
                {
                    _gripCount = result.Value;
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"提示选项板选项失败: {ex.Message}");
            }
        }

        #region 辅助方法

        /// <summary>
        /// 显示参数统计信息
        /// </summary>
        private void ShowParameterStatistics()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null || _pointManager == null) return;

                var stats = _pointManager.GetParameterStatistics();
                editor.WriteMessage($"\n=== 参数统计信息 ===");
                editor.WriteMessage($"参数点总数: {stats.TotalParameterPoints}");
                editor.WriteMessage($"已命名参数: {stats.NamedParameters}");
                editor.WriteMessage($"已标记参数: {stats.LabeledParameters}");
                editor.WriteMessage($"有效参数比例: {stats.ValidParameterRatio:P2}");
                editor.WriteMessage($"已标记比例: {stats.LabeledRatio:P2}");
                editor.WriteMessage("====================");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示参数统计信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 列出所有参数
        /// </summary>
        private void ListAllParameters()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null || _pointManager == null) return;

                var parameters = _pointManager.FindAllParameterPoints();
                if (parameters.Count == 0)
                {
                    editor.WriteMessage("\n未找到任何查寻参数");
                    return;
                }

                editor.WriteMessage($"\n=== 查寻参数列表 ({parameters.Count}个) ===");
                foreach (var point in parameters)
                {
                    var info = _pointManager.GetParameterInfo(point);
                    if (info != null)
                    {
                        editor.WriteMessage($"名称: {info.Name}");
                        editor.WriteMessage($"标签: {info.Label}");
                        editor.WriteMessage($"位置: {info.Location}");
                        editor.WriteMessage($"描述: {info.Description}");
                        editor.WriteMessage("---");
                    }
                }
                editor.WriteMessage("====================================");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"列出所有参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重置参数设置为默认值
        /// </summary>
        private void ResetToDefaults()
        {
            _parameterName = "LookupParameter";
            _label = "标签";
            _description = "查寻参数描述";
            _showPalette = true;
            _gripCount = 1;

            var editor = PluginEntry.GetEditor();
            editor?.WriteMessage("\n参数设置已重置为默认值");
        }

        #endregion

        #region 命令别名

        /// <summary>
        /// BPARAMETER命令的K选项（查寻参数）
        /// </summary>
        [CommandMethod("ZWBPARAMETERK")]
        public void ExecuteKOption()
        {
            // 直接调用主执行方法
            Execute();
        }

        /// <summary>
        /// 快速创建查寻参数（使用默认设置）
        /// </summary>
        [CommandMethod("ZWBPARAMETERQ")]
        public void ExecuteQuick()
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                var doc = PluginEntry.GetActiveDocument();
                
                if (editor == null || doc == null)
                {
                    editor?.WriteMessage("\n错误：无法获取CAD编辑器或文档");
                    return;
                }

                InitializeManagers(doc);

                if (!IsInBlockEditor(doc))
                {
                    editor.WriteMessage("\n错误：请在块编辑器中执行此命令");
                    return;
                }

                // 使用默认设置
                _parameterName = $"LookupParameter{DateTime.Now.Ticks % 1000}";
                _label = "快速参数";
                _description = "快速创建的查寻参数";
                _showPalette = true;
                _gripCount = 1;

                var point = editor.GetPoint("\n指定查寻参数位置: ");
                if (point.Status == PromptStatus.OK)
                {
                    _currentParameter = _parameterCreator.CreateParameter(
                        point.Value, _parameterName, _label, _description, _gripCount, _showPalette
                    );
                    editor.Regen();
                    editor.WriteMessage($"\n快速查寻参数 '{_parameterName}' 已创建");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"快速创建查寻参数失败: {ex.Message}");
                var editor = PluginEntry.GetEditor();
                editor?.WriteMessage($"\n错误：快速创建失败 - {ex.Message}");
            }
        }

        #endregion
    }
}