
using Android.Graphics;

namespace OMapScratch.Views
{
  public class Graphics : IGraphics<GraphicsPaint>
  {
    private readonly Canvas _canvas;
    public Graphics(Canvas canvas)
    {
      _canvas = canvas;
    }
    public Canvas Canvas => _canvas;

    public void Save()
    { _canvas.Save(); }
    public void Restore()
    { _canvas.Restore(); }

    public GraphicsPaint CreatePaint()
    {
      return new GraphicsPaint();
    }

    public static Path GetPath(Curve curve)
    {
      Path path = new Path();
      curve[0].Init(path);
      foreach (var segment in curve.Segments)
      { segment.AppendTo(path); }
      return path;
    }

    public void Translate(float dx, float dy) => _canvas.Translate(dx, dy);
    public void Scale(float fx, float fy) => _canvas.Scale(fx, fy);
    public void Rotate(float rad)
      => _canvas.Rotate(rad * 180 / (float)System.Math.PI);

    public void DrawText(string text, float x0, float y0, GraphicsPaint p)
      => _canvas.DrawText(text, x0, y0, p.Paint);

    public void DrawPath(Curve curve, IProjection toLocal, GraphicsPaint p)
    {
      Curve local = toLocal != null ? curve.Project(toLocal) : curve;
      using (Path path = GetPath(local))
      {
        _canvas.DrawPath(path, p.Paint);
      }
    }

    IPathMeasure IGraphics<GraphicsPaint>.GetPathMeasure(Curve line)
    {
      return new GraphicsPathMeasure(line);
    }
  }

  public class GraphicsPaint : IPaint
  {
    private Paint _paint;
    private readonly bool _dispose;

    public GraphicsPaint()
    {
      _paint = new Paint();
      _dispose = true;
    }
    public GraphicsPaint(Paint paint)
    {
      _paint = paint;
      _dispose = false;
    }

    public void Dispose()
    {
      if (_dispose)
      { _paint?.Dispose(); }
      _paint = null;
    }
    public Paint Paint => _paint;

    public float TextSize
    {
      get { return _paint.TextSize; }
      set { _paint.TextSize = value; }
    }
    public void TextAlignSetCenter()
    {
      _paint.TextAlign = Paint.Align.Center;
    }

    public ColorRef Color { set { _paint.Color = value?.Color ?? MapView.DefaultColor; } }
    public float StrokeWidth {
      get { return _paint.StrokeWidth; }
      set { _paint.StrokeWidth = value; }
    }

    public void SetStyle(bool fill, bool stroke)
    {
      if (fill && stroke)
      { _paint.SetStyle(Paint.Style.FillAndStroke); }
      else if (fill)
      { _paint.SetStyle(Paint.Style.Fill); }
      else if (stroke)
      { _paint.SetStyle(Paint.Style.Stroke); }
      else
      { _paint.SetStyle(Paint.Style.Fill); }

    }

    private PathEffect _orig;
    private PathEffect _effect;
    public void SetDashEffect(float[] dash, float offset)
    {
      _orig = _paint.PathEffect;

      _effect?.Dispose();
      _effect = new DashPathEffect(dash, offset);
      _paint.SetPathEffect(_effect);
    }
    public void ResetPathEffect()
    {
      _paint.SetPathEffect(_orig);
      _effect?.Dispose();
      _effect = null;
    }
  }

  public class GraphicsPathMeasure : IPathMeasure
  {
    private Path _path;
    private PathMeasure _measure;
    public GraphicsPathMeasure(Curve line)
    {
      if (line.Count > 0)
      {
        _path = Graphics.GetPath(line);
        _measure = new PathMeasure(_path, forceClosed: false);
      }
    }

    public float Length => _measure?.Length ?? 0;
    public Lin GetTangentAt(float dist)
    {
      if (_measure == null)
        return null;

      float[] pos = new float[2];
      float[] tan = new float[2];
      _measure.GetPosTan(dist, pos, tan);

      return new Lin(new Pnt(pos[0], pos[1]), new Pnt(pos[0] + tan[0], pos[1] + tan[1]));
    }

    public void Dispose()
    {
      _measure?.Dispose();
      _measure = null;

      _path?.Dispose();
      _path = null;
    }
  }
}