using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Basics.Data;
using TMap;
using TMapWin.Browse;

namespace TMapWin
{
  public partial class WdgMain : Form, IContext
  {
    public WdgMain()
    {
      InitializeComponent();
    }

    void CurrentView_Change(object sender, EventArgs e)
    {
      Redraw();
    }

    public GroupMapData VisibleList()
    {
      return wdgToc.MapData.VisibleList();
    }
    public IList<IDrawable> Maps
    {
      get { return new IDrawable[] { wdgMap }; }
    }
    public GroupMapData Data
    {
      get { return wdgToc.MapData; }
    }

    void IContext.SetExtent(Basics.Geom.IBox extent)
    {
      wdgMap.SetExtent(extent);
    }
    void IContext.Refresh()
    {
      wdgToc.BuildTree();
    }
    void IContext.Draw()
    {
      Redraw();
    }

    private class _Draw : IDrawable
    {
      private CntMap _map;
      private bool _breakDraw;
      public bool BreakDraw
      {
        get { return _breakDraw; }
        set { _breakDraw = value; }
      }
      public _Draw(CntMap map)
      {
        _map = map;
      }
      public void BeginDraw()
      { _map.BeginDraw(); }
      public void BeginDraw(MapData mapData)
      {
        if (!_breakDraw) _map.BeginDraw(mapData);
      }
      public void BeginDraw(ISymbolPart symbolPart)
      { _map.BeginDraw(symbolPart); }
      public Basics.Geom.IProjection Projection
      { get { return _map.Projection; } }
      public void DrawLine(Basics.Geom.Polyline line, ISymbolPart symbolPart)
      { _map.DrawLine(line, symbolPart); }
      public void DrawArea(Basics.Geom.Area area, ISymbolPart symbolPart)
      { _map.DrawArea(area, symbolPart); }
      public void DrawRaster(GridMapData raster)
      { _map.DrawRaster(raster); }
      public void Draw(MapData data)
      { data.Draw(this); }
      public void Flush()
      { _map.Flush(); }
      public Basics.Geom.Box Extent
      { get { return _map.Extent; } }
      public void SetExtent(Basics.Geom.IBox proposedExtent)
      { _map.SetExtent(proposedExtent); }
      public void EndDraw(ISymbolPart symbolPart)
      { _map.EndDraw(symbolPart); }
      public void EndDraw(MapData mapData)
      { _map.EndDraw(mapData); }
      public void EndDraw()
      { _map.EndDraw(); }
    }
    private List<_Draw> _drawList = new List<_Draw>();
    internal void Redraw()
    {
      foreach (_Draw draw in _drawList)
      { draw.BreakDraw = true; }
      _drawList.Clear();

      _Draw redraw = new _Draw(wdgMap);
      _drawList.Add(redraw);

      TimeSpan t0 = Process.GetCurrentProcess().TotalProcessorTime;
      redraw.BeginDraw();
      redraw.Draw(wdgToc.GetSortedMapData().VisibleList());
      redraw.EndDraw();
      TimeSpan t1 = Process.GetCurrentProcess().TotalProcessorTime;
      Debug.WriteLine((t1 - t0).TotalSeconds);
    }

    private bool OpenData(string fileName)
    {
      GC.Collect();
      MapData data = GraphicMapData.FromFile(fileName);
      if (data == null)
      {
        MessageBox.Show("Unknown Dataset\n" + fileName, "Unknown Dataset");
        return false;
      }
      wdgToc.MapData.Subparts.Add(data);
      wdgToc.BuildTree();
      return true;
    }


    #region context events
    private void OptZoomIn_SelectionEnd(object sender, ToolArgs e)
    {
      if (e.IsPoint == false)
      { wdgMap.SetExtent(new Basics.Geom.Box(e.Start, e.End, true)); }
      else
      { wdgMap.SetExtent(e.Middle, 2); }
      Redraw();
    }

