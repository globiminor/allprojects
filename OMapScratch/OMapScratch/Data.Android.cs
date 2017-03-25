using Android.Graphics;
using Android.OS;

namespace OMapScratch
{
  public partial interface ISegment
  {
    void Init(Path path);
    void AppendTo(Path path);
  }

  public partial interface IDrawable
  {
    void Draw(Canvas canvas, Symbol symbol, Paint paint);
  }

  public partial class Map
  {
    public event System.ComponentModel.CancelEventHandler ImageChanging;
    public event System.EventHandler ImageChanged;

    private Bitmap _currentImage;

    public void LoadDefaultImage()
    {
      Java.IO.File store = Environment.ExternalStorageDirectory;
      string initPath = store.AbsolutePath;
      string path = $"{initPath}/Pictures/fadacher_2017.jpg";
      if (!System.IO.File.Exists(path))
      { return; }
      LoadImage(path);
    }

    public Bitmap CurrentImage { get { return _currentImage; } }

    public void LoadLocalImage(string path)
    {
      LoadImage(VerifyLocalPath(path));
    }

    private void LoadImage(string path)
    {
      if (string.IsNullOrEmpty(path))
      { return; }

      if (Utils.Cancel(this, ImageChanging))
      { return; }

      BitmapFactory.Options opts = new BitmapFactory.Options();
      opts.InPreferredConfig = Bitmap.Config.Argb8888;
      Bitmap img = BitmapFactory.DecodeFile(path, opts);
      _currentImage?.Dispose();
      _currentImage = img;
      _currentImagePath = path;

      ImageChanged?.Invoke(this, null);
    }

    private System.Collections.Generic.List<ColorRef> GetDefaultColors()
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
    void IDrawable.Draw(Canvas canvas, Symbol symbol, Paint paint)
    {
      SymbolUtils.DrawPoint(canvas, symbol, this, paint);
    }
  }

  public partial class Lin
  {
    void ISegment.Init(Path path)
    {
      path.MoveTo(From.X, From.Y);
    }

    void ISegment.AppendTo(Path path)
    {
      path.LineTo(To.X, To.Y);
    }
  }

  public partial class Circle
  {
    void ISegment.Init(Path path)
    { }
    void ISegment.AppendTo(Path path)
    {
      RectF r = new RectF
      {
        Left = Center.X - Radius,
        Right = Center.X + Radius,
        Top = Center.Y - Radius,
        Bottom = Center.X + Radius,
      };
      path.AddArc(r, 0, 360);
    }
  }

  public partial class Bezier
  {
    void ISegment.Init(Path path)
    {
      path.MoveTo(From.X, From.Y);
    }
    void ISegment.AppendTo(Path path)
    {
      path.CubicTo(I0.X, I0.Y, I1.X, I1.Y, To.X, To.Y);
    }
  }

  public partial class Curve
  {
    void IDrawable.Draw(Canvas canvas, Symbol symbol, Paint paint)
    {
      SymbolUtils.DrawLine(canvas, symbol, this, paint);
    }
  }

  public static class SymbolUtils
  {
    public static void DrawLine(Canvas canvas, Symbol sym, Curve line, Paint p)
    {
      foreach (SymbolCurve curve in sym.Curves)
      { DrawCurve(canvas, line, curve.LineWidth, curve.Fill, curve.Stroke, p); }
    }

    public static void DrawPoint(Canvas canvas, Symbol sym, Pnt point, Paint p)
    {
      canvas.Save();
      try
      {
        canvas.Translate(point.X, point.Y);
        foreach (SymbolCurve curve in sym.Curves)
        { DrawCurve(canvas, curve.Curve, curve.LineWidth, curve.Fill, curve.Stroke, p); }
      }
      finally
      { canvas.Restore(); }
    }
    private static void DrawCurve(Canvas canvas, Curve curve, float lineWidth, bool fill, bool stroke, Paint p)
    {
      if (curve.Count == 0)
      {
        Pnt pt = curve.From;
        canvas.DrawPoint(pt.X, pt.Y, p);
        return;
      }

      Path path = new Path();
      curve[0].Init(path);
      foreach (ISegment segment in curve)
      { segment.AppendTo(path); }

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