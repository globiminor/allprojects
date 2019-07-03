
using Android.Graphics;
using System;

namespace OMapScratch
{
  partial interface IGeoImage
  {
    Bitmap LoadImage(MatrixPrj worldPrj, int width, int height);
  }
  public interface IGeoImageViewmodel
  {
    float Transparency { get; }
    float? GrayTransform { get; }
    float? ColorRotation { get; }
  }

  public static class IGeoImageViewUtils
  {
    public static void SetColor(Paint p, IGeoImageViewmodel geoImage)
    {
      float o = 1 - geoImage.Transparency;

      if (geoImage.ColorRotation != null || geoImage.GrayTransform != null)
      {
        float g = geoImage.GrayTransform ?? geoImage.ColorRotation.Value;
        g = Math.Min(1, Math.Max(0, g));

        float a = (g < 1 / 3f) ? 3 * (1 / 3f - g) : (g > 2 / 3f) ? 3 * (g - 2 / 3f) : 0;
        float b = (g < 2 / 3f) ? 1 - 3 * Math.Abs(g - 1 / 3f) : 0;
        float c = (g > 1 / 3f) ? 1 - 3 * Math.Abs(g - 2 / 3f) : 0;

        if (geoImage.ColorRotation.HasValue)
        {
          p.SetColorFilter(new ColorMatrixColorFilter(new float[] {
          a, b, c, 0, 0,
          c, a, b, 0, 0,
          b, c, a, 0, 0,
          0, 0, 0, o, 0
        }));
        }
        else if (geoImage.GrayTransform.HasValue)
        {
          p.SetColorFilter(new ColorMatrixColorFilter(new float[] {
          a, b, c, 0, 0,
          a, b, c, 0, 0,
          a, b, c, 0, 0,
          0, 0, 0, o, 0
        }));
        }
      }
      else
      {
        p.Alpha = Math.Max(0, Math.Min(255, (int)(255 * o)));
      }
    }
  }

  partial class GeoImageVm : IDisposable, IGeoImageViewmodel
  {
    private Bitmap _bitmap;

    public Bitmap Bitmap => _bitmap ?? (_bitmap = GetBitmap());

    public void Dispose()
    {
      _bitmap?.Dispose();
    }

    private Bitmap GetBitmap()
    {
      Bitmap bitmap = null;
      _baseImage.LoadImageParts(Projection, Width, Height, ref bitmap, null);
      return bitmap;
    }

    float IGeoImageViewmodel.Transparency => 1 - Opacity / 100.0f;
    float? IGeoImageViewmodel.GrayTransform => Gray <= 0 ? (float?)null : Math.Min(1.0f, Gray / 100.0f);
    float? IGeoImageViewmodel.ColorRotation => ColorRotation <= 0 ? (float?)null : Math.Min(1.0f, ColorRotation / 100.0f);
  }
  partial class GeoImageComb
  {
    public Bitmap LoadImage(MatrixPrj worldPrj, int width, int height)
    {
      MatrixPrj imagePrj = _baseImage.GetImagePrj(worldPrj, width, height, out int nx, out int ny, out IBox imagePartExtent);

      Bitmap bitmap = null;

      for (int iPart = _views.Count - 1; iPart >= 0; iPart--)
      {
        GeoImageView part = _views[iPart];
        part.LoadImage(imagePrj, nx, ny, ref bitmap);
      }
      if (bitmap != null)
      {
        _currentWorldMatrix = imagePrj.Matrix;
        _currentImagePartExtent = imagePartExtent;
      }
      return bitmap;
    }
  }
  partial class GeoImageView : IGeoImageViewmodel
  {
    public Bitmap LoadImage(MatrixPrj worldPrj, int width, int height)
    {
      MatrixPrj imagePrj = BaseImage.GetImagePrj(worldPrj, width, height, out int nx, out int ny, out IBox imagePartExtent);

      Bitmap bitmap = null;
      LoadImage(imagePrj, nx, ny, ref bitmap);

      if (bitmap != null)
      {
        _currentWorldMatrix = imagePrj.Matrix;
        _currentImagePartExtent = imagePartExtent;
      }
      return bitmap;
    }

    float IGeoImageViewmodel.Transparency => Transparency;
    float? IGeoImageViewmodel.GrayTransform => Gray <= 0 ? (float?)null : Math.Min(1.0f, Gray / 100.0f);
    float? IGeoImageViewmodel.ColorRotation => ColorRotation <= 0 ? (float?)null : Math.Min(1.0f, ColorRotation / 100.0f);

    internal void LoadImage(MatrixPrj imagePrj, int nx, int ny, ref Bitmap bitmap)
    {
      using (Paint p = new Paint())
      {
        p.Color = Color.White;
        if (_colorTransform == null)
        {
          IGeoImageViewUtils.SetColor(p, this);
        }
        else
        {
          p.SetColorFilter(new ColorMatrixColorFilter(_colorTransform));
        }
        BaseImage.LoadImageParts(imagePrj, nx, ny, ref bitmap, p);
      }
    }
  }
  partial class GeoImage
  {
    public static Part InitPart(string path)
    {
      using (BitmapFactory.Options opts = new BitmapFactory.Options
      { InJustDecodeBounds = true })
      {
        BitmapFactory.DecodeFile(path, opts);

        Part part = new Part { Path = path, Height = opts.OutHeight, Width = opts.OutWidth };
        return part;
      }
    }

    public Bitmap LoadImageParts(MatrixPrj targetPrj, int width, int height,
      ref Bitmap bmp, Paint paint)
    {

      Pnt targetMax = new Pnt(width, height);
      double[] m = targetPrj.Matrix;

      double[] localMatrix = new double[] { m[0], -m[1], -m[2], m[3], 0, 0 };
      MatrixPrj localTargetPrj = new MatrixPrj(localMatrix);
      MatrixPrj invLocalTargetPrj = localTargetPrj.GetInverse();

      foreach (var part in Parts)
      {
        double[] p = part.WorldMatrix;
        double[] localPartMatrix = new double[] { p[0], -p[1], -p[2], p[3], p[4] - m[4], p[5] - m[5] };
        MatrixPrj partPrj = new MatrixPrj(localPartMatrix);
        MatrixPrj invPartPrj = partPrj.GetInverse();

        Curve targetBoxInPart = MatrixPrj.GetLocalBox(invPartPrj, targetMax, localTargetPrj);
        IBox b = targetBoxInPart.Extent;

        int x0 = Math.Max(0, (int)b.Min.X);
        int y0 = Math.Max(0, (int)b.Min.Y);
        int x1 = Math.Min(part.Width, (int)b.Max.X);
        int y1 = Math.Min(part.Height, (int)b.Max.Y);

        if (x0 >= x1 || y0 >= y1)
        { continue; }

        bmp = bmp ?? Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

        double[] o00 = invLocalTargetPrj.Project(partPrj.Project(0, 0));
        double[] o10 = invLocalTargetPrj.Project(partPrj.Project(1, 0));
        double[] o01 = invLocalTargetPrj.Project(partPrj.Project(0, 1));

        double[] partImg00 = invLocalTargetPrj.Project(partPrj.Project(x0, y0));

        using (Bitmap partImg = LoadImage(part.Path, x0, y0, x1, y1))
        using (Canvas canvas = new Canvas(bmp))
        using (Matrix partImageMatrix = new Matrix())
        {
          double f = (double)(x1 - x0) / partImg.Width;
          float x00 = (float)(f * (o10[0] - o00[0]));
          float x01 = (float)(f * (-(o10[1] - o00[1])));
          float x10 = (float)(f * (-(o01[0] - o00[0])));
          float x11 = (float)(f * (o01[1] - o00[1]));

          partImageMatrix.SetValues(new float[] {
            x00, x01, (float)partImg00[0],
            x10, x11, (float)partImg00[1],
            0, 0, 1 });

          canvas.DrawBitmap(partImg, partImageMatrix, paint);

        }
      }
      return bmp;
    }

    public static Bitmap LoadImage(string path)
    {
      Part part = InitPart(path);
      return LoadImage(part);
    }

    public static Bitmap LoadImage(Part part)
    {
      return LoadImage(part.Path, 0, 0, part.Width, part.Height);
    }
    public static Bitmap LoadImage(string path, int x0, int y0, int x1, int y1)
    {
      Bitmap img;

      if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
      { throw new InvalidOperationException($"Cannot find file '{path}'"); }

      int height = y1 - y0;
      int width = x1 - x0;

      int resample = 1;
      while (height * width / resample > MaxDim * MaxDim)
      {
        resample *= 2;
      }

      //if (resample > 1)
      //{ opts.InSampleSize = resample; }
      using (BitmapFactory.Options opts = new BitmapFactory.Options
      {
        InPreferredConfig = Bitmap.Config.Argb8888,
        InSampleSize = resample
      })
      using (BitmapRegionDecoder dec = BitmapRegionDecoder.NewInstance(path, isShareable: false))
      {
        Rect region = new Rect(x0, y0, x0 + width, y0 + height);
        try
        { img = img = dec.DecodeRegion(region, opts); }
        catch (Java.Lang.OutOfMemoryError e)
        { throw new Exception($"Use images smaller than 5000 x 5000 pixels (File:{path})", e); }
      }

      return img;
    }
  }
}