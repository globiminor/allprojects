using System;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace Basics.Forms
{
  public static class BindUtils
  {
    public static Binding Bind<T, S>(this T obj, Expression<Func<T, S>> expr, BindingSource bindingSource,
      string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode)
      where T : IBindableComponent
    {
      string objProp = ((MemberExpression)expr.Body).Member.Name;
      Binding binding = new Binding(objProp, bindingSource, dataMember, formattingEnabled, updateMode);
      obj.DataBindings.Add(binding);

      return binding;
    }

  }
}
