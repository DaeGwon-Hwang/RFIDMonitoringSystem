using System;
using RFID_CONTROLLER.Collections;
using RFID_CONTROLLER.Controls.Editors;
using RFID_CONTROLLER.Controls.Grid;

namespace RFID_CONTROLLER.Controls.Programs
{
	public interface IBaseProgram
	{
		/// <summary>
		/// 조회버튼을 위한 메소드
		/// </summary>
		void SearchBtnClick(object sender, EventArgs e);

		/// <summary>
		/// 다른 프로그램에서 조건값과 같이 프로그램을 열었을 때를 위한 메소드
		/// </summary>
		void SearchWithData(Data<string> data);

		/// <summary>
		/// 신규버튼을 위한 메소드
		/// </summary>
		void NewBtnClick(object sender, EventArgs e);

		/// <summary>
		/// 저장버튼을 위한 메소드
		/// </summary>
		void SaveBtnClick(object sender, EventArgs e);

		/// <summary>
		/// 삭제버튼을 위한 메소드
		/// </summary>
		void DeleteBtnClick(object sender, EventArgs e);

		/// <summary>
		/// 출력버튼을 위한 메소드
		/// </summary>
		void PrintBtnClick(object sender, EventArgs e);

		/// <summary>
		/// 초기화버튼을 위한 메소드
		/// </summary>
		void InitBtnClick(object sender, EventArgs e);

		/// <summary>
		/// F1 또는 헬프버튼을 눌렀을 때를 위한 메소드
		/// </summary>
		void HelpBtnClick(object sender, EventArgs e);

		/// <summary>
		/// 프로그램을 열거나, 포커스가 갔을 경우에 호출하는 메소드
		/// </summary>
		void ShownHandler(object sender, EventArgs e);

		/// <summary>
		/// IBaseEditor 들의 값이 변경되는 중에 호출되는 메소드
		/// </summary>
		void ValueChangingHandler(object sender, DataValueChangedEventArgs e);

		/// <summary>
		/// IBaseEditor 들의 값이 변경되었을 때 호출되는 메소드
		/// </summary>
		void ValueChangedHandler(object sender, DataValueChangedEventArgs e);

		/// <summary>
		/// GridView 의 값이 변경되는 중에 발생하는 이벤트를 위한 메소드
		/// </summary>
		void GridValueChangingHandler(object sender, GridValueChangedEventArgs e);

		/// <summary>
		/// GridView 의 값이 변경되었을 때를 위한 메소드
		/// </summary>
		void GridValueChangedHandler(object sender, GridValueChangedEventArgs e);

		/// <summary>
		/// GridView 의 행 클릭을 위한 메소드
		/// </summary>
		void GridRowClickHandler(object sender, EventArgs e);

		/// <summary>
		/// GridView 의 행 더블클릭을 위한 메소드
		/// </summary>
		void GridRowDoubleClickHandler(object sender, EventArgs e);

		
	}
}