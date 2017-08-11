using Basics.Data;
using System.Data;

namespace TennisStat.Data
{
  public class SetTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }
    }

    public static readonly TypedColumn<int> IdSetColumn = new TypedColumn<int>("IdSet");
    public static readonly TypedColumn<int> FkMatchColumn = new TypedColumn<int>("FkMatch");
    public static readonly TypedColumn<int> SetNrColumn = new TypedColumn<int>("SetNr");
    public static readonly TypedColumn<int> GamesXColumn = new TypedColumn<int>("GamesX");
    public static readonly TypedColumn<int> GamesYColumn = new TypedColumn<int>("GamesY");
    public static readonly TypedColumn<int> TieBreakColumn = new TypedColumn<int>("TieBreak");
    public static readonly TypedColumn<string> SpezialColumn = new TypedColumn<string>("Spezial");

    public SetTable()
      : base("Set")
    {
      IdSetColumn.CreatePrimaryKey(this);
      Columns.Add(FkMatchColumn.CreateColumn());
      Columns.Add(SetNrColumn.CreateColumn());
      Columns.Add(GamesXColumn.CreateColumn());
      Columns.Add(GamesYColumn.CreateColumn());
      Columns.Add(TieBreakColumn.CreateColumn());
      Columns.Add(SpezialColumn.CreateColumn());
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
  }
}
