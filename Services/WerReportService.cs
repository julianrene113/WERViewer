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
        // Standard location for WER reports on a Windows machine
        // @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive";

        public async Task<List<WerReport>> LoadReportsAsync(string rootPath)
        {
            var reports = new List<WerReport>();
            
            if (string.IsNullOrWhiteSpace(rootPath))
                rootPath = @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive";

            if (!Directory.Exists(rootPath))
                return reports;

            foreach (var dir in Directory.GetDirectories(rootPath))
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

            int index = 0;
            foreach (var line in lines)
            {
                if (!line.Contains("=")) continue;

                var parts = line.Split(delimiter, 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                report.RawValues[key] = value;

                #region [Additional Hang Type Evaluation]
                if (value.StartsWith("Hang Type"))
                { 
                    // Get the value to convert
                    var tmp = lines[index + 1].Split(delimiter, 2);
                    var hkey = tmp[0].Trim();
                    var hvalue = tmp[1].Trim();
                    if (long.TryParse(hvalue, out long ht))
                    {
                        report.HangInfo = WerHangAnalyzer.AnalyzeHangType(ht);
                    }
                   
                }
                #endregion

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
                    case "ReportDescription":
                        report.ReportDescription = value;
                        break;
                    case "UploadTime":
                        if (long.TryParse(value, out long upload))
                        {
                            try
                            {
                                DateTime utcDate = DateTime.FromFileTimeUtc(upload); // Converts to UTC time
                                DateTime localDate = DateTime.FromFileTime(upload); // Converts to the local system time
                                report.UploadTime = $"{localDate}";
                            }
                            catch { report.UploadTime = value; }
                        }
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
                    case "ReportType":
                        if (int.TryParse(value, out int rt))
                        {
                            switch (rt)
                            {
                                case 0:
                                    report.ReportType = "WerReportNonCritical";
                                    break;
                                case 1:
                                    report.ReportType = "WerReportCritical";
                                    break;
                                case 2:
                                    report.ReportType = "WerReportApplicationCrash";
                                    break;
                                case 3:
                                    report.ReportType = "WerReportApplicationHang";
                                    break;
                                case 4:
                                    report.ReportType = "WerReportKernel";
                                    break;
                                default:
                                    report.ReportType = $"{rt}";
                                    break;

                            }
                        }
                        else
                        {
                            report.ReportType = $"{value}";
                        }
                        break;
                }

                index++;
            }

            return report;
        }
    }

    public class WerHangAnalyzer
    {
        public static HangType AnalyzeHangType(long hangTypeDecimal, bool dump = false)
        {
            try
            {
                // If you see 0x19, your next step should be using Analyze Wait Chain in Windows Resource Monitor.
                // It will tell you which specific process ID (PID) is holding the resource that Byte 19 is waiting for.
                // Convert to 32-bit hex (e.g., 0x04021900)
                uint hangType = (uint)hangTypeDecimal;
                string hex = hangType.ToString("X8");

                // Extract bytes (Big Endian order)
                byte highByte = (byte)((hangType >> 24) & 0xFF); // 0x04
                byte midHigh = (byte)((hangType >> 16) & 0xFF);  // 0x02
                byte midLow = (byte)((hangType >> 8) & 0xFF);    // 0x19
                byte lowByte = (byte)(hangType & 0xFF);          // 0x00

                if (dump)
                {
                    Debug.WriteLine($"Decimal Value: {hangTypeDecimal}");
                    Debug.WriteLine($"Full Hex:      0x{hex}\n");
                    Debug.WriteLine("--- Categorization ---");
                    Debug.WriteLine($"[Category] 0x{highByte:X2}: {GetCategoryDescription(highByte)}");
                    Debug.WriteLine($"[Reason]   0x{midHigh:X2}: {GetReasonDescription(midHigh)}");
                    Debug.WriteLine($"[State]    0x{midLow:X2}:  {GetStateDescription(midLow)}");
                    Debug.WriteLine($"[Flags]    0x{lowByte:X2}: Additional metadata");
                }
                return new HangType
                {
                    Category = $"0x{highByte:X2}: {GetCategoryDescription(highByte)}",
                    Reason = $"0x{midHigh:X2}: {GetReasonDescription(midHigh)}",
                    State = $"0x{midLow:X2}: {GetStateDescription(midLow)}",
                    Flags = $"0x{lowByte:X2}: Additional metadata"
                };
            }
            catch (Exception ex)
            {
                Extensions.WriteToLog($"AnalyzeHangType: {ex.Message}", LogLevel.ERROR);
                return new HangType();
            }
        }

        static string GetCategoryDescription(byte b) => b == 0x04 ? "UI Thread Hang (Top-Level Window)" : "Other/System Hang";
        static string GetReasonDescription(byte b) => b == 0x02 ? "No Message Pump (Message loop is blocked/not calling GetMessage)" : "Unknown Reason";
        static string GetStateDescription(byte b) => b == 0x19 ? "Synchronous Block (Waiting on DesktopWindowManager or Cross-Process Call)" : b == 0x01 ? "Generic Wait State" : "Specific Wait State";
    }


}
