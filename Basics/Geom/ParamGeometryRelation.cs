using System;
using System.Collections.Generic;
using System.Text;

namespace Basics.Geom
{
  public class ParamGeometryRelation
  {

    private class ParentGeometry
    {
      public IGeometry Geometry;
      public int ChildIndex;
      public ParentGeometry(IGeometry geom, int childIndex)
      {
        Geometry = geom;
        ChildIndex = childIndex;
      }
      public override string ToString()
      {
        string s = string.Format("{0},{1}", Geometry.GetType().Name, ChildIndex);
        return s;
      }
    }

    private class Info
    {
      public readonly IParamGeometry Geometry;
      public readonly IPoint Param;
      private LinkedList<ParentGeometry> _parents;
      private LinkedListNode<ParentGeometry> _parent;

      public Info(IParamGeometry geometry, IPoint param)
      {
        Geometry = geometry;
        Param = param;
      }

      public override string ToString()
      {
        if (_parents == null)
        { return Param.ToString(); }
        LinkedListNode<ParentGeometry> n = _parents.Last;
        StringBuilder s = new StringBuilder(n.Value.ToString());
        while (n.Previous != null)
        {
          n = n.Previous;
          s.AppendFormat("->{0}", n.Value);
        }
        s.AppendFormat(":{0}", Param);
        return s.ToString();
      }
      public IGeometry Current
      {
        get
        {
          if (_parent != null)
          { return _parent.Value.Geometry; }
          else
          { return Geometry; }
        }
      }

      public LinkedList<ParentGeometry> Hierarchy
      {
        get { return _parents; }
      }
      public bool TryAddParent(IGeometry parent, int childIndex, IGeometry child)
      {
        bool result;
        if (Geometry != null && (child == Geometry || Geometry.EqualGeometry(child)))
        {
          if (_parents != null)
          { throw new InvalidOperationException("Parent already added"); }
          _parents = new LinkedList<ParentGeometry>();
          _parents.AddFirst(new ParentGeometry(parent, childIndex));

          result = true;
        }
        else if (_parents != null && _parents.Last.Value.Geometry == child)
        {
          _parents.AddLast(new ParentGeometry(parent, childIndex));
          result = true;
        }
        else
        { result = false; }

        return result;
      }

      public bool TryGetChildRelation(IGeometry parent,
        ParamGeometryRelation rel, Info asY, out ParamGeometryRelation result)
      {
        result = null;
        Info resultInfo;
        if (!TryGetInfo(parent, out resultInfo))
        { return false; }

        if (resultInfo != null)
        {
          result = new ParamGeometryRelation();

          result._isWithin = rel._isWithin;
          result._intersect = rel._intersect;
          result._offset = rel._offset;
          result._offset2 = rel._offset2;

          resultInfo._parents = _parents;

          result._X = resultInfo;
          result._Y = asY;
        }
        else
        { result = null; }

        return true;
      }

      public bool TryGetInfo(IGeometry parent, out Info resultInfo)
      {
        resultInfo = null;

        if (_parent != null && parent.EqualGeometry(_parent.Value.Geometry))
        {
          resultInfo = new Info(Geometry, Param);
          resultInfo._parent = _parent.Previous;
        }
        else if (_parent == null && _parents != null && parent.EqualGeometry(_parents.Last.Value.Geometry))
        {
          resultInfo = new Info(Geometry, Param);
          resultInfo._parent = _parents.Last.Previous;
        }
        else if (_parent == null && parent.EqualGeometry(Geometry))
        { }
        else
        {
          return false;
        }
        return true;
      }

      public void SetFirstParent(IGeometry first)
      {
        if (_parents != null)
        { throw new InvalidOperationException("parents already set"); }
        _parents = new LinkedList<ParentGeometry>();
        _parents.AddFirst(new ParentGeometry(first, -1));
      }

