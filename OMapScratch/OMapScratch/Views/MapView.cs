
using Android.Graphics;
using Android.Widget;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class MapView : ImageView
  {
    private readonly MainActivity _context;
    public MapView(MainActivity context)
      : base(context)
    {
      _context = context;
    }

    public void Scale(float f)
    {
      Rect rect = new Rect();
      GetGlobalVisibleRect(rect);
      Scale(f, rect.Width() / 2, rect.Height() / 2);
    }
    public void Scale(float scale, float centerX, float centerY)
    {
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
        float[] matrix = new float[9];
        ImageMatrix.GetValues(matrix);
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