using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Basics.ViewModels
{
  public enum BindingMode { OneTime, OneWay, TwoWay };

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
      bool pushed = false;
      Views.Utils.Try(() =>
      {
        object value = _getMethod.Invoke(_vm, new object[] { });
        pushed = PushValueCore(value);
      });
      return pushed;
    }
    protected abstract bool PushValueCore(object value);
  }

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
      Views.Utils.Try(() =>
      {
        SetMethod?.Invoke(Vm, new object[] { _getFunc() });
      });
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

}