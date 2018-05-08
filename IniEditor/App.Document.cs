using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Redux;
using ScintillaNET;

namespace IniEditor
{
    public partial class App
    {
        public Document LoadDocument(string fullPath, bool inactive = false)
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

                if (!inactive)
                {
                    // set active document
                    x.Document = document;
                }

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

        public int GetCurrentLine()
        {
            return Model.Document?.Editor?.CurrentLine ?? -1;
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

        public void CloseDocument(Document document = null)
        {
            Update(x =>
            {
                if (document == null)
                {
                    document = x.Document;
                }

                if (document == null) return;

                document.LastPosition = -1;

                if (document.Editor != null)
                {
                    var editor = document.Editor;
                    document.Editor.Parent.SafeInvoke(() => editor.Parent.Controls.Remove(editor));
                    document.Editor = null;
                }

                x.Documents.Remove(document.FullPath);

                x.Documents = x.Documents.Clone();

                // activate another document
                if (x.Document == document)
                {
                    x.Document = x.Documents.Count > 0 ? x.Documents.Values.First() : null;
                }

                // is project file
                if (document.FullPath.IgnoreCaseEqual(x.Project?.FullPath))
                {
                    x.Project.Contents = document.Contents;
                    ReloadConfigs();
                }
            });
        }

        public async Task<string> FindSection(int position)
        {
            if (Model.Document == null) return null;
            var fileId = FileId(Model.Document.FullPath);
            var analytics = await Model.Analyzed;
            if (analytics.Files.TryGetValue(fileId, out AData.FileDef file))
            {
                return (
                    from location in file.Locations
                    where position >= location.Value.Position && position <= location.Value.EndPosition
                    select location.Key).FirstOrDefault();
            }

            return null;
        }

        public async void GoToDeclaration(string word, int position, bool inlineEdit, Action<IDictionary<string, AData.Location>, Action<AData.Location>> handleMultipleLocations)
        {
            var analytics = await Model.Analyzed;

            void InternalGoTo(AData.Location location)
            {
                if (inlineEdit)
                {
                    var filePath = FilePath(location.FileId);
                    // load document for edit
                    LoadDocument(filePath, true).Ready.Then(d =>
                    {
                        Update(x =>
                        {
                            x.InlineEdit = new InlineEditData(d.Editor.GetTextRange(location.Position, location.EndPosition - location.Position), location);
                            x.InlineEdit.Closed.Then(inlineEditData =>
                            {
                                void UpdateInlineEdit()
                                {
                                    Update(y =>
                                    {
                                        y.Document?.Editor?.Focus();
                                        y.InlineEdit = null;
                                    });
                                }

                                // make sure inline edit must be the same of current
                                if (Model.InlineEdit == inlineEditData)
                                {
                                    if (d.Editor != null && inlineEditData.Changed)
                                    {
                                        Redux.Timer.Timeout(100, () =>
                                        {
                                            d.Editor.SafeInvoke(() =>
                                            {
                                                var selectionStart = d.Editor.SelectionStart;
                                                var selectionEnd = d.Editor.SelectionEnd;

                                                d.Editor.Enabled = false;
                                                d.Editor.SetSelection(location.Position, location.EndPosition);
                                                d.Editor.ReplaceSelection(inlineEditData.Text);
                                                d.Editor.SetSelection(selectionStart, selectionEnd);

                                                d.Editor.GotoPosition(selectionEnd);
                                                d.Editor.Enabled = true;

                                                UpdateInlineEdit();
                                            });
                                        });



                                    }
                                    else
                                    {
                                        UpdateInlineEdit();
                                    }
                                }
                            });
                        });
                    });
                }
                else
                {
                    GoTo(location);
                }
            }

            if (analytics.References.TryGetValue("s:" + word, out AData.Reference reference))
            {
                var data = new Dictionary<string, AData.Location>(StringComparer.OrdinalIgnoreCase);
                foreach (var location in reference.Locations)
                {
                    if (position >= location.Position && position <= location.EndPosition)
                    {
                        // do not inlcude this location, current caret is here
                        continue;
                    }

                    if (analytics.Files.TryGetValue(location.FileId, out AData.FileDef file))
                    {
                        data[(inlineEdit ? "Edit" : "Jump To") + $" [{word}] ({Path.GetFileName(file.FullPath)} -> {Path.GetDirectoryName(file.FullPath)})"] = location;
                    }
                }

                if (data.Count == 1)
                {
                    InternalGoTo(data.Values.First());
                }
                else
                {
                    handleMultipleLocations(data, InternalGoTo);
                }
            }
            else
            {
                MessageBox.Show($"No [{word}] found");
            }
        }

        public void GoTo(AData.Location location)
        {
            var document = Model.Document;
            if (document != null && document.Id == location.FileId)
            {
                document.Editor.SafeInvoke(() => document.Editor.GotoPosition(location.Position));
            }
            else // document not active or loaded
            {
                var fullPath = FilePath(location.FileId);

                LoadDocument(fullPath).Ready.Then(d =>
                {
                    d.Editor.SafeInvoke(() =>
                    {
                        d.Editor.Focus();
                        d.Editor.GotoPosition(location.Position);
                    });
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
            const char nullChar = (char) 0;
            const char cr = (char) 13;
            const char lf = (char) 10;
            const char tab = (char) 9;

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

        public IEnumerable<string> GetOpenedDocumentPaths()
        {
            return Model.Documents.Values.Select(document => document.FullPath);
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
                    document.Editor.GotoPosition(firstResult.Position);
                    document.Editor.SetSelection(firstResult.Position, firstResult.EndPosition);
                }
            });
        }

        public void FindAll(string term, FindReplaceAllMode mode, SearchOptions searchOptions)
        {
            if (mode == FindReplaceAllMode.CurrentFile && Model.Document == null) return;

            var documentPaths = new Queue<string>(mode == FindReplaceAllMode.CurrentFile ? new[] {Model.Document.FullPath} : GetAllDocumentPaths(mode == FindReplaceAllMode.AllFiles));
            var result = new List<AData.Location>();

            void Find(IEnumerable<AData.Location> locations)
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

            Find(Enumerable.Empty<AData.Location>());
        }

        private void DisplayFindResult(string term, IEnumerable<AData.Location> result)
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

        public void FindAll(string fullPath, int offset, int length, string term, bool previous, SearchOptions searchOptions, Action<IEnumerable<AData.Location>> action)
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

                            return new AData.Location(fileId, position)
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
                    document.Editor.SetSelection(firstResult.Position, firstResult.EndPosition);
                    document.Editor.ReplaceSelection(replace);
                }
            });
        }

        public void ReplaceAll(string term, string replace, FindReplaceAllMode mode, SearchOptions searchOptions)
        {
            if (MessageBox.Show("Do you want to continue with Replace All ?", "Replace All", MessageBoxButtons.YesNo) == DialogResult.No) return;

            if (mode == FindReplaceAllMode.CurrentFile && Model.Document == null) return;

            var documentPaths = new Queue<string>(mode == FindReplaceAllMode.CurrentFile ? new[] {Model.Document.FullPath} : GetAllDocumentPaths(mode == FindReplaceAllMode.AllFiles));
            var result = new List<AData.Location>();

            void Replace(IEnumerable<AData.Location> locations)
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
                            document.Editor.SetSelection(location.Position, location.EndPosition);
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

            Replace(Enumerable.Empty<AData.Location>());
        }

        public void GoBack()
        {
            var editor = Model.Document?.Editor;
            if (editor == null || Model.Document.LastPosition == -1)
            {
                return;
            }

            editor.Focus();
            editor.GotoPosition(Model.Document.LastPosition);
        }

        public void SavePosition()
        {
            if (Model.Document?.Editor != null)
            {
                Model.Document.LastPosition = Model.Document.Editor.CurrentPosition;
            }
        }

        public string GetSelectedText()
        {
            return Model.Document?.Editor.SelectedText;
        }

        public void ActivateEditor()
        {
            Model.Document?.Editor?.Focus();
        }

        public void InlineEdit()
        {
            DocumentReady(d =>
            {
                var word = d.Editor.GetCurrentWord();

                Instance.GoToDeclaration(word, d.Editor.CurrentPosition, true, (data, @goto) =>
                {
                    d.Editor.AutoComplete(0, true, data).Then(@goto);
                });
            });
        }

        public async void ShowContextActionsFor(Scintilla editor)
        {
            var word = editor.GetCurrentWord();
            var inlineEdit = editor.GetData<InlineEditData>(0).FirstOrDefault();
            var fileId = inlineEdit?.Location.FileId ?? Model.Document.Id;
            var analytics = await Model.Analyzed;
            

            if (analytics.Files.TryGetValue(fileId, out AData.FileDef file))
            {
                var actions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

                void AddSection(string path)
                {
                    LoadDocument(path, true).Ready.Then(d =>
                    {
                        d.Editor.AppendText($"\r\n[{word}]");
                    });
                }

                if (!file.Sections.ContainsKey(word))
                {
                    actions[$"Add Section [{word}]"] = delegate
                    {
                        AddSection(file.FullPath);
                    };
                }

                var filesCanAddSection = analytics.Files.Values.Where(x => !x.Sections.ContainsKey(word)).ToArray();
                if (filesCanAddSection.Length > 0)
                {
                    actions[$"Add Section [{word}] To..."] = delegate
                    {
                        editor.AutoComplete(0, true, filesCanAddSection.ToDictionary(f => $"{Path.GetFileName(f.FullPath)} ({Path.GetDirectoryName(f.FullPath)})", f => f))
                            .Then(fileToAdd =>
                            {
                                AddSection(fileToAdd.FullPath);
                            });
                    };
                }

                editor.AutoComplete(0, true, actions).Then(x => x());
            }
        }

        public async Task Diagram(string word, int fileId, Action<IEnumerable<SectionBlock.Data>> handleBlocksAction)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            
            var analytics = await Model.Analyzed;

            if (!analytics.Files.TryGetValue(fileId, out AData.FileDef file))
            {
                return;
            }

            if (!file.Sections.TryGetValue(word, out AData.SectionDef section))
            {
                return;
            }

            IEnumerable<AData.Location> FindRefs(string name)
            {
                analytics.References.TryGetValue("s:" + name, out AData.Reference r);
                return (r?.Locations ?? Enumerable.Empty<AData.Location>()).Distinct(new AData.LocationComparer());
            }

            var sectonGroups = Model.SectionGroups;
            var sectionProps = new List<object>();
            var sectionLinks = new List<object>();
            var sectionUsages = new List<object>();
            var otherBlocks = new List<SectionBlock.Data>();
            var namedBlocks = new Dictionary<string, SectionBlock.Data>(StringComparer.OrdinalIgnoreCase);
            var anotherRefs = FindRefs(word).Where(x => x.FileId != fileId);

            bool AddNamedBlock(string prop, params object[] data)
            {
                foreach (var sectonGroup in sectonGroups)
                {
                    foreach (var pattern in sectonGroup.Value.Patterns)
                    {
                        if (pattern.IsMatch(prop))
                        {
                            if (!namedBlocks.TryGetValue(sectonGroup.Key, out SectionBlock.Data d))
                            {
                                namedBlocks[sectonGroup.Key] = d = new SectionBlock.Data
                                {
                                    Text = sectonGroup.Key,
                                    Type = BlockType.Default
                                };
                                otherBlocks.Add(d);
                            }
                            d.Properties = d.Properties.Concat(data).ToArray();
                            return true;
                        }
                    }
                }
                return false;
            }

            foreach (var location in anotherRefs)
            {
                sectionUsages.Add(new AData.LocationGroup($"{word.ToUpper()} ({Path.GetFileName(FilePath(location.FileId))})", word, new[] { location }));
            }

            // collection all props of select section
            foreach (var propertyDef in section.Properties)
            {
                var values = propertyDef.Value.Value.Split(',');
                if (values.Length == 1)
                {
                    var refs = FindRefs(propertyDef.Value.Value);
                    // dont include current section location
                    if (propertyDef.Value.Value.IgnoreCaseEqual(word))
                    {
                        refs = refs.Where(x => x.FileId != fileId);
                    }

                    var distinctRefs = new HashSet<AData.Location>();
                    foreach (var location in refs)
                    {
                        if (distinctRefs.Any(x => x.FileId == location.FileId && x.Position == location.Position)) continue;
                        distinctRefs.Add(location);
                    }
                    var propText = $"{propertyDef.Key} = {propertyDef.Value.Value}";
                    var locationGroup = new AData.LocationGroup(propText, propertyDef.Value.Value, distinctRefs);
                    if (locationGroup.Locations.Length > 0)
                    {
                        if (!AddNamedBlock(propertyDef.Key, locationGroup))
                        {
                            sectionLinks.Add(locationGroup);
                        }
                    }
                    else
                    {
                        if (!AddNamedBlock(propertyDef.Key, propText))
                        {
                            sectionProps.Add(propText);
                        }
                    }
                }
                else
                {
                    var links = new List<object>();
                    // multiple value
                    foreach (var value in values)
                    {
                        if (float.TryParse(value, out _))
                        {
                            links.Add(value);
                        }
                        else
                        {
                            // try to find out link
                            var refs = FindRefs(value);
                            var distinctRefs = new HashSet<AData.Location>();
                            foreach (var location in refs)
                            {
                                if (distinctRefs.Any(x => x.FileId == location.FileId && x.Position == location.Position)) continue;
                                distinctRefs.Add(location);
                            }
                            var locationGroup = new AData.LocationGroup(value, value, distinctRefs);
                            if (locationGroup.Locations.Length > 0)
                            {
                                links.Add(locationGroup);
                            }
                            else
                            {
                                links.Add(value);
                            }
                        }
                    }

                    if (links.OfType<AData.LocationGroup>().Any())
                    {
                        if (!AddNamedBlock(propertyDef.Key, links.Select(l =>
                        {
                            if (l is AData.LocationGroup lg)
                            {
                                return new AData.LocationGroup($"{propertyDef.Key} = {lg.Text}", lg.Key, lg.Locations) as object;
                            }
                            return $"{propertyDef.Key} = {l}" as object;
                        }).ToArray()))
                        {
                            otherBlocks.Add(new SectionBlock.Data
                            {
                                Text = propertyDef.Key,
                                Type = BlockType.Default,
                                Properties = links.ToArray()
                            });
                        }
                    }
                    else
                    {
                        var propText = $"{propertyDef.Key} = {values.First()}";
                        if (!AddNamedBlock(propertyDef.Key, propText))
                        {
                            // not contains any link => simple value
                            sectionProps.Add(propText);
                        }
                    }
                }
            }


            foreach (var analyticsFile in analytics.Files)
            {
                foreach (var sectionDef in analyticsFile.Value.Sections)
                {
                    if (fileId == analyticsFile.Key && sectionDef.Key.IgnoreCaseEqual(word))
                    {
                        continue;
                    }

                    foreach (var propertyDef in sectionDef.Value.Properties)
                    {
                        if (float.TryParse(propertyDef.Value.Value, out float _))
                        {
                            
                        }
                        else if (propertyDef.Value.Value.IgnoreCaseEqual(word)
                                 || propertyDef.Value.Value.StartsWith(word + ",", StringComparison.OrdinalIgnoreCase)
                                 || propertyDef.Value.Value.EndsWith("," + word, StringComparison.OrdinalIgnoreCase)
                                 || propertyDef.Value.Value.IndexOf("," + word + ",", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            // find location
                            if (analytics.References.TryGetValue("s:" + sectionDef.Key, out AData.Reference refs))
                            {
                                var location = refs.Locations.FirstOrDefault(x => x.FileId == analyticsFile.Key);
                                if (location != null)
                                {
                                    var text = $"{sectionDef.Key}  »  {propertyDef.Key} = {propertyDef.Value.Value}";
                                    sectionUsages.Add(new AData.LocationGroup(text, sectionDef.Key, new[] {location}));
                                }
                            }
                        }
                    }
                }
            }

            

            var allBlocks = new List<SectionBlock.Data>
            {
                new SectionBlock.Data
                {
                    Text = $"{word.ToUpper()} ({Path.GetFileName(file.FullPath)})",
                    Type = BlockType.Main,
                    Properties = sectionProps.ToArray()
                }
            };


            if (sectionLinks.Count > 0)
            {
                allBlocks.Add(new SectionBlock.Data
                {
                    Text = "Links",
                    Type = BlockType.Links,
                    Properties = sectionLinks.ToArray()
                });
            }

            if (sectionUsages.Count > 0)
            {
                allBlocks.Add(new SectionBlock.Data
                {
                    Text = "Usages",
                    Type = BlockType.Usages,
                    Properties = sectionUsages.ToArray()
                });
            }

            allBlocks.AddRange(otherBlocks);

            // apply section styles
            foreach (var block in allBlocks)
            {
                if (Model.SectionDetails.TryGetValue(block.Type == BlockType.Main ? "Main" : block.Text, out SectionStyle s))
                {
                    block.Style = s;
                }
            }

            handleBlocksAction(allBlocks);
        }

        public async void Diagram(string word)
        {
            if (Model.Document == null) return;
            var fileId = Model.Document.Id;
            await Diagram(word, fileId, blocks =>
            {
                var diagramDialog = new DiagramDialog();
                diagramDialog.AddBlocks(blocks.ToArray());
                diagramDialog.ShowDialog();
            });
        }

        public async void Diagram(Scintilla editor = null)
        {
            if (editor == null)
            {
                editor = Model.Document?.Editor;
            }

            if (editor == null) return;


            var word = editor.GetCurrentWord();
            var inlineEdit = editor.GetData<InlineEditData>(0).FirstOrDefault();
            var fileId = inlineEdit?.Location.FileId ?? Model.Document.Id;
            await Diagram(word, fileId, blocks =>
            {
                var diagramDialog = new DiagramDialog();
                diagramDialog.AddBlocks(blocks.ToArray());
                diagramDialog.ShowDialog();
            });
        }
    }
}
