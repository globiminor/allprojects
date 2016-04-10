using System.Collections.Generic;
using System.Data;
using Basics.Data;

namespace TMap
{

  public class SymbolTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public string Condition
      {
        get { return ConditionColumn.GetValue(this); }
        set { ConditionColumn.SetValue(this, value); }
      }

      public int Position
      {
        get { return PositionColumn.GetValue(this); }
        set { PositionColumn.SetValue(this, value); }
      }
      public Symbol Symbol
      {
        get { return SymbolColumn.GetValue(this); }
        set { SymbolColumn.SetValue(this, value); }
      }
      public DataView View
      {
        get { return ViewColumn.GetValue(this); }
        set { ViewColumn.SetValue(this, value); }
      }

      private List<System.Type> _geomTypes;

      public new SymbolTable Table
      {
        get { return (SymbolTable)base.Table; }
      }
      public bool FulFillsCondition(DataRow dataRow, DataColumn geomColumn)
      {
        if (IsNull(ViewColumn.Name) || View == null) View = new DataView(dataRow.Table);

        DataView conditionView = View;
        if (string.IsNullOrEmpty(conditionView.RowFilter))
        {
          InitConditionView(conditionView, Condition, geomColumn);
        }
        bool ret = false;
        if (conditionView.Count == 1)
        {
          ret = true;
        }
        if (ret && _geomTypes != null)
        {
          ret = false;
          System.Type geomType = dataRow[geomColumn.ColumnName].GetType();
          foreach (System.Type t in _geomTypes)
          {
            if (t.IsAssignableFrom(geomType) || t == geomType)
            {
              ret = true;
              break;
            }
          }
        }
        return ret;
      }

      public void InitConditionView(DataView conditionView, string condition, DataColumn geomColumn)
      {
        _geomTypes = null;
        string[] bracketExpressions = condition.Split('(', ')');
        foreach (string bracketExpression in bracketExpressions)
        {
          string[] expressions = bracketExpression.Replace(" AND ", "|").Replace(" OR ", "|").Split('|');
          foreach (string expression in expressions)
          {
            List<System.Type> geomType = null;

            string[] geomTerms = expression.Split();
            int i = 0;
            bool geomCondition = true;
            foreach (string geomTerm in geomTerms)
            {
              if (string.IsNullOrEmpty(geomTerm))
              {
                continue;
              }
              if (i == 0 && geomTerm.Equals(geomColumn.ColumnName,
                System.StringComparison.InvariantCultureIgnoreCase))
              { }
              else if (i == 1 && geomTerm.Equals("is",
                System.StringComparison.InvariantCultureIgnoreCase))
              {
                geomType = new List<System.Type>();
              }
              else if (i == 2 && geomTerm.Equals("Point",
                System.StringComparison.InvariantCultureIgnoreCase))
              {
                geomType.Add(typeof(Basics.Geom.IPoint));
              }
              else if (i == 2 && geomTerm.Equals("Polyline",
                System.StringComparison.InvariantCultureIgnoreCase))
              {
                geomType.Add(typeof(Basics.Geom.Polyline));
                geomType.Add(typeof(Basics.Geom.Curve));
              }
              else if (i == 2 && geomTerm.Equals("Area",
                System.StringComparison.InvariantCultureIgnoreCase))
              {
                geomType.Add(typeof(Basics.Geom.Area));
              }
              else
              {
                geomCondition = false;
                break;
              }
              i++;
            }
            if (geomCondition)
            {
              condition = condition.Replace(expression, "1=1");
              if (_geomTypes == null)
              {
                _geomTypes = new List<System.Type>(geomType);
              }
              else
              {
                _geomTypes.AddRange(geomType);
              }
            }
          }
        }

        conditionView.RowFilter = condition;
      }

    }

    public static readonly TypedColumn<string> ConditionColumn = new TypedColumn<string>("Condition");
    public static readonly TypedColumn<int> PositionColumn = new TypedColumn<int>("Position");
    public static readonly TypedColumn<Symbol> SymbolColumn = new TypedColumn<Symbol>("Symbol");
    public static readonly TypedColumn<DataView> ViewColumn = new TypedColumn<DataView>("View");

    public SymbolTable()
      : base("Symbol")
    {
      Columns.Add(ConditionColumn.CreateColumn());
      Columns.Add(PositionColumn.CreateColumn());
      Columns.Add(SymbolColumn.CreateColumn());
      Columns.Add(ViewColumn.CreateColumn());
    }

    public new Row NewRow()
    {
      return (Row)base.NewRow();
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      Row row = new Row(builder);
      return row;
    }

    public void AddRow(Row row)
    {
      Rows.Add(row);
    }

    public Row AddRow(string condition)
    {
      Row row = NewRow();
      row.Condition = condition;
      row.Position = Rows.Count;
      AddRow(row);

      return row;
    }
    public Row AddRow(Symbol symbol, string condition, int position)
    {
      Row row = NewRow();
      row.Condition = condition;
      row.Position = position;
      row.Symbol = symbol;
      AddRow(row);

      return row;
    }

  }

  public class SymbolPartTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public int Index
      {
        get { return IndexColumn.GetValue(this); }
        set { IndexColumn.SetValue(this, value); }
      }

      public ISymbolPart Part
      {
        get { return PartColumn.GetValue(this); }
        set { PartColumn.SetValue(this, value); }
      }

      public ISymbolPart Type
      {
        get { return TypeColumn.GetValue(this); }
        set { TypeColumn.SetValue(this, value); }
      }

      public new SymbolPartTable Table
      {
        get { return (SymbolPartTable)base.Table; }
      }
    }

    public static readonly TypedColumn<int> IndexColumn = new TypedColumn<int>("Index");
    public static readonly TypedColumn<ISymbolPart> PartColumn = new TypedColumn<ISymbolPart>("Part");
    public static readonly TypedColumn<ISymbolPart> TypeColumn = new TypedColumn<ISymbolPart>("Type");

    public SymbolPartTable()
      : base("SymbolPart")
    {
      Columns.Add(IndexColumn.CreateColumn());
      Columns.Add(PartColumn.CreateColumn());
      Columns.Add(TypeColumn.CreateColumn());
    }

    public new Row NewRow()
    {
      return (Row)base.NewRow();
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      Row row = new Row(builder);
      return row;
    }

    public void AddRow(Row row)
    {
      Rows.Add(row);
    }

    public Row AddRow(int index, ISymbolPart part)
    {
      Row row = NewRow();
      row.Index = index;
      row.Part = part;
      AddRow(row);

      return row;
    }
  }

}