      public int CompareTo(Info other)
      {
        LinkedListNode<ParentGeometry> thisParent = _parents.Last;
        LinkedListNode<ParentGeometry> otherParent = other._parents.Last;
        while (true)
        {
          if (thisParent == null)
          {
            if (otherParent == null)
            {
              break;
            }
            return 1;
          }
          if (otherParent == null)
          {
            return -1;
          }
          int i = thisParent.Value.ChildIndex.CompareTo(otherParent.Value.ChildIndex);
          if (i != 0)
          { return i; }

          thisParent = thisParent.Previous;
          otherParent = otherParent.Previous;
        }

        if (Param == null)
        {
          if (other.Param == null)
          { return 0; }
          return 1;
        }
        if (other.Param == null)
        { return -1; }
        int f = Param.X.CompareTo(other.Param.X);
        return f;
      }
    }

    private class EqualParamGeometry : IEqualityComparer<ParamGeometryRelation>
    {
      public bool Equals(ParamGeometryRelation x, ParamGeometryRelation y)
      {
        if (x == y) return true;
        if (x._X.Param != null && x._Y.Param != null)
        {
          if (x._X.Param == y._X.Param && x._Y.Param == y._Y.Param) return true;
          if (x._X.Param == y._Y.Param && x._Y.Param == y._X.Param) return true;
          return false;
        }
        if (x._X.Hierarchy.Count == 1 && x._Y.Hierarchy.Count == 1 &&
          x._X.Hierarchy.First.Value.Geometry == x._Y.Hierarchy.First.Value.Geometry)
        {
          if (y._X.Hierarchy.Count == 1 && y._Y.Hierarchy.Count == 1 &&
            y._X.Hierarchy.First.Value.Geometry == y._Y.Hierarchy.First.Value.Geometry)
          {
            bool equals = x._X.Hierarchy.First.Value == y._X.Hierarchy.First.Value;
            return equals;
          }
        }
        return false;
      }

      public int GetHashCode(ParamGeometryRelation obj)
      {
        if (obj._X == null || obj._X.Param == null) return base.GetHashCode();
        if (obj._Y == null || obj._Y.Param == null) return base.GetHashCode();

        int code = obj._X.Param.GetHashCode() + obj._Y.Param.GetHashCode();
        return code;
      }
    }

    private List<ParamGeometryRelation> _borderRelations;
    private Info _X;
    private Info _Y;
    private IPoint _offset;

    private IGeometry _intersect;
    private bool? _isWithin;
    double _offset2 = -1;

    private ParamGeometryRelation()
    { }

    public ParamGeometryRelation(IParamGeometry x, IParamGeometry y,
      IGeometry intersection)
    {
      _X = new Info(x, null);
      _Y = new Info(y, null);
      Intersection = intersection;
    }

    public static ParamGeometryRelation CreateWithin(IGeometry contains, IGeometry within)
    {
      ParamGeometryRelation rel = new ParamGeometryRelation(null, null, within);
      rel._X = new Info(null, null);
      rel._X.SetFirstParent(contains);
      rel._Y = new Info(null, null);
      rel._Y.SetFirstParent(within);

      return rel;
    }
    public ParamGeometryRelation(IParamGeometry x, IPoint xParam,
      IParamGeometry y, IPoint yParam, IPoint offset)
    {
      _X = new Info(x, xParam);
      _Y = new Info(y, yParam);
      _offset = offset;
    }

    public void AddParent(IGeometry parent, int childIndex, IGeometry child)
    {
      if (_X.TryAddParent(parent, childIndex, child))
      { }
      else if (_Y.TryAddParent(parent, childIndex, child))
      { }
      else
      { throw new InvalidOperationException("child not valid"); }
    }

    public ParamGeometryRelation GetChildRelation(IGeometry parent)
    {
      ParamGeometryRelation relation;
      if (_X.TryGetChildRelation(parent, this, _Y, out relation))
      { }
      else if (_Y.TryGetChildRelation(parent, this, _X, out relation))
      { }
      else
      { throw new InvalidOperationException("child not valid"); }
      return relation;
    }

    public IPoint XParam
    {
      get { return _X.Param; }
    }

    public IGeometry CurrentX
    {
      get { return _X.Current; }
    }

