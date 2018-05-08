using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public List<LogEntry> Logs = new List<LogEntry>();

        public Guid FileVersion = Guid.NewGuid();

        public InlineEditData InlineEdit;

        public Task<AData> Analyzed = Task.FromResult(new AData());

        public IDictionary<string, SectionStyle> SectionDetails = new Dictionary<string, SectionStyle>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, SectionGroup> SectionGroups = new Dictionary<string, SectionGroup>(StringComparer.OrdinalIgnoreCase);
    }
}
