using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;

namespace Basics.Views
{
  public static class ListViewUtils
  {
    public static ListSortDescriptionCollection AdaptSort(IBindingListView view,
      string property, bool add)
    {
      if (view == null)
      { return null; }

      Type listType = null;
      Type e = typeof(IEnumerable<>);
      foreach (Type type in view.GetType().GetInterfaces())
      {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == e)
        {
          listType = type.GetGenericArguments()[0];
          break;
        }
      }
      if (listType == null)
      { return null; }

      ListSortDescriptionCollection oldSorts = view.SortDescriptions;
      List<ListSortDescription> newSorts = new List<ListSortDescription>();
      bool propSortExists = false;
      if (oldSorts != null)
      {
        foreach (ListSortDescription sort in oldSorts)
        {
          ListSortDescription newSort;
          if (sort.PropertyDescriptor.Name == property)
          {
            propSortExists = true;
            if (sort.SortDirection == ListSortDirection.Descending)
            { continue; }

            newSort = new ListSortDescription(sort.PropertyDescriptor, ListSortDirection.Descending);
          }
          else
          {
            if (!add)
            { continue; }

            newSort = new ListSortDescription(sort.PropertyDescriptor, sort.SortDirection);
          }
          newSorts.Add(newSort);
        }
      }
      if (!propSortExists)
      {
        PropertyDescriptorCollection properties =
          TypeDescriptor.GetProperties(listType);
        PropertyDescriptor prop = properties.Find(property, true);

        newSorts.Add(new ListSortDescription(prop, ListSortDirection.Ascending));
      }

      ListSortDescriptionCollection sortDescs = new ListSortDescriptionCollection(newSorts.ToArray());

