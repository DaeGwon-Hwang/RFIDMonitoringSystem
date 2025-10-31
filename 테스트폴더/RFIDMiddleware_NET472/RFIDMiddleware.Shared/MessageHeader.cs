using System;
using System.Text;

namespace RFID.Middleware.Shared
{
    public class MessageHeader
    {
        public byte CommandType { get; set; }
        public sbyte Status { get; set; }
        public byte Reserved1 { get; set; }
        public byte Reserved2 { get; set; }
        public int TitleLength { get; set; }
        public int BodyLength { get; set; }

        public static MessageHeader FromBytes(byte[] b)
        {
            if (b == null || b.Length < 12) throw new ArgumentException(nameof(b));
            return new MessageHeader
            {
                CommandType = b[0],
                Status = (sbyte)b[1],
                Reserved1 = b[2],
                Reserved2 = b[3],
                TitleLength = (b[4]<<24)|(b[5]<<16)|(b[6]<<8)|b[7],
                BodyLength = (b[8]<<24)|(b[9]<<16)|(b[10]<<8)|b[11]
            };
        }

        public byte[] ToBytes()
        {
            var b = new byte[12];
            b[0] = CommandType;
            b[1] = (byte)Status;
            b[2] = Reserved1;
            b[3] = Reserved2;
            b[4] = (byte)((TitleLength>>24)&0xFF);
            b[5] = (byte)((TitleLength>>16)&0xFF);
            b[6] = (byte)((TitleLength>>8)&0xFF);
            b[7] = (byte)(TitleLength&0xFF);
            b[8] = (byte)((BodyLength>>24)&0xFF);
            b[9] = (byte)((BodyLength>>16)&0xFF);
            b[10] = (byte)((BodyLength>>8)&0xFF);
            b[11] = (byte)(BodyLength&0xFF);
            return b;
        }
    }
}
