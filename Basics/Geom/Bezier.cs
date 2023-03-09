using Basics.Num;
using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public abstract class BezierCore : Curve, IBezier, IMultipartSegment, ISegment<Bezier>
  {
    #region nested classes
    private class _InnerCurve : InnerCurve
    {
      private readonly IPoint _p1;
      private readonly IPoint _p2;
      private List<double> _parts;

      public _InnerCurve(IPoint p1, IPoint p2)
      {
        _p1 = p1;
        _p2 = p2;
      }

      public override Curve Complete(IPoint start, IPoint end)
      {
        Bezier bezier = new Bezier(start, _p1, _p2, end);
        bezier._innerCurve = this;
        return bezier;
      }
      public List<double> PartParams
      {
        get
        { return _parts; }
        set
        { _parts = value; }
      }
    }

    private class _SubpartsEnumerator : IEnumerator<Bezier>
    {
      private BezierCore _bezier;

      private Bezier _currentPart;
      private int _iPart;
      private int _iNParts;

      public _SubpartsEnumerator(BezierCore bezier)
      {
        _bezier = bezier;

        if (_bezier.PartParams == null)
        { _bezier.PartParams = _bezier.CalcSubparts(); }
        Reset();
      }

      #region IEnumerator Members

      public void Dispose()
      { }

      public void Reset()
      {
        _iNParts = _bezier.PartParams.Count - 1;
        _iPart = 0;
      }

      public Bezier Current
      {
        get
        { return _currentPart; }
      }
      object System.Collections.IEnumerator.Current
      { get { return Current; } }

      public bool MoveNext()
      {
        double t0;
        t0 = _bezier.PartParams[_iPart];

        while (true)
        {
          if (_iPart >= _iNParts)
          { return false; }

          _iPart++;
          double t1 = _bezier.PartParams[_iPart];

          if (t1 > t0)
          {
            _currentPart = _bezier.Subpart(t0, t1);
            _currentPart._hasSubparts = false;
            return true;
          }
        }
      }

      #endregion
    }

    private class _SubpartsEnumerable : IEnumerable<Bezier>
    {
      private readonly BezierCore _bezier;

      public _SubpartsEnumerable(BezierCore bezier)
      {
        _bezier = bezier;
      }

      #region IEnumerable Members

      public IEnumerator<Bezier> GetEnumerator()
      {
        return new _SubpartsEnumerator(_bezier);
      }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      { return GetEnumerator(); }

      #endregion
    }

    private class ParamAtFct
    {
      private readonly IBezier _bezier;
      private readonly double _distance;

      public ParamAtFct(IBezier bezier, double distance)
      {
        _bezier = bezier;
        _distance = distance;
      }
      public double LengthAt(double t)
      {
        double l0 = BezierOp.Subpart(_bezier, 0, t).Length();
        if (t < 0) l0 = -l0;
        double diff = l0 - _distance;
        return diff;
      }
    }

    #endregion

    private _InnerCurve _innerCurve;
    private double? _maxOffset;
    private List<double> _dParts;

    private bool _paramKnown = false;
    private double[] _dA0;
    private double[] _dA1;
    private double[] _dA2;
    private double[] _dA3;

    private bool? _hasSubparts;

    public override IPoint Start => GetStart();
    public override IPoint End => GetEnd();
    public IPoint P1 => GetP1();
    public IPoint P2 => GetP2();

    protected abstract IPoint GetStart();
    protected abstract IPoint GetP1();
    protected abstract IPoint GetP2();
    protected abstract IPoint GetEnd();

    protected override Curve CloneCore() => BezierOp.Clone(this);
    public new Bezier Clone() => BezierOp.Clone(this);
    public override bool IsLinear => false;
    public new Box Extent => BezierOp.GetExtent(this);
    protected override IBox GetExtent() => BezierOp.GetExtent(this);

    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }
      if (!(other is IBezier o))
      { return false; }
      if (Dimension != other.Dimension)
      { return false; }

      return BezierOp.EqualGeometry(this, o);
    }

    public override double Length() => BezierOp.Length(this);

    protected override Curve InvertCore() => BezierOp.Invert(this);
    public Bezier Invert() => BezierOp.Invert(this);

    public override double ParamAt(double distance) => BezierOp.ParamAt(this, distance);
    /// <summary>
    /// Punkt bei Parameter t
    /// </summary>
    /// <param name="t">Wert zwischen 0 und 1</param>
    /// <returns></returns>
    public override Point PointAt(double t) => BezierOp.PointAt(this, t);

    public override Point TangentAt(double param) => BezierOp.TangentAt(this, param);
    public double RadiusAt(double param) => BezierOp.RadiusAt(this, param);

    protected override IGeometry ProjectCore(IProjection projection) => BezierOp.Project(this, projection);
    public Bezier Project(IProjection projection) => BezierOp.Project(this, projection);

    public IEnumerable<Bezier> Subparts()
    {
      return new _SubpartsEnumerable(this);
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }

    private List<double> PartParams
    {
      get
      {
        if (_innerCurve != null)
        { return _innerCurve.PartParams; }
        else
        { return _dParts; }
      }
      set
      {
        _dParts = value;
        if (_innerCurve != null)
        { _innerCurve.PartParams = value; }
      }
    }

    private void AssignParams()
    {
      //               
      // P(t) = (1 - t)^3 P0 + 3 t (1 - t)^2 P1 + 3 t^2 (1 - t) P2 + t^3 P1
      //
      if (_paramKnown)
      { return; }
      int iDim = Dimension;

      _dA0 = new double[iDim];
      _dA1 = new double[iDim];
      _dA2 = new double[iDim];
      _dA3 = new double[iDim];

      for (int i = 0; i < iDim; i++)
      {
        _dA0[i] = Start[i];
        _dA1[i] = 3 * (P1[i] - Start[i]);
        _dA3[i] = End[i] - 3 * P2[i] + _dA1[i] + 2 * _dA0[i];
        _dA2[i] = End[i] - _dA3[i] - _dA1[i] - _dA0[i];
      }

      _paramKnown = true;
    }

    public double NormedMaxOffset => NormedMaxOffsetCore;
    protected override double NormedMaxOffsetCore => _maxOffset ?? (_maxOffset = BezierOp.GetNormedMaxOffset(this)).Value;

    double IMultipartSegment.Parameter(ParamGeometryRelation split) => Parameter(split);
    private double Parameter(ParamGeometryRelation split)
    {
      if (HasSubparts == false)
      { throw new InvalidProgramException(); }
      IList<double> pp = PartParams;
      int i = 0;
      foreach (var subpart in Subparts())
      {
        if (split.CurrentX.EqualGeometry(subpart))
        {
          double p = split.XParam[0];
          p = pp[i] + p * (pp[i + 1] - pp[i]);
          return p;
        }
        i++;
      }
      throw new InvalidProgramException();
    }

    internal Point PointAtCore(double t, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(this);
      AssignParams();
      List<double> coords = new List<double>();
      foreach (int iDim in dimensions)
      {
        coords.Add(_dA0[iDim] + t * (_dA1[iDim] + t * (_dA2[iDim] + t * _dA3[iDim])));
      }

      return Point.Create(coords.ToArray());
    }

    internal double ParamAtCore(double distance)
    {
      Calc numMath = new Calc(1e-6);
      ParamAtFct fct = new ParamAtFct(this, distance);

      return numMath.SolveNewton(fct.LengthAt, LengthFactor, distance / BezierOp.Length(this));
    }


    internal double LengthCore()
    {
      AssignParams();
      Calc pCalc = new Calc(1e-8);

      return pCalc.Integral(LengthFactor, 0, 1);
    }

    private double LengthFactor(double t)
    {
      return Math.Sqrt(TangentAt(t).OrigDist2());
    }

    public override InnerCurve GetInnerCurve()
    {
      return new _InnerCurve(P1, P2);
    }

    internal Point TangentAtCore(double param, IEnumerable<int> dimensions = null)
    {
      AssignParams();

      dimensions = dimensions ?? GeometryOperator.GetDimensions(this);
      List<double> coords = new List<double>();

      foreach (int i in dimensions)
      { coords.Add(_dA1[i] + param * (2 * _dA2[i] + param * 3 * _dA3[i])); }
      return Point.Create(coords.ToArray());
    }

    internal double RadiusAtCore(double param, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(this);
      // R(t) = (x'^2 + y'^2)^3/2 / (x'y" - x"y')

      Point tan = TangentAtCore(param, dimensions);

      List<double> coords = new List<double>();

      foreach (int i in dimensions)
      { coords.Add(2 * _dA2[i] + param * (6 * _dA3[i])); }

      double ds = Math.Pow(tan[0] * tan[0] + tan[1] * tan[1], 1.5);
      double dn = tan[0] * coords[1] - coords[0] * tan[1];

      double r = ds / dn;
      return r;
    }

    public bool IsContinuous => true;
    public bool HasSubparts
    {
      get
      {
        if (this._hasSubparts.HasValue)
        { return this._hasSubparts.Value; }

        AssignParams();
        _hasSubparts = true; // will be overwritten at the end of the method when false
        for (int iDim = 0; iDim < Dimension; iDim++)
        {
          if (IsMonoton(iDim) == false)
          { return true; }
        }
        _hasSubparts = false;
        return false;
      }
    }

    private bool IsMonoton(int dim) => BezierOp.IsMonoton(this, dim);

    private List<double> CalcSubparts()
    {
      List<double> pParts = new List<double>();
      pParts.Add(0);
      double t0, t1;
      for (int iDim = 0; iDim < Dimension; iDim++)
      {
        if (Calc.SolveSqr(3 * _dA3[iDim],
          2 * _dA2[iDim], _dA1[iDim],
          out t0, out t1))
        {
          if (t0 > 0 && t0 < 1)
          { pParts.Add(t0); }
          if (t1 > 0 && t1 < 1)
          { pParts.Add(t1); }
        }
      }
      // Wendepunkt
      if (Calc.SolveSqr(
        3 * (_dA3[0] * _dA2[1] - _dA2[0] * _dA3[1]),
        3 * (_dA3[0] * _dA1[1] - _dA1[0] * _dA3[1]),
        _dA2[0] * _dA1[1] - _dA1[0] * _dA2[1], out t0, out t1))
      {
        if (t0 > 0 && t0 < 1)
        { pParts.Add(t0); }
        if (t1 > 0 && t1 < 1)
        { pParts.Add(t1); }
      }

      pParts.Add(1);
      pParts.Sort();
      return pParts;
    }

    protected override Curve SubpartCurve(double t0, double t1) => Subpart(t0, t1);
    public new Bezier Subpart(double t0, double t1) => BezierOp.Subpart(this, t0, t1);

    public override IList<IPoint> LinApprox(double t0, double t1) => BezierOp.LinApprox(this, t0, t1);

  }
  public class BezierWrap : BezierCore
  {
    public IBezier Bezier { get; }
    public BezierWrap(IBezier bezier)
    {
      Bezier = bezier;
    }
    protected override IPoint GetStart() => Bezier.Start;
    protected override IPoint GetP1() => Bezier.P1;
    protected override IPoint GetP2() => Bezier.P2;
    protected override IPoint GetEnd() => Bezier.End;

  }

  public class Bezier : BezierCore
  {
    private readonly IPoint _pStart;
    private readonly IPoint _pEnd;
    private readonly IPoint _p1;
    private readonly IPoint _p2;

    public Bezier(IPoint p0, IPoint p1, IPoint p2, IPoint p3)
    {
      _pStart = p0;
      _p1 = p1;
      _p2 = p2;
      _pEnd = p3;
    }

    protected override IPoint GetStart() => _pStart;
    protected override IPoint GetP1() => _p1;
    protected override IPoint GetP2() => _p2;
    protected override IPoint GetEnd() => _pEnd;
  }

  public static class BezierOp
  {
    public static Box GetExtent(IBezier bezier)
    {
      Box box = new Box(Point.Create(bezier.Start), Point.Create(bezier.End), true);
      box.Include(bezier.P1);
      box.Include(bezier.P2);
      return box;
    }

    public static double GetNormedMaxOffset(IBezier bezier)
    {
      // TODO: verify
      Point p = PointOp.Sub(bezier.End, bezier.Start);
      double l2 = p.OrigDist2();
      if (l2 == 0)
      {
        p = PointOp.Sub(bezier.P1, bezier.Start);
        l2 = p.OrigDist2();
      }
      if (l2 == 0)
      {
        p = PointOp.Sub(bezier.P2, bezier.Start);
        l2 = p.OrigDist2();
      }
      double maxOffset;
      if (l2 > 0)
      {
        OrthogonalSystem sys = OrthogonalSystem.Create(new IPoint[] { p });
        double o0 = sys.GetOrthogonal(PointOp.Sub(bezier.P1, bezier.Start)).Length2Epsi / l2;
        double o1 = sys.GetOrthogonal(PointOp.Sub(bezier.P2, bezier.P1)).Length2Epsi / l2;
        double o2 = sys.GetOrthogonal(PointOp.Sub(bezier.End, bezier.P2)).Length2Epsi / l2;
        maxOffset = Math.Sqrt(o0) + Math.Sqrt(o1) + Math.Sqrt(o2);
      }
      else
      { maxOffset = 0; }
      if (double.IsNaN(maxOffset))
      { }
      return maxOffset;
    }

    public static Bezier Clone(IBezier bezier)
    {
      return new Bezier(Point.Create(bezier.Start), Point.Create(bezier.P1),
        Point.Create(bezier.P2), Point.Create(bezier.End));
    }

    public static bool EqualGeometry(IBezier x, IBezier o, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(x, o);
      bool equal =
        PointOp.EqualGeometry(x.Start, o.Start, dimensions) &&
        PointOp.EqualGeometry(x.P1, o.P1, dimensions) &&
        PointOp.EqualGeometry(x.P2, o.P2, dimensions) &&
        PointOp.EqualGeometry(x.End, o.End, dimensions);
      return equal;
    }

    public static BezierCore CastOrWrap(IBezier bezier) => bezier as BezierCore ?? new BezierWrap(bezier);

    public static Bezier Invert(IBezier bezier)
    {
      return new Bezier(bezier.End, bezier.P2, bezier.P1, bezier.Start);
    }

    public static Point PointAt(IBezier bezier, double t, IEnumerable<int> dimensions = null)
    {
      BezierCore b = CastOrWrap(bezier);
      return b.PointAtCore(t, dimensions);
    }

    public static double Length(IBezier bezier)
    {
      BezierCore b = CastOrWrap(bezier);
      return b.LengthCore();
    }

    public static double ParamAt(IBezier bezier, double distance)
    {
      BezierCore b = CastOrWrap(bezier);
      return b.ParamAtCore(distance);
    }

    public static Bezier Project(IBezier bezier, IProjection projection)
    {
      return new Bezier(PointOp.Project(bezier.Start, projection),
        PointOp.Project(bezier.P1, projection), PointOp.Project(bezier.P2, projection),
        PointOp.Project(bezier.End, projection));
    }

    public static Bezier Subpart(IBezier bezier, double t0, double t1)
    {
      double dt = (t1 - t0) / 3;

      Point p0 = PointAt(bezier, t0);
      Point p3 = PointAt(bezier, t1);

      Point p1 = p0 + dt * TangentAt(bezier, t0);
      Point p2 = p3 - dt * TangentAt(bezier, t1);

      return new Bezier(p0, p1, p2, p3);
    }

    public static Point TangentAt(IBezier bezier, double param, IEnumerable<int> dimensions = null)
    {
      BezierCore b = CastOrWrap(bezier);
      return b.TangentAtCore(param, dimensions);
    }
    public static double RadiusAt(IBezier bezier, double param, IEnumerable<int> dimensions = null)
    {
      BezierCore b = CastOrWrap(bezier);
      return b.RadiusAtCore(param, dimensions);
    }

    public static IList<IPoint> LinApprox(IBezier bezier, double t0, double t1)
    {
      Bezier pPart = Subpart(bezier, t0, t1);
      return new IPoint[] { pPart.Start, pPart.P1, pPart.P2, pPart.End };
    }

    public static bool IsMonoton(IBezier bezier, int dim)
    {
      bool b01 = bezier.Start[dim] < bezier.P1[dim];
      bool b12 = bezier.P1[dim] < bezier.P2[dim];
      bool b23 = bezier.P2[dim] < bezier.End[dim];
      return ((b01 == b12 && b12 == b23) ||
        (bezier.Start[dim] == bezier.P1[dim] &&
        (b12 == b23 || bezier.P1[dim] == bezier.P2[dim] || bezier.P2[dim] == bezier.End[dim])) ||
        (bezier.P1[dim] == bezier.P2[dim] &&
        (b01 == b23 || bezier.P2[dim] == bezier.End[dim])) ||
        (bezier.P2[dim] == bezier.End[dim] && b01 == b12));
    }
  }

}
