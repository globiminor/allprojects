using Android.Graphics;
using Android.OS;

namespace OMapScratch
{
  public partial interface ISegment
  {
    void Init(Path path, float[] matrix);
    void AppendTo(Path path, float[] matrix);
  }

  public partial interface IDrawable
  {
    void Draw(Canvas canvas, Symbol symbol, float[] matrix, float symbolScale, Paint paint);
  }

  public partial class MapVm
  {
    public event System.ComponentModel.CancelEventHandler ImageChanging;
    public event System.EventHandler ImageChanged;

    public Bitmap CurrentImage
    { get { return _map.CurrentImage; } }

    public void RefreshImage()
    {
      if (Utils.Cancel(this, ImageChanging))
      { return; }

      ImageChanged?.Invoke(this, null);
    }

    public void SetCurrentLocation(Android.Locations.Location location)
    {
      if (location == null)
      {
        _currentLocalLocation = null;
        return;
      }
      SetCurrentLocation(location.Latitude, location.Longitude, location.Altitude, location.Accuracy);
    }

    public void LoadLocalImage(int imageIndex)
    {
      if (_map.Images?.Count <= imageIndex)
      { return; }

      string path = _map.Images[imageIndex].Path;
      LoadImage(_map.VerifyLocalPath(path));
    }
    public void LoadLocalImage(string path)
    {
      LoadImage(_map.VerifyLocalPath(path));
    }

    private void LoadImage(string path)
    {
      if (string.IsNullOrEmpty(path))
      { return; }

      if (Utils.Cancel(this, ImageChanging))
      { return; }

      _map.LoadImage(path);

      ImageChanged?.Invoke(this, null);
    }
  }

  public partial class Map
  {
    private Bitmap _currentImage;

    public Bitmap CurrentImage { get { return _currentImage; } }

    public void LoadImage(string path)
    {
      BitmapFactory.Options opts = new BitmapFactory.Options();
      opts.InPreferredConfig = Bitmap.Config.Argb8888;
      Bitmap img = BitmapFactory.DecodeFile(path, opts);
      _currentImage?.Dispose();
      _currentImage = img;
      _currentImagePath = path;
    }

    public System.Collections.Generic.List<ColorRef> GetDefaultColors()
    {
      return new System.Collections.Generic.List<ColorRef>
      {
        new ColorRef { Id = "Bl", Color = Color.Black },
        new ColorRef { Id = "Gy", Color = Color.Gray },
        new ColorRef { Id = "Bw", Color = Color.Brown },
        new ColorRef { Id = "Y", Color = Color.Yellow },
        new ColorRef { Id = "G", Color = Color.Green },
        new ColorRef { Id = "K", Color = Color.Khaki },
        new ColorRef { Id = "R", Color = Color.Red },
        new ColorRef { Id = "B", Color = Color.Blue } };
    }
  }

  public partial class XmlColor
  {
    private void GetEnvColor(ColorRef color)
    {
      color.Color = new Color { A = byte.MaxValue, R = Red, G = Green, B = Blue };
    }
    private void SetEnvColor(ColorRef color)
    {
      Red = color.Color.R;
      Green = color.Color.G;
      Blue = color.Color.B;
    }
  }

  public partial class ColorRef
  {
    public Color Color { get; set; }
  }

  public partial class Pnt
  {
    void IDrawable.Draw(Canvas canvas, Symbol symbol, float[] matrix, float symbolScale, Paint paint)
    {
      SymbolUtils.DrawPoint(canvas, symbol, matrix, symbolScale, this, paint);
    }
  }

  public partial class Lin
  {
    void ISegment.Init(Path path, float[] matrix)
    {
      Pnt t = From.Trans(matrix);
      path.MoveTo(t.X, t.Y);
    }

    void ISegment.AppendTo(Path path, float[] matrix)
    {
      Pnt t = To.Trans(matrix);
      path.LineTo(t.X, t.Y);
    }
  }

