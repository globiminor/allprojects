using System;
using System.Drawing;

namespace Grid.View
{
  public class Scene
  {
    private Bitmap _bitmap;
    private double[,] _fields;
    private int[,] _argbs;
    private bool[] _line;
    public int Nx { get; private set; }
    public int Ny { get; private set; }
    public bool Escape { get; set; }

    public Bitmap Bitmap => _bitmap;
    public Scene()
    {
    }
    public void Resize(int nx, int ny)
    {
      _fields = new double[nx, ny];
      _line = new bool[nx];
      Nx = nx;
      Ny = ny;

      _bitmap?.Dispose();
      _bitmap = new Bitmap(nx, ny, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      _argbs = new int[nx, ny];
    }
    public void Fill(int argb)
    {
      for (int i = 0; i < Nx; i++)
      {
        for (int j = 0; j < Ny; j++)
        {
          _argbs[i, j] = argb;
        }
      }
    }
    public void Flush()
    {
      ImageGrid.GridToImage(_bitmap, (x, y) => _argbs[x, y]);
    }

    public double[,] Fields => _fields;
    public bool[] Line => _line;
    public void DrawPoint(int x, int y, int color)
    {
      _argbs[x, y] = color;
    }
    public void SetNew(double t, int u, int v)
    {
      _fields[u, v] = t;
    }
    public bool TrySetNew(double t, int u, int v)
    {
      if (t > _fields[u, v])
      {
        _fields[u, v] = t;
        return true;
      }
      else
      {
        return false;
      }
    }

    public int ResetLine(int xMin, int xMax)
    {
      xMin = Math.Max(0, xMin);
      xMax = Math.Min(xMax, Nx - 1);
      for (int i = xMin; i <= xMax; i++)
      {
        _line[i] = false;
      }
      return 0;
    }

    public bool PointIsVisible(double t, int u, int v)
    {
      if (t > _fields[u, v])
      {
        return true;
      }
      return false;
    }

  }
}
