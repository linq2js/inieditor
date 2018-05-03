using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ScintillaNET;

namespace IniEditor
{
    public partial class MainForm : Form
    {

        private readonly string _findDefaultText;
        private readonly string _replaceDefaultText;

        public MainForm()
        {
            InitializeComponent();

            logTextBox.Lexer = Lexer.Container;
            logTextBox.Styles[Style.Default].BackColor = Color.Black;
            logTextBox.Styles[Style.Default].ForeColor = Color.Silver;
            logTextBox.Styles[Style.Default].Size = 10;
            logTextBox.Styles[Style.Default].Font = FontFamily.GenericMonospace.Name;

            logTextBox.Styles[Style.CallTip].BackColor = Color.Black;
            logTextBox.Styles[Style.CallTip].ForeColor = Color.LightGreen;
            logTextBox.Styles[Style.CallTip].Hotspot = true;


            logTextBox.Styles[LogEntry.Heading].ForeColor = Color.Aqua;
            logTextBox.Styles[LogEntry.Heading].BackColor = Color.Black;

            logTextBox.SetSelectionBackColor(true, Color.Gray);
            logTextBox.SetSelectionForeColor(true, Color.Black);

            _findDefaultText = findTextBox.Text;
            _replaceDefaultText = replaceTextBox.Text;

            App.Instance.Init();
            App.Instance.Subscribe(this);
        }

        public string[] CommandLineArgs { get; set; }

