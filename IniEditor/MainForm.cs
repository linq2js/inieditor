using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ScintillaNET;
using Timer = Redux.Timer;

namespace IniEditor
{
    public partial class MainForm : Form
    {

        private readonly string _findDefaultText;
        private readonly string _replaceDefaultText;
        private readonly Action _quickSearchThrottle;
        private InlineEditData _inlineEditData;

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

            logTextBox.Styles[LogEntry.Error].ForeColor = Color.White;
            logTextBox.Styles[LogEntry.Error].BackColor = Color.Red;

            logTextBox.SetSelectionBackColor(true, Color.Gray);
            logTextBox.SetSelectionForeColor(true, Color.Black);

            _findDefaultText = findTextBox.Text;
            _replaceDefaultText = replaceTextBox.Text;

            _quickSearchThrottle = Timer.Throttle(200, QuickSearch, this);

            inlineEditPanel.BackColor = Color.FromArgb(127, 0xca, 0xca, 0xca);
            inlineEditTextBox.Configure(true);

            HandleEscape(new Control[]
                {
                    logTextBox
                },
                new Action<KeyEventHandler>[]
                {
                    x => findTextBox.KeyDown += x,
                    x => replaceTextBox.KeyDown += x
                },
                new Action<Action<Keys>>[]
                {
                    x => mainToolbar.PreviewKeyDown += (sender, e) => x(e.KeyCode),
                    x => statusToolbar.PreviewKeyDown += (sender, e) => x(e.KeyCode)
                });

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
            openFileDialog.FileName = string.Empty;
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
            openProjectDialog.FileName = string.Empty;
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
                                if (!editor.Enabled) return;
                                App.Instance.UpdateUi();
                            };
                            editorPanel.Controls.Add(editor);

