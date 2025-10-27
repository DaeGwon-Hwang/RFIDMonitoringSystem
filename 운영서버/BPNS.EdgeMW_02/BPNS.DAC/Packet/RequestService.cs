
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class RequestService
    {
        int nCount;

        public string DB { set; get; }
        public TxType TransactionType { set; get; }

        List<RequestPacket> listPacket = new List<RequestPacket>();

        public RequestService()
        {
            nCount = 0;
            listPacket = new List<RequestPacket>();
        }

        public int Count
        {
            get { return this.nCount; }
        }

        public void AddService(RequestPacket QP)
        {
            listPacket.Add(QP);
            nCount++;
        }

        public RequestPacket GetService(int nIndex)
        {
            return listPacket[nIndex];
        }
    }
}
