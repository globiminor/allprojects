
using Android.Graphics;
using Android.Widget;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class MapView : ImageView
  {
    private readonly MainActivity _context;
    private float[] _elemMatrix;
    private Matrix _inversElemMatrix;
    private Matrix _initMatrix;

    public MapView(MainActivity context)
      : base(context)
    {
      _context = context;

      SetScaleType(ScaleType.Matrix);
    }

    public void Scale(float f)
    {
      Rect rect = new Rect();
      GetGlobalVisibleRect(rect);
      Scale(f, rect.Width() / 2, rect.Height() / 2);
    }

    public void ResetMap()
    {
      _elemMatrix = null;
      _inversElemMatrix = null;
    }

    private void ResetElemMatrix()
    {
      _elemMatrix = null;
      _inversElemMatrix = null;
    }

    private float[] ElemMatrix
    {
      get
      {
        if (_elemMatrix == null)
        {
          float[] matrix = new float[9];
          ImageMatrix.GetValues(matrix);
          _elemMatrix = matrix;
        }
        return _elemMatrix;
      }
    }
    private Matrix InversElemMatrix
    {
      get
      {
        if (_inversElemMatrix == null)
        {
          Matrix inverse = new Matrix();
          ImageMatrix.Invert(inverse);

          _inversElemMatrix = inverse;
        }
        return _inversElemMatrix;
      }
    }

    private float[] _lastWorldMatrix;
    public void PrepareUpdateMapImage(Map map)
    {
      _lastWorldMatrix = map.GetCurrentWorldMatrix();
    }

    public void UpdateMapImage(Map map)
    {
      ResetElemMatrix();

      SetScaleType(ScaleType.Matrix);
      SetImageBitmap(map.CurrentImage);

      if (_lastWorldMatrix != null)
      {
        float[] currentWorldMatrix = map.GetCurrentWorldMatrix();
        if (currentWorldMatrix[0] != _lastWorldMatrix[0] ||
          currentWorldMatrix[4] != _lastWorldMatrix[4] ||
          currentWorldMatrix[5] != _lastWorldMatrix[5])
        {
          float[] last = new float[] { 0, 0 };
          ImageMatrix.MapPoints(last);

          float[] current = new float[] {
            (currentWorldMatrix[4] - _lastWorldMatrix[4]) / _lastWorldMatrix[0],
            (currentWorldMatrix[5] - _lastWorldMatrix[5]) / _lastWorldMatrix[0]
          };
          ImageMatrix.MapPoints(current);

          float scale = currentWorldMatrix[0] / _lastWorldMatrix[0];
          float dx = current[0] - last[0];
          float dy = current[1] - last[1];
          ImageMatrix.PostTranslate(dx, dy);
          if (scale != 1)
          { Scale(scale, current[0], current[1]); }
          return;
        }
        if (_initMatrix != null)
        { return; }
      }

      if (_initMatrix == null)
      {
        Matrix m = new Matrix();
        m.PostTranslate(10, 10);
        _initMatrix = m;
      }
      ImageMatrix = _initMatrix;
    }

    public void AddPoint(Map map, Symbol symbol, ColorRef color, float x, float y)
    {
      //Matrix inverse = new Matrix();
      //ImageMatrix.Invert(inverse);
      //            float[] inverted = { x - rect.Left, y - rect.Top };
      float[] inverted = { x, y };
      InversElemMatrix.MapPoints(inverted);

      map.AddPoint(inverted[0], inverted[1], symbol, color);
    }
    public void Translate(float dx, float dy)
    {
      ResetElemMatrix();
      ImageMatrix.PostTranslate(dx, dy);

      PostInvalidate();
    }
    public void Scale(float scale, float centerX, float centerY)
    {
      ResetElemMatrix();

      float[] pre = new float[] { centerX, centerY };
      Matrix matrix = new Matrix();
      ImageMatrix.Invert(matrix);
      matrix.MapPoints(pre);

      ImageMatrix.PostScale(scale, scale);
      float[] post = new float[] { pre[0], pre[1] };
      ImageMatrix.MapPoints(post);

      ImageMatrix.PostTranslate(centerX - post[0], centerY - post[1]);

      PostInvalidate();
    }
    protected override void OnDraw(Canvas canvas)
    {
      base.OnDraw(canvas);
      DrawElems(canvas, _context.Map.Elems);
    }

    private void DrawElems(Canvas canvas, IEnumerable<Elem> elems)
    {
      if (elems == null)
      { return; }

      Paint p = new Paint();
      canvas.Save();
      try
      {
        float[] matrix = ElemMatrix;

        canvas.Translate(matrix[2], matrix[5]);
        canvas.Scale(matrix[0], matrix[0]);
        foreach (Elem elem in elems)
        {
          p.Color = elem.Color.Color;
          elem.Geometry.Draw(canvas, elem.Symbol, p);
        }
      }
      finally
      { canvas.Restore(); }
    }
  }
}