using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ZWDynLookup.Models;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 添加参数特性对话框
    /// </summary>
    public partial class AddParameterPropertyDialog : Window, INotifyPropertyChanged
    {
        private ParameterProperty _parameterProperty;
        private string _dialogTitle;
        private string _validationMessage = "";
        private bool _isEditMode;

        /// <summary>
        /// 特性属性
        /// </summary>
        public ParameterProperty ParameterProperty 
        { 
            get => _parameterProperty; 
            set
            {
                _parameterProperty = value;
                OnPropertyChanged(nameof(ParameterProperty));
            }
        }

        /// <summary>
        /// 对话框标题
        /// </summary>
        public string DialogTitle
        {
            get => _dialogTitle;
            set
            {
                _dialogTitle = value;
                OnPropertyChanged(nameof(DialogTitle));
            }
        }

        /// <summary>
        /// 验证消息
        /// </summary>
        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
                txtValidationMessage.Text = value;
            }
        }

        /// <summary>
        /// 预览信息
        /// </summary>
        public string PreviewInfo
        {
            get
            {
                if (ParameterProperty == null) return "";
                return $"特性名称: {ParameterProperty.PropertyName} | 数据类型: {ParameterProperty.DataType}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyType">特性类型</param>
        public AddParameterPropertyDialog(string propertyType = "参数特性")
        {
            InitializeComponent();
            InitializeData(propertyType, null);
        }

        /// <summary>
        /// 构造函数（编辑模式）
        /// </summary>
        /// <param name="propertyType">特性类型</param>
        /// <param name="existingProperty">现有特性</param>
        public AddParameterPropertyDialog(string propertyType, ParameterProperty existingProperty) : this(propertyType)
        {
            if (existingProperty != null)
            {
                _isEditMode = true;
                LoadExistingProperty(existingProperty);
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData(string propertyType, ParameterProperty property)
        {
            _parameterProperty = property ?? new ParameterProperty();
            _dialogTitle = property == null ? $"添加{propertyType}" : $"编辑{propertyType}";
            
            // 设置数据类型下拉框
            InitializeDataTypeComboBox();
            
            // 绑定数据上下文
            this.DataContext = this;
            
            // 设置事件处理器
            InitializeEventHandlers();
            
            // 更新预览
            UpdatePreview();
        }

        /// <summary>
        /// 加载现有特性
        /// </summary>
        private void LoadExistingProperty(ParameterProperty property)
        {
            ParameterProperty = new ParameterProperty
            {
                PropertyName = property.PropertyName,
                DataType = property.DataType,
                DefaultValue = property.DefaultValue,
                Description = property.Description,
                IsRequired = property.IsRequired,
                MinValue = property.MinValue,
                MaxValue = property.MaxValue,
                RegexPattern = property.RegexPattern
            };

            // 设置控件值
            txtPropertyName.Text = ParameterProperty.PropertyName;
            cmbDataType.SelectedValue = ParameterProperty.DataType;
            txtDefaultValue.Text = ParameterProperty.DefaultValue;
            txtDescription.Text = ParameterProperty.Description;
            chkIsRequired.IsChecked = ParameterProperty.IsRequired;
            txtMinValue.Text = ParameterProperty.MinValue;
            txtMaxValue.Text = ParameterProperty.MaxValue;
            txtRegexPattern.Text = ParameterProperty.RegexPattern;
        }

        /// <summary>
        /// 初始化数据类型下拉框
        /// </summary>
        private void InitializeDataTypeComboBox()
        {
            var dataTypes = new[]
            {
                new { DisplayName = "字符串", Value = "string" },
                new { DisplayName = "整数", Value = "int" },
                new { DisplayName = "双精度浮点数", Value = "double" },
                new { DisplayName = "十进制数", Value = "decimal" },
                new { DisplayName = "单精度浮点数", Value = "float" },
                new { DisplayName = "布尔值", Value = "bool" },
                new { DisplayName = "日期时间", Value = "DateTime" },
                new { DisplayName = "GUID", Value = "Guid" }
            };

            cmbDataType.ItemsSource = dataTypes;
            cmbDataType.DisplayMemberPath = "DisplayName";
            cmbDataType.SelectedValuePath = "Value";
        }

        /// <summary>
        /// 初始化事件处理器
        /// </summary>
        private void InitializeEventHandlers()
        {
            // 文本变化事件
            txtPropertyName.TextChanged += TextBox_TextChanged;
            txtDefaultValue.TextChanged += TextBox_TextChanged;
            txtDescription.TextChanged += TextBox_TextChanged;
            txtMinValue.TextChanged += TextBox_TextChanged;
            txtMaxValue.TextChanged += TextBox_TextChanged;
            txtRegexPattern.TextChanged += TextBox_TextChanged;
            
            // 复选框变化事件
            chkIsRequired.Checked += CheckBox_Changed;
            chkIsRequired.Unchecked += CheckBox_Changed;
            
            // 下拉框变化事件
            cmbDataType.SelectionChanged += ComboBox_SelectionChanged;
        }

        #region 事件处理方法

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                UpdateParameterProperty();
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 测试验证按钮点击事件
        /// </summary>
        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            TestValidation();
        }

        /// <summary>
        /// 文本框变化事件
        /// </summary>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateParameterProperty();
            UpdatePreview();
            ValidateInput(false);
        }

        /// <summary>
        /// 复选框变化事件
        /// </summary>
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateParameterProperty();
            UpdatePreview();
            ValidateInput(false);
        }

        /// <summary>
        /// 下拉框变化事件
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateParameterProperty();
            UpdatePreview();
            ValidateInput(false);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 更新特性属性
        /// </summary>
        private void UpdateParameterProperty()
        {
            if (_parameterProperty == null) return;

            _parameterProperty.PropertyName = txtPropertyName.Text?.Trim();
            _parameterProperty.DataType = cmbDataType.SelectedValue?.ToString();
            _parameterProperty.DefaultValue = txtDefaultValue.Text?.Trim();
            _parameterProperty.Description = txtDescription.Text?.Trim();
            _parameterProperty.IsRequired = chkIsRequired.IsChecked ?? false;
            _parameterProperty.MinValue = txtMinValue.Text?.Trim();
            _parameterProperty.MaxValue = txtMaxValue.Text?.Trim();
            _parameterProperty.RegexPattern = txtRegexPattern.Text?.Trim();
        }

        /// <summary>
        /// 更新预览
        /// </summary>
        private void UpdatePreview()
        {
            var dataType = cmbDataType.SelectedValue?.ToString();
            var defaultValue = txtDefaultValue.Text?.Trim();
            
            OnPropertyChanged(nameof(PreviewInfo));
            
            // 显示默认值预览
            if (!string.IsNullOrEmpty(defaultValue) && !string.IsNullOrEmpty(dataType))
            {
                try
                {
                    var previewValue = ConvertValue(defaultValue, dataType);
                    txtDefaultValuePreview.Text = previewValue?.ToString() ?? "转换失败";
                    gridDefaultValuePreview.Visibility = Visibility.Visible;
                }
                catch
                {
                    txtDefaultValuePreview.Text = "格式不正确";
                    gridDefaultValuePreview.Visibility = Visibility.Visible;
                }
            }
            else
            {
                gridDefaultValuePreview.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 值类型转换
        /// </summary>
        private object ConvertValue(string value, string dataType)
        {
            if (string.IsNullOrEmpty(value)) return null;

            return dataType.ToLower() switch
            {
                "string" => value,
                "int" => int.Parse(value),
                "double" => double.Parse(value, CultureInfo.InvariantCulture),
                "decimal" => decimal.Parse(value, CultureInfo.InvariantCulture),
                "float" => float.Parse(value, CultureInfo.InvariantCulture),
                "bool" => bool.Parse(value),
                "datetime" => DateTime.Parse(value),
                "guid" => Guid.Parse(value),
                _ => value
            };
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput(bool showMessage = true)
        {
            var errors = new System.Collections.Generic.List<string>();

            // 验证特性名称
            var propertyName = txtPropertyName.Text?.Trim();
            if (string.IsNullOrEmpty(propertyName))
            {
                errors.Add("特性名称不能为空");
            }
            else if (!Regex.IsMatch(propertyName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                errors.Add("特性名称只能包含字母、数字和下划线，且不能以数字开头");
            }

            // 验证数据类型
            if (cmbDataType.SelectedValue == null)
            {
                errors.Add("请选择数据类型");
            }

            // 验证默认值
            var defaultValue = txtDefaultValue.Text?.Trim();
            if (!string.IsNullOrEmpty(defaultValue))
            {
                var dataType = cmbDataType.SelectedValue?.ToString();
                try
                {
                    ConvertValue(defaultValue, dataType);
                }
                catch
                {
                    errors.Add($"默认值 '{defaultValue}' 不能转换为 {dataType} 类型");
                }
            }

            // 验证数值范围
            var minValue = txtMinValue.Text?.Trim();
            var maxValue = txtMaxValue.Text?.Trim();
            if (!string.IsNullOrEmpty(minValue) && !string.IsNullOrEmpty(maxValue))
            {
                var dataType = cmbDataType.SelectedValue?.ToString();
                if (IsNumericType(dataType))
                {
                    try
                    {
                        var min = ConvertValue(minValue, dataType);
                        var max = ConvertValue(maxValue, dataType);
                        if (CompareValues(min, max) > 0)
                        {
                            errors.Add("最小值不能大于最大值");
                        }
                    }
                    catch
                    {
                        errors.Add("最小值或最大值格式不正确");
                    }
                }
            }

            // 验证正则表达式
            var regexPattern = txtRegexPattern.Text?.Trim();
            if (!string.IsNullOrEmpty(regexPattern))
            {
                try
                {
                    Regex.IsMatch("", regexPattern);
                }
                catch
                {
                    errors.Add("正则表达式格式不正确");
                }
            }

            var errorMessage = string.Join("\n", errors);
            ValidationMessage = errorMessage;

            if (showMessage && errors.Count > 0)
            {
                MessageBox.Show(errorMessage, "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// 测试验证
        /// </summary>
        private void TestValidation()
        {
            UpdateParameterProperty();
            
            if (ValidateInput(false))
            {
                MessageBox.Show("验证通过！", "测试结果", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"验证失败：\n{ValidationMessage}", "测试结果", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 判断是否为数值类型
        /// </summary>
        private bool IsNumericType(string dataType)
        {
            return dataType?.ToLower() switch
            {
                "int" or "double" or "decimal" or "float" => true,
                _ => false
            };
        }

        /// <summary>
        /// 比较值
        /// </summary>
        private int CompareValues(object value1, object value2)
        {
            if (value1 is int int1 && value2 is int int2) return int1.CompareTo(int2);
            if (value1 is double dbl1 && value2 is double dbl2) return dbl1.CompareTo(dbl2);
            if (value1 is decimal dec1 && value2 is decimal dec2) return dec1.CompareTo(dec2);
            if (value1 is float flt1 && value2 is float flt2) return flt1.CompareTo(flt2);
            return 0;
        }

        /// <summary>
        /// 属性变化通知
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}