using System;
using System.IO;

namespace SecureMonitor
{
   public class AppendLog
    {

        /*
            Log Level
            ____________

            1 = Information
            2 = Warning
            3 = Error
            */

        public static string LogFileName = Variables.errorLog;

        public static void LogFile(int LogType, string logMessage)
        {

            string LogTypeTxt = "";

            if (LogType == 1)
                LogTypeTxt = "Information";
            if (LogType == 2)
                LogTypeTxt = "Warning";
            if (LogType == 3)
                LogTypeTxt = "Error";

            try
            {
                if (File.Exists(LogFileName))
                 using (StreamWriter w = File.AppendText(LogFileName))
                {
                    w.WriteLine("{0} :   {1} {2}  : {3}", LogTypeTxt, DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString(), logMessage);

                }
                else
                    using (StreamWriter w = File.CreateText(LogFileName))
                    {
                        w.WriteLine("{0} :   {1} {2}  : {3}", LogTypeTxt, DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString(), logMessage);

                    }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception with writing to log file..." + e.ToString());
            }
        }


        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }

        private void createLogFile()
        {
            if (!System.IO.File.Exists(Variables.errorLog))
                createFile(Variables.errorLog, DateTime.Now.ToString() + Environment.NewLine + "Error Log file Created\n" + Environment.NewLine);
        }

        public static bool createFile(string saveFile, string contentToWrite)
        {
            try
            {
                using (System.IO.FileStream fs = System.IO.File.Create(saveFile))
                {
                    for (byte i = 0; i < 100; i++)
                    {
                        fs.WriteByte(i);
                    }
                }

                System.IO.File.WriteAllText(saveFile, contentToWrite);
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }

        public static bool writeToFile(string saveFile, string contentToWrite)
        {
            try
            {
                using (StreamWriter w = File.AppendText(saveFile))
                {
                    w.WriteLine(contentToWrite);
                }
                return true;
            }
            catch (Exception exc)
            {
                System.Console.WriteLine(exc);
                return false;
            }
        }

    }
}
