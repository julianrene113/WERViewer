using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WERViewer
{
    public static class FileAsyncHelper
    {
        public static async Task<string[]> ReadAllLinesAsync(string filePath)
        {
            // Default to UTF8 if encoding is not specified
            return await ReadAllLinesAsync(filePath, Encoding.UTF8);
        }

        public static async Task<string[]> ReadAllLinesAsync(string filePath, Encoding encoding)
        {
            var lines = new List<string>();

            // Use FileOptions.Asynchronous to ensure true asynchronous I/O at the OS level
            using (var sourceStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true)) // Critical for .NET 4.5 async I/O
            {
                using (var reader = new StreamReader(sourceStream, encoding))
                {
                    string line;
                    // ReadLineAsync was introduced in .NET 4.5
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }

            return lines.ToArray();
        }
    }
}
