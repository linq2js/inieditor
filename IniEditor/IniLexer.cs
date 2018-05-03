using System;
using System.Text.RegularExpressions;
using System.Threading;
using ScintillaNET;

namespace IniEditor
{
    public class IniLexer
    {

        public delegate void TokenCollector(string token, int position, int type);

        public const int StyleDefault = 0;
        public const int StyleSection = 1;
        public const int StyleKey = 2;
        public const int StyleValue = 3;
        public const int StyleExtension = 4;
        public const int StyleComment = 5;
        public const int StyleNumber = 6;
        public const int StyleBoolean = 7;
        public const int StyleHeading = 8;

        public void Parse(string text, TokenCollector collect, int position = 0, bool exploreValues = true, CancellationTokenSource cts = null)
        {
            void Collect(Group match, int style, int offset = 0)
            {
                CollectDynamicStyle(match, x => style, offset);
            }

            void CollectDynamicStyle(Group match, Func<string, int> style, int offset = 0)
            {
                if (!match.Success) return;
                collect(match.Value, position + offset + match.Index, style(match.Value));
            }


            // multi line comment     \s*(?'mcomment'\/\*[\w\W]*?\*\/) |

            var matches = Regex.Matches(text, @"
(?:
    \s*(?'comment';.*) |
    \s*(?'section'\[[^\]\r\n]+\])\s*(?'comment';.*)? |
    \s*(?'key'[^;=\r\n]+)\s*=\s*(?'value'[^;\r\n]+)?(?'comment';.*)? |
    \s*(?'unknown'.+)
)
", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

            foreach (Match m in matches)
            {
                if (cts != null && cts.IsCancellationRequested)
                {
                    return;
                }
                Collect(m.Groups["mcomment"], StyleComment);
                CollectDynamicStyle(m.Groups["comment"], s => s.Contains("***") ? StyleHeading : StyleComment);
                Collect(m.Groups["section"], StyleSection);
                Collect(m.Groups["key"], StyleKey);
                Collect(m.Groups["unknown"], StyleDefault);

                var value = m.Groups["value"];
                Collect(value, StyleValue);

                if (exploreValues && value.Success && value.Value.IndexOf('{') == -1) // not is locomoto/guid value
                {
                    Regex.Replace(value.Value, @"\b(?:(?'bool'true|false|yes|no)|(?'number'\d+\.\d+|\.\d+|\d+))\b", vm =>
                    {
                        Collect(vm.Groups["bool"], StyleBoolean, value.Index);
                        Collect(vm.Groups["number"], StyleNumber, value.Index);
                        return null;
                    });
                }
            }
        }

        public void Style(Scintilla scintilla, string text, int pos = 0, CancellationTokenSource cts = null)
        {
            Parse(text, delegate(string token, int position, int type)
            {
                scintilla.StartStyling(position);
                scintilla.SetStyling(token.Length, type);
            }, pos, cts: cts);
        }
    }
}
