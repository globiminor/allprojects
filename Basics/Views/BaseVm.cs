
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Basics.Views
{
  public abstract class BaseVm : INotifyPropertyChanged, IDataErrorInfo
  {
    private Dictionary<string, string> _errors;
    private string _error;
    private string _allError;

    public string Error
    {
      get
      {
        if (_error == null)
        {
          StringBuilder eb = new StringBuilder();
          if (!string.IsNullOrEmpty(_allError))
          { eb.AppendLine($"{_allError};"); }
          foreach (var pair in Errors)
          {
            if (!string.IsNullOrWhiteSpace(pair.Value))
            {
              eb.AppendLine($"[{pair.Key}]:{pair.Value};");
            }
          }
          _error = eb.ToString();
        }
        return _error;
      }
    }

    public string this[string columnName]
    {
      get
      {
        if (!Errors.TryGetValue(columnName, out string error))
        { return null; }

        return error;
      }
    }

    protected Dictionary<string, string> Errors
    { get { return _errors ?? (_errors = new Dictionary<string, string>()); } }

    public int SetError(string error, string column = null)
    {
      _error = null;
      if (column == null)
      { _allError = error; }
      else
      {
        if (string.IsNullOrWhiteSpace(error))
        { Errors.Remove(column); }
        else
        { Errors[column] = error; }
      }
      return string.IsNullOrWhiteSpace(error) ? 0 : 1;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void ChangedAdd()
    {
      Changed(null);
    }
    protected void Changed([CallerMemberName] string caller = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
    }

    protected bool SetStruct<T>(ref T current, T newValue, [CallerMemberName] string caller = "")
      where T : struct
    {
      if (current.Equals(newValue))
      { return false; }
      current = newValue;
      Changed(caller);
      return true;
    }

    protected bool SetValue<T>(ref T current, T newValue, [CallerMemberName] string caller = "")
      where T : class
    {
      if (current == newValue)
      {
        return false;
      }
      current = newValue;
      Changed(caller);
      return true;
    }

  }
}
