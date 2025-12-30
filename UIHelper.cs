using System;
using System.Windows.Forms;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;

namespace ZWDynLookup
{
    /// <summary>
    /// UI辅助类
    /// </summary>
    public static class UIHelper
    {
        private static ToolStripMenuItem? _pluginMenuItem;
        private static ToolStripMenuItem? _bParameterMenuItem;
        private static ToolStripMenuItem? _bActionToolMenuItem;
        private static ToolStripMenuItem? _bLookupTableMenuItem;
        private static ToolStripMenuItem? _bPropertiesMenuItem;

        /// <summary>
        /// 创建插件菜单
        /// </summary>
        public static void CreatePluginMenu()
        {
            try
            {
                var app = Application.AcadApplication as Application;
                if (app == null) return;

                var menuBar = app.MenuBar;
                if (menuBar == null) return;

                // 检查是否已存在插件菜单
                foreach (MenuItem menuItem in menuBar)
                {
                    if (menuItem.Caption == "动态块查寻(&D)")
                    {
                        PluginEntry.Log("插件菜单已存在，跳过创建");
                        return;
                    }
                }

                // 创建主菜单项
                _pluginMenuItem = new MenuItem();
                _pluginMenuItem.Caption = "动态块查寻(&D)";

                // 创建子菜单项
                CreateSubMenuItems();

                // 添加到菜单栏
                menuBar.Add(_pluginMenuItem);

                PluginEntry.Log("插件菜单创建完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建插件菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建子菜单项
        /// </summary>
        private static void CreateSubMenuItems()
        {
            try
            {
                if (_pluginMenuItem == null) return;

                // BPARAMETER菜单项
                _bParameterMenuItem = new MenuItem();
                _bParameterMenuItem.Caption = "创建查寻参数(&P)";
                _bParameterMenuItem.Click += (sender, e) => ExecuteCommand("ZWBPARAMETER");
                _pluginMenuItem.MenuItems.Add(_bParameterMenuItem);

                // BACTIONTOOL菜单项
                _bActionToolMenuItem = new MenuItem();
                _bActionToolMenuItem.Caption = "创建查寻动作(&A)";
                _bActionToolMenuItem.Click += (sender, e) => ExecuteCommand("ZWBACTIONTOOL");
                _pluginMenuItem.MenuItems.Add(_bActionToolMenuItem);

                // 分隔线
                _pluginMenuItem.MenuItems.Add("-");

                // BLOOKUPTABLE菜单项
                _bLookupTableMenuItem = new MenuItem();
                _bLookupTableMenuItem.Caption = "管理查寻表(&L)";
                _bLookupTableMenuItem.Click += (sender, e) => ExecuteCommand("ZWBLOOKUPTABLE");
                _pluginMenuItem.MenuItems.Add(_bLookupTableMenuItem);

                // BPROPERTIES菜单项
                _bPropertiesMenuItem = new MenuItem();
                _bPropertiesMenuItem.Caption = "管理参数特性(&R)";
                _bPropertiesMenuItem.Click += (sender, e) => ExecuteCommand("ZWBPROPERTIES");
                _pluginMenuItem.MenuItems.Add(_bPropertiesMenuItem);

                // 分隔线
                _pluginMenuItem.MenuItems.Add("-");

                // 关于菜单项
                var aboutMenuItem = new MenuItem();
                aboutMenuItem.Caption = "关于(&A)";
                aboutMenuItem.Click += (sender, e) => ShowAboutDialog();
                _pluginMenuItem.MenuItems.Add(aboutMenuItem);

                PluginEntry.Log("子菜单项创建完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建子菜单项失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        private static void ExecuteCommand(string commandName)
        {
            try
            {
                var editor = PluginEntry.GetEditor();
                if (editor == null) return;

                editor.SendStringToExecute(commandName + " ");
                PluginEntry.Log($"命令已发送: {commandName}");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"执行命令失败: {commandName} - {ex.Message}");
                MessageBox.Show($"执行命令失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 显示关于对话框
        /// </summary>
        private static void ShowAboutDialog()
        {
            try
            {
                var message = "中望CAD动态块查寻插件\n\n" +
                             "版本: 1.0.0\n" +
                             "功能: 提供动态块查寻参数和动作的创建和管理\n\n" +
                             "支持的命令:\n" +
                             "• ZWBPARAMETER - 创建查寻参数\n" +
                             "• ZWBACTIONTOOL - 创建查寻动作\n" +
                             "• ZWBLOOKUPTABLE - 管理查寻表\n" +
                             "• ZWBPROPERTIES - 管理参数特性\n\n" +
                             "Copyright © 2024";

                MessageBox.Show(message, "关于动态块查寻插件", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示关于对话框失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理UI资源
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                var app = Application.AcadApplication as Application;
                if (app == null) return;

                var menuBar = app.MenuBar;
                if (menuBar == null) return;

                // 移除插件菜单
                for (int i = menuBar.Count - 1; i >= 0; i--)
                {
                    var menuItem = menuBar[i];
                    if (menuItem.Caption == "动态块查寻(&D)")
                    {
                        menuBar.Remove(i);
                        PluginEntry.Log("插件菜单已移除");
                        break;
                    }
                }

                _pluginMenuItem = null;
                _bParameterMenuItem = null;
                _bActionToolMenuItem = null;
                _bLookupTableMenuItem = null;
                _bPropertiesMenuItem = null;

                PluginEntry.Log("UI资源清理完成");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"清理UI资源失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="title">标题</param>
        public static void ShowError(string message, string title = "错误")
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示错误消息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示信息消息
        /// </summary>
        /// <param name="message">信息消息</param>
        /// <param name="title">标题</param>
        public static void ShowInfo(string message, string title = "信息")
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示信息消息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="message">警告消息</param>
        /// <param name="title">标题</param>
        public static void ShowWarning(string message, string title = "警告")
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示警告消息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="message">确认消息</param>
        /// <param name="title">标题</param>
        /// <returns>用户选择结果</returns>
        public static bool ShowConfirm(string message, string title = "确认")
        {
            try
            {
                var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return result == DialogResult.Yes;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"显示确认对话框失败: {ex.Message}");
                return false;
            }
        }
    }
}