    public IGeometry Intersection
    {
      get
      {
        if (IsWithin)
        { return IntersectionAnywhere; }
        else
        { return null; }
      }
      set
      {
        _intersect = value;
        if (value != null)
        {
          _isWithin = true;
          _offset2 = 0;
        }
        else
        {
          _isWithin = false;
          _offset2 = 1;
        }
      }
    }

    public IGeometry IntersectionAnywhere
    {
      get
      {
        if (_intersect == null)
        {
          if (Offset2 == 0 && _X.Param != null)
          { _intersect = _X.Geometry.PointAt(_X.Param); }
        }
        return _intersect;
      }
    }

    public bool IsWithin
    {
      get
      {
        if (_isWithin == null)
        { _isWithin = CheckRange(_X.Param) && CheckRange(_Y.Param); }
        return _isWithin.Value;
      }
    }

    private bool CheckRange(IPoint p)
    {
      if (p == null)
      { return false; }

      int dim = p.Dimension;
      for (int i = 0; i < dim; i++)
      {
        double x = p[i];
        if (x < 0 || x > 1)
        { return false; }
      }
      return true;
    }

    public double Offset2
    {
      get
      {
        if (_offset2 < 0)
        {
          _offset2 = PointOperator.OrigDist2(_offset);
          if (_offset2 < 1.0e-12)
          { _offset2 = 0; }
        }
        return _offset2;
      }
    }

    public static void Sort(List<ParamGeometryRelation> relations, IGeometry parentGeometry)
    {
      SortComparer cmp = new SortComparer(parentGeometry);
      relations.Sort(cmp);
    }
    private Info GetInfo(IGeometry parent)
    {
      Info i;
      Info rel;
      if (_X.TryGetInfo(parent, out rel))
      { i = _X; }
      else if (_Y.TryGetInfo(parent, out rel))
      { i = _Y; }
      else { throw new InvalidOperationException("Unknown Parent"); }
      return i;
    }

    private class SortComparer : IComparer<ParamGeometryRelation>
    {
      private readonly IGeometry _parent;

      public SortComparer(IGeometry parentGeometry)
      { _parent = parentGeometry; }

      public int Compare(ParamGeometryRelation x, ParamGeometryRelation y)
      {
        if (x == null)
        {
          if (y == null)
          { return 0; }
          return 1;
        }
        if (y == null)
        { return -1; }
        Info ix = x.GetInfo(_parent);
        Info iy = y.GetInfo(_parent);
        int i = ix.CompareTo(iy);
        return i;
      }
    }

    private static int SortLinePt(ParamGeometryRelation x, ParamGeometryRelation y)
    {
      return ((IPoint)x.Intersection).X.CompareTo(((IPoint)y.Intersection).X);
    }
    public static ParamGeometryRelation GetLineRelation(IParamGeometry xLine, IGeometry y)
    {
      IParamGeometry yLine = y as IParamGeometry;
      if (yLine == null)
      { throw new NotImplementedException(); }


      IPoint p0 = Point.Create(new double[] { 0 });
      IPoint p1 = Point.Create(new double[] { 1 });

      IPoint x0 = xLine.PointAt(p0);
      IPoint x1 = xLine.PointAt(p1);
      IPoint dx = PointOperator.Sub(x1, x0);
      IPoint y0 = xLine.PointAt(p0);
      IPoint y1 = xLine.PointAt(p1);
      IPoint dy = PointOperator.Sub(y1, y0);
      double xy0 = PointOperator.SkalarProduct(dx, PointOperator.Sub(y0, x0));
      double xy1 = PointOperator.SkalarProduct(dx, PointOperator.Sub(y1, x0));
      double yx0 = PointOperator.SkalarProduct(dy, PointOperator.Sub(x0, y0));
      double yx1 = PointOperator.SkalarProduct(dy, PointOperator.Sub(x1, y0));

      List<ParamGeometryRelation> rels = new List<ParamGeometryRelation>(4);
      ParamGeometryRelation r0 = new ParamGeometryRelation(xLine, p0, yLine, Point.Create(new double[] { yx0 }), null);
      ParamGeometryRelation r1 = new ParamGeometryRelation(xLine, p1, yLine, Point.Create(new double[] { yx1 }), null);
      ParamGeometryRelation r2 = new ParamGeometryRelation(xLine, Point.Create(new double[] { xy0 }), yLine, p0, null);
      ParamGeometryRelation r3 = new ParamGeometryRelation(xLine, Point.Create(new double[] { xy1 }), yLine, p1, null);
      rels.Add(r0);
      rels.Add(r1);
      rels.Add(r2);
      rels.Add(r3);

      rels.Sort(SortLinePt);
      double t = ((IPoint)rels[1].IntersectionAnywhere).X;
      if (Math.Abs(Math.Sign(t - x0.X) + Math.Sign(t - x1.X)) > 1 ||
        Math.Abs(Math.Sign(t - y0.X) + Math.Sign(t - y1.X)) > 1)
      { return null; }

      if (((IPoint)rels[2].IntersectionAnywhere).X == t)
      { return rels[1]; }

      Line intersect = new Line(
        Point.Create((IPoint)rels[1].Intersection),
        Point.Create((IPoint)rels[2].Intersection));
      ParamGeometryRelation rel = new ParamGeometryRelation(xLine, yLine, intersect);
      rel._borderRelations = new List<ParamGeometryRelation>(2);
      rel._borderRelations.Add(rels[1]);
      rel._borderRelations.Add(rels[2]);

      return rel;
    }