  public partial class Circle
  {
    void ISegment.Init(Path path, float[] matrix)
    { }
    void ISegment.AppendTo(Path path, float[] matrix)
    {
      Pnt lt = new Pnt(Center.X - Radius, Center.Y - Radius).Trans(matrix);
      Pnt rb = new Pnt(Center.X + Radius, Center.Y + Radius).Trans(matrix);
      RectF r = new RectF
      {
        Left = lt.X,
        Right = rb.X,
        Top = lt.Y,
        Bottom = rb.Y,
      };
      path.AddArc(r, 0, 360);
    }
  }

  public partial class Bezier
  {
    void ISegment.Init(Path path, float[] matrix)
    {
      Pnt t = From.Trans(matrix);
      path.MoveTo(t.X, t.Y);
    }
    void ISegment.AppendTo(Path path, float[] matrix)
    {
      Pnt i0 = I0.Trans(matrix);
      Pnt i1 = I1.Trans(matrix);
      Pnt to = To.Trans(matrix);
      path.CubicTo(i0.X, i0.Y, i1.X, i1.Y, to.X, to.Y);
    }
  }

  public partial class Curve
  {
    void IDrawable.Draw(Canvas canvas, Symbol symbol, float[] matrix, float symbolScale, Paint paint)
    {
      SymbolUtils.DrawLine(canvas, symbol, matrix, symbolScale, this, paint);
    }
  }

  public static class SymbolUtils
  {
    public static void DrawLine(Canvas canvas, Symbol sym, float[] matrix, float symbolScale, Curve line, Paint p)
    {
      float lineScale = matrix?[0] * symbolScale ?? 1;
      foreach (SymbolCurve curve in sym.Curves)
      { DrawCurve(canvas, line, matrix, curve.LineWidth * lineScale, curve.Fill, curve.Stroke, p); }
    }

    public static void DrawPoint(Canvas canvas, Symbol sym, float[] matrix, float symbolScale, Pnt point, Paint p)
    {
      if (!string.IsNullOrEmpty(sym.Text))
      {
        DrawText(canvas, sym.Text, matrix, symbolScale, point, p);
        return;
      }
      canvas.Save();
      try
      {
        Pnt t = point.Trans(matrix);
        canvas.Translate(t.X, t.Y);
        float pntScale = 1;
        if (matrix != null)
        {
          pntScale = matrix[0] * symbolScale;
        }
        canvas.Scale(pntScale, -pntScale);
        foreach (SymbolCurve curve in sym.Curves)
        { DrawCurve(canvas, curve.Curve, null, curve.LineWidth, curve.Fill, curve.Stroke, p); }
      }
      finally
      { canvas.Restore(); }
    }

    public static void DrawText(Canvas canvas, string text, float[] matrix, float symbolScale, Pnt point, Paint p)
    {
      canvas.Save();
      try
      {
        p.SetStyle(Paint.Style.Stroke);
        p.StrokeWidth = 0;
        p.TextAlign = Paint.Align.Center;

        Pnt t = point.Trans(matrix);
        canvas.Translate(t.X, t.Y);
        if (matrix != null)
        { canvas.Scale(matrix[0] * symbolScale, matrix[4] * symbolScale); }
        canvas.DrawText(text, 0, 0 + p.TextSize / 2.5f, p);
      }
      finally
      { canvas.Restore(); }
    }

    public static void DrawCurve(Canvas canvas, Curve curve, float[] matrix, float lineWidth, bool fill, bool stroke, Paint p)
    {
      if (curve.Count == 0)
      {
        Pnt pt = curve.From;
        canvas.DrawPoint(pt.X, pt.Y, p);
        return;
      }

      Path path = new Path();
      curve[0].Init(path, matrix);
      foreach (ISegment segment in curve)
      { segment.AppendTo(path, matrix); }

      p.StrokeWidth = lineWidth;
      if (fill && stroke)
      { p.SetStyle(Paint.Style.FillAndStroke); }
      else if (fill)
      { p.SetStyle(Paint.Style.Fill); }
      else if (lineWidth > 0)
      { p.SetStyle(Paint.Style.Stroke); }
      else
      { p.SetStyle(Paint.Style.Fill); }

      canvas.DrawPath(path, p);
    }
  }

}