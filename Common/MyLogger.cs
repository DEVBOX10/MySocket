using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MyLogger
    {
        public string LogFolder { get; set; }
        public string LogName { get; set; }

        public void WriteLog(string text)
        {
            try
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(now);
                sb.AppendLine(text);

                string yyyy = now.Substring(0, 4);
                string mm = now.Substring(5, 2);

                string logDirectory = Path.Combine(this.LogFolder, yyyy + @"\" + mm);
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                // .log
                string logFileName = string.Format("{0}_{1}.log", this.LogName, DateTime.Now.ToString("yyyyMMdd"));
                string logFilePath = Path.Combine(logDirectory, logFileName);


                FileStream fs = null;
                if (File.Exists(logFilePath))
                    fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write);
                else
                    fs = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write);

                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine(sb.ToString());
                }

                fs.Dispose();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