    public static List<ParamGeometryRelation> Assemble(IGeometry x, IGeometry y, IList<ParamGeometryRelation> borderRelations)
    {
      Area area = x as Area;
      if (area == null)
      { return null; }

      List<ParamGeometryRelation> sortY;
      if (borderRelations != null)
      { sortY = new List<ParamGeometryRelation>(borderRelations); }
      else
      { sortY = new List<ParamGeometryRelation>(0); }
      Sort(sortY, y);

      List<ParamGeometryRelation> assembled = new List<ParamGeometryRelation>();

      if (y is Area)
      {
        if (borderRelations != null)
        {
          List<ParamGeometryRelation> childRels = new List<ParamGeometryRelation>(borderRelations.Count * 2);
          List<Polyline> fullBorders = new List<Polyline>(borderRelations.Count);
          foreach (ParamGeometryRelation xBorder in borderRelations)
          {
            if (xBorder._borderRelations != null)
            {
              foreach (ParamGeometryRelation borderRel in xBorder._borderRelations)
              {
                if (borderRel != null)
                { childRels.Add(borderRel); }
              }
            }
            else
            {
              fullBorders.Add((Polyline)xBorder.Intersection);
            }
          }
          List<ParamGeometryRelation> yBorders = Assemble(x, y.Border, childRels);
          foreach (ParamGeometryRelation yRel in yBorders)
          {
            if (yRel._borderRelations == null) // Line completly within x
            { fullBorders.Add((Polyline)yRel.Intersection); }
          }
          Area intersect = BuildPolyons(borderRelations, yBorders);
          intersect.Border.AddRange(fullBorders);

          ParamGeometryRelation rel = CreateWithin(x, y);
          rel.Intersection = intersect;
          rel._borderRelations = new List<ParamGeometryRelation>(borderRelations.Count + yBorders.Count);
          rel._borderRelations.AddRange(borderRelations);
          rel._borderRelations.AddRange(yBorders);
          assembled.Add(rel);
        }
        else
        { CheckAddPart(x, y, assembled); }
      }
      else if (y is PolylineCollection)
      {
        PolylineCollection lines = (PolylineCollection)y;
        int nLines = lines.Count;
        Dictionary<int, List<ParamGeometryRelation>> parts = new Dictionary<int, List<ParamGeometryRelation>>(nLines);
        foreach (ParamGeometryRelation rel in sortY)
        {
          Info yInfo = rel.GetInfo(y);
          int i = yInfo.Hierarchy.Last.Value.ChildIndex;
          List<ParamGeometryRelation> rels;
          if (!parts.TryGetValue(i, out rels))
          {
            rels = new List<ParamGeometryRelation>();
            parts.Add(i, rels);
          }
          rels.Add(rel);
        }
        for (int i = 0; i < nLines; i++)
        {
          Polyline line = lines[i];
          List<ParamGeometryRelation> part;
          if (parts.TryGetValue(i, out part))
          {
            List<ParamGeometryRelation> splits = new List<ParamGeometryRelation>(part.Count);
            foreach (ParamGeometryRelation split in part)
            {
              ParamGeometryRelation s = split.GetChildRelation(y);
              splits.Add(s);
            }
            GetLineParts(x, line, splits, assembled);
          }
          else
          {
            CheckAddPart(x, line, assembled);
          }
        }
      }
      else if (y is Polyline || y is Curve)
      {
        if (borderRelations != null)
        {
          GetLineParts(x, y, sortY, assembled);
        }
        else
        {
          CheckAddPart(x, y, assembled);
        }
      }
      else if (y is IPoint)
      {
        CheckAddPart(x, y, assembled);
      }
      else if (y is PointCollection)
      {
        return null;
      }
      else if (y is IBox)
      { return null; }
      else
      { throw new NotImplementedException("Unhandled type " + y.GetType()); }
      if (assembled.Count == 0)
      { return null; }
      return assembled;
    }

