using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ScintillaNET;

namespace IniEditor
{
    public partial class App
    {
        public IEnumerable<string> GetProjectDocuments(string projectFullPath)
        {
            if (string.IsNullOrWhiteSpace(projectFullPath))
            {
                yield break;
            }

            var projectDir = Path.GetDirectoryName(projectFullPath);
            var extensions = new[] { "*.inid", "*.ini" };

            foreach (var extension in extensions)
            {
                foreach (var documentFullPath in Directory.GetFiles(projectDir, extension, SearchOption.AllDirectories))
                {
                    yield return documentFullPath;
                }
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
                    if (project.FullPath.IgnoreCaseEqual(fullPath)) return project;
                }

                project = new Project(fullPath)
                {
                    Contents = ReadFile(fullPath),
                    Changed = false
                };

                x.Project = project;

                ReloadConfigs();

                Update();

                Log($"Project loaded {fullPath}");

                return project;
            });
        }

        public void ReloadConfigs()
        {
            Update(x =>
            {
                
                const string detailsPrefix = "Details.";
                const string groupingPrefix = "Grouping.";
                var lexer = new IniLexer();
                string sectionName = null;
                string propertyName = null;

                x.SectionDetails.Clear();

                lexer.Parse(x.Project.Contents, (token, position, type) =>
                {
                    if (type == IniLexer.StyleSection)
                    {
                        sectionName = token.Trim('[', ']');
                    }
                    else if (type == IniLexer.StyleKey)
                    {
                        propertyName = token.Trim();
                    }
                    else if(type == IniLexer.StyleValue)
                    {
                        token = token.Trim();
                        // read section style
                        if (sectionName.StartsWith(detailsPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            var blockName = sectionName.Substring(detailsPrefix.Length);
                            if (!x.SectionDetails.TryGetValue(blockName, out SectionStyle s))
                            {
                                x.SectionDetails[blockName] = s = new SectionStyle();
                            }

                            if (propertyName.IgnoreCaseEqual("backColor"))
                            {
                                s.BackColor = token.ToColor();
                            }
                            else if (propertyName.IgnoreCaseEqual("foreColor"))
                            {
                                s.ForeColor = token.ToColor();
                            }
                            else if (propertyName.IgnoreCaseEqual("borderColor"))
                            {
                                s.BorderColor = token.ToColor();
                            }
                        }
                        // read section grouping
                        else if (sectionName.StartsWith(groupingPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            var blockName = sectionName.Substring(groupingPrefix.Length);
                            if (!x.SectionGroups.TryGetValue(blockName, out SectionGroup s))
                            {
                                x.SectionGroups[blockName] = s = new SectionGroup();
                            }
                            // process
                            var parts = token.Split('*');
                            if (parts.Length > 2)
                            {
                                LogError($"Invalid grouping match pattern: {token}. Valid pattern must contains only ONE wildcard");
                                return;
                            }
                            s.Patterns.Add(new Regex("^" + string.Join(".+?", parts.Select(Regex.Escape)) + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase));
                        }
                    }
                });
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
            ClearLog();
            LogHeading("PROJECT FILES:");
            Log(files
                .Select(x =>
                {
                    var text = $"{Path.GetFileName(x)} ({Path.GetDirectoryName(x)})";
                    return new LogEntry(text, (e, i) =>
                    {
                        e.Format(i, text.Length, Style.CallTip);
                        e.AddData(i, i + text.Length, EditorExtraDataType.OpenDocument, x);
                    });
                })
                .OrderBy(x => x.Contents, StringComparer.OrdinalIgnoreCase)
                .ToArray());
        }

        public void EditProject()
        {
            if (Model.Project == null) return;

            Update(x =>
            {
                var projectPath = x.Project.FullPath;

                if (!x.Documents.ContainsKey(projectPath))
                {
                    x.Document = x.Documents[projectPath] = new Document(FileId(projectPath), projectPath);
                    x.Document.Contents = x.Project.Contents;
                }
            });
        }
    }
}
