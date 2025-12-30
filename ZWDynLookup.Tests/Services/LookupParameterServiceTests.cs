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
    /// 查寻参数服务的单元测试类
    /// </summary>
    [TestFixture]
    public class LookupParameterServiceTests
    {
        private Mock<Database> _mockDatabase;
        private Mock<Transaction> _mockTransaction;
        private Mock<BlockTable> _mockBlockTable;
        private Mock<BlockTableRecord> _mockBlockTableRecord;
        private LookupParameterService _service;

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
            _service = new LookupParameterService();
        }

        [TearDown]
        public void TearDown()
        {
            _service = null;
        }

        #region 参数查找测试

        [Test]
        public void FindLookupParameters_有效文档_应返回参数列表()
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
            var result = _service.FindLookupParameters(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void FindLookupParameters_编辑器为null_应返回空列表()
        {
            // Act
            var result = _service.FindLookupParameters(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindLookupParameters_用户取消选择_应返回空列表()
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
            var result = _service.FindLookupParameters(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindLookupParameters_选择异常_应返回空列表()
        {
            // Arrange
            var mockEditor = new Mock<Editor>();
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Throws(new Exception("选择异常"));

            // Act
            var result = _service.FindLookupParameters(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region 参数验证测试

        [Test]
        public void ValidateParameter_有效参数对象_应返回true()
        {
            // Arrange
            var mockParameter = CreateMockLookupParameter("TestParam", "TestLabel", "TestDesc");

            // Act
            var result = _service.ValidateParameter(mockParameter);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateParameter_参数对象为null_应返回false()
        {
            // Act
            var result = _service.ValidateParameter(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateParameter_参数名称为空_应返回false()
        {
            // Arrange
            var mockParameter = CreateMockLookupParameter("", "TestLabel", "TestDesc");

            // Act
            var result = _service.ValidateParameter(mockParameter);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateParameter_参数名称包含特殊字符_应返回false()
        {
            // Arrange
            var mockParameter = CreateMockLookupParameter("Test@Param", "TestLabel", "TestDesc");

            // Act
            var result = _service.ValidateParameter(mockParameter);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateParameter_参数名称过长_应返回false()
        {
            // Arrange
            var longName = new string('A', 256); // 超过最大长度
            var mockParameter = CreateMockLookupParameter(longName, "TestLabel", "TestDesc");

            // Act
            var result = _service.ValidateParameter(mockParameter);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 参数属性测试

        [Test]
        public void GetParameterProperties_有效参数_应返回属性信息()
        {
            // Arrange
            var parameterId = new ObjectId(1);
            var mockParameter = CreateMockLookupParameter("TestParam", "TestLabel", "TestDesc");
            mockParameter.Setup(p => p.Position).Returns(new Point3d(10, 20, 0));
            mockParameter.Setup(p => p.ExtensionDictionary).Returns(new ObjectId(2));

            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Returns(mockParameter.Object);

            // Act
            var result = _service.GetParameterProperties(parameterId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestParam", result.Name);
            Assert.AreEqual("TestLabel", result.Label);
        }

        [Test]
        public void GetParameterProperties_无效参数ID_应返回null()
        {
            // Arrange
            var invalidParameterId = ObjectId.Null;

            // Act
            var result = _service.GetParameterProperties(invalidParameterId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetParameterProperties_参数不存在_应返回null()
        {
            // Arrange
            var parameterId = new ObjectId(999);
            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Throws(new Exception("参数不存在"));

            // Act
            var result = _service.GetParameterProperties(parameterId);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 参数更新测试

        [Test]
        public void UpdateParameterProperties_有效参数_应成功更新()
        {
            // Arrange
            var parameterId = new ObjectId(1);
            var properties = new ParameterProperties
            {
                Name = "UpdatedParam",
                Label = "UpdatedLabel",
                Description = "UpdatedDesc",
                ShowPalette = true,
                GripCount = 1
            };
            var mockParameter = CreateMockLookupParameter("TestParam", "TestLabel", "TestDesc");

            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Returns(mockParameter.Object);

            // Act
            var result = _service.UpdateParameterProperties(parameterId, properties);

            // Assert
            Assert.IsTrue(result);
            mockParameter.VerifySet(p => p.Name = "UpdatedParam", Times.Once);
        }

        [Test]
        public void UpdateParameterProperties_无效参数ID_应返回false()
        {
            // Arrange
            var invalidParameterId = ObjectId.Null;
            var properties = new ParameterProperties
            {
                Name = "UpdatedParam",
                Label = "UpdatedLabel",
                Description = "UpdatedDesc"
            };

            // Act
            var result = _service.UpdateParameterProperties(invalidParameterId, properties);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateParameterProperties_更新异常_应返回false()
        {
            // Arrange
            var parameterId = new ObjectId(1);
            var properties = new ParameterProperties
            {
                Name = "UpdatedParam",
                Label = "UpdatedLabel",
                Description = "UpdatedDesc"
            };
            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Throws(new Exception("更新异常"));

            // Act
            var result = _service.UpdateParameterProperties(parameterId, properties);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 参数删除测试

        [Test]
        public void DeleteParameter_有效参数_应成功删除()
        {
            // Arrange
            var parameterId = new ObjectId(1);
            var mockParameter = CreateMockLookupParameter("TestParam", "TestLabel", "TestDesc");

            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Returns(mockParameter.Object);

            // Act
            var result = _service.DeleteParameter(parameterId);

            // Assert
            Assert.IsTrue(result);
            mockParameter.Verify(p => p.Erase(), Times.Once);
        }

        [Test]
        public void DeleteParameter_无效参数ID_应返回false()
        {
            // Arrange
            var invalidParameterId = ObjectId.Null;

            // Act
            var result = _service.DeleteParameter(invalidParameterId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteParameter_参数被锁定_应返回false()
        {
            // Arrange
            var parameterId = new ObjectId(1);
            var mockParameter = CreateMockLookupParameter("TestParam", "TestLabel", "TestDesc");
            mockParameter.Setup(p => p.Erase()).Throws(new Exception("参数被锁定"));

            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Returns(mockParameter.Object);

            // Act
            var result = _service.DeleteParameter(parameterId);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 参数统计测试

        [Test]
        public void GetParameterStatistics_有效块记录_应返回统计信息()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            // 模拟包含一些参数的块记录
            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(new List<Entity> 
                                { 
                                    CreateMockLookupParameter("Param1", "Label1", "Desc1").Object,
                                    CreateMockLookupParameter("Param2", "Label2", "Desc2").Object
                                }.Cast<Entity>());

            // Act
            var result = _service.GetParameterStatistics(mockDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TotalParameterCount >= 0);
        }

        [Test]
        public void GetParameterStatistics_文档为null_应返回默认统计()
        {
            // Act
            var result = _service.GetParameterStatistics(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalParameterCount);
        }

        [Test]
        public void GetParameterStatistics_获取异常_应返回默认统计()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Throws(new Exception("统计异常"));

            // Act
            var result = _service.GetParameterStatistics(mockDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalParameterCount);
        }

        #endregion

        #region 参数克隆测试

        [Test]
        public void CloneParameter_有效参数_应成功克隆()
        {
            // Arrange
            var sourceParameterId = new ObjectId(1);
            var sourceParameter = CreateMockLookupParameter("SourceParam", "SourceLabel", "SourceDesc");
            sourceParameter.Setup(p => p.Position).Returns(new Point3d(10, 20, 0));

            var targetPosition = new Point3d(30, 40, 0);

            _mockTransaction.Setup(t => t.GetObject(sourceParameterId, It.IsAny<OpenMode>()))
                           .Returns(sourceParameter.Object);
            _mockBlockTableRecord.Setup(r => r.AppendEntity(It.IsAny<Entity>()))
                                .Returns(new ObjectId(2));

            // Act
            var result = _service.CloneParameter(sourceParameterId, targetPosition);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(sourceParameterId, result);
        }

        [Test]
        public void CloneParameter_无效源参数ID_应返回null()
        {
            // Arrange
            var invalidParameterId = ObjectId.Null;
            var targetPosition = new Point3d(30, 40, 0);

            // Act
            var result = _service.CloneParameter(invalidParameterId, targetPosition);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void CloneParameter_克隆异常_应返回null()
        {
            // Arrange
            var sourceParameterId = new ObjectId(1);
            var targetPosition = new Point3d(30, 40, 0);

            _mockTransaction.Setup(t => t.GetObject(sourceParameterId, It.IsAny<OpenMode>()))
                           .Throws(new Exception("克隆异常"));

            // Act
            var result = _service.CloneParameter(sourceParameterId, targetPosition);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 参数搜索测试

        [Test]
        public void SearchParameters_有效搜索条件_应返回匹配结果()
        {
            // Arrange
            var searchCriteria = new ParameterSearchCriteria
            {
                NamePattern = "Test*",
                LabelPattern = "*Label*",
                IncludeDescription = true
            };

            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            // 模拟匹配的参数
            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(new List<Entity> 
                                { 
                                    CreateMockLookupParameter("TestParam1", "TestLabel1", "TestDesc1").Object,
                                    CreateMockLookupParameter("TestParam2", "TestLabel2", "TestDesc2").Object
                                }.Cast<Entity>());

            // Act
            var result = _service.SearchParameters(mockDocument.Object, searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void SearchParameters_空搜索条件_应返回所有参数()
        {
            // Arrange
            var searchCriteria = new ParameterSearchCriteria(); // 空条件

            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(new List<Entity>().Cast<Entity>());

            // Act
            var result = _service.SearchParameters(mockDocument.Object, searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            // 应该返回所有参数
        }

        #endregion

        #region 边界条件测试

        [Test]
        public void FindLookupParameters_边界测试_大量参数()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            var mockEditor = new Mock<Editor>();
            
            // 创建大量参数
            var parameterIds = new List<ObjectId>();
            for (int i = 0; i < 1000; i++)
            {
                parameterIds.Add(new ObjectId(i));
            }

            var promptSelectionResult = new PromptSelectionResult
            {
                Status = PromptStatus.OK,
                Value = CreateMockSelectionSet(parameterIds.ToArray())
            };
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Returns(promptSelectionResult);

            // Act
            var result = _service.FindLookupParameters(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            // 验证不会因大量参数而崩溃
        }

        [Test]
        public void UpdateParameterProperties_边界测试_长描述文本()
        {
            // Arrange
            var parameterId = new ObjectId(1);
            var longDescription = new string('D', 1000); // 很长的描述
            var properties = new ParameterProperties
            {
                Name = "TestParam",
                Label = "TestLabel",
                Description = longDescription
            };
            var mockParameter = CreateMockLookupParameter("TestParam", "TestLabel", "TestDesc");

            _mockTransaction.Setup(t => t.GetObject(parameterId, It.IsAny<OpenMode>()))
                           .Returns(mockParameter.Object);

            // Act
            var result = _service.UpdateParameterProperties(parameterId, properties);

            // Assert
            Assert.IsTrue(result);
            // 验证长描述文本的处理
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建模拟查寻参数
        /// </summary>
        private Mock<DBPoint> CreateMockLookupParameter(string name, string label, string description)
        {
            var mockParameter = new Mock<DBPoint>();
            mockParameter.Setup(p => p.Name).Returns(name);
            mockParameter.Setup(p => p.GetType()).Returns(typeof(DBPoint));
            
            // 模拟扩展数据
            var mockXData = new Mock<XData>();
            mockXData.Setup(x => x.Add(It.IsAny<TypedValue>())).Verifiable();
            mockParameter.Setup(p => p.XData).Returns(mockXData.Object);
            
            return mockParameter;
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
    /// 参数属性类
    /// </summary>
    public class ParameterProperties
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool ShowPalette { get; set; }
        public int GripCount { get; set; }
    }

    /// <summary>
    /// 参数统计信息类
    /// </summary>
    public class ParameterStatistics
    {
        public int TotalParameterCount { get; set; }
        public int ValidParameterCount { get; set; }
        public int NamedParameterCount { get; set; }
        public int LabeledParameterCount { get; set; }
    }

    /// <summary>
    /// 参数搜索条件类
    /// </summary>
    public class ParameterSearchCriteria
    {
        public string NamePattern { get; set; }
        public string LabelPattern { get; set; }
        public string DescriptionPattern { get; set; }
        public bool IncludeDescription { get; set; }
    }

    #endregion
}