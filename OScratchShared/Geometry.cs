
using System;
using System.Collections.Generic;
using System.Text;

namespace OMapScratch
{
  public class Box : IBox
  {
    public Box(Pnt min, Pnt max)
    {
      Min = min;
      Max = max;
    }
    public Pnt Min { get; }
    public Pnt Max { get; }

    public override string ToString()
    {
      return $"{Min};{Max}";
    }
    public void Include(IBox box)
    {
      Include(box.Min);
      Include(box.Max);
    }
    public void Include(Pnt pnt)
    {
      Min.X = Math.Min(Min.X, pnt.X);
      Min.Y = Math.Min(Min.Y, pnt.Y);
      Max.X = Math.Max(Max.X, pnt.X);
      Max.Y = Math.Max(Max.Y, pnt.Y);
    }

    public bool Intersects(IBox box)
    {
      if (box.Min.X > Max.X)
      { return false; }
      if (box.Min.Y > Max.Y)
      { return false; }
      if (box.Max.X < Min.X)
      { return false; }
      if (box.Max.Y < Min.Y)
      { return false; }

      return true;
    }
  }
  public class DirectedPnt : Pnt
  {
    public static DirectedPnt Create(Pnt p, float azimuth)
    {
      return new DirectedPnt(p.X, p.Y, azimuth);
    }
    public DirectedPnt()
    { }
    public DirectedPnt(float x, float y, float azimuth)
      : base(x, y)
    {
      Azimuth = azimuth;
    }
    public float Azimuth { get; set; }

    protected override string ToDrawableText()
    {
      return $"{DrawableUtils.DirPoint} {X:f1} {Y:f1} {Azimuth * 180 / Math.PI:f1}";
    }
  }
  public partial class Pnt : IDrawable, IBox
  {
    public float X { get; set; }
    public float Y { get; set; }

    public Pnt()
    { }
    public Pnt(float x, float y)
    {
      X = x;
      Y = y;
    }

    public override string ToString()
    {
      return $"{X},{Y}";
    }

    IBox IDrawable.Extent { get { return this; } }

    public Pnt Clone()
    {
      return CloneCore();
    }

    protected virtual Pnt CloneCore()
    {
      Pnt clone = (Pnt)MemberwiseClone();
      return clone;
    }

    Pnt IBox.Min { get { return this; } }
    Pnt IBox.Max { get { return this; } }

    public Pnt Project(IProjection prj)
    {
      return prj.Project(this);
    }

    IDrawable IDrawable.Project(IProjection prj)
    { return Project(prj); }

    string IDrawable.ToText()
    {
      return ToDrawableText();
    }
    protected virtual string ToDrawableText()
    {
      return $"{DrawableUtils.Point} {X:f1} {Y:f1}";
    }

    public float Dist2(Pnt other = null)
    {
      float dx = X - (other?.X ?? 0);
      float dy = Y - (other?.Y ?? 0);
      return (dx * dx + dy * dy);
    }
    IEnumerable<Pnt> IDrawable.GetVertices()
    {
      yield return new Pnt { X = X, Y = Y };
    }

    void IDrawable.Draw<T>(IGraphics<T> canvas, Symbol symbol, MatrixProps matrix, float symbolScale, T paint)
    {
      SymbolUtils.DrawPoint(canvas, symbol, matrix, symbolScale, this, paint);
    }

  }

  public partial class Lin : ISegment
  {
    public Pnt From { get; set; }
    public Pnt To { get; set; }

    public Lin()
    { }
    public Lin(Pnt from, Pnt to)
    {
      From = from;
      To = to;
    }
    ISegment ISegment.Clone()
    { return Clone(); }
    public Lin Clone()
    {
      return new Lin { From = From.Clone(), To = To.Clone() };
    }
    ISegment ISegment.Flip()
    { return Flip(); }
    public Lin Flip()
    {
      return new Lin { From = To.Clone(), To = From.Clone() };
    }

