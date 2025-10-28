using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class CheckBoxEditor : UserControl, IBaseEditor {
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;

		private string _fieldName;
		private bool _required = false;
		private bool _changed = false;
		private Control _parentContainer = null;
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		private string _defaultCode = "";

		public CheckBoxEditor() {
			InitializeComponent();
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
		[Description("DefaultCode")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DefaultCode {
			get => _defaultCode;
			set => _defaultCode = value;
		}

		[Category("UserDefine")]
		[Description("DependantEditors")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public List<IBaseEditor> DependantEditors => _dependantEditors;

		[Category("UserDefine")]
		[Description("FieldName")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName {
			get => _fieldName; set => _fieldName = value;
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
			get => !CheckBoxPanel.Enabled;
			set => CheckBoxPanel.Enabled = !value;
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

        [Category("UserDefine")]
        [Description("CheckBoxControls")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlCollection CheckBoxControls
        {
            get
            {
                return CheckBoxPanel.Controls ;
            }
        }

        public IBaseEditor ParentEditor { get; }

		public bool IsValid(bool messsage = true)
		{
            if (Required && GetValue().Equals(""))
            {
                if (messsage)
                    //Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
                if (CheckBoxPanel.Controls.Count > 0)
                    //Utils.DelayFocus(CheckBoxPanel.Controls[0]);
                return false;
            }

            return true;
        }

		private void CheckBoxEditor_Load(object sender, EventArgs e)
		{

		}

        private void CheckBoxButton_CheckedChanged(object sender, EventArgs e) {
			_changed = true;
			DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { Source = sender });
		}


		private void CheckBoxEditor_Enter(object sender, EventArgs e) {
			if (CheckBoxPanel.Controls.Count > 0) {
				CheckBoxPanel.Controls[0].Select();
			}
		}

		public Data<string> GetData () {
			string val = "";
			foreach (CheckBox checkBox in CheckBoxPanel.Controls) {
				if (checkBox.Checked) {
					val += "," + checkBox.Tag as string;
				}
			}
			if (!val.Equals(""))
				val = val.Remove(0, 1);
			var result = new Data<string>();
			result[FieldName] = val;
			return result;
		}

		public void SetData (Data<string> value, bool fireEvent = true) {
			if (!value.ContainsKey(FieldName)) return;
			if (CheckBoxPanel.Controls.Count <= 0) return;
			string commaText = "," + value[FieldName] + ",";
			var changed = false;
			foreach (CheckBox checkBox in CheckBoxPanel.Controls) {
				if (commaText.IndexOf("," + checkBox.Tag as string + ",") >= 0) {
					if (!checkBox.Checked) {
						checkBox.Checked = true;
						changed = true;
					}
				} else {
					if (checkBox.Checked) {
						checkBox.Checked = false;
						changed = true;
					}
				}
			}
			if (changed && fireEvent) {
				DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
			}
		}

		public string GetValue () {
			string val = "";
			foreach (CheckBox checkBox in CheckBoxPanel.Controls) {
				if (checkBox.Checked) {
					val += "," + checkBox.Tag as string;
				}
			}
			if (!val.Equals(""))
				val = val.Remove(0, 1);
			return val;
		}

		public void SetValue (string value, bool fireEvent = true) {
			if (CheckBoxPanel.Controls.Count <= 0)
				return;

			string commaText = value == null ? "" : "," + value + ",";

			var changed = false;
			foreach (CheckBox checkBox in CheckBoxPanel.Controls) {
				if (commaText.IndexOf("," + checkBox.Tag as string + ",") >= 0) {
					if (!checkBox.Checked) {
						checkBox.Checked = true;
					}
				} else {
					if (checkBox.Checked) {
						checkBox.Checked = false;
						changed = true;
					}
				}
				if (changed && fireEvent) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void Clear(bool fireEvent = true) {
			SetValue(_defaultCode, fireEvent);
		}

		private void CheckBoxEditor_Leave(object sender, EventArgs e) {
			if (_changed) {
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs());
				_changed = false;
			}
			//CheckBoxPanel.BorderStyle = BorderStyle.None;
		}
	}
}
