using Basics.Geom;
using Ocad;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.Linq;
using P = Basics.Geom.Point2D;

namespace OCourse.Commands
{
  public class CmdCoursePlaceControlNrs
  {
    private readonly OcadWriter _w;
    private readonly CourseMap _cm;

    private Dictionary<string, IPoint> _controlsDict;
    private Dictionary<string, IPoint> ControlsDict => _controlsDict ?? (_controlsDict = GetControlsDict());

    public CmdCoursePlaceControlNrs(OcadWriter w, CourseMap cm)
    {
      _w = w;
      _cm = cm;
    }

    public void Execute()
    {
      _w.ProjectElements((elem) => PlaceElem(elem));
    }

    private GeoElement PlaceElem(GeoElement elem)
    {
      if (!_cm.ControlNrSymbols.Contains(elem.Symbol))
      { return null; }
      if (string.IsNullOrEmpty(elem.ObjectString))
      { return null; }

      ControlPar p = new ControlPar(elem.ObjectString);
      string controlNr = p.GetString('d');
      if (!ControlsDict.TryGetValue(controlNr, out IPoint place))
      { return null; }

      foreach (var elemP in elem.Geometry.EnumPoints())
      {
        Basics.Geom.Projection.Translate prj = new Basics.Geom.Projection.Translate(PointOp.Sub(place, elemP));
        elem.Geometry = elem.Geometry.Project(prj);
        return elem;
      }
      return null;
    }

    private Dictionary<string, IPoint> GetControlsDict()
    {
      Dictionary<string, double> nrWidthDict = new Dictionary<string, double>();
      foreach (var nrElem in _cm.ControlNrElems)
      {
        if (string.IsNullOrEmpty(nrElem.ObjectString))
        { continue; }

        ControlPar p = new ControlPar(nrElem.ObjectString);
        string controlNr = p.GetString('d');

        IBox b = nrElem.Geometry.Extent;
        nrWidthDict[controlNr] = b.Max.X - b.Min.X;
      }

      List<ControlRegion> allRegions = new List<ControlRegion>();
      foreach (var control in _cm.AllControls)
      {
        if (string.IsNullOrEmpty(control.ObjectString))
        { continue; }

        ControlPar par = new ControlPar(control.ObjectString);
        if (!nrWidthDict.TryGetValue(par.Id, out double w))
        { continue; }

        if (!(control.Geometry.GetGeometry() is IPoint cp))
        { throw new InvalidOperationException($"invalid geometry for control {par.Id}"); }

        ControlRegion cr = ControlRegion.Create(par.Id, cp, w, _cm);
        allRegions.Add(cr);
      }

      //foreach (var r in allRegions)
      //{
      //  foreach (var seg in r.NrPositions.EnumSegments())
      //  {
      //    if (seg is Line line)
      //    {
      //      _w.Append(new GeoElement(Polyline.Create(new[] { line.Start, line.End })) { Symbol = 705000 });
      //    }
      //  }
      //}

      Dictionary<string, IPoint> dict = new Dictionary<string, IPoint>();
      BoxTree<ControlRegion> regionTree = BoxTree.Create(allRegions, (x) => x.NrBox, nElem: 4);
      List<ControlRegion> unhandledRegions = new List<ControlRegion>(allRegions);
      while (unhandledRegions.Count > 0)
      {
        unhandledRegions.Sort((x, y) => -x.MaxFreeLength.CompareTo(y.MaxFreeLength));

        int i = unhandledRegions.Count - 1;
        ControlRegion r = unhandledRegions[i];

        dict.Add(r.ControlId, r.Position);

        unhandledRegions.RemoveAt(i);
      }

      return dict;
    }

