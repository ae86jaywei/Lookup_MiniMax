using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using ZWDynLookup.Models;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 查寻表编辑器
    /// </summary>
    public partial class LookupTableEditor : UserControl, INotifyPropertyChanged
    {
        private DataTable _lookupTable;
        private string _tableName = "默认查寻表";
        private List<ParameterProperty> _inputProperties;
        private List<ParameterProperty> _lookupProperties;
        private string _filterText = "";
        private string _statusText = "就绪";
        private string _positionText = "";
        private int _totalRows = 0;
        private int _validRows = 0;
        private int _duplicateRows = 0;
        private bool _hasChanges = false;

        /// <summary>
        /// 查寻表数据
        /// </summary>
        public DataTable LookupTable
        {
            get => _lookupTable;
            set
            {
                _lookupTable = value;
                OnPropertyChanged(nameof(LookupTable));
                RefreshDataGrid();
                UpdateStatistics();
            }
        }

        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                OnPropertyChanged(nameof(TableName));
                txtTableName.Text = value;
            }
        }

        /// <summary>
        /// 输入特性列表
        /// </summary>
        public List<ParameterProperty> InputProperties
        {
            get => _inputProperties;
            set
            {
                _inputProperties = value ?? new List<ParameterProperty>();
                GenerateColumns();
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
                GenerateColumns();
            }
        }

        /// <summary>
        /// 过滤文本
        /// </summary>
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
                ApplyFilter();
            }
        }

        /// <summary>
        /// 状态文本
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
                txtStatus.Text = value;
            }
        }

        /// <summary>
        /// 位置文本
        /// </summary>
        public string PositionText
        {
            get => _positionText;
            set
            {
                _positionText = value;
                OnPropertyChanged(nameof(PositionText));
                txtPosition.Text = value;
            }
        }

        /// <summary>
        /// 总行数
        /// </summary>
        public int TotalRows
        {
            get => _totalRows;
            set
            {
                _totalRows = value;
                OnPropertyChanged(nameof(TotalRows));
                txtTotalRows.Text = value.ToString();
            }
        }

        /// <summary>
        /// 有效行数
        /// </summary>
        public int ValidRows
        {
            get => _validRows;
            set
            {
                _validRows = value;
                OnPropertyChanged(nameof(ValidRows));
                txtValidRows.Text = value.ToString();
            }
        }

        /// <summary>
        /// 重复行数
        /// </summary>
        public int DuplicateRows
        {
            get => _duplicateRows;
            set
            {
                _duplicateRows = value;
                OnPropertyChanged(nameof(DuplicateRows));
                txtDuplicateRows.Text = value.ToString();
            }
        }

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataChangedEventArgs> DataChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LookupTableEditor()
        {
            InitializeComponent();
            InitializeData();
            InitializeEventHandlers();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            // 创建默认数据表
            LookupTable = new DataTable("LookupTable");
            
            // 添加默认列
            AddDefaultColumns();
            
            // 添加示例数据
            AddSampleData();
            
            // 设置默认查寻规则
            cmbMatchType.SelectedIndex = 0; // 精确匹配
            cmbPriority.SelectedIndex = 0;  // 优先级1
        }

        /// <summary>
        /// 添加默认列
        /// </summary>
        private void AddDefaultColumns()
        {
            // ID列
            var idColumn = new DataColumn("ID", typeof(int))
            {
                AutoIncrement = true,
                AutoIncrementSeed = 1,
                Unique = true
            };
            LookupTable.Columns.Add(idColumn);

            // 输入列
            if (_inputProperties == null)
            {
                _inputProperties = new List<ParameterProperty>
                {
                    new ParameterProperty { PropertyName = "InputValue", DataType = "string" }
                };
            }

            foreach (var property in _inputProperties)
            {
                var columnType = GetTypeFromString(property.DataType);
                var column = new DataColumn(property.PropertyName, columnType);
                if (!string.IsNullOrEmpty(property.DefaultValue))
                {
                    try
                    {
                        column.DefaultValue = Convert.ChangeType(property.DefaultValue, columnType);
                    }
                    catch
                    {
                        column.DefaultValue = GetDefaultValue(columnType);
                    }
                }
                LookupTable.Columns.Add(column);
            }

            // 查寻结果列
            if (_lookupProperties == null)
            {
                _lookupProperties = new List<ParameterProperty>
                {
                    new ParameterProperty { PropertyName = "ResultValue", DataType = "string" }
                };
            }

            foreach (var property in _lookupProperties)
            {
                var columnType = GetTypeFromString(property.DataType);
                var column = new DataColumn(property.PropertyName, columnType);
                if (!string.IsNullOrEmpty(property.DefaultValue))
                {
                    try
                    {
                        column.DefaultValue = Convert.ChangeType(property.DefaultValue, columnType);
                    }
                    catch
                    {
                        column.DefaultValue = GetDefaultValue(columnType);
                    }
                }
                LookupTable.Columns.Add(column);
            }

            // 备注列
            LookupTable.Columns.Add("Remarks", typeof(string));
        }

        /// <summary>
        /// 添加示例数据
        /// </summary>
        private void AddSampleData()
        {
            var sampleData = new[]
            {
                new { InputValue = "A", ResultValue = "100", Remarks = "A级结果" },
                new { InputValue = "B", ResultValue = "200", Remarks = "B级结果" },
                new { InputValue = "C", ResultValue = "300", Remarks = "C级结果" },
                new { InputValue = "D", ResultValue = "400", Remarks = "D级结果" }
            };

            foreach (var data in sampleData)
            {
                var row = LookupTable.NewRow();
                row["InputValue"] = data.InputValue;
                row["ResultValue"] = data.ResultValue;
                row["Remarks"] = data.Remarks;
                LookupTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// 初始化事件处理器
        /// </summary>
        private void InitializeEventHandlers()
        {
            // 键盘快捷键
            this.KeyDown += LookupTableEditor_KeyDown;
            
            // 窗口关闭事件
            this.Unloaded += LookupTableEditor_Unloaded;
        }

        /// <summary>
        /// 生成列
        /// </summary>
        private void GenerateColumns()
        {
            if (LookupTable == null) return;

            // 清除现有列（保留ID列）
            var columnsToRemove = LookupTable.Columns.Cast<DataColumn>()
                .Where(c => c.ColumnName != "ID")
                .ToList();
            
            foreach (var column in columnsToRemove)
            {
                LookupTable.Columns.Remove(column);
            }

            // 重新添加列
            AddDefaultColumns();
            RefreshDataGrid();
        }

        /// <summary>
        /// 刷新数据网格
        /// </summary>
        private void RefreshDataGrid()
        {
            if (LookupTable == null) return;

            dgLookupTable.ItemsSource = null;
            dgLookupTable.Columns.Clear();

            foreach (DataColumn column in LookupTable.Columns)
            {
                var dataGridColumn = CreateDataGridColumn(column);
                dgLookupTable.Columns.Add(dataGridColumn);
            }

            // 设置数据源
            var collectionView = new BindingListCollectionView(LookupTable.DefaultView);
            dgLookupTable.ItemsSource = collectionView;
        }

        /// <summary>
        /// 创建数据网格列
        /// </summary>
        private DataGridColumn CreateDataGridColumn(DataColumn column)
        {
            var columnType = column.DataType;
            
            if (columnType == typeof(bool))
            {
                return new DataGridCheckBoxColumn
                {
                    Header = column.ColumnName,
                    Binding = new System.Windows.Data.Binding(column.ColumnName),
                    IsReadOnly = column.ColumnName == "ID"
                };
            }
            else
            {
                var textColumn = new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    Binding = new System.Windows.Data.Binding(column.ColumnName),
                    IsReadOnly = column.ColumnName == "ID"
                };

                // 设置列宽
                if (column.ColumnName == "ID")
                {
                    textColumn.Width = 50;
                }
                else if (column.ColumnName == "Remarks")
                {
                    textColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                else
                {
                    textColumn.Width = 120;
                }

                return textColumn;
            }
        }

        /// <summary>
        /// 应用过滤器
        /// </summary>
        private void ApplyFilter()
        {
            if (LookupTable == null) return;

            if (string.IsNullOrEmpty(_filterText))
            {
                LookupTable.DefaultView.RowFilter = "";
            }
            else
            {
                var filter = BuildFilterExpression(_filterText);
                LookupTable.DefaultView.RowFilter = filter;
            }
            
            UpdateStatistics();
        }

        /// <summary>
        /// 构建过滤表达式
        /// </summary>
        private string BuildFilterExpression(string filterText)
        {
            var expressions = new List<string>();
            var columns = LookupTable.Columns.Cast<DataColumn>();

            foreach (var column in columns)
            {
                if (column.DataType == typeof(string))
                {
                    expressions.Add($"[{column.ColumnName}] LIKE '%{filterText.Replace("'", "''")}%'");
                }
                else if (IsNumericType(column.DataType))
                {
                    if (double.TryParse(filterText, out var numericValue))
                    {
                        expressions.Add($"[{column.ColumnName}] = {numericValue}");
                    }
                }
            }

            return string.Join(" OR ", expressions);
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics()
        {
            if (LookupTable == null) return;

            var totalView = new DataView(LookupTable);
            var filteredView = new DataView(LookupTable);
            
            TotalRows = totalView.Count;
            
            if (!string.IsNullOrEmpty(_filterText))
            {
                filteredView.RowFilter = BuildFilterExpression(_filterText);
                ValidRows = filteredView.Count;
            }
            else
            {
                ValidRows = TotalRows;
            }

            // 计算重复行数
            DuplicateRows = CalculateDuplicateRows();
        }

        /// <summary>
        /// 计算重复行数
        /// </summary>
        private int CalculateDuplicateRows()
        {
            if (LookupTable == null) return 0;

            var groupedRows = LookupTable.AsEnumerable()
                .GroupBy(row => string.Join("|", LookupTable.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName != "ID")
                    .Select(c => row[c]?.ToString() ?? "")))
                .Where(g => g.Count() > 1);

            return groupedRows.Sum(g => g.Count() - 1);
        }

        #region 事件处理方法

        /// <summary>
        /// 添加行按钮点击事件
        /// </summary>
        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            if (LookupTable == null) return;

            var newRow = LookupTable.NewRow();
            
            // 设置默认值
            foreach (DataColumn column in LookupTable.Columns)
            {
                if (column.ColumnName != "ID" && column.DefaultValue != DBNull.Value)
                {
                    newRow[column.ColumnName] = column.DefaultValue;
                }
            }

            LookupTable.Rows.Add(newRow);
            HasChanges = true;
            UpdateStatistics();
            
            // 选中新行
            dgLookupTable.SelectedIndex = dgLookupTable.Items.Count - 1;
            dgLookupTable.ScrollIntoView(dgLookupTable.SelectedItem);
            
            StatusText = "已添加新行";
            DataChanged?.Invoke(this, new DataChangedEventArgs("RowAdded", newRow));
        }

        /// <summary>
        /// 删除行按钮点击事件
        /// </summary>
        private void BtnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (dgLookupTable.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的行", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("确定要删除选中的行吗？", "确认删除", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var selectedRows = dgLookupTable.SelectedItems.Cast<DataRowView>().ToList();
                
                foreach (var rowView in selectedRows)
                {
                    rowView.Row.Delete();
                }
                
                HasChanges = true;
                UpdateStatistics();
                
                StatusText = $"已删除 {selectedRows.Count} 行";
                DataChanged?.Invoke(this, new DataChangedEventArgs("RowsDeleted", selectedRows.Count));
            }
        }

        /// <summary>
        /// 清空所有按钮点击事件
        /// </summary>
        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要清空所有数据吗？此操作不可恢复！", "确认清空", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                LookupTable.Clear();
                HasChanges = true;
                UpdateStatistics();
                
                StatusText = "已清空所有数据";
                DataChanged?.Invoke(this, new DataChangedEventArgs("AllCleared", null));
            }
        }

        /// <summary>
        /// 导入数据按钮点击事件
        /// </summary>
        private void BtnImportData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV文件 (*.csv)|*.csv|Excel文件 (*.xlsx;*.xls)|*.xlsx;*.xls|所有文件 (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ImportDataFromFile(dialog.FileName);
                    HasChanges = true;
                    UpdateStatistics();
                    StatusText = "数据导入成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败：{ex.Message}", "导入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 导出数据按钮点击事件
        /// </summary>
        private void BtnExportData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV文件 (*.csv)|*.csv|Excel文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"{TableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExportDataToFile(dialog.FileName);
                    StatusText = "数据导出成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败：{ex.Message}", "导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 验证数据按钮点击事件
        /// </summary>
        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            ValidateData();
        }

        /// <summary>
        /// 优化查寻按钮点击事件
        /// </summary>
        private void BtnOptimize_Click(object sender, RoutedEventArgs e)
        {
            OptimizeLookup();
        }

        /// <summary>
        /// 过滤文本变化事件
        /// </summary>
        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterText = txtFilter.Text;
        }

        /// <summary>
        /// 清除过滤器按钮点击事件
        /// </summary>
        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtFilter.Clear();
        }

        /// <summary>
        /// 数据网格加载行事件
        /// </summary>
        private void DgLookupTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row.DataContext as DataRowView;
            if (row != null)
            {
                // 设置行号
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
                
                // 设置行背景色（奇偶行）
                if (e.Row.GetIndex() % 2 == 0)
                {
                    e.Row.Background = Brushes.White;
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                }
            }
        }

        /// <summary>
        /// 数据网格单元格编辑结束事件
        /// </summary>
        private void DgLookupTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            HasChanges = true;
            StatusText = "数据已修改";
            
            var editedRow = e.Row.DataContext as DataRowView;
            DataChanged?.Invoke(this, new DataChangedEventArgs("CellEdited", editedRow));
        }

        /// <summary>
        /// 数据网格选择变化事件
        /// </summary>
        private void DgLookupTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLookupTable.SelectedItems.Count > 0)
            {
                var selectedRow = dgLookupTable.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    var rowIndex = LookupTable.Rows.IndexOf(selectedRow.Row);
                    PositionText = $"行 {rowIndex + 1}，列 {dgLookupTable.CurrentColumn?.DisplayIndex + 1}";
                }
            }
            else
            {
                PositionText = "";
            }
        }

        /// <summary>
        /// 测试输入文本变化事件
        /// </summary>
        private void TxtTestInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 自动测试
            TestLookup();
        }

        /// <summary>
        /// 测试查寻按钮点击事件
        /// </summary>
        private void BtnTestLookup_Click(object sender, RoutedEventArgs e)
        {
            TestLookup();
        }

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        private void LookupTableEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        BtnAddRow_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Key.D:
                        BtnDeleteRow_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Key.S:
                        // 保存功能
                        e.Handled = true;
                        break;
                    case Key.F:
                        txtFilter.Focus();
                        txtFilter.SelectAll();
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.F5)
            {
                RefreshDataGrid();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 卸载事件
        /// </summary>
        private void LookupTableEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (HasChanges)
            {
                var result = MessageBox.Show("有未保存的更改，是否保存？", "确认保存", 
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // 这里可以触发保存事件
                    DataChanged?.Invoke(this, new DataChangedEventArgs("SaveRequested", null));
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从字符串获取类型
        /// </summary>
        private Type GetTypeFromString(string typeString)
        {
            return typeString?.ToLower() switch
            {
                "string" => typeof(string),
                "int" => typeof(int),
                "double" => typeof(double),
                "decimal" => typeof(decimal),
                "float" => typeof(float),
                "bool" => typeof(bool),
                "datetime" => typeof(DateTime),
                "guid" => typeof(Guid),
                _ => typeof(string)
            };
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// 判断是否为数值类型
        /// </summary>
        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(double) || 
                   type == typeof(decimal) || type == typeof(float);
        }

        /// <summary>
        /// 从文件导入数据
        /// </summary>
        private void ImportDataFromFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            
            switch (extension)
            {
                case ".csv":
                    ImportFromCsv(filePath);
                    break;
                case ".xlsx":
                case ".xls":
                    ImportFromExcel(filePath);
                    break;
                default:
                    throw new NotSupportedException("不支持的文件格式");
            }
        }

        /// <summary>
        /// 从CSV导入数据
        /// </summary>
        private void ImportFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (lines.Length == 0) return;

            // 解析CSV头部
            var headers = ParseCsvLine(lines[0]);
            
            // 检查列是否匹配
            if (!ValidateColumns(headers))
            {
                throw new Exception("CSV文件的列结构与当前查寻表不匹配");
            }

            // 导入数据
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                
                var values = ParseCsvLine(lines[i]);
                var newRow = LookupTable.NewRow();
                
                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    if (headers[j] != "ID") // 跳过ID列
                    {
                        var column = LookupTable.Columns[headers[j]];
                        if (column != null && !string.IsNullOrEmpty(values[j]))
                        {
                            try
                            {
                                newRow[headers[j]] = Convert.ChangeType(values[j], column.DataType);
                            }
                            catch
                            {
                                newRow[headers[j]] = values[j];
                            }
                        }
                    }
                }
                
                LookupTable.Rows.Add(newRow);
            }
        }

        /// <summary>
        /// 解析CSV行
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            // 简单的CSV解析，实际项目中可能需要更复杂的处理
            return line.Split(',').Select(s => s.Trim().Trim('"')).ToArray();
        }

        /// <summary>
        /// 验证列
        /// </summary>
        private bool ValidateColumns(string[] headers)
        {
            var tableColumns = LookupTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            
            foreach (var header in headers)
            {
                if (header != "ID" && !tableColumns.Contains(header))
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 从Excel导入数据
        /// </summary>
        private void ImportFromExcel(string filePath)
        {
            // 这里需要添加Excel处理逻辑，暂时抛出异常
            throw new NotImplementedException("Excel导入功能尚未实现");
        }

        /// <summary>
        /// 导出数据到文件
        /// </summary>
        private void ExportDataToFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            
            switch (extension)
            {
                case ".csv":
                    ExportToCsv(filePath);
                    break;
                case ".xlsx":
                    ExportToExcel(filePath);
                    break;
                default:
                    throw new NotSupportedException("不支持的文件格式");
            }
        }

        /// <summary>
        /// 导出到CSV
        /// </summary>
        private void ExportToCsv(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 写入头部
                var headers = LookupTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                writer.WriteLine(string.Join(",", headers));
                
                // 写入数据
                foreach (DataRow row in LookupTable.Rows)
                {
                    var values = LookupTable.Columns.Cast<DataColumn>()
                        .Select(c => FormatCsvValue(row[c]));
                    writer.WriteLine(string.Join(",", values));
                }
            }
        }

        /// <summary>
        /// 格式化CSV值
        /// </summary>
        private string FormatCsvValue(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            var str = value.ToString();
            if (str.Contains(",") || str.Contains("\"") || str.Contains("\n"))
            {
                return $"\"{str.Replace("\"", "\"\"")}\"";
            }
            return str;
        }

        /// <summary>
        /// 导出到Excel
        /// </summary>
        private void ExportToExcel(string filePath)
        {
            // 这里需要添加Excel处理逻辑，暂时抛出异常
            throw new NotImplementedException("Excel导出功能尚未实现");
        }

        /// <summary>
        /// 验证数据
        /// </summary>
        private void ValidateData()
        {
            var errors = new List<string>();
            
            foreach (DataRow row in LookupTable.Rows)
            {
                var rowNumber = LookupTable.Rows.IndexOf(row) + 1;
                
                // 检查必填字段
                foreach (DataColumn column in LookupTable.Columns)
                {
                    if (column.ColumnName != "ID" && 
                        _inputProperties.Concat(_lookupProperties).Any(p => p.PropertyName == column.ColumnName && p.IsRequired))
                    {
                        if (row[column] == DBNull.Value || string.IsNullOrEmpty(row[column].ToString()))
                        {
                            errors.Add($"第{rowNumber}行：{column.ColumnName}为必填字段");
                        }
                    }
                }
                
                // 检查数据格式
                foreach (DataColumn column in LookupTable.Columns)
                {
                    if (column.ColumnName != "ID" && row[column] != DBNull.Value)
                    {
                        try
                        {
                            Convert.ChangeType(row[column], column.DataType);
                        }
                        catch
                        {
                            errors.Add($"第{rowNumber}行：{column.ColumnName}数据格式不正确");
                        }
                    }
                }
            }
            
            // 检查重复行
            var duplicates = FindDuplicateRows();
            foreach (var duplicate in duplicates)
            {
                errors.Add($"发现重复行：{string.Join(", ", duplicate.Select(r => $"第{r}行"))}");
            }
            
            // 显示结果
            if (errors.Count == 0)
            {
                txtErrorInfo.Text = "验证通过！未发现数据错误。";
                StatusText = "数据验证通过";
            }
            else
            {
                txtErrorInfo.Text = string.Join("\n", errors);
                StatusText = $"发现 {errors.Count} 个错误";
            }
        }

        /// <summary>
        /// 查找重复行
        /// </summary>
        private List<List<int>> FindDuplicateRows()
        {
            var duplicates = new List<List<int>>();
            var groupedRows = LookupTable.AsEnumerable()
                .Select((row, index) => new { Row = row, Index = index + 1 })
                .GroupBy(x => string.Join("|", LookupTable.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName != "ID")
                    .Select(c => x.Row[c]?.ToString() ?? "")))
                .Where(g => g.Count() > 1);

            foreach (var group in groupedRows)
            {
                duplicates.Add(group.Select(x => x.Index).ToList());
            }
            
            return duplicates;
        }

        /// <summary>
        /// 优化查寻
        /// </summary>
        private void OptimizeLookup()
        {
            // 移除重复行
            var duplicates = FindDuplicateRows();
            var rowsToDelete = new List<DataRow>();
            
            foreach (var duplicateGroup in duplicates)
            {
                // 保留第一行，删除其他行
                for (int i = 1; i < duplicateGroup.Count; i++)
                {
                    var rowIndex = duplicateGroup[i] - 1; // 转换为0基索引
                    if (rowIndex >= 0 && rowIndex < LookupTable.Rows.Count)
                    {
                        rowsToDelete.Add(LookupTable.Rows[rowIndex]);
                    }
                }
            }
            
            foreach (var row in rowsToDelete)
            {
                row.Delete();
            }
            
            // 重新排序（按优先级）
            LookupTable.DefaultView.Sort = "Priority ASC, ID ASC";
            
            HasChanges = true;
            UpdateStatistics();
            
            StatusText = $"优化完成，删除了 {rowsToDelete.Count} 行重复数据";
        }

        /// <summary>
        /// 测试查寻
        /// </summary>
        private void TestLookup()
        {
            var testInput = txtTestInput.Text?.Trim();
            if (string.IsNullOrEmpty(testInput))
            {
                txtTestResult.Text = "请输入测试值";
                return;
            }
            
            var result = PerformLookup(testInput);
            txtTestResult.Text = result;
        }

        /// <summary>
        /// 执行查寻
        /// </summary>
        private string PerformLookup(string inputValue)
        {
            var matchType = (cmbMatchType.SelectedItem as ComboBoxItem)?.Tag.ToString();
            var defaultResult = txtDefaultResult.Text?.Trim();
            
            foreach (DataRow row in LookupTable.Rows)
            {
                if (IsMatch(row, inputValue, matchType))
                {
                    // 返回第一个匹配的查寻结果
                    foreach (DataColumn column in LookupTable.Columns)
                    {
                        if (_lookupProperties.Any(p => p.PropertyName == column.ColumnName))
                        {
                            var value = row[column];
                            return value != DBNull.Value ? value.ToString() : "";
                        }
                    }
                }
            }
            
            return string.IsNullOrEmpty(defaultResult) ? "未找到匹配结果" : defaultResult;
        }

        /// <summary>
        /// 判断是否匹配
        /// </summary>
        private bool IsMatch(DataRow row, string inputValue, string matchType)
        {
            foreach (DataColumn column in LookupTable.Columns)
            {
                if (_inputProperties.Any(p => p.PropertyName == column.ColumnName))
                {
                    var rowValue = row[column]?.ToString();
                    
                    switch (matchType)
                    {
                        case "Exact":
                            return string.Equals(rowValue, inputValue, StringComparison.OrdinalIgnoreCase);
                        case "Fuzzy":
                            return rowValue?.IndexOf(inputValue, StringComparison.OrdinalIgnoreCase) >= 0;
                        case "Range":
                            return IsInRange(inputValue, rowValue);
                        case "Regex":
                            return IsRegexMatch(inputValue, rowValue);
                        default:
                            return false;
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// 判断是否在范围内
        /// </summary>
        private bool IsInRange(string inputValue, string rangeValue)
        {
            // 简单的范围匹配逻辑，实际可能需要更复杂的处理
            return inputValue == rangeValue;
        }

        /// <summary>
        /// 判断是否正则匹配
        /// </summary>
        private bool IsRegexMatch(string inputValue, string pattern)
        {
            try
            {
                return Regex.IsMatch(inputValue, pattern);
            }
            catch
            {
                return false;
            }
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

    /// <summary>
    /// 数据变化事件参数
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        public string ChangeType { get; }
        public object Data { get; }

        public DataChangedEventArgs(string changeType, object data)
        {
            ChangeType = changeType;
            Data = data;
        }
    }
}