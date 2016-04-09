using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Basics.Views
{
  public class EnumText
  {
    public string Text { get; set; }

    public static string GetText<T>(IEnumerable<EnumText<T>> lookup, T id)
    {
      foreach (EnumText<T> enumText in lookup)
      {
        if (enumText.Id.Equals(id))
        { return enumText.Text; }
      }
      return null;
    }
  }

  public class EnumText<E> : EnumText
  {
    public EnumText() { }
    public EnumText(string text, E id)
    {
      Text = text;
      Id = id;
    }
    public E Id { get; set; }
  }
}
