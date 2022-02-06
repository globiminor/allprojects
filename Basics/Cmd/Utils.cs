using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Basics.Cmd
{
  public static class Utils
  {
    // Unmanaged system assembly. 
    // Dubious whether this import will still work with Windows 10. 
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

    private const string Kernel32_DllName = "kernel32.dll";

    [DllImport(Kernel32_DllName)]
    private static extern bool AllocConsole();

    [DllImport(Kernel32_DllName)]
    private static extern bool FreeConsole();

    [DllImport(Kernel32_DllName)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport(Kernel32_DllName)]
    private static extern int GetConsoleOutputCP();

    public static bool HasConsole
    {
      get { return GetConsoleWindow() != IntPtr.Zero; }
    }

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void HideConsoleWindow()
    {
      IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

      if (hWnd != IntPtr.Zero)
      {
        ShowWindow(hWnd, 0); // 0 = SW_HIDE
      }
    }

    /// <summary>
    /// Creates a new console instance if the process is not attached to a console already.
    /// </summary>
    public static void ShowConsole()
    {
      //#if DEBUG
      if (!HasConsole)
      {
        AllocConsole();
        InvalidateOutAndError();
      }
      //#endif
    }

    /// <summary>
    /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
    /// </summary>
    public static void HideConsole()
    {
      //#if DEBUG
      if (HasConsole)
      {
        SetOutAndErrorNull();
        FreeConsole();
      }
      //#endif
    }

    public static void ToggleConsole()
    {
      if (HasConsole)
      {
        HideConsole();
      }
      else
      {
        ShowConsole();
      }
    }

    static void InvalidateOutAndError()
    {
      Type type = typeof(System.Console);

      System.Reflection.FieldInfo _out = type.GetField("_out",
          System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

      System.Reflection.FieldInfo _error = type.GetField("_error",
          System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

      System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
          System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

      Debug.Assert(_out != null);
      Debug.Assert(_error != null);

      Debug.Assert(_InitializeStdOutError != null);

      _out.SetValue(null, null);
      _error.SetValue(null, null);

      _InitializeStdOutError.Invoke(null, new object[] { true });
    }

    static void SetOutAndErrorNull()
    {
      Console.SetOut(null);
      Console.SetError(null);
    }

    public static string ReadLine()
    {
      if (!Console.IsInputRedirected)
      {
        int bufSize = 1024;
        System.IO.Stream inStream = Console.OpenStandardInput(bufSize);
        Console.SetIn(new System.IO.StreamReader(inStream, Console.InputEncoding, false, bufSize));
      }

      string line = Console.ReadLine();
      return line;
    }

    /// <summary>
    ///     Convert commandLine to arguments as Console applications do when starting an application
    /// </summary>
    public static string[] CommandLineToArgs(string commandLine)
    {
      IntPtr argv = CommandLineToArgvW(commandLine, out int argc);
      if (argv == IntPtr.Zero)
        throw new Win32Exception();
      try
      {
        var args = new string[argc];
        for (var i = 0; i < args.Length; i++)
        {
          IntPtr p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
          args[i] = Marshal.PtrToStringUni(p);
        }

        return args;
      }
      finally
      {
        Marshal.FreeHGlobal(argv);
      }
    }

    public static bool Run(IList<string> allArgs, Dictionary<string, Func<IList<string>, ICommand>> commands)
    {
      try
      {
        if (allArgs == null || allArgs.Count == 0)
        {
          return ShowCommands(commands);
        }
        string cmd = allArgs[0];
        if (!commands.TryGetValue(cmd, out Func<IList<string>, ICommand> func))
        {
          Console.WriteLine($"Unknown command {cmd}");
          return ShowCommands(commands);
        }

        int nArgs = allArgs.Count;
        List<string> args = new List<string>(nArgs - 1);
        for (int i = 1; i < nArgs; i++)
        {
          args.Add(allArgs[i]);
        }

        ICommand command = func(args);
        if (command == null)
        { return false; }

        string error;
        bool success;
        try
        {
          success = command.Execute(out error);
        }
        catch (Exception e)
        {
          success = false;
          error = e.Message;
        }

        Console.WriteLine();
        if (!string.IsNullOrWhiteSpace(error))
        {
          Console.WriteLine(error);
        }
        return success;
      }
      catch (Exception exp)
      {
        Console.WriteLine();
        string msg = Basics.Utils.GetMsg(exp);
        Console.WriteLine(msg);
        return false;
      }
    }

    public static void Try(Action action, Func<string> errorDetail)
    {
      try { action(); }
      catch (Exception e)
      { throw new InvalidOperationException(errorDetail(), e); }
    }
    private static bool ShowCommands(Dictionary<string, Func<IList<string>, ICommand>> commands)
    {
      Console.WriteLine("Possible commands:");
      foreach (string cmd in commands.Keys)
      {
        Console.WriteLine(cmd);
      }
      return false;
    }

    /// <summary>
    ///     Read a complete commandline into t
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t">object where the interpreted args are stored</param>
    /// <param name="args">commandline arguments</param>
    /// <param name="cmds">List of commands for interpreting args</param>
    /// <param name="error">error message, when the args could not be interpreted completly</param>
    /// <returns>
    ///     true, if all args were interpreted,
    ///     <para>false, otherwise. In this case, error will have some value and the a syntax description of the command</para>
    /// </returns>
    public static bool ReadArgs<T>(T t, IList<string> args, IList<Command<T>> cmds, out string error)
    {
      var sb = new StringBuilder();

      var i = 0;
      Dictionary<Command<T>, int> calls = new Dictionary<Command<T>, int>();
      foreach (Command<T> cmd in cmds)
      { calls.Add(cmd, 0); }
      while (i < args.Count)
      {
        bool success = false;
        foreach (Command<T> cmd in cmds)
        {
          if (cmd.TryApply(t, args, ref i))
          {
            calls[cmd]++;
            success = true;
          }
        }

        if (!success)
        {
          if (args[i] == "?")
          {
            sb.Append("Usage: ");
            AppendUsage(sb, cmds);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Parameters: ");
            AppendParameters(sb, cmds);
          }
          else
          {
            sb.AppendFormat("Unknown parameter {0}", args[i]);
            sb.AppendLine();
            sb.Append("Usage: ");
            AppendUsage(sb, cmds);
          }
          error = sb.ToString();
          return false;
        }
      }

      sb = new StringBuilder();
      foreach (var pair in calls)
      {
        if (pair.Value > 1)
        {
          sb.AppendLine($"Multiple usage of parameter {pair.Key.Key}");
        }
        if (!pair.Key.Optional && pair.Value <= 0)
        {
          sb.AppendLine($"Missing parameter {pair.Key.Key}");
        }
      }
      if (sb.Length > 0)
      {
        sb.Append("Usage: ");
        AppendUsage(sb, cmds);
        error = sb.ToString();
        return false;
      }

      foreach (Command<T> cmd in cmds)
      {
        if (calls[cmd] == 0 && cmd.Default != null)
        {
          cmd.Read(t, cmd.Default(), -1);
        }
      }
      error = null;
      return true;
    }

    private static void AppendUsage<T>(StringBuilder sb, IEnumerable<Command<T>> cmds)
    {
      foreach (var cmd in cmds)
      {
        if (!cmd.Optional)
        { sb.AppendFormat(" {0} {1}", cmd.Key, cmd.Parameters); }
        else
        { sb.AppendFormat(" [{0} {1}]", cmd.Key, cmd.Parameters); }
      }
    }

    private static void AppendParameters<T>(StringBuilder sb, IEnumerable<Command<T>> cmds)
    {
      foreach (var cmd in cmds)
      {
        if (!cmd.Optional)
        { sb.Append($" {cmd.Key} {cmd.Parameters}"); }
        else
        {
          sb.Append($"[{cmd.Key} {cmd.Parameters}]");
          if (cmd.Default != null)
          {
            sb.Append($"; Default = ");
            foreach (string s in cmd.Default())
            {
              string t = s;
              if (s.Contains(" "))
              { t = $"\"{s}\""; }
              sb.Append($"{t} ");
            }
          }
        }
        if (!string.IsNullOrWhiteSpace(cmd.Info))
        { sb.Append($"; Description: {cmd.Info}"); }
        sb.AppendLine();
      }
    }

    private static string _lastString = string.Empty;
    public static void ReportProgress(object sender, EventArgs e)
    {
      ProgressEventArgs args = e as ProgressEventArgs;
      if (args == null)
      { return; }

      string sPct;
      if (args.Progress < 0)
      { sPct = null; }
      else
      {
        int pct = (int)Math.Round(args.Progress * 100);
        string sP;
        if (pct == 0 && args.Progress > 0)
        { sP = ">0"; }
        else if (pct == 100 && args.Progress < 1)
        { sP = "<100"; }
        else
        { sP = $"{pct}"; }
        sPct = $"{sP}%: ";
      }
      string progress;
      if (args.KeepVisible)
      { progress = args.Comment; }
      else
      {
        progress = $"{sPct}{args.Comment}";
        if (progress.Length >= Console.BufferWidth)
        { progress = progress.Substring(0, Console.BufferWidth - 4) + "..."; }
      }

      StringBuilder erase = new StringBuilder();
      if (_lastString.Length > progress.Length)
      {
        for (int i = progress.Length; i < _lastString.Length; i++)
        { erase.Append(" "); }
      }
      Console.Write($"\r{progress}");
      if (erase.Length > 0)
      { Console.Write($"{erase}"); }

      _lastString = progress;
      if (args.KeepVisible)
      { Console.WriteLine(); }
    }
  }
}
