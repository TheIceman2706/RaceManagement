using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class Log
    {
        private static Log _instance;
        private IList<string> entries;
        private StreamWriter outStream;

        public static Log Instance { get{ if (_instance == null) _instance = new Log(); return _instance; }}

        public Stream OutStream { get => outStream.BaseStream; set => outStream = new StreamWriter(value); }

        public EventHandler<string> EntryAppended;

        public Log()
        {
            entries = new List<string>();
        }

        public void Append(string entry)
        {

            entries.Add(entry);
            if (outStream != null)
            {
                outStream.WriteLine(entry);
            }
            if(EntryAppended != null)
                EntryAppended(this, entry);
        }

        public void SafeTo(string path)
        {
            path = System.IO.Path.GetFullPath(path);
            System.IO.File.WriteAllText(path, this.ToString());
        }

        public override string ToString()
        {
            string srt = "";
            entries.All((str) => { srt += "\n" + str; return true; });
            return srt;
        }

        public void AppendWithTimestamp(string entry)
        {
            Append("[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + entry);
        }

        public void Info(string msg)
        {
            AppendWithTimestamp("[INFO] " + msg);
        }
        public void Warn(string msg)
        {
            AppendWithTimestamp("[WARNING] " + msg);
        }
        public void Error(string msg)
        {
            AppendWithTimestamp("[ERROR] " + msg);
        }
        public void Fail(string msg)
        {
            AppendWithTimestamp("[FAILURE] " + msg);
        }

    }
}