    private static void CheckAddPart(IGeometry x, IGeometry y, List<ParamGeometryRelation> assembled)
    {
      IGeometry p = GeometryOperator.GetFirstPoint(y);
      if (p != null)
      {
        bool within = x.IsWithin((IPoint)p);
        if (within)
        {
          ParamGeometryRelation rel = CreateWithin(x, y);
          assembled.Add(rel);
        }
      }
    }

    private static Area BuildPolyons(IList<ParamGeometryRelation> xBorders,
      List<ParamGeometryRelation> yBorders)
    {
      EqualParamGeometry cmp = new EqualParamGeometry();

      Dictionary<ParamGeometryRelation, ParamGeometryRelation[]> rels = GetBorderRelations(xBorders, yBorders, cmp);
      PolylineCollection borders = new PolylineCollection();
      while (rels.Count > 0)
      {
        ParamGeometryRelation start = null;
        foreach (ParamGeometryRelation s in rels.Keys)
        {
          start = s;
          break;
        }
        if (start == null) throw new InvalidOperationException();

        ParamGeometryRelation pre = null;
        Polyline border = new Polyline();
        while (start != null)
        {

          ParamGeometryRelation[] connected;
          if (!rels.TryGetValue(start, out connected))
          { break; }

          ParamGeometryRelation b;
          ParamGeometryRelation next;
          if (connected[0] != pre)
          { b = connected[0]; }
          else
          { b = connected[1]; }
          if (cmp.Equals(b._borderRelations[0], start))
          {
            next = b._borderRelations[1];
          }
          else if (cmp.Equals(b._borderRelations[1], start))
          {
            next = b._borderRelations[0];
          }
          else
          { throw new InvalidProgramException(); }
          pre = b;

          Polyline line = (Polyline)b.Intersection;
          if (border.Points.Count == 0)
          { }
          else
          {
            Point end = Point.CastOrCreate(border.Points.Last.Value);
            if (border.Points.Count == 0 || end.EqualGeometry(line.Points.First.Value))
            { }
            else if (end.EqualGeometry(line.Points.Last.Value))
            {
              line = line.Clone().Invert();
            }
            else
            {
              double d02 = end.Dist2(line.Points.First.Value);
              double d12 = end.Dist2(line.Points.Last.Value);
              line = line.Clone();
              if (d02 < d12 * 1e-12)
              { }
              else if (d12 < d02 * 1e-12)
              { line = line.Invert(); }
              else throw new NotImplementedException();
              line.Points.First.Value = end;
            }
          }
          foreach (Curve curve in line.Segments)
          {
            border.Add(curve);
          }

          rels.Remove(start);
          start = next;
        }

        borders.Add(border);
      }
      Area area = new Area(borders);
      return area;
    }

