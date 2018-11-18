
using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class BlockGrid
  {
    private class BlockIndex
    {
      public Block Parent { get; set; }
      public byte Ix { get; set; }
      public byte Iy { get; set; }

      public override string ToString()
      {
        return $"{Parent} {Ix} {Iy}";
      }
    }

    private class Cell : ICell
    {
      public class CellBlockComparer : IComparer<Cell>, IEqualityComparer<Cell>
      {
        bool IEqualityComparer<Cell>.Equals(Cell x, Cell y)
        { return Compare(x, y, onlyEquality: true) == 0; }

        int IEqualityComparer<Cell>.GetHashCode(Cell obj)
        {
          int hashCode = 1;
          foreach (BlockIndex idx in obj.Blocks)
          {
            hashCode = hashCode << 2;
            hashCode += idx.Ix + 2 * idx.Iy;
          }
          return hashCode * 29 + obj._cellX + 37 * obj._cellY;
        }

        int IComparer<Cell>.Compare(Cell x, Cell y)
        { return Compare(x, y); }

        public static int Compare(Cell x, Cell y, bool onlyEquality = false)
        {
          if (x == y)
          { return 0; }

          if (x == null) return 1;
          if (y == null) return -1;

          if (x.Detail == y.Detail) { return 0; }
          if (onlyEquality) { return 1; }

          int d = x._cellX - y._cellX;
          if (d != 0) return d;

          d = x._cellY - y._cellY;
          if (d != 0) return d;

          int nBlocks = x.Blocks.Length;
          d = nBlocks - y.Blocks.Length;
          if (d != 0) return d;

          for (int i = 0; i < nBlocks; i++)
          {
            BlockIndex bx = x.Blocks[i];
            BlockIndex by = y.Blocks[i];

            d = bx.Ix - by.Ix;
            if (d != 0) { return d; }
            d = bx.Iy - by.Iy;
            if (d != 0) { return d; }
          }

          return 0;
        }
      }

      public int Ix { get; set; }
      public int Iy { get; set; }

      private int _cellX;
      private int _cellY;

      public BlockIndex[] Blocks { get; private set; }
      public Block Detail { get; private set; }


      private readonly BlockGrid _grid;
      private Box _detailBox;
      public Box DetailBox => _detailBox ?? (_detailBox = GetDetailBox());

      public override string ToString()
      {
        return $"{Ix,-5} {Iy,-5} Det:{Blocks.Length} {Detail}";
      }

      public static Cell Create(BlockGrid grid, int ix, int iy)
      {
        int size0 = grid._blockSize;
        int cellX = ix / size0;
        int cellY = iy / size0;
        Block top = grid._blocks[cellX, cellY];

        int restX = ix - cellX * size0;
        int restY = iy - cellY * size0;

        Block parent = top;
        int size = size0;

        List<BlockIndex> blocks = new List<BlockIndex>();
        while (parent.Children != null)
        {
          size = size / 2;
          BlockIndex idx = new BlockIndex
          {
            Parent = parent,
            Ix = (byte)(restX / size),
            Iy = (byte)(restY / size)
          };
          blocks.Add(idx);

          restX = restX % size;
          restY = restY % size;

          parent = parent.Children[idx.Ix, idx.Iy];
        }

        Cell result = new Cell(grid)
        {
          Ix = ix,
          Iy = iy,
          _cellX = cellX,
          _cellY = cellY,
          Blocks = blocks.ToArray(),
          Detail = parent
        };

        return result;
      }

      public Cell(BlockGrid grid)
      {
        _grid = grid;
      }

      public static bool IsBlocked(int lx, int ly, ushort blockValue)
      {
        int idx = 15 - (4 * lx + ly);
        int blocked = (blockValue >> idx) & 1;

        return blocked == 1;
      }
      public bool IsBlocked()
      {
        if (Detail.BlockType == BlockType.NoBlock)
        { return false; }
        if (Detail.BlockType == BlockType.FullBlock)
        { return true; }

        Box detailBox = DetailBox;
        int ix = Ix - (int)detailBox.Min.X;
        int iy = Iy - (int)detailBox.Min.Y;

        bool isBlocked = IsBlocked(ix, iy, Detail.BlockValue);
        return isBlocked;
      }

      private Box GetDetailBox()
      {
        int blockSize = _grid._blockSize;
        int x0 = _cellX * blockSize;
        int y0 = _cellY * blockSize;
        int size = blockSize;
        foreach (BlockIndex idx in Blocks)
        {
          size = size / 2;
          x0 += idx.Ix * size;
          y0 += idx.Iy * size;
        }
        return new Box(new Point2D(x0, y0), new Point2D(x0 + size, y0 + size));
      }
    }
    private enum BlockType { Unknown = -1, NoBlock = 0, FullBlock = ushort.MaxValue, PartBlock = 1 }
    private class Block
    {
      public BlockType BlockType => (BlockType)BlockValue;
      public ushort BlockValue { get; set; }
      public Block[,] Children { get; set; }

      public string Code()
      {
        if (BlockType == BlockType.NoBlock)
        {
          return "\u25A1";
        }
        if (BlockType == BlockType.FullBlock)
        {
          return "\u25A0";
        }
        if (Children != null)
        {
          return "\u25A3";
        }
        //string code = \u2800 - \u28ff;
        byte[] b = new byte[4];
        b[1] = 40;
        b[3] = 40;

        int bv = BlockValue;
        byte b0 = GetCode(BlockValue / 256);
        byte b2 = GetCode(BlockValue % 256);
        b[0] = b0;
        b[2] = b2;
        string code = System.Text.Encoding.Unicode.GetString(b);
        return code;
      }

      private byte GetCode(int value)
      {
        int result = 0;
        if (((value >> 7) & 1) == 1) result += 1;
        if (((value >> 6) & 1) == 1) result += 2;
        if (((value >> 5) & 1) == 1) result += 4;

        if (((value >> 3) & 1) == 1) result += 8;
        if (((value >> 2) & 1) == 1) result += 16;
        if (((value >> 1) & 1) == 1) result += 32;

        if (((value >> 4) & 1) == 1) result += 64;
        if (((value >> 0) & 1) == 1) result += 128;

        return (byte)result;
      }

      public override string ToString()
      {
        Block[,] c = Children;
        return c == null
          ? $"{Code()}"
          : $"{Code()} {c[0, 0].Code()},{c[0, 1].Code()},{c[1, 0].Code()},{c[1, 1].Code()}";
      }
    }

    private Block[,] _blocks;
    private GridExtent _extent;
    public GridExtent Extent => _extent;

    private int _blockSize;

    public const int DefaultBlockSize = 128;
    public static BlockGrid Create(VelocityGrid veloGrid)
    {
      int nx = veloGrid.Extent.Nx;
      int ny = veloGrid.Extent.Ny;

      int dx = DefaultBlockSize;
      BlockGrid created = new BlockGrid
      {
        _blocks = new Block[(int)Math.Ceiling(nx / (double)dx), (int)Math.Ceiling(ny / (double)dx)],
        _extent = veloGrid.Extent,
        _blockSize = dx
      };

      for (int ix = 0; ix < nx; ix += dx)
      {
        for (int iy = 0; iy < ny; iy += dx)
        {
          Block block = CreateBlock(ix, nx, iy, ny, dx, veloGrid);
          created._blocks[ix / dx, iy / dx] = block;
        }
      }

      return created;
    }

    private static Block CreateBlock(int x0, int nx, int y0, int ny, int size, VelocityGrid veloGrid)
    {
      if (size == 4)
      {
        int block = 0;
        for (int ix = x0; ix < x0 + 4; ix++)
        {
          for (int iy = y0; iy < y0 + 4; iy++)
          {
            int b;
            if (ix >= nx || iy >= ny)
            {
              b = 1;
            }
            else
            {
              double velo = veloGrid[ix, iy];
              b = velo < 0.01 ? 1 : 0;
            }
            block = block << 1;
            block += b;
          }
        }
        if (block > 0 && block < ushort.MaxValue)
        { }
        return new Block { BlockValue = (ushort)block };
      }
      int s_2 = size / 2;

      Block[,] children = new Block[2, 2];
      int t = -1;
      for (int bx = 0; bx < 2; bx++)
      {
        for (int by = 0; by < 2; by++)
        {
          Block b = CreateBlock(x0 + bx * s_2, nx, y0 + by * s_2, ny, s_2, veloGrid);

          if (t < 0)
          { t = b.BlockValue; }
          else if (t != b.BlockValue)
          { t = 1; }
          children[bx, by] = b;
        }
      }

      Block created = new Block { BlockValue = (ushort)t };
      if (t > 0 && t < ushort.MaxValue)
      {
        created.Children = children;
      }
      return created;
    }

    public bool HasBlocks(ICell start, ICell end)
    {
      Cell cs = Cell.Create(this, start.Ix, start.Iy);
      Cell ce = Cell.Create(this, end.Ix, end.Iy);

      if (Cell.CellBlockComparer.Compare(cs, ce, onlyEquality: true) == 0)
      {
        if (cs.Detail.BlockType == BlockType.NoBlock)
        { return false; }
      }
      if (cs.Detail.BlockType == BlockType.FullBlock)
      { return true; }
      if (ce.Detail.BlockType == BlockType.FullBlock)
      { return true; }

      int dx = end.Ix - start.Ix;
      int dy = end.Iy - start.Iy;

      Cell t = cs;
      while (t != null)
      {
        t = TraverseDetail(t, ref dx, ref dy, out bool hasBlocks);
        if (hasBlocks)
        { return true; }

        if (ce.Detail.BlockType == BlockType.NoBlock && t != null &&
          Cell.CellBlockComparer.Compare(t, ce, onlyEquality:true) == 0)
        {
          return false;
        }
      }

      return false;
    }

    private Cell TraverseDetail(Cell cell, ref int dx, ref int dy, out bool hasBlocks)
    {
      bool direct;
      hasBlocks = true;
      if (cell.Detail.BlockType == BlockType.NoBlock)
      {
        hasBlocks = false;
        direct = true;
      }
      else if (cell.Detail.BlockType == BlockType.FullBlock)
      {
        hasBlocks = true;
        direct = true;
      }
      else { direct = false; }
      Box box = cell.DetailBox;
      Line line = new Line(new Point2D(cell.Ix + 0.5, cell.Iy + 0.5), 
        new Point2D(cell.Ix + 0.5 + dx, cell.Iy + 0.5 + dy));
      GeometryCollection intersections = box.Intersection(line);
      Cell next = null;
      if (intersections?.Count == 1)
      {
        Point2D border = (Point2D)intersections[0];
        int newX = (int)border.X;
        int newY = (int)border.Y;
        if (border.X == box.Min.X)
        {
          newX--;
        }
        else if (border.Y == box.Min.Y)
        {
          newY--;
        }
        else if (newX == box.Max.X && newY == box.Max.Y)
        {
          newX--;
        }
        next = Cell.Create(this, newX, newY);
        dx -= next.Ix - cell.Ix;
        dy -= next.Iy - cell.Iy;
      }
      else
      { throw new NotImplementedException(); }

      if (!direct)
      {
        int ix = cell.Ix;
        int iy = cell.Iy;

        int blockValue = cell.Detail.BlockValue;

        while (ix != next.Ix || iy != next.Iy)
        {
          int lx = ix - (int)box.Min.X;
          int ly = iy - (int)box.Min.Y;

          int idx = 15 - (4 * lx + ly);
          int blocked = (blockValue >> idx) & 1;
          if (blocked == 1)
          {
            hasBlocks = true;
            return next;
          }

          int ddx = next.Ix - ix;
          int ddy = next.Iy - iy;

          bool xSet = false;
          if (Math.Abs(ddx) > Math.Abs(ddy))
          {
            int t = ix + Math.Sign(ddx);
            if (t >= box.Min.X && t < box.Max.X)
            {
              ix = t;
              xSet = true;
            }
          }
          if (!xSet)
          {
            iy = iy + Math.Sign(ddy);
          }
        }

        throw new NotImplementedException();
        hasBlocks = true;
      }

      return next;
    }

    private void FindWay(ICell start, ICell end)
    {
      Cell cs = Cell.Create(this, start.Ix, start.Iy);
      Cell ce = Cell.Create(this, end.Ix, end.Iy);

    }
  }
}