using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;

namespace ZWDynLookup.Commands
{
    /// <summary>
    /// 选择集管理器
    /// 负责创建、管理和维护查寻动作的选择集
    /// </summary>
    public class SelectionSetManager
    {
        private Dictionary<string, SelectionSetInfo> _namedSelectionSets = new Dictionary<string, SelectionSetInfo>();
        private List<ObjectId> _currentSelectionSet = new List<ObjectId>();
        private string _currentSetName = string.Empty;

        /// <summary>
        /// 选择集信息类
        /// </summary>
        public class SelectionSetInfo
        {
            public string Name { get; set; }
            public List<ObjectId> ObjectIds { get; set; } = new List<ObjectId>();
            public DateTime CreatedTime { get; set; }
            public string Description { get; set; }
            public Point3d CenterPoint { get; set; }
            public int Count => ObjectIds?.Count ?? 0;

            public SelectionSetInfo(string name)
            {
                Name = name;
                CreatedTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 创建新的选择集
        /// </summary>
        /// <param name="editor">CAD编辑器</param>
        /// <param name="setName">选择集名称</param>
        /// <param name="description">描述</param>
        /// <returns>是否成功创建</returns>
        public bool CreateSelectionSet(Editor editor, string setName, string description = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(setName))
                {
                    MessageBox.Show("选择集名称不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (_namedSelectionSets.ContainsKey(setName))
                {
                    MessageBox.Show($"选择集 '{setName}' 已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 获取用户选择
                var promptSelectionOptions = CreateSelectionOptions();
                var result = editor.GetSelection(promptSelectionOptions);

                if (result.Status == PromptStatus.OK)
                {
                    var objectIds = result.Value.GetObjectIds();
                    var selectionInfo = new SelectionSetInfo(setName)
                    {
                        ObjectIds = new List<ObjectId>(objectIds),
                        Description = description,
                        CenterPoint = CalculateCenterPoint(objectIds)
                    };

                    _namedSelectionSets[setName] = selectionInfo;
                    _currentSetName = setName;
                    _currentSelectionSet = new List<ObjectId>(objectIds);

                    PluginEntry.Log($"选择集 '{setName}' 已创建，包含 {objectIds.Count} 个对象");
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"创建选择集失败: {ex.Message}");
                MessageBox.Show($"创建选择集失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 从当前选择创建选择集
        /// </summary>
        /// <param name="setName">选择集名称</param>
        /// <param name="description">描述</param>
        /// <returns>是否成功创建</returns>
        public bool CreateFromCurrentSelection(string setName, string description = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(setName))
                {
                    MessageBox.Show("选择集名称不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (_currentSelectionSet.Count == 0)
                {
                    MessageBox.Show("当前没有选择任何对象", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (_namedSelectionSets.ContainsKey(setName))
                {
                    MessageBox.Show($"选择集 '{setName}' 已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var selectionInfo = new SelectionSetInfo(setName)
                {
                    ObjectIds = new List<ObjectId>(_currentSelectionSet),
                    Description = description,
                    CenterPoint = CalculateCenterPoint(_currentSelectionSet)
                };

                _namedSelectionSets[setName] = selectionInfo;
                _currentSetName = setName;

                PluginEntry.Log($"选择集 '{setName}' 已从当前选择创建，包含 {_currentSelectionSet.Count} 个对象");
                return true;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"从当前选择创建选择集失败: {ex.Message}");
                MessageBox.Show($"从当前选择创建选择集失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 获取命名选择集
        /// </summary>
        /// <param name="setName">选择集名称</param>
        /// <returns>选择集信息</returns>
        public SelectionSetInfo GetNamedSelectionSet(string setName)
        {
            if (_namedSelectionSets.ContainsKey(setName))
            {
                return _namedSelectionSets[setName];
            }
            return null;
        }

        /// <summary>
        /// 获取所有命名选择集
        /// </summary>
        /// <returns>命名选择集字典</returns>
        public Dictionary<string, SelectionSetInfo> GetAllNamedSelectionSets()
        {
            return new Dictionary<string, SelectionSetInfo>(_namedSelectionSets);
        }

        /// <summary>
        /// 设置当前选择集
        /// </summary>
        /// <param name="setName">选择集名称</param>
        /// <returns>是否成功设置</returns>
        public bool SetCurrentSelectionSet(string setName)
        {
            try
            {
                if (!_namedSelectionSets.ContainsKey(setName))
                {
                    MessageBox.Show($"选择集 '{setName}' 不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var selectionInfo = _namedSelectionSets[setName];
                _currentSetName = setName;
                _currentSelectionSet = new List<ObjectId>(selectionInfo.ObjectIds);

                PluginEntry.Log($"当前选择集已设置为: {setName}");
                return true;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"设置当前选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除命名选择集
        /// </summary>
        /// <param name="setName">选择集名称</param>
        /// <returns>是否成功删除</returns>
        public bool DeleteSelectionSet(string setName)
        {
            try
            {
                if (!_namedSelectionSets.ContainsKey(setName))
                {
                    MessageBox.Show($"选择集 '{setName}' 不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 如果删除的是当前选择集，清除当前选择
                if (_currentSetName == setName)
                {
                    _currentSetName = string.Empty;
                    _currentSelectionSet.Clear();
                }

                _namedSelectionSets.Remove(setName);
                PluginEntry.Log($"选择集 '{setName}' 已删除");
                return true;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"删除选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 重命名选择集
        /// </summary>
        /// <param name="oldName">旧名称</param>
        /// <param name="newName">新名称</param>
        /// <returns>是否成功重命名</returns>
        public bool RenameSelectionSet(string oldName, string newName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("新名称不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!_namedSelectionSets.ContainsKey(oldName))
                {
                    MessageBox.Show($"选择集 '{oldName}' 不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (_namedSelectionSets.ContainsKey(newName))
                {
                    MessageBox.Show($"选择集 '{newName}' 已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var selectionInfo = _namedSelectionSets[oldName];
                selectionInfo.Name = newName;
                _namedSelectionSets[newName] = selectionInfo;
                _namedSelectionSets.Remove(oldName);

                // 如果重命名的是当前选择集，更新当前选择集名称
                if (_currentSetName == oldName)
                {
                    _currentSetName = newName;
                }

                PluginEntry.Log($"选择集 '{oldName}' 已重命名为 '{newName}'");
                return true;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"重命名选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 添加对象到当前选择集
        /// </summary>
        /// <param name="objectIds">对象ID列表</param>
        /// <returns>是否成功添加</returns>
        public bool AddToCurrentSelectionSet(List<ObjectId> objectIds)
        {
            try
            {
                if (objectIds == null || objectIds.Count == 0)
                {
                    return false;
                }

                _currentSelectionSet.AddRange(objectIds);
                _currentSelectionSet = _currentSelectionSet.Distinct().ToList(); // 去重

                // 更新当前选择集的命名信息
                if (!string.IsNullOrEmpty(_currentSetName) && _namedSelectionSets.ContainsKey(_currentSetName))
                {
                    var selectionInfo = _namedSelectionSets[_currentSetName];
                    selectionInfo.ObjectIds = new List<ObjectId>(_currentSelectionSet);
                    selectionInfo.CenterPoint = CalculateCenterPoint(_currentSelectionSet);
                }

                PluginEntry.Log($"已添加 {objectIds.Count} 个对象到当前选择集");
                return true;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"添加对象到当前选择集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从当前选择集移除对象
        /// </summary>
        /// <param name="objectIds">对象ID列表</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveFromCurrentSelectionSet(List<ObjectId> objectIds)
        {
            try
            {
                if (objectIds == null || objectIds.Count == 0)
                {
                    return false;
                }

                foreach (var objId in objectIds)
                {
                    _currentSelectionSet.Remove(objId);
                }

                // 更新当前选择集的命名信息
                if (!string.IsNullOrEmpty(_currentSetName) && _namedSelectionSets.ContainsKey(_currentSetName))
                {
                    var selectionInfo = _namedSelectionSets[_currentSetName];
                    selectionInfo.ObjectIds = new List<ObjectId>(_currentSelectionSet);
                    selectionInfo.CenterPoint = CalculateCenterPoint(_currentSelectionSet);
                }

                PluginEntry.Log($"已从当前选择集移除 {objectIds.Count} 个对象");
                return true;
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"从当前选择集移除对象失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清空当前选择集
        /// </summary>
        public void ClearCurrentSelectionSet()
        {
            _currentSelectionSet.Clear();
            _currentSetName = string.Empty;
            PluginEntry.Log("当前选择集已清空");
        }

        /// <summary>
        /// 获取当前选择集对象ID列表
        /// </summary>
        /// <returns>对象ID列表</returns>
        public List<ObjectId> GetCurrentSelectionSet()
        {
            return new List<ObjectId>(_currentSelectionSet);
        }

        /// <summary>
        /// 获取当前选择集名称
        /// </summary>
        /// <returns>选择集名称</returns>
        public string GetCurrentSelectionSetName()
        {
            return _currentSetName;
        }

        /// <summary>
        /// 获取选择集统计信息
        /// </summary>
        /// <param name="setName">选择集名称</param>
        /// <returns>统计信息</returns>
        public SelectionSetStatistics GetSelectionSetStatistics(string setName = null)
        {
            var stats = new SelectionSetStatistics();

            try
            {
                List<ObjectId> objectIds;
                if (string.IsNullOrEmpty(setName) || setName == _currentSetName)
                {
                    objectIds = _currentSelectionSet;
                }
                else if (_namedSelectionSets.ContainsKey(setName))
                {
                    objectIds = _namedSelectionSets[setName].ObjectIds;
                }
                else
                {
                    return stats;
                }

                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return stats;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    var typeCounts = new Dictionary<string, int>();
                    var layerCounts = new Dictionary<string, int>();

                    foreach (var objId in objectIds)
                    {
                        try
                        {
                            var entity = transaction.GetObject(objId, OpenMode.ForRead) as Entity;
                            if (entity != null)
                            {
                                // 统计类型
                                var typeName = entity.GetType().Name;
                                typeCounts[typeName] = typeCounts.ContainsKey(typeName) ? typeCounts[typeName] + 1 : 1;

                                // 统计图层
                                var layerName = entity.Layer;
                                layerCounts[layerName] = layerCounts.ContainsKey(layerName) ? layerCounts[layerName] + 1 : 1;

                                stats.TotalCount++;
                            }
                        }
                        catch { /* 忽略单个对象错误 */ }
                    }

                    stats.TypeStatistics = typeCounts;
                    stats.LayerStatistics = layerCounts;
                    stats.CenterPoint = CalculateCenterPoint(objectIds);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"获取选择集统计信息失败: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// 创建选择选项
        /// </summary>
        /// <returns>选择选项</returns>
        private PromptSelectionOptions CreateSelectionOptions()
        {
            return new PromptSelectionOptions
            {
                MessageForAdding = "\n选择对象创建选择集 [窗口(W)/交叉(C)]: ",
                AllowDuplicates = false,
                AllowSubSelections = true,
                SinglePickInSpace = false,
                RejectPaperSpaceEntities = true
            };
        }

        /// <summary>
        /// 计算对象集合的中心点
        /// </summary>
        /// <param name="objectIds">对象ID列表</param>
        /// <returns>中心点</returns>
        private Point3d CalculateCenterPoint(List<ObjectId> objectIds)
        {
            if (objectIds == null || objectIds.Count == 0)
            {
                return Point3d.Origin;
            }

            try
            {
                var doc = PluginEntry.GetActiveDocument();
                if (doc?.Database == null) return Point3d.Origin;

                double sumX = 0, sumY = 0, sumZ = 0;
                int count = 0;

                using (var transaction = doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (var objId in objectIds)
                    {
                        try
                        {
                            var entity = transaction.GetObject(objId, OpenMode.ForRead) as Entity;
                            if (entity != null)
                            {
                                var bounds = entity.Bounds;
                                if (bounds.HasValue)
                                {
                                    var center = (bounds.Value.MinPoint + bounds.Value.MaxPoint) / 2;
                                    sumX += center.X;
                                    sumY += center.Y;
                                    sumZ += center.Z;
                                    count++;
                                }
                            }
                        }
                        catch { /* 忽略单个对象错误 */ }
                    }
                }

                if (count > 0)
                {
                    return new Point3d(sumX / count, sumY / count, sumZ / count);
                }
            }
            catch (System.Exception ex)
            {
                PluginEntry.Log($"计算中心点失败: {ex.Message}");
            }

            return Point3d.Origin;
        }

        /// <summary>
        /// 显示选择集管理对话框
        /// </summary>
        /// <returns>对话框结果</returns>
        public DialogResult ShowSelectionSetManagerDialog()
        {
            using (var dialog = new SelectionSetManagerDialog(this))
            {
                return dialog.ShowDialog();
            }
        }
    }

    /// <summary>
    /// 选择集统计信息
    /// </summary>
    public class SelectionSetStatistics
    {
        public int TotalCount { get; set; }
        public Dictionary<string, int> TypeStatistics { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> LayerStatistics { get; set; } = new Dictionary<string, int>();
        public Point3d CenterPoint { get; set; }

        public string GetStatisticsText()
        {
            var lines = new List<string>
            {
                $"总计: {TotalCount} 个对象",
                $"中心点: ({CenterPoint.X:F1}, {CenterPoint.Y:F1})"
            };

            if (TypeStatistics.Count > 0)
            {
                lines.Add("\n类型统计:");
                foreach (var kvp in TypeStatistics)
                {
                    lines.Add($"  {kvp.Key}: {kvp.Value}");
                }
            }

            if (LayerStatistics.Count > 0)
            {
                lines.Add("\n图层统计:");
                foreach (var kvp in LayerStatistics.Take(5)) // 只显示前5个
                {
                    lines.Add($"  {kvp.Key}: {kvp.Value}");
                }
                if (LayerStatistics.Count > 5)
                {
                    lines.Add($"  ... 还有 {LayerStatistics.Count - 5} 个图层");
                }
            }

            return string.Join("\n", lines);
        }
    }

    /// <summary>
    /// 选择集管理对话框
    /// </summary>
    public class SelectionSetManagerDialog : Form
    {
        private SelectionSetManager _selectionSetManager;
        private ListBox _selectionSetListBox;
        private TextBox _selectionSetNameTextBox;
        private TextBox _descriptionTextBox;
        private Button _createButton;
        private Button _deleteButton;
        private Button _renameButton;
        private Button _selectButton;
        private Button _closeButton;

        public SelectionSetManagerDialog(SelectionSetManager selectionSetManager)
        {
            _selectionSetManager = selectionSetManager;
            InitializeDialog();
            RefreshSelectionSetList();
        }

        private void InitializeDialog()
        {
            this.Text = "选择集管理器";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 选择集列表
            var listLabel = new Label
            {
                Text = "选择集列表:",
                Location = new Point(20, 20),
                Size = new Size(100, 23)
            };
            this.Controls.Add(listLabel);

            _selectionSetListBox = new ListBox
            {
                Location = new Point(20, 45),
                Size = new Size(200, 200),
                SelectionMode = SelectionMode.One
            };
            _selectionSetListBox.SelectedIndexChanged += SelectionSetListBox_SelectedIndexChanged;
            this.Controls.Add(_selectionSetListBox);

            // 名称输入
            var nameLabel = new Label
            {
                Text = "名称:",
                Location = new Point(250, 45),
                Size = new Size(50, 23)
            };
            this.Controls.Add(nameLabel);

            _selectionSetNameTextBox = new TextBox
            {
                Location = new Point(310, 42),
                Size = new Size(150, 23)
            };
            this.Controls.Add(_selectionSetNameTextBox);

            // 描述输入
            var descLabel = new Label
            {
                Text = "描述:",
                Location = new Point(250, 75),
                Size = new Size(50, 23)
            };
            this.Controls.Add(descLabel);

            _descriptionTextBox = new TextBox
            {
                Location = new Point(310, 72),
                Size = new Size(150, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(_descriptionTextBox);

            // 按钮
            _createButton = new Button
            {
                Text = "创建(&C)",
                Location = new Point(250, 150),
                Size = new Size(75, 23)
            };
            _createButton.Click += CreateButton_Click;
            this.Controls.Add(_createButton);

            _deleteButton = new Button
            {
                Text = "删除(&D)",
                Location = new Point(335, 150),
                Size = new Size(75, 23),
                Enabled = false
            };
            _deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(_deleteButton);

            _renameButton = new Button
            {
                Text = "重命名(&R)",
                Location = new Point(250, 180),
                Size = new Size(75, 23),
                Enabled = false
            };
            _renameButton.Click += RenameButton_Click;
            this.Controls.Add(_renameButton);

            _selectButton = new Button
            {
                Text = "选择(&S)",
                Location = new Point(335, 180),
                Size = new Size(75, 23),
                Enabled = false
            };
            _selectButton.Click += SelectButton_Click;
            this.Controls.Add(_selectButton);

            _closeButton = new Button
            {
                Text = "关闭(&X)",
                Location = new Point(250, 250),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_closeButton);
        }

        private void RefreshSelectionSetList()
        {
            _selectionSetListBox.Items.Clear();
            var selectionSets = _selectionSetManager.GetAllNamedSelectionSets();
            foreach (var kvp in selectionSets)
            {
                _selectionSetListBox.Items.Add($"{kvp.Key} ({kvp.Value.Count})");
            }
        }

        private void SelectionSetListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = _selectionSetListBox.SelectedItem as string;
            if (selectedItem != null)
            {
                var name = selectedItem.Split('(')[0].Trim();
                var selectionSet = _selectionSetManager.GetNamedSelectionSet(name);
                if (selectionSet != null)
                {
                    _selectionSetNameTextBox.Text = selectionSet.Name;
                    _descriptionTextBox.Text = selectionSet.Description;
                    _deleteButton.Enabled = true;
                    _renameButton.Enabled = true;
                    _selectButton.Enabled = true;
                }
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            var name = _selectionSetNameTextBox.Text.Trim();
            var description = _descriptionTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("请输入选择集名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectionSetManager.CreateFromCurrentSelection(name, description))
            {
                RefreshSelectionSetList();
                MessageBox.Show("选择集创建成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var selectedItem = _selectionSetListBox.SelectedItem as string;
            if (selectedItem != null)
            {
                var name = selectedItem.Split('(')[0].Trim();
                if (MessageBox.Show($"确定要删除选择集 '{name}' 吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_selectionSetManager.DeleteSelectionSet(name))
                    {
                        RefreshSelectionSetList();
                        _selectionSetNameTextBox.Clear();
                        _descriptionTextBox.Clear();
                        _deleteButton.Enabled = false;
                        _renameButton.Enabled = false;
                        _selectButton.Enabled = false;
                    }
                }
            }
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            var selectedItem = _selectionSetListBox.SelectedItem as string;
            if (selectedItem != null)
            {
                var oldName = selectedItem.Split('(')[0].Trim();
                var newName = _selectionSetNameTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(newName))
                {
                    MessageBox.Show("请输入新名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_selectionSetManager.RenameSelectionSet(oldName, newName))
                {
                    RefreshSelectionSetList();
                }
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            var selectedItem = _selectionSetListBox.SelectedItem as string;
            if (selectedItem != null)
            {
                var name = selectedItem.Split('(')[0].Trim();
                _selectionSetManager.SetCurrentSelectionSet(name);
                MessageBox.Show($"当前选择集已设置为: {name}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}