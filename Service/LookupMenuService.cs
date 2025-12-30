using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using ZWDynLookup.Commands;
using ZWDynLookup.UI;

namespace ZWDynLookup.Service
{
    /// <summary>
    /// 查寻菜单服务
    /// 负责管理查寻上下文菜单和运行时菜单
    /// </summary>
    public class LookupMenuService
    {
        private static LookupMenuService _instance;
        private readonly List<MenuItem> _contextMenuItems = new List<MenuItem>();
        private readonly List<MenuItem> _runtimeMenuItems = new List<MenuItem>();

        public static LookupMenuService Instance => _instance ??= new LookupMenuService();

        public LookupMenuService()
        {
            InitializeContextMenuItems();
            InitializeRuntimeMenuItems();
        }

        /// <summary>
        /// 初始化上下文菜单项
        /// </summary>
        private void InitializeContextMenuItems()
        {
            _contextMenuItems.Add(new MenuItem
            {
                Name = "删除查寻标签",
                Command = "ZW_DEL_LOOKUP_TAG",
                Icon = "Delete"
            });

            _contextMenuItems.Add(new MenuItem
            {
                Name = "动作选择集",
                SubItems = new List<MenuItem>
                {
                    new MenuItem { Name = "新建选择集", Command = "ZW_NEW_SELECTION_SET" },
                    new MenuItem { Name = "修改选择集", Command = "ZW_MODIFY_SELECTION_SET" }
                }
            });

            _contextMenuItems.Add(new MenuItem
            {
                Name = "显示查寻表",
                Command = "ZW_SHOW_LOOKUP_TABLE"
            });

            _contextMenuItems.Add(new MenuItem
            {
                Name = "重命名动作",
                Command = "ZW_RENAME_ACTION"
            });

            _contextMenuItems.Add(new MenuItem
            {
                Name = "显示所有动作",
                Command = "ZW_SHOW_ALL_ACTIONS"
            });

            _contextMenuItems.Add(new MenuItem
            {
                Name = "隐藏所有动作",
                Command = "ZW_HIDE_ALL_ACTIONS"
            });
        }

        /// <summary>
        /// 初始化运行时菜单项
        /// </summary>
        private void InitializeRuntimeMenuItems()
        {
            _runtimeMenuItems.Add(new MenuItem
            {
                Name = "切换到动作1",
                Command = "ZW_SWITCH_ACTION_1"
            });

            _runtimeMenuItems.Add(new MenuItem
            {
                Name = "切换到动作2",
                Command = "ZW_SWITCH_ACTION_2"
            });

            _runtimeMenuItems.Add(new MenuItem
            {
                Name = "切换到动作3",
                Command = "ZW_SWITCH_ACTION_3"
            });

            _runtimeMenuItems.Add(new MenuItem
            {
                Name = "查寻参数...",
                Command = "ZW_LOOKUP_PARAMETER"
            });
        }

