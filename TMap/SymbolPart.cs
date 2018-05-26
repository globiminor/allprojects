using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using Basics.Geom;

namespace TMap
{
  public interface ISymbolPart
  {
    int Topology { get; }
    int DrawLevel { get; }
    double Size();
    DataRow TemplateRow { get; }
    void Draw(IGeometry geometry, DataRow row, IDrawable drawable);

    Color LineColor { get; }

    object EditProperties { get;}
  }

  public interface ILineWidthPart
  {
    double LineWidth { get;}
  }
  public interface IScaleablePart
  {
    bool Scale { get;}
  }

  public enum SymbolType { Point, Line, Area };
  public enum SymbolPartType { Point, Line_Line, Line_Point };
  /// <summary>
  /// Base Class for SymbolPart.
  /// </summary>
  public abstract class SymbolPart : ISymbolPart
  {
    private Random _random = new Random();
    // member variables
    protected DataTable _templateTable;
    protected DataRow _templateRow;
    private double[] _dash;

    [Browsable(false)]
    public DataRow TemplateRow => _templateRow;

    public object EditProperties
    {
      get { return this; }
    }

    public double DashOffset { get; set; }

    public bool DashAdjust { get; set; }

    public bool DirectPoints { get; set; }

    protected SymbolPart(DataRow templateRow)
    {
      if (templateRow != null)
      {
        _templateTable = Basics.Data.Utils.GetTemplateTable(templateRow.Table);
        _templateRow = _templateTable.NewRow();
        _templateTable.Rows.Add(_templateRow);
      }
      LineColor = Color.FromArgb(
        _random.Next(128) + 64,
        _random.Next(128) + 64,
        _random.Next(128) + 64);
    }

    protected DataColumn AddColumn(string proposedName, string value)
    {
      DataColumn col = _templateTable.Columns.Add(proposedName, typeof(double), value);
      return col;
    }

    protected bool IsPointVisible(Basics.Geom.Point p, IDrawable drawable)
    {
      if (p == null)
      { return false; }
      return true; // TODO;
    }

    public abstract int Topology { get; }
    public abstract void Draw(IGeometry geometry, DataRow properties, IDrawable drawable);

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

    public Color LineColor { get; set; }

    public Color FillColor { get; } = Color.Black;

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
