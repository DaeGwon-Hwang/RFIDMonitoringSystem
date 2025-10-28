using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RFID_CONTROLLER.Collections;

namespace RFID_CONTROLLER.Controls.Editors {
	public class DataValueChangedEventArgs : EventArgs {
		public bool FromUiChanged = true;
        public object Source { get; set; }
	}

	public delegate void DataValueChangedEventHandler(object sender, DataValueChangedEventArgs a);  

	public interface IBaseEditor {
		event DataValueChangedEventHandler DataValueChanged;
		event DataValueChangedEventHandler DataValueChanging;
		string Name { get; set; }
		string Caption { get; set; }
		bool ReadOnly { get; set; }
		bool Required { get; set; }
		IBaseEditor ParentEditor { get; }
		Control ParentContainer { get; set; }
		List<IBaseEditor> DependantEditors { get; }

		Data<string> GetData();
		void SetData(Data<string> value, bool fireEvent = true);
		bool IsValid(bool messsage = true);
		void Clear(bool fireEvent = true);
	}
}