using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TData;
using TMapWin.Browse;

namespace TMapWin.Browse
{
  public partial class WdgBrowse : Form
  {
    public event ItemHandler AddingItem;
    public event ValueCancelHandler<string> ChangingDir;

    private static string _lastDir;
    private string _dir;
    private object _currentDataset;
    private bool _created;

    private bool _init;
    string _oldName;
    private ImageList _smallImageList;

    private Dictionary<ListViewItem, DatasetInfo> _dictData;

    public WdgBrowse()
    {
      InitializeComponent();
      _dictData = new Dictionary<ListViewItem, DatasetInfo>();

      if (_lastDir == null)
      {
        _lastDir = Settings.Default.BrowsePath;
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);

      Settings.Default.BrowsePath = _lastDir;
      Settings.Default.Save();
    }

    public string CurrentDirectory
    {
      get { return _dir; }
    }

    public object CurrentDataset
    {
      get { return _currentDataset; }
    }

    public void SetDir(string dir)
    {
      if (dir == null)
      { dir = ""; }
      string newDir;
      if (dir != "..")
      {
        newDir = dir;
      }
      else
      {
        newDir = Path.GetFullPath(_dir);
        newDir = Path.GetDirectoryName(newDir);
        if (newDir == null)
        { newDir = ""; }
      }

      if (ChangingDir != null)
      {
        ValueCancelEventArgs<string> args = new ValueCancelEventArgs<string>(newDir);
        ChangingDir(this, args);
        if (args.Cancel)
        { return; }
      }

      _dir = newDir;
      _lastDir = _dir;

      if (_created && Visible)
      {
        _currentDataset = UpdateList();
      }

      OnDirSet();
    }

    protected virtual void OnDirSet()
    {
    }

    protected virtual void OnSelectedIndexChanged()
    { }

    protected virtual void OnOpen(string text)
    { }

    protected ListView ListData
    {
      get { return lstData; }
    }
    protected void OnAddingItem(ItemArgs args)
    {
      if (AddingItem != null)
      { AddingItem(this, args); }
    }

    protected void AddItem(string item, object dsName, DataType dataType)
    {
      ItemArgs args = new ItemArgs(item, dataType);
      OnAddingItem(args);
      if (args.Cancel)
      { return; }

      string name = Path.GetFileName(item);
      if (string.IsNullOrEmpty(name))
      { name = item; }

      ListViewItem addItem = lstData.Items.Add(name);
      _dictData.Add(addItem, new DatasetInfo(item, dsName, dataType));

      SetImageIndex(addItem, dataType);
    }

    private void SetImageIndex(ListViewItem item, DataType dataType)
    {
      if (item.ListView.SmallImageList == null)
      {
        item.ListView.SmallImageList = SmallImageList;
      }
      item.ImageKey = dataType.ToString();
    }

    private ImageList SmallImageList
    {
      get
      {
        if (_smallImageList == null)
        {
          _smallImageList = new ImageList();
          _smallImageList.Images.Add(DataType.Folder.ToString(), Images.folder_closed);
          _smallImageList.Images.Add(DataType.FileGdb.ToString(), Images.db);
          _smallImageList.Images.Add(DataType.PersGdb.ToString(), Images.db);
          _smallImageList.Images.Add(DataType.SdeDb.ToString(), Images.db);
          _smallImageList.Images.Add(DataType.Dataset.ToString(), Images.db_set);
          _smallImageList.Images.Add(DataType.Table.ToString(), Images.db_tbl);
          _smallImageList.Images.Add(DataType.PointFc.ToString(), Images.db_point);
          _smallImageList.Images.Add(DataType.PolyFc.ToString(), Images.db_poly);
          _smallImageList.Images.Add(DataType.LineFc.ToString(), Images.db_line);
          _smallImageList.Images.Add(DataType.UnknownFc.ToString(), Images.db_geom);

          _smallImageList.Images.Add(DataType.Relation.ToString(), Images.db_rel);
          _smallImageList.Images.Add(DataType.Network.ToString(), Images.db_net);
          _smallImageList.Images.Add(DataType.Raster.ToString(), Images.db_grid);
          _smallImageList.Images.Add(DataType.Tin.ToString(), Images.db_tin);
          _smallImageList.Images.Add(DataType.Measure.ToString(), Images.db_meas);
          _smallImageList.Images.Add(DataType.Multipatch.ToString(), Images.db_patch);
        }
        return _smallImageList;
      }
    }

    private object UpdateList()
    {
      Cursor origCrs = Cursor;
      try
      {
        Cursor = Cursors.WaitCursor;

        if (_dir == null)
        { return null; }

        lstData.Clear();
        _dictData.Clear();

        SetParents();

        if (_dir == "")
        {
          DriveInfo[] allDrives = DriveInfo.GetDrives();
          foreach (DriveInfo drive in allDrives)
          {
            AddItem(drive.Name, null, DataType.Folder);
          }
          return null;
        }

        List<DatasetInfo> objList = TData.Browse.GetObjects(_dir);

        foreach (DatasetInfo info in objList)
        {
          AddItem(info.Name, info.DsName, info.DataType);
        }
        object dirDs = null;

        _currentDataset = dirDs;
        return dirDs;
      }
      finally
      {
        Cursor = origCrs;
      }
    }

