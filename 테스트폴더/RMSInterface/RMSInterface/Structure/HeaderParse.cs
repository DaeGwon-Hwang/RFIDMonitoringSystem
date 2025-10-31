using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RMSInterface.Structure;

namespace RMSInterface.Structure
{
    public class HeaderParse
    {
        public  int headerSize { get; private set; } = 12;
        private RMSInterface.Structure.Header header {  get; set; }


        public HeaderParse() 
        {
            header = new RMSInterface.Structure.Header();
        }

        public Header SetHeader(byte[] bytes)
        {
            header = new RMSInterface.Structure.Header();
            byte[] _headerPackit = new byte[headerSize];
            Buffer.BlockCopy(bytes, 0, _headerPackit, 0, headerSize);
            Parse(_headerPackit);

            return header;
        }

        private void Parse(byte[] bytes)
        {
            int offset = 0;
            header.CommanType = Parse2Short(bytes, ref offset);
            header.Status = (short)Parse2Short(bytes, ref offset);
            header.Reserved1 = Parse2Short(bytes, ref offset);
            header.Reserved2 = Parse2Short(bytes, ref offset);
            header.TitleLength = Parse2Int(bytes, ref offset);
            header.BodyLength = Parse2Int(bytes, ref offset);
        }

        private ushort Parse2Short(byte[] bytes, ref int offset)
        {
            byte[] temp = new byte[1];
            Array.Copy(bytes, offset, temp, 0, temp.Length);
            offset += temp.Length;
            return (ushort)BitConverter.ToInt32(temp, 0);
        }

        private int Parse2Int(byte[] bytes, ref int offset)
        {
            byte[] temp = new byte[4];
            Array.Copy(bytes, offset, temp, 0, temp.Length);
            //Array.Reverse(temp);
            offset += temp.Length;
            return (int)BigEndian2Int(temp); // BitConverter.ToInt32(temp, 0);
        }

        private string Parse2Chars(byte[] bytes, ref int offset)
        {
            byte[] temp = new byte[2];
            Array.Copy(bytes, offset, temp, 0, temp.Length);
            offset += temp.Length;
            return Bytes2String(temp);
        }

        private string Parse2Char(byte[] bytes, ref int offset)
        {
            byte[] temp = new byte[1];
            Array.Copy(bytes, offset, temp, 0, temp.Length);
            offset += temp.Length;
            return Bytes2String(temp);
        }

        private string Bytes2String(byte[] bytes)
        {
            string result = string.Empty;
            foreach (byte b in bytes) result += b.ToString();
            return result;
        }

        public static int LittleEndian2Int(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 4)
                throw new ArgumentException("Invalid byte array length.");

            // BitConverter는 시스템 엔디안을 따름 (대부분 리틀엔디안)
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int BigEndian2Int(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 4)
                throw new ArgumentException("Invalid byte array length.");

            byte[] temp = new byte[4];
            Array.Copy(bytes, 0, temp, 0, 4);
            Array.Reverse(temp); // Big → Little 변환
            return BitConverter.ToInt32(temp, 0);
        }

        /// <summary>
        /// byte[] leBytes = Int2LittleEndian(0x12345678);
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Int2LittleEndian(int value)
        {
            return BitConverter.GetBytes(value); // 시스템 엔디안 (보통 Little)
        }

        /// <summary>
        /// byte[] beBytes = Int2BigEndian(0x12345678);
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Int2BigEndian(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes); // Little → Big
            return bytes;
        }
    }
}