    private void OptZoomOut_SelectionEnd(object sender, ToolArgs e)
    {
      if (e.IsPoint == false)
      {
        Basics.Geom.Point p = 0.5 * (e.Start + e.End);
        double f;
        double fx, fy;
        if (wdgMap.Extent != null)
        {
          fx = Math.Abs((e.Start.X - e.End.X) /
                               (wdgMap.Extent.Max.X - wdgMap.Extent.Min.X));

          fy = Math.Abs((e.Start.Y - e.End.Y) /
                               (wdgMap.Extent.Max.Y - wdgMap.Extent.Min.Y));
        }
        else
        {
          fx = Math.Abs((e.Start.X - e.End.X) / wdgMap.Width);
          fy = Math.Abs((e.Start.Y - e.End.Y) / wdgMap.Height);
        }
        if (fx > fy)
        { f = fx; }
        else
        { f = fy; }
        wdgMap.SetExtent(p, f);
      }
      else
      { wdgMap.SetExtent(e.Middle, 0.5); }
      Redraw();
    }

    private void OptMove_SelectionEnd(object sender, ToolArgs e)
    {
      if (e.IsPoint == false)
      {
        Basics.Geom.Point move = e.Start - e.End;

        wdgMap.SetExtent(new Basics.Geom.Box(wdgMap.Extent.Min + move,
          wdgMap.Extent.Max + move, false));
      }

      Redraw();
    }


    private void OptSelect_SelectionEnd(object sender, ToolArgs e)
    {
      wdgSel.Clear();
      foreach (MapData mapData in wdgToc.MapData.VisibleList().Subparts)
      {
        if (mapData is TableMapData == false)
        { continue; }

        TableMapData grData = (TableMapData)mapData;
        TData.TTable tbl = grData.Data;
        if (tbl == null)
        { continue; }

        string sMap = "";
        DbBaseCommand cmd = tbl.GetSelectCmd(
          new TData.GeometryQuery(grData.Symbolisation.GeometryColumn,
          new Basics.Geom.Box(e.Start, e.End, true), Basics.Geom.Relation.Intersect),
          null);
        DbBaseAdapter adapter = cmd.Connection.CreateAdapter();
        adapter.SelectCommand = cmd;
        DataTable selection = new DataTable();
        adapter.Fill(selection);
        foreach (DataRow row in selection.Rows)
        {
          if (sMap == "")
          {
            sMap = mapData.Name;
            wdgSel.Add(mapData);
          }
          wdgSel.Add(row);
        }
      }
      wdgSel.InitSetting();
      wdgSel.Map = this;
    }
    #endregion contextevents

    #region events

    private void MnuLoad_Click(object sender, EventArgs e)
    {
      string fileName;
      using (WdgOpen open = new WdgOpen())
      {
        if (open.ShowDialog(this) != DialogResult.OK)
        { return; }
        fileName = open.FileName;
      }
      //if (dlgDsOpen.ShowDialog() != DialogResult.OK)
      //{ return;}
      //string fileName = dlgDsOpen.FileName;

      try
      {
        if (OpenData(fileName))
        {
          if (wdgMap.Extent == null)
          { wdgMap.SetExtent(wdgToc.MapData.Extent); }
          Redraw();
        }
        //else
        //{ e.Cancel = true; }
      }
      catch (Exception exp)
      {
        MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
        //e.Cancel = true;
      }
    }

    private void TINTest1()
    {
      // remove reference to TMap ?,... when working
      Basics.Geom.Mesh mesh = new Basics.Geom.Mesh();
      mesh.Fuzzy = 0.01;

      mesh.Add(new Basics.Geom.Point2D(1, 0));
      mesh.Add(new Basics.Geom.Point2D(1.0, 0.0));
      mesh.Add(new Basics.Geom.Point2D(3, 0));
      mesh.Add(new Basics.Geom.Point2D(2, 0));
      mesh.Add(new Basics.Geom.Point2D(2.005, 0.005));
      mesh.Add(new Basics.Geom.Point2D(4, 0));
      mesh.Add(new Basics.Geom.Point2D(0, 0));

      mesh.Add(new Basics.Geom.Point2D(1.5, 2));
      mesh.Add(new Basics.Geom.Point2D(1.505, 2.005));
      mesh.Add(new Basics.Geom.Point2D(2.5, 2));
      mesh.Add(new Basics.Geom.Point2D(3.5, 2));

      mesh.Add(new Basics.Geom.Point2D(2, 1));

      mesh.Add(new Basics.Geom.Point2D(1.5, -1));

      mesh.InsertSeg(new Basics.Geom.Point2D(2, 1), new Basics.Geom.Point2D(1.5, -1),
        1, InsertFct);
      mesh.InsertSeg(new Basics.Geom.Point2D(2, 0.5), new Basics.Geom.Point2D(3, 0.5),
        1, InsertFct);

      try
      {
        TData.TTable pDat = TData.TTable.FromData(mesh);
        MapData mapData = new TableMapData("mesh", pDat);
        wdgToc.MapData.Subparts.Add(mapData);
        Redraw();
      }
      catch (Exception exp)
      {
        MessageBox.Show(exp.Message + "\n" + exp.StackTrace);
      }
    }

