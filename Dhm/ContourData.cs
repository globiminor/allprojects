using System;
using System.Collections.Generic;
using Basics.Data;
using Basics.Geom;

namespace Dhm
{
  public class ContourSimpleData : ISimpleData<Contour>
  {
    public static readonly string FieldLine = "Line";
    public static readonly string FieldId = "Id";
    public static readonly string FieldHeightIndex = "HeightIndex";
    public static readonly string FieldMaxHeightIndex = "MaxHeightIndex";
    public static readonly string FieldMinHeightIndex = "MinHeightIndex";
    public static readonly string FieldOrientation = "Orientation";
    public static readonly string FieldLoopChecked = "LoopState";
    public static readonly string FieldType = "ContourType";

    private IList<Contour> _contours;
    public ContourSimpleData(IList<Contour> contours)
    {
      _contours = contours;
    }

    public IList<Contour> Contours
    {
      get { return _contours; }
      set { _contours = value; }
    }
    public IBox GetExtent()
    {
      Box box = null;
      foreach (var contour in _contours)
      {
        if (box == null)
        { box = new Box(contour.Polyline.Extent); }
        else
        { box.Include(contour.Polyline.Extent); }
      }
      return box;
    }

    public IEnumerator<Contour> GetEnumerator(IBox geom)
    {
      foreach (var contour in _contours)
      {
        if (contour.Polyline.Extent.Intersects(geom))
        {
          yield return contour;
        }
      }
    }

    public object GetValue(Contour element, string name)
    {
      if (FieldLine.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.Polyline;
      }
      else if (FieldId.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.Id;
      }
      else if (FieldHeightIndex.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.HeightIndex;
      }
      else if (FieldMinHeightIndex.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.MinHeightIndex;
      }
      else if (FieldMaxHeightIndex.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.MaxHeightIndex;
      }
      else if (FieldLoopChecked.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.LoopChecked;
      }
      else if (FieldOrientation.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.Orientation;
      }
      else if (FieldType.Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        return element.Type;
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
      schema.AddSchemaColumn(FieldId, typeof(int));
      schema.AddSchemaColumn(FieldHeightIndex, typeof(int));
      schema.AddSchemaColumn(FieldMaxHeightIndex, typeof(int));
      schema.AddSchemaColumn(FieldMinHeightIndex, typeof(int));
      schema.AddSchemaColumn(FieldOrientation, typeof(Orientation));
      schema.AddSchemaColumn(FieldLoopChecked, typeof(Contour.LoopState));
      schema.AddSchemaColumn(FieldType, typeof(ContourType));

      return schema;
    }
  }
}
