using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace demo.Controls
{
    public class IconLabelButton : Control
    {
        #region 颜色定义
        private Color _defaultBackColor = Color.White;
        private Color _defaultTextColor = ColorTranslator.FromHtml("#323232");
        private Color _hoverBackColor = ColorTranslator.FromHtml("#E6E6E6");
        private Color _hoverTextColor = ColorTranslator.FromHtml("#323232");
        private Color _selectedBackColor = ColorTranslator.FromHtml("#5D80F3");
        private Color _selectedTextColor = ColorTranslator.FromHtml("#FFFFFF");
        #endregion

        #region 字段
        private Image _icon;
        private string _text = "按钮";
        private bool _isHover;
        private bool _isSelected;
        private int _iconSize = 24;
        private int _iconTextSpacing = 4;
        #endregion

        #region 属性
        [Category("自定义属性")]
        [Description("图标图像")]
        public Image Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("图标大小")]
        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("图标与文字间距")]
        public int IconTextSpacing
        {
            get => _iconTextSpacing;
            set
            {
                _iconTextSpacing = value;
                Invalidate();
            }
        }

        public new string Text
        {
            get => _text;
            set
            {
                _text = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("是否选中")]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("正常背景色")]
        public Color DefaultBackColor2
        {
            get => _defaultBackColor;
            set
            {
                _defaultBackColor = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("正常文字颜色")]
        public Color DefaultTextColor2
        {
            get => _defaultTextColor;
            set
            {
                _defaultTextColor = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("悬停背景色")]
        public Color HoverBackColor
        {
            get => _hoverBackColor;
            set
            {
                _hoverBackColor = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("悬停文字颜色")]
        public Color HoverTextColor
        {
            get => _hoverTextColor;
            set
            {
                _hoverTextColor = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("选中背景色")]
        public Color SelectedBackColor
        {
            get => _selectedBackColor;
            set
            {
                _selectedBackColor = value;
                Invalidate();
            }
        }

        [Category("自定义属性")]
        [Description("选中文字颜色")]
        public Color SelectedTextColor
        {
            get => _selectedTextColor;
            set
            {
                _selectedTextColor = value;
                Invalidate();
            }
        }
        #endregion

        #region 构造函数
        public IconLabelButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Size = new Size(48, 43);
            Font = new Font("微软雅黑", 9f);
            BackColor = _defaultBackColor;
            ForeColor = _defaultTextColor;
            Cursor = Cursors.Hand;

            Margin = new Padding(0);
            Padding = new Padding(0);
        }
        #endregion

        #region 事件
        public event EventHandler Clicked;
        #endregion

        #region 重写方法
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHover = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color currentBackColor;
            Color currentTextColor;

            if (_isSelected)
            {
                currentBackColor = _selectedBackColor;
                currentTextColor = _selectedTextColor;
            }
            else if (_isHover)
            {
                currentBackColor = _hoverBackColor;
                currentTextColor = _hoverTextColor;
            }
            else
            {
                currentBackColor = _defaultBackColor;
                currentTextColor = _defaultTextColor;
            }

            using (SolidBrush backBrush = new SolidBrush(currentBackColor))
            {
                g.FillRectangle(backBrush, 0, 0, Width, Height);
            }

            int contentWidth = Math.Max(_iconSize, TextRenderer.MeasureText(_text, Font).Width);
            int startX = (Width - contentWidth) / 2;

            if (_icon != null)
            {
                int iconX = (Width - _iconSize) / 2;
                int iconY = 2;
                g.DrawImage(_icon, iconX, iconY, _iconSize, _iconSize);
            }

            using (SolidBrush textBrush = new SolidBrush(currentTextColor))
            {
                SizeF textSize = g.MeasureString(_text, Font);
                float textX = (Width - textSize.Width) / 2;
                float textY = 2 + _iconSize + _iconTextSpacing;
                g.DrawString(_text, Font, textBrush, textX, textY);
            }
        }
        #endregion

        #region 公共方法
        public void LoadIconFromFile(string iconPath)
        {
            try
            {
                if (System.IO.File.Exists(iconPath))
                {
                    Icon = Image.FromFile(iconPath);
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
