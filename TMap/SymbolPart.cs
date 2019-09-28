using Basics.Data;
using Basics.Geom;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;

namespace TMap
{
  public interface ISymbolPart
  {
    int Topology { get; }
    int DrawLevel { get; }
    double Size();
    string GetDrawExpressions();
    void Draw(IGeometry geometry, IDrawable drawable);

    Color Color { get; }

    object EditProperties { get; }
    void SetProperties(DataRow properties);
  }

  public interface ILineWidthPart
  {
    double LineWidth { get; }
  }
  public interface IScaleablePart
  {
    bool Scale { get; }
  }

  public enum SymbolType { Point, Line, Area };
  public enum SymbolPartType { Point, Line_Line, Line_Point };
  /// <summary>
  /// Base Class for SymbolPart.
  /// </summary>
  public abstract class SymbolPart : ISymbolPart
  {
    private Random _random = new Random();
    private double[] _dash;

    protected DataRow Properties { get; private set; }
    public void SetProperties(DataRow properties)
    {
      Properties = properties;
    }

    public object EditProperties
    {
      get { return this; }
    }

    public double DashOffset { get; set; }

    public bool DashAdjust { get; set; }

    public bool DirectPoints { get; set; }

    protected SymbolPart()
    {
      Color = Color.FromArgb(
        _random.Next(128) + 64,
        _random.Next(128) + 64,
        _random.Next(128) + 64);
    }

    protected T? GetValue<T>(DataColumn column, Func<object,T> convert)
      where T : struct
    {
      if (column == null)
      { return null; }

      object oValue = Properties[column];
      if (oValue == DBNull.Value)
      { return null; }

      T value = convert(oValue);
      return value;
    }

    protected DataColumn GetColumn(DataRow properties, ref DataColumn expressionColumn, string proposedName, string expression, Type dataType)
    {
      if (string.IsNullOrWhiteSpace(expression))
      { return null; }

      if (expressionColumn?.Table == properties?.Table)
      { return expressionColumn; }
      if (properties?.Table == null)
      { return null; }

      foreach (var col in properties.Table.Columns.Enum())
      {
        if (col.Expression == expression && col.DataType == dataType)
        {
          expressionColumn = col;
          return col;
        }
      }
      int i = 0;
      while (properties.Table.Columns.IndexOf($"{proposedName}{i}") >= 0)
      { i++; }

      expressionColumn = properties.Table.Columns.Add(proposedName, dataType, expression);
      return expressionColumn;
    }

    protected bool IsPointVisible(IPoint p, IDrawable drawable)
    {
      if (p == null)
      { return false; }
      return true; // TODO;
    }

    public abstract int Topology { get; }
    public abstract void Draw(IGeometry geometry, IDrawable drawable);
    public abstract string GetDrawExpressions();

    public int DrawLevel { get; set; }

    public int SplitDash(int i, double length)
    {
      int j;
      int iNDash;
      double[] newDash;

      if (_dash == null)
      { iNDash = 0; }
      else
      { iNDash = _dash.Length; }
      // check input
      if (length <= 0.0)
      {
        throw new InvalidOperationException(
          string.Format("Dash length ({0}) must be > 0 ", length));
      }
      if (i > iNDash)
      {
        throw new InvalidOperationException(
          string.Format("Out of range: {0} > {1}", i, iNDash));
      }
      if (i < iNDash && _dash[i] <= length)
      {
        throw new Exception(
          string.Format("length ({0}) > dashlength {0}", length, _dash[i]));
      }

      // assign data
      newDash = new double[iNDash + 1];
      _dash.CopyTo(newDash, 0);
      _dash = newDash;

      for (j = iNDash; j > i; j--)
      { _dash[j] = _dash[j - 1]; }
      _dash[i] = length;
      if (i < iNDash)
      { _dash[i + 1] -= length; }
      return iNDash + 1;
    }

    private string _colorExpression;
    private DataColumn _colorColumn;
    public string ColorExpression
    {
      get { return _colorExpression; }
      set
      {
        _colorExpression = value;
        _color = null;
        _colorColumn = null;
      }
    }
    private DataColumn LineColorColumn
    {
      get { return GetColumn(Properties, ref _colorColumn, "__Color__", ColorExpression, typeof(int)); }
    }

    private Color? _color;
    public Color Color
    {
      get
      {
        return _color ?? GetValue(LineColorColumn, (argb) => Color.FromArgb(Convert.ToInt32(argb))) ?? Color.Red;
      }
      set
      {
        _color = value;
        _colorExpression = null;
      }
    }

    [Browsable(false)]
    public object Tag { get; set; }

    public abstract double Size();
  }

  public class TTTExpandableObjectConverter : ExpandableObjectConverter
  {
  }

  public class TTTEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.Modal;
    }

    public override bool GetPaintValueSupported(ITypeDescriptorContext context)
    {
      return true;
    }
    public override void PaintValue(PaintValueEventArgs e)
    {
      e.Graphics.FillRectangle(new SolidBrush(Color.Red), e.Bounds);
    }
  }

  [Editor(typeof(TTTEditor), typeof(UITypeEditor))]
  [TypeConverter(typeof(TTTExpandableObjectConverter))]
  public class TTT
  {
    private int _x, _y, _z, _u, _v;
    public int X { get { return _x; } set { _x = value; } }
    public int Y { get { return _y; } set { _y = value; } }
    public int Z { get { return _z; } set { _z = value; } }
    public int U { get { return _u; } set { _u = value; } }
    public int V { get { return _v; } set { _v = value; } }
  }
}
