using System.Data;
using Basics.Data;
using Basics.Geom;

namespace Grid.Lcp
{
  public class RouteTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public Polyline Route
      {
        get { return RouteColumn.GetValue(this); }
        set { RouteColumn.SetValue(this, value); }
      }

      public double Delay
      {
        get { return DelayColumn.GetValue(this); }
        set { DelayColumn.SetValue(this, value); }
      }
      public double TotalCost
      {
        get { return TotalCostColumn.GetValue(this); }
        set { TotalCostColumn.SetValue(this, value); }
      }

      public new RouteTable Table
      {
        get { return (RouteTable)base.Table; }
      }

    }

    public static readonly TypedColumn<Polyline> RouteColumn = new TypedColumn<Polyline>("Route");
    public static readonly TypedColumn<double> TotalCostColumn = new TypedColumn<double>("TotalCost");
    public static readonly TypedColumn<double> DelayColumn = new TypedColumn<double>("Delay");

    public RouteTable()
      : base("Route")
    {
      Columns.Add(RouteColumn.CreateColumn());
      Columns.Add(TotalCostColumn.CreateColumn());
      Columns.Add(DelayColumn.CreateColumn());
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

    public Row AddRow(Polyline route, double totalCost, double delay)
    {
      Row row = NewRow();
      row.Route = route;
      row.TotalCost = totalCost;
      row.Delay = delay;
      Rows.Add(row);
      return row;
    }
  }
}
