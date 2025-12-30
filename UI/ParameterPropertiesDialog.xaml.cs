using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 参数属性设置对话框 (WPF版本)
    /// </summary>
    public partial class ParameterPropertiesDialog : Window, INotifyPropertyChanged
    {
        #region 私有字段

        private string _parameterName = "LookupParameter";
        private string _label = "标签";
        private string _description = "查寻参数描述";
        private bool _showPalette = true;
        private int _gripCount = 1;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public ParameterPropertiesDialog()
        {
            InitializeComponent();
            DataContext = this;
            
            // 设置初始值
            ParameterName = "LookupParameter";
            Label = "标签";
            Description = "查寻参数描述";
            ShowPalette = true;
            GripCount = 1;
            
            // 订阅事件
            Loaded += ParameterPropertiesDialog_Loaded;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName
        {
            get => _parameterName;
            set
            {
                if (_parameterName != value)
                {
                    _parameterName = value;
                    OnPropertyChanged(nameof(ParameterName));
                    UpdatePreview();
                    ValidateParameterName();
                }
            }
        }

        /// <summary>
        /// 参数标签
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                    UpdatePreview();
                    ValidateLabel();
                }
            }
        }

        /// <summary>
        /// 参数说明
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                    ValidateDescription();
                }
            }
        }

        /// <summary>
        /// 显示选项板
        /// </summary>
        public bool ShowPalette
        {
            get => _showPalette;
            set
            {
                if (_showPalette != value)
                {
                    _showPalette = value;
                    OnPropertyChanged(nameof(ShowPalette));
                }
            }
        }

        /// <summary>
        /// 夹点数量
        /// </summary>
        public int GripCount
        {
            get => _gripCount;
            set
            {
                if (_gripCount != value)
                {
                    _gripCount = value;
                    OnPropertyChanged(nameof(GripCount));
                    UpdatePreview();
                    ValidateGripCount();
                }
            }
        }

        /// <summary>
        /// 验证是否通过
        /// </summary>
        public bool IsValidationPassed { get; private set; } = true;

        #endregion

        #region 事件处理

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ParameterPropertiesDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置焦点到参数名称文本框
            ParameterNameTextBox.Focus();
            ParameterNameTextBox.SelectAll();
        }

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAll())
            {
                IsValidationPassed = true;
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsValidationPassed = false;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 参数名称文本框键盘事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ParameterNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ValidateParameterName())
                {
                    LabelTextBox.Focus();
                    LabelTextBox.SelectAll();
                }
            }
        }

        /// <summary>
        /// 参数标签文本框键盘事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void LabelTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ValidateLabel())
                {
                    DescriptionTextBox.Focus();
                    DescriptionTextBox.SelectAll();
                }
            }
        }

        /// <summary>
        /// 参数说明文本框键盘事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void DescriptionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (ValidateDescription())
                {
                    GripCountComboBox.Focus();
                }
            }
        }

        /// <summary>
        /// 夹点数量组合框键盘事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void GripCountComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ValidateGripCount())
                {
                    OKButton.Focus();
                }
            }
        }

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证参数名称
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateParameterName()
        {
            bool isValid = true;
            string errorMessage = "";

            if (string.IsNullOrWhiteSpace(ParameterName))
            {
                errorMessage = "参数名称不能为空";
                isValid = false;
            }
            else if (ParameterName.Length > 255)
            {
                errorMessage = "参数名称不能超过255个字符";
                isValid = false;
            }
            else if (ContainsInvalidCharacters(ParameterName))
            {
                errorMessage = "参数名称包含非法字符";
                isValid = false;
            }

            ParameterNameErrorTextBlock.Text = errorMessage;
            ParameterNameTextBox.BorderBrush = isValid ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Red;
            
            return isValid;
        }

        /// <summary>
        /// 验证参数标签
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateLabel()
        {
            bool isValid = true;
            string errorMessage = "";

            if (string.IsNullOrWhiteSpace(Label))
            {
                errorMessage = "参数标签不能为空";
                isValid = false;
            }
            else if (Label.Length > 255)
            {
                errorMessage = "参数标签不能超过255个字符";
                isValid = false;
            }

            LabelErrorTextBlock.Text = errorMessage;
            LabelTextBox.BorderBrush = isValid ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Red;
            
            return isValid;
        }

        /// <summary>
        /// 验证参数说明
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateDescription()
        {
            bool isValid = true;
            string errorMessage = "";

            if (!string.IsNullOrEmpty(Description) && Description.Length > 1000)
            {
                errorMessage = "参数说明不能超过1000个字符";
                isValid = false;
            }

            DescriptionErrorTextBlock.Text = errorMessage;
            DescriptionTextBox.BorderBrush = isValid ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Red;
            
            return isValid;
        }

        /// <summary>
        /// 验证夹点数量
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateGripCount()
        {
            bool isValid = true;
            string errorMessage = "";

            if (GripCount < 0 || GripCount > 1)
            {
                errorMessage = "夹点数量只能是0或1";
                isValid = false;
            }

            GripCountErrorTextBlock.Text = errorMessage;
            GripCountComboBox.BorderBrush = isValid ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Red;
            
            return isValid;
        }

        /// <summary>
        /// 验证所有输入
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateAll()
        {
            bool nameValid = ValidateParameterName();
            bool labelValid = ValidateLabel();
            bool descriptionValid = ValidateDescription();
            bool gripCountValid = ValidateGripCount();

            return nameValid && labelValid && descriptionValid && gripCountValid;
        }

        /// <summary>
        /// 检查是否包含非法字符
        /// </summary>
        /// <param name="text">要检查的文本</param>
        /// <returns>是否包含非法字符</returns>
        private bool ContainsInvalidCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // 检查非法字符：<>:"/\|?*
            char[] invalidChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            return text.IndexOfAny(invalidChars) >= 0;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 更新预览信息
        /// </summary>
        private void UpdatePreview()
        {
            PreviewNameTextBlock.Text = $"参数名称: {ParameterName}";
            PreviewLabelTextBlock.Text = $"标签: {Label}";
            PreviewGripTextBlock.Text = $"夹点数量: {GripCount}个";
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="label">标签</param>
        /// <param name="description">描述</param>
        /// <param name="showPalette">显示选项板</param>
        /// <param name="gripCount">夹点数量</param>
        public void SetDefaults(string parameterName = null, string label = null, 
            string description = null, bool? showPalette = null, int? gripCount = null)
        {
            if (!string.IsNullOrEmpty(parameterName))
                ParameterName = parameterName;
            if (!string.IsNullOrEmpty(label))
                Label = label;
            if (!string.IsNullOrEmpty(description))
                Description = description;
            if (showPalette.HasValue)
                ShowPalette = showPalette.Value;
            if (gripCount.HasValue)
                GripCount = gripCount.Value;
        }

        /// <summary>
        /// 获取验证错误信息
        /// </summary>
        /// <returns>错误信息</returns>
        public string GetValidationErrorMessage()
        {
            var errors = new System.Text.StringBuilder();

            if (string.IsNullOrWhiteSpace(ParameterName))
                errors.AppendLine("参数名称不能为空");
            if (string.IsNullOrWhiteSpace(Label))
                errors.AppendLine("参数标签不能为空");
            if (GripCount < 0 || GripCount > 1)
                errors.AppendLine("夹点数量只能是0或1");

            return errors.ToString();
        }

        #endregion

        #region INotifyPropertyChanged 实现

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 显示参数属性对话框并返回结果
        /// </summary>
        /// <param name="owner">父窗口</param>
        /// <param name="parameterName">初始参数名称</param>
        /// <param name="label">初始标签</param>
        /// <param name="description">初始描述</param>
        /// <param name="showPalette">初始显示选项板</param>
        /// <param name="gripCount">初始夹点数量</param>
        /// <returns>用户选择的结果</returns>
        public static ParameterPropertiesDialogResult ShowDialog(
            Window owner = null,
            string parameterName = null,
            string label = null,
            string description = null,
            bool? showPalette = null,
            int? gripCount = null)
        {
            var dialog = new ParameterPropertiesDialog
            {
                Owner = owner
            };

            dialog.SetDefaults(parameterName, label, description, showPalette, gripCount);

            bool? result = dialog.ShowDialog();
            
            return new ParameterPropertiesDialogResult
            {
                Success = result == true,
                ParameterName = dialog.ParameterName,
                Label = dialog.Label,
                Description = dialog.Description,
                ShowPalette = dialog.ShowPalette,
                GripCount = dialog.GripCount,
                IsValidationPassed = dialog.IsValidationPassed
            };
        }

        #endregion
    }

    /// <summary>
    /// 参数属性对话框结果
    /// </summary>
    public class ParameterPropertiesDialogResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// 参数标签
        /// </summary>
        public string Label { get; set; }

        /// <parameter>
        /// 参数说明
        /// </parameter>
        public string Description { get; set; }

        /// <summary>
        /// 显示选项板
        /// </summary>
        public bool ShowPalette { get; set; }

        /// <summary>
        /// 夹点数量
        /// </summary>
        public int GripCount { get; set; }

        /// <summary>
        /// 验证是否通过
        /// </summary>
        public bool IsValidationPassed { get; set; }
    }
}