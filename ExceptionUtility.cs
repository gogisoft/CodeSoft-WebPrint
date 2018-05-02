using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CodeSoftPrinterApp
{
    public class ExceptionUtility
    {
        const string ApplName = "CodeSoft Print Application";

        // Log an Exception
        public static void LogException(Exception exc)
        {
            log4net.LogManager.GetLogger("ERROR").Error(exc);
        }
        public static void LogDebug(Exception exc)
        {
            log4net.LogManager.GetLogger("DEBUG").Debug(exc);
        }

        public static void WriteTraceToLog(String msg)
        {

            try
            {
                String logFile = String.Format("Logs/Trace_{0}.log", DateTime.Now.ToString("yyyyMMdd"));
                logFile = HttpContext.Current.Server.MapPath(logFile);
                if (!System.IO.File.Exists(logFile))
                {
                    System.IO.File.Create(logFile);
                }
                // Open the log file for append and write the log
                using (var sw = new StreamWriter(logFile, true))
                {
                    sw.WriteLine(DateTime.Now + " " + msg);
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}