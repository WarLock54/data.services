using ServiceStack;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public static class LogUtil
    {
        public static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static readonly NLog.Logger _odataLogger = NLog.LogManager.GetLogger("Data");
        public static readonly NLog.Logger _guiLogger = NLog.LogManager.GetLogger("GUI");

        public static void LogGuiEvents(string logLevel, string formName, string action, string key, string Message)
        {
            var logMessage = formName + "-" + action + "(" + key + ") " + Message;
            // Trace, Debug, Info, Warn, Error, Fatal
            var level = logLevel.ToLower2();
            if (level == "fatal") _guiLogger.Fatal(logMessage);
            else if (level == "error") _guiLogger.Error(logMessage);
            else if (level == "warn") _guiLogger.Warn(logMessage);
            else if (level == "info") _guiLogger.Info(logMessage);
            else if (level == "debug") _guiLogger.Debug(logMessage);
            else _guiLogger.Trace(logMessage);
        }
        public static string? ToLower2(this string item, CultureInfo? cul = null)
        {
            try
            {
                if (item.IsNullOrEmpty())
                    return "";
                return item.ToLower(cul).Replace("ı", "i");
                //cul == null ?
                //item.ToLower().Replace("ı", "i") :
                //item.ToLower(cul).Replace("ı", "i");
            }
            catch { return (item ?? "").ToLower(); }
        }
    }
}
