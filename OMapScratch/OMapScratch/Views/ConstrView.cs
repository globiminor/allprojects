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

    public bool IsInStartArea(Pnt p)
    {
      if (p == null)
      { return false; }

      Pnt compassPnt = ViewModel.GetCompassPoint();
      if (compassPnt == null)
      { return false; }

      IProjection prj = new Translation(_parent.ElemMatrixValues);

      Pnt mapCompass =compassPnt.Project(prj);
      Pnt mapP = p.Project(prj);

      double dx = mapP.X - mapCompass.X;
      double dy = mapP.Y - mapCompass.Y;

      double l2 = dx * dx + dy * dy;
      double mm = Utils.GetMmPixel(this);
      return l2 < mm * mm * _compassRadius * _compassRadius;
    }

    protected override void OnDraw(Canvas canvas)
    {
      base.OnDraw(canvas);

      DrawCurrentLocation(canvas, _parent.GetCurrentMapLocation());

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

        DrawCurve(canvas, displayGeom, wp, bp);
      }

      double? declination = ViewModel.GetDeclination();
      if (declination != null)
      {
        prj = prj ?? new Translation(_parent.ElemMatrixValues);
        DrawCompass(canvas, ViewModel.GetCompassPoint().Project(prj), declination.Value, wp, bp);
      }


      bool first = true;
      foreach (Elem textElem in ViewModel.GetTexts())
      {
        if (first)
        {
          wp.SetStyle(Paint.Style.Fill);

          bp.TextAlign = Paint.Align.Center;
          bp.TextSize = 1.6f * Utils.GetMmPixel(this);
          bp.SetStyle(Paint.Style.Fill);
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

    private void DrawCurve(Canvas canvas, Curve displayGeom, Paint wp, Paint bp)
    {
      SymbolUtils.DrawCurve(canvas, displayGeom, null, 3, false, true, wp);
      SymbolUtils.DrawCurve(canvas, displayGeom, null, 1, false, true, bp);
    }

    private static float _compassRadius = 8;
    private void DrawCompass(Canvas canvas, Pnt center, double declination, Paint wp, Paint bp)
    {
      float mm = Utils.GetMmPixel(this);
      DrawCurve(canvas, new Curve().Circle(center.X, center.Y, _compassRadius * mm), wp, bp);

      double l = 16 * mm;
      foreach (double angle in new List<double> { 0, 90 })
      {
        double sin = System.Math.Sin((angle + declination) / 180 * System.Math.PI);

        float dy = (float)(l * sin);
        float dx = (float)System.Math.Sqrt(l * l - dy * dy);

        DrawCurve(canvas, new Curve().MoveTo(center.X - dx, center.Y - dy).LineTo(center.X - 0.3f * dx, center.Y - 0.3f * dy), wp, bp);
        DrawCurve(canvas, new Curve().MoveTo(center.X + dx, center.Y + dy).LineTo(center.X + 0.3f * dx, center.Y + 0.3f * dy), wp, bp);
      }
    }

    private void DrawCurrentLocation(Canvas canvas, Pnt p)
    {
      if (p == null)
      { return; }

      float mm = Utils.GetMmPixel(this);

      Paint red = new Paint();
      red.Color = new Color(255, 0, 0, 128);
      red.StrokeWidth = 0.3f * mm;
      red.SetStyle(Paint.Style.Stroke);

      float r = 2.5f * mm;
      canvas.DrawCircle(p.X, p.Y, r, red);

      canvas.DrawLine(p.X - r, p.Y, p.X - 1.8f * r, p.Y, red);
      canvas.DrawLine(p.X + r, p.Y, p.X + 1.8f * r, p.Y, red);

      canvas.DrawLine(p.X, p.Y - r, p.X, p.Y - 1.8f * r, red);
      canvas.DrawLine(p.X, p.Y + r, p.X, p.Y + 1.8f * r, red);

    }
  }
}