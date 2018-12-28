using System;
using System.Collections.Generic;
using PntOp = Basics.Geom.PointOperator;

namespace Basics.Geom
{
  public static partial class GeometryOperator
  {
    private class LinearIntersect : IGeometry
    {
      private readonly IPoint _fullOrig;
      private OrthogonalSystem _fullSystem;
      private readonly int _xTopology;
      private readonly IPoint _offset;
      private Axis _offsetAxis;
      private IList<Axis> _commonAxes;
      private ParamGeometryRelation _commonRel;

      private IList<double> _vectorFactors;

      private IPoint _near;

      public LinearIntersect(IPoint fullOffset, OrthogonalSystem fullSystem, int xTopology,
        IPoint offset, Axis offsetAxis, IList<Axis> commonAxes)
      {
        _fullOrig = fullOffset;
        _fullSystem = fullSystem;
        _xTopology = xTopology;
        _offset = offset;
        _offsetAxis = offsetAxis;
        _commonAxes = commonAxes;
      }

      public static LinearIntersect Create(
        IPoint xOrig, OrthogonalSystem xSystem, IPoint yOrig, IList<IPoint> yAxes)
      {
        OrthogonalSystem fullSystem = xSystem;
        int xTopology = xSystem.Axes.Count;

        List<Axis> nullAxes = null;

        foreach (var yAxis in yAxes)
        {
          Axis axis = fullSystem.GetOrthogonal(yAxis);
          if (axis.Length2Epsi > 0)
          { fullSystem.Axes.Add(axis); }
          else
          {
            if (nullAxes == null)
            { nullAxes = new List<Axis>(); }
            nullAxes.Add(axis);
          }
        }

        Point offset = PntOp.Sub(yOrig, xOrig);
        Axis offsetAxis = fullSystem.GetOrthogonal(offset);

        LinearIntersect intersect = new LinearIntersect(xOrig, fullSystem, xTopology,
          offset, offsetAxis, nullAxes);
        return intersect;
      }

      public int Dimension
      {
        get { return _offsetAxis.Point.Dimension; }
      }
      public int Topology
      {
        get
        {
          if (_commonAxes == null)
          { return 0; }
          return _commonAxes.Count;
        }
      }
      public IBox Extent
      {
        get { throw new NotImplementedException("TODO: get extent of "); }
      }
      public IGeometry Border
      {
        get
        {
          if (Topology == 0)
          { return null; }
          throw new NotImplementedException("TODO: get border of ");
        }
      }
      public bool EqualGeometry(IGeometry other)
      {
        throw new NotImplementedException("TODO: project ");
      }
      public IGeometry Project(IProjection prj)
      {
        throw new NotImplementedException("TODO: project ");
      }
      public bool IsWithin(IPoint point)
      {
        throw new NotImplementedException("TODO: intersects ");
      }

      public double Offset2
      {
        get { return _offsetAxis.Length2; }
      }

      public double Offset2Epsi
      {
        get { return _offsetAxis.Length2Epsi; }
      }

      private static bool _optimzeWarned = false;
      public IPoint Near
      {
        get
        {
          if (_near == null)
          {
            if (_vectorFactors == null)
            {
              if (_optimzeWarned == false)
              {
                System.Diagnostics.Debug.Assert(false, "TODO: Optimize");
                _optimzeWarned = true;
              }
            }

            IList<double> factors = VectorFactors;
            Point near = PntOp.Add(_fullOrig, _offset);
            for (int i = factors.Count - 1; i >= _xTopology; i--)
            {
              Point p = _fullSystem.OrigVector(i);
              near = near - factors[i] * p;
            }
            _near = near;
          }
          return _near;
        }
      }

      public bool IsInside(params bool[] usePoints)
      {
        int n = usePoints.Length;
        if (n != 2)
        { throw new InvalidOperationException("TODO: Implementation for non-Point-Intersections"); }

        for (int i = 0; i < n; i++)
        {
          if (IsInside(i, usePoints[i]) == false)
          { return false; }
        }
        return true;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="part"></param>
      /// <param name="usePoints">
      /// if true : returns (intersections lies inside the convex hull of the points)
      /// if false: returns (intersections lies inside the convex hull of the axes)
      /// </param>
      /// <returns></returns>
      public bool IsInside(int part, bool usePoints)
      {
        IList<double> factors = VectorFactors;

        if (_commonAxes != null && _commonAxes.Count > 0)
        {
          return CommonRel != null;
        }

        bool inside;
        if (part == 0)
        {
          inside = IsInside(factors, 0, _xTopology, usePoints);
        }
        else if (part == 1)
        {
          inside = IsInside(factors, _xTopology, factors.Count, usePoints);
        }
        else
        { throw new NotImplementedException("TODO: Implementation for intersections with more than 2 parts"); }
        return inside;
      }

      public ParamGeometryRelation GetCommonRel()
      {
        ParamGeometryRelation commonRel = null;
        if (_commonAxes != null && _commonAxes.Count > 0)
        {
          IList<double> factors = VectorFactors;
          if (_commonAxes.Count == 1 && factors.Count == 1)
          {
            Line l0 = new Line(Point.Create(new double[] { 0 }), Point.Create(new double[] { 1 }));
            Line l1 = new Line(Point.Create(new double[] { factors[0] }), Point.Create(new double[] { factors[0] + _commonAxes[0].Factors[0] }));
            commonRel = ParamGeometryRelation.GetLineRelation(l0, l1);
          }
          else
          { throw new NotImplementedException("TODO: Implementation for non-Point-Intersections"); }
        }
        return commonRel;
      }
      public ParamGeometryRelation CommonRel
      {
        get
        {
          if (_commonRel == null)
          { _commonRel = GetCommonRel(); }
          return _commonRel;
        }
      }
      private bool IsInside(IList<double> factors, int i0, int i1, bool points)
      {
        double sum = 0;
        for (int i = i0; i < i1; i++)
        {
          double f = factors[i];
          if (i0 > 0)
          { f = -f; }
          if (0 > f || f > 1)
          { return false; }
          sum += f;
        }

        if (points && sum > 1)
        { return false; }

        return true;
      }

      public IList<double> VectorFactors
      {
        get
        {
          if (_vectorFactors == null)
          { _vectorFactors = _fullSystem.VectorFactors(_offsetAxis.Factors); }
          return _vectorFactors;
        }
      }

      public Point Parameter(int index, IBox box)
      {
        Point d = PntOp.Sub(box.Max, box.Min);
        if (index == 0)
        {
          Point p = Point.Create(_xTopology);
          for (int i = 0; i < _xTopology; i++)
          {
            p[i] = box.Min[i] + d[i] * VectorFactors[i];
          }
          return p;
        }
        else if (index == 1)
        {
          int n = VectorFactors.Count - _xTopology;
          Point p = Point.Create(n);
          for (int i = 0; i < n; i++)
          {
            p[i] = box.Min[i] - d[i] * VectorFactors[i + _xTopology];
          }
          return p;
        }
        else
        { throw new InvalidOperationException("Invalid index " + index); }
      }
    }
  }
}