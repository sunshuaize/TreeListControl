using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 自定义表格列
    /// </summary>
    public class CustomColumn
    {
        /// <summary>
        /// 列键名
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 列标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 列宽度
        /// </summary>
        public int Width { get; set; } = 100;

        /// <summary>
        /// 对齐方式
        /// </summary>
        public HorizontalAlignment Align { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 自定义绘制事件
        /// </summary>
        public event EventHandler<CustomCellPaintEventArgs> CellPaint;

        internal void OnCellPaint(CustomCellPaintEventArgs e)
        {
            CellPaint?.Invoke(this, e);
        }

        public CustomColumn(string key, string title)
        {
            Key = key;
            Title = title;
        }

        public CustomColumn(string key, string title, int width)
        {
            Key = key;
            Title = title;
            Width = width;
        }
    }

    /// <summary>
    /// 自定义绘制单元格事件参数
    /// </summary>
    public class CustomCellPaintEventArgs : PaintEventArgs
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public object CellValue { get; set; }
        public Rectangle CellBounds { get; set; }
        public CustomColumn Column { get; set; }
        public bool Handled { get; set; }

        public CustomCellPaintEventArgs(Graphics graphics, Rectangle clipRect, int rowIndex, int columnIndex, object cellValue, Rectangle cellBounds, CustomColumn column)
            : base(graphics, clipRect)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            CellValue = cellValue;
            CellBounds = cellBounds;
            Column = column;
        }
    }
}
