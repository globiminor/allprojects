using Android.Graphics;
using Android.OS;
using Basics;
using Java.IO;

namespace OMapScratch
{
  public partial interface ISegment
  {
    void Init(Path path, float[] matrix);
    void AppendTo(Path path, float[] matrix);
  }
  public interface ILocationAction
  {
    string WaitDescription { get; }
    string SetDescription { get; }
    void Action(Android.Locations.Location loc);
  }

  public partial interface IMapView
  {
    void SetNextLocationAction(ILocationAction action);
  }

  public partial interface IDrawable
  {
    void Draw(Canvas canvas, Symbol symbol, float[] matrix, float symbolScale, Paint paint);
  }

  public partial class MapVm
  {
    public event System.ComponentModel.CancelEventHandler ImageChanging;
    public event System.EventHandler ImageChanged;
    private Android.Locations.Location _currentWorldLocation;

    public Bitmap CurrentImage
    { get { return _map.CurrentImage; } }

    public Android.Locations.Location CurrentWorldLocation
    { get { return _currentWorldLocation; } }
    public void RefreshImage()
    {
      if (EventUtils.Cancel(this, ImageChanging))
      { return; }

      ImageChanged?.Invoke(this, null);
    }

    public System.Collections.Generic.IList<string> GetRecents()
    {
      string path;
      XmlRecents recents = GetRecents(out path, create: false);
      return recents?.Recents;
    }

    private XmlRecents GetRecents(out string recentPath, bool create)
    {
      File store = Environment.ExternalStorageDirectory;
      string path = store.AbsolutePath;
      string home = System.IO.Path.Combine(path, ".oscratch");
      if (!System.IO.Directory.Exists(home))
      {
        if (!create)
        {
          recentPath = null;
          return null;
        }
        System.IO.Directory.CreateDirectory(home);
      }

      recentPath = System.IO.Path.Combine(home, "recent.xml");
      XmlRecents recentList;
      if (System.IO.File.Exists(recentPath))
      {
        using (System.IO.TextReader r = new System.IO.StreamReader(recentPath))
        { Serializer.TryDeserialize(out recentList, r); }
      }
      else
      { return null; }

      return recentList;
    }

    public void SetRecent(string configPath)
    {
      string recentPath;
      XmlRecents recentList = GetRecents(out recentPath, create: true)
        ?? new XmlRecents { Recents = new System.Collections.Generic.List<string>() }; ;

      recentList.Recents.Insert(0, configPath);
      for (int iPos = recentList.Recents.Count - 1; iPos > 0; iPos--)
      {
        if (recentList.Recents[iPos].Equals(configPath, System.StringComparison.InvariantCultureIgnoreCase))
        { recentList.Recents.RemoveAt(iPos); }
      }
      while (recentList.Recents.Count > 5)
      { recentList.Recents.RemoveAt(recentList.Recents.Count - 1); }

      using (System.IO.TextWriter w = new System.IO.StreamWriter(recentPath))
      { Serializer.Serialize(recentList, w); }
    }

    public void SetCurrentLocation(Android.Locations.Location location)
    {
      _currentWorldLocation = location;
      if (location == null)
      {
        _currentLocalLocation = null;
        return;
      }
      SetCurrentLocation(location.Latitude, location.Longitude, location.Altitude, location.Accuracy);
    }

    public void SynchLocation(Pnt mapCoord, Android.Locations.Location location)
    {
      _map.SynchLocation(mapCoord, location.Latitude, location.Longitude, location.Altitude, location.Accuracy);
    }

    public void LoadLocalImage(int imageIndex)
    {
      if (_map.Images?.Count <= imageIndex)
      { return; }

      string path = _map.Images[imageIndex].Path;
      string valid = _map.VerifyLocalPath(path);
      if (string.IsNullOrEmpty(valid))
      { throw new System.InvalidOperationException($"Cannot find file '{path}' (Index {imageIndex})"); }
      LoadImage(valid);
    }
    public void LoadLocalImage(string path)
    {
      LoadImage(_map.VerifyLocalPath(path));
    }

    private void LoadImage(string path)
    {
      if (string.IsNullOrEmpty(path))
      { return; }

      if (EventUtils.Cancel(this, ImageChanging))
      { return; }

      _map.LoadImage(path);

      ImageChanged?.Invoke(this, null);
    }
  }

  public partial class Map
  {
    private Bitmap _currentImage;

    public Bitmap CurrentImage { get { return _currentImage; } }

    public Matrix ImageMatrix { get; set; }

    public void LoadImage(string path)
    {
      //BitmapFactory.Options bounds = new BitmapFactory.Options();
      //bounds.InJustDecodeBounds = true;
      //BitmapFactory.DecodeFile(path, bounds);
      //int width = bounds.OutWidth;
      //int height = bounds.OutHeight;
      //long size = height * width;
      //const long max = 4096 * 4096;
      //int resample = 1;
      //while (size > max)
      //{
      //  size /= 4;
      //  resample *= 2;
      //}

      //using (BitmapRegionDecoder decoder = BitmapRegionDecoder.NewInstance(path, isShareable: false))
      //{
      //}

      BitmapFactory.Options opts = new BitmapFactory.Options();
      opts.InPreferredConfig = Bitmap.Config.Argb8888;
      //if (resample > 1)
      //{ opts.InSampleSize = resample; }

      Bitmap img;
      try
      { img = BitmapFactory.DecodeFile(path, opts); }
      catch (Java.Lang.OutOfMemoryError e)
      { throw new System.Exception("Use images smaller than 5000 x 5000 pixels", e); }

      _currentImage?.Dispose();
      _currentImage = img;
      _currentImagePath = path;
    }

