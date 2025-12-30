using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Win32;
using ZWDynLookup.Models;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 特性查寻表主对话框
    /// </summary>
    public partial class LookupTableDialog : Window
    {
        private List<ParameterProperty> _inputProperties;
        private List<ParameterProperty> _lookupProperties;
        private List<ParameterProperty> _parameterProperties;
        private ParameterProperty _selectedInputProperty;
        private ParameterProperty _selectedLookupProperty;
        private bool _hasChanges;

        /// <summary>
        /// 输入特性列表
        /// </summary>
        public List<ParameterProperty> InputProperties
        {
            get => _inputProperties;
            set
            {
                _inputProperties = value ?? new List<ParameterProperty>();
                RefreshInputPropertiesGrid();
            }
        }

        /// <summary>
        /// 查寻特性列表
        /// </summary>
        public List<ParameterProperty> LookupProperties
        {
            get => _lookupProperties;
            set
            {
                _lookupProperties = value ?? new List<ParameterProperty>();
                RefreshLookupPropertiesGrid();
            }
        }

        /// <summary>
        /// 参数特性列表
        /// </summary>
        public List<ParameterProperty> ParameterProperties
        {
            get => _parameterProperties;
            set
            {
                _parameterProperties = value ?? new List<ParameterProperty>();
                RefreshParameterPropertiesGrid();
            }
        }

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        public bool HasChanges => _hasChanges;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LookupTableDialog()
        {
            InitializeComponent();
            InitializeData();
            InitializeEventHandlers();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inputProperties">输入特性列表</param>
        /// <param name="lookupProperties">查寻特性列表</param>
        /// <param name="parameterProperties">参数特性列表</param>
        public LookupTableDialog(List<ParameterProperty> inputProperties, 
                                List<ParameterProperty> lookupProperties,
                                List<ParameterProperty> parameterProperties) : this()
        {
            InputProperties = inputProperties;
            LookupProperties = lookupProperties;
            ParameterProperties = parameterProperties;
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            _inputProperties = new List<ParameterProperty>();
            _lookupProperties = new List<ParameterProperty>();
            _parameterProperties = new List<ParameterProperty>();
            _hasChanges = false;

            // 初始化示例数据
            InitializeSampleData();
        }

        /// <summary>
        /// 初始化示例数据
        /// </summary>
        private void InitializeSampleData()
        {
            // 输入特性示例
            InputProperties = new List<ParameterProperty>
            {
                new ParameterProperty { PropertyName = "Length", DataType = "double", DefaultValue = "0.0", Description = "长度参数" },
                new ParameterProperty { PropertyName = "Width", DataType = "double", DefaultValue = "0.0", Description = "宽度参数" },
                new ParameterProperty { PropertyName = "Material", DataType = "string", DefaultValue = "Steel", Description = "材料类型" }
            };

            // 查寻特性示例
            LookupProperties = new List<ParameterProperty>
            {
                new ParameterProperty { PropertyName = "Cost", DataType = "double", DefaultValue = "0.0", Description = "成本计算" },
                new ParameterProperty { PropertyName = "Weight", DataType = "double", DefaultValue = "0.0", Description = "重量计算" },
                new ParameterProperty { PropertyName = "Price", DataType = "decimal", DefaultValue = "0.00", Description = "价格计算" }
            };

            // 参数特性示例
            ParameterProperties = new List<ParameterProperty>
            {
                new ParameterProperty { PropertyName = "CalculationFormula", DataType = "string", DefaultValue = "", Description = "计算公式" },
                new ParameterProperty { PropertyName = "DecimalPlaces", DataType = "int", DefaultValue = "2", Description = "小数位数" },
                new ParameterProperty { PropertyName = "Unit", DataType = "string", DefaultValue = "mm", Description = "单位" }
            };
        }

        /// <summary>
        /// 初始化事件处理器
        /// </summary>
        private void InitializeEventHandlers()
        {
            // 数据网格选择变化事件
            dgInputProperties.SelectionChanged += DgInputProperties_SelectionChanged;
            dgLookupProperties.SelectionChanged += DgLookupProperties_SelectionChanged;

            // 属性网格变化事件
            propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
        }

        #region 事件处理方法

        /// <summary>
        /// 添加输入特性按钮点击事件
        /// </summary>
        private void BtnAddInputProperty_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddParameterPropertyDialog("输入特性");
            if (dialog.ShowDialog() == true && dialog.ParameterProperty != null)
            {
                InputProperties.Add(dialog.ParameterProperty);
                RefreshInputPropertiesGrid();
                MarkAsChanged();
            }
        }

        /// <summary>
        /// 添加查寻特性按钮点击事件
        /// </summary>
        private void BtnAddLookupProperty_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddParameterPropertyDialog("查寻特性");
            if (dialog.ShowDialog() == true && dialog.ParameterProperty != null)
            {
                LookupProperties.Add(dialog.ParameterProperty);
                RefreshLookupPropertiesGrid();
                MarkAsChanged();
            }
        }

        /// <summary>
        /// 编辑参数按钮点击事件
        /// </summary>
        private void BtnEditParameter_Click(object sender, RoutedEventArgs e)
        {
            if (propertyGrid.SelectedObject != null)
            {
                // 编辑当前选中的参数属性
                var property = propertyGrid.SelectedObject as ParameterProperty;
                if (property != null)
                {
                    var dialog = new AddParameterPropertyDialog("编辑特性", property);
                    if (dialog.ShowDialog() == true)
                    {
                        // 更新属性值
                        property.PropertyName = dialog.ParameterProperty.PropertyName;
                        property.DataType = dialog.ParameterProperty.DataType;
                        property.DefaultValue = dialog.ParameterProperty.DefaultValue;
                        property.Description = dialog.ParameterProperty.Description;
                        
                        RefreshParameterPropertiesGrid();
                        MarkAsChanged();
                    }
                }
            }
        }

        /// <summary>
        /// 删除参数按钮点击事件
        /// </summary>
        private void BtnDeleteParameter_Click(object sender, RoutedEventArgs e)
        {
            if (propertyGrid.SelectedObject != null)
            {
                var result = MessageBox.Show("确定要删除选中的参数特性吗？", "确认删除", 
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var property = propertyGrid.SelectedObject as ParameterProperty;
                    if (property != null)
                    {
                        ParameterProperties.Remove(property);
                        RefreshParameterPropertiesGrid();
                        MarkAsChanged();
                    }
                }
            }
        }

        /// <summary>
        /// 导出配置按钮点击事件
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON文件 (*.json)|*.json|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"查寻表配置_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExportConfiguration(dialog.FileName);
                    MessageBox.Show("配置导出成功！", "导出完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败：{ex.Message}", "导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 导入配置按钮点击事件
        /// </summary>
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON文件 (*.json)|*.json|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ImportConfiguration(dialog.FileName);
                    RefreshAllGrids();
                    MarkAsChanged();
                    MessageBox.Show("配置导入成功！", "导入完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败：{ex.Message}", "导入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateConfiguration())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_hasChanges)
            {
                var result = MessageBox.Show("有未保存的更改，确定要退出吗？", "确认退出", 
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 帮助按钮点击事件
        /// </summary>
        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("查寻表配置帮助：\n\n" +
                           "1. 添加输入特性：定义需要查寻的输入参数\n" +
                           "2. 添加查寻特性：定义查寻结果的输出参数\n" +
                           "3. 编辑参数：配置计算参数和规则\n" +
                           "4. 导出/导入：保存或加载配置", 
                           "帮助", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 输入特性过滤文本变化事件
        /// </summary>
        private void TxtInputFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FilterInputProperties();
        }

        /// <summary>
        /// 查寻特性过滤文本变化事件
        /// </summary>
        private void TxtLookupFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FilterLookupProperties();
        }

        /// <summary>
        /// 清除输入特性过滤事件
        /// </summary>
        private void BtnClearInputFilter_Click(object sender, RoutedEventArgs e)
        {
            txtInputFilter.Clear();
            FilterInputProperties();
        }

        /// <summary>
        /// 清除查寻特性过滤事件
        /// </summary>
        private void BtnClearLookupFilter_Click(object sender, RoutedEventArgs e)
        {
            txtLookupFilter.Clear();
            FilterLookupProperties();
        }

        /// <summary>
        /// 输入特性网格选择变化事件
        /// </summary>
        private void DgInputProperties_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgInputProperties.SelectedItem is ParameterProperty property)
            {
                _selectedInputProperty = property;
            }
        }

        /// <summary>
        /// 查寻特性网格选择变化事件
        /// </summary>
        private void DgLookupProperties_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgLookupProperties.SelectedItem is ParameterProperty property)
            {
                _selectedLookupProperty = property;
            }
        }

        /// <summary>
        /// 属性网格值变化事件
        /// </summary>
        private void PropertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            MarkAsChanged();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 刷新输入特性网格
        /// </summary>
        private void RefreshInputPropertiesGrid()
        {
            dgInputProperties.ItemsSource = null;
            dgInputProperties.ItemsSource = _inputProperties;
            FilterInputProperties();
        }

        /// <summary>
        /// 刷新查寻特性网格
        /// </summary>
        private void RefreshLookupPropertiesGrid()
        {
            dgLookupProperties.ItemsSource = null;
            dgLookupProperties.ItemsSource = _lookupProperties;
            FilterLookupProperties();
        }

        /// <summary>
        /// 刷新参数特性网格
        /// </summary>
        private void RefreshParameterPropertiesGrid()
        {
            propertyGrid.SelectedObject = null;
            if (_parameterProperties.Count > 0)
            {
                propertyGrid.SelectedObject = _parameterProperties[0];
            }
        }

        /// <summary>
        /// 过滤输入特性
        /// </summary>
        private void FilterInputProperties()
        {
            var filter = txtInputFilter.Text.ToLower();
            var filteredProperties = string.IsNullOrEmpty(filter) 
                ? _inputProperties 
                : _inputProperties.Where(p => p.PropertyName.ToLower().Contains(filter)).ToList();
            
            dgInputProperties.ItemsSource = filteredProperties;
        }

        /// <summary>
        /// 过滤查寻特性
        /// </summary>
        private void FilterLookupProperties()
        {
            var filter = txtLookupFilter.Text.ToLower();
            var filteredProperties = string.IsNullOrEmpty(filter) 
                ? _lookupProperties 
                : _lookupProperties.Where(p => p.PropertyName.ToLower().Contains(filter)).ToList();
            
            dgLookupProperties.ItemsSource = filteredProperties;
        }

        /// <summary>
        /// 刷新所有网格
        /// </summary>
        private void RefreshAllGrids()
        {
            RefreshInputPropertiesGrid();
            RefreshLookupPropertiesGrid();
            RefreshParameterPropertiesGrid();
        }

        /// <summary>
        /// 标记为已更改
        /// </summary>
        private void MarkAsChanged()
        {
            _hasChanges = true;
            this.Title = "*特性查寻表配置";
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        private bool ValidateConfiguration()
        {
            if (_inputProperties.Count == 0)
            {
                MessageBox.Show("请至少添加一个输入特性！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_lookupProperties.Count == 0)
            {
                MessageBox.Show("请至少添加一个查寻特性！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 检查特性名称是否重复
            var allProperties = _inputProperties.Concat(_lookupProperties).Concat(_parameterProperties);
            var duplicateNames = allProperties.GroupBy(p => p.PropertyName).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateNames.Count > 0)
            {
                MessageBox.Show($"发现重复的特性名称：{string.Join(", ", duplicateNames)}", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        private void ExportConfiguration(string filePath)
        {
            var config = new
            {
                ExportTime = DateTime.Now,
                InputProperties = _inputProperties,
                LookupProperties = _lookupProperties,
                ParameterProperties = _parameterProperties
            };

            var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        private void ImportConfiguration(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (config.ContainsKey("InputProperties"))
            {
                _inputProperties = System.Text.Json.JsonSerializer.Deserialize<List<ParameterProperty>>(
                    config["InputProperties"].ToString()) ?? new List<ParameterProperty>();
            }
            
            if (config.ContainsKey("LookupProperties"))
            {
                _lookupProperties = System.Text.Json.JsonSerializer.Deserialize<List<ParameterProperty>>(
                    config["LookupProperties"].ToString()) ?? new List<ParameterProperty>();
            }
            
            if (config.ContainsKey("ParameterProperties"))
            {
                _parameterProperties = System.Text.Json.JsonSerializer.Deserialize<List<ParameterProperty>>(
                    config["ParameterProperties"].ToString()) ?? new List<ParameterProperty>();
            }
        }

        #endregion
    }
}