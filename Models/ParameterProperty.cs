using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ZWDynLookup.Models
{
    /// <summary>
    /// 参数特性类型
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// 输入特性
        /// </summary>
        Input,

        /// <summary>
        /// 查寻特性
        /// </summary>
        Lookup
    }

    /// <summary>
    /// 参数特性
    /// </summary>
    public class ParameterProperty : INotifyPropertyChanged
    {
        private string _name = "";
        private string _displayName = "";
        private string _description = "";
        private string _value = "";
        private string _displayValue = "";
        private PropertyType _type = PropertyType.Input;
        private bool _isModified = false;

        /// <summary>
        /// 特性名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        /// <summary>
        /// 特性值
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// 显示值
        /// </summary>
        public string DisplayValue
        {
            get => _displayValue;
            set
            {
                if (_displayValue != value)
                {
                    _displayValue = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(DisplayValue));
                }
            }
        }

        /// <summary>
        /// 特性类型
        /// </summary>
        public PropertyType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        /// <summary>
        /// 是否已修改
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ParameterProperty()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">特性名称</param>
        /// <param name="displayName">显示名称</param>
        /// <param name="type">特性类型</param>
        public ParameterProperty(string name, string displayName, PropertyType type)
        {
            Name = name;
            DisplayName = displayName;
            Type = type;
        }

        /// <summary>
        /// 复制对象
        /// </summary>
        /// <returns>复制的对象</returns>
        public ParameterProperty Clone()
        {
            return new ParameterProperty
            {
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Value = Value,
                DisplayValue = DisplayValue,
                Type = Type,
                IsModified = false
            };
        }

        /// <summary>
        /// 重置修改状态
        /// </summary>
        public void ResetModified()
        {
            _isModified = false;
            OnPropertyChanged(nameof(IsModified));
        }

        /// <summary>
        /// 获取特性类型的显示名称
        /// </summary>
        /// <returns>显示名称</returns>
        public string GetTypeDisplayName()
        {
            return Type switch
            {
                PropertyType.Input => "输入特性",
                PropertyType.Lookup => "查寻特性",
                _ => "未知"
            };
        }

        /// <summary>
        /// 验证特性
        /// </summary>
        /// <returns>验证结果</returns>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.AddError("Name", "特性名称不能为空");
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                result.AddError("DisplayName", "显示名称不能为空");
            }

            if (string.IsNullOrWhiteSpace(Value))
            {
                result.AddError("Value", "特性值不能为空");
            }

            if (Type == PropertyType.Lookup && string.IsNullOrWhiteSpace(DisplayValue))
            {
                result.AddError("DisplayValue", "查寻特性的显示值不能为空");
            }

            return result;
        }

        /// <summary>
        /// 属性更改事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性更改事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"{DisplayName} ({GetTypeDisplayName()}) = {DisplayValue}";
        }

        /// <summary>
        /// 静态方法：从名称列表创建特性
        /// </summary>
        /// <param name="names">名称列表</param>
        /// <param name="type">特性类型</param>
        /// <returns>特性列表</returns>
        public static List<ParameterProperty> CreateFromNames(List<string> names, PropertyType type)
        {
            var properties = new List<ParameterProperty>();
            
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    properties.Add(new ParameterProperty
                    {
                        Name = name,
                        DisplayName = name,
                        Type = type,
                        Value = name
                    });
                }
            }

            return properties;
        }

        /// <summary>
        /// 静态方法：验证特性列表
        /// </summary>
        /// <param name="properties">特性列表</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateList(List<ParameterProperty> properties)
        {
            var result = new ValidationResult();

            if (properties == null || properties.Count == 0)
            {
                result.AddError("Properties", "特性列表不能为空");
                return result;
            }

            // 检查名称重复
            var names = properties.Select(p => p.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            var duplicateNames = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            
            foreach (var duplicateName in duplicateNames)
            {
                result.AddError("Name", $"特性名称 '{duplicateName}' 重复");
            }

            // 验证每个特性
            for (int i = 0; i < properties.Count; i++)
            {
                var propertyValidation = properties[i].Validate();
                if (!propertyValidation.IsValid)
                {
                    foreach (var error in propertyValidation.Errors)
                    {
                        foreach (var message in error.Value)
                        {
                            result.AddError($"Properties[{i}].{error.Key}", message);
                        }
                    }
                }
            }

            return result;
        }
    }
}