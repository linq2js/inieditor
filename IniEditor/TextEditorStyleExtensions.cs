using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Redux;
using ScintillaNET;

namespace IniEditor
{
    public class EditorExtraData
    {
        public object Data { get; }
        public EditorExtraDataType Type { get; }
        public int From { get; }
        public int To { get; }

        public EditorExtraData(int from, int to, EditorExtraDataType type, object data)
        {
            From = from;
            To = to;
            Type = type;
            Data = data;
        }
    }

    public enum EditorExtraDataType
    {
        OpenDocument,
        InlineEdit
    }

    public static class TextEditorStyleExtensions
    {
        

        private const char AutoCompleteSeprator = '|';
        private const int MaxAutoCompleteItems = 50;
        private const int MaxAutoCompleteHeight = 20;

        private static Deferred<string> _autoCompleteDeferred;
        private static Redux.Timer _autoCompleteTimer;

        public static Scintilla AddData(this Scintilla editor, int from, int to, EditorExtraDataType type, object data)
        {
            var extraData = (IList<EditorExtraData>) editor.Tag;
            if (extraData == null)
            {
                editor.Tag = extraData = new List<EditorExtraData>();
            }
            extraData.Add(new EditorExtraData(from, to, type, data));
            return editor;
        }

        public static IEnumerable<EditorExtraData> GetData(this Scintilla editor, int position)
        {
            var extraData = (IList<EditorExtraData>) editor.Tag;
            if (extraData == null) return Enumerable.Empty<EditorExtraData>();
            return extraData.Where(x => position >= x.From && position < x.To);
        }

        public static IEnumerable<T> GetData<T>(this Scintilla editor, int position)
        {
            var extraData = (IList<EditorExtraData>)editor.Tag;
            if (extraData == null) return Enumerable.Empty<T>();
            return extraData.Where(x => position >= x.From && position < x.To).Select(x => x.Data).OfType<T>();
        }

        public static Scintilla RemoveData(this Scintilla editor, int position)
        {
            if (editor != null)
            {
                var extraData = (IList<EditorExtraData>) editor.Tag;
                editor.Tag = extraData.Where(x => !(position >= x.From && position < x.To)).ToList();
            }
            return editor;
        }

        public static Scintilla RemoveAllData(this Scintilla editor)
        {
            editor.Tag = null;
            return editor;
        }


        public static Scintilla Format(this Scintilla editor, int start, int length, int style)
        {
            editor.StartStyling(start);
            editor.SetStyling(length, style);

            return editor;
        }

        public static Deferred<string> AutoComplete(this Scintilla editor, int length, bool cancel, IEnumerable<string> data, bool allItem = true)
        {
            return AutoComplete(editor, length, cancel, data.ToDictionary(x => x, x => x, StringComparer.OrdinalIgnoreCase), allItem);
        }

        public static Deferred<T> AutoComplete<T>(this Scintilla editor, int length, bool cancel, IDictionary<string, T> data, bool allItem = true)
        {
            _autoCompleteTimer?.Stop();

            var defer = new Deferred<T>();

            if (data.Count == 0)
            {
                editor.AutoCCancel();
                defer.Reject();
                return defer;
            }

            _autoCompleteTimer = Redux.Timer.Timeout(100, () =>
            {
                editor.SafeInvoke(delegate
                {
                    var items = data.Keys.Take(MaxAutoCompleteItems).OrderBy(x => x.StartsWith("@") ? "\0" : x).ToArray();
                    var list = (data.Count <= 20 && allItem && !cancel ? "<All>" + AutoCompleteSeprator : string.Empty) + string.Join(AutoCompleteSeprator.ToString(), items);

                    editor.AutoCShow(length, list);

                    _autoCompleteDeferred = new Deferred<string>();
                    _autoCompleteDeferred.Then(x =>
                        {
                            if (x == "<All>" && allItem && !cancel)
                            {
                                editor.AutoCCancel();
                                editor.AddText(string.Join(",", items.Take(20)));
                                return;
                            }

                            if (data.TryGetValue(x, out T d))
                            {
                                defer.Resolve(d);
                            }
                            if (cancel)
                            {
                                editor.AutoCCancel();
                            }

                        })
                        .Fail(x =>
                        {
                            defer.Reject(x);
                        });
                });
            });

            return defer;
        }

