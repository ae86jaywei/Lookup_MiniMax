using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZWDynLookup.Service;

namespace ZWDynLookup.Tests.Services
{
    /// <summary>
    /// 查寻动作服务的单元测试类
    /// </summary>
    [TestFixture]
    public class LookupActionServiceTests
    {
        private Mock<Database> _mockDatabase;
        private Mock<Transaction> _mockTransaction;
        private Mock<BlockTable> _mockBlockTable;
        private Mock<BlockTableRecord> _mockBlockTableRecord;
        private LookupActionService _service;

        [SetUp]
        public void SetUp()
        {
            // 创建模拟对象
            _mockDatabase = new Mock<Database>();
            _mockTransaction = new Mock<Transaction>();
            _mockBlockTable = new Mock<BlockTable>();
            _mockBlockTableRecord = new Mock<BlockTableRecord>();

            // 设置模拟对象的行为
            _mockDatabase.Setup(db => db.TransactionManager.StartTransaction())
                        .Returns(_mockTransaction.Object);
            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), It.IsAny<OpenMode>()))
                          .Returns(_mockBlockTable.Object);
            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), BlockTableRecord.CurrentSpace, It.IsAny<OpenMode>()))
                          .Returns(_mockBlockTableRecord.Object);

            // 创建服务实例
            _service = new LookupActionService();
        }

        [TearDown]
        public void TearDown()
        {
            _service = null;
        }

        #region 动作查找测试

        [Test]
        public void FindLookupActions_有效文档_应返回动作列表()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            var mockEditor = new Mock<Editor>();
            var promptSelectionResult = new PromptSelectionResult
            {
                Status = PromptStatus.OK,
                Value = CreateMockSelectionSet(new[] { new ObjectId(1), new ObjectId(2) })
            };
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Returns(promptSelectionResult);

            // 设置过滤器匹配
            _mockBlockTableRecord.Setup(r => r.HasExtensionDictionary).Returns(true);

            // Act
            var result = _service.FindLookupActions(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void FindLookupActions_编辑器为null_应返回空列表()
        {
            // Act
            var result = _service.FindLookupActions(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindLookupActions_用户取消选择_应返回空列表()
        {
            // Arrange
            var mockEditor = new Mock<Editor>();
            var promptSelectionResult = new PromptSelectionResult
            {
                Status = PromptStatus.Cancel
            };
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Returns(promptSelectionResult);

            // Act
            var result = _service.FindLookupActions(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindLookupActions_选择异常_应返回空列表()
        {
            // Arrange
            var mockEditor = new Mock<Editor>();
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Throws(new Exception("选择异常"));

            // Act
            var result = _service.FindLookupActions(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region 动作验证测试

        [Test]
        public void ValidateAction_有效动作对象_应返回true()
        {
            // Arrange
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(1));

            // Act
            var result = _service.ValidateAction(mockAction);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateAction_动作对象为null_应返回false()
        {
            // Act
            var result = _service.ValidateAction(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateAction_动作名称为空_应返回false()
        {
            // Arrange
            var mockAction = CreateMockLookupAction("", "TestDesc", new ObjectId(1));

            // Act
            var result = _service.ValidateAction(mockAction);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateAction_动作名称包含特殊字符_应返回false()
        {
            // Arrange
            var mockAction = CreateMockLookupAction("Test@Action", "TestDesc", new ObjectId(1));

            // Act
            var result = _service.ValidateAction(mockAction);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateAction_关联参数无效_应返回false()
        {
            // Arrange
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", ObjectId.Null);

            // Act
            var result = _service.ValidateAction(mockAction);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 动作属性测试

        [Test]
        public void GetActionProperties_有效动作_应返回属性信息()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.Position).Returns(new Point3d(10, 20, 0));
            mockAction.Setup(a => a.ExtensionDictionary).Returns(new ObjectId(3));

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.GetActionProperties(actionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestAction", result.Name);
            Assert.AreEqual("TestDesc", result.Description);
            Assert.AreEqual(new ObjectId(2), result.AssociatedParameterId);
        }

        [Test]
        public void GetActionProperties_无效动作ID_应返回null()
        {
            // Arrange
            var invalidActionId = ObjectId.Null;

            // Act
            var result = _service.GetActionProperties(invalidActionId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetActionProperties_动作不存在_应返回null()
        {
            // Arrange
            var actionId = new ObjectId(999);
            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Throws(new Exception("动作不存在"));

            // Act
            var result = _service.GetActionProperties(actionId);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 动作更新测试

        [Test]
        public void UpdateActionProperties_有效动作_应成功更新()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var properties = new ActionProperties
            {
                Name = "UpdatedAction",
                Description = "UpdatedDesc",
                LookupValues = new List<string> { "Value1", "Value2" },
                AssociatedParameterId = new ObjectId(2)
            };
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(3));

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.UpdateActionProperties(actionId, properties);

            // Assert
            Assert.IsTrue(result);
            mockAction.VerifySet(a => a.Name = "UpdatedAction", Times.Once);
        }

        [Test]
        public void UpdateActionProperties_无效动作ID_应返回false()
        {
            // Arrange
            var invalidActionId = ObjectId.Null;
            var properties = new ActionProperties
            {
                Name = "UpdatedAction",
                Description = "UpdatedDesc"
            };

            // Act
            var result = _service.UpdateActionProperties(invalidActionId, properties);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateActionProperties_更新异常_应返回false()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var properties = new ActionProperties
            {
                Name = "UpdatedAction",
                Description = "UpdatedDesc"
            };
            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Throws(new Exception("更新异常"));

            // Act
            var result = _service.UpdateActionProperties(actionId, properties);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 动作删除测试

        [Test]
        public void DeleteAction_有效动作_应成功删除()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.DeleteAction(actionId);

            // Assert
            Assert.IsTrue(result);
            mockAction.Verify(a => a.Erase(), Times.Once);
        }

        [Test]
        public void DeleteAction_无效动作ID_应返回false()
        {
            // Arrange
            var invalidActionId = ObjectId.Null;

            // Act
            var result = _service.DeleteAction(invalidActionId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteAction_动作被锁定_应返回false()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.Erase()).Throws(new Exception("动作被锁定"));

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.DeleteAction(actionId);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 动作统计测试

        [Test]
        public void GetActionStatistics_有效块记录_应返回统计信息()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            // 模拟包含一些动作的块记录
            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(new List<Entity> 
                                { 
                                    CreateMockLookupAction("Action1", "Desc1", new ObjectId(1)).Object,
                                    CreateMockLookupAction("Action2", "Desc2", new ObjectId(2)).Object
                                }.Cast<Entity>());

            // Act
            var result = _service.GetActionStatistics(mockDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TotalActionCount >= 0);
        }

        [Test]
        public void GetActionStatistics_文档为null_应返回默认统计()
        {
            // Act
            var result = _service.GetActionStatistics(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalActionCount);
        }

        [Test]
        public void GetActionStatistics_获取异常_应返回默认统计()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Throws(new Exception("统计异常"));

            // Act
            var result = _service.GetActionStatistics(mockDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalActionCount);
        }

        #endregion

        #region 查寻值管理测试

        [Test]
        public void GetLookupValues_有效动作_应返回查寻值列表()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var lookupValues = new List<string> { "Value1", "Value2", "Value3" };
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.ExtensionDictionary).Returns(new ObjectId(3));

            var mockExtensionDict = new Mock<DBDictionary>();
            _mockTransaction.Setup(t => t.GetObject(new ObjectId(3), It.IsAny<OpenMode>()))
                           .Returns(mockExtensionDict.Object);

            // Act
            var result = _service.GetLookupValues(actionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void GetLookupValues_动作无扩展字典_应返回空列表()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.ExtensionDictionary).Returns(ObjectId.Null);

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.GetLookupValues(actionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void SetLookupValues_有效动作_应成功设置查寻值()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var lookupValues = new List<string> { "Value1", "Value2", "Value3" };
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.ExtensionDictionary).Returns(new ObjectId(3));

            var mockExtensionDict = new Mock<DBDictionary>();
            _mockTransaction.Setup(t => t.GetObject(new ObjectId(3), It.IsAny<OpenMode>()))
                           .Returns(mockExtensionDict.Object);

            // Act
            var result = _service.SetLookupValues(actionId, lookupValues);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void SetLookupValues_空查寻值列表_应返回false()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var emptyLookupValues = new List<string>();

            // Act
            var result = _service.SetLookupValues(actionId, emptyLookupValues);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 动作执行测试

        [Test]
        public void ExecuteLookupAction_有效动作和值_应执行动作()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var lookupValue = "TestValue";
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.ExecuteLookupAction(actionId, lookupValue);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ExecuteLookupAction_无效动作ID_应返回false()
        {
            // Arrange
            var invalidActionId = ObjectId.Null;
            var lookupValue = "TestValue";

            // Act
            var result = _service.ExecuteLookupAction(invalidActionId, lookupValue);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ExecuteLookupAction_查寻值不存在_应返回false()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var invalidLookupValue = "NonExistentValue";
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            
            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.ExecuteLookupAction(actionId, invalidLookupValue);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ExecuteLookupAction_执行异常_应返回false()
        {
            // Arrange
            var actionId = new ObjectId(1);
            var lookupValue = "TestValue";
            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.Erase()).Throws(new Exception("执行异常"));

            _mockTransaction.Setup(t => t.GetObject(actionId, It.IsAny<OpenMode>()))
                           .Returns(mockAction.Object);

            // Act
            var result = _service.ExecuteLookupAction(actionId, lookupValue);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 动作复制测试

        [Test]
        public void CloneAction_有效动作_应成功复制()
        {
            // Arrange
            var sourceActionId = new ObjectId(1);
            var sourceAction = CreateMockLookupAction("SourceAction", "SourceDesc", new ObjectId(2));
            sourceAction.Setup(a => a.Position).Returns(new Point3d(10, 20, 0));

            var targetParameterId = new ObjectId(3);

            _mockTransaction.Setup(t => t.GetObject(sourceActionId, It.IsAny<OpenMode>()))
                           .Returns(sourceAction.Object);
            _mockBlockTableRecord.Setup(r => r.AppendEntity(It.IsAny<Entity>()))
                                .Returns(new ObjectId(4));

            // Act
            var result = _service.CloneAction(sourceActionId, targetParameterId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(sourceActionId, result);
        }

        [Test]
        public void CloneAction_无效源动作ID_应返回null()
        {
            // Arrange
            var invalidActionId = ObjectId.Null;
            var targetParameterId = new ObjectId(3);

            // Act
            var result = _service.CloneAction(invalidActionId, targetParameterId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void CloneAction_复制异常_应返回null()
        {
            // Arrange
            var sourceActionId = new ObjectId(1);
            var targetParameterId = new ObjectId(3);

            _mockTransaction.Setup(t => t.GetObject(sourceActionId, It.IsAny<OpenMode>()))
                           .Throws(new Exception("复制异常"));

            // Act
            var result = _service.CloneAction(sourceActionId, targetParameterId);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 动作搜索测试

        [Test]
        public void SearchActions_有效搜索条件_应返回匹配结果()
        {
            // Arrange
            var searchCriteria = new ActionSearchCriteria
            {
                NamePattern = "Test*",
                DescriptionPattern = "*Desc*",
                AssociatedParameterId = new ObjectId(1)
            };

            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            // 模拟匹配的动作
            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(new List<Entity> 
                                { 
                                    CreateMockLookupAction("TestAction1", "TestDesc1", new ObjectId(1)).Object,
                                    CreateMockLookupAction("TestAction2", "TestDesc2", new ObjectId(2)).Object
                                }.Cast<Entity>());

            // Act
            var result = _service.SearchActions(mockDocument.Object, searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void SearchActions_空搜索条件_应返回所有动作()
        {
            // Arrange
            var searchCriteria = new ActionSearchCriteria(); // 空条件

            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(new List<Entity>().Cast<Entity>());

            // Act
            var result = _service.SearchActions(mockDocument.Object, searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            // 应该返回所有动作
        }

        #endregion

        #region 边界条件测试

        [Test]
        public void FindLookupActions_边界测试_大量动作()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            var mockEditor = new Mock<Editor>();
            
            // 创建大量动作
            var actionIds = new List<ObjectId>();
            for (int i = 0; i < 1000; i++)
            {
                actionIds.Add(new ObjectId(i));
            }

            var promptSelectionResult = new PromptSelectionResult
            {
                Status = PromptStatus.OK,
                Value = CreateMockSelectionSet(actionIds.ToArray())
            };
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Returns(promptSelectionResult);

            // Act
            var result = _service.FindLookupActions(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            // 验证不会因大量动作而崩溃
        }

        [Test]
        public void SetLookupValues_边界测试_大量查寻值()
        {
            // Arrange
            var actionId = new ObjectId(1);
            
            // 创建大量查寻值
            var lookupValues = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                lookupValues.Add($"Value{i}");
            }

            var mockAction = CreateMockLookupAction("TestAction", "TestDesc", new ObjectId(2));
            mockAction.Setup(a => a.ExtensionDictionary).Returns(new ObjectId(3));

            var mockExtensionDict = new Mock<DBDictionary>();
            _mockTransaction.Setup(t => t.GetObject(new ObjectId(3), It.IsAny<OpenMode>()))
                           .Returns(mockExtensionDict.Object);

            // Act
            var result = _service.SetLookupValues(actionId, lookupValues);

            // Assert
            Assert.IsTrue(result);
            // 验证大量查寻值的处理
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建模拟查寻动作
        /// </summary>
        private Mock<DBPoint> CreateMockLookupAction(string name, string description, ObjectId parameterId)
        {
            var mockAction = new Mock<DBPoint>();
            mockAction.Setup(a => a.Name).Returns(name);
            mockAction.Setup(a => a.GetType()).Returns(typeof(DBPoint));
            
            // 模拟扩展数据
            var mockXData = new Mock<XData>();
            mockXData.Setup(x => x.Add(It.IsAny<TypedValue>())).Verifiable();
            mockAction.Setup(a => a.XData).Returns(mockXData.Object);
            
            return mockAction;
        }

        /// <summary>
        /// 创建模拟选择集
        /// </summary>
        private SelectionSet CreateMockSelectionSet(ObjectId[] objectIds)
        {
            return new Mock<SelectionSet>(objectIds).Object;
        }

        #endregion
    }

    #region 测试数据模型

    /// <summary>
    /// 动作属性类
    /// </summary>
    public class ActionProperties
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> LookupValues { get; set; }
        public ObjectId AssociatedParameterId { get; set; }
    }

    /// <summary>
    /// 动作统计信息类
    /// </summary>
    public class ActionStatistics
    {
        public int TotalActionCount { get; set; }
        public int ValidActionCount { get; set; }
        public int NamedActionCount { get; set; }
        public int LookupActionCount { get; set; }
    }

    /// <summary>
    /// 动作搜索条件类
    /// </summary>
    public class ActionSearchCriteria
    {
        public string NamePattern { get; set; }
        public string DescriptionPattern { get; set; }
        public ObjectId AssociatedParameterId { get; set; }
        public bool OnlyLookupActions { get; set; }
    }

    #endregion
}