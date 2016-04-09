using System.Data;
using System.Data.Common;

namespace Basics.Data
{
  public class DbBaseParameter : DbParameter
  {
    private string _name;

    private DbType _dbType;
    private ParameterDirection _dir;
    private bool _nullable;
    private int _size;

    private string _sourceName;
    private bool _sourceNull;

    private DataRowVersion _version;

    private object _paramValue;

    public DbBaseParameter()
    { }

    public DbBaseParameter(string name, ParameterDirection direction)
    {
      _name = name;
      _dir = direction;

      _sourceName = name;
      _version = DataRowVersion.Default;
    }

    public override DbType DbType
    {
      get { return _dbType; }
      set { _dbType = value; }
    }

    public override ParameterDirection Direction
    {
      get { return _dir; }
      set { _dir = value; }
    }

    public override bool IsNullable
    {
      get { return _nullable; }
      set { _nullable = value; }
    }

    public override string ParameterName
    {
      get { return _name; }
      set { _name = value; }
    }

    public override void ResetDbType()
    {
      _dbType = DbType.Object;
    }

    public override int Size
    {
      get { return _size; }
      set { _size = value; }
    }

    public override string SourceColumn
    {
      get { return _sourceName; }
      set { _sourceName = value; }
    }

    public override bool SourceColumnNullMapping
    {
      get { return _sourceNull; }
      set { _sourceNull = value; }
    }

    public override DataRowVersion SourceVersion
    {
      get { return _version; }
      set { _version = value; }
    }

    public override object Value
    {
      get { return _paramValue; }
      set { _paramValue = value; }
    }
  }
}
