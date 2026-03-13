using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace demo.Controls
{
    public class CustomTabControl : Control
    {
        #region 颜色定义
        private Color _defaultBackColor = ColorTranslator.FromHtml("#F0F0F0");
        private Color _defaultTextColor = ColorTranslator.FromHtml("#323232");
        private Color _selectedBackColor = ColorTranslator.FromHtml("#5D80F3");
        private Color _selectedTextColor = ColorTranslator.FromHtml("#FFFFFF");
        private Color _defaultBorderColor = ColorTranslator.FromHtml("#CDCDCD");
        private Color _contentBackColor = Color.White;
        #endregion

        #region 属性
        private int _tabHeight = 30;
        private int _selectedIndex = -1;
        private int _borderWidth = 2;
        private List<TabPage> _tabPages = new List<TabPage>();

        public int TabHeight
        {
            get => _tabHeight;
            set
            {
                _tabHeight = value;
                Invalidate();
            }
        }

        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= -1 && value < _tabPages.Count)
                {
                    _selectedIndex = value;
                    UpdateTabPageVisibility();
                    Invalidate();
                }
            }
        }

        public TabPage SelectedTab
        {
            get => _selectedIndex >= 0 && _selectedIndex < _tabPages.Count ? _tabPages[_selectedIndex] : null;
            set
            {
                int index = _tabPages.IndexOf(value);
                if (index >= 0)
                {
                    SelectedIndex = index;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<TabPage> TabPages => _tabPages;
        #endregion

        #region 构造函数
        public CustomTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = _contentBackColor;
            Size = new Size(400, 300);
            Font = new Font("微软雅黑", 9f);
        }
        #endregion

        #region 公共方法
        public TabPage AddTab(string text)
        {
            TabPage tabPage = new TabPage(text);
            _tabPages.Add(tabPage);

            if (_selectedIndex == -1 && _tabPages.Count > 0)
            {
                _selectedIndex = 0;
            }

            UpdateTabPageVisibility();
            Invalidate();
            return tabPage;
        }

        public void RemoveTab(TabPage tabPage)
        {
            int index = _tabPages.IndexOf(tabPage);
            if (index >= 0)
            {
                _tabPages.RemoveAt(index);

                if (_selectedIndex >= _tabPages.Count)
                {
                    _selectedIndex = _tabPages.Count - 1;
                }

                UpdateTabPageVisibility();
                Invalidate();
            }
        }

        public void RemoveTabAt(int index)
        {
            if (index >= 0 && index < _tabPages.Count)
            {
                RemoveTab(_tabPages[index]);
            }
        }
        #endregion

        #region 布局相关
        protected override void OnLayout(System.Windows.Forms.LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            UpdateTabPageVisibility();
        }

        private void UpdateTabPageVisibility()
        {
            foreach (TabPage tabPage in _tabPages)
            {
                if (tabPage != null)
                {
                    tabPage.Visible = false;
                }
            }

            if (_selectedIndex >= 0 && _selectedIndex < _tabPages.Count)
            {
                TabPage selectedTab = _tabPages[_selectedIndex];
                selectedTab.Visible = true;
                selectedTab.Bounds = new Rectangle(0, _tabHeight, Width, Height - _tabHeight);
            }
        }
        #endregion

        #region 绘制相关
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawTabs(g);
        }

        private void DrawTabs(Graphics g)
        {
            if (_tabPages.Count == 0) return;

            int tabWidth = Width / _tabPages.Count;

            for (int i = 0; i < _tabPages.Count; i++)
            {
                bool isSelected = (i == _selectedIndex);
                bool isFirst = (i == 0);
                bool isLast = (i == _tabPages.Count - 1);
                Rectangle tabRect = new Rectangle(i * tabWidth - i * _borderWidth, 0, tabWidth + _borderWidth, _tabHeight);

                DrawTab(g, _tabPages[i].Text, tabRect, isSelected, isFirst, isLast);
            }

            DrawContentBorder(g);
        }

        private void DrawTab(Graphics g, string text, Rectangle bounds, bool isSelected, bool isFirst, bool isLast)
        {
            Color backColor = isSelected ? _selectedBackColor : _defaultBackColor;
            Color textColor = isSelected ? _selectedTextColor : _defaultTextColor;
            Color borderColor = isSelected ? _selectedBackColor : _defaultBorderColor;

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            using (Pen pen = new Pen(borderColor, _borderWidth))
            {
                g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Top);
                g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom);
                if (isLast)
                {
                    g.DrawLine(pen, bounds.Right, bounds.Top, bounds.Right, bounds.Bottom);
                }
            }

            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                SizeF textSize = g.MeasureString(text, Font);
                float x = bounds.X + (bounds.Width - textSize.Width) / 2;
                float y = bounds.Y + (bounds.Height - textSize.Height) / 2;
                g.DrawString(text, Font, textBrush, x, y);
            }
        }

        private void DrawContentBorder(Graphics g)
        {
            Rectangle contentRect = new Rectangle(0, _tabHeight, Width, Height - _tabHeight);
            using (Pen pen = new Pen(_defaultBorderColor, 1))
            {
                g.DrawRectangle(pen, contentRect.X, contentRect.Y, contentRect.Width - 1, contentRect.Height - 1);
            }
        }
        #endregion

        #region 鼠标事件
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Y <= _tabHeight && e.Y >= 0)
            {
                int tabWidth = Width / _tabPages.Count;
                int clickedIndex = e.X / tabWidth;

                if (clickedIndex >= 0 && clickedIndex < _tabPages.Count)
                {
                    SelectedIndex = clickedIndex;
                }
            }
        }
        #endregion
    }
}
