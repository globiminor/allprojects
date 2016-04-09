using System;
using System.Data;
using System.Data.Common;

namespace Basics.Data
{
  public abstract class DbBaseReader : DbDataReader
  {
    private DbBaseCommand _command;
    private CommandBehavior _behavior;

    private bool _closed;

    private DataTable _schema;
    private int _fieldCount;

    public DbBaseReader(DbBaseCommand command, CommandBehavior behavior)
    {
      _command = command;
      _behavior = behavior;


      _fieldCount = -1;
    }

    public override void Close()
    {
      _closed = true;
    }
    public override bool IsClosed
    {
      get { return _closed; }
    }

    public override int Depth
    {
      get { return 0; }
    }

    public override int FieldCount
    {
      get
      {
        if (_fieldCount < 0)
        { _fieldCount = Schema.Columns.Count; }
        return _fieldCount;
      }
    }

    public override Type GetFieldType(int ordinal)
    {
      Type type = Schema.Columns[ordinal].DataType;
      return type;
    }

    public override bool GetBoolean(int ordinal)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override byte GetByte(int ordinal)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override char GetChar(int ordinal)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override string GetDataTypeName(int ordinal)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override DateTime GetDateTime(int ordinal)
    {
      DateTime o = (DateTime)GetValue(ordinal);
      return o;
    }

    public override decimal GetDecimal(int ordinal)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override double GetDouble(int ordinal)
    {
      double o = (double)GetValue(ordinal);
      return o;
    }

    public override float GetFloat(int ordinal)
    {
      float o = (float)GetValue(ordinal);
      return o;
    }

    public override Guid GetGuid(int ordinal)
    {
      string o = (string)GetValue(ordinal);
      Guid g = new Guid(o);
      return g;
    }

    public override short GetInt16(int ordinal)
    {
      short o = (short)GetValue(ordinal);
      return o;
    }

    public override int GetInt32(int ordinal)
    {
      int o = (int)GetValue(ordinal);
      return o;
    }

    public override long GetInt64(int ordinal)
    {
      long o = (long)GetValue(ordinal);
      return o;
    }

    public override string GetName(int ordinal)
    {
      string name = Schema.Columns[ordinal].ColumnName;
      return name;
    }

    public override int GetOrdinal(string name)
    {
      int ordinal = Schema.Columns.IndexOf(name);
      return ordinal;
    }

    public override string GetString(int ordinal)
    {
      string o = (string)GetValue(ordinal);
      return o;
    }

    public override int GetValues(object[] values)
    {
      int n = values.Length;
      for (int i = 0; i < n; i++)
      {
        values[i] = GetValue(i);
      }
      return n;
    }

    public override bool HasRows
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override bool IsDBNull(int ordinal)
    {
      //int idx = FieldIndex(ordinal);
      //object o = CurrentRow.get_Value(idx);
      //return o == DBNull.Value;
      return false;
    }

    public override int RecordsAffected
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override object this[string name]
    {
      get
      {
        int ord = GetOrdinal(name);
        return GetValue(ord);
      }
    }

    public override object this[int ordinal]
    {
      get { return GetValue(ordinal); }
    }

    public sealed override DataTable GetSchemaTable()
    {
      return GetSchemaTableCore();
    }

    protected abstract SchemaColumnsTable GetSchemaTableCore();

    protected DataTable Schema
    {
      get
      {
        if (_schema == null)
        {
          SchemaColumnsTable schemaTable = GetSchemaTableCore();

          DataTable schema = new DataTable(_command.TableName);
          foreach (SchemaColumnsTable.Row row in schemaTable.Rows)
          {
            schema.Columns.Add(row.ColumnName, row.DataType);
          }
          _schema = schema;
        }
        return _schema;
      }
    }

    public static DataTable GetSchema(IDataReader reader)
    {
      DataTable schemaTable = reader.GetSchemaTable();
      return GetSchema(schemaTable);
    }

    public static DataTable GetSchema(DataTable schemaTable)
    {
      DataTable schema = new DataTable();
      System.Collections.Generic.List<DataColumn> key =
        new System.Collections.Generic.List<DataColumn>();
      foreach (SchemaColumnsTable.Row row in schemaTable.Rows)
      {
        DataColumn col = schema.Columns.Add(row.ColumnName, row.DataType);

        if (!SchemaColumnsTable.IsKeyColumn.IsNull(row) && row.IsKey)
        { key.Add(col); }
      }
      schema.PrimaryKey = key.ToArray();
      return schema;
    }
  }
}
