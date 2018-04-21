
using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Ocad
{
  /// <summary>
  /// Summary description for Control.
  /// </summary>
  public class Control : ISection
  {
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
          control.Code = value[1];
        }
      }
      return control;
    }

    public Control(string name)
    {
      Name = name;
    }

    public Control(string name, char code)
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
    /// <summary>
    /// Start: s
    /// Control: c
    /// Finish: f
    /// Pflicht: m
    /// </summary>
    public char Code { get; set; }

    public int Symbol { get; set; }

    public Element Element { get; set; }

    public string Text { get; set; }

    public override string ToString()
    {
      return string.Format("{0,3} {1,4}", Name, Code);
    }

    public IPoint GetPoint()
    {
      IGeometry geom = Element.Geometry;
      IPoint point;
      if (geom is IPoint)
      { point = (IPoint)geom; }
      else if (geom is PointCollection)
      { point = ((PointCollection)geom)[0]; }
      else if (geom is Polyline)
      { point = ((Polyline)geom).Points.First.Value; }
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
