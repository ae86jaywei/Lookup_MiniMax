using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ZWDynLookup.Models;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 数据验证助手类
    /// 提供各种数据验证功能
    /// </summary>
    public static class DataValidationHelper
    {
        /// <summary>
        /// 验证参数特性
        /// </summary>
        /// <param name="property">参数特性</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateParameterProperty(ParameterProperty property)
        {
            var errors = new List<string>();

            if (property == null)
            {
                return new ValidationResult(false, "参数特性不能为空");
            }

            // 验证特性名称
            if (string.IsNullOrWhiteSpace(property.PropertyName))
            {
                errors.Add("特性名称不能为空");
            }
            else if (!Regex.IsMatch(property.PropertyName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                errors.Add("特性名称只能包含字母、数字和下划线，且不能以数字开头");
            }

            // 验证数据类型
            if (string.IsNullOrWhiteSpace(property.DataType))
            {
                errors.Add("数据类型不能为空");
            }
            else if (!IsValidDataType(property.DataType))
            {
                errors.Add($"不支持的数据类型: {property.DataType}");
            }

            // 验证默认值
            if (!string.IsNullOrWhiteSpace(property.DefaultValue))
            {
                var validationResult = ValidateValue(property.DefaultValue, property.DataType);
                if (!validationResult.IsValid)
                {
                    errors.Add($"默认值验证失败: {validationResult.ErrorMessage}");
                }
            }

            // 验证数值范围
            if (!string.IsNullOrWhiteSpace(property.MinValue) || !string.IsNullOrWhiteSpace(property.MaxValue))
            {
                var rangeValidation = ValidateRange(property.MinValue, property.MaxValue, property.DataType);
                if (!rangeValidation.IsValid)
                {
                    errors.Add($"数值范围验证失败: {rangeValidation.ErrorMessage}");
                }
            }

            // 验证正则表达式
            if (!string.IsNullOrWhiteSpace(property.RegexPattern))
            {
                try
                {
                    Regex.IsMatch("", property.RegexPattern);
                }
                catch
                {
                    errors.Add("正则表达式格式不正确");
                }
            }

            return new ValidationResult(errors.Count == 0, string.Join("\n", errors));
        }

        /// <summary>
        /// 验证值
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateValue(string value, string dataType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new ValidationResult(true, "值为空");
            }

            try
            {
                switch (dataType?.ToLower())
                {
                    case "string":
                        // 字符串不需要特殊验证
                        return new ValidationResult(true, "验证通过");

                    case "int":
                        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的整数");
                        }
                        return new ValidationResult(true, "验证通过");

                    case "double":
                        if (!double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的双精度浮点数");
                        }
                        return new ValidationResult(true, "验证通过");

                    case "decimal":
                        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的十进制数");
                        }
                        return new ValidationResult(true, "验证通过");

                    case "float":
                        if (!float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的单精度浮点数");
                        }
                        return new ValidationResult(true, "验证通过");

                    case "bool":
                        if (!bool.TryParse(value, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的布尔值 (true/false)");
                        }
                        return new ValidationResult(true, "验证通过");

                    case "datetime":
                        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的日期时间格式");
                        }
                        return new ValidationResult(true, "验证通过");

                    case "guid":
                        if (!Guid.TryParse(value, out _))
                        {
                            return new ValidationResult(false, $"'{value}' 不是有效的GUID格式");
                        }
                        return new ValidationResult(true, "验证通过");

                    default:
                        return new ValidationResult(false, $"不支持的数据类型: {dataType}");
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"验证过程中发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证数值范围
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateRange(string minValue, string maxValue, string dataType)
        {
            // 如果没有提供范围值，则认为有效
            if (string.IsNullOrWhiteSpace(minValue) && string.IsNullOrWhiteSpace(maxValue))
            {
                return new ValidationResult(true, "未指定范围");
            }

            // 验证是否为数值类型
            if (!IsNumericType(dataType))
            {
                return new ValidationResult(false, "范围验证仅适用于数值类型");
            }

            try
            {
                object min = null, max = null;

                if (!string.IsNullOrWhiteSpace(minValue))
                {
                    var minValidation = ValidateValue(minValue, dataType);
                    if (!minValidation.IsValid)
                    {
                        return minValidation;
                    }
                    min = ConvertValue(minValue, dataType);
                }

                if (!string.IsNullOrWhiteSpace(maxValue))
                {
                    var maxValidation = ValidateValue(maxValue, dataType);
                    if (!maxValidation.IsValid)
                    {
                        return maxValidation;
                    }
                    max = ConvertValue(maxValue, dataType);
                }

                // 比较最小值和最大值
                if (min != null && max != null)
                {
                    if (CompareValues(min, max) > 0)
                    {
                        return new ValidationResult(false, "最小值不能大于最大值");
                    }
                }

                return new ValidationResult(true, "范围验证通过");
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"范围验证过程中发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证正则表达式
        /// </summary>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="testValue">测试值（可选）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateRegexPattern(string pattern, string testValue = null)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return new ValidationResult(true, "未指定正则表达式");
            }

            try
            {
                var regex = new Regex(pattern);
                
                if (!string.IsNullOrEmpty(testValue))
                {
                    var match = regex.Match(testValue);
                    if (!match.Success)
                    {
                        return new ValidationResult(false, $"正则表达式验证失败: '{testValue}' 不匹配模式 '{pattern}'");
                    }
                }

                return new ValidationResult(true, "正则表达式验证通过");
            }
            catch (ArgumentException ex)
            {
                return new ValidationResult(false, $"正则表达式格式错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"正则表达式验证过程中发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证查寻表数据
        /// </summary>
        /// <param name="inputProperties">输入特性列表</param>
        /// <param name="lookupProperties">查寻特性列表</param>
        /// <param name="data">查寻表数据</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateLookupTableData(
            List<ParameterProperty> inputProperties, 
            List<ParameterProperty> lookupProperties, 
            object data)
        {
            var errors = new List<string>();

            // 验证特性列表
            if (inputProperties == null || inputProperties.Count == 0)
            {
                errors.Add("输入特性列表不能为空");
            }

            if (lookupProperties == null || lookupProperties.Count == 0)
            {
                errors.Add("查寻特性列表不能为空");
            }

            // 检查特性名称重复
            if (inputProperties != null && lookupProperties != null)
            {
                var allProperties = inputProperties.Concat(lookupProperties);
                var duplicateNames = allProperties
                    .GroupBy(p => p.PropertyName)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateNames.Count > 0)
                {
                    errors.Add($"发现重复的特性名称: {string.Join(", ", duplicateNames)}");
                }
            }

            // 验证数据格式
            if (data != null)
            {
                // 这里可以根据具体的数据结构进行验证
                // 例如：验证DataTable、List<T>等
                var dataValidation = ValidateDataStructure(data);
                if (!dataValidation.IsValid)
                {
                    errors.Add($"数据结构验证失败: {dataValidation.ErrorMessage}");
                }
            }

            return new ValidationResult(errors.Count == 0, string.Join("\n", errors));
        }

        /// <summary>
        /// 验证数据类型
        /// </summary>
        /// <param name="dataType">数据类型字符串</param>
        /// <returns>是否为有效的数据类型</returns>
        public static bool IsValidDataType(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType)) return false;

            var validTypes = new[]
            {
                "string", "int", "double", "decimal", "float",
                "bool", "datetime", "guid", "byte", "short",
                "long", "uint", "ulong", "ushort"
            };

            return validTypes.Contains(dataType.ToLower());
        }

        /// <summary>
        /// 判断是否为数值类型
        /// </summary>
        /// <param name="dataType">数据类型字符串</param>
        /// <returns>是否为数值类型</returns>
        public static bool IsNumericType(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType)) return false;

            var numericTypes = new[]
            {
                "int", "double", "decimal", "float",
                "byte", "short", "long", "uint", "ulong", "ushort"
            };

            return numericTypes.Contains(dataType.ToLower());
        }

        /// <summary>
        /// 转换值
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>转换后的值</returns>
        public static object ConvertValue(string value, string dataType)
        {
            if (string.IsNullOrEmpty(value)) return null;

            return dataType?.ToLower() switch
            {
                "string" => value,
                "int" => int.Parse(value, CultureInfo.InvariantCulture),
                "double" => double.Parse(value, CultureInfo.InvariantCulture),
                "decimal" => decimal.Parse(value, CultureInfo.InvariantCulture),
                "float" => float.Parse(value, CultureInfo.InvariantCulture),
                "bool" => bool.Parse(value),
                "datetime" => DateTime.Parse(value, CultureInfo.InvariantCulture),
                "guid" => Guid.Parse(value),
                "byte" => byte.Parse(value, CultureInfo.InvariantCulture),
                "short" => short.Parse(value, CultureInfo.InvariantCulture),
                "long" => long.Parse(value, CultureInfo.InvariantCulture),
                "uint" => uint.Parse(value, CultureInfo.InvariantCulture),
                "ulong" => ulong.Parse(value, CultureInfo.InvariantCulture),
                "ushort" => ushort.Parse(value, CultureInfo.InvariantCulture),
                _ => value
            };
        }

        /// <summary>
        /// 比较值
        /// </summary>
        /// <param name="value1">值1</param>
        /// <param name="value2">值2</param>
        /// <returns>比较结果 (-1: value1 < value2, 0: value1 = value2, 1: value1 > value2)</returns>
        public static int CompareValues(object value1, object value2)
        {
            if (value1 == null && value2 == null) return 0;
            if (value1 == null) return -1;
            if (value2 == null) return 1;

            // 根据值类型进行比较
            if (value1 is int i1 && value2 is int i2) return i1.CompareTo(i2);
            if (value1 is double d1 && value2 is double d2) return d1.CompareTo(d2);
            if (value1 is decimal dec1 && value2 is decimal dec2) return dec1.CompareTo(dec2);
            if (value1 is float f1 && value2 is float f2) return f1.CompareTo(f2);
            if (value1 is long l1 && value2 is long l2) return l1.CompareTo(l2);
            if (value1 is DateTime dt1 && value2 is DateTime dt2) return dt1.CompareTo(dt2);
            if (value1 is string s1 && value2 is string s2) return string.Compare(s1, s2, StringComparison.Ordinal);

            // 转换为字符串进行比较
            return string.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// 验证数据结构
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>验证结果</returns>
        private static ValidationResult ValidateDataStructure(object data)
        {
            if (data == null)
            {
                return new ValidationResult(true, "数据为空");
            }

            try
            {
                // 这里可以根据需要添加更多数据结构的验证
                // 例如：验证DataTable、List<T>等
                
                // 简单的类型检查
                var dataType = data.GetType();
                if (dataType.IsPrimitive || dataType == typeof(string) || dataType.IsClass)
                {
                    return new ValidationResult(true, "数据结构验证通过");
                }

                return new ValidationResult(false, $"不支持的数据结构类型: {dataType.Name}");
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"数据结构验证过程中发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取数据类型的默认值
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <returns>默认值</returns>
        public static object GetDefaultValue(string dataType)
        {
            return dataType?.ToLower() switch
            {
                "string" => "",
                "int" => 0,
                "double" => 0.0,
                "decimal" => 0m,
                "float" => 0f,
                "bool" => false,
                "datetime" => DateTime.MinValue,
                "guid" => Guid.Empty,
                "byte" => (byte)0,
                "short" => (short)0,
                "long" => 0L,
                "uint" => 0U,
                "ulong" => 0UL,
                "ushort" => (ushort)0,
                _ => null
            };
        }

        /// <summary>
        /// 格式化值
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="format">格式化字符串（可选）</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatValue(object value, string dataType, string format = null)
        {
            if (value == null) return "";

            try
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return string.Format(CultureInfo.InvariantCulture, $"{{0:{format}}}", value);
                }

                return dataType?.ToLower() switch
                {
                    "decimal" => ((decimal)value).ToString("F2", CultureInfo.InvariantCulture),
                    "double" => ((double)value).ToString("G", CultureInfo.InvariantCulture),
                    "float" => ((float)value).ToString("G", CultureInfo.InvariantCulture),
                    "datetime" => ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    _ => value.ToString()
                };
            }
            catch
            {
                return value?.ToString() ?? "";
            }
        }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isValid">是否验证通过</param>
        /// <param name="errorMessage">错误消息</param>
        public ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage ?? "";
        }
    }
}