using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class TitleLabel : UserControl {
		public TitleLabel () {
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

		[Category("UserDefine")]
		[Description("ColorLabel")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Label ColorLabel {
			get => colorLabel;
		}
	}
}
