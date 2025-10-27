using BPNS.BIZ.Base;
using BPNS.DAC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.BIZ
{
    public class BizMWwithService
    {

        /// <summary>
        /// 리더 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public DataTable GetReaderData()
        {
            try
            {
                SqlParameter[] param = new SqlParameter[] { };                

                return NTxBaseService.ExecuteDataSetBySP("MWCONN", "PD_POB_READER_INFO_SEL", null, param).Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Log 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public DataTable GetLogData()
        {
            try
            {
                SqlParameter[] param = new SqlParameter[] { };

                return NTxBaseService.ExecuteDataSetBySP("MWCONN", "POB_MW_LOG_SEL", null, param).Tables[0];
            }           
            catch (OverflowException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 출입 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public DataTable GetTagData()
        {
            return null;

            //try
            //{
            //    SqlParameter[] param = new SqlParameter[] { };

            //    return NTxBaseService.ExecuteDataSetBySP("MWCONN", "POB_READER_DATA_ROW_SEL", null, param).Tables[0];
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }






        /// <summary>
        /// 로그데이터 쓰기
        /// </summary>
        /// <param name="Log_Date">로그 일자</param>
        /// <param name="Log_Msg">로그 메시지</param>
        /// <param name="Log_Sender">로그 전달 개체</param>
        /// <param name="Log_Event">로그 이벤트 구분</param>
        /// <returns></returns>
        public void SetLogData(string Log_Date, string Log_Msg, string Log_Sender, string Log_Event)
        {
            try
            {
                SqlParameter[] param = new SqlParameter[] { };
                ParamHelper.SqlParamAdd(ref param, "@I_Log_Date",   SqlDbType.NVarChar, Log_Date);                
                ParamHelper.SqlParamAdd(ref param, "@I_Log_Msg",    SqlDbType.NVarChar, Log_Msg.Length > 190 ? Log_Msg.Substring(0, 190):Log_Msg);
                ParamHelper.SqlParamAdd(ref param, "@I_Log_Sender", SqlDbType.NVarChar, Log_Sender);
                ParamHelper.SqlParamAdd(ref param, "@I_Log_Event",  SqlDbType.NVarChar, Log_Event);                

                TxBaseService.ExecuteNoneQueryBySP("MWCONN", "POB_MW_LOG_SAVE", null, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        public void SetReadData(string READER_ID, string READER_NAME, string TAG_UID, string TAG_COUNT, string READTIME, string ANT_ID)
        {
            try
            {
                SqlParameter[] param = new SqlParameter[] { };
                ParamHelper.SqlParamAdd(ref param, "@I_READER_ID",  SqlDbType.NVarChar, READER_ID);
                ParamHelper.SqlParamAdd(ref param, "@I_READER_NM",  SqlDbType.NVarChar, READER_NAME);
                ParamHelper.SqlParamAdd(ref param, "@I_TAGUID",     SqlDbType.NVarChar, TAG_UID);
                ParamHelper.SqlParamAdd(ref param, "@I_TAG_COUNT",  SqlDbType.NVarChar, TAG_COUNT);
                ParamHelper.SqlParamAdd(ref param, "@I_ReadTime",   SqlDbType.NVarChar, READTIME);
                ParamHelper.SqlParamAdd(ref param, "@I_ANT_ID",     SqlDbType.NVarChar, ANT_ID);

                TxBaseService.ExecuteNoneQueryBySP("MWCONN", "POB_READER_DATA_ROW_SAVE", null, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 출입데이터 삭제, 전송한 데이터는 삭제 한다.
        /// </summary>
        /// <param name="READER_ID">리더기 아이디</param>
        /// <param name="TAG_UID">테그아이디</param>
        /// <param name="READTIME">읽은 시간</param>
        /// <param name="ANT_ID">안테나 아이디</param>
        /// <returns></returns>
        public void DelReadData(string READER_ID, string TAG_UID, string READTIME, string ANT_ID)
        {
            try
            {
                SqlParameter[] param = new SqlParameter[] { };
                ParamHelper.SqlParamAdd(ref param, "@I_READER_ID",   SqlDbType.NVarChar, READER_ID);
                ParamHelper.SqlParamAdd(ref param, "@I_TAG_UID",     SqlDbType.NVarChar, TAG_UID);
                ParamHelper.SqlParamAdd(ref param, "@I_ReadTime",     SqlDbType.NVarChar, READTIME);
                ParamHelper.SqlParamAdd(ref param, "@I_ANT_ID",      SqlDbType.NVarChar, ANT_ID);

                TxBaseService.ExecuteNoneQueryBySP("MWCONN", "POB_READER_DATA_ROW_DEL", null, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
