using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Columns;
using System.Diagnostics;
using System.Threading.Tasks;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.BandedGrid;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;
using RFID_CONTROLLER.Collections;
using RFID_CONTROLLER.Controls.Programs;

namespace RFID_CONTROLLER.Controls.Grid
{
    public class GridValueChangedEventArgs : CellValueChangedEventArgs
	{
		private bool _fromUiChanged = true;
		private ColumnOptions _options = null;

		public bool FromUiChanged { get => _fromUiChanged; }
		public ColumnOptions ColumnOptions { get => _options; }

		public GridValueChangedEventArgs(
			int rowHandle, GridColumn column, object value, ColumnOptions options, bool fromUiChanged
		) : base(rowHandle, column, value)
		{
			_options = options;
			_fromUiChanged = fromUiChanged;
		}
	}

	public class GridOptions
	{
		public bool AllowAddRow = true;
		public bool AllowCellMerge = false;
		public bool AllowAddRowWithNoSearch = false;
		public bool CheckBoxSelector = true;
		public bool CheckBoxSelectorWithLookup = false;
		public bool BestFitColumns = true;
		public bool ShowFooter = false;
	}

	public class SearchOptions
	{
		public string QueryUri = "";
		public Data<string> SearchParam = null;
		public Func<Data<string>, bool> FocusPred = null;
		public bool AddRowIfNoData = false;
		public bool ShowNoDataMessage = true;
		public string NoDataMessage = "조회할 자료가 없습니다.";
	}

	public enum ClipboardPasteType
	{
		FirstHeader, OnlyData
	}

	public delegate void GridValueChangedEventHandler(object sender, GridValueChangedEventArgs a);

	public static class GridExtension
	{

		private static bool _dataValidating = false;

		public static bool _changedFromHelpPopup = false;

		public const string RowIndexFieldName = "RowIndex";

		public static void ApplyDefaultSettings(this GridView gridView, GridOptions gridOptions = null)
		{
			var gridControl = gridView.GridControl;
			var program = gridControl.FindForm() as IBaseProgram;
			if (gridOptions == null) gridView.Tag = new GridOptions();
			else gridView.Tag = gridOptions;

			gridView.OptionsClipboard.AllowCopy = DefaultBoolean.False;
			gridView.OptionsClipboard.PasteMode = DevExpress.Export.PasteMode.Update;
			gridView.KeyDown += (s, e) =>
			{
				if (e.KeyCode == Keys.F3)
					e.Handled = true;
			};

			gridView.OptionsView.AllowCellMerge = gridOptions != null && gridOptions.AllowCellMerge;
			gridView.GridControl.LookAndFeel.UseDefaultLookAndFeel = false;

			//컬럼 높이
			gridView.ColumnPanelRowHeight = 30;
			gridView.RowHeight = 30;

			//우클릭 메뉴 X
			gridView.OptionsMenu.EnableColumnMenu = false;
			gridView.OptionsMenu.EnableGroupPanelMenu = false;

			//엔터시 옆으로 이동
			gridView.OptionsNavigation.EnterMoveNextColumn = true;

			//스타일
			gridView.Appearance.FocusedRow.ForeColor = Color.Blue;
			gridView.Appearance.SelectedRow.ForeColor = Color.Blue;

			gridView.Appearance.FocusedRow.BackColor = Color.FromArgb(224, 224, 224);
			gridView.Appearance.FocusedCell.BackColor = Color.FromArgb(224, 224, 224);
			gridView.Appearance.SelectedRow.BackColor = Color.FromArgb(224, 224, 224);

			gridView.Appearance.Row.Font = new Font("돋움", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(129)));
			gridView.Appearance.Row.ForeColor = Color.FromArgb(0x44, 0x44, 0x44);

			gridView.Appearance.HideSelectionRow.BackColor = Color.WhiteSmoke;

			//홀짝구분하여 색상표시
			gridView.OptionsView.EnableAppearanceOddRow = true;
			gridView.Appearance.OddRow.BackColor = Color.FromArgb(0xF7, 0xFA, 0xFF);

			//컬럼헤드
			gridView.Appearance.HeaderPanel.Font = new Font("돋움", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

			if (gridView is BandedGridView)
			{
				var bandedGridView = gridView as BandedGridView;
				bandedGridView.Appearance.BandPanel.Font = new Font("돋움", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));
				bandedGridView.BandPanelRowHeight = 30;
				bandedGridView.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			}

			if (gridView is AdvBandedGridView)
			{
				var advBandedGridView = gridView as AdvBandedGridView;
				advBandedGridView.Appearance.BandPanel.Font = new Font("돋움", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));
				advBandedGridView.BandPanelRowHeight = 30;
				advBandedGridView.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			}

