using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class ParamAttribute
    {
        string _strParam;
        OracleType _oraCleType;
        object _Value;

        public ParamAttribute()
        {
            //
            // TODO: 여기에 생성자 논리를 추가합니다.
            //
        }

        public string strParam
        {
            set { this._strParam = value; }
            get { return this._strParam; }
        }

        public OracleType oracleType
        {
            set { this._oraCleType = value; }
            get { return this._oraCleType; }
        }

        public object Value
        {
            set { this._Value = value; }
            get { return this._Value; }
        }
    }

    [Serializable]
    public class ParamAttributeCollection
    {
        ParamAttribute[] _Params;

        public ParamAttributeCollection()
        {
        }

        public ParamAttribute[] Params
        {
            set { this._Params = value; }
            get { return this._Params; }
        }
    }
}
