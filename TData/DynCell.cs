using System;
using System.Diagnostics;

namespace TData
{
	/// <summary>
	/// Summary description for DynCell.
	/// </summary>
  public class DynCell
  {
    private double _valX0,_valX1;
    private double _valY0,_valY1;
    private double _valCenter;
    private bool _bX0,_bX1;
    private bool _bY0,_bY1;
    private bool _bCenter;

    private DynCell[,] _child = new DynCell[2,2];
    private DynCell _parent;
    private DynGrid _grid;

    internal DynCell(DynGrid grid)
    {
      _grid = grid;
    }
    internal DynCell(DynCell parent)
    {
      _parent = parent;
      _grid = parent._grid;
    }
    public DynCell[,] Cell
    {
      get
      { return _child; }
    }
    public double ValueX0
    {
      get
      { return _valX0; }
      set
      {
        _valX0 = value;
        _bX0 = true;
      }
    }
    public double ValueX1
    {
      get
      { return _valX1; }
      set
      {
        _valX1 = value;
        _bX1 = true;
      }
    }
    public double ValueY0
    {
      get
      { return _valY0; }
      set
      {
        _valY0 = value;
        _bY0 = true;
      }
    }
    public double ValueY1
    {
      get
      { return _valY1; }
      set
      {
        _valY1 = value;
        _bY1 = true;
      }
    }
    public double ValueCenter
    {
      get
      { return _valCenter; }
      set
      {
        _valCenter = value;
        _bCenter = true;
      }
    }
    public bool IsX0Set
    {
      get
      { return _bX0; }
      set
      { _bX0 = value; }
    }
    public bool IsX1Set
    {
      get
      { return _bX1; }
      set
      { _bX1 = value; }
    }
    public bool IsY0Set
    {
      get
      { return _bY0; }
      set
      { _bY0 = value; }
    }
    public bool IsY1Set
    {
      get
      { return _bY1; }
      set
      { _bY1 = value; }
    }
    public bool IsCenterSet
    {
      get
      { return _bCenter; }
      set
      { _bCenter = value; }
    }

    public double GetValue(ref double x,ref double y,double[,] val,out bool isSet)
    {
      int ix,iy;
      if (x < 0.5)
      { ix = 0; }
      else
      { ix = 1; }
      if (y < 0.5)
      { iy = 0; }
      else
      { iy = 1; }
      // Aufloesung von x,y verdoppeln : (x - ix * 0.5) * 2, ...
      x += x - ix;
      y += y - iy;
      if (_child[ix,iy] != null)
      { 
        int bx = (x >= 0.5) ? 1 : 0;
        int by = (y >= 0.5) ? 1 : 0;
        double res = _child[ix,iy].GetValue(ref x,ref y,val,out isSet);
        if (isSet)
        { return res; }
        int cx = ix + bx;
        int cy = iy + by;
        if (ix == 1 || iy == 1)
        { 
          val[bx,by] = Value(cx,cy);
          isSet = true;
        }
        if (isSet)
        {
          return DynGrid.Value(x,y,val);
        }
        else
        { return 0; }
      }
      else
      {
        val[0,0] = Value(ix,iy);
        val[0,1] = Value(ix,iy + 1);
        val[1,0] = Value(ix + 1,iy);
        val[1,1] = Value(ix + 1,iy + 1);
        isSet = false;
        return 0;
      }
    }
    private double Value(int ix,int iy)
    {
      if (ix == 0)
      {
        if (iy == 1)
        { return _valX0; }
        else
        { return 0; }
      }
      else if (ix == 1)
      {
        if (iy == 0)
        { return _valY0; }
        else if (iy == 1)
        { return _valCenter; }
        else 
        { return _valY1 ; }
      }
      else
      {
        if (iy == 1)
        { return _valX1; }
        else
        { return 0; }
      }
    }
  }
}
