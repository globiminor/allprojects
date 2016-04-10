
using System;
using System.Collections;
using System.Data;
using Basics.Data;

namespace ArcSde.Data
{
  public class SdeDbReader : DbBaseReader
  {

    private SdeDbCommand _command;
    private CommandBehavior _behavior;

    private SeStream.FetchEnum _enum;
    private bool _closed;

    private int _nRows;

    public SdeDbReader(SdeDbCommand command, CommandBehavior behavior)
      : base(command, behavior)
    {
      _command = command;
      _behavior = behavior;
    }

    protected override void Dispose(bool disposing)
    {
      IDisposable disp = _enum;
      if (disp != null)
      {
        disp.Dispose();
        _enum = null;
      }
      base.Dispose(disposing);
    }

    public override IEnumerator GetEnumerator()
    {
      _enum = _command.GetEnumerator();
      return _enum;
    }

    public override object GetValue(int ordinal)
    {
      SeStream.FetchEnum fetch = _enum;
      Type type = Schema.Columns[ordinal].DataType;
      Type fetchType = type;
      if (type == typeof(Basics.Geom.IGeometry))
      {
        fetchType = typeof(SeShape);
      }

      object value = fetch.GetValue(ordinal, fetchType);

      if (type == typeof(Basics.Geom.IGeometry))
      {
        SeShape shape = (SeShape)value;
        Basics.Geom.IGeometry geom = Convert.ToGeom(shape);
        return geom;
      }

      return value;
    }

    public override bool Read()
    {
      if (_enum == null)
      { GetEnumerator(); }

      bool next = _enum.MoveNext();
      if (next == false)
      { _closed = true; }
      else
      { _nRows++; }
      return next;
    }

    public override bool NextResult()
    {
      return true;
    }

    public override int RecordsAffected
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    protected override SchemaColumnsTable GetSchemaTableCore()
    {
      SchemaColumnsTable schema = _command.GetSchemaTable();
      return schema;
    }
  }
}