    public static void Deserialize<T>(string path, out T obj)
    {
      using (System.IO.TextReader r = new System.IO.StreamReader(path))
      { Serializer.Deserialize(out obj, r); }
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

  partial class Circle
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
      path.AddArc(r, 90 - Azi / (float)System.Math.PI * 180, (float)(-Ang / System.Math.PI) * 180);
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
    private class Dash : System.IDisposable
    {
      private readonly Paint _p;
      private DashPathEffect _dash;
      private readonly PathEffect _orig;
      public Dash(SymbolCurve sym, float scale, Paint p, Curve line)
      {
        _p = p;
        if (sym.Dash == null)
        { return; }

        if (sym.Dash.EndOffset != 0 && line.Count > 0)
        {
          using (Path path = GetPath(line, null))
          {
            using (PathMeasure m = new PathMeasure(path, false))
            {
              float l = m.Length;
              scale = (float)(scale * sym.Dash.GetFactor(l));
            }
          }
        }

        _orig = p.PathEffect;
        int n = sym.Dash.Intervals.Length;
        float[] dash = new float[n];
        for (int i = 0; i < n; i++)
        { dash[i] = sym.Dash.Intervals[i] * scale; }

        _dash = new DashPathEffect(dash, sym.Dash.StartOffset * scale);
        p.SetPathEffect(_dash);
      }
      public void Dispose()
      {
        if (_dash != null)
        {
          _p.SetPathEffect(_orig);
          _dash.Dispose();
          _dash = null;
        }
      }
    }
    public static void DrawLine(Canvas canvas, Symbol symbol, float[] matrix, float symbolScale, Curve line, Paint p)
    {
      float lineScale = matrix?[0] * symbolScale ?? 1;
      foreach (SymbolCurve sym in symbol.Curves)
      {
        if (sym.Curve == null)
        {
          using (new Dash(sym, lineScale, p, line))
          {
            DrawCurve(canvas, line, matrix, sym.LineWidth * lineScale, sym.Fill, sym.Stroke, p);
          }
        }
        else if (sym.Dash != null && line.Count > 0)
        {
          using (Path path = GetPath(line, null))
          {
            using (PathMeasure m = new PathMeasure(path, false))
            {
              float l = m.Length;

              Symbol pntSym = new Symbol { Curves = new System.Collections.Generic.List<SymbolCurve> { sym }, };
              OMapScratch.Dash scaled = matrix != null ? sym.Dash.Scale(symbolScale) : sym.Dash;

              foreach (float dist in scaled.GetPositions(l))
              {
                if (dist > l)
                { break; }

                float[] pos = new float[2];
                float[] tan = new float[2];
                m.GetPosTan(dist, pos, tan);

                DirectedPnt pnt = new DirectedPnt { X = pos[0], Y = pos[1], Azimuth = (float)(System.Math.Atan2(tan[0], tan[1]) + System.Math.PI / 2) };
                DrawPoint(canvas, pntSym, matrix, symbolScale, pnt, p);
              }
            }
          }
        }
      }
    }

    private class ScalePrj : IProjection
    {
      private readonly float _scale;

      public ScalePrj(float scale)
      { _scale = scale; }

      public Pnt Project(Pnt pnt)
      {
        return new Pnt(pnt.X * _scale, pnt.Y * _scale);
      }
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
        canvas.Scale(1, -1);

        float? azi = (point as DirectedPnt)?.Azimuth;
        if (azi != null)
        { canvas.Rotate(azi.Value * 180 / (float)System.Math.PI); }

        ScalePrj prj = new ScalePrj(pntScale);
        foreach (SymbolCurve curve in sym.Curves)
        {
          DrawCurve(canvas, curve.Curve.Project(prj), null, curve.LineWidth * pntScale, curve.Fill, curve.Stroke, p);
        }
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

    public static Path GetPath(Curve curve, float[] matrix = null)
    {
      Path path = new Path();
      curve[0].Init(path, matrix);
      foreach (ISegment segment in curve)
      { segment.AppendTo(path, matrix); }
      return path;
    }

    public static void DrawCurve(Canvas canvas, Curve curve, float[] matrix, float lineWidth, bool fill, bool stroke, Paint p)
    {
      p.StrokeWidth = lineWidth;

      if (curve.Count == 0)
      {
        Pnt pt = curve.From.Trans(matrix);
        canvas.DrawLine(pt.X - p.StrokeWidth, pt.Y, pt.X + p.StrokeWidth, pt.Y, p);
        return;
      }

      using (Path path = GetPath(curve, matrix))
      {
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

}