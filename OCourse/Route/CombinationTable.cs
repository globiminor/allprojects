using System.Data;
using Basics.Data;
using OCourse.Ext;
using System.Collections.Generic;
using Ocad;

namespace OCourse.Route
{
  public class CostMean : CostBase, ICostSectionlists
  {
    public CostMean(CostBase cost, List<CostSectionlist> group)
      :base(cost)
    { Group = group; }
    public List<CostSectionlist> Group { get; private set; }

    IEnumerable<CostSectionlist> ICostSectionlists.Costs
    { get { return Group; } }

  }

  public class CombinationTableX : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public string Name
      {
        get { return NameColumn.GetValue(this); }
        set { NameColumn.SetValue(this, value); }
      }

      public double DirectLKm
      {
        get { return DirectLKmColumn.GetValue(this); }
        set { DirectLKmColumn.SetValue(this, value); }
      }

      public double DirectKm
      {
        get { return DirectKmColumn.GetValue(this); }
        set { DirectKmColumn.SetValue(this, value); }
      }

      public double Climb
      {
        get { return ClimbColumn.GetValue(this); }
        set { ClimbColumn.SetValue(this, value); }
      }

      public double OptimalLKm
      {
        get { return OptimalLKmColumn.GetValue(this); }
        set { OptimalLKmColumn.SetValue(this, value); }
      }

      public double OptimalKm
      {
        get { return OptimalKmColumn.GetValue(this); }
        set { OptimalKmColumn.SetValue(this, value); }
      }

      public double OptimalCost
      {
        get { return OptimalCostColumn.GetValue(this); }
        set { OptimalCostColumn.SetValue(this, value); }
      }

      public SectionList Combination
      {
        get { return CombinationColumn.GetValue(this); }
        set { CombinationColumn.SetValue(this, value); }
      }

      public List<Row> SectionInfos
      {
        get { return SectionInfosColumn.GetValue(this); }
        set { SectionInfosColumn.SetValue(this, value); }
      }

      public new CombinationTableX Table
      {
        get { return (CombinationTableX)base.Table; }
      }

    }

    public static readonly TypedColumn<string> NameColumn = new TypedColumn<string>("Name");
    public static readonly TypedColumn<double> DirectLKmColumn = new TypedColumn<double>("DirectLKm");
    public static readonly TypedColumn<double> DirectKmColumn = new TypedColumn<double>("DirectKm");
    public static readonly TypedColumn<double> ClimbColumn = new TypedColumn<double>("Climb");
    public static readonly TypedColumn<double> OptimalLKmColumn = new TypedColumn<double>("OptimalLKm");
    public static readonly TypedColumn<double> OptimalKmColumn = new TypedColumn<double>("OptimalKm");
    public static readonly TypedColumn<double> OptimalCostColumn = new TypedColumn<double>("OptimalCost");
    public static readonly TypedColumn<SectionList> CombinationColumn = new TypedColumn<SectionList>("Combination");
    public static readonly TypedColumn<List<Row>> SectionInfosColumn = new TypedColumn<List<Row>>("SectionInfos");


    public CombinationTableX()
      : base("Combination")
    {
      Columns.Add(NameColumn.CreateColumn());
      Columns.Add(DirectLKmColumn.CreateColumn());
      Columns.Add(DirectKmColumn.CreateColumn());
      Columns.Add(ClimbColumn.CreateColumn());
      Columns.Add(OptimalLKmColumn.CreateColumn());
      Columns.Add(OptimalKmColumn.CreateColumn());
      Columns.Add(OptimalCostColumn.CreateColumn());
      Columns.Add(CombinationColumn.CreateColumn());
      Columns.Add(SectionInfosColumn.CreateColumn());
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

    public Row AddRow(CostBase leg, string info, SectionList combination)
    {
      return AddRow(
        info, (leg.Direct + 10 * leg.Climb) / 1000.0, leg.Direct / 1000.0,
        leg.Climb, (leg.OptimalLength + 10.0 * leg.Climb) / 1000.0, leg.OptimalLength / 1000.0,
        leg.OptimalCost, combination);
    }

    public Row AddRow(string Name, double DirectLKm, double DirectKm, double Climb,
      double OptimalLKm, double OptimalKm, double OptimalCost, SectionList Combination)
    {
      Row row = NewRow();
      row.Name = Name;
      row.DirectLKm = DirectLKm;
      row.DirectKm = DirectKm;
      row.Climb = Climb;
      row.OptimalLKm = OptimalLKm;
      row.OptimalKm = OptimalKm;
      row.OptimalCost = OptimalCost;
      row.Combination = Combination;
      Rows.Add(row);
      return row;
    }
  }
}
