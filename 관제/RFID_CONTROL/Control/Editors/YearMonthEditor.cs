using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors
{
	public partial class YearMonthEditor : UserControl, IBaseEditor
	{
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;

		private bool _selectAllDone = false;
		private string _fieldName;
		private bool _required = false;
		private bool _changed = false;
		private Control _parentContainer = null;
		private BorderStyle _borderStyle = BorderStyle.None;
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		private string _defaultValue = "";

		public YearMonthEditor()
		{
			InitializeComponent();
			BorderStyle = BorderStyle.FixedSingle;
		}

		[Category("UserDefine")]
		[Description("BorderStyle")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(BorderStyle), "FixedSingle")]
		new public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				_borderStyle = value;
				if (_borderStyle == BorderStyle.None)
				{
					Editor.Width += HelpButtonVisible ? 1 : 2;
					Editor.Height += 2;
					Editor.Margin = new Padding(0);
				}
				else if (_borderStyle == BorderStyle.Fixed3D)
				{
				}
				else if (_borderStyle == BorderStyle.FixedSingle)
				{
					Editor.Width -= HelpButtonVisible ? 1 : 2;
					Editor.Height -= 2;
					Editor.Margin = new Padding(1, 1, HelpButtonVisible ? 0 : 1, 1);
				}
			}
		}

		[Category("UserDefine")]
		[Description("Caption")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(string), "레이블")]
		public string Caption
		{
			get => DisplayLabel.Text; set => DisplayLabel.Text = value;
		}

		[Category("UserDefine")]
		[Description("DefaultValue")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DefaultValue
		{
			get => _defaultValue; set => _defaultValue = value;
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
		public DateEdit Editor
		{
			get
			{
				return yearMonthEdit;
			}
		}

		[Category("UserDefine")]
		[Description("EditorSize")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "63, 25")]
		public Size EditorSize
		{
			get => Editor.Size; set => Editor.Size = value;
		}

		[Category("UserDefine")]
		[Description("FieldName")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FieldName
		{
			get => _fieldName; set => _fieldName = value;
		}

		[Category("UserDefine")]
		[Description("HAlignment")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(DevExpress.Utils.HorzAlignment), "Default")]
		public DevExpress.Utils.HorzAlignment HAlignment
		{
			get => Editor.Properties.Appearance.TextOptions.HAlignment;
			set => Editor.Properties.Appearance.TextOptions.HAlignment = value;
		}

		[Category("UserDefine")]
		[Description("HelpButton")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SimpleButton HelpButton
		{
			get
			{
				return HelpBtn;
			}
		}

		[Category("UserDefine")]
		[Description("HelpButtonVisible")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "true")]
		public bool HelpButtonVisible
		{
			get => HelpBtn.Visible;
			set
			{
				HelpBtn.Visible = value;
				if (HelpBtn.Visible)
				{
					if (_borderStyle == BorderStyle.FixedSingle)
					{
						if (Editor.Margin != new Padding(1, 1, 0, 1))
						{
							Editor.Width += 1;
							Editor.Margin = new Padding(1, 1, 0, 1);
						}
					}
				}
				else
				{
					if (_borderStyle == BorderStyle.FixedSingle)
					{
						if (Editor.Margin != new Padding(1))
						{
							Editor.Width -= 1;
							Editor.Margin = new Padding(1);
						}
					}
				}
			}
		}

		[Category("UserDefine")]
		[Description("Label")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Label Label
		{
			get
			{
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
		public Control ParentContainer
		{
			get => _parentContainer == null ? Parent : _parentContainer;
			set => _parentContainer = value;
		}

		[Category("UserDefine")]
		[Description("ReadOnly")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool ReadOnly
		{
			get => Editor.ReadOnly;
			set => Editor.ReadOnly = value;
		}

		[Category("UserDefine")]
		[Description("Required")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(bool), "false")]
		public bool Required
		{
			get => _required;
			set
			{
				_required = value;
				if (_required)
				{
					DisplayLabel.ForeColor = Color.FromArgb(0xE3, 0x18, 0x37);
				}
				else
				{
					DisplayLabel.ForeColor = Color.FromArgb(0x44, 0x44, 0x44);
				}
			}
		}

		public IBaseEditor ParentEditor { get; }

		public bool IsValid(bool messsage = true)
		{
			Editor.DoValidate();
			if (Required && Editor.Text.Trim().Equals(""))
			{
				if (messsage)
					//Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
				//Utils.DelayFocus(Editor);
				return false;
			}
			return true;
		}

		private void YearMonthEditor_Load(object sender, EventArgs e)
		{
			DataValueChanged += Editor_DataValueChanged;
			DataValueChanging += Editor_DataValueChanging;
			HelpBtn.Click += HelpButton_Click;
			Editor.Closed += Editor_Closed;
			SizeChangedHandler();
		}

		private void YearMonthEditor_Enter(object sender, EventArgs e)
		{
			Editor.Select();
		}

		private void Editor_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
		{
			//Utils.DelayFocus(this);
		}

		private void Editor_DataValueChanging(object sender, DataValueChangedEventArgs e)
		{
			foreach (var editor in _dependantEditors)
			{
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangingHandler(this, e);
		}

		private void Editor_DataValueChanged(object sender, DataValueChangedEventArgs e)
		{
			foreach (var editor in _dependantEditors)
			{
				editor.Clear();
			}
			//(ParentForm as IBaseProgram)?.ValueChangedHandler(this, e);
		}

		private void HelpButton_Click(object sender, EventArgs e)
		{
			if (ReadOnly) return;
			Editor.ShowPopup();
		}

		private void Editor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			var parent = ParentEditor == null ? ParentContainer : ParentEditor.ParentContainer;
			if (e.KeyCode == Keys.Tab && !e.Shift)
			{
				//Utils.FocusNext(parent, this, true);
			}
			else if (e.KeyCode == Keys.Tab && e.Shift)
			{
				//Utils.FocusPrevious(parent, this, true);
			}
		}

		private void KeyDownHandler(object sender, KeyEventArgs e)
		{
			OnKeyDown(e);
			if (e.KeyCode == Keys.F1)
			{
				if (ReadOnly) return;
				Editor.ShowPopup();
			}
			else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
			{
				//Utils.NextOrSearch(this, ParentContainer, (ParentForm as IBaseProgram));
			}
		}

		public Data<string> GetData()
		{
			var result = new Data<string>();
			result[FieldName] = Editor.Text.Replace("-", "");
			return result;
		}

		public void SetData(Data<string> value, bool fireEvent = true)
		{
			if (!value.ContainsKey(FieldName)) return;
			if (GetValue() != value[FieldName].Replace("-", ""))
			{
				if (value[FieldName] == "")
				{
					Editor.Text = value[FieldName];
				}
				else
				{
					value[FieldName] = value[FieldName].Replace("-", "");
					if (value[FieldName].Length > 6)
						value[FieldName] = value[FieldName].Remove(6);
					try
					{
						var format = Editor.Properties.VistaCalendarViewStyle == VistaCalendarViewStyle.YearsGroupView ? "yyyy" : "yyyyMM";
						Editor.DateTime = DateTime.ParseExact(value[FieldName], format, null);
					}
					catch (Exception ex)
					{
						//Utils.ShowErrorMessage(ex.Message + "\r\n\r\n\r\n\r\n\r\n" + ex.StackTrace);
						//Utils.DelayFocus(Editor);
					}
				}
				if (fireEvent)
				{
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public string GetValue()
		{
			return Editor.Text.Replace("-", "");
		}

		public void SetValue(string value, bool fireEvent = true)
		{
			if (GetValue() != value.Replace("-", ""))
			{
				if (value == "")
				{
					Editor.Text = value;
				}
				else
				{
					value = value.Replace("-", "");
					if (value.Length > 6)
						value = value.Remove(6);
					try
					{
						var format = Editor.Properties.VistaCalendarViewStyle == VistaCalendarViewStyle.YearsGroupView ? "yyyy" : "yyyyMM";
						Editor.DateTime = DateTime.ParseExact(value, format, null);
					}
					catch (Exception ex)
					{
						//Utils.ShowErrorMessage(ex.Message + "\r\n\r\n\r\n\r\n\r\n" + ex.StackTrace);
						//Utils.DelayFocus(Editor);
					}
				}
				if (fireEvent)
				{
					DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
					DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				}
			}
		}

		public void Clear(bool fireEvent = true)
		{
			SetValue(_defaultValue, fireEvent);
		}

		private void Editor_Leave(object sender, EventArgs e)
		{
			_selectAllDone = false;
			if (_changed)
			{
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs());
				_changed = false;
			}
		}

		private void Editor_Modified(object sender, EventArgs e)
		{
			_changed = true;
		}

		private void Editor_TextChanged(object sender, EventArgs e)
		{
			DataValueChanging?.Invoke(this, new DataValueChangedEventArgs());
		}

		private void SizeChangedHandler()
		{
			if (Dock != DockStyle.None || !AutoSize)
			{
				var labelWidth = Label.ClientSize.Width + Label.Margin.Left + Label.Margin.Right;
				if (!Label.Visible) labelWidth = 0;
				var helpBtnWidth = HelpBtn.ClientSize.Width + HelpBtn.Margin.Left + HelpBtn.Margin.Right;
				if (!HelpBtn.Visible) helpBtnWidth = 0;
				var editorMargin = Editor.Margin.Left + Editor.Margin.Right;
				Editor.Width = ClientSize.Width - labelWidth - editorMargin - helpBtnWidth;
			}
		}

		private void _SizeChanged(object sender, EventArgs e)
		{
			SizeChangedHandler();
		}

		private void Editor_Enter(object sender, EventArgs e)
		{
			if (MouseButtons == MouseButtons.None)
			{
				yearMonthEdit.SelectAll();
				_selectAllDone = true;
			}
		}

		private void Editor_MouseUp(object sender, MouseEventArgs e)
		{
			if (!_selectAllDone && yearMonthEdit.SelectionLength == 0)
			{
				_selectAllDone = true;
				yearMonthEdit.SelectAll();
			}
		}
	}
}