    private static Dictionary<ParamGeometryRelation, ParamGeometryRelation[]> GetBorderRelations(
      IList<ParamGeometryRelation> xborders, IList<ParamGeometryRelation> yborders,
      EqualParamGeometry cmp)
    {
      Dictionary<Polyline, ParamGeometryRelation> startEnds = new Dictionary<Polyline, ParamGeometryRelation>();
      Dictionary<ParamGeometryRelation, ParamGeometryRelation[]> rels =
        new Dictionary<ParamGeometryRelation, ParamGeometryRelation[]>(cmp);
      foreach (ParamGeometryRelation rel in xborders)
      {
        if (rel._borderRelations == null)
        {
          System.Diagnostics.Trace.WriteLine("Unexpected code");
          continue;
        }

        for (int i = 0; i < rel._borderRelations.Count; i++)
        {
          ParamGeometryRelation bordRel = rel._borderRelations[i];
          ParamGeometryRelation b = bordRel;
          if (b == null)
          {
            b = GetBorderRelations(startEnds, rel);
            rel._borderRelations[i] = b;
            if (rels.ContainsKey(b))
            {
              rels[b][1] = rel;
            }
            else
            {
              rels.Add(b, new ParamGeometryRelation[] { rel, null });
            }
            continue;
          }
          rels.Add(b, new ParamGeometryRelation[] { rel, null });
        }
      }
      foreach (ParamGeometryRelation rel in yborders)
      {
        if (rel._borderRelations == null)
        { continue; }

        for (int i = 0; i < rel._borderRelations.Count; i++)
        {
          ParamGeometryRelation bordRel = rel._borderRelations[i];

          ParamGeometryRelation b = bordRel;
          if (b == null)
          {
            b = GetBorderRelations(startEnds, rel);
            rel._borderRelations[i] = b;
            if (!rels.ContainsKey(b))
            {
              rels.Add(b, new ParamGeometryRelation[] { rel, null });
            }
            else
            {
              rels[b][1] = rel;
            }
            continue;
          }

          ParamGeometryRelation[] xRel = rels[b];
          xRel[1] = rel;
        }
      }

      return rels;
    }

    private static ParamGeometryRelation GetBorderRelations(Dictionary<Polyline, ParamGeometryRelation> startEnds,
      ParamGeometryRelation rel)
    {
      Polyline line;
      if (rel._X.Hierarchy.Last.Value.Geometry is Polyline)
      { line = (Polyline)rel._X.Hierarchy.Last.Value.Geometry; }
      else if (rel._Y.Hierarchy.Last.Value.Geometry is Polyline)
      { line = (Polyline)rel._Y.Hierarchy.Last.Value.Geometry; }
      else
      { return null; }

      ParamGeometryRelation s;
      if (!startEnds.TryGetValue(line, out s))
      {
        s = CreateWithin(line, line);
        s.Intersection = GeometryOperator.GetFirstPoint(line);
        startEnds.Add(line, s);
      }
      return s;
    }

    private static void GetLineParts(IGeometry x, IGeometry y, List<ParamGeometryRelation> sortY,
      List<ParamGeometryRelation> assembled)
    {
      IGeometry p = GeometryOperator.GetFirstPoint(y);
      bool within;
      if (p != null)
      { within = x.IsWithin((IPoint)p); }
      else
      { return; }

      System.Collections.IEnumerable parts;
      if (y is Polyline)
      {
        parts = ((Polyline)y).Split(sortY);
      }
      else
      {
        parts = ((Curve)y).Split(sortY);
      }

      int i = 0;
      foreach (IGeometry part in parts)
      {
        if ((i % 2 == 0) != within)
        {
          i++;
          continue;
        }
        ParamGeometryRelation start = null;
        if (i > 0) start = sortY[i - 1];
        ParamGeometryRelation end = null;
        if (i < sortY.Count) end = sortY[i];

        ParamGeometryRelation r = CreateWithin(x, y);
        r._borderRelations = new List<ParamGeometryRelation>();
        r._borderRelations.Add(start);
        r._borderRelations.Add(end);
        r.Intersection = part;

        assembled.Add(r);
        i++;
      }
    }
  }
}