    private Basics.Geom.Point2D InsertFct(Basics.Geom.Mesh m, Basics.Geom.Line insertLine, object insertLineInfo,
      Basics.Geom.Mesh.MeshLine cutLine, double fCut)
    {
      Basics.Geom.Point start = Basics.Geom.Point.CastOrCreate(cutLine.Start);
      Basics.Geom.Point p = start + fCut * (cutLine.End - start);
      return (Basics.Geom.Point2D)p;
    }

    private void AddPlugin(Attribute.TblICustomRow plugin)
    {
      if (Settings.Default.Plugins == null)
      {
        Settings.Default.Plugins = new Attribute();
      }
      if (plugin.Table != Settings.Default.Plugins.TblICustom)
      {
        Attribute.TblICustomRow n = Settings.Default.Plugins.TblICustom.NewTblICustomRow();
        n.Assembly = plugin.Assembly;
        n.Type = plugin.Type;
        n.Name = plugin.Name;
        Settings.Default.Plugins.TblICustom.AddTblICustomRow(n);
        Settings.Default.Plugins.TblICustom.AcceptChanges();

        plugin = n;
      }
      int pos = mniPlugins.DropDownItems.IndexOf(mniPluginSepNew);
      ToolStripMenuItem newPlugin = new ToolStripMenuItem(plugin.Name);
      newPlugin.Tag = plugin;
      newPlugin.Click += Plugin_Click;
      mniPlugins.DropDownItems.Insert(pos, newPlugin);
    }

    private bool VerifySchema(DataTable t0, DataTable t1)
    {
      foreach (DataColumn col in t1.Columns)
      {
        if (t0.Columns.Contains(col.ColumnName) == false)
        {
          return false;
        }
      }
      return true;
    }

    private void RunPlugin(Attribute.TblICustomRow runPlugin)
    {
      System.Reflection.Assembly assembly =
        System.Reflection.Assembly.LoadFile(runPlugin.Assembly);

      ICommand cmd;
      Type t = assembly.GetType(runPlugin.Type);
      cmd = (ICommand)Activator.CreateInstance(t);
      cmd.Execute(this);

      if (cmd is ITool tool)
      {
        _currentTool = tool;
        wdgMap.ToolEnd = PluginToolEnd;
        wdgMap.ToolMove = PluginToolMove;
      }
    }

    private ITool _currentTool;
    private void PluginToolMove(object sender, ToolArgs e)
    {
      if (_currentTool == null)
      { return; }
      _currentTool.ToolMove(e.Position, e.Mouse, e.Start, e.End);
    }

    private void PluginToolEnd(object sender, ToolArgs e)
    {
      if (_currentTool == null)
      { return; }
      _currentTool.ToolEnd(e.Start, e.End);
    }

    private void Plugin_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem iPlugin = (ToolStripMenuItem)sender;
      Attribute.TblICustomRow rPlugin = (Attribute.TblICustomRow)iPlugin.Tag;
      RunPlugin(rPlugin);
    }

    private void WdgMap_Resize(object sender, EventArgs e)
    {
      if (((Form)wdgMap.TopLevelControl).WindowState == FormWindowState.Minimized)
      { return; }
      wdgMap.SetExtent(null, 1);
      Settings.Default.WdgMain_Location = Location;
      Settings.Default.WdgMain_Size = Size;
      Redraw();
    }

