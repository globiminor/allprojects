using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;

namespace Basics.Views
{
  public interface IPartialListView
  {
    event EventHandler DataLoading;
    event EventHandler DataLoaded;

    event EventHandler DistinctValuesLoading;
    event EventHandler DistinctValuesLoaded;

    IEnumerable GetDistinctValues(string dataProperty, int max);
  }
  public abstract class PartialListView : IBindingListView, IPartialListView
  {
    public event EventHandler DataLoading;
    public event EventHandler DataLoaded;

    public event EventHandler DistinctValuesLoading;
    public event EventHandler DistinctValuesLoaded;

    private class Cache
    {
      public DateTime LastAccess { get; set; }
      public object Value { get; set; }
    }
    private Dictionary<int, Cache> _objectDict;
    private ListSortDescriptionCollection _sortDescriptions;
    private string _filter;

    public int MaxCache { get; protected set; }
    public int QueryCount { get; protected set; }

    protected PartialListView()
    {
      MaxCache = 4000;
      QueryCount = 100;
    }

    object IList.this[int index] { get { return this[index]; } set { this[index] = value; } }
    public object this[int index]
    {
      get
      {
        if (_objectDict == null)
        { _objectDict = new Dictionary<int, Cache>(); }

        if (!_objectDict.TryGetValue(index, out Cache value))
        {
          value = LoadData(index, _objectDict);
        }
        value.LastAccess = DateTime.Now;
        return value.Value;
      }
      set { throw new NotImplementedException(); }
    }

    protected int CacheCount
    {
      get { return _objectDict?.Count ?? 0; }
    }

    void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
    { ApplySort(property, direction); }
    public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
    {
      ListSortDescription desc = new ListSortDescription(property, direction);
      ListSortDescriptionCollection list = new ListSortDescriptionCollection(new ListSortDescription[] { desc });
      ApplySort(list);
    }

    void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
    { ApplySort(sorts); }
    public void ApplySort(ListSortDescriptionCollection sorts)
    {
      List<ListSortDescription> clones = new List<ListSortDescription>();
      foreach (ListSortDescription sort in sorts)
      {
        clones.Add(new ListSortDescription(sort.PropertyDescriptor, sort.SortDirection));
      }

      _objectDict = null;
      _sortDescriptions = new ListSortDescriptionCollection(clones.ToArray());
      OnListChanged(ListChangedType.Reset);
    }

    public virtual void Reset()
    {
      _objectDict = null;
    }

    public bool UseThreading { get; set; } = true;

    protected abstract object GetDefaultValue();

    public event ListChangedEventHandler ListChanged;
    protected void OnListChanged(ListChangedType changeType)
    {
      if (System.Threading.Thread.CurrentThread.IsThreadPoolThread)
      { return; }

      ListChanged?.Invoke(this, new ListChangedEventArgs(changeType, 0));
    }

