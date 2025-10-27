using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPNS.TransitiveMiddleware.Comm
{
    public enum EnumEQState
    {
        Unknown         = -1,
        Normal          = 0,
        Disconnected    = 1,
        Error           = 2,
    }
    public class EQErrorState
    {
        // 장치 ID
        private string strEQID;
        public string EQID
        {
            get { return strEQID; }
            set { strEQID = value; }
        }

        private EnumEQState eEQState;
        public EnumEQState EQState
        {
            get { return eEQState; }
            set { eEQState = value; }
        }

        private EnumEQState eSMSSendState;

        public EnumEQState SMSSendState
        {
            get { return eSMSSendState; }
            set { eSMSSendState = value; }
        }


        private string strEQErrorCode;
        public string EQErrorCode
        {
            get { return strEQErrorCode; }
            set { strEQErrorCode = value; }
        }

        private string strEQErrorMessage;
        public string EQErrorMessage
        {
            get { return strEQErrorMessage; }
            set { strEQErrorMessage = value; }
        }
        private DateTime dtErrorDateTime;
        public DateTime ErrorDateTime
        {
            get { return dtErrorDateTime; }
            set { dtErrorDateTime = value; }
        }

        private DateTime dtLastStateUpdateDateTime;
        public DateTime LastStateUpdateDateTime
        {
            get { return dtLastStateUpdateDateTime; }
            set { dtLastStateUpdateDateTime = value; }
        }

        private bool bSendErrorSMS;
        public bool SendErrorSMS
        {
            get { return bSendErrorSMS; }
            set { bSendErrorSMS = value; }
        }
        private bool bSendNormalSMS;
        public bool SendNormalSMS
        {
            get { return bSendNormalSMS; }
            set { bSendNormalSMS = value; }
        }

        public EQErrorState()
        {
            strEQID = string.Empty;
            eEQState = EnumEQState.Normal;
            strEQErrorCode = "0000";
            strEQErrorMessage = "정상";
            dtLastStateUpdateDateTime = DateTime.Now;
            dtErrorDateTime = DateTime.Now;
            bSendErrorSMS = false;
            bSendNormalSMS = false;
            eSMSSendState = EnumEQState.Unknown;
        }

        public EQErrorState(string pstrEQID, EnumEQState peEQState, string pstrEQErrorCode, string pstrEQErrorMessage,string pstrErrorDateTime)
        {
            strEQID = pstrEQID;
            eEQState = peEQState;
            strEQErrorCode = pstrEQErrorCode;
            strEQErrorMessage = pstrEQErrorMessage;
            dtLastStateUpdateDateTime = DateTime.Now;
            dtErrorDateTime = new DateTime(int.Parse(pstrErrorDateTime.Substring(0, 4)), int.Parse(pstrErrorDateTime.Substring(4, 2)), int.Parse(pstrErrorDateTime.Substring(6, 2)),
                                           int.Parse(pstrErrorDateTime.Substring(8, 2)), int.Parse(pstrErrorDateTime.Substring(10, 2)), int.Parse(pstrErrorDateTime.Substring(12, 2)));
            bSendErrorSMS = false;
            bSendNormalSMS = false;
            eSMSSendState = EnumEQState.Unknown;
        }

        public bool CheckSendErrorSMSTime()
        {
            TimeSpan StateCheckTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.NET_EQErrorCheckTime);
            TimeSpan SMSResendTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.SMSResendSecond);
            if (bSendErrorSMS == false && eEQState != EnumEQState.Normal && (DateTime.Now - dtLastStateUpdateDateTime > StateCheckTimeSpan))
            {
                return true;
            }
            if (bSendErrorSMS == true && eEQState != EnumEQState.Normal && (DateTime.Now - dtLastStateUpdateDateTime > SMSResendTimeSpan))
            {
                return true;
            }
            return false;
        }
        public bool CheckSendNormalSMSTime()
        {
            TimeSpan StateCheckTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.NET_EQErrorCheckTime);
            if (bSendNormalSMS == false && bSendErrorSMS == true && eEQState == EnumEQState.Normal && (DateTime.Now - dtLastStateUpdateDateTime > StateCheckTimeSpan))
            {
                return true;
            }
            return false;
        }
        public void SetEQErrorState(string pstrEQID, EnumEQState peEQState, string pstrEQErrorCode, string pstrEQErrorMessage, string pstrEQErrorDT)
        {
            DateTime dtErrorDT = new DateTime(int.Parse(pstrEQErrorDT.Substring(0, 4)), int.Parse(pstrEQErrorDT.Substring(4, 2)), int.Parse(pstrEQErrorDT.Substring(6, 2)),
                                               int.Parse(pstrEQErrorDT.Substring(8, 2)), int.Parse(pstrEQErrorDT.Substring(10, 2)), int.Parse(pstrEQErrorDT.Substring(12, 2)), int.Parse(pstrEQErrorDT.Substring(14, 3)));
            if (strEQID != pstrEQID || dtErrorDateTime > dtErrorDT)
            {
                return;
            }
            if (eEQState != peEQState)
            {
                eEQState = peEQState;
                strEQErrorCode = pstrEQErrorCode;
                strEQErrorMessage = pstrEQErrorMessage;
                dtErrorDateTime = dtErrorDT;
                dtLastStateUpdateDateTime = DateTime.Now;
                if (eSMSSendState == EnumEQState.Normal && eEQState == EnumEQState.Error)
                    bSendErrorSMS = false;
                if (eSMSSendState == EnumEQState.Error && eEQState == EnumEQState.Normal)
                    bSendNormalSMS = false;

            }
        }
        public bool CheckRemoveCondition()
        {
            TimeSpan ErrorStateStoreTime = new TimeSpan(24, 0, 0);
            TimeSpan StateCheckTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.NET_EQErrorCheckTime);
            if (bSendErrorSMS == false && eEQState == EnumEQState.Normal)
            {
                return true;
            }
            if (bSendErrorSMS && bSendNormalSMS && (eSMSSendState == EnumEQState.Normal) && (DateTime.Now - LastStateUpdateDateTime > StateCheckTimeSpan))
            {
                return true;
            }
            if (DateTime.Now - LastStateUpdateDateTime > ErrorStateStoreTime)
            {
                return true;
            }
            return false;
        }
    }
}
