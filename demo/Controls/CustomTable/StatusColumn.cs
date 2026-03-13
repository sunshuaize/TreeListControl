using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 状态类型
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// 成功（绿色）
        /// </summary>
        Success,

        /// <summary>
        /// 失败（红色）
        /// </summary>
        Error,

        /// <summary>
        /// 警告（橙色）
        /// </summary>
        Warning,

        /// <summary>
        /// 处理中（蓝色）
        /// </summary>
        Processing,

        /// <summary>
        /// 等待中（灰色）
        /// </summary>
        Pending,

        /// <summary>
        /// 信息（青色）
        /// </summary>
        Info
    }

    /// <summary>
    /// 状态列
    /// </summary>
    public class StatusColumn : CustomColumn
    {
        public StatusColumn(string key, string title) : base(key, title)
        {
            Width = 120;
            Align = HorizontalAlignment.Left;
            ReadOnly = true;
        }

        public StatusColumn(string key, string title, int width) : base(key, title, width)
        {
            Align = HorizontalAlignment.Left;
            ReadOnly = true;
        }
    }

    /// <summary>
    /// 状态渲染器
    /// </summary>
    internal class StatusRenderer
    {
        private const int DotSize = 8;
        private const int DotSpacing = 8;

        public static void DrawStatus(Graphics g, Rectangle rect, object cellValue, bool isSelected = false)
        {
            if (cellValue == null) return;

            StatusType status;
            string displayText;

            // 支持直接传入StatusType枚举或字符串
            if (cellValue is StatusType statusType)
            {
                status = statusType;
                displayText = GetStatusText(statusType);
            }
            else if (cellValue is string statusString)
            {
                status = ParseStatusType(statusString);
                displayText = statusString;
            }
            else
            {
                return;
            }

            var dotColor = GetStatusColor(status);
            var textColor = isSelected ? Color.White : Color.FromArgb(51, 51, 51);

            // 计算圆点位置
            var dotX = rect.X + 8;
            var dotY = rect.Y + (rect.Height - DotSize) / 2;
            var dotRect = new Rectangle(dotX, dotY, DotSize, DotSize);

            // 计算文字位置
            var textX = dotX + DotSize + DotSpacing;
            var textWidth = Math.Max(20, rect.Width - textX - 8);
            var textRect = new Rectangle(textX, rect.Y, textWidth, rect.Height);

            // 绘制圆点
            using (var brush = new SolidBrush(dotColor))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillEllipse(brush, dotRect);
                g.SmoothingMode = SmoothingMode.Default;
            }

            // 绘制文字
            try
            {
                using (var font = new Font("Microsoft YaHei UI", 9F))
                {
                    // 先测量文字大小
                    var textSize = g.MeasureString(displayText, font);

                    // 使用TextRenderer（更可靠的文字渲染）
                    TextRenderer.DrawText(
                        g,
                        displayText,
                        font,
                        new Point(textX, rect.Y + (rect.Height - font.Height) / 2),
                        textColor);
                }
            }
            catch
            {
                // 如果TextRenderer失败，使用传统方法
                using (var brush = new SolidBrush(textColor))
                using (var font = new Font("Microsoft YaHei UI", 9F))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };

                    g.DrawString(displayText, font, brush, textRect, format);
                    format.Dispose();
                }
            }
        }

        private static Color GetStatusColor(StatusType status)
        {
            switch (status)
            {
                case StatusType.Success:
                    return Color.FromArgb(82, 196, 26);
                case StatusType.Error:
                    return Color.FromArgb(255, 77, 79);
                case StatusType.Warning:
                    return Color.FromArgb(250, 173, 20);
                case StatusType.Processing:
                    return Color.FromArgb(24, 144, 255);
                case StatusType.Pending:
                    return Color.FromArgb(191, 191, 191);
                case StatusType.Info:
                    return Color.FromArgb(19, 194, 194);
                default:
                    return Color.FromArgb(191, 191, 191);
            }
        }

        private static string GetStatusText(StatusType status)
        {
            switch (status)
            {
                case StatusType.Success:
                    return "成功";
                case StatusType.Error:
                    return "失败";
                case StatusType.Warning:
                    return "警告";
                case StatusType.Processing:
                    return "处理中";
                case StatusType.Pending:
                    return "等待中";
                case StatusType.Info:
                    return "信息";
                default:
                    return "未知";
            }
        }

        private static StatusType ParseStatusType(string statusText)
        {
            if (string.IsNullOrWhiteSpace(statusText))
                return StatusType.Pending;

            switch (statusText.Trim())
            {
                case "成功":
                case "设计完成":
                case "已处理":
                    return StatusType.Success;
                case "失败":
                case "错误":
                    return StatusType.Error;
                case "警告":
                    return StatusType.Warning;
                case "处理中":
                case "设计中":
                case "待处理":
                    return StatusType.Processing;
                case "等待中":
                    return StatusType.Pending;
                case "信息":
                    return StatusType.Info;
                default:
                    return StatusType.Pending;
            }
        }
    }
}
