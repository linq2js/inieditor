using System.Collections.Generic;
using Redux;

namespace IniEditor
{
    public partial class App
    {
        public void AutoSave()
        {
            Timer.Interval(200, () =>
            {
                var l = new List<IDocument>();

                if (Model.Document != null)
                {
                    l.Add(Model.Document);
                }

                l.AddRange(Model.Documents.Values);

                foreach (var document in l)
                {
                    if (!document.Changed) continue;
                    WriteFile(document.FullPath, document.Contents);

                    if (document.Changed)
                    {
                        document.Changed = false;
                    }
                }

                Update();
            });
        }
    }
}
