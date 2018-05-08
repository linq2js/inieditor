using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IniEditor
{
    public class AData
    {
        public class SectionDef
        {
            public SectionDef(string name)
            {
                Name = name;
            }

            public string Name { get;  }
            public string ItemOf { get; set; }
            public SectionType Type { get; set; }
            public bool Optional { get; set; }
            public IDictionary<string, PropertyDef> Properties { get; } = new Dictionary<string, PropertyDef>(StringComparer.OrdinalIgnoreCase);
        }

        public class PropertyDef
        {
            public PropertyType Type { get; set; }

            public bool Optional { get; set; } = true;

            /// <summary>
            /// Used for storing file data, not for definition
            /// </summary>
            public string Value { get; set; } = string.Empty;

            public HashSet<string> Values { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public string ApplyTo { get; set; }

            public string[] Lists { get; set; } = new string[0];
        }

        public enum SectionType
        {
            Default,
            List,
            Class
        }

        public enum PropertyType
        {
            Default,
            Section,
            NumberList,
            PercentList,
            YesNo,
            TrueFalse,
            ItemOf
        }

        public class LocationGroup
        {
            public LocationGroup(string text, string key, IEnumerable<Location> locations)
            {
                Text = text;
                Key = key;
                Locations = locations.ToArray();
            }

            public string Text { get; }
            public string Key { get; }

            public Location[] Locations { get; }

            public override string ToString()
            {
                return Text;
            }
        }

        public class LocationComparer : IEqualityComparer<Location>
        {
            public bool Equals(Location x, Location y)
            {
                return x.Position == y.Position && x.FileId == y.FileId;
            }

            public int GetHashCode(Location obj)
            {
                return obj.Position.GetHashCode() ^ obj.FileId.GetHashCode();
            }
        }

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

            public override int GetHashCode()
            {
                return FileId.GetHashCode() ^ Position.GetHashCode();
            }

            /// <summary>
            /// Length of match text
            /// </summary>
            public int TextLength { get; set; }

            public override string ToString()
            {
                return PreviewText;
            }
        }

        public class Pair
        {
            public SortedSet<string> Values { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public class Reference
        {

            public IList<Location> Locations { get; } = new List<Location>();
        }

        public enum FileType
        {
            Default,
            Definition
        }

        public class FileDef
        {
            public FileDef(string fullPath)
            {
                FullPath = fullPath;
                var extension = Path.GetExtension(fullPath);
                Type = ".inid".IgnoreCaseEqual(extension) ? FileType.Definition : FileType.Default;
            }

            public IDictionary<string, SectionDef> Sections { get; } = new Dictionary<string, SectionDef>(StringComparer.OrdinalIgnoreCase);

            public string FullPath { get; }

            public FileType Type { get; }

            public IDictionary<string, Location> Locations { get; } = new Dictionary<string, Location>(StringComparer.OrdinalIgnoreCase);

            public HashSet<SectionDef> Lists { get; } = new HashSet<SectionDef>();
        }

        public IDictionary<string, HashSet<FileDef>> Definitions { get; } = new Dictionary<string, HashSet<FileDef>>(StringComparer.OrdinalIgnoreCase)
        {
            {string.Empty, new HashSet<FileDef>()} // generic defs
        };

        public IDictionary<int, FileDef> Files { get; } = new Dictionary<int, FileDef>();

        public SortedSet<string> Words { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, Reference> References { get; } = new Dictionary<string, Reference>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, Pair> Pairs { get; } = new Dictionary<string, Pair>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> Templates { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public SectionDef AddDef(FileDef file, string sectionName)
        {
            if (file.Type == FileType.Definition)
            {
                var parts = sectionName.Split('.');
                if (!file.Sections.TryGetValue(parts[0], out SectionDef section))
                {
                    file.Sections[parts[0]] = section = new SectionDef(parts[0]);
                }

                // add property def
                if (parts.Length > 1)
                {
                    var propName = string.Join(".", parts.Skip(1));
                    if (!section.Properties.ContainsKey(propName))
                    {
                        section.Properties[propName] = new PropertyDef();
                    }
                }

                return section;
            }
            else
            {
                if (!file.Sections.TryGetValue(sectionName, out SectionDef section))
                {
                    file.Sections[sectionName] = section = new SectionDef(sectionName);
                }

                if (section.Type == SectionType.List)
                {
                    file.Lists.Add(section);
                }

                AddWord(sectionName);
                return section;
            }
        }

        public IEnumerable<FileDef> FindDefinitions(string filePath)
        {
            if (Definitions.TryGetValue(string.Empty, out HashSet<FileDef> genericDefs))
            {
                foreach (var genericDef in genericDefs)
                {
                    yield return genericDef;
                }
            }

            var fileName = Path.GetFileName(filePath);

            if (Definitions.TryGetValue(fileName, out HashSet<FileDef> specificDefs))
            {
                foreach (var specificDef in specificDefs)
                {
                    yield return specificDef;
                }
            }
        }

        public void AddDef(FileDef file, string sectionName, string propertyName, string propertyValue)
        {
            if (file.Type == FileType.Definition)
            {
                // process special sections
                if (sectionName.IgnoreCaseEqual("applyto"))
                {
                    if (!Definitions.TryGetValue(propertyValue, out HashSet<FileDef> list))
                    {
                        Definitions[propertyValue] = list = new HashSet<FileDef>();
                    }
                    list.Add(file);

                    // remove the file from generic def
                    var genericDefs = Definitions[string.Empty];
                    if (genericDefs.Contains(file))
                    {
                        genericDefs.Remove(file);
                    }
                    return;
                }

                if (sectionName.IgnoreCaseEqual("templates"))
                {
                    Templates[propertyName] = propertyValue.Replace(@"\n", "\r\n");
                    return;
                }

                var parts = sectionName.Split('.');

                var section = AddDef(file, sectionName);

                if (parts.Length > 1)
                {
                    // property metadata
                    var propName = string.Join(".", parts.Skip(1));
                    var property = section.Properties[propName];
                    if (propertyName.IgnoreCaseEqual("optional"))
                    {
                        property.Optional = propertyValue.IsTrue();
                    }
                    else if (propertyName.IgnoreCaseEqual("applyto"))
                    {
                        property.ApplyTo = propertyValue;
                    }
                    else if (propertyName.IgnoreCaseEqual("type"))
                    {
                        var itemOfPrefix = "itemof:";
                        if (propertyValue.StartsWith(itemOfPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            property.Type = PropertyType.ItemOf;
                            property.Lists = propertyValue.Substring(itemOfPrefix.Length).SplitAndTrim().ToArray();
                        }
                        else
                        {
                            property.Type = propertyValue.ToEnum<PropertyType>();
                        }
                    }
                    else if (propertyName.IgnoreCaseEqual("value"))
                    {
                        property.Value = propertyValue;
                    }
                    else if (int.TryParse(propertyName, out int _)) // is item
                    {
                        property.Values.Add(propertyValue);
                    }
                }
                else
                {
                    // section metadata
                    if (propertyName.IgnoreCaseEqual("optional"))
                    {
                        section.Optional = propertyValue.IsTrue();
                    }
                    else if (propertyName.IgnoreCaseEqual("type"))
                    {
                        section.Type = propertyValue.ToEnum<SectionType>();
                    }
                    else if (propertyName.IgnoreCaseEqual("itemof"))
                    {
                        section.ItemOf = propertyValue;
                    }
                }
            }
            else
            {
                var section = AddDef(file, sectionName);

                if (!section.Properties.TryGetValue(propertyName, out PropertyDef property))
                {
                    section.Properties[propertyName] = property = new PropertyDef();
                }

                property.Value = propertyValue;
                AddWord(sectionName);
                AddWord(propertyName);
                AddPair(propertyName, propertyValue.SplitAndTrim().ToArray());
            }
        }


        public Location AddRef(string key, int fileId, int position)
        {
            if (!References.TryGetValue(key, out Reference reference))
            {
                References[key] = reference = new Reference();
            }

            var location = new Location(fileId, position);
            reference.Locations.Add(location);

            return location;
        }

        public AData AddWord(string word)
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

        public AData AddPair(string key, params string[] values)
        {
            if (!Pairs.TryGetValue(key, out Pair pair))
            {
                Pairs[key] = pair = new Pair();
            }

            foreach (var value in values)
            {
                if (float.TryParse(value, out float _))
                {
                    continue;
                }
                pair.Values.Add(value);
            }
            return this;
        }
    }
}