			//하단 합계
			gridView.Appearance.FooterPanel.Font = new Font("돋움", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

			//Caption
			gridView.Appearance.ViewCaption.Options.UseFont = true;
			gridView.Appearance.ViewCaption.Font = new Font("돋움", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

			//헤더 가운데 정렬
			gridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

			//인디케이터 설정
			gridView.IndicatorWidth = 45;
			gridView.OptionsView.ShowIndicator = true;
			gridView.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(CustomDrawRowIndicator);

			//gridView.OptionsCustomization.AllowQuickHideColumns = false;

			if (gridOptions != null && gridOptions.BestFitColumns) gridView.BestFitColumns();

			if (gridOptions != null && gridOptions.ShowFooter) gridView.OptionsView.ShowFooter = gridOptions.ShowFooter;

			gridView.DoubleClick += program.GridRowDoubleClickHandler;
			
			gridView.MouseMove += (s, e) => {
				GridHitInfo hitInfo = (s as GridView).CalcHitInfo(e.Location);
				if (!hitInfo.InRowCell || hitInfo.Column == null)
				{
					(s as GridView).GridControl.Cursor = Cursors.Default;
					return;
				}
				
				(s as GridView).GridControl.Cursor = Cursors.Default;
			};

			gridView.FocusedRowChanged += (s, e) => {
				if (gridView.IsValidRowHandle(e.FocusedRowHandle))
				{
					program.GridRowClickHandler(s, e);
				}
			};

			var contextMenuStrip = new ContextMenuStrip();

			//contextMenuStrip.Items.Add("화면캡쳐").Click += (s, e) => {
			//	Utils.ScreenCapture(Utils.GetMainForm() as Form, gridControl.FindForm().Text);
			//};
			//contextMenuStrip.Items.Add("프로그램정보").Click += (s, e) => {
			//	Utils.ShowInfoMessage(Utils.GetProgramInfo(gridControl.FindForm().Name));
			//};

			//contextMenuStrip.Items.Add("-");

			//contextMenuStrip.Items.Add("Data Export").Click += (s, e) => {
			//	Utils.ExportExcelHandler(gridView, gridControl.FindForm().Text.Replace(" ", "_"));
			//};

			//if (Utils.GetClientSession().UserInfo.MANAGER_YN.Equals("Y"))
			//{
			//	contextMenuStrip.Items.Add("Data to Json").Click += (s, e) =>
			//	{
			//		var dataList = gridView.GetDataList();
			//		if (dataList == null || dataList.Count < 1)
			//		{
			//			Utils.ShowWarnMessage("조회 후 진행해주세요");
			//			return;
			//		}
			//		Utils.ShowInfoMessage(dataList.ToString());
			//	};
			//}

			//contextMenuStrip.Items.Add("Print").Click += (s, e) => {
			//	//gridView.PrintDialog();
			//	gridView.ShowPrintPreview();
			//};

			//contextMenuStrip.Items.Add("-");

			//var menuItemCellCopy = new ToolStripMenuItem();
			//menuItemCellCopy.Text = "선택한셀 복사";
			//menuItemCellCopy.ShowShortcutKeys = true;
			//menuItemCellCopy.ShortcutKeys = Keys.Control | Keys.C;
			//menuItemCellCopy.Click += (s, e) =>
			//{
			//	if (gridView.IsEditing)
			//	{
			//		if (gridView.EditingValue.ToString() == "")
			//			return;
			//		Clipboard.SetText(gridView.EditingValue.ToString());
			//	}
			//	else
			//	{
			//		if (gridView.RowCount == 0 || gridView.GetFocusedDisplayText() == "")
			//			return;
			//		Clipboard.SetText(gridView.GetFocusedDisplayText());
			//	}
			//};
			//contextMenuStrip.Items.Add(menuItemCellCopy);

			//contextMenuStrip.Items.Add("선택한행 복사").Click += (s, e) => {
			//	var selectedRowHandles = gridView.GetSelectedRows();
			//	CopyToClipboard(gridView, selectedRowHandles);
			//};

			//contextMenuStrip.Items.Add("모든자료 복사").Click += (s, e) => {
			//	CopyToClipboard(gridView, Enumerable.Range(0, gridView.RowCount).ToArray());
			//};

			//contextMenuStrip.Items.Add("-");

			//contextMenuStrip.Items.Add("행추가").Click += (s, e) => {
			//	if (gridView.OptionsBehavior.Editable && (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch))
			//	{
			//		if (!gridView.GetGridOptions().AllowAddRowWithNoSearch)
			//		{
			//			if (gridView.GridControl.DataSource == null)
			//			{
			//				Utils.ShowWarnMessage("조회 후 진행해주세요");
			//				return;
			//			}
			//		}
			//		gridView.AddNewRow(data: null);
			//		gridView.FocusedColumn = gridView.FirstFocusableField();
			//	}
			//};

			//contextMenuStrip.Items.Add("-");

			//contextMenuStrip.Items.Add("자료 붙여넣기 (첫째줄 헤더)").Click += (s, e) => {
			//	if (gridView.OptionsBehavior.Editable && (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch))
			//	{
			//		PasteFromClipboard(gridView, ClipboardPasteType.FirstHeader);
			//	}
			//};

			//contextMenuStrip.Items.Add("자료 붙여넣기").Click += (s, e) => {
			//	if (gridView.OptionsBehavior.Editable && (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch))
			//	{
			//		PasteFromClipboard(gridView, ClipboardPasteType.OnlyData);
			//	}
			//};

			//contextMenuStrip.Items.Add("자료 검증").Click += (s, e) => {
			//	var dataList = gridView.GetSelectedDataList();
			//	if (dataList == null || dataList.Count < 1)
			//	{
			//		Utils.ShowWarnMessage("검증할 자료가 없습니다");
			//		return;
			//	}
			//	var keys = dataList[0].Keys;
			//	foreach (var data in dataList)
			//	{
			//		foreach (var key in keys)
			//		{
			//			var column = gridView.Columns[key];
			//			if (column != null && column.GetColumnOptions().ColumnType == ColumnType.CodeHelperEdit)
			//			{
			//				var rowHandle = int.Parse(data[GridExtension.RowIndexFieldName]);
			//				if (!data[key].Equals("") && data[(column.GetColumnOptions() as CodeHelperColumnOptions).NameFieldName].Equals(""))
			//				{
			//					_dataValidating = true;
			//					try
			//					{
			//						gridView.SetRowCellValue(rowHandle, column, data[key]);
			//					}
			//					finally
			//					{
			//						_dataValidating = false;
			//					}
			//				}
			//			}
			//		}
			//	}

			//	if (Utils.CheckGridsValid(gridView, !gridView.GetGridOptions().CheckBoxSelector))
			//		Utils.ShowInfoMessage("자료검증이 완료되었습니다.");
			//};

			//contextMenuStrip.Items.Add("-");

			contextMenuStrip.Items.Add("컬럼최적너비").Click += (s, e) => {
				gridView.BestFitColumns();
			};

			var menuItemColumnAutoWidth = new ToolStripMenuItem();
			menuItemColumnAutoWidth.Text = "컬럼자동맞추기";
			menuItemColumnAutoWidth.Checked = gridView.OptionsView.ColumnAutoWidth;
			menuItemColumnAutoWidth.Click += (s, e) => {
				gridView.OptionsView.ColumnAutoWidth = !gridView.OptionsView.ColumnAutoWidth;
				menuItemColumnAutoWidth.Checked = gridView.OptionsView.ColumnAutoWidth;
			};
			contextMenuStrip.Items.Add(menuItemColumnAutoWidth);

			contextMenuStrip.Items.Add("-");

			if (gridView is BandedGridView)
			{
				var menuItemShowBandHeaders = new ToolStripMenuItem();
				menuItemShowBandHeaders.Text = "밴드보이기";
				menuItemShowBandHeaders.Checked = (gridView as BandedGridView).OptionsView.ShowBands;
				menuItemShowBandHeaders.Click += (s, e) => {
					(gridView as BandedGridView).OptionsView.ShowBands = !(gridView as BandedGridView).OptionsView.ShowBands;
					menuItemShowBandHeaders.Checked = (gridView as BandedGridView).OptionsView.ShowBands;
				};
				contextMenuStrip.Items.Add(menuItemShowBandHeaders);
			}

			var menuItemShowColumnHeaders = new ToolStripMenuItem();
			menuItemShowColumnHeaders.Text = "컬럼보이기";
			menuItemShowColumnHeaders.Checked = gridView.OptionsView.ShowColumnHeaders;
			menuItemShowColumnHeaders.Click += (s, e) => {
				gridView.OptionsView.ShowColumnHeaders = !gridView.OptionsView.ShowColumnHeaders;
				menuItemShowColumnHeaders.Checked = gridView.OptionsView.ShowColumnHeaders;
			};
			contextMenuStrip.Items.Add(menuItemShowColumnHeaders);

			contextMenuStrip.Items.Add("-");

			var menuItemFind = new ToolStripMenuItem();
			menuItemFind.Text = "찾기";
			menuItemFind.Checked = gridView.OptionsFind.AlwaysVisible;
			menuItemFind.Click += (s, e) => {
				gridView.OptionsFind.AlwaysVisible = !gridView.OptionsFind.AlwaysVisible;
				menuItemFind.Checked = gridView.OptionsFind.AlwaysVisible;
			};
			contextMenuStrip.Items.Add(menuItemFind);

			contextMenuStrip.Items.Add("필터").Click += (s, e) => {
				if (gridView.VisibleColumns.Count > 0)
				{
					gridView.ShowFilterEditor(gridView.VisibleColumns[0]);
				}
			};

			contextMenuStrip.Items.Add("필터취소").Click += (s, e) => {
				gridView.ClearColumnsFilter();
			};

			contextMenuStrip.Items.Add("정렬취소").Click += (s, e) => {
				gridView.ClearSorting();
			};

			contextMenuStrip.Items.Add("-");

			var menuItemGroupBy = new ToolStripMenuItem();
			menuItemGroupBy.Text = "그룹핑";
			menuItemGroupBy.Checked = gridView.OptionsView.ShowGroupPanel;
			menuItemGroupBy.Click += (s, e) => {
				gridView.OptionsView.ShowGroupPanel = !gridView.OptionsView.ShowGroupPanel;
				menuItemGroupBy.Checked = gridView.OptionsView.ShowGroupPanel;
				if (!gridView.OptionsView.ShowGroupPanel)
					gridView.ClearGrouping();
			};
			contextMenuStrip.Items.Add(menuItemGroupBy);

			contextMenuStrip.Items.Add("모두펼치기").Click += (s, e) => {
				gridView.ExpandAllGroups();
			};

			contextMenuStrip.Items.Add("모두접기").Click += (s, e) => {
				gridView.CollapseAllGroups();
			};

			contextMenuStrip.Items.Add("그룹지우기").Click += (s, e) => {
				gridView.ClearGrouping();
			};

			contextMenuStrip.Opening += (s, e) => {
				var beforeText = "";
				var visible = true;
				for (int i = 0; i < contextMenuStrip.Items.Count; i++)
				{

					if (contextMenuStrip.Items[i].Text == "찾기")
					{
						//visible = !gridView.OptionsBehavior.Editable || gridView.OptionsFind.AlwaysVisible;
						contextMenuStrip.Items[i].Visible = visible;
					}

					if (contextMenuStrip.Items[i].Text == "행추가" ||
						contextMenuStrip.Items[i].Text.StartsWith("엑셀 붙여넣기") ||
						contextMenuStrip.Items[i].Text == "엑셀 불러오기" ||
						contextMenuStrip.Items[i].Text == "자료검증")
					{
						visible = gridView.OptionsBehavior.Editable && (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch);
						contextMenuStrip.Items[i].Visible = visible;
					}

					if (contextMenuStrip.Items[i].Text == "그룹핑")
					{
						//visible = !gridView.OptionsBehavior.Editable || gridView.OptionsView.ShowGroupPanel;
						//contextMenuStrip.Items[i].Visible = visible;
					}

					if (contextMenuStrip.Items[i].Text == "모두펼치기" ||
						contextMenuStrip.Items[i].Text == "모두접기" ||
						contextMenuStrip.Items[i].Text == "그룹지우기")
					{
						visible = gridView.OptionsView.ShowGroupPanel;
						contextMenuStrip.Items[i].Visible = visible;
					}

					if (!visible)
					{
						visible = true;
						continue;
					}

					if (beforeText == "" && contextMenuStrip.Items[i].Text == "")
					{
						contextMenuStrip.Items[i].Visible = false;
					}
					beforeText = contextMenuStrip.Items[i].Text;
				}
			};

			gridView.GridControl.ContextMenuStrip = contextMenuStrip;

			gridView.CustomDrawCell += (s, e) =>
			{
				if (gridView.OptionsBehavior.Editable && e.Column.GetColumnOptions().ReadOnly && e.Column.GetColumnOptions().ReadOnlyWithColorChange)
				{
					Pen p = new Pen(Color.FromArgb(0xC0, 0xC0, 0xC0));
					Point top1 = new Point(e.Bounds.Left, e.Bounds.Top);
					Point top2 = new Point(e.Bounds.Right, e.Bounds.Top);

					Point left1 = new Point(e.Bounds.Left, e.Bounds.Top);
					Point left2 = new Point(e.Bounds.Left, e.Bounds.Bottom);

					Point right1 = new Point(e.Bounds.Right, e.Bounds.Top);
					Point right2 = new Point(e.Bounds.Right, e.Bounds.Bottom);

					Point bottom1 = new Point(e.Bounds.Left, e.Bounds.Bottom);
					Point bottom2 = new Point(e.Bounds.Right, e.Bounds.Bottom);

					e.Graphics.DrawLine(p, top1, top2);
					e.Graphics.DrawLine(p, left1, left2);
					e.Graphics.DrawLine(p, right1, right2);
					e.Graphics.DrawLine(p, bottom1, bottom2);
				}
			};

			gridView.RowCellStyle += (s, e) => {
				var data = gridView.GetDataRow(e.RowHandle).ToData();
				if (data != null &&
					data.ContainsKey(e.Column.FieldName) &&
					data[e.Column.FieldName] != "" &&
					(e.Column.GetColumnOptions().ColumnType == ColumnType.IntegerEdit || e.Column.GetColumnOptions().ColumnType == ColumnType.FloatEdit) &&
					double.Parse(data[e.Column.FieldName]) < 0)
					e.Appearance.ForeColor = Color.Blue;
			};
		}

		public static void ApplyLookupSettings(this GridView gridView, GridOptions gridOptions = null)
		{
			var gridControl = gridView.GridControl;
			var program = gridControl.FindForm() as IBaseProgram;
			if (gridOptions == null) gridView.Tag = new GridOptions();
			else gridView.Tag = gridOptions;

			gridView.FocusRectStyle = DrawFocusRectStyle.None;
			gridView.OptionsBehavior.Editable = false;
			gridView.OptionsSelection.MultiSelect = true;
			if (gridView.GetGridOptions().CheckBoxSelectorWithLookup)
			{
				gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
				//gridView.OptionsSelection.MultiSelect = true;
				gridView.OptionsBehavior.EditorShowMode = EditorShowMode.MouseDown;
				gridView.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DefaultBoolean.True;
				gridView.OptionsSelection.CheckBoxSelectorColumnWidth = 35;
			}
			else
			{
				gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
			}

			ApplyDefaultSettings(gridView, gridOptions);
		}

		public static void ApplyEditableSettings(this GridView gridView, GridOptions gridOptions = null)
		{
			var gridControl = gridView.GridControl;
			var program = gridControl.FindForm() as IBaseProgram;
			if (gridOptions == null) gridView.Tag = new GridOptions();
			else gridView.Tag = gridOptions;

			gridView.FocusRectStyle = DrawFocusRectStyle.CellFocus;
			gridView.OptionsBehavior.Editable = true;
			gridView.OptionsSelection.MultiSelect = true;
			if (gridView.GetGridOptions().CheckBoxSelector)
			{
				gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
				//gridView.OptionsSelection.MultiSelect = true;
				gridView.OptionsBehavior.EditorShowMode = EditorShowMode.MouseDown;
				gridView.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DefaultBoolean.True;
				gridView.OptionsSelection.CheckBoxSelectorColumnWidth = 35;
			}
			else
			{
				gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
			}
			//gridView.OptionsClipboard.AllowCopy = DefaultBoolean.False;
			gridView.OptionsSelection.UseIndicatorForSelection = false;
			gridView.OptionsBehavior.AllowAddRows = (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch) ? DefaultBoolean.True : DefaultBoolean.False;

			ApplyDefaultSettings(gridView, gridOptions);

			gridView.ShownEditor += (s, e) => {
				var selectAllDone = false;
				gridView.ActiveEditor.DoubleClick += program.GridRowDoubleClickHandler;
				gridView.ActiveEditor.MouseUp += (s1, e1) => {
					if (!selectAllDone)
					{
						gridView.ActiveEditor.SelectAll();
						selectAllDone = true;
					}
				};
				gridView.ActiveEditor.Enter += (s1, e1) => {
					if (Control.MouseButtons == MouseButtons.None)
					{
						gridView.ActiveEditor.SelectAll();
						selectAllDone = true;
					}
				};
				gridView.ActiveEditor.Leave += (s1, e1) => {
					selectAllDone = false;
				};
			};

			gridView.CellValueChanging += (s, e) => {
				
				if (e.Column.OptionsColumn.AllowEdit)
				{
					var options = e.Column.GetColumnOptions();
					if (options != null && options.DependantFieldNames != null)
					{
						foreach (var fieldName in options.DependantFieldNames)
						{
							ClearRowCellValueRecursivelyByChanging(gridView, e.RowHandle, gridView.Columns[fieldName], program);
						}
					}
				}
				var eventArgs = new GridValueChangedEventArgs(e.RowHandle, e.Column, e.Value, e.Column.GetColumnOptions(), e.Column.OptionsColumn.AllowEdit);
				program.GridValueChangingHandler(s, eventArgs);

				var columnOptions = e.Column.GetColumnOptions();
				if (columnOptions.ColumnType == ColumnType.LookUpEdit
					|| columnOptions.ColumnType == ColumnType.RadioEdit
					|| columnOptions.ColumnType == ColumnType.CheckEdit
				)
				{
					gridView.SetRowCellValue(e.RowHandle, e.Column, e.Value);
				}
			};

			gridView.CellValueChanged += (s, e) => {
				gridView.SelectRow(e.RowHandle);
				gridView.UpdateCurrentRow();

				var rowIndex = gridView.GetDataSourceRowIndex(e.RowHandle);
				var dataTable = gridControl.DataSource as DataTable;
				var data = e.RowHandle <0 ? null : dataTable?.Rows[rowIndex].ToData();

				if (data == null)
					return;

				if (e.Column.OptionsColumn.AllowEdit)
				{
					var options = e.Column.GetColumnOptions();
					if (!_dataValidating)
					{ //자료검증이 아닐때만
						if (options != null && options.DependantFieldNames != null)
						{
							foreach (var fieldName in options.DependantFieldNames)
							{
								ClearRowCellValueRecursivelyByChanged(gridView, e.RowHandle, gridView.Columns[fieldName], program);
							}
						}
					}

					if (options.ColumnType == ColumnType.DateEdit || options.ColumnType == ColumnType.YearMonthEdit)
					{
						if (!(e.Value as string).Equals(""))
						{
							if (options.ColumnType == ColumnType.DateEdit)
							{
								data[e.Column.FieldName] = data[e.Column.FieldName].Replace("-", "").Substring(0, 8);
								dataTable.Rows[rowIndex].SetData(data);
							}
							else if (options.ColumnType == ColumnType.YearMonthEdit)
							{
								data[e.Column.FieldName] = data[e.Column.FieldName].Replace("-", "").Substring(0, 6);
								dataTable.Rows[rowIndex].SetData(data);
							}
						}
					}
					if (!_changedFromHelpPopup)
					{ //헬프팝업에서 변경된 경우 제외
						if (options.ColumnType == ColumnType.CodeHelperEdit)
						{
							
						}
					}
				}
				var eventArgs = new GridValueChangedEventArgs(e.RowHandle, e.Column, e.Value, e.Column.GetColumnOptions(), !_dataValidating);
				program.GridValueChangedHandler(s, eventArgs);
			};

			gridView.FocusedColumnChanged += (s, e) => {
				gridView.ShowEditorByMouse();
			};

			gridView.KeyDown += (s, e) => {
				var column = gridView.FocusedColumn;
				if (column == null)
					return;
				var rowHandle = gridView.FocusedRowHandle;
				var data = gridView.GetFocusedRowData();
				//var value = gridView.FocusedValue;
				if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab || e.KeyCode == Keys.Insert)
				{
					if (gridView.ActiveEditor != null)
					{
						if (!gridView.ActiveEditor.IsModified)
						{
							if (gridView.ActiveEditor.DoValidate())
							{
								gridView.CloseEditor();
								////var eventArgs = CreateRealFieldEvent(gridView, rowHandle, column, value, true);
								//var eventArgs = new GridValueChangedEventArgs(rowHandle, column, value, column.GetColumnOptions(), true);
								//program.GridValueChangedHandler(s, eventArgs);
								if (column.GetColumnOptions().ColumnType == ColumnType.CodeHelperEdit)
								{
									var options = column.GetColumnOptions() as CodeHelperColumnOptions;
									if (!data[column.FieldName].Equals("") && data[options.NameFieldName].Equals(""))
									{
										//var eventArgs = new GridValueChangedEventArgs(rowHandle, column, value, column.GetColumnOptions(), true);
										//GridView_CellValueChanged(s, eventArgs);
										gridView.SetRowCellValue(rowHandle, column, data[column.FieldName]);
									}
								}
							}
						}
						else
						{
							if (gridView.ActiveEditor.DoValidate())
							{
								gridView.CloseEditor();
							}
						}
					}
					else
					{
						////var eventArgs = CreateRealFieldEvent(gridView, rowHandle, column, value, true);
						//var eventArgs = new GridValueChangedEventArgs(rowHandle, column, value, column.GetColumnOptions(), true);
						//program.GridValueChangedHandler(s, eventArgs);
					}
					gridView.UpdateCurrentRow();

					if (gridView.IsNewItemRow(rowHandle))
					{
						rowHandle = gridView.FocusedRowHandle;
						////var eventArgs = CreateRealFieldEvent(gridView, rowHandle, column, value, true);
						//var eventArgs = new GridValueChangedEventArgs(rowHandle, column, value, column.GetColumnOptions(), true);
						//program.GridValueChangedHandler(s, eventArgs);
					}

					if (e.KeyCode == Keys.Tab && e.Shift)
					{
						var sortedColumns = gridView.Columns.OrderByDescending(c => c.VisibleIndex);
						var remainColumns = sortedColumns.SkipWhile(c => c != column).Skip(1).Where(Focusable);
						if (remainColumns.Count() > 0)
						{
							gridView.FocusedColumn = remainColumns.First();
							gridView.ShowEditorByMouse();
							e.Handled = true;
						}
						else
						{
							if (!gridView.IsFirstRow)
							{
								gridView.FocusedRowHandle -= 1;
								gridView.FocusedColumn = LastFocusableField(gridView);
								e.Handled = true;
							}
						}
					}
					else
					{
						if (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch)
						{
							if ((gridView.Columns.OrderBy(c => c.VisibleIndex).Last(d => Focusable(d)) == gridView.FocusedColumn
									|| gridView.Columns.OrderBy(c => c.VisibleIndex).Last(d => d.Visible) == gridView.FocusedColumn)
									&& gridView.IsLastRow)
							{
								gridView.AddNewRow(data: null);
								gridView.FocusedColumn = gridView.FirstFocusableField();
								e.Handled = true;
								return;
							}
						}

						//if (gridView.IsValidField(rowHandle, column.FieldName)) {
						var sortedColumns = gridView.Columns.OrderBy(c => c.VisibleIndex);
						var remainColumns = sortedColumns.SkipWhile(c => c != column).Skip(1).Where(Focusable);
						if (remainColumns.Count() > 0)
						{
							gridView.FocusedColumn = remainColumns.First();
							gridView.ShowEditorByMouse();
							e.Handled = true;
						}
						else
						{
							if (!gridView.IsLastRow)
							{
								gridView.FocusedRowHandle += 1;
								gridView.FocusedColumn = FirstFocusableField(gridView);
								e.Handled = true;
							}
						}
						//}
					}
				}
				else if (e.KeyCode == Keys.Up)
				{
					if (gridView.IsNewItemRow(gridView.FocusedRowHandle))
					{
						gridView.DeleteRow(gridView.FocusedRowHandle);
						e.Handled = true;
					}
				}
				else if (e.KeyCode == Keys.Left && gridView.FocusedColumn == gridView.VisibleColumns[0])
				{
					if (gridView.IsNewItemRow(gridView.FocusedRowHandle))
					{
						gridView.DeleteRow(gridView.FocusedRowHandle);
						e.Handled = true;
					}
				}
				else if (e.KeyCode == Keys.Down && (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch))
				{
					if (!gridView.IsNewItemRow(gridView.FocusedRowHandle) && gridView.IsLastRow)
					{
						if (gridView.GridControl.DataSource != null || gridView.GetGridOptions().AllowAddRowWithNoSearch)
						{
							if (gridView.ActiveEditor != null)
							{
								if (!gridView.ActiveEditor.IsModified)
								{
									if (gridView.ActiveEditor.DoValidate())
									{
										gridView.CloseEditor();
									}
								}
								else
								{
									if (gridView.ActiveEditor.DoValidate())
									{
										gridView.CloseEditor();
									}
								}
							}
							gridView.AddNewRow(data: null);
						}
						e.Handled = true;
					}
					else if (gridView.IsNewItemRow(gridView.FocusedRowHandle))
					{
						e.Handled = true;
					}
				}
			};

			gridControl.ProcessGridKey += (s, e) => {
				var column = gridView.FocusedColumn;
				var rowHandle = gridView.FocusedRowHandle;
				var value = gridView.FocusedValue;
				if (e.KeyCode == Keys.F1)
				{
					var columnOptions = column.GetColumnOptions();
					if (columnOptions.ColumnType == ColumnType.DateEdit)
					{
						if (gridView.ActiveEditor == null) gridView.ShowEditorByMouse();
						var dateEdit = (gridView.ActiveEditor as DateEdit);
						if (dateEdit == null) return;
						dateEdit.ShowPopup();
						e.Handled = true;
					}
					else if (columnOptions.ColumnType == ColumnType.YearMonthEdit)
					{
						if (gridView.ActiveEditor == null) gridView.ShowEditorByMouse();
						var dateEdit = (gridView.ActiveEditor as DateEdit);
						if (dateEdit == null) return;
						dateEdit.ShowPopup();
						e.Handled = true;
					}
					else if (columnOptions.ColumnType == ColumnType.CodeHelperEdit)
					{
						if (gridView.ActiveEditor == null) gridView.ShowEditorByMouse();
						var buttonEdit = (gridView.ActiveEditor as ButtonEdit);
						if (buttonEdit == null) return;
						if (buttonEdit.Properties.Buttons.Count > 0) buttonEdit.PerformClick(buttonEdit.Properties.Buttons[0]);
						e.Handled = true;
					}
				}
			};
		}