        public static string GetCurrentWord(this Scintilla editor, int position = -1, string pattern = @"^[\w@\.-]+")
        {
            if (position == -1)
            {
                position = editor.CurrentPosition;
            }

            var lineIndex = editor.LineFromPosition(position);
            var line = editor.Lines[lineIndex];
            var text = line.Text;
            position -= line.Position;

            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var start = Math.Max(0, position - 1);
            Match lastMatch = null;
            while (start >= 0)
            {
                var match = regex.Match(text.Substring(start));
                if (!match.Success) break;
                lastMatch = match;
                start--;
            }

            return lastMatch?.Value ?? string.Empty;
        }

        /// <summary>
        /// collect existing props and name of current section
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="lineIndex"></param>
        /// <param name="sectionName"></param>
        /// <param name="existingProps"></param>
        public static int FindUpSection(this Scintilla editor, int lineIndex, out string sectionName, out HashSet<string> existingProps)
        {
            sectionName = string.Empty;
            existingProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // find current selection name
            // need to optimize this logic, it should be reused for many places (update document status, intellisense)
            while (lineIndex >= 0)
            {
                var l = editor.Lines[lineIndex];
                var trimStart = l.Text.TrimStart();
                if (trimStart.StartsWith("["))
                {
                    var p = trimStart.IndexOfAny(new[] {';', ']', '\r', '\n'});
                    if (p != -1 && trimStart[p] != ';')
                    {
                        // dont overwrite value of @type prop
                        if (string.IsNullOrEmpty(sectionName))
                        {
                            sectionName = trimStart.Substring(1, p - 1);
                        }
                        break;
                    }
                }
                else
                {
                    var p = trimStart.IndexOf('=');
                    if (p != -1)
                    {
                        var propName = trimStart.Substring(0, p).TrimEnd();
                        var propValue = trimStart.Substring(p + 1).Trim();

                        // this is special type
                        if (propName.IgnoreCaseEqual("@type"))
                        {
                            sectionName = propValue;
                        }

                        existingProps.Add(propName);
                    }
                }
                lineIndex--;
            }
            return lineIndex;
        }

        public static void ShowTemplates(this Scintilla editor, IDictionary<string, string> templates)
        {
            var word = editor.GetCurrentWord();
            editor.AutoComplete(word.Length, true, templates, false).Then(x =>
            {
                editor.SafeInvoke(delegate
                {
                    editor.InsertText(editor.CurrentPosition, x);
                });
            });
        }

        public static void ScrollToLine(this Scintilla editor, int line, bool moveCaret = false)
        {
            var startLine = editor.Lines[line];
            var endLine = editor.Lines[Math.Min(line + editor.LinesOnScreen, editor.Lines.Count - 1)];
            editor.ScrollRange(startLine.Position, endLine.Position);
            if (moveCaret)
            {
                editor.SelectionStart = startLine.Position;
                // dont select \r\n chars
                editor.SelectionEnd = startLine.Length > 2 ? startLine.EndPosition - 2 : startLine.EndPosition;
            }
        }

