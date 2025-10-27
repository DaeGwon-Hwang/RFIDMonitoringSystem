
namespace RFIDTagWriter
{
    using Ionic.Zip;
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    // 로그 파일 관리 클래스
    public class Log
    {
        // 파일 보관 기간(일자)
        private int nStorageTerm = 30;
        public int StorageTerm
        {
            get { return nStorageTerm; }
            set { nStorageTerm = value; }
        }

        // 로그 파일 패스
        private string strFilePath;
        public string FilePath
        {
            get { return strFilePath; }
            set { strFilePath = value; }
        }

        // 로그 파일 명
        private string strFileName;
        public string FileName
        {
            get { return strFileName; }
            set { strFileName = value; }
        }

        // 파일명에 일자 추가 여부 true인 경우 로그 파일명에 일자 추가
        private bool bDateAppendToFileName;
        public bool DateAppendToFileName
        {
            get { return bDateAppendToFileName; }
            set { bDateAppendToFileName = value; }
        }

        // 멀티 쓰레드의 경우 로그 파일 함수를 동시에 실행하지 않기 위한 크리티컬 오브젝트
        private object oCriticalObject = new object();

        private DateTime lastLogDateTime = new DateTime(1901, 1, 1);

        // 호출 함수정보와 로그 문자열 저장 함수
        // 2016-05-16 15:25:49 819] 서버 Connection 오류
        // yyyy-MM-dd HH:mm:ss fff] [로그 문자열]
        public void Write(string logString)
        {
            string logFileFullName;
            FileStream logFileStream;
            StreamWriter logStreamWriter;
            StringBuilder logMsg;
            System.Diagnostics.StackTrace callStack;

            //if (!Properties.Settings.Default.APP_bUseFileLog)
            //    return;

            logFileStream = null;
            logStreamWriter = null;

            lock (oCriticalObject)
            {
                try
                {
                    string logDir;
                    logDir = Path.Combine(Application.StartupPath, string.Format("{0}", FilePath));
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    if (bDateAppendToFileName)
                    {
                        if (!lastLogDateTime.Date.Equals(DateTime.Now.Date))
                            lastLogDateTime = DateTime.Now.Date;
                        logFileFullName = Path.Combine(Application.StartupPath, string.Format("{0}\\{1}_{2:yyyyMMdd}.log", FilePath, FileName, lastLogDateTime));
                    }
                    else
                        logFileFullName = Path.Combine(Application.StartupPath, string.Format("{0}\\{1}.log", FilePath, FileName));

                    logFileStream = new FileStream(logFileFullName, FileMode.Append, FileAccess.Write);
                    logStreamWriter = new StreamWriter(logFileStream);
                    callStack = new System.Diagnostics.StackTrace();
                    logMsg = new StringBuilder();
                    logMsg.AppendFormat("{0:yyyy-MM-dd HH:mm:ss fff}] ", DateTime.Now);
                    //                    logMsg.AppendFormat("[{0}][{1}] : ", sender.GetType().ToString(), callStack.GetFrame(1).GetMethod());
                    logMsg.Append(logString);

                    logStreamWriter.WriteLine(logMsg);
                    //logStreamWriter.Close();
                    //logFileStream.Close();
                    //logFileStream.Dispose();
                }
                catch (Exception e)
                {
                    ((ListBox)(Application.OpenForms["MainForm"].Controls["logListBox"])).Items.Add("Log 파일 저장 오류[" + e.Message + "]");
                }
                finally
                {
                    if (logStreamWriter != null)
                    {
                        logStreamWriter.Close();
                        logStreamWriter = null;
                    }
                    if (logFileStream != null)
                    {
                        logFileStream.Close();
                        logFileStream = null;
                    }
                }
            }
        }


        // 호출 함수정보와 로그 문자열 저장 함수
        // 2016-05-16 15:25:49 182] [POBEdgeMiddleware.DataTransfer.DataTransferModule][Void TagDataTransfer()] : 서버 Connection 오류
        // yyyy-MM-dd HH:mm:ss fff] [클래스명][호출함수명] : [로그 문자열]

