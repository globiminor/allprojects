using System.Data;
using Basics.Geom;

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
    private string _scaleExpression;
    private string _lineWidthExpression;
    private string _rotateExpression;


    public SymbolPartPoint(DataRow templateRow)
      : base(templateRow)
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

    private bool _scale;
    public bool Scale
    {
      get { return _scale; }
      set { _scale = value; }
    }

    public override void Draw(IGeometry geometry, DataRow properties, IDrawable drawable)
    {
      Point p = geometry as Point;
      if (drawable.Extent == null || IsPointVisible(p, drawable))
      {
        double dScale = 1;
        //if (_templateRow[_colScale] != System.DBNull.Value)
        //{ dScale = (double) _templateRow[_colScale]; }
        double dRotate = 0;
        //if (_templateRow[_colRotate] != System.DBNull.Value)
        //{ dRotate = (double) _templateRow[_colRotate]; }

        Draw(p, drawable, dScale, dRotate);
      }
    }

    public void Draw(Point p, IDrawable drawable,
                     double scale, double rotate)
    {
      _projection = new _Projection();
      _projection.Rotate(rotate);
      _projection.Scale(scale);
      drawable.DrawLine(PointLine(p, drawable), this);
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

    public string LineWidthExpression
    {
      get { return _lineWidthExpression; }
      set { _lineWidthExpression = value; }
    }

    public string ScaleExpression
    {
      get { return _scaleExpression; }
      set { _scaleExpression = value; }
    }
    //public DataColumn ScaleExprCol
    //{
    //  get { return _colScale; }
    //}

    public string RotateExpression
    {
      get { return _rotateExpression; }
      set { _rotateExpression = value; }
    }
    //public DataColumn RotateExprCol
    //{
    //  get { return _colRotate; }
    //}


    public Polyline PointLine(IPoint point, IDrawable drawable)
    {
      Polyline line = _line.Clone();
      Polyline drawLine;
      if (_scale)
      {
        line = line.Project(_projection);

        Basics.Geom.Projection.Translate trsPrj = new Basics.Geom.Projection.Translate(point);
        Polyline geomLine = line.Project(trsPrj);
        drawLine = geomLine.Project(drawable.Projection);
      }
      else
      {
        IPoint drawPoint = point.Project(drawable.Projection);
        Basics.Geom.Projection.Translate trsPrj = new Basics.Geom.Projection.Translate(drawPoint);
        drawLine = line.Project(trsPrj);
      }
      return drawLine;
    }

  }
}