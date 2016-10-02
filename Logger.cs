using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

static class Logger
{
    public static void Log(LogType logType, string format, params object[] parameters)
    {
        if (logType == LogType.NeworkComms) return;
        using (StreamWriter sw = new StreamWriter(@"logs.txt", true))
            sw.WriteLine("["+DateTime.Now.ToLongTimeString()+"] "+logType+ ": " + format, parameters);
    }
}

class BanException : Exception
{

}