        public void Write(object sender, string logString)
        {
            string logFileFullName;
            FileStream logFileStream;
            StreamWriter logStreamWriter;
            StringBuilder logMsg;
            //System.Diagnostics.StackTrace callStack;

            // 파일로그 사용여부 설정 확인
            //if (!Properties.Settings.Default.APP_bUseFileLog)
            //   return;

            logFileStream = null;
            logStreamWriter = null;

            // 로그 저장 기능 중복 실행 방지
            lock (oCriticalObject)
            {
                try
                {
                    string logDir;
                    logDir = Path.Combine(Application.StartupPath, string.Format("{0}", strFilePath));

                    // 로그 폴더 확인 후 없으면 생성
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    // 파일명 생성(일자를 사용할 경우 일자 추가하여 파일명 생성)
                    if (bDateAppendToFileName)
                    {
                        if (!lastLogDateTime.Date.Equals(DateTime.Now.Date))
                            lastLogDateTime = DateTime.Now.Date;
                        logFileFullName = Path.Combine(Application.StartupPath, string.Format("{0}\\{1}_{2:yyyyMMdd}.log", strFilePath, strFileName, lastLogDateTime));
                    }
                    else
                    {
                        logFileFullName = Path.Combine(Application.StartupPath, string.Format("{0}\\{1}.log", strFilePath, strFileName));
                    }

                    // FileStream 생성
                    logFileStream = new FileStream(logFileFullName, FileMode.Append, FileAccess.Write);
                    // StreamWriter 생성 
                    logStreamWriter = new StreamWriter(logFileStream);
                    //callStack = new System.Diagnostics.StackTrace();
                    // 로그 문자열 작성
                    logMsg = new StringBuilder();
                    logMsg.AppendFormat("{0:yyyy-MM-dd HH:mm:ss fff}] ", DateTime.Now);
                    //logMsg.AppendFormat("[{0}][{1}] : ", sender.GetType().ToString(), callStack.GetFrame(1).GetMethod());
                    logMsg.Append(logString);
                    // 파일 저장
                    logStreamWriter.WriteLine(logMsg);
                }
                catch (Exception e)
                {
                    ((ListBox)(Application.OpenForms["MainForm"].Controls["logListBox"])).Items.Add("Log 파일 저장 오류[" + e.Message + "]");
                }
                finally
                {
                    if (logStreamWriter != null)
                    {
                        logStreamWriter.Close();
                        logStreamWriter = null;
                    }
                    if (logFileStream != null)
                    {
                        logFileStream.Close();
                        logFileStream = null;
                    }
                }
            }
        }

        #region ErrorWrite
        // 오류 로그 저장(호출자, 오류메시지) - 예상되는 오류용으로 작성하였으나 구분이 모호 하여 Exception 정보를 같이 write하는 함수 사용
        //public void ErrorWrite(object sender, string logString)
        //{
        //    string logFileFullName;
        //    FileStream logFileStream;
        //    StreamWriter logStreamWriter;
        //    StringBuilder logMsg;
        //    System.Diagnostics.StackTrace callStack;

        //    if (!Properties.Settings.Default.APP_bUseFileLog)
        //        return;

        //    lock (oCriticalObject)
        //    {
        //        try
        //        {
        //            string logDir;
        //            logDir = System.IO.Path.Combine(Application.StartupPath, string.Format("{0}", strFilePath));
        //            if (!Directory.Exists(logDir))
        //            {
        //                Directory.CreateDirectory(logDir);
        //            }

        //            if (bDateAppendToFileName)
        //            {
        //                if (!lastLogDateTime.Date.Equals(DateTime.Now.Date))
        //                    lastLogDateTime = DateTime.Now.Date;
        //                logFileFullName = System.IO.Path.Combine(Application.StartupPath, string.Format("{0}\\{1}_{2:yyyyMMdd}.log", strFilePath, strFileName, lastLogDateTime));
        //            }
        //            else
        //                logFileFullName = System.IO.Path.Combine(Application.StartupPath, string.Format("{0}\\{1}.log", strFilePath, strFileName));

        //            logFileStream = new FileStream(logFileFullName, FileMode.Append, FileAccess.Write);
        //            logStreamWriter = new StreamWriter(logFileStream);
        //            callStack = new System.Diagnostics.StackTrace();
        //            logMsg = new StringBuilder();
        //            logMsg.AppendFormat("{0:yyyy-MM-dd HH:mm:ss}] ", DateTime.Now);
        //            logMsg.AppendFormat("[{0}][{1}] : ", sender.GetType().ToString(), callStack.GetFrame(1).GetMethod());
        //            logMsg.Append(logString);

