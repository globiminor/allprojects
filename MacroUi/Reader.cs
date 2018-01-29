using Macro;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MacroUi
{
  public class Reader
  {
    private int _sleep = 200;
    private Dictionary<string, string> _variables;
    private string _currentCmd;

    public event EventHandler Executing;
    public event EventHandler Executed;
    public event ProgressEventHandler Progress;

    public int SleepPerStep
    {
      get { return _sleep; }
      set { _sleep = value; }
    }

    public void RunMacro(IntPtr startWindow, string macro, Dictionary<string, string> vars)
    {
      using (StreamReader reader = new StreamReader(macro))
      {
        RunMacro(startWindow, reader, vars);
      }
    }

    public string CurrentCommand
    {
      get { return _currentCmd; }
    }

    public void SetVariable(string var, string value)
    {
      _variables[var] = value;
    }

    public void RunMacro(IntPtr startWindow, StreamReader reader, Dictionary<string, string> vars)
    {
      _variables = new Dictionary<string, string>();
      if (vars != null)
      {
        foreach (var pair in vars)
        { _variables.Add(pair.Key, pair.Value); }
      }

      Processor macro = new Processor();
      try
      {
        macro.Progress += Macro_Progress;

        Process[] processes = Process.GetProcesses();

        IntPtr hWnd = startWindow;

        bool first = true;
        RunReader(macro, reader, first);
      }
      finally
      {
        macro.Progress -= Macro_Progress;
      }
    }

    private void RunReader(Processor macro, StreamReader reader, bool first)
    {
      for (string line = reader.ReadLine(); line != null;
           line = reader.ReadLine())
      {
        ///ppp.WaitForInputIdle();
        if (first && line.StartsWith("#"))
        {
          continue;
        }
        if (first == false)
        { OnExecuted(null); }

        first = false;
        System.Threading.Thread.Sleep(_sleep);
        string command = line.Trim();
        int iComment = command.IndexOf(" //");
        if (iComment > 0)
        { command = command.Substring(0, iComment).Trim(); }

        _currentCmd = command;
        OnExecuting(null);
        string[] keys = command.Split();

        if (keys.Length == 0)
        { continue; }

        if (keys[0].StartsWith("//"))
        { continue; }

        if (keys[0].StartsWith("$") && keys.Length == 1)
        {
          keys[0] = Value(keys[0]);
        }
        if (keys[0].StartsWith("$") && keys.Length > 2 && keys[1] == "=")
        {
          string var = keys[0];
          string operand0 = null;
          int i = 2;
          while (i < keys.Length)
          {
            string operand = Value(keys[i]);
            if (operand == "FULLPATH" && i < keys.Length - 1 && operand0 == null)
            {
              i++;
              operand0 = Path.GetFullPath(Value(keys[i]));
            }
            else if (operand == "REPLACE" && i < keys.Length - 3 && operand0 == null)
            {
              operand0 = Value(keys[i + 1]).Replace(Value(keys[i + 2]), Value(keys[i + 3]));
              i += 3;
            }
            else
            {
              operand0 = operand;
            }
            i++;
          }
          if (_variables.ContainsKey(var))
          { _variables.Remove(var); }
          _variables.Add(var, operand0);

          continue;
        }
        if (keys[0] == "SLEEP" && keys.Length == 2)
        {
          System.Threading.Thread.Sleep(int.Parse(keys[1]));
          continue;
        }
        if (keys[0] == "WAITIDLE" && keys.Length == 1)
        {
          macro.WaitIdle();
          continue;
        }
        if (keys[0] == "WAITIDLE" && keys.Length == 2)
        {
          macro.WaitFileIdle(Value(keys[1]));
          continue;
        }
        if (keys[0] == "RUN" && keys.Length == 2)
        {
          using (StreamReader runReader = new StreamReader(keys[1]))
          {
            RunReader(macro, runReader, false);
          }
          continue;
        }

        if (keys.Length == 2 && keys[0].Trim() == "SET_WINDOW")
        {
          macro.SetForegroundWindow(keys[1].Trim());
          continue;
        }
        if (keys[0] == "SET_PROCESS" && keys.Length == 2)
        {
          macro.SetForegroundProcess(Value(keys[1]));
          continue;
        }
        if (keys[0] == "WAITFORINPUTIDLE" && keys.Length == 1)
        {
          macro.WaitForInputIdle();
          continue;
        }
        if (keys.Length == 3 && keys[0] == "CLICK")
        {
          string sx = Value(keys[1]);
          string sy = Value(keys[2]);
          int x = int.Parse(sx);
          int y = int.Parse(sy);
          macro.ForegroundClick(x, y);
          continue;
        }

        List<byte> context = new List<byte>();
        char? c = null;
        foreach (string key in keys)
        {
          if (string.IsNullOrEmpty(key))
          { continue; }
          string trimmed = key.Trim();
          if (string.IsNullOrEmpty(trimmed))
          { continue; }

          if (key.Length == 1)
          { c = key[0]; }
          else if (key == "VK_NEXT")
          { context.Add((byte)'\t'); }
          else if (key == "VK_PRIOR")
          {
            context.Add(Ui.VK_SHIFT);
            context.Add((byte)'\t');
          }
          else if (key == "VK_RETURN")
          { context.Add(Ui.VK_RETURN); }
          else if (key == "VK_CONTROL")
          { context.Add(Ui.VK_CONTROL); }
          else if (key == "VK_ALT")
          { context.Add(Ui.VK_ALT); }
          else if (keys.Length == 1)
          { macro.SendKeys(key); }
          else
          { throw new InvalidOperationException("unhandled word " + key); }
        }

        if (c != null)
        { macro.SendCommand(c.Value, context); }
        else
        { macro.SendCode(context); }
      }
    }

    protected virtual void OnExecuting(EventArgs args)
    {
      Executing?.Invoke(this, args);
    }
    protected virtual void OnExecuted(EventArgs args)
    {
      Executed?.Invoke(this, args);
    }
    protected virtual void OnProgress(ProgressEventArgs args)
    {
      Progress?.Invoke(this, args);
    }
    private void Macro_Progress(object sender, ProgressEventArgs e)
    {
      OnProgress(e);
    }

    private string Value(string key)
    {
      if (key.StartsWith("$"))
      {
        return _variables[key];
      }
      else if (key == "CLIPBOARD")
      {
        IDataObject data = Clipboard.GetDataObject();
        return (string)data.GetData(typeof(string));
      }
      else
      {
        return key.Trim('\'');
      }
    }

  }
}
