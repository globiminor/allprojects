using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Basics.Views
{
  public interface INotifyListener
  {
    void Changed(string property);
  }

  public interface IWorker
  {
    bool Start();
    void Finish();
    void Finally();
  }

  public abstract class NotifyListener : INotifyPropertyChanged, INotifyListener, IDataErrorInfo, IDisposable
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly Dictionary<string, string> _dictErrors = new Dictionary<string, string>();

    private Runner _runner;
    private string _progress;

    public string Progress
    {
      get { return _progress; }
      set
      {
        _progress = value;
        Changed();
      }
    }

    protected void RunAsync(IWorker worker)
    {
      _runner = new Runner(worker, this);
      _runner.Run();
    }

    protected void CancelRun()
    {
      if (_runner == null)
      { return; }

      _runner.Cancel();
    }

    public void SetProgressAsync(string progress)
    {
      if (_runner == null)
      {
        _progress = progress;
        return;
      }
      _progress = progress;
      _runner.SetProgress(progress);
    }


    private class Runner
    {
      private readonly IWorker _worker;
      private readonly NotifyListener _parent;
      private volatile string _progress;

      private BackgroundWorker _bgw;
      public Runner(IWorker worker, NotifyListener parent)
      {
        _worker = worker;
        _parent = parent;
      }

      public void Run()
      {
        _bgw = new BackgroundWorker
        {
          WorkerReportsProgress = true,
          WorkerSupportsCancellation = true
        };

        _bgw.DoWork += Bgw_DoWork;
        _bgw.RunWorkerCompleted += Bgw_RunWorkerCompleted;
        _bgw.ProgressChanged += _bgw_ProgressChanged;

        _bgw.RunWorkerAsync();
      }

      public void SetProgress(string progress)
      {
        _progress = progress;
        _bgw?.ReportProgress(10);
      }

      private void _bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
        _parent.Progress = _progress;
      }

      public void Cancel()
      {
        if (_bgw == null)
        { return; }

        _bgw.CancelAsync();
      }

      private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
        try
        {
          if (e.Cancelled)
          { }
          else if (e.Error == null)
          { _worker.Finish(); }
        }
        finally
        {
          _bgw = null;
          _worker.Finally();
        }
      }

      private void Bgw_DoWork(object sender, DoWorkEventArgs e)
      {
        e.Result = _worker.Start();
      }
    }

    private ChangingHandler _changing;
    protected IDisposable ChangingAll()
    {
      return new ChangingHandler(this, null);
    }
    protected IDisposable Changing([CallerMemberName] string property = "")
    {
      return new ChangingHandler(this, property);
    }
    private class ChangingHandler : IDisposable
    {
      private readonly NotifyListener _parent;
      public ChangingHandler(NotifyListener parent, string property)
      {
        _parent = parent;
        SetChanging(this, property);
      }

      private static object _changeLock = new object();
      private void SetChanging(ChangingHandler change, string property)
      {
        lock (_changeLock)
        {
          if (change == null)
          {
            ChangingHandler pc = _parent._changing;
            string prop = pc?.Property;
            _parent._changing = null;
            _parent.Changed(prop);
          }
          else
          {
            if (_parent._changing == null)
            {
              _parent._changing = change;
              _parent._changing.Property = property;
            }
            else if (_parent._changing.Property != property)
            {
              _parent._changing.Property = null;
            }
          }
        }
      }

      private string Property { get; set; }
      public void Dispose()
      {
        if (_parent._changing == this)
        { SetChanging(null, null); }
      }
    }

    void INotifyListener.Changed(string property)
    { Changed(property); }
    protected void ChangedAll()
    {
      Changed(string.Empty);
    }
    protected virtual void Changed([CallerMemberName] string property = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public void Dispose()
    {
      Disposing(true);
    }

    public void ClearErrors()
    {
      _dictErrors.Clear();
    }

    public string Error
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var pair in _dictErrors)
        {
          stringBuilder.AppendFormat("{0}: {1}", pair.Key, pair.Value);
          stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
      }
    }

    string IDataErrorInfo.this[string columnName]
    {
      get
      {
        if (_dictErrors.ContainsKey(columnName))
        {
          return _dictErrors[columnName];
        }
        return null;
      }
    }
    protected string GetError(string property)
    {
      return ((IDataErrorInfo)this)[property];
    }

    /// <summary>
    /// Removes the errorn for the given property name from the list of errors.
    /// </summary>
    /// <param name="propertyName">The property name, for which the error is removed.</param>
    public void RemoveError(string propertyName)
    {
      _dictErrors.Remove(propertyName);
    }

    public void SetError(string propertyName, string error)
    {
      if (string.IsNullOrEmpty(error))
      {
        RemoveError(propertyName);
      }
      else
      {
        if (_dictErrors.ContainsKey(propertyName) == false)
        {
          _dictErrors.Add(propertyName, error);
        }
        else
        {
          _dictErrors[propertyName] = error;
        }
      }
    }

    protected abstract void Disposing(bool disposing);
  }
}
