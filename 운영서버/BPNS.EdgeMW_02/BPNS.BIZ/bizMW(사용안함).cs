using BPNS.COM;
using BPNS.DAC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.BIZ
{
    public class bizMW
    {
        
        /// <summary>
        /// 리더 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public static DataTable GetReaderData()
        {
            ParameterCollection parameters = new ParameterCollection();

            QueryAssist query = new QueryAssist();

            DataSet ds = query.ExecuteDataSet("POB", "PD_POB_READER_INFO_SEL", parameters);

            return ds?.Tables[0];
        }

        /// <summary>
        /// Log 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLogData()
        {
            ParameterCollection parameters = new ParameterCollection();

            QueryAssist query = new QueryAssist();

            DataSet ds = query.ExecuteDataSet("POB", "POB_MW_LOG_SEL", parameters);

            return ds?.Tables[0];
        }


        /// <summary>
        /// 출입 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTagData()
        {
            ParameterCollection parameters = new ParameterCollection();

            QueryAssist query = new QueryAssist();

            DataSet ds = query.ExecuteDataSet("POB", "POB_READER_DATA_ROW_SEL", parameters);

            return ds?.Tables[0];
        }

        /// <summary>
        /// 출입데이터 삭제, 전송한 데이터는 삭제 한다.
        /// </summary>
        /// <param name="READER_ID">리더기 아이디</param>
        /// <param name="TAG_UID">테그아이디</param>
        /// <param name="READTIME">읽은 시간</param>
        /// <param name="ANT_ID">안테나 아이디</param>
        /// <returns></returns>
        public static OutParameter DelReadData(string READER_ID, string TAG_UID, string READTIME, string ANT_ID)
        {
            OutParameter outParameter = new OutParameter();

            ParameterCollection parameters = new ParameterCollection();

            parameters.Add(new Parameter("I_READER_ID",     READER_ID,      "Varchar2"));
            parameters.Add(new Parameter("I_TAG_UID",       TAG_UID,        "Varchar2"));
            parameters.Add(new Parameter("I_READIME",       READTIME,       "Varchar2"));
            parameters.Add(new Parameter("I_ANT_ID",        ANT_ID,         "Varchar2"));
            parameters.Add(new Parameter("O_APP_CODE",      "Varchar2",     ParameterDirection.Output, 100));
            parameters.Add(new Parameter("O_APP_MSG",       "Varchar2",     ParameterDirection.Output, 4000));


            OutParameter currentOutParameter = new OutParameter();

            QueryAssist query = new QueryAssist();

            currentOutParameter = query.ExecuteNonQuery("POB", "POB_READER_DATA_ROW_DEL", parameters);

            query.SetOutPameterList(ref outParameter, currentOutParameter);

            return outParameter;
        }

        /// <summary>
        /// 테그 정보 쓰기
        /// </summary>
        /// <param name="READER_ID">리더기 아이디</param>
        /// <param name="READER_NAME">리더기 명</param>
        /// <param name="TAG_UID">테그 아이디</param>
        /// <param name="TAG_COUNT">테그 읽은 수</param>
        /// <param name="READTIME">테그 읽은 시간</param>
        /// <param name="ANT_ID">안테나 아이디</param>
        /// <returns></returns>
        public static OutParameter SetReadData(string READER_ID, string READER_NAME, string TAG_UID, string TAG_COUNT, string READTIME, string ANT_ID)
        {
            try
            {
                OutParameter outParameter = new OutParameter();

                ParameterCollection parameters = new ParameterCollection();

                parameters.Add(new Parameter("I_READER_ID", READER_ID, "Varchar2"));
                parameters.Add(new Parameter("I_READER_NM", READER_NAME, "Varchar2"));
                parameters.Add(new Parameter("I_TAGUID", TAG_UID, "Varchar2"));
                parameters.Add(new Parameter("I_TAG_COUNT", TAG_COUNT, "Varchar2"));
                parameters.Add(new Parameter("I_ReadTime", READTIME, "Varchar2"));
                parameters.Add(new Parameter("I_ANT_ID", ANT_ID, "Varchar2"));
                parameters.Add(new Parameter("O_APP_CODE", "Varchar2", ParameterDirection.Output, 100));
                parameters.Add(new Parameter("O_APP_MSG", "Varchar2", ParameterDirection.Output, 4000));


                OutParameter currentOutParameter = new OutParameter();

                QueryAssist query = new QueryAssist();

                currentOutParameter = query.ExecuteNonQuery("POB", "POB_READER_DATA_ROW_SAVE", parameters);

                query.SetOutPameterList(ref outParameter, currentOutParameter);



                return outParameter;
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }


        /// <summary>
        /// 로그 데이터 쓰기
        /// </summary>
        /// <param name="Log_Date">로그 일자</param>
        /// <param name="Log_Msg">로그 메시지</param>
        /// <param name="Log_Sender">로그 전달 개체</param>
        /// <param name="Log_Event">로그 이벤트 구분</param>
        /// <returns></returns>
        public static OutParameter SetLogData(string Log_Date, string Log_Msg, string Log_Sender, string Log_Event)
        {
            OutParameter outParameter = new OutParameter();

            ParameterCollection parameters = new ParameterCollection();

            parameters.Add(new Parameter("I_Log_Date",      Log_Date,       "Varchar2"));
            parameters.Add(new Parameter("I_Log_Msg",       Log_Msg,        "Varchar2"));
            parameters.Add(new Parameter("I_Log_Sender",    Log_Sender,     "Varchar2"));
            parameters.Add(new Parameter("I_Log_Event",     Log_Event,      "Varchar2"));
            parameters.Add(new Parameter("O_APP_CODE",      "Varchar2",     ParameterDirection.Output, 100));
            parameters.Add(new Parameter("O_APP_MSG",       "Varchar2",     ParameterDirection.Output, 4000));


            OutParameter currentOutParameter = new OutParameter();

            QueryAssist query = new QueryAssist();

            currentOutParameter = query.ExecuteNonQuery("POB", "POB_MW_LOG_SAVE", parameters);

            //query.SetOutPameterList(ref outParameter, currentOutParameter);



            return outParameter;
        }

    }
}
