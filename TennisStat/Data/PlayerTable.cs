using Basics.Data;
using System.Data;

namespace TennisStat.Data
{
  public class PlayerTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public int IdPlayer
      {
        get { return IdPlayerColumn.GetValue(this); }
      }

      public string KeyName
      {
        get { return KeyNameColumn.GetValue(this); }
        set { KeyNameColumn.SetValue(this, value); }
      }

      public string Name
      {
        get { return NameColumn.GetValue(this); }
        set { NameColumn.SetValue(this, value); }
      }
    }

    public static readonly TypedColumn<int> IdPlayerColumn = new TypedColumn<int>("IdPlayer");
    public static readonly TypedColumn<string> KeyNameColumn = new TypedColumn<string>("KeyName");
    public static readonly TypedColumn<string> NameColumn = new TypedColumn<string>("Name");

    public PlayerTable()
      : base("Player")
    {
      IdPlayerColumn.CreatePrimaryKey(this).AutoIncrement = true;
      Columns.Add(KeyNameColumn.CreateColumn());
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