        //            logStreamWriter.WriteLine(logMsg);
        //            logStreamWriter.Close();
        //            logFileStream.Close();
        //            logFileStream.Dispose();
        //        }
        //        catch (Exception e)
        //        {
        //            ((System.Windows.Forms.ListBox)(System.Windows.Forms.Application.OpenForms["MainForm"].Controls["logListBox"])).Items.Add("Log 파일 저장 오류[" + e.Message + "]");
        //        }
        //    }
        //}
        #endregion

        // ErrorWrite와 Write를 패러미터로 처리
        // 호출 함수정보,로그 문자열, Exception 정보 저장 함수
        // 2016-05-16 15:25:49 182] [POBEdgeMiddleware.DataTransfer.DataTransferModule][Void TagDataTransfer()] : 서버 Connection 오류
        //                          [Exception Type : System.Net.Sockets.SocketException]
        //                          [Exception Message : 현재 연결은 원격 호스트에 의해 강제로 끊겼습니다]
        //                          [Exception Stack Trace :    위치: System.Net.Sockets.Socket.Receive(Byte[] buffer, Int32 offset, Int32 size, SocketFlags socketFlags)
        //                                                  위치: System.Net.Sockets.Socket.Receive(Byte[] buffer, SocketFlags socketFlags)
        //                                                  위치: POBEdgeMiddleware.DataTransfer.DataTransferModule.ConnectionCheck() 파일 D:\Users\dhkim\10. 현대중공업 해양정보부 승선인원관리\05. 구현\Projects\BPNS Project\Edge MiddleWare\DataTransfer\DataTransferModule.cs:줄 1495
        //                                                  위치: POBEdgeMiddleware.DataTransfer.DataTransferModule.TagDataTransfer() 파일 D:\Users\dhkim\10. 현대중공업 해양정보부 승선인원관리\05. 구현\Projects\BPNS Project\Edge MiddleWare\DataTransfer\DataTransferModule.cs:줄 1113]
        // yyyy-MM-dd HH:mm:ss fff] [클래스명][호출함수명] : [로그 문자열]
        //                          [Exception Type : Exception 종류]
        //                          [Exception Message : Exception 메시지]
        //                          [Exception Stack Trace : Stack Trace 정보]

        public void Write(object sender, string logString, Exception ex)
        {
            string logFileFullName;
            FileStream logFileStream;
            StreamWriter logStreamWriter;
            StringBuilder logMsg;
            System.Diagnostics.StackTrace callStack;
            string logDir;

            // 파일 로그 저장 설정값 확인
            //if (!Properties.Settings.Default.APP_bUseFileLog)
            //    return;

            logStreamWriter = null;
            logFileStream = null;

            // Write 함수 중복 실행 방지
            lock (oCriticalObject)
            {
                try
                {
                    // 파일명 생성
                    logDir = Path.Combine(Application.StartupPath, string.Format("{0}", strFilePath));
                    // 로그 폴더 확인 후 없으면 생성
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    // 파일명 생성(일자를 사용할 경우 일자 추가하여 파일명 생성)
                    if (bDateAppendToFileName)
                    {
                        if (!lastLogDateTime.Date.Equals(DateTime.Now.Date))
                            lastLogDateTime = DateTime.Now.Date;
                        logFileFullName = Path.Combine(Application.StartupPath, string.Format("{0}\\{1}_{2:yyyyMMdd}.log", strFilePath, strFileName, lastLogDateTime));
                    }
                    else
                    {
                        logFileFullName = Path.Combine(Application.StartupPath, string.Format("{0}\\{1}.log", strFilePath, strFileName));
                    }
                    
                    // FileStream 생성
                    logFileStream = new FileStream(logFileFullName, FileMode.Append, FileAccess.Write);
                    // StreamWriter 생성
                    logStreamWriter = new StreamWriter(logFileStream);
                    // callStack 정보 처리를 위한 StackTrace 클래스 생성
                    callStack = new System.Diagnostics.StackTrace();
                    // 로그 문자열 생성
                    logMsg = new StringBuilder();
                    logMsg.AppendFormat("{0:yyyy-MM-dd HH:mm:ss fff}] ", DateTime.Now);
                    logMsg.AppendFormat("[{0}][{1}] : ", sender.GetType().ToString(), callStack.GetFrame(1).GetMethod());
                    logMsg.Append(logString);
                    logMsg.AppendLine();
                    logMsg.AppendLine(  "                         [Exception Type : " + ex.GetType().ToString() + "]");
                    logMsg.AppendLine(  "                         [Exception Message : " + ex.Message + "]");
                    logMsg.Append(      "                         [Exception Stack Trace : " + ex.StackTrace.Replace("\n", "\n                                                  ") + "]");
                    // 파일 저장
                    logStreamWriter.WriteLine(logMsg);
                    //logStreamWriter.Close();
                    //logFileStream.Close();
                }
                catch (Exception e)
                {
                    ((ListBox)(Application.OpenForms["MainForm"].Controls["logListBox"])).Items.Add("Log 파일 저장 오류[" + e.Message + "]");
                }
                finally
                {
                    if (logStreamWriter != null)
                    {
                        logStreamWriter.Close();
                        logStreamWriter = null;
                    }
                    if (logFileStream != null)
                    {
                        logFileStream.Close();
                        logFileStream = null;
                    }
                }
            }
        }

