
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
    private string _name;
    private char _code;
    private int _symbol;
    private Element _element;
    private string _text;

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
      _name = name;
    }

    public Control(string name, char code)
    {
      _name = name;
      _code = code;
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
      clone._name = _name;
      clone._code = _code;
      clone._symbol = _symbol;
      clone._element = _element;
      clone._text = _text;
      return clone;
    }

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
    /// <summary>
    /// Start: s
    /// Control: c
    /// Finish: f
    /// Pflicht: m
    /// </summary>
    public char Code
    {
      get { return _code; }
      set { _code = value; }
    }

    public int Symbol
    {
      get { return _symbol; }
      set { _symbol = value; }
    }

    public Element Element
    {
      get { return _element; }
      set { _element = value; }
    }

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    public override string ToString()
    {
      return string.Format("{0,3} {1,4}", _name, _code);
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
}
