using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Linq;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 自定义表格控件
    /// </summary>
    [DefaultEvent("CellClick")]
    [DefaultProperty("Columns")]
    public partial class CustomTable : Control
    {
        #region 字段

        private readonly CustomColumnCollection _columns;
        private object _dataSource;
        private DataTable _dataTable;
        private int _headerHeight = 30;
        private int _rowHeight = 30;
        private int _selectedRowIndex = -1;
        private int _hoveredRowIndex = -1;
        private int _hoveredColumnIndex = -1;
        private int _editingRowIndex = -1;
        private int _editingColumnIndex = -1;
        private TextBox _editTextBox;
        private VScrollBar _vScrollBar;
        private HScrollBar _hScrollBar;
        private int _scrollOffset = 0;
        private int _horzScrollOffset = 0;
        private readonly List<int> _visibleRows = new List<int>();
        private readonly List<int> _visibleColumns = new List<int>();
        private readonly HashSet<int> _checkedRows = new HashSet<int>();
        private bool _showCheckBoxColumn = false;
        private bool? _headerCheckState = false;
        private int _lastClickedRow = -1;
        private int _shiftAnchorRow = -1;

        #endregion

        #region 构造函数

        public CustomTable()
        {
            _columns = new CustomColumnCollection(this);
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            BackColor = Color.White;
            ForeColor = Color.Black;
            Font = new Font("Microsoft YaHei UI", 9F);

            InitializeScrollBars();
            InitializeEditTextBox();
            UpdateCheckBoxColumn();
        }

        private void InitializeScrollBars()
        {
            _vScrollBar = new VScrollBar
            {
                Visible = false,
                Dock = DockStyle.Right
            };
            _vScrollBar.Scroll += VScrollBar_Scroll;
            Controls.Add(_vScrollBar);

            _hScrollBar = new HScrollBar
            {
                Visible = false,
                Dock = DockStyle.Bottom
            };
            _hScrollBar.Scroll += HScrollBar_Scroll;
            Controls.Add(_hScrollBar);
        }

        private void InitializeEditTextBox()
        {
            _editTextBox = new TextBox
            {
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _editTextBox.LostFocus += EditTextBox_LostFocus;
            _editTextBox.KeyDown += EditTextBox_KeyDown;
            Controls.Add(_editTextBox);
        }

        #endregion

        #region 属性

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("Data")]
        [Description("表格列集合")]
        public CustomColumnCollection Columns => _columns;

        [Category("Data")]
        [Description("数据源")]
        [DefaultValue(null)]
        public object DataSource
        {
            get => _dataSource;
            set
            {
                _dataSource = value;
                LoadData();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("表头高度")]
        [DefaultValue(30)]
        public int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                _headerHeight = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("行高度")]
        [DefaultValue(30)]
        public int RowHeight
        {
            get => _rowHeight;
            set
            {
                _rowHeight = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("选中行背景色")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color SelectedBackColor { get; set; } = Color.FromArgb(0, 120, 215);

        [Category("Appearance")]
        [Description("选中行文字颜色")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color SelectedForeColor { get; set; } = Color.White;

        [Category("Appearance")]
        [Description("悬停行背景色")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color HoverBackColor { get; set; } = Color.FromArgb(240, 240, 240);

        [Category("Appearance")]
        [Description("表头背景色")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color HeaderBackColor { get; set; } = Color.FromArgb(239, 240, 246);

        [Category("Appearance")]
        [Description("边框颜色")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color GridColor { get; set; } = Color.FromArgb(230, 230, 230);

        [Category("Appearance")]
        [Description("是否显示网格线")]
        [DefaultValue(true)]
        public bool ShowGridLines { get; set; } = false;

        [Category("Appearance")]
        [Description("是否显示复选框列")]
        [DefaultValue(false)]
        public bool ShowCheckBoxColumn
        {
            get => _showCheckBoxColumn;
            set
            {
                if (_showCheckBoxColumn == value) return;
                _showCheckBoxColumn = value;
                UpdateCheckBoxColumn();
                Invalidate();
            }
        }

        [Browsable(false)]
        public HashSet<int> CheckedRows => _checkedRows;

        [Category("Behavior")]
        [Description("是否允许编辑")]
        [DefaultValue(true)]
        public bool AllowEdit { get; set; } = true;

        [Category("Behavior")]
        [Description("是否允许选择行")]
        [DefaultValue(true)]
        public bool AllowSelection { get; set; } = true;

        [Category("Behavior")]
        [Description("整行选择")]
        [DefaultValue(true)]
        public bool FullRowSelect { get; set; } = true;

        #endregion

        #region 事件

        [Category("Action")]
        [Description("单元格点击事件")]
        public event EventHandler<CustomCellEventArgs> CellClick;

        [Category("Action")]
        [Description("单元格双击事件")]
        public event EventHandler<CustomCellEventArgs> CellDoubleClick;

        [Category("Action")]
        [Description("选中行改变事件")]
        public event EventHandler<CustomSelectionEventArgs> SelectedIndexChanged;

        [Category("Action")]
        [Description("单元格值改变事件")]
        public event EventHandler<CustomCellValueChangedEventArgs> CellValueChanged;

        #endregion

        #region 数据加载

        private void LoadData()
        {
            if (_dataSource == null)
            {
                _dataTable = null;
                return;
            }

            if (_dataSource is DataTable dataTable)
            {
                _dataTable = dataTable;
            }
            else if (_dataSource is System.Collections.IList list)
            {
                ConvertListToDataTable(list);
            }

            UpdateScrollBars();
        }

        private void ConvertListToDataTable(System.Collections.IList list)
        {
            _dataTable = new DataTable();

            if (list.Count == 0)
            {
                foreach (var column in _columns)
                {
                    _dataTable.Columns.Add(column.Key, typeof(string));
                }
                return;
            }

            var firstItem = list[0];
            var properties = firstItem.GetType().GetProperties();

            foreach (var prop in properties)
            {
                _dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in list)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item) ?? DBNull.Value;
                }
                _dataTable.Rows.Add(values);
            }
        }

        #endregion

        #region 渲染

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.AssumeLinear;

            DrawBackground(g);
            DrawHeader(g);
            DrawRows(g);
            DrawGridLines(g);
        }

        private void DrawBackground(Graphics g)
        {
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        private void DrawHeader(Graphics g)
        {
            if (!Columns.Any(c => c.Visible)) return;

            var clipRect = new Rectangle(0, 0, Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0), _headerHeight);
            g.SetClip(clipRect);

            using (var brush = new SolidBrush(HeaderBackColor))
            {
                g.FillRectangle(brush, clipRect);
            }

            var x = -_horzScrollOffset;
            foreach (var column in Columns.Where(c => c.Visible))
            {
                var rect = new Rectangle(x, 0, column.Width, _headerHeight);
                DrawHeaderCell(g, rect, column);
                x += column.Width;
            }

            g.ResetClip();
        }

        private void DrawHeaderCell(Graphics g, Rectangle rect, CustomColumn column)
        {
            using (var brush = new SolidBrush(HeaderBackColor))
            {
                g.FillRectangle(brush, rect);
            }

            if (ShowGridLines)
            {
                using (var pen = new Pen(GridColor))
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
            }

            if (column is CheckBoxColumn)
            {
                var checkState = _headerCheckState == true;
                CheckBoxRenderer.DrawCheckBox(g, rect, checkState);
                return;
            }

            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            if (column.Align == HorizontalAlignment.Center)
                format.Alignment = StringAlignment.Center;
            else if (column.Align == HorizontalAlignment.Right)
                format.Alignment = StringAlignment.Far;

            using (var brush = new SolidBrush(ForeColor))
            {
                var textRect = new Rectangle(rect.X + 4, rect.Y, rect.Width - 8, rect.Height);
                g.DrawString(column.Title, Font, brush, textRect, format);
            }

            format.Dispose();
        }

        private void DrawRows(Graphics g)
        {
            if (_dataTable == null || _dataTable.Rows.Count == 0) return;

            var visibleRowsArray = _visibleRows.ToArray();
            var y = _headerHeight;

            for (int i = 0; i < visibleRowsArray.Length; i++)
            {
                var rowIndex = visibleRowsArray[i];
                DrawRow(g, rowIndex, y);
                y += _rowHeight;
            }
        }

        private void DrawRow(Graphics g, int rowIndex, int y)
        {
            var rowRect = new Rectangle(0, y, Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0), _rowHeight);

            if (_checkedRows.Contains(rowIndex))
            {
                using (var brush = new SolidBrush(SelectedBackColor))
                {
                    g.FillRectangle(brush, rowRect);
                }
            }
            else if (rowIndex == _hoveredRowIndex)
            {
                using (var brush = new SolidBrush(HoverBackColor))
                {
                    g.FillRectangle(brush, rowRect);
                }
            }

            var x = -_horzScrollOffset;

            var visibleColumns = Columns.Where(c => c.Visible).ToArray();
            foreach (var column in visibleColumns)
            {
                var cellRect = new Rectangle(x, y, column.Width, _rowHeight);
                DrawCell(g, rowIndex, column, cellRect);
                x += column.Width;
            }
        }

        private void DrawCell(Graphics g, int rowIndex, CustomColumn column, Rectangle cellRect)
        {
            if (column is CheckBoxColumn)
            {
                var isChecked = _checkedRows.Contains(rowIndex);
                CheckBoxRenderer.DrawCheckBox(g, cellRect, isChecked);
                return;
            }

            if (column is StatusColumn)
            {
                var isSelected = _checkedRows.Contains(rowIndex);
                StatusRenderer.DrawStatus(g, cellRect, GetCellValue(rowIndex, column), isSelected);
                return;
            }

            // 处理LinkLabelsColumn
            if (column is LinkLabelsColumn linkLabelsColumn)
            {
                var isSelected = _checkedRows.Contains(rowIndex);
                var linkCellValue = GetCellValue(rowIndex, column);
                
                // 使用列中保存的悬停状态
                int hoveredLabelIndex = -1;
                if (linkLabelsColumn.HoveredRowIndex == rowIndex)
                {
                    hoveredLabelIndex = linkLabelsColumn.HoveredLabelIndex;
                }
                
                LinkLabelsRenderer.DrawLinkLabels(g, cellRect, linkCellValue, linkLabelsColumn, rowIndex, isSelected, hoveredLabelIndex);
                return;
            }

            var cellValue = GetCellValue(rowIndex, column);

            var columnIndex = -1;
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i] == column)
                {
                    columnIndex = i;
                    break;
                }
            }

            var args = new CustomCellPaintEventArgs(g, cellRect, rowIndex, columnIndex, cellValue, cellRect, column);
            column.OnCellPaint(args);

            if (!args.Handled)
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                if (column.Align == HorizontalAlignment.Center)
                    format.Alignment = StringAlignment.Center;
                else if (column.Align == HorizontalAlignment.Right)
                    format.Alignment = StringAlignment.Far;

                var textColor = _checkedRows.Contains(rowIndex) ? SelectedForeColor : ForeColor;
                using (var brush = new SolidBrush(textColor))
                {
                    var textRect = new Rectangle(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height);
                    var text = cellValue?.ToString() ?? "";
                    g.DrawString(text, Font, brush, textRect, format);
                }

                format.Dispose();
            }
        }

        private void DrawGridLines(Graphics g)
        {
            if (!ShowGridLines) return;

            using (var pen = new Pen(GridColor))
            {
                var width = Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0);
                var height = Height - (_hScrollBar.Visible ? _hScrollBar.Height : 0);

                var y = _headerHeight;
                var visibleRowsCount = _visibleRows.Count;
                for (int i = 0; i <= visibleRowsCount; i++)
                {
                    g.DrawLine(pen, 0, y, width, y);
                    y += _rowHeight;
                }

                var x = -_horzScrollOffset;
                var visibleColumns = Columns.Where(c => c.Visible).ToArray();
                foreach (var column in visibleColumns)
                {
                    x += column.Width;
                    g.DrawLine(pen, x, 0, x, height);
                }
            }
        }

        private object GetCellValue(int rowIndex, CustomColumn column)
        {
            if (_dataTable == null || rowIndex < 0 || rowIndex >= _dataTable.Rows.Count)
                return null;

            if (_dataTable.Columns.Contains(column.Key))
            {
                return _dataTable.Rows[rowIndex][column.Key];
            }

            return null;
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_vScrollBar.Visible)
            {
                int delta = e.Delta;
                int linesToScroll = Math.Abs(delta) / 120;
                if (linesToScroll == 0) linesToScroll = 1;

                int newValue = _vScrollBar.Value;
                if (delta > 0)
                {
                    newValue = Math.Max(_vScrollBar.Minimum, _vScrollBar.Value - linesToScroll * _vScrollBar.SmallChange);
                }
                else
                {
                    newValue = Math.Min(_vScrollBar.Maximum, _vScrollBar.Value + linesToScroll * _vScrollBar.SmallChange);
                }

                if (newValue != _vScrollBar.Value)
                {
                    _vScrollBar.Value = newValue;
                    _scrollOffset = newValue;
                    UpdateVisibleRows();

                    var contentRect = new Rectangle(
                        0,
                        _headerHeight,
                        Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0),
                        Height - _headerHeight - (_hScrollBar.Visible ? _hScrollBar.Height : 0)
                    );
                    Invalidate(contentRect);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var rowIndex = GetRowIndexFromPoint(e.Location);
            var columnIndex = GetColumnIndexFromPoint(e.Location);

            // 处理LinkLabelsColumn的悬停效果
            Cursor currentCursor = Cursors.Default;
            
            if (rowIndex >= 0 && columnIndex >= 0)
            {
                var column = Columns.Where(c => c.Visible).ElementAtOrDefault(columnIndex);
                if (column is LinkLabelsColumn linkLabelsColumn)
                {
                    var cellRect = GetCellRect(rowIndex, columnIndex);
                    var cellValue = GetCellValue(rowIndex, column);
                    
                    // 检查鼠标是否在某个标签上
                    var labelInfo = LinkLabelsRenderer.GetLabelInfoAtPoint(
                        CreateGraphics(), 
                        cellRect, 
                        cellValue, 
                        linkLabelsColumn, 
                        e.Location, 
                        out int labelIndex);
                    
                    if (labelInfo != null && labelInfo.Enabled)
                    {
                        currentCursor = Cursors.Hand;
                    }
                }
            }

            if (Cursor != currentCursor)
            {
                Cursor = currentCursor;
            }

            if (rowIndex != _hoveredRowIndex || columnIndex != _hoveredColumnIndex)
            {
                _hoveredRowIndex = rowIndex;
                _hoveredColumnIndex = columnIndex;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredRowIndex = -1;
            _hoveredColumnIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            var rowIndex = GetRowIndexFromPoint(e.Location);
            var columnIndex = GetColumnIndexFromPoint(e.Location);

            // 处理表头复选框点击
            if (rowIndex == -1 && columnIndex == 0 && _showCheckBoxColumn)
            {
                var column = Columns.Where(c => c.Visible).ElementAtOrDefault(0);
                if (column is CheckBoxColumn)
                {
                    ToggleHeaderCheck();
                    return;
                }
            }

            if (rowIndex >= 0 && columnIndex >= 0)
            {
                var column = Columns.Where(c => c.Visible).ElementAtOrDefault(columnIndex);

                // 处理行复选框点击 - 始终切换该行
                if (column is CheckBoxColumn)
                {
                    ToggleRowCheck(rowIndex);
                    _lastClickedRow = rowIndex;
                    _shiftAnchorRow = rowIndex;
                    return;
                }

                // 处理LinkLabelsColumn标签点击
                if (column is LinkLabelsColumn linkLabelsColumn)
                {
                    var cellRect = GetCellRect(rowIndex, columnIndex);
                    var cellValue = GetCellValue(rowIndex, column);
                    
                    var labelInfo = LinkLabelsRenderer.GetLabelInfoAtPoint(
                        CreateGraphics(),
                        cellRect,
                        cellValue,
                        linkLabelsColumn,
                        e.Location,
                        out int labelIndex);
                    
                    if (labelInfo != null && labelInfo.Enabled)
                    {
                        // 触发标签点击事件
                        linkLabelsColumn.OnLinkLabelClick(rowIndex, columnIndex, labelInfo);
                        return;
                    }
                }

                // 处理数据列点击 - 支持Ctrl和Shift
                if (AllowSelection && FullRowSelect)
                {
                    // Shift键：范围选择
                    if ((ModifierKeys & Keys.Shift) == Keys.Shift && _lastClickedRow >= 0)
                    {
                        ShiftSelectRows(rowIndex);
                    }
                    // Ctrl键：切换选中
                    else if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        ToggleRowCheck(rowIndex);
                        _lastClickedRow = rowIndex;
                        _shiftAnchorRow = rowIndex;
                    }
                    // 普通点击：只选中该行（清除其他）
                    else
                    {
                        _checkedRows.Clear();
                        _checkedRows.Add(rowIndex);
                        _lastClickedRow = rowIndex;
                        _shiftAnchorRow = rowIndex;
                        UpdateHeaderCheckState();
                    }

                    _selectedRowIndex = rowIndex;
                    Invalidate();
                    SelectedIndexChanged?.Invoke(this, new CustomSelectionEventArgs(rowIndex));
                }

                CellClick?.Invoke(this, new CustomCellEventArgs(rowIndex, columnIndex, column, GetCellValue(rowIndex, column)));
            }
        }

        private void ShiftSelectRows(int endRow)
        {
            // 确定Shift范围选择的起点
            int startRow;
            if (_shiftAnchorRow >= 0)
            {
                // 已经在进行Shift范围选择，使用锚点
                startRow = _shiftAnchorRow;
            }
            else if (_lastClickedRow >= 0)
            {
                // 第一次Shift+点击，使用上次点击的行作为起点和锚点
                startRow = _lastClickedRow;
                _shiftAnchorRow = _lastClickedRow;
            }
            else
            {
                startRow = 0;
            }

            int minRow = Math.Min(startRow, endRow);
            int maxRow = Math.Max(startRow, endRow);

            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                _checkedRows.Clear();
            }

            for (int i = minRow; i <= maxRow; i++)
            {
                _checkedRows.Add(i);
            }

            _lastClickedRow = endRow;
            UpdateHeaderCheckState();
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var rowIndex = GetRowIndexFromPoint(e.Location);
            var columnIndex = GetColumnIndexFromPoint(e.Location);

            if (rowIndex >= 0 && columnIndex >= 0)
            {
                var column = Columns.Where(c => c.Visible).ElementAtOrDefault(columnIndex);
                CellDoubleClick?.Invoke(this, new CustomCellEventArgs(rowIndex, columnIndex, column, GetCellValue(rowIndex, column)));

                if (AllowEdit && column != null && !column.ReadOnly)
                {
                    BeginEdit(rowIndex, columnIndex);
                }
            }
        }

        #endregion

        #region 编辑功能

        private void BeginEdit(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || columnIndex < 0) return;

            var column = Columns.Where(c => c.Visible).ElementAtOrDefault(columnIndex);
            if (column == null || column.ReadOnly) return;

            _editingRowIndex = rowIndex;
            _editingColumnIndex = columnIndex;

            var cellValue = GetCellValue(rowIndex, column);
            _editTextBox.Text = cellValue?.ToString() ?? "";

            var cellRect = GetCellRect(rowIndex, columnIndex);
            _editTextBox.Location = new Point(cellRect.X + 1, cellRect.Y + 1);
            _editTextBox.Size = new Size(cellRect.Width - 2, cellRect.Height - 2);
            _editTextBox.Visible = true;
            _editTextBox.Focus();
            _editTextBox.SelectAll();
        }

        private void EndEdit()
        {
            if (_editingRowIndex < 0 || _editingColumnIndex < 0) return;

            var column = Columns.Where(c => c.Visible).ElementAtOrDefault(_editingColumnIndex);
            if (column == null) return;

            var newValue = _editTextBox.Text;
            var oldValue = GetCellValue(_editingRowIndex, column);

            if (newValue != oldValue?.ToString())
            {
                if (_dataTable != null && _dataTable.Columns.Contains(column.Key))
                {
                    _dataTable.Rows[_editingRowIndex][column.Key] = newValue;
                    CellValueChanged?.Invoke(this, new CustomCellValueChangedEventArgs(_editingRowIndex, _editingColumnIndex, column, oldValue, newValue));
                }
            }

            _editTextBox.Visible = false;
            _editingRowIndex = -1;
            _editingColumnIndex = -1;
            Invalidate();
        }

        private void EditTextBox_LostFocus(object sender, EventArgs e)
        {
            EndEdit();
        }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EndEdit();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _editTextBox.Visible = false;
                _editingRowIndex = -1;
                _editingColumnIndex = -1;
            }
        }

        #endregion

        #region 滚动条

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            _scrollOffset = e.NewValue;
            UpdateVisibleRows();

            var contentRect = new Rectangle(
                0,
                _headerHeight,
                Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0),
                Height - _headerHeight - (_hScrollBar.Visible ? _hScrollBar.Height : 0)
            );
            Invalidate(contentRect);
        }

        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            _horzScrollOffset = e.NewValue;

            Invalidate();
        }

        private void UpdateScrollBars()
        {
            int rowCount = _dataTable?.Rows.Count ?? 0;
            int visibleRowCount = Math.Max(1, (Height - _headerHeight - (_hScrollBar.Visible ? _hScrollBar.Height : 0)) / _rowHeight);

            if (rowCount > visibleRowCount)
            {
                _vScrollBar.Visible = true;
                _vScrollBar.Minimum = 0;
                _vScrollBar.Maximum = rowCount - 1;
                _vScrollBar.LargeChange = visibleRowCount;
                _vScrollBar.SmallChange = 1;
            }
            else
            {
                _vScrollBar.Visible = false;
                _scrollOffset = 0;
            }

            int totalWidth = Columns.Where(c => c.Visible).Sum(c => c.Width);
            int visibleWidth = Math.Max(1, Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0));

            if (totalWidth > visibleWidth)
            {
                _hScrollBar.Visible = true;
                _hScrollBar.Minimum = 0;
                _hScrollBar.Maximum = totalWidth - 1;
                _hScrollBar.LargeChange = visibleWidth;
                _hScrollBar.SmallChange = _rowHeight;
            }
            else
            {
                _hScrollBar.Visible = false;
                _horzScrollOffset = 0;
            }

            UpdateVisibleRows();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBars();
            Invalidate();
        }

        private void UpdateVisibleRows()
        {
            _visibleRows.Clear();
            int rowCount = _dataTable?.Rows.Count ?? 0;
            if (rowCount == 0) return;

            int visibleRowCount = (Height - _headerHeight - (_hScrollBar.Visible ? _hScrollBar.Height : 0)) / _rowHeight;
            int endRow = Math.Min(_scrollOffset + visibleRowCount + 1, rowCount);

            _visibleRows.Capacity = Math.Max(_visibleRows.Capacity, visibleRowCount + 1);

            for (int i = _scrollOffset; i < endRow; i++)
            {
                _visibleRows.Add(i);
            }
        }

        #endregion

        #region 复选框列

        private void UpdateCheckBoxColumn()
        {
            var existingCheckBoxColumn = _columns.FirstOrDefault(c => c is CheckBoxColumn);

            if (_showCheckBoxColumn)
            {
                if (existingCheckBoxColumn == null)
                {
                    var checkBoxColumn = new CheckBoxColumn();
                    _columns.Insert(0, checkBoxColumn);
                }
            }
            else
            {
                if (existingCheckBoxColumn != null)
                {
                    _columns.Remove(existingCheckBoxColumn);
                }
                _checkedRows.Clear();
                _headerCheckState = false;
            }
        }

        private void ToggleRowCheck(int rowIndex)
        {
            if (_checkedRows.Contains(rowIndex))
            {
                _checkedRows.Remove(rowIndex);
            }
            else
            {
                _checkedRows.Add(rowIndex);
            }

            UpdateHeaderCheckState();
            Invalidate();
        }

        private void ToggleHeaderCheck()
        {
            if (_headerCheckState == true)
            {
                _checkedRows.Clear();
                _headerCheckState = false;
            }
            else
            {
                var rowCount = _dataTable?.Rows.Count ?? 0;
                for (int i = 0; i < rowCount; i++)
                {
                    _checkedRows.Add(i);
                }
                _headerCheckState = true;
            }

            Invalidate();
        }

        private void UpdateHeaderCheckState()
        {
            var rowCount = _dataTable?.Rows.Count ?? 0;
            if (rowCount == 0)
            {
                _headerCheckState = false;
                return;
            }

            if (_checkedRows.Count == 0)
            {
                _headerCheckState = false;
            }
            else if (_checkedRows.Count == rowCount)
            {
                _headerCheckState = true;
            }
            else
            {
                _headerCheckState = null;
            }
        }

        #endregion

        #region 辅助方法

        private int GetRowIndexFromPoint(Point point)
        {
            if (point.Y < _headerHeight) return -1;

            int relativeY = point.Y - _headerHeight;
            int rowIndex = _scrollOffset + (relativeY / _rowHeight);
            int rowCount = _dataTable?.Rows.Count ?? 0;

            return rowIndex < rowCount ? rowIndex : -1;
        }

        private int GetColumnIndexFromPoint(Point point)
        {
            var x = -_horzScrollOffset;
            int index = 0;

            foreach (var column in Columns.Where(c => c.Visible))
            {
                if (point.X >= x && point.X < x + column.Width)
                {
                    return index;
                }
                x += column.Width;
                index++;
            }

            return -1;
        }

        private Rectangle GetCellRect(int rowIndex, int columnIndex)
        {
            int y = _headerHeight + (rowIndex - _scrollOffset) * _rowHeight;
            int x = -_horzScrollOffset;

            foreach (var column in Columns.Where(c => c.Visible))
            {
                if (columnIndex == 0)
                {
                    return new Rectangle(x, y, column.Width, _rowHeight);
                }
                x += column.Width;
                columnIndex--;
            }

            return Rectangle.Empty;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editTextBox?.Dispose();
                _vScrollBar?.Dispose();
                _hScrollBar?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