      view.ApplySort(sortDescs);
      return view.SortDescriptions;
    }
  }
  public class BindingListView<T> : BindingList<T>, IBindingListView
  {

    public BindingListView()
    {
      ListChanged += BindingListView_ListChanged;
    }

    void BindingListView_ListChanged(object sender, ListChangedEventArgs e)
    {
      if (_originalListValue == null || _originalListValue.Count == 0)
      {
        _originalListValue = new List<T>(Items);
      }
    }

    private List<T> _originalListValue = new List<T>();
    public List<T> OriginalList
    {
      get
      { return _originalListValue; }
    }
    #region Searching

    protected override bool SupportsSearchingCore
    {
      get { return true; }
    }

    protected override int FindCore(PropertyDescriptor prop, object key)
    {
      // Get the property info for the specified property.
      PropertyInfo propInfo = typeof(T).GetProperty(prop.Name);

      if (key != null)
      {
        // Loop through the items to see if the key
        // value matches the property value.
        for (int i = 0; i < Count; ++i)
        {
          T item = Items[i];
          if (propInfo.GetValue(item, null).Equals(key))
            return i;
        }
      }
      return -1;
    }

    public int Find(string property, object key)
    {
      // Check the properties for a property with the specified name.
      PropertyDescriptorCollection properties =
          TypeDescriptor.GetProperties(typeof(T));
      PropertyDescriptor prop = properties.Find(property, true);

      // If there is not a match, return -1 otherwise pass search to
      // FindCore method.
      if (prop == null)
        return -1;
      else
        return FindCore(prop, key);
    }

    #endregion Searching

    #region Sorting

    protected override bool SupportsSortingCore
    {
      get { return true; }
    }

    protected override bool IsSortedCore
    {
      get { return _sorts != null; }
    }

    public void ApplySort(string propertyName, ListSortDirection direction)
    {
      // Check the properties for a property with the specified name.
      PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(T))[propertyName];

      // If there is not a match, return -1 otherwise pass search to
      // FindCore method.
      if (prop == null)
        throw new ArgumentException(propertyName +
            " is not a valid property for type:" + typeof(T).Name);
      else
      {
        ListSortDescription desc = new ListSortDescription(prop, direction);
        ListSortDescriptionCollection list = new ListSortDescriptionCollection(new ListSortDescription[] { desc });
        ApplySort(list);
      }
    }

    private class Comparer<C> : IComparer<C>
    {
      private readonly ListSortDescriptionCollection _propDirs;
      private readonly Dictionary<ListSortDescription, IComparer<object>> _comparers;

      public Comparer(ListSortDescriptionCollection propDirs)
      {
        _propDirs = propDirs;

        _comparers = new Dictionary<ListSortDescription, IComparer<object>>(_propDirs.Count);
        foreach (ListSortDescription propDir in _propDirs)
        {
          IComparer<object> cmp = null;

          PropertyDescriptor prop = propDir.PropertyDescriptor;
          foreach (Attribute attr in prop.Attributes)
          {
            SortAttribute sortAttr = attr as SortAttribute;
            if (sortAttr != null && sortAttr.ComparerType != null)
            {
              object oCmp = Activator.CreateInstance(sortAttr.ComparerType);
              cmp = oCmp as IComparer<object>;
            }
          }

          _comparers.Add(propDir, cmp);
        }
      }

      public int Compare(C x, C y)
      {
        foreach (var pair in _comparers)
        {
          ListSortDescription desc = pair.Key;
          PropertyDescriptor prop = desc.PropertyDescriptor;
          IComparer<object> comparer = pair.Value;

          object xVal = prop.GetValue(x);
          object yVal = prop.GetValue(y);

          int diff;
          if (comparer != null)
          {
            diff = comparer.Compare(xVal, yVal);
          }
          else
          {
            IComparable xCmp = xVal as IComparable;

            if (xCmp != null)
            { diff = xCmp.CompareTo(yVal); }
            else
            {
              IComparable yCmp = yVal as IComparable;
              if (yCmp != null)
              { diff = -yCmp.CompareTo(xVal); }
              else
              { diff = 0; }
            }
          }
          if (diff != 0)
          {
            if (desc.SortDirection == ListSortDirection.Ascending)
            { return diff; }
            if (desc.SortDirection == ListSortDirection.Descending)
            { return -diff; }
          }
        }
        return 0;
      }
    }
    protected override void ApplySortCore(PropertyDescriptor prop,
        ListSortDirection direction)
    {
      return;
    }

    protected override void RemoveSortCore()
    {
      RaiseListChangedEvents = false;
      // Ensure the list has been sorted.
      Clear();
      List<T> items = GetFiltered(Filter);

      foreach (T item in items)
      { Add(item); }

      RaiseListChangedEvents = true;
      // Raise the list changed event, indicating a reset, and index
      // of -1.
      OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    public void RemoveSort()
    {
      RemoveSortCore();
    }


    public override void EndNew(int itemIndex)
    {
      // Check to see if the item is added to the end of the list,
      // and if so, re-sort the list.
      if (IsSortedCore && itemIndex > 0
          && itemIndex == Count - 1)
      {
        // TODO : resort
        base.EndNew(itemIndex);
      }
    }

    #endregion Sorting

    #region AdvancedSorting
    private ListSortDescriptionCollection _sorts;

    public bool SupportsAdvancedSorting
    {
      get { return true; }
    }
    public ListSortDescriptionCollection SortDescriptions
    {
      get { return _sorts; }
    }

    public void ApplySort(ListSortDescriptionCollection sorts)
    {
      _sorts = null;

      RaiseListChangedEvents = false;

      List<T> sort;
      if (sorts.Count > 0)
      {
        sort = new List<T>(Items);
        Comparer<T> cmp = new Comparer<T>(sorts);
        sort.Sort(cmp);
      }
      else
      {
        sort = GetFiltered(Filter);
      }

      int nItems = Items.Count;
      for (int i = 0; i < nItems; i++)
      {
        this[i] = sort[i];
      }

      _sorts = sorts;

      RaiseListChangedEvents = true;
      // raise the ListChanged event so bound controls refresh their
      // values. Pass -1 for the index since this is a Reset.
      OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    #endregion AdvancedSorting

    #region Filtering

    public bool SupportsFiltering
    {
      get { return true; }
    }

    public void RemoveFilter()
    {
      if (Filter != null) Filter = null;
    }

    private string _filterValue;

    public string Filter
    {
      get
      {
        return _filterValue;
      }
      set
      {
        if (_filterValue == value) return;

        List<T> filtered = GetFiltered(value);

        if (_sorts != null && _sorts.Count > 0)
        {
          if (filtered == _originalListValue)
          {
            filtered = new List<T>(filtered); // To avoid override of _originalListValue
          }
          Comparer<T> cmp = new Comparer<T>(_sorts);
          filtered.Sort(cmp);
        }

        //Turn off list-changed events.
        RaiseListChangedEvents = false;

        ClearItems();

        foreach (T item in filtered)
        { Add(item); }

        // Set the filter value and turn on list changed events.
        _filterValue = value;
        RaiseListChangedEvents = true;
        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
      }
    }

    private List<T> GetFiltered(string filterValue)
    {
      if (string.IsNullOrEmpty(filterValue))
      { return _originalListValue; }

      List<PropertyInfo> filterProperties = new List<PropertyInfo>();
      foreach (PropertyInfo prop in typeof(T).GetProperties())
      {
        if (filterValue.IndexOf(prop.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          filterProperties.Add(prop);
        }
      }
      List<T> filtered = new List<T>(_originalListValue.Count);
      using (DataTable filterTable = new DataTable())
      {
        foreach (PropertyInfo prop in filterProperties)
        {
          Type propType = prop.PropertyType;
          if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
          {
            propType = propType.GetGenericArguments()[0];
          }
          filterTable.Columns.Add(prop.Name, propType);
        }
        DataRow filterRow = filterTable.NewRow();
        filterTable.Rows.Add(filterRow);

        using (DataView filterView = new DataView(filterTable))
        {
          filterView.RowFilter = filterValue;

          foreach (T item in _originalListValue)
          {
            foreach (PropertyInfo prop in filterProperties)
            {
              object val = prop.GetValue(item, null);
              filterRow[prop.Name] = val ?? DBNull.Value;
            }

            if (filterView.Count == 1)
            {
              filtered.Add(item);
            }
          }
        }
        filterTable.Clear();
      }
      return filtered;

    }


    // Build a regular expression to determine if 
    // filter is in correct format.
    public static string BuildRegExForFilterFormat()
    {
      StringBuilder regex = new StringBuilder();

      // Look for optional literal brackets, 
      // followed by word characters or space.
      regex.Append(@"\[?[\w\s]+\]?\s?");

      // Add the operators: > < or =.
      regex.Append(@"[><=]");

      //Add optional space followed by optional quote and
      // any character followed by the optional quote.
      regex.Append(@"\s?'?.+'?");

      return regex.ToString();
    }

    protected override void OnListChanged(ListChangedEventArgs e)
    {
      // Add the new item to the original list.
      if (e.ListChangedType == ListChangedType.ItemAdded)
      {
        OriginalList.Add(this[e.NewIndex]);
        if (!String.IsNullOrEmpty(Filter))
        //if (Filter == null || Filter == "")
        {
          string cachedFilter = Filter;
          Filter = "";
          Filter = cachedFilter;
        }
      }
      // Remove the new item from the original list.
      if (e.ListChangedType == ListChangedType.ItemDeleted)
        OriginalList.RemoveAt(e.NewIndex);

      base.OnListChanged(e);
    }

    #endregion Filtering
  }

  public class SortAttribute : Attribute
  {
    private readonly Type _comparerType;
    public SortAttribute(Type comparerType)
    {
      _comparerType = comparerType;
    }
    public Type ComparerType
    {
      get { return _comparerType; }
    }
  }
}
