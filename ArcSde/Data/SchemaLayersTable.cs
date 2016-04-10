using System.Data;

namespace ArcSde.Data
{
  public class SchemaLayersTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public string TableName
      {
        get { return (string)this[ColTableName]; }
        set { this[ColTableName] = value; }
      }
      public string SpatialColumn
      {
        get { return (string)this[colSpatialColName]; }
        set { this[colSpatialColName] = value; }
      }
      public int SpatialTypes
      {
        get { return (int)this[colSpatialTypesName]; }
        set { this[colSpatialTypesName] = value; }
      }

    }

    public static readonly string ColTableName = "TABLE_NAME";
    public static readonly string colSpatialColName = "SPATIAL_COL_NAME";
    public static readonly string colSpatialTypesName = "SPATIAL_TYPES_NAME";
    public SchemaLayersTable()
      : base("SchemaLayers")
    {
      Columns.Add(ColTableName, typeof(string));
      Columns.Add(colSpatialColName, typeof(string));
      Columns.Add(colSpatialTypesName, typeof(int));
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