		public static void CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (e.RowHandle >= 0)
			{
				e.Info.DisplayText = (e.RowHandle + 1).ToString();
			}
		}

		public static GridOptions GetGridOptions(this GridView gridView)
		{
			var gridOptions = gridView.Tag as GridOptions;
			if (gridOptions == null) new GridOptions();
			return gridOptions;
		}

		public static void AddNewRow(this GridView gridView, Data<string> data)
		{
			if (gridView.GridControl.DataSource == null)
				gridView.GridControl.DataSource = gridView.NewDataTable();

			foreach (DataColumn dataColumn in (gridView.GridControl.DataSource as DataTable).Columns)
			{
				var column = gridView.Columns.ColumnByFieldName(dataColumn.ColumnName);
				var defaultValue = column != null ? column.GetColumnOptions().DefaultValue : null;
				if (data != null && data.Keys.Contains(dataColumn.ColumnName))
				{
					try
					{
						//dataColumn.DefaultValue = Convert.ChangeType(data[dataColumn.ColumnName], dataColumn.DataType);
						if (string.IsNullOrEmpty(data[dataColumn.ColumnName]))
							dataColumn.DefaultValue = DBNull.Value;
						else
							dataColumn.DefaultValue = Convert.ChangeType(data[dataColumn.ColumnName], dataColumn.DataType);
					}
					catch (Exception ex)
					{
						//Utils.CloseWaiting(gridView.GridControl.FindForm());
						//Utils.ShowErrorMessage(dataColumn.ColumnName + " / " + data[dataColumn.ColumnName] + "\r\n\r\n" + ex.Message + "\r\n\r\n\r\n\r\n" + ex.StackTrace);
						throw;
					}
				}
				else if (defaultValue != null)
				{
					dataColumn.DefaultValue = defaultValue;
				}
			}
			int[] rows = gridView.GetSelectedRows();
			gridView.AddNewRow();
			foreach (DataColumn dataColumn in (gridView.GridControl.DataSource as DataTable).Columns)
			{
				if (dataColumn.DefaultValue != null)
					dataColumn.DefaultValue = null;
			}
			if (rows.Count() > 0)
			{
				foreach (var i in rows)
					gridView.SelectRow(i);
			}
			gridView.UpdateCurrentRow();
		}

		private static bool Focusable(GridColumn c)
		{
			return c.OptionsColumn.AllowEdit && c.Visible;
		}

		public static GridColumn FirstFocusableField(this GridView gridView)
		{
			return gridView.Columns.FirstOrDefault(Focusable);
		}

		public static GridColumn LastFocusableField(this GridView gridView)
		{
			return gridView.Columns.LastOrDefault(Focusable);
		}

		//public static void CopyToClipboard(GridView gridView, int[] rowHandles)
		//{
		//	if (rowHandles.Count() < 1)
		//	{
		//		Utils.ShowWarnMessage("선택한 자료가 없습니다");
		//		return;
		//	}
		//	var parentForm = gridView.GridControl.FindForm();
		//	Utils.ShowWaiting(parentForm);
		//	var task = new TaskFactory().StartNew(() => {
		//		var buffer = new StringBuilder();
		//		var dataTable = gridView.GridControl.DataSource as DataTable;
		//		var columnsToCopy = gridView.VisibleColumns.Where(c => c.FieldName != "DX$CheckboxSelectorColumn");
		//		buffer.Append(String.Join("\t", columnsToCopy.Select(c => c.Caption)) + "\r\n"); // Header
		//		for (int i = 0; i < rowHandles.Count(); i++)
		//		{
		//			var row = dataTable.Rows[rowHandles[i]];
		//			DataList<string> rowTextList = new DataList<string>();
		//			foreach (GridColumn c in columnsToCopy)
		//			{
		//				//rowTextList.Add(row[c.GetRealFieldName()].ToString());
		//				rowTextList.Add(row[c.FieldName].ToString());
		//			}
		//			buffer.Append(String.Join("\t", rowTextList) + "\r\n");
		//		}
		//		Utils.InvokeAction(parentForm, () => {
		//			Clipboard.SetText(buffer.ToString());
		//		});
		//	});
		//	task.ContinueWith((taskResult) => {
		//		Utils.InvokeAction(parentForm, () => {
		//			Utils.CloseWaiting(parentForm);
		//		});
		//	});
		//}

		//public static void PasteFromClipboard(GridView gridView, ClipboardPasteType pasteType)
		//{
		//	var parentForm = gridView.GridControl.FindForm();
		//	var gridOptions = gridView.Tag as GridOptions;
		//	Debug.Assert(gridOptions != null, "EditableGrid 에서는 GridOptions 가 null 일 수 없습니다.");
		//	if (!gridOptions.AllowAddRow && !gridOptions.AllowAddRowWithNoSearch)
		//	{
		//		Utils.ShowWarnMessage("행추가 기능이 없는 그리드에서는 붙여넣기를 할 수 없습니다.");
		//		return;
		//	}
		//	var dataTable = gridView.GridControl.DataSource as DataTable;
		//	if (dataTable == null && !gridView.GetGridOptions().AllowAddRowWithNoSearch)
		//	{
		//		Utils.ShowWarnMessage("조회 후 진행해주세요");
		//		return;
		//	}
		//	var clipboardText = Clipboard.GetText();
		//	if (clipboardText == null && clipboardText == "")
		//	{
		//		Utils.ShowWarnMessage("붙여넣을 자료가 없습니다");
		//		return;
		//	}
		//	if (pasteType == ClipboardPasteType.OnlyData)
		//	{
		//		var header = "";
		//		foreach (GridColumn column in gridView.Columns)
		//		{
		//			header += column.FieldName + "\t";
		//		}
		//		clipboardText = string.Format("{0}\r\n{1}", header, clipboardText);
		//	}
		//	string[] lines = clipboardText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		//	if (pasteType == ClipboardPasteType.FirstHeader)
		//	{
		//		if (lines.Count() < 2)
		//		{
		//			Utils.ShowWarnMessage("헤더를 포함해 2줄 이상의 자료가 필요합니다.");
		//			return;
		//		}
		//	}
		//	Utils.ShowWaiting(parentForm);
		//	var task = new TaskFactory().StartNew(() => {
		//		string[] dataKeys;
		//		string[] allKeys;
		//		string[] dateKeys;
		//		IEnumerable<string> realLines;
		//		allKeys = lines[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
		//		for (int i = 0; i < allKeys.Length; i++)
		//		{
		//			allKeys[i] = allKeys[i].Trim();
		//		}
		//		var countMap = new Data<int>();
		//		foreach (var key in allKeys)
		//		{
		//			if (countMap.ContainsKey(key))
		//			{
		//				Utils.CloseWaiting(parentForm);
		//				throw new Exception(String.Format("동일한 컬럼이름이 2개이상 있습니다. ({0})", key));
		//			}
		//			countMap[key] = 1;
		//		}
		//		var columns = gridView.Columns;
		//		for (int i = 0; i < allKeys.Count(); i++)
		//		{
		//			if (columns.Where(c => c.FieldName == allKeys[i]).Count() == 0)
		//			{
		//				var cols = columns.Where(c => c.Caption == allKeys[i]).ToArray();
		//				if (cols != null && cols.Count() > 0)
		//					allKeys[i] = cols[0].FieldName;
		//			}
		//		}
		//		dataKeys = allKeys.Where(k => !gridView.Columns[k].GetColumnOptions().ReadOnly).ToArray();
		//		dateKeys = dataKeys.Where(k => gridView.Columns[k].GetColumnOptions().ColumnType == ColumnType.DateEdit).ToArray();
		//		var defaultData = new Data<string>();
		//		foreach (GridColumn column in columns)
		//		{
		//			if (!dataKeys.Contains(column.FieldName) && column.GetColumnOptions().DefaultValue != null && !string.IsNullOrEmpty(column.GetColumnOptions().DefaultValue.ToString()))
		//			{
		//				defaultData[column.FieldName] = column.GetColumnOptions().DefaultValue.ToString();
		//			}
		//		}
		//		realLines = lines.Skip(1);
		//		var dataList = new DataList<Data<string>>();
		//		foreach (var line in realLines)
		//		{
		//			var texts = line.Split(new char[] { '\t' });
		//			for (int i = 0; i < texts.Length; i++)
		//			{
		//				texts[i] = texts[i].Trim();
		//			}
		//			var data = new Data<string>();
		//			for (int i = 0; i < allKeys.Count() && i < texts.Count(); i++)
		//			{
		//				if (dataKeys.Contains(allKeys[i]))
		//				{
		//					if (dateKeys.Contains(allKeys[i]))
		//						texts[i] = texts[i].Replace(".", "").Replace("-", "").Replace("/", "");
		//					data.Add(allKeys[i], texts[i]);
		//				}
		//			}
		//			if (defaultData != null && defaultData.Count > 0)
		//				data.Merge(defaultData);
		//			dataList.Add(data);
		//		}
		//		return dataList;
		//	});

		//	task.ContinueWith((taskResult) => {
		//		Utils.InvokeAction(parentForm, () => {
		//			Utils.CloseWaiting(parentForm);
		//		});

		//		if (taskResult.IsFaulted)
		//		{
		//			Utils.ShowErrorMessage(taskResult.Exception.InnerException.Message);
		//		}
		//		else
		//		{
		//			var dataList = taskResult.Result;

		//			Utils.InvokeAction(parentForm, () => {
		//				try
		//				{
		//					if (dataTable == null)
		//					{
		//						dataTable = gridView.NewDataTable();
		//						gridView.GridControl.DataSource = dataTable;
		//					}
		//					var rowHandle = -1;
		//					foreach (var data in dataList)
		//					{
		//						var newRow = dataTable.NewRow();
		//						newRow.SetData(data);
		//						dataTable.Rows.Add(newRow);
		//						var rowIndex = dataTable.Rows.IndexOf(newRow);
		//						rowHandle = gridView.GetRowHandle(rowIndex);
		//						gridView.SelectRow(rowHandle);
		//					}
		//					if (rowHandle != -1)
		//						gridView.FocusRow(rowHandle);
		//				}
		//				catch (Exception ex)
		//				{
		//					Utils.ShowErrorMessage(ex.Message + "\r\n\r\n\r\n\r\n\r\n" + ex.StackTrace);
		//				}
		//			});
		//		}
		//	});
		//}

		//public static void Search(this GridView gridView, SearchOptions searchOptions)
		//{
		//	var form = gridView.GridControl.FindForm();
		//	gridView.ClearData();
		//	if (searchOptions.SearchParam == null) searchOptions.SearchParam = new Data<string>();
		//	var apiClient = new HttpApiClient(Utils.GetClientSession());
		//	Utils.ShowWaiting(form);
		//	apiClient.Get<DataList<Data<string>>>(searchOptions.QueryUri, searchOptions.SearchParam).ContinueWith((apiResultTask) => {
		//		Utils.InvokeAction(form, () => {
		//			Utils.CloseWaiting(form);
		//			var apiResult = apiResultTask.Result;
		//			if (!apiResult.IsError)
		//			{
		//				if (apiResult.Content != null && apiResult.Content.Count > 0)
		//				{
		//					if (searchOptions.FocusPred == null)
		//						gridView.SetDataList(apiResult.Content);
		//					else
		//						gridView.SetDataListWithFocus(apiResult.Content, searchOptions.FocusPred);
		//				}
		//				else
		//				{
		//					if (searchOptions.ShowNoDataMessage)
		//						Utils.ShowInfoMessage(searchOptions.NoDataMessage);
		//					if (searchOptions.AddRowIfNoData && (gridView.GetGridOptions().AllowAddRow || gridView.GetGridOptions().AllowAddRowWithNoSearch))
		//					{
		//						gridView.AddNewRow(data: null);
		//						gridView.FocusRow(0);
		//					}
		//				}
		//			}
		//			else
		//			{
		//				Logger.Get(typeof(ApiUtils)).Error(apiResult.TraceMsg);
		//				Utils.ShowErrorMessage(apiResult.ErrorMsg);
		//			}
		//		});
		//	});
		//}

		public static void SetDataList(this GridView gridView, DataList<Data<string>> dataList)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			if (dataList == null || dataList.Count < 1)
				return;
			dataTable?.Dispose();
			dataTable = dataList.ToDataTable(gridView);
			gridView.GridControl.DataSource = dataTable;
			//if (gridView.GetGridOptions().BestFitColumns)
			//	gridView.BestFitColumns();
			gridView.FocusRow(0);
		}

		public static void SetDataListWithFocus(this GridView gridView, DataList<Data<string>> dataList, Func<Data<string>, bool> pred)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			if (dataList == null || dataList.Count < 1)
				return;
			dataTable?.Dispose();
			dataTable = dataList.ToDataTable(gridView);
			gridView.GridControl.DataSource = dataTable;
			if (gridView.GetGridOptions().BestFitColumns)
				gridView.BestFitColumns();
			gridView.FocusRowBy(pred);
			if (gridView.FocusedRowHandle == 0)
			{
				//(gridView.GridControl.FindForm() as IBaseProgram)?.GridRowClickHandler(gridView, new EventArgs());
			}
		}

		public static DataTable ToDataTable(this DataList<Data<string>> dataList)
		{
			Debug.Assert(dataList != null && dataList.Count > 0);
			var dataTable = dataList[0].Keys.NewDataTable();
			foreach (Data<string> data in dataList)
			{
				var newRow = dataTable.NewRow();
				newRow.SetData(data);
				dataTable.Rows.Add(newRow);
			}
			return dataTable;
		}

		public static DataTable ToDataTable(this DataList<Data<string>> dataList, GridView gridView)
		{
			Debug.Assert(dataList != null && dataList.Count > 0);
			var dataTable = gridView.NewDataTable(dataList[0].Keys);
			foreach (Data<string> data in dataList)
			{
				var newRow = dataTable.NewRow();
				newRow.SetData(gridView, data);
				dataTable.Rows.Add(newRow);
			}
			return dataTable;
		}

		public static DataTable NewDataTable(this GridView gridView)
		{
			var dataTable = new DataTable();
			foreach (GridView view in gridView.GridControl.Views)
			{
				foreach (GridColumn column in view.Columns)
				{
					//if (column.FieldName != null && !column.FieldName.Equals("") && !column.FieldName.StartsWith("__")) {
					if (column.FieldName != null && !column.FieldName.Equals("") && !dataTable.Columns.Contains(column.FieldName))
					{
						var dataColumn = dataTable.Columns.Add(column.FieldName);
						dataColumn.Caption = column.Caption;
						dataColumn.ColumnName = column.FieldName;
						/*
						var defaultValue = column.GetColumnOptions()?.DefaultValue;
						if (defaultValue != null) {
							dataColumn.DefaultValue = defaultValue;
						}
						*/
					}
				}
			}
			dataTable.SetDataType(gridView);
			return dataTable;
		}

		public static DataTable NewDataTable(this IEnumerable<string> keys)
		{
			var dataTable = new DataTable();
			foreach (var key in keys)
			{
				if (!dataTable.Columns.Contains(key))
				{
					var dataColumn = dataTable.Columns.Add(key);
					dataColumn.Caption = key;
					dataColumn.ColumnName = key;
				}
			}
			return dataTable;
		}

		public static DataTable NewDataTable(this GridView gridView, IEnumerable<string> additionalKeys)
		{
			var dataTable = NewDataTable(gridView);
			foreach (var key in additionalKeys)
			{
				if (!dataTable.Columns.Contains(key))
				{
					var dataColumn = dataTable.Columns.Add(key);
					dataColumn.Caption = key;
					dataColumn.ColumnName = key;
				}
			}
			/*
			foreach (GridColumn column in gridView.Columns) {
				var dataColumn = dataTable.Columns[column.FieldName];
				var defaultValue = column.GetColumnOptions()?.DefaultValue;
				if (defaultValue != null)
					dataColumn.DefaultValue = defaultValue;
			}
			*/
			dataTable.SetDataType(gridView);
			return dataTable;
		}
		/*
		public static Data<string> NewData (this GridView gridView) {
			var result = new Data<string>();
			foreach (GridView view in gridView.GridControl.Views) {
				foreach (GridColumn column in view.Columns) {
					//if (column.FieldName != null && !column.FieldName.Equals("") && !column.FieldName.StartsWith("__")) {
					if (column.FieldName != null && !column.FieldName.Equals("")) {
						var defaultValue = column.GetColumnOptions()?.DefaultValue;
						if (defaultValue != null)
							result[column.FieldName] = defaultValue.ToString();
					}
				}
			}
			return result;
		}
		*/

		public static void SetDataType(this DataTable dataTable, GridView gridView)
		{
			foreach (DataColumn dataColumn in dataTable.Columns)
			{
				var key = dataColumn.ColumnName;
				if (gridView.Columns.ColumnByFieldName(key) != null)
				{
					if (gridView.Columns[key].DisplayFormat.FormatType == FormatType.Numeric)
					{
						dataColumn.DataType = typeof(double);
					}
				}
			}
		}

		public static void ClearData(this GridView gridView)
		{
			(gridView.GridControl.DataSource as DataTable)?.Dispose();
			gridView.GridControl.DataSource = null;
		}

		public static DataList<Data<string>> GetDataList(this GridView gridView)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			if (dataTable == null)
				dataTable = gridView.NewDataTable();
			var result = new DataList<Data<string>>();
			foreach (DataRow dataRow in dataTable.Rows)
			{
				var data = dataRow.ToData();
				result.Add(data);
			}
			return result;
		}

		public static DataList<Data<string>> GetSelectedDataList(this GridView gridView)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			if (dataTable == null)
				dataTable = gridView.NewDataTable();
			var rowHandles = gridView.GetSelectedRows();
			var result = new DataList<Data<string>>();
			//var unboundFieldNames = gridView.Columns.Where(c => c.GetColumnOptions() is UnboundColumnOptions).Select(c => c.FieldName);
			foreach (var rowHandle in rowHandles)
			{
				var data = gridView.GetDataRow(rowHandle).ToData();
				/*
				foreach (var key in unboundFieldNames) {
					data.Remove(key);
				}
				*/
				result.Add(data);
			}
			return result;
		}

		public static Data<string> GetFocusedRowData(this GridView gridView)
		{
			return gridView.GetFocusedDataRow().ToData();
		}

		public static void FocusRow(this GridView gridView, int rowHandle)
		{
			gridView.FocusedRowHandle = rowHandle;
			var focusableColumns = gridView.VisibleColumns.Where(c => Focusable(c)).Where(c => c.FieldName != "DX$CheckboxSelectorColumn");
			if (focusableColumns.Count() > 0)
			{
				gridView.FocusedColumn = focusableColumns.First();
			}
		}

		public static void FocusCell(this GridView gridView, int rowHandle, string fieldName)
		{
			//var column = gridView.Columns[gridView.Columns[fieldName].GetUnboundedFieldName()];
			var column = gridView.Columns[fieldName];
			gridView.FocusedRowHandle = rowHandle;
			gridView.FocusedColumn = column;
			gridView.ShowEditorByMouse();
		}

		public static void FocusCell(this GridView gridView, string fieldName)
		{
			//var column = gridView.Columns[gridView.Columns[fieldName].GetUnboundedFieldName()];
			var column = gridView.Columns[fieldName];
			gridView.FocusedColumn = column;
			gridView.ShowEditorByMouse();
		}

		//public static void DelayFocusCell(this GridView gridView, int rowHandle, string fieldName)
		//{
		//	Utils.DelayInvoke(() => {
		//		gridView.FocusCell(rowHandle, fieldName);
		//	}, 10);
		//}

		public static void SetData(this DataRow dataRow, Data<string> data)
		{
			var dataTable = dataRow.Table;
			foreach (var key in data.Keys)
			{
				if (dataTable.Columns.Contains(key) && data[key] != null && data[key] != "")
					dataRow[key] = data[key];
			}
		}

		public static void SetData(this DataRow dataRow, GridView gridView, Data<String> data)
		{
			var mergedData = data.MergeTo(dataRow.ToData());
			var dataTable = dataRow.Table;
			foreach (var key in mergedData.Keys)
			{
				if (dataTable.Columns.Contains(key))
				{
					//var options = gridView.Columns[key]?.GetColumnOptions();
					//string defaultValue = "";
					//if (options != null && options.DefaultValue != null)
					//defaultValue = options.DefaultValue.ToString();
					if (data.ContainsKey(key) && data[key] != null && data[key] != "")
						dataRow[key] = data[key];
				}
			}
		}

		public static Data<string> ToData(this DataRow dataRow)
		{
			var result = new Data<string>();
			if (dataRow == null) return result;
			var dataTable = dataRow.Table;
			foreach (DataColumn column in dataTable.Columns)
			{
				result[column.ColumnName] = dataRow[column].ToString();
				result[RowIndexFieldName] = dataTable.Rows.IndexOf(dataRow).ToString();
			}
			return result;
		}

		public static DataRow GetDataRowBy(this GridView gridView, Func<Data<string>, bool> pred)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			foreach (DataRow dataRow in dataTable.Rows)
			{
				if (pred(dataRow.ToData())) return dataRow;
			}
			return null;
		}

		public static void FocusRowBy(this GridView gridView, Func<Data<string>, bool> pred)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				if (pred(dataTable.Rows[i].ToData()))
				{
					gridView.FocusRow(gridView.GetRowHandle(i));
					break;
				}
			}
		}

		public static DataList<string> GetRequiredFieldNames(this GridView gridView)
		{
			var requiredFieldNames = new DataList<string>();
			foreach (GridColumn column in gridView.Columns)
			{
				var options = column.GetColumnOptions();
				//if(options != null && !(options is UnboundColumnOptions) && options.Required) {
				if (options != null && options.Required)
				{
					requiredFieldNames.Add(column.FieldName);
				}
			}
			return requiredFieldNames;
		}

		public static DataList<string> GetFieldNames(this GridView gridView)
		{
			var fieldNames = new DataList<string>();
			foreach (GridColumn column in gridView.Columns)
			{
				fieldNames.Add(column.FieldName);
				//var options = column.GetColumnOptions();
				//if(options != null && !(options is UnboundColumnOptions)) {
				//if (options != null) {
				//fieldNames.Add(column.FieldName);
				//}
			}
			return fieldNames;
		}

		public static bool IsValidField(this GridView gridView, int rowHandle, string codeFieldName, bool required = false)
		{
			//var realFieldName = gridView.Columns[codeFieldName].GetRealFieldName();
			//var codeColumn = gridView.Columns[realFieldName];
			var codeColumn = gridView.Columns[codeFieldName];
			if (codeColumn != null)
			{
				var codeSetting = codeColumn.GetColumnOptions();
				var codeValue = gridView.GetDataRow(rowHandle).ToData()[codeFieldName];

				if ((codeSetting.Required || required) && (codeValue == null || codeValue.Equals(""))) return false;

				if (codeSetting.ColumnType == ColumnType.CodeHelperEdit)
				{
					var nameFieldName = (codeColumn.GetColumnOptions() as CodeHelperColumnOptions).NameFieldName;
					var nameValue = gridView.GetDataRow(rowHandle).ToData()[nameFieldName];

					if ((nameValue == null || nameValue.Equals("")) && codeValue != null && !codeValue.Equals("")) return false;
				}
			}
			return true;
		}

		public static void ClearRowCellValueRecursivelyByChanging(this GridView gridView, int rowHandle, GridColumn column, IBaseProgram program = null)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			if (dataTable != null)
			{
				var rowIndex = gridView.GetDataSourceRowIndex(rowHandle);
				dataTable.Rows[rowIndex][column.FieldName] = DBNull.Value;
				var options = column.GetColumnOptions();
				if (program == null) program = (gridView.GridControl.FindForm() as IBaseProgram);
				var args = new GridValueChangedEventArgs(rowHandle, column, "", options, false);
				program.GridValueChangingHandler(gridView, args);
				if (options != null && options.DependantFieldNames != null)
				{
					foreach (string fieldName in options.DependantFieldNames)
					{
						ClearRowCellValueRecursivelyByChanging(gridView, rowHandle, gridView.Columns[fieldName], program);
					}
				}
			}
		}

		public static void ClearRowCellValueRecursivelyByChanged(this GridView gridView, int rowHandle, GridColumn column, IBaseProgram program = null)
		{
			var dataTable = gridView.GridControl.DataSource as DataTable;
			if (dataTable != null)
			{
				var rowIndex = gridView.GetDataSourceRowIndex(rowHandle);
				if (dataTable.Rows[rowIndex][column.FieldName] != DBNull.Value)
				{
					dataTable.Rows[rowIndex][column.FieldName] = DBNull.Value;
					var options = column.GetColumnOptions();
					if (program == null)
						program = (gridView.GridControl.FindForm() as IBaseProgram);
					var args = new GridValueChangedEventArgs(rowHandle, column, "", options, false);
					program.GridValueChangedHandler(gridView, args);
					if (options != null && options.DependantFieldNames != null)
					{
						foreach (string fieldName in options.DependantFieldNames)
						{
							ClearRowCellValueRecursivelyByChanged(gridView, rowHandle, gridView.Columns[fieldName], program);
						}
					}
				}
			}
		}

		public static void Merge(this Data<string> data, DataRow dataRow)
		{
			if (dataRow == null) return;
			else
			{
				var dataTable = dataRow.Table as DataTable;
				foreach (DataColumn column in dataTable.Columns)
				{
					data[column.ColumnName] = (string)dataRow[column.ColumnName];
				}
			}
		}

		public static ColumnOptions GetColumnOptions(this GridColumn column)
		{
			if (column == null || column.Tag == null || !(column.Tag is ColumnOptions))
				return new ColumnOptions();
			else
				return column.Tag as ColumnOptions;
		}

		public static void ValidationAll(this GridView gridView)
		{
            var dataList = gridView.GetDataList();
            if (dataList == null || dataList.Count < 1)
            {
                //Utils.ShowWarnMessage("검증할 자료가 없습니다");
                return;
            }
            var keys = dataList[0].Keys;
            foreach (var data in dataList)
            {
                foreach (var key in keys)
				{
                    var column = gridView.Columns[key];
                    if (column != null && column.GetColumnOptions().ColumnType == ColumnType.CodeHelperEdit)
                    {
                        var rowHandle = int.Parse(data[GridExtension.RowIndexFieldName]);
                        if (!data[key].Equals("") && data[(column.GetColumnOptions() as CodeHelperColumnOptions).NameFieldName].Equals(""))
                        {
                            _dataValidating = true;
                            try
							{
                                gridView.SetRowCellValue(rowHandle, column, data[key]);
                            }
                            finally
                            {
                                _dataValidating = false;
                            }
                        }
                    }
                }
            }
			//if (Utils.CheckGridsValid(gridView, !gridView.GetGridOptions().CheckBoxSelector))
			//	Utils.ShowInfoMessage("자료검증이 완료되었습니다.");
        }
	}
}