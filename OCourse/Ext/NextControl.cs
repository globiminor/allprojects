using System;
using System.Collections.Generic;
using Ocad;

namespace OCourse.Ext
{
  public class NextControl
  {
    private string _where;
    public class EqualityComparer : IEqualityComparer<NextControl>
    {
      public bool Equals(NextControl x, NextControl y)
      {
        bool equals = x.Control == y.Control && x.Where == y.Where;
        return equals;
      }

      public int GetHashCode(NextControl obj)
      {
        int code = obj.Control.Name.GetHashCode();
        if (!string.IsNullOrEmpty(obj.Where))
        { code += 29 * obj.Where.GetHashCode(); }
        return code;
      }
    }

    public class EqualControlComparer : IEqualityComparer<NextControl>
    {
      public bool Equals(NextControl x, NextControl y)
      {
        bool equals = x.Control == y.Control;
        return equals;
      }

      public int GetHashCode(NextControl obj)
      {
        int code = obj.Control.Name.GetHashCode();
        return code;
      }
    }

    public class EqualCodeComparer : IEqualityComparer<NextControl>
    {
      public bool Equals(NextControl x, NextControl y)
      {
        bool equals = x.Control == y.Control && x.Code == y.Code;
        return equals;
      }

      public int GetHashCode(NextControl obj)
      {
        int code = obj.Control.Name.GetHashCode();
        if (!string.IsNullOrEmpty(obj.Where))
        { code += 29 * obj.Code.GetHashCode(); }
        return code;
      }
    }

    public string Where
    {
      get { return _where; }
      set { _where = value; }
    }
    public Control Control { get; set; }
    public char Code { get; set; }
    public List<Control> Inter { get; private set; }

    public NextControl()
    { }
    public NextControl(Control control)
    {
      Control = control;
      Inter = new List<Control>();
    }

    public override string ToString()
    {
      return string.Format("{0} {1}", Control, Where);
    }
  }
}