    IList<ISegment> ISegment.Split(float t)
    { return Split(t); }
    public Lin[] Split(float t)
    {
      Pnt split = At(t);
      Lin[] splits = new Lin[]
      {
        new Lin(From.Clone(), split),
        new Lin(split.Clone(), To.Clone())
      };
      return splits;
    }

    public Lin Project(IProjection prj)
    {
      return new Lin { From = From.Project(prj), To = To.Project(prj) };
    }

    public Pnt At(float t)
    {
      return new Pnt(From.X + t * (To.X - From.X), From.Y + t * (To.Y - From.Y));
    }

    public float GetAlong(Pnt p)
    {
      float l2 = From.Dist2(To);
      float scalar = (p.X - From.X) * (To.X - From.X) + (p.Y - From.Y) * (To.Y - From.Y);
      return scalar / l2;
    }

    public Box GetExtent()
    {
      Pnt min = From.Clone();
      Pnt max = From.Clone();

      Box box = new Box(min, max);
      box.Include(To);

      return box;
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

    void ISegment.InitToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.MoveTo} {From.X:f1} {From.Y:f1}"); }

    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.LineTo} {To.X:f1} {To.Y:f1}"); }
  }

  public partial class Circle : ISegment
  {
    public Pnt Center { get; set; }
    public float Radius { get; set; }

    public float? Azimuth { get; set; }
    public float? Angle { get; set; }

    Pnt ISegment.From
    {
      get { return new Pnt(GetStartX(), GetStartY()); }
      set { Center = new Pnt(value.X, value.Y - Radius); }
    }
    Pnt ISegment.To
    {
      get { return new Pnt(GetEndX(), GetEndY()); }
      set { Center = new Pnt(value.X, value.Y - Radius); }
    }

    private float Azi { get { return Azimuth ?? 0; } }
    private double Ang { get { return Angle ?? (2.0 * Math.PI); } }

    private float GetStartX()
    { return Center.X + (float)Math.Sin(Azi) * Radius; }
    private float GetStartY()
    { return Center.Y + (float)Math.Cos(Azi) * Radius; }

    private float GetEndX()
    { return Center.X + (float)Math.Sin(Azi + Ang) * Radius; }
    private float GetEndY()
    { return Center.Y + (float)Math.Cos(Azi + Ang) * Radius; }

    public Pnt At(float t)
    {
      double angle = Azi + t * Ang;
      double sin = Math.Sin(angle);
      double cos = Math.Cos(angle);

      double x = Center.X + sin * Radius;
      double y = Center.Y + cos * Radius;
      return new Pnt((float)x, (float)y);
    }

    IList<ISegment> ISegment.Split(float t)
    {
      return new ISegment[]
      {
        new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azi, Angle = t * Angle },
        new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azi + t * Angle, Angle = (1 - t) * Angle },
      };
    }

    float ISegment.GetAlong(Pnt p)
    {
      double f = (Math.PI / 2 - Math.Atan2(p.Y - Center.Y, p.X - Center.X)) / (2 * Math.PI);
      if (f < 0) { f += 1; }
      f = (f - Azi) / Ang;
      return (float)f;
    }
    public Box GetExtent()
    {
      Pnt min = new Pnt(Center.X - Radius, Center.Y - Radius);
      Pnt max = new Pnt(Center.X + Radius, Center.Y + Radius);

      return new Box(min, max);
    }

    ISegment ISegment.Clone()
    { return Clone(); }
    public Circle Clone()
    {
      return new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azimuth, Angle = Angle };
    }
    ISegment ISegment.Flip()
    { return Flip(); }
    public Circle Flip()
    {
      return new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azimuth + Angle, Angle = -Angle };
    }


    public Circle Project(IProjection prj)
    {
      Pnt center = Center.Project(prj);
      Pnt start = new Pnt(GetStartX(), GetStartY()).Project(prj);
      float dx = start.X - center.X;
      float dy = start.Y - center.Y;
      float radius = (float)Math.Sqrt(dx * dx + dy * dy);
      Circle projected = new Circle { Center = center, Radius = radius };

      if (Azimuth != null && Angle != null)
      {
        projected.Azimuth = (float)Math.Atan2(dx, dy);
        projected.Angle = Angle;
      }
      return projected;
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

    void ISegment.InitToText(StringBuilder sb)
    { }
    void ISegment.AppendToText(StringBuilder sb)
    {
      if (Azimuth == null || Angle == null)
      { sb.Append($" {DrawableUtils.Circle} {Center.X:f1} {Center.Y:f1} {Radius:f1}"); }
      else
      { sb.Append($" {DrawableUtils.Arc} {Center.X:f1} {Center.Y:f1} {Radius:f1} {Azimuth * 180 / Math.PI:f1} {Angle * 180 / Math.PI:f1}"); }
    }
  }

  public partial class Bezier : ISegment
  {
    public Pnt From { get; set; }
    public Pnt I0 { get; set; }
    public Pnt I1 { get; set; }
    public Pnt To { get; set; }

    public Bezier()
    { }
    public Bezier(Pnt from, Pnt i0, Pnt i1, Pnt to)
    {
      From = from;
      I0 = i0;
      I1 = i1;
      To = to;
    }
    ISegment ISegment.Clone()
    { return Clone(); }
    public Bezier Clone()
    {
      return new Bezier(From.Clone(), I0.Clone(), I1.Clone(), To.Clone());
    }
    ISegment ISegment.Flip()
    { return Flip(); }
    public Bezier Flip()
    {
      return new Bezier(To.Clone(), I1.Clone(), I0.Clone(), From.Clone());
    }

    public Bezier Project(IProjection prj)
    {
      return new Bezier(From.Project(prj), I0.Project(prj), I1.Project(prj), To.Project(prj));
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

    public float GetAlong(Pnt p)
    {
      float a0 = new Lin(From, I0).GetAlong(p);
      float a1 = new Lin(I0, I1).GetAlong(p);
      float a2 = new Lin(I1, To).GetAlong(p);

      float b0 = Math.Max(0, Math.Min(1, a0));
      float b1 = Math.Max(0, Math.Min(1, a1));
      float b2 = Math.Max(0, Math.Min(1, a2));

      float at0 = b0 / 3;
      float at1 = (b1 + 1) / 3;
      float at2 = (b2 + 2) / 3;
      Pnt p0 = At(at0);
      Pnt p1 = At(at1);
      Pnt p2 = At(at2);

      float d0 = p0.Dist2(p);
      float d1 = p1.Dist2(p);
      float d2 = p2.Dist2(p);

      if (d0 <= d1 && d0 <= d2)
      { return at0; }
      if (d1 <= d0 && d1 <= d2)
      { return at1; }

      return at2;
    }

    IList<ISegment> ISegment.Split(float t)
    { return Split(t); }
    public Bezier[] Split(float t)
    {
      Pnt splt = At(t);
      float u = 1 - t;
      float dx0 = I0.X - From.X;
      float dy0 = I0.Y - From.Y;

      float dx1 = I1.X - From.X;
      float dy1 = I1.Y - From.Y;

      Bezier[] splits = new Bezier[]
      {
        new Bezier(From.Clone(), new Pnt(From.X + t * dx0, From.Y + t * dy0), new Pnt(From.X + t * dx1, From.Y + t * dy1), splt),
        new Bezier(splt.Clone(), new Pnt(splt.X + u * dx0, splt.Y + u * dy0), new Pnt(splt.X + u * dx1, splt.Y + u * dy1), To.Clone()),
      };
      return splits;
    }

    public Pnt At(float t)
    {
      float x;
      {
        float a0 = From.X;
        float a1 = 3 * (I0.X - From.X);
        float a3 = To.X - 3 * I1.X + a1 + 2 * a0;
        float a2 = To.X - a3 - a1 - a0;

        x = a0 + t * (a1 + t * (a2 + t * a3));
      }
      float y;
      {
        float a0 = From.Y;
        float a1 = 3 * (I0.Y - From.Y);
        float a3 = To.Y - 3 * I1.Y + a1 + 2 * a0;
        float a2 = To.Y - a3 - a1 - a0;

        y = a0 + t * (a1 + t * (a2 + t * a3));
      }

      return new Pnt(x, y);
    }

    public Box GetExtent()
    {
      Pnt min = From.Clone();
      Pnt max = From.Clone();

      Box box = new Box(min, max);
      box.Include(I0);
      box.Include(I1);
      box.Include(To);

      return box;
    }

    void ISegment.InitToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.MoveTo} {From.X:f1} {From.Y:f1}"); }

    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.CubicTo} {I0.X:f1} {I0.Y:f1} {I1.X:f1} {I1.Y:f1} {To.X:f1} {To.Y:f1}"); }
  }

  public partial class Curve : IDrawable
  {
    private Box _extent;
    private readonly List<ISegment> _segments = new List<ISegment>();

    private float _tx, _ty;

    public Curve MoveTo(Pnt pnt) => MoveTo(pnt.X, pnt.Y);
    public Curve MoveTo(double[] xy) => MoveTo((float)xy[0], (float)xy[1]);
    public Curve MoveTo(float x, float y)
    {
      _tx = x;
      _ty = y;
      _extent = null;
      return this;
    }

    public IReadOnlyList<ISegment> Segments
    { get { return _segments; } }

    public ISegment this[int i]
    {
      get { return _segments[i]; }
      set
      {
        _segments[i] = value;
        _extent = null;
      }
    }
    public void Add(ISegment seg)
    {
      _segments.Add(seg);
      _extent?.Include(seg.GetExtent());
      SetTo(seg.To);
    }

    public void Insert(int i, ISegment seg)
    {
      if (i >= _segments.Count)
      {
        Add(seg);
        return;
      }
      _segments.Insert(i, seg);
      _extent?.Include(seg.GetExtent());
    }
    public void RemoveAt(int i)
    {
      _segments.RemoveAt(i);
    }
    public IBox Extent
    {
      get
      {
        _extent = _extent ?? GetExtent();
        return (IBox)_extent ?? From;

        Box GetExtent()
        {
          if (_segments.Count == 0)
          { return null; }
          Box extent = _segments[0].GetExtent();
          foreach (var seg in _segments)
          { extent.Include(seg.GetExtent()); }
          return extent;
        }
      }
    }
    public int Count { get { return _segments.Count; } }
    public Pnt From
    {
      get
      {
        if (Count > 0)
        { return this[0].From; }
        return new Pnt { X = _tx, Y = _ty };
      }
    }
    public Curve Append(ISegment segment)
    {
      Add(segment);
      Pnt end = segment.To;
      _tx = end.X;
      _ty = end.Y;

      _extent = null;
      return this;
    }

    public Curve LineTo(Pnt pnt) => LineTo(pnt.X, pnt.Y);
    public Curve LineTo(double[] xy) => LineTo((float)xy[0], (float)xy[1]);
    public Curve LineTo(float x, float y)
    {
      Add(new Lin { From = new Pnt { X = _tx, Y = _ty }, To = new Pnt { X = x, Y = y } });
      return this;
    }

    public Curve CubicTo(float tx0, float ty0, float tx1, float ty1, float x, float y)
    {
      Add(new Bezier
      {
        From = new Pnt { X = _tx, Y = _ty },
        I0 = new Pnt { X = tx0, Y = ty0 },
        I1 = new Pnt { X = tx1, Y = ty1 },
        To = new Pnt { X = x, Y = y }
      });
      return this;
    }

    public Curve Circle(float centerX, float centerY, float radius, float? azimuth = null, float? angle = null)
    {
      Circle circle = new Circle { Center = new Pnt { X = centerX, Y = centerY }, Radius = radius, Azimuth = azimuth, Angle = angle };
      Add(circle);
      return this;
    }

    public Curve Project(IProjection prj)
    {
      Curve projected = new Curve();
      foreach (var seg in Segments)
      {
        projected.Add(seg.Project(prj));
      }
      return projected;
    }
    IDrawable IDrawable.Project(IProjection prj)
    { return Project(prj); }
    string IDrawable.ToText()
    {
      if (Count <= 0)
      { return null; }

      StringBuilder sb = new StringBuilder();
      this[0].InitToText(sb);
      foreach (var seg in Segments)
      { seg.AppendToText(sb); }

      return sb.ToString();
    }
    IEnumerable<Pnt> IDrawable.GetVertices()
    {
      if (Count <= 0)
      { yield break; }
      yield return this[0].From;
      foreach (var seg in Segments)
      { yield return seg.To; }
    }

    private void SetTo(Pnt p)
    {
      _tx = p.X;
      _ty = p.Y;
    }
    public void RemoveLast()
    {
      if (Count <= 0)
      { return; }
      if (Count == 1)
      { SetTo(this[0].From); }
      else
      { SetTo(this[Count - 2].To); }
      RemoveAt(Count - 1);

      _extent = null;
    }

    public Curve Clone()
    {
      Curve clone = new Curve();
      foreach (var seg in Segments)
      { clone.Add(seg.Clone()); }
      clone._tx = _tx;
      clone._ty = _ty;
      return clone;
    }

    public Curve Flip()
    {
      Curve flip = new Curve();
      List<ISegment> reverse = new List<ISegment>(Segments);
      reverse.Reverse();
      foreach (var seg in reverse)
      { flip.Add(seg.Flip()); }
      return flip;
    }

    void IDrawable.Draw<T>(IGraphics<T> canvas, Symbol symbol, MatrixProps matrix, float symbolScale, T paint)
    {
      SymbolUtils.DrawLine(canvas, symbol, matrix, symbolScale, this, paint);
    }
  }

  public class MatrixProps
  {
    private readonly float[] _m;
    private float? _scale;
    private float? _rotate;

    public MatrixProps(float[] matrix)
    {
      _m = matrix;
    }
    public float[] Matrix { get { return _m; } }

    public float Scale
    {
      get
      {
        return _scale ??
          (_scale = (float)Math.Sqrt(Math.Abs(_m[0] * _m[4] - _m[1] * _m[3]))).Value;
      }
    }

    public float Rotate
    {
      get
      {
        return _rotate ??
          (_rotate = (float)Math.Atan2(_m[1], _m[0])).Value;
      }
    }
  }

  public partial class MatrixPrj : IProjection
  {
    private readonly double[] _matrix;
    public MatrixPrj(double[] matrix)
    { _matrix = matrix; }
    public Pnt Project(Pnt p)
    {
      double[] xy = Project(p.X, p.Y);
      return new Pnt((float)xy[0], (float)xy[1]);
    }
    public double[] Matrix => _matrix;

    public double[] Project(double[] xy)
    {
      return Project(xy[0], xy[1]);
    }
    public double[] Project(double x, double y)
    {
      return new double[]
      {
        _matrix[0] * x + _matrix[1] * y + _matrix[4],
        _matrix[2] * x + _matrix[3] * y + _matrix[5] };
    }

    public MatrixPrj GetInverse()
    {
      double[] m = _matrix;
      double det = m[0] * m[3] - m[1] * m[2];
      double[] inv = new double[] {
        m[3] / det , - m[1] / det,
        -m[2] / det,  m[0] / det,
        (m[5] * m[1] - m[4] * m[3]) / det, (m[4] * m[2] - m[5] * m[0]) / det
      };
      return new MatrixPrj(inv);
    }

    public static Curve GetLocalBox(MatrixPrj targetInversePrj, Pnt boxMax, MatrixPrj boxPrj)
    {
      double[] p0 = targetInversePrj.Project(boxPrj.Project(0, 0));
      double[] p1 = targetInversePrj.Project(boxPrj.Project(boxMax.X, 0));
      double[] p2 = targetInversePrj.Project(boxPrj.Project(0, boxMax.Y));
      double[] p3 = targetInversePrj.Project(boxPrj.Project(boxMax.X, boxMax.Y));
      Curve c = new Curve().MoveTo(p0).LineTo(p1).LineTo(p2).LineTo(p3);

      return c;
    }
  }
}