    IEnumerable IPartialListView.GetDistinctValues(string dataProperty, int max)
    { return GetDistinctValues(dataProperty, max); }
    public IEnumerable<object> GetDistinctValues(string dataProperty, int max)
    {
      SortedDictionary<string, object> dict = new SortedDictionary<string, object>();
      Func<object, object> getProterty = null;
      if (_objectDict != null)
      {
        foreach (Cache cache in _objectDict.Values)
        {
          if (getProterty == null)
          {
            var method = cache.Value.GetType().GetProperty(dataProperty).GetGetMethod();
            getProterty = (o) => method.Invoke(o, new object[] { });
          }

          object value = getProterty(cache.Value);
          dict[$"{value}"] = value;

          if (dict.Count >= max)
          { return dict.Values; }
        }
      }
      if (_objectDict != null && _objectDict.Count >= Count)
      { return dict.Values; }

      DistinctValuesLoading?.Invoke(this, null);
      foreach (object value in LoadDistinctValues(dataProperty, max))
      {
        dict[$"{value}"] = value;

        if (dict.Count >= max)
        { break; }
      }
      DistinctValuesLoaded?.Invoke(this, null);
      return dict.Values;
    }
    private Cache LoadData(int index, Dictionary<int, Cache> objectDict)
    {
      if (objectDict.Count > MaxCache)
      {
        List<KeyValuePair<int, Cache>> pairs = new List<KeyValuePair<int, Cache>>(objectDict.Count);
        pairs.AddRange(objectDict);

        pairs.Sort((x, y) => x.Value.LastAccess.CompareTo(y.Value.LastAccess));
        int nRemove = objectDict.Count - MaxCache + QueryCount;
        for (int remove = 0; remove < nRemove; remove++)
        { objectDict.Remove(pairs[remove].Key); }
      }

      int i0 = Math.Max(0, index - 5);
      while (i0 > 0 && objectDict.ContainsKey(i0 + QueryCount - 1))
      {
        i0--;
      }
      int i;
      int? start;
      int? end;
      if (Count < 2 * QueryCount)
      {
        i = 0;
        start = null;
        end = null;
      }
      else
      {
        i = i0;
        start = i0;
        end = i0 + QueryCount;
      }

      if (UseThreading)
      {
        DateTime access = DateTime.Now;
        for (int j = i0; j < i0 + QueryCount; j++)
        {
          objectDict[j] = new Cache { LastAccess = access, Value = GetDefaultValue() };
        }

        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
          LoadData(objectDict, i, start, end);
        });
        t.Start();
      }
      else
      {
        LoadData(objectDict, i, start, end);
      }
      return objectDict[index];
    }

    private void LoadData(Dictionary<int, Cache> objectDict, int i0, int? start, int? end)
    {
      lock (this)
      {
        DataLoading?.Invoke(this, null);

        DateTime access = DateTime.Now;
        int i = i0;
        foreach (object value in LoadData(start, end))
        {
          objectDict[i] = new Cache { LastAccess = access, Value = value };
          i++;
        }
        DataLoaded?.Invoke(this, null);
      }
    }

    protected abstract IEnumerable<object> LoadData(int? startIndex, int? endIndex);
    protected abstract IEnumerable<object> LoadDistinctValues(string property, int queryCount);


    bool IBindingList.AllowEdit { get { return AllowEditCore; } }
    protected virtual bool AllowEditCore { get { return false; } }

    bool IBindingList.AllowNew { get { return AllowNewCore; } }
    protected virtual bool AllowNewCore { get { return false; } set { } }

    bool IBindingList.AllowRemove
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    int ICollection.Count { get { return Count; } }
    public int Count
    {
      get { return CountCore; }
    }

    protected abstract int CountCore { get; }


    string IBindingListView.Filter { get { return Filter; } set { Filter = value; } }
    public string Filter
    {
      get { return _filter; }
      set
      {
        _filter = SetFilterCore(value);
      }
    }
    public virtual string SetFilterCore(string filter)
    {
      return filter;
    }

    bool IList.IsFixedSize
    {
      get { throw new NotImplementedException(); }
    }

    bool IList.IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

    bool IBindingList.IsSorted
    {
      get { return _sortDescriptions != null; }
    }

    bool ICollection.IsSynchronized
    {
      get { throw new NotImplementedException(); }
    }

    ListSortDescriptionCollection IBindingListView.SortDescriptions
    {
      get { return _sortDescriptions; }
    }

    ListSortDirection IBindingList.SortDirection
    {
      get
      {
        if (_sortDescriptions == null || _sortDescriptions.Count < 1)
        { return 0; }
        return _sortDescriptions[0].SortDirection;
      }
    }

    PropertyDescriptor IBindingList.SortProperty
    {
      get
      {
        if (_sortDescriptions == null || _sortDescriptions.Count < 1)
        { return null; }
        return _sortDescriptions[0].PropertyDescriptor;
      }
    }

    bool IBindingListView.SupportsAdvancedSorting
    {
      get { throw new NotImplementedException(); }
    }

    bool IBindingList.SupportsChangeNotification
    {
      get { return true; }
    }

    bool IBindingListView.SupportsFiltering
    {
      get { return true; }
    }

    bool IBindingList.SupportsSearching
    {
      get { throw new NotImplementedException(); }
    }

    bool IBindingList.SupportsSorting
    {
      get { return true; }
    }

    object ICollection.SyncRoot
    {
      get { throw new NotImplementedException(); }
    }

    int IList.Add(object value)
    {
      throw new NotImplementedException();
    }

    void IBindingList.AddIndex(PropertyDescriptor property)
    {
      throw new NotImplementedException();
    }

    object IBindingList.AddNew()
    {
      throw new NotImplementedException();
    }

    void IList.Clear()
    {
      Reset();
    }

    bool IList.Contains(object value)
    {
      throw new NotImplementedException();
    }

    void ICollection.CopyTo(Array array, int index)
    {
      throw new NotImplementedException();
    }

    int IBindingList.Find(PropertyDescriptor property, object key)
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }

    int IList.IndexOf(object value)
    {
      throw new NotImplementedException();
    }

    void IList.Insert(int index, object value)
    {
      throw new NotImplementedException();
    }

    void IList.Remove(object value)
    {
      throw new NotImplementedException();
    }

    void IList.RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    void IBindingListView.RemoveFilter()
    {
      throw new NotImplementedException();
    }

    void IBindingList.RemoveIndex(PropertyDescriptor property)
    {
      throw new NotImplementedException();
    }

    void IBindingList.RemoveSort()
    {
      throw new NotImplementedException();
    }

    protected virtual string GetMapping(string property)
    {
      return property;
    }

  }

  public abstract class PartialListView<T> : PartialListView, IList<T>
  {
    T IList<T>.this[int index]
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    protected override object GetDefaultValue()
    {
      return default(T);
    }

    int ICollection<T>.Count
    {
      get { return CountCore; }
    }

    bool ICollection<T>.IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

    void ICollection<T>.Add(T item)
    {
      throw new NotImplementedException();
    }

    void ICollection<T>.Clear()
    {
      throw new NotImplementedException();
    }

    bool ICollection<T>.Contains(T item)
    {
      throw new NotImplementedException();
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      throw new NotImplementedException();
    }

    int IList<T>.IndexOf(T item)
    {
      throw new NotImplementedException();
    }

    void IList<T>.Insert(int index, T item)
    {
      throw new NotImplementedException();
    }

    bool ICollection<T>.Remove(T item)
    {
      throw new NotImplementedException();
    }

    void IList<T>.RemoveAt(int index)
    {
      throw new NotImplementedException();
    }
  }
}

