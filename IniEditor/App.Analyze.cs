using System.Collections.Generic;
using System.Linq;
using Redux;

namespace IniEditor
{
    public partial class App
    {
        public AnalyzingResult.File GetCurrentAnalyzedFile()
        {
            var analytics = Model.Analytics;
            var document = Model.Document;
            if (document == null) return null;
            analytics.Files.TryGetValue(FileId(document.FullPath), out AnalyzingResult.File file);
            return file;
        }

        public string GetFileFullPath(int id)
        {
            return Model.Analytics.Files
                .Where(x => x.Key == id)
                .Select(x => x.Value.FullPath)
                .FirstOrDefault();
        }

        public void Analyze()
        {
            Model.Disposables.Add(Timer.Interval(200, () =>
            {
                var filePaths = GetAllDocumentPaths().ToArray();

                if (filePaths.Length == 0) return;

                var lexer = new IniLexer();
                var analytics = new AnalyzingResult();

                foreach (var filePath in filePaths)
                {
                    var fileId = FileId(filePath);
                    var file = analytics.Files[fileId] = new AnalyzingResult.File(filePath);

                    var contents = ReadFile(filePath);

                    string lastKey = null;
                    string lastSection = null;
                    AnalyzingResult.Location lastLocation = null;
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
                                var sectionName = token.Substring(1, token.Length - 2).Trim();
                                lastLocation = analytics.AddReference("s:" + sectionName, fileId, position);
                                lastLocation.TextLength = token.Length;
                                // section range is end of section name by default
                                //lastLocation.EndPosition = position + sectionName.Length;
                                analytics.AddWord(sectionName);
                                // save location for searching section from current position
                                file.Locations[sectionName] = lastLocation;
                                lastSection = sectionName;
                                break;
                            case IniLexer.StyleKey:
                                lastKey = token.Trim();
                                analytics.AddWord(lastKey);
                                break;
                            case IniLexer.StyleValue:
                                var trimmedValue = token.Trim();
                                // is a list
                                if (trimmedValue.Contains(","))
                                {
                                    var values = trimmedValue.Split(',').Select(x => x.Trim()).ToArray();

                                    foreach (var value in values)
                                    {
                                        analytics.AddWord(value);
                                    }

                                    analytics.AddPair(lastKey, values);
                                }
                                else
                                {
                                    analytics.AddWord(trimmedValue);
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

                Model.Analytics = analytics;

                Update();
            }));
        }
    }
}
