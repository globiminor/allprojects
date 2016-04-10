using System;
using System.Diagnostics;

namespace TData
{
	/// <summary>
	/// Summary description for DynCell.
	/// </summary>
  public class DynCell
  {
    private double mValX0,mValX1;
    private double mValY0,mValY1;
    private double mValCenter;
    private bool mbX0,mbX1;
    private bool mbY0,mbY1;
    private bool mbCenter;

    private DynCell[,] mChild = new DynCell[2,2];
    private DynCell mParent;
    private DynGrid mGrid;

    internal DynCell(DynGrid grid)
    {
      mGrid = grid;
    }
    internal DynCell(DynCell parent)
    {
      mParent = parent;
      mGrid = parent.mGrid;
    }
    public DynCell[,] Cell
    {
      get
      { return mChild; }
    }
    public double ValueX0
    {
      get
      { return mValX0; }
      set
      {
        mValX0 = value;
        mbX0 = true;
      }
    }
    public double ValueX1
    {
      get
      { return mValX1; }
      set
      {
        mValX1 = value;
        mbX1 = true;
      }
    }
    public double ValueY0
    {
      get
      { return mValY0; }
      set
      {
        mValY0 = value;
        mbY0 = true;
      }
    }
    public double ValueY1
    {
      get
      { return mValY1; }
      set
      {
        mValY1 = value;
        mbY1 = true;
      }
    }
    public double ValueCenter
    {
      get
      { return mValCenter; }
      set
      {
        mValCenter = value;
        mbCenter = true;
      }
    }
    public bool IsX0Set
    {
      get
      { return mbX0; }
      set
      { mbX0 = value; }
    }
    public bool IsX1Set
    {
      get
      { return mbX1; }
      set
      { mbX1 = value; }
    }
    public bool IsY0Set
    {
      get
      { return mbY0; }
      set
      { mbY0 = value; }
    }
    public bool IsY1Set
    {
      get
      { return mbY1; }
      set
      { mbY1 = value; }
    }
    public bool IsCenterSet
    {
      get
      { return mbCenter; }
      set
      { mbCenter = value; }
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
      if (mChild[ix,iy] != null)
      { 
        int bx = (x >= 0.5) ? 1 : 0;
        int by = (y >= 0.5) ? 1 : 0;
        double res = mChild[ix,iy].GetValue(ref x,ref y,val,out isSet);
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
        { return mValX0; }
        else
        { return 0; }
      }
      else if (ix == 1)
      {
        if (iy == 0)
        { return mValY0; }
        else if (iy == 1)
        { return mValCenter; }
        else 
        { return mValY1 ; }
      }
      else
      {
        if (iy == 1)
        { return mValX1; }
        else
        { return 0; }
      }
    }
  }
}
