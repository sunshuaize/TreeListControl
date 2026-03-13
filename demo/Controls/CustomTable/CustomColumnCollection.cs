using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// 自定义列集合
    /// </summary>
    public class CustomColumnCollection : IList<CustomColumn>
    {
        private readonly List<CustomColumn> _columns = new List<CustomColumn>();
        private readonly CustomTable _table;

        public CustomColumnCollection(CustomTable table)
        {
            _table = table;
        }

        public CustomColumn this[int index]
        {
            get => _columns[index];
            set
            {
                _columns[index] = value;
                _table.Invalidate();
            }
        }

        public CustomColumn this[string key]
        {
            get
            {
                foreach (var column in _columns)
                {
                    if (column.Key == key)
                        return column;
                }
                return null;
            }
        }

        public int Count => _columns.Count;

        public bool IsReadOnly => false;

        public void Add(CustomColumn item)
        {
            _columns.Add(item);
            _table.Invalidate();
        }

        public void AddRange(params CustomColumn[] columns)
        {
            _columns.AddRange(columns);
            _table.Invalidate();
        }

        public void Clear()
        {
            _columns.Clear();
            _table.Invalidate();
        }

        public bool Contains(CustomColumn item) => _columns.Contains(item);

        public void CopyTo(CustomColumn[] array, int arrayIndex) => _columns.CopyTo(array, arrayIndex);

        public IEnumerator<CustomColumn> GetEnumerator() => _columns.GetEnumerator();

        public int IndexOf(CustomColumn item) => _columns.IndexOf(item);

        public void Insert(int index, CustomColumn item)
        {
            _columns.Insert(index, item);
            _table.Invalidate();
        }

        public bool Remove(CustomColumn item)
        {
            var result = _columns.Remove(item);
            if (result) _table.Invalidate();
            return result;
        }

        public void RemoveAt(int index)
        {
            _columns.RemoveAt(index);
            _table.Invalidate();
        }

        IEnumerator IEnumerable.GetEnumerator() => _columns.GetEnumerator();
    }
}
