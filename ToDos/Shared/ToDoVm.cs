
using System;
using System.Collections.Generic;

namespace ToDos.Shared
{
  public class ToDoVm : Views.BaseVm
  {
    private readonly ToDo _toDo;
    private bool _timesSorted;
    public ToDoVm(ToDo toDo)
    {
      _toDo = toDo;
    }

    private bool _isCompleted;
    public bool IsCompleted
    {
      get => _isCompleted;
      set
      {
        SetStruct(ref _isCompleted, value);
        if (value)
        {
          _toDo.CompleteTimes = _toDo.CompleteTimes ?? new List<DateTime>();
          _toDo.CompleteTimes.Add(DateTime.Now);
          _timesSorted = false;
          _urgencyChange++;
          Changed(nameof(UrgencyChange));
          Changed(null);
        }
      }
    }

    public string Name
    {
      get => _toDo.Name;
      set
      {
        if (value == _toDo.Name)
          return;
        _toDo.Name = value;
        Changed();
      }
    }
    public IReadOnlyList<DateTime> CompletedTimes
    {
      get
      {
        if (_toDo?.CompleteTimes == null) return null;
        if (!_timesSorted)
        {
          _toDo.CompleteTimes.Sort();
          _toDo.CompleteTimes.Reverse();
          _timesSorted = true;
        }
        return _toDo.CompleteTimes;
      }
    }

    public double Urgency => (NextTime - DateTime.Now).TotalSeconds / _toDo.Repeat.TotalSeconds;
    private int _urgencyChange;
    public int UrgencyChange => _urgencyChange;

    public DateTime DueTime => GetDueTime(DateTime.Now);
    public DateTime GetDueTime(DateTime minTime)
    {
      DateTime next = NextTime;
      return next < minTime ? minTime : next;
    }
    public DateTime NextTime
    {
      get
      {
        if ((_toDo?.CompleteTimes?.Count ?? 0) <= 0)
          return DateTime.MinValue;
        return _toDo.CompleteTimes[0] + _toDo.Repeat;
      }
    }

    public class SortDueTime : IComparer<ToDoVm>
    {
      private readonly DateTime _minDate;
      public SortDueTime(DateTime minDate)
      {
        _minDate = minDate;
      }
      public int Compare(ToDoVm x, ToDoVm y)
      {
        int d = x.GetDueTime(_minDate).CompareTo(y.GetDueTime(_minDate));
        if (d != 0)
          return d;

        double dx = (_minDate - x.NextTime).TotalSeconds / x._toDo.Repeat.TotalSeconds;
        double dy = (_minDate - y.NextTime).TotalSeconds / y._toDo.Repeat.TotalSeconds;
        d = dx.CompareTo(dy);
        if (d != 0)
          return -d;

        return x._toDo.Name.CompareTo(y._toDo.Name);
      }
    }
  }
}