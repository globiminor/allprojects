
using System;

namespace Grid
{
  public class Pyramide
  {
    public class Block
    {
      private Block[] _children;
      private double _hMin;
      private double _hMax;
      private double _dh;

      public double HMin
      {
        get { return _hMin; }
        set { _hMin = value; }
      }
      public double HMax
      {
        get { return _hMax; }
        set { _hMax = value; }
      }
      public double Dh
      {
        get { return _dh; }
        set { _dh = value; }
      }

      public Block[] Child
      {
        get
        {
          if (_children == null)
          {
            int n = 4;
            _children = new Block[n];
            for (int i = 0; i < 4; i++)
            { _children[i] = new Block(); }
          }
          return _children;
        }
      }
      public Block ChildNW
      { get { return Child[0]; } }
      public Block ChildSW
      { get { return Child[1]; } }
      public Block ChildNE
      { get { return Child[2]; } }
      public Block ChildSE
      { get { return Child[3]; } }


      public bool HasChildren()
      {
        return _children != null;
      }
    }

    private IDoubleGrid _grid;
    private Block _parentBlock;

    private int _nMax;

    private Pyramide()
    { }

    public static Pyramide Create(IDoubleGrid grid)
    {
      Pyramide p = new Pyramide();
      p._grid = grid;

      p._parentBlock = new Block();

      p.CalcPyramid();

      return p;
    }
    public IDoubleGrid Grid
    {
      get { return _grid; }
    }

    public Block ParentBlock
    {
      get { return _parentBlock; }
    }

    public int NMax
    {
      get { return _nMax; }
    }
    private void CalcPyramid()
    {
      int n, t;
      GridExtent extent = _grid.Extent;
      n = Math.Max(extent.Nx, extent.Ny);

      t = 1;
      while (t < n)
      {
        t *= 2;
      }
      _nMax = t;
      DivideBlock(_parentBlock, 0, 0, t);
      AssignMinMax(_parentBlock, 0, 0, t);

    }

    void DivideBlock(Block parentBlock, int i0, int j0, int n)
    {
      int i, j, i1, j1;
      if (n <= 2)
      {  // smallest Block
        return;
      }

      // search height element != 0 within block
      i1 = i0 + n;
      if (i1 > _grid.Extent.Nx)
      {
        i1 = _grid.Extent.Nx;
      }
      j1 = j0 + n;
      if (j1 > _grid.Extent.Ny)
      {
        j1 = _grid.Extent.Ny;
      }

      i = i0;
      j = j0;
      while (i < i1 && j < j1 && _grid[i, j] <= 0)
      {
        while (j < j1 && _grid[i, j] <= 0)
        {
          j++;
        }
        if (j >= j1)
        {
          i++;
          j = j0;
        }
      }

      if (i < i1 && j < j1)
      { // height found --> divide
        int n2 = n / 2;
        DivideBlock(parentBlock.ChildNW, i0, j0, n2);
        DivideBlock(parentBlock.ChildSW, i0, j0 + n2, n2);
        DivideBlock(parentBlock.ChildNE, i0 + n2, j0, n2);
        DivideBlock(parentBlock.ChildSE, i0 + n2, j0 + n2, n2);
      }
    }

    void AssignMinMax(Block parentBlock, int i0, int j0, int n)
    {
      if (parentBlock.HasChildren())
      {
        int n2 = n / 2;
        AssignMinMax(parentBlock.ChildNW, i0, j0, n2);
        AssignMinMax(parentBlock.ChildSW, i0, j0 + n2, n2);
        AssignMinMax(parentBlock.ChildNE, i0 + n2, j0, n2);
        AssignMinMax(parentBlock.ChildSE, i0 + n2, j0 + n2, n2);

        for (int i = 0; i < 4; i++)
        {
          if (parentBlock.Child[i].HMin > 0 && (parentBlock.HMin == 0 ||
              parentBlock.HMin > parentBlock.Child[i].HMin))
          {
            parentBlock.HMin = parentBlock.Child[i].HMin;
          }
          if (parentBlock.HMax < parentBlock.Child[i].HMax)
          {
            parentBlock.HMax = parentBlock.Child[i].HMax;
          }
        }
      }
      else if (n == 2)
      {
        for (int i = i0; i <= i0 + 2; i++)
        {
          for (int j = j0; j <= j0 + 2; j++)
          {
            double th = _grid[i, j];
            if (th > 0 && (th < parentBlock.HMin || parentBlock.HMin == 0))
            { parentBlock.HMin = th; }
            if (th > parentBlock.HMax) parentBlock.HMax = th;
          }
        }
      }
      parentBlock.Dh = DoubleGrid.CalcDh(_grid, i0, j0, n);
    }



  }
}