    private class ControlRegion
    {
      public static ControlRegion Create(string controlId, IPoint cp, double textWidth, CourseMap courseMap, double distanceFactor = 1.2)
      {
        CourseMap cm = courseMap;

        double r = cm.ControlDistance * distanceFactor;
        double h = cm.ControlNrHeight;
        double w = textWidth;

        Polyline nrPos = GetCirclePositions(cp, r, w, h);

        double minDist = 2 * r;
        double q = minDist + r;
        List<Area> usedAreas = new List<Area>();
        Box controlSearchBox = new Box(
          new P(cp.X - w - q, cp.Y - h - q), new P(cp.X + w + q, cp.Y + h + q));
        foreach (var entry in courseMap.ControlsTree.Search(controlSearchBox))
        {
          if (PointOp.EqualGeometry(cp, entry.Value.P))
          { continue; }

          usedAreas.Add(new Area(GetCirclePositions(entry.Value.P, minDist, w, h)));
        }

        Box connectionSearchBox = new Box(
          new P(cp.X - w - r, cp.Y - h - r), new P(cp.X + w + r, cp.Y + h + r));
        foreach (var entry in courseMap.ConnectionTree.Search(connectionSearchBox))
        {
          usedAreas.Add(GetConnectionArea(entry.Value.L, w, h));
        }

        return new ControlRegion(controlId, cp, nrPos, usedAreas);
      }

      private readonly double _textWidth;
      private readonly CourseMap _courseMap;

      private readonly Polyline _nrPos;
      private readonly List<Area> _usedAreas;

      private readonly Dictionary<Area, List<UsedPart>> _areaSplits;
      private double _maxFreeLength;
      private List<Polyline> _freeParts;
      private IPoint _p;
      private IPoint _position;

      public IReadOnlyList<Area> UsedAreas => _usedAreas;
      private List<Polyline> FreePartsCore => _freeParts ?? (_freeParts = GetPossibleParts());
      public IReadOnlyList<Polyline> FreeParts => FreePartsCore;
      public IPoint Position => _position ?? _p;
      public Polyline NrPositions => _nrPos;

      public string ControlId { get; }
      public IBox NrBox;
      private ControlRegion(string controlId, IPoint p, Polyline nrPos, List<Area> usedAreas)
      {
        ControlId = controlId;
        _p = p;
        _nrPos = nrPos;
        _usedAreas = usedAreas;

        NrBox = nrPos.Extent;
        _areaSplits = new Dictionary<Area, List<UsedPart>>();
        _maxFreeLength = -1;
      }

      public double MaxFreeLength => _maxFreeLength < 0 ? (_maxFreeLength = GetMaxFreeLength()) : _maxFreeLength;

      private double GetMaxFreeLength()
      {
        Dictionary<Polyline, double> lengthsDict = FreeParts.ToDictionary(x => x, y => y.Length());
        FreePartsCore.Sort((x, y) => -lengthsDict[x].CompareTo(lengthsDict[y]));
        _position = null;
        if (FreePartsCore.Count > 0)
        {
          Polyline maxLine = FreePartsCore[0];
          double l = lengthsDict[maxLine];
          _position = maxLine.SubPart(l / 2, l / 2).GetPoint(0);
          return lengthsDict[maxLine];
        }
        return 0;
      }

      private List<Polyline> GetPossibleParts()
      {
        List<UsedPart> allUsedParts = GetAllUsedParts();
        List<ParamGeometryRelation> mergedSplits = GetMergedSplits(allUsedParts, out bool startFree);
        List<Polyline> parts = _nrPos.Split(mergedSplits);

        int nParts = parts.Count;
        int iStart;
        List<Polyline> freeParts = new List<Polyline>();
        if (startFree)
        {
          Polyline joined = new Polyline();
          foreach (var seg in parts[nParts - 1].EnumSegments())
          { joined.Add(seg); }
          foreach (var seg in parts[0].EnumSegments())
          { joined.Add(seg); }

          freeParts.Add(joined);

          iStart = 2;
        }
        else { iStart = 1; }

        for (int iPart = iStart; iPart < nParts - 1; iPart += 2)
        {
          freeParts.Add(parts[iPart]);
        }
        return freeParts;
      }

