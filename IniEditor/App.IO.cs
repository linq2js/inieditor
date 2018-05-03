using System;
using System.Collections.Generic;
using System.IO;

namespace IniEditor
{
    public partial class App
    {
        private static readonly IDictionary<int, string> FileNames = new Dictionary<int, string>();

        public static int FileId(string path)
        {
            var id = StringComparer.OrdinalIgnoreCase.GetHashCode(path);
            FileNames[id] = path;
            return id;
        }

        public static string FilePath(int id)
        {
            FileNames.TryGetValue(id, out string name);
            return name;
        }

        public string ReadFile(string fullPath)
        {
            return Model.Files.GetOrAdd(fullPath, x =>
            {
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var contents = reader.ReadToEnd();
                        reader.Close();
                        return contents;
                    }
                }
            });
        }

        public void WriteFile(string fullPath, string contents)
        {
            Model.Files[fullPath] = contents;

            File.WriteAllText(fullPath, contents);

            Model.FileVersion = Guid.NewGuid();

            Update();
        }
    }
}
