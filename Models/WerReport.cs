using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WERViewer
{
    public class WerReport
    {
        public string EventType { get; set; }
        public string EventTime { get; set; } // Win32 FILETIME or Active Directory Timestamp
        public string NsAppName { get; set; }
        public string AppName { get; set; }
        public string AppPath { get; set; }
        public string FriendlyEventName { get; set; }
        public string ReportId { get; set; }
        public string ApplicationName { get; set; }
        public string FaultModule { get; set; }
        public string ExceptionCode { get; set; }
        public DateTime ReportTime { get; set; }
        public string UploadTime { get; set; }
        public string ReportPath { get; set; }
        public string ReportType { get; set; }
        public string ReportDescription { get; set; }
        public HangType HangInfo { get; set; } = new HangType();
        public Dictionary<string, string> RawValues { get; set; } = new Dictionary<string, string>();
    }

    public class HangType
    {
        //"[Category] 0x{highByte:X2}: {GetCategoryDescription(highByte)}");
        //"[Reason]   0x{midHigh:X2}: {GetReasonDescription(midHigh)}");
        //"[State]    0x{midLow:X2}:  {GetStateDescription(midLow)}");
        //"[Flags]    0x{lowByte:X2}: Additional metadata");

        public string Category { get; set; } 
        public string Reason { get; set; }
        public string State { get; set; }
        public string Flags { get; set; }
    }
}
