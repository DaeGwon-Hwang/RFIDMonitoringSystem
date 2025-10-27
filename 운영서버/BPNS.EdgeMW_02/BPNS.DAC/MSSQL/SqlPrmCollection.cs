using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class SqlPrmCollection : IEnumerable
    {
        #region ## field member ##

        /// <summary>
        /// List&lt;object&gt; for container
        /// </summary>
        private List<object> list;

        /// <summary>
        /// total count
        /// </summary>
        public int Count { get { return this.list.Count; } }

        /// <summary>
        /// indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SqlParameter this[int index] { get { return (SqlParameter)this.list[index]; } }

        /// <summary>
        /// creator
        /// </summary>
        public SqlPrmCollection()
        {
            list = new List<object>();
        }

        #endregion

        #region ## Add ##

        /// <summary>
        /// List&lt;object&gt; 에 SqlParameter 개체를 추가 한다.
        /// </summary>
        /// <param name="name">ParameterName</param>
        /// <param name="value">Value</param>
        public void Add(string name, object value)
        {
            SqlParameter prm = new SqlParameter();

            prm.ParameterName = name;
            prm.Value = value;

            this.list.Add(prm);
        }

        /// <summary>
        /// List&lt;object&gt; 에 SqlParameter 개체를 추가 한다.
        /// </summary>
        /// <param name="name">ParameterName</param>
        /// <param name="type">SqlDbType</param>
        /// <param name="size">Size</param>
        /// <param name="value">Value</param>
        public void Add(string name, SqlDbType type, int size, object value)
        {
            SqlParameter prm = new SqlParameter();

            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            prm.Value = value;

            this.list.Add(prm);
        }

        /// <summary>
        /// List&lt;object&gt; 에 SqlParameter 개체를 추가 한다.
        /// </summary>
        /// <param name="name">ParameterName</param>
        /// <param name="type">SqlDbType</param>
        /// <param name="size">Size</param>
        /// <param name="direction">Direction</param>
        public void Add(string name, SqlDbType type, int size, ParameterDirection direction)
        {
            SqlParameter prm = new SqlParameter();

            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            prm.Direction = direction;

            this.list.Add(prm);
        }

        /// <summary>
        /// List&lt;object&gt; 에 SqlParameter 개체를 추가 한다.
        /// </summary>
        /// <param name="name">ParameterName</param>
        /// <param name="type">SqlDbType</param>
        /// <param name="size">Size</param>
        /// <param name="direction">Direction</param>
        /// <param name="value">Value</param>
        public void Add(string name, SqlDbType type, int size, ParameterDirection direction, object value)
        {
            SqlParameter prm = new SqlParameter();

            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            prm.Direction = direction;
            prm.Value = value;

            this.list.Add(prm);
        }

        #endregion

        #region ## method ##

        /// <summary>
        /// Collection 의 내용을 모두 삭제 합니다.
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
        }

        #endregion

        #region IEnumerable 멤버

        /// <summary>
        /// 개체의 열거자를 반환
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        #endregion
    }
}
