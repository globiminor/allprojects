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
    private Color? _constrClr;
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

    public Color ConstrColor
    { get { return _constrClr ?? Color.Black; } }

    private float? _constrLineWidth;
    public float ConstrLineWidth
    {
      get { return _constrLineWidth ?? (_constrLineWidth = 0.1f * Utils.GetMmPixel(this)).Value; }
    }

    IMapView ViewModels.IConstrView.MapView
    { get { return _parent; } }

    void ViewModels.IConstrView.SetConstrColor(ColorRef color)
    { _constrClr = color?.Color; }


    public bool IsInStartArea(Pnt p)
    {
      if (p == null)
      { return false; }

      Pnt compassPnt = ViewModel.GetCompassPoint();
      if (compassPnt == null)
      { return false; }

      IProjection prj = new Translation(_parent.ElemMatrixValues);

      Pnt mapCompass = compassPnt.Project(prj);
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
      wp.StrokeWidth = 3 * ConstrLineWidth;
      wp.SetStyle(Paint.Style.Stroke);
      Paint bp = new Paint();
      bp.Color = ConstrColor;
      bp.StrokeWidth = ConstrLineWidth;
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

      DrawOrientation(canvas, ViewModel.GetOrientation(), declination, wp, bp);

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
      SymbolUtils.DrawCurve(canvas, displayGeom, null, wp.StrokeWidth, false, true, wp);
      SymbolUtils.DrawCurve(canvas, displayGeom, null, bp.StrokeWidth, false, true, bp);
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
        double cos = System.Math.Cos((angle + declination) / 180 * System.Math.PI);

        float dy = (float)(l * sin);
        float dx = (float)(l * cos);

        DrawCurve(canvas, new Curve().MoveTo(center.X - dx, center.Y - dy).LineTo(center.X - 0.3f * dx, center.Y - 0.3f * dy), wp, bp);
        DrawCurve(canvas, new Curve().MoveTo(center.X + dx, center.Y + dy).LineTo(center.X + 0.3f * dx, center.Y + 0.3f * dy), wp, bp);
      }
    }

    private float? _orientationAngle;
    private float GetOrientationAngle()
    {
      return (_orientationAngle ?? (_orientationAngle = Utils.GetSurfaceOrientation()).Value);
    }

    private void DrawOrientation(Canvas canvas, float? orientation, double? declination, Paint wp, Paint bp)
    {
      if (orientation == null || _parent.HideCompass)
      {
        _orientationAngle = null;
        return;
      }

      float orientationAngle = GetOrientationAngle();

      double azimuth = -(orientationAngle + orientation.Value) / 180 * System.Math.PI;
      float mm = Utils.GetMmPixel(this);

      float x0 = 15 * mm;
      {
        float sin = (float)System.Math.Sin(azimuth);
        float cos = (float)System.Math.Cos(azimuth);

        Paint red = new Paint();
        red.Color = Color.Red;
        red.SetStyle(Paint.Style.Fill);
        red.StrokeWidth = mm;

        float b = 0.5f * mm;
        float l = 10 * mm;
        Curve c = new Curve().MoveTo(x0 + sin * b, x0 - cos * b).LineTo(x0 + l * cos, x0 + l * sin).LineTo(x0 - sin * b, x0 + cos * b);
        canvas.DrawPath(SymbolUtils.GetPath(c), red);
      }

      if (declination == null)
      { declination = _parent.MapVm.GetDeclination(); }
      if (declination != null)
      {
        double decl = declination.Value / 180 * System.Math.PI;
        double sin = System.Math.Sin(decl);
        double cos = System.Math.Cos(decl);

        float l = 12 * mm;

        float dx = (float)(l * sin);
        float dy = -(float)(l * cos);

        DrawCurve(canvas, new Curve().MoveTo(x0, x0).LineTo(x0 + dx, x0 + dy), wp, bp);
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