                            // re-enable panel after editor is configured
                            editor.Configure(false);

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
                async (project, document, position) =>
                {

                    projectOptionsItem.Enabled = project != null;

                    var projectStatus = project == null ? null : Path.GetFileName(project.FullPath) + " | ";
                    string documentStatus = null;

                    if (document != null)
                    {
                        documentStatus = $"{Path.GetFileName(document.FullPath)}";
                        // find section from current position
                        var sectionName = await App.Instance.FindSection(position);
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
                                if (logEntry.Contents.IgnoreCaseEqual("<clear>"))
                                {
                                    logTextBox.ClearAll();
                                    logTextBox.RemoveAllData();
                                    start = 0;
                                    continue;
                                }

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

                            logTextBox.GotoPosition(logTextBox.TextLength);
                        });
                    }
                });

            // handle menu states for current document
            App.Instance.Subscribe(
                x => x.Document?.Editor,
                editor =>
                {
                    diagramItem.Enabled =
                        quickSearchItem.Enabled =
                            inlineEditItem.Enabled =
                                showTemplatesItem.Enabled =
                                    goBackItem.Enabled =
                                        closeItem.Enabled =
                                            intelliSenseItem.Enabled =
                                                currentFileItem.Enabled =
                                                    gotoDeclarationItem.Enabled =
                                                        gotoLineItem.Enabled =
                                                            editor != null;
                }
            );


            inlineEditTextBox.KeyDown += (s, ee) =>
            {
                // dont close inline panel if user try to close autocomplete popup
                if (ee.KeyCode == Keys.Escape && !inlineEditTextBox.AutoCActive)
                {
                    _inlineEditData.Close();
                }
            };

            inlineEditTextBox.TextChanged += delegate
            {
                _inlineEditData?.Change(inlineEditTextBox.Text);
            };

            inlineEditTextBox.Leave += delegate
            {
                _inlineEditData.Close();
            };

            App.Instance.Subscribe(
                x => x.InlineEdit,
                inlineEditData =>
                {
                    if (inlineEditData != null)
                    {
                        _inlineEditData = inlineEditData;
                        inlineEditTextBox.AddData(0, 0, EditorExtraDataType.InlineEdit, inlineEditData);
                        inlineEditTextBox.Text = inlineEditData.Text;
                        _inlineEditData.Changed = false;
                        inlineEditPanel.Show();
                        inlineEditTextBox.Focus();
                    }
                    else
                    {
                        inlineEditPanel.Hide();
                        inlineEditTextBox.EmptyUndoBuffer();
                    }
                });

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
            var currentLine = App.Instance.GetCurrentLine();
            if (currentLine == -1) return;
            SetQuickSearchQuery(currentLine.ToString(), "Enter line number");
        }

        private void SetQuickSearchQuery(string query, string helpText = null)
        {
            // save current position first
            App.Instance.SavePosition();

            findTextBox.Text = "?" + query;
            findTextBox.Focus();
            findTextBox.SelectionStart = 1;
            findTextBox.SelectionLength = query.Length;

            if (!string.IsNullOrWhiteSpace(helpText))
            {
                help.SetHelpString(statusLabel, helpText);
                help.SetShowHelp(statusLabel, true);
            }
        }

        private void gotoDeclaration_Click(object sender, EventArgs e)
        {
            App.Instance.DocumentReady(d =>
            {
                var word = d.Editor.GetCurrentWord();

                App.Instance.GoToDeclaration(word, d.Editor.CurrentPosition, false, (data, @goto) =>
                {
                    d.Editor.AutoComplete(0, true, data).Then(@goto);
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
            saveFileDialog.FileName = string.Empty;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                App.Instance.CreateDocument(saveFileDialog.FileName);
            }
        }

        private void newProjectItem_Click(object sender, EventArgs e)
        {
            saveProjectDialog.FileName = string.Empty;
            if (saveProjectDialog.ShowDialog() == DialogResult.OK)
            {
                App.Instance.CreateProject(saveProjectDialog.FileName);
            }
        }

        private void findItem_Click(object sender, EventArgs e)
        {
            var selectedText = App.Instance.GetSelectedText();
            findTextBox.Text = selectedText;
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
            if (findTextBox.Text.TrimStart().StartsWith("?"))
            {
                findTextBox.Text = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(findTextBox.Text))
            {
                findTextBox.Text = _findDefaultText;
            }
        }

        private void QuickSearch()
        {
            var term = findTextBox.Text.Trim().TrimStart('?');
            App.Instance.QuickSearch(term, logTextBox);
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
                        if (d.Data is AData.Location location)
                        {
                            App.Instance.LoadDocument(App.FilePath(location.FileId)).Ready.Then(x =>
                            {
                                x.Editor.SetSelection(location.Position, location.EndPosition);
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

        private void intelliSenseItem_Click(object sender, EventArgs e)
        {
            App.Instance.Intellisense();
        }

        private void replaceItem_Click(object sender, EventArgs e)
        {
            var selectedText = App.Instance.GetSelectedText();
            findTextBox.Text = selectedText;
            replaceTextBox.Focus();
            replaceTextBox.Select();
        }

        private void closeItem_Click(object sender, EventArgs e)
        {
            App.Instance.CloseDocument();
        }

        private void showTemplatesItem_Click(object sender, EventArgs e)
        {
            App.Instance.ShowTemplates();
        }

        private void findTextBox_TextChanged(object sender, EventArgs e)
        {
            if (findTextBox.Text.TrimStart().StartsWith("?"))
            {
                _quickSearchThrottle();
            }
        }

        private void quickSearchItem_Click(object sender, EventArgs e)
        {
            SetQuickSearchQuery(string.Empty);
        }

        private void goBackItem_Click(object sender, EventArgs e)
        {
            App.Instance.GoBack();
        }

        private void inlineEditItem_Click(object sender, EventArgs e)
        {
            App.Instance.InlineEdit();
        }

        private static void HandleEscape(IEnumerable<Control> controls = null, Action<KeyEventHandler>[] keyEventRegisterActions = null, IEnumerable<Action<Action<Keys>>> keyActions = null)
        {
            void ProcessKeyCode(Keys keys)
            {
                if (keys == Keys.Escape)
                {
                    App.Instance.ActivateEditor();
                }
            }

            void ProcessKeyDown(object sender, KeyEventArgs e)
            {
                ProcessKeyCode(e.KeyCode);
            }

            foreach (var control in controls ?? Enumerable.Empty<Control>())
            {
                control.KeyDown += ProcessKeyDown;
            }

            foreach (var keyEventRegisterAction in keyEventRegisterActions ?? Enumerable.Empty<Action<KeyEventHandler>>())
            {
                keyEventRegisterAction(ProcessKeyDown);
            }

            foreach (var action in keyActions ?? Enumerable.Empty<Action<Action<Keys>>>())
            {
                action(ProcessKeyCode);
            }
        }

        private void diagramItem_Click(object sender, EventArgs e)
        {
            if (findTextBox.Focused)
            {
                var word = findTextBox.Text;
                findTextBox.Text = string.Empty;
                App.Instance.Diagram(word);
            }
            else
            {
                App.Instance.Diagram();
            }
        }

        private void projectOptionsItem_Click(object sender, EventArgs e)
        {
            App.Instance.EditProject();
        }
    }
}