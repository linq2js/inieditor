using System;

namespace IniEditor
{
    [Flags]
    public enum SearchOptions
    {
        None = 0,
        MatchCase = 1,
        WholeWord = 2,
        WordStart = 4,
        Standard = 8,
        Extended = 16,
        Regex = 32,
        MarkLine = 64,
        Highlight = 128,
        Retry = 256
    }
}
