using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IniEditor
{
    public class AppModel
    {
        public Project Project;

        public Document Document;

        public Dictionary<string, Document> Documents = new Dictionary<string, Document>(StringComparer.OrdinalIgnoreCase);

        public ConcurrentDictionary<string, string> Files = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<IDisposable> Disposables = new HashSet<IDisposable>();

        public HashSet<Action> Disposers = new HashSet<Action>();

        public ConcurrentBag<LogEntry> Logs = new ConcurrentBag<LogEntry>();

        public AnalyzingResult Analytics = new AnalyzingResult();

        public Guid FileVersion = Guid.NewGuid();
    }
}
