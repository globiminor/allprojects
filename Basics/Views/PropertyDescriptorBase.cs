using System;
using System.ComponentModel;

namespace Basics.Views
{
  public abstract class PropertyDescriptorBase : PropertyDescriptor
  {
    protected PropertyDescriptorBase(string name, Attribute[] attrs)
      : base(name, attrs)
    { }

    public override bool CanResetValue(object component)
    {
      throw new NotImplementedException();
    }

    public override void ResetValue(object component)
    {
      throw new NotImplementedException();
    }

    public override void SetValue(object? component, object? value)
    {
      throw new NotImplementedException();
    }

    public override bool ShouldSerializeValue(object component)
    {
      throw new NotImplementedException();
    }

    public override Type ComponentType
    {
      get { throw new NotImplementedException(); }
    }

    public override bool IsReadOnly
    {
      get { return true; }
    }
  }
}
