using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Runtime;

namespace ZWDynLookup
{
    /// <summary>
    /// 中望CAD动态块查寻插件主入口类
    /// </summary>
    [Guid("A1B2C3D4-E5F6-7890-ABCD-123456789012")]
    [Assembly: AssemblyVersion("1.0.0.0")]
    [Assembly: AssemblyFileVersion("1.0.0.0")]
    public class PluginEntry : IExtensionApplication
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ZWDynLookup", "Plugin.log");

        /// <summary>
        /// 插件初始化
        /// </summary>
        public void Initialize()
        {
            try
            {
                Log("插件初始化开始");

                // 创建日志目录
                var logDir = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // 注册命令
                RegisterCommands();

                // 初始化插件界面
                InitializeUI();

                Log("插件初始化完成");
            }
            catch (System.Exception ex)
            {
                Log($"插件初始化失败: {ex.Message}");
                MessageBox.Show($"插件初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 插件卸载
        /// </summary>
        public void Terminate()
        {
            try
            {
                Log("插件卸载开始");

                // 清理资源
                Cleanup();

                Log("插件卸载完成");
            }
            catch (System.Exception ex)
            {
                Log($"插件卸载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册所有命令
        /// </summary>
        private void RegisterCommands()
        {
            try
            {
                // 注册查寻参数命令
                CommandMethodService.RegisterCommand("ZWBPARAMETER", typeof(BParameterCommand), 
寻参数", "                    "创建查BPARAMETER", "创建动态块查寻参数");

                // 注册查寻动作命令
                CommandMethodService.RegisterCommand("ZWBACTIONTOOL", typeof(BActionToolCommand), 
                    "创建查寻动作", "BACTIONTOOL", "创建动态块查寻动作");

                // 注册查寻表管理命令
                CommandMethodService.RegisterCommand("ZWBLOOKUPTABLE", typeof(LookupTableCommand), 
                    "管理查寻表", "BLOOKUPTABLE", "管理动态块查寻表");

                // 注册特性管理命令
                CommandMethodService.RegisterCommand("ZWBPROPERTIES", typeof(PropertiesCommand), 
                    "管理参数特性", "BPROPERTIES", "管理动态块参数特性");

                Log("命令注册完成");
            }
            catch (System.Exception ex)
            {
                Log($"命令注册失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化插件界面
        /// </summary>
        private void InitializeUI()
        {
            try
            {
                // 创建工具栏和菜单项
                UIHelper.CreatePluginMenu();

                Log("插件界面初始化完成");
            }
            catch (System.Exception ex)
            {
                Log($"界面初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            try
            {
                // 清理命令注册
                CommandMethodService.UnregisterCommands();

                // 清理界面资源
                UIHelper.Cleanup();

                Log("资源清理完成");
            }
            catch (System.Exception ex)
            {
                Log($"资源清理失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Log(string message)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // 静默处理日志写入失败
            }
        }

        /// <summary>
        /// 获取当前活动的CAD文档
        /// </summary>
        /// <returns>CAD文档对象</returns>
        public static Document? GetActiveDocument()
        {
            try
            {
                var app = Application.AcadApplication as Application;
                return app?.ActiveDocument;
            }
            catch (System.Exception ex)
            {
                Log($"获取活动文档失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取CAD编辑器
        /// </summary>
        /// <returns>CAD编辑器对象</returns>
        public static Editor? GetEditor()
        {
            try
            {
                var doc = GetActiveDocument();
                return doc?.Editor;
            }
            catch (System.Exception ex)
            {
                Log($"获取编辑器失败: {ex.Message}");
                return null;
            }
        }
    }
}