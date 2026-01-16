using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WERViewer
{
    public static class FileAsyncHelper
    {
        #region [Read]
        public static async Task<string[]> ReadAllLinesAsync(string filePath)
        {
            // Default to UTF8 if encoding is not specified
            var list = await ReadAllLinesAsync(filePath, Encoding.UTF8);
            return list.ToArray();
        }

        public static async Task<List<string>> ReadAllLinesAsync(string filePath, Encoding encoding)
        {
            var lines = new List<string>();

            // Use FileOptions.Asynchronous to ensure true asynchronous I/O at the OS level
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))  // useAsync: true is critical for .NET 4.5 async I/O
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

            return lines;
        }

        /// <summary>
        /// Generic implementation to return either a string[] or List<string>. Usage: 
        /// <code>
        ///   var lines = await ReadAllLinesAsync<string[]>("file.txt", Encoding.UTF8);
        /// </code>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<T> ReadAllLinesAsync<T>(string filePath, Encoding encoding) where T : class, IEnumerable<string>
        {
            var lines = new List<string>();

            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true)) // useAsync: true is critical for .NET 4.5 async I/O
            {
                using (var reader = new StreamReader(sourceStream, encoding))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }

            // Handle the return type conversion
            if (typeof(T) == typeof(string[]))
            {
                return lines.ToArray() as T;
            }
            else if (typeof(T) == typeof(List<string>))
            {
                return lines as T;
            }

            throw new NotSupportedException($"Type {typeof(T).Name} is not supported. Use string[] or List<string>.");
        }
        #endregion

        #region [Write]
        /// <summary>
        /// Overload using default UTF8 encoding.
        /// </summary>
        public static Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines)
        {
            return WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
        }

        /// <summary>
        /// Writes a collection of strings to a file asynchronously in .NET 4.5.
        /// </summary>
        public static async Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines, Encoding encoding)
        {
            // Creates new file or overwrites existing
            using (var sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true)) // useAsync: true is critical for .NET 4.5 async I/O
            {
                using (var writer = new StreamWriter(sourceStream, encoding))
                {
                    foreach (var line in lines)
                    {
                        // WriteLineAsync was introduced in .NET 4.5
                        await writer.WriteLineAsync(line);
                    }
                }
            }
        }
        #endregion
    }
}
