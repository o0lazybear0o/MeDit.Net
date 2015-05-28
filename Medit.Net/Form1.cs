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
        protected string currentCSS = CSSGitHub;

        private string lastKeyword;

        public Form1()
        {
            InitializeComponent();

            currentEditor = this.tbSource;
            // Initialize WebView Control
            wbPreview.Navigate("about:blank");
        }

        private void tbSource_TextChanged(object sender, EventArgs e)
        {
            if (previewToolStripMenuItem.Checked)
                RefreshPreview();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
                RefreshPreview(); // refresh it immediately when enabling preview
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
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tbSource.SelectAll();
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
            int occurPos = currentEditor.Text.IndexOf(keyword, startpos);
            if (occurPos >= 0)
            {
                currentEditor.SelectionStart = occurPos;
                currentEditor.SelectionLength = keyword.Length;
                return true;
            }
            else
            {
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
            int lineNumber;
            try
            {
                lineNumber = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Line number: (starting from 1)", "GoTo Line")) - 1;
                currentEditor.SelectionStart = currentEditor.GetFirstCharIndexFromLine(lineNumber);
            }catch (Exception ex)
            {
            }
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save file.\n" + ex.Message, "Save Markdown File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: prompt to save changes
            Application.Exit();
        }

        private void printHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshPreview();
            wbPreview.ShowPrintDialog();
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectedText = "**" + currentEditor.SelectedText + "**";
        }

        private void timestampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectedText = currentEditor.SelectedText + DateTime.Now.ToLocalTime().ToLongTimeString();
        }

        private void currentDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentEditor.SelectedText = currentEditor.SelectedText + DateTime.Now.ToLocalTime().ToLongDateString();
        }
    }

}
