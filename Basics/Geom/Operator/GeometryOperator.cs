using System;
using System.Collections.Generic;
using PntOp = Basics.Geom.PointOperator;

namespace Basics.Geom
{
  public static partial class GeometryOperator
  {
    #region nested classes

    private class PartEnumerable : IEnumerable<IList<IPoint>>
    {
      private class Enumerator : IEnumerator<IList<IPoint>>
      {
        private PartEnumerable _parent;

        private IEnumerator<PartEnumerable> _listEnumerator;
        private Enumerator _partEnumerator;
        private bool _first;

        public Enumerator(PartEnumerable enumerable)
        {
          _parent = enumerable;
          Reset();
        }

        public void Reset()
        {
          int nDim = _parent._dimensions.Count;
          if (nDim == 0)
          {
            _listEnumerator = null;
            _first = true;
            return;
          }

          int nPoints = _parent._points.Count;
          IPoint c = _parent._points[nPoints - 1];
          IList<PartEnumerable> childEnumerables
            = new List<PartEnumerable>(2 * nDim);
          for (int iDim = 0; iDim < nDim; iDim++)
          {
            int dim = _parent._dimensions[iDim];
            List<int> childDims = new List<int>(nDim - 1);
            for (int i = 0; i < nDim; i++)
            {
              if (i != iDim)
              { childDims.Add(_parent._dimensions[i]); }
            }
            foreach (var d in new double[] { _parent._range.Min[dim], _parent._range.Max[dim] })
            {
              Point p = Point.Create(c);
              p[dim] = d;

              List<IPoint> childPoints = new List<IPoint>(_parent._points) { p };

              PartEnumerable childEnum = new PartEnumerable(childPoints, childDims, _parent._range);
              childEnumerables.Add(childEnum);
            }
          }
          _listEnumerator = childEnumerables.GetEnumerator();
        }

        object System.Collections.IEnumerator.Current
        { get { return Current; } }
        public IList<IPoint> Current
        {
          get
          {
            if (_listEnumerator == null)
            {
              return _parent._points;
            }
            else
            {
              return _partEnumerator.Current;
            }
          }
        }

        public void Dispose()
        { }


        public bool MoveNext()
        {
          if (_listEnumerator == null)
          {
            if (_first)
            {
              _first = false;
              return true;
            }
            return false;
          }

          while (_partEnumerator == null || _partEnumerator.MoveNext() == false)
          {
            bool next = _listEnumerator.MoveNext();
            if (next == false)
            { return false; }

            PartEnumerable partEnumerable = _listEnumerator.Current;
            _partEnumerator = partEnumerable.GetEnumerator_();
          }
          return true;
        }

      }

      private IBox _range;
      private IList<IPoint> _points;
      private IList<int> _dimensions;

      private Point _center;

      public PartEnumerable(IBox range)
      {
        _range = range;

        _center = 0.5 * PntOp.Add(_range.Min, _range.Max);
        int dim = _center.Dimension;
        _dimensions = new int[dim];
        for (int i = 0; i < dim; i++)
        { _dimensions[i] = i; }

        _points = new List<IPoint> { _center };
      }

      private PartEnumerable(IList<IPoint> points, IList<int> dimensions, IBox range)
      {
        _points = points;
        _dimensions = dimensions;
        _range = range;
      }

      public IPoint Center
      {
        get { return _center; }
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      { return GetEnumerator(); }
      public IEnumerator<IList<IPoint>> GetEnumerator()
      { return GetEnumerator_(); }
      private Enumerator GetEnumerator_()
      {
        return new Enumerator(this);
      }
    }

    private class ParamInfo
    {
      private readonly IRelParamGeometry _paramGeom;
      private IPoint _min;
      private IPoint _delta;
      private SortedList<IPoint, IPoint> _pointsAt;

      public ParamInfo(IRelParamGeometry paraGeom)
      {
        _paramGeom = paraGeom;
      }

      public IRelParamGeometry BaseGeometry
      { get { return _paramGeom; } }

