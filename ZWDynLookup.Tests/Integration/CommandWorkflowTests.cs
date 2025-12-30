using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ZWDynLookup.Core;
using ZWDynLookup.Core.Models;
using ZWDynLookup.Core.Services;
using ZWDynLookup.Tests.Integration.Helpers;

namespace ZWDynLookup.Tests.Integration
{
    /// <summary>
    /// 命令工作流集成测试
    /// 验证BPARAMETER K和BACTIONTOOL L命令的协作
    /// </summary>
    public class CommandWorkflowTests : IDisposable
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IParameterService _parameterService;
        private readonly IActionService _actionService;
        private readonly ICommandService _commandService;
        private readonly IIntegrationTestHelper _testHelper;

        public CommandWorkflowTests()
        {
            _lookupTableService = new LookupTableService();
            _parameterService = new ParameterService();
            _actionService = new ActionService();
            _commandService = new CommandService();
            _testHelper = new IntegrationTestHelper();
        }

        [Fact]
        public async Task BparameterKCommandSequence_ShouldCreateValidParameter()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Act - 执行BPARAMETER K命令序列
            var parameterCreateCommand = new BparameterKCommand
            {
                Name = "测试参数",
                Type = ParameterType.Lookup,
                LookupTableId = lookupTableId,
                Description = "通过BPARAMETER K命令创建的测试参数",
                DefaultValue = "Default",
                Constraints = new ParameterConstraints
                {
                    Required = true,
                    MinLength = 1,
                    MaxLength = 100
                }
            };

            var commandResult = await _commandService.ExecuteCommandAsync(parameterCreateCommand, commandContext);

