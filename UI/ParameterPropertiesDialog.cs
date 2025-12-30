using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 参数属性设置对话框
    /// </summary>
    public partial class ParameterPropertiesDialog : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ParameterPropertiesDialog()
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
            this.Text = "设置查寻参数属性";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 参数名称标签
            var nameLabel = new Label
            {
                Text = "参数名称(&N):",
                Location = new Point(20, 20),
                Size = new Size(100, 23)
            };
            this.Controls.Add(nameLabel);

            // 参数名称文本框
            var nameTextBox = new TextBox
            {
                Name = "ParameterNameTextBox",
                Location = new Point(130, 17),
                Size = new Size(240, 23),
                TabIndex = 0
            };
            this.Controls.Add(nameTextBox);

            // 参数标签标签
            var labelLabel = new Label
            {
                Text = "参数标签(&L):",
                Location = new Point(20, 60),
                Size = new Size(100, 23)
            };
            this.Controls.Add(labelLabel);

            // 参数标签文本框
            var labelTextBox = new TextBox
            {
                Name = "LabelTextBox",
                Location = new Point(130, 57),
                Size = new Size(240, 23),
                TabIndex = 1
            };
            this.Controls.Add(labelTextBox);

            // 参数说明标签
            var descriptionLabel = new Label
            {
                Text = "参数说明(&D):",
                Location = new Point(20, 100),
                Size = new Size(100, 23)
            };
            this.Controls.Add(descriptionLabel);

            // 参数说明文本框
            var descriptionTextBox = new TextBox
            {
                Name = "DescriptionTextBox",
                Location = new Point(130, 97),
                Size = new Size(240, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                TabIndex = 2
            };
            this.Controls.Add(descriptionTextBox);

            // 选项板选项标签
            var paletteLabel = new Label
            {
                Text = "选项板设置:",
                Location = new Point(20, 170),
                Size = new Size(100, 23)
            };
            this.Controls.Add(paletteLabel);

            // 显示选项板复选框
            var showPaletteCheckBox = new CheckBox
            {
                Name = "ShowPaletteCheckBox",
                Text = "显示特性选项板(&P)",
                Location = new Point(130, 170),
                Size = new Size(240, 24),
                Checked = true,
                TabIndex = 3
            };
            this.Controls.Add(showPaletteCheckBox);

            // 夹点数量标签
            var gripCountLabel = new Label
            {
                Text = "夹点数量:",
                Location = new Point(20, 200),
                Size = new Size(100, 23)
            };
            this.Controls.Add(gripCountLabel);

            // 夹点数量组合框
            var gripCountComboBox = new ComboBox
            {
                Name = "GripCountComboBox",
                Location = new Point(130, 197),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                TabIndex = 4
            };
            gripCountComboBox.Items.AddRange(new object[] { "0", "1" });
            gripCountComboBox.SelectedIndex = 1; // 默认选择1
            this.Controls.Add(gripCountComboBox);

            // 确定按钮
            var okButton = new Button
            {
                Text = "确定(&O)",
                Location = new Point(200, 250),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK,
                TabIndex = 5
            };
            this.Controls.Add(okButton);

            // 取消按钮
            var cancelButton = new Button
            {
                Text = "取消(&C)",
                Location = new Point(285, 250),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel,
                TabIndex = 6
            };
            this.Controls.Add(cancelButton);

            // 设置接受按钮
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        #region 属性

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName
        {
            get
            {
                var textBox = this.Controls["ParameterNameTextBox"] as TextBox;
                return textBox?.Text ?? "";
            }
            set
            {
                var textBox = this.Controls["ParameterNameTextBox"] as TextBox;
                if (textBox != null)
                {
                    textBox.Text = value;
                }
            }
        }

        /// <summary>
        /// 参数标签
        /// </summary>
        public string Label
        {
            get
            {
                var textBox = this.Controls["LabelTextBox"] as TextBox;
                return textBox?.Text ?? "";
            }
            set
            {
                var textBox = this.Controls["LabelTextBox"] as TextBox;
                if (textBox != null)
                {
                    textBox.Text = value;
                }
            }
        }

        /// <summary>
        /// 参数说明
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
        /// 显示选项板
        /// </summary>
        public bool ShowPalette
        {
            get
            {
                var checkBox = this.Controls["ShowPaletteCheckBox"] as CheckBox;
                return checkBox?.Checked ?? true;
            }
            set
            {
                var checkBox = this.Controls["ShowPaletteCheckBox"] as CheckBox;
                if (checkBox != null)
                {
                    checkBox.Checked = value;
                }
            }
        }

        /// <summary>
        /// 夹点数量
        /// </summary>
        public int GripCount
        {
            get
            {
                var comboBox = this.Controls["GripCountComboBox"] as ComboBox;
                if (comboBox?.SelectedItem != null)
                {
                    return int.Parse(comboBox.SelectedItem.ToString() ?? "1");
                }
                return 1;
            }
            set
            {
                var comboBox = this.Controls["GripCountComboBox"] as ComboBox;
                if (comboBox != null)
                {
                    comboBox.SelectedItem = value.ToString();
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
        private void ParameterPropertiesDialog_Load(object sender, EventArgs e)
        {
            // 设置默认焦点
            var parameterNameTextBox = this.Controls["ParameterNameTextBox"] as TextBox;
            parameterNameTextBox?.Focus();
        }

        /// <summary>
        /// 参数名称文本框验证事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ParameterNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider1.SetError(textBox, "参数名称不能为空");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(textBox, "");
            }
        }

        /// <summary>
        /// 参数标签文本框验证事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void LabelTextBox_Validating(object sender, CancelEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider1.SetError(textBox, "参数标签不能为空");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(textBox, "");
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