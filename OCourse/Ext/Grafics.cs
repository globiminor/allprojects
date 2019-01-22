using System;
using System.Collections.Generic;
using System.Linq;
using Basics.Geom;
using Ocad;

namespace OCourse.Ext
{
  public class Grafics
  {
    private string _condition;
    private Element _elem;
    private int _replaceSymbol = -1;

    private IGeometry _geom;
    private string _fromName;
    private string _toName;

    private IPoint _fromPnt;
    private IPoint _toPnt;

    private Grafics()
    { }

    public string Key
    {
      get { return _fromName + "-" + _toName; }
    }

    public string FromName
    { get { return _fromName; } }
    public string ToName
    { get { return _toName; } }
    public IGeometry Geometry
    { get { return _geom; } }

    public int ReplaceSymbol
    {
      get
      {
        if (_replaceSymbol < 0)
        { return _elem.Symbol; }
        else
        { return _replaceSymbol; }
      }
    }
    public static List<Grafics> GetGrafics(OcadReader reader, Dictionary<int, int> symbols,
      Dictionary<string, Control> controls)
    {
      List<Grafics> lst = new List<Grafics>();
      foreach (var element in reader.EnumGeoElements(null))
      {
        if (symbols != null && symbols.ContainsKey(element.Symbol) == false)
        { continue; }

        Grafics g = Create(element, controls);
        if (symbols != null)
        {
          g._replaceSymbol = symbols[element.Symbol];
        }
        lst.Add(g);
      }
      return lst;
    }

    public static Grafics Create(GeoElement elem, Dictionary<string, Control> controls)
    {
      Grafics g = new Grafics();

      g._geom = elem.Geometry;
      g._elem = elem;
      string text = elem.Text.Replace("_", "").Replace(".", "");

      string[] parts = text.Split(';');
      if (parts.Length != 2) throw new InvalidOperationException(text);

      g._condition = parts[1];
      string[] where = parts[0].Split('-');
      if (where.Length > 2 || where.Length < 1) throw new InvalidOperationException(parts[0]);

      {
        g._fromName = where[0];
        Control cntFrom = controls[g._fromName];
        if (cntFrom.Element.Geometry is IPoint)
        { g._fromPnt = (IPoint)cntFrom.Element.Geometry; }
        else if (cntFrom.Element.Geometry is Polyline)
        { g._fromPnt = ((Polyline)cntFrom.Element.Geometry).Points.Last(); }
        else
        {
          throw new NotImplementedException("Unhandled geometry type " + cntFrom.Element.Geometry);
        }
      }

      if (where.Length > 1)
      {
        g._toName = where[1];
        Control cntTo = controls[g._toName];
        if (cntTo.Element.Geometry is IPoint)
        { g._toPnt = (IPoint)cntTo.Element.Geometry; }
        else if (cntTo.Element.Geometry is Polyline)
        { g._toPnt = ((Polyline)cntTo.Element.Geometry).Points[0]; }
        else
        {
          throw new NotImplementedException("Unhandled geometry type " + cntTo.Element.Geometry);
        }
      }

      return g;
    }

    private IList<Condition> GetConditions()
    {
      Condition cond = new Condition();
      cond.Code = (ControlCode)_condition[0];
      cond.Name = _condition.Substring(1);
      return new Condition[] { cond };
    }
    public bool IsApplicable(Course course, List<Control> controls)
    {
      foreach (var condition in GetConditions())
      {
        if (condition.IsTrue(course, controls) == false)
        { return false; }
      }
      return true;
    }

    private class Condition
    {
      public ControlCode Code;
      public string Name;

      public bool IsTrue(Course course, List<Control> controls)
      {
        foreach (var control in controls)
        {
          if (control.Code == Code &&
              control.Name == Name)
          {
            return true;
          }
        }
        return false;
      }
    }

    public void Apply(OcadWriter writer)
    {
      Apply(writer, _geom);
    }

    public void Apply(OcadWriter writer, IGeometry geoGeometry)
    {
      AdaptLine adapt = new AdaptLine(this, writer);
      writer.DeleteElements(adapt.DeleteLine);
      if (adapt.Line == null)
      { return; }

      adapt.Line.Geometry = geoGeometry;
      writer.Append(adapt.Line);
    }

    public enum ClipParam { AtStart, AtEnd }

    public Polyline Clip(Polyline geometry, GeoElement pnt, GeometryCollection symbol, ClipParam clip)
    {
      if (symbol == null)
      {
        return geometry;
      }
      IPoint pntGeom = (IPoint)pnt.Geometry;
      IList<Polyline> split = Ocad.Utils.Split(geometry, symbol, pntGeom, pnt.Angle);
      Polyline clipped;
      if (split == null)
      { clipped = geometry; }

      else if (clip == ClipParam.AtStart)
      {
        clipped = split[split.Count - 1];
      }
      else if (clip == ClipParam.AtEnd)
      {
        clipped = split[0];
      }
      else { throw new NotImplementedException("Unhandled clip " + clip); }

      return clipped;
    }

    public bool IsConnection(GeoElement elem)
    {
      Polyline line = (Polyline)elem.Geometry;

      double d2From = PointOperator.Dist2(_fromPnt, line.Points[0], GeometryOperator.DimensionXY);
      double d2To = PointOperator.Dist2(_toPnt, line.Points.Last(), GeometryOperator.DimensionXY);

      if (d2From > 200000)
      { return false; }

      if (d2To > 100000)
      { return false; }

      return true;
    }

    private class AdaptLine
    {
      private Grafics _parent;
      private OcadWriter _writer;

      public GeoElement Line;

      public AdaptLine(Grafics parent, OcadWriter writer)
      {
        _parent = parent;
        _writer = writer;
      }

      public bool DeleteLine(ElementIndex index)
      {
        if (index.Symbol != _parent.ReplaceSymbol)
        { return false; }

        _writer.ReadElement(index, out GeoElement elem);

        if (_parent.IsConnection(elem) == false)
        { return false; }

        if (Line != null) { throw new InvalidOperationException("Multiple Lines"); }

        Line = elem;
        return true;
      }
    }

  }
}