        /// <summary>
        /// 显示查寻上下文菜单
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="mousePosition">鼠标位置</param>
        public void ShowContextMenu(BlockReference blockReference, Point3d mousePosition)
        {
            var contextMenu = new ContextMenu();

            try
            {
                // 添加基本菜单项
                foreach (var item in _contextMenuItems)
                {
                    if (item.SubItems == null)
                    {
                        var menuItem = new MenuItem(item.Name);
                        menuItem.Click += (s, e) => ExecuteMenuCommand(item.Command, blockReference);
                        contextMenu.Items.Add(menuItem);
                    }
                    else
                    {
                        // 添加子菜单
                        var subMenu = new MenuItem(item.Name);
                        foreach (var subItem in item.SubItems)
                        {
                            var subMenuItem = new MenuItem(subItem.Name);
                            subMenuItem.Click += (s, e) => ExecuteMenuCommand(subItem.Command, blockReference);
                            subMenu.DropDownItems.Add(subMenuItem);
                        }
                        contextMenu.Items.Add(subMenu);
                    }
                }

                // 在鼠标位置显示菜单
                var screenPosition = Application.DocumentManager.MdiActiveDocument?.Editor?.ViewportToScreen(mousePosition);
                if (screenPosition.HasValue)
                {
                    contextMenu.Show(Application.MainWindow.Handle, screenPosition.Value);
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示上下文菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示运行时查寻菜单
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="gripPoint">夹点位置</param>
        public void ShowRuntimeMenu(BlockReference blockReference, Point3d gripPoint)
        {
            var runtimeMenu = new ContextMenu();

            try
            {
                // 获取当前有效的动作列表
                var availableActions = GetAvailableActions(blockReference);

                // 动态添加可用动作
                foreach (var action in availableActions)
                {
                    var menuItem = new MenuItem($"切换到: {action.Name}");
                    menuItem.Click += (s, e) => SwitchToAction(blockReference, action.Name);
                    runtimeMenu.Items.Add(menuItem);
                }

                // 添加分隔线
                if (runtimeMenu.Items.Count > 0)
                {
                    runtimeMenu.Items.Add(new ToolStripSeparator());
                }

                // 添加其他菜单项
                foreach (var item in _runtimeMenuItems)
                {
                    var menuItem = new MenuItem(item.Name);
                    menuItem.Click += (s, e) => ExecuteMenuCommand(item.Command, blockReference);
                    runtimeMenu.Items.Add(menuItem);
                }

                // 显示菜单
                var screenPosition = Application.DocumentManager.MdiActiveDocument?.Editor?.ViewportToScreen(gripPoint);
                if (screenPosition.HasValue)
                {
                    runtimeMenu.Show(Application.MainWindow.Handle, screenPosition.Value);
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示运行时菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行菜单命令
        /// </summary>
        /// <param name="command">命令字符串</param>
        /// <param name="blockReference">块引用</param>
        private void ExecuteMenuCommand(string command, BlockReference blockReference)
        {
            try
            {
                switch (command)
                {
                    case "ZW_DEL_LOOKUP_TAG":
                        DeleteLookupTag(blockReference);
                        break;
                    case "ZW_NEW_SELECTION_SET":
                        CreateNewSelectionSet();
                        break;
                    case "ZW_MODIFY_SELECTION_SET":
                        ModifySelectionSet();
                        break;
                    case "ZW_SHOW_LOOKUP_TABLE":
                        ShowLookupTable(blockReference);
                        break;
                    case "ZW_RENAME_ACTION":
                        RenameAction(blockReference);
                        break;
                    case "ZW_SHOW_ALL_ACTIONS":
                        ShowAllActions();
                        break;
                    case "ZW_HIDE_ALL_ACTIONS":
                        HideAllActions();
                        break;
                    case "ZW_LOOKUP_PARAMETER":
                        ShowLookupParameterDialog(blockReference);
                        break;
                    default:
                        Application.DocumentManager.MdiActiveDocument?.Editor.Command(command);
                        break;
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n执行菜单命令失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 切换到指定动作
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="actionName">动作名称</param>
        private void SwitchToAction(BlockReference blockReference, string actionName)
        {
            try
            {
                // 切换动作逻辑
                var tagManager = LookupTagManager.Instance;
                var tag = tagManager.GetTag(blockReference, "Action");
                if (tag != null)
                {
                    tagManager.UpdateTagValue(blockReference, "Action", actionName);
                    GraphicsUpdateService.Instance.UpdateGraphics(blockReference);
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n切换动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除查寻标签
        /// </summary>
        /// <param name="blockReference">块引用</param>
        private void DeleteLookupTag(BlockReference blockReference)
        {
            var tagManager = LookupTagManager.Instance;
            var tags = tagManager.GetTagsForBlock(blockReference);
            
            foreach (var tag in tags)
            {
                tagManager.RemoveTag(blockReference, tag.Name);
            }
        }

        /// <summary>
        /// 创建新选择集
        /// </summary>
        private void CreateNewSelectionSet()
        {
            // 调用选择集管理器创建新选择集
            var selectionManager = new SelectionSetManager();
            selectionManager.CreateNewSelectionSet();
        }

        /// <summary>
        /// 修改选择集
        /// </summary>
        private void ModifySelectionSet()
        {
            // 调用选择集管理器修改选择集
            var selectionManager = new SelectionSetManager();
            selectionManager.ModifySelectionSet();
        }

        /// <summary>
        /// 显示查寻表
        /// </summary>
        /// <param name="blockReference">块引用</param>
        private void ShowLookupTable(BlockReference blockReference)
        {
            var lookupTableDialog = new LookupTableDialog();
            lookupTableDialog.Show(blockReference);
        }

        /// <summary>
        /// 重命名动作
        /// </summary>
        /// <param name="blockReference">块引用</param>
        private void RenameAction(BlockReference blockReference)
        {
            var prompt = new PromptStringOptions("\n请输入新的动作名称: ");
            var result = Application.DocumentManager.MdiActiveDocument?.Editor.GetString(prompt);
            
            if (result?.Status == PromptStatus.OK && !string.IsNullOrEmpty(result.StringResult))
            {
                var tagManager = LookupTagManager.Instance;
                tagManager.RenameTag(blockReference, "Action", result.StringResult);
            }
        }

        /// <summary>
        /// 显示所有动作
        /// </summary>
        private void ShowAllActions()
        {
            var tagManager = LookupTagManager.Instance;
            tagManager.ShowAllTags();
        }

        /// <summary>
        /// 隐藏所有动作
        /// </summary>
        private void HideAllActions()
        {
            var tagManager = LookupTagManager.Instance;
            tagManager.HideAllTags();
        }

        /// <summary>
        /// 显示查寻参数对话框
        /// </summary>
        /// <param name="blockReference">块引用</param>
        private void ShowLookupParameterDialog(BlockReference blockReference)
        {
            var parameterDialog = new ParameterPropertiesDialog();
            parameterDialog.ShowDialog(blockReference);
        }

        /// <summary>
        /// 获取可用动作列表
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>动作列表</returns>
        private List<LookupAction> GetAvailableActions(BlockReference blockReference)
        {
            var actions = new List<LookupAction>();
            
            try
            {
                // 从块定义中获取查寻动作
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null) return actions;

                using (var trans = document.Database.TransactionManager.StartTransaction())
                {
                    var blockTable = trans.GetObject(document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null && blockTable.Has(blockReference.Name))
                    {
                        var blockDef = trans.GetObject(blockTable[blockReference.Name], OpenMode.ForRead) as BlockTableRecord;
                        if (blockDef != null)
                        {
                            // 解析查寻动作（这里需要根据实际的查寻动作存储格式来实现）
                            actions.Add(new LookupAction { Name = "动作1", IsActive = true });
                            actions.Add(new LookupAction { Name = "动作2", IsActive = false });
                            actions.Add(new LookupAction { Name = "动作3", IsActive = false });
                        }
                    }
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取可用动作失败: {ex.Message}");
            }

            return actions;
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        public void RegisterShortcuts()
        {
            // 注册菜单快捷键
            // 这里可以添加快捷键注册逻辑
        }

        /// <summary>
        /// 注销菜单
        /// </summary>
        public void UnregisterMenus()
        {
            _contextMenuItems.Clear();
            _runtimeMenuItems.Clear();
        }
    }

    /// <summary>
    /// 菜单项
    /// </summary>
    public class MenuItem
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Icon { get; set; }
        public List<MenuItem> SubItems { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 查寻动作
    /// </summary>
    public class LookupAction
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}