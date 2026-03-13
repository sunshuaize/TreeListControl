using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using demo.Controls;

namespace demo
{
    public partial class Form1 : Form
    {
        private TreeListControl treeList;
        private IconLabelButton iconButton1;
        private IconLabelButton iconButton2;
        private IconLabelButton iconButton3;

        public Form1()
        {
            InitializeComponent();
            //InitTreeList();
            InitIconButtons();
        }

        private void InitIconButtons()
        {
            int startX = 10;
            int startY = 10;
            int spacing = 60;

            iconButton1 = new IconLabelButton
            {
                Location = new Point(startX, startY),
                Text = "文件",
                Size = new Size(48, 48)
            };
            iconButton1.LoadIconFromFile(AppDomain.CurrentDomain.BaseDirectory + "Icons\\folder_blue.png");
            iconButton1.Clicked += (s, e) => MessageBox.Show("点击了: 文件");
            this.Controls.Add(iconButton1);

            iconButton2 = new IconLabelButton
            {
                Location = new Point(startX + spacing, startY),
                Text = "文档",
                Size = new Size(48, 43)
            };
            iconButton2.LoadIconFromFile(AppDomain.CurrentDomain.BaseDirectory + "Icons\\file_blue.png");
            iconButton2.Clicked += (s, e) => MessageBox.Show("点击了: 文档");
            this.Controls.Add(iconButton2);

            iconButton3 = new IconLabelButton
            {
                Location = new Point(startX + spacing * 2, startY),
                Text = "设置",
                Size = new Size(48, 43)
            };
            iconButton3.LoadIconFromFile(AppDomain.CurrentDomain.BaseDirectory + "Icons\\folder_gray.png");
            iconButton3.Clicked += (s, e) => MessageBox.Show("点击了: 设置");
            this.Controls.Add(iconButton3);
        }

        private void InitTreeList()
        {
            treeList = new TreeListControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(treeList);

            // 项目A - 设计任务
            var project1 = new TreeListControl.TreeListNode("项目A - 设计任务", "2024-01-15 09:00:00", "2024-02-15 17:00:00");
            
            // 建筑结构设计
            var task1 = new TreeListControl.TreeListNode("建筑结构设计", "2024-01-16 10:00:00", "2024-02-01 17:00:00");
            var subtask1_1 = new TreeListControl.TreeListNode("基础结构", "2024-01-17 09:00:00", "2024-01-25 17:00:00");
            var subtask1_2 = new TreeListControl.TreeListNode("主体结构", "2024-01-18 09:00:00", "2024-01-28 17:00:00");
            var subtask1_3 = new TreeListControl.TreeListNode("屋面结构", "2024-01-19 09:00:00", "2024-01-30 17:00:00");
            
            // 电气系统设计
            var task2 = new TreeListControl.TreeListNode("电气系统设计", "2024-01-17 11:00:00", "2024-02-05 17:00:00");
            var subtask2_1 = new TreeListControl.TreeListNode("配电系统", "2024-01-18 14:00:00", "2024-01-25 17:00:00");
            var subtask2_2 = new TreeListControl.TreeListNode("照明系统", "2024-01-18 15:00:00", "2024-01-28 17:00:00");
            var subtask2_3 = new TreeListControl.TreeListNode("防雷接地", "2024-01-19 10:00:00", "2024-01-29 17:00:00");
            
            // 给排水设计
            var task3 = new TreeListControl.TreeListNode("给排水设计", "2024-01-20 09:00:00", "2024-02-10 17:00:00");
            var subtask3_1 = new TreeListControl.TreeListNode("给水系统", "2024-01-21 09:00:00", "2024-02-01 17:00:00");
            var subtask3_2 = new TreeListControl.TreeListNode("排水系统", "2024-01-21 10:00:00", "2024-02-03 17:00:00");
            var subtask3_3 = new TreeListControl.TreeListNode("消防系统", "2024-01-22 09:00:00", "2024-02-05 17:00:00");
            
            // 暖通设计
            var task4 = new TreeListControl.TreeListNode("暖通设计", "2024-01-22 11:00:00", "2024-02-15 17:00:00");
            var subtask4_1 = new TreeListControl.TreeListNode("空调系统", "2024-01-23 09:00:00", "2024-02-08 17:00:00");
            var subtask4_2 = new TreeListControl.TreeListNode("通风系统", "2024-01-23 10:00:00", "2024-02-10 17:00:00");
            var subtask4_3 = new TreeListControl.TreeListNode("排烟系统", "2024-01-24 09:00:00", "2024-02-12 17:00:00");
            
            // 项目B - 施工图
            var project2 = new TreeListControl.TreeListNode("项目B - 施工图", "2024-01-20 09:00:00", "2024-03-01 17:00:00");
            var task5 = new TreeListControl.TreeListNode("建筑施工图", "2024-01-21 10:00:00", "2024-02-10 17:00:00");
            var task6 = new TreeListControl.TreeListNode("结构施工图", "2024-01-22 11:00:00", "2024-02-15 17:00:00");
            var task7 = new TreeListControl.TreeListNode("设备施工图", "2024-01-23 11:00:00", "2024-02-20 17:00:00");
            
            // 项目C - 竣工验收
            var project3 = new TreeListControl.TreeListNode("项目C - 竣工验收", "2024-01-25 09:00:00", "2024-03-15 17:00:00");
            var task8 = new TreeListControl.TreeListNode("资料整理", "2024-01-26 09:00:00", "2024-03-01 17:00:00");
            var task9 = new TreeListControl.TreeListNode("现场验收", "2024-01-27 09:00:00", "2024-03-10 17:00:00");
            var task10 = new TreeListControl.TreeListNode("整改完善", "2024-01-28 09:00:00", "2024-03-14 17:00:00");
            
            // 添加节点
            treeList.AddNode(project1);
            treeList.AddNode(project1, task1);
            treeList.AddNode(task1, subtask1_1);
            treeList.AddNode(task1, subtask1_2);
            treeList.AddNode(task1, subtask1_3);
            
            treeList.AddNode(project1, task2);
            treeList.AddNode(task2, subtask2_1);
            treeList.AddNode(task2, subtask2_2);
            treeList.AddNode(task2, subtask2_3);
            
            treeList.AddNode(project1, task3);
            treeList.AddNode(task3, subtask3_1);
            treeList.AddNode(task3, subtask3_2);
            treeList.AddNode(task3, subtask3_3);
            
            treeList.AddNode(project1, task4);
            treeList.AddNode(task4, subtask4_1);
            treeList.AddNode(task4, subtask4_2);
            treeList.AddNode(task4, subtask4_3);
            
            treeList.AddNode(project2);
            treeList.AddNode(project2, task5);
            treeList.AddNode(project2, task6);
            treeList.AddNode(project2, task7);
            
            treeList.AddNode(project3);
            treeList.AddNode(project3, task8);
            treeList.AddNode(project3, task9);
            treeList.AddNode(project3, task10);
            
            // 展开项目
            project1.IsExpanded = true;
            task2.IsExpanded = true;
            project2.IsExpanded = true;
            project3.IsExpanded = true;
            treeList.RefreshVisibleNodes();
        }
    }
}
