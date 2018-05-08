using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Redux;
using ScintillaNET;

namespace IniEditor
{
    public partial class App
    {
        public async Task<AData.FileDef> GetCurrentAnalyzedFile()
        {
            var analytics = await Model.Analyzed;
            var document = Model.Document;
            if (document == null) return null;
            analytics.Files.TryGetValue(FileId(document.FullPath), out AData.FileDef file);
            return file;
        }

        public void Analyze()
        {
            Model.Disposables.Add(Timer.Interval(200, () =>
            {
                Model.Analyzed = InternalAnalyze(Model.Analyzed);
            }));
        }

        private async Task<AData> InternalAnalyze(Task<AData> oldData)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            // should parse definition and template files first
            var filePaths = GetProjectDocuments(Model.Project?.FullPath)
                .Concat(GetOpenedDocumentPaths())
                .ToArray();

            if (filePaths.Length == 0) return await oldData;

            var lexer = new IniLexer();
            var analytics = new AData();

            foreach (var filePath in filePaths)
            {
                var fileId = FileId(filePath);
                var file = analytics.Files[fileId] = new AData.FileDef(filePath);
                var contents = ReadFile(filePath);

                // add to generic defs by default
                if (file.Type == AData.FileType.Definition)
                {
                    analytics.Definitions[string.Empty].Add(file);
                }

                string propertyName = null;
                string sectionName = null;
                AData.Location lastLocation = null;
                lexer.Parse(contents, (token, position, type) =>
                {
                    switch (type)
                    {
                        case IniLexer.StyleSection:
                            if (lastLocation != null)
                            {
                                // extend last section location
                                lastLocation.EndPosition = position - 1;
                            }
                            sectionName = token.Substring(1, token.Length - 2).Trim();
                            lastLocation = analytics.AddRef("s:" + sectionName, fileId, position);
                            lastLocation.TextLength = token.Length;
                            file.Locations[sectionName] = lastLocation;
                            analytics.AddDef(file, sectionName);
                            break;
                        case IniLexer.StyleKey:
                            propertyName = token.Trim();
                            break;
                        case IniLexer.StyleValue:
                            if (string.IsNullOrWhiteSpace(sectionName))
                            {
                                Debug.WriteLine("Invalid data. {0}: {1}", filePath, position);
                            }
                            else
                            {
                                var trimmedValue = token.Trim();
                                analytics.AddDef(file, sectionName, propertyName, trimmedValue);
                            }
                            break;
                    }
                }, exploreValues: false);

                // there is single section
                if (lastLocation != null && lastLocation.EndPosition == -1)
                {
                    lastLocation.EndPosition = contents.Length;
                }
            }


