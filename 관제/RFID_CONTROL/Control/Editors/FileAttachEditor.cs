using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public partial class FileAttachEditor : UserControl, IBaseEditor {
		public event DataValueChangedEventHandler DataValueChanged;
		public event DataValueChangedEventHandler DataValueChanging;
		
		private string _fileID = "";
		private int _fileMaxSeq = 0;
		private int _maxFileCount = 0;
		private string _fileUploadPath = "";
		private bool _readonly = false;
		private bool _required = false;
		private Control _parentContainer = null;
		private BorderStyle _borderStyle = BorderStyle.None;
		private List<IBaseEditor> _dependantEditors = new List<IBaseEditor>();

		private string _defaultExt = "";
		private string _filter = "";
		
		private DataList<Data<string>> _fileParams = new DataList<Data<string>>();

		public FileAttachEditor () {
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
					BorderPanel.Padding = new Padding(0);
				} else if (_borderStyle == BorderStyle.Fixed3D) {
				} else if (_borderStyle == BorderStyle.FixedSingle) {
					BorderPanel.Padding = new Padding(1);
				}
			}
		}

		[Category("UserDefine")]
		[Description("Caption")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(string), "첨부파일")]
		public string Caption {
			get => DisplayLabel.Text;
			set => DisplayLabel.Text = value;
		}

		[Category("UserDefine")]
		[Description("DefaultExt")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DefaultExt {
			get => _defaultExt;
			set => _defaultExt = value;
		}

		[Category("UserDefine")]
		[Description("DependantEditors")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public List<IBaseEditor> DependantEditors => _dependantEditors;

		[Category("UserDefine")]
		[Description("파일ID")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string 파일ID {
			get => _fileID;
		}

		[Category("UserDefine")]
		[Description("FileCount")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int FileCount {
			get => _fileParams.Count();
		}

		[Category("UserDefine")]
		[Description("FileUploadPath")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string FileUploadPath {
			get => _fileUploadPath;
			set => _fileUploadPath = value;
		}

		[Category("UserDefine")]
		[Description("FlowDirection")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(FlowDirection), "LeftToRight")]
		public FlowDirection FlowDirection {
			get => AttachPanel.FlowDirection;
			set => AttachPanel.FlowDirection = value;
		}

		[Category("UserDefine")]
		[Description("Filter")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Filter {
			get => _filter;
			set => _filter = value;
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
		[Description("LabelDock")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(DockStyle), "Left")]
		public DockStyle LabelDock
		{
			get => LabelPanel.Dock;
			set => LabelPanel.Dock = value;
		}

		[Category("UserDefine")]
		[Description("LabelSize")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(Size), "63, 13")]
		public Size LabelSize
		{
			get => Label.Size; set => Label.Size = value;
		}

		[Category("UserDefine")]
		[Description("MaxFileCount")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(typeof(int), "0")]
		public int MaxFileCount {
			get => _maxFileCount;
			set => _maxFileCount = value;
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
			get => _readonly;
			set {
				_readonly = value;
				AttachBtn.Visible = !_readonly;
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
			if(Required && FileCount == 0) {
				if (messsage)
					//Utils.ShowWarnMessage(Caption + " 을(를) 확인하세요.");
				return false;
			}
			return true;
		}

		private void FileAttachEditor_Load (object sender, EventArgs e) {
		}

		public void SetFileID (string fileID = "", bool fireEvent = true) {
			//var changed = false;
			//Clear(fireEvent);
			//_fileID = fileID;
			//_fileMaxSeq = 0;
			//if (!fileID.Equals("")) {
			//	Data<string> param = Data.New(Tuple.Create("파일ID", fileID));
			//	var _apiClient = new HttpApiClient(Utils.GetClientSession());
			//	var apiResultTask = _apiClient.Get<DataList<Data<string>>>("/com/file_list", param);
			//	var apiResult = apiResultTask.Result;
			//	if(!apiResult.IsError) {
			//		foreach(var fileInfo in apiResult.Content) {
			//			AddAttachFile(fileInfo);
			//			_fileParams.Add(fileInfo);
			//			_fileMaxSeq = Math.Max(_fileMaxSeq, int.Parse(fileInfo["파일순번"]));
			//			changed = true;
			//		}
			//	} else {
			//		Logger.Get(typeof(ApiUtils)).Error(apiResult.TraceMsg);
			//		Utils.ShowErrorMessage(apiResult.ErrorMsg);
			//	}
			//	if (changed && fireEvent) {
			//		DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
			//		DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
			//	}
			//}
		}

		public Data<string> GetData () {
			var param = new Data<string>();
			param["파일ID"] = 파일ID;
			return param;
		}

		public DataList<Data<string>> GetDataList () {
			return _fileParams;
		}

		public void SetData (Data<string> value, bool fireEvent = true) {
			if (!value.ContainsKey("파일ID")) return;
			SetFileID(value["파일ID"], fireEvent);
		}
		
		public void Clear (bool fireEvent = true) {
			var changed = AttachFileList.Controls.Count > 0;
			_fileID = "";
			_fileMaxSeq = 0;
			_fileParams.Clear();
			while (AttachFileList.Controls.Count > 0)
				AttachFileList.Controls[0].Dispose();
			if (changed && fireEvent) {
				DataValueChanging?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
				DataValueChanged?.Invoke(this, new DataValueChangedEventArgs() { FromUiChanged = false });
			}
		}

		private void AttachBtn_Click(object sender, EventArgs e) {
			if(ReadOnly) {
				//Utils.ShowWarnMessage("읽기전용 모드입니다.");
				return;
			}
			if(FileUploadPath.Equals("")) {
				//Utils.ShowWarnMessage("파일 업로드 경로가 설정되지 않았습니다.");
				return;
			}
			if(MaxFileCount > 0 && FileCount >= MaxFileCount) {
				//Utils.ShowWarnMessage("최대 등록 가능한 파일수를 초과하였습니다.");
				return;
			}
			//var openFileDialog = new OpenFileDialog();
			//if(Filter != "") openFileDialog.Filter = Filter;
			//if (DefaultExt != "") openFileDialog.DefaultExt = DefaultExt;
			//if (openFileDialog.ShowDialog() == DialogResult.OK) {
			//	if(_fileID.Equals("")) _fileID = Utils.NewFileID();
			//	var fileNames = openFileDialog.FileNames;
			//	var uploadParam = new Data<string>();
			//	uploadParam.Add("path", FileUploadPath);
			//	var saveDataList = new DataList<Data<string>>();
			//	var _apiClient = new HttpApiClient(Utils.GetClientSession());
			//	_apiClient.UploadFiles<DataList<Data<string>>>("/com/upload", fileNames, uploadParam).ContinueWith((apiResultTask) => {
			//		var apiResult = apiResultTask.Result;
			//		Logger.Get(this).Debug(apiResult.ToJson());
			//		if (!apiResult.IsError) {
			//			foreach (var info in apiResult.Content) {
			//				info["파일ID"] = _fileID;
			//				info["파일순번"] = (++_fileMaxSeq).ToString("000");
			//				Utils.InvokeAction(this, () => AddAttachFile(info));
			//				_fileParams.Add(info);
			//				info["등록사번"] = Utils.GetClientSession().UserInfo.USER_ID;
			//				info["IP"] = Utils.GetClientSession().IpAddress;
			//				saveDataList.Add(info);
			//			}
			//		} else {
			//			Utils.InvokeAction(this, () => Utils.ShowErrorMessage("파일 업로드에 실패했습니다"));
			//		}
			//	}).ContinueWith((task) => {
			//		if(saveDataList.Count() > 0) {
			//			ApiUtils.SaveDataList(_apiClient, "/com/file_save", saveDataList, false);
			//		}
			//	});
			//}
		}

		private void AddAttachFile (Data<string> fileInfo) {
			//	var fileLabel = new Label() {
			//		ForeColor = Color.FromArgb(0x31, 0xA1, 0xFD),
			//		Text = String.Format("{0}.{1} [{2:#,##0} KB]", fileInfo["파일명"], fileInfo["파일확장자"], Math.Max(int.Parse(fileInfo["파일크기"]) / 1024, 1)),
			//		TextAlign = ContentAlignment.MiddleCenter,
			//		Font = new Font("돋움", 9.75F, FontStyle.Underline, GraphicsUnit.Point, ((byte)(129))),
			//		Margin = new Padding(0, 0, 2, 0),
			//		Cursor = Cursors.Hand,
			//		MinimumSize = new Size(0, 23),
			//		AutoSize = true,
			//		Anchor = AnchorStyles.None
			//	};
			//	var baseUrl = String.Format("{0}://{1}:{2}", Utils.GetClientSession().ApiProtocol, Utils.GetClientSession().ApiBaseUrl, Utils.GetClientSession().ApiPort);
			//	fileLabel.Click += (s, e) => {
			//		Utils.DownloadFile(baseUrl, fileInfo);
			//	};
			//	fileLabel.MouseHover += (s, e) => {
			//		fileLabel.Font = new Font(fileLabel.Font, FontStyle.Underline);
			//	};
			//	fileLabel.MouseDown += (s, e) => {
			//		fileLabel.Font = new Font(fileLabel.Font, FontStyle.Underline);
			//	};
			//	fileLabel.MouseLeave += (s, e) => {
			//		fileLabel.Font = new Font(fileLabel.Font, FontStyle.Regular);
			//	};
			//	var closeLabel = new Label() {
			//		TextAlign = ContentAlignment.MiddleCenter,
			//		Image = Properties.Resources.btn_close,
			//		Margin = new Padding(0),
			//		Cursor = Cursors.Hand,
			//		MinimumSize = new Size(16, 16),
			//		MaximumSize = new Size(16, 16),
			//		AutoSize = true,
			//		Anchor = AnchorStyles.None
			//	};
			//	var panel = new FlowLayoutPanel() {
			//		Margin = new Padding(0),
			//		AutoSize = true,
			//		BackColor = Color.FromArgb(0xE4, 0xEB, 0xFE),
			//		MinimumSize = new Size(0, 23),
			//	};
			//	panel.Controls.Add(fileLabel);
			//	if(!ReadOnly) {
			//		panel.Controls.Add(closeLabel);
			//	}
			//	var borderPanel = new FlowLayoutPanel() {
			//		Margin = new Padding(3, 1, 5, 0),
			//		Padding = new Padding(1),
			//		BackColor = Color.FromArgb(0xFF, 0xFF, 0xFF),
			//		AutoSize = true,
			//		MinimumSize = new Size(0, 24),
			//		Anchor = AnchorStyles.None,
			//		Tag = fileInfo
			//	};
			//	closeLabel.Click += (s, e) => {
			//		borderPanel.Dispose();
			//		var _apiClient = new HttpApiClient(Utils.GetClientSession());
			//		try {
			//			//ApiUtils.DeleteData(_apiClient, "/com/file_delete", fileInfo, false);
			//			var apiResult = _apiClient.Post<int>("/com/file_delete", fileInfo).Result;
			//			if (apiResult.IsError) {
			//				Logger.Get(typeof(ApiUtils)).Error(apiResult.TraceMsg);
			//				Utils.ShowErrorMessage(apiResult.ErrorMsg);
			//			}
			//		} catch(Exception ex) {
			//			Logger.Get(this).Error(ex.StackTrace);
			//		}
			//		try {
			//			var apiResult = _apiClient.Post<int>("/com/deleteFile", fileInfo).Result;
			//			if (apiResult.IsError) {
			//				Logger.Get(typeof(ApiUtils)).Error(apiResult.TraceMsg);
			//				Utils.ShowErrorMessage(apiResult.ErrorMsg);
			//			}
			//		} catch (Exception ex) {
			//			Logger.Get(this).Error(ex.StackTrace);
			//		}
			//		_fileParams.Remove(fileInfo);
			//	};
			//	borderPanel.Controls.Add(panel);
			//	AttachFileList.Controls.Add(borderPanel);
		}
	}
	}
