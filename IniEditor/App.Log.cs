using System;
using System.Collections.Generic;
using System.Linq;
using Redux;

namespace IniEditor
{
    public partial class App
    {
        public void Log(params string[] logEntries)
        {
            Log(logEntries.Select(x => new LogEntry(x, null)).ToArray());
        }

        public void ClearLog()
        {
            Model.Logs.Add(new LogEntry("<clear>", null));
        }

        public void LogHeading(string text)
        {
            Log(text, LogEntry.Heading);
        }

        public void LogError(string text)
        {
            Log(text, LogEntry.Error);
        }

        private void Log(string text, int style)
        {
            Log(new LogEntry(text, (x, i) => x.Format(i, text.Length, style)));
        }


        public void Log(params LogEntry[] logEntries)
        {
            if (logEntries.Length <= 0) return;

            Model.Logs.AddRange(logEntries);
            Update();
        }

        public IEnumerable<LogEntry> PullLogs()
        {
            var result = Model.Logs.ToArray();
            if (result.Length > 0)
            {
                Timer.Timeout(0, (Action) Update);
            }
            Model.Logs.Clear();
            return result;
        }
    }
}
