using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 动作属性设置对话框
    /// </summary>
    public partial class ActionPropertiesDialog : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ActionPropertiesDialog()
        {
            InitializeComponent();
            InitializeControls();
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 设置表单属性
            this.Text = "设置查寻动作属性";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 动作类型标签
            var actionTypeLabel = new Label
            {
                Text = "动作类型:",
                Location = new Point(20, 15),
                Size = new Size(80, 23)
            };
            this.Controls.Add(actionTypeLabel);

            var actionTypeTextBox = new TextBox
            {
                Name = "ActionTypeTextBox",
                Location = new Point(110, 12),
                Size = new Size(150, 23),
                ReadOnly = true,
                BackColor = SystemColors.Control,
                Text = "查寻动作"
            };
            this.Controls.Add(actionTypeTextBox);

            // 动作名称标签
            var nameLabel = new Label
            {
                Text = "动作名称(&A):",
                Location = new Point(20, 45),
                Size = new Size(80, 23)
            };
            this.Controls.Add(nameLabel);

            // 动作名称文本框
            var nameTextBox = new TextBox
            {
                Name = "ActionNameTextBox",
                Location = new Point(110, 42),
                Size = new Size(200, 23),
                TabIndex = 0
            };
            this.Controls.Add(nameTextBox);

            // 描述标签
            var descriptionLabel = new Label
            {
                Text = "动作描述(&D):",
                Location = new Point(20, 75),
                Size = new Size(80, 23)
            };
            this.Controls.Add(descriptionLabel);

            // 描述文本框
            var descriptionTextBox = new TextBox
            {
                Name = "DescriptionTextBox",
                Location = new Point(110, 72),
                Size = new Size(350, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                TabIndex = 1
            };
            this.Controls.Add(descriptionTextBox);

            // 默认值标签
            var defaultValueLabel = new Label
            {
                Text = "默认值:",
                Location = new Point(20, 145),
                Size = new Size(80, 23)
            };
            this.Controls.Add(defaultValueLabel);

            var defaultValueComboBox = new ComboBox
            {
                Name = "DefaultValueComboBox",
                Location = new Point(110, 142),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                TabIndex = 2
            };
            this.Controls.Add(defaultValueComboBox);

            // 查寻值列表标签
            var lookupValuesLabel = new Label
            {
                Text = "查寻值列表:",
                Location = new Point(20, 175),
                Size = new Size(80, 23)
            };
            this.Controls.Add(lookupValuesLabel);

            var lookupValuesTextBox = new TextBox
            {
                Name = "LookupValuesTextBox",
                Location = new Point(110, 172),
                Size = new Size(350, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = SystemColors.ControlLight,
                TabIndex = 3
            };
            this.Controls.Add(lookupValuesTextBox);

            // 编辑查寻值按钮
            var editLookupValuesButton = new Button
            {
                Text = "编辑查寻值(&E)",
                Location = new Point(320, 260),
                Size = new Size(100, 23),
                TabIndex = 4
            };
            editLookupValuesButton.Click += EditLookupValuesButton_Click;
            this.Controls.Add(editLookupValuesButton);

            // 选择信息标签
            var selectionInfoLabel = new Label
            {
                Text = "选择信息:",
                Location = new Point(20, 290),
                Size = new Size(80, 23)
            };
            this.Controls.Add(selectionInfoLabel);

            // 选择信息显示
            var selectionInfoTextBox = new TextBox
            {
                Name = "SelectionInfoTextBox",
                Location = new Point(110, 287),
                Size = new Size(350, 40),
                Multiline = true,
                ReadOnly = true,
                BackColor = SystemColors.Control,
                TabIndex = 5
            };
            this.Controls.Add(selectionInfoTextBox);

            // 动作设置标签
            var actionSettingsLabel = new Label
            {
                Text = "动作设置:",
                Location = new Point(20, 340),
                Size = new Size(80, 23)
            };
            this.Controls.Add(actionSettingsLabel);

            // 启用动作复选框
            var enableActionCheckBox = new CheckBox
            {
                Name = "EnableActionCheckBox",
                Text = "启用动作",
                Location = new Point(110, 338),
                Size = new Size(80, 23),
                Checked = true,
                TabIndex = 6
            };
            this.Controls.Add(enableActionCheckBox);

            // 显示标签复选框
            var showLabelCheckBox = new CheckBox
            {
                Name = "ShowLabelCheckBox",
                Text = "显示标签",
                Location = new Point(200, 338),
                Size = new Size(80, 23),
                Checked = true,
                TabIndex = 7
            };
            this.Controls.Add(showLabelCheckBox);

            // 确定按钮
            var okButton = new Button
            {
                Text = "确定(&O)",
                Location = new Point(280, 380),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK,
                TabIndex = 8
            };
            this.Controls.Add(okButton);

            // 取消按钮
            var cancelButton = new Button
            {
                Text = "取消(&C)",
                Location = new Point(365, 380),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel,
                TabIndex = 9
            };
            this.Controls.Add(cancelButton);

            // 设置接受按钮
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        #region 属性

        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName
        {
            get
            {
                var textBox = this.Controls["ActionNameTextBox"] as TextBox;
                return textBox?.Text ?? "";
            }
            set
            {
                var textBox = this.Controls["ActionNameTextBox"] as TextBox;
                if (textBox != null)
                {
                    textBox.Text = value;
                }
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get
            {
                var textBox = this.Controls["DescriptionTextBox"] as TextBox;
                return textBox?.Text ?? "";
            }
            set
            {
                var textBox = this.Controls["DescriptionTextBox"] as TextBox;
                if (textBox != null)
                {
                    textBox.Text = value;
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue
        {
            get
            {
                var comboBox = this.Controls["DefaultValueComboBox"] as ComboBox;
                return comboBox?.SelectedItem?.ToString() ?? "";
            }
            set
            {
                var comboBox = this.Controls["DefaultValueComboBox"] as ComboBox;
                if (comboBox != null && !string.IsNullOrEmpty(value))
                {
                    var index = comboBox.Items.IndexOf(value);
                    if (index >= 0)
                    {
                        comboBox.SelectedIndex = index;
                    }
                }
            }
        }

        /// <summary>
        /// 查寻值列表
        /// </summary>
        public List<string> LookupValues
        {
            get
            {
                var textBox = this.Controls["LookupValuesTextBox"] as TextBox;
                if (textBox == null) return new List<string>();
                
                return textBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(line => !string.IsNullOrWhiteSpace(line))
                                 .Select(line => line.Trim())
                                 .ToList();
            }
            set
            {
                var textBox = this.Controls["LookupValuesTextBox"] as TextBox;
                var comboBox = this.Controls["DefaultValueComboBox"] as ComboBox;
                
                if (textBox != null && comboBox != null && value != null)
                {
                    var valuesText = string.Join("\n", value);
                    textBox.Text = valuesText;
                    
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(value.ToArray());
                    if (comboBox.Items.Count > 0)
                    {
                        comboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 选中的参数数量
        /// </summary>
        public int SelectedParameters
        {
            set
            {
                var textBox = this.Controls["SelectionInfoTextBox"] as TextBox;
                if (textBox != null)
                {
                    textBox.Text = $"已选择 {value} 个查寻参数";
                }
            }
        }

        /// <summary>
        /// 选中的对象数量
        /// </summary>
        public int SelectedObjects
        {
            set
            {
                var textBox = this.Controls["SelectionInfoTextBox"] as TextBox;
                if (textBox != null)
                {
                    var currentText = textBox.Text;
                    if (string.IsNullOrEmpty(currentText))
                    {
                        textBox.Text = $"已选择 {value} 个对象";
                    }
                    else
                    {
                        textBox.Text = currentText + $"，{value} 个对象";
                    }
                }
            }
        }

        /// <summary>
        /// 是否启用动作
        /// </summary>
        public bool EnableAction
        {
            get
            {
                var checkBox = this.Controls["EnableActionCheckBox"] as CheckBox;
                return checkBox?.Checked ?? true;
            }
            set
            {
                var checkBox = this.Controls["EnableActionCheckBox"] as CheckBox;
                if (checkBox != null)
                {
                    checkBox.Checked = value;
                }
            }
        }

        /// <summary>
        /// 是否显示标签
        /// </summary>
        public bool ShowLabel
        {
            get
            {
                var checkBox = this.Controls["ShowLabelCheckBox"] as CheckBox;
                return checkBox?.Checked ?? true;
            }
            set
            {
                var checkBox = this.Controls["ShowLabelCheckBox"] as CheckBox;
                if (checkBox != null)
                {
                    checkBox.Checked = value;
                }
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ActionPropertiesDialog_Load(object sender, EventArgs e)
        {
            // 设置默认焦点
            var actionNameTextBox = this.Controls["ActionNameTextBox"] as TextBox;
            actionNameTextBox?.Focus();
        }

        /// <summary>
        /// 动作名称文本框验证事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ActionNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider1.SetError(textBox, "动作名称不能为空");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(textBox, "");
            }
        }

        /// <summary>
        /// 编辑查寻值按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void EditLookupValuesButton_Click(object sender, EventArgs e)
        {
            var editDialog = new LookupValuesEditDialog();
            var currentValues = this.LookupValues;
            if (currentValues.Any())
            {
                editDialog.LookupValues = new List<string>(currentValues);
            }

            if (editDialog.ShowDialog() == DialogResult.OK)
            {
                this.LookupValues = editDialog.LookupValues;
                
                // 更新默认值下拉框
                var comboBox = this.Controls["DefaultValueComboBox"] as ComboBox;
                if (comboBox != null && comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            // 验证输入
            if (!this.ValidateChildren())
            {
                return;
            }

            // 额外验证
            var comboBox = this.Controls["DefaultValueComboBox"] as ComboBox;
            if (comboBox != null && comboBox.Items.Count == 0)
            {
                MessageBox.Show("请至少添加一个查寻值", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

        #region 嵌套对话框类

        /// <summary>
        /// 查寻值编辑对话框
        /// </summary>
        private class LookupValuesEditDialog : Form
        {
            private ListBox _valuesListBox;
            private TextBox _valueTextBox;
            private Button _addButton;
            private Button _removeButton;
            private Button _editButton;
            private Button _clearButton;
            private Button _okButton;
            private Button _cancelButton;
            
            public List<string> LookupValues { get; set; } = new List<string>();

            public LookupValuesEditDialog()
            {
                InitializeDialog();
                RefreshListBox();
            }

            private void InitializeDialog()
            {
                this.Text = "编辑查寻值";
                this.Size = new Size(400, 350);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                var label = new Label
                {
                    Text = "查寻值列表:",
                    Location = new Point(20, 20),
                    Size = new Size(100, 23)
                };
                this.Controls.Add(label);

                _valuesListBox = new ListBox
                {
                    Location = new Point(20, 45),
                    Size = new Size(250, 150),
                    SelectionMode = SelectionMode.One
                };
                this.Controls.Add(_valuesListBox);

                _valueTextBox = new TextBox
                {
                    Location = new Point(20, 210),
                    Size = new Size(250, 23)
                };
                this.Controls.Add(_valueTextBox);

                _addButton = new Button
                {
                    Text = "添加(&A)",
                    Location = new Point(280, 45),
                    Size = new Size(75, 23)
                };
                _addButton.Click += AddButton_Click;
                this.Controls.Add(_addButton);

                _editButton = new Button
                {
                    Text = "编辑(&E)",
                    Location = new Point(280, 75),
                    Size = new Size(75, 23)
                };
                _editButton.Click += EditButton_Click;
                this.Controls.Add(_editButton);

                _removeButton = new Button
                {
                    Text = "删除(&R)",
                    Location = new Point(280, 105),
                    Size = new Size(75, 23)
                };
                _removeButton.Click += RemoveButton_Click;
                this.Controls.Add(_removeButton);

                _clearButton = new Button
                {
                    Text = "清空(&C)",
                    Location = new Point(280, 135),
                    Size = new Size(75, 23)
                };
                _clearButton.Click += ClearButton_Click;
                this.Controls.Add(_clearButton);

                _okButton = new Button
                {
                    Text = "确定(&O)",
                    Location = new Point(200, 260),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.OK
                };
                _okButton.Click += OkButton_Click;
                this.Controls.Add(_okButton);

                _cancelButton = new Button
                {
                    Text = "取消(&C)",
                    Location = new Point(285, 260),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.Cancel
                };
                this.Controls.Add(_cancelButton);
            }

            private void RefreshListBox()
            {
                _valuesListBox.Items.Clear();
                _valuesListBox.Items.AddRange(LookupValues.ToArray());
            }

            private void AddButton_Click(object sender, EventArgs e)
            {
                var value = _valueTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    if (!LookupValues.Contains(value))
                    {
                        LookupValues.Add(value);
                        RefreshListBox();
                        _valueTextBox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("该值已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            private void EditButton_Click(object sender, EventArgs e)
            {
                var selectedIndex = _valuesListBox.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    var newValue = _valueTextBox.Text.Trim();
                    if (!string.IsNullOrEmpty(newValue) && !LookupValues.Contains(newValue))
                    {
                        LookupValues[selectedIndex] = newValue;
                        RefreshListBox();
                    }
                    else
                    {
                        MessageBox.Show("请输入有效的值或确保值不重复", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            private void RemoveButton_Click(object sender, EventArgs e)
            {
                var selectedIndex = _valuesListBox.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    LookupValues.RemoveAt(selectedIndex);
                    RefreshListBox();
                }
            }

            private void ClearButton_Click(object sender, EventArgs e)
            {
                if (MessageBox.Show("确定要清空所有查寻值吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    LookupValues.Clear();
                    RefreshListBox();
                }
            }

            private void OkButton_Click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        #endregion
    }
}