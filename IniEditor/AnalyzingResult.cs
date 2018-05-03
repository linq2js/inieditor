using System;
using System.Collections.Generic;

namespace IniEditor
{
    public class AnalyzingResult
    {
        public class Location
        {
            public Location(int fileId, int position)
            {
                FileId = fileId;
                Position = position;
            }

            public int FileId { get; }

            public int Position { get; }

            public int EndPosition { get; set; } = -1;

            public int Line { get; set; }

            public int Column { get; set; }

            public string PreviewText { get; set; }

            /// <summary>
            /// Length of match text
            /// </summary>
            public int TextLength { get; set; }
        }

        public class Pair
        {
            public SortedSet<string> Values { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public class Reference
        {

            public IList<Location> Locations { get; } = new List<Location>();
        }

        public class File
        {
            public File(string fullPath)
            {
                FullPath = fullPath;
            }

            public string FullPath { get; }

            public IDictionary<string, Location> Locations { get; } = new Dictionary<string, Location>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<int, File> Files { get; } = new Dictionary<int, File>();

        public SortedSet<string> Words { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, Reference> References { get; } = new Dictionary<string, Reference>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, Pair> Pairs { get; } = new Dictionary<string, Pair>(StringComparer.OrdinalIgnoreCase);


        public Location AddReference(string key, int fileId, int position)
        {
            if (!References.TryGetValue(key, out Reference reference))
            {
                References[key] = reference = new Reference();
            }

            var location = new Location(fileId, position);
            reference.Locations.Add(location);

            return location;
        }

        public AnalyzingResult AddWord(string word)
        {
            if (word.Length > 1)
            {
                // word should start with letter/digit and not be number
                if (char.IsLetter(word[0]) || char.IsDigit(word[0]))
                {
                    // trim all specials
                    var testWord = word.Trim('%', '[', '(', '{', '}', ')', ']', '-');

                    if (testWord.Length > 1 && !float.TryParse("0" + testWord, out float _))
                    {
                        Words.Add(testWord);
                    }
                }
            }

            return this;
        }

        public AnalyzingResult AddPair(string key, params string[] values)
        {
            if (!Pairs.TryGetValue(key, out Pair pair))
            {
                Pairs[key] = pair = new Pair();
            }

            foreach (var value in values)
            {
                pair.Values.Add(value);
            }
            return this;
        }
    }
}
