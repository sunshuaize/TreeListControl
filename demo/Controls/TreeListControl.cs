using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace demo.Controls
{
    /// <summary>
    /// 自定义树表格控件
    /// </summary>
    public class TreeListControl : Control
    {
        #region 节点数据结构

        public class TreeListNode
        {
            public string Name { get; set; }
            public string UploadTime { get; set; }
            public string RequiredTime { get; set; }
            public bool IsChecked { get; set; }
            public Image Icon { get; set; }
            public bool IsExpanded { get; set; }
            public bool IsVisible { get; set; } = true;
            public TreeListNode Parent { get; set; }
            public List<TreeListNode> Children { get; set; } = new List<TreeListNode>();
            public int Level => Parent == null ? 0 : Parent.Level + 1;
            public int Index { get; set; }  // 在同级节点中的索引
            public bool HasChildren => Children.Count > 0;
            public bool IsLastChild => Parent == null ? true : Parent.Children.Last() == this;
            public bool IsFirstChild => Parent == null ? true : Parent.Children.First() == this;

            public TreeListNode(string name)
            {
                Name = name;
                UploadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                RequiredTime = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");
            }

            public TreeListNode(string name, string uploadTime, string requiredTime)
            {
                Name = name;
                UploadTime = uploadTime;
                RequiredTime = requiredTime;
            }
        }

        public class TreeListColumn
        {
            public string Header { get; set; }
            public int Width { get; set; }
            public int MinWidth { get; set; } = 30;
            public bool Visible { get; set; } = true;
            public HorizontalAlignment TextAlign { get; set; } = HorizontalAlignment.Left;

            public TreeListColumn(string header, int width = 150)
            {
                Header = header;
                Width = width;
            }
        }

        #endregion 节点数据结构

        #region 字段

        private List<TreeListNode> _rootNodes = new List<TreeListNode>();
        private List<TreeListNode> _visibleNodes = new List<TreeListNode>();
        private List<TreeListColumn> _columns = new List<TreeListColumn>();
        private Image _folderIcon;
        private Image _fileIcon;
        private Image _folderIconGray;
        private Image _fileIconGray;
        private Image _collapseIcon;
        private Image _expandIcon;
        private int _rowHeight = 28;
        private int _headerHeight = 30;
        private int _indent = 35;
        private int _nodeWidth = 72;
        private int _checkboxSize = 16;
        private int _iconSize = 16;
        private int _totalColumnsWidth;
        private int _hScrollPos;
        private int _vScrollPos;
        private int _selectedIndex = -1;
        private TreeListNode _selectedNode;
        private bool _isDragging;
        private int _dragColumnIndex = -1;
        private int _dragStartX;
        private int _lastHitColumn = -1;
        private Point _lastMousePos;
        private ContextMenuStrip _contextMenu;

        #endregion 字段

        #region 属性

        public List<TreeListNode> Nodes => _rootNodes;
        public List<TreeListColumn> Columns => _columns;

        public int RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = value; Invalidate(); }
        }

        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = value; Invalidate(); }
        }

        public int Indent
        {
            get => _indent;
            set { _indent = value; Invalidate(); }
        }

        public Color BackColor2 { get; set; } = Color.FromArgb(245, 245, 245);
        public Color LineColor { get; set; } = Color.FromArgb(200, 200, 200);
        public Color SelectedColor { get; set; } = Color.FromArgb(239, 244, 255); // EFF4FF
        public Color HoverColor { get; set; } = Color.FromArgb(230, 240, 250);
        public Color HeaderBackColor { get; set; } = Color.FromArgb(239, 240, 246); // EFF0F6
        public Color HeaderTextColor { get; set; } = Color.FromArgb(61, 61, 61); // 3D3D3D
        public Color HeaderBorderColor { get; set; } = Color.FromArgb(180, 180, 180);
        public Color ExpandArrowColor { get; set; } = Color.FromArgb(156, 156, 156); // 折叠状态灰色
        public Color ExpandArrowExpandedColor { get; set; } = Color.FromArgb(63, 125, 243); // 展开状态蓝色
        public Color CheckBoxBorderColor { get; set; } = Color.FromArgb(235, 235, 235); // EBEBEB
        public Font HeaderFont { get; set; } = new Font("微软雅黑", 9F, FontStyle.Bold);
        public Font NodeFont { get; set; } = new Font("微软雅黑", 9F);

        /// <summary>
        /// 文件夹图标（用于有子节点的节点）
        /// </summary>
        public Image FolderIcon
        {
            get => _folderIcon;
            set { _folderIcon = value; Invalidate(); }
        }

        /// <summary>
        /// 文件图标（用于叶子节点）
        /// </summary>
        public Image FileIcon
        {
            get => _fileIcon;
            set { _fileIcon = value; Invalidate(); }
        }

        #endregion 属性

        #region 构造函数

        public TreeListControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            ForeColor = Color.FromArgb(50, 50, 50);

            // 初始化列
            _columns.Add(new TreeListColumn("名称", 250));
            _columns.Add(new TreeListColumn("上传时间", 150));
            _columns.Add(new TreeListColumn("要求完成时间", 150));

            // 自动适应列宽
            AutoFitColumns();

            // 加载默认图标
            LoadDefaultIcons();

            // 初始化右键菜单
            InitContextMenu();

            // 启用鼠标滚轮
            MouseWheel += OnMouseWheel;
        }

        /// <summary>
        /// 加载默认图标
        /// </summary>
        private void LoadDefaultIcons()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "folder_blue.png");
                if (File.Exists(iconPath))
                    _folderIcon = Image.FromFile(iconPath);

                iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "file_blue.png");
                if (File.Exists(iconPath))
                    _fileIcon = Image.FromFile(iconPath);

                iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "folder_gray.png");
                if (File.Exists(iconPath))
                    _folderIconGray = Image.FromFile(iconPath);

                iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "file_gray.png");
                if (File.Exists(iconPath))
                    _fileIconGray = Image.FromFile(iconPath);

                iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "collapse.png");
                if (File.Exists(iconPath))
                    _collapseIcon = Image.FromFile(iconPath);

                iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "expand.png");
                if (File.Exists(iconPath))
                    _expandIcon = Image.FromFile(iconPath);
            }
            catch
            {
                // 图标加载失败时忽略
            }
        }

        /// <summary>
        /// 自动适应列宽，使总宽度等于控件宽度
        /// </summary>
        /// <param name="defaultWidth">当控件宽度为0时的默认宽度</param>
        public void AutoFitColumns(int defaultWidth = 800)
        {
            if (_columns.Count == 0) return;

            int availableWidth = Width > 0 ? Width : defaultWidth;
            int totalMinWidth = _columns.Sum(c => c.MinWidth);

            if (availableWidth <= totalMinWidth)
            {
                // 如果控件宽度小于最小宽度，按最小宽度分配
                for (int i = 0; i < _columns.Count; i++)
                {
                    _columns[i].Width = _columns[i].MinWidth;
                }
            }
            else
            {
                // 按比例分配额外宽度
                int extraWidth = availableWidth - totalMinWidth;
                double totalRatio = _columns.Sum(c => (double)c.Width);

                for (int i = 0; i < _columns.Count; i++)
                {
                    double ratio = (double)_columns[i].Width / totalRatio;
                    _columns[i].Width = _columns[i].MinWidth + (int)(extraWidth * ratio);
                }
            }

            CalculateColumnsWidth();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AutoFitColumns();
            Invalidate();
        }

        private void InitContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            var menuItems = new[]
            {
                "发起校审", "发起会签", "图纸拆分", "参照图纸", "更新参照",
                "保存更新文件", "获取新版", "导出文件", "删除文件", "不签字打印",
                "生成图纸目录", "重命名文件", "信息查看", "文件移交", "打开文件位置"
            };

            foreach (var item in menuItems)
            {
                var menuItem = new ToolStripMenuItem(item);
                menuItem.Click += (s, e) => { /* 仅UI，无功能 */ };
                _contextMenu.Items.Add(menuItem);
            }
        }

        private void CalculateColumnsWidth()
        {
            _totalColumnsWidth = _columns.Sum(c => c.Width);
        }

        #endregion 构造函数

        #region 公开方法

        public void AddNode(TreeListNode node)
        {
            // 添加根节点
            _rootNodes.Add(node);
            RefreshVisibleNodes();
            Invalidate();
        }

        public void AddNode(TreeListNode parent, TreeListNode child)
        {
            child.Parent = parent;
            parent.Children.Add(child);
            // 子节点不添加到 _allNodes，只在父节点的 Children 中管理
            // 不在这里调用 RefreshVisibleNodes()，由用户手动调用或在设置展开状态后调用
            Invalidate();
        }

        public void Clear()
        {
            ClearNodeAndChildren(_rootNodes);
            _rootNodes.Clear();
            _visibleNodes.Clear();
            _selectedIndex = -1;
            _selectedNode = null;
            Invalidate();
        }

        private void ClearNodeAndChildren(List<TreeListNode> nodes)
        {
            foreach (var node in nodes)
            {
                ClearNodeAndChildren(node.Children);
                node.Children.Clear();
            }
        }

        public void ExpandAll()
        {
            foreach (var node in _rootNodes)
            {
                ExpandNode(node);
            }
            RefreshVisibleNodes();
            Invalidate();
        }

        public void CollapseAll()
        {
            foreach (var node in _rootNodes)
            {
                CollapseNode(node);
            }
            RefreshVisibleNodes();
            Invalidate();
        }

        private void ExpandNode(TreeListNode node)
        {
            node.IsExpanded = true;
            foreach (var child in node.Children)
            {
                ExpandNode(child);
            }
        }

        private void CollapseNode(TreeListNode node)
        {
            node.IsExpanded = false;
            foreach (var child in node.Children)
            {
                CollapseNode(child);
            }
        }

        public void RefreshVisibleNodes()
        {
            _visibleNodes.Clear();
            foreach (var root in _rootNodes)
            {
                AddVisibleNode(root);
            }
        }

        /// <summary>
        /// 递归设置节点及其所有子节点的选中状态
        /// </summary>
        private void SetNodeChecked(TreeListNode node, bool isChecked)
        {
            node.IsChecked = isChecked;
            foreach (var child in node.Children)
            {
                SetNodeChecked(child, isChecked);
            }
            
            // 如果是取消选中某个子节点，检查并更新父节点状态
            if (!isChecked && node.Parent != null)
            {
                UpdateParentCheckedState(node.Parent);
            }
        }

        /// <summary>
        /// 更新父节点的选中状态：如果子节点全部选中则父节点选中，否则取消选中
        /// </summary>
        private void UpdateParentCheckedState(TreeListNode parent)
        {
            if (parent == null) return;
            
            // 检查是否有任何子节点未选中
            bool anyChildUnchecked = parent.Children.Any(c => !c.IsChecked);
            parent.IsChecked = !anyChildUnchecked;
            
            // 递归更新祖父节点
            UpdateParentCheckedState(parent.Parent);
        }

        private void AddVisibleNode(TreeListNode node)
        {
            node.IsVisible = true;
            _visibleNodes.Add(node);

            // 按添加顺序排列子节点
            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    AddVisibleNode(child);
                }
            }
        }

        #endregion 公开方法

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 绘制表头
            DrawHeader(g);

            // 绘制行
            DrawRows(g);
        }

        private void DrawHeader(Graphics g)
        {
            int x = -_hScrollPos;
            for (int i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                if (!col.Visible) continue;

                // 背景
                using (var brush = new SolidBrush(HeaderBackColor))
                {
                    g.FillRectangle(brush, x, 0, col.Width, _headerHeight);
                }

                // 文本
                var textRect = new Rectangle(x + 5, 0, col.Width - 10, _headerHeight);
                using (var sf = new StringFormat { LineAlignment = StringAlignment.Center })
                {
                    using (var brush = new SolidBrush(HeaderTextColor))
                    {
                        g.DrawString(col.Header, HeaderFont, brush, textRect, sf);
                    }
                }

                x += col.Width;
            }
        }

        private void DrawRows(Graphics g)
        {
            int y = _headerHeight - _vScrollPos;
            for (int i = 0; i < _visibleNodes.Count; i++)
            {
                if (y > Height) break;
                if (y + _rowHeight < 0)
                {
                    y += _rowHeight;
                    continue;
                }

                var node = _visibleNodes[i];
                var rowRect = new Rectangle(0, y, Width, _rowHeight);

                // 选中行背景
                if (i == _selectedIndex)
                {
                    using (var brush = new SolidBrush(SelectedColor))
                    {
                        g.FillRectangle(brush, rowRect);
                    }
                }
                else if (rowRect.Contains(_lastMousePos))
                {
                    using (var brush = new SolidBrush(HoverColor))
                    {
                        g.FillRectangle(brush, rowRect);
                    }
                }

                DrawNode(g, node, y);

                y += _rowHeight;
            }
        }

        private void DrawNode(Graphics g, TreeListNode node, int y)
        {
            int x = -_hScrollPos;
            int startX = node.Level * _indent + 5;

            // 绘制第一列（名称）
            int col0Width = _columns[0].Width;

            // 基准位置：箭头 + checkbox + 图标 连续排列
            int arrowSize = 16;
            int currentX = startX;

            // 展开/折叠箭头
            if (node.HasChildren)
            {
                DrawExpandArrow(g, node, currentX, y);
                currentX += arrowSize + 6;
            }

            // Checkbox - 只有非一级节点才显示
            if (node.Level > 0)
            {
                DrawCheckbox(g, node, currentX, y, true);
                currentX += _checkboxSize + 10;
            }

            // 文件夹或文件图标 - 根据选中/悬停状态选择颜色
            Image displayIcon = node.Icon;
            if (displayIcon == null)
            {
                bool isSelected = _selectedIndex >= 0 && _selectedIndex < _visibleNodes.Count && _visibleNodes[_selectedIndex] == node;
                var rowRect = new Rectangle(0, y, Width, _rowHeight);
                bool isHover = rowRect.Contains(_lastMousePos);

                if (node.HasChildren)
                {
                    displayIcon = (isSelected || isHover) ? _folderIcon : _folderIconGray;
                }
                else
                {
                    displayIcon = (isSelected || isHover) ? _fileIcon : _fileIconGray;
                }
            }

            if (displayIcon != null)
            {
                int iconW = displayIcon.Width;
                int iconH = displayIcon.Height;
                g.DrawImage(displayIcon, currentX, y + (_rowHeight - iconH) / 2, iconW, iconH);
                currentX += iconW + 6;
            }

            // 节点文本
            var textRect = new Rectangle(currentX + 4, y, col0Width - currentX + x, _rowHeight);
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            {
                bool isSelected = _selectedIndex >= 0 && _selectedIndex < _visibleNodes.Count && _visibleNodes[_selectedIndex] == node;
                var textColor = isSelected ? Color.FromArgb(63, 125, 243) : ForeColor;
                using (var brush = new SolidBrush(textColor))
                {
                    g.DrawString(node.Name, NodeFont, brush, textRect, sf);
                }
            }

            x += col0Width;

            // 绘制其他列
            for (int i = 1; i < _columns.Count; i++)
            {
                var col = _columns[i];
                var cellRect = new Rectangle(x, y, col.Width, _rowHeight);

                string cellText = "";
                if (i == 1) cellText = node.UploadTime;
                else if (i == 2) cellText = node.RequiredTime;

                using (var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = col.TextAlign == HorizontalAlignment.Center ? StringAlignment.Center :
                               col.TextAlign == HorizontalAlignment.Right ? StringAlignment.Far : StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    bool isSelected = _selectedIndex >= 0 && _selectedIndex < _visibleNodes.Count && _visibleNodes[_selectedIndex] == node;
                    var textColor = isSelected ? Color.FromArgb(63, 125, 243) : ForeColor;
                    using (var brush = new SolidBrush(textColor))
                    {
                        var textRect2 = new Rectangle(x + 5, y, col.Width - 10, _rowHeight);
                        g.DrawString(cellText, NodeFont, brush, textRect2, sf);
                    }
                }

                x += col.Width;
            }
        }

        private void DrawExpandArrow(Graphics g, TreeListNode node, int x, int y)
        {
            // 使用图标
            const int arrowSize = 16;
            Image arrowIcon = node.IsExpanded ? _expandIcon : _collapseIcon;
            if (arrowIcon != null)
            {
                // 统一缩放到16x16
                g.DrawImage(arrowIcon, x, y + (_rowHeight - arrowSize) / 2, arrowSize, arrowSize);
            }
        }

        private void DrawCheckbox(Graphics g, TreeListNode node, int x, int y, bool isHover)
        {
            int checkX = x;
            int checkY = y + (_rowHeight - _checkboxSize) / 2;
            int radius = 3;

            // 绘制圆角checkbox背景（悬停时为白色）
            var rect = new Rectangle(checkX, checkY, _checkboxSize, _checkboxSize);
            using (var path = CreateRoundedRectangle(rect, radius))
            {
                var fillColor = isHover ? Color.White : Color.Transparent;
                if (fillColor != Color.Transparent)
                {
                    using (var brush = new SolidBrush(fillColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
                using (var pen = new Pen(CheckBoxBorderColor))
                {
                    g.DrawPath(pen, path);
                }
            }

            // 绘制勾选状态
            if (node.IsChecked)
            {
                var fillRect = new Rectangle(checkX + 1, checkY + 1, _checkboxSize - 2, _checkboxSize - 2);
                using (var path = CreateRoundedRectangle(fillRect, radius - 1))
                {
                    using (var brush = new SolidBrush(Color.FromArgb(51, 153, 255)))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // 绘制勾
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawLine(pen, checkX + 3, checkY + _checkboxSize / 2, checkX + 6, checkY + _checkboxSize - 4);
                    g.DrawLine(pen, checkX + 6, checkY + _checkboxSize - 4, checkX + 12, checkY + 3);
                }
            }
        }

        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void DrawColumnLines(Graphics g)
        {
            int y = _headerHeight;
            using (var pen = new Pen(HeaderBorderColor))
            {
                g.DrawLine(pen, 0, y, Width, y);
            }
        }

        #endregion 绘制

        #region 鼠标事件

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _lastMousePos = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                // 检查是否点击列宽调整区域
                int x = -_hScrollPos;
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (!_columns[i].Visible) continue;
                    if (Math.Abs(e.X - (x + _columns[i].Width)) < 5)
                    {
                        _isDragging = true;
                        _dragColumnIndex = i;
                        _dragStartX = e.X;
                        return;
                    }
                    x += _columns[i].Width;
                }

                // 检查是否点击节点
                int row = (e.Y - _headerHeight + _vScrollPos) / _rowHeight;
                if (row >= 0 && row < _visibleNodes.Count)
                {
                    int nodeX = e.X + _hScrollPos;
                    var node = _visibleNodes[row];
                    int startX = node.Level * _indent + 5;

                    // 点击展开/折叠箭头区域
                    if (node.HasChildren && nodeX >= startX && nodeX < startX + _nodeWidth * 2)
                    {
                        node.IsExpanded = !node.IsExpanded;
                        RefreshVisibleNodes();
                        Invalidate();
                        return;
                    }

                    // 鼠标悬停在该行上且是非一级节点，点击切换选中状态
                    int rowY = _headerHeight + row * _rowHeight - _vScrollPos;
                    if (node.Level > 0 && _lastMousePos.Y >= rowY && _lastMousePos.Y < rowY + _rowHeight)
                    {
                        SetNodeChecked(node, !node.IsChecked);
                        Invalidate();
                        return;
                    }

                    // 选中节点
                    _selectedIndex = row;
                    _selectedNode = node;
                    Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 右键菜单
                int row = (e.Y - _headerHeight + _vScrollPos) / _rowHeight;
                if (row >= 0 && row < _visibleNodes.Count)
                {
                    _selectedIndex = row;
                    _selectedNode = _visibleNodes[row];
                    _selectedNode.IsChecked = true; // 选中后自动勾选
                    Invalidate();
                }
                _contextMenu.Show(this, e.Location);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _lastMousePos = e.Location;

            if (_isDragging && _dragColumnIndex >= 0)
            {
                int delta = e.X - _dragStartX;
                int newWidth = _columns[_dragColumnIndex].Width + delta;
                if (newWidth >= _columns[_dragColumnIndex].MinWidth)
                {
                    _columns[_dragColumnIndex].Width = newWidth;
                    _dragStartX = e.X;
                    CalculateColumnsWidth();
                    Invalidate();
                }
            }
            else
            {
                // 改变光标
                int x = -_hScrollPos;
                bool isResize = false;
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (!_columns[i].Visible) continue;
                    if (Math.Abs(e.X - (x + _columns[i].Width)) < 5)
                    {
                        isResize = true;
                        break;
                    }
                    x += _columns[i].Width;
                }
                Cursor = isResize ? Cursors.SizeWE : Cursors.Default;

                // 重绘以显示hover效果
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
            _dragColumnIndex = -1;
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta > 0 ? -_rowHeight : _rowHeight;
            int maxScroll = _visibleNodes.Count * _rowHeight - (Height - _headerHeight);
            _vScrollPos = Math.Max(0, Math.Min(maxScroll, _vScrollPos + delta));
            Invalidate();
        }

        #endregion 鼠标事件

        #region 键盘事件

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_selectedNode == null) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (_selectedNode.IsExpanded)
                    {
                        _selectedNode.IsExpanded = false;
                        RefreshVisibleNodes();
                    }
                    else if (_selectedNode.Parent != null)
                    {
                        _selectedIndex = _visibleNodes.IndexOf(_selectedNode.Parent);
                        _selectedNode = _selectedNode.Parent;
                    }
                    Invalidate();
                    e.Handled = true;
                    break;

                case Keys.Right:
                    if (!_selectedNode.IsExpanded && _selectedNode.HasChildren)
                    {
                        _selectedNode.IsExpanded = true;
                        RefreshVisibleNodes();
                    }
                    else if (_selectedNode.Children.Count > 0)
                    {
                        _selectedIndex = _visibleNodes.IndexOf(_selectedNode.Children[0]);
                        _selectedNode = _selectedNode.Children[0];
                    }
                    Invalidate();
                    e.Handled = true;
                    break;

                case Keys.Space:
                    _selectedNode.IsChecked = !_selectedNode.IsChecked;
                    Invalidate();
                    e.Handled = true;
                    break;
            }
        }

        #endregion 键盘事件

        protected override bool IsInputKey(Keys key)
        {
            return key == Keys.Left || key == Keys.Right || key == Keys.Space || base.IsInputKey(key);
        }
    }
}