
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Basics.Data;
using Basics.Geom;

namespace Ocad.Data
{
  public class OcadDbReader : DbBaseReader
  {
    private OcadCommand _command;

    private IEnumerator<GeoElement> _enum;
    private int _nRows;

    public OcadDbReader(OcadCommand command, CommandBehavior behavior)
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

      IBox extent = null;

      foreach (var parameter in _command.Parameters.Enum())
      {
        if (parameter.Value is IGeometry intersect)
        { extent = intersect.Extent; }
      }

      _enum = OcadInfo.CreateReader().EnumGeoElements(extent, null).GetEnumerator();
      _nRows = 0;
      return _enum;
    }

    public override object GetValue(int ordinal)
    {
      string name = GetName(ordinal);
      if (OcadConnection.FieldId.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return _enum.Current.Index;
      }
      else if (OcadConnection.FieldShape.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return _enum.Current.Geometry;
      }
      else if (OcadConnection.FieldSymbol.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return _enum.Current.Symbol;
      }
      else if (OcadConnection.FieldAngle.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return _enum.Current.Angle;
      }
      else if (OcadConnection.FieldHeight.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return _enum.Current.Height;
      }
      else
      {
        throw new InvalidOperationException("Invalid ordinal " + ordinal);
      }
    }

    private OcadElemsInfo _ocadInfo;
    private OcadElemsInfo OcadInfo
    {
      get
      {
        if (_ocadInfo == null)
        {
          _ocadInfo = _command.Connection.GetReader(_command.TableName);
        }
        return _ocadInfo;
      }
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
