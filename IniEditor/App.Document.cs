using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace IniEditor
{
    public partial class App
    {
        public Document LoadDocument(string fullPath)
        {
            return Update(x =>
            {
                // dont load twice
                if (!x.Documents.TryGetValue(fullPath, out Document document))
                {
                    document = new Document(FileId(fullPath), fullPath);
                    var documents = x.Documents.Clone();
                    documents[fullPath] = document;
                    document.Contents = ReadFile(fullPath);
                    x.Documents = documents;

                    Log($"Document loaded {fullPath}");
                }

                // set active document
                x.Document = document;

                return document;
            });
        }

        public void EditorAdded(Document document, Scintilla editor)
        {
            Update(x =>
            {
                document.Editor = editor;
                document.Ready.Resolve(document);
            });
        }

        public void ActivateDocument(Document document)
        {
            Update(x =>
            {
                x.Document = document;
            });
        }

        public void DocumentChanged(Document document, string contents)
        {
            Update(x =>
            {
                document.Contents = contents;
                document.Changed = true;
            });
        }

        public void CreateDocument(string fullPath)
        {

            Update(x =>
            {
                var document = new Document(FileId(fullPath), fullPath);
                document.Contents = string.Join("\r\n",
                    "; CreatedDate: " + DateTime.Now.ToString("yyyy-MM-dd"),
                    "; Author: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name
                    );
                document.Changed = true;
                x.Documents[fullPath] = document;
                x.Document = document;
            });
        }


        public void CloseDocument(Document document)
        {
            Update(x =>
            {
                x.Documents.Remove(document.FullPath);

                x.Documents = x.Documents.Clone();

                // activate another document
                if (x.Document == document)
                {
                    x.Document = x.Documents.Count > 0 ? x.Documents.Values.First() : null;
                }
            });
        }

        public string FindSection(int position)
        {
            if (Model.Document == null) return null;
            var fileId = FileId(Model.Document.FullPath);
            if (Model.Analytics.Files.TryGetValue(fileId, out AnalyzingResult.File file))
            {
                return (
                    from location in file.Locations
                    where position >= location.Value.Position && position <= location.Value.EndPosition
                    select location.Key).FirstOrDefault();
            }

            return null;
        }

        public void GoToDeclaration(string word, int position, Action<IDictionary<string, AnalyzingResult.Location>> handleMultipleLocations)
        {
            var analytics = Model.Analytics;


            if (analytics.References.TryGetValue("s:" + word, out AnalyzingResult.Reference reference))
            {
                var data = new Dictionary<string, AnalyzingResult.Location>(StringComparer.OrdinalIgnoreCase);
                foreach (var location in reference.Locations)
                {
                    if (position >= location.Position && position <= location.Position + location.TextLength)
                    {
                        // do not inlcude this location, current caret is here
                        continue;
                    }

                    if (analytics.Files.TryGetValue(location.FileId, out AnalyzingResult.File file))
                    {
                        data[$"Jump To [{word}] ({Path.GetFileName(file.FullPath)} -> {Path.GetDirectoryName(file.FullPath)})"] = location;
                    }
                }

                if (data.Count == 1)
                {
                    GoTo(data.Values.First());
                }
                else
                {
                    handleMultipleLocations(data);
                }
            }
            else
            {
                MessageBox.Show($"No [{word}] found");
            }
        }

        public void GoTo(AnalyzingResult.Location location)
        {
            var document = Model.Document;
            if (document != null && document.Id == location.FileId)
            {
                document.Editor.GotoPosition(location.Position);
            }
            else // document not active or loaded
            {
                var fullPath = GetFileFullPath(location.FileId);

                LoadDocument(fullPath).Ready.Then(d =>
                {
                    d.Editor.Focus();
                    d.Editor.GotoPosition(location.Position);
                    return d;
                });
            }
        }

        public void UpdateUi()
        {
            Update();
        }

        public void DocumentReady(Action<Document> action)
        {
            Model.Document?.Ready.Then(d =>
            {
                action(d);
                return d;
            });
        }

        private static string TransformExtended(string data)
        {
            var result = data;
            const char nullChar = (char)0;
            const char cr = (char)13;
            const char lf = (char)10;
            const char tab = (char)9;

            result = result.Replace("\\r\\n", Environment.NewLine);
            result = result.Replace("\\r", cr.ToString());
            result = result.Replace("\\n", lf.ToString());
            result = result.Replace("\\t", tab.ToString());
            result = result.Replace("\\0", nullChar.ToString());

            return result;
        }

        private static Regex BuildExpression(string term, bool searchPrevious, SearchOptions searchOptions)
        {
            var regexOptions = RegexOptions.Compiled | RegexOptions.Singleline;

            if (!searchOptions.HasFlag(SearchOptions.MatchCase))
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            if (!searchOptions.HasFlag(SearchOptions.Regex))
            {
                if (searchOptions.HasFlag(SearchOptions.Extended))
                {
                    term = TransformExtended(term);
                }

                term = Regex.Escape(term);
            }

            if (searchOptions.HasFlag(SearchOptions.WholeWord))
            {
                term = "\\b" + term + "\\b";
            }
            else if (searchOptions.HasFlag(SearchOptions.WordStart))
            {
                term = "\\b" + term;
            }

            term = searchPrevious ? $".*({term})" : $"({term})";

            return new Regex(term, regexOptions);
        }

        public IEnumerable<string> GetAllDocumentPaths(bool entireProject = true)
        {
            var filePaths = new HashSet<string>();

            // analyze all project files and current document file

            foreach (var document in Model.Documents.Values)
            {
                filePaths.Add(document.FullPath);
            }

            if (Model.Project != null)
            {
                foreach (var documentFullPath in GetProjectDocuments(Model.Project.FullPath))
                {
                    filePaths.Add(documentFullPath);
                }
            }

            return filePaths;
        }

        public void Find(string term, bool findPrevious, SearchOptions searchOptions)
        {
            if (Model.Document == null) return;
            var document = Model.Document;
            var nothingSelected = document.Editor.SelectionStart == document.Editor.SelectionEnd;
            var offset = findPrevious
                ? 0 // find from begin of file
                : nothingSelected // nothing selected
                    ? document.Editor.CurrentPosition // start from current position
                    : Math.Max(0, Math.Max(document.Editor.SelectionStart, document.Editor.SelectionEnd) + 1);
            var length = findPrevious
                ? nothingSelected
                    ? document.Editor.CurrentPosition
                    : Math.Min(document.Editor.SelectionStart, document.Editor.SelectionEnd) - 1
                : document.Editor.TextLength;

            if (searchOptions.HasFlag(SearchOptions.Retry))
            {
                offset = 0;
                length = document.Editor.TextLength;
            }

            FindAll(document.FullPath, offset, length, term, findPrevious, searchOptions, result =>
            {
                var firstResult = result.FirstOrDefault();
                if (firstResult == null)
                {
                    if (offset > 0 && !findPrevious && !searchOptions.HasFlag(SearchOptions.Retry))
                    {
                        Find(term, false, searchOptions | SearchOptions.Retry);
                    }
                    else
                    {
                        MessageBox.Show("Not found");
                    }
                }
                else
                {
                    document.Editor.SetSel(firstResult.Position, firstResult.EndPosition);
                }
            });
        }

        public void FindAll(string term, FindReplaceAllMode mode, SearchOptions searchOptions)
        {
            if (mode == FindReplaceAllMode.CurrentFile && Model.Document == null) return;

            var documentPaths = new Queue<string>(mode == FindReplaceAllMode.CurrentFile ? new[] {Model.Document.FullPath} : GetAllDocumentPaths(mode == FindReplaceAllMode.AllFiles));
            var result = new List<AnalyzingResult.Location>();

            void Find(IEnumerable<AnalyzingResult.Location> locations)
            {
                result.AddRange(locations);

                if (documentPaths.Count == 0)
                {
                    DisplayFindResult(term, result);
                }
                else
                {
                    var fullPath = documentPaths.Dequeue();
                    FindAll(fullPath, 0, -1, term, false, searchOptions, Find);
                }
            }

            Find(Enumerable.Empty<AnalyzingResult.Location>());
        }

        private void DisplayFindResult(string term, IEnumerable<AnalyzingResult.Location> result)
        {
            LogHeading($"FIND RESULT FOR \"{term}\":");

            var resultArray = result.ToArray();
            // dont show file path if the result is in single file
            var singleFile = resultArray.Select(x => x.FileId).Distinct().Count() == 1;


            var logEntries = resultArray.GroupBy(x => x.FileId)
                .SelectMany(g =>
                {
                    var filePath = FilePath(g.Key);

                    return g.OrderBy(x => x.Position)
                        .Select(x =>
                        {
                            var text = $"{x.PreviewText} [{x.Line}]";
                            if (!singleFile)
                            {
                                text += $" => {Path.GetFileName(filePath)} ({Path.GetDirectoryName(filePath)})";
                            }

                            return new LogEntry(text, (e, i) =>
                            {
                                e.Format(i, text.Length, Style.CallTip);
                                e.AddData(i, i + text.Length, EditorExtraDataType.OpenDocument, x);
                            });
                        });
                }).ToArray();
            if (logEntries.Length == 0)
            {
                Log("Not found");
            }
            else
            {
                Log(logEntries);
            }
        }

        public void FindAll(string fullPath, int offset, int length, string term, bool previous, SearchOptions searchOptions, Action<IEnumerable<AnalyzingResult.Location>> action)
        {
            var fileId = FileId(fullPath);

            void Ready(Document document)
            {
                if (offset == -1)
                {
                    offset = document.Editor.CurrentPosition;
                }

                if (length == -1)
                {
                    length = document.Editor.TextLength;
                }

                IEnumerable<Match> CollectMatches()
                {

                    var regex = BuildExpression(term, previous, searchOptions);
                    var match = regex.Match(document.Editor.GetTextRange(offset, length));

                    if (previous)
                    {
                        while (match.Success)
                        {
                            yield return match;

                            var lastPosition = Math.Min(document.Editor.SelectionEnd, document.Editor.SelectionStart);

                            match = regex.Match(document.Editor.GetTextRange(offset, lastPosition - offset));
                        }
                    }
                    else
                    {
                        while (match.Success)
                        {
                            yield return match;
                            match = match.NextMatch();
                        }
                    } 
                }


                action(
                    CollectMatches()
                        .Select(x =>
                        {
                            var group = x.Groups[1];
                            var previewExpand = 10;
                            var position = offset + group.Index;
                            var line = document.Editor.Lines[document.Editor.LineFromPosition(position)];
                            var column = position - line.Position;
                            var startOfPreview = Math.Max(0, column - previewExpand);
                            var endOfPreview = Math.Min(line.Length - 1, column + group.Length + previewExpand);
                            var previewText = line.Text.Substring(startOfPreview, endOfPreview - startOfPreview).Trim('\r', '\n');

                            if (startOfPreview > 0)
                            {
                                previewText = "..." + previewText;
                            }

                            if (endOfPreview < line.Length - 1)
                            {
                                previewText += "...";
                            }

                            if (searchOptions.HasFlag(SearchOptions.Highlight))
                            {
                                document.Editor.IndicatorFillRange(position, group.Length);
                            }

                            return new AnalyzingResult.Location(fileId, position)
                            {
                                TextLength = group.Length,
                                EndPosition = position + group.Length,
                                Line = line.Index,
                                Column = column,
                                // extract preview text and remove all new line chars
                                PreviewText = previewText
                            };
                        }));
            }

            // load document from cache
            if (Model.Documents.TryGetValue(fullPath, out Document d))
            {
                Ready(d);
            }
            else
            {
                // load from file
                LoadDocument(fullPath).Ready.Then(x => Ready(x));
            }
        }

        public void ClearHighlights()
        {
            
        }

        public void Replace(string term, string replace, bool replacePrevious, SearchOptions searchOptions)
        {
            if (Model.Document == null) return;
            var document = Model.Document;
            var offset = replacePrevious ? 0 : Math.Max(0, Math.Max(document.Editor.SelectionStart, document.Editor.SelectionEnd) - 1);
            var length = replacePrevious ? Math.Min(document.Editor.SelectionStart, document.Editor.SelectionEnd) : document.Editor.TextLength;

            FindAll(document.FullPath, offset, length, term, replacePrevious, searchOptions, result =>
            {
                var firstResult = result.FirstOrDefault();

                if (firstResult == null)
                {
                    MessageBox.Show("Not found");
                }
                else
                {
                    document.Editor.SetSel(firstResult.Position, firstResult.EndPosition);
                    document.Editor.ReplaceSelection(replace);
                }
            });
        }

        public void ReplaceAll(string term, string replace, FindReplaceAllMode mode, SearchOptions searchOptions)
        {
            if (MessageBox.Show("Do you want to continue with Replace All ?", "Replace All", MessageBoxButtons.YesNo) == DialogResult.No) return;

            if (mode == FindReplaceAllMode.CurrentFile && Model.Document == null) return;

            var documentPaths = new Queue<string>(mode == FindReplaceAllMode.CurrentFile ? new[] { Model.Document.FullPath } : GetAllDocumentPaths(mode == FindReplaceAllMode.AllFiles));
            var result = new List<AnalyzingResult.Location>();

            void Replace(IEnumerable<AnalyzingResult.Location> locations)
            {
                result.AddRange(locations);

                if (documentPaths.Count == 0)
                {
                    // must re-order result set before replacing
                    var orderedResult = result.OrderBy(x => x.FileId).ThenByDescending(x => x.Position);
                    // replace all items from bottom to top
                    foreach (var location in orderedResult)
                    {
                        var filePath = FilePath(location.FileId);
                        if (Model.Documents.TryGetValue(filePath, out Document document))
                        {
                            document.Editor.SetSel(location.Position, location.EndPosition);
                            document.Editor.ReplaceSelection(replace);
                        }
                    }

                    MessageBox.Show("Done", "Replace All");
                }
                else
                {
                    var fullPath = documentPaths.Dequeue();
                    FindAll(fullPath, 0, -1, term, false, searchOptions, Replace);
                }
            }

            Replace(Enumerable.Empty<AnalyzingResult.Location>());
        }
    }
}