            // process generic properties
            foreach (var definitions in analytics.Definitions.Values)
            {
                foreach (var file in definitions)
                {
                    if (file.Sections.TryGetValue(string.Empty, out AData.SectionDef genericSection))
                    {
                        var shouldMoveProperties = genericSection.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Value.ApplyTo)).ToArray();

                        foreach (var pair in shouldMoveProperties)
                        {
                            genericSection.Properties.Remove(pair.Key);
                            var targetSectionNames = pair.Value.ApplyTo.SplitAndTrim();
                            foreach (var targetSectionName in targetSectionNames)
                            {
                                if (!file.Sections.TryGetValue(targetSectionName, out AData.SectionDef targetSection))
                                {
                                    file.Sections[targetSectionName] = targetSection = new AData.SectionDef(targetSectionName)
                                    {
                                        Type = AData.SectionType.Class
                                    };
                                }

                                // move property to target section
                                targetSection.Properties[pair.Key] = pair.Value;
                            }

                            // remove apply for data
                            pair.Value.ApplyTo = null;
                        }
                    }
                }
            }

            stopwatch.Stop();
            Debug.WriteLine("Analyzed {0}", stopwatch.Elapsed.TotalMilliseconds);

            return analytics;
        }


        public void AppendToList(string sectionName, params string[] listNames)
        {
            var editor = Model.Document?.Editor;

            Timer.Timeout(10, delegate
            {
                editor?.SafeInvoke(delegate
                {
                    foreach (var listName in listNames)
                    {
                        var listGroup = FindSection(listName, editor.Text);
                        if (!listGroup.Success) continue;

                        var startLine = editor.LineFromPosition(listGroup.Index);
                        var maxLine = editor.Lines.Count;
                        var lastNonEmptyLine = startLine;
                        var maxId = -1;
                        var appended = false;
                        startLine++;
                        while (startLine < maxLine)
                        {
                            var t = editor.Lines[startLine].Text;
                            if (string.IsNullOrWhiteSpace(t))
                            {
                                // skip this line
                            }
                            else
                            {

                                var p = t.IndexOfAny(new[] {'[', ';', '='});

                                if (p == -1 || t[p] == ';')
                                {
                                    // invalid line data or the line contains only comment
                                }
                                else
                                {
                                    // next section
                                    if (t[p] == '[')
                                    {
                                        // stop finding
                                        break;
                                    }

                                    // parse id
                                    var idString = t.Substring(0, p).Trim();
                                    // remove comment if any
                                    var valueString = t.Substring(p + 1).Split(';').First().Trim();

                                    appended = valueString.IgnoreCaseEqual(sectionName);

                                    // dont append again
                                    if (appended)
                                    {
                                        break;
                                    }

                                    // try to parse id and find out max id
                                    if (int.TryParse(idString, out int id))
                                    {
                                        if (id > maxId)
                                        {
                                            maxId = id;
                                        }
                                    }

                                    lastNonEmptyLine = startLine;
                                }
                            }

                            startLine++;
                        }

                        if (appended) continue;

                        var insertPosition = editor.Lines[lastNonEmptyLine].EndPosition;
                        editor.InsertText(insertPosition, $"{maxId + 1}={sectionName}\r\n");
                    }
                });
            });
        }

        public async Task<IDictionary<string, string>> GetAvailProps(string sectionName, bool includeOptionals = true, bool forInsert = false)
        {
            if (Model.Document == null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // user is editing ini definition file
            if (Path.GetExtension(Model.Document.FullPath).IgnoreCaseEqual(".inid"))
            {
                // is prop definition
                if (sectionName.Contains("."))
                {
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"Type", ""},
                        {"Value", ""},
                        {"Optional", "true"},
                        {"ApplyTo", "true"}
                    };
                }

                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Type", ""},
                    {"ItemOf", ""},
                    //{"Optional", "true"},
                };
            }

            var analytics = await Model.Analyzed;
            var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var definitions = analytics.FindDefinitions(Model.Document.FullPath).ToArray();
            // add generic section ([<empty>.PropName]) to look up
            var sectionNames = sectionName.SplitAndTrim().Concat(includeOptionals ? new[] {string.Empty} : new string[0]).ToArray();
            // special props
            props["@Type"] = string.Empty;
            var listNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


            foreach (var definitionFile in definitions)
            {
                foreach (var name in sectionNames)
                {
                    if (definitionFile.Sections.TryGetValue(name, out AData.SectionDef sectionDef))
                    {
                        if (!string.IsNullOrWhiteSpace(sectionDef.ItemOf))
                        {
                            foreach (var listName in sectionDef.ItemOf.SplitAndTrim())
                            {
                                listNames.Add(listName);
                            }
                        }

                        foreach (var property in sectionDef.Properties)
                        {
                            // dont include optional properties
                            if (!includeOptionals && property.Value.Optional)
                            {
                                continue;
                            }

                            // extract default value
                            var value = property.Value.Value;
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                value = property.Value.Values.FirstOrDefault();
                            }
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                switch (property.Value.Type)
                                {
                                    case AData.PropertyType.YesNo:
                                        value = "yes";
                                        break;
                                    case AData.PropertyType.TrueFalse:
                                        value = "false";
                                        break;
                                }
                            }
                            props[property.Key] = value;
                        }
                    }
                }
            }

            if (forInsert)
            {
                if (listNames.Count > 0)
                {
                    props[";append"] = string.Join(",", listNames);
                }
            }



            // no approx definition for these sections
            if (props.Count == 1 && !forInsert)
            {
                foreach (var word in analytics.Words)
                {
                    props[word] = string.Empty;
                }
            }

            return props;
        }

        private static Group FindSection(string sectionName, string text, bool exactMatch = true)
        {
            var escapedSectionName = Regex.Escape(sectionName);
            var wildcard = exactMatch ? string.Empty : "[^;\\]]*";
            return Regex.Match(text, $@"(?:^(?'name'\[\s*{escapedSectionName}{wildcard}\s*\])|[\r\n]\s*(?'name'\[\s*{escapedSectionName}{wildcard}\s*\]))", RegexOptions.Compiled | RegexOptions.IgnoreCase).Groups["name"];
        }

        public async Task<IEnumerable<string>> GetAvailValues(string sectionName, string propName, IEnumerable<string> existingValues)
        {
            if (Model.Document == null) return Enumerable.Empty<string>();

            var analytics = await Model.Analyzed;

            // user is editing ini definition file
            if (Path.GetExtension(Model.Document.FullPath).IgnoreCaseEqual(".inid"))
            {
                // is prop definition
                if (sectionName.Contains("."))
                {
                    switch (propName)
                    {
                        case "Optional": return new[] {"true", "false"};
                        case "ApplyTo":
                            if (analytics.Files.TryGetValue(Model.Document.Id, out AData.FileDef currentDef))
                            {
                                return currentDef.Sections.Where(x => x.Value.Type == AData.SectionType.Class).Select(x => x.Key);
                            }
                            break;
                    }
                }
                return Enumerable.Empty<string>();
            }

            IEnumerable<string> InternalGetValues()
            {

                var definitions = analytics.FindDefinitions(Model.Document.FullPath).ToArray();
                if (!analytics.Files.TryGetValue(Model.Document.Id, out AData.FileDef file))
                {
                    file = new AData.FileDef(string.Empty);
                }

                if (propName.IgnoreCaseEqual("@type"))
                {
                    // collect all section names
                    return definitions.SelectMany(x => x.Sections.Where(s => s.Value.Type == AData.SectionType.Class).Select(s => s.Key));
                }

                // add generic section ([<empty>.PropName]) to look up
                var sectionNames = sectionName.SplitAndTrim().Concat(new[] {string.Empty}).ToArray();
                var noDefinition = true;
                var values = new HashSet<string>();

                foreach (var name in sectionNames)
                {
                    var propDef = definitions
                        .Select(x =>
                        {
                            x.Sections.TryGetValue(name, out AData.SectionDef s);
                            return s;
                        })
                        .Where(x => x != null)
                        .Select(x =>
                        {
                            x.Properties.TryGetValue(propName, out AData.PropertyDef p);

                            // no standard prop matched
                            if (p == null)
                            {
                                // try to find wildcard props
                                foreach (var propPair in x.Properties)
                                {
                                    if (propPair.Key.Contains("*"))
                                    {
                                        var wildcardReplacement = ";";
                                        // replace wildcard * with special char (ex: ;), make sure the rest of prop name is escaped and then replace wild card with .* pattern
                                        var regex = "^" + Regex.Escape(propPair.Key.Replace("*", wildcardReplacement)).Replace(wildcardReplacement, @"[^\s\.]*") + "$";
                                        var propMatch = Regex.Match(propName, regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                        if (propMatch.Success)
                                        {
                                            p = propPair.Value;
                                            break;
                                        }
                                    }
                                }
                            }

                            return p;
                        })
                        .FirstOrDefault(x => x != null);

                    if (propDef == null) continue;

                    noDefinition = false;

                    switch (propDef.Type)
                    {
                        case AData.PropertyType.TrueFalse:
                            values.Add("true");
                            values.Add("false");
                            break;
                        case AData.PropertyType.YesNo:
                            values.Add("yes");
                            values.Add("no");
                            break;
                        case AData.PropertyType.Section:
                            // get all avail sections
                            foreach (var key in file.Sections.Keys)
                            {
                                values.Add(key);
                            }
                            break;
                        case AData.PropertyType.ItemOf:

                            foreach (var l in propDef.Lists)
                            {
                                var listName = l;
                                var fileToLookup = file;

                                // lookup external file
                                var openParentheseIndex = listName.IndexOf('(');
                                if (openParentheseIndex != -1)
                                {
                                    var fileName = listName.Substring(openParentheseIndex).Trim('(', ')') + ".ini";
                                    listName = listName.Substring(0, openParentheseIndex);
                                    fileToLookup = analytics.Files.Values.FirstOrDefault(x => x.FullPath.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                                }

                                if (fileToLookup != null)
                                {
                                    if (fileToLookup.Sections.TryGetValue(listName, out AData.SectionDef listSection))
                                    {
                                        foreach (var item in listSection.Properties.Values)
                                        {
                                            values.Add(item.Value);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            foreach (var value in propDef.Values)
                            {
                                values.Add(value);
                            }
                            break;
                    }
                }

                if (noDefinition)
                {
                    if (analytics.Pairs.TryGetValue(propName, out AData.Pair pair))
                    {
                        return pair.Values;
                    }
                    return analytics.Words;
                }

                return values;
            }

            return InternalGetValues().Except(existingValues, StringComparer.OrdinalIgnoreCase);
        }

        public void Intellisense()
        {
            Model.Document?.Editor?.Intellisense();
        }

        public async Task ShowTemplates()
        {
            var editor = Model.Document?.Editor;
            if (editor == null) return;
            var analytics = await Model.Analyzed;
            editor.ShowTemplates(analytics.Templates);
        }

        public void QuickSearch(string term, Scintilla output)
        {
            var editor = Model.Document?.Editor;
            if (editor == null) return;

            // goto line
            if (int.TryParse(term, out int line))
            {
                if (line < 0 || line >= editor.Lines.Count) return;

                editor.Lines[line].Goto();
            }
            else
            {
                var parts = term.Split('.');
                var sectionName = parts.First();
                var propName = string.Join(".", parts.Skip(1));

                // goto section
                var group = FindSection(sectionName, editor.Text, false);
                if (group.Success)
                {
                    if (string.IsNullOrWhiteSpace(propName))
                    {
                        editor.GotoPosition(group.Index);
                        editor.SetSelection(group.Index, group.Index + group.Length);
                    }
                    else
                    {
                        var start = editor.LineFromPosition(group.Index) + 1;
                        var count = editor.Lines.Count;
                        var found = false;

                        while (start < count)
                        {
                            var l = editor.Lines[start];
                            var t = l.Text;
                            var p = t.IndexOfAny(new[] {';', '=', '['});

                            if (p != -1)
                            {
                                if (t[p] == '[')
                                {
                                    // next section
                                    break;
                                }
                                if (t[p] == '=')
                                {
                                    var prop = t.Split('=').First().Trim();
                                    if (prop.StartsWith(propName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        l.Goto();
                                        editor.SetSelection(l.Position, l.EndPosition);
                                        found = true;
                                        break;
                                    }
                                }
                            }

                            
                            start++;
                        }

                        if (!found)
                        {
                            editor.GotoPosition(group.Index);
                            editor.SetSelection(group.Index, group.Index + group.Length);
                            Log($"Section [{sectionName}] does not have tag {propName}");
                        }
                    }
                }
            }
        }
    }
}
