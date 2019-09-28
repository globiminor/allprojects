using Basics.Geom;
using System.Data;

namespace TMap
{
  public class SymbolPartPoint : SymbolPart, ILineWidthPart, IScaleablePart
  {
    #region nested classes
    private class _Projection : IProjection
    {
      #region IProjection Members
      double _sin;
      double _cos;
      public _Projection()
      {
        _cos = 1;
        _sin = 0;
      }

      public void Scale(double factor)
      {
        _cos *= factor;
        _sin *= factor;
      }

      public void Rotate(double angle)
      {
        angle = System.Math.Atan2(_sin, _cos) - angle;
        double f = System.Math.Sqrt(_sin * _sin + _cos * _cos);
        _cos = f * System.Math.Cos(angle);
        _sin = f * System.Math.Sin(angle);
      }

      public IPoint Project(IPoint point)
      {
        return new Point2D(_cos * point.X + _sin * point.Y,
                           -_sin * point.X + _cos * point.Y);
      }

      #endregion
    }
    #endregion

    // member variables
    private double _lineWidth;
    private Polyline _line;
    //private DataColumn _colLineWidth;
    //private DataColumn _colScale;
    //private DataColumn _colRotate;
    private _Projection _projection;

    public SymbolPartPoint()
    {
      DrawLevel = 0;
    }

    public double LineWidth
    {
      get { return _lineWidth; }
      set { _lineWidth = value; }
    }

    public override int Topology
    {
      get { return 0; }
    }

    public bool Scale { get; set; }

    public override string GetDrawExpressions()
    {
      return $"{ColorExpression} {RotateExpression} {ScaleExpression}";
    }
    public override void Draw(IGeometry geometry, IDrawable drawable)
    {
      IPoint p = geometry as IPoint;
      if (drawable.Extent == null || IsPointVisible(p, drawable))
      {
        double dScale = 1;
        if (!string.IsNullOrWhiteSpace(_scaleExpression))
        {
          GetColumn(Properties, ref _scaleCol, "__scale__", _scaleExpression, typeof(double));
          object oScale = Properties[_scaleCol];
          if (oScale != System.DBNull.Value)
          { dScale = System.Convert.ToDouble(oScale); }

        }

        //if (_templateRow[_colScale] != System.DBNull.Value)
        //{ dScale = (double) _templateRow[_colScale]; }

        double dRotate = 0;
        if (!string.IsNullOrWhiteSpace(_rotateExpression))
        {
          GetColumn(Properties, ref _rotateCol, "__rotate__", _rotateExpression, typeof(double));
          object oRotate = Properties[_rotateCol];
          if (oRotate != System.DBNull.Value)
          { dRotate = System.Convert.ToDouble(oRotate); }
        }

        Draw(p, drawable, dScale, dRotate);
      }
    }

    public void Draw(IPoint p, IDrawable drawable,
                     double scale, double rotate)
    {
      _projection = new _Projection();
      _projection.Rotate(rotate);
      _projection.Scale(scale);
      Polyline l = PointLine(p, drawable);
      if (Fill)
      { drawable.DrawArea(new Area(l), this); }
      if (Stroke)
      { drawable.DrawLine(l, this); }
    }


    public override double Size()
    {
      double dSize = 0;
      if (_line != null)
      {
        Box box = new Box(_line.Extent);
        if (box.Max.X < 0)
        { box.Max.X = 0; }
        if (box.Max.Y < 0)
        { box.Max.Y = 0; }
        if (box.Min.X > 0)
        { box.Min.X = 0; }
        if (box.Min.Y > 0)
        { box.Min.Y = 0; }
        dSize = box.GetMaxExtent();
      }
      dSize += _lineWidth;
      return dSize;
    }


    public Polyline SymbolLine
    {
      get { return _line; }
      set { _line = value; }
    }

    private string _rotateExpression { get; set; }
    public string RotateExpression
    {
      get => _rotateExpression;
      set
      {
        _rotateExpression = value;
        _rotateCol = null;
      }
    }
    private DataColumn _rotateCol;

    private string _scaleExpression { get; set; }
    public string ScaleExpression
    {
      get => _scaleExpression;
      set
      {
        _scaleExpression = value;
        _scaleCol = null;
      }
    }
    private DataColumn _scaleCol;

    public bool Fill { get; set; }
    public bool Stroke { get; set; } = true;

    public Polyline PointLine(IPoint point, IDrawable drawable)
    {
      Polyline line = _line.Clone();
      Polyline drawLine;
      if (Scale)
      {
        line = line.Project(_projection);

        Basics.Geom.Projection.Translate trsPrj = new Basics.Geom.Projection.Translate(point);
        Polyline geomLine = line.Project(trsPrj);
        drawLine = geomLine.Project(drawable.Projection);
      }
      else
      {
        IPoint drawPoint = PointOp.Project(point, drawable.Projection);
        Basics.Geom.Projection.Translate trsPrj = new Basics.Geom.Projection.Translate(drawPoint);
        drawLine = line.Project(trsPrj);
      }
      return drawLine;
    }

  }
}