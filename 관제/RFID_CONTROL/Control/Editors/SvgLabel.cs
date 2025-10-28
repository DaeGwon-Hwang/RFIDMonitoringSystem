using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class SvgLabel : UserControl
    {
        new public event EventHandler Click;
        public SvgLabel() {
			InitializeComponent();
		}

		[Category("UserDefine")]
		[Description("Text")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(string), "타이틀")]
		public override string Text {
			get => label.Text;
			set => label.Text = value;
		}

		[Category("UserDefine")]
		[Description("Label")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Label Label {
			get => label;
		}

        private void svgImageBox_Click(object sender, EventArgs e)
        {
            Click?.Invoke(sender, e);
        }


        private void svgImageBox_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }
    }
}
