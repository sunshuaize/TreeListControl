using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace demo.Controls.CustomTable
{
    /// <summary>
    /// CustomTable 综合示例窗体
    /// 展示CustomTable支持的所有功能
    /// </summary>
    public partial class CustomTableAllFeaturesDemo : Form
    {
        public CustomTableAllFeaturesDemo()
        {
            InitializeComponent();
            InitializeCustomTable();
            LoadSampleData();
            SetupEventHandlers();
        }

        private void InitializeCustomTable()
        {
            // ==================== 1. 复选框列 ====================
            // 支持行多选
            tblMain.ShowCheckBoxColumn = true;

            // ==================== 2. 基本文本列 ====================
            var idColumn = new CustomColumn("Id", "编号", 60)
            {
                Align = HorizontalAlignment.Center,
                ReadOnly = true
            };
            tblMain.Columns.Add(idColumn);

            var nameColumn = new CustomColumn("Name", "名称", 150);
            tblMain.Columns.Add(nameColumn);

            var descColumn = new CustomColumn("Description", "描述", 200);
            tblMain.Columns.Add(descColumn);

            // ==================== 3. 状态列 ====================
            // 带颜色圆点的状态显示
            var statusColumn = new StatusColumn("Status", "状态", 100);
            tblMain.Columns.Add(statusColumn);

            // ==================== 4. 链接标签列（新增功能） ====================
            // 一个单元格中显示多个类似链接的按钮
            var actionsColumn = new LinkLabelsColumn("Actions", "操作", 220)
            {
                LinkColor = Color.FromArgb(93, 128, 243),  // 默认#5D80F3
                LabelSpacing = 15,
                ShowUnderlineOnHover = true
            };
            tblMain.Columns.Add(actionsColumn);

            // ==================== 5. 自定义颜色配置 ====================
            tblMain.HeaderHeight = 35;
            tblMain.RowHeight = 40;
            tblMain.ShowGridLines = true;
            tblMain.GridColor = Color.FromArgb(230, 230, 230);
            
            // 选中行颜色
            tblMain.SelectedBackColor = Color.FromArgb(0, 120, 215);
            tblMain.SelectedForeColor = Color.White;
            
            // 悬停行颜色
            tblMain.HoverBackColor = Color.FromArgb(240, 240, 240);
            
            // 表头颜色
            tblMain.HeaderBackColor = Color.FromArgb(239, 240, 246);

            // ==================== 6. 行为配置 ====================
            tblMain.AllowSelection = true;
            tblMain.FullRowSelect = true;
            tblMain.AllowEdit = true;
        }

        private void LoadSampleData()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));
            dataTable.Columns.Add("Actions", typeof(string));

            // 添加示例数据
            var sampleData = new[]
            {
                new { Id = "001", Name = "项目A", Description = "这是一个演示项目", Status = "设计中", Actions = "编辑|edit,查看|view" },
                new { Id = "002", Name = "项目B", Description = "已完成的设计", Status = "成功", Actions = "编辑|edit,删除|delete,查看|view" },
                new { Id = "003", Name = "项目C", Description = "等待审批", Status = "处理中", Actions = "审核|approve,查看|view" },
                new { Id = "004", Name = "项目D", Description = "有错误需要修复", Status = "失败", Actions = "编辑|edit,重试|retry" },
                new { Id = "005", Name = "项目E", Description = "等待中", Status = "等待中", Actions = "编辑|edit,查看|view" },
                new { Id = "006", Name = "项目F", Description = "这是一个较长的项目描述用于测试文字截断", Status = "成功", Actions = "编辑|edit,导出|export,删除|delete" },
                new { Id = "007", Name = "项目G", Description = "测试数据", Status = "警告", Actions = "查看|view" },
                new { Id = "008", Name = "项目H", Description = "测试数据2", Status = "处理中", Actions = "编辑|edit,查看|view" },
                new { Id = "009", Name = "项目I", Description = "测试数据3", Status = "成功", Actions = "编辑|edit,删除|delete" },
                new { Id = "010", Name = "项目J", Description = "测试数据4", Status = "成功", Actions = "查看|view,导出|export" },
                new { Id = "011", Name = "项目K", Description = "测试数据5", Status = "处理中", Actions = "审核|approve,编辑|edit" },
                new { Id = "012", Name = "项目L", Description = "测试数据6", Status = "成功", Actions = "编辑|edit" },
            };

            foreach (var item in sampleData)
            {
                dataTable.Rows.Add(item.Id, item.Name, item.Description, item.Status, item.Actions);
            }

            tblMain.DataSource = dataTable;
        }

        private void SetupEventHandlers()
        {
            // ==================== 事件1: 单元格点击 ====================
            tblMain.CellClick += CustomTable_CellClick;

            // ==================== 事件2: 单元格双击 ====================
            tblMain.CellDoubleClick += CustomTable_CellDoubleClick;

            // ==================== 事件3: 选中行改变 ====================
            tblMain.SelectedIndexChanged += CustomTable_SelectedIndexChanged;

            // ==================== 事件4: 单元格值改变 ====================
            tblMain.CellValueChanged += CustomTable_CellValueChanged;

            // ==================== 事件5: 链接标签点击 ====================
            // 找到LinkLabelsColumn并订阅事件
            var actionsColumn = tblMain.Columns["Actions"] as LinkLabelsColumn;
            if (actionsColumn != null)
            {
                actionsColumn.LinkLabelClick += ActionsColumn_LinkLabelClick;
            }

            UpdateInfoLabel("CustomTable 已加载，支持以下功能：\n" +
                           "1. 复选框列（支持多选） 2. 基本文本列（可编辑） 3. 状态列（带颜色圆点） 4. 链接标签列（可点击按钮）\n" +
                           "操作说明：\n" +
                           "• 单击复选框选择行  • Ctrl+点击多选  • Shift+点击范围选择  • 双击单元格编辑\n" +
                           "• 点击链接标签触发操作");
        }

        private void CustomTable_CellClick(object sender, CustomCellEventArgs e)
        {
            UpdateInfoLabel($"单元格点击: 行={e.RowIndex + 1}, 列={e.ColumnIndex + 1}, 值={e.CellValue}");
        }

        private void CustomTable_CellDoubleClick(object sender, CustomCellEventArgs e)
        {
            UpdateInfoLabel($"单元格双击: 行={e.RowIndex + 1}, 列={e.ColumnIndex + 1}, 值={e.CellValue}");
        }

        private void CustomTable_SelectedIndexChanged(object sender, CustomSelectionEventArgs e)
        {
            var selectedCount = tblMain.CheckedRows.Count;
            var selectedRows = string.Join(", ", tblMain.CheckedRows);
            UpdateInfoLabel($"选中行改变: 当前选中{e.SelectedIndex + 1}行，总共选中{selectedCount}行，行索引: [{selectedRows}]");
        }

        private void CustomTable_CellValueChanged(object sender, CustomCellValueChangedEventArgs e)
        {
            UpdateInfoLabel($"单元格值改变: 行={e.RowIndex + 1}, 列={e.ColumnIndex + 1}, 旧值={e.OldValue}, 新值={e.NewValue}");
        }

        private void ActionsColumn_LinkLabelClick(object sender, LinkLabelClickEventArgs e)
        {
            UpdateInfoLabel($"链接标签点击: 行={e.RowIndex + 1}, 列={e.ColumnIndex + 1}, 标签={e.LabelInfo.Text}, 值={e.LabelInfo.Value}");
            
            // 根据不同的标签值执行不同操作
            switch (e.LabelInfo.Value)
            {
                case "edit":
                    MessageBox.Show($"编辑: 行{e.RowIndex + 1}", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "delete":
                    var result = MessageBox.Show($"确认删除行{e.RowIndex + 1}?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        // 这里可以添加删除逻辑
                        MessageBox.Show("删除操作已执行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    break;
                case "view":
                    MessageBox.Show($"查看详情: 行{e.RowIndex + 1}", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "export":
                    MessageBox.Show($"导出: 行{e.RowIndex + 1}", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "approve":
                    MessageBox.Show($"审核: 行{e.RowIndex + 1}", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "retry":
                    MessageBox.Show($"重试: 行{e.RowIndex + 1}", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    MessageBox.Show($"点击了: {e.LabelInfo.Text}", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void UpdateInfoLabel(string text)
        {
            lblInfo.Text = text;
        }

        // ==================== 静态方法：创建示例窗体 ====================
        
        /// <summary>
        /// 创建并显示CustomTable功能演示窗体
        /// </summary>
        public static void ShowDemo()
        {
            Application.Run(new CustomTableAllFeaturesDemo());
        }

        /// <summary>
        /// 创建一个配置好的CustomTable控件（用于嵌入其他窗体）
        /// </summary>
        public static CustomTable CreateSampleTable()
        {
            var table = new CustomTable
            {
                Dock = DockStyle.Fill
            };

            // 添加列
            table.ShowCheckBoxColumn = true;
            table.Columns.Add(new CustomColumn("Id", "编号", 60) { Align = HorizontalAlignment.Center, ReadOnly = true });
            table.Columns.Add(new CustomColumn("Name", "名称", 150));
            table.Columns.Add(new CustomColumn("Description", "描述", 200));
            table.Columns.Add(new StatusColumn("Status", "状态", 100));
            
            var actionsColumn = new LinkLabelsColumn("Actions", "操作", 180)
            {
                LinkColor = Color.FromArgb(93, 128, 243),
                LabelSpacing = 10
            };
            table.Columns.Add(actionsColumn);

            // 配置样式
            table.HeaderHeight = 32;
            table.RowHeight = 36;
            table.ShowGridLines = true;
            table.GridColor = Color.FromArgb(230, 230, 230);
            table.SelectedBackColor = Color.FromArgb(0, 120, 215);
            table.SelectedForeColor = Color.White;
            table.HoverBackColor = Color.FromArgb(240, 240, 240);
            table.HeaderBackColor = Color.FromArgb(239, 240, 246);

            return table;
        }

        /// <summary>
        /// 使用List数据源创建示例
        /// </summary>
        public static CustomTable CreateWithListData()
        {
            var table = CreateSampleTable();

            // 创建数据列表
            var dataList = new List<SampleItem>
            {
                new SampleItem { Id = "001", Name = "测试1", Description = "描述1", Status = "成功", Actions = "编辑|edit,查看|view" },
                new SampleItem { Id = "002", Name = "测试2", Description = "描述2", Status = "处理中", Actions = "编辑|edit,删除|delete" },
            };

            table.DataSource = dataList;
            return table;
        }
    }

    /// <summary>
    /// 示例数据类（用于List数据绑定）
    /// </summary>
    public class SampleItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Actions { get; set; }
    }
}
