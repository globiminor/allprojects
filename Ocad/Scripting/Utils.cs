using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;

namespace Ocad.Scripting
{
  public class Utils
  {
    public string Exe { get; set; }

    public static void CreatePdf(string dir, string exe = null)
    {
      string script = Path.Combine(dir, "CreatePdf.xml");
      CreatePdf(script, Directory.GetFiles(dir, "*.ocd"), exe);
    }

    public static void CreatePdf(string script, IEnumerable<string> files, string exe = null)
    {
      string defaultExe = CreatePdfScript(files, script);
      new Utils { Exe = exe ?? defaultExe }.RunScript(script, defaultExe);
    }

    public static void Optimize(IEnumerable<string> files, string script, string exe = null, bool wait = false)
    {
      string defaultExe = OptimizeScript(files, script, out string lastFile);
      if (lastFile == null)
      { return; }
      DateTime t = DateTime.Now;
      new Utils { Exe = exe ?? defaultExe }.RunScript(script, defaultExe);

      if (wait)
      {
        while (new FileInfo(lastFile).LastWriteTime < t)
        {
          System.Threading.Thread.Sleep(500);
        }
      }
    }

    public string RunScript(string script, string exe, bool setAccess = true, int wait = -1)
    {
      if (exe == null)
      {
        return null;
      }

      exe = Exe ?? exe;

      if (setAccess)
      { SetAccessControl(Environment.CurrentDirectory); }

      ProcessStartInfo start = new ProcessStartInfo(exe, script);
      Process proc = new Process();
      proc.StartInfo = start;

      proc.Start();
      string procName = proc.ProcessName;

      if (wait > 0)
      { proc.WaitForExit(wait); }
      else
      { proc.WaitForExit(); }

      if (setAccess)
      { SetAccessControl(Environment.CurrentDirectory); }
      return procName;
    }

    private static void SetAccessControl(string dir)
    {
      foreach (var path in Directory.GetDirectories(dir))
      {
        SetAccessControl(path);
      }
      foreach (var path in Directory.GetFiles(dir))
      {
        FileSecurity sec = File.GetAccessControl(path);
        FileSystemAccessRule access_rule = new FileSystemAccessRule(@"Benutzer",
            FileSystemRights.FullControl, AccessControlType.Allow);
        sec.AddAccessRule(access_rule);
        File.SetAccessControl(path, sec);
      }
    }


    private static string CreatePdfScript(IEnumerable<string> files, string script)
    {
      string exe = null;
      using (Script expPdf = new Script(script))
      {
        foreach (var ocdFile in files)
        {
          if (exe == null)
          {
            exe = Basics.Utils.FindExecutable(ocdFile);
          }
          string pdfFile = Path.GetFileNameWithoutExtension(ocdFile) + ".pdf";
          string dir = Path.GetDirectoryName(ocdFile);
          string pdfPath = Path.Combine(dir, pdfFile);
          if (File.Exists(pdfPath))
          { File.Delete(pdfPath); }

          using (Node node = expPdf.FileOpen())
          { node.File(ocdFile); }
          using (Node node = expPdf.FileExport())
          {
            node.File(pdfPath);
            node.Format(Format.Pdf);
          }
          using (Node node = expPdf.FileClose())
          {
            node.Enabled(true);
          }
        }
        if (exe != null)
        {
          using (Node node = expPdf.FileExit())
          {
            node.Enabled(true);
          }
        }
      }
      return exe;
    }

    private static string OptimizeScript(IEnumerable<string> files, string script, out string lastFile)
    {
      string exe = null;
      lastFile = null;
      using (Script opt = new Script(script))
      {
        foreach (var ocdFile in files)
        {
          lastFile = ocdFile;
          if (exe == null)
          {
            exe = Basics.Utils.FindExecutable(ocdFile);
          }
          string dir = Path.GetDirectoryName(ocdFile);

          using (Node node = opt.FileOpen())
          { node.File(ocdFile); }
          using (Node node = opt.MapOptimize())
          { node.Enabled(true); }
          using (Node node = opt.FileSave())
          { node.Enabled(true); }
          using (Node node = opt.FileClose())
          { node.Enabled(true); }
        }
        if (exe != null)
        {
          using (Node node = opt.FileExit())
          {
            node.Enabled(true);
          }
        }
      }
      return exe;
    }

    private static bool RunScript(string[] args)
    {
      if (args == null || args.Length != 1)
      { return false; }
      string script = args[0];
      if (Path.GetExtension(script) != ".xml")
      { return false; }
      if (!File.Exists(script))
      { return false; }

      string exe = Basics.Utils.FindExecutable(@"C:\daten\felix\OL\divers\velo_symbol.ocd");
      FileInfo fPdf = new FileInfo(@"C:\daten\ASVZ\SOLA\2014\Exp_Uebergabe\Ue_00_01_Bucheggplatz.pdf");
      FileInfo fGif = new FileInfo(@"C:\daten\ASVZ\SOLA\2014\Exp_Uebergabe\Ue_00_01_Bucheggplatz.gif");

      FileSecurity sec = File.GetAccessControl(fPdf.FullName, AccessControlSections.All);
      var rules = sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

      return true;
    }

    public static void CloseOcadProcs(string ocadExe, bool force = false)
    {
      bool cleanRoaming = force;
      IList<Process> procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ocadExe));
      foreach (var proc in procs)
      {
        try
        {
          proc.CloseMainWindow();
          System.Threading.Thread.Sleep(500);
          if (!proc.HasExited && force)
          {
            proc.Kill();
            cleanRoaming = true;
          }
        }
        catch { }
      }
      if (cleanRoaming)
      {
        System.Threading.Thread.Sleep(500);
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string ocd = Path.Combine(appData, "OCAD");
        Clean(ocd, DateTime.Now.AddHours(-1));
      }
    }

    private static void Clean(string folder, DateTime limit)
    {
      foreach (var file in Directory.GetFiles(folder))
      {
        FileInfo fi = new FileInfo(file);
        if (fi.LastWriteTime > DateTime.Now.AddHours(-1))
        {
          File.Delete(file);
        }
      }

      foreach (var child in Directory.GetDirectories(folder))
      {
        Clean(child, limit);
      }
    }
  }
}
