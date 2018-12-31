using Basics.Views;
using OCourse.Ext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace OCourse.ViewModels
{
  public class PermutationVm
  {
    private readonly SectionList _permutation;
    private readonly List<SectionList> _parts;
    public PermutationVm(SectionList permutation)
    {
      _permutation = permutation;
      _parts = permutation.GetParts();
    }

    public IReadOnlyList<SectionList> Parts => _parts;
    public SectionList Permutation => _permutation;
    public int StartNr { get; set; }
    public int Index { get; set; }
  }

  public class PermutationVms : BindingListView<PermutationVm>, ITypedList
  {
    string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
    { throw new NotImplementedException(); }

    PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
    {
      List<PropertyDescriptor> descList = GetItemPropertiesCore();

      if (Count > 0)
      {
        int iPart = 0;
        foreach (var part in this[0].Parts)
        {
          descList.Add(new PartPropertyDesc(iPart, new Attribute[] { new SortAttribute(typeof(SectionList.NameComparer)) }));
          iPart++;
        }
      }
      PropertyDescriptorCollection descs = new PropertyDescriptorCollection(descList.ToArray());
      return descs;
    }
    public static string GetPartPropertyName(int iPart)
    {
      return $"Part_{iPart}";
    }
    private class PartPropertyDesc : PropertyDescriptorBase
    {
      private readonly int _partIndex;

      public PartPropertyDesc(int partIndex, Attribute[] attrs)
        : base(GetPartPropertyName(partIndex), attrs)
      {
        _partIndex = partIndex;
      }

      public override object GetValue(object component)
      {
        if (component is PermutationVm permutation)
        {
          return permutation.Parts[_partIndex];
        }
        return null;
      }

      private static TypeConverter _conv;
      public override TypeConverter Converter => _conv ?? (_conv = new SectionListConverter());

      public override Type PropertyType
      {
        get { return typeof(SectionList); }
      }
    }
    private class SectionListConverter : TypeConverter
    {
      public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
      {
        if (value is SectionList s && destinationType == typeof(string))
        {
          return s.GetName();
        }
        return base.ConvertTo(context, culture, value, destinationType);
      }
    }
  }
}
