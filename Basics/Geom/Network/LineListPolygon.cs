using System;
using System.Collections.Generic;

namespace Basics.Geom.Network
{
  public class LineListPolygon
  {
    private LineList _mainRing;
    private List<LineList> _innerRingList = new List<LineList>();
    private List<IPoint> _centroids = new List<IPoint>();

    private bool _isInnerRing;
    private bool _canProcess;
    private bool _processed;
    public LineListPolygon(LineList outerRing)
    {
      _mainRing = outerRing;
      _canProcess = true;
      _processed = true;
      foreach (DirectedRow row in outerRing.DirectedRows)
      {
        row.RightPoly = this;
      }
    }
    public LineList OuterRing
    {
      get
      {
        if (_isInnerRing)
        { throw new ArgumentException("cannot query outer Ring of an innerRing polygon"); }
        return _mainRing;
      }
    }
    public List<LineList> InnerRingList
    {
      get
      {
        if (_isInnerRing)
        { throw new ArgumentException("cannot query inner rings of an innerRing polygon"); }
        return _innerRingList;
      }
    }
    public IList<IPoint> Centroids
    {
      get { return _centroids; }
    }

    public Area GetPolygon()
    {
      Area polygon = new Area();
      Polyline ring = new Polyline();

      foreach (DirectedRow pRow in _mainRing.DirectedRows)
      {
        foreach (Curve c in pRow.Line().Segments)
        { ring.Add(c); }
      }
      polygon.Border.Add(ring);

      foreach (LineList lc in _innerRingList)
      {
        polygon.Border.Add(lc.GetPolygon().Border[0]);
      }
      return polygon;
    }

    public LineListPolygon(LineList ring, bool isInnerRing)
    {
      _isInnerRing = isInnerRing;
      if (_isInnerRing == false)
      {
        _canProcess = true;
        _processed = false;
      }
      _mainRing = ring;
      foreach (DirectedRow row in ring.DirectedRows)
      {
        row.RightPoly = this;
      }
    }
    public bool IsInnerRing
    {
      get { return _isInnerRing; }
    }
    public bool Processed
    {
      get { return _processed; }
      set { _processed = value; }
    }
    public bool CanProcess
    {
      get { return _canProcess; }
      set { _canProcess = value; }
    }

    public void Add(LineListPolygon innerRing)
    {
      if (_isInnerRing)
      { throw new ArgumentException("Cannot add rings to an inner ring"); }
      if (innerRing._isInnerRing == false)
      { throw new ArgumentException("Cannot add outer ring to a polygon"); }

      if (_innerRingList == null)
      { _innerRingList = new List<LineList>(); }

      _innerRingList.Add(innerRing._mainRing);
      foreach (DirectedRow row in innerRing._mainRing.DirectedRows)
      {
        row.RightPoly = this;
      }

    }
  }
}
