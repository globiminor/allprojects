namespace TMap
{
  public class GroupMapData : MapData
  {
    public event System.EventHandler Change;

    private System.Collections.Generic.List<MapData> _subparts = new System.Collections.Generic.List<MapData>();

    public GroupMapData(string name)
      : base(name)
    { }
    public System.Collections.Generic.List<MapData> Subparts
    {
      get { return _subparts; }
    }
    public override void Draw(IDrawable drawable)
    {
      for (int iPart = _subparts.Count - 1; iPart >= 0; iPart--)
      {
        drawable.BeginDraw(_subparts[iPart]);
        _subparts[iPart].Draw(drawable);
        drawable.EndDraw(_subparts[iPart]);
      }
    }
    public override Basics.Geom.IBox Extent
    {
      get
      {
        Basics.Geom.IBox box = null;
        foreach (MapData data in _subparts)
        {
          if (box == null)
          { box = data.Extent.Clone(); }
          else
          { box.Include(data.Extent); }
        }
        return box;
      }
    }

    public void Refresh()
    {
      if (Change != null)
      { Change(this, new System.EventArgs()); }
    }

    private void AddData(System.Collections.Generic.List<MapData> list, GroupMapData data)
    {
      for (int i = data.Subparts.Count - 1; i >= 0; i--)
      {
        MapData subpart = data.Subparts[i];
        if (subpart.Visible)
        {
          if (subpart is GroupMapData)
          { AddData(list, (GroupMapData)subpart); }

          else if (subpart is GraphicMapData)
          { list.Add(subpart); }
        }
      }
    }
    public GroupMapData VisibleList()
    {
      GroupMapData list = new GroupMapData("Visibles");
      AddData(list._subparts, this);
      return list;
    }
  }
}