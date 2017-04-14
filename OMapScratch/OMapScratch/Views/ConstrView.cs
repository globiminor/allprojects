using Android.Graphics;
using Android.Views;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class ConstrView : View, ViewModels.IConstrView
  {
    private class Translation : IProjection
    {
      private readonly float[] _matrix;
      public Translation(float[] matrix)
      { _matrix = matrix; }
      public Pnt Project(Pnt p)
      { return new Pnt(_matrix[0] * p.X + _matrix[2], _matrix[4] * p.Y + _matrix[5]); }
    }

    private readonly MapView _parent;

    private ViewModels.ConstrVm _constrVm;
    public ConstrView(MapView parent)
      : base(parent.Context)
    {
      _parent = parent;
      parent.ConstrView = this;
    }

    public ViewModels.ConstrVm ViewModel
    {
      get { return _constrVm ?? (_constrVm = new ViewModels.ConstrVm(this)); }
    }

    IMapView ViewModels.IConstrView.MapView
    { get { return _parent; } }

    protected override void OnDraw(Canvas canvas)
    {
      base.OnDraw(canvas);

      Paint wp = new Paint();
      wp.Color = Color.White;
      wp.StrokeWidth = 3;
      wp.SetStyle(Paint.Style.Stroke);
      Paint bp = new Paint();
      bp.Color = Color.Black;
      bp.StrokeWidth = 1;
      bp.SetStyle(Paint.Style.Stroke);

      IProjection prj = null;
      foreach (Curve geom in ViewModel.GetGeometries())
      {
        prj = prj ?? new Translation(_parent.ElemMatrixValues);
        Curve displayGeom = geom.Project(prj);

        SymbolUtils.DrawCurve(canvas, displayGeom, null, 3, false, true, wp);
        SymbolUtils.DrawCurve(canvas, displayGeom, null, 1, false, true, bp);
      }

      bool first = true;
      foreach (Elem textElem in ViewModel.GetTexts())
      {
        if (first)
        {
          wp.SetStyle(Paint.Style.Fill);

          bp.TextAlign = Paint.Align.Center;
          bp.TextSize = 1.6f * Utils.GetMmPixel(this);
          first = false;
        }
        prj = prj ?? new Translation(_parent.ElemMatrixValues);
        Pnt pos = (Pnt)textElem.Geometry;
        Pnt t = pos.Project(prj);

        float width = bp.MeasureText(textElem.Symbol.Text);
        canvas.DrawRect(t.X - 0.6f * width, t.Y - 1.0f * bp.TextSize, t.X + 0.6f * width, t.Y + 0.2f * bp.TextSize, wp);
        canvas.DrawText(textElem.Symbol.Text, t.X, t.Y, bp);
      }
    }
  }
}