using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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
        OpenDocument
    }

    public static class TextEditorStyleExtensions
    {
        

        private const char AutoCompleteSeprator = '|';
        private const int MaxAutoCompleteItems = 100;
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

        public static Deferred<string> AutoComplete(this Scintilla editor, int length, bool cancel, IEnumerable<string> data)
        {
            return AutoComplete(editor, length, cancel, data.ToDictionary(x => x, x => x, StringComparer.OrdinalIgnoreCase));
        }

        public static Deferred<T> AutoComplete<T>(this Scintilla editor, int length, bool cancel, IDictionary<string, T> data)
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
                editor.Invoke((Action) delegate
                {
                    var list = string.Join(AutoCompleteSeprator.ToString(), data.Keys);
                    editor.AutoCShow(length, list);

                    _autoCompleteDeferred = new Deferred<string>();
                    _autoCompleteDeferred.Then(x =>
                        {
                            if (data.TryGetValue(x, out T d))
                            {
                                defer.Resolve(d);
                            }
                            if (cancel)
                            {
                                editor.AutoCCancel();
                            }

                            return x;
                        })
                        .Fail(x =>
                        {
                            defer.Reject(x);
                        });
                });
            });

            return defer;
        }

        public static string GetCurrentWord(this Scintilla editor, int position = -1, string pattern = @"^[\w-]+")
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
            var start = position;
            Match lastMatch = null;
            while (start >= 0)
            {
                var match = regex.Match(text.Substring(start));
                if (!match.Success) break;
                lastMatch = match;
                start--;
            }

            return lastMatch?.Value;
        }

        public static void Configure(this Scintilla editor)
        {
            editor.Styles[Style.Default].Font = "Verdana";
            editor.Styles[Style.Default].Size = 10;

            //editor.Styles[IniLexer.StyleSection].ForeColor = Color.White;
            editor.Styles[IniLexer.StyleSection].ForeColor = Color.Brown;
            //editor.Styles[IniLexer.StyleSection].Hotspot = true;

            editor.Styles[IniLexer.StyleComment].ForeColor = Color.Gray;
            //editor.Styles[IniLexer.StyleComment].Italic = true;

            editor.Styles[IniLexer.StyleKey].ForeColor = Color.DarkBlue;

            editor.Styles[IniLexer.StyleBoolean].ForeColor = Color.DarkRed;

            editor.Styles[IniLexer.StyleHeading].BackColor = Color.DarkMagenta;
            editor.Styles[IniLexer.StyleHeading].FillLine = true;
            editor.Styles[IniLexer.StyleHeading].ForeColor = Color.White;
            //editor.Styles[IniLexer.StyleHeading].Bold = true;
            //editor.Styles[IniLexer.StyleHeading].Font = FontFamily.GenericMonospace.Name;
            editor.Styles[IniLexer.StyleHeading].Case = StyleCase.Upper;

            editor.Styles[IniLexer.StyleNumber].ForeColor = Color.DarkRed;

            editor.AutoCIgnoreCase = true;
            editor.AutoCMaxHeight = MaxAutoCompleteHeight;
            editor.AutoCSeparator = AutoCompleteSeprator;

            editor.BorderStyle = BorderStyle.None;
            //editor.Lexer = Lexer.Properties;
            editor.Lexer = Lexer.Null;

            var lexer = new IniLexer();


            editor.CharAdded += (s, e) =>
            {
                if (e.Char == '=' || e.Char == ',')
                {
                    var line = editor.Lines[editor.LineFromPosition(editor.CurrentPosition)];
                    var equalIndex = line.Text.IndexOf('=');
                    if (equalIndex != -1)
                    {
                        var currentValues = new HashSet<string>(line.Text.Substring(equalIndex + 1).Split(',').Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase);

                        var key = line.Text.Substring(0, equalIndex).Trim().Split('@').First();

                        if (App.Instance.Model.Analytics.Pairs.TryGetValue(key, out AnalyzingResult.Pair pair))
                        {
                            const string allItem = "<All>";
                            var values = pair.Values.Except(currentValues, StringComparer.OrdinalIgnoreCase).Take(MaxAutoCompleteItems).ToArray();
                            if (values.Length > 0)
                            {
                                editor.AutoComplete(0, false, new[] {allItem}.Concat(values)).Then(x =>
                                {
                                    // handle all item is selected
                                    if (x == allItem)
                                    {
                                        editor.AutoCCancel();
                                        editor.AddText(string.Join(",", values));
                                    }
                                });
                            }
                        }
                    }
                }
                else
                {
                    var word = editor.GetCurrentWord();

                    if (word.Length >= 1)
                    {
                        // show first 20 words
                        var availWords = App.Instance.Model.Analytics.Words.Where(x => x.StartsWith(word, StringComparison.OrdinalIgnoreCase)).Take(MaxAutoCompleteItems);

                        editor.AutoComplete(word.Length, false, availWords);
                    }
                }
            };



            editor.AutoCSelection += (s, e) =>
            {
                _autoCompleteDeferred?.Resolve(e.Text);
            };

            editor.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    var position = editor.CharPositionFromPoint(e.X, e.Y);
                    var word = editor.GetCurrentWord(position);

                    App.Instance.GoToDeclaration(word, position, data =>
                    {
                        editor.AutoComplete(0, true, data).Then(x => App.Instance.GoTo(x));
                    });
                }
            };

            var firstTime = true;

            void ApplyStyling()
            {
                var firstVisibleLine = editor.Lines[editor.FirstVisibleLine];
                var lastVisibleLine = editor.Lines[editor.FirstVisibleLine + editor.LinesOnScreen - 1];
                var text = editor.GetTextRange(firstVisibleLine.Position, lastVisibleLine.EndPosition - firstVisibleLine.Position);
                lexer.Style(editor, text, firstVisibleLine.Position);
            }


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
