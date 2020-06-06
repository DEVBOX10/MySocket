using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Util
    {
        public static void WriteFile(string text, string folder)
        {
            try
            {
                var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt";
                var filePath = Path.Combine(folder, fileName);

                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine(text);
                }

                fs.Dispose();
            }
            catch (Exception ex)
            {
            }
        }


        public static string ReadFile(string filePath)
        {
            string text = string.Empty;
            try
            {
                FileStream fs = null;
                if (File.Exists(filePath))
                    fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                using (StreamReader reader = new StreamReader(fs))
                {
                    text = reader.ReadToEnd();
                }

                fs.Close();
            }
            catch (Exception ex)
            {
            }
            return text;
        }
    }
}
