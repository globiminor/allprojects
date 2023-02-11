using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Grid
{
  /// <summary>
  /// Summary description for ImageGrid.
  /// </summary>
  public class ImageGrid : BaseGrid<Color>, ILockable
  {
    private Bitmap _bitmap;
    private BitmapData _data;
    private Color[] _palette;
    private PixelFormat _pixelFormat;

    public static string GetWorldPath(string imagePath)
    {
      string ext = Path.GetExtension(imagePath);
      string sWorldName = Path.Combine(Path.GetDirectoryName(imagePath),
        Path.GetFileNameWithoutExtension(imagePath) + "." + ext[1] + ext[3] + "w");
      return sWorldName;
    }
    public static ImageGrid FromFile(string name)
    {
      string sWorldName = GetWorldPath(name);
      Bitmap bitmap = (Bitmap)Bitmap.FromFile(name, true);
      GridExtent extent = GridExtent.GetWorldFile(sWorldName);
      extent.SetSize(bitmap.Size.Width, bitmap.Size.Height);

      return new ImageGrid(extent, bitmap);
    }
    public Bitmap Bitmap
    {
      get { return _bitmap; }
    }
    private ImageGrid(GridExtent extent, Bitmap bitmap)
      : base(extent)
    {
      _bitmap = bitmap;
    }
    public Image Image
    {
      get
      { return _bitmap; }
    }
    void ILockable.LockBits()
    { LockBits(); }
    public BitmapData LockBits()
    {
      UnlockBits();
      if (_bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
      {
        _data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
        ImageLockMode.ReadOnly, _bitmap.PixelFormat);
        _pixelFormat = _bitmap.PixelFormat;
        _palette = _bitmap.Palette.Entries;
      }
      return _data;
    }
    public void UnlockBits()
    {
      if (_data != null)
      {
        _bitmap.UnlockBits(_data);
        _data = null;
      }
    }

    public override Color GetCell(int ix, int iy)
    {
      if (_data == null || _pixelFormat != PixelFormat.Format8bppIndexed)
      { return _bitmap.GetPixel(ix, iy); }
      else
      {
        unsafe
        {
          byte* b = (byte*)_data.Scan0;
          byte bxy = b[iy * _data.Stride + ix];
          return _palette[bxy];
        }
      }
    }
    public void Close()
    {
      if (_bitmap != null)
      {
        _bitmap.Dispose();
        _bitmap = null;
      }
    }

    public static void WriteWorldFile(IGrid grd, string path)
    {
      using (TextWriter worldWriter = new StreamWriter(path))
      using (new InvariantCulture())
      {
        worldWriter.WriteLine(grd.Extent.Dx);
        worldWriter.WriteLine(0);
        worldWriter.WriteLine(0);
        worldWriter.WriteLine(grd.Extent.Dy);
        worldWriter.WriteLine(grd.Extent.X0);
        worldWriter.WriteLine(grd.Extent.Y0);
      }
    }
    public static void GridToImage(IGrid<int> grd, string name, byte[] r = null, byte[] g = null, byte[] b = null, ImageFormat format = null)
    {
      WriteWorldFile(grd, GetWorldPath(name));

      int width = grd.Extent.Nx;
      int height = grd.Extent.Ny;
      GridToImage(name, width, height,
        getValue: (col, row) =>
        {
          int iVal = grd[col, row];
          return (byte)iVal;
        },
        r: r, g: g, b: b, format: format);
    }

    public static void GridToImage(string name, int width, int height, Func<int, int, byte> getValue,
      byte[] r = null, byte[] g = null, byte[] b = null, ImageFormat format = null)
    {
      if (r == null)
      {
        r = new byte[256];
        g = new byte[256];
        b = new byte[256];
        Common.InitColors(r, g, b);
      }

      int nColors = r.GetLength(0);
      if (nColors > 256)
      {
        nColors = 256;
      }

      using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
      {
        // It seems Bitmap.Palette does not return the instance of the bitmap palette (if this exists) but derives
        // the "get" palette from internal properties
        // Therefore, get the palette standalone, assign the colors and set the Bitmap.Palette afterwards
        ColorPalette palette = bitmap.Palette;
        for (int i = 0; i < nColors; i++)
        {
          palette.Entries[i] = Color.FromArgb(r[i], g[i], b[i]);
        }
        bitmap.Palette = palette;


        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
          ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

        // Write to the temporary buffer that is provided by LockBits.
        // Copy the pixels from the source image in this loop.
        // Because you want an index, convert RGB to the appropriate
        // palette index here.
        IntPtr pixels = data.Scan0;

        unsafe
        {
          // Get the pointer to the image bits.
          // This is the unsafe operation.
          byte* bytes;
          if (data.Stride > 0)
          {
            bytes = (byte*)pixels.ToPointer();
          }
          else
          {
            // If the Stide is negative, Scan0 points to the last
            // scanline in the buffer. To normalize the loop, obtain
            // a pointer to the front of the buffer that is located
            // (Height-1) scanlines previous.
            bytes = (byte*)pixels.ToPointer() + data.Stride * (bitmap.Height - 1);
          }
          uint stride = (uint)Math.Abs(data.Stride);

          for (int row = 0; row < height; row++)
          {
            for (int col = 0; col < width; col++)
            {
              byte val = getValue(col, row);

              // The destination pixel.
              // The pointer to the color index byte of the
              // destination; this real pointer causes this
              // code to be considered unsafe.
              byte* p8bppPixel = bytes + row * stride + col;
              *p8bppPixel = val;
            }
          }
        } /* end unsafe */

        // To commit the changes, unlock the portion of the bitmap.
        bitmap.UnlockBits(data);

        using (FileStream file = new FileStream(name, FileMode.Create))
        {
          format = format ?? ImageFormat.Tiff;

          //Encoder enc = Encoder.SaveFlag;
          //EncoderParameters encPars = new EncoderParameters();
          //encPars.Param[0] = new EncoderParameter(enc, (long)EncoderValue.CompressionLZW);

          bitmap.Save(file, format);
        }
      }
    }

    public static void GridToImage(string name, int width, int height,
      Func<int, int, byte> getRValue, Func<int, int, byte> getGValue, ImageFormat format = null)
    {
      using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
      {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
          ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        // Write to the temporary buffer that is provided by LockBits.
        // Copy the pixels from the source image in this loop.
        // Because you want an index, convert RGB to the appropriate
        // palette index here.
        IntPtr pixels = data.Scan0;

        unsafe
        {
          // Get the pointer to the image bits.
          // This is the unsafe operation.
          byte* bytes;
          if (data.Stride > 0)
          {
            bytes = (byte*)pixels.ToPointer();
          }
          else
          {
            // If the Stride is negative, Scan0 points to the last
            // scanline in the buffer. To normalize the loop, obtain
            // a pointer to the front of the buffer that is located
            // (Height-1) scanlines previous.
            bytes = (byte*)pixels.ToPointer() + data.Stride * (bitmap.Height - 1);
          }
          uint stride = (uint)Math.Abs(data.Stride);

          for (int row = 0; row < height; row++)
          {
            for (int col = 0; col < width; col++)
            {
              byte rVal = getRValue(col, row);
              byte bVal = getGValue(col, row);

              // The destination pixel.
              // The pointer to the color index byte of the
              // destination; this real pointer causes this
              // code to be considered unsafe.
              {
                byte* p8bppPixel = bytes + row * stride + 4 * col + 1;
                *p8bppPixel = rVal;
              }
              {
                byte* p8bppPixel = bytes + row * stride + 4 * col + 2;
                *p8bppPixel = bVal;
              }
            }
          }
        } /* end unsafe */

        // To commit the changes, unlock the portion of the bitmap.
        bitmap.UnlockBits(data);

        using (FileStream file = new FileStream(name, FileMode.Create))
        {
          //Encoder enc = Encoder.SaveFlag;
          //EncoderParameters encPars = new EncoderParameters();
          //encPars.Param[0] = new EncoderParameter(enc, (long)EncoderValue.CompressionLZW);

          format = format ?? ImageFormat.Tiff;

          bitmap.Save(file, format);
        }
      }
    }

    public static void GridToImage(Bitmap bitmap, Func<int, int, int> getArgb)
    {
      int width = bitmap.Width;
      int height = bitmap.Height;
      BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      // Write to the temporary buffer that is provided by LockBits.
      // Copy the pixels from the source image in this loop.
      // Because you want an index, convert RGB to the appropriate
      // palette index here.
      IntPtr pixels = data.Scan0;

      unsafe
      {
        // Get the pointer to the image bits.
        // This is the unsafe operation.
        int* ints;
        if (data.Stride > 0)
        {
          ints = (int*)pixels.ToPointer();
        }
        else
        {
          // If the Stride is negative, Scan0 points to the last
          // scanline in the buffer. To normalize the loop, obtain
          // a pointer to the front of the buffer that is located
          // (Height-1) scanlines previous.
          ints = (int*)pixels.ToPointer() + data.Stride / 4 * (bitmap.Height - 1);
        }
        uint stride = (uint)Math.Abs(data.Stride) / 4;

        for (int row = 0; row < height; row++)
        {
          for (int col = 0; col < width; col++)
          {
            int argb = getArgb(col, row);

            // The destination pixel.
            // The pointer to the color index byte of the
            // destination; this real pointer causes this
            // code to be considered unsafe.
            {
              int* p8bppPixel = ints + row * stride + col;
              *p8bppPixel = argb;
            }
          }
        }
      } /* end unsafe */

      // To commit the changes, unlock the portion of the bitmap.
      bitmap.UnlockBits(data);
    }

    public static void ImageToGrid(Bitmap bitmap, Action<int, int, int> getArgb)
    {
      int width = bitmap.Width;
      int height = bitmap.Height;
      BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      // Write to the temporary buffer that is provided by LockBits.
      // Copy the pixels from the source image in this loop.
      // Because you want an index, convert RGB to the appropriate
      // palette index here.
      IntPtr pixels = data.Scan0;

      unsafe
      {
        // Get the pointer to the image bits.
        // This is the unsafe operation.
        int* ints;
        if (data.Stride > 0)
        {
          ints = (int*)pixels.ToPointer();
        }
        else
        {
          // If the Stride is negative, Scan0 points to the last
          // scanline in the buffer. To normalize the loop, obtain
          // a pointer to the front of the buffer that is located
          // (Height-1) scanlines previous.
          ints = (int*)pixels.ToPointer() + data.Stride / 4 * (bitmap.Height - 1);
        }
        uint stride = (uint)Math.Abs(data.Stride) / 4;

        for (int row = 0; row < height; row++)
        {
          for (int col = 0; col < width; col++)
          {
            // The destination pixel.
            // The pointer to the color index byte of the
            // destination; this real pointer causes this
            // code to be considered unsafe.
            {
              int* p8bppPixel = ints + row * stride + col;
              int argb = *p8bppPixel;
              getArgb(col, row, argb);
            }
          }
        }
      } /* end unsafe */

      // To commit the changes, unlock the portion of the bitmap.
      bitmap.UnlockBits(data);
    }

  }
}
