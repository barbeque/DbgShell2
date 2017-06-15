using DbgShell;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DbgShellUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void openDump_Click(object sender, EventArgs e)
        {
            var dbgPath = Settings.Default.DbgPath;
            if (string.IsNullOrEmpty(dbgPath))
            {
                MessageBox.Show("DbgPath appears to be missing/blank in the settings");
                return;
            }
            if (!(openFileDialog.ShowDialog() == DialogResult.OK))
            {
                return;
            }
            ProcessStartInfo cdbInfo = new ProcessStartInfo();
            cdbInfo.FileName = Path.Combine(dbgPath, "cdb.exe");
            cdbInfo.CreateNoWindow = true;
            cdbInfo.RedirectStandardInput = true;
            cdbInfo.RedirectStandardOutput = true;
            cdbInfo.WorkingDirectory = dbgPath;
            cdbInfo.UseShellExecute = false;
            cdbInfo.Arguments = "-z \"" + openFileDialog.FileName + "\"";

            if(!File.Exists(cdbInfo.FileName))
            {
                MessageBox.Show($"Can't find cdb at '{cdbInfo.FileName}'. Consider changing the DbgPath setting to the place where cdb.exe lives on your system.");
                return;
            }

            Dbg.Init(Process.Start(cdbInfo));
            Dbg.Execute(".echo hello"); //removes startup output
            foreach (string cmd in Settings.Default.Startup)
            {
                Execute(cmd);
            }

            reloadMenuItem_Click(sender, e);
            dbgPanel.Visible = true;
            RefreshContext();
        }

        private void ShowBusy()
        {
            contextLabel.Text = "Busy:";
            contextLabel.Refresh();
        }

        private void Execute(string cmd)
        {
            ShowBusy();
            try
            {
                if (cmd == ".cls")
                {
                    output.Clear();
                }
                else if (cmd.StartsWith("Run "))
                {
                    var commands = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (commands.Length < 3)
                    {
                        output.AddText("Invalid command." + cmd + "\n");
                        return;
                    }
                    var prms = new string[commands.Length - 3];
                    if (prms.Length > 0)
                    {
                        Array.Copy(commands, 2, prms, 0, prms.Length);
                    }
                    RunScript(commands[1], commands[2], prms);
                }
                else
                {
                    output.AddText(cmd + "\n");
                    output.AddText(Dbg.Execute(cmd));
                    output.ScrollToEnd();
                }
            }
            finally
            {
                RefreshContext();
            }
        }

        private void RunScript(string typeName, string methodName, params string[] parameters)
        {
            ShowBusy();
            try
            {

                var cmd = $"Run {typeName} {methodName}";
                if (parameters != null)
                {
                    foreach (string param in parameters)
                    {
                        cmd += " " + param;
                    }
                }
                output.AddText(cmd + "\n");
                string res = ScriptsApi.Run(typeName, methodName, parameters);
                output.AddOutput(res);
                output.ScrollToEnd();
            }
            finally
            {
                RefreshContext();
            }
        }

        const int MaxHistory = 100;
        int autoCompleteIndex = -1;
        private void commandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
/*                commandTextBox.AutoCompleteCustomSource.Remove(commandTextBox.Text);
                if (commandTextBox.AutoCompleteCustomSource.Count > MaxHistory)
                {
                    commandTextBox.AutoCompleteCustomSource.RemoveAt(0);
                }
                commandTextBox.AutoCompleteCustomSource.Add(commandTextBox.Text);
                Execute(commandTextBox.Text);
                commandTextBox.Text = string.Empty;
                e.Handled = true;
                autoCompleteIndex = -1;*/
            }
            if (e.KeyCode == Keys.Up)
            {
                ShowHistoryCmd();
                autoCompleteIndex--;
            }
            if (e.KeyCode == Keys.Down)
            {
                ShowHistoryCmd();
                autoCompleteIndex++;
            }
            if (e.KeyCode == Keys.Escape)
            {
                commandTextBox.Text = string.Empty;
                autoCompleteIndex = -1;
            }
        }

        private void ShowHistoryCmd()
        {
            if (commandTextBox.AutoCompleteCustomSource.Count == 0)
            {
                return;
            }
            int index = autoCompleteIndex % commandTextBox.AutoCompleteCustomSource.Count;
            if (index < 0)
            {
                index += commandTextBox.AutoCompleteCustomSource.Count;
            }
            commandTextBox.Text = commandTextBox.AutoCompleteCustomSource[index];
        }

        private void RefreshContext()
        {
            contextLabel.Text = Dbg.Context;
        }

        private void typesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            methodsCombo.Items.Clear();
            methodsCombo.Items.AddRange(ScriptsApi.GetMethods((string)typesCombo.SelectedItem).ToArray());
        }

        private void runScript_Click(object sender, EventArgs e)
        {
            var prms = paramsTextBox.Text.Split(new string[] { " " }, StringSplitOptions.None);

            try
            {
                this.Cursor = Cursors.WaitCursor;

                RunScript(typesCombo.Text, methodsCombo.Text, prms);
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void unloadMenuItem_Click(object sender, EventArgs e)
        {
            ScriptsApi.Unload();

            typesCombo.Items.Clear();
        }

        private void reloadMenuItem_Click(object sender, EventArgs e)
        {
            ScriptsApi.Load();

            typesCombo.Items.Clear();
            typesCombo.Items.AddRange(ScriptsApi.GetTypes().ToArray());
        }

        private void output_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            int cmdStart = e.LinkText.IndexOf('#');
            if (cmdStart == -1)
            {
                return;
            }
            string linkCommand = e.LinkText.Substring(cmdStart + 1);
            if (linkCommand.StartsWith(RichTextBoxEx.DbgPrefix))
            {
                linkCommand = linkCommand.Substring(RichTextBoxEx.DbgPrefix.Length);
                Execute(linkCommand);
            }
            else if (linkCommand.StartsWith(RichTextBoxEx.ScriptPrefix))
            {
                linkCommand = linkCommand.Substring(RichTextBoxEx.ScriptPrefix.Length);
                var commands = linkCommand.Split(new string[] {RichTextBoxEx.Separator}, StringSplitOptions.None);
                if (commands.Length < 2)
                {
                    return;
                }
                var prms = new string[commands.Length - 2];
                if (prms.Length > 0)
                {
                    Array.Copy(commands, 2, prms, 0, prms.Length);
                }
                RunScript(commands[0], commands[1], prms);
            }
        }

        private void clearOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            output.Clear();
        }

        private void runCmdBtn_Click(object sender, EventArgs e)
        {
            commandTextBox.AutoCompleteCustomSource.Remove(commandTextBox.Text);
            if (commandTextBox.AutoCompleteCustomSource.Count > MaxHistory)
            {
                commandTextBox.AutoCompleteCustomSource.RemoveAt(0);
            }
            commandTextBox.AutoCompleteCustomSource.Add(commandTextBox.Text);
            Execute(commandTextBox.Text);
            commandTextBox.Text = string.Empty;
            autoCompleteIndex = -1;
        }
    }
}
