
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Basics.Data;

namespace Shape.Data
{
  public class ShapeDbReader : DbBaseReader
  {
    #region nested classes
    private class Enumerator : IEnumerator<DataRow>
    {
      private ShapeReader _shapeReader;
      private IEnumerator<DataRow> _shape;
      private DataRow _currentRow;
      private ShapeDbReader _reader;

      public Enumerator(ShapeDbReader reader)
      {
        string path = reader._command.GetFullName();
        _shapeReader = new ShapeReader(path);
        _shape = _shapeReader.GetEnumerator();
        _reader = reader;
      }

      #region IEnumerator Members

      public void Dispose()
      {
        _shapeReader.Dispose();
      }

      public void Reset()
      {
        _shape.Reset();
        _currentRow = null;
      }

      object IEnumerator.Current
      { get { return Current; } }
      public DataRow Current
      {
        get { return _currentRow; }
      }

      public bool MoveNext()
      {
        bool value = _shape.MoveNext();
        bool conditions = false;
        _currentRow = null;
        while (value && conditions == false)
        {
          _currentRow = _shape.Current;

          conditions = _reader._command.Validate(_reader);

          if (conditions == false)
          { value = _shape.MoveNext(); }
        }
        return value;
      }

      #endregion
    }
    #endregion

    private ShapeCommand _command;

    private Enumerator _enum;
    private int _nRows;

    public ShapeDbReader(ShapeCommand command, CommandBehavior behavior)
      : base(command, behavior)
    {
      _command = command;
    }

    protected override void Dispose(bool disposing)
    {
      if (_enum != null)
      {
        _enum.Dispose();
        _enum = null;
      }
      base.Dispose(disposing);
    }

    public override IEnumerator GetEnumerator()
    {
      if (_enum != null)
      { _enum.Dispose(); }

      _enum = new Enumerator(this);
      _nRows = 0;
      return _enum;
    }

    public override object GetValue(int ordinal)
    {
      return _enum.Current[ordinal];
    }

    public override bool Read()
    {
      if (_enum == null)
      { GetEnumerator(); }

      bool next = _enum.MoveNext();
      if (next == false)
      { }
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
