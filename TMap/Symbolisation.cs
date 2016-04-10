using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using D = System.Drawing;
using System.Text;

namespace TMap
{
  public abstract class Symbolisation
  {
  }
  public abstract class GridSymbolisation : Symbolisation
  {
    public abstract Color? Color(double value);
  }
  public class RangeSymbolisation : GridSymbolisation
  {
    private double _rangeMin;
    private double _rangeMax;
    private double? _transparent;
    private Color _minColor = D.Color.Black;
    private Color _maxColor = D.Color.White;
    private bool _cyclic;

    public double RangeMin
    {
      get { return _rangeMin; }
      set { _rangeMin = value; }
    }
    public double RangeMax
    {
      get { return _rangeMax; }
      set { _rangeMax = value; }
    }
    public double? Transparent
    {
      get { return _transparent; }
      set { _transparent = value; }
    }
    public bool Cyclic
    {
      get { return _cyclic; }
      set { _cyclic = value; }
    }

    public override Color? Color(double value)
    {
      if (value == _transparent)
      { return null; }

      value = (value - _rangeMin) / (_rangeMax - _rangeMin);
      if (_cyclic)
      {
        value = value - Math.Truncate(value);
        if (value < 0)
        { value += 1; }
      }
      if (value < 0 || value > 1)
      { return null; }

      return D.Color.FromArgb(
        (int)(_minColor.R + value * (_maxColor.R - _minColor.R)),
        (int)(_minColor.G + value * (_maxColor.G - _minColor.G)),
        (int)(_minColor.B + value * (_maxColor.B - _minColor.B)));
    }
  }
  public class LookupSymbolisation : GridSymbolisation
  {
    private List<Color?> _colors = new List<Color?>(
      new Color?[]
      { 
        D.Color.Cyan, D.Color.Red, D.Color.Yellow, D.Color.Green,
        D.Color.Magenta, D.Color.Blue, null, null 
      });
    private bool _cyclic;
    private int _transparency = 255;

    public override Color? Color(double value)
    {
      int nColor = _colors.Count;
      int iVal = (int)Math.Floor(value);
      if (_cyclic)
      {
        iVal = iVal % nColor;
        if (iVal < 0)
        { iVal += nColor; }
      }
      if (iVal < 0 || iVal >= nColor)
      { return null; }
      return _colors[iVal];
    }

    public int Transparency
    {
      get { return _transparency; }
      set
      {
        _transparency = value;
        int nColors = _colors.Count;
        List<Color?> newColors = new List<Color?>(nColors);
        for (int iColor = 0; iColor < nColors; iColor++)
        {
          Color? c = _colors[iColor];
          if (c.HasValue && c.Value.A != _transparency)
          {
            Color cc = c.Value;
            newColors.Add(D.Color.FromArgb(_transparency, cc.R, cc.G, cc.B));
          }
        }
        _colors = newColors;
      }
    }
    public bool Cyclic
    {
      get { return _cyclic; }
      set { _cyclic = value; }
    }
    public List<D.Color?> Colors
    {
      get { return _colors; }
    }
  }
  public interface ISymbolisationView
  {
    DataView SymbolList { get;}
  }
  /// <summary>
  /// Summary description for Symobisation.
  /// </summary>
  public class VectorSymbolisation : Symbolisation, ISymbolisationView
  {
    private DataColumn _geomColumn;
    private SymbolTable _symTable;
    private DataView _symbolView;
    private SymbolTable.Row _symRow;

    private string _neededFields;
    private IList<DataColumn> _neededColumns;

    public VectorSymbolisation(DataColumn geomColumn)
    {
      _geomColumn = geomColumn;
      DataRow templateRow = _geomColumn.Table.NewRow();

      _symTable = new SymbolTable();

      string sCol = geomColumn.ColumnName + " is ";
      _symRow = _symTable.AddRow(sCol + "Point");
      _symRow.Symbol = Symbol.DefaultPoint(templateRow);

      _symRow = _symTable.AddRow(sCol + "Polyline");
      _symRow.Symbol = Symbol.Create(templateRow, 1);

      _symRow = _symTable.AddRow(sCol + "Area");
      _symRow.Symbol = Symbol.Create(templateRow, 2);

      _symbolView = new DataView(_symTable);
      _symbolView.Sort = SymbolTable.PositionColumn.Name;

      DataTable templateTable = Basics.Data.Utils.GetTemplateTable(geomColumn.Table);
      _templateRow = templateTable.NewRow();
      templateTable.Rows.Add(_templateRow);

      _symTable.RowChanged += _symbolList_RowChanged;
    }

