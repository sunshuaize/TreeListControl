using System;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 单元格事件参数
    /// </summary>
    public class CustomCellEventArgs : EventArgs
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public CustomColumn Column { get; set; }
        public object CellValue { get; set; }

        public CustomCellEventArgs(int rowIndex, int columnIndex, CustomColumn column, object cellValue)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            Column = column;
            CellValue = cellValue;
        }
    }

    /// <summary>
    /// 选择改变事件参数
    /// </summary>
    public class CustomSelectionEventArgs : EventArgs
    {
        public int SelectedIndex { get; set; }

        public CustomSelectionEventArgs(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }

    /// <summary>
    /// 单元格值改变事件参数
    /// </summary>
    public class CustomCellValueChangedEventArgs : EventArgs
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public CustomColumn Column { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public CustomCellValueChangedEventArgs(int rowIndex, int columnIndex, CustomColumn column, object oldValue, object newValue)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            Column = column;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
