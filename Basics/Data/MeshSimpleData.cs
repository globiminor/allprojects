using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Basics.Data
{
  public class MeshSimpleData : ISimpleData<Mesh.MeshLine>
  {
    public static readonly string FieldLine = "Line";
    public static readonly string FieldTag = "Tag";
    public static readonly string FieldTagReverse = "TagReverse";

    private Mesh _mesh;
    public MeshSimpleData(Mesh mesh)
    {
      _mesh = mesh;
    }

    public Mesh Mesh
    {
      get { return _mesh; }
      set { _mesh = value; }
    }
    public IBox GetExtent()
    {
      return _mesh.Extent;
    }

    public IEnumerator<Mesh.MeshLine> GetEnumerator(IBox geom)
    {
      return _mesh.Lines(geom).GetEnumerator();
    }

    public object GetValue(Mesh.MeshLine element, string name)
    {
      if (FieldLine.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return Polyline.Create(new[] { element.Start, element.End });
      }
      else if (FieldTag.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        bool reverse;
        return element.GetTag(out reverse);
      }
      else if (FieldTagReverse.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        bool reverse;
        element.GetTag(out reverse);
        return reverse;
      }
      else
      {
        throw new InvalidOperationException("Invalid name " + name);
      }
    }

    public SchemaColumnsTable GetTableSchema()
    {
      SchemaColumnsTable schema = new SchemaColumnsTable();

      schema.AddSchemaColumn(FieldLine, typeof(Polyline));
      schema.AddSchemaColumn(FieldTag, typeof(object));
      schema.AddSchemaColumn(FieldTagReverse, typeof(bool));

      return schema;
    }
  }
}
