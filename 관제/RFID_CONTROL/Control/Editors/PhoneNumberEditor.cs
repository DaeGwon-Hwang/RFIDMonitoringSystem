using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class PhoneNumberEditor : UserControl, IBaseEditor {
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;

		private bool _selectAllDone1 = false, _selectAllDone2 = false, _selectAllDone3 = false;
		private string _fieldName, _fieldName1, _fieldName2, _fieldName3;
		private bool _required = false;
		private bool _changed = false;
		private Control _parentContainer = null;
		private BorderStyle _borderStyle = BorderStyle.None;
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		public PhoneNumberEditor () {
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
					Editor1.Width += 2;
					Editor1.Height += 2;
					Editor1.Margin = new Padding(0);
					Editor2.Width += 2;
					Editor2.Height += 2;
					Editor2.Margin = new Padding(0);
					Editor3.Width += 2;
					Editor3.Height += 2;
					Editor3.Margin = new Padding(0);
				} else if (_borderStyle == BorderStyle.Fixed3D) {
				} else if (_borderStyle == BorderStyle.FixedSingle) {
					Editor1.Width -= 2;
					Editor1.Height -= 2;
					Editor1.Margin = new Padding(1);
					Editor2.Width -= 2;
					Editor2.Height -= 2;
					Editor2.Margin = new Padding(1);
					Editor3.Width -= 2;
					Editor3.Height -= 2;
					Editor3.Margin = new Padding(1);
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
		[Description("Editor1")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TextEdit Editor1 {
			get {
				return phoneNumberEdit1;
			}
		}

		[Category("UserDefine")]
		[Description("Editor2")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TextEdit Editor2 {
			get {
				return phoneNumberEdit2;
			}
		}

		[Category("UserDefine")]
		[Description("Editor3")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TextEdit Editor3 {
			get {
				return phoneNumberEdit3;
			}
		}

		[Category("UserDefine")]
		[Description("Editor1Size")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "48, 25")]
		public Size Editor1Size {
			get => Editor1.Size; set => Editor1.Size = value;
		}

		[Category("UserDefine")]
		[Description("Editor2Size")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "58, 25")]
		public Size Editor2Size {
			get => Editor2.Size; set => Editor2.Size = value;
		}

		[Category("UserDefine")]
		[Description("Editor3Size")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "58, 25")]
		public Size Editor3Size {
			get => Editor3.Size; set => Editor3.Size = value;
		}

		[Category("UserDefine")]
		[Description("FieldName")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName {
			get => _fieldName;
			set {
				_fieldName = value;
				_fieldName1 = value + "1";
				_fieldName2 = value + "2";
				_fieldName3 = value + "3";
			}
		}

		[Category("UserDefine")]
		[Description("FieldName1")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName1 {
			get => _fieldName1 == null || _fieldName1.Equals("") ? FieldName + "1" : _fieldName1;
			set => _fieldName1 = value;
		}

		[Category("UserDefine")]
		[Description("FieldName2")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName2 {
			get => _fieldName2 == null || _fieldName2.Equals("") ? FieldName + "2" : _fieldName2;
			set => _fieldName2 = value;
		}

		[Category("UserDefine")]
		[Description("FieldName3")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName3 {
			get => _fieldName3 == null || _fieldName3.Equals("") ? FieldName + "3" : _fieldName3;
			set => _fieldName3 = value;
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
			get => Editor1.ReadOnly;
			set {
				Editor1.ReadOnly = value;
				Editor2.ReadOnly = value;
				Editor3.ReadOnly = value;
			}
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
			var ret = true;
			//if (Required && Editor1.Text.Equals("")) {
			//	ret = false;
			//	Utils.DelayFocus(Editor1);
			//}
			//if (Required && Editor2.Text.Equals("")) {
			//	ret = false;
			//	Utils.DelayFocus(Editor2);
			//}
			//if (Required && Editor3.Text.Equals("")) {
			//	ret = false;
			//	Utils.DelayFocus(Editor3);
			//}
			//if (!ret && messsage)
			//	Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
			return ret;
		}

		private void PhoneNumberOldEditor_Load (object sender, EventArgs e) {
			DataValueChanged += Editor_DataValueChanged;
			DataValueChanging += Editor_DataValueChanging;
			SizeChangedHandler();
		}

		private void PhoneNumberOldEditor_Enter (object sender, EventArgs e) {
			Editor1.Select();
		}

		private void Editor_DataValueChanging (object sender, DataValueChangedEventArgs e) {
			foreach (var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangingHandler(this, e);
		}

		private void Editor_DataValueChanged (object sender, DataValueChangedEventArgs e) {
			foreach (var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangedHandler(this, e);
		}

		private void Editor_PreviewKeyDown (object sender, PreviewKeyDownEventArgs e) {
			//var parent = ParentEditor == null ? ParentContainer : ParentEditor.ParentContainer;
			//if (e.KeyCode == Keys.Tab && !e.Shift) {
			//	if (sender is TextEdit && sender == phoneNumberEdit1)
			//		Utils.DelayFocus(phoneNumberEdit2);
			//	else if (sender is TextEdit && sender == phoneNumberEdit2)
			//		Utils.DelayFocus(phoneNumberEdit3);
			//	else
			//		Utils.FocusNext(parent, this, true);
			//} else if (e.KeyCode == Keys.Tab && e.Shift) {
			//	if (sender is TextEdit && sender == phoneNumberEdit3)
			//		Utils.DelayFocus(phoneNumberEdit2);
			//	else if (sender is TextEdit && sender == phoneNumberEdit2)
			//		Utils.DelayFocus(phoneNumberEdit1);
			//	else
			//		Utils.FocusPrevious(parent, this, true);
			//}
		}

		private void KeyDownHandler (object sender, KeyEventArgs e) {
			//OnKeyDown(e);
			//if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
			//	if (sender is TextEdit && sender == phoneNumberEdit1)
			//		Utils.DelayFocus(phoneNumberEdit2);
			//	else if (sender is TextEdit && sender == phoneNumberEdit2)
			//		Utils.DelayFocus(phoneNumberEdit3);
			//	else {
			//		Utils.NextOrSearch(this, ParentContainer, (ParentForm as IBaseProgram));
			//	}
			//}
		}

		public Data<string> GetData () {
			var result = new Data<string>();
			result[FieldName] = Editor1.Text + Editor2.Text + Editor3.Text;
			result[FieldName1] = Editor1.Text;
			result[FieldName2] = Editor2.Text;
			result[FieldName3] = Editor3.Text;
			return result;
		}

		public void SetData (Data<string> value, bool fireEvent = true) {
			var changed = false;
			if (value.ContainsKey(FieldName1)) {
				if (Editor1.Text != value[FieldName1]) {
					Editor1.Text = value[FieldName1];
					changed = true;
				}
			}
			if (value.ContainsKey(FieldName2)) {
				if (Editor2.Text != value[FieldName2]) {
					Editor2.Text = value[FieldName2];
					changed = true;
				}
			}
			if (value.ContainsKey(FieldName3)) {
				if (Editor3.Text != value[FieldName3]) {
					Editor3.Text = value[FieldName3];
					changed = true;
				}
			}
			if (changed && fireEvent) {
				DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
			}
		}

		public string GetValue () {
			return Editor1.Text + Editor2.Text + Editor3.Text;
		}

		public string GetValue1 () {
			return Editor1.Text;
		}

		public string GetValue2 () {
			return Editor2.Text;
		}

		public string GetValue3 () {
			return Editor3.Text;
		}

		public void SetValue1 (string value, bool fireEvent = true) {
			if (Editor1.Text != value) {
				Editor1.Text = value;
				if (fireEvent) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void SetValue2 (string value, bool fireEvent = true) {
			if (Editor2.Text != value) {
				Editor2.Text = value;
				if (fireEvent) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void SetValue3 (string value, bool fireEvent = true) {
			if (Editor3.Text != value) {
				Editor3.Text = value;
				if (fireEvent) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void Clear (bool fireEvent = true) {
			var changed = false;
			if (Editor1.Text != "") {
				Editor1.Text = "";
				changed = true;
			}
			if (Editor2.Text != "") {
				Editor2.Text = "";
				changed = true;
			}
			if (Editor3.Text != "") {
				Editor3.Text = "";
				changed = true;
			}
			if (fireEvent && changed) {
				DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
			}
		}

		private void Editor_Leave (object sender, EventArgs e) {
			if (sender is TextEdit) {
				if (sender == phoneNumberEdit1) {
					_selectAllDone1 = false;
				} else if (sender == phoneNumberEdit2) {
					_selectAllDone2 = false;
				} else if (sender == phoneNumberEdit3) {
					_selectAllDone3 = false;
				}
				if (_changed) {
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs());
					_changed = false;
				}
			}
		}

		private void Editor_Modified (object sender, EventArgs e) {
			_changed = true;
		}

		private void Editor_TextChanged (object sender, EventArgs e) {
			DataValueChanging?.Invoke(this, new DataValueChangedEventArgs());
		}

		private void SizeChangedHandler () {
			if (Dock != DockStyle.None || !AutoSize) {
				var labelWidth = Label.ClientSize.Width + Label.Margin.Left + Label.Margin.Right;
				if (!Label.Visible) labelWidth = 0;
				var editorMargin = Editor1.Margin.Left + Editor1.Margin.Right + Editor2.Margin.Left + Editor2.Margin.Right + Editor3.Margin.Left + Editor3.Margin.Right;
				var editorWidth = ClientSize.Width - labelWidth - editorMargin;
				var editor1Width = (int)Math.Floor((double)editorWidth * (5/17));
				var editor2Width = (int)Math.Floor((double)(editorWidth - editor1Width) / 2);
				Editor1.Width = editor1Width;
				Editor2.Width = editor2Width;
				Editor3.Width = editor2Width;
			}
		}

		private void _SizeChanged (object sender, EventArgs e) {
			SizeChangedHandler();
		}

		private void Editor_Enter (object sender, EventArgs e) {
			if (MouseButtons == MouseButtons.None) {
				if (sender is TextEdit) {
					if (sender == phoneNumberEdit1) {
						phoneNumberEdit1.SelectAll();
						_selectAllDone1 = true;
					} else if (sender == phoneNumberEdit2) {
						phoneNumberEdit2.SelectAll();
						_selectAllDone2 = true;
					} else if (sender == phoneNumberEdit3) {
						phoneNumberEdit3.SelectAll();
						_selectAllDone3 = true;
					}
				}
			}
		}

		private void Editor_MouseUp (object sender, MouseEventArgs e) {
			if (sender is TextEdit) {
				if (sender == phoneNumberEdit1) {
					if (!_selectAllDone1 && phoneNumberEdit1.SelectionLength == 0) {
						_selectAllDone1 = true;
						phoneNumberEdit1.SelectAll();
					}
				} else if (sender == phoneNumberEdit2) {
					if (!_selectAllDone2 && phoneNumberEdit2.SelectionLength == 0) {
						_selectAllDone2 = true;
						phoneNumberEdit2.SelectAll();
					}
				} else if (sender == phoneNumberEdit3) {
					if (!_selectAllDone3 && phoneNumberEdit3.SelectionLength == 0) {
						_selectAllDone3 = true;
						phoneNumberEdit3.SelectAll();
					}
				}
			}
		}
	}
}
