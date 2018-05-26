using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom
{

  public interface IMesh
  {
    /// <summary>
    /// enumerates all points of mesh, with point being p.Start of the returned IMeshLine p
    /// </summary>
    IEnumerable<IMeshLine> Points { get; }
    IComparer<IMeshLine> LineComparer { get; }
  }
  public interface ITileMesh : IMesh
  {
    IBox TileExtent { get; }
  }

  public interface IMeshTri
  {

  }

  public interface IMeshLine
  {
    IPoint Start { get; }
    IPoint End { get; }

    IMeshLine GetNextTriLine();
    IMeshLine GetPreviousTriLine();

    IMeshLine Invers();
  }

  public partial class Mesh
  {
    public abstract class Tri : IGeometry, IMeshTri
    {
      private Point2D _center;
      private double _radius2;

      private uint _processed;

      public bool EqualGeometry(IGeometry other)
      { return this == other; }

      public abstract MeshLine this[int i] { get; }

      public Point2D Center
      {
        get
        {
          if (_center == null)
          { CalcCenterRadius(); }
          return _center;
        }
        protected set { _center = value; }
      }
      public double Radius2
      {
        get
        {
          if (_radius2 <= 0)
          { CalcCenterRadius(); }
          return _radius2;
        }
        protected set { _radius2 = value; }
      }

      private void CalcCenterRadius()
      {
        IPoint p0 = this[0].Start;
        _center = Geometry.OutCircleCenter(p0, this[1].Start, this[2].Start);
        if (_center != null)
        { _radius2 = _center.Dist2(p0); }
        else
        { _radius2 = double.PositiveInfinity; }
      }

      public int GetLineIndex(MeshLine line)
      {
        for (int i = 0; i < 3; i++)
        {
          if (this[i].HasEqualBaseLine(line) &&
            this[i].IsReverse == line.IsReverse)
          { return i; }
        }
        return -1;
      }

      /// <summary>
      /// M2GetNextLine
      /// </summary>
      public abstract MeshLine GetNextLine(MeshLine line);

      /// <summary>
      /// M2GetPreviousLine
      /// </summary>
      public abstract MeshLine GetPreviousLine(MeshLine line);

      public IPoint GetPoint(int i)
      {
        return this[i].Start;
      }

      public int GetPointIndex(Point2D point)
      {
        if (GetPoint(0) == point) { return 0; }
        if (GetPoint(1) == point) { return 1; }
        if (GetPoint(2) == point) { return 2; }
        return -1;
      }
      #region IGeometry Members

      public int Dimension
      {
        get { return 2; }
      }

      public int Topology
      {
        get { return 2; }
      }

      public bool IsWithin(IPoint point)
      {
        IPoint p0 = this[0].Start;
        double dx = point.X - p0.X;
        double dy = point.Y - p0.Y;

        IPoint p = this[1].Start;
        double x = p.X - p0.X;
        double y = p.Y - p0.Y;
        double u = x * dx + y * dy;
        if (u < 0 || u > 1)
        { return false; }

        p = this[2].Start;
        x = p.X - p0.X;
        y = p.Y - p0.Y;
        double v = x * dx + y * dy;

        if (v < 0 || v > 1 || u + v > 1)
        { return false; }
        return true;
      }

      public abstract Box Extent { get; }
      IBox IGeometry.Extent
      { get { return Extent; } }

      public Polyline Border
      {
        get
        {
          return Polyline.Create(
            new[] { this[0].Start, this[1].Start, this[2].Start });
        }
      }
      IGeometry IGeometry.Border
      { get { return Border; } }

      IGeometry IGeometry.Project(IProjection projection)
      { return null; }

      public IGeometry Split(IGeometry[] border)
      {
        throw new NotImplementedException();
      }

      #endregion

      internal bool GetProcessed(uint processId)
      {
        return ((_processed & processId) != 0);
      }
      internal void SetProcessed(uint processId, bool processed)
      {
        if (processed)
        { _processed |= processId; }
        else
        { _processed = (_processed & processId) ^ _processed; }
      }

    }

    internal class TriEx : Tri
    {
      private readonly MeshLineEx[] _line;
      public TriEx(MeshLineEx l0, MeshLineEx l1, MeshLineEx l2)
      {
        _line = new[] { l0, l1, l2 };
        l0.LeftTriEx = this;
        l1.LeftTriEx = this;
        l2.LeftTriEx = this;
      }

      public override MeshLine this[int i]
      {
        get
        { return _line[i]; }
      }
      public MeshLineEx _GetLine(int i)
      {
        return _line[i];
      }
      public void _SetLine(int i, MeshLineEx l)
      {
        _line[i] = l;
        l.LeftTriEx = this;
      }

      public MeshLineEx _GetNextLine(MeshLine line)
      {
        int index = GetLineIndex(line);
        if (index < 0)
        { return null; }

        return _GetLine(index < 2 ? index + 1 : 0);
      }
      public override MeshLine GetNextLine(MeshLine line)
      {
        return _GetNextLine(line);
      }

      public MeshLineEx _GetPreviousLine(MeshLine line)
      {
        int index = GetLineIndex(line);
        if (index < 0)
        { return null; }

        return _GetLine(index > 0 ? index - 1 : 2);
      }

      public override MeshLine GetPreviousLine(MeshLine line)
      {
        return _GetPreviousLine(line);
      }

      public void ClearCircle()
      {
        Center = null;
        Radius2 = 0;
      }

      public bool Validate()
      {
        //for (int i = 0; i < 3; i++)
        //{
        //  if (_line[i].LeftTri != this)
        //  { return false; }
        //  if (_line[i].End != _line[i < 2 ? i + 1 : 0].Start)
        //  { return false; }
        //  if (_line[i].Start == _line[i < 2 ? i + 1 : 0].Start)
        //  { return false; }
        //}

        return true;
      }

      public override Box Extent
      {
        get
        {
          IPoint p0 = Point.Create(_line[0].Start);
          IPoint p1 = Point.Create(p0);
          for (int i = 1; i < 3; i++)
          {
            IPoint p = _line[i].Start;
            if (p.X < p0.X)
            { p0.X = p.X; }
            else if (p.X > p1.X)
            { p1.X = p.X; }
            if (p.Y < p0.Y)
            { p0.Y = p.Y; }
            else if (p.Y > p1.Y)
            { p1.Y = p.Y; }
          }
          return new Box(p0, p1);
        }
      }
    }

    internal class MeshBaseLine
    {
      private object _tag;
      private bool _tagReverse;

      private uint _processed;

      public MeshBaseLine(MeshPoint startPoint, MeshPoint endPoint, object tag)
      {
        StartPoint = startPoint;
        EndPoint = endPoint;
        _tag = tag;
      }

      public MeshPoint StartPoint { get; set; }

      public MeshPoint EndPoint { get; set; }

      public TriEx LeftTri { get; set; }

      public TriEx RightTri { get; set; }

      public object GetTag(out bool isReverse)
      {
        isReverse = _tagReverse;
        return _tag;
      }

      public void SetTag(object tag, bool isReverse)
      {
        _tag = tag;
        _tagReverse = isReverse;
      }

      internal bool GetProcessed(uint processId)
      {
        return ((_processed & processId) != 0);
      }
      internal void SetProcessed(uint processId, bool processed)
      {
        if (processed)
        { _processed |= processId; }
        else
        { _processed = (_processed & processId) ^ _processed; }
      }
    }

    public abstract class MeshLine : Line, IMeshLine
    {
      public abstract Tri LeftTri { get; }
      public abstract Tri RightTri { get; }
      public abstract object GetTag(out bool isReverse);
      public abstract void SetTag(object tag, bool isReverse);

      public bool IsReverse { get; protected set; }

      public abstract bool HasEqualBaseLine(MeshLine line);
      IMeshLine IMeshLine.Invers()
      { return Invers(); }
      public abstract MeshLine Invers();

      public static MeshLine operator -(MeshLine line)
      {
        return line.Invers();
      }

      IMeshLine IMeshLine.GetNextTriLine()
      { return GetNextTriLine(); }
      /// <summary>
      /// M2GetNextLine_
      /// </summary>
      public MeshLine GetNextTriLine()
      {
        Tri tri = LeftTri;
        if (tri == null)
        { return null; }
        return tri.GetNextLine(this);
      }

      IMeshLine IMeshLine.GetPreviousTriLine()
      { return GetPreviousTriLine(); }
      /// <summary>
      /// M2GetPreviosLine_
      /// </summary>
      public MeshLine GetPreviousTriLine()
      {
        Tri tri = LeftTri;
        if (tri == null)
        { return null; }
        return tri.GetPreviousLine(this);
      }

      /// <summary>
      /// return next line after this starting at start point of this 
      /// in counterclockwise direction
      /// </summary>
      public MeshLine GetNextPointLine()
      {
        Tri tri = LeftTri;
        if (tri != null)
        { return -tri.GetPreviousLine(this); }

        MeshLine l = this;
        tri = RightTri;
        while (tri != null)
        {
          l = tri.GetNextLine(-l);
          tri = l.RightTri;
        }
        return l;
      }

      /// <summary>
      /// return next line after this starting at start point of this 
      /// in clockwise direction
      /// </summary>
      public MeshLine GetPreviousPointLine()
      {
        Tri tri = RightTri;
        if (tri != null)
        { return tri.GetNextLine(-this); }

        MeshLine l = this;
        tri = LeftTri;
        while (tri != null)
        {
          l = -tri.GetPreviousLine(l);
          tri = l.LeftTri;
        }
        return l;
      }

      public abstract void ChangeDiag(int rec);

      public bool OptionChangeDiag(int rec)
      {
        if (GetTag(out bool isReverse) != null)
        { return false; }

        Tri lTri = LeftTri;

        int i0 = lTri.GetLineIndex(this);
        int i1 = i0 < 2 ? i0 + 1 : 0;

        IPoint left = lTri[i1].End;
        Tri rTri = RightTri;
        if (rTri == null)
        {
          if (double.IsPositiveInfinity(lTri.Radius2) == false)
          { return false; }
          ChangeDiag(rec);
          return true;
        }
        if (rTri.Center != null && rTri.Center.Dist2(left) >= rTri.Radius2)
        { return false; }

        ChangeDiag(rec);
        return true;
      }
      #region ILine Members
      public int Count
      {
        get
        { return 2; }
      }
      public IPoint this[int index]
      {
        get
        {
          if (index == 0)
          { return Start; }
          else if (index == 1)
          { return End; }
          else
          { throw new InvalidOperationException(string.Format("Index {0} out of Range", index)); }
        }
      }
      #endregion
    }

    internal class MeshLineEx : MeshLine
    {
      private readonly MeshBaseLine _baseLine;

      public MeshLineEx(MeshPoint startPoint, MeshPoint endPoint, object tag, bool isReverse)
      {
        _baseLine = new MeshBaseLine(startPoint, endPoint, tag);
        IsReverse = isReverse;
      }
      public MeshLineEx(MeshPoint startPoint, MeshPoint endPoint)
      {
        _baseLine = new MeshBaseLine(startPoint, endPoint, null);
        IsReverse = false;
      }
      public MeshLineEx(MeshBaseLine baseLine, bool isReverse)
      {
        _baseLine = baseLine;
        IsReverse = isReverse;
      }

      public override MeshLine Invers()
      {
        return new MeshLineEx(BaseLine, !(IsReverse));
      }

      public MeshPoint StartPointEx
      {
        get
        {
          if (IsReverse == false)
          { return _baseLine.StartPoint; }
          else
          { return _baseLine.EndPoint; }
        }
        set
        {
          if (IsReverse == false)
          { _baseLine.StartPoint = value; }
          else
          { _baseLine.EndPoint = value; }
        }
      }

      public override IPoint Start
      {
        get { return StartPointEx.Point; }
      }

      public MeshPoint EndPointEx
      {
        get
        {
          if (IsReverse == false)
          { return _baseLine.EndPoint; }
          else
          { return _baseLine.StartPoint; }
        }
        set
        {
          if (IsReverse == false)
          { _baseLine.EndPoint = value; }
          else
          { _baseLine.StartPoint = value; }
        }
      }

      public override IPoint End
      {
        get { return EndPointEx.Point; }
      }


      public TriEx LeftTriEx
      {
        get
        {
          if (IsReverse == false)
          { return _baseLine.LeftTri; }
          else
          { return _baseLine.RightTri; }
        }
        set
        {
          if (IsReverse == false)
          { _baseLine.LeftTri = value; }
          else
          { _baseLine.RightTri = value; }
        }
      }

      public override Tri LeftTri
      {
        get
        { return LeftTriEx; }
      }

      public TriEx RightTriEx
      {
        get
        {
          if (IsReverse == false)
          { return _baseLine.RightTri; }
          else
          { return _baseLine.LeftTri; }
        }
      }

      public override Tri RightTri
      {
        get
        { return RightTriEx; }
      }

      public override object GetTag(out bool isReverse)
      {
        object tag = _baseLine.GetTag(out isReverse);
        isReverse = (IsReverse == isReverse);
        return tag;
      }
      public override void SetTag(object tag, bool isReverse)
      {
        isReverse = (isReverse == IsReverse);
        _baseLine.SetTag(tag, isReverse);
      }

      public MeshBaseLine BaseLine
      {
        get
        { return _baseLine; }
      }

      public override bool HasEqualBaseLine(MeshLine line)
      {
        MeshLineEx l = line as MeshLineEx;
        if (l == null)
        { return false; }
        return _baseLine == l.BaseLine;
      }


      public new MeshLineEx GetNextTriLine()
      {
        TriEx tri = LeftTriEx;
        if (tri == null)
        { return null; }
        return tri._GetNextLine(this);
      }

      public static MeshLineEx operator -(MeshLineEx line)
      {
        return new MeshLineEx(line._baseLine, !(line.IsReverse));
      }

      /// <summary>
      /// M2GetNextLine_
      /// </summary>
      public MeshLineEx _GetNextTriLine()
      {
        TriEx tri = LeftTriEx;
        if (tri == null)
        { return null; }
        return tri._GetNextLine(this);
      }

      /// <summary>
      /// M2GetPreviosLine_
      /// </summary>
      public MeshLineEx _GetPreviousTriLine()
      {
        TriEx tri = LeftTriEx;
        if (tri == null)
        { return null; }
        return tri._GetPreviousLine(this);
      }

      /// <summary>
      /// return next line after this starting at start point of this 
      /// in counterclockwise direction
      /// </summary>
      public MeshLineEx _GetNextPointLine()
      {
        TriEx tri = LeftTriEx;
        if (tri != null)
        { return -tri._GetPreviousLine(this); }

        MeshLineEx l = this;
        tri = RightTriEx;
        while (tri != null)
        {
          l = tri._GetNextLine(-l);
          tri = l.RightTriEx;
        }
        return l;
      }

      /// <summary>
      /// return next line after this starting at start point of this 
      /// in clockwise direction
      /// </summary>
      public MeshLineEx _GetPreviousPointLine()
      {
        TriEx tri = RightTriEx;
        if (tri != null)
        { return tri._GetNextLine(-this); }

        MeshLineEx l = this;
        tri = LeftTriEx;
        while (tri != null)
        {
          l = -tri._GetPreviousLine(l);
          tri = l.LeftTriEx;
        }
        return l;
      }

      /// <summary>
      /// splits the meshline at point by creating a new meshline from start to point
      /// (return value) and replacing the start point of this by point
      /// </summary>
      /// <param name="point"></param>
      /// <returns>new meshline</returns>
      public MeshLineEx Split(MeshPoint point)
      {
        MeshLineEx l = new MeshLineEx(StartPointEx, point);
        StartPointEx.IndexLine = l;
        StartPointEx = point;
        point.IndexLine = this;
        return l;
      }

      public override void ChangeDiag(int rec)
      {
        if (rec > 100)
        { }

        TriEx lTri = LeftTriEx;
        TriEx rTri = RightTriEx;
        int i0 = lTri.GetLineIndex(this);
        int i1 = (i0 < 2 ? i0 + 1 : 0);
        int i2 = (i1 < 2 ? i1 + 1 : 0);
        MeshLineEx lL1 = lTri._GetLine(i1);
        MeshLineEx lL2 = lTri._GetLine(i2);

        if (rTri == null)
        { // Drop line
          if (lL1._baseLine.LeftTri == lTri)
          { lL1._baseLine.LeftTri = null; }
          else
          {
            Debug.Assert(lL1._baseLine.RightTri == lTri);
            lL1._baseLine.RightTri = null;
          }
          if (lL2._baseLine.LeftTri == lTri)
          { lL2._baseLine.LeftTri = null; }
          else
          {
            Debug.Assert(lL2._baseLine.RightTri == lTri);
            lL2._baseLine.RightTri = null;
          }
          lL1.StartPointEx.IndexLine = lL1;
          lL2.EndPointEx.IndexLine = -lL2;

          return;
        }

        MeshLineEx rL0 = -this;
        i0 = rTri.GetLineIndex(rL0);
        i1 = (i0 < 2 ? i0 + 1 : 0);
        i2 = (i1 < 2 ? i1 + 1 : 0);
        MeshLineEx rL1 = rTri._GetLine(i1);
        MeshLineEx rL2 = rTri._GetLine(i2);

        lTri.ClearCircle();
        rTri.ClearCircle();

        StartPointEx = rL2.StartPointEx;
        EndPointEx = lL2.StartPointEx;

        lTri._SetLine(0, rL2);
        lTri._SetLine(1, lL1);
        lTri._SetLine(2, rL0);

        rTri._SetLine(0, rL1);
        rTri._SetLine(1, this);
        rTri._SetLine(2, lL2);

        if (lTri.Validate() == false) throw new InvalidProgramException("Error near " + rL2.Start);
        if (rTri.Validate() == false) throw new InvalidProgramException("Error near " + rL1.Start);

        lL1.StartPointEx.IndexLine = lL1;
        rL1.StartPointEx.IndexLine = rL1;

        rL2.OptionChangeDiag(rec + 1);
        rL1.OptionChangeDiag(rec + 1);
      }
    }

    public class MeshPoint : IGeometry
    {
      private readonly IPoint _point;
      private MeshLineEx _indexLine;

      public MeshPoint(IPoint point)
      {
        _point = point;
      }
      public IPoint Point
      {
        get { return _point; }
      }

      public bool EqualGeometry(IGeometry other)
      { return this == other; }

      internal MeshLineEx IndexLine
      {
        get { return _indexLine; }
        set
        {
          Debug.Assert(value.StartPointEx == this);
          _indexLine = value;
        }
      }
      #region IGeometry Members

      public int Dimension
      {
        get
        { return _point.Dimension; }
      }

      public int Topology
      {
        get
        { return _point.Topology; }
      }

      public bool IsWithin(IPoint point)
      { return false; }

      public bool Intersects(IGeometry other)
      {
        return other.IsWithin(_point);
      }

      public IBox Extent
      {
        get
        { return _point.Extent; }
      }

      public IGeometry Border
      { get { return null; } }

      public IPoint Project(IProjection projection)
      {
        return _point.Project(projection);
      }
      IGeometry IGeometry.Project(IProjection projection)
      { return Project(projection); }

      #endregion
    }

    private class PointBoxTree : BoxTree<MeshPoint>
    {
      internal PointBoxTree()
        : base(2)
      { }
      internal PointBoxTree(int capacity)
        : base(2, capacity, true)
      { }
      internal int Add(MeshPoint point)
      {
        return Add(point.Point.Extent, point);
      }
      internal void Insert(int index, MeshPoint point)
      {
        Insert(index, new TileEntry(point.Point.Extent, point));
      }
    }
  }
}
