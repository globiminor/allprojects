
using System;

namespace Grid.Lcp
{
  public class BlockGrid
  {
    private enum BlockType { Unknown = -1, NoBlock = 0, FullBlock = ushort.MaxValue, PartBlock = 1 }
    private class Block
    {
      public ushort BlockType { get; set; }
      public Block[,] Children { get; set; }

      public override string ToString()
      {
        return Children == null
          ? $"{BlockType}"
          : $"{BlockType} {Children[0, 0].BlockType},{Children[0, 1].BlockType},{Children[1, 0].BlockType},{Children[1, 1].BlockType}";
      }
    }

    private Block[,] _blocks;
    public static BlockGrid Create(VelocityGrid veloGrid)
    {
      int nx = veloGrid.Extent.Nx;
      int ny = veloGrid.Extent.Ny;

      int dx = 128;
      BlockGrid created = new BlockGrid
      { _blocks = new Block[(int)Math.Ceiling( nx / (double)dx), (int)Math.Ceiling(ny / (double)dx)] };

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
        return new Block { BlockType = (ushort)block };
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
          { t = b.BlockType; }
          else if (t != b.BlockType)
          { t = 1; }
          children[bx, by] = b;
        }
      }

      Block created = new Block { BlockType = (ushort)t };
      if (t > 0 && t <= ushort.MaxValue)
      {
        created.Children = children;
      }
      return created;
    }
  }
}