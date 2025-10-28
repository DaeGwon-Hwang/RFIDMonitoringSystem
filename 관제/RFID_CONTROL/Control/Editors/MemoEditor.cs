using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class MemoEditor : UserControl, IBaseEditor {
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;

		private string _fieldName;
		private bool _required = false;
		private bool _changed = false;
		private Control _parentContainer = null;
		private BorderStyle _borderStyle = BorderStyle.None;
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		private bool _keydowncalled = false;

		public MemoEditor() {
			InitializeComponent();
			BorderStyle = BorderStyle.FixedSingle;
		}

		[Category("UserDefine")]
		[Description("BorderStyle")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(BorderStyle), "FixedSingle")]
		new public BorderStyle BorderStyle {
			get {
				return _borderStyle;
			}
			set {
				_borderStyle = value;
				if (_borderStyle == BorderStyle.None) {
					Editor.Width += 2;
					Editor.Height += 2;
					Editor.Margin = new Padding(0);
				} else if (_borderStyle == BorderStyle.Fixed3D) {
				} else if (_borderStyle == BorderStyle.FixedSingle) {
					Editor.Width -= 2;
					Editor.Height -= 2;
					Editor.Margin = new Padding(1);
				}
			}
		}

		[Category("UserDefine")]
		[Description("Caption")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(string), "레이블")]
		public string Caption {
			get => DisplayLabel.Text; set => DisplayLabel.Text = value;
		}

		[Category("UserDefine")]
		[Description("DependantEditors")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public List<IBaseEditor> DependantEditors => _dependantEditors;

		[Category("UserDefine")]
		[Description("Editor")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MemoEdit Editor {
			get {
				return memoEdit;
			}
		}

		[Category("UserDefine")]
		[Description("EditorSize")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "109, 25")]
		public Size EditorSize {
			get => Editor.Size; set => Editor.Size = value;
		}

		[Category("UserDefine")]
		[Description("FieldName")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName {
			get => _fieldName; set => _fieldName = value;
		}

		[Category("UserDefine")]
		[Description("HAlignment")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(DevExpress.Utils.HorzAlignment), "Default")]
		public DevExpress.Utils.HorzAlignment HAlignment {
			get => Editor.Properties.Appearance.TextOptions.HAlignment;
			set => Editor.Properties.Appearance.TextOptions.HAlignment = value;
		}

		[Category("UserDefine")]
		[Description("ImeMode")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(ImeMode), "NoControl")]
		new public ImeMode ImeMode {
			get => Editor.ImeMode;
			set => Editor.ImeMode = value;
		}

		[Category("UserDefine")]
		[Description("Label")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Label Label {
			get {
				return DisplayLabel;
			}
		}

		[Category("UserDefine")]
		[Description("LabelAutoSize")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "true")]
		public bool LabelAutoSize
		{
			get => Label.AutoSize; set => Label.AutoSize = value;
		}

		[Category("UserDefine")]
		[Description("LabelSize")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "49, 13")]
		public Size LabelSize
		{
			get => Label.Size; set => Label.Size = value;
		}

		[Category("UserDefine")]
		[Description("Mask")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public DevExpress.XtraEditors.Mask.MaskProperties Mask {
			get => Editor.Properties.Mask;
		}

		[Category("UserDefine")]
		[Description("MaxLength")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(int), "0")]
		public int MaxLength {
			get => Editor.Properties.MaxLength;
			set => Editor.Properties.MaxLength = value;
		}

		[Category("UserDefine")]
		[Description("ParentContainer")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Control ParentContainer {
			get => _parentContainer == null ? Parent : _parentContainer;
			set => _parentContainer = value;
		}

		[Category("UserDefine")]
		[Description("ReadOnly")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool ReadOnly {
			get => Editor.ReadOnly;
			set => Editor.ReadOnly = value;
		}

		[Category("UserDefine")]
		[Description("Required")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool Required {
			get => _required;
			set {
				_required = value;
				if (_required) {
					DisplayLabel.ForeColor = Color.FromArgb(0xE3, 0x18, 0x37);
				} else {
					DisplayLabel.ForeColor = Color.FromArgb(0x44, 0x44, 0x44);
				}
			}
		}

		public IBaseEditor ParentEditor {
			get;
		}

		public bool IsValid (bool messsage = true) {
			if (Required && Editor.Text.Trim().Equals("")) {
				if (messsage)
					//Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
				//Utils.DelayFocus(Editor);
				return false;
			}
			return true;
		}

		private void MemoEditor_Load(object sender, EventArgs e) {
			DataValueChanged += Editor_DataValueChanged;
			DataValueChanging += Editor_DataValueChanging;
			SizeChangedHandler();
		}

		private void MemoEditor_Enter (object sender, EventArgs e) {
			Editor.Select();
		}

		private void Editor_DataValueChanging(object sender, DataValueChangedEventArgs e) {
			foreach (var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangingHandler(this, e);
		}

		private void Editor_DataValueChanged(object sender, DataValueChangedEventArgs e) {
			foreach (var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangedHandler(this, e);
		}

		private void Editor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			var parent = ParentEditor == null ? ParentContainer : ParentEditor.ParentContainer;
			if (e.KeyCode == Keys.Tab && !e.Shift) {
				//Utils.FocusNext(parent, this, true);
			} else if (e.KeyCode == Keys.Tab && e.Shift) {
				//Utils.FocusPrevious(parent, this, true);
			}
		}

		private void KeyDownHandler(object sender, KeyEventArgs e) {
			OnKeyDown(e);
			_keydowncalled = false;
			if (e.KeyData == (Keys.Control | Keys.Enter)) {
				_keydowncalled = true;
				//Utils.NextOrSearch(this, ParentContainer, (ParentForm as IBaseProgram));
			}
		}

		private void KeyPressHandler (object sender, KeyPressEventArgs e) {
			if (_keydowncalled == true)
				e.Handled = true;
		}

		public Data<string> GetData() {
			var result = new Data<string>();
			result[FieldName] = Editor.Text;
			return result;
		}

		public void SetData(Data<string> value, bool fireEvent = true) {
			if (!value.ContainsKey(FieldName)) return;
			if (Editor.Text != value[FieldName]) {
				//Editor.Text = value[FieldName].Replace("\n", "\r\n");
				Editor.Text = value[FieldName];
				if (fireEvent) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public string GetValue () {
			return Editor.Text;
		}

		public void SetValue (string value, bool fireEvent = true) {
			if (Editor.Text != value) {
				//Editor.Text = value.Replace("\n", "\r\n");
				Editor.Text = value;
				if (fireEvent) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void Clear(bool fireEvent = true) {
			SetValue("", fireEvent);
		}

		private void Editor_Leave (object sender, EventArgs e) {
			if (_changed) {
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs());
				_changed = false;
			}
		}

		private void Editor_Modified (object sender, EventArgs e) {
			_changed = true;
		}

		private void Editor_TextChanged (object sender, EventArgs e) {
			DataValueChanging?.Invoke(this, new DataValueChangedEventArgs());
		}

		private void SizeChangedHandler() {
			if (Dock != DockStyle.None || !AutoSize) {
				var labelWidth = Label.ClientSize.Width + Label.Margin.Left + Label.Margin.Right;
				if (!Label.Visible) labelWidth = 0;
				var editorMargin = Editor.Margin.Left + Editor.Margin.Right;
				var newWidth = ClientSize.Width - labelWidth - editorMargin;
				Editor.Width = newWidth;
			}
		}

		private void _SizeChanged(object sender, EventArgs e) {
			SizeChangedHandler();
		}
	}
}