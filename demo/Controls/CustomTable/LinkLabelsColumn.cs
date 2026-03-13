using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 链接标签信息
    /// </summary>
    public class LinkLabelInfo
    {
        /// <summary>
        /// 标签文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 标签值（用于标识点击的是哪个标签）
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        public LinkLabelInfo() { }

        public LinkLabelInfo(string text, string value = null)
        {
            Text = text;
            Value = value ?? text;
        }

        public LinkLabelInfo(string text, string value, bool visible)
        {
            Text = text;
            Value = value ?? text;
            Visible = visible;
        }
    }

    /// <summary>
    /// 链接标签列（支持一个单元格中显示多个类似链接的label）
    /// </summary>
    public class LinkLabelsColumn : CustomColumn
    {
        /// <summary>
        /// 默认链接颜色
        /// </summary>
        public Color LinkColor { get; set; } = Color.FromArgb(93, 128, 243); // #5D80F3

        /// <summary>
        /// 悬停时的链接颜色
        /// </summary>
        public Color HoverLinkColor { get; set; } = Color.FromArgb(73, 108, 223); // 稍深的颜色

        /// <summary>
        /// 标签之间的间距
        /// </summary>
        public int LabelSpacing { get; set; } = 10;

        /// <summary>
        /// 标签内边距
        /// </summary>
        public int LabelPadding { get; set; } = 4;

        /// <summary>
        /// 悬停时是否显示下划线
        /// </summary>
        public bool ShowUnderlineOnHover { get; set; } = true;

        /// <summary>
        /// 标签点击事件
        /// </summary>
        public event EventHandler<LinkLabelClickEventArgs> LinkLabelClick;

        /// <summary>
        /// 当前悬停的标签索引（-1表示没有）
        /// </summary>
        internal int HoveredLabelIndex { get; set; } = -1;

        /// <summary>
        /// 当前悬停的标签所在行索引
        /// </summary>
        internal int HoveredRowIndex { get; set; } = -1;

        public LinkLabelsColumn(string key, string title) : base(key, title)
        {
            Width = 150;
            Align = HorizontalAlignment.Left;
            ReadOnly = true;
        }

        public LinkLabelsColumn(string key, string title, int width) : base(key, title, width)
        {
            Align = HorizontalAlignment.Left;
            ReadOnly = true;
        }

        /// <summary>
        /// 触发标签点击事件
        /// </summary>
        internal void OnLinkLabelClick(int rowIndex, int columnIndex, LinkLabelInfo labelInfo)
        {
            LinkLabelClick?.Invoke(this, new LinkLabelClickEventArgs(rowIndex, columnIndex, this, labelInfo));
        }
    }

    /// <summary>
    /// 链接标签点击事件参数
    /// </summary>
    public class LinkLabelClickEventArgs : EventArgs
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public LinkLabelsColumn Column { get; set; }
        public LinkLabelInfo LabelInfo { get; set; }

        public LinkLabelClickEventArgs(int rowIndex, int columnIndex, LinkLabelsColumn column, LinkLabelInfo labelInfo)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            Column = column;
            LabelInfo = labelInfo;
        }
    }

    /// <summary>
    /// 链接标签渲染器
    /// </summary>
    internal class LinkLabelsRenderer
    {
        private const int DefaultFontSize = 9;

        /// <summary>
        /// 绘制链接标签列
        /// </summary>
        /// <param name="g">Graphics对象</param>
        /// <param name="rect">单元格区域</param>
        /// <param name="cellValue">单元格值（List&lt;LinkLabelInfo&gt;、LinkLabelInfo[]、IEnumerable&lt;LinkLabelInfo&gt;或字符串）</param>
        /// <param name="column">列对象</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="isSelected">是否选中</param>
        /// <param name="hoveredLabelIndex">当前悬停的标签索引（-1表示没有）</param>
        /// <returns>悬停的标签索引，没有则为-1</returns>
        public static int DrawLinkLabels(Graphics g, Rectangle rect, object cellValue, LinkLabelsColumn column, int rowIndex, bool isSelected, int hoveredLabelIndex = -1)
        {
            if (cellValue == null)
            {
                return -1;
            }

            List<LinkLabelInfo> labels = new List<LinkLabelInfo>();

            // 支持多种数据格式
            if (cellValue is List<LinkLabelInfo> list)
            {
                labels = list;
            }
            else if (cellValue is LinkLabelInfo[] array)
            {
                labels = new List<LinkLabelInfo>(array);
            }
            else if (cellValue is IEnumerable<LinkLabelInfo> enumerable)
            {
                labels = new List<LinkLabelInfo>(enumerable);
            }
            else if (cellValue is string str)
            {
                // 字符串格式：支持 "标签1|值1,标签2|值2,标签3|值3" 或简单的 "标签1,标签2,标签3"
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var parts = str.Split(',');
                    foreach (var part in parts)
                    {
                        var labelParts = part.Split('|');
                        var text = labelParts[0].Trim();
                        var value = labelParts.Length > 1 ? labelParts[1].Trim() : text;
                        labels.Add(new LinkLabelInfo(text, value));
                    }
                }
            }

            if (labels.Count == 0)
            {
                return -1;
            }

            // 计算绘制位置（水平排列）
            float x = rect.X + column.LabelPadding;
            float y = rect.Y + (rect.Height - DefaultFontSize - 6) / 2;

            using (var font = new Font("Microsoft YaHei UI", DefaultFontSize))
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    var label = labels[i];
                    if (!label.Visible) continue;

                    // 测量文本大小
                    var textSize = g.MeasureString(label.Text, font);
                    var labelRect = new RectangleF(x, y, textSize.Width, textSize.Height + 4);

                    // 确定文字颜色
                    Color textColor;
                    bool showUnderline = false;
                    
                    if (!label.Enabled)
                    {
                        textColor = Color.FromArgb(153, 153, 153); // 禁用状态为灰色
                    }
                    else if (isSelected)
                    {
                        textColor = Color.White;
                    }
                    else
                    {
                        textColor = column.LinkColor;
                        // 检查是否悬停
                        if (hoveredLabelIndex == i && column.ShowUnderlineOnHover)
                        {
                            showUnderline = true;
                        }
                    }

                    // 绘制文字
                    using (var brush = new SolidBrush(textColor))
                    {
                        // 悬停时绘制下划线
                        if (showUnderline)
                        {
                            // 使用System.Windows.Forms.TextRenderer绘制带下划线的文字
                            var drawPoint = new PointF(x, y);
                            
                            // 先绘制文字
                            TextRenderer.DrawText(
                                g,
                                label.Text,
                                font,
                                new Point((int)drawPoint.X, (int)drawPoint.Y),
                                textColor);

                            // 然后在下绘制下划线
                            var underlineY = y + textSize.Height;
                            using (var pen = new Pen(textColor, 1))
                            {
                                g.DrawLine(pen, x, underlineY, x + textSize.Width, underlineY);
                            }
                        }
                        else
                        {
                            TextRenderer.DrawText(
                                g,
                                label.Text,
                                font,
                                new Point((int)x, (int)y),
                                textColor);
                        }
                    }

                    // 更新x位置
                    x += textSize.Width + column.LabelSpacing;
                }
            }
            
            return -1;
        }

        /// <summary>
        /// 获取指定位置的标签索引
        /// </summary>
        public static int GetLabelIndexAtPoint(Graphics g, Rectangle rect, object cellValue, LinkLabelsColumn column, Point mousePos)
        {
            if (cellValue == null) return -1;

            List<LinkLabelInfo> labels = new List<LinkLabelInfo>();

            if (cellValue is List<LinkLabelInfo> list)
            {
                labels = list;
            }
            else if (cellValue is LinkLabelInfo[] array)
            {
                labels = new List<LinkLabelInfo>(array);
            }
            else if (cellValue is IEnumerable<LinkLabelInfo> enumerable)
            {
                labels = new List<LinkLabelInfo>(enumerable);
            }
            else if (cellValue is string str)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var parts = str.Split(',');
                    foreach (var part in parts)
                    {
                        var labelParts = part.Split('|');
                        var text = labelParts[0].Trim();
                        var value = labelParts.Length > 1 ? labelParts[1].Trim() : text;
                        labels.Add(new LinkLabelInfo(text, value));
                    }
                }
            }

            if (labels.Count == 0) return -1;

            float x = rect.X + column.LabelPadding;
            float y = rect.Y;
            float height = rect.Height;

            using (var font = new Font("Microsoft YaHei UI", DefaultFontSize))
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    var label = labels[i];
                    if (!label.Visible) continue;

                    var textSize = g.MeasureString(label.Text, font);
                    var labelRect = new RectangleF(x, y, textSize.Width, height);

                    if (labelRect.Contains(mousePos) && label.Enabled)
                    {
                        return i;
                    }

                    x += textSize.Width + column.LabelSpacing;
                }
            }

            return -1;
        }

        /// <summary>
        /// 获取指定位置的标签信息
        /// </summary>
        public static LinkLabelInfo GetLabelInfoAtPoint(Graphics g, Rectangle rect, object cellValue, LinkLabelsColumn column, Point mousePos, out int labelIndex)
        {
            labelIndex = GetLabelIndexAtPoint(g, rect, cellValue, column, mousePos);
            
            if (labelIndex < 0)
            {
                return null;
            }

            List<LinkLabelInfo> labels = new List<LinkLabelInfo>();

            if (cellValue is List<LinkLabelInfo> list)
            {
                labels = list;
            }
            else if (cellValue is LinkLabelInfo[] array)
            {
                labels = new List<LinkLabelInfo>(array);
            }
            else if (cellValue is IEnumerable<LinkLabelInfo> enumerable)
            {
                labels = new List<LinkLabelInfo>(enumerable);
            }
            else if (cellValue is string str)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var parts = str.Split(',');
                    foreach (var part in parts)
                    {
                        var labelParts = part.Split('|');
                        var text = labelParts[0].Trim();
                        var value = labelParts.Length > 1 ? labelParts[1].Trim() : text;
                        labels.Add(new LinkLabelInfo(text, value));
                    }
                }
            }

            if (labelIndex < labels.Count)
            {
                return labels[labelIndex];
            }

            return null;
        }
    }
}
