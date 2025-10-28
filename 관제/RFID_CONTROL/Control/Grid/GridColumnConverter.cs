using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using DevExpress.XtraGrid.Columns;
using RFID_CONTROLLER.Controls.Programs;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Grid
{
    public enum ColumnType
	{
		CheckEdit, CodeHelperEdit, CodeTextEdit, DateEdit, FloatEdit, IntegerEdit,
		LookUpEdit, RadioEdit, TextEdit, YearMonthEdit, CheckYNEdit, MemoEdit, MemoExEdit, CustomEdit
	}

	public class ColumnOptions
	{
		public ColumnType ColumnType;
		public object DefaultValue = null;
		public HorzAlignment HAlignment = HorzAlignment.Default;
		public bool ReadOnly = false;
		public bool ReadOnlyWithColorChange = true;
		public bool Required = false;
		public DefaultBoolean AllowSort = DefaultBoolean.Default;
		public bool AllowFilter = true;
		public bool AllowFocus = true;
		public DefaultBoolean AllowMerge = DefaultBoolean.False;
		public MaskType MaskType = MaskType.None;
		public string EditMask = "";
		public bool UseMaskAsDisplayFormat = true;
		public int MaxLength = 0;
		public int MaxWidth = 0;
		public int MinWidth = 20;
		public object MaxValue = null;
		public object MinValue = null;
		public bool NumberOnly = false;
		public int Precision = 2;
		public string DisplayFormat = "";
		public string ValueChecked = "Y";
		public string ValueUnchecked = "N";
		public bool ShowSummary = false;
		public DataList<string> DependantFieldNames = null;
		public Color foreColor = Color.Empty;
	}

	public class DataColumnOptions : ColumnOptions
	{
		public string QueryUri = "/com/code_list/common_code_list";
		public Data<string> QueryParam = null;
		public DataList<Data<string>> DataList = null;
		public string CodeFieldName;
		public string NameFieldName;
		public string OrderFieldName = null;
		public bool ShowCode = false;
		public string AllCode = "";
		public string AllCodeName = "";
	}

	public class CodeHelperColumnOptions : ColumnOptions
	{
		public string QueryUri = "/com/code_list/common_code_list";
		public string CodeQueryUri = "/com/code/common_code";
		public CharacterCasing CharacterCasing = CharacterCasing.Upper;
		public string NameFieldName = null;
		//public CodeHelperWindow.Setting CodeHelperSetting = null;
		public bool InputCodeNM = false;
	}

	public static class GridColumnConverter
	{
		//private static Logger _log = Logger.Get(typeof(GridColumnConverter));
		private static Color _requiredColor = Color.FromArgb(0xE3, 0x18, 0x37);

		private static void SetColumnSetting(GridColumn column, ColumnOptions options)
		{
			if (options.Required)
			{
				column.AppearanceHeader.ForeColor = _requiredColor;
			}
			else
			{
				column.AppearanceHeader.ForeColor = Color.Empty;
			}
			if (options.ReadOnly)
			{
				if (options.ColumnType == ColumnType.MemoExEdit)
					column.OptionsColumn.ReadOnly = true;
				else
					column.OptionsColumn.AllowEdit = false;
			}
			else
			{
				if (options.ColumnType == ColumnType.MemoExEdit)
					column.OptionsColumn.ReadOnly = false;
				else
					column.OptionsColumn.AllowEdit = true;
			}
			if (options.ReadOnly && options.ReadOnlyWithColorChange)
			{
				column.AppearanceHeader.BackColor = Color.FromArgb(0xB0, 0xB0, 0xB0);
			}

			column.OptionsColumn.AllowSort = options.AllowSort;
			column.OptionsFilter.AllowFilter = options.AllowFilter;
			column.OptionsColumn.AllowFocus = options.AllowFocus;
			column.OptionsColumn.AllowMerge = options.AllowMerge;
			
			if (!Color.Empty.Equals(options.foreColor))
			{
                column.AppearanceCell.ForeColor = options.foreColor;
            }


        }

		public static void SetRequired(this GridColumn column, bool required)
		{
			var options = column.Tag as ColumnOptions;
			if (options != null)
			{
				options.Required = required;
			}
			if (required)
			{
				column.AppearanceHeader.ForeColor = _requiredColor;
			}
			else
			{
				column.AppearanceHeader.ForeColor = Color.Empty;
			}
		}

		public static void SetReadOnly(this GridColumn column, bool readOnly)
		{
			var options = column.Tag as ColumnOptions;
			if (readOnly)
			{
				if (options.ColumnType == ColumnType.MemoExEdit)
					column.OptionsColumn.ReadOnly = true;
				else
					column.OptionsColumn.AllowEdit = false;
			}
			else
			{
				if (options.ColumnType == ColumnType.MemoExEdit)
					column.OptionsColumn.ReadOnly = false;
				else
					column.OptionsColumn.AllowEdit = true;
			}

			if (options.ReadOnlyWithColorChange)
			{
				if (readOnly)
				{
					column.AppearanceHeader.BackColor = Color.FromArgb(0xB0, 0xB0, 0xB0);
				}
				else
				{
					column.AppearanceHeader.BackColor = Color.Empty;
				}
			}
		}

		public static void AsIntegerEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null)
				options = new ColumnOptions();
			options.ColumnType = ColumnType.IntegerEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemTextEdit();
			var minValue = options.MinValue != null && Convert.ToInt64(options.MinValue) >= 0 ? Convert.ToInt64(options.MinValue) : Int64.MinValue;
			itemEdit.KeyPress += (s, e) => {
				if (minValue >= 0)
				{
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
						e.Handled = true;
				}
				else
				{
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back) || e.KeyChar == '-'))
						e.Handled = true;
				}
			};
			itemEdit.Validating += (s, e) => {
				new ErrorProvider().SetError(s as DevExpress.XtraEditors.TextEdit, null);
			};
			itemEdit.Validating += (s, e) => {
				var edit = s as DevExpress.XtraEditors.TextEdit;
				if (edit.Text == "")
					return;
				if (!Int64.TryParse(edit.Text.Replace(",", ""), out var value))
					return;

				var _minValue = options.MinValue != null ? Convert.ToInt64(options.MinValue) : Int64.MinValue;
				var _maxValue = options.MaxValue != null ? Convert.ToInt64(options.MaxValue) : Int64.MaxValue;
				if (value < _minValue || value > _maxValue)
				{
					e.Cancel = true;
					edit.Select(0, edit.Text.Length);
					edit.Focus();
					new ErrorProvider().SetError(edit, "입력가능 수치를 벋어났습니다.");
				}
				else
				{
					e.Cancel = false;
					new ErrorProvider().SetError(edit, null);
				}
			};
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			column.DisplayFormat.FormatType = FormatType.Numeric;
			if (options.DisplayFormat != "")
				column.DisplayFormat.FormatString = options.DisplayFormat;
			else
				column.DisplayFormat.FormatString = "{0:#,##0}";
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;

			if (options.ShowSummary)
			{
				column.SummaryItem.FieldName = column.FieldName;
				column.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
				column.SummaryItem.DisplayFormat = column.DisplayFormat.FormatString;
                
                var summaryItem = new DevExpress.XtraGrid.GridGroupSummaryItem()
				{
					FieldName = column.FieldName,
					SummaryType = DevExpress.Data.SummaryItemType.Sum,
					DisplayFormat = column.DisplayFormat.FormatString,
					ShowInGroupColumnFooter = column
				};
				if (column.View is GridView)
					(column.View as GridView).GroupSummary.Add(summaryItem);
				else if (column.View is AdvBandedGridView)
					(column.View as AdvBandedGridView).GroupSummary.Add(summaryItem);
				else if (column.View is BandedGridView)
					(column.View as BandedGridView).GroupSummary.Add(summaryItem);
			}
		}

		public static void AsFloatEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null)
				options = new ColumnOptions();
			options.ColumnType = ColumnType.FloatEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemTextEdit();
			var minValue = options.MinValue != null && Convert.ToDouble(options.MinValue) >= 0 ? Convert.ToDouble(options.MinValue) : double.MinValue;
			itemEdit.KeyPress += (s, e) => {
				if (minValue >= 0)
				{
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back) || e.KeyChar == '.'))
						e.Handled = true;
				}
				else
				{
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back) || e.KeyChar == '.' || e.KeyChar == '-'))
						e.Handled = true;
				}
			};
			itemEdit.Validating += (s, e) => {
				new ErrorProvider().SetError(s as DevExpress.XtraEditors.TextEdit, null);
			};
			itemEdit.Validating += (s, e) => {
				var edit = s as DevExpress.XtraEditors.TextEdit;
				if (edit.Text == "")
					return;
				if (!double.TryParse(edit.Text.Replace(",", ""), out var value))
					return;

				var _minValue = options.MinValue != null ? Convert.ToDouble(options.MinValue) : double.MinValue;
				var _maxValue = options.MaxValue != null ? Convert.ToDouble(options.MaxValue) : double.MaxValue;
				if (value < _minValue || value > _maxValue)
				{
					e.Cancel = true;
					edit.Select(0, edit.Text.Length);
					edit.Focus();
					new ErrorProvider().SetError(edit, "입력가능 수치를 벋어났습니다.");
				}
				else
				{
					e.Cancel = false;
					new ErrorProvider().SetError(edit, null);
				}
			};
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			column.DisplayFormat.FormatType = FormatType.Numeric;
			if (options.DisplayFormat != "")
				column.DisplayFormat.FormatString = options.DisplayFormat;
			else
			{
				var format = "";
				format = format.PadLeft(options.Precision, '0');
				column.DisplayFormat.FormatString = "{0:#,##0." + format + "}";
			}
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;

			if (options.ShowSummary)
			{
				column.SummaryItem.FieldName = column.FieldName;
				column.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
				column.SummaryItem.DisplayFormat = column.DisplayFormat.FormatString;

				var summaryItem = new DevExpress.XtraGrid.GridGroupSummaryItem()
				{
					FieldName = column.FieldName,
					SummaryType = DevExpress.Data.SummaryItemType.Sum,
					DisplayFormat = column.DisplayFormat.FormatString,
					ShowInGroupColumnFooter = column
				};
				if (column.View is GridView)
					(column.View as GridView).GroupSummary.Add(summaryItem);
				else if (column.View is AdvBandedGridView)
					(column.View as AdvBandedGridView).GroupSummary.Add(summaryItem);
				else if (column.View is BandedGridView)
					(column.View as BandedGridView).GroupSummary.Add(summaryItem);
			}
		}

		public static void AsCodeTextEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null)
				options = new ColumnOptions();
			options.ColumnType = ColumnType.CodeTextEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemTextEdit();
			itemEdit.AutoHeight = false;
			itemEdit.CharacterCasing = CharacterCasing.Upper;
			//itemEdit.Mask.EditMask = "[A-Z0-9]*";
			itemEdit.Mask.EditMask = options.EditMask == "" ? "[A-Z0-9_.\\-]*|[*]?" : options.EditMask;
			itemEdit.Mask.MaskType = options.MaskType == MaskType.None ? MaskType.RegEx : options.MaskType;
			itemEdit.Mask.UseMaskAsDisplayFormat = options.UseMaskAsDisplayFormat;
			itemEdit.MaxLength = options.MaxLength;
			if (options.NumberOnly)
			{
				itemEdit.Mask.EditMask = "[0-9]*";
				itemEdit.Mask.MaskType = MaskType.RegEx;
				/*
				itemEdit.KeyPress += (s, e) => {
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
						e.Handled = true;
				};
				*/
			}
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
		}

		public static void AsDateEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null)
				options = new ColumnOptions();
			options.ColumnType = ColumnType.DateEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemDateEdit();
			itemEdit.AutoHeight = false;
			itemEdit.DisplayFormat.FormatType = FormatType.DateTime;
			itemEdit.DisplayFormat.FormatString = "d";
			itemEdit.EditFormat.FormatType = FormatType.DateTime;
			itemEdit.EditFormat.FormatString = "d";
			itemEdit.Mask.MaskType = MaskType.RegEx;
			itemEdit.Mask.EditMask = "([1-9][0-9][0-9][0-9])-(0?[1-9]|1[012])-([012]?[1-9]|[123]0|31)";
			itemEdit.Mask.UseMaskAsDisplayFormat = true;
			itemEdit.AllowNullInput = DefaultBoolean.True;
			itemEdit.NullValuePromptShowForEmptyValue = true;
			itemEdit.ShowNullValuePromptWhenFocused = true;
			itemEdit.NullValuePrompt = "";
			itemEdit.NullDate = "";
			itemEdit.NullText = "";
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
		}

		public static void AsYearMonthEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.YearMonthEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemDateEdit();
			itemEdit.AutoHeight = false;
			itemEdit.DisplayFormat.FormatType = FormatType.DateTime;
			itemEdit.DisplayFormat.FormatString = "d";
			itemEdit.EditFormat.FormatType = FormatType.DateTime;
			itemEdit.EditFormat.FormatString = "d";
			itemEdit.Mask.MaskType = MaskType.RegEx;
			itemEdit.Mask.EditMask = "([1-9][0-9][0-9][0-9])-(0?[1-9]|1[012])";
			itemEdit.Mask.UseMaskAsDisplayFormat = true;
			itemEdit.AllowNullInput = DefaultBoolean.True;
			itemEdit.NullValuePromptShowForEmptyValue = true;
			itemEdit.ShowNullValuePromptWhenFocused = true;
			itemEdit.NullValuePrompt = "";
			itemEdit.NullDate = "";
			itemEdit.NullText = "";
			itemEdit.VistaCalendarViewStyle = DevExpress.XtraEditors.VistaCalendarViewStyle.YearView;
			itemEdit.VistaCalendarInitialViewStyle = DevExpress.XtraEditors.VistaCalendarInitialViewStyle.YearView;
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
		}

		public static void AsTextEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.TextEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemTextEdit();
			itemEdit.AutoHeight = false;
			itemEdit.Mask.EditMask = options.EditMask;
			itemEdit.Mask.MaskType = options.MaskType;
			itemEdit.Mask.UseMaskAsDisplayFormat = options.UseMaskAsDisplayFormat;
			itemEdit.MaxLength = options.MaxLength;
			if (options.NumberOnly)
			{
				itemEdit.Mask.EditMask = "[0-9]*";
				itemEdit.Mask.MaskType = MaskType.RegEx;
				/*
				itemEdit.KeyPress += (s, e) => {
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
						e.Handled = true;
				};
				*/
			}
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Default;
		}

		public static void AsCodeHelperEdit(this GridColumn column, CodeHelperColumnOptions options = null)
		{
			if (options == null) options = new CodeHelperColumnOptions();
			options.ColumnType = ColumnType.CodeHelperEdit;
			SetColumnSetting(column, options);
			if (options.NameFieldName != null || !options.NameFieldName.Equals(""))
			{
				if (options.DependantFieldNames == null)
					options.DependantFieldNames = new DataList<string>();
			}
			column.Tag = options;
			var itemEdit = new RepositoryItemButtonEdit();
			itemEdit.AutoHeight = false;
			itemEdit.CharacterCasing = options.CharacterCasing;
			if (options.InputCodeNM)
				itemEdit.Mask.EditMask = "";
			else if (options.CharacterCasing == CharacterCasing.Normal)
				itemEdit.Mask.EditMask = "[A-Za-z0-9_.\\-]*|[*]?";
			else if (options.CharacterCasing == CharacterCasing.Lower)
				itemEdit.Mask.EditMask = "[a-z0-9_.\\-]*|[*]?";
			else
				itemEdit.Mask.EditMask = "[A-Z0-9_.\\-]*|[*]?";
			itemEdit.Mask.MaskType = options.InputCodeNM ? MaskType.None : MaskType.RegEx;
			itemEdit.MaxLength = options.MaxLength;
			if (options.NumberOnly)
			{
				itemEdit.Mask.EditMask = "[0-9]*";
				itemEdit.Mask.MaskType = MaskType.RegEx;
				/*
				itemEdit.KeyPress += (s, e) => {
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
						e.Handled = true;
				};
				*/
			}
			
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
		}

		public static void AsCheckEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.CheckEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemCheckEdit();
			itemEdit.AutoHeight = false;
			itemEdit.ValueChecked = options.ValueChecked;
			itemEdit.ValueUnchecked = options.ValueUnchecked;
			itemEdit.ValueGrayed = "";
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
		}

		public static void AsCheckYNEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.CheckYNEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemCheckEdit();
			itemEdit.AutoHeight = false;
			itemEdit.ValueChecked = "Y";
			itemEdit.ValueUnchecked = "N";
			itemEdit.ValueGrayed = "";
			column.ColumnEdit = itemEdit;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
		}

		public static void AsMemoEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.MemoEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemMemoEdit();
			itemEdit.AutoHeight = true;
			itemEdit.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
			itemEdit.WordWrap = true;
			itemEdit.MaxLength = options.MaxLength;
			itemEdit.ScrollBars = ScrollBars.Vertical;
			column.ColumnEdit = itemEdit;
			column.AppearanceCell.TextOptions.VAlignment = VertAlignment.Center;
			(column.View as GridView).OptionsView.RowAutoHeight = true;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Default;
		}

		public static void AsMemoExEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.MemoExEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			var itemEdit = new RepositoryItemMemoExEdit();
			itemEdit.Buttons[0].Kind = ButtonPredefines.Ellipsis;
			itemEdit.ShowIcon = false;
			itemEdit.AutoHeight = false;
			itemEdit.MaxLength = options.MaxLength;
			itemEdit.PopupFormSize = new Size(340, 100);
			itemEdit.ScrollBars = ScrollBars.Vertical;
			column.ColumnEdit = itemEdit;
			column.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Default;
		}

		public static void AsCustomEdit(this GridColumn column, ColumnOptions options = null)
		{
			if (options == null) options = new ColumnOptions();
			options.ColumnType = ColumnType.CustomEdit;
			SetColumnSetting(column, options);
			column.Tag = options;
			if (options.MaxWidth != 0)
				column.MaxWidth = options.MaxWidth;
			if (options.MinWidth != 20)
				column.MinWidth = options.MinWidth;
			if (options.HAlignment != HorzAlignment.Default)
				column.AppearanceCell.TextOptions.HAlignment = options.HAlignment;
			else
				column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Default;
		}
	}
}