using System;
using Basics.Geom;

namespace Ocad
{
  public enum WorkMode
  {
    freehand = 5,
    straight = 6,
    rectangular = 7,
    circle = 8,
    ellipse = 9,
    curve = 10,
    editPoint = 11,
    editObject = 12
  }

  public enum LineMode
  {
    freehand = 5,
    straight = 6,
    rectangular = 7,
    circle = 8,
    ellipse = 9,
    curve = 10
  }

  public enum EditMode
  {
    editPoint = 11,
    editObject = 12
  }

  public enum GeomType
  {
    point = 1,
    line = 2,
    area = 3,
    unformattedText = 4,
    formattedText = 5,
    lineText = 6,
    rectangle = 7,

    undefined = 0
  }

  public enum ObjectStringType
  {
    None = 0,
    // Cs = CourseSetting
    CsObject = 1,
    CsPreview = 2,
    CsLayout = 3,
    CsThematic = 4
  }
  /// <summary>
  /// Summary description for Setup.
  /// </summary>
  public class Setup
  {
    private class MyMap2Prj : IProjection
    {
      private Setup _setup;

      public MyMap2Prj(Setup setup)
      {
        _setup = setup;
      }

      public IPoint Project(IPoint point)
      {
        double dX = point.X * _setup._cos + point.Y * _setup._sin + _setup._prjTrans.X;
        double dY = -point.X * _setup._sin + point.Y * _setup._cos + _setup._prjTrans.Y;

        return new Point2D(dX, dY);
      }
    }

    private class MyPrj2Map : IProjection
    {
      private Setup _setup;

      public MyPrj2Map(Setup setup)
      {
        _setup = setup;
      }

      public IPoint Project(IPoint point)
      {
        double dx = point.X - _setup._prjTrans.X;
        double dy = point.Y - _setup._prjTrans.Y;

        double xPrj = (dx * _setup._cos - dy * _setup._sin) / _setup._factor;
        double yPrj = (dx * _setup._sin + dy * _setup._cos) / _setup._factor;

        if (point is Coord.CodePoint cp)
        { return new Coord.CodePoint(xPrj, yPrj) { Flags = cp.Flags }; };

        return new Point2D(xPrj, yPrj);
      }
    }

    private Point2D _offset;
    private WorkMode _workMode;
    private LineMode _lineMode;
    private EditMode _editMode;
    private int _symbol;
    private double _gridDist;

    private double _prjRotation;
    private double _scale = 1;
    private double _prjGrid;
    private Point2D _prjTrans;

    private double _cos;
    private double _sin;
    private double _factor = 1;

    public Setup()
    {
      _offset = new Point2D();
      _prjTrans = new Point2D();
    }

    public double Scale
    {
      get
      { return _scale; }
      set
      {
        _scale = value;
        _factor = FileParam.OCAD_UNIT * _scale;
        _factor *= _factor;
        InitPrj();
      }
    }

    public Point Offset
    {
      get { return _offset; }
    }

    public double GridDistance
    {
      get
      { return _gridDist; }
      set
      { _gridDist = value; }
    }

    public WorkMode WorkMode
    {
      get
      { return _workMode; }
      set
      { _workMode = value; }
    }

    public LineMode LineMode
    {
      get { return _lineMode; }
      set { _lineMode = value; }
    }

    public EditMode EditMode
    {
      get { return _editMode; }
      set { _editMode = value; }
    }

    public int Symbol
    {
      get { return _symbol; }
      set { _symbol = value; }
    }

    public double PrjRotation
    {
      get { return _prjRotation; }
      set
      {
        _prjRotation = value;
        InitPrj();
      }
    }

    public double PrjGrid
    {
      get { return _prjGrid; }
      set { _prjGrid = value; }
    }

    public Point PrjTrans
    {
      get { return _prjTrans; }
    }

    private void InitPrj()
    {
      _cos = _scale * Math.Cos(_prjRotation) * FileParam.OCAD_UNIT;
      _sin = _scale * Math.Sin(_prjRotation) * FileParam.OCAD_UNIT;
    }

    public IProjection Map2Prj
    {
      get { return new MyMap2Prj(this); }
    }

    public IProjection Prj2Map
    {
      get { return new MyPrj2Map(this); }
    }

    public override string ToString()
    {
      return string.Format(
        "Offset X  : {0,9}" + Environment.NewLine +
        "Offset Y  : {1,9}" + Environment.NewLine +
        "Map Scale : {2,9:0.0}" + Environment.NewLine + Environment.NewLine +
        "Grid Distance : {3,9:0.0}" + Environment.NewLine + Environment.NewLine +
        "Work mode : {4}" + Environment.NewLine +
        "Line mode : {5}" + Environment.NewLine +
        "Edit mode : {6}" + Environment.NewLine +
        "Actual Symbol : {7,5}" + Environment.NewLine + Environment.NewLine +
        "Trans X to projected co-ord : {8,12:0.000}" + Environment.NewLine +
        "Trans Y to projected co-ord : {9,12:0.000}" + Environment.NewLine +
        "Rotion to projected co-ord  : {10,12:0.000}" + Environment.NewLine +
        "Projected grid              : {11,12:0.000}",
        _offset.X, _offset.Y, _scale,
        _gridDist, _workMode, _lineMode, _editMode, _symbol,
        _prjTrans.X, _prjTrans.Y, _prjRotation, _prjGrid);
    }
  }
}
