using System;

namespace TData
{
	/// <summary>
	/// Summary description for DynGrid.
	/// </summary>
	public class DynGrid
	{
    private double _x0,_x1;
    private double _y0,_y1;
    private double[,] _val = new double[2,2];
    private bool[,] _set = new bool[2,2];
    private DynCell _cell;
		public DynGrid(double x0,double y0,double x1,double y1)
		{
      _x0 = x0;
      _y0 = y0;
      _x1 = x1;
      _y1 = y1;
		}

    public DynCell Split()
    {
      _cell = new DynCell(this);
      return _cell;
    }
    public void Append(bool xPos,bool yPos,
      double x,double y,double xy)
    {
      DynCell newCell = new DynCell(this);
      if (xPos)
      { _x1 = _x1 + (_x1 - _x0); }
      else
      { _x0 = _x0 - (_x1 - _x0); }

      if (yPos)
      { _y1 = _y1 + (_y1 - _y0); }
      else
      { _y0 = _y0 - (_y1 - _y0); }

      if (xPos)
      {
        if (yPos)
        {
          if (_set[0,1])
          { newCell.ValueX0 = _val[0,1]; }
          if (_set[1,0])
          { newCell.ValueY0 = _val[1,0]; }
          if (_set[1,1])
          { newCell.ValueCenter = _val[1,1]; }
          _set[0,1] = false;
          _set[1,0] = false;
          _set[1,1] = false;
          newCell.Cell[0,0] = _cell;
        }
        else
        {
          if (_set[0,0])
          { newCell.ValueX0 = _val[0,0]; }
          if (_set[1,1])
          { newCell.ValueY1 = _val[1,1]; }
          if (_set[1,0])
          { newCell.ValueCenter = _val[1,0]; }
          _set[0,0] = false;
          _set[1,1] = false;
          _set[1,0] = false;
          newCell.Cell[0,1] = _cell;
        }
      }
      else
      {
        if (yPos)
        {
          if (_set[1,1])
          { newCell.ValueX1 = _val[1,1]; }
          if (_set[0,0])
          { newCell.ValueY0 = _val[0,0]; }
          if (_set[0,1])
          { newCell.ValueCenter = _val[0,1]; }
          _set[1,1] = false;
          _set[0,0] = false;
          _set[0,1] = false;
          newCell.Cell[1,0] = _cell;
        }
        else
        {
          if (_set[1,0])
          { newCell.ValueX1 = _val[1,0]; }
          if (_set[0,1])
          { newCell.ValueY1 = _val[0,1]; }
          if (_set[0,0])
          { newCell.ValueCenter = _val[0,0]; }
          _set[1,0] = false;
          _set[0,1] = false;
          _set[0,0] = false;
          newCell.Cell[1,1] = _cell;
        }
      }
      _cell = newCell;
    }
    public double GetValue(int ix,int iy)
    {
      return _val[ix,iy];
    }
    public void SetValue(int ix,int iy,double value)
    {
      _val[ix,iy] = value;
      _set[ix,iy] = true;
    }
    public void SetValueNull(int ix,int iy)
    {
      _set[ix,iy] = false;
    }
    public double GetValue(double x,double y)
    {
      double dx = (x - _x0) / (_x1 - _x0);
      double dy = (y - _y0) / (_y1 - _y0);
      if (0 > dx || dx > 1 || 0 > dy || dy > 1)
      { throw new System.Exception("Out of bounds"); }
      if (_cell != null)
      {
        int bx = (x >= 0.5) ? 1 : 0;
        int by = (y >= 0.5) ? 1 : 0;
        double[,] val = new double[2,2];
        double res = _cell.GetValue(ref dx,ref dy,val,out bool bIsSet);
        if (bIsSet == false)
        {
          val[bx,by] = _val[bx,by];
          return Value(dx,dy,val);
        }
        else
        { return res; }
      }
      else
      { return Value(dx,dy,_val); }
    }
    public static double Value(double x,double y,double[,] val)
    {
      double x0 = val[0,0] + y * (val[0,1] - val[0,0]);
      double x1 = val[1,0] + y * (val[1,1] - val[1,0]);
      return x0 + x * (x1 - x0);
    }
  }
}
