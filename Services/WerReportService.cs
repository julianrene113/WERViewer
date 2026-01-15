using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WERViewer
{
    public class WerReportService
    {
        // Standard location for WER reports on Windows
        readonly string _root = @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive";

        public async Task<List<WerReport>> LoadReportsAsync()
        {
            var reports = new List<WerReport>();

            if (!Directory.Exists(_root))
                return reports;

            foreach (var dir in Directory.GetDirectories(_root))
            {
                var werFile = Path.Combine(dir, "Report.wer");
                if (!File.Exists(werFile))
                    continue;

                var parsed = await ParseWerFileAsync(werFile);
                if (parsed != null)
                    reports.Add(parsed);
            }

            return reports.OrderByDescending(r => r.EventTime).ToList();
        }

        async Task<WerReport> ParseWerFileAsync(string file)
        {
            var lines = await FileAsyncHelper.ReadAllLinesAsync(file);
            var report = new WerReport { ReportPath = file };
            char[] delimiter = new char[] { '=' };

            foreach (var line in lines)
            {
                if (!line.Contains("=")) continue;

                var parts = line.Split(delimiter, 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                report.RawValues[key] = value;

                switch (key)
                {
                    case "EventType":
                        report.EventType = value;
                        break;
                    case "EventTime":
                        if (long.TryParse(value, out long fileTime))
                        {
                            try
                            {
                                DateTime utcDate = DateTime.FromFileTimeUtc(fileTime); // Converts to UTC time
                                DateTime localDate = DateTime.FromFileTime(fileTime); // Converts to the local system time
                                report.EventTime = $"{localDate}";
                            }
                            catch { report.EventTime = value; }
                        }
                        break;
                    case "NsAppName":
                        report.NsAppName = value;
                        break;
                    case "AppName":
                        report.AppName = value;
                        break;
                    case "AppPath":
                        report.AppPath = value;
                        break;
                    case "FriendlyEventName":
                        report.FriendlyEventName = value;
                        break;
                    case "ReportIdentifier":
                        report.ReportId = value;
                        break;
                    case "ApplicationName":
                        report.ApplicationName = value;
                        break;
                    case "FaultingModuleName":
                        report.FaultModule = value;
                        break;
                    case "ExceptionCode":
                        report.ExceptionCode = value;
                        break;
                    case "ReportTimestamp":
                        if (long.TryParse(value, out long ts))
                        {
                            try
                            {
                                DateTime utcDate = DateTime.FromFileTimeUtc(ts); // Converts to UTC time
                                report.ReportTime = DateTime.FromFileTime(ts); // Converts to the local system time
                            }
                            catch { report.ReportTime = DateTimeOffset.FromUnixTimeSeconds(ts).DateTime; }
                        }
                        break;
                }
            }

            return report;
        }
    }

}
