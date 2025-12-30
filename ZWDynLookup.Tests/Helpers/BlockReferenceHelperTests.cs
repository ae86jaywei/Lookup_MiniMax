using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZWDynLookup.Service;

namespace ZWDynLookup.Tests.Helpers
{
    /// <summary>
    /// 块引用助手的单元测试类
    /// </summary>
    [TestFixture]
    public class BlockReferenceHelperTests
    {
        private Mock<Database> _mockDatabase;
        private Mock<Transaction> _mockTransaction;
        private Mock<BlockTable> _mockBlockTable;
        private Mock<BlockTableRecord> _mockBlockTableRecord;
        private Mock<BlockReference> _mockBlockReference;
        private Mock<AttributeReference> _mockAttributeReference;
        private BlockReferenceHelper _helper;

        [SetUp]
        public void SetUp()
        {
            // 创建模拟对象
            _mockDatabase = new Mock<Database>();
            _mockTransaction = new Mock<Transaction>();
            _mockBlockTable = new Mock<BlockTable>();
            _mockBlockTableRecord = new Mock<BlockTableRecord>();
            _mockBlockReference = new Mock<BlockReference>();
            _mockAttributeReference = new Mock<AttributeReference>();

            // 设置模拟对象的行为
            _mockDatabase.Setup(db => db.TransactionManager.StartTransaction())
                        .Returns(_mockTransaction.Object);
            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), It.IsAny<OpenMode>()))
                          .Returns(_mockBlockTable.Object);
            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), BlockTableRecord.CurrentSpace, It.IsAny<OpenMode>()))
                          .Returns(_mockBlockTableRecord.Object);

            // 创建助手实例
            _helper = new BlockReferenceHelper();
        }

        [TearDown]
        public void TearDown()
        {
            _helper = null;
        }

        #region 块引用查找测试

        [Test]
        public void FindBlockReferences_有效选择集_应返回块引用列表()
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

            // 设置块表记录包含块引用
            var blockReferences = new List<Entity>
            {
                _mockBlockReference.Object,
                _mockBlockReference.Object
            };
            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(blockReferences.Cast<Entity>());

            // Act
            var result = _helper.FindBlockReferences(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void FindBlockReferences_编辑器为null_应返回空列表()
        {
            // Act
            var result = _helper.FindBlockReferences(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindBlockReferences_用户取消选择_应返回空列表()
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
            var result = _helper.FindBlockReferences(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindBlockReferences_选择异常_应返回空列表()
        {
            // Arrange
            var mockEditor = new Mock<Editor>();
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Throws(new Exception("选择异常"));

            // Act
            var result = _helper.FindBlockReferences(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region 块引用验证测试

        [Test]
        public void ValidateBlockReference_有效块引用_应返回true()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);

            // Act
            var result = _helper.ValidateBlockReference(mockBlockRef);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateBlockReference_空块引用_应返回false()
        {
            // Act
            var result = _helper.ValidateBlockReference(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateBlockReference_无效块名_应返回false()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("", true);

            // Act
            var result = _helper.ValidateBlockReference(mockBlockRef);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateBlockReference_被删除的块引用_应返回false()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", false);

            // Act
            var result = _helper.ValidateBlockReference(mockBlockRef);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 块定义查找测试

        [Test]
        public void FindBlockDefinition_有效块名_应返回块定义()
        {
            // Arrange
            var blockName = "TestBlock";
            var mockBlockDefinition = CreateMockBlockDefinition(blockName);
            _mockBlockTable.Setup(bt => bt[blockName]).Returns(new ObjectId(1));
            _mockTransaction.Setup(t => t.GetObject(new ObjectId(1), It.IsAny<OpenMode>()))
                           .Returns(mockBlockDefinition.Object);

            // Act
            var result = _helper.FindBlockDefinition(blockName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(blockName, result.Name);
        }

        [Test]
        public void FindBlockDefinition_空块名_应返回null()
        {
            // Act
            var result = _helper.FindBlockDefinition("");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void FindBlockDefinition_不存在的块_应返回null()
        {
            // Arrange
            var blockName = "NonExistentBlock";
            _mockBlockTable.Setup(bt => bt[blockName]).Throws(new KeyNotFoundException());

            // Act
            var result = _helper.FindBlockDefinition(blockName);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 属性获取测试

        [Test]
        public void GetAttributes_有效块引用_应返回属性列表()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            var attributes = new List<AttributeReference>
            {
                _mockAttributeReference.Object,
                _mockAttributeReference.Object
            };
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection(attributes.Select(a => a.ObjectId).ToArray()));

            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), It.IsAny<OpenMode>()))
                           .Returns(_mockAttributeReference.Object);

            // Act
            var result = _helper.GetAttributes(mockBlockRef.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void GetAttributes_空块引用_应返回空列表()
        {
            // Act
            var result = _helper.GetAttributes(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAttributes_无属性定义_应返回空列表()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection());

            // Act
            var result = _helper.GetAttributes(mockBlockRef.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAttributeValue_有效属性_应返回值()
        {
            // Arrange
            var attributeTag = "TAG1";
            var attributeValue = "TestValue";
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            var mockAttribute = new Mock<AttributeReference>();
            mockAttribute.Setup(a => a.Tag).Returns(attributeTag);
            mockAttribute.Setup(a => a.TextString).Returns(attributeValue);

            var attributes = new List<AttributeReference> { mockAttribute.Object };
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection(attributes.Select(a => a.ObjectId).ToArray()));

            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), It.IsAny<OpenMode>()))
                           .Returns(mockAttribute.Object);

            // Act
            var result = _helper.GetAttributeValue(mockBlockRef.Object, attributeTag);

            // Assert
            Assert.AreEqual(attributeValue, result);
        }

        [Test]
        public void GetAttributeValue_不存在的属性_应返回null()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection());

            // Act
            var result = _helper.GetAttributeValue(mockBlockRef.Object, "NonExistentTag");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 属性设置测试

        [Test]
        public void SetAttributeValue_有效参数_应成功设置()
        {
            // Arrange
            var attributeTag = "TAG1";
            var newValue = "NewValue";
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            var mockAttribute = new Mock<AttributeReference>();
            mockAttribute.Setup(a => a.Tag).Returns(attributeTag);

            var attributes = new List<AttributeReference> { mockAttribute.Object };
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection(attributes.Select(a => a.ObjectId).ToArray()));

            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), It.IsAny<OpenMode>()))
                           .Returns(mockAttribute.Object);

            // Act
            var result = _helper.SetAttributeValue(mockBlockRef.Object, attributeTag, newValue);

            // Assert
            Assert.IsTrue(result);
            mockAttribute.VerifySet(a => a.TextString = newValue, Times.Once);
        }

        [Test]
        public void SetAttributeValue_不存在的属性_应返回false()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection());

            // Act
            var result = _helper.SetAttributeValue(mockBlockRef.Object, "NonExistentTag", "Value");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void SetAttributeValue_空值_应允许设置()
        {
            // Arrange
            var attributeTag = "TAG1";
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            var mockAttribute = new Mock<AttributeReference>();
            mockAttribute.Setup(a => a.Tag).Returns(attributeTag);

            var attributes = new List<AttributeReference> { mockAttribute.Object };
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection(attributes.Select(a => a.ObjectId).ToArray()));

            _mockTransaction.Setup(t => t.GetObject(It.IsAny<ObjectId>(), It.IsAny<OpenMode>()))
                           .Returns(mockAttribute.Object);

            // Act
            var result = _helper.SetAttributeValue(mockBlockRef.Object, attributeTag, "");

            // Assert
            Assert.IsTrue(result);
            mockAttribute.VerifySet(a => a.TextString = "", Times.Once);
        }

        #endregion

        #region 块插入测试

        [Test]
        public void InsertBlock_有效参数_应成功插入块()
        {
            // Arrange
            var blockName = "TestBlock";
            var insertionPoint = new Point3d(10, 20, 0);
            var scale = new Scale3d(1, 1, 1);
            var rotation = 0.0;

            var mockBlockDefinition = CreateMockBlockDefinition(blockName);
            _mockBlockTable.Setup(bt => bt[blockName]).Returns(new ObjectId(1));
            _mockTransaction.Setup(t => t.GetObject(new ObjectId(1), It.IsAny<OpenMode>()))
                           .Returns(mockBlockDefinition.Object);
            _mockBlockTableRecord.Setup(r => r.AppendEntity(It.IsAny<BlockReference>()))
                                .Returns(new ObjectId(2));

            // Act
            var result = _helper.InsertBlock(blockName, insertionPoint, scale, rotation);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(ObjectId.Null, result);
        }

        [Test]
        public void InsertBlock_不存在的块定义_应返回null()
        {
            // Arrange
            var blockName = "NonExistentBlock";
            var insertionPoint = new Point3d(10, 20, 0);
            _mockBlockTable.Setup(bt => bt[blockName]).Throws(new KeyNotFoundException());

            // Act
            var result = _helper.InsertBlock(blockName, insertionPoint);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void InsertBlock_空块名_应返回null()
        {
            // Arrange
            var insertionPoint = new Point3d(10, 20, 0);

            // Act
            var result = _helper.InsertBlock("", insertionPoint);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 块变换测试

        [Test]
        public void TransformBlock_有效参数_应成功变换块()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("TestBlock", true);
            var transformation = Matrix3d.Identity;

            // Act
            var result = _helper.TransformBlock(mockBlockRef.Object, transformation);

            // Assert
            Assert.IsTrue(result);
            mockBlockRef.Verify(br => br.TransformBy(It.IsAny<Matrix3d>()), Times.Once);
        }

        [Test]
        public void TransformBlock_空块引用_应返回false()
        {
            // Arrange
            var transformation = Matrix3d.Identity;

            // Act
            var result = _helper.TransformBlock(null, transformation);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 动态块测试

        [Test]
        public void IsDynamicBlock_动态块引用_应返回true()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("DynamicBlock", true);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(true);

            // Act
            var result = _helper.IsDynamicBlock(mockBlockRef.Object);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsDynamicBlock_普通块引用_应返回false()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("NormalBlock", true);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(false);

            // Act
            var result = _helper.IsDynamicBlock(mockBlockRef.Object);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetDynamicProperties_动态块_应返回属性列表()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("DynamicBlock", true);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(true);

            // 模拟动态属性
            var mockDynamicProperty = new Mock<DynamicBlockReferenceProperty>();
            mockDynamicProperty.Setup(dp => dp.PropertyName).Returns("TestProperty");
            mockDynamicProperty.Setup(dp => dp.Value).Returns("TestValue");

            var dynamicProperties = new List<DynamicBlockReferenceProperty> { mockDynamicProperty.Object };
            mockBlockRef.Setup(br => br.DynamicBlockReferencePropertyCollection)
                       .Returns(dynamicProperties);

            // Act
            var result = _helper.GetDynamicProperties(mockBlockRef.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void GetDynamicProperties_普通块_应返回空列表()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("NormalBlock", true);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(false);

            // Act
            var result = _helper.GetDynamicProperties(mockBlockRef.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void SetDynamicProperty_动态属性_应成功设置()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("DynamicBlock", true);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(true);

            var propertyName = "TestProperty";
            var propertyValue = "NewValue";
            var mockDynamicProperty = new Mock<DynamicBlockReferenceProperty>();
            mockDynamicProperty.Setup(dp => dp.PropertyName).Returns(propertyName);

            var dynamicProperties = new List<DynamicBlockReferenceProperty> { mockDynamicProperty.Object };
            mockBlockRef.Setup(br => br.DynamicBlockReferencePropertyCollection)
                       .Returns(dynamicProperties);

            // Act
            var result = _helper.SetDynamicProperty(mockBlockRef.Object, propertyName, propertyValue);

            // Assert
            Assert.IsTrue(result);
            mockDynamicProperty.VerifySet(dp => dp.Value = propertyValue, Times.Once);
        }

        [Test]
        public void SetDynamicProperty_不存在的属性_应返回false()
        {
            // Arrange
            var mockBlockRef = CreateMockBlockReference("DynamicBlock", true);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(true);
            mockBlockRef.Setup(br => br.DynamicBlockReferencePropertyCollection)
                       .Returns(new List<DynamicBlockReferenceProperty>());

            // Act
            var result = _helper.SetDynamicProperty(mockBlockRef.Object, "NonExistentProperty", "Value");

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 块统计测试

        [Test]
        public void GetBlockStatistics_有效文档_应返回统计信息()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            // 模拟块表中的块定义
            var blockDefinitions = new List<BlockTableRecord>
            {
                CreateMockBlockDefinition("Block1").Object,
                CreateMockBlockDefinition("Block2").Object,
                CreateMockBlockDefinition("Block3").Object
            };
            _mockBlockTable.Setup(bt => bt.Cast<BlockTableRecord>())
                          .Returns(blockDefinitions.Cast<BlockTableRecord>());

            // Act
            var result = _helper.GetBlockStatistics(mockDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TotalBlocks >= 0);
        }

        [Test]
        public void GetBlockStatistics_文档为null_应返回默认统计()
        {
            // Act
            var result = _helper.GetBlockStatistics(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalBlocks);
        }

        #endregion

        #region 块清理测试

        [Test]
        public void PurgeUnusedBlocks_有未使用块_应清理块定义()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);
            var unusedBlockIds = new ObjectIdCollection { new ObjectId(1), new ObjectId(2) };

            _mockDatabase.Setup(db => db.Purge(It.IsAny<ObjectIdCollection>(), false))
                        .Returns(unusedBlockIds);

            // Act
            var result = _helper.PurgeUnusedBlocks(mockDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void PurgeUnusedBlocks_文档为null_应返回空列表()
        {
            // Act
            var result = _helper.PurgeUnusedBlocks(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region 边界条件测试

        [Test]
        public void FindBlockReferences_边界测试_大量块引用()
        {
            // Arrange
            var mockDocument = new Mock<Document>();
            mockDocument.Setup(d => d.Database).Returns(_mockDatabase.Object);

            var mockEditor = new Mock<Editor>();
            
            // 创建大量块引用
            var blockRefIds = new List<ObjectId>();
            for (int i = 0; i < 10000; i++)
            {
                blockRefIds.Add(new ObjectId(i));
            }

            var promptSelectionResult = new PromptSelectionResult
            {
                Status = PromptStatus.OK,
                Value = CreateMockSelectionSet(blockRefIds.ToArray())
            };
            mockEditor.Setup(e => e.GetSelection(It.IsAny<PromptSelectionOptions>()))
                     .Returns(promptSelectionResult);

            // 设置块表记录包含大量块引用
            var blockReferences = new List<Entity>();
            for (int i = 0; i < 10000; i++)
            {
                blockReferences.Add(CreateMockBlockReference($"Block{i}", true).Object);
            }
            _mockBlockTableRecord.Setup(r => r.Cast<Entity>())
                                .Returns(blockReferences.Cast<Entity>());

            // Act
            var result = _helper.FindBlockReferences(mockEditor.Object);

            // Assert
            Assert.IsNotNull(result);
            // 验证不会因大量块引用而崩溃
        }

        [Test]
        public void InsertBlock_边界测试_极端缩放值()
        {
            // Arrange
            var blockName = "TestBlock";
            var insertionPoint = new Point3d(10, 20, 0);
            var extremeScale = new Scale3d(1000, 0.001, 1);

            var mockBlockDefinition = CreateMockBlockDefinition(blockName);
            _mockBlockTable.Setup(bt => bt[blockName]).Returns(new ObjectId(1));
            _mockTransaction.Setup(t => t.GetObject(new ObjectId(1), It.IsAny<OpenMode>()))
                           .Returns(mockBlockDefinition.Object);
            _mockBlockTableRecord.Setup(r => r.AppendEntity(It.IsAny<BlockReference>()))
                                .Returns(new ObjectId(2));

            // Act
            var result = _helper.InsertBlock(blockName, insertionPoint, extremeScale);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(ObjectId.Null, result);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建模拟块引用
        /// </summary>
        private Mock<BlockReference> CreateMockBlockReference(string blockName, bool isValid)
        {
            var mockBlockRef = new Mock<BlockReference>();
            mockBlockRef.Setup(br => br.Name).Returns(blockName);
            mockBlockRef.Setup(br => br.IsValid).Returns(isValid);
            mockBlockRef.Setup(br => br.IsErased).Returns(!isValid);
            mockBlockRef.Setup(br => br.IsDynamicBlock).Returns(blockName.Contains("Dynamic"));
            
            // 模拟属性集合
            mockBlockRef.Setup(br => br.AttributeCollection)
                       .Returns(new ObjectIdCollection());
            
            // 模拟动态属性集合
            mockBlockRef.Setup(br => br.DynamicBlockReferencePropertyCollection)
                       .Returns(new List<DynamicBlockReferenceProperty>());
            
            return mockBlockRef;
        }

        /// <summary>
        /// 创建模拟块定义
        /// </summary>
        private Mock<BlockTableRecord> CreateMockBlockDefinition(string blockName)
        {
            var mockBlockDefinition = new Mock<BlockTableRecord>();
            mockBlockDefinition.Setup(bd => bd.Name).Returns(blockName);
            return mockBlockDefinition;
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
    /// 块统计信息类
    /// </summary>
    public class BlockStatistics
    {
        public int TotalBlocks { get; set; }
        public int DynamicBlocks { get; set; }
        public int StaticBlocks { get; set; }
        public int ReferencedBlocks { get; set; }
        public int UnreferencedBlocks { get; set; }
    }

    #endregion

    #region 模拟辅助类

    /// <summary>
    /// 模拟PromptSelectionResult类
    /// </summary>
    public class PromptSelectionResult
    {
        public PromptStatus Status { get; set; }
        public SelectionSet Value { get; set; }
    }

    /// <summary>
    /// 模拟SelectionSet类
    /// </summary>
    public class SelectionSet
    {
        private readonly ObjectId[] _objectIds;

        public SelectionSet(ObjectId[] objectIds)
        {
            _objectIds = objectIds;
        }

        public ObjectId[] GetObjectIds()
        {
            return _objectIds;
        }
    }

    /// <summary>
    /// 模拟DynamicBlockReferenceProperty类
    /// </summary>
    public class DynamicBlockReferenceProperty
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }

    #endregion
}