using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using Basics.Data;
using Basics.Geom;

namespace Shape.Data
{
  public class AccessDbReader : DbBaseReader
  {
    private AccessCommand _command;

    private OleDbCommand _oleDbCmd;
    private OleDbDataReader _reader;

    public AccessDbReader(AccessCommand command, CommandBehavior behavior)
      : base(command, behavior)
    {
      _command = command;
      _oleDbCmd = new OleDbCommand(command.GetStandardSqlText(), command.Connection.OleDbConnection);
      foreach (var parameter in command.Parameters.Enum())
      {
        if (parameter.Value is IGeometry)
        { continue; }
        OleDbParameter param = new OleDbParameter(parameter.ParameterName, parameter.Value);
        _oleDbCmd.Parameters.Add(param);
      }
      _reader = _oleDbCmd.ExecuteReader(behavior);
    }

    protected override void Dispose(bool disposing)
    {
      if (_reader != null)
      {
        _reader.Dispose();
        _reader = null;
      }
      if (_oleDbCmd != null)
      {
        _oleDbCmd.Dispose();
        _oleDbCmd = null;
      }
      base.Dispose(disposing);
    }

    public override IEnumerator GetEnumerator()
    {
      return _reader.GetEnumerator();
    }

    private object[] _record;
    private int _maxOrdinal;
    public override object GetValue(int ordinal)
    {
      if (_record == null)
      {
        _record = new object[Schema.Columns.Count];
        _maxOrdinal = 0;
      }

      if (_record[ordinal] != null)
      { return _record[ordinal]; }

      for (int i = _maxOrdinal; i <= ordinal; i++)
      {
        object recValue;
        object value = _reader.GetValue(i);
        if (!(value is byte[]))
        { recValue = value; }
        else if (!typeof(IGeometry).IsAssignableFrom(Schema.Columns[i].DataType))
        { recValue = value; }
        else
        {
          IGeometry geom = AccessUtils.GetGeometry((byte[]) value);
          recValue = geom;
        }
        _record[i] = recValue;
        _maxOrdinal = i;
      }
      return _record[ordinal];
    }

    public override bool Read()
    {
      bool accessSuccess = _reader.Read();
      while (accessSuccess)
      {
        _record = null;
        bool conditions = _command.Validate(this);
        if (conditions)
        { break; }

        accessSuccess = _reader.Read();
      }
      return accessSuccess;
    }

    public override bool NextResult()
    {
      return _reader.NextResult();
    }

    public override int RecordsAffected
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    protected override SchemaColumnsTable GetSchemaTableCore()
    {
      DataTable accessSchema = _reader.GetSchemaTable();
      SchemaColumnsTable schema = new SchemaColumnsTable();
      foreach (var aRow in accessSchema.Rows.Enum())
      {
        SchemaColumnsTable.Row sRow = schema.NewRow();
        sRow.ColumnName = (string)aRow["ColumnName"];
        sRow.ColumnOrdinal = (int) aRow["ColumnOrdinal"];

        Type dataType = (Type)aRow["DataType"];
        if (dataType == typeof(byte[]))
        {
          dataType = typeof(IGeometry);
        }
        sRow.DataType = dataType;

        schema.AddRow(sRow);
      }
      return schema;
    }
  }

}
