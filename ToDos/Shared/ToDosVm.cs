
using System;
using System.Collections.Generic;

namespace ToDos.Shared
{
  public interface IToDosView
  {
    void InvalidateOrder();
  }

  public class ToDosVm
  {
    private readonly List<ToDoVm> _toDos;

    private bool _sorted;

    public ToDosVm(IEnumerable<ToDoVm> toDos)
    {
      _toDos = new List<ToDoVm>(toDos);
    }

    public void ResetSorted()
    {
      _sorted = false;
    }
    public IReadOnlyList<ToDoVm> ToDos
    {
      get
      {
        if (!_sorted)
        {
          _toDos.Sort(new ToDoVm.SortDueTime(DateTime.Now));
          _sorted = true;
        }
        return _toDos;
      }
    }

    public static ToDosVm Test()
    {
      ToDosVm test = new ToDosVm(new List<ToDoVm>
      {
        new ToDoVm(new ToDo { Name = "Rumpfbeugen", Repeat = new TimeSpan(3,0,0,0), CompleteTimes = new List<DateTime>{ DateTime.Today } }),
        new ToDoVm(new ToDo { Name = "Liegestützen", Repeat = new TimeSpan(5,0,0,0), CompleteTimes = new List<DateTime>() }),
        new ToDoVm(new ToDo { Name = "Lauftraining", Repeat = new TimeSpan(2,0,0,0) }),
      });
      return test;
    }
  }
}