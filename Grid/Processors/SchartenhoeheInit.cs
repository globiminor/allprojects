using Basics.Geom;
using Basics.Geom.Network;
using Basics.Geom.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grid.Processors
{
  public class SchartenhoeheInit : ContainerProcessor
  {
    private readonly IDoubleGrid _grid;

    public SchartenhoeheInit(IDoubleGrid grid)
    {
      _grid = grid;
    }

    protected override IList<TableAction> Init()
    {
      GridTable<double> gridTbl = new GridTable<double>(_grid);
      TableAction action = TableAction.Create<GridRow<double>>(gridTbl, HandleGridRow);
      return new List<TableAction> { action };
    }

    private void HandleGridRow(GridRow<double> gridRow)
    {
      IGrid<double> rowGrid = gridRow.Grid;
      HandleTileMesh(new GridMesh(rowGrid));
    }

    private void HandleTileMesh(ITileMesh mesh)
    {
      TileHandler tileHandler = new TileHandler(mesh);
      tileHandler.ProcessPoints();
    }

    private interface ITopoGeometry
    { }
    private interface ITopoPoint : ITopoGeometry
    {
      IList<TopoLine> IsoLines { get; }
    }
    private interface ITopoLine : ITopoGeometry
    {
    }
    private interface ICulmPoint : ITopoPoint
    { }
    private interface ICulmLine
    { }

    private class TopoPoint : ITopoPoint
    {
      private readonly IList<TopoLine> _lines;
      public TopoPoint(IList<TopoLine> lines)
      { _lines = lines; }

      public IList<TopoLine> IsoLines { get { return _lines; } }

      public static TopoPoint Create(LineDzNodes isoLineNodes)
      {
        List<TopoLine> lines = new List<TopoLine>(isoLineNodes.Count);
        foreach (var node in isoLineNodes)
        {
          TopoLine line;
          if (node.Value.DzSign != 0)
          { line = new TopoLine(node.Value.Line, false); }
          else
          {
            IMeshLine meshLine = node.Value.Line;
            if (meshLine.GetPreviousTriLine() == null || meshLine.GetNextTriLine() == null)
            { line = new TopoLine(meshLine, true); }
            else
            {
              int pre = node.Previous.Value.DzSign;
              int next = node.Next.Value.DzSign;
              if (pre == 0 || next == 0 || pre == next)
              { line = new TopoLine(meshLine, true); }
              else
              { line = new TopoLine(meshLine, false); }
            }
          }
          lines.Add(line);
        }

        return new TopoPoint(lines);
      }
    }

    private class TopoLine : ITopoLine
    {
      private readonly IMeshLine _line;
      private readonly bool _isFlat;
      public TopoLine(IMeshLine line, bool isFlat)
      {
        _line = line;
        _isFlat = isFlat;
      }

      public IMeshLine MeshLine { get { return _line; } }
      public bool IsFlat { get { return _isFlat; } }
    }

    [Obsolete]
    private class SaddlePoint : ITopoPoint
    {
      IList<TopoLine> ITopoPoint.IsoLines { get { return null; } }
    }
    private class CulmPoint : ICulmPoint
    {
      private readonly IMeshLine _start;

      public CulmPoint(IMeshLine start)
      { _start = start; }

      IList<TopoLine> ITopoPoint.IsoLines { get { return null; } }
    }

    [Obsolete]
    private class FlatLinePoint : ICulmPoint
    {
      IList<TopoLine> ITopoPoint.IsoLines { get { return null; } }
    }
    [Obsolete]
    private class FlatBorderPoint : ICulmPoint
    {
      IList<TopoLine> ITopoPoint.IsoLines { get { return null; } }
    }
    private class FlatArea : ITopoGeometry
    {
      private readonly IMeshLine _init;
      public FlatArea(IMeshLine init)
      {
        _init = init;
      }
      private List<ITopoPoint> _border;
      public IList<ITopoPoint> Border { get { return _border ?? (_border = GetBorder()); } }

      private List<ITopoPoint> GetBorder()
      {
        // TODO: init from _init;
        throw new NotImplementedException();
      }
    }

    private class TileHandler
    {
      private readonly ITileMesh _mesh;

      private readonly IBox _extent;

      private readonly Border _left;
      private readonly Border _bottom;
      private readonly Border _top;
      private readonly Border _right;

      private readonly double _diag;
      private readonly Point _center;

      private readonly Dictionary<IPoint, ITopoPoint> _topoPoints;

      public TileHandler(ITileMesh mesh)
      {
        _mesh = mesh;
        _extent = mesh.TileExtent;

        _left = new Border(_extent.Min.X);
        _bottom = new Border(_extent.Min.Y);
        _top = new Border(_extent.Max.X);
        _right = new Border(_extent.Max.Y);

        Point diagP = 0.5 * PointOperator.Sub(_extent.Max, _extent.Min);
        _diag = diagP.X + diagP.Y;
        _center = _extent.Min + diagP;

        _topoPoints = new Dictionary<IPoint, ITopoPoint>(new PointComparer());
      }

      private RingGrower _ringGrower_;
      private RingGrower _ringGrower => _ringGrower_ ?? (_ringGrower_ = new RingGrower(new Geometry.Comparer(new[] { 0, 1 })));

      public void ProcessPoints()
      {
        List<ITopoPoint> topoPoints = new List<ITopoPoint>();
        foreach (var start in _mesh.Points)
        {
          ITopoPoint topoPt;

          // ProcessPoint(start);

          topoPt = InitTopoPt(start);
          if (topoPt == null)
          { continue; }

          _topoPoints.Add(start.Start, topoPt);
        }

        foreach (var topo in topoPoints)
        {
          List<DirectedRow> dirRows = new List<DirectedRow>();
          foreach (var line in topo.IsoLines)
          {
            DirectedRow r;
            if (line.IsFlat)
            {
              Polyline l = Polyline.Create(new[] { line.MeshLine.Start, line.MeshLine.End });
              TopologicalLine tl = new TopologicalLine(l);
              r = new DirectedRow(tl, isBackward: false);
            }
            else
            {
              double h0 = line.MeshLine.Start.Z;
              double h1 = line.MeshLine.End.Z;
              double h2 = line.MeshLine.GetPreviousTriLine().Start.Z;
              double f = (h0 - h1) / (h2 - h1);
              Polyline l = MeshUtils.GetContour(line.MeshLine, f, _mesh.LineComparer);
              TopologicalLine tl = new TopologicalLine(l);
              r = new DirectedRow(tl, isBackward: false);
            }
            dirRows.Add(r);
          }

          DirectedRow pre = dirRows.Last();
          foreach (var line in dirRows)
          {
            _ringGrower.Add(pre.Reverse(), line);
            pre = line;
          }

        }
      }

      private void Process(ITopoPoint topoPt)
      {
        if (topoPt.IsoLines != null && topoPt.IsoLines.Count > 0)
        {
          List<DirectedRow> dirRows = new List<DirectedRow>(topoPt.IsoLines.Count);
          foreach (var line in topoPt.IsoLines)
          {
            DirectedRow r;
            if (line.IsFlat)
            {
              Polyline l = Polyline.Create(new[] { line.MeshLine.Start, line.MeshLine.End });
              TopologicalLine tl = new TopologicalLine(l);
              r = new DirectedRow(tl, isBackward: false);
            }
            else
            {
              double h0 = line.MeshLine.Start.Z;
              double h1 = line.MeshLine.End.Z;
              double h2 = line.MeshLine.GetPreviousTriLine().Start.Z;
              double f = (h0 - h1) / (h2 - h1);
              Polyline l = MeshUtils.GetContour(line.MeshLine, f, _mesh.LineComparer);
              TopologicalLine tl = new TopologicalLine(l);
              r = new DirectedRow(tl, isBackward: false);
            }
            dirRows.Add(r);
          }

          DirectedRow pre = dirRows.Last();
          foreach (var line in dirRows)
          {
            _ringGrower.Add(pre.Reverse(), line);
            pre = line;
          }
        }
      }

      private ITopoPoint InitTopoPt(IMeshLine start)
      {
        List<IMeshLine> lines = MeshUtils.GetPointLines(start, _mesh.LineComparer);
        LinkedList<LineDz> dzLines = new LinkedList<LineDz>();
        foreach (var line in lines)
        {
          dzLines.AddLast(new LineDz(line));
        }

        Dictionary<int, LineDzNodes> dhPos = new Dictionary<int, LineDzNodes>();
        LineDzNodes isoLineNodes = new LineDzNodes();
        LinkedListNode<LineDz> preNode = dzLines.Last;
        for (LinkedListNode<LineDz> lineNode = dzLines.First; lineNode != null; lineNode = lineNode.Next)
        {
          LineDz line = lineNode.Value;

          int dh = line.DzSign;

          if (dh != preNode.Value.DzSign && !isoLineNodes.Contains(preNode))
          {
            if (dh != 0)
            { isoLineNodes.Add(preNode); }
            else
            { isoLineNodes.Add(lineNode); }
          }
          preNode = lineNode;

          if (!dhPos.TryGetValue(dh, out LineDzNodes dhLines))
          {
            dhLines = new LineDzNodes();
            dhPos.Add(dh, dhLines);
          }
          dhLines.Add(lineNode);
        }

        if (dhPos.Count == 1)
        {
          var pair = dhPos.First();
          int sign = pair.Key;

          if (sign == LineDz.NullSign)
          { return null; }

          if (sign == 0)
          { return null; }

          CulmPoint culmPt = new CulmPoint(start);
          return culmPt;
        }

        if (!dhPos.ContainsKey(LineDz.NullSign))
        {
          if (dhPos.Count == 2 && dhPos.ContainsKey(0))
          {
            return TopoPoint.Create(isoLineNodes);
          }
          if (isoLineNodes.Count == 2)
          { return null; }

          return TopoPoint.Create(isoLineNodes);
        }

        if (dhPos.Count == 2)
        {
          if (!dhPos.ContainsKey(0))
          { return new CulmPoint(start); }
          return null;
        }

        if (dhPos.Count == 3 && dhPos.ContainsKey(0))
        {
          return TopoPoint.Create(isoLineNodes);
        }

        if (isoLineNodes.Count > 1)
        { return TopoPoint.Create(isoLineNodes); }

        Point d = _center - start.Start;
        double de = Math.Abs(d.X) + Math.Abs(d.Y);
        if (Math.Abs(_diag - de) < 1e-6)
        { return TopoPoint.Create(isoLineNodes); }

        return null;
      }

      private void ProcessPoint(IMeshLine start)
      {
        List<IMeshLine> lines = MeshUtils.GetPointLines(start, _mesh.LineComparer);
        LinkedList<LineDz> dzLines = new LinkedList<LineDz>();
        foreach (var line in lines)
        {
          dzLines.AddLast(new LineDz(line));
        }

        Dictionary<int, List<LinkedListNode<LineDz>>> dhPos =
          new Dictionary<int, List<LinkedListNode<LineDz>>>();
        List<LinkedListNode<LineDz>> changes = new List<LinkedListNode<LineDz>>();
        int dh0 = dzLines.Last.Value.DzSign;
        for (LinkedListNode<LineDz> lineNode = dzLines.First; lineNode != null; lineNode = lineNode.Next)
        {
          LineDz line = lineNode.Value;

          int dh = line.DzSign;
          if (dh != 0)
          {
            if (dh != dh0)
            { changes.Add(lineNode); }
            dh0 = dh;
          }
          if (!dhPos.TryGetValue(dh, out List<LinkedListNode<LineDz>> dhLines))
          {
            dhLines = new List<LinkedListNode<LineDz>>();
            dhPos.Add(dh, dhLines);
          }
          dhLines.Add(lineNode);
        }

        if (dhPos.ContainsKey(LineDz.NullSign))
        {
          AddBorderInfo(start, dhPos);
        }

        LinkedListNode<LineDz> flatBorderNode;
        if (dhPos.TryGetValue(0, out List<LinkedListNode<LineDz>> line0s) && dhPos.Count > 1)
        { flatBorderNode = GetFlatBorderNode(line0s); }
        else
        { flatBorderNode = null; }

        if (flatBorderNode != null)
        {
          List<IMeshLine> forewardLegs = new List<IMeshLine>();
          IMeshLine startFlatBorder = flatBorderNode.Value.Line;
          GetFlatBorder(startFlatBorder, _mesh, forewardLegs, clockWise: true);
          int nLegs = forewardLegs.Count;
          if (nLegs <= 1 ||
            _mesh.LineComparer.Compare(forewardLegs[0], forewardLegs[nLegs - 1]) != 0)
          {
            List<IMeshLine> reverseLegs = new List<IMeshLine>();
            GetFlatBorder(startFlatBorder.Invers(), _mesh, reverseLegs, clockWise: false);
          }
        }
        else if (changes.Count > 2)
        {
          foreach (var change in changes)
          {
            if (change.Value.DzSign == LineDz.NullSign)
            { continue; }
            LineDz pre = (change.Previous ?? change.List.Last).Value;
            if (pre.DzSign == LineDz.NullSign)
            { continue; }

            Polyline contour = GetContour(pre.Line, out bool complete);
          }
        }
      }

      private static Polyline GetContour(IMeshLine start, out bool complete)
      {
        double h = start.Start.Z;
        IMeshLine slope = start.GetNextTriLine().Invers();

        Point p0 = Point.CastOrCreate(slope.Start);
        IPoint p1 = slope.End;
        double h0 = p0.Z;
        double h1 = p1.Z;

        if (h0 == h1)
        {
          complete = true;
          return null;
        }

        double fraction = (h - h0) / (h1 - h0);
        if (fraction < 0 || fraction > 1)
        {
          complete = false;
          return null;
        }

        Polyline contour = new Polyline();
        contour.Add(start.Start);

        Point pFraction2d = p0 + fraction * (p1 - p0);
        Point pFraction3d = new Point3D(pFraction2d.X, pFraction2d.Y, h);
        contour.Add(pFraction3d);

        PointComparer comparer = new PointComparer();

        IMeshLine t = slope;
        while (comparer.Compare(contour.Points.First.Value, contour.Points.Last.Value) != 0)
        {
          IMeshLine next = t.GetNextTriLine();
          if (next == null)
          {
            complete = false;
            return contour;
          }

          double h2 = next.End.Z;
          double f1;
          if ((h2 >= h) == (h1 >= h))
          {
            f1 = (h - h0) / (h2 - h0);
            t = t.GetPreviousTriLine().Invers();
            h1 = h2;
            p1 = t.End;
          }
          else
          {
            f1 = (h - h2) / (h1 - h2);
            t = next.Invers();
            h0 = h2;
            p0 = Point.CastOrCreate(t.Start);
          }

          pFraction2d = p0 + f1 * (p1 - p0);
          pFraction3d = new Point3D(pFraction2d.X, pFraction2d.Y, h);
          if (comparer.Equals(contour.Points.Last.Value, pFraction3d))
          { continue; }

          contour.Add(pFraction3d);
        }
        complete = true;
        return contour;
      }

      private LinkedListNode<LineDz> GetFlatBorderNode(List<LinkedListNode<LineDz>> line0s)
      {
        foreach (var line0Node in line0s)
        {
          LinkedListNode<LineDz> pre = line0Node.Previous ?? line0Node.List.Last;
          LinkedListNode<LineDz> next = line0Node.Next ?? line0Node.List.First;
          if (pre.Value.DzSign == LineDz.NullSign || next.Value.DzSign == LineDz.NullSign)
          { continue; }
          if (next.Value.DzSign == 0)
          {
            LinkedListNode<LineDz> flatBorderNode = line0Node;
            LinkedListNode<LineDz> preNode;
            while ((preNode = flatBorderNode.Previous ?? flatBorderNode.List.Last).Value.DzSign == 0)
            { flatBorderNode = preNode; }

            if (preNode.Value.DzSign != LineDz.NullSign)
            { return flatBorderNode; }
          }
          if (pre.Value.DzSign == next.Value.DzSign)
          {
            return line0Node;
          }
        }
        return null;
      }

      private void AddBorderInfo(IMeshLine start, Dictionary<int, List<LinkedListNode<LineDz>>> dhPos)
      {
        IPoint s = start.Start;

        bool addToBorder = false;
        Point d = _center - s;
        double de = Math.Abs(d.X) + Math.Abs(d.Y);
        if (Math.Abs(_diag - de) < 1e-6)
        { addToBorder = true; }

        if (dhPos.Count <= 2 && !dhPos.ContainsKey(0))
        { addToBorder = true; }

        if (addToBorder)
        {
          if (s.X <= _extent.Min.X)
          { _left.Add(s); }
          if (s.Y <= _extent.Min.Y)
          { _bottom.Add(s); }
          if (s.X >= _extent.Max.X)
          { _right.Add(s); }
          if (s.Y >= _extent.Max.Y)
          { _top.Add(s); }
        }
      }

      private List<IMeshLine> GetFlatBorder(IMeshLine start, ITileMesh m, List<IMeshLine> saddleLines, bool clockWise)
      {
        List<IMeshLine> border = new List<IMeshLine> { start };
        IMeshLine x = clockWise ?
          start.Invers().GetPreviousTriLine().Invers() :
          start.GetNextTriLine();

        int dh0 = Math.Sign(x.End.Z - start.End.Z);
        int dh = dh0;
        while (true)
        {
          if (dh != 0)
          {
            saddleLines.Add(x);
            if (saddleLines.Count > 1 && m.LineComparer.Compare(x, saddleLines[0]) == 0)
            {
              return border;
            }

            x = GetNext(x, clockWise);
            if (x == null)
            { return border; }
            dh0 = dh;

            dh = Math.Sign(x.End.Z - start.End.Z);
          }
          else
          {
            IMeshLine y = GetNext(x, clockWise);
            if (y == null)
            { return border; }

            int dh1 = Math.Sign(y.End.Z - start.End.Z);
            if (dh1 == dh0 || dh1 == 0)
            {
              border.Add(x);
              y = clockWise ?
                x.Invers().GetPreviousTriLine().Invers() :
                start.GetNextTriLine();
              dh0 = Math.Sign(y.End.Z - start.End.Z);
              dh = dh0;
            }
            else
            {
              dh0 = dh1;
              dh = dh0;
            }
            x = y;
          }
        }
      }

      private IMeshLine GetNext(IMeshLine line, bool clockWise)
      {
        IMeshLine next;
        if (clockWise)
        {
          IMeshLine pre = line.GetPreviousTriLine();
          if (pre == null)
          { return null; }
          next = pre.Invers();
        }
        else
        { next = line.Invers().GetNextTriLine(); }
        return next;
      }
    }

    private class Border
    {
      private readonly double _limit;
      private readonly List<IPoint> _points;
      public Border(double limit)
      {
        _limit = limit;
        _points = new List<IPoint>();
      }

      public void Add(IPoint p)
      {
        _points.Add(p);
      }
    }

    private class LineDz
    {
      public const int NullSign = -2;

      public LineDz(IMeshLine line)
      {
        Line = line;
        Dz = (line != null) ? line.End.Z - line.Start.Z : double.NaN;
        DzSign = !double.IsNaN(Dz) ? Math.Sign(Dz) : NullSign;
      }
      public IMeshLine Line { get; private set; }
      public double Dz { get; private set; }
      public int DzSign { get; private set; }

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        if (!double.IsNaN(Dz))
        {
          char cAngle = MeshUtils.GetChar(Line, out double angle);

          sb.AppendFormat("{0} {1:N2} ,", cAngle, angle);
        }
        sb.AppendFormat("{0:N1}", Dz);
        return sb.ToString();
      }
    }
    private class Gaga
    {
      private readonly int _ix;
      private readonly int _iy;
      private readonly IGrid<double> _grid;
      private readonly IGrid<int> _idx;
      private readonly GridExtent _extent;
      private readonly SortedDictionary<double, GagaPoints> _dict = new SortedDictionary<double, GagaPoints>();

      public Gaga(IGrid<double> grid, int ix, int iy, IGrid<int> idx)
      {
        _ix = ix;
        _iy = iy;
        _grid = grid;
        _extent = grid.Extent;

        _idx = idx;
      }
      private bool T(double h0, int ix, int iy)
      {
        foreach (var dir in Dir.MeshDirs)
        {
          int tx = ix + dir.Dx;
          if (tx < 0 || tx >= _extent.Nx)
          { continue; }
          int ty = iy + dir.Dy;
          if (ty < 0 || ty >= _extent.Ny)
          { continue; }

          if (_idx[ix, iy] > 0)
          { continue; }

          double h = _grid[tx, ty];
          double dh = h - h0;

          if (dh > 0)
          { return false; }

          if (dh == 0)
          { continue; }

          GagaPoint p = new GagaPoint(_grid, tx, ty);

          if (_dict.TryGetValue(h, out GagaPoints equalHeights))
          {
            equalHeights = new GagaPoints();
            _dict.Add(h, equalHeights);
          }
          if (!equalHeights.ContainsKey(p))
          { equalHeights.Add(p, h); }
        }

        return true;
      }
    }
    private class GagaPoints : Dictionary<GagaPoint, double>
    {
      public GagaPoints()
        : base(new GagaPointEquatable())
      { }
    }
    private class GagaPointEquatable : IEqualityComparer<GagaPoint>
    {
      public bool Equals(GagaPoint x, GagaPoint y)
      {
        return x.Ix == y.Ix && x.Iy == y.Iy;
      }

      public int GetHashCode(GagaPoint obj)
      {
        return obj.Ix.GetHashCode() ^ (29 * obj.Iy.GetHashCode());
      }
    }
    private class GagaPoint
    {
      public int Ix { get; private set; }
      public int Iy { get; private set; }
      public GagaPoint(IGrid<double> grid, int ix, int iy)
      {
        Ix = ix;
        Iy = iy;
      }
    }

    private class LineDzNodes : List<LinkedListNode<LineDz>>
    {
      public override string ToString()
      {
        int n = Count;
        if (n <= 0)
        { return base.ToString(); }

        LinkedListNode<LineDz> pre = this[n - 1];
        StringBuilder s = new StringBuilder();
        foreach (var node in this)
        {
          if (pre == node.Previous)
          { s.Append(";"); }
          else
          { s.Append("|"); }
          s.AppendFormat("{0}", node.Value.ToString());

          pre = node;
        }
        return s.ToString();
      }
    }

    private class GagaComparer : IComparer<GagaPoint>
    {
      public int Compare(GagaPoint x, GagaPoint y)
      {
        int d = x.Ix.CompareTo(y.Iy);
        if (d != 0)
        { return d; }

        d = x.Iy.CompareTo(y.Iy);

        return d;
      }
    }
  }
}
