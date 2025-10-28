using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class ComboBoxEditor : UserControl, IBaseEditor {
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;

		private string _fieldName = "", _nameFieldName = "";
		private bool _required = false;
		private bool _changed = false;
		private BorderStyle _borderStyle = BorderStyle.None;
		private Control _parentContainer = null;
		private DataList<string> _itemDatas = new DataList<string>();
		private DataList<Data<string>> _dataList = new DataList<Data<string>>();
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		private string _defaultCode = "";
		private string _allCode = "";
		private string _allCodeName = "";
		private bool _showCode = false;

		public ComboBoxEditor() {
			InitializeComponent();
			BorderStyle = BorderStyle.FixedSingle;
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
				if(_borderStyle == BorderStyle.None) {
					Editor.Width += 2;
					Editor.Height += 2;
					Editor.Margin = new Padding(0);
				} else if(_borderStyle == BorderStyle.Fixed3D) {
				} else if(_borderStyle == BorderStyle.FixedSingle) {
					Editor.Width -= 2;
					Editor.Height -= 2;
					Editor.Margin = new Padding(1);
				}
			}
		}

		[Category("UserDefine")]
		[Description("Buttons")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DevExpress.XtraEditors.Controls.EditorButtonCollection Buttons {
			get {
				return comboBoxEdit.Properties.Buttons;
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
		[Description("Editor")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ComboBoxEdit Editor {
			get {
				return comboBoxEdit;
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
			get => _fieldName;
			set {
				_fieldName = value;
				if (value.EndsWith("_CD"))
					NameFieldName = value.Substring(0, value.LastIndexOf("_CD")) + "_NM";
				else
					NameFieldName = value + "_NM";
			}
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
		[Description("NameFieldName")] 
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string NameFieldName {
			get
			{
				if (string.IsNullOrEmpty(_nameFieldName))
				{
					if (FieldName.EndsWith("_CD"))
						return FieldName.Substring(0, FieldName.LastIndexOf("_CD")) + "_NM";
					else
						return FieldName + "_NM";
				}
				else
					return _nameFieldName;
			}
			set => _nameFieldName = value;
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
			set {
				Editor.ReadOnly = value;
				for(int i = 0; i < Buttons.Count; i++) {
					if(!Editor.ReadOnly && !Buttons[i].Visible)
						Buttons[i].Visible = true;
					else if(Editor.ReadOnly && Buttons[i].Visible)
						Buttons[i].Visible = false;
				}
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
				if(_required) {
					DisplayLabel.ForeColor = Color.FromArgb(0xE3, 0x18, 0x37);
				} else {
					DisplayLabel.ForeColor = Color.FromArgb(0x44, 0x44, 0x44);
				}
			}
		}

		[Category("UserDefine")]
		[Description("ShowCode")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool ShowCode {
			get => _showCode;
			set => _showCode = value;
		}

		public IBaseEditor ParentEditor {
			get;
		}

		public bool IsValid(bool messsage = true) {
			if(Required && GetValue().Equals("")) {
				if(messsage)
					//Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
				//Utils.DelayFocus(Editor);
				return false;
			}
			return true;
		}

		private void ComboBoxEditor_Load(object sender, EventArgs e) {
			DataValueChanged += Editor_DataValueChanged;
			DataValueChanging += Editor_DataValueChanging;
			if(Buttons.Count > 1) {
				while(Buttons.Count > 1)
					Buttons.RemoveAt(0);
			}
			if(Buttons.Count == 0) {
				Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
			}
			SizeChangedHandler();
		}

		private void ComboBoxEditor_Enter(object sender, EventArgs e) {
			comboBoxEdit.Select();
		}

		private void Editor_DataValueChanging(object sender, DataValueChangedEventArgs e) {
			foreach(var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangingHandler(this, e);
		}

		private void Editor_DataValueChanged(object sender, DataValueChangedEventArgs e) {
			foreach(var editor in _dependantEditors) {
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangedHandler(this, e);
		}

		private void Editor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			var parent = ParentEditor == null ? ParentContainer : ParentEditor.ParentContainer;
			if(e.KeyCode == Keys.Tab && !e.Shift) {
				//Utils.FocusNext(parent, this, true);
			} else if(e.KeyCode == Keys.Tab && e.Shift) {
				//Utils.FocusPrevious(parent, this, true);
			}
		}

		private void KeyDownHandler(object sender, KeyEventArgs e) {
			OnKeyDown(e);
			if(e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab) {
				//Utils.NextOrSearch(this, ParentContainer, (ParentForm as IBaseProgram));
			}
		}

		public void SetItems(DataList<Data<string>> dataList, string codeFieldName, string nameFieldName, string orderFieldName = null) {
			_itemDatas.Clear();

            comboBoxEdit.Properties.Items.Clear();
			if(dataList == null || dataList.Count == 0)
				return;
			if(orderFieldName != null)
				//dataList = dataList.OrderBy(d => long.Parse(Utils.CheckNull(d[orderFieldName], "999"))).ToDataList();

			comboBoxEdit.Properties.Items.BeginUpdate();
			if(!Required || _allCode != "" || _allCodeName != "") {
				_itemDatas.Add(_allCode);
				_dataList.Add(new Data<string>() { { codeFieldName, _allCode }, { nameFieldName, _allCodeName } });
				
				if(_showCode && _allCode != "" && _allCodeName != "")
					comboBoxEdit.Properties.Items.Add(String.Format("{0}[{1}]", _allCodeName, _allCode));
				else
					comboBoxEdit.Properties.Items.Add(_allCodeName);
			}
			foreach(var data in dataList) {
				_itemDatas.Add(data[codeFieldName]);
				_dataList.Add(new Data<string>(data));
				if(_showCode)
					comboBoxEdit.Properties.Items.Add(String.Format("{0}[{1}]", data[nameFieldName], data[codeFieldName]));
				else
					comboBoxEdit.Properties.Items.Add(data[nameFieldName]);
			}
			comboBoxEdit.Properties.Items.EndUpdate();
			if(_defaultCode != "")
				SetValue(_defaultCode, false);
			else
				comboBoxEdit.SelectedIndex = 0;
		}

		public void SetItems(Data<string> param,
								string queryUri = "/com/code_list/common_code_list",
								string codeFieldName = "CODE",
								string nameFieldName = "CODE_NM",
								string orderFieldName = "SORT_NO") {
			//var dataList = ApiUtils.SearchDataList(new HttpApiClient(Utils.GetClientSession()), queryUri, param);
			//SetItems(dataList, codeFieldName, nameFieldName, orderFieldName);
		}

        public Data<string> GetDataItem()
        {
            return _dataList[comboBoxEdit.SelectedIndex];
        }

        public Data<string> GetData() {
			var result = Data.New(Tuple.Create(FieldName, ""));
			if(comboBoxEdit.SelectedIndex != -1)
			{
				result[FieldName] = _itemDatas[comboBoxEdit.SelectedIndex];
				if(!NameFieldName.Equals("") && !NameFieldName.Equals(FieldName))
					result[NameFieldName] = comboBoxEdit.Properties.Items[comboBoxEdit.SelectedIndex].ToString();
			}
			return result;
		}

		public void SetData(Data<string> value, bool fireEvent = true) {
			if(!value.ContainsKey(FieldName))
				return;
			if(comboBoxEdit.Properties.Items == null || comboBoxEdit.Properties.Items.Count <= 0)
				return;
			var selectedIndex = comboBoxEdit.SelectedIndex;
			bool itemExisted = false;
			for (int i = 0; i < _itemDatas.Count; i++) {
				if (_itemDatas[i].Equals(value[FieldName]))
				{
					itemExisted = true;
					if (selectedIndex != i)
					{
						comboBoxEdit.SelectedIndex = i;
						if (fireEvent)
						{
							DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
							DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
						}
						break;
					}
				}
			}
			if (!itemExisted)
			{
				comboBoxEdit.SelectedIndex = 0;
				if (fireEvent)
				{
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public string GetValue() {
			if(comboBoxEdit.SelectedIndex != -1)
				return _itemDatas[comboBoxEdit.SelectedIndex];
			return "";
		}

		public void SetValue(string value, bool fireEvent = true) {
			if(comboBoxEdit.Properties.Items == null || comboBoxEdit.Properties.Items.Count <= 0)
				return;

			var selectedIndex = comboBoxEdit.SelectedIndex;
			bool itemExisted = false;
			for(int i = 0; i < _itemDatas.Count; i++) {
				if (_itemDatas[i].Equals(value))
				{
					itemExisted = true;
					if (selectedIndex != i)
					{
						comboBoxEdit.SelectedIndex = i;
						if (fireEvent)
						{
							DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
							DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
						}
						break;
					}
				}
			}
			if (!itemExisted)
			{
				comboBoxEdit.SelectedIndex = 0;
				if (fireEvent)
				{
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void Clear(bool fireEvent = true) {
			if(_defaultCode != "")
				SetValue(_defaultCode, fireEvent);
			else {
				if(comboBoxEdit.Properties.Items.Count > 0 && comboBoxEdit.SelectedIndex != 0) {
					comboBoxEdit.SelectedIndex = 0;
					if(fireEvent) {
						DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
						DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					}
				}
			}
		}

		private void Editor_Leave(object sender, EventArgs e) {
			if(_changed) {
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs());
				_changed = false;
			}
		}

		private void Editor_Modified(object sender, EventArgs e) {
			_changed = true;
		}

		private void Editor_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e) {
			DataValueChanging?.Invoke(this, new DataValueChangedEventArgs());
		}

		private void SizeChangedHandler() {
			if(Dock != DockStyle.None || !AutoSize) {
				var labelWidth = Label.ClientSize.Width + Label.Margin.Left + Label.Margin.Right;
				if(!Label.Visible)
					labelWidth = 0;
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
