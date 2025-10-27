using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.COM
{
    public static class ProgramSettings
    {
        public const string LibrarySettingFilePath = @".\RFIDLibrarystringSetting.xml";
        public const string ReaderModelMappingFilePath = @".\RFIDReaderLibraryMapping.xml";

        public const string ProgramSettingFileName = @".\ProgramSettings.xml";
        public const string LocalProgramSettingFileName = @".\LocalProgramSettings.xml";

        public const string EventDataSaveFilePath = @".\EventDataSave.txt";
        public const string NotifyDataSaveFilePath = @".\NotifyDataSave.txt";
        public const string TagDataSaveFilePath = @".\TAGDATA_SAVE.txt";
        public const string TagDataSnapshotFilePath = @".\TAGDATA_SNAPSHOT.txt";

        public const string ReaderConnectionTypeEthernet = "ETHERNET";
        public const string ReaderConnectionTypeSerial = "SERIAL";
        public const string LogFolder = @".\Log\";

        public const string SystemLogFilename = "SYS";
        public const string ApplicationLogFilename = "APP";
        public const string TagDataLogFilename = "TAG";
        public const bool DateAppendToLogFilename = true;

        public const string ReaderITPU9R = "ITP_U9R";
        public const string ReaderMKUR300 = "MKUR_300";

        public const string ReaderConnectionState = "CON";
        public const string ReaderDisconnectionState = "DIS";

        //        public const int MaxLogtabRowCount            = 5000;

        //        public const int TAGDATA_BUFFER_ONEDATA_SIZE  = 5;

        public const int TrasnferDataMaxSize = 10;

        //        public const int HeartBeatTimeInterval        = 5000; // 밀리세컨드
        //        public const int MutexWaitTime                = 5000; // 밀리세컨드
        //        public const int PingTimeoutSecond            = 5;
        public const int MaxLogListboxCount = 200;

        public static string MiddlewareID = "SN2097E001";

        // Tag 데이터 처리 주기
        public static int TagReadInterval = 500;
        // Tag 데이터 분리 주기(지정된 시간동안 인식하지 않을 경우)
        public static int TagDivideTerm = 2000;
        // Tag 데이터 저장 주기
        public static int TagStoreInterval = 5000;

        //public static int EventDataProcessInterval    = 5000;
        // 데이터 통신 주기
        public static int SendInterval = 10000;
        // 중복 체크 주기
        public static int DuplicateCheckTerm = 1000;
        // 게이트 데이터 분리 주기
        public static int MaxRecognizeTerm = 5000;
        // UPS 배터리 오류 발생 후 SMS 전송 시간(초)
        public static int UPSSMSSendTerm = 60;
        // UPS 배터리 오류 발생 후 시스템 중지 시간(초)
        public static int UPSShutdownTerm = 300;

        // Tag Mask 사용여부
        public static bool bTagIDFilterYN = false;
        // Tag Mask 시 사용할 고정값
        public static string TagIDFilterValue = string.Empty;

        // 최소 읽기 필터 사용여부
        //public static bool IsMinFilterUse             = false;
        // 최소 읽기 카운트
        //public static int MinReadCountValue           = 0;
        // 차량 번호 여부 사용유무
        //public static bool IsCarNoFilterUse           = false;

        // 태그 무시 시간 사용여부
        //public static bool UseIgnoreTime              = false;
        // 태그 무시 시간 주기
        //public static int IgnoreTime                  = 0;

        // 서버 설정 정보 저장

        // 데이터 미들웨어 서버 IP
        public static string ServerIP = "192.168.111.100";
        // 데이터 미들웨어 서버 Port
        public static int ServerPort = 21000;

        // 무선 MAC Address
        public static string WirelessMACAddress = string.Empty;
        // 유선 MAC Address
        public static string WireMACAddress = string.Empty;

        // Health Check 사용여부
        //public static bool      UseHealthCheck                  = false;
        // Health Check 전송 주기
        //public static int       HealthCheckInterval             = 0;

        // 시간 동기화 주기
        public static int TimeSyncInterval = 180;
        // 통신 재시도 횟수
        public static int SendRetryCount = 10;

        // 파일 로그 사용여부
        public static bool APPUseFileLog = true;
        // 프로그램 시작시 자동 리더기 연결 여부
        public static bool IsAutoConnectAtStart = true;
        // 다중실행 방지 여부
        public static bool IsPreventMultiInstance = true;

        //public static DataTable RFIDReaderConnectionSetting = new DataTable();
        //public static DataTable RFIDLibraryStringInfo = new DataTable();
        //public static DataTable RFIDReaderModelLibraryMap = new DataTable();
        //public static DataTable DisplaySetting = new DataTable();
        //        public static ConfigSettings settings                 = new ConfigSettings();

        public const bool APP_bUseFileLog = true;
        public const string LOG_LogFilePath = "Logs";
        public const string LOG_SystemLogFilename = "SYS";
        public const string LOG_ApplicationLogFilename = "APP";
        public const bool LOG_bDateAppendToLogFileName = true;


    }
}