    private bool _suspendRowChanged;
    private void _symbolList_RowChanged(object sender, DataRowChangeEventArgs e)
    {
      if (_suspendRowChanged) return;
      try
      {
        _suspendRowChanged = true;

        _neededFields = null;
        _neededColumns = null;

        SymbolTable.Row r = (SymbolTable.Row)e.Row;
        if (!r.IsNull(SymbolTable.ViewColumn.Name)) r.View.RowFilter = null;
      }
      finally
      {
        _suspendRowChanged = false;
      }
    }

    public IList<DataColumn> NeededColumns
    {
      get
      {
        if (_neededColumns == null)
        {
          DataTable schema = _geomColumn.Table;

          StringBuilder fullQuery = new StringBuilder(GeometryColumn.ColumnName);
          foreach (DataRowView vRow in SymbolList)
          {
            SymbolTable.Row row = (SymbolTable.Row)vRow.Row;
            string condition = row.Condition;
            fullQuery.AppendFormat(" {0}", condition);
          }

          if (fullQuery.Length == 0) return null;

          string queryWithNeededFields = fullQuery.ToString().ToUpper();
          List<DataColumn> fields = new List<DataColumn>();
          foreach (DataColumn column in schema.Columns)
          {
            if (queryWithNeededFields.Contains(column.ColumnName.ToUpper()))
            { fields.Add(column); }
          }
          _neededColumns = fields;
        }
        return _neededColumns;
      }
    }

    public string NeededFields
    {
      get
      {
        if (_neededFields == null)
        {
          IList<DataColumn> columns = NeededColumns;

          StringBuilder sbFields = new StringBuilder();
          if (columns == null)
          {
            sbFields.Append("*");
          }
          else
          {
            foreach (DataColumn field in columns)
            {
              if (sbFields.Length > 0) sbFields.Append(", ");
              sbFields.Append(field.ColumnName);
            }
          }
          _neededFields = sbFields.ToString();
        }
        return _neededFields;
      }
    }

    public int Add(Symbol symbol, string condition)
    {
      int n = _symTable.Rows.Count;
      _symTable.AddRow(symbol, condition, n);
      return n;
    }

    public DataColumn GeometryColumn
    {
      get { return _geomColumn; }
      set
      {
        if (_geomColumn.Table != value.Table)
        {
          throw new Exception(string.Format(
            "invalid column : {0} does not belong to same table as {1}",
            value.ColumnName, _geomColumn.ColumnName));
        }
        _geomColumn = value;
      }
    }

    private DataRow _templateRow;
    public Symbol GetSymbol(DbDataReader reader, out DataRow dataRow)
    {
      foreach (DataColumn column in NeededColumns)
      {
        int idx = reader.GetOrdinal(column.ColumnName);
        _templateRow[column.ColumnName] = reader.GetValue(idx);
      }

      dataRow = _templateRow;
      //if (_dict == null)
      //{
      //  _dict = new Dictionary<int, Symbol>();
      //  foreach (DataRowView symView in _vSymbolList)
      //  {
      //    DsSymbol.SymbolRow rowSymbol = (DsSymbol.SymbolRow)symView.Row;
      //    string[] parts = rowSymbol.Condition.Split();
      //    int s;
      //    if (parts.Length == 3 && int.TryParse(parts[2], out s))
      //    {
      //      _dict.Add(s, (Symbol)rowSymbol["Symbol"]);
      //    }
      //  }
      //}
      //int x = (int) _templateRow["Symbol"];
      //Symbol sym;
      //if (_dict.TryGetValue(x, out sym))
      //{
      //  return sym;
      //}
      //return null;


      foreach (DataRowView symView in SymbolList)
      {
        SymbolTable.Row rowSymbol = (SymbolTable.Row)symView.Row;
        if (rowSymbol.FulFillsCondition(_templateRow, _geomColumn))
        {
          return rowSymbol.Symbol;
        }
      }
      return null;
    }

    public DataView SymbolList
    {
      get { return _symbolView; }
    }
  }
}
