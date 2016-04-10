using System;

namespace TData
{
	/// <summary>
	/// Summary description for DynGrid.
	/// </summary>
	public class DynGrid
	{
    private double mX0,mX1;
    private double mY0,mY1;
    private double[,] mVal = new double[2,2];
    private bool[,] mSet = new bool[2,2];
    private DynCell mCell;
		public DynGrid(double x0,double y0,double x1,double y1)
		{
      mX0 = x0;
      mY0 = y0;
      mX1 = x1;
      mY1 = y1;
		}

    public DynCell Split()
    {
      mCell = new DynCell(this);
      return mCell;
    }
    public void Append(bool xPos,bool yPos,
      double x,double y,double xy)
    {
      DynCell newCell = new DynCell(this);
      if (xPos)
      { mX1 = mX1 + (mX1 - mX0); }
      else
      { mX0 = mX0 - (mX1 - mX0); }

      if (yPos)
      { mY1 = mY1 + (mY1 - mY0); }
      else
      { mY0 = mY0 - (mY1 - mY0); }

      if (xPos)
      {
        if (yPos)
        {
          if (mSet[0,1])
          { newCell.ValueX0 = mVal[0,1]; }
          if (mSet[1,0])
          { newCell.ValueY0 = mVal[1,0]; }
          if (mSet[1,1])
          { newCell.ValueCenter = mVal[1,1]; }
          mSet[0,1] = false;
          mSet[1,0] = false;
          mSet[1,1] = false;
          newCell.Cell[0,0] = mCell;
        }
        else
        {
          if (mSet[0,0])
          { newCell.ValueX0 = mVal[0,0]; }
          if (mSet[1,1])
          { newCell.ValueY1 = mVal[1,1]; }
          if (mSet[1,0])
          { newCell.ValueCenter = mVal[1,0]; }
          mSet[0,0] = false;
          mSet[1,1] = false;
          mSet[1,0] = false;
          newCell.Cell[0,1] = mCell;
        }
      }
      else
      {
        if (yPos)
        {
          if (mSet[1,1])
          { newCell.ValueX1 = mVal[1,1]; }
          if (mSet[0,0])
          { newCell.ValueY0 = mVal[0,0]; }
          if (mSet[0,1])
          { newCell.ValueCenter = mVal[0,1]; }
          mSet[1,1] = false;
          mSet[0,0] = false;
          mSet[0,1] = false;
          newCell.Cell[1,0] = mCell;
        }
        else
        {
          if (mSet[1,0])
          { newCell.ValueX1 = mVal[1,0]; }
          if (mSet[0,1])
          { newCell.ValueY1 = mVal[0,1]; }
          if (mSet[0,0])
          { newCell.ValueCenter = mVal[0,0]; }
          mSet[1,0] = false;
          mSet[0,1] = false;
          mSet[0,0] = false;
          newCell.Cell[1,1] = mCell;
        }
      }
      mCell = newCell;
    }
    public double GetValue(int ix,int iy)
    {
      return mVal[ix,iy];
    }
    public void SetValue(int ix,int iy,double value)
    {
      mVal[ix,iy] = value;
      mSet[ix,iy] = true;
    }
    public void SetValueNull(int ix,int iy)
    {
      mSet[ix,iy] = false;
    }
    public double GetValue(double x,double y)
    {
      double dx = (x - mX0) / (mX1 - mX0);
      double dy = (y - mY0) / (mY1 - mY0);
      if (0 > dx || dx > 1 || 0 > dy || dy > 1)
      { throw new System.Exception("Out of bounds"); }
      if (mCell != null)
      {
        bool bIsSet;
        int bx = (x >= 0.5) ? 1 : 0;
        int by = (y >= 0.5) ? 1 : 0;
        double[,] val = new double[2,2];
        double res = mCell.GetValue(ref dx,ref dy,val,out bIsSet);
        if (bIsSet == false)
        {
          val[bx,by] = mVal[bx,by];
          return Value(dx,dy,val);
        }
        else
        { return res; }
      }
      else
      { return Value(dx,dy,mVal); }
    }
    public static double Value(double x,double y,double[,] val)
    {
      double x0 = val[0,0] + y * (val[0,1] - val[0,0]);
      double x1 = val[1,0] + y * (val[1,1] - val[1,0]);
      return x0 + x * (x1 - x0);
    }
  }
}
