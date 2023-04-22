using System;
using System.Collections.ObjectModel;
using System.IO;

namespace VolumeShot.Models
{
    public static class Error
    {
        public static ObservableCollection<string> Log { get; set; } = new();
        public static void WriteLog(string path, string file, string text)
        {
            try
            {
                //if (!path.Contains("orders"))
                //{
                //    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                //        Log.Insert(0, $"{DateTime.UtcNow} {text}");
                //    }));
                //}
                App.Current.Dispatcher.BeginInvoke(new Action(() => {
                    Log.Insert(0, $"{DateTime.UtcNow} {text}");
                }));
                File.AppendAllText(path + file, $"{DateTime.UtcNow} {text}\n");
            }
            catch { }
        }
    }
}