        // nStorageTerm 변수에 지정된 일자를 초과한 이전 파일을 삭제한다.
        // 당일 로그 파일을 제외하고 압축
        // 보관 기간을 초과한 압축 파일 삭제
        public void Clearance()
        {
            string strLogDir;
            string strStartDateFileName;
            string strTodayFileLogName;
            string strZipFileName;
            DirectoryInfo diDirectoryInfo;
            FileInfo fiZipFileInfo;
            TimeSpan tsStorageTimeSpan;
            ZipFile zfZipFile;
            try
            {
                // 일자별 파일 저장이 아니면 정리가 의미가 없으므로 종료
                if (!bDateAppendToFileName)
                {
                    return;
                }

                // 중복실행 방지
                lock (oCriticalObject)
                {
                    // 파일 보관 일자 계산을 위한 TimeSpan 생성
                    tsStorageTimeSpan = new TimeSpan(nStorageTerm, 0, 0, 0);

                    // 압축 제외를 위한 당일 로그 파일명 생성
                    strTodayFileLogName = strFileName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                    // 보관기간 시작 파일명 생성(압축파일)
                    strStartDateFileName = strFileName + "_" + (DateTime.Now - tsStorageTimeSpan).ToString("yyyyMMdd") + ".zip";
                    strLogDir = Path.Combine(Application.StartupPath, string.Format("{0}", strFilePath));
                    diDirectoryInfo = new DirectoryInfo(strLogDir);
                    // 파일 압축 루프(오늘 로그 파일을 제외한 파일을 압축한다.
                    foreach (FileInfo fiFileInfo in diDirectoryInfo.EnumerateFiles(strFileName + "_????????.log", SearchOption.TopDirectoryOnly))
                    {
                        // 로그 파일명이 당일 로그 파일명과 다르면 압축
                        if (!strTodayFileLogName.Equals(fiFileInfo.Name))
                        {
                            // 파일 압축
                            strZipFileName = Path.Combine(Path.Combine(Application.StartupPath, strFilePath),fiFileInfo.Name.Replace(".log", ".zip"));
                            fiZipFileInfo = new FileInfo(strZipFileName);
                            if (fiZipFileInfo.Exists)
                            {
                                fiZipFileInfo.Delete();
                            }
                            zfZipFile = new ZipFile(strZipFileName);
                            zfZipFile.AddFile(Path.Combine(Path.Combine(Application.StartupPath, strFilePath), fiFileInfo.Name)).FileName = fiFileInfo.Name;
                            zfZipFile.Save();
                            // 원본 파일 삭제
                            fiFileInfo.Delete();
                        }
                    }

                    // 압축 파일 보관 기간 경과시 삭제 루프
                    foreach (FileInfo fiFileInfo in diDirectoryInfo.EnumerateFiles(strFileName + "_????????.zip", SearchOption.TopDirectoryOnly))
                    {
                        // 파일명이 보관 시작일 파일명보다 적을 경우 삭제a
                        if (strStartDateFileName.CompareTo(fiFileInfo.Name) > 0)
                        {
                            // 파일 삭제
                            fiFileInfo.Delete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ((ListBox)(Application.OpenForms["MainForm"].Controls["logListBox"])).Items.Add("Log 파일 정리 오류[" + e.Message + "]");
            }
        }
    }
}
