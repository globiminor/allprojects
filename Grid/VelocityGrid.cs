using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Grid
{
  public class VelocityGrid : DoubleGrid, ILockable
  {
    private ImageGrid _img;
    private readonly bool _grayscale;

    public static VelocityGrid FromImage(string name, bool velocityFromGrayscale)
    {
      ImageGrid img = ImageGrid.FromFile(name);
      return new VelocityGrid(img, velocityFromGrayscale);
    }

    public static VelocityGrid FromImage(string name)
    {
      ImageGrid img = ImageGrid.FromFile(name);
      return new VelocityGrid(img);
    }

    protected VelocityGrid(ImageGrid img)
      : base(img.Extent)
    {
      _img = img;
      _grayscale = GetIsGrayScale(_img.Image);
    }
    protected VelocityGrid(ImageGrid img, bool velocityFromGrayScale)
      : base(img.Extent)
    {
      _img = img;
      _grayscale = velocityFromGrayScale;
    }

    private bool GetIsGrayScale(Image image)
    {
      ColorPalette palette = image.Palette;
      if (palette != null && palette.Entries != null &&
          palette.Entries.Length > 1)
      {
        bool gray = true;
        foreach (var color in _img.Image.Palette.Entries)
        {
          if (color.R != color.G || color.R != color.B)
          {
            gray = false;
          }
        }
        if (gray)
        { return true; }
      }
      int nx = Extent.Nx;
      int ny = Extent.Ny;
      for (int ix = 0; ix < nx; ix++)
      {
        for (int iy = 0; iy < ny; iy++)
        {
          Color color = _img[ix, iy];
          if (color.R != color.G || color.R != color.B)
          {
            return false;
          }
        }
      }
      return true;
    }

    public void LockBits()
    {
      _img.LockBits();
    }
    public void UnlockBits()
    {
      _img.UnlockBits();
    }
    public const double DefaultMinVelo = 0.0001;
    public double MinVelo { get; set; } = DefaultMinVelo;
    public override double this[int ix, int iy]
    {
      get
      {
        if (ix < 0 || ix >= Extent.Nx || iy < 0 || iy >= Extent.Ny)
        { return 0.001; }

        Color pCol = _img[ix, iy];
        double t;
        double gray;

        if (_grayscale)
        {
          double velo = 1 - (pCol.R / 255.0);
          if (velo == 0)
          {
            velo = MinVelo;
          }
          return velo;
        }

        gray = (pCol.R + pCol.G + pCol.B) / (3.0 * Byte.MaxValue);

        if (gray > 0.95)
        { /* white, no obstruction */
          t = 0.9;
        }
        else if (((double)pCol.G / Byte.MaxValue - gray) / (1.0 - gray) < 0.2)
        {
          if (pCol.G < 70 &&
            pCol.R > Byte.MaxValue / 2)
          {
            /* impassable / forbidden */
            t = MinVelo;
          }
          else
          {
            /* road */
            t = 1.0;
          }
        }
        else if (gray > 0.8)
        { /* slight obstruction */
          t = 0.7;
        }
        else if (gray > 0.5)
        { /* strong obstruction */
          t = 0.4;
        }
        else
        {                 /* very strong obstruction */
          t = 0.2;
        }
        return t;
      }

      set
      {
      }
    }

    public void Close()
    {
      _img.Close();
    }
  }
}
