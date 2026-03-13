using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace demo.Controls
{
    public class TabPage : Panel
    {
        private string _text = "";

        [Localizable(true)]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnTextChanged(EventArgs.Empty);
            }
        }

        public TabPage()
        {
            BackColor = Color.White;
            Padding = new Padding(5);
        }

        public TabPage(string text) : this()
        {
            _text = text;
        }
    }
}
