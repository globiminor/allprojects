using System;
using System.Data;

namespace Basics.Data
{
  public class TypedRelation<P, C> : DataRelation
    where P : DataRow
    where C : DataRow
  {
    public TypedRelation(string name, DataColumn parentColumn, DataColumn childColumn)
      : base(name, parentColumn, childColumn)
    { }

    public P GetParentRow(C child)
    {
      DataRow parent = child.GetParentRow(this);
      return (P)parent;
    }
    public C[] GetChildRows(P parent)
    {
      DataRow[] rows = parent.GetChildRows(this);
      try
      {
        return (C[])rows;
      }
      catch (Exception e)
      {
        string msg = "Make sure that protected method ChildTable.GetRowType() == typeof(C)";
        throw new Exception(msg, e);
      }
    }
  }
}
