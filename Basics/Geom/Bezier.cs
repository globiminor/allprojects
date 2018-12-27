using System;
using System.Collections.Generic;
using Basics.Num;
using PntOp = Basics.Geom.PointOperator;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Bezier.
  /// </summary>
  public class Bezier : Curve, IMultipartGeometry
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
      private Bezier _bezier;

      private Bezier _currentPart;
      private int _iPart;
      private int _iNParts;

      public _SubpartsEnumerator(Bezier bezier)
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
            _currentPart._bHasSubparts = false;
            _currentPart._bSubpartsKnown = true;
            return true;
          }
        }
      }

      #endregion
    }

    private class _SubpartsEnumerable : IEnumerable<Bezier>
    {
      private readonly Bezier _bezier;

      public _SubpartsEnumerable(Bezier bezier)
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

    #endregion

    private readonly IPoint _pStart;
    private readonly IPoint _pEnd;
    private IPoint _p1;
    private IPoint _p2;
    private _InnerCurve _innerCurve;

    private double _maxOffset = -1;

    private bool _paramKnown = false;
    private bool _bSubpartsKnown = false;
    private bool _bHasSubparts;
    private List<double> _dParts;

    private double[] _dA0;
    private double[] _dA1;
    private double[] _dA2;
    private double[] _dA3;

    public Bezier(IPoint p0, IPoint p1, IPoint p2, IPoint p3)
    {
      _pStart = p0;
      _p1 = p1;
      _p2 = p2;
      _pEnd = p3;
    }

    public IPoint P1
    {
      get { return _p1; }
    }
    public IPoint P2
    {
      get { return _p2; }
    }

    public override IBox Extent
    {
      get
      {
        Box box = new Box(Point.Create(_pStart), Point.Create(_pEnd), true);
        box.Include(_p1);
        box.Include(_p2);
        return box;
      }
    }

    public override IPoint Start
    {
      get { return _pStart; }
    }
    public override IPoint End
    {
      get { return _pEnd; }
    }

    public double NormedMaxOffset
    {
      get { return NormedMaxOffsetCore; }
    }
    protected override double NormedMaxOffsetCore
    {
      get
      {
        if (_maxOffset < 0)
        {
          // TODO: verify
          Point p = PntOp.Sub(End, Start);
          double l2 = p.OrigDist2();
          OrthogonalSystem sys = OrthogonalSystem.Create(new IPoint[] { p });
          double o0 = sys.GetOrthogonal(PntOp.Sub(P1, Start)).Length2Epsi / l2;
          double o1 = sys.GetOrthogonal(PntOp.Sub(P2, P1)).Length2Epsi / l2;
          double o2 = sys.GetOrthogonal(PntOp.Sub(End, P2)).Length2Epsi / l2;
          _maxOffset = Math.Sqrt(o0) + Math.Sqrt(o1) + Math.Sqrt(o2);
        }
        return _maxOffset;
      }
    }
    public override bool IsLinear
    { get { return false; } }

    public new Bezier Clone()
    { return (Bezier)Clone_(); }
    protected override Curve Clone_()
    { return Clone__(); }
    private Bezier Clone__()
    {
      return new Bezier(Point.Create(_pStart), Point.Create(_p1),
        Point.Create(_p2), Point.Create(_pEnd));
    }

    protected override double Parameter(ParamGeometryRelation split)
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
    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }

      if (!(other is Bezier o))
      { return false; }
      bool equal =
        Start.EqualGeometry(o.Start) &&
        P1.EqualGeometry(o.P1) &&
        P2.EqualGeometry(o.P2) &&
        End.EqualGeometry(o.End);
      return equal;
    }

    public new Bezier Invert()
    { return (Bezier)Invert_(); }
    protected override Curve Invert_()
    { return Invert__(); }
    private Bezier Invert__()
    {
      return new Bezier(_pEnd, _p2, _p1, _pStart);
    }

    /// <summary>
    /// Punkt bei Parameter t
    /// </summary>
    /// <param name="t">Wert zwischen 0 und 1</param>
    /// <returns></returns>
    public override Point PointAt(double t)
    {
      AssignParams();
      Point p = Point.Create(Dimension);
      for (int i = 0; i < Dimension; i++)
      {
        p[i] = _dA0[i] + t * (_dA1[i] + t * (_dA2[i] + t * _dA3[i]));
      }

      return p;
    }

    public override double Length()
    {
      AssignParams();
      Calc pCalc = new Calc(1e-8);

      return pCalc.Integral(LengthFactor, 0, 1);
    }
    public IPoint PointAt(double t, CurveAtType type)
    {
      if (type == CurveAtType.Parameter)
      { return PointAt(t); }
      else if (type == CurveAtType.Distance)
      {
        return null;
      }
      else
      { throw new ArgumentException(type + " not implemented"); }
    }

    private class ParamAtFct
    {
      private Bezier _bezier;
      private readonly double _distance;

      public ParamAtFct(Bezier bezier, double distance)
      {
        _bezier = bezier;
        _distance = distance;
      }
      public double LengthAt(double t)
      {
        double l0 = _bezier.Subpart(0, t).Length();
        if (t < 0) l0 = -l0;
        double diff = l0 -_distance;
        return diff;
      }
    }
    public override double ParamAt(double distance)
    {
      Calc numMath = new Calc(1e-6);
      ParamAtFct fct = new ParamAtFct(this, distance);

      return numMath.SolveNewton(fct.LengthAt, LengthFactor, distance / Length());
    }
    public double ParamAt_(double distance)
    {
      double dEpsi = 1e-5;
      Point t0 = (1.0 / 3.0) * TangentAt(0);
      Point t1 = (1.0 / 3.0) * TangentAt(1);

      double l0 = Math.Sqrt(t0.OrigDist2());
      double l1 = Math.Sqrt(t1.OrigDist2());

      return ParamAt(distance, 1, 0, _pStart, t0, l0, 1, _pEnd, t1, l1, dEpsi);
    }
    private double ParamAt(double distance, double f,
      double d0, IPoint p0, Point t0, double l0,
      double d1, IPoint p1, Point t1, double l1, double dEpsi)
    {
      Point f0 = p0 - f * t0;
      Point f1 = p1 - f * t1;
      double l2 = Math.Sqrt(f0.Dist2(f1));

      double lMin = Math.Sqrt(PntOp.Dist2(p0, p1));

      double lMax = (l0 + l1 + l2);
      if (distance > lMax)
      { return 0; }

      double lMean = (lMax + lMin) / 2.0;

      if (lMean - lMin > dEpsi * lMin)
      {
        double dm = (d0 + d1) / 2;
        Point pm = PointAt(dm);
        Point tm = (1.0 / 3.0) * TangentAt(dm);

        f /= 2;
        double lm = Math.Sqrt(tm.OrigDist2()) * f;

        double lSub0 = ParamAt(distance, f, d0, p0, t0, l0 / 2, dm, pm, tm, lm, dEpsi);
        double lSub1 = ParamAt(distance, f, d0, p0, t0, l0 / 2, dm, pm, tm, lm, dEpsi);

        lMean = lSub0 + lSub1;
      }

      return lMean;
    }

    private Bezier Project__(IProjection projection)
    {
      return new Bezier(Start.Project(projection),
        _p1.Project(projection), _p2.Project(projection),
        End.Project(projection));
    }
    protected override Geometry Project_(IProjection projection)
    { return Project__(projection); }
    public new Bezier Project(IProjection projection)
    { return (Bezier)Project_(projection); }

    public override InnerCurve InnerCurve()
    {
      return new _InnerCurve(_p1, _p2);
    }

    public new Bezier[] Split(IList<ParamGeometryRelation> splits)
    { return (Bezier[])SplitCore(splits); }
    protected override IList<Curve> SplitCore(IList<ParamGeometryRelation> splits)
    { return Split__(splits); }
    private Bezier[] Split__(IList<ParamGeometryRelation> splits)
    { return (Bezier[])GetSplitArray(this, splits); }

    public new Bezier Subpart(double t0, double t1)
    {
      double dt = (t1 - t0) / 3;

      Point p0 = PointAt(t0);
      Point p3 = PointAt(t1);

      Point p1 = p0 + dt * TangentAt(t0);
      Point p2 = p3 - dt * TangentAt(t1);

      return new Bezier(p0, p1, p2, p3);
    }
    protected override Curve SubpartCurve(double t0, double t1)
    { return Subpart(t0, t1); }

    public override Point TangentAt(double param)
    {
      AssignParams();
      int iDim = Dimension;

      Point p0 = Point.Create(iDim);

      for (int i = 0; i < iDim; i++)
      { p0[i] = _dA1[i] + param * (2 * _dA2[i] + param * 3 * _dA3[i]); }
      return p0;
    }
    public double RadiusAt(double param)
    {
      // R(t) = (x'^2 + y'^2)^3/2 / (x'y" - x"y')
      int iDim = Dimension;

      Point tan = TangentAt(param);

      Point p0 = Point.Create(iDim);

      for (int i = 0; i < iDim; i++)
      { p0[i] = 2 * _dA2[i] + param * (6 * _dA3[i]); }

      double ds = Math.Pow(tan[0] * tan[0] + tan[1] * tan[1], 1.5);
      double dn = tan[0] * p0[1] - p0[0] * tan[1];

      double r = ds / dn;
      return r;
    }

    public override IList<IPoint> LinApprox(double t0, double t1)
    {
      Bezier pPart = Subpart(t0, t1);
      return new IPoint[] { pPart.Start, pPart._p1, pPart._p2, pPart.End };
    }

    private double LengthFactor(double t)
    {
      return Math.Sqrt(TangentAt(t).OrigDist2());
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
        _dA1[i] = 3 * (_p1[i] - Start[i]);
        _dA3[i] = End[i] - 3 * _p2[i] + _dA1[i] + 2 * _dA0[i];
        _dA2[i] = End[i] - _dA3[i] - _dA1[i] - _dA0[i];
      }

      _paramKnown = true;
    }

    #region IMultipartGeometry Members

    public bool IsContinuous
    {
      get { return true; }
    }
    public bool HasSubparts
    {
      get
      {
        if (_bSubpartsKnown)
        { return _bHasSubparts; }

        AssignParams();
        _bSubpartsKnown = true;
        _bHasSubparts = true; // will be overwritten at the end of the method when false
        for (int iDim = 0; iDim < Dimension; iDim++)
        {
          if (IsMonoton(iDim) == false)
          { return true; }
        }
        _bHasSubparts = false;
        return false;
      }
    }

    private bool IsMonoton(int dim)
    {
      bool b01 = Start[dim] < _p1[dim];
      bool b12 = _p1[dim] < _p2[dim];
      bool b23 = _p2[dim] < End[dim];
      return ((b01 == b12 && b12 == b23) ||
        (Start[dim] == _p1[dim] &&
        (b12 == b23 || _p1[dim] == _p2[dim] || _p2[dim] == End[dim])) ||
        (_p1[dim] == _p2[dim] &&
        (b01 == b23 || _p2[dim] == End[dim])) ||
        (_p2[dim] == End[dim] && b01 == b12));
    }

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

    public IEnumerable<Bezier> Subparts()
    {
      return new _SubpartsEnumerable(this);
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }

    #endregion
  }

}
