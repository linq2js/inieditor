namespace IniEditor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainToolbar = new System.Windows.Forms.ToolStrip();
            this.fileItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.newItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.newProjectItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.quitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.findItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoLineItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoDeclarationItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearLogsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTextBox = new ScintillaNET.Scintilla();
            this.bottomSplitter = new System.Windows.Forms.Splitter();
            this.editorPanel = new System.Windows.Forms.Panel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveProjectDialog = new System.Windows.Forms.SaveFileDialog();
            this.openProjectDialog = new System.Windows.Forms.OpenFileDialog();
            this.statusToolbar = new System.Windows.Forms.ToolStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripLabel();
            this.replaceDropdown = new System.Windows.Forms.ToolStripSplitButton();
            this.replaceNextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceAllItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceAllCurrentFileItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceAllOpenedFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceAllAllFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceTextBox = new System.Windows.Forms.ToolStripComboBox();
            this.findDropdown = new System.Windows.Forms.ToolStripSplitButton();
            this.findNextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findPreviousItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAllItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentFileItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openedFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearHighlightsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.matchCaseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wholeWordItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wordStartItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.searchTypeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.standardItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendedItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regexItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markLineItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightMatchesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findTextBox = new System.Windows.Forms.ToolStripComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainToolbar.SuspendLayout();
            this.statusToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainToolbar
            // 
            this.mainToolbar.BackColor = System.Drawing.Color.Transparent;
            this.mainToolbar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainToolbar.GripMargin = new System.Windows.Forms.Padding(0);
            this.mainToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileItem,
            this.editItem});
            this.mainToolbar.Location = new System.Drawing.Point(0, 0);
            this.mainToolbar.Name = "mainToolbar";
            this.mainToolbar.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.mainToolbar.Size = new System.Drawing.Size(800, 25);
            this.mainToolbar.TabIndex = 0;
            // 
            // fileItem
            // 
            this.fileItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newItem,
            this.openItem,
            this.toolStripMenuItem1,
            this.newProjectItem,
            this.openProjectItem,
            this.projectFilesItem,
            this.toolStripMenuItem2,
            this.quitItem});
            this.fileItem.Image = ((System.Drawing.Image)(resources.GetObject("fileItem.Image")));
            this.fileItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileItem.Name = "fileItem";
            this.fileItem.Size = new System.Drawing.Size(38, 22);
            this.fileItem.Text = "&File";
            // 
            // newItem
            // 
            this.newItem.Name = "newItem";
            this.newItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newItem.Size = new System.Drawing.Size(218, 22);
            this.newItem.Text = "&New";
            this.newItem.Click += new System.EventHandler(this.newItem_Click);
            // 
            // openItem
            // 
            this.openItem.Name = "openItem";
            this.openItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openItem.Size = new System.Drawing.Size(218, 22);
            this.openItem.Text = "&Open";
            this.openItem.Click += new System.EventHandler(this.openItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(215, 6);
            // 
            // newProjectItem
            // 
            this.newProjectItem.Name = "newProjectItem";
            this.newProjectItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
            this.newProjectItem.Size = new System.Drawing.Size(218, 22);
            this.newProjectItem.Text = "New Project";
            this.newProjectItem.Click += new System.EventHandler(this.newProjectItem_Click);
            // 
            // openProjectItem
            // 
            this.openProjectItem.Name = "openProjectItem";
            this.openProjectItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.openProjectItem.Size = new System.Drawing.Size(218, 22);
            this.openProjectItem.Text = "Open Project";
            this.openProjectItem.Click += new System.EventHandler(this.openProjectItem_Click);
            // 
            // projectFilesItem
            // 
            this.projectFilesItem.Name = "projectFilesItem";
            this.projectFilesItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D)));
            this.projectFilesItem.Size = new System.Drawing.Size(218, 22);
            this.projectFilesItem.Text = "Project Files";
            this.projectFilesItem.Click += new System.EventHandler(this.projectFilesItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(215, 6);
            // 
            // quitItem
            // 
            this.quitItem.Name = "quitItem";
            this.quitItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quitItem.Size = new System.Drawing.Size(218, 22);
            this.quitItem.Text = "&Quit";
            this.quitItem.Click += new System.EventHandler(this.quitItem_Click);
            // 
            // editItem
            // 
            this.editItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.editItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findItem,
            this.gotoLineItem,
            this.gotoDeclarationItem,
            this.clearLogsItem});
            this.editItem.Image = ((System.Drawing.Image)(resources.GetObject("editItem.Image")));
            this.editItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.editItem.Name = "editItem";
            this.editItem.Size = new System.Drawing.Size(40, 22);
            this.editItem.Text = "&Edit";
            // 
            // findItem
            // 
            this.findItem.Name = "findItem";
            this.findItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findItem.Size = new System.Drawing.Size(206, 22);
            this.findItem.Text = "&Find";
            this.findItem.Click += new System.EventHandler(this.findItem_Click);
            // 
            // gotoLineItem
            // 
            this.gotoLineItem.Name = "gotoLineItem";
            this.gotoLineItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.gotoLineItem.Size = new System.Drawing.Size(206, 22);
            this.gotoLineItem.Text = "&Go To Line";
            this.gotoLineItem.Click += new System.EventHandler(this.gotoLineItem_Click);
            // 
            // gotoDeclarationItem
            // 
            this.gotoDeclarationItem.Name = "gotoDeclarationItem";
            this.gotoDeclarationItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.J)));
            this.gotoDeclarationItem.Size = new System.Drawing.Size(206, 22);
            this.gotoDeclarationItem.Text = "Go To &Declaration";
            this.gotoDeclarationItem.Click += new System.EventHandler(this.gotoDeclaration_Click);
            // 
            // clearLogsItem
            // 
            this.clearLogsItem.Name = "clearLogsItem";
            this.clearLogsItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.clearLogsItem.Size = new System.Drawing.Size(206, 22);
            this.clearLogsItem.Text = "&Clear Logs";
            this.clearLogsItem.Click += new System.EventHandler(this.clearLogsItem_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logTextBox.Lexer = ScintillaNET.Lexer.PowerShell;
            this.logTextBox.Location = new System.Drawing.Point(0, 415);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.Size = new System.Drawing.Size(800, 185);
            this.logTextBox.TabIndex = 1;
            this.logTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.logTextBox_MouseClick);
            // 
            // bottomSplitter
            // 
            this.bottomSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomSplitter.Location = new System.Drawing.Point(0, 412);
            this.bottomSplitter.MinExtra = 150;
            this.bottomSplitter.MinSize = 150;
            this.bottomSplitter.Name = "bottomSplitter";
            this.bottomSplitter.Size = new System.Drawing.Size(800, 3);
            this.bottomSplitter.TabIndex = 2;
            this.bottomSplitter.TabStop = false;
            // 
            // editorPanel
            // 
            this.editorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorPanel.Location = new System.Drawing.Point(0, 55);
            this.editorPanel.Name = "editorPanel";
            this.editorPanel.Size = new System.Drawing.Size(800, 357);
            this.editorPanel.TabIndex = 5;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "INI File (*.ini)|*.ini";
            this.openFileDialog.Multiselect = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "INI File (*.ini)|*.ini";
            // 
            // saveProjectDialog
            // 
            this.saveProjectDialog.Filter = "INI Project (*.inip)|*.inip";
            // 
            // openProjectDialog
            // 
            this.openProjectDialog.Filter = "INI Project (*.inip)|*.inip";
            // 
            // statusToolbar
            // 
            this.statusToolbar.BackColor = System.Drawing.Color.Transparent;
            this.statusToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.statusToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.replaceDropdown,
            this.replaceTextBox,
            this.findDropdown,
            this.toolStripDropDownButton1,
            this.findTextBox});
            this.statusToolbar.Location = new System.Drawing.Point(0, 25);
            this.statusToolbar.Name = "statusToolbar";
            this.statusToolbar.Padding = new System.Windows.Forms.Padding(10, 0, 5, 0);
            this.statusToolbar.Size = new System.Drawing.Size(800, 25);
            this.statusToolbar.TabIndex = 6;
            this.statusToolbar.Text = "s";
            // 
            // statusLabel
            // 
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(117, 22);
            this.statusLabel.Text = "Document Title";
            // 
            // replaceDropdown
            // 
            this.replaceDropdown.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.replaceDropdown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.replaceDropdown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.replaceNextItem,
            this.replaceAllItem});
            this.replaceDropdown.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.replaceDropdown.Image = ((System.Drawing.Image)(resources.GetObject("replaceDropdown.Image")));
            this.replaceDropdown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.replaceDropdown.Name = "replaceDropdown";
            this.replaceDropdown.Size = new System.Drawing.Size(33, 22);
            this.replaceDropdown.Text = "";
            this.replaceDropdown.ToolTipText = "Replace";
            this.replaceDropdown.ButtonClick += new System.EventHandler(this.replaceDropdown_ButtonClick);
            // 
            // replaceNextItem
            // 
            this.replaceNextItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.replaceNextItem.Name = "replaceNextItem";
            this.replaceNextItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.replaceNextItem.Size = new System.Drawing.Size(181, 22);
            this.replaceNextItem.Text = "Replace &Next";
            this.replaceNextItem.Click += new System.EventHandler(this.replaceNextItem_Click);
            // 
            // replaceAllItem
            // 
            this.replaceAllItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.replaceAllCurrentFileItem,
            this.replaceAllOpenedFilesItem,
            this.replaceAllAllFilesItem});
            this.replaceAllItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.replaceAllItem.Name = "replaceAllItem";
            this.replaceAllItem.Size = new System.Drawing.Size(181, 22);
            this.replaceAllItem.Text = "Replace &All";
            // 
            // replaceAllCurrentFileItem
            // 
            this.replaceAllCurrentFileItem.Name = "replaceAllCurrentFileItem";
            this.replaceAllCurrentFileItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
            this.replaceAllCurrentFileItem.Size = new System.Drawing.Size(238, 22);
            this.replaceAllCurrentFileItem.Text = "Current File";
            this.replaceAllCurrentFileItem.Click += new System.EventHandler(this.replaceAllCurrentFileItem_Click);
            // 
            // replaceAllOpenedFilesItem
            // 
            this.replaceAllOpenedFilesItem.Name = "replaceAllOpenedFilesItem";
            this.replaceAllOpenedFilesItem.ShortcutKeys = ((System.Windows.Forms.Keys)((((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
            this.replaceAllOpenedFilesItem.Size = new System.Drawing.Size(238, 22);
            this.replaceAllOpenedFilesItem.Text = "Opened Files";
            this.replaceAllOpenedFilesItem.Click += new System.EventHandler(this.replaceAllOpenedFilesItem_Click);
            // 
            // replaceAllAllFilesItem
            // 
            this.replaceAllAllFilesItem.Name = "replaceAllAllFilesItem";
            this.replaceAllAllFilesItem.Size = new System.Drawing.Size(238, 22);
            this.replaceAllAllFilesItem.Text = "All Files";
            this.replaceAllAllFilesItem.Click += new System.EventHandler(this.replaceAllAllFilesItem_Click);
            // 
            // replaceTextBox
            // 
            this.replaceTextBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.replaceTextBox.Name = "replaceTextBox";
            this.replaceTextBox.Size = new System.Drawing.Size(150, 25);
            this.replaceTextBox.Text = "Replace...";
            this.replaceTextBox.Enter += new System.EventHandler(this.replaceTextBox_Enter);
            this.replaceTextBox.Leave += new System.EventHandler(this.replaceTextBox_Leave);
            this.replaceTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.replaceTextBox_KeyDown);
            // 
            // findDropdown
            // 
            this.findDropdown.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.findDropdown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.findDropdown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findNextItem,
            this.findPreviousItem,
            this.findAllItem,
            this.clearHighlightsItem});
            this.findDropdown.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.findDropdown.Image = ((System.Drawing.Image)(resources.GetObject("findDropdown.Image")));
            this.findDropdown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.findDropdown.Name = "findDropdown";
            this.findDropdown.Size = new System.Drawing.Size(33, 22);
            this.findDropdown.Text = "";
            this.findDropdown.ToolTipText = "Find";
            this.findDropdown.ButtonClick += new System.EventHandler(this.findDropdown_ButtonClick);
            // 
            // findNextItem
            // 
            this.findNextItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.findNextItem.Name = "findNextItem";
            this.findNextItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findNextItem.Size = new System.Drawing.Size(194, 22);
            this.findNextItem.Text = "Find &Next";
            this.findNextItem.Click += new System.EventHandler(this.findNextItem_Click);
            // 
            // findPreviousItem
            // 
            this.findPreviousItem.Enabled = false;
            this.findPreviousItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.findPreviousItem.Name = "findPreviousItem";
            this.findPreviousItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F3)));
            this.findPreviousItem.Size = new System.Drawing.Size(194, 22);
            this.findPreviousItem.Text = "Find &Previous";
            this.findPreviousItem.Visible = false;
            this.findPreviousItem.Click += new System.EventHandler(this.findPreviousItem_Click);
            // 
            // findAllItem
            // 
            this.findAllItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentFileItem,
            this.openedFilesItem,
            this.allFilesItem});
            this.findAllItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.findAllItem.Name = "findAllItem";
            this.findAllItem.Size = new System.Drawing.Size(194, 22);
            this.findAllItem.Text = "Find &All";
            this.findAllItem.Click += new System.EventHandler(this.findAllItem_Click);
            // 
            // currentFileItem
            // 
            this.currentFileItem.Name = "currentFileItem";
            this.currentFileItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.currentFileItem.Size = new System.Drawing.Size(237, 22);
            this.currentFileItem.Text = "&Current File";
            this.currentFileItem.Click += new System.EventHandler(this.currentFileItem_Click);
            // 
            // openedFilesItem
            // 
            this.openedFilesItem.Name = "openedFilesItem";
            this.openedFilesItem.ShortcutKeys = ((System.Windows.Forms.Keys)((((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.openedFilesItem.Size = new System.Drawing.Size(237, 22);
            this.openedFilesItem.Text = "&Opened Files";
            this.openedFilesItem.Click += new System.EventHandler(this.openedFilesItem_Click);
            // 
            // allFilesItem
            // 
            this.allFilesItem.Name = "allFilesItem";
            this.allFilesItem.Size = new System.Drawing.Size(237, 22);
            this.allFilesItem.Text = "&All Files";
            this.allFilesItem.Click += new System.EventHandler(this.allFilesItem_Click);
            // 
            // clearHighlightsItem
            // 
            this.clearHighlightsItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.clearHighlightsItem.Name = "clearHighlightsItem";
            this.clearHighlightsItem.Size = new System.Drawing.Size(194, 22);
            this.clearHighlightsItem.Text = "Clear Highlights";
            this.clearHighlightsItem.Visible = false;
            this.clearHighlightsItem.Click += new System.EventHandler(this.clearHighlightsItem_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.matchCaseItem,
            this.wholeWordItem,
            this.wordStartItem,
            this.toolStripMenuItem3,
            this.searchTypeItem,
            this.findAllToolStripMenuItem});
            this.toolStripDropDownButton1.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(30, 22);
            this.toolStripDropDownButton1.Text = "";
            this.toolStripDropDownButton1.ToolTipText = "Search Options";
            // 
            // matchCaseItem
            // 
            this.matchCaseItem.CheckOnClick = true;
            this.matchCaseItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.matchCaseItem.Name = "matchCaseItem";
            this.matchCaseItem.Size = new System.Drawing.Size(138, 22);
            this.matchCaseItem.Text = "Match &Case";
            // 
            // wholeWordItem
            // 
            this.wholeWordItem.CheckOnClick = true;
            this.wholeWordItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wholeWordItem.Name = "wholeWordItem";
            this.wholeWordItem.Size = new System.Drawing.Size(138, 22);
            this.wholeWordItem.Text = "&Whole Word";
            // 
            // wordStartItem
            // 
            this.wordStartItem.CheckOnClick = true;
            this.wordStartItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wordStartItem.Name = "wordStartItem";
            this.wordStartItem.Size = new System.Drawing.Size(138, 22);
            this.wordStartItem.Text = "Word &Start";
            this.wordStartItem.Visible = false;
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(135, 6);
            // 
            // searchTypeItem
            // 
            this.searchTypeItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.standardItem,
            this.extendedItem,
            this.regexItem});
            this.searchTypeItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchTypeItem.Name = "searchTypeItem";
            this.searchTypeItem.Size = new System.Drawing.Size(138, 22);
            this.searchTypeItem.Text = "Search &Type";
            // 
            // standardItem
            // 
            this.standardItem.Checked = true;
            this.standardItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.standardItem.Name = "standardItem";
            this.standardItem.Size = new System.Drawing.Size(175, 22);
            this.standardItem.Text = "Standard";
            this.standardItem.Click += new System.EventHandler(this.standardItem_Click);
            // 
            // extendedItem
            // 
            this.extendedItem.Name = "extendedItem";
            this.extendedItem.Size = new System.Drawing.Size(175, 22);
            this.extendedItem.Text = "Extended (\\r, \\n, \\t)";
            this.extendedItem.Click += new System.EventHandler(this.extendedItem_Click);
            // 
            // regexItem
            // 
            this.regexItem.Name = "regexItem";
            this.regexItem.Size = new System.Drawing.Size(175, 22);
            this.regexItem.Text = "Regular Expression";
            this.regexItem.Click += new System.EventHandler(this.regexItem_Click);
            // 
            // findAllToolStripMenuItem
            // 
            this.findAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.markLineItem,
            this.highlightMatchesItem});
            this.findAllToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.findAllToolStripMenuItem.Name = "findAllToolStripMenuItem";
            this.findAllToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.findAllToolStripMenuItem.Text = "Find &All";
            this.findAllToolStripMenuItem.Visible = false;
            // 
            // markLineItem
            // 
            this.markLineItem.CheckOnClick = true;
            this.markLineItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.markLineItem.Name = "markLineItem";
            this.markLineItem.Size = new System.Drawing.Size(172, 22);
            this.markLineItem.Text = "Mark &Line";
            // 
            // highlightMatchesItem
            // 
            this.highlightMatchesItem.CheckOnClick = true;
            this.highlightMatchesItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.highlightMatchesItem.Name = "highlightMatchesItem";
            this.highlightMatchesItem.Size = new System.Drawing.Size(172, 22);
            this.highlightMatchesItem.Text = "&Highlight Matches";
            // 
            // findTextBox
            // 
            this.findTextBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.findTextBox.Name = "findTextBox";
            this.findTextBox.Size = new System.Drawing.Size(150, 25);
            this.findTextBox.Text = "Find...";
            this.findTextBox.Enter += new System.EventHandler(this.findTextBox_Enter);
            this.findTextBox.Leave += new System.EventHandler(this.findTextBox_Leave);
            this.findTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.findTextBox_KeyDown);
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 5);
            this.panel1.TabIndex = 7;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.editorPanel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.bottomSplitter);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.statusToolbar);
            this.Controls.Add(this.mainToolbar);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "INI Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainToolbar.ResumeLayout(false);
            this.mainToolbar.PerformLayout();
            this.statusToolbar.ResumeLayout(false);
            this.statusToolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip mainToolbar;
        private System.Windows.Forms.ToolStripDropDownButton fileItem;
        private System.Windows.Forms.ToolStripDropDownButton editItem;
        private System.Windows.Forms.ToolStripMenuItem newItem;
        private System.Windows.Forms.ToolStripMenuItem openItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newProjectItem;
        private System.Windows.Forms.ToolStripMenuItem openProjectItem;
        private System.Windows.Forms.ToolStripMenuItem projectFilesItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem quitItem;
        private ScintillaNET.Scintilla logTextBox;
        private System.Windows.Forms.Splitter bottomSplitter;
        private System.Windows.Forms.Panel editorPanel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.SaveFileDialog saveProjectDialog;
        private System.Windows.Forms.OpenFileDialog openProjectDialog;
        private System.Windows.Forms.ToolStripMenuItem findItem;
        private System.Windows.Forms.ToolStripMenuItem gotoLineItem;
        private System.Windows.Forms.ToolStripMenuItem clearLogsItem;
        private System.Windows.Forms.ToolStripMenuItem gotoDeclarationItem;
        private System.Windows.Forms.ToolStrip statusToolbar;
        private System.Windows.Forms.ToolStripLabel statusLabel;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem matchCaseItem;
        private System.Windows.Forms.ToolStripMenuItem wholeWordItem;
        private System.Windows.Forms.ToolStripMenuItem wordStartItem;
        private System.Windows.Forms.ToolStripSplitButton findDropdown;
        private System.Windows.Forms.ToolStripSplitButton replaceDropdown;
        private System.Windows.Forms.ToolStripMenuItem findNextItem;
        private System.Windows.Forms.ToolStripMenuItem findPreviousItem;
        private System.Windows.Forms.ToolStripMenuItem findAllItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem replaceNextItem;
        private System.Windows.Forms.ToolStripMenuItem replaceAllItem;
        private System.Windows.Forms.ToolStripComboBox findTextBox;
        private System.Windows.Forms.ToolStripComboBox replaceTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem searchTypeItem;
        private System.Windows.Forms.ToolStripMenuItem standardItem;
        private System.Windows.Forms.ToolStripMenuItem extendedItem;
        private System.Windows.Forms.ToolStripMenuItem regexItem;
        private System.Windows.Forms.ToolStripMenuItem findAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markLineItem;
        private System.Windows.Forms.ToolStripMenuItem highlightMatchesItem;
        private System.Windows.Forms.ToolStripMenuItem clearHighlightsItem;
        private System.Windows.Forms.ToolStripMenuItem openedFilesItem;
        private System.Windows.Forms.ToolStripMenuItem allFilesItem;
        private System.Windows.Forms.ToolStripMenuItem currentFileItem;
        private System.Windows.Forms.ToolStripMenuItem replaceAllCurrentFileItem;
        private System.Windows.Forms.ToolStripMenuItem replaceAllOpenedFilesItem;
        private System.Windows.Forms.ToolStripMenuItem replaceAllAllFilesItem;
    }
}