        public static async void Intellisense(this Scintilla editor)
        {
            var position = editor.CurrentPosition;
            var line = editor.Lines[editor.LineFromPosition(position)];
            var lineText = line.Text;
            var column = position - line.Position;
            var lineTrimmedText = lineText.TrimStart();
            var word = editor.GetCurrentWord(position);
            string sectionName, propName;

            if (lineTrimmedText.StartsWith("[") || lineTrimmedText.StartsWith(";"))
            {
                return;
            }

            // eval row
            if (lineTrimmedText.StartsWith("="))
            {
                var file = await App.Instance.GetCurrentAnalyzedFile();
                // no file analyized, wait a while
                if (file == null) return;
                var parts = word.Split('.');
                sectionName = parts.First();
                propName = string.Join(".", parts.Skip(1));

                if (parts.Length < 2) // dont use string.IsNullOrWhiteSpace(propName) , it does not cover the user case entering dot only
                {
                    // display all section names
                    editor.AutoComplete(sectionName.Length, false, file.Sections.Keys.Where(x => x.StartsWith(sectionName, StringComparison.OrdinalIgnoreCase)));
                }
                else if (file.Sections.TryGetValue(sectionName, out AData.SectionDef sectionDef))
                {
                    

                    editor.AutoComplete(propName.Length, true, sectionDef.Properties
                        .Where(x => x.Key.StartsWith(propName, StringComparison.OrdinalIgnoreCase))
                        .Select(x => $"{x.Key}={x.Value.Value}"))
                        .Then(selectedProp =>
                        {
                            editor.SafeInvoke(() =>
                            {
                                editor.SetSelection(line.Position, line.EndPosition);
                                editor.ReplaceSelection(selectedProp + "\r\n");
                            });
                        });
                }
                return;
            }

            editor.FindUpSection(line.Index - 1, out sectionName, out HashSet<string> existingProps);

            
            var equalIndex = lineText.IndexOfAny(new[] {'=', ';'});
            if (equalIndex != -1 && lineText[equalIndex] == ';' && equalIndex <= column)
            {
                // hide auto complete for comment
                return;
            }

            if (equalIndex == -1 || column < equalIndex)
            {
                // show intellisense for property name
                // collect avail prop names
                var props = (await App.Instance.GetAvailProps(sectionName)).Keys
                    .Where(x => x.StartsWith("@") || x.StartsWith(word))
                    .Except(existingProps, StringComparer.OrdinalIgnoreCase).ToArray();
                editor.AutoComplete(word.Length, false, props, false);
            }
            else
            {
                // show intellisense for property value
                var existingValues = lineText.Substring(equalIndex + 1).SplitAndTrim();
                propName = lineText.Substring(0, equalIndex).Trim();
                var values = (await App.Instance.GetAvailValues(sectionName, propName, existingValues))
                    .Where(x => x.StartsWith(word))
                    .ToArray();
                editor.AutoComplete(word.Length, false, values).Then(selectedValue =>
                {
                    editor.SafeInvoke(async delegate
                    {
                        if (!propName.IgnoreCaseEqual("@type")) return;

                        // fill out all props
                        // find down
                        var maxLineIndex = editor.Lines.Count - 1;
                        var l = line.Index + 1;
                        var text = new StringBuilder();
                        var removeStart = -1;
                        var removeEnd = -1;

                        while (l <= maxLineIndex)
                        {
                            if (removeStart == -1)
                            {
                                removeStart = editor.Lines[l].Position;
                            }
                            var t = editor.Lines[l].Text;
                            var trimmedText = t.TrimStart();

                            if (trimmedText.StartsWith("["))
                            {
                                l--;
                                break;
                            }

                            removeEnd = editor.Lines[l].EndPosition;

                            if (!string.IsNullOrWhiteSpace(trimmedText))
                            {
                                text.Append(t);
                            }

                            var p = trimmedText.IndexOf('=');
                            if (p == -1)
                            {
                                // skip
                            }
                            else
                            {
                                existingProps.Add(trimmedText.Substring(0, p).TrimEnd());
                            }
                            l++;
                        }

                        var availProps = (await App.Instance.GetAvailProps(selectedValue, false, true)).Where(x => !existingProps.Contains(x.Key)).ToArray();
                        string[] appendToLists = null;

                        foreach (var availProp in availProps)
                        {
                            if (availProp.Key.StartsWith("@")) continue;
                            // dont suggest the prop contains wildcard
                            if (availProp.Key.IndexOfAny(new[] {'?', '*'}) != -1) continue;

                            // special commands
                            if (availProp.Key.StartsWith(";"))
                            {
                                switch (availProp.Key.Substring(1))
                                {
                                    case "append":
                                        appendToLists = availProp.Value.SplitAndTrim().ToArray();
                                        break;
                                }
                                continue;
                            }


                            text.AppendLine($"{availProp.Key}={availProp.Value}");
                        }

                        if (removeStart != -1 && removeEnd != -1)
                        {
                            editor.DeleteRange(removeStart, removeEnd - removeStart);
                        }


                        editor.InsertText(removeStart == -1 ? editor.TextLength : removeStart, (removeStart == -1 ? "\r\n" : string.Empty) + text.ToString());

                        if (appendToLists != null)
                        {
                            App.Instance.AppendToList(sectionName, appendToLists);
                        }
                    });
                });
            }
        }

