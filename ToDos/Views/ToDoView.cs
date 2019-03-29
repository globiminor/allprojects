
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using ToDos.Shared;

namespace ToDos.Views
{
  public class ToDoView : LinearLayout
  {
    private readonly CheckBox _chkCompleted;
    private readonly TextView _txtName;
    private readonly View _warnView;
    private readonly IToDosView _toDosView;

    internal ToDoView(Context context, IToDosView toDosView)
      : base(context)
    {
      _toDosView = toDosView;

      Orientation = Orientation.Horizontal;

      {
        CheckBox chkCompleted = new CheckBox(Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          chkCompleted.LayoutParameters = lprams;
        }
        AddView(chkCompleted);

        //chkCompleted.BindToChecked(vm, nameof(vm.Visible));
        //chkCompleted.Enabled = vm.VisibleEnabled;
        _chkCompleted = chkCompleted;
      }
      {
        View warnView = new View(Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.Width = 60;
          lprams.Height = 60;
          warnView.LayoutParameters = lprams;
        }
        AddView(warnView);
        _warnView = warnView;
      }
      {
        TextView txtName = new TextView(Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          txtName.LayoutParameters = lprams;
        }
        AddView(txtName);
        _txtName = txtName;
      }
    }

    private double _urgency;
    public double Urgency
    {
      get => _urgency;
      set
      {
        _urgency = value;
        if (value >= 0)
        {
          _warnView.SetBackgroundColor(Color.Green);
        }
        else
        {
          byte r = 255;
          byte g = value < -1 ? (byte)0 : (byte)(255 * (1 - value));
          byte b = value < -1 ? (byte)System.Math.Min(255, 255 * (value + 1) / value) : (byte)0;
          _warnView.SetBackgroundColor(new Color(r, g, b));
        }
      }
    }

    private int _urgencyChange;
    public int UrgencyChange
    {
      get => _urgencyChange;
      set
      {
        _urgencyChange = value;
        if (value != 0)
          _toDosView.InvalidateOrder();
      }
    }
    private ToDoVm _vm;
    public ToDoVm Vm
    {
      get => _vm;
      set
      {
        if (_vm == value)
          return;

        _chkCompleted.BindToChecked(value, nameof(value.IsCompleted));
        _txtName.BindToText(value, nameof(value.Name));

        new Binding<double>(value, nameof(value.Urgency), (t) => Urgency = t, () => Urgency);
        new Binding<int>(value, nameof(value.UrgencyChange), (t) => UrgencyChange = t, () => UrgencyChange);

        _vm = value;
      }
    }
  }
}