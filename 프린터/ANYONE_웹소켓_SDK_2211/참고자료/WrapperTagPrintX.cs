using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RFIDTagWriter
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ST_PrinterSetting
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string strModelName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string strIpAddress;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string strSerialPort;

        public int iConnectType;
        public int iTcpPort;
        public int iRfAttenW;
        public int iRfAttenR;
        public int iRfEncOffset;
        public int iCutPos;
        public int iDarkness;
        public int iSpeed;
        public int iMediaType;
        public int iSensorType;
        public int iHorOffset;
        public int iVerOffset;

        [MarshalAs(UnmanagedType.I1)]
        public bool bUseUMI;
    }


    internal static class WrapperTagPrintX
    { 
        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUp();

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string strFileName);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetVariableData([MarshalAs(UnmanagedType.LPWStr)] string strTagDts, [MarshalAs(UnmanagedType.LPWStr)] string strDelim);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int TagPrint([MarshalAs(UnmanagedType.LPTStr, SizeConst = 512)] StringBuilder sResult, [MarshalAs(UnmanagedType.LPTStr, SizeConst = 128)] StringBuilder sErrorMessage);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int TagPrint2([MarshalAs(UnmanagedType.LPTStr, SizeConst = 512)] StringBuilder sResult, [MarshalAs(UnmanagedType.LPTStr, SizeConst = 128)] StringBuilder sErrorMessage, [MarshalAs(UnmanagedType.I4)] out int nErrorCount);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SendStr([MarshalAs(UnmanagedType.LPWStr)] string strSend);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReadResponse([MarshalAs(UnmanagedType.LPTStr, SizeConst = 512)] StringBuilder sResponse);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SendFile([MarshalAs(UnmanagedType.LPWStr)] string strFilePath);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SendBytes([MarshalAs(UnmanagedType.LPArray)] byte[] pPuf, ulong count);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ClosePort([MarshalAs(UnmanagedType.LPWStr)] string strSerial);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsConnected();

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SendControlCommand([MarshalAs(UnmanagedType.LPWStr)] string strCommand);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Connect(int iType, int iPortIndex, [MarshalAs(UnmanagedType.LPWStr)] string strIp, [MarshalAs(UnmanagedType.LPWStr)] string strModel);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Connect2();

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPrinterSetting(ST_PrinterSetting setupValue);

        [DllImport("ANYONE_TagPrint_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ExecuteBT200InternalCommand([MarshalAs(UnmanagedType.LPWStr)] string strCommand, [MarshalAs(UnmanagedType.LPStr, SizeConst = 512)] StringBuilder sResponse);

        static WrapperTagPrintX()
        {
        }

    }
}