        public static void Configure(this Scintilla editor, bool inlineEdit)
        {
            editor.Styles[Style.Default].Font = FontFamily.GenericMonospace.Name;
            editor.Styles[Style.Default].Size = 10;

            //editor.Styles[IniLexer.StyleSection].ForeColor = Color.White;
            editor.Styles[IniLexer.StyleSection].ForeColor = Color.Brown;
            //editor.Styles[IniLexer.StyleSection].Hotspot = true;

            editor.Styles[IniLexer.StyleComment].ForeColor = Color.Gray;
            //editor.Styles[IniLexer.StyleComment].Italic = true;

            editor.Styles[IniLexer.StyleKey].ForeColor = Color.DarkBlue;

            editor.Styles[IniLexer.StyleBoolean].ForeColor = Color.DarkRed;

            editor.Styles[IniLexer.StyleHeading].BackColor = Color.DarkMagenta;
            editor.Styles[IniLexer.StyleHeading].ForeColor = Color.White;
            editor.Styles[IniLexer.StyleHeading].Case = StyleCase.Upper;

            editor.Styles[IniLexer.StyleSubHeading].BackColor = Color.DarkBlue;
            editor.Styles[IniLexer.StyleSubHeading].ForeColor = Color.White;
            editor.Styles[IniLexer.StyleSubHeading].Case = StyleCase.Upper;

            editor.Styles[IniLexer.StyleNumber].ForeColor = Color.DarkRed;

            editor.AutoCIgnoreCase = true;
            editor.AutoCMaxHeight = MaxAutoCompleteHeight;
            editor.AutoCSeparator = AutoCompleteSeprator;
            editor.AutoCCancelAtStart = true;

            editor.BorderStyle = BorderStyle.None;
            //editor.Lexer = Lexer.Properties;
            editor.Lexer = Lexer.Null;

            var lexer = new IniLexer();


            editor.CharAdded += (s, e) =>
            {
                if (e.Char == '\r' || e.Char == '\n' || e.Char == '\t') return;
                editor.Intellisense();
            };

            editor.AutoCSelection += (s, e) =>
            {
                _autoCompleteDeferred?.Resolve(e.Text);
            };

            if (!inlineEdit)
            {
                editor.MouseClick += (s, e) =>
                {
                    if (e.Button != MouseButtons.Left || (Control.ModifierKeys & Keys.Control) != Keys.Control) return;

                    var position = editor.CharPositionFromPoint(e.X, e.Y);
                    var word = editor.GetCurrentWord(position);

                    App.Instance.GoToDeclaration(word, position, false, (data, @goto) =>
                    {
                        editor.AutoComplete(0, true, data, false).Then(@goto);
                    });
                };
            }

            

            var firstTime = true;

            void ApplyStyling()
            {
                var firstVisibleLine = editor.Lines[editor.FirstVisibleLine];
                var lastVisibleLine = editor.Lines[editor.FirstVisibleLine + editor.LinesOnScreen - 1];
                try
                {
                    // sometimes stupid exception throws, dont know why editor.Text is shorter than it is
                    var text = editor.GetTextRange(firstVisibleLine.Position, lastVisibleLine.EndPosition - firstVisibleLine.Position);
                    lexer.Style(editor, text, firstVisibleLine.Position);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            editor.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && e.Alt)
                {
                    App.Instance.ShowContextActionsFor(editor);
                }
                // goto next bookmark
                else if (e.Alt && e.KeyCode == Keys.Down)
                {
                    var line = editor.CurrentLine + 1;
                    var documentHeading = e.Control;
                    
                    while (line < editor.Lines.Count)
                    {
                        var l = editor.Lines[line];
                        var t = l.Text.TrimStart();
                        if (documentHeading ? t.Contains("***") : t.StartsWith("[") || t.StartsWith(";") && t.Contains("###"))
                        {
                            editor.ScrollToLine(line, true);
                            break;
                        }

                        line++;
                    }
                }
                // goto previous bookmark
                else if (e.Alt && e.KeyCode == Keys.Up)
                {
                    var line = editor.CurrentLine - 1;
                    var documentHeading = e.Control;

                    while (line >= 0)
                    {
                        var l = editor.Lines[line];
                        var t = l.Text.TrimStart();
                        if (documentHeading ? t.Contains("***") : t.StartsWith("[") || t.StartsWith(";") && t.Contains("###"))
                        {
                            editor.ScrollToLine(line, true);
                            break;
                        }

                        line--;
                    }
                }
                else if (e.Alt && e.KeyCode == Keys.Oemplus)
                {
                    
                }
                else
                {
                    return;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            };

            editor.Enter += delegate
            {
                ApplyStyling();
            };

            editor.Resize += delegate
            {
                ApplyStyling();
            };

            editor.UpdateUI += (sender, e) =>
            {
                if (firstTime)
                {
                    firstTime = false;
                    ApplyStyling();
                }
                else if (e.Change.HasFlag(UpdateChange.VScroll) || e.Change.HasFlag(UpdateChange.Content))
                {
                    ApplyStyling();
                }
            };
        }
    }
}