    private void MnuRefresh_Click(object sender, EventArgs e)
    {
      Redraw();
    }

    private void OptZoomIn_Click(object sender, EventArgs e)
    {
      wdgMap.ToolMove = wdgMap.ToolDownHandler(MouseDownStyle.Box);
      wdgMap.ToolEnd = OptZoomIn_SelectionEnd;
    }

    private void OptZoomOut_Click(object sender, EventArgs e)
    {
      wdgMap.ToolMove = wdgMap.ToolDownHandler(MouseDownStyle.Box);
      wdgMap.ToolEnd = OptZoomOut_SelectionEnd;
    }

    private void OptMove_Click(object sender, EventArgs e)
    {
      wdgMap.ToolMove = wdgMap.ToolMovePanelHandler();
      wdgMap.ToolEnd = OptMove_SelectionEnd;
    }

    private void OptSelect_Click(object sender, EventArgs e)
    {
      wdgMap.ToolMove = wdgMap.ToolDownHandler(MouseDownStyle.Box);
      wdgMap.ToolEnd = OptSelect_SelectionEnd;
    }

    private void WdgMap_MouseMove(object sender, MouseEventArgs e)
    {
      if (wdgMap.Extent == null)
      {
        txtPosition.Visible = false;
        return;
      }

      txtPosition.Visible = true;
      Basics.Geom.Point2D pos = wdgMap.InvProject(e.X, e.Y);
      txtPosition.Text = string.Format("{0:N2};{1:N2}", pos.X, pos.Y);
    }

    private void Data_DragDrop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        // Assign the file names to a string array, in 
        // case the user has selected multiple files.
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (OpenData(files[0]))
        { Redraw(); }
      }
    }

    private void Data_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      { e.Effect = DragDropEffects.Copy; }
      else
      { e.Effect = DragDropEffects.None; }
    }

    private void WdgToc_ListChanged(object sender, EventArgs e)
    {
      Redraw();
    }

    private void MnuNew_Click(object sender, EventArgs e)
    {
      WdgCustom wdgCustom = new WdgCustom();
      if (wdgCustom.ShowDialog(this) != DialogResult.OK)
      { return; }

      foreach (Attribute.TblICustomRow addPlugin in wdgCustom.AddPlugins())
      {
        AddPlugin(addPlugin);
      }

      Attribute.TblICustomRow runPlugin = wdgCustom.RunPlugin();
      if (runPlugin != null)
      {
        RunPlugin(runPlugin);
      }
    }

    private void WdgMain_DragOver(object sender, DragEventArgs e)
    {
      Point off = PointToScreen(new Point(0, 0));
      Point qry = new Point(e.X - off.X, e.Y - off.Y);
      Control c = GetChildAtPoint(qry);
      if (c != null)
      {
        Debug.WriteLine(c.Name);
        if (c == mainMenu)
        {
          qry.X = qry.X - mainMenu.Left;
          qry.Y = qry.Y - mainMenu.Top;
          foreach (ToolStripMenuItem item in mainMenu.Items)
          {
            if (item.Bounds.Contains(qry))
            {
              item.Select();
              item.ShowDropDown();
              break;
            }
          }
        }
      }
      else
      {
        Debug.WriteLine(qry.X + " " + qry.Y);
      }
    }

    private void WdgMain_Load(object sender, EventArgs e)
    {
      Location = Settings.Default.WdgMain_Location;
      if (Settings.Default.WdgMain_Size.Width > 0)
      {
        Size = Settings.Default.WdgMain_Size;
      }
      if (Settings.Default.Plugins != null &&
        Settings.Default.Plugins.TblICustom != null)
      {
        if (VerifySchema(Settings.Default.Plugins.TblICustom,
          new Attribute.TblICustomDataTable()) == false)
        {
          Settings.Default.Plugins = new Attribute();
        }
        foreach (Attribute.TblICustomRow plugin in
          Settings.Default.Plugins.TblICustom)
        {
          try
          {
            AddPlugin(plugin);
          }
          catch
          {
            plugin.Delete();
          }
        }
      }
    }

    #endregion events
  }
}