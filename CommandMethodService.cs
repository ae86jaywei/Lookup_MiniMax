using System;
using System.Collections.Generic;
using System.Reflection;
using ZwSoft.ZwCAD.Runtime;

namespace ZWDynLookup
{
    /// <summary>
    /// 命令注册服务
    /// </summary>
    public static class CommandMethodService
    {
        private static readonly List<CommandMethod> _registeredCommands = new List<CommandMethod>();

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="globalName">全局名称</param>
        /// <param name="localName">本地名称</param>
        /// <param name="description">描述</param>
        public static void RegisterCommand(string commandName, Type commandType, string globalName, string localName, string description)
        {
            try
            {
                var method = new CommandMethod(commandName, commandType, globalName, localName, description);
                _registeredCommands.Add(method);

                PluginEntry.Log($"注册命令: {commandName} ({globalName})");
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"注册命令失败: {commandName} - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 注销所有命令
        /// </summary>
        public static void UnregisterCommands()
        {
            try
            {
                foreach (var command in _registeredCommands)
                {
                    PluginEntry.Log($"注销命令: {command.CommandName}");
                }
                _registeredCommands.Clear();
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"注销命令失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取已注册的命令列表
        /// </summary>
        /// <returns>命令列表</returns>
        public static IReadOnlyList<CommandMethod> GetRegisteredCommands()
        {
            return _registeredCommands.AsReadOnly();
        }
    }

    /// <summary>
    /// 命令方法信息
    /// </summary>
    public class CommandMethod
    {
        public string CommandName { get; }
        public Type CommandType { get; }
        public string GlobalName { get; }
        public string LocalName { get; }
        public string Description { get; }

        public CommandMethod(string commandName, Type commandType, string globalName, string localName, string description)
        {
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            GlobalName = globalName ?? throw new ArgumentNullException(nameof(globalName));
            LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public void Execute()
        {
            try
            {
                var instance = Activator.CreateInstance(CommandType);
                var method = CommandType.GetMethod("Execute");
                
                if (method != null)
                {
                    method.Invoke(instance, null);
                }
                else
                {
                    throw new InvalidOperationException($"未找到Execute方法: {CommandType.FullName}");
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"执行命令失败: {CommandName} - {ex.Message}");
                throw;
            }
        }
    }
}