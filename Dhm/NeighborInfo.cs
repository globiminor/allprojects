using System;

namespace Dhm
{
  internal class NeighborInfo
  {
    private double _length;

    private double _weightLeft;
    private double _weightRight;

    private int _height;
    private int? _min;
    private int? _max;

    public double Length
    {
      get { return _length; }
      set { _length = value; }
    }
    public double Weight
    {
      get { return _weightLeft + _weightRight; }
    }
    public bool LeftSide
    {
      get { return Math.Abs(_weightLeft) > Math.Abs(_weightRight); }
    }
    public void AddWeight(double weight, bool leftSide)
    {
      if (leftSide)
      { _weightLeft += weight; }
      else
      { _weightRight += weight; }
    }

    public int Height
    { get { return _height; } }
    public int? MaxHeight
    { get { return _max; } }
    public int? MinHeight
    { get { return _min; } }

    public void CalcHeight(Contour contour, Contour neighbor,
      NeighborInfo neighbor2contour)
    {
      int dh; // direction of height change from neighbor to contour
      if (LeftSide == (contour.Orientation == Orientation.LeftSideDown))
      {
        if (neighbor2contour.LeftSide == (neighbor.Orientation == Orientation.LeftSideDown))
        { dh = 0; }
        else
        { dh = 1; }
      }
      else
      {
        if (neighbor2contour.LeftSide == (neighbor.Orientation == Orientation.LeftSideDown))
        { dh = -1; }
        else
        { dh = 0; }
      }

      _min = null;
      _max = null;
      _height = NextHeight(neighbor.HeightIndex.Value,
        dh, neighbor2contour.LeftSide == (neighbor.Orientation == Orientation.LeftSideDown),
        neighbor.Type, contour.Type);
      if (dh == 1)
      { _min = _height; }
      else if (dh == -1)
      { _max = _height; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="h0"></param>
    /// <param name="dh"></param>
    /// <param name="down">only used if dh == 0</param>
    /// <param name="type0"></param>
    /// <param name="type1"></param>
    /// <returns></returns>
    public static int NextHeight(int h0, int dh, bool down, ContourType type0, ContourType type1)
    {
      int h1 = (h0 - type1.Offset) / type1.Intervall * type1.Intervall + type1.Offset;

      if (dh < 0 && h1 >= h0)
      { h1 = h1 - type1.Intervall; }
      else if (dh > 0 && h1 <= h0)
      { h1 = h1 + type1.Intervall; }
      else if (dh == 0 && h1 != h0)
      {
        if (type0 == type1) throw new InvalidProgramException("Error in software design assumption");

        int j = type0.CompareTo(type1);
        if (j > 0 && h1 > h0) // TODO: verify j > or < 0
        {
          h1 = h1 - type1.Intervall;
        }
        else if (j < 0 && h1 < h0 && down == false) // TODO: verify j > or < 0
        {
          //if (down == false)
          { h1 = h1 + type1.Intervall; }
          //else
          //{ }
        }
      }
      return h1;

      //if (type0 == Hk.FormLine && type1 != Hk.FormLine && dh == 0)
      //{ down = !down; }

      //int h1 = NextHeight(h0, type1, down);

      //if (dh == 0)
      //{
      //  //Debug.Assert(Math.Abs(h1 - h0) <= 1, "TODO: Replace with Event");
      //}
      //if (dh == 0 || Math.Sign(h1 - h0) == dh)
      //{ return h1; }
      //else
      //{ return NextHeight(h0 + dh, type1, dh < 0); }
    }

    //private int NextHeight(int h, ContourType type, bool down)
    //{
    //  int h0;

    //  h0 = h / type.AequiDistanz * type.AequiDistanz + type.Offset;
    //  if (type == Hk.ContourLine)
    //  {
    //    h0 = h / 2 * 2;
    //    if (h0 != h && down == false)
    //    {
    //      h0 += 2;
    //    }
    //  }
    //  else if (type == Hk.CountLine)
    //  {
    //    int d = 2 * Hk.CountInterval;
    //    h0 = h / d * d;
    //    if (h0 != h && down == false)
    //    {
    //      h0 += d;
    //    }
    //  }
    //  else if (type == Hk.FormLine)
    //  {
    //    h0 = h / 2 * 2 + 1;
    //    if (h0 != h && down)
    //    {
    //      h0 -= 2;
    //    }
    //  }
    //  else
    //  { throw new ArgumentException("Unhandled Symbol " + type); }
    //  return h0;
    //}
  }
}
