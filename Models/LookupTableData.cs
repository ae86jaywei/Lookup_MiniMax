using System;
using System.Collections.Generic;
using System.ComponentModel;
using ZwSoft.ZwCAD.DatabaseServices;

namespace ZWDynLookup.Models
{
    /// <summary>
    /// 查寻表数据
    /// </summary>
    public class LookupTableData : INotifyPropertyChanged
    {
        private string _actionName = "";
        private string _description = "";
        private bool _isModified = false;

        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName
        {
            get => _actionName;
            set
            {
                if (_actionName != value)
                {
                    _actionName = value;
                    _isModified = true;
                    OnPropertyChanged(nameof(ActionName));
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
        /// 参数ID
        /// </summary>
        public ObjectId ParameterId { get; set; } = ObjectId.Null;

        /// <summary>
        /// 动作ID
        /// </summary>
        public ObjectId ActionId { get; set; } = ObjectId.Null;

        /// <summary>
        /// 选择集
        /// </summary>
        public List<ObjectId> SelectionSet { get; set; } = new List<ObjectId>();

        /// <parameter name="properties">查寻特性列表</parameter>
        public List<ParameterProperty> Properties { get; set; } = new List<ParameterProperty>();

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
        public LookupTableData()
        {
            Properties = new List<ParameterProperty>();
            SelectionSet = new List<ObjectId>();
        }

        /// <summary>
        /// 复制对象
        /// </summary>
        /// <returns>复制的对象</returns>
        public LookupTableData Clone()
        {
            return new LookupTableData
            {
                ActionName = ActionName,
                Description = Description,
                ParameterId = ParameterId,
                ActionId = ActionId,
                SelectionSet = new List<ObjectId>(SelectionSet),
                Properties = new List<ParameterProperty>(Properties.Select(p => p.Clone())),
                IsModified = false
            };
        }

        /// <summary>
        /// 获取特性数量
        /// </summary>
        /// <returns>特性数量</returns>
        public int GetPropertyCount()
        {
            return Properties?.Count ?? 0;
        }

        /// <summary>
        /// 获取选择集大小
        /// </summary>
        /// <returns>选择集大小</returns>
        public int GetSelectionSetSize()
        {
            return SelectionSet?.Count ?? 0;
        }

        /// <summary>
        /// 重置修改状态
        /// </summary>
        public void ResetModified()
        {
            _isModified = false;
            OnPropertyChanged(nameof(IsModified));

            foreach (var property in Properties)
            {
                property.ResetModified();
            }
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
            return $"{ActionName} (特性: {Properties.Count}, 选择集: {SelectionSet.Count})";
        }

        /// <summary>
        /// 验证数据
        /// </summary>
        /// <returns>验证结果</returns>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(ActionName))
            {
                result.AddError("ActionName", "动作名称不能为空");
            }

            if (Properties.Count == 0)
            {
                result.AddError("Properties", "至少需要一个特性");
            }

            foreach (var property in Properties)
            {
                var propertyValidation = property.Validate();
                if (!propertyValidation.IsValid)
                {
                    result.AppendValidation(propertyValidation);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// 错误列表
        /// </summary>
        public IReadOnlyDictionary<string, List<string>> Errors => _errors;

        /// <summary>
        /// 添加错误
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="message">错误消息</param>
        public void AddError(string field, string message)
        {
            if (!_errors.ContainsKey(field))
            {
                _errors[field] = new List<string>();
            }
            _errors[field].Add(message);
        }

        /// <summary>
        /// 合并验证结果
        /// </summary>
        /// <param name="other">其他验证结果</param>
        public void AppendValidation(ValidationResult other)
        {
            foreach (var error in other.Errors)
            {
                foreach (var message in error.Value)
                {
                    AddError(error.Key, message);
                }
            }
        }

        /// <summary>
        /// 获取所有错误消息
        /// </summary>
        /// <returns>错误消息列表</returns>
        public List<string> GetAllErrorMessages()
        {
            var messages = new List<string>();
            foreach (var error in _errors)
            {
                foreach (var message in error.Value)
                {
                    messages.Add($"{error.Key}: {message}");
                }
            }
            return messages;
        }
    }
}