using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 复选框列
    /// </summary>
    public class CheckBoxColumn : CustomColumn
    {
        public CheckBoxColumn() : base("_CHECKBOX", "")
        {
            Width = 40;
            Align = HorizontalAlignment.Center;
            ReadOnly = true;
        }
    }

    /// <summary>
    /// 复选框渲染器
    /// </summary>
    internal class CheckBoxRenderer
    {
        public static void DrawCheckBox(Graphics g, Rectangle rect, bool checkedState, bool enabled = true)
        {
            var checkBoxSize = 16;
            var x = rect.X + (rect.Width - checkBoxSize) / 2;
            var y = rect.Y + (rect.Height - checkBoxSize) / 2;
            var checkBoxRect = new Rectangle(x, y, checkBoxSize, checkBoxSize);
            var cornerRadius = 3;

            // 绘制背景（圆角矩形）
            using (var backBrush = new SolidBrush(Color.White))
            {
                using (var path = CreateRoundedRectangle(checkBoxRect, cornerRadius))
                {
                    g.FillPath(backBrush, path);
                }
            }

            // 如果选中，绘制选中背景色
            if (checkedState)
            {
                using (var backBrush = new SolidBrush(Color.FromArgb(93, 128, 243)))
                {
                    using (var path = CreateRoundedRectangle(checkBoxRect, cornerRadius))
                    {
                        g.FillPath(backBrush, path);
                    }
                }
            }

            // 绘制边框（圆角矩形）
            using (var pen = new Pen(Color.FromArgb(226, 226, 226), 1))
            {
                using (var path = CreateRoundedRectangle(checkBoxRect, cornerRadius))
                {
                    g.DrawPath(pen, path);
                }
            }

            // 绘制勾选标记
            if (checkedState)
            {
                DrawCheckMark(g, checkBoxRect);
            }
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle rect, int cornerRadius)
        {
            var path = new GraphicsPath();

            if (cornerRadius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            var arcSize = cornerRadius * 2;

            // 左上角
            path.AddArc(rect.X, rect.Y, arcSize, arcSize, 180, 90);
            // 右上角
            path.AddArc(rect.Right - arcSize, rect.Y, arcSize, arcSize, 270, 90);
            // 右下角
            path.AddArc(rect.Right - arcSize, rect.Bottom - arcSize, arcSize, arcSize, 0, 90);
            // 左下角
            path.AddArc(rect.X, rect.Bottom - arcSize, arcSize, arcSize, 90, 90);

            path.CloseFigure();
            return path;
        }

        private static void DrawCheckMark(Graphics g, Rectangle rect)
        {
            using (var path = new GraphicsPath())
            {
                // 勾选标记的路径（缩进一点以适应圆角）
                var padding = 2;
                var points = new Point[]
                {
                    new Point(rect.X + 3 + padding, rect.Y + rect.Height / 2),
                    new Point(rect.X + rect.Width / 2 - 1, rect.Bottom - 4 - padding),
                    new Point(rect.Right - 3 - padding, rect.Y + 3 + padding)
                };

                path.AddLines(points);
                using (var pen = new Pen(Color.White, 2))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawPath(pen, path);
                    g.SmoothingMode = SmoothingMode.Default;
                }
            }
        }

        public static bool HitTestCheckBox(Rectangle cellRect, Point point)
        {
            var checkBoxSize = 16;
            var x = cellRect.X + (cellRect.Width - checkBoxSize) / 2;
            var y = cellRect.Y + (cellRect.Height - checkBoxSize) / 2;
            var checkBoxRect = new Rectangle(x, y, checkBoxSize, checkBoxSize);
            return checkBoxRect.Contains(point);
        }
    }
}
