using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using Basics.Geom;

namespace Basics.Data.DataSetDb
{
  public class EnumReader : DbBaseReader
  {
    #region nested classes
    private class Enumerator : IEnumerator<DataRow>
    {
      private readonly EnumReader _reader;
      private IEnumerator _enumerator;
      private DataRow _currentElem;

      public Enumerator(EnumReader reader)
      {
        _reader = reader;

        EnumCommand cmd = _reader._command;
      }

      #region IEnumerator Members

      public void Dispose()
      { }

      public void Reset()
      {
        _enumerator.Reset();
        _currentElem = null;
      }

      object IEnumerator.Current
      { get { return Current; } }
      public DataRow Current
      {
        get { return _currentElem; }
      }

      public bool MoveNext()
      {
        bool value = _enumerator.MoveNext();
        bool conditions = false;
        _currentElem = null;
        while (value && conditions == false)
        {
          _currentElem = ((DataRowView)_enumerator.Current).Row;

          conditions = _reader._command.Validate(_reader);

          if (conditions == false)
          { value = _enumerator.MoveNext(); }
        }
        return value;
      }

      #endregion
    }
    #endregion

    private EnumCommand _command;


    private Enumerator _enum;
    private int _nRows;

    public EnumReader(EnumCommand command, CommandBehavior behavior)
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
      string name = GetName(ordinal);

      return _enum.Current[name];
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
