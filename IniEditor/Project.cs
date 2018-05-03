using System;
using System.Xml.Linq;

namespace IniEditor
{
    public class Project : IDocument
    {
        public string FullPath { get; }

        public string Contents
        {
            get
            {
                var root = new XElement("project");

                return root.ToString();
            }

            set
            {
                Changed = true;
            }
        }

        public Project(string fullPath)
        {
            FullPath = fullPath;
        }

        public bool Changed { get; set; }
    }
}
