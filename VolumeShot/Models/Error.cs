using System;
using System.IO;

namespace VolumeShot.Models
{
    public static class Error
    {
        public static void WriteLog(string path, string file, string text)
        {
            try
            {
                File.AppendAllText(path + file, $"{DateTime.UtcNow} {text}\n");
            }
            catch { }
        }
    }
}
