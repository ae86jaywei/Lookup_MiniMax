using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ZWDynLookup.Commands;
using ZWDynLookup.Service;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 查寻上下文菜单
    /// 交互逻辑
    /// </summary>
    public partial class LookupContextMenu : UserControl
    {
        private BlockReference _targetBlock;
        private Point3d _clickPosition;
        private readonly LookupMenuService _menuService;

        public LookupContextMenu()
        {
            InitializeComponent();
            _menuService = LookupMenuService.Instance;
        }

        /// <summary>
        /// 显示查寻上下文菜单
        /// </summary>
        /// <param name="blockReference">目标块引用</param>
        /// <param name="clickPosition">点击位置</param>
        public void ShowContextMenu(BlockReference blockReference, Point3d clickPosition)
        {
            try
            {
                _targetBlock = blockReference;
                _clickPosition = clickPosition;

                // 检查并更新菜单项状态
                UpdateMenuItemStates();

                // 显示菜单
                var contextMenu = this.FindResource("LookupContextMenu") as ContextMenu;
                if (contextMenu != null)
                {
                    contextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示上下文菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新菜单项状态
        /// </summary>
        private void UpdateMenuItemStates()
        {
            try
            {
                var tagManager = LookupTagManager.Instance;
                var hasTags = tagManager.GetTagsForBlock(_targetBlock).Any();

                // 更新删除标签菜单项
                var deleteTagItem = FindMenuItem("DeleteTag");
                if (deleteTagItem != null)
                {
                    deleteTagItem.IsEnabled = hasTags;
                    deleteTagItem.Visibility = hasTags ? Visibility.Visible : Visibility.Collapsed;
                }

                // 更新显示/隐藏动作菜单项
                var showAllItem = FindMenuItem("ShowAllActions");
                var hideAllItem = FindMenuItem("HideAllActions");
                
                if (showAllItem != null && hideAllItem != null)
                {
                    showAllItem.IsEnabled = hasTags;
                    hideAllItem.IsEnabled = hasTags;
                }

                // 更新重命名动作菜单项
                var renameActionItem = FindMenuItem("RenameAction");
                if (renameActionItem != null)
                {
                    var actionTag = tagManager.GetTag(_targetBlock, "Action");
                    renameActionItem.IsEnabled = actionTag != null;
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新菜单项状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找菜单项
        /// </summary>
        /// <param name="tag">菜单项标签</param>
        /// <returns>菜单项</returns>
        private MenuItem FindMenuItem(string tag)
        {
            try
            {
                var contextMenu = this.FindResource("LookupContextMenu") as ContextMenu;
                if (contextMenu != null)
                {
                    foreach (var item in contextMenu.Items)
                    {
                        if (item is MenuItem menuItem && Equals(menuItem.Tag, tag))
                        {
                            return menuItem;
                        }

                        // 查找子菜单项
                        var subItem = FindSubMenuItem(menuItem, tag);
                        if (subItem != null)
                        {
                            return subItem;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n查找菜单项失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 查找子菜单项
        /// </summary>
        /// <param name="parentItem">父菜单项</param>
        /// <param name="tag">菜单项标签</param>
        /// <returns>子菜单项</returns>
        private MenuItem FindSubMenuItem(MenuItem parentItem, string tag)
        {
            if (parentItem?.Items == null) return null;

            foreach (var item in parentItem.Items)
            {
                if (item is MenuItem menuItem)
                {
                    if (Equals(menuItem.Tag, tag))
                    {
                        return menuItem;
                    }

                    // 递归查找
                    var subItem = FindSubMenuItem(menuItem, tag);
                    if (subItem != null)
                    {
                        return subItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 删除标签菜单项点击事件
        /// </summary>
        private void DeleteTagMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("确定要删除所有查寻标签吗？", 
                                            "确认删除", 
                                            MessageBoxButton.YesNo, 
                                            MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var tagManager = LookupTagManager.Instance;
                    var tags = tagManager.GetTagsForBlock(_targetBlock);
                    
                    foreach (var tag in tags)
                    {
                        tagManager.RemoveTag(_targetBlock, tag.Name);
                    }

                    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n已删除所有查寻标签");
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n删除标签失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 新建选择集菜单项点击事件
        /// </summary>
        private void NewSelectionSetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectionManager = new SelectionSetManager();
                selectionManager.CreateNewSelectionSet();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n新建选择集失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 修改选择集菜单项点击事件
        /// </summary>
        private void ModifySelectionSetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectionManager = new SelectionSetManager();
                selectionManager.ModifySelectionSet();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n修改选择集失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示查寻表菜单项点击事件
        /// </summary>
        private void ShowLookupTableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lookupTableDialog = new LookupTableDialog();
                lookupTableDialog.Show(_targetBlock);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示查寻表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重命名动作菜单项点击事件
        /// </summary>
        private void RenameActionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tagManager = LookupTagManager.Instance;
                var actionTag = tagManager.GetTag(_targetBlock, "Action");
                
                if (actionTag != null)
                {
                    var prompt = new PromptStringOptions($"\n当前动作名称: {actionTag.Name}\n请输入新的动作名称: ");
                    var result = Application.DocumentManager.MdiActiveDocument?.Editor.GetString(prompt);
                    
                    if (result?.Status == PromptStatus.OK && !string.IsNullOrEmpty(result.StringResult))
                    {
                        tagManager.RenameTag(_targetBlock, "Action", result.StringResult);
                        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n动作已重命名为: {result.StringResult}");
                    }
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n重命名动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示所有动作菜单项点击事件
        /// </summary>
        private void ShowAllActionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tagManager = LookupTagManager.Instance;
                tagManager.ShowAllTags();
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n已显示所有动作");
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示所有动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 隐藏所有动作菜单项点击事件
        /// </summary>
        private void HideAllActionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tagManager = LookupTagManager.Instance;
                tagManager.HideAllTags();
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n已隐藏所有动作");
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n隐藏所有动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 属性菜单项点击事件
        /// </summary>
        private void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var propertiesDialog = new PropertiesManagerDialog();
                propertiesDialog.ShowDialog(_targetBlock);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置菜单项点击事件
        /// </summary>
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 显示设置对话框
                var settingsDialog = new SettingsDialog();
                settingsDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前选中块的标签信息
        /// </summary>
        /// <returns>标签信息字符串</returns>
        private string GetBlockTagInfo()
        {
            try
            {
                var tagManager = LookupTagManager.Instance;
                var tags = tagManager.GetTagsForBlock(_targetBlock);
                
                if (!tags.Any())
                {
                    return "无查寻标签";
                }

                var info = new System.Text.StringBuilder();
                info.AppendLine("查寻标签:");
                
                foreach (var tag in tags)
                {
                    info.AppendLine($"  {tag.Name}: {tag.Value} ({tag.Type})");
                }

                return info.ToString();
            }
            catch (Exception ex)
            {
                return $"获取标签信息失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 验证菜单操作
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <returns>验证结果</returns>
        private bool ValidateOperation(string operation)
        {
            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return false;

                // 检查文档锁定状态
                if (document.LockMode)
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n操作 '{operation}' 失败: 文档已锁定");
                    return false;
                }

                // 检查目标块是否有效
                if (_targetBlock == null || _targetBlock.IsDisposed)
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n操作 '{operation}' 失败: 目标块无效");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n验证操作失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 执行操作并处理异常
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <param name="action">要执行的操作</param>
        private void ExecuteOperation(string operation, Action action)
        {
            try
            {
                if (ValidateOperation(operation))
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n执行操作 '{operation}' 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新菜单状态
        /// </summary>
        public void RefreshMenuState()
        {
            UpdateMenuItemStates();
        }

        /// <summary>
        /// 关闭菜单
        /// </summary>
        public void CloseMenu()
        {
            try
            {
                var contextMenu = this.FindResource("LookupContextMenu") as ContextMenu;
                if (contextMenu?.IsOpen == true)
                {
                    contextMenu.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n关闭菜单失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 设置对话框（占位符）
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
            this.Title = "查寻设置";
            this.Width = 400;
            this.Height = 300;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}