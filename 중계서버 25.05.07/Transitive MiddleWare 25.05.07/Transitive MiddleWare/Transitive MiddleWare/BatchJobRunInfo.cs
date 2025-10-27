
namespace BPNS.TransitiveMiddleware
{
    using System;

    public class BatchJobRunInfo
    {
        private string strProcessID;

        public string ProcessID
        {
            get { return strProcessID; }
            set { strProcessID = value; }
        }

        private string strPackageName;

        public string PackageName
        {
            get { return strPackageName; }
            set { strPackageName = value; }
        }

        private string strProcedureName;

        public string ProcedureName
        {
            get { return strProcedureName; }
            set { strProcedureName = value; }
        }

        private string strStartTime;
        public string StartTime
        {
            get { return strStartTime; }
            set { strStartTime = value; }
        }

        private int nJobInterval;
        public int JobInterval
        {
            get { return nJobInterval; }
            set { nJobInterval = value; }
        }

        DateTime dtLastExecuteTime;
        public DateTime LastExecuteTime
        {
            get { return dtLastExecuteTime; }
            set { dtLastExecuteTime = value; }
        }

        private bool bProcessExists;
        public bool ProcessExists
        {
            get { return bProcessExists; }
            set { bProcessExists = value; }
        }

        public BatchJobRunInfo()
        {
            strProcessID = string.Empty;
            strStartTime = string.Empty;
            nJobInterval = 0;
            dtLastExecuteTime = new DateTime();
        }

        public BatchJobRunInfo(string pstrProcessID, string pstrStartTime, int pnJobInterval, DateTime pdtLastExecuteTime, bool pbProcessExists)
        {
            strProcessID = pstrProcessID;
            if (pstrProcessID.IndexOf('.') == -1)
            {
                strPackageName = string.Empty;
                strProcedureName = pstrProcessID;
            }
            else
            {
                strPackageName = pstrProcessID.Substring(0, pstrProcessID.IndexOf('.'));
                strProcedureName = pstrProcessID.Substring(pstrProcessID.IndexOf('.') + 1);
            }
            strStartTime = pstrStartTime;
            nJobInterval = pnJobInterval;
            dtLastExecuteTime = pdtLastExecuteTime;
            bProcessExists = pbProcessExists;
        }

        public bool SameJob(string pstrProcessID, string pstrStartTime)
        {
            return strProcessID.Equals(pstrProcessID) && strStartTime.Equals(pstrStartTime);
        }
    }
}
