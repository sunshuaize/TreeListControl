using System;
using System.Drawing;
using System.Windows.Forms;
using demo.Controls;

namespace demo
{
    public partial class TabControlDemoForm : Form
    {
        public TabControlDemoForm()
        {
            InitializeComponent();
            InitTabs();
        }

        private void InitTabs()
        {
            var tab1 = tabControl.AddTab("用户管理");
            tab1.BackColor = Color.White;
            var label1 = new Label
            {
                Text = "这是用户管理页面",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };
            tab1.Controls.Add(label1);

            var button1 = new Button
            {
                Text = "添加用户",
                Location = new Point(20, 80),
                Size = new Size(100, 30)
            };
            tab1.Controls.Add(button1);

            var tab2 = tabControl.AddTab("权限设置");
            tab2.BackColor = Color.White;
            var label2 = new Label
            {
                Text = "这是权限设置页面",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };
            tab2.Controls.Add(label2);

            var tab3 = tabControl.AddTab("系统配置");
            tab3.BackColor = Color.White;
            var label3 = new Label
            {
                Text = "这是系统配置页面",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };
            tab3.Controls.Add(label3);

            var tab4 = tabControl.AddTab("关于");
            tab4.BackColor = Color.White;
            var label4 = new Label
            {
                Text = "CustomTabControl 示例 v1.0",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };
            tab4.Controls.Add(label4);
        }
    }
}