      public int Topology
      {
        get { return _paramGeom.Topology; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="param">Parameter of ParamInfo, not of base geometry</param>
      /// <returns></returns>
      public IPoint CachedPointAt(IPoint param)
      {
        if (_pointsAt == null)
        { _pointsAt = new SortedList<IPoint, IPoint>(new PointComparer()); }

        if (_pointsAt.TryGetValue(param, out IPoint pointAt) == false)
        {
          IPoint paramAt = GeometryParamAt(param);
          pointAt = _paramGeom.PointAt(paramAt);
          _pointsAt.Add(param, pointAt);
        }

        return pointAt;
      }

      private IPoint Min
      {
        get
        {
          if (_min == null)
          { _min = _paramGeom.ParameterRange.Min; }
          return _min;
        }
      }
      private IPoint Delta
      {
        get
        {
          if (_delta == null)
          { _delta = PntOp.Sub(_paramGeom.ParameterRange.Max, Min); }
          return _delta;
        }
      }

      public Point GeometryParamAt(IPoint param)
      {
        Point paramAt = Point.Create(param);
        for (int iDim = 0; iDim < param.Topology; iDim++)
        {
          paramAt[iDim] = Min[iDim] + param[iDim] * Delta[iDim];
        }
        return paramAt;
      }
    }

    private class ParamPartList : List<ParamPart>
    {
      private class CornerInfo
      {
        IList<ParamPart> _neighbors;
        Dictionary<ParamPart, IList<ParamsRelation>> _relations;

        public CornerInfo()
        {
          _neighbors = new List<ParamPart>();
        }

        public IList<ParamPart> Neighbors
        {
          get { return _neighbors; }
        }
        public void Add(ParamPart boxNeighbor)
        {
          _neighbors.Add(boxNeighbor);
        }
        public void AddSplit(ParamPart intersect, IList<ParamsRelation> relations)
        {
          if (_relations == null)
          { _relations = new Dictionary<ParamPart, IList<ParamsRelation>>(); }

          _relations.Add(intersect, relations);
        }
        public bool ContainsSplit(ParamPart intersect)
        {
          if (_relations == null)
          { return false; }
          bool contains = _relations.ContainsKey(intersect);
          return contains;
        }
      }

      private readonly ParamInfo _geometry;
      private readonly IPoint _center;

      private SortedList<IPoint, CornerInfo> _corners;

      public ParamPartList(ParamInfo geometry, IPoint center)
      {
        _geometry = geometry;
        _center = center;
      }

      private SortedList<IPoint, CornerInfo> Corners
      {
        get
        {
          if (_corners == null)
          {
            _corners = new SortedList<IPoint, CornerInfo>(new PointComparer());
            foreach (var part in this)
            {
              IPoint corner = part.BoxCorner;
              if (_corners.TryGetValue(corner, out CornerInfo cornerInfo) == false)
              {
                cornerInfo = new CornerInfo();
                _corners.Add(corner, cornerInfo);
              }
              cornerInfo.Add(part);
            }
          }
          return _corners;
        }
      }

      public IPoint Center
      { get { return _center; } }

      public void Split(IPoint cornerPoint, ParamPart yPart, List<ParamsRelation> parentList)
      {
        CornerInfo cornerInfo = Corners[cornerPoint];

        IList<ParamPart> subParts = ParamPart.CreateParts(_geometry, Point.Create(_center),
          Point.Create(cornerPoint));

        List<ParamsRelation> relations = new List<ParamsRelation>();
        foreach (var subPart in subParts)
        {
          ParamsRelation relation = new ParamsRelation(subPart, yPart, parentList);
          parentList.Add(relation);
        }
        cornerInfo.AddSplit(yPart, relations);
      }

      public bool IsSplit(IPoint cornerPoint, ParamPart yPart)
      {
        if (_corners == null)
        { return false; }

        CornerInfo corner = Corners[cornerPoint];

        return corner.ContainsSplit(yPart);
      }
    }

    private class ParamPart
    {
      private ParamInfo _geometry;
      private IList<IPoint> _cornerParams;
      private IBox _paramBox;

      private IPoint _origin;
      private List<IPoint> _axes;
      private OrthogonalSystem _orthoSystem;

      private readonly ParamPartList _parentList;
      private double _maxOffset = -1;

      public static IList<ParamPart> CreateParts(ParamInfo x)
      {
        int dimension = x.Topology;
        IPoint min = x.BaseGeometry.ParameterRange.Min;
        IPoint max = x.BaseGeometry.ParameterRange.Max;

        if (x.BaseGeometry.NormedMaxOffset == 0)
        {
          IList<IPoint> corners = new List<IPoint>(dimension + 1);
          Point p;

          p = Point.Create(min);
          corners.Add(p);
          for (int i = 0; i < dimension; i++)
          {
            p = Point.Create(min);
            p[i] = max[i];
            corners.Add(p);
          }
          ParamPart y = new ParamPart(x, corners, null);
          IList<ParamPart> list = new ParamPart[] { y };
          return list;
        }

        return CreateParts(x, min, max);
      }

      public static IList<ParamPart> CreateParts(ParamInfo x, IPoint range0, IPoint range1)
      {
        IBox range = new Box(Point.Create(range0), Point.Create(range1), true);
        PartEnumerable enumPoints = new PartEnumerable(range);
        List<IList<IPoint>> parts = new List<IList<IPoint>>(enumPoints);

        ParamPartList parentList = new ParamPartList(x, enumPoints.Center);
        foreach (var part in parts)
        {
          ParamPart xPart = new ParamPart(x, part, parentList);
          parentList.Add(xPart);
        }

        return parentList;
      }

      public ParamPart(ParamInfo geometry, IList<IPoint> cornerParameters,
      ParamPartList parentList)
      {
        _geometry = geometry;
        _cornerParams = cornerParameters;

        _parentList = parentList;
      }

      private ParamPart()
      { }

      public int Topology
      {
        get
        {
          if (_axes != null)
          {
            return _axes.Count;
          }
          else
          {
            return _cornerParams.Count - 1;
          }
        }
      }

      public ParamInfo ParamInfo
      {
        get { return _geometry; }
      }

      public ParamPartList ParentList
      {
        get { return _parentList; }
      }

      public double MaxOffset
      {
        get
        {
          if (_maxOffset < 0)
          {
            double normedOffset = _geometry.BaseGeometry.NormedMaxOffset;
            if (normedOffset == 0)
            { _maxOffset = 0; }
            else
            {
              IList<IPoint> axes = Axes;
              double d20 = 0;
              int n = axes.Count;
              for (int i0 = 0; i0 < n; i0++)
              {
                double d2;

                d2 = PntOp.OrigDist2(axes[i0]);
                if (d2 > d20)
                { d20 = d2; }

                for (int i1 = i0 + 1; i1 < n; i1++)
                {
                  d2 = PntOp.Dist2(axes[i0], axes[i1]);
                  if (d2 > d20)
                  { d20 = d2; }
                }
              }
              _maxOffset = d20 * normedOffset;
            }
          }
          return _maxOffset;
        }
      }

      private List<ParamPart> _borders;
      public IList<ParamPart> Borders
      {
        get
        {
          if (_borders == null)
          {
            int n = Axes.Count + 1;
            _borders = new List<ParamPart>(n);
            for (int j = n - 1; j >= 0; j--)
            {
              ParamPart border = new ParamPart { _axes = new List<IPoint>(n - 1) };
              if (j != n - 1)
              {
                border._origin = Origin;
                for (int i = 0; i < n - 1; i++)
                {
                  if (i != j)
                  {
                    border._axes.Add(Axes[i]);
                  }
                }
              }
              else
              {
                IPoint ax0 = _axes[0];
                border._origin = PntOp.Add(Origin, ax0);
                for (int i = 1; i < n - 1; i++)
                {
                  border._axes.Add(PntOp.Sub(Axes[i], ax0));
                }
              }

              _borders.Add(border);
            }
          }
          return _borders;
        }
      }

      public IPoint BoxCorner
      {
        get
        {
          int n = _cornerParams.Count - 1;
          IPoint p = _cornerParams[n];
          return p;
        }
      }

      public IBox ParamBox
      {
        get
        {
          if (_paramBox == null)
          {
            Point range0 = Point.Create(_cornerParams[0]);
            Point range1 = Point.Create(BoxCorner);
            IBox range = new Box(range0, range1, true);
            _paramBox = range;
          }
          return _paramBox;
        }
      }

      public LinearIntersect LinearIntersect(ParamPart other)
      {
        OrthogonalSystem fullSystem = OrthoSystem.Clone();
        LinearIntersect intersect =
          GeometryOperator.LinearIntersect.Create(Origin, fullSystem, other.Origin, other.Axes);
        return intersect;
      }

      public IBox GeometryBox()
      {
        Point min = ParamInfo.GeometryParamAt(ParamBox.Min);
        Point max = ParamInfo.GeometryParamAt(ParamBox.Max);
        Box box = new Box(min, max);
        return box;
      }

      private void InitSystem()
      {
        int n = _cornerParams.Count;
        _axes = new List<IPoint>(n - 1);
        for (int i = 0; i < n; i++)
        {
          IPoint corner = _geometry.CachedPointAt(_cornerParams[i]);
          if (i == 0)
          { _origin = corner; }
          else
          {
            Point axis = PntOp.Sub(corner, _origin);
            _axes.Add(axis);
          }
        }
      }

      private IPoint Origin
      {
        get
        {
          if (_origin == null)
          { InitSystem(); }
          return _origin;
        }
      }
      private IList<IPoint> Axes
      {
        get
        {
          if (_axes == null)
          { InitSystem(); }
          return _axes;
        }
      }

      public OrthogonalSystem OrthoSystem
      {
        get
        {
          if (_orthoSystem == null)
          { _orthoSystem = OrthogonalSystem.Create(Axes); }
          return _orthoSystem;
        }
      }


      public static ParamPart CreatePart(ParamPart part, Point center)
      {
        ParamPart p = new ParamPart
        {
          _origin = center,
          _axes = new List<IPoint>(part.Axes.Count)
        };

        foreach (var axis in part.Axes)
        {
          Point a = PntOp.Add(axis, part.Origin) - center;
          p._axes.Add(a);
        }
        return p;
      }
    }

    private class ParamsRelation
    {
      private ParamPart _x;
      private ParamPart _y;
      private readonly List<ParamsRelation> _parentList;

      private LinearIntersect _linearIntersect;
      private ParamRelate _rel;

      public ParamsRelation(ParamPart x, ParamPart y, List<ParamsRelation> parentList)
      {
        _x = x;
        _y = y;
        _parentList = parentList;

        _rel = ParamRelate.Unknown;
      }

      public bool IsKnown()
      {
        if (_linearIntersect != null)
        { return true; }

        if (_x.ParentList != null && _x.ParentList.IsSplit(_x.BoxCorner, _y))
        { return true; }

        if (_y.ParentList != null && _y.ParentList.IsSplit(_y.BoxCorner, _x))
        { return true; }

        return false;
      }

      private IPoint _intersection;
      private ParamGeometryRelation _paramRelation;

      public IPoint Intersection
      {
        get
        {
          if (_intersection == null)
          { Solve(LinearIntersect); }
          return _intersection;
        }
      }

      private static int _maxTrys = 0;
      private void Solve(LinearIntersect intersect)
      {
        if (intersect.CommonRel != null)
        {
          ParamGeometryRelation rel = intersect.GetCommonRel();
          IGeometry i = rel.Intersection;
          if (i is IPoint)
          { rel.Intersection = _x.ParamInfo.BaseGeometry.PointAt(rel.XParam); }
          else
          { throw new NotImplementedException(); }
          _paramRelation = rel;
          return;
        }
        if (_x.ParamInfo.BaseGeometry.IsLinear &&
          _y.ParamInfo.BaseGeometry.IsLinear)
        {
          _intersection = intersect.Near;
          _paramRelation = new ParamGeometryRelation(
            _x.ParamInfo.BaseGeometry, null, _y.ParamInfo.BaseGeometry, null,
            Point.Create(_x.ParamInfo.BaseGeometry.Dimension))
          {
            Intersection = _intersection
          };
          return;
        }

        IParamGeometry xGeom = _x.ParamInfo.BaseGeometry;
        IBox xBox = _x.GeometryBox();
        int xDim = xGeom.Topology;

        IParamGeometry yGeom = _y.ParamInfo.BaseGeometry;
        IBox yBox = _y.GeometryBox();
        int yDim = yGeom.Topology;

        Point xParam = intersect.Parameter(0, _x.ParamBox);
        xParam = _x.ParamInfo.GeometryParamAt(xParam);

        Point yParam = intersect.Parameter(1, _y.ParamBox);
        yParam = _y.ParamInfo.GeometryParamAt(yParam);

        IPoint xAt = xGeom.PointAt(xParam);
        IPoint yAt = yGeom.PointAt(yParam);
        Point d = PntOp.Sub(xAt, yAt);
        double d2 = d.OrigDist2();

        bool solveExtended = false;
        Point xLastParam = null;
        Point yLastParam = null;

        int iTrys = 0;
        while (d2 > 1.0e-12)
        {
          iTrys++;
          if (iTrys > 10)
          { solveExtended = true; }

          OrthogonalSystem xSys = XSystem(xGeom, xParam, xBox, xAt);

          IList<IPoint> yApprox;
          if (yGeom is ITangentGeometry yTan)
          {
            yApprox = yTan.TangentAt(yParam);
          }
          else
          { yApprox = Sekante(yGeom, yParam, yBox, yAt); }

          intersect = GeometryOperator.LinearIntersect.Create(xAt, xSys, yAt, yApprox);

          for (int j = 0; j < xDim; j++)
          {
            SetDeltaParam(xParam, j, intersect.VectorFactors[j], xBox);
          }
          for (int j = 0; j < yDim; j++)
          {
            SetDeltaParam(yParam, j, -intersect.VectorFactors[j + xDim], yBox);
          }

          if (solveExtended)
          {
            if (xLastParam == null)
            {
              xLastParam = Point.Create(xParam);
              yLastParam = Point.Create(yParam);
            }
            else
            {
              for (int i = 0; i < xParam.Dimension; i++)
              {
                if (xLastParam[i] == xParam[i])
                {
                  if (xParam[i] == xBox.Max[i])
                  { xParam[i] = xBox.Min[i]; }
                  else if (xParam[i] == xBox.Min[i])
                  { xParam[i] = xBox.Max[i]; }
                }
              }
              for (int i = 0; i < yParam.Dimension; i++)
              {
                if (yLastParam[i] == yParam[i])
                {
                  if (yParam[i] == yBox.Max[i])
                  { yParam[i] = yBox.Min[i]; }
                  else if (yParam[i] == yBox.Min[i])
                  { yParam[i] = yBox.Max[i]; }
                }
              }
            }
          }

          xAt = xGeom.PointAt(xParam);
          yAt = yGeom.PointAt(yParam);
          d = PntOp.Sub(xAt, yAt);
          d2 = d.OrigDist2();
          if (solveExtended)
          { }
          if (iTrys > _maxTrys)
          {
            _maxTrys = iTrys;
          }
        }

        _intersection = xAt;
        _paramRelation = new ParamGeometryRelation(
          _x.ParamInfo.BaseGeometry, xParam, _y.ParamInfo.BaseGeometry, yParam,
          Point.Create(_x.ParamInfo.BaseGeometry.Dimension))
        {
          Intersection = _intersection
        };

      }

      public ParamGeometryRelation ParamRelation
      {
        get
        {
          if (_paramRelation == null)
          { Solve(LinearIntersect); }

          return _paramRelation;
        }
      }
      private static void SetDeltaParam(Point param, int index, double delta, IBox box)
      {
        double newParam = param[index] + delta;
        if (newParam > box.Max[index])
        { newParam = box.Max[index]; }
        else if (newParam < box.Min[index])
        { newParam = box.Min[index]; }
        param[index] = newParam;
      }

      private static OrthogonalSystem XSystem(IParamGeometry xGeom, Point xParam, IBox xBox, IPoint xAt)
      {
        IList<IPoint> xApprox;
        OrthogonalSystem xSys = null;
        if (xGeom is ITangentGeometry xTan)
        {
          xApprox = xTan.TangentAt(xParam);
          xSys = new OrthogonalSystem(xApprox.Count);
          foreach (var point in xApprox)
          {
            Axis newAxe = xSys.GetOrthogonal(point);
            if (newAxe.Length2Epsi == 0)
            {
              xSys = null;
              break;
            }
            xSys.Axes.Add(newAxe);
          }
        }
        if (xSys == null)
        {
          xApprox = Sekante(xGeom, xParam, xBox, xAt);
          xSys = OrthogonalSystem.Create(xApprox);
        }
        return xSys;
      }

      private static IList<IPoint> Sekante(IParamGeometry geom, Point param, IBox box, IPoint at)
      {
        //IList<IPoint> sek = new List<IPoint>();
        //for (int i = 0; i < param.Dimension; i++)
        //{
        //  double f = box.Max[i] - box.Min[i];
        //  IPoint p0 = geom.PointAt(box.Min);
        //  IPoint p1 = geom.PointAt(box.Max);
        //  sek.Add( 1 / f * PntOp.Sub(p1, p0));
        //}
        //return sek;

        IList<IPoint> sek = new List<IPoint>();
        for (int i = 0; i < param.Dimension; i++)
        {
          double imax = box.Max[i];
          double idt = 0.1 * (imax - box.Min[i]);
          double iparam = param[i] + idt;
          if (iparam < imax)
          {
            idt = -idt;
            param[i] += idt;
          }
          else
          { param[i] = iparam; }

          try
          {
            IPoint ipoint = geom.PointAt(param);
            Point isek = (1 / idt) * PntOp.Sub(ipoint, at);
            sek.Add(isek);
          }
          finally
          {
            param[i] -= idt;
          }
        }
        return sek;
      }
      public LinearIntersect LinearIntersect
      {
        get
        {
          if (_linearIntersect == null)
          {
            if (_x.Topology >= _y.Topology)
            { _linearIntersect = _x.LinearIntersect(_y); }
            else
            { _linearIntersect = _y.LinearIntersect(_x); }
          }
          return _linearIntersect;
        }
      }

      /// <summary>
      /// Relation  of the x and y linear approximations
      /// </summary>
      public ParamRelate Relation
      {
        get
        {
          if (_rel == ParamRelate.Unknown)
          {
            bool xLinear = _x.ParamInfo.BaseGeometry.IsLinear;
            bool yLinear = _y.ParamInfo.BaseGeometry.IsLinear;

            LinearIntersect i = LinearIntersect;
            if (_x.MaxOffset == 0 && _y.MaxOffset == 0)
            {
              if (i.Offset2Epsi == 0 && i.IsInside(!xLinear, !yLinear))
              { _rel = ParamRelate.Intersect; }
              else
              { _rel = ParamRelate.Disjoint; }
              return _rel;
            }
            else
            {
              if (i.Offset2Epsi == 0 && i.IsInside(!xLinear, !yLinear))
              {
                if (_y.MaxOffset > 0)
                {
                  double n2 = _y.MaxOffset;
                  n2 = n2 * n2;
                  foreach (var border in _x.Borders)
                  {
                    LinearIntersect near = _y.LinearIntersect(border);
                    if (near.Offset2 < n2 && near.IsInside(!xLinear, !yLinear))
                    {
                      _rel = ParamRelate.Near;
                      return _rel;
                    }
                  }
                }
                if (_x.MaxOffset > 0)
                {
                  double n2 = _x.MaxOffset;
                  n2 = n2 * n2;
                  foreach (var border in _y.Borders)
                  {
                    LinearIntersect near = _x.LinearIntersect(border);
                    if (near.Offset2 < n2 && near.IsInside(!xLinear, !yLinear))
                    {
                      _rel = ParamRelate.Near;
                      return _rel;
                    }
                  }
                }
                _rel = ParamRelate.Intersect;
                return _rel;

              }
              double m2 = _x.MaxOffset + _y.MaxOffset;
              m2 = m2 * m2;
              foreach (var border in _x.Borders)
              {
                LinearIntersect near = _y.LinearIntersect(border);
                if (near.Offset2 < m2 && near.IsInside(!xLinear, !yLinear))
                {
                  _rel = ParamRelate.Near;
                  return _rel;
                }
              }
              foreach (var border in _y.Borders)
              {
                LinearIntersect near = _x.LinearIntersect(border);
                if (near.Offset2 < m2 && near.IsInside(!xLinear, !yLinear))
                {
                  _rel = ParamRelate.Near;
                  return _rel;
                }
              }
              _rel = ParamRelate.Disjoint;
            }
          }
          return _rel;
        }
      }

      public void Split()
      {
        double xMax = _x.MaxOffset;
        double yMax = _y.MaxOffset;

        if (xMax > yMax)
        { Split(_x, _y); }
        else
        { Split(_y, _x); }
      }

      public ParamPart X
      { get { return _x; } }
      public ParamPart Y
      { get { return _y; } }

      private void Split(ParamPart xPart, ParamPart yPart)
      {
        ParamPartList rangeList = xPart.ParentList;
        rangeList.Split(xPart.BoxCorner, yPart, _parentList);
      }

    }

    #endregion

    public static IPoint ClosestPoint(IPoint p, IGeometry g)
    {
      if (IsMultipart(g))
      {
        IMultipartGeometry xMulti = (IMultipartGeometry)g;
        return ClosestPoint(p, xMulti);
      }
      if (!(g is IRelParamGeometry pg))
      { throw new InvalidOperationException("IGeometry is not of type " + typeof(IRelParamGeometry)); }

      if (pg.IsWithin(p))
      { return null; }

      if (pg.IsLinear)
      {
        Gaga gaga = new Gaga(pg, null);
        Axis a = gaga.OrthoSys.GetOrthogonal(p);
        // TODO
      }

      return null;
    }

    private static IPoint ClosestPoint(IPoint p, IMultipartGeometry xMulti)
    {
      IPoint pMin = null;
      double d2 = 0;
      foreach (var part in xMulti.Subparts())
      {
        if (pMin == null)
        {
          pMin = ClosestPoint(p, part);
          d2 = PntOp.Dist2(p, pMin);
        }
        else
        {
          IPoint extent = ClosestPoint(p, part.Extent);
          if (PntOp.Dist2(extent, p) < d2)
          {
            IPoint pPart = ClosestPoint(p, part);
            double dPart2 = PntOp.Dist2(pPart, p);

            if (dPart2 < d2)
            {
              pMin = pPart;
              d2 = dPart2;
            }
          }
        }
      }
      return pMin;
    }

    private static Point ClosestPoint(IPoint p, IBox b)
    {
      int dim = Math.Min(p.Dimension, b.Dimension);
      Point closest = Point.Create(dim);
      for (int i = 0; i < dim; i++)
      {
        double t;
        if (p[i] < b.Min[i])
        { t = b.Min[i]; }
        else if (p[i] < b.Max[i])
        { t = p[i]; }
        else
        { t = b.Max[i]; }
        closest[i] = t;
      }
      return closest;
    }

    public static bool Intersects(IGeometry x, IGeometry y)
    {
      TrackOperatorProgress track = new TrackOperatorProgress();
      try
      {
        track.RelationFound += Intersects_RelationFound;
        IList<ParamGeometryRelation> firstRelation = CreateRelations(x, y, track);
      }
      finally
      { track.RelationFound -= Intersects_RelationFound; }

      return track.Cancel;
    }

    private static void Intersects_RelationFound(object sender, ParamRelationCancelArgs args)
    {
      args.Cancel = true;
    }

    public static GeometryCollection Intersection(IGeometry x, IGeometry y)
    {
      IList<ParamGeometryRelation> list = CreateRelations(x, y);
      if (list == null)
      { return null; }

      GeometryCollection result = new GeometryCollection();
      foreach (var rel in list)
      { result.Add(rel.Intersection); }

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="track"></param>
    /// <param name="calcLinearized">true: Die Relation via Linearisierung von x und y bestimmt werden.
    /// Dieser Parameter kommt nur zur Anwendung, falls x und y keine Multipart-Geometrien sind und 
    /// IParamGeom implementieren</param>
    /// <returns></returns>
    public static IList<ParamGeometryRelation> CreateRelations(
      IGeometry x, IGeometry y, TrackOperatorProgress track = null, bool calcLinearized = false)
    {
      if (y.Topology > x.Topology)
      { return CreateRelations(y, x, track, calcLinearized); }

      Relation rel;
      if (x.Extent == null)
      { rel = Relation.Disjoint; }
      else
      { rel = x.Extent.RelationTo(y.Extent); }
      return CreateRelations(x, y, track, rel, calcLinearized);
    }

    private static IList<ParamGeometryRelation> CreateRelations(IGeometry x, IGeometry y,
      TrackOperatorProgress track, Relation extentRelation, bool calcLinearized)
    {
      if (extentRelation == Relation.Disjoint)
      { return null; }
      if (extentRelation == Relation.Contains)
      {
        if (x is IBox && track != null)
        { track.OnRelationFound(null, null); }
      }
      if (extentRelation == Relation.Within)
      {
        if (y is IBox && track != null)
        { track.OnRelationFound(null, null); }
      }

      if (track != null && track.Cancel)
      { return null; }

      if (IsMultipart(x))
      {
        if (IsMultipart(y) && y.Topology >= x.Topology)
        {
          IBox xExtent = x.Extent;
          IBox yExtent = y.Extent;

          if (xExtent.GetMaxExtent() > yExtent.GetMaxExtent())
          {
            return MpRelations(x, y, track);
          }
          else
          {
            return MpRelations(y, x, track);
          }
        }

        // y is single part
        if (extentRelation == Relation.Within)
        { return IntersectReduce(y, x, track, false); }
        else
        { return MpRelations(x, y, track); }
      }
      if (y.Topology >= x.Topology && IsMultipart(y))
      {
        if (extentRelation == Relation.Contains)
        { return IntersectReduce(x, y, track, calcLinearized); }
        else
        { return MpRelations(y, x, track); }
      }
      // no multiparts
      if (x.Topology >= y.Topology)
      { return IntersectReduce(x, y, track, calcLinearized); }
      else
      { return IntersectReduce(y, x, track, false); }
    }

    private static IList<ParamGeometryRelation> MpRelations(
      IGeometry x, IGeometry y, TrackOperatorProgress track)
    {
      List<ParamGeometryRelation> relations = null;
      IMultipartGeometry xMulti = (IMultipartGeometry)x;

      IBox yExtent = y.Extent;
      int iPart = -1;
      foreach (var xPart in xMulti.Subparts())
      {
        iPart++;
        if (track != null && track.Cancel)
        { return relations; }

        Relation extentRelation = xPart.Extent.RelationTo(yExtent);
        IList<ParamGeometryRelation> list = CreateRelations(xPart, y,
          track, extentRelation, false);
        if (list != null)
        {
          if (relations == null)
          { relations = new List<ParamGeometryRelation>(); }
          foreach (var rel in list)
          {
            rel.AddParent(x, iPart, xPart);
            relations.Add(rel);
          }
        }
      }
      return relations;
    }

    private static IList<ParamGeometryRelation> IntersectReduce(
      IGeometry x, IGeometry y, TrackOperatorProgress track, bool calcLinearized)
    {
      if (!(x.Topology >= y.Topology)) throw new InvalidOperationException("Topology");
      if (!(IsMultipart(x) == false)) throw new InvalidOperationException("multipart");

      if (x.Topology < x.Dimension)
      {
        if (IsMultipart(y))
        { return MpRelations(y, x, track); }

        IRelParamGeometry xParam = x as IRelParamGeometry;
        if (xParam == null && x is IBox)
        { xParam = new ParamBox((IBox)x); }

        IRelParamGeometry yParam = y as IRelParamGeometry;
        if (yParam == null && y is IBox)
        { yParam = new ParamBox((IBox)y); }

        if (xParam == null || yParam == null)
        {
          throw new NotImplementedException("Base geometry parts are not typeof " +
          typeof(IRelParamGeometry));
        }

        if (calcLinearized)
        {
          IList<ParamGeometryRelation> list = CreateRelParamRelations(xParam, yParam, track);
          return list;
        }

        {
          IList<ParamGeometryRelation> list = xParam.CreateRelations(yParam, track);
          return list;
        }
      }
      else
      {
        if (x.Topology == 1)
        {
          ParamGeometryRelation rel = ParamGeometryRelation.GetLineRelation((IParamGeometry)x, y);
          if (rel != null)
          {
            if (track != null)
            { track.OnRelationFound(track, rel); }
            return new ParamGeometryRelation[] { rel };
          }
          return null;
        }
        IGeometry border = x.Border;
        IList<ParamGeometryRelation> list = CreateRelations(border, y, track, false);

        if (track == null || !track.Cancel)
        {
          List<ParamGeometryRelation> adapted = ParamGeometryRelation.Assemble(x, y, list);
          if (adapted != null && adapted.Count > 0 && track != null)
          { track.OnRelationFound(track, adapted[0]); }
          if (adapted != null)
          { list = adapted; }
        }
        return list;
      }
    }

    public static IGeometry GetFirstPoint(IGeometry y)
    {
      IGeometry p = y;
      while (!(p is IPoint) && p != null)
      {
        if (p is IMultipartGeometry m && m.HasSubparts)
        {
          foreach (var part in m.Subparts())
          {
            p = part;
            break;
          }
        }
        else
        {
          p = p.Border;
        }
      }
      return p;
    }

    public static bool IsMultipart(IGeometry geom)
    {
      return geom is IMultipartGeometry multi && multi.HasSubparts;
    }

    private class ParamBox : IRelTanParamGeometry
    {
      private IBox _box;
      private IBox _paramRange;

      public ParamBox(IBox box)
      { _box = box; }

      public IList<IPoint> TangentAt(IPoint parameters)
      {
        List<IPoint> tangents = new List<IPoint>(parameters.Dimension);

        int d = Dimension;

        for (int i = 0; i < d; i++)
        {
          double o = _box.Max[i] - _box.Min[i];
          if (o > 0)
          {
            Point p = Point.Create(d);
            p[i] = o;
            tangents.Add(p);
          }
        }

        return tangents;
      }

      public IBox ParameterRange
      {
        get
        {
          if (_paramRange == null)
          {
            int t = Topology;
            Point min = Point.Create(t);
            Point max = Point.Create(t);
            for (int i = 0; i < t; i++)
            { max[i] = 1; }

            _paramRange = new Box(min, max);
          }
          return _paramRange;
        }
      }

      public double NormedMaxOffset
      {
        get { return 0; }
      }

      public IPoint PointAt(IPoint parameters)
      {
        int d = Dimension;
        int t = 0;

        Point at = Point.Create(_box.Min);
        for (int i = 0; i < d; i++)
        {
          double o = _box.Max[i] - _box.Min[i];
          if (o > 0)
          {
            at[i] += o * parameters[t];
            t++;
          }
        }

        return at;
      }

      public bool IsLinear
      { get { return true; } }

      public ParamRelate Relation(IBox paramBox)
      {
        int n = paramBox.Dimension;
        ParamRelate rel = ParamRelate.Intersect;
        for (int i = 0; i < n; i++)
        {
          double min = paramBox.Min[i];
          double max = paramBox.Max[i];
          if (min > 1 || max < 0)
          { return ParamRelate.Disjoint; }
          if (min < 0 || max > 1)
          { rel = ParamRelate.Near; }
        }
        return rel;
      }

      public IList<ParamGeometryRelation> CreateRelations(IParamGeometry other,
        TrackOperatorProgress trackProgress)
      {
        if (other is IRelParamGeometry relOther)
        { return CreateRelParamRelations(this, relOther, trackProgress); }

        return GeometryOperator.CreateRelations(this, other, trackProgress);
      }

      public int Dimension
      { get { return _box.Dimension; } }

      public int Topology
      { get { return _box.Topology; } }

      public bool IsWithin(IPoint point)
      { return _box.IsWithin(point); }

      public IBox Extent
      { get { return _box; } }

      public IGeometry Border
      { get { return _box.Border; } }

      public IGeometry Project(IProjection projection)
      { return _box.Project(projection); }

      public bool EqualGeometry(IGeometry other)
      { return _box == other; }
    }

    private static IList<ParamGeometryRelation> CreateRelParamRelations(IRelParamGeometry x, IRelParamGeometry y,
      TrackOperatorProgress track)
    {
      ParamInfo xInfo = new ParamInfo(x);
      ParamInfo yInfo = new ParamInfo(y);
      IList<ParamPart> xParts = ParamPart.CreateParts(xInfo);
      IList<ParamPart> yParts = ParamPart.CreateParts(yInfo);

      List<ParamGeometryRelation> result = null;

      List<ParamsRelation> relations = new List<ParamsRelation>();
      foreach (var xPart in xParts)
      {
        foreach (var yPart in yParts)
        {
          ParamsRelation rel = new ParamsRelation(xPart, yPart, relations);
          relations.Add(rel);
        }
      }

      for (int i = 0; i < relations.Count; i++)
      {
        if (track != null && track.Cancel)
        { return result; }

        ParamsRelation rel = relations[i];
        if (rel.IsKnown())
        { continue; }

        ParamRelate r = rel.Relation;
        if (r == ParamRelate.Disjoint)
        { continue; }

        if (r == ParamRelate.Intersect)
        {
          ParamRelate xRel = x.Relation(rel.X.ParamBox);
          if (xRel == ParamRelate.Disjoint)
          { continue; }
          ParamRelate yRel = y.Relation(rel.Y.ParamBox);
          if (yRel == ParamRelate.Disjoint)
          { continue; }

          if (xRel == ParamRelate.Intersect && yRel == ParamRelate.Intersect)
          {
            if (result == null)
            { result = new List<ParamGeometryRelation>(); }
            result.Add(rel.ParamRelation);

            if (track != null)
            { track.OnRelationFound(track, rel.ParamRelation); }
            continue;
          }

          if (rel.X.ParamInfo.BaseGeometry.IsLinear == false ||
            rel.Y.ParamInfo.BaseGeometry.IsLinear == false)
          {
            rel.Split();
          }
          continue;
        }

        if (r != ParamRelate.Near)
        { throw new InvalidOperationException("Unhandled relation " + r); }

        rel.Split();
      }

      return result;
    }

    private class Gaga
    {
      private readonly IRelParamGeometry _geom;
      private readonly IBox _paramRange;
      private IBox _extent;

      private OrthogonalSystem _orthoSys;
      private IPoint _center;

      private IList<IPoint> _axes;
      private IPoint _origin;

      public Gaga(IRelParamGeometry geom, IRelationGeometry rel)
      {
        _geom = geom;
      }

      private Gaga(IRelParamGeometry geom, IBox paramRange, IBox extent)
      {
        _geom = geom;
        _paramRange = paramRange;
        _extent = extent;
      }
      private IList<IPoint> Axes
      {
        get
        {
          if (_axes == null)
          { InitSystem(); }
          return _axes;
        }
      }

      public LinearIntersect LinearIntersect(Gaga other)
      {
        OrthogonalSystem fullSystem = OrthoSys.Clone();
        LinearIntersect intersect =
          GeometryOperator.LinearIntersect.Create(Origin, fullSystem, other.Origin, other.Axes);
        return intersect;
      }

      private void InitSystem()
      {
        IBox paramRange;
        if (_paramRange == null)
        { paramRange = _geom.ParameterRange; }
        else
        { paramRange = _paramRange; }

        int n = paramRange.Dimension;
        IPoint paramMin = paramRange.Min;
        IPoint paramMax = paramRange.Max;
        Point param = Point.Create(paramMin);
        List<IPoint> axes = new List<IPoint>(n);
        IPoint origin = _geom.PointAt(param);
        for (int i = 0; i < n; i++)
        {
          param[i] = paramMax[i];

          IPoint corner = _geom.PointAt(param);
          Point axis = PntOp.Sub(corner, origin);
          axes.Add(axis);

          param[i] = paramMin[i];
        }
        _origin = origin;
        _axes = axes;
      }

      public OrthogonalSystem OrthoSys
      {
        get
        {
          if (_orthoSys == null)
          {
            OrthogonalSystem orthoSys = OrthogonalSystem.Create(Axes);
            _orthoSys = orthoSys;
          }
          return _orthoSys;
        }
      }

      public IRelParamGeometry BaseGeom
      {
        get { return _geom; }
      }

      public IBox Extent
      {
        get
        {
          if (_extent == null)
          {
            Box extent;
            int dim = _geom.Dimension;

            if (_paramRange == null)
            {
              IBox geomExtent = _geom.Extent;
              extent = new Box(Point.Create(geomExtent.Min), Point.Create(geomExtent.Max));
            }
            else
            {
              extent = GetExtent(_paramRange);
            }
            double normed = _geom.NormedMaxOffset;
            Point min = extent.Min;
            Point max = extent.Max;
            double maxOffset = normed * normed * PntOp.Dist2(min, max);
            for (int iDim = 0; iDim < dim; iDim++)
            {
              min[iDim] -= maxOffset;
              max[iDim] += maxOffset;
            }

            _extent = extent;
          }
          return _extent;
        }
      }

      public IPoint Origin
      {
        get
        {
          if (_origin == null)
          { InitSystem(); }
          return _origin;
        }
      }
      public IPoint Center
      {
        get
        {
          if (_center == null)
          {
            IBox extent = Extent;
            _center = 0.5 * PntOp.Add(extent.Min, extent.Max);
          }
          return _center;
        }
      }
      public IList<Gaga> Split(bool includeLinears)
      {
        if (includeLinears && BaseGeom.IsLinear)
        {
          return new Gaga[] { this };
        }
        return Split();
      }

      public List<Gaga> Split()
      {
        IBox paramRange;
        if (_paramRange == null)
        {
          paramRange = _geom.ParameterRange;
        }
        else
        {
          paramRange = _paramRange;
        }
        return GetSplits(paramRange);
      }
      private class GagaBuilder
      {
        private readonly int _dim;
        private readonly int[] _pos;
        public Box GeomExtent;
        public Box ParamExtent;
        public GagaBuilder(int[] pos, Box paramExtent)
        {
          _pos = pos;
          ParamExtent = paramExtent;

          _dim = pos.Length;
        }
        public bool TryAdd(int[] p, IPoint pt)
        {
          for (int i = 0; i < _dim; i++)
          {
            int d = p[i] - _pos[i];
            if (d < 0 || d > 1)
            { return false; }
          }
          if (GeomExtent == null)
          { GeomExtent = new Box(Point.Create(pt), Point.Create(pt)); }
          else
          { GeomExtent.Include(pt); }
          return true;
        }
      }
      private List<Gaga> GetSplits(IBox paramRange)
      {
        int dim = paramRange.Dimension;
        IPoint min = paramRange.Min;
        IPoint max = paramRange.Max;
        IPoint center = 0.5 * PntOp.Add(min, max);
        IPoint[] cs = new IPoint[] { min, center, max };

        int nCombs = 1;
        int nPts = 1;
        for (int i = 0; i < dim; i++)
        {
          nCombs *= 2;
          nPts *= 3;
        }

        List<GagaBuilder> builders = new List<GagaBuilder>(nCombs);
        for (int iComb = 0; iComb < nCombs; iComb++)
        {
          Point splitMin = Point.Create(dim);
          Point splitMax = Point.Create(dim);
          int comb = iComb;
          int[] pos = new int[dim];
          for (int iDim = 0; iDim < dim; iDim++)
          {
            if ((comb & 1) == 0)
            {
              splitMin[iDim] = min[iDim];
              splitMax[iDim] = center[iDim];
              pos[iDim] = 0;
            }
            else
            {
              splitMin[iDim] = center[iDim];
              splitMax[iDim] = max[iDim];
              pos[iDim] = 1;
            }
          }
          Box splitParamBox = new Box(splitMin, splitMax);
          builders.Add(new GagaBuilder(pos, splitParamBox));
        }

        for (int i = 0; i < nPts; i++)
        {
          int[] code = new int[dim];
          double[] coords = new double[dim];
          int j = i;
          for (int d = 0; d < dim; d++)
          {
            int r = j % 3;
            code[d] = r;
            coords[d] = cs[r][dim];
            j = j / 3;
          }
          IPoint paramPt = Point.Create(coords);
          IPoint geomPt = _geom.PointAt(paramPt);

          foreach (var b in builders)
          { b.TryAdd(code, geomPt); }
        }

        List<Gaga> splits = new List<Gaga>(builders.Count);
        double normed = _geom.NormedMaxOffset;
        normed = normed * normed;
        foreach (var builder in builders)
        {
          Box extent = builder.GeomExtent;
          Point sMin = extent.Min;
          Point sMax = extent.Max;
          double off = PntOp.Dist2(sMin, sMax) * normed;
          for (int iDim = 0; iDim < sMin.Dimension; iDim++)
          {
            sMin[iDim] -= off;
            sMax[iDim] += off;
          }
          splits.Add(new Gaga(_geom, builder.ParamExtent, extent));
        }
        return splits;
      }

      [Obsolete]
      private Box GetExtent(IBox paramRange)
      {
        int dim = paramRange.Dimension;
        int nCombs = 1 >> dim;
        IPoint min = paramRange.Min;
        IPoint max = paramRange.Max;
        Box extent = null;
        for (int iComb = 0; iComb < nCombs; iComb++)
        {
          Point p = Point.Create(dim);
          int comb = iComb;
          for (int iDim = 0; iDim < dim; iDim++)
          {
            if ((comb & 1) == 0)
            { p[iDim] = min[iDim]; }
            else
            { p[iDim] = max[iDim]; }
            comb /= 2;
          }
          IPoint pointAt = _geom.PointAt(p);
          if (extent == null)
          { extent = new Box(Point.Create(pointAt), Point.Create(pointAt)); }
          else
          { extent.Include(pointAt); }
        }
        return extent;
      }
    }

    private class GagaRel
    {
      private readonly Gaga _x;
      private readonly Gaga _y;

      public GagaRel(Gaga x, Gaga y)
      {
        _x = x;
        _y = y;
      }

      private ParamRelate _rel;
      public ParamRelate Relation
      {
        get
        {
          if (_rel == ParamRelate.Unknown)
          {
            ParamRelate rel;
            if (!_x.Extent.Intersects(_y.Extent))
            {
              rel = ParamRelate.Disjoint;
            }
            LinearIntersect intersect = _x.LinearIntersect(_y);

            if (_x.BaseGeom.IsLinear)
            {
              if (_y.BaseGeom.IsLinear)
              {
                // TODO;
                rel = ParamRelate.Unknown;
              }
              else
              {
                rel = GetNearRelation(_x, _y);
              }
            }
            else if (_y.BaseGeom.IsLinear)
            {
              rel = GetNearRelation(_y, _x);
            }
            else
            {
              if (_x.Extent.Intersects(_y.Extent))
              {
                rel = ParamRelate.Near;
              }
              else
              {
                rel = ParamRelate.Disjoint;
              }
            }
            _rel = rel;
          }
          return _rel;
        }
      }

      public ParamGeometryRelation ParamRelation
      {
        get { throw new NotImplementedException(); }
      }

      public bool IsKnown()
      {
        return _rel != ParamRelate.Unknown;
      }

      private ParamRelate GetNearRelation(Gaga linear, Gaga y)
      {
        OrthogonalSystem o = linear.OrthoSys;

        IPoint cy = PntOp.Sub(y.Center, linear.Origin);
        Axis ay = o.GetOrthogonal(cy);
        //if (ay.Length2 < y.MaxOffset)
        //{
        //  return ParamRelate.Near;
        //}
        //TODO
        return ParamRelate.Unknown;
      }

      public IEnumerable<GagaRel> Split()
      {
        if (_x.BaseGeom.IsLinear && _y.BaseGeom.IsLinear)
        {
          return new GagaRel[] { };
        }

        IList<Gaga> xSplits = _x.Split(false);
        IList<Gaga> ySplits = _y.Split(false);
        List<GagaRel> rels = new List<GagaRel>(xSplits.Count * ySplits.Count);
        foreach (var xSplit in xSplits)
        {
          foreach (var ySplit in ySplits)
          {
            rels.Add(new GagaRel(xSplit, ySplit));
          }
        }
        return rels;
      }
    }
    [Obsolete("make private")]
    public static IList<ParamGeometryRelation> CreateRelations1(
      IRelParamGeometry x, IRelParamGeometry y, TrackOperatorProgress track)
    {
      List<ParamGeometryRelation> result = null;

      Gaga xPara = new Gaga(x, null);
      Gaga yPara = new Gaga(y, null);

      Queue<GagaRel> relations = new Queue<GagaRel>();
      relations.Enqueue(new GagaRel(xPara, yPara));


      while (relations.Count > 0)
      {
        if (track != null && track.Cancel)
        { return result; }

        GagaRel rel = relations.Dequeue();
        if (rel.IsKnown())
        { continue; }

        ParamRelate r = rel.Relation;
        if (r == ParamRelate.Disjoint)
        { continue; }

        else if (r == ParamRelate.Intersect)
        {
          if (result == null)
          { result = new List<ParamGeometryRelation>(); }
          result.Add(rel.ParamRelation);

          if (track != null)
          { track.OnRelationFound(track, rel.ParamRelation); }
          continue;
        }
        else if (r == ParamRelate.Near)
        {
          foreach (var splitRel in rel.Split())
          {
            relations.Enqueue(splitRel);
          }
        }
        else
        { throw new NotImplementedException("Unhandled relation " + r); }
      }

      return result;
    }
  }
}
