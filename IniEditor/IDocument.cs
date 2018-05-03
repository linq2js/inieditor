using System;

namespace IniEditor
{
    public interface IDocument
    {
        bool Changed { get; set; }

        string FullPath { get; }

        string Contents { get; }
    }
}
