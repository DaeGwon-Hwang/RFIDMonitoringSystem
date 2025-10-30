using System.ComponentModel;

namespace RMSInterface.Enums
{
    public enum WorkCode
    {
        [Description("서버시간 조회, WD00")] WC001,
        [Description("사용자 조회, WD00")] WC002,
        [Description("공통코드 조회, WD00")] WC003,
        [Description("위치정보 조회, WD00")] WC004,
        [Description("서가배치, WD00")] WC005,
        [Description("서가배치(재배치), WD00")] WC006,
        [Description("정수점검 계획조회, WD00")] WC007,
        [Description("정수점검 상세조회, WD00")] WC008,
        [Description("정수점검 상세결과등록, WD00")] WC009,
        [Description("정수점검 결과등록, WD00")] WC010,
        [Description("반출서작성, WD00")] WC011,
        [Description("반입서작성, WD00")] WC012,
        [Description("유출예방, WD00")] WC013,
        [Description("탐지조회, WD00")] WC014,
        [Description("기록물 정보조회, WD00")] WC015,
    }
}
