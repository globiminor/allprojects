
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using ToDos.Shared;

namespace ToDos.Views
{
  public class ToDosView : LinearLayout
  {
    public static ToDosView Create<T>(T context)
      where T : Context, IToDosView
    {
      return new ToDosView(context, context);
    }

    private readonly IToDosView _toDosView;
    private ToDosView(Context context, IToDosView toDosView)
      : base(context)
    {
      _toDosView = toDosView;
    }

    private ToDosVm _vm;
    public ToDosVm Vm
    {
      get => _vm;
      set
      {
        if (_vm == value)
        { return; }

        RemoveAllViews();

        foreach (var toDoVm in value.ToDos)
        {
          ToDoView view = new ToDoView(Context, _toDosView) { Vm = toDoVm };
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            view.LayoutParameters = lprams;
          }
          AddView(view);
        }
        _vm = value;
      }
    }

    public void InvalidateOrder()
    {
      Dictionary<ToDoVm, ToDoView> views = new Dictionary<ToDoVm, ToDoView>(ChildCount);
      for (int iChild = 0; iChild < ChildCount; iChild++)
      {
        if (!(GetChildAt(iChild) is ToDoView toDo))
        { continue; }
        views.Add(toDo.Vm, toDo);
      }
      foreach (var toDo in views.Values)
      {
        RemoveView(toDo);
      }

      Vm.ResetSorted();
      foreach (var toDoVm in Vm.ToDos)
      {
        if (views.TryGetValue(toDoVm, out ToDoView view))
        {
          AddView(view);
        }
      }
      PostInvalidate();
    }
  }
}