    private bool Rename(ListViewItem item, string newLabel)
    {
      throw new NotImplementedException();
      /*
      if (string.IsNullOrEmpty(newLabel))
      { return false; }
      if (_oldName == newLabel)
      { return false; }

      string oldName = Path.Combine(_dir, _oldName);
      string newName = Path.Combine(_dir, newLabel);

      if (Directory.Exists(oldName) &&
        Directory.Exists(newName) == false && File.Exists(newName) == false)
      {
        Directory.Move(oldName, newName);
        _dictData[item] = new DatasetInfo(newName, null);
        return true;
      }
      if (CurrentDataset == null)
      {
        return false;
      }
      bool success = false;
      IEnumDataset subsets = CurrentDataset.Subsets;
      subsets.Reset();
      for (IDataset subset = subsets.Next(); subset != null; subset = subsets.Next())
      {
        if (subset.Name.Equals(_oldName, StringComparison.InvariantCultureIgnoreCase))
        {
          if (subset.CanRename())
          {
            subset.Rename(newLabel);
            _dictData[item] = new DatasetInfo(newLabel, subset.FullName as IDatasetName);
            success = true;
            break;
          }
        }
      }

      subsets.Reset();
      return success;
       */
    }

    private void SetParents()
    {
      bool origInit = _init;
      try
      {
        _init = true;
        lstParents.Items.Clear();

        List<string> parents = TData.Browse.GetParents(_dir);

        foreach (string parent in parents)
        {
          lstParents.Items.Add(parent);
        }

        lstParents.SelectedIndex = lstParents.Items.Count - 1;
      }
      finally
      { _init = origInit; }
    }

    private void WdgOpen_Load(object sender, EventArgs e)
    {
      _created = true;
      SetDir(_lastDir);
    }

    private void WdgOpen_VisibleChanged(object sender, EventArgs e)
    {
      if (Visible)
      {
        SetDir(_lastDir);
      }
    }

    private void _lstParents_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_init == false)
      {
        if (lstParents.SelectedIndex == 0 && lstParents.SelectedItem as string == TData.Browse.Arbeitsplatz)
        {
          SetDir("");
        }
        else
        {
          string dir = (string)lstParents.Items[1];
          for (int i = 2; i <= lstParents.SelectedIndex; i++)
          {
            dir = Path.Combine(dir, (string)lstParents.Items[i]);
          }
          SetDir(dir);
        }
      }
    }

    private void _lstData_DoubleClick(object sender, EventArgs e)
    {
      if (lstData.SelectedItems == null || lstData.SelectedItems.Count != 1)
      {
        return;
      }

      ListViewItem item = lstData.SelectedItems[0];
      DatasetInfo dsInfo = _dictData[item];
      if (dsInfo.DataType != DataType.Folder
        && dsInfo.DataType != DataType.FileGdb
        && dsInfo.DataType != DataType.PersGdb
        && dsInfo.DataType != DataType.Dataset)
      {
        OnOpen(item.Text);
        return;
      }

      string name = item.Text;
      string fullName = Path.Combine(_dir, name);

      SetDir(fullName);
    }

    private void _lstData_BeforeLabelEdit(object sender, LabelEditEventArgs e)
    {
      _oldName = lstData.Items[e.Item].Text;
    }

    private void _lstData_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      if (Rename(lstData.Items[e.Item], e.Label) == false)
      { e.CancelEdit = true; }

      _oldName = null;
    }

    private void _lstData_SelectedIndexChanged(object sender, EventArgs e)
    {
      StringBuilder itemList = new StringBuilder();
      foreach (ListViewItem item in ListData.SelectedItems)
      {
        if (itemList.Length > 0)
        { itemList.Append("; "); }
        itemList.Append(item.Text);
      }
      txtNew.Text = itemList.ToString();
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
      if (ListData.SelectedItems != null && ListData.SelectedItems.Count == 1)
      {
        ListViewItem selItem = ListData.SelectedItems[0];

        if (selItem.Text == txtNew.Text)
        {
          DatasetInfo selInfo = _dictData[selItem];
          if (selInfo.DsName == null)
          {
            SetDir(selInfo.Name);
            return;
          }
          throw new NotImplementedException();
          //if (selInfo.DsName.Type == esriDatasetType.esriDTFeatureDataset)
          //{
          //  string fullName = Path.Combine(_dir, selInfo.Name);
          //  SetDir(fullName);
          //  return;
          //}
        }
      }
      if (string.IsNullOrEmpty(txtNew.Text))
      {
        return;
      }

      OnOpen(txtNew.Text);
    }
    #region nested classes
    public delegate void ValueCancelHandler<T>(object sender, ValueCancelEventArgs<T> args);
    public delegate void ItemHandler(object sender, ItemArgs args);

    public class ValueCancelEventArgs<T> : CancelEventArgs
    {
      private T _value;

      public ValueCancelEventArgs(T value)
      {
        _value = value;
      }

      public T Value
      {
        get { return _value; }
      }
    }

    public class ItemArgs : CancelEventArgs
    {
      private string _name;
      private DataType _dataType;

      public ItemArgs(string name, DataType dataType)
      {
        _name = name;
        _dataType = dataType;
      }

      public string Name
      {
        get { return _name; }
      }
      public DataType DataType
      {
        get { return _dataType; }
      }
    }

    #endregion
  }
}