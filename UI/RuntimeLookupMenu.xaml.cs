using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ZWDynLookup.Service;

namespace ZWDynLookup.UI
{
    /// <summary>
    /// 运行时查寻下拉菜单
    /// 交互逻辑
    /// </summary>
    public partial class RuntimeLookupMenu : Window
    {
        private BlockReference _targetBlock;
        private Point3d _displayPosition;
        private List<LookupAction> _availableActions = new List<LookupAction>();

        public RuntimeLookupMenu()
        {
            InitializeComponent();
            this.Loaded += RuntimeLookupMenu_Loaded;
            this.Deactivated += RuntimeLookupMenu_Deactivated;
        }

        /// <summary>
        /// 显示运行时查寻菜单
        /// </summary>
        /// <param name="blockReference">目标块引用</param>
        /// <param name="displayPosition">显示位置</param>
        /// <param name="availableActions">可用动作列表</param>
        public void ShowMenu(BlockReference blockReference, Point3d displayPosition, List<LookupAction> availableActions = null)
        {
            try
            {
                _targetBlock = blockReference;
                _displayPosition = displayPosition;
                _availableActions = availableActions ?? GetDefaultActions();

                // 更新菜单内容
                UpdateMenuItems();

                // 计算屏幕位置
                var screenPosition = CalculateScreenPosition(displayPosition);
                
                // 设置窗口位置
                this.Left = screenPosition.X;
                this.Top = screenPosition.Y;

                // 显示窗口
                this.Show();
                this.Activate();

                // 记录当前动作状态
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示运行时菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 窗口加载完成
        /// </summary>
        private void RuntimeLookupMenu_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 设置鼠标事件
                this.MouseLeftButtonDown += RuntimeLookupMenu_MouseLeftButtonDown;
                
                // 添加键盘事件
                this.KeyDown += RuntimeLookupMenu_KeyDown;
                
                // 设置焦点
                this.Focus();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n初始化运行时菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 窗口失去焦点时关闭
        /// </summary>
        private void RuntimeLookupMenu_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 更新菜单项
        /// </summary>
        private void UpdateMenuItems()
        {
            try
            {
                MainMenu.Items.Clear();

                // 添加动作菜单项
                foreach (var action in _availableActions)
                {
                    var menuItem = new MenuItem
                    {
                        Header = $"切换到: {action.Name}",
                        Tag = action.Name,
                        IsEnabled = action.IsActive || !action.Name.Contains("当前")
                    };

                    // 根据动作状态设置样式
                    if (action.IsActive)
                    {
                        menuItem.FontWeight = FontWeights.Bold;
                        menuItem.Foreground = new SolidColorBrush(Colors.Yellow);
                    }

                    menuItem.Click += ActionMenuItem_Click;
                    MainMenu.Items.Add(menuItem);
                }

                // 添加分隔线
                MainMenu.Items.Add(new Separator { Style = (Style)FindResource("SeparatorStyle") });

                // 添加其他菜单项
                AddUtilityMenuItems();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新菜单项失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加实用工具菜单项
        /// </summary>
        private void AddUtilityMenuItems()
        {
            try
            {
                // 查寻参数菜单项
                var lookupParamItem = new MenuItem
                {
                    Header = "查寻参数...",
                    Tag = "LookupParameter"
                };
                lookupParamItem.Click += LookupParameterMenuItem_Click;
                MainMenu.Items.Add(lookupParamItem);

                // 显示查寻表菜单项
                var showTableItem = new MenuItem
                {
                    Header = "显示查寻表",
                    Tag = "ShowLookupTable"
                };
                showTableItem.Click += ShowLookupTableMenuItem_Click;
                MainMenu.Items.Add(showTableItem);

                // 分隔线
                MainMenu.Items.Add(new Separator { Style = (Style)FindResource("SeparatorStyle") });

                // 属性菜单项
                var propertiesItem = new MenuItem
                {
                    Header = "属性",
                    Tag = "Properties"
                };
                propertiesItem.Click += PropertiesMenuItem_Click;
                MainMenu.Items.Add(propertiesItem);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n添加实用工具菜单项失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 动作菜单项点击事件
        /// </summary>
        private void ActionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = sender as MenuItem;
                if (menuItem?.Tag is string actionName)
                {
                    SwitchToAction(actionName);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n切换动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查寻参数菜单项点击事件
        /// </summary>
        private void LookupParameterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parameterDialog = new ParameterPropertiesDialog();
                parameterDialog.ShowDialog(_targetBlock);
                this.Close();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示查寻参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示查寻表菜单项点击事件
        /// </summary>
        private void ShowLookupTableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lookupTableDialog = new LookupTableDialog();
                lookupTableDialog.Show(_targetBlock);
                this.Close();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示查寻表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 属性菜单项点击事件
        /// </summary>
        private void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 显示属性对话框
                var propertiesDialog = new PropertiesManagerDialog();
                propertiesDialog.ShowDialog(_targetBlock);
                this.Close();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n显示属性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 鼠标左键按下事件（用于拖拽窗口）
        /// </summary>
        private void RuntimeLookupMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.DragMove();
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n拖拽菜单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 键盘按键事件
        /// </summary>
        private void RuntimeLookupMenu_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        this.Close();
                        break;
                    case Key.Enter:
                    case Key.Space:
                        // 执行当前选中项
                        if (MainMenu.SelectedItem != null)
                        {
                            var selectedItem = MainMenu.SelectedItem as MenuItem;
                            selectedItem?.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n处理键盘事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 切换到指定动作
        /// </summary>
        /// <param name="actionName">动作名称</param>
        private void SwitchToAction(string actionName)
        {
            try
            {
                // 更新标签管理器中的动作
                var tagManager = LookupTagManager.Instance;
                var actionTag = tagManager.GetTag(_targetBlock, "Action");
                
                if (actionTag != null)
                {
                    tagManager.UpdateTagValue(_targetBlock, "Action", actionName);
                    
                    // 更新图形
                    GraphicsUpdateService.Instance.UpdateGraphics(_targetBlock);
                    
                    // 更新状态
                    StatusTextBlock.Text = $"已切换到: {actionName}";
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n切换动作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 计算屏幕位置
        /// </summary>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <returns>屏幕坐标点</returns>
        private Point CalculateScreenPosition(Point3d worldPosition)
        {
            try
            {
                var editor = Application.DocumentManager.MdiActiveDocument?.Editor;
                if (editor == null) return new Point(0, 0);

                var viewport = editor.CurrentViewport;
                var screenPoint = viewport.WorldToScreen(worldPosition);

                // 调整位置以避免超出屏幕边界
                var adjustedX = Math.Max(screenPoint.X, 10);
                var adjustedY = Math.Max(screenPoint.Y, 10);
                adjustedX = Math.Min(adjustedX, System.Windows.SystemParameters.PrimaryScreenWidth - this.Width - 10);
                adjustedY = Math.Min(adjustedY, System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - 10);

                return new Point(adjustedX, adjustedY);
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n计算屏幕位置失败: {ex.Message}");
                return new Point(100, 100);
            }
        }

        /// <summary>
        /// 获取默认动作列表
        /// </summary>
        /// <returns>默认动作列表</returns>
        private List<LookupAction> GetDefaultActions()
        {
            return new List<LookupAction>
            {
                new LookupAction { Name = "动作1", IsActive = false },
                new LookupAction { Name = "动作2", IsActive = false },
                new LookupAction { Name = "动作3", IsActive = false }
            };
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText()
        {
            try
            {
                var activeAction = _availableActions.FirstOrDefault(a => a.IsActive);
                if (activeAction != null)
                {
                    StatusTextBlock.Text = $"当前动作: {activeAction.Name}";
                }
                else
                {
                    StatusTextBlock.Text = "选择动作";
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n更新状态文本失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 关闭菜单
        /// </summary>
        public void CloseMenu()
        {
            this.Close();
        }

        /// <summary>
        /// 重新定位菜单
        /// </summary>
        /// <param name="newPosition">新位置</param>
        public void RepositionMenu(Point3d newPosition)
        {
            try
            {
                _displayPosition = newPosition;
                var screenPosition = CalculateScreenPosition(newPosition);
                this.Left = screenPosition.X;
                this.Top = screenPosition.Y;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n重新定位菜单失败: {ex.Message}");
            }
        }
    }
}