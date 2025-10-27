using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class ParamHelper
    {
        #region Oracle Parameter Add

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// parameterValue = parameterNull 이면 DBNull 처리
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, string parameterName, OracleType dbType, object parameterNull, object parameterValue)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dbType);

            if (parameterValue.ToString() == parameterNull.ToString())
                parameter.Value = DBNull.Value;
            else
                parameter.Value = parameterValue;

            OracleParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, string parameterName, OracleType dbType, object parameterValue)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dbType);
            parameter.Value = parameterValue;

            OracleParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 타입속성을 추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="direction">OracleParameter의 출력 속성지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, string parameterName, OracleType dbType, ParameterDirection direction, object parameterValue)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dbType);
            parameter.Value = parameterValue;
            parameter.Direction = direction;

            OracleParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 길이을  추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="size">추가하고자 하는 OracleParameter의 Size지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, string parameterName, OracleType dbType, int size, object parameterValue)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dbType, size);
            parameter.Value = parameterValue;

            OracleParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 타입속성을 추가적으로 지정할수 있다.
        /// OracleParameter 의 길이을  추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="size">추가하고자 하는 OracleParameter의 Size지정</param>
        /// <param name="direction">OracleParameter의 출력 속성지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, string parameterName, OracleType dbType, int size, ParameterDirection direction, object parameterValue)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dbType, size);
            parameter.Value = parameterValue;
            parameter.Direction = direction;

            OracleParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 타입속성을 추가적으로 지정할수 있다.
        /// OracleParameter 의 길이을  추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="size">추가하고자 하는 OracleParameter의 Size지정</param>
        /// <param name="direction">OracleParameter의 출력 속성지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, string parameterName, OracleType dbType, ParameterDirection direction)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dbType);
            parameter.Direction = direction;

            OracleParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter[] 를 추가한다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="newParameters">추가하고자 하는 OracleParameter[]</param>
        public static void OracleParamAdd(ref OracleParameter[] paramArray, params OracleParameter[] newParameters)
        {
            OracleParameter[] newArray = Array.CreateInstance(typeof(OracleParameter), paramArray.Length + newParameters.Length) as OracleParameter[];
            paramArray.CopyTo(newArray, 0);
            newParameters.CopyTo(newArray, paramArray.Length);

            paramArray = newArray;
        }

        #endregion

        #region SQL Parameter Add

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// parameterValue = parameterNull 이면 DBNull 처리
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, string parameterName, SqlDbType dbType, object parameterNull, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);

            if (parameterValue.ToString() == parameterNull.ToString())
                parameter.Value = DBNull.Value;
            else
                parameter.Value = parameterValue;

            SqlParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, string parameterName, SqlDbType dbType, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);
            parameter.Value = parameterValue;

            SqlParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 타입속성을 추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="direction">OracleParameter의 출력 속성지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, string parameterName, SqlDbType dbType, ParameterDirection direction, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);
            parameter.Value = parameterValue;
            parameter.Direction = direction;

            SqlParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 길이을  추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="size">추가하고자 하는 OracleParameter의 Size지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, string parameterName, SqlDbType dbType, int size, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType, size);
            parameter.Value = parameterValue;

            SqlParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 타입속성을 추가적으로 지정할수 있다.
        /// OracleParameter 의 길이을  추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="size">추가하고자 하는 OracleParameter의 Size지정</param>
        /// <param name="direction">OracleParameter의 출력 속성지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, string parameterName, SqlDbType dbType, int size, ParameterDirection direction, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType, size);

            parameter.Value = parameterValue;
            parameter.Direction = direction;

            SqlParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter 를 생성 Array에 추가한다.
        /// OracleParameter 의 타입속성을 추가적으로 지정할수 있다.
        /// OracleParameter 의 길이을  추가적으로 지정할수 있다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="parameterName">새롭게 생성하고자 하는 OracleParameter 에 사용할 이름</param>
        /// <param name="dbType">추가하고자 하는 OracleParameter의 DB타입지정</param>
        /// <param name="size">추가하고자 하는 OracleParameter의 Size지정</param>
        /// <param name="direction">OracleParameter의 출력 속성지정</param>
        /// <param name="paramValue">새롭게 생성하고자 하는 OracleParameter 에 사용할 값</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, string parameterName, SqlDbType dbType, ParameterDirection direction)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);
            parameter.Direction = direction;

            SqlParamAdd(ref paramArray, parameter);
        }

        /// <summary>
        /// ref로써 OracleParameter[]를 받아서 새로운 OracleParameter[] 를 추가한다.
        /// </summary>
        /// <param name="paramArray">기존에 OracleParameter[]</param>
        /// <param name="newParameters">추가하고자 하는 OracleParameter[]</param>
        public static void SqlParamAdd(ref SqlParameter[] paramArray, params SqlParameter[] newParameters)
        {
            SqlParameter[] newArray = Array.CreateInstance(typeof(SqlParameter), paramArray.Length + newParameters.Length) as SqlParameter[];
            paramArray.CopyTo(newArray, 0);
            newParameters.CopyTo(newArray, paramArray.Length);

            paramArray = newArray;
        }

        #endregion
    }
}
