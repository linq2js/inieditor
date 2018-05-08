using System;
using System.Xml.Linq;

namespace IniEditor
{
    public class Project : IDocument
    {
        public string FullPath { get; }

        public string Contents { get; set; }

        public Project(string fullPath)
        {
            FullPath = fullPath;
        }

        public bool Changed { get; set; }
    }
}
