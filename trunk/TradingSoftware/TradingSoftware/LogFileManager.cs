using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TradingSoftware
{
    /// <summary>
    /// This is a static class used to Create a LogFile, write entries into it and dump it´s whole content from whatever class it is needed.
    /// </summary>
    /// <remarks></remarks>
    internal static class LogFileManager
    {
        /// <summary>
        /// The name/path of/to the logfile. Here a datatype shall also be added by the user.
        /// </summary>
        private static String LogName;

        /// <summary>
        /// Creates the log file it it not already exists with the same name. If it already exists, the future entries will be appended to the current log file.
        /// </summary>
        /// <param name="logName">Name of the log file.</param>
        /// <remarks></remarks>
        public static void CreateLog(String logName)
        {
            LogName = logName;
            if (!File.Exists(LogName))
            {
                var fs = File.Create(LogName);
                //This laso closes the underlaying file
                fs.Close();
            }
        }

        /// <summary>
        /// Writes a new entry to the log file. The message that should be written to the log file can be handed over as a parameter at the call of the method.
        /// </summary>
        /// <param name="logMessage">The message that shall be written to the log.</param>
        /// <remarks></remarks>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteToLog(String logMessage)
        {
            using (StreamWriter w = File.AppendText(LogName))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  :");
                w.WriteLine("  :{0}", logMessage);
                w.WriteLine("-------------------------------");
                // Update the underlying file.
                w.Flush();
                // Close the writer and underlying file.
                w.Close();
            }
        }

        /// <summary>
        /// Dumps the hole content of the log file directly into the console.
        /// </summary>
        /// <remarks></remarks>
        private static void DumpLog()
        {
            // Open and read the file.
            using (StreamReader r = File.OpenText(LogName))
            {
                // While not at the end of the file, read and write lines.
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
                r.Close();
            }
        }
    }
}