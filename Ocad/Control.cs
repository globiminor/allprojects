
using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Ocad
{
  /// <summary>
  /// Start: s
  /// Control: c
  /// Finish: f
  /// Pflicht: m
  /// TextDescription: t
  /// </summary>
  public enum ControlCode
  {
    Start = 's',
    Control = 'c',
    Finish = 'f',
    MarkedRoute = 'm',
    TextBlock = 't',
    MapChange = 'g'
  }

  /// <summary>
  /// Summary description for Control.
  /// </summary>
  public class Control : ISection
  {
    public class NameComparer : IEqualityComparer<Control>
    {
      public bool Equals(Control x, Control y)
      {
        return x.Name.Equals(y.Name);
      }

      public int GetHashCode(Control obj)
      {
        return obj.Name.GetHashCode();
      }
    }
    public Control()
    { }
    public static Control FromStringParam(string param)
    {
      IList<string> values = param.Split('\t');
      Control control = new Control(values[0]);
      int nValues = values.Count;
      for (int iValue = 1; iValue < nValues; iValue++)
      {
        string value = values[iValue];
        if (value[0] == 'Y')
        {
          control.Code = (ControlCode)value[1];
        }
      }
      return control;
    }

    public Control(string name)
    {
      Name = name;
    }

    public Control(string name, ControlCode code)
    {
      Name = name;
      Code = code;
    }

    object ICloneable.Clone()
    { return Clone(); }
    ISection ISection.Clone()
    { return Clone(); }
    public Control Clone()
    {
      Control clone = CloneCore();
      return clone;
    }
    protected virtual Control CloneCore()
    {
      Control clone = (Control)Activator.CreateInstance(GetType(), true);
      clone.Name = Name;
      clone.Code = Code;
      clone.Symbol = Symbol;
      clone.Element = Element;
      clone.Text = Text;
      return clone;
    }

    public string Name { get; set; }
    public ControlCode Code { get; set; }

    public int Symbol { get; set; }

    public GeoElement Element { get; set; }

    public string Text { get; set; }

    public override string ToString()
    {
      return string.Format("{0,3} {1,4}", Name, Code);
    }

    public IPoint GetPoint()
    {
      GeoElement.Geom geom = Element.Geometry;
      IPoint point;
      if (geom is GeoElement.Point p)
      { point = p.BaseGeometry; }
      else if (geom is GeoElement.Points pts)
      { point = pts.BaseGeometry[0]; }
      else if (geom is GeoElement.Line l)
      { point = l.BaseGeometry.Points[0]; }
      else
      { throw new NotImplementedException("Unhandled geometry type " + geom.GetType()); }
      return point;
    }
  }

  public class ControlInfo
  {
    public string Key { get; set; }
    public string Info { get; set; }
  }

}
