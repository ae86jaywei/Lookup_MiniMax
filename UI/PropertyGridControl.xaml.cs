using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ZWDynLookup.Models;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 属性网格控件
    /// </summary>
    public partial class PropertyGridControl : UserControl
    {
        private object _selectedObject;
        private List<object> _availableObjects;
        private Dictionary<string, PropertyItem> _propertyItems;
        private PropertyDescriptorCollection _propertyDescriptors;
        private bool _isUpdating = false;

        /// <summary>
        /// 选中的对象
        /// </summary>
        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject != value)
                {
                    _selectedObject = value;
                    RefreshPropertyGrid();
                    OnSelectionChanged();
                }
            }
        }

        /// <summary>
        /// 可用对象列表
        /// </summary>
        public List<object> AvailableObjects
        {
            get => _availableObjects;
            set
            {
                _availableObjects = value ?? new List<object>();
                RefreshObjectSelector();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PropertyGridControl()
        {
            InitializeComponent();
            InitializeData();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            _availableObjects = new List<object>();
            _propertyItems = new Dictionary<string, PropertyItem>();
            
            // 设置默认可用对象
            SetDefaultAvailableObjects();
        }

        /// <summary>
        /// 设置默认可用对象
        /// </summary>
        private void SetDefaultAvailableObjects()
        {
            // 创建默认的ParameterProperty对象
            var defaultProperty = new ParameterProperty
            {
                PropertyName = "ExampleProperty",
                DataType = "string",
                DefaultValue = "",
                Description = "示例属性",
                IsRequired = false
            };

            AvailableObjects = new List<object> { defaultProperty };
            SelectedObject = defaultProperty;
        }

        /// <summary>
        /// 刷新对象选择器
        /// </summary>
        private void RefreshObjectSelector()
        {
            cmbObjectSelector.ItemsSource = null;
            cmbObjectSelector.ItemsSource = _availableObjects;
            
            if (_availableObjects.Count > 0)
            {
                cmbObjectSelector.SelectedIndex = _availableObjects.IndexOf(_selectedObject);
                if (cmbObjectSelector.SelectedIndex < 0)
                {
                    cmbObjectSelector.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 刷新属性网格
        /// </summary>
        private void RefreshPropertyGrid()
        {
            spPropertyGrid.Children.Clear();
            _propertyItems.Clear();

            if (_selectedObject == null)
            {
                // 显示空状态
                var emptyText = new TextBlock
                {
                    Text = "未选择对象",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                spPropertyGrid.Children.Add(emptyText);
                return;
            }

            // 获取属性描述符
            var properties = TypeDescriptor.GetProperties(_selectedObject);
            _propertyDescriptors = properties;

            // 按类别分组属性
            var groupedProperties = properties.Cast<PropertyDescriptor>()
                .GroupBy(p => p.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groupedProperties)
            {
                // 添加类别标题
                AddCategoryHeader(group.Key);
                
                // 添加类别属性
                foreach (var property in group.OrderBy(p => p.DisplayName))
                {
                    AddPropertyRow(property);
                }
            }
        }

        /// <summary>
        /// 添加类别标题
        /// </summary>
        private void AddCategoryHeader(string categoryName)
        {
            var headerBorder = new Border
            {
                Style = (Style)FindResource("CategoryHeaderStyle")
            };

            var headerText = new TextBlock
            {
                Text = categoryName,
                FontWeight = FontWeights.Bold,
                FontSize = 11
            };

            headerBorder.Child = headerText;
            spPropertyGrid.Children.Add(headerBorder);
        }

        /// <summary>
        /// 添加属性行
        /// </summary>
        private void AddPropertyRow(PropertyDescriptor property)
        {
            var propertyGrid = new Grid
            {
                Style = (Style)FindResource("PropertyRowStyle")
            };

            propertyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            propertyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 属性名称
            var nameTextBlock = new TextBlock
            {
                Text = property.DisplayName,
                Style = (Style)FindResource("PropertyNameStyle"),
                ToolTip = property.Description
            };

            // 属性值编辑器
            var valueEditor = CreatePropertyEditor(property);
            valueEditor.Style = (Style)FindResource("PropertyValueStyle");
            
            Grid.SetColumn(nameTextBlock, 0);
            Grid.SetColumn(valueEditor, 1);

            propertyGrid.Children.Add(nameTextBlock);
            propertyGrid.Children.Add(valueEditor);

            // 添加属性项到字典
            var propertyItem = new PropertyItem
            {
                PropertyDescriptor = property,
                NameControl = nameTextBlock,
                ValueControl = valueEditor,
                PropertyGrid = this
            };

            _propertyItems[property.Name] = propertyItem;

            spPropertyGrid.Children.Add(propertyGrid);

            // 初始化属性值
            InitializePropertyValue(propertyItem);
        }

        /// <summary>
        /// 创建属性编辑器
        /// </summary>
        private FrameworkElement CreatePropertyEditor(PropertyDescriptor property)
        {
            var propertyType = property.PropertyType;
            var isReadOnly = property.IsReadOnly;
            var isBrowsable = property.IsBrowsable;

            // 处理可空类型
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            // 根据属性类型创建编辑器
            if (propertyType == typeof(bool))
            {
                return CreateCheckBoxEditor(property, isReadOnly);
            }
            else if (propertyType == typeof(DateTime))
            {
                return CreateDateTimeEditor(property, isReadOnly);
            }
            else if (propertyType.IsEnum)
            {
                return CreateEnumEditor(property, isReadOnly);
            }
            else if (propertyType == typeof(string))
            {
                return CreateStringEditor(property, isReadOnly);
            }
            else if (IsNumericType(propertyType))
            {
                return CreateNumericEditor(property, isReadOnly);
            }
            else if (propertyType == typeof(Color))
            {
                return CreateColorEditor(property, isReadOnly);
            }
            else
            {
                return CreateDefaultEditor(property, isReadOnly);
            }
        }

        /// <summary>
        /// 创建复选框编辑器
        /// </summary>
        private FrameworkElement CreateCheckBoxEditor(PropertyDescriptor property, bool isReadOnly)
        {
            var checkBox = new CheckBox
            {
                IsEnabled = !isReadOnly,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 绑定数据
            var binding = new Binding(property.Name)
            {
                Source = _selectedObject,
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);

            // 添加事件处理器
            checkBox.Checked += (s, e) => OnPropertyValueChanged(property);
            checkBox.Unchecked += (s, e) => OnPropertyValueChanged(property);

            return checkBox;
        }

        /// <summary>
        /// 创建日期时间编辑器
        /// </summary>
        private FrameworkElement CreateDateTimeEditor(PropertyDescriptor property, bool isReadOnly)
        {
            var datePicker = new DatePicker
            {
                IsEnabled = !isReadOnly
            };

            // 绑定数据
            var binding = new Binding(property.Name)
            {
                Source = _selectedObject,
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            datePicker.SetBinding(DatePicker.SelectedDateProperty, binding);

            // 添加事件处理器
            datePicker.SelectedDateChanged += (s, e) => OnPropertyValueChanged(property);

            return datePicker;
        }

        /// <summary>
        /// 创建枚举编辑器
        /// </summary>
        private FrameworkElement CreateEnumEditor(PropertyDescriptor property, bool isReadOnly)
        {
            var comboBox = new ComboBox
            {
                IsEnabled = !isReadOnly,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // 设置枚举值
            var enumValues = Enum.GetValues(property.PropertyType);
            comboBox.ItemsSource = enumValues;

            // 绑定数据
            var binding = new Binding(property.Name)
            {
                Source = _selectedObject,
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);

            // 添加事件处理器
            comboBox.SelectionChanged += (s, e) => OnPropertyValueChanged(property);

            return comboBox;
        }

        /// <summary>
        /// 创建字符串编辑器
        /// </summary>
        private FrameworkElement CreateStringEditor(PropertyDescriptor property, bool isReadOnly)
        {
            var textBox = new TextBox
            {
                IsEnabled = !isReadOnly,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MaxLines = 3
            };

            // 绑定数据
            var binding = new Binding(property.Name)
            {
                Source = _selectedObject,
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBox.SetBinding(TextBox.TextProperty, binding);

            // 添加事件处理器
            textBox.TextChanged += (s, e) => OnPropertyValueChanged(property);

            return textBox;
        }

        /// <summary>
        /// 创建数值编辑器
        /// </summary>
        private FrameworkElement CreateNumericEditor(PropertyDescriptor property, bool isReadOnly)
        {
            var textBox = new TextBox
            {
                IsEnabled = !isReadOnly,
                HorizontalContentAlignment = HorizontalAlignment.Right
            };

            // 设置输入验证
            textBox.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidNumericInput(e.Text, property.PropertyType);
            };

            // 绑定数据
            var binding = new Binding(property.Name)
            {
                Source = _selectedObject,
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new NumericConverter(property.PropertyType)
            };
            textBox.SetBinding(TextBox.TextProperty, binding);

            // 添加事件处理器
            textBox.TextChanged += (s, e) => OnPropertyValueChanged(property);

            return textBox;
        }

        /// <summary>
        /// 创建颜色编辑器
        /// </summary>
        private FrameworkElement CreateColorEditor(PropertyDescriptor property, bool isReadOnly)
        {
            // 这里可以扩展颜色选择器，暂时使用文本框
            return CreateStringEditor(property, isReadOnly);
        }

        /// <summary>
        /// 创建默认编辑器
        /// </summary>
        private FrameworkElement CreateDefaultEditor(PropertyDescriptor property, bool isReadOnly)
        {
            var textBox = new TextBox
            {
                IsEnabled = !isReadOnly
            };

            // 绑定数据
            var binding = new Binding(property.Name)
            {
                Source = _selectedObject,
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new ObjectToStringConverter()
            };
            textBox.SetBinding(TextBox.TextProperty, binding);

            // 添加事件处理器
            textBox.TextChanged += (s, e) => OnPropertyValueChanged(property);

            return textBox;
        }

        /// <summary>
        /// 初始化属性值
        /// </summary>
        private void InitializePropertyValue(PropertyItem propertyItem)
        {
            if (_selectedObject != null && propertyItem.PropertyDescriptor != null)
            {
                try
                {
                    var value = propertyItem.PropertyDescriptor.GetValue(_selectedObject);
                    propertyItem.SetValue(value);
                }
                catch (Exception ex)
                {
                    // 忽略初始化错误
                    System.Diagnostics.Debug.WriteLine($"初始化属性值失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 判断是否为数值类型
        /// </summary>
        private bool IsNumericType(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) ||
                   type == typeof(short) || type == typeof(ushort) ||
                   type == typeof(int) || type == typeof(uint) ||
                   type == typeof(long) || type == typeof(ulong) ||
                   type == typeof(float) || type == typeof(double) ||
                   type == typeof(decimal);
        }

        /// <summary>
        /// 验证数值输入
        /// </summary>
        private bool IsValidNumericInput(string input, Type numericType)
        {
            if (string.IsNullOrEmpty(input)) return true;

            // 允许负号和小数点
            var allowedChars = "0123456789.-";
            if (input.Any(c => !allowedChars.Contains(c))) return false;

            // 检查小数点数量
            var decimalCount = input.Count(c => c == '.');
            if (decimalCount > 1) return false;

            // 检查负号位置
            var minusIndex = input.IndexOf('-');
            if (minusIndex > 0) return false;

            return true;
        }

        #region 事件处理方法

        /// <summary>
        /// 对象选择器变化事件
        /// </summary>
        private void CmbObjectSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbObjectSelector.SelectedItem != null)
            {
                SelectedObject = cmbObjectSelector.SelectedItem;
            }
        }

        /// <summary>
        /// 添加对象按钮点击事件
        /// </summary>
        private void BtnAddObject_Click(object sender, RoutedEventArgs e)
        {
            // 弹出添加对象对话框
            var dialog = new AddParameterPropertyDialog("参数特性");
            if (dialog.ShowDialog() == true && dialog.ParameterProperty != null)
            {
                _availableObjects.Add(dialog.ParameterProperty);
                RefreshObjectSelector();
                
                // 选择新添加的对象
                cmbObjectSelector.SelectedIndex = _availableObjects.Count - 1;
            }
        }

        /// <summary>
        /// 移除对象按钮点击事件
        /// </summary>
        private void BtnRemoveObject_Click(object sender, RoutedEventArgs e)
        {
            if (cmbObjectSelector.SelectedItem != null)
            {
                var selectedObject = cmbObjectSelector.SelectedItem;
                _availableObjects.Remove(selectedObject);
                
                RefreshObjectSelector();
                
                if (_availableObjects.Count > 0)
                {
                    cmbObjectSelector.SelectedIndex = 0;
                }
                else
                {
                    SelectedObject = null;
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 属性值变化处理
        /// </summary>
        private void OnPropertyValueChanged(PropertyDescriptor property)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            try
            {
                // 触发属性值变化事件
                PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs
                {
                    PropertyName = property.Name,
                    NewValue = property.GetValue(_selectedObject),
                    Object = _selectedObject
                });
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// 选择变化处理
        /// </summary>
        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 公共方法和事件

        /// <summary>
        /// 设置选中的对象
        /// </summary>
        public void SetSelectedObject(object obj)
        {
            SelectedObject = obj;
        }

        /// <summary>
        /// 刷新属性显示
        /// </summary>
        public void RefreshProperties()
        {
            RefreshPropertyGrid();
        }

        /// <summary>
        /// 折叠所有类别
        /// </summary>
        public void CollapseAllCategories()
        {
            // 实现折叠功能
        }

        /// <summary>
        /// 展开所有类别
        /// </summary>
        public void ExpandAllCategories()
        {
            // 实现展开功能
        }

        /// <summary>
        /// 搜索属性
        /// </summary>
        public void SearchProperties(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                // 显示所有属性
                foreach (var item in _propertyItems.Values)
                {
                    item.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // 过滤属性
                var searchLower = searchText.ToLower();
                foreach (var item in _propertyItems.Values)
                {
                    var isVisible = item.PropertyDescriptor.DisplayName.ToLower().Contains(searchLower) ||
                                  item.PropertyDescriptor.Description.ToLower().Contains(searchLower);
                    item.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 属性值变化事件
        /// </summary>
        public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

        /// <summary>
        /// 选择变化事件
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion
    }

    /// <summary>
    /// 属性项
    /// </summary>
    internal class PropertyItem
    {
        public PropertyDescriptor PropertyDescriptor { get; set; }
        public TextBlock NameControl { get; set; }
        public FrameworkElement ValueControl { get; set; }
        public PropertyGridControl PropertyGrid { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Visible;

        public void SetValue(object value)
        {
            if (ValueControl is CheckBox checkBox && value is bool boolValue)
            {
                checkBox.IsChecked = boolValue;
            }
            else if (ValueControl is TextBox textBox)
            {
                textBox.Text = value?.ToString() ?? "";
            }
        }
    }

    /// <summary>
    /// 属性值变化事件参数
    /// </summary>
    public class PropertyValueChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; }
        public object NewValue { get; set; }
        public object Object { get; set; }
    }

    /// <summary>
    /// 数值转换器
    /// </summary>
    internal class NumericConverter : IValueConverter
    {
        private readonly Type _targetType;

        public NumericConverter(Type targetType)
        {
            _targetType = targetType;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                try
                {
                    return System.Convert.ChangeType(stringValue, _targetType);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 对象到字符串转换器
    /// </summary>
    internal class ObjectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}