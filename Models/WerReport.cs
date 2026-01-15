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
        public string ReportPath { get; set; }
        public Dictionary<string, string> RawValues { get; set; } = new Dictionary<string, string>();
    }

}