      private List<ParamGeometryRelation> GetMergedSplits(List<UsedPart> usedParts, out bool startFree)
      {
        startFree = true;
        List<ParamGeometryRelation> mergedSplits = new List<ParamGeometryRelation>();
        ParamGeometryRelation.SortComparer cmp = new ParamGeometryRelation.SortComparer(_nrPos, nullFirst: true);
        usedParts.Sort((x, y) => cmp.Compare(x.Start, y.Start));
        ParamGeometryRelation max = null;
        bool endFree = true;
        foreach (var usedPart in usedParts)
        {
          if (usedPart.Start == null)
          {
            startFree = false;
          }
          else if (max == null)
          {
            mergedSplits.Add(usedPart.Start);
          }
          else if (cmp.Compare(max, usedPart.Start) < 0)
          {
            mergedSplits.Add(max);
            mergedSplits.Add(usedPart.Start);
          }
          if (max == null)
          {
            max = usedPart.End;
          }
          else if (usedPart.End == null)
          {
            endFree = false;
            break;
          }
          else if (cmp.Compare(max, usedPart.End) < 0)
          {
            max = usedPart.End;
          }
        }
        if (endFree)
        {
          mergedSplits.Add(max);
        }
        if (mergedSplits.Count % 2 != 0)
        { }
        return mergedSplits;
      }

      private List<UsedPart> GetAllUsedParts()
      {
        List<UsedPart> allSplits = new List<UsedPart>();
        foreach (var usedArea in _usedAreas)
        {
          if (!_areaSplits.TryGetValue(usedArea, out List<UsedPart> areaSplits))
          {
            List<ParamGeometryRelation> rels = new List<ParamGeometryRelation>(GeometryOperator.CreateRelations(usedArea, _nrPos));
            areaSplits = new List<UsedPart>();

            foreach (var rel in rels)
            {
              areaSplits.Add(new UsedPart { Start = rel.BorderRelations[0], End = rel.BorderRelations[1] });
            }
            _areaSplits.Add(usedArea, areaSplits);
          }
          allSplits.AddRange(areaSplits);
        }
        return allSplits;
      }

      private class UsedPart
      {
        public ParamGeometryRelation Start { get; set; }
        public ParamGeometryRelation End { get; set; }

        public override string ToString()
        {
          return $"S:{GetString(Start)} - E:{GetString(End)}";
        }
        private string GetString(ParamGeometryRelation r)
        {
          if (r == null)
          { return "<null>"; }

          return r.GetInfoString((x) => x is Polyline);
        }
      }

      private static Polyline GetCirclePositions(IPoint cp, double r, double w, double h)
      {
        Polyline pos = new Polyline();
        pos.Add(new Line(new P(cp.X, cp.Y + r), new P(cp.X - w, cp.Y + r)));
        pos.Add(new Arc(new P(cp.X - w, cp.Y), r, Math.PI / 2, Math.PI / 2));
        pos.Add(new Line(new P(cp.X - w - r, cp.Y), new P(cp.X - w - r, cp.Y - h)));
        pos.Add(new Arc(new P(cp.X - w, cp.Y - h), r, Math.PI, Math.PI / 2));
        pos.Add(new Line(new P(cp.X - w, cp.Y - h - r), new P(cp.X, cp.Y - h - r)));
        pos.Add(new Arc(new P(cp.X, cp.Y - h), r, 3 * Math.PI / 2, Math.PI / 2));
        pos.Add(new Line(new P(cp.X + r, cp.Y - h), new P(cp.X + r, cp.Y)));
        pos.Add(new Arc(new P(cp.X, cp.Y), r, 0, Math.PI / 2));

        return pos;
      }

      private static Area GetConnectionArea(Polyline connection, double w, double h)
      {
        List<IPoint> pts = new List<IPoint> { connection.GetPoint(0), connection.GetPoint(-1) };
        pts.Sort((x, y) => x.X.CompareTo(y.X));
        double x0 = pts[0].X;
        double y0 = pts[0].Y;
        double x1 = pts[1].X;
        double y1 = pts[1].Y;
        IEnumerable<IPoint> ps = (y0 > y1)
          ? new Point[] { new P(x0 - w, y0), new P(x0 - w, y0 - h), new P(x1 - w, y1 - h), new P(x1, y1 - h), new P(x1, y1), new P(x0, y0), new P(x0 - w, y0) }
          : new Point[] { new P(x0 - w, y0), new P(x0 - w, y0 - h), new P(x0, y0 - h), new P(x1, y1 - h), new P(x1, y1), new P(x1 - w, y1), new P(x0 - w, y0) };
        Polyline border = Polyline.Create(ps);

        Area area = new Area(border);
        return area;
      }
    }
  }
}
