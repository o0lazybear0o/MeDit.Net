using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace Medit.Net
{
    public partial class Form1 : Form
    {
        [DllImport("MDParser.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr ParseMarkdown(
            byte[] src,
            int len);

        public const string CSSClearness = ".\\CSS\\Clearness.css";
        public const string CSSClearnessDark = ".\\CSS\\ClearnessDark.css";
        public const string CSSGitHub = ".\\CSS\\GitHub.css";
        public const string CSSGitHub2 = ".\\CSS\\GitHub2.css";

        protected TextBox currentEditor;
        protected DocumentInfo currentDocument;
        protected string currentCSS = CSSGitHub;

        private string lastKeyword;

        public class DocumentInfo
        {
            public string FilePath;
            public string FileName;
            public string Title;
            public bool isModified;
            public TextBox editor;
            public TabPage tab;
            private Form1 frm;

            public DocumentInfo(Form1 frm)
            {
                this.frm = frm;
                this.FilePath = null;
                this.FileName = null;
                this.Title = "undefined";
                this.isModified = false;
                buildControls();
            }

            public DocumentInfo(Form1 frm, string path)
            {
                this.frm = frm;
                this.FilePath = path;
                this.FileName = Path.GetFileName(path);
                this.Title = this.FileName;
                this.isModified = false;
                buildControls();
            }

            private void buildControls()
            {
                this.editor = createMarkdownEditor();
                this.tab = createMarkdownTabPage();
                frm.tabs.TabPages.Add(this.tab);
            }

            public void close()
            {
                save();
                this.tab.Controls.Remove(this.editor);
                frm.tabs.TabPages.Remove(this.tab);
            }

            private void _save_file()
            {
                if (this.FileName == null)
                {
                    this.FilePath = frm.saveMarkdown(this.editor); // need to ask the location
                    if (this.FilePath == "") return; // save cancelled by user
                    this.FileName = Path.GetFileName(this.FilePath);
                    this.Title = this.FileName;
                }
                else
                {
                    try
                    {
                        File.WriteAllText(this.FilePath, this.editor.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to write to the file.\n" + ex.Message);
                        return;
                    }
                }
                isModified = false;
                this.tab.Text = this.Title;
            }

            public void save(bool noprompt = false)
            {
                if (noprompt)
                {
                    _save_file();
                }
                else if (isModified)
                {
                    switch (MessageBox.Show("Would you like to save the changes?", this.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        case DialogResult.Yes:
                            _save_file();
                            break;
                    }
                }
            }

            public void onChanged()
            {
                if (!isModified)
                {
                    isModified = true;
                    this.tab.Text = "* " + this.Title;
                }
            }

            protected TextBox createMarkdownEditor()
            {
                TextBox tb = new TextBox();
                tb.WordWrap = true;
                tb.Multiline = true;
                tb.ScrollBars = ScrollBars.Both;
                tb.HideSelection = false;
                tb.AcceptsTab = true;
                tb.AcceptsReturn = true;
                tb.Location = new Point(0, 0);
                tb.Dock = DockStyle.Fill;
                tb.Visible = true;
                tb.TextChanged += frm.Editor_onTextChanged;
                tb.Tag = this;
                return tb;
            }

            protected TabPage createMarkdownTabPage()
            {
                TabPage tp = new TabPage(this.Title);
                tp.Controls.Add(this.editor);
                tp.Tag = this;
                return tp;
            }
        };

        public Form1()
        {
            InitializeComponent();

        }

        public void Editor_onTextChanged(object sender, EventArgs e)
        {
            if (currentDocument != null)
            {
                if (previewToolStripMenuItem.Checked)
                    RefreshPreview();
                currentDocument.onChanged();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize WebView Control
            wbPreview.Navigate("about:blank");
            // Create new document
            DocumentInfo def = new DocumentInfo(this);
            setCurrentDocument(def.tab);
        }

        public void setCurrentDocument(TabPage tp)
        {
            if (tp == null) return;
            var doc = tp.Tag as DocumentInfo;
            tabs.SelectTab(tp);
            currentDocument = doc;
            currentEditor = doc.editor;
            currentEditor.Focus();
            RefreshPreview();
        }

        public void RefreshPreview()
        {
            PreviewMarkdown();
        }

        public string ParseMarkdown(string markdown, string cssfile)
        {
            // Convert source to utf-8
            byte[] input = Encoding.UTF8.GetBytes(markdown);

            // Parse
            IntPtr output = ParseMarkdown(input, input.Length);

            // Convert output to Unicode
            string html = Marshal.PtrToStringAnsi(output);
            html = Encoding.UTF8.GetString(Encoding.Default.GetBytes(html));

            // Generate HTML
            return "<!doctype html>\n<html>\n<head>"+"<link href=\"" + cssfile + "\" rel=\"stylesheet\" type=\"text/css\">" + "<style>\n</style>\n" + "</head>\n<body>" + html + "</body>\n</html>";
        }

        public string ParseCurrentMarkdown()
        {
            return ParseMarkdown(currentEditor.Text, currentCSS);
        }

        public void PreviewMarkdown()
        {
            // Generate HTML
            string html = ParseCurrentMarkdown();
            // Display HTML
            wbPreview.Document.OpenNew(true);
            wbPreview.Document.Write(html);
        }

        private void layoutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEditor.CanUndo)
                currentEditor.Undo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectedText = "";
        }

        private void wordWrapToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            currentEditor.WordWrap = wordWrapToolStripMenuItem.Checked;
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wordWrapToolStripMenuItem.Checked = !wordWrapToolStripMenuItem.Checked;
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            previewToolStripMenuItem.Checked = !previewToolStripMenuItem.Checked;
        }

        private void previewToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (previewToolStripMenuItem.Checked)
            {
                statusPanel.Text = "Real-Time Preview Enabled";
                RefreshPreview(); // refresh it immediately when enabling preview
            }
            else
            {
                statusPanel.Text = "Real-Time Preview Disabled";
            }
        }

        private void previewinbrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayHTMLInBrowser(ParseCurrentMarkdown());
        }

        public static void OpenFileInBrowser(string filepath)
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\"); 
            string s = key.GetValue("").ToString(); 
 
            Regex reg = new Regex("\"([^\"]+)\""); 
            MatchCollection matchs = reg.Matches(s); 
 
            string filename=""; 
            if (matchs.Count > 0)
            { 
                filename = matchs[0].Groups[1].Value; 
                System.Diagnostics.Process.Start(filename, filepath); 
            }
        }

        public static void DisplayHTMLInBrowser(string htmlcontent)
        {
            string TempFilePath = Path.GetTempFileName() + ".html";
            File.WriteAllBytes(TempFilePath, Encoding.UTF8.GetBytes(htmlcontent));
            OpenFileInBrowser(TempFilePath);
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            verticalToolStripMenuItem.Checked = true;
            horizontalToolStripMenuItem.Checked = false;
            splitContainer1.Orientation = Orientation.Vertical;
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            horizontalToolStripMenuItem.Checked = true;
            verticalToolStripMenuItem.Checked = false;
            splitContainer1.Orientation = Orientation.Horizontal;
        }

        private void clearnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeCSS(clearnessToolStripMenuItem, CSSClearness);
        }

        private void clearnessDarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeCSS(clearnessDarkToolStripMenuItem, CSSClearnessDark);
        }

        private void gitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeCSS(gitHubToolStripMenuItem, CSSGitHub);
        }

        private void gitHub2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeCSS(gitHub2ToolStripMenuItem, CSSGitHub2);
        }

        private void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openCSSDialog.ShowDialog();
            if (openCSSDialog.FileName != "") changeCSS(customToolStripMenuItem, openCSSDialog.FileName);
        }

        public void changeCSS(ToolStripMenuItem selectedItem, string selectedCSS)
        {
            foreach (ToolStripMenuItem menuItem in cSSToolStripMenuItem.DropDownItems)
                menuItem.Checked = (menuItem == selectedItem);
            currentCSS = selectedCSS;
            RefreshPreview();
            statusPanel.Text = "Selected CSS File: " + selectedCSS;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentInfo def = new DocumentInfo(this);
            setCurrentDocument(def.tab);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectAll();
        }

        private void copyHTMLStripMenuItem10_Click(object sender, EventArgs e)
        {
            wbPreview.Document.ExecCommand("SelectAll", false, null);
            wbPreview.Document.ExecCommand("Copy", false, null);
            wbPreview.Document.ExecCommand("Unselect", false, null);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            findTextInEditor(lastKeyword = Microsoft.VisualBasic.Interaction.InputBox("Text to find", "Find"), 0);
        }

        public bool findTextInEditor(string keyword, int startpos)
        {
            if (keyword == null || keyword == "") return false;
            int occurPos = currentEditor.Text.IndexOf(keyword, startpos);
            if (occurPos >= 0)
            {
                currentEditor.SelectionStart = occurPos;
                currentEditor.SelectionLength = keyword.Length;
                return true;
            }
            else
            {
                statusPanel.Text = keyword + " not found";
                return false;
            }
        }

        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lastKeyword != "")
                findTextInEditor(lastKeyword, currentEditor.SelectionStart + 1);
        }

        private void gotoLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int lineNumber = -1;
            try
            {
                lineNumber = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Line number: (starting from 1)", "GoTo Line")) - 1;
                currentEditor.SelectionStart = currentEditor.GetFirstCharIndexFromLine(lineNumber);
                currentEditor.SelectionLength = currentEditor.Lines[lineNumber].Length;
            }catch (Exception ex)
            {
                statusPanel.Text = "Line " + (lineNumber + 1).ToString() + " doesn't exist.";
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openMarkdownDialog.ShowDialog();
            if (openMarkdownDialog.FileName != "")
            {
                try
                {
                    using (var strm = openMarkdownDialog.OpenFile())
                    {
                    
                        byte[] data = new byte[strm.Length];
                        strm.Read(data, 0, (int)strm.Length);
                        var doc = new DocumentInfo(this, openMarkdownDialog.FileName);
                        doc.editor.Text = Encoding.UTF8.GetString(data);
                        setCurrentDocument(doc.tab);
                    }
                    statusPanel.Text = "Loaded successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to open file.\n" + ex.Message, "Open Markdown File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void exportHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveHTMLDialog.ShowDialog();
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(ParseCurrentMarkdown());
                using (var strm = saveHTMLDialog.OpenFile())
                {
                    strm.Write(data, 0, data.Length);
                }
                statusPanel.Text = "Export success";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save file.\n" + ex.Message, "Export HTML", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void printHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshPreview();
            wbPreview.ShowPrintDialog();
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEditor.SelectedText == "")
            {
                currentEditor.SelectedText = "**" + currentEditor.SelectedText + "**";
                currentEditor.SelectionStart -= 2;
            }
            else
            {
                currentEditor.SelectedText = "**" + currentEditor.SelectedText + "**";
            }
        }

        private void timestampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectedText = currentEditor.SelectedText + DateTime.Now.ToLocalTime().ToLongTimeString();
        }

        private void currentDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectedText = currentEditor.SelectedText + DateTime.Now.ToLocalTime().ToLongDateString();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveMarkdown(currentEditor);
        }

        protected string saveMarkdown(TextBox editor)
        {
            saveMarkdownDialog.ShowDialog();
            if (saveMarkdownDialog.FileName == "") return "";

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(editor.Text);
                using (var strm = saveMarkdownDialog.OpenFile())
                {
                    strm.Write(data, 0, data.Length);
                }
                statusPanel.Text = "Saved";
                return saveMarkdownDialog.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save file.\n" + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return "";
        }

        private void tabs_Selected(object sender, TabControlEventArgs e)
        {
            setCurrentDocument(e.TabPage);
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var item in editToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                    (item as ToolStripMenuItem).Enabled = (currentEditor != null);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentDocument != null)
            {
                currentDocument.close();
            }
        }

        private void tabs_Deselected(object sender, TabControlEventArgs e)
        {
            currentDocument = null;
            currentEditor = null;
        }

        private void insertToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var item in insertToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                    (item as ToolStripMenuItem).Enabled = (currentEditor != null);
            }

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentDocument.save(true);
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (currentDocument != null)
            {
                currentDocument.close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // prompt to save changes at exit
            closeAllToolStripMenuItem_Click(null, null);
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            saveToolStripMenuItem.Enabled = (currentDocument != null);
            saveAsToolStripMenuItem.Enabled = (currentDocument != null);
            closeToolStripMenuItem.Enabled = (currentDocument != null);
            printHTMLToolStripMenuItem.Enabled = (currentDocument != null);
            exportHTMLToolStripMenuItem.Enabled = (currentDocument != null);
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex + 1 < tabs.TabCount)
                setCurrentDocument(tabs.TabPages[tabs.SelectedIndex + 1]);
        }

        private void prevToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (tabs.SelectedIndex -1>=0)
                setCurrentDocument(tabs.TabPages[tabs.SelectedIndex - 1]);
        }

        private void emphasizeStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEditor.SelectedText == "")
            {
                currentEditor.SelectedText = "*" + currentEditor.SelectedText + "*";
                currentEditor.SelectionStart -= 1;
            }
            else
            {
                currentEditor.SelectedText = "*" + currentEditor.SelectedText + "*";
            }
        }

        private void inlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEditor.SelectedText == "")
            {
                currentEditor.SelectedText = "`" + currentEditor.SelectedText + "`";
                currentEditor.SelectionStart -= 1;
            }
            else
            {
                currentEditor.SelectedText = "`" + currentEditor.SelectedText + "`";
            }
        }

        private void linkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEditor.SelectedText == "")
            {
                currentEditor.SelectedText = "[Link](http://)";
                currentEditor.Select(currentEditor.SelectionStart - 8, 7);
            }
            else
            {
                currentEditor.SelectedText = "[" + currentEditor.SelectedText + "](http://)";
                currentEditor.Select(currentEditor.SelectionStart - 8, 7);
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEditor.SelectedText == "")
            {
                currentEditor.SelectedText = "![Link](http://)";
                currentEditor.Select(currentEditor.SelectionStart - 8, 7);
            }
            else
            {
                currentEditor.SelectedText = "![" + currentEditor.SelectedText + "](http://)";
                currentEditor.Select(currentEditor.SelectionStart - 8, 7);
            }
        }
    }

}
