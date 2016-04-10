
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Basics.Data;
using Basics.Geom;

namespace ArcSde.Data
{
  public sealed class SdeDbCommand : DbBaseCommand
  {
    public SdeDbCommand(SdeDbConnection connection)
      : base(connection)
    { }

    public new SdeDbConnection Connection
    {
      get { return (SdeDbConnection)DbConnection; }
      set { DbConnection = value; }
    }

    protected override DbConnection DbConnection
    {
      get { return base.Connection; }
      set
      {
        SdeDbConnection connection = (SdeDbConnection)value;
        base.DbConnection = connection;
      }
    }

    public new SdeDbTransaction Transaction
    {
      get { return (SdeDbTransaction)DbTransaction; }
      set { DbTransaction = value; }
    }
    protected override DbTransaction DbTransaction
    {
      get { return base.DbTransaction; }
      set
      {
        SdeDbTransaction trans = (SdeDbTransaction)value;
        base.DbTransaction = trans;
      }
    }

    public new SdeDbReader ExecuteReader()
    { return ExecuteReader(CommandBehavior.Default); }
    public new SdeDbReader ExecuteReader(CommandBehavior behavior)
    { return (SdeDbReader)ExecuteDbDataReader(behavior); }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      SdeDbReader reader = new SdeDbReader(this, behavior);
      return reader;
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    { throw new NotImplementedException(); }
    protected override int ExecuteInsert(InsertCommand insert)
    { throw new NotImplementedException(); }
    protected override int ExecuteDelete(DeleteCommand delete)
    { throw new NotImplementedException(); }

    public SeStream.FetchEnum GetEnumerator()
    {
      SelectCommand cmd = (SelectCommand)BaseCmd;

      List<string> cols = new List<string>(cmd.Fields.Count);
      foreach (FieldExpression field in cmd.Fields)
      {
        cols.Add(field.FieldName);
      }

      string where = null;
      List<Se_Filter> filters = new List<Se_Filter>();
      if (cmd.WhereExp != null)
      {
        IList<ParameterInfo> prmList = cmd.WhereExp.Parameters;
        IList<Function> fctList = cmd.WhereExp.Functions;
        StringBuilder whereBuilder = new StringBuilder(cmd.Where);

        foreach (Function function in fctList)
        {
          if (function.Name == "ST_Intersects")
          {
            function.ReplaceByTrue(whereBuilder);
            if (Parameters.Count > 0)
            {
              IGeometry geom = (IGeometry)Parameters[0].Value;
              SeLayer lyr = new SeLayer(Connection.SeConnection, cmd.BaseCmnd.TableName);
              SeShape shape = Convert.ToShape(geom, lyr);

              Filter f = new Filter();
              f.shape = shape.Ptr;
              Se_Filter filter = new Se_Filter(cmd.BaseCmnd.TableName, "Shape", f, Sg.SM_AI);

              filters.Add(filter);
            }
          }
        }
        where = whereBuilder.ToString();
      }
      SeQueryInfo query = new SeQueryInfo(cols, new string[] { cmd.BaseCmnd.TableName }, where);
      SeStream.FetchEnum fetch = new SeStream.FetchEnum(Connection.SeConnection, query, filters);

      return fetch;
    }
  }
}
