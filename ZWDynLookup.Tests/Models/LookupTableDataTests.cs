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
    /// 查寻表数据模型的单元测试类
    /// </summary>
    [TestFixture]
    public class LookupTableDataTests
    {
        private LookupTableData _lookupTableData;

        [SetUp]
        public void SetUp()
        {
            _lookupTableData = new LookupTableData();
        }

        [TearDown]
        public void TearDown()
        {
            _lookupTableData = null;
        }

        #region 基本属性测试

        [Test]
        public void Constructor_默认构造_应初始化属性()
        {
            // Act
            var tableData = new LookupTableData();

            // Assert
            Assert.IsNotNull(tableData.TableName);
            Assert.IsEmpty(tableData.TableName);
            Assert.IsNotNull(tableData.Rows);
            Assert.AreEqual(0, tableData.Rows.Count);
            Assert.IsNotNull(tableData.Columns);
            Assert.AreEqual(0, tableData.Columns.Count);
            Assert.IsTrue(tableData.IsValid);
        }

        [Test]
        public void TableName_设置有效名称_应成功设置()
        {
            // Arrange
            var expectedName = "TestLookupTable";

            // Act
            _lookupTableData.TableName = expectedName;

            // Assert
            Assert.AreEqual(expectedName, _lookupTableData.TableName);
        }

        [Test]
        public void TableName_设置空名称_应保持原值()
        {
            // Arrange
            _lookupTableData.TableName = "OriginalName";

            // Act
            _lookupTableData.TableName = "";

            // Assert
            Assert.AreEqual("OriginalName", _lookupTableData.TableName);
        }

        [Test]
        public void TableName_设置null_应保持原值()
        {
            // Arrange
            _lookupTableData.TableName = "OriginalName";

            // Act
            _lookupTableData.TableName = null;

            // Assert
            Assert.AreEqual("OriginalName", _lookupTableData.TableName);
        }

        #endregion

        #region 列管理测试

        [Test]
        public void AddColumn_有效列信息_应成功添加()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "TestColumn",
                Type = ColumnType.Text,
                IsRequired = true,
                DefaultValue = "Default"
            };

            // Act
            var result = _lookupTableData.AddColumn(columnInfo);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _lookupTableData.Columns.Count);
            Assert.AreEqual("TestColumn", _lookupTableData.Columns[0].Name);
        }

        [Test]
        public void AddColumn_重复列名_应返回false()
        {
            // Arrange
            var column1 = new ColumnInfo
            {
                Name = "TestColumn",
                Type = ColumnType.Text
            };
            var column2 = new ColumnInfo
            {
                Name = "TestColumn", // 重复名称
                Type = ColumnType.Number
            };
            _lookupTableData.AddColumn(column1);

            // Act
            var result = _lookupTableData.AddColumn(column2);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, _lookupTableData.Columns.Count);
        }

        [Test]
        public void AddColumn_空列名_应返回false()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "",
                Type = ColumnType.Text
            };

            // Act
            var result = _lookupTableData.AddColumn(columnInfo);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _lookupTableData.Columns.Count);
        }

        [Test]
        public void AddColumn_列名包含特殊字符_应返回false()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "Test@Column",
                Type = ColumnType.Text
            };

            // Act
            var result = _lookupTableData.AddColumn(columnInfo);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _lookupTableData.Columns.Count);
        }

        [Test]
        public void RemoveColumn_存在的列_应成功删除()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "TestColumn",
                Type = ColumnType.Text
            };
            _lookupTableData.AddColumn(columnInfo);

            // Act
            var result = _lookupTableData.RemoveColumn("TestColumn");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, _lookupTableData.Columns.Count);
        }

        [Test]
        public void RemoveColumn_不存在的列_应返回false()
        {
            // Act
            var result = _lookupTableData.RemoveColumn("NonExistentColumn");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetColumn_存在的列_应返回列信息()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "TestColumn",
                Type = ColumnType.Text
            };
            _lookupTableData.AddColumn(columnInfo);

            // Act
            var result = _lookupTableData.GetColumn("TestColumn");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestColumn", result.Name);
            Assert.AreEqual(ColumnType.Text, result.Type);
        }

        [Test]
        public void GetColumn_不存在的列_应返回null()
        {
            // Act
            var result = _lookupTableData.GetColumn("NonExistentColumn");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void UpdateColumn_存在的列_应成功更新()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "TestColumn",
                Type = ColumnType.Text
            };
            _lookupTableData.AddColumn(columnInfo);

            var updatedColumnInfo = new ColumnInfo
            {
                Name = "TestColumn",
                Type = ColumnType.Number,
                IsRequired = true
            };

            // Act
            var result = _lookupTableData.UpdateColumn("TestColumn", updatedColumnInfo);

            // Assert
            Assert.IsTrue(result);
            var column = _lookupTableData.GetColumn("TestColumn");
            Assert.AreEqual(ColumnType.Number, column.Type);
            Assert.IsTrue(column.IsRequired);
        }

        [Test]
        public void UpdateColumn_不存在的列_应返回false()
        {
            // Arrange
            var columnInfo = new ColumnInfo
            {
                Name = "NonExistentColumn",
                Type = ColumnType.Text
            };

            // Act
            var result = _lookupTableData.UpdateColumn("NonExistentColumn", columnInfo);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 行管理测试

        [Test]
        public void AddRow_有效行数据_应成功添加()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column2", Type = ColumnType.Number });

            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value1" },
                { "Column2", 123 }
            };

            // Act
            var result = _lookupTableData.AddRow(rowData);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _lookupTableData.Rows.Count);
        }

        [Test]
        public void AddRow_缺少必需列_应返回false()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text, IsRequired = true });
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column2", Type = ColumnType.Number });

            var rowData = new Dictionary<string, object>
            {
                { "Column2", 123 } // 缺少必需列Column1
            };

            // Act
            var result = _lookupTableData.AddRow(rowData);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _lookupTableData.Rows.Count);
        }

        [Test]
        public void AddRow_数据类型不匹配_应返回false()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "NumberColumn", Type = ColumnType.Number });

            var rowData = new Dictionary<string, object>
            {
                { "NumberColumn", "NotANumber" } // 应该是数字类型
            };

            // Act
            var result = _lookupTableData.AddRow(rowData);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _lookupTableData.Rows.Count);
        }

        [Test]
        public void AddRow_空行数据_应返回false()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });

            var emptyRowData = new Dictionary<string, object>();

            // Act
            var result = _lookupTableData.AddRow(emptyRowData);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _lookupTableData.Rows.Count);
        }

        [Test]
        public void UpdateRow_存在的行_应成功更新()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            
            var originalRowData = new Dictionary<string, object>
            {
                { "Column1", "OriginalValue" }
            };
            _lookupTableData.AddRow(originalRowData);

            var updatedRowData = new Dictionary<string, object>
            {
                { "Column1", "UpdatedValue" }
            };

            // Act
            var result = _lookupTableData.UpdateRow(0, updatedRowData);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("UpdatedValue", _lookupTableData.Rows[0]["Column1"]);
        }

        [Test]
        public void UpdateRow_不存在的行_应返回false()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });

            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value" }
            };

            // Act
            var result = _lookupTableData.UpdateRow(999, rowData); // 不存在的行索引

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveRow_存在的行_应成功删除()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            
            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value" }
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.RemoveRow(0);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, _lookupTableData.Rows.Count);
        }

        [Test]
        public void RemoveRow_不存在的行_应返回false()
        {
            // Act
            var result = _lookupTableData.RemoveRow(999); // 不存在的行索引

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetRow_存在的行_应返回行数据()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            
            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value" }
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.GetRow(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Value", result["Column1"]);
        }

        [Test]
        public void GetRow_不存在的行_应返回null()
        {
            // Act
            var result = _lookupTableData.GetRow(999);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region 验证测试

        [Test]
        public void Validate_有效数据_应返回true()
        {
            // Arrange
            _lookupTableData.TableName = "ValidTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text, IsRequired = true });
            
            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value" }
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.Validate();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Validate_表名为空_应返回false()
        {
            // Arrange
            _lookupTableData.TableName = ""; // 空表名

            // Act
            var result = _lookupTableData.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_无列定义_应返回false()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            // 没有添加任何列

            // Act
            var result = _lookupTableData.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_有列但无行_应返回true()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });

            // Act
            var result = _lookupTableData.Validate();

            // Assert
            Assert.IsTrue(result); // 空表也是有效的
        }

        [Test]
        public void Validate_数据类型不匹配_应返回false()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "NumberColumn", Type = ColumnType.Number });
            
            var rowData = new Dictionary<string, object>
            {
                { "NumberColumn", "NotANumber" }
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_缺少必需列_应返回false()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "RequiredColumn", Type = ColumnType.Text, IsRequired = true });
            _lookupTableData.AddColumn(new ColumnInfo { Name = "OptionalColumn", Type = ColumnType.Text });
            
            var rowData = new Dictionary<string, object>
            {
                { "OptionalColumn", "Value" } // 缺少必需列
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.Validate();

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 序列化测试

        [Test]
        public void ToJson_有效数据_应返回有效JSON()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            
            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value1" }
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.ToJson();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("TestTable"));
            Assert.IsTrue(result.Contains("Column1"));
            Assert.IsTrue(result.Contains("Value1"));
        }

        [Test]
        public void FromJson_有效JSON_应成功反序列化()
        {
            // Arrange
            var json = @"
            {
                ""tableName"": ""TestTable"",
                ""columns"": [
                    {
                        ""name"": ""Column1"",
                        ""type"": ""Text"",
                        ""isRequired"": true
                    }
                ],
                ""rows"": [
                    {
                        ""Column1"": ""Value1""
                    }
                ]
            }";

            // Act
            var result = LookupTableData.FromJson(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestTable", result.TableName);
            Assert.AreEqual(1, result.Columns.Count);
            Assert.AreEqual(1, result.Rows.Count);
        }

        [Test]
        public void FromJson_无效JSON_应抛出异常()
        {
            // Arrange
            var invalidJson = "invalid json content";

            // Act & Assert
            Assert.Throws<JsonDeserializationException>(() => LookupTableData.FromJson(invalidJson));
        }

        [Test]
        public void FromJson_空JSON_应抛出异常()
        {
            // Arrange
            var emptyJson = "";

            // Act & Assert
            Assert.Throws<JsonDeserializationException>(() => LookupTableData.FromJson(emptyJson));
        }

        #endregion

        #region 导入导出测试

        [Test]
        public void ExportToCsv_有效数据_应返回有效CSV内容()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column2", Type = ColumnType.Number });
            
            var row1 = new Dictionary<string, object>
            {
                { "Column1", "Value1" },
                { "Column2", 123 }
            };
            var row2 = new Dictionary<string, object>
            {
                { "Column1", "Value2" },
                { "Column2", 456 }
            };
            _lookupTableData.AddRow(row1);
            _lookupTableData.AddRow(row2);

            // Act
            var result = _lookupTableData.ExportToCsv();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Column1"));
            Assert.IsTrue(result.Contains("Column2"));
            Assert.IsTrue(result.Contains("Value1"));
            Assert.IsTrue(result.Contains("Value2"));
        }

        [Test]
        public void ImportFromCsv_有效CSV_应成功导入()
        {
            // Arrange
            var csvContent = @"Column1,Column2
Value1,123
Value2,456";

            // Act
            var result = _lookupTableData.ImportFromCsv(csvContent);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, _lookupTableData.Columns.Count);
            Assert.AreEqual(2, _lookupTableData.Rows.Count);
        }

        [Test]
        public void ImportFromCsv_无效CSV_应返回false()
        {
            // Arrange
            var invalidCsvContent = "invalid,csv,content";

            // Act
            var result = _lookupTableData.ImportFromCsv(invalidCsvContent);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 克隆测试

        [Test]
        public void Clone_有效对象_应成功克隆()
        {
            // Arrange
            _lookupTableData.TableName = "TestTable";
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });
            
            var rowData = new Dictionary<string, object>
            {
                { "Column1", "Value" }
            };
            _lookupTableData.AddRow(rowData);

            // Act
            var result = _lookupTableData.Clone();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_lookupTableData.TableName, result.TableName);
            Assert.AreEqual(_lookupTableData.Columns.Count, result.Columns.Count);
            Assert.AreEqual(_lookupTableData.Rows.Count, result.Rows.Count);
            
            // 验证是深拷贝
            result.TableName = "ModifiedName";
            Assert.AreNotEqual(_lookupTableData.TableName, result.TableName);
        }

        #endregion

        #region 查找测试

        [Test]
        public void FindRows_有效查询条件_应返回匹配行()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Name", Type = ColumnType.Text });
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Age", Type = ColumnType.Number });
            
            var row1 = new Dictionary<string, object>
            {
                { "Name", "Alice" },
                { "Age", 25 }
            };
            var row2 = new Dictionary<string, object>
            {
                { "Name", "Bob" },
                { "Age", 30 }
            };
            _lookupTableData.AddRow(row1);
            _lookupTableData.AddRow(row2);

            var query = new RowQuery
            {
                Conditions = new Dictionary<string, object>
                {
                    { "Name", "Alice" }
                }
            };

            // Act
            var result = _lookupTableData.FindRows(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Alice", result[0]["Name"]);
        }

        [Test]
        public void FindRows_无匹配条件_应返回空列表()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Name", Type = ColumnType.Text });
            
            var rowData = new Dictionary<string, object>
            {
                { "Name", "Alice" }
            };
            _lookupTableData.AddRow(rowData);

            var query = new RowQuery
            {
                Conditions = new Dictionary<string, object>
                {
                    { "Name", "NonExistent" }
                }
            };

            // Act
            var result = _lookupTableData.FindRows(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region 边界条件测试

        [Test]
        public void AddRow_边界测试_大量行数据()
        {
            // Arrange
            _lookupTableData.AddColumn(new ColumnInfo { Name = "Column1", Type = ColumnType.Text });

            // 添加大量行
            for (int i = 0; i < 10000; i++)
            {
                var rowData = new Dictionary<string, object>
                {
                    { "Column1", $"Value{i}" }
                };
                _lookupTableData.AddRow(rowData);
            }

            // Act & Assert
            Assert.AreEqual(10000, _lookupTableData.Rows.Count);
        }

        [Test]
        public void AddColumn_边界测试_大量列()
        {
            // 添加大量列
            for (int i = 0; i < 1000; i++)
            {
                var columnInfo = new ColumnInfo
                {
                    Name = $"Column{i}",
                    Type = ColumnType.Text
                };
                _lookupTableData.AddColumn(columnInfo);
            }

            // Act & Assert
            Assert.AreEqual(1000, _lookupTableData.Columns.Count);
        }

        [Test]
        public void TableName_边界测试_超长名称()
        {
            // Arrange
            var longName = new string('A', 1000); // 超长名称

            // Act
            _lookupTableData.TableName = longName;

            // Assert
            // 应该能够处理长名称
            Assert.AreEqual(longName, _lookupTableData.TableName);
        }

        #endregion

        #region 辅助类

        /// <summary>
        /// 列信息类
        /// </summary>
        public class ColumnInfo
        {
            public string Name { get; set; }
            public ColumnType Type { get; set; }
            public bool IsRequired { get; set; }
            public object DefaultValue { get; set; }
        }

        /// <summary>
        /// 列类型枚举
        /// </summary>
        public enum ColumnType
        {
            Text,
            Number,
            Boolean,
            DateTime
        }

        /// <summary>
        /// 行查询类
        /// </summary>
        public class RowQuery
        {
            public Dictionary<string, object> Conditions { get; set; }
        }

        /// <summary>
        /// JSON反序列化异常类
        /// </summary>
        public class JsonDeserializationException : Exception
        {
            public JsonDeserializationException(string message) : base(message) { }
        }

        #endregion
    }
}