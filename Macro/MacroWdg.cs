﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Macro
{
  public partial class MacroWdg : Form
  {
    IntPtr _active;
    IntPtr _handle;

    private Process _proc;

    private string _processedColumn = "Processed";
    private string _nameColumn = "FileName";
    private string _fullNameColumn = "FullFileName";

    public MacroWdg()
    {
      InitializeComponent();

      SetStyle(ControlStyles.EnableNotifyMessage, true);

      DataTable tblFiles = new DataTable();
      DataColumn dataCol = new DataColumn(_processedColumn, typeof(CheckState));
      dataCol.DefaultValue = CheckState.Unchecked;
      tblFiles.Columns.Add(dataCol);
      tblFiles.Columns.Add(_nameColumn, typeof(string));
      tblFiles.Columns.Add(_fullNameColumn, typeof(string));
      DataView view = new DataView(tblFiles);
      view.AllowNew = false;

      dgvData.AutoGenerateColumns = false;
      dgvData.DataSource = view;
      dgvData.RowHeadersVisible = false;

      DataGridViewColumn col;
      DataGridViewCheckBoxCell chkCell = new DataGridViewCheckBoxCell();
      chkCell.ThreeState = true;
      col = new DataGridViewColumn(chkCell);
      col.DataPropertyName = _processedColumn;
      col.HeaderText = "Processed";
      col.Width = 40;
      col.ReadOnly = true;
      dgvData.Columns.Add(col);

      col = new DataGridViewTextBoxColumn();
      col.DataPropertyName = _nameColumn;
      col.HeaderText = "File Name";
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      dgvData.Columns.Add(col);
    }

    private string[] SetData(string dir)
    {
      if (_proc != null)
      {
        if (_proc.ProcessName == "AcroRd32")
        {
          dlgOpenData.Filter = "PDF files|*.pdf|All files|*.*";
        }
      }

      string[] filter = dlgOpenData.Filter.Split('|');
      string[] files = Directory.GetFiles(dir, filter[1].Trim());

      SetData(files);
      return files;
    }

    private void SetData(IList<string> fileNames)
    {
      DataTable tbl = ((DataView)dgvData.DataSource).Table;
      tbl.Clear();
      foreach (string fileName in fileNames)
      {
        tbl.Rows.Add(CheckState.Unchecked, Path.GetFileName(fileName), Path.GetFullPath(fileName));
      }
      optFiles.Checked = (fileNames.Count > 0);
    }

    public void Init(string[] args)
    {
      if (args.Length < 2)
      { return; }

      string macro = args[1];
      if (File.Exists(macro) == false)
      { return; }

      TextReader reader = new StreamReader(macro);
      string line;
      try
      {
        line = reader.ReadLine();
        while (line.StartsWith("#"))
        {
          string execName = line.Substring(1);
          Process proc = Macro.InitProcess(macro, execName);

          if (_proc == null)
          {
            _proc = proc;
            txtProgress.Items.Add(_proc.MainWindowHandle.ToString());
            chkActive.Checked = false;
          }

          line = reader.ReadLine();
        }
      }
      finally
      {
        reader.Close();
      }

      SetData(Environment.CurrentDirectory);

      txtMacro.Text = macro;
      TryEnableRun();
    }

    private void GetLastForegroundWindow()
    {
      IntPtr active = Macro.GetForegroundWindow();
      if (active != IntPtr.Zero && active != _handle)
      { _active = active; }

      txtProgress.Items.Add(string.Format("Handle of last activated window: {0}", _active));
    }

    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
    {
      Console.WriteLine(string.Format("{0} {1} {2} {3} {4}",
        e.KeyCode, e.KeyData, e.KeyValue, e.Modifiers, e.IsInputKey));
      base.OnPreviewKeyDown(e);
    }
    protected override void OnNotifyMessage(Message m)
    {
      if (chkActive.Checked)
      {
        GetLastForegroundWindow();
      }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    private void Run(IntPtr window, string macro, Dictionary<string, string> vars)
    {
      Reader reader = new Reader();

      try
      {
        reader.Executing += reader_Executing;
        reader.Executed += reader_Executed;
        reader.Progress += reader_Progress;
        reader.RunMacro(window, macro, vars);
      }
      finally
      {
        reader.Executing -= reader_Executing;
        reader.Executed -= reader_Executed;
        reader.Progress -= reader_Progress;
      }
    }
    private void btnRun_Click(object sender, EventArgs e)
    {
      txtProgress.Items.Clear();
      if (optOnce.Checked)
      { Run(_active, txtMacro.Text, null); }
      else if (optFiles.Checked)
      { RunFiles(); }
      else
      { throw new InvalidProgramException("Unhandled option"); }
    }

    private void reader_Executing(object sender, EventArgs e)
    {
      Reader reader = sender as Reader;
      if (reader != null)
      {
        int pos = txtProgress.Items.Add(reader.CurrentCommand);
        txtProgress.SelectedIndex = pos;
        txtProgress.TopLevelControl.Refresh();
      }
    }
    private void reader_Executed(object sender, EventArgs e)
    {
      txtProgress.TopLevelControl.Refresh();
    }
    private void reader_Progress(object sender, ProgressEventArgs e)
    {
      if (e != null)
      {
        int pos = txtProgress.Items.Add(e.Message);
        txtProgress.SelectedIndex = pos;
      }
      txtProgress.TopLevelControl.Refresh();
    }

    private void WdgMacro_Load(object sender, EventArgs e)
    {
      _handle = Handle;
    }

    private void TryEnableRun()
    {
      bool enable = File.Exists(txtMacro.Text);
      btnRun.Enabled = enable;
    }

    private void txtMacro_Validating(object sender, System.ComponentModel.CancelEventArgs e)
    {
      TryEnableRun();
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
      dlgOpen.FileName = txtMacro.Text;
      if (dlgOpen.ShowDialog() != DialogResult.OK)
      { return; }

      txtMacro.Text = dlgOpen.FileName;
      TryEnableRun();
    }

    private string OcadMainWnd = "OCAD";
    private string OcadKarteOpen = "Karte öffnen";
    private string OcadPdfExport = "PDF";
    private string OcadPraef = "Präferenzen";

    private static readonly string OcadProcName = "Ocad11Std";
    private static readonly string OcadProgram = @"C:\Program Files (x86)\OCAD\OCAD11\Ocad11Std.exe";

    private delegate bool FuncBool();
    private bool Try(Macro macro, int tries, int sleep, FuncBool function)
    {
      for (int i = 0; i < tries; i++)
      {
        if (function())
        { return true; }

        macro.Sleep(sleep);
      }
      return false;
    }

    private bool TryWindowName(string name, Macro macro, int tries, int sleep)
    {
      bool success = Try(macro, tries, sleep,
        delegate
        {
          string wndName;
          wndName = macro.ForeGroundWindowName();
          return (wndName.StartsWith(name));
        });
      return success;
    }

    private bool Open(Macro macro, string fileName)
    {
      macro.SetForegroundProcess();

      macro.SendCommands('o', Macro.VK_CONTROL);
      macro.Sleep(100);

      if (TryWindowName(OcadKarteOpen, macro, 5, 200) == false)
      {
        macro.KillProcess(OcadProcName);
        return false;
      }
      macro.Sleep(100);

      WriteName(macro, fileName);
      macro.Sleep(100);
      macro.SetForegroundProcess();
      //macro.Sleep(500);

      //IntPtr hWnd = Macro.GetForegroundWindow();
      //Rectangle rect = macro.GetWindowRect(hWnd);

      //Macro.DestroyWindow(hWnd);


      if (TryWindowName(OcadMainWnd, macro, 5, 200) == false)
      {
        macro.KillProcess(OcadProcName);
        return false;
      }

      return true;
    }

    private void WriteName(Macro macro, string name)
    {
      macro.SendCommands('n', Macro.VK_ALT);
      macro.Sleep(100);

      foreach (char c in name)
      { macro.SendKey(c); }

      macro.Sleep(100);

      macro.SendCodes(Macro.VK_RETURN);
      macro.Sleep(100);
    }

    public void RunFile5(string ocd)
    {
      string pdf = Path.ChangeExtension(ocd, ".pdf");
      bool success = false;

      if (File.Exists(pdf))
      { return; }


      while (success == false)
      {
        string wndName;

        if (File.Exists(pdf))
        {
          File.Delete(pdf);
        }

        Macro macro = new Macro();

        bool procSuccess = false;
        while (procSuccess == false)
        {
          IList<Process> processes = Process.GetProcessesByName(OcadProcName);
          if (processes == null || processes.Count == 0)
          {
            // Open Ocad
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = Path.GetDirectoryName(ocd);
            startInfo.FileName = OcadProgram;
            Process proc = Process.Start(startInfo);
            macro.Sleep(10000);
          }
          else
          { procSuccess = true; }
        }

        macro.SetForegroundProcess(OcadProcName);

        if (TryWindowName(OcadMainWnd, macro, 5, 200) == false)
        {
          throw new NotImplementedException();
        }

        if (Open(macro, ocd) == false)
        { continue; }


        while (Export(macro, pdf, OcadPdfExport) == false)
        {
          macro.Sleep(1000);
          if (File.Exists(pdf))
          { File.Delete(pdf); }
        }

        macro.SendCommands('w', Macro.VK_CONTROL);
        macro.Sleep(200);

        macro.SendCommands('n', Macro.VK_ALT);
        macro.Sleep(200);

        Application.DoEvents();

        success = true;
      }
    }

    private bool Export(Macro macro, string pdf, string export)
    {
      bool success;
      int iTry = 0;
      do
      {
        macro.SetForegroundProcess();

        macro.Sleep(300);
        macro.ForegroundClick(100, 10);
        macro.Sleep(100);
        macro.SendCommands('e', Macro.VK_CONTROL);
        macro.Sleep(100);

        macro.SendCodes(Macro.VK_RETURN);
        success = TryWindowName(export, macro, 5, 200);
        iTry++;
      } while (success == false && iTry < 10);

      if (success == false)
      {
        macro.KillProcess(OcadProcName);
        return false;
      }
      macro.Sleep(100);

      WriteName(macro, pdf);

      macro.Sleep(100);
      macro.SetForegroundProcess();


      bool praefSuccess = false;
      while (praefSuccess == false)
      {
        bool finished;
        try
        {
          FileStream stream = File.OpenRead(pdf);
          stream.Close();
          finished = true;
        }
        catch
        {
          finished = false;
        }

        if (finished)
        {
          macro.SendCommands('o', Macro.VK_ALT);
          macro.Sleep(100);
          macro.SendKey('p');
          macro.Sleep(100);

          if (TryWindowName(OcadPraef, macro, 5, 100))
          {
            macro.Sleep(100);
            macro.SendCodes(Macro.VK_RETURN);
            macro.Sleep(100);

            praefSuccess = true;
            continue;
          }
        }

        macro.SetForegroundProcess();
        macro.Sleep(100);
        IntPtr hWnd = Macro.GetForegroundWindow();
        Rectangle rect = Macro.GetWindowRect(hWnd);
        if (rect.Width < 500 && rect.Height < 200 &&
          rect.Height > 0 && rect.Width > 0)
        {
          // Error
          macro.Sleep(100);
          macro.SendCodes(Macro.VK_RETURN);
          macro.Sleep(100);
          return false;
        }
      }


      if (TryWindowName(OcadMainWnd, macro, 5, 200) == false)
      {
        macro.KillProcess(OcadProcName);
        return false;
      }

      return true;
    }

    public void Run5()
    {
      string[] fileList = Directory.GetFiles(
        @"D:\daten\felix\kapreolo\karten\wangenerwald\2008\5erStaffel2010\Bahnen",
        "*.ocd");
      foreach (string path in fileList)
      {
        string fileName = Path.GetFileName(path);
        if (fileName.StartsWith("__") == false)
        { continue; }
        if (fileName.EndsWith(".4.ocd"))
        { continue; }
        RunFile5(path);
      }
    }

    private void RunFiles()
    {
      try
      {
        dgvData.AllowUserToAddRows = false;
        dgvData.ReadOnly = true;
        dgvData.Enabled = true;

        foreach (DataGridViewRow vRow in dgvData.Rows)
        {
          DataRowView row = (DataRowView)vRow.DataBoundItem;
          row[_processedColumn] = CheckState.Unchecked;
        }

        int iRow = 0;
        foreach (DataGridViewRow vRow in dgvData.Rows)
        {
          dgvData.FirstDisplayedScrollingRowIndex = iRow;
          iRow++;

          DataRowView row = (DataRowView)vRow.DataBoundItem;

          object o = row[_fullNameColumn];
          if (o is string == false)
          { o = row[_nameColumn]; }

          if (o is string == false)
          { continue; }
          string file = Path.GetFullPath((string)o);
          //if (file.EndsWith(".ocd") == false)
          //{ continue; }
          if (File.Exists(file) == false)
          { continue; }

          row[_processedColumn] = CheckState.Indeterminate;
          dgvData.Invalidate();
          Application.DoEvents();

          Macro macro = new Macro();

          //macro.SetForegroundProcess(OcadProcName);
          //macro.SendCommands('o', Macro.VK_CONTROL);
          //macro.Sleep(200);
          //string clipBoard = null;
          //while (clipBoard != file)
          //{
          //  macro.SetForegroundProcess(OcadProcName);
          //  macro.SendCommands('n', Macro.VK_ALT);
          //  macro.Sleep(200);
          //  foreach (char c in file)
          //  {
          //    macro.SendKey(c);
          //  }
          //  macro.Sleep(200);
          //  macro.SendCommands('a', Macro.VK_CONTROL);
          //  macro.Sleep(200);
          //  macro.SendCommands('c', Macro.VK_CONTROL);
          //  macro.Sleep(200);
          //  IDataObject data = Clipboard.GetDataObject();
          //  clipBoard = (string)data.GetData(typeof(string));
          //}
          //macro.SendCommands('f', Macro.VK_ALT);

          //macro.Sleep(200);
          //macro.SetForegroundProcess();

          IntPtr ptr = IntPtr.Zero;
          // ptr = macro.MainWindowHandle;
          Dictionary<string, string> vars = new Dictionary<string, string>();
          vars.Add("$a", file);
          Run(ptr, txtMacro.Text, vars);

          macro.Sleep(200);
          row[_processedColumn] = CheckState.Checked;
          Application.DoEvents();
        }
      }
      finally
      {
        dgvData.Enabled = false;
        dgvData.ReadOnly = false;
        dgvData.AllowUserToAddRows = true;
      }
    }

    private void MacroWdg_KeyDown(object sender, KeyEventArgs e)
    {

    }



    private void txtMacro_MouseDown(object sender, MouseEventArgs e)
    {

    }


    private void txtMacro_KeyDown(object sender, KeyEventArgs e)
    {
      e.Handled = true;
    }

    private void txtMacro_KeyUp(object sender, KeyEventArgs e)
    {
      e.Handled = true;
    }

    private void btnSelectFiles_Click(object sender, EventArgs e)
    {
      if (dlgOpenData.ShowDialog() != DialogResult.OK)
      { return; }

      IList<string> files = dlgOpenData.FileNames;
      SetData(files);
    }

    private void optRun_CheckChanged(object sender, EventArgs e)
    {
      dgvData.Enabled = (optFiles.Checked == false);
    }

    private void btn5Staffel_Click(object sender, EventArgs e)
    {
      Run5();
    }
  }
}