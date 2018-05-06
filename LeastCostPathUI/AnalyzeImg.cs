using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System;

namespace LeastCostPathUI
{
  public class AnalyzeImg : IDisposable
  {
    private Bitmap _bitmap;
    private BitmapData _data;

    public AnalyzeImg(string file)
    {
      _bitmap = (Bitmap)Image.FromFile(file);
    }
    public void Dispose()
    {
      if (_bitmap != null)
      {
        _bitmap.Dispose();
      }
      _bitmap = null;
    }

    private double Delta(int x, int y)
    {
      double d = Math.Sqrt(256 - x) - Math.Sqrt(256 - y);
      return d;
    }
    public Color GetFarestColor(IList<Color> colors)
    {
      int nx = _bitmap.Width;
      int ny = _bitmap.Height;

      double maxD = 0;
      Color max = Color.Empty;
      for (int ix = 0; ix < nx; ix++)
      {

        for (int iy = 0; iy < ny; iy++)
        {
          Color color = _bitmap.GetPixel(ix, iy);
          double maxC = double.MaxValue;
          foreach (Color refColor in colors)
          {
            double dr = Delta(refColor.R, color.R);
            double dg = Delta(refColor.G, color.G);
            double db = Delta(refColor.B, color.B);

            double d = dr * dr + dg * dg + db * db;
            if (maxC > d)
            {
              maxC = d;
            }
          }
          if (maxC > maxD)
          {
            max = color;
            maxD = maxC;
          }
        }
      }
      return max;
    }
    public BitmapData LockBits()
    {
      _data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
        ImageLockMode.ReadOnly, _bitmap.PixelFormat);
      return _data;
    }
    public void UnlockBits()
    {
      _bitmap.UnlockBits(_data);
      _data = null;
    }

  }
}
