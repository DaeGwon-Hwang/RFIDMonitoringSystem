using System;

namespace RFID.Middleware.Shared
{
    public static class BigEndianConverter
    {
        public static int BigEndian2Int(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array length.");
            return (bytes[startIndex] << 24) | (bytes[startIndex+1] << 16) | (bytes[startIndex+2] << 8) | bytes[startIndex+3];
        }

        public static byte[] Int2BigEndian(int value)
        {
            return new byte[] {
                (byte)((value>>24)&0xFF),
                (byte)((value>>16)&0xFF),
                (byte)((value>>8)&0xFF),
                (byte)(value&0xFF)
            };
        }
    }
}
