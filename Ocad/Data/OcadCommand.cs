
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Basics.Data;

namespace Ocad.Data
{
  [System.ComponentModel.ToolboxItem(false)]
  public sealed class OcadCommand : DbBaseCommand
  {
    public OcadCommand(OcadConnection connection)
      : base(connection)
    { }

    public new OcadConnection Connection
    {
      get { return (OcadConnection)DbConnection; }
      set { DbConnection = value; }
    }

    protected override DbConnection DbConnection
    {
      get { return base.DbConnection; }
      set
      {
        OcadConnection connection = (OcadConnection)value;
        base.DbConnection = connection;
      }
    }

    public bool? SortByColors { get; set; }

    public new OcadTransaction Transaction
    {
      get { return (OcadTransaction)DbTransaction; }
      set { DbTransaction = value; }
    }
    protected override DbTransaction DbTransaction
    {
      get { return base.DbTransaction; }
      set
      {
        OcadTransaction trans = (OcadTransaction)value;
        base.DbTransaction = trans;
      }
    }

    public new OcadDbReader ExecuteReader()
    { return ExecuteReader(CommandBehavior.Default); }
    public new OcadDbReader ExecuteReader(CommandBehavior behavior)
    { return (OcadDbReader)ExecuteDbDataReader(behavior); }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      OcadDbReader reader = new OcadDbReader(this, behavior);
      return reader;
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    {
      if (update.WhereExp.Parameters.Count != 1)
      { throw new NotImplementedException("Unhandled Where expression " + update.Where); }
      DbParameter whereParam = update.WhereExp.Parameters[0].DbParameter;
      if (whereParam.ParameterName != OcadConnection.FieldId)
      { throw new NotImplementedException("Unhandled Where Parameter " + whereParam.ParameterName); }
      int whereIdx = (int)whereParam.Value;

      OcadTransaction trans = Transaction;
      OcadWriter w;
      if (trans != null)
      { w = trans.Writer; }
      else
      { w = OcadWriter.AppendTo(Connection.ConnectionString); }
      ElementIndex elemIdx = w.Io.ReadIndex(whereIdx);
       w.Io.ReadElement(elemIdx, out GeoElement elem);

      foreach (var pInfo in update.UpdateParameterInfos)
      {
        DbParameter p = pInfo.DbParameter;
        if (p.ParameterName == OcadConnection.FieldId)
        {
          int id = (int)p.Value;
          if (id != whereIdx)
          {
            string msg = string.Format("Cannot update id from {0} to {1}", whereIdx, id);
            throw new InvalidOperationException(msg);
          }
        }
        else if (p.ParameterName == OcadConnection.FieldShape.Name)
        {
          elem.Geometry = GeoElement.Geom.Create(p.Value);
        }
        else if (p.ParameterName == OcadConnection.FieldSymbol)
        { elem.Symbol = (int)p.Value; }
        else if (p.ParameterName == OcadConnection.FieldAngle)
        { elem.Angle = (double)p.Value; }
        else if (p.ParameterName == OcadConnection.FieldHeight)
        { elem.Height = (double)p.Value; }
        else
        { throw new InvalidOperationException("Unhandled parameter " + p.ParameterName); }
      }

      w.Overwrite(elem, elemIdx.Index);

      if (trans == null)
      {
        w.Close();
        w.Dispose();
      }
      return 1;
    }
    protected override int ExecuteInsert(InsertCommand insert)
    { throw new NotImplementedException(); }
    protected override int ExecuteDelete(DeleteCommand delete)
    {
      int id = (int)delete.BaseCmnd.Parameters[0].Value;
      OcadTransaction trans = Transaction;
      if (trans != null)
      { trans.Deletes.Add(id); }
      else
      {
        using (OcadWriter w = OcadWriter.AppendTo(Connection.ConnectionString))
        {
          DeleteIndices dels = new DeleteIndices();
          dels.Add(id);
          w.DeleteElements(dels.Delete);
          w.Close();
        }
      }
      return 1;
    }
    internal class DeleteIndices
    {
      private readonly List<int> _idList = new List<int>();
      private bool _sorted;
      public bool Delete(ElementIndex idx)
      {
        if (!_sorted)
        {
          _idList.Sort();
          _sorted = true;
        }
        return _idList.BinarySearch(idx.Index) >= 0;
      }

      public void Add(int id)
      {
        _idList.Add(id);
        _sorted = false;
      }
    }
  }
}
