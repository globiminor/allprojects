
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Basics.Data
{
  public class DbBaseParameterCollection : DbParameterCollection
  {
    private List<DbBaseParameter> _list;

    public DbBaseParameterCollection()
    {
      _list = new List<DbBaseParameter>();
    }

    public override int Add(object value)
    {
      _list.Add((DbBaseParameter)value);
      return _list.Count;
    }

    public override void AddRange(Array values)
    {
      foreach (DbBaseParameter param in values)
      { Add(param); }
    }

    public override void Clear()
    {
      _list.Clear();
    }

    public override bool Contains(string value)
    {
      return IndexOf(value) >= 0;
    }

    public override bool Contains(object value)
    {
      return IndexOf(value) >= 0;
    }

    public override void CopyTo(Array array, int index)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override int Count
    {
      get { return _list.Count; }
    }

    public override System.Collections.IEnumerator GetEnumerator()
    {
      return _list.GetEnumerator();
    }

    protected override DbParameter GetParameter(string parameterName)
    {
      return _list[IndexOf(parameterName)];
    }

    protected override DbParameter GetParameter(int index)
    {
      return _list[index];
    }

    public override int IndexOf(string parameterName)
    {
      int n = Count;
      for (int i = 0; i < n; i++)
      {
        if (_list[i].ParameterName == parameterName)
        { return i; }
      }
      return -1;
    }

    public override int IndexOf(object value)
    {
      return _list.IndexOf((DbBaseParameter)value);
    }

    public override void Insert(int index, object value)
    {
      _list.Insert(index, (DbBaseParameter)value);
    }

    public override bool IsFixedSize
    {
      get { return false; }
    }

    public override bool IsReadOnly
    {
      get { return false; }
    }

    public override bool IsSynchronized
    {
      get { return true; }
    }

    public override void Remove(object value)
    {
      _list.Remove((DbBaseParameter)value);
    }

    public override void RemoveAt(string parameterName)
    {
      RemoveAt(IndexOf(parameterName));
    }

    public override void RemoveAt(int index)
    {
      _list.RemoveAt(index);
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
      int i = IndexOf(parameterName);
      SetParameter(i, value);
    }

    protected override void SetParameter(int index, DbParameter value)
    {
      _list[index] = (DbBaseParameter)value;
    }

    public override object SyncRoot
    {
      get { return null; }
    }
  }
}
