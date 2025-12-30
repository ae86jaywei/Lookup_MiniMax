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
    /// 查寻表管理器对话框
    /// </summary>
    public partial class LookupTableManagerDialog : Form
    {
        private List<LookupTableData> _lookupTables;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lookupTables">查寻表数据列表</param>
        public LookupTableManagerDialog(List<LookupTableData> lookupTables)
        {
            InitializeComponent();
            _lookupTables = lookupTables ?? new List<LookupTableData>();
            InitializeControls();
            LoadData();
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 设置表单属性
            this.Text = "查寻表管理器";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 查寻表列表标签
            var listLabel = new Label
            {
                Text = "查寻表列表:",
                Location = new Point(20, 20),
                Size = new Size(100, 23)
            };
            this.Controls.Add(listLabel);

            // 查寻表列表视图
            var listView = new ListView
            {
                Name = "LookupTableListView",
                Location = new Point(20, 45),
                Size = new Size(400, 350),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                TabIndex = 0
            };
            
            // 添加列
            listView.Columns.Add("动作名称", 120);
            listView.Columns.Add("描述", 150);
            listView.Columns.Add("特性数量", 80);
            listView.Columns.Add("选择集大小", 80);
            listView.Columns.Add("状态", 60);
            
            this.Controls.Add(listView);

            // 特性编辑区域标签
            var propertiesLabel = new Label
            {
                Text = "查寻特性:",
                Location = new Point(440, 20),
                Size = new Size(100, 23)
            };
            this.Controls.Add(propertiesLabel);

            // 特性数据网格
            var dataGridView = new DataGridView
            {
                Name = "PropertiesDataGridView",
                Location = new Point(440, 45),
                Size = new Size(320, 200),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                ReadOnly = false,
                MultiSelect = false,
                TabIndex = 1
            };
            
            // 设置列
            dataGridView.Columns.Add("PropertyName", "特性名称");
            dataGridView.Columns.Add("DisplayValue", "显示值");
            dataGridView.Columns.Add("Value", "特性值");
            
            this.Controls.Add(dataGridView);

            // 按钮区域
            // 新建按钮
            var newButton = new Button
            {
                Text = "新建(&N)",
                Location = new Point(20, 410),
                Size = new Size(75, 23),
                TabIndex = 2
            };
            newButton.Click += NewButton_Click;
            this.Controls.Add(newButton);

            // 编辑按钮
            var editButton = new Button
            {
                Text = "编辑(&E)",
                Location = new Point(105, 410),
                Size = new Size(75, 23),
                TabIndex = 3
            };
            editButton.Click += EditButton_Click;
            this.Controls.Add(editButton);

            // 删除按钮
            var deleteButton = new Button
            {
                Text = "删除(&D)",
                Location = new Point(190, 410),
                Size = new Size(75, 23),
                TabIndex = 4
            };
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);

            // 应用按钮
            var applyButton = new Button
            {
                Text = "应用(&A)",
                Location = new Point(440, 260),
                Size = new Size(75, 23),
                TabIndex = 5
            };
            applyButton.Click += ApplyButton_Click;
            this.Controls.Add(applyButton);

            // 确定按钮
            var okButton = new Button
            {
                Text = "确定(&O)",
                Location = new Point(610, 410),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK,
                TabIndex = 6
            };
            this.Controls.Add(okButton);

            // 取消按钮
            var cancelButton = new Button
            {
                Text = "取消(&C)",
                Location = new Point(695, 410),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel,
                TabIndex = 7
            };
            this.Controls.Add(cancelButton);

            // 设置接受按钮
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            // 绑定事件
            var listView = this.Controls["LookupTableListView"] as ListView;
            listView!.SelectedIndexChanged += LookupTableListView_SelectedIndexChanged;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            var listView = this.Controls["LookupTableListView"] as ListView;
            if (listView == null) return;

            listView.Items.Clear();

            foreach (var lookupTable in _lookupTables)
            {
                var item = new ListViewItem(lookupTable.ActionName);
                item.SubItems.Add(lookupTable.Description);
                item.SubItems.Add(lookupTable.Properties.Count.ToString());
                item.SubItems.Add(lookupTable.SelectionSet.Count.ToString());
                item.SubItems.Add(lookupTable.IsModified ? "已修改" : "正常");
                item.Tag = lookupTable;

                listView.Items.Add(item);
            }
        }

        /// <summary>
        /// 获取查寻表数据
        /// </summary>
        /// <returns>查寻表数据列表</returns>
        public List<LookupTableData> GetLookupTables()
        {
            return _lookupTables;
        }

        #region 事件处理

        /// <summary>
        /// 查寻表列表选择更改事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void LookupTableListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listView = sender as ListView;
            var dataGridView = this.Controls["PropertiesDataGridView"] as DataGridView;
            
            if (listView?.SelectedItems.Count > 0 && dataGridView != null)
            {
                var selectedLookupTable = listView.SelectedItems[0].Tag as LookupTableData;
                if (selectedLookupTable != null)
                {
                    LoadPropertiesData(selectedLookupTable);
                }
            }
        }

        /// <summary>
        /// 加载特性数据
        /// </summary>
        /// <param name="lookupTable">查寻表数据</param>
        private void LoadPropertiesData(LookupTableData lookupTable)
        {
            var dataGridView = this.Controls["PropertiesDataGridView"] as DataGridView;
            if (dataGridView == null) return;

            dataGridView.Rows.Clear();

            foreach (var property in lookupTable.Properties)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.Name });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.DisplayValue });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = property.Value });
                dataGridView.Rows.Add(row);
            }
        }

        /// <summary>
        /// 新建按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void NewButton_Click(object sender, EventArgs e)
        {
            // 创建新的查寻表
            var newLookupTable = new LookupTableData
            {
                ActionName = "新查寻动作",
                Description = "新建的查寻动作",
                IsModified = true
            };

            _lookupTables.Add(newLookupTable);
            LoadData();

            // 选中新创建的项目
            var listView = this.Controls["LookupTableListView"] as ListView;
            if (listView?.Items.Count > 0)
            {
                listView.Items[listView.Items.Count - 1].Selected = true;
            }
        }

        /// <summary>
        /// 编辑按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void EditButton_Click(object sender, EventArgs e)
        {
            var listView = this.Controls["LookupTableListView"] as ListView;
            if (listView?.SelectedItems.Count > 0)
            {
                var selectedLookupTable = listView.SelectedItems[0].Tag as LookupTableData;
                if (selectedLookupTable != null)
                {
                    // 这里可以打开编辑对话框
                    MessageBox.Show($"编辑查寻表: {selectedLookupTable.ActionName}", "编辑", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// 删除按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var listView = this.Controls["LookupTableListView"] as ListView;
            if (listView?.SelectedItems.Count > 0)
            {
                var selectedLookupTable = listView.SelectedItems[0].Tag as LookupTableData;
                if (selectedLookupTable != null)
                {
                    var result = MessageBox.Show($"确定要删除查寻表 '{selectedLookupTable.ActionName}' 吗？", 
                        "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        _lookupTables.Remove(selectedLookupTable);
                        LoadData();
                    }
                }
            }
        }

        /// <summary>
        /// 应用按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            var listView = this.Controls["LookupTableListView"] as ListView;
            var dataGridView = this.Controls["PropertiesDataGridView"] as DataGridView;
            
            if (listView?.SelectedItems.Count > 0 && dataGridView != null)
            {
                var selectedLookupTable = listView.SelectedItems[0].Tag as LookupTableData;
                if (selectedLookupTable != null)
                {
                    UpdatePropertiesData(selectedLookupTable, dataGridView);
                    LoadData(); // 刷新列表
                }
            }
        }

        /// <summary>
        /// 更新特性数据
        /// </summary>
        /// <param name="lookupTable">查寻表数据</param>
        /// <param name="dataGridView">数据网格</param>
        private void UpdatePropertiesData(LookupTableData lookupTable, DataGridView dataGridView)
        {
            lookupTable.Properties.Clear();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["PropertyName"].Value != null)
                {
                    var property = new ParameterProperty
                    {
                        Name = row.Cells["PropertyName"].Value?.ToString() ?? "",
                        DisplayValue = row.Cells["DisplayValue"].Value?.ToString() ?? "",
                        Value = row.Cells["Value"].Value?.ToString() ?? "",
                        Type = PropertyType.Lookup
                    };

                    lookupTable.Properties.Add(property);
                }
            }

            lookupTable.IsModified = true;
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void LookupTableManagerDialog_Load(object sender, EventArgs e)
        {
            // 窗体加载时的处理
        }

        #endregion
    }
}