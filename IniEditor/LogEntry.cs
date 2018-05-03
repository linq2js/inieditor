using System;
using ScintillaNET;

namespace IniEditor
{
    public class LogEntry
    {
        public const int Heading = 50;

        public LogEntry(string contents, Action<Scintilla, int> formatter)
        {
            Contents = contents;
            Formatter = formatter;
        }

        public string Contents { get; }

        public Action<Scintilla, int> Formatter { get; }
    }
}
