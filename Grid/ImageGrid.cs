using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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

    public static ImageGrid FromFile(string name)
    {
      string sWorldName =
        Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".tfw";

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

    public override Color this[int ix, int iy]
    {
      get
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
      set
      { }
    }
    public void Close()
    {
      if (_bitmap != null)
      {
        _bitmap.Dispose();
        _bitmap = null;
      }
    }

    public static void GridToTif(IntGrid grd, string name, byte[] r = null, byte[] g = null, byte[] b = null)
    {
      if (r == null)
      {
        r = new byte[256];
        g = new byte[256];
        b = new byte[256];
        Common.InitColors(r, g, b);
      }

      Encoder pEnc = Encoder.SaveFlag;
      EncoderParameters pEncParams = new EncoderParameters();
      pEncParams.Param[0] = new EncoderParameter(pEnc, (long)EncoderValue.CompressionLZW);

      string tfw = Path.Combine( Path.GetDirectoryName(name),
        Path.GetFileNameWithoutExtension(name) + ".tfw");
      using (TextWriter tfwWriter = new StreamWriter(tfw))
      using (new InvariantCulture())
      {
        tfwWriter.WriteLine(grd.Extent.Dx);
        tfwWriter.WriteLine(0);
        tfwWriter.WriteLine(0);
        tfwWriter.WriteLine(grd.Extent.Dy);
        tfwWriter.WriteLine(grd.Extent.X0);
        tfwWriter.WriteLine(grd.Extent.Y0);
      }

      FileStream pFile = new FileStream(name, FileMode.Create);
      try
      {
        ImageFormat pFormat = ImageFormat.Tiff;
        int Width = grd.Extent.Nx;
        int Height = grd.Extent.Ny;
        int nColors = r.GetLength(0);
        if (nColors > 256)
        {
          nColors = 256;
        }

        Bitmap pBitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
        BitmapData pData = pBitmap.LockBits(new Rectangle(0, 0, Width, Height),
          ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

        // It seems Bitmap.Palette does not return the instance of the bitmap palette (if this exists) but derives
        // the "get" palette from internal properties
        // Therefore, get the palette standalone, assign the colors and set the Bitmap.Palette afterwards
        ColorPalette pPalette = pBitmap.Palette;
        for (int i = 0; i < nColors; i++)
        {
          pPalette.Entries[i] = Color.FromArgb(r[i], g[i], b[i]);
        }
        pBitmap.Palette = pPalette;


        // Write to the temporary buffer that is provided by LockBits.
        // Copy the pixels from the source image in this loop.
        // Because you want an index, convert RGB to the appropriate
        // palette index here.
        IntPtr pPixel = pData.Scan0;

        unsafe
        {
          // Get the pointer to the image bits.
          // This is the unsafe operation.
          byte* pBits;
          if (pData.Stride > 0)
          {
            pBits = (byte*)pPixel.ToPointer();
          }
          else
          {
            // If the Stide is negative, Scan0 points to the last
            // scanline in the buffer. To normalize the loop, obtain
            // a pointer to the front of the buffer that is located
            // (Height-1) scanlines previous.
            pBits = (byte*)pPixel.ToPointer() + pData.Stride * (pBitmap.Height - 1);
          }
          uint stride = (uint)Math.Abs(pData.Stride);

          for (int row = 0; row < Height; row++)
          {
            for (int col = 0; col < Width; col++)
            {
              // The destination pixel.
              // The pointer to the color index byte of the
              // destination; this real pointer causes this
              // code to be considered unsafe.
              byte* p8bppPixel = pBits + row * stride + col;

              int iVal = grd[col, row];

              byte val = (byte)iVal;
              *p8bppPixel = val;

            }
          }
        } /* end unsafe */

        // To commit the changes, unlock the portion of the bitmap.
        pBitmap.UnlockBits(pData);

        pBitmap.Save(pFile, pFormat);

        pBitmap.Dispose(); // free resources
      }
      finally
      {
        pFile.Close();
      }
    }
  }
}