        private void quitItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            App.Instance.Close();
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    App.Instance.LoadDocument(fileName);
                }
            }
        }

        private void openProjectItem_Click(object sender, EventArgs e)
        {
            if (openProjectDialog.ShowDialog(this) == DialogResult.OK)
            {
                App.Instance.LoadProject(openProjectDialog.FileName);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // handle document list changed or document activated
            App.Instance.Subscribe(
                x => x.Documents,
                x => x.Document,
                (documents, document) =>
                {

                    replaceAllOpenedFilesItem.Enabled =
                        replaceAllCurrentFileItem.Enabled =
                            replaceNextItem.Enabled =
                                clearHighlightsItem.Enabled =
                                    findNextItem.Enabled =
                                        findPreviousItem.Enabled =
                                            openedFilesItem.Enabled =
                                                documents.Count > 0;

                    // remove next siblings of menu separator
                    var removedItems = mainToolbar.Items.OfType<ToolStripItem>().SkipWhile(x => x != editItem).Skip(1).ToArray();

                    foreach (var item in removedItems)
                    {
                        mainToolbar.Items.Remove(item);
                    }

                    // render document buttons
                    var fileIndex = 0;
                    foreach (var d in documents.Values.ToArray())
                    {
                        // add separator before each file button
                        mainToolbar.Items.Add(new ToolStripSeparator());

                        var item = new ToolStripButton
                        {
                            // add shortcut for file buttons
                            // shortcut indexes should be sorted from 1-0
                            // no shortcut for 11th file and above
                            Text = (fileIndex == 9 ? "&0. " : fileIndex < 9 ? $"&{fileIndex + 1}. " : "") + Path.GetFileName(d.FullPath).Ellipsis(15),
                            Checked = d == document,
                            ToolTipText = d.FullPath
                        };

                        item.Click += delegate
                        {
                            App.Instance.ActivateDocument(d);
                        };

                        item.MouseDown += (s, me) =>
                        {
                            if (me.Button != MouseButtons.Middle) return;

                            App.Instance.CloseDocument(d);

                            if (d.Editor != null)
                            {
                                editorPanel.Controls.Remove(d.Editor);
                            }
                        };

                        mainToolbar.Items.Add(item);

                        fileIndex++;
                    }

                    if (document != null)
                    {

                        if (document.Editor == null)
                        {
                            // disable panel for configuring
                            var editor = new Scintilla
                            {
                                Dock = DockStyle.Fill
                            };
                            editor.Text = document.Contents;
                            editor.EmptyUndoBuffer();
                            editor.TextChanged += delegate
                            {
                                App.Instance.DocumentChanged(document, editor.Text);
                            };
                            editor.UpdateUI += delegate
                            {
                                App.Instance.UpdateUi();
                            };
                            editorPanel.Controls.Add(editor);

                            // re-enable panel after editor is configured
                            editor.Configure();

                            App.Instance.EditorAdded(document, editor);
                        }

                        // show active document and hide each other
                        foreach (Control control in editorPanel.Controls)
                        {
                            control.Visible = control == document.Editor;
                        }
                    }
                });

            App.Instance.Subscribe(
                x => x.Project,
                x => x.Document,
                x => x.Document?.Editor?.CurrentPosition ?? 0,
                (project, document, position) =>
                {
                    var projectStatus = project == null ? null : Path.GetFileName(project.FullPath) + " | ";
                    string documentStatus = null;

                    if (document != null)
                    {
                        documentStatus = $"{Path.GetFileName(document.FullPath)}";
                        // find section from current position
                        var sectionName = App.Instance.FindSection(position);
                        if (!string.IsNullOrWhiteSpace(sectionName))
                        {
                            documentStatus += " » " + sectionName;
                        }
                    }
                    else
                    {
                        documentStatus = "No document loaded";
                    }

                    statusLabel.Text = projectStatus + documentStatus;
                });

            // handle logging
            App.Instance.Subscribe(
                x => x.Logs.Count,
                logCount =>
                {
                    if (logCount > 0)
                    {
                        UpdateOutput(() =>
                        {
                            var start = logTextBox.TextLength;
                            foreach (var logEntry in App.Instance.PullLogs())
                            {
                                logTextBox.StartStyling(start);
                                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss : ");
                                logTextBox.AppendText(date);
                                logTextBox.SetStyling(date.Length, Style.Default);
                                var entryStart = logTextBox.TextLength;
                                logTextBox.AppendText(logEntry.Contents);

                                start += date.Length;

                                if (logEntry.Formatter == null)
                                {
                                    logTextBox.SetStyling(logEntry.Contents.Length, Style.Default);
                                }
                                else
                                {
                                    logEntry.Formatter.Invoke(logTextBox, entryStart);
                                }

                                start += logEntry.Contents.Length;
                                logTextBox.AppendText("\r\n");
                                logTextBox.Format(start, 2, Style.Default);
                                start += 2;
                            }
                        });
                    }
                });

            // handle menu states for current document
            App.Instance.Subscribe(
                x => x.Document?.Editor,
                (editor) =>
                {
                    currentFileItem.Enabled = gotoDeclarationItem.Enabled = gotoLineItem.Enabled = editor != null;
                }
            );

            if (CommandLineArgs?.Any() ?? false)
            {
                foreach (var commandLineArg in CommandLineArgs)
                {
                    if (commandLineArg.EndsWith(".inip", StringComparison.OrdinalIgnoreCase))
                    {
                        // project file
                        App.Instance.LoadProject(commandLineArg);
                    }
                    else
                    {
                        App.Instance.LoadDocument(commandLineArg);
                    }
                }
            }
        }

        private void clearLogsItem_Click(object sender, EventArgs e)
        {
            UpdateOutput(() =>
            {
                logTextBox.ClearAll();
                logTextBox.RemoveAllData();
            });
        }

        private void UpdateOutput(Action action)
        {
            logTextBox.ReadOnly = false;
            try
            {
                action();
            }
            finally
            {
                logTextBox.ReadOnly = true;
            }
        }

        private void gotoLineItem_Click(object sender, EventArgs e)
        {
            App.Instance.GoToLine();
        }

        private void gotoDeclaration_Click(object sender, EventArgs e)
        {
            App.Instance.DocumentReady(d =>
            {
                var word = d.Editor.GetCurrentWord();

                App.Instance.GoToDeclaration(word, d.Editor.CurrentPosition, data =>
                {
                    d.Editor.AutoComplete(0, true, data).Then(x => App.Instance.GoTo(x));
                });
            });
        }

        private void projectFilesItem_Click(object sender, EventArgs e)
        {
            App.Instance.ShowProjectFiles();
        }

        private void findDropdown_ButtonClick(object sender, EventArgs e)
        {
            // do find all files if no document loaded
            if (App.Instance.Model.Document == null)
            {
                allFilesItem_Click(allFilesItem, e);
            }
            else
            {
                findNextItem_Click(findNextItem, e);
            }
        }

        private void newItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                App.Instance.CreateDocument(saveFileDialog.FileName);
            }
        }

        private void newProjectItem_Click(object sender, EventArgs e)
        {
            if (saveProjectDialog.ShowDialog() == DialogResult.OK)
            {
                App.Instance.CreateProject(saveProjectDialog.FileName);
            }
        }

        private void findItem_Click(object sender, EventArgs e)
        {
            findTextBox.Focus();
            findTextBox.Select();
        }

        private SearchOptions GetSearchOptions()
        {
            var flags = SearchOptions.None;

            if (matchCaseItem.Checked) flags |= SearchOptions.MatchCase;
            if (regexItem.Checked) flags |= SearchOptions.Regex;
            if (extendedItem.Checked) flags |= SearchOptions.Extended;
            if (highlightMatchesItem.Checked) flags |= SearchOptions.Highlight;
            if (markLineItem.Checked) flags |= SearchOptions.MarkLine;
            if (standardItem.Checked) flags |= SearchOptions.Standard;
            if (wholeWordItem.Checked) flags |= SearchOptions.WholeWord;
            if (wordStartItem.Checked) flags |= SearchOptions.WordStart;

            return flags;
        }

        private void findPreviousItem_Click(object sender, EventArgs e)
        {
            DoFind(() => App.Instance.Find(findTextBox.Text, true, GetSearchOptions()));
        }

        private void findAllItem_Click(object sender, EventArgs e)
        {

        }

        private void clearHighlightsItem_Click(object sender, EventArgs e)
        {
            App.Instance.ClearHighlights();
        }

        private void replaceNextItem_Click(object sender, EventArgs e)
        {
            DoReplace(() => App.Instance.Replace(findTextBox.Text, replaceTextBox.Text, false, GetSearchOptions()));
        }
        

        private void standardItem_Click(object sender, EventArgs e)
        {
            standardItem.Checked = true;
            extendedItem.Checked = false;
            regexItem.Checked = false;
        }

        private void extendedItem_Click(object sender, EventArgs e)
        {
            standardItem.Checked = false;
            extendedItem.Checked = true;
            regexItem.Checked = false;
        }

        private void regexItem_Click(object sender, EventArgs e)
        {
            standardItem.Checked = false;
            extendedItem.Checked = false;
            regexItem.Checked = true;
        }

        private void replaceDropdown_ButtonClick(object sender, EventArgs e)
        {
            App.Instance.Replace(findTextBox.Text, replaceTextBox.Text, false, GetSearchOptions());
        }



        private void findTextBox_Enter(object sender, EventArgs e)
        {

            if (findTextBox.Text == _findDefaultText)
            {
                findTextBox.Text = string.Empty;
            }
        }

        private void findTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(findTextBox.Text))
            {
                findTextBox.Text = _findDefaultText;
            }
        }

        private void replaceTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(replaceTextBox.Text))
            {
                replaceTextBox.Text = _replaceDefaultText;
            }
        }

        private void replaceTextBox_Enter(object sender, EventArgs e)
        {
            if (replaceTextBox.Text == _replaceDefaultText)
            {
                replaceTextBox.Text = string.Empty;
            }
        }

        private void logTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            var position = logTextBox.CharPositionFromPoint(e.X, e.Y);
            var data = logTextBox.GetData(position);
            foreach (var d in data)
            {
                switch (d.Type)
                {
                    case EditorExtraDataType.OpenDocument:
                        if (d.Data is AnalyzingResult.Location location)
                        {
                            App.Instance.LoadDocument(App.FilePath(location.FileId)).Ready.Then(x =>
                            {
                                x.Editor.SetSel(location.Position, location.EndPosition);
                            });
                        }
                        else
                        {
                            App.Instance.LoadDocument(d.Data.ToString());
                        }
                        break;
                }
            }
        }

        private void DoFind(Action action)
        {
            if (string.IsNullOrWhiteSpace(findTextBox.Text) || findTextBox.Text == _findDefaultText)
            {
                return;
            }

            var text = findTextBox.Text;
            if (findTextBox.Items.Contains(text))
            {
                findTextBox.Items.Remove(text);
            }

            findTextBox.Items.Insert(0, text);

            action();
        }

        private void DoReplace(Action action)
        {
            DoFind(() =>
            {
                if (string.IsNullOrWhiteSpace(replaceTextBox.Text) || replaceTextBox.Text == _findDefaultText || replaceTextBox.Text == findTextBox.Text)
                {
                    return;
                }

                var text = replaceTextBox.Text;

                if (replaceTextBox.Items.Contains(text))
                {
                    findTextBox.Items.Remove(text);
                }

                replaceTextBox.Items.Insert(0, text);

                action();
            });
        }

        private void openedFilesItem_Click(object sender, EventArgs e)
        {
            DoFind(() => App.Instance.FindAll(findTextBox.Text, FindReplaceAllMode.OpenedFile, GetSearchOptions()));
        }

        private void allFilesItem_Click(object sender, EventArgs e)
        {
            DoFind(() => App.Instance.FindAll(findTextBox.Text, FindReplaceAllMode.AllFiles, GetSearchOptions()));
        }

        private void currentFileItem_Click(object sender, EventArgs e)
        {
            DoFind(() => App.Instance.FindAll(findTextBox.Text, FindReplaceAllMode.CurrentFile, GetSearchOptions()));
        }

        private void findNextItem_Click(object sender, EventArgs e)
        {
            DoFind(() => App.Instance.Find(findTextBox.Text, false, GetSearchOptions()));
        }

        private void replaceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (App.Instance.Model.Document == null)
                {
                    replaceAllAllFilesItem_Click(replaceAllItem, e);
                }
                else
                {
                    replaceNextItem_Click(replaceNextItem, e);
                }
            }
        }

        private void replaceAllCurrentFileItem_Click(object sender, EventArgs e)
        {
            DoReplace(() =>
            {
                App.Instance.ReplaceAll(findTextBox.Text, replaceTextBox.Text, FindReplaceAllMode.CurrentFile, GetSearchOptions());
            });
        }

        private void replaceAllOpenedFilesItem_Click(object sender, EventArgs e)
        {
            DoReplace(() =>
            {
                App.Instance.ReplaceAll(findTextBox.Text, replaceTextBox.Text, FindReplaceAllMode.OpenedFile, GetSearchOptions());
            });
        }

        private void replaceAllAllFilesItem_Click(object sender, EventArgs e)
        {
            DoReplace(() =>
            {
                App.Instance.ReplaceAll(findTextBox.Text, replaceTextBox.Text, FindReplaceAllMode.AllFiles, GetSearchOptions());
            });
        }

        private void findTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                findDropdown_ButtonClick(findItem, e);
            }
        }
    }
}