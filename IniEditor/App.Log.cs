using System.Collections.Generic;
using System.Linq;

namespace IniEditor
{
    public partial class App
    {
        public void Log(params string[] logEntries)
        {
            Log(logEntries.Select(x => new LogEntry(x, null)).ToArray());
        }

        public void LogHeading(string text)
        {
            Log(new LogEntry(text, (x, i) => x.Format(i, text.Length, LogEntry.Heading)));
        }


        public void Log(params LogEntry[] logEntries)
        {
            if (logEntries.Length <= 0) return;

            foreach (var logEntry in logEntries)
            {
                Model.Logs.Add(logEntry);
            }
            Update();
        }

        public IEnumerable<LogEntry> PullLogs()
        {
            var changed = false;
            while (Model.Logs.Count > 0)
            {
                if (Model.Logs.TryTake(out LogEntry log))
                {
                    changed = true;
                    yield return log;
                }
                else
                {
                    break;
                }
            }

            if (changed)
            {
                Update();
            }
        }
    }
}
