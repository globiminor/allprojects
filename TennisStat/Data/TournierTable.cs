using Basics.Data;
using System.Data;

namespace TennisStat.Data
{
  public class TournierTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public int IdTournier
      {
        get { return IdTournierColumn.GetValue(this); }
        set { IdTournierColumn.SetValue(this, value); }
      }

      public string Name
      {
        get { return NameColumn.GetValue(this); }
        set { NameColumn.SetValue(this, value); }
      }
    }

    public static readonly TypedColumn<int> IdTournierColumn = new TypedColumn<int>("IdTournier");
    //public static readonly TypedColumn<int> FkTournierColumn = new TypedColumn<int>("FkTournier");
    public static readonly TypedColumn<string> NameColumn = new TypedColumn<string>("Name");

    public TournierTable()
      : base("Tournier")
    {
      IdTournierColumn.CreatePrimaryKey(this).AutoIncrement = false;
      //Columns.Add(FkTournierColumn.CreateColumn());
      Columns.Add(NameColumn.CreateColumn());
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
