using System;
using System.IO;

namespace VolumeShot.Models
{
    public static class Error
    {
        private static string directory = $"{Directory.GetCurrentDirectory()}/log/";
        public static void WriteLog(string path, string file, string text)
        {
            try
            {
                if (!Directory.Exists(directory + path)) Directory.CreateDirectory(directory + path);
                if (path != "") path += "/";
                File.AppendAllText(directory + path + file, $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
