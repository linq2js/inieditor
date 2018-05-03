using ScintillaNET;

namespace IniEditor
{
    public class Document : IDocument
    {
        public bool Changed { get; set; }

        public string FullPath { get; }

        public string Contents { get; set; }

        public Scintilla Editor { get; set; }

        public int Id { get; }

        public Deferred<Document> Ready { get; }

        public Document(int id, string fullPath)
        {
            Id = id;
            FullPath = fullPath;
            Ready = new Deferred<Document>();
        }
    }
}
