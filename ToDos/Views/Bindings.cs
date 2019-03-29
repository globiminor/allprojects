using Android.Widget;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ToDos.Views
{
  public enum BindingMode { OneTime, OneWay, TwoWay };

  [Obsolete("move")]
  public abstract class BaseVm
  {
    private class SetTransaction : IDisposable
    {
      private readonly BaseVm _parent;
      private readonly HashSet<Binding> _processed;

      public SetTransaction(BaseVm parent)
      {
        _parent = parent;
        _parent._transaction?.Dispose();
        _parent._transaction = this;

        _processed = new HashSet<Binding>();
      }
      public void Dispose()
      {
        _parent._transaction = null;
      }
      public bool TryAdd(Binding b)
      {
        return _processed.Add(b);
      }
    }
    private SetTransaction _transaction;

    private List<Binding> _bindings = new List<Binding>();
    public void AddBinding(Binding binding)
    {
      _bindings.Add(binding);
    }

    protected void Changed([CallerMemberName] string caller = "")
    {
      if (_transaction == null)
      {
        using (_transaction = new SetTransaction(this))
        { ApplyBindings(caller, _transaction); }
      }
      else
        ApplyBindings(caller, _transaction);
    }
    private void ApplyBindings(string caller, SetTransaction transaction)
    {
      foreach (Binding b in _bindings)
      {
        if (string.IsNullOrEmpty(caller) || caller == b.Property)
        {
          if (!transaction.TryAdd(b))
          { continue; }

          b.PushValue();
        }
      }
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

  [Obsolete("move")]
  public abstract class Binding
  {
    private readonly BaseVm _vm;
    private readonly string _property;
    private readonly MethodInfo _getMethod;
    private readonly MethodInfo _setMethod;

    protected Binding(BaseVm vm, string property)
    {
      _vm = vm;
      _property = property;

      _getMethod = vm.GetType().GetProperty(property).GetGetMethod();
      _setMethod = vm.GetType().GetProperty(property).GetSetMethod();

      _vm.AddBinding(this);
    }
    public string Property => _property;
    protected BaseVm Vm => _vm;
    protected MethodInfo SetMethod => _setMethod;

    public Action DisposeFct { get; set; }

    public bool PushValue()
    {
      object value = _getMethod.Invoke(_vm, new object[] { });
      bool pushed = PushValueCore(value);
      return pushed;
    }
    protected abstract bool PushValueCore(object value);
  }

  [Obsolete("move")]
  public class Binding<T> : Binding
  {
    private readonly Action<T> _setFunc;
    private readonly Func<T> _getFunc;

    public Binding(BaseVm vm, string property, Action<T> setFunc, Func<T> getFunc)
      : base(vm, property)
    {
      _setFunc = setFunc;
      _getFunc = getFunc;

      PushValue();
    }

    public void ValueChanged<TS, TA>(TS sender, TA arg)
    {
      SetMethod.Invoke(Vm, new object[] { _getFunc() });
    }

    protected override bool PushValueCore(object value)
    {
      T typedValue = (T)value;
      if (_getFunc().Equals(typedValue))
      { return false; }
      _setFunc(typedValue);
      return true;
    }
  }

  public static class DepProps
  {
    public static Binding<int> BindToProgress(this SeekBar seekBar, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<int> b = new Binding<int>(vm, prop, (t) => seekBar.Progress = t, () => seekBar.Progress);
      seekBar.ProgressChanged += b.ValueChanged;
      b.DisposeFct = () => seekBar.ProgressChanged -= b.ValueChanged;

      return b;
    }

    public static Binding<bool> BindToChecked(this CheckBox checkBox, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<bool> b = new Binding<bool>(vm, prop, (t) => checkBox.Checked = t, () => checkBox.Checked);
      checkBox.CheckedChange += b.ValueChanged;
      b.DisposeFct = () => checkBox.CheckedChange -= b.ValueChanged;

      return b;
    }

    public static Binding<string> BindToText(this TextView textView, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<string> b = new Binding<string>(vm, prop, (t) => textView.Text = t, () => textView.Text);
      textView.TextChanged += b.ValueChanged;
      b.DisposeFct = () => textView.TextChanged -= b.ValueChanged;

      return b;
    }

  }
}