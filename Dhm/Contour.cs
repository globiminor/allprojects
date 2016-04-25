using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Dhm
{
  public class ContourType : IComparable<ContourType>
  {
    private int _intervall;
    private int _offset;

    public ContourType(int intervall, int offset)
    {
      _intervall = intervall;
      _offset = offset;
    }

    public int Intervall
    { get { return _intervall; } }
    public int Offset
    { get { return _offset; } }

    #region IComparable<ContourType> Members

    public int CompareTo(ContourType other)
    {
      int i;
      i = other._intervall.CompareTo(_intervall);
      if (i != 0)
      { return i; }

      i = _offset.CompareTo(other._offset);

      return i;
    }

    #endregion
  }

  public enum Orientation { Unknown, LeftSideDown, RightSideDown }

  public class Contour
  {
    public enum LoopState { Unknown, Checking, Loop, NoLoop }

    private class Comparer : IComparer<Contour>
    {
      #region IComparer<Contour> Members

      public int Compare(Contour x, Contour y)
      {
        if (x == y)
        { return 0; }

        int i;

        i = x.Type.CompareTo(y.Type);
        if (i != 0) { return i; }

        Polyline lx = x.Polyline;
        Polyline ly = y.Polyline;

        i = lx.Points.Count - ly.Points.Count;
        if (i != 0) { return i; }

        i = lx.Points.First.Value.X.CompareTo(ly.Points.First.Value.X);
        if (i != 0) { return i; }

        i = lx.Points.First.Value.Y.CompareTo(ly.Points.First.Value.Y);
        if (i != 0) { return i; }

        i = lx.Points.First.Next.Value.X.CompareTo(ly.Points.First.Next.Value.X);
        if (i != 0) { return i; }

        i = lx.Points.First.Next.Value.Y.CompareTo(ly.Points.First.Next.Value.Y);
        if (i == 0)
        { throw new InvalidProgramException("Error in software design assumption"); }
        return i;
      }

      #endregion
    }

    private int _id;
    private Polyline _line;
    private ContourType _type;

    private int? _heightIdx;
    private int? _maxHeightIdx;
    private int? _minHeightIdx;

    private Orientation _orientation = Orientation.Unknown;
    private LoopState _loopChecked;

    private static Comparer _comparer = new Comparer();

    private SortedDictionary<Contour, NeighborInfo> neighbors;

    public Contour(int id, Polyline polyline, ContourType type)
    {
      _id = id;
      _line = polyline;
      _type = type;

      neighbors = new SortedDictionary<Contour, NeighborInfo>(_comparer);
    }

    internal SortedDictionary<Contour, NeighborInfo> Neighbors
    {
      get { return neighbors; }
    }
    public Polyline Polyline
    { get { return _line; } }

    public int Id
    {
      get { return _id; }
    }
    public ContourType Type
    { get { return _type; } }

    public int? HeightIndex
    {
      get { return _heightIdx; }
      set { _heightIdx = value; }
    }
    public int? MaxHeightIndex
    {
      get { return _maxHeightIdx; }
      set { _maxHeightIdx = value; }
    }
    public int? MinHeightIndex
    {
      get { return _minHeightIdx; }
      set { _minHeightIdx = value; }
    }

    public Orientation Orientation
    {
      get { return _orientation; }
      set { _orientation = value; }
    }

    public LoopState LoopChecked
    {
      get { return _loopChecked; }
      set { _loopChecked = value; }
    }
  }
}
