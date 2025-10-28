using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class RadioEditor : UserControl, IBaseEditor {
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;

		private string _fieldName;
		private bool _required = false;
		private bool _changed = false;
		private string _oldVal = "Y";
		private Control _parentContainer = null;
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		private string _defaultCode = "";
		private string _allCode = "";
		private string _allCodeName = "";

		public RadioEditor() {
			InitializeComponent();
		}

		[Category("UserDefine")]
		[Description("AllCode")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string AllCode {
			get => _allCode;
			set => _allCode = value;
		}

		[Category("UserDefine")]
		[Description("AllCodeName")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string AllCodeName {
			get => _allCodeName;
			set => _allCodeName = value;
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
		[Description("Nullable")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool Nullable { get; set; } = false;

		[Category("UserDefine")]
		[Description("ParentContainer")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Control ParentContainer {
			get => _parentContainer == null ? Parent : _parentContainer;
			set => _parentContainer = value;
		}

		[Category("UserDefine")]
		[Description("RadioPanel")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FlowLayoutPanel RadioPanel {
			get {
				return RdoPanel;
			}
		}

		[Category("UserDefine")]
		[Description("ReadOnly")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool ReadOnly {
			get => !RdoPanel.Enabled;
			set => RdoPanel.Enabled = !value;
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

		public IBaseEditor ParentEditor { get; }

		public bool IsValid(bool messsage = true) {
			//if (Required && GetValue().Equals("")) {
			//	if (messsage)
			//		Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
			//	if (RdoPanel.Controls.Count > 0)
			//		Utils.DelayFocus(RdoPanel.Controls[0]);
			//	return false;
			//}
			return true;
		}

		private void RadioEditor_Load(object sender, EventArgs e) {
			DataValueChanged += RadioEditor_DataValueChanged;
			DataValueChanging += RadioEditor_DataValueChanging;
			foreach (RadioButton rb in RdoPanel.Controls) {
				rb.KeyDown += KeyDownHandler;
				rb.PreviewKeyDown += RadioEditor_PreviewKeyDown;
				rb.Click += RadioButton_Click;
				rb.Enter += (s, e1) => { rb.BackColor = Color.FromArgb(0xCC, 0xCC, 0xCC); };
				rb.Leave += (s, e1) => { rb.BackColor = Color.Transparent; };
			}
		}

		private void RadioEditor_DataValueChanged(object sender, DataValueChangedEventArgs e) {
			foreach (var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangedHandler(this, e);
		}

		private void RadioEditor_DataValueChanging(object sender, DataValueChangedEventArgs e) {
			foreach (var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangingHandler(this, e);
		}

		private void RadioButton_Click(object sender, EventArgs e) {
			if (!(sender is RadioButton)) return;
			if ((sender as RadioButton).Tag as string == _oldVal) return;
			_changed = true;
			_oldVal = (sender as RadioButton).Tag as string;
			//Utils.DelayInvoke(() => {
			//	DataValueChanging?.Invoke(this, new DataValueChangedEventArgs());
			//}, 10);
		}

		private void RadioEditor_Enter(object sender, EventArgs e) {
			/*
            if (RadioPanel.Controls.Count > 0)
            {
                var isChecked = false;
                foreach (RadioButton rb in RadioPanel.Controls)
                {
                    if (rb.Checked)
                    {
                        rb.Select();
                        isChecked = true;
                        break;
                    }
                }
                if (!isChecked)
                    RadioPanel.Controls[0].Select();
            }
            */
		}

		private void RadioEditor_Leave(object sender, EventArgs e) {
			if (_changed) {
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs());
				_changed = false;
			}
		}

		private void RadioEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			//var parent = ParentEditor == null ? ParentContainer : ParentEditor.ParentContainer;
			//if (e.KeyCode == Keys.Tab && !e.Shift) {
			//	Utils.FocusNext(parent, this, true);
			//} else if (e.KeyCode == Keys.Tab && e.Shift) {
			//	Utils.FocusPrevious(parent, this, true);
			//}
		}

		private void KeyDownHandler(object sender, KeyEventArgs e) {
			OnKeyDown(e);
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
				//Utils.NextOrSearch(this, ParentContainer, (ParentForm as IBaseProgram));
			}
		}

		public void SetItems(DataList<Data<string>> dataList, string codeFieldName, string nameFieldName, string orderFieldName = null) {
			while (RdoPanel.Controls.Count > 0) {
				RdoPanel.Controls[0].Dispose();
			}
			if (_allCode != "" || _allCodeName != "") {
				var rb = new RadioButton();
				rb.Text = _allCodeName;
				rb.Tag = _allCode;
				rb.Font = new Font("돋움", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(129)));
				rb.Margin = new Padding(0, 6, 0, 0);
				rb.KeyDown += KeyDownHandler;
				//rb.PreviewKeyDown += RadioEditor_PreviewKeyDown;
				//rb.CheckedChanged += RadioButton_CheckedChanged;
				rb.AutoSize = true;
				rb.Show();
				RdoPanel.Controls.Add(rb);
			}
			if (dataList == null || dataList.Count == 0)
				return;
			if (orderFieldName != null)
				//dataList = dataList.OrderBy(d => long.Parse(Utils.CheckNull(d[orderFieldName], "999"))).ToDataList();

			foreach (var data in dataList) {
				var rb = new RadioButton();
				rb.Text = data[nameFieldName];
				rb.Tag = data[codeFieldName];
				rb.Font = new Font("돋움", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(129)));
				rb.Margin = new Padding(0, 6, 0, 0);
				rb.KeyDown += KeyDownHandler;
				//rb.PreviewKeyDown += RadioEditor_PreviewKeyDown;
				//rb.CheckedChanged += RadioButton_CheckedChanged;
				rb.AutoSize = true;
				rb.Show();
				RdoPanel.Controls.Add(rb);
			}
			if (_defaultCode != "")
				SetValue(_defaultCode, false);
			else if (RdoPanel.Controls.Count > 0) {
				(RdoPanel.Controls[0] as RadioButton).Checked = true;
				_oldVal = (RdoPanel.Controls[0] as RadioButton).Tag as string;
			}
		}

		public void SetItems(Data<string> param,
								string queryUri = "/com/code_list/common_code_list",
								string codeFieldName = "코드",
								string nameFieldName = "코드명",
								string orderFieldName = "순서") {
			//var dataList = ApiUtils.SearchDataList(new HttpApiClient(Utils.GetClientSession()), queryUri, param);
			//SetItems(dataList, codeFieldName, nameFieldName, orderFieldName);
		}

		public Data<string> GetData() {
			var result = Data.New(Tuple.Create(FieldName, ""));
			foreach (RadioButton rb in RdoPanel.Controls) {
				if (rb.Checked) {
					result[FieldName] = rb.Tag as string;
					return result;
				}
			}
			return result;
		}

		public void SetData(Data<string> value, bool fireEvent = true) {
			if (!value.ContainsKey(FieldName)) return;
			if (RdoPanel.Controls.Count <= 0 || GetValue() == value[FieldName])
				return;
			bool changed = false;
			foreach (RadioButton rb in RdoPanel.Controls) {
				if (rb.Tag.Equals(value[FieldName]) && !rb.Checked) {
					rb.Checked = true;
					changed = true;
					break;
				} else if (rb.Checked) {
					rb.Checked = false;
					changed = true;
				}
			}
			try {
				if (fireEvent && changed) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			} finally {
				_oldVal = value[FieldName];
			}
		}

		public string GetValue() {
			foreach (RadioButton rb in RdoPanel.Controls) {
				if (rb.Checked) {
					return rb.Tag as string;
				}
			}
			return "";
		}

		public void SetValue(string value, bool fireEvent = true) {
			if (RdoPanel.Controls.Count <= 0 || GetValue() == value)
				return;

			bool changed = false;
			foreach (RadioButton rb in RdoPanel.Controls) {
				if (rb.Tag.Equals(value) && !rb.Checked) {
					rb.Checked = true;
					changed = true;
					break;
				} else if (rb.Checked) {
					rb.Checked = false;
					changed = true;
				}
			}
			try {
				if (fireEvent && changed) {
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			} finally {
				_oldVal = value;
			}
		}

		public void Clear(bool fireEvent = true) {
			if (_defaultCode != "")
				SetValue(_defaultCode, fireEvent);
			else {
				if (Nullable) {
					if (RdoPanel.Controls.Count > 0) {
						var oldValue = GetValue();
						foreach (RadioButton radioButton in RdoPanel.Controls)
							if (radioButton.Checked) radioButton.Checked = false;
						if (oldValue != GetValue()) {
							try {
								if (fireEvent) {
									DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
									DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
								}
							} finally {
								_oldVal = GetValue();
							}
						}
					}
				} else {
					if (RdoPanel.Controls.Count > 0 && (RdoPanel.Controls[0] as RadioButton)?.Checked == false) {
						(RdoPanel.Controls[0] as RadioButton).Checked = true;
						try {
							if (fireEvent) {
								DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
								DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
							}
						} finally {
							_oldVal = (RdoPanel.Controls[0] as RadioButton).Tag as string;
						}
					}
				}
			}
		}
	}
}