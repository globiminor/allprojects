using System.Data;

namespace Basics.Data
{
  public class SchemaCollectionTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public string CollectionName
      {
        get { return (string)this[ColCollectionItem]; }
        set { this[ColCollectionItem] = value; }
      }
    }

    public static readonly string ColCollectionItem = "CollectionItem";
    public SchemaCollectionTable()
      : base("SchemaCollection")
    {
      Columns.Add(ColCollectionItem, typeof(string));
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