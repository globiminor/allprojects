using System.Data;
using System.Data.Common;

namespace Basics.Data
{
  [System.ComponentModel.ToolboxItem(false)]
  public class DbBaseAdapter : DbDataAdapter
  {
    private bool _adaptColumnStringLength;

    protected bool AdaptColumnStringLength
    {
      get { return _adaptColumnStringLength; }
      set { _adaptColumnStringLength = value; }
    }

    protected override int Fill(DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
    {
      if (_adaptColumnStringLength && dataTables != null && dataTables.Length == 1)
      {
        AdaptStringLength(dataTables[0], dataReader);
      }
      return base.Fill(dataTables, dataReader, startRecord, maxRecords);
    }

    protected override int Fill(DataTable dataTable, IDataReader dataReader)
    {
      if (_adaptColumnStringLength)
      {
        AdaptStringLength(dataTable, dataReader);
      }

      return base.Fill(dataTable, dataReader);
    }

    private void AdaptStringLength(DataTable dataTable, IDataReader dataReader)
    {
      bool first = true;
      DataTable schemaTable = null;
      foreach (var o in dataTable.Columns)
      {
        DataColumn dataColumn = (DataColumn)o;
        if (dataColumn.DataType == typeof(string) && dataColumn.MaxLength < 0)
        {
          if (first)
          {
            if (dataReader is DbDataReader dbReader)
            { schemaTable = dbReader.GetSchemaTable(); }

            first = false;
          }
          if (schemaTable != null)
          {
            string columnName = dataColumn.ColumnName;
            DataColumn schemaColumn = schemaTable.Columns[columnName];
            if (schemaColumn != null && schemaColumn.MaxLength > 0)
            { dataColumn.MaxLength = schemaColumn.MaxLength; }
          }
        }
      }
    }

  }
}