            // Assert
            Assert.True(commandResult.Success);
            Assert.NotNull(commandResult.Parameter);
            Assert.Equal(lookupTableId, commandResult.Parameter.LookupTableId);
            Assert.True(commandResult.Parameter.IsActive);
        }

        [Fact]
        public async Task BactiontoolLCommandSequence_ShouldAssociateActionWithParameter()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var parameterId = await _testHelper.CreateTestParameterAsync(lookupTableId);
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Act - 执行BACTIONTOOL L命令
            var actionCreateCommand = new BactiontoolLCommand
            {
                ParameterId = parameterId,
                ActionType = ActionType.Lookup,
                Trigger = "OnSelect",
                ActionData = new Dictionary<string, object>
                {
                    { "Source", "LookupTable" },
                    { "TargetField", "Value" }
                },
                Description = "通过BACTIONTOOL L命令创建的动作"
            };

            var commandResult = await _commandService.ExecuteCommandAsync(actionCreateCommand, commandContext);

            // Assert
            Assert.True(commandResult.Success);
            Assert.NotNull(commandResult.Action);
            Assert.Equal(parameterId, commandResult.Action.ParameterId);
            Assert.Equal(ActionType.Lookup, commandResult.Action.Type);
        }

        [Fact]
        public async Task CompleteCommandWorkflow_ShouldEstablishParameterActionRelationship()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Step 1: 创建查找参数 (BPARAMETER K)
            var parameterCommand = new BparameterKCommand
            {
                Name = "材料类型",
                Type = ParameterType.Lookup,
                LookupTableId = lookupTableId,
                DefaultValue = "钢"
            };

            var parameterResult = await _commandService.ExecuteCommandAsync(parameterCommand, commandContext);
            Assert.True(parameterResult.Success);

            // Step 2: 创建查找动作 (BACTIONTOOL L)
            var actionCommand = new BactiontoolLCommand
            {
                ParameterId = parameterResult.Parameter.Id,
                ActionType = ActionType.Lookup,
                Trigger = "OnValueChanged",
                ActionData = new Dictionary<string, object>
                {
                    { "LookupField", "Value" },
                    { "DisplayField", "Description" }
                }
            };

            var actionResult = await _commandService.ExecuteCommandAsync(actionCommand, commandContext);
            Assert.True(actionResult.Success);

            // Step 3: 验证关联关系
            var parameter = await _parameterService.GetByIdAsync(parameterResult.Parameter.Id);
            var action = await _actionService.GetByIdAsync(actionResult.Action.Id);

            Assert.NotNull(parameter);
            Assert.NotNull(action);
            Assert.Equal(parameter.Id, action.ParameterId);
            Assert.True(parameter.Actions.Any(a => a.Id == action.Id));
        }

        [Fact]
        public async Task MultipleParametersAndActions_ShouldHandleCorrectly()
        {
            // Arrange
            var lookupTableIds = await _testHelper.CreateMultipleLookupTablesAsync(3);
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Act - 创建多个参数和动作
            var results = new List<CommandResult>();

            for (int i = 0; i < 3; i++)
            {
                // 创建参数
                var paramCommand = new BparameterKCommand
                {
                    Name = $"参数{i + 1}",
                    Type = ParameterType.Lookup,
                    LookupTableId = lookupTableIds[i].Id
                };
                var paramResult = await _commandService.ExecuteCommandAsync(paramCommand, commandContext);
                results.Add(paramResult);

                // 创建动作
                var actionCommand = new BactiontoolLCommand
                {
                    ParameterId = paramResult.Parameter.Id,
                    ActionType = ActionType.Lookup,
                    Trigger = $"Trigger{i + 1}",
                    ActionData = new Dictionary<string, object> { { "Index", i } }
                };
                var actionResult = await _commandService.ExecuteCommandAsync(actionCommand, commandContext);
                results.Add(actionResult);
            }

            // Assert
            Assert.All(results, result => Assert.True(result.Success));
            
            // 验证参数和动作的关联
            for (int i = 0; i < 3; i++)
            {
                var param = await _parameterService.GetByIdAsync(results[i * 2].Parameter.Id);
                var action = await _actionService.GetByIdAsync(results[i * 2 + 1].Action.Id);
                
                Assert.NotNull(param);
                Assert.NotNull(action);
                Assert.Equal(param.Id, action.ParameterId);
            }
        }

        [Fact]
        public async Task CommandWithInvalidLookupTable_ShouldFailGracefully()
        {
            // Arrange
            var invalidLookupTableId = "invalid-table-id";
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Act - 尝试使用不存在的查找表
            var command = new BparameterKCommand
            {
                Name = "无效参数",
                Type = ParameterType.Lookup,
                LookupTableId = invalidLookupTableId
            };

            var result = await _commandService.ExecuteCommandAsync(command, commandContext);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Lookup table not found", result.ErrorMessage);
            Assert.Null(result.Parameter);
        }

        [Fact]
        public async Task CommandUndo_ShouldRestorePreviousState()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Step 1: 创建参数
            var paramCommand = new BparameterKCommand
            {
                Name = "撤销测试参数",
                Type = ParameterType.Lookup,
                LookupTableId = lookupTableId
            };
            var paramResult = await _commandService.ExecuteCommandAsync(paramCommand, commandContext);
            var parameterId = paramResult.Parameter.Id;

            // Step 2: 创建动作
            var actionCommand = new BactiontoolLCommand
            {
                ParameterId = parameterId,
                ActionType = ActionType.Lookup,
                Trigger = "OnSelect"
            };
            var actionResult = await _commandService.ExecuteCommandAsync(actionCommand, commandContext);
            var actionId = actionResult.Action.Id;

            // Act - 撤销命令
            await _commandService.UndoCommandAsync(paramResult.CommandId, commandContext);
            await _commandService.UndoCommandAsync(actionResult.CommandId, commandContext);

            // Assert - 验证撤销结果
            var parameter = await _parameterService.GetByIdAsync(parameterId);
            var action = await _actionService.GetByIdAsync(actionId);

            Assert.False(parameter.IsActive);
            Assert.False(action.IsActive);
        }

        [Fact]
        public async Task CommandHistory_ShouldTrackAllOperations()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            var commandIds = new List<string>();

            // Act - 执行多个命令
            var commands = new[]
            {
                new BparameterKCommand { Name = "历史测试参数1", Type = ParameterType.Lookup, LookupTableId = lookupTableId },
                new BparameterKCommand { Name = "历史测试参数2", Type = ParameterType.Lookup, LookupTableId = lookupTableId },
                new BparameterKCommand { Name = "历史测试参数3", Type = ParameterType.Lookup, LookupTableId = lookupTableId }
            };

            foreach (var command in commands)
            {
                var result = await _commandService.ExecuteCommandAsync(command, commandContext);
                commandIds.Add(result.CommandId);
            }

            // 获取命令历史
            var history = await _commandService.GetCommandHistoryAsync(commandContext.SessionId);

            // Assert
            Assert.Equal(3, history.Count);
            Assert.All(commandIds, id => Assert.Contains(history, h => h.Id == id));
        }

        [Fact]
        public async Task CommandValidation_ShouldPreventInvalidOperations()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Act & Assert - 测试各种无效命令
            var invalidCommands = new[]
            {
                new BparameterKCommand { Name = "", Type = ParameterType.Lookup, LookupTableId = lookupTableId },
                new BparameterKCommand { Name = null, Type = ParameterType.Lookup, LookupTableId = lookupTableId },
                new BparameterKCommand { Name = "ValidName", Type = ParameterType.Lookup, LookupTableId = null },
                new BactiontoolLCommand { ParameterId = null, ActionType = ActionType.Lookup, Trigger = "OnSelect" }
            };

            foreach (var command in invalidCommands)
            {
                var result = await _commandService.ExecuteCommandAsync(command, commandContext);
                Assert.False(result.Success, $"Command should fail: {command.GetType().Name}");
            }
        }

        [Fact]
        public async Task CommandPerformance_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var lookupTableId = await _testHelper.CreateTestLookupTableAsync();
            var commandContext = new CommandContext
            {
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now
            };

            // Act - 执行复杂命令
            var startTime = DateTime.Now;
            var command = new BparameterKCommand
            {
                Name = "性能测试参数",
                Type = ParameterType.Lookup,
                LookupTableId = lookupTableId,
                Description = "用于性能测试的详细描述参数，包含大量配置信息以测试系统性能",
                Constraints = new ParameterConstraints
                {
                    Required = true,
                    MinLength = 1,
                    MaxLength = 1000,
                    Pattern = "^[A-Za-z0-9_]+$"
                },
                DefaultValue = "默认值"
            };

            var result = await _commandService.ExecuteCommandAsync(command, commandContext);
            var endTime = DateTime.Now;

            // Assert
            Assert.True(result.Success);
            Assert.True((endTime - startTime).TotalMilliseconds < 1000, "命令执行应该在1秒内完成");
        }

        public void Dispose()
        {
            _testHelper.CleanupAsync().Wait();
        }
    }

    // 命令类定义
    public class BparameterKCommand
    {
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public string LookupTableId { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public ParameterConstraints Constraints { get; set; }
    }

    public class BactiontoolLCommand
    {
        public string ParameterId { get; set; }
        public ActionType ActionType { get; set; }
        public string Trigger { get; set; }
        public Dictionary<string, object> ActionData { get; set; }
        public string Description { get; set; }
    }

    public class CommandResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public ParameterDto Parameter { get; set; }
        public ActionDto Action { get; set; }
        public string CommandId { get; set; }
    }
}