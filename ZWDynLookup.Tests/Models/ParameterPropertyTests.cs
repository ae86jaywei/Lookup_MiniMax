using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using ZWDynLookup.Models;

namespace ZWDynLookup.Tests.Models
{
    /// <summary>
    /// 参数特性模型的单元测试类
    /// </summary>
    [TestFixture]
    public class ParameterPropertyTests
    {
        private ParameterProperty _parameterProperty;

        [SetUp]
        public void SetUp()
        {
            _parameterProperty = new ParameterProperty();
        }

        [TearDown]
        public void TearDown()
        {
            _parameterProperty = null;
        }

        #region 基本属性测试

        [Test]
        public void Constructor_默认构造_应初始化属性()
        {
            // Act
            var property = new ParameterProperty();

            // Assert
            Assert.IsNotNull(property.PropertyName);
            Assert.IsEmpty(property.PropertyName);
            Assert.IsNotNull(property.DisplayName);
            Assert.IsEmpty(property.DisplayName);
            Assert.IsNotNull(property.Description);
            Assert.IsEmpty(property.Description);
            Assert.AreEqual(PropertyType.Text, property.PropertyType);
            Assert.AreEqual(0, property.PropertyValue);
            Assert.IsNull(property.DefaultValue);
            Assert.IsNull(property.MinValue);
            Assert.IsNull(property.MaxValue);
            Assert.IsNotNull(property.AllowedValues);
            Assert.AreEqual(0, property.AllowedValues.Count);
            Assert.IsTrue(property.IsVisible);
            Assert.IsTrue(property.IsEditable);
            Assert.IsFalse(property.IsRequired);
            Assert.AreEqual("", property.Unit);
            Assert.AreEqual("", property.Format);
        }

        [Test]
        public void PropertyName_设置有效名称_应成功设置()
        {
            // Arrange
            var expectedName = "TestProperty";

            // Act
            _parameterProperty.PropertyName = expectedName;

            // Assert
            Assert.AreEqual(expectedName, _parameterProperty.PropertyName);
        }

        [Test]
        public void PropertyName_设置空名称_应抛出异常()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _parameterProperty.PropertyName = "");
        }

        [Test]
        public void PropertyName_设置null_应抛出异常()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _parameterProperty.PropertyName = null);
        }

        [Test]
        public void PropertyName_设置包含特殊字符的名称_应抛出异常()
        {
            // Arrange
            var invalidName = "Test@Property";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _parameterProperty.PropertyName = invalidName);
        }

        [Test]
        public void DisplayName_设置有效显示名称_应成功设置()
        {
            // Arrange
            var expectedDisplayName = "测试属性";

            // Act
            _parameterProperty.DisplayName = expectedDisplayName;

            // Assert
            Assert.AreEqual(expectedDisplayName, _parameterProperty.DisplayName);
        }

        [Test]
        public void Description_设置有效描述_应成功设置()
        {
            // Arrange
            var expectedDescription = "这是一个测试属性的描述";

            // Act
            _parameterProperty.Description = expectedDescription;

            // Assert
            Assert.AreEqual(expectedDescription, _parameterProperty.Description);
        }

        #endregion

        #region 属性类型测试

        [Test]
        public void PropertyType_设置不同类型_应正确更新()
        {
            // Arrange & Act & Assert
            _parameterProperty.PropertyType = PropertyType.Text;
            Assert.AreEqual(PropertyType.Text, _parameterProperty.PropertyType);

            _parameterProperty.PropertyType = PropertyType.Number;
            Assert.AreEqual(PropertyType.Number, _parameterProperty.PropertyType);

            _parameterProperty.PropertyType = PropertyType.Boolean;
            Assert.AreEqual(PropertyType.Boolean, _parameterProperty.PropertyType);

            _parameterProperty.PropertyType = PropertyType.DateTime;
            Assert.AreEqual(PropertyType.DateTime, _parameterProperty.PropertyType);
        }

        [Test]
        public void PropertyValue_设置不同类型值_应正确存储()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;

            // Act & Assert
            _parameterProperty.PropertyValue = "TextValue";
            Assert.AreEqual("TextValue", _parameterProperty.PropertyValue);

            _parameterProperty.PropertyType = PropertyType.Number;
            _parameterProperty.PropertyValue = 123.45;
            Assert.AreEqual(123.45, _parameterProperty.PropertyValue);

            _parameterProperty.PropertyType = PropertyType.Boolean;
            _parameterProperty.PropertyValue = true;
            Assert.AreEqual(true, _parameterProperty.PropertyValue);
        }

        [Test]
        public void PropertyValue_类型不匹配_应抛出异常()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => _parameterProperty.PropertyValue = "NotANumber");
        }

        #endregion

        #region 默认值测试

        [Test]
        public void DefaultValue_设置有效默认值_应成功设置()
        {
            // Arrange
            var expectedDefault = "DefaultValue";

            // Act
            _parameterProperty.DefaultValue = expectedDefault;

            // Assert
            Assert.AreEqual(expectedDefault, _parameterProperty.DefaultValue);
        }

        [Test]
        public void DefaultValue_类型不匹配_应抛出异常()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => _parameterProperty.DefaultValue = "NotANumber");
        }

        [Test]
        public void DefaultValue_设置为null_应允许()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;

            // Act
            _parameterProperty.DefaultValue = null;

            // Assert
            Assert.IsNull(_parameterProperty.DefaultValue);
        }

        #endregion

        #region 数值范围测试

        [Test]
        public void MinValueMaxValue_设置有效范围_应成功设置()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;

            // Act
            _parameterProperty.MinValue = 0;
            _parameterProperty.MaxValue = 100;

            // Assert
            Assert.AreEqual(0, _parameterProperty.MinValue);
            Assert.AreEqual(100, _parameterProperty.MaxValue);
        }

        [Test]
        public void MinValueMaxValue_最小值大于最大值_应抛出异常()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;

            // Act & Assert
            _parameterProperty.MinValue = 100;
            Assert.Throws<ArgumentException>(() => _parameterProperty.MaxValue = 0);
        }

        [Test]
        public void MinValueMaxValue_非数值类型_应抛出异常()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _parameterProperty.MinValue = 0);
            Assert.Throws<InvalidOperationException>(() => _parameterProperty.MaxValue = 100);
        }

        #endregion

        #region 允许值测试

        [Test]
        public void AddAllowedValue_有效值_应成功添加()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;

            // Act
            var result = _parameterProperty.AddAllowedValue("Value1");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _parameterProperty.AllowedValues.Count);
            Assert.IsTrue(_parameterProperty.AllowedValues.Contains("Value1"));
        }

        [Test]
        public void AddAllowedValue_重复值_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("Value1");

            // Act
            var result = _parameterProperty.AddAllowedValue("Value1");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, _parameterProperty.AllowedValues.Count);
        }

        [Test]
        public void AddAllowedValue_空值_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;

            // Act
            var result = _parameterProperty.AddAllowedValue("");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _parameterProperty.AllowedValues.Count);
        }

        [Test]
        public void AddAllowedValue_类型不匹配_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;

            // Act
            var result = _parameterProperty.AddAllowedValue("NotANumber");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _parameterProperty.AllowedValues.Count);
        }

        [Test]
        public void RemoveAllowedValue_存在的值_应成功删除()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("Value1");
            _parameterProperty.AddAllowedValue("Value2");

            // Act
            var result = _parameterProperty.RemoveAllowedValue("Value1");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _parameterProperty.AllowedValues.Count);
            Assert.IsFalse(_parameterProperty.AllowedValues.Contains("Value1"));
            Assert.IsTrue(_parameterProperty.AllowedValues.Contains("Value2"));
        }

        [Test]
        public void RemoveAllowedValue_不存在的值_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("Value1");

            // Act
            var result = _parameterProperty.RemoveAllowedValue("NonExistentValue");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, _parameterProperty.AllowedValues.Count);
        }

        [Test]
        public void ClearAllowedValues_有允许值_应清空列表()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("Value1");
            _parameterProperty.AddAllowedValue("Value2");

            // Act
            _parameterProperty.ClearAllowedValues();

            // Assert
            Assert.AreEqual(0, _parameterProperty.AllowedValues.Count);
        }

        [Test]
        public void IsValueAllowed_允许的值_应返回true()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("Allowed1");
            _parameterProperty.AddAllowedValue("Allowed2");

            // Act & Assert
            Assert.IsTrue(_parameterProperty.IsValueAllowed("Allowed1"));
            Assert.IsTrue(_parameterProperty.IsValueAllowed("Allowed2"));
        }

        [Test]
        public void IsValueAllowed_不允许的值_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("Allowed1");

            // Act & Assert
            Assert.IsFalse(_parameterProperty.IsValueAllowed("NotAllowed"));
        }

        [Test]
        public void IsValueAllowed_无允许值限制_应返回true()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;
            // 不添加任何允许值

            // Act & Assert
            Assert.IsTrue(_parameterProperty.IsValueAllowed("AnyValue"));
        }

        #endregion

        #region 验证测试

        [Test]
        public void Validate_有效属性值_应返回true()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.PropertyValue = "ValidValue";

            // Act
            var result = _parameterProperty.Validate();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Validate_属性名为空_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyName = "";

            // Act
            var result = _parameterProperty.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_属性值为null但非必需_应返回true()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.IsRequired = false;
            _parameterProperty.PropertyValue = null;

            // Act
            var result = _parameterProperty.Validate();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Validate_属性值为null但必需_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.IsRequired = true;
            _parameterProperty.PropertyValue = null;

            // Act
            var result = _parameterProperty.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_超出数值范围_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.PropertyType = PropertyType.Number;
            _parameterProperty.MinValue = 0;
            _parameterProperty.MaxValue = 100;
            _parameterProperty.PropertyValue = 150;

            // Act
            var result = _parameterProperty.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_不允许的值_应返回false()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.AddAllowedValue("AllowedValue");
            _parameterProperty.PropertyValue = "NotAllowedValue";

            // Act
            var result = _parameterProperty.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 格式化测试

        [Test]
        public void FormatValue_设置格式_应正确格式化()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;
            _parameterProperty.PropertyValue = 123.456;
            _parameterProperty.Format = "F2";

            // Act
            var result = _parameterProperty.FormatValue();

            // Assert
            Assert.AreEqual("123.46", result);
        }

        [Test]
        public void FormatValue_日期类型_应正确格式化()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.DateTime;
            var testDate = new DateTime(2024, 1, 15);
            _parameterProperty.PropertyValue = testDate;
            _parameterProperty.Format = "yyyy-MM-dd";

            // Act
            var result = _parameterProperty.FormatValue();

            // Assert
            Assert.AreEqual("2024-01-15", result);
        }

        [Test]
        public void FormatValue_无格式设置_应返回默认值格式()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Number;
            _parameterProperty.PropertyValue = 123.456;

            // Act
            var result = _parameterProperty.FormatValue();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("123"));
        }

        #endregion

        #region 克隆测试

        [Test]
        public void Clone_有效对象_应成功克隆()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.DisplayName = "测试属性";
            _parameterProperty.Description = "测试描述";
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.PropertyValue = "TestValue";
            _parameterProperty.DefaultValue = "DefaultValue";
            _parameterProperty.IsRequired = true;
            _parameterProperty.IsVisible = false;
            _parameterProperty.IsEditable = false;
            _parameterProperty.Unit = "mm";
            _parameterProperty.Format = "TestFormat";
            _parameterProperty.AddAllowedValue("Value1");
            _parameterProperty.AddAllowedValue("Value2");

            // Act
            var result = _parameterProperty.Clone();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_parameterProperty.PropertyName, result.PropertyName);
            Assert.AreEqual(_parameterProperty.DisplayName, result.DisplayName);
            Assert.AreEqual(_parameterProperty.Description, result.Description);
            Assert.AreEqual(_parameterProperty.PropertyType, result.PropertyType);
            Assert.AreEqual(_parameterProperty.PropertyValue, result.PropertyValue);
            Assert.AreEqual(_parameterProperty.DefaultValue, result.DefaultValue);
            Assert.AreEqual(_parameterProperty.IsRequired, result.IsRequired);
            Assert.AreEqual(_parameterProperty.IsVisible, result.IsVisible);
            Assert.AreEqual(_parameterProperty.IsEditable, result.IsEditable);
            Assert.AreEqual(_parameterProperty.Unit, result.Unit);
            Assert.AreEqual(_parameterProperty.Format, result.Format);
            Assert.AreEqual(_parameterProperty.AllowedValues.Count, result.AllowedValues.Count);

            // 验证是深拷贝
            result.PropertyName = "ModifiedName";
            Assert.AreNotEqual(_parameterProperty.PropertyName, result.PropertyName);
        }

        #endregion

        #region 比较测试

        [Test]
        public void Equals_相同属性值_应返回true()
        {
            // Arrange
            var property1 = new ParameterProperty
            {
                PropertyName = "TestProperty",
                PropertyType = PropertyType.Text,
                PropertyValue = "TestValue"
            };
            var property2 = new ParameterProperty
            {
                PropertyName = "TestProperty",
                PropertyType = PropertyType.Text,
                PropertyValue = "TestValue"
            };

            // Act
            var result = property1.Equals(property2);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_不同属性值_应返回false()
        {
            // Arrange
            var property1 = new ParameterProperty
            {
                PropertyName = "TestProperty1",
                PropertyType = PropertyType.Text,
                PropertyValue = "TestValue"
            };
            var property2 = new ParameterProperty
            {
                PropertyName = "TestProperty2",
                PropertyType = PropertyType.Text,
                PropertyValue = "TestValue"
            };

            // Act
            var result = property1.Equals(property2);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_null对象_应返回false()
        {
            // Arrange
            var property = new ParameterProperty();

            // Act
            var result = property.Equals(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_相同属性值_应返回相同哈希码()
        {
            // Arrange
            var property1 = new ParameterProperty
            {
                PropertyName = "TestProperty",
                PropertyType = PropertyType.Text,
                PropertyValue = "TestValue"
            };
            var property2 = new ParameterProperty
            {
                PropertyName = "TestProperty",
                PropertyType = PropertyType.Text,
                PropertyValue = "TestValue"
            };

            // Act
            var hashCode1 = property1.GetHashCode();
            var hashCode2 = property2.GetHashCode();

            // Assert
            Assert.AreEqual(hashCode1, hashCode2);
        }

        #endregion

        #region 转换测试

        [Test]
        public void ToString_有效属性_应返回描述信息()
        {
            // Arrange
            _parameterProperty.PropertyName = "TestProperty";
            _parameterProperty.DisplayName = "测试属性";
            _parameterProperty.PropertyType = PropertyType.Text;
            _parameterProperty.PropertyValue = "TestValue";

            // Act
            var result = _parameterProperty.ToString();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("TestProperty"));
            Assert.IsTrue(result.Contains("测试属性"));
            Assert.IsTrue(result.Contains("TestValue"));
        }

        #endregion

        #region 边界条件测试

        [Test]
        public void AllowedValues_边界测试_大量允许值()
        {
            // Arrange
            _parameterProperty.PropertyType = PropertyType.Text;

            // 添加大量允许值
            for (int i = 0; i < 10000; i++)
            {
                _parameterProperty.AddAllowedValue($"Value{i}");
            }

            // Act & Assert
            Assert.AreEqual(10000, _parameterProperty.AllowedValues.Count);
            
            // 测试查找性能
            var startTime = DateTime.Now;
            var isAllowed = _parameterProperty.IsValueAllowed("Value5000");
            var endTime = DateTime.Now;
            
            Assert.IsTrue(isAllowed);
            Assert.IsTrue((endTime - startTime).TotalMilliseconds < 100); // 应该在100ms内完成
        }

        [Test]
        public void PropertyName_边界测试_超长名称()
        {
            // Arrange
            var longName = new string('A', 1000);

            // Act
            _parameterProperty.PropertyName = longName;

            // Assert
            Assert.AreEqual(longName, _parameterProperty.PropertyName);
        }

        [Test]
        public void Description_边界测试_超长描述()
        {
            // Arrange
            var longDescription = new string('D', 5000);

            // Act
            _parameterProperty.Description = longDescription;

            // Assert
            Assert.AreEqual(longDescription, _parameterProperty.Description);
        }

        #endregion

        #region 辅助方法测试

        [Test]
        public void CopyFrom_有效源对象_应复制所有属性()
        {
            // Arrange
            var source = new ParameterProperty
            {
                PropertyName = "SourceProperty",
                DisplayName = "源属性",
                Description = "源描述",
                PropertyType = PropertyType.Number,
                PropertyValue = 42,
                DefaultValue = 10,
                IsRequired = true,
                IsVisible = false,
                IsEditable = false,
                Unit = "kg",
                Format = "N0",
                MinValue = 0,
                MaxValue = 100
            };
            source.AddAllowedValue("Value1");
            source.AddAllowedValue("Value2");

            // Act
            _parameterProperty.CopyFrom(source);

            // Assert
            Assert.AreEqual(source.PropertyName, _parameterProperty.PropertyName);
            Assert.AreEqual(source.DisplayName, _parameterProperty.DisplayName);
            Assert.AreEqual(source.Description, _parameterProperty.Description);
            Assert.AreEqual(source.PropertyType, _parameterProperty.PropertyType);
            Assert.AreEqual(source.PropertyValue, _parameterProperty.PropertyValue);
            Assert.AreEqual(source.DefaultValue, _parameterProperty.DefaultValue);
            Assert.AreEqual(source.IsRequired, _parameterProperty.IsRequired);
            Assert.AreEqual(source.IsVisible, _parameterProperty.IsVisible);
            Assert.AreEqual(source.IsEditable, _parameterProperty.IsEditable);
            Assert.AreEqual(source.Unit, _parameterProperty.Unit);
            Assert.AreEqual(source.Format, _parameterProperty.Format);
            Assert.AreEqual(source.MinValue, _parameterProperty.MinValue);
            Assert.AreEqual(source.MaxValue, _parameterProperty.MaxValue);
            Assert.AreEqual(source.AllowedValues.Count, _parameterProperty.AllowedValues.Count);
        }

        [Test]
        public void CopyFrom_null源对象_应抛出异常()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _parameterProperty.CopyFrom(null));
        }

        #endregion
    }

    #region 辅助枚举和类

    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum PropertyType
    {
        Text,
        Number,
        Boolean,
        DateTime,
        Color,
        Point,
        Distance,
        Angle
    }

    #endregion
}