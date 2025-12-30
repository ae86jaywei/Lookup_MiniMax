using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZWDynLookup.Models;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 特性管理器对话框
    /// </summary>
    public partial class PropertiesManagerDialog : Form
    {
        private List<ParameterProperty> _availableProperties;
        private List<ParameterProperty> _selectedProperties;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="availableProperties">可用特性列表</param>
        /// <param name="selectedProperties">已选特性列表</param>
        public PropertiesManagerDialog(List<ParameterProperty> availableProperties, List<ParameterProperty> selectedProperties)
        {
            InitializeComponent();
            _availableProperties = availableProperties ?? new List<ParameterProperty>();
            _selectedProperties = selectedProperties ?? new List<ParameterProperty>();
            InitializeControls();
            LoadData();
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 设置表单属性
            this.Text = "参数特性管理器";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 可用特性标签
            var availableLabel = new Label
            {
                Text = "可用特性:",
                Location = new Point(20, 20),
                Size = new Size(100, 23)
            };
            this.Controls.Add(availableLabel);

            // 可用特性列表框
            var availableListBox = new ListBox
            {
                Name = "AvailableListBox",
                Location = new Point(20, 45),
                Size = new Size(250, 200),
                SelectionMode = SelectionMode.MultiExtended,
                TabIndex = 0
            };
            this.Controls.Add(availableListBox);

            // 已选特性标签
            var selectedLabel = new Label
            {
                Text = "已选特性:",
                Location = new Point(320, 20),
                Size = new Size(100, 23)
            };
            this.Controls.Add(selectedLabel);

            // 已选特性列表框
            var selectedListBox = new ListBox
            {
                Name = "SelectedListBox",
                Location = new Point(320, 45),
                Size = new Size(250, 200),
                SelectionMode = SelectionMode.MultiExtended,
                TabIndex = 1
            };
            this.Controls.Add(selectedListBox);

            // 添加按钮
            var addButton = new Button
            {
                Text = "添加 >>",
                Location = new Point(280, 80),
                Size = new Size(75, 23),
                TabIndex = 2
            };
            addButton.Click += AddButton_Click;
            this.Controls.Add(addButton);

            // 移除按钮
            var removeButton = new Button
            {
                Text = "<< 移除",
                Location = new Point(280, 110),
                Size = new Size(75, 23),
                TabIndex = 3
            };
            removeButton.Click += RemoveButton_Click;
            this.Controls.Add(removeButton);

            // 全部添加按钮
            var addAllButton = new Button
            {
                Text = "全部添加 >>",
                Location = new Point(280, 150),
                Size = new Size(75, 23),
                TabIndex = 4
            };
            addAllButton.Click += AddAllButton_Click;
            this.Controls.Add(addAllButton);

            // 全部移除按钮
            var removeAllButton = new Button
            {
                Text = "<< 全部移除",
                Location = new Point(280, 180),
                Size = new Size(75, 23),
                TabIndex = 5
            };
            removeAllButton.Click += RemoveAllButton_Click;
            this.Controls.Add(removeAllButton);

            // 特性编辑区域
            var editLabel = new Label
            {
                Text = "特性编辑:",
                Location = new Point(20, 260),
                Size = new Size(100, 23)
            };
            this.Controls.Add(editLabel);

            // 特性属性网格
            var propertiesDataGridView = new DataGridView
            {
                Name = "PropertiesDataGridView",
                Location = new Point(20, 285),
                Size = new Size(550, 120),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                ReadOnly = false,
                MultiSelect = false,
                TabIndex = 6
            };

            // 设置列
            propertiesDataGridView.Columns.Add("PropertyName", "特性名称");
            propertiesDataGridView.Columns.Add("DisplayName", "显示名称");
            propertiesDataGridView.Columns.Add("Type", "类型");
            propertiesDataGridView.Columns.Add("Value", "特性值");
            propertiesDataGridView.Columns.Add("DisplayValue", "显示值");

            this.Controls.Add(propertiesDataGridView);

            // 按钮区域
            // 确定按钮
            var okButton = new Button
            {
                Text = "确定(&O)",
                Location = new Point(420, 420),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK,
                TabIndex = 7
            };
            this.Controls.Add(okButton);

            // 取消按钮
            var cancelButton = new Button
            {
                Text = "取消(&C)",
                Location = new Point(510, 420),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel,
                TabIndex = 8
            };
            this.Controls.Add(cancelButton);

            // 应用按钮
            var applyButton = new Button
            {
                Text = "应用(&A)",
                Location = new Point(330, 420),
                Size = new Size(75, 23),
                TabIndex = 9
            };
            applyButton.Click += ApplyButton_Click;
            this.Controls.Add(applyButton);

            // 设置接受按钮
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            // 绑定事件
            var availableListBox = this.Controls["AvailableListBox"] as ListBox;
            var selectedListBox = this.Controls["SelectedListBox"] as ListBox;
            var propertiesDataGridView = this.Controls["PropertiesDataGridView"] as DataGridView;

            availableListBox!.DoubleClick += AvailableListBox_DoubleClick;
            selectedListBox!.DoubleClick += SelectedListBox_DoubleClick;
            propertiesDataGridView!.SelectionChanged += PropertiesDataGridView_SelectionChanged;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            var availableListBox = this.Controls["AvailableListBox"] as ListBox;
            var selectedListBox = this.Controls["SelectedListBox"] as ListBox;

            if (availableListBox != null)
            {
                availableListBox.Items.Clear();
                foreach (var property in _availableProperties)
                {
                    availableListBox.Items.Add(property);
                }
            }

            if (selectedListBox != null)
            {
                selectedListBox.Items.Clear();
                foreach (var property in _selectedProperties)
                {
                    selectedListBox.Items.Add(property);
                }
            }

            LoadPropertiesData();
        }

        /// <summary>
        /// 加载特性数据
        /// </summary>
        private void LoadPropertiesData()
        {
            var propertiesDataGridView = this.Controls["PropertiesDataGridView"] as DataGridView;
            if (propertiesDataGridView == null) return;

            propertiesDataGridView.Rows.Clear();

            foreach (var property in _selectedProperties)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.Name });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.DisplayName });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.GetTypeDisplayName() });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.Value });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.DisplayValue });
                row.Tag = property;
                
                propertiesDataGridView.Rows.Add(row);
            }
        }

        /// <summary>
        /// 获取选中的特性
        /// </summary>
        /// <returns>特性列表</returns>
        public List<ParameterProperty> GetSelectedProperties()
        {
            return new List<ParameterProperty>(_selectedProperties);
        }

        #region 事件处理

        /// <summary>
        /// 添加按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            var availableListBox = this.Controls["AvailableListBox"] as ListBox;
            var selectedListBox = this.Controls["SelectedListBox"] as ListBox;

            if (availableListBox?.SelectedItems.Count > 0 && selectedListBox != null)
            {
                foreach (var selectedItem in availableListBox.SelectedItems)
                {
                    if (selectedItem is ParameterProperty property && !_selectedProperties.Contains(property))
                    {
                        _selectedProperties.Add(property);
                        selectedListBox.Items.Add(property);
                    }
                }
            }
        }

        /// <summary>
        /// 移除按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            var selectedListBox = this.Controls["SelectedListBox"] as ListBox;

            if (selectedListBox?.SelectedItems.Count > 0)
            {
                var itemsToRemove = new List<ParameterProperty>();
                foreach (var selectedItem in selectedListBox.SelectedItems)
                {
                    if (selectedItem is ParameterProperty property)
                    {
                        itemsToRemove.Add(property);
                    }
                }

                foreach (var property in itemsToRemove)
                {
                    _selectedProperties.Remove(property);
                    selectedListBox.Items.Remove(property);
                }

                LoadPropertiesData();
            }
        }

        /// <summary>
        /// 全部添加按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void AddAllButton_Click(object sender, EventArgs e)
        {
            var availableListBox = this.Controls["AvailableListBox"] as ListBox;
            var selectedListBox = this.Controls["SelectedListBox"] as ListBox;

            if (availableListBox != null && selectedListBox != null)
            {
                foreach (var property in _availableProperties)
                {
                    if (!_selectedProperties.Contains(property))
                    {
                        _selectedProperties.Add(property);
                        selectedListBox.Items.Add(property);
                    }
                }
            }
        }

        /// <summary>
        /// 全部移除按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void RemoveAllButton_Click(object sender, EventArgs e)
        {
            var selectedListBox = this.Controls["SelectedListBox"] as ListBox;

            if (selectedListBox != null)
            {
                _selectedProperties.Clear();
                selectedListBox.Items.Clear();
                LoadPropertiesData();
            }
        }

        /// <summary>
        /// 可用特性列表双击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void AvailableListBox_DoubleClick(object sender, EventArgs e)
        {
            AddButton_Click(sender, e);
        }

        /// <summary>
        /// 已选特性列表双击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void SelectedListBox_DoubleClick(object sender, EventArgs e)
        {
            RemoveButton_Click(sender, e);
        }

        /// <summary>
        /// 特性网格选择更改事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void PropertiesDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            // 处理特性网格选择更改
        }

        /// <summary>
        /// 应用按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            var propertiesDataGridView = this.Controls["PropertiesDataGridView"] as DataGridView;
            if (propertiesDataGridView == null) return;

            // 更新特性数据
            foreach (DataGridViewRow row in propertiesDataGridView.Rows)
            {
                if (row.Tag is ParameterProperty property && row.Cells["PropertyName"].Value != null)
                {
                    property.Name = row.Cells["PropertyName"].Value?.ToString() ?? "";
                    property.DisplayName = row.Cells["DisplayName"].Value?.ToString() ?? "";
                    property.Value = row.Cells["Value"].Value?.ToString() ?? "";
                    property.DisplayValue = row.Cells["DisplayValue"].Value?.ToString() ?? "";
                    property.IsModified = true;
                }
            }

            MessageBox.Show("特性数据已应用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}