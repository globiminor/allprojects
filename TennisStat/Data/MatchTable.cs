using Basics.Data;
using System.Data;

namespace TennisStat.Data
{
  public class MatchFullTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public int FkPlayerX
      {
        get { return FkPlayerXColumn.GetValue(this); }
        set { FkPlayerXColumn.SetValue(this, value); }
      }

      public int FkPlayerY
      {
        get { return FkPlayerYColumn.GetValue(this); }
        set { FkPlayerYColumn.SetValue(this, value); }
      }
    }

    public static readonly TypedColumn<int> IdMatchColumn = new TypedColumn<int>("IdMatch");
    public static readonly TypedColumn<int> FkEventColumn = new TypedColumn<int>("FkEvent");
    public static readonly TypedColumn<string> RoundColumn = new TypedColumn<string>("Round");
    public static readonly TypedColumn<int> FkPlayerXColumn = new TypedColumn<int>("FkPlayerX");
    public static readonly TypedColumn<int> FkPlayerYColumn = new TypedColumn<int>("FkPlayerY");

    public static readonly TypedColumn<string> RefXColumn = new TypedColumn<string>("RefX");
    public static readonly TypedColumn<string> NameXColumn = new TypedColumn<string>("NameX");
    public static readonly TypedColumn<string> NatXColumn = new TypedColumn<string>("NatX");
    public static readonly TypedColumn<string> RefYColumn = new TypedColumn<string>("RefY");
    public static readonly TypedColumn<string> NameYColumn = new TypedColumn<string>("NameY");
    public static readonly TypedColumn<string> NatYColumn = new TypedColumn<string>("NatY");

    public static readonly TypedColumn<string> ScoreColumn = new TypedColumn<string>("Score");

    public static readonly TypedColumn<bool> XWinnerColumn = new TypedColumn<bool>("XWinner");

    public MatchFullTable()
      : base("MatchFull")
    {
      IdMatchColumn.CreatePrimaryKey(this);
      Columns.Add(FkEventColumn.CreateColumn());
      Columns.Add(RoundColumn.CreateColumn());
      Columns.Add(FkPlayerXColumn.CreateColumn());
      Columns.Add(FkPlayerYColumn.CreateColumn());

      Columns.Add(XWinnerColumn.CreateColumn());

      Columns.Add(RefXColumn.CreateColumn());
      Columns.Add(NameXColumn.CreateColumn());
      Columns.Add(NatXColumn.CreateColumn());
      Columns.Add(RefYColumn.CreateColumn());
      Columns.Add(NameYColumn.CreateColumn());
      Columns.Add(NatYColumn.CreateColumn());

      Columns.Add(ScoreColumn.CreateColumn());
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
