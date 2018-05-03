using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScintillaNET;

namespace IniEditor
{
    public partial class App
    {
        public IEnumerable<string> GetProjectDocuments(string projectFullPath)
        {
            var projectDir = Path.GetDirectoryName(projectFullPath);

            foreach (var documentFullPath in Directory.GetFiles(projectDir, "*.ini", SearchOption.AllDirectories))
            {
                yield return documentFullPath;
            }
        }

        public Project LoadProject(string fullPath)
        {
            return Update(x =>
            {
                var project = x.Project;

                if (project != null)
                {
                    // dont load twice
                    if (string.Compare(project.FullPath, fullPath, StringComparison.OrdinalIgnoreCase) == 0) return project;
                }

                project = new Project(fullPath)
                {
                    Contents = ReadFile(fullPath),
                    Changed = false
                };

                x.Project = project;

                Update();

                Log($"Project loaded {fullPath}");

                return project;
            });
        }

        public void CreateProject(string fullPath)
        {
            Update(x =>
            {
                var project = new Project(fullPath);
                project.Changed = true;
                x.Project = project;
            });
        }

        public void ShowProjectFiles()
        {
            if (Model.Project == null) return;
            var files = GetProjectDocuments(Model.Project.FullPath);
            LogHeading("PROJECT FILES:");
            Log(files.Select(x =>
            {
                var text = $"{Path.GetFileName(x)} ({Path.GetDirectoryName(x)})";
                return new LogEntry(text, (e, i) =>
                {
                    e.Format(i, text.Length, Style.CallTip);
                    e.AddData(i, i + text.Length, EditorExtraDataType.OpenDocument, x);
                });
            }).ToArray());
        }
    }
}
