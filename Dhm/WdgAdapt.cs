using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Basics.Data;
using Basics.Forms;
using Basics.Geom;
using TMap;

namespace Dhm
{
  public partial class WdgAdapt : Form
  {
    private readonly string _ocdPath;
    private readonly string _fullPath;
    private readonly ContourSorter.ProgressEventArgs _args;
    private readonly IContext _context;
    private TableMapData _data;

    public WdgAdapt()
    {
      InitializeComponent();
      Load += WdgAdapt_Load;
    }

    public WdgAdapt(string dataPath, ContourSorter.ProgressEventArgs args, IContext context)
      : this()
    {
      _ocdPath = dataPath;
      _fullPath = System.IO.Path.Combine(dataPath, Ocad.Data.OcadConnection.TableElements);
      _args = args;
      _context = context;
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
    }

    private void WdgAdapt_Load(object sender, EventArgs e)
    {
      if (_context == null || _args == null)
      { return; }
      if (!(_args.Contour is ContourSorter.InfoContour contour))
      { return; }

      grdContours.AutoGenerateColumns = false;
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
        col.HeaderText = "Id";
        col.DataPropertyName = "Id";
        grdContours.Columns.Add(col);
      }

      List<Contour> contours = new List<Contour>();
      contours.Add(contour);
      foreach (var involved in contour.Involveds)
      {
        if (involved == null)
        { continue; }

        contours.Add(involved);
      }

      grdContours.DataSource = contours;

      IBox box = contour.Polyline.Extent;
      Basics.Geom.Point min = Basics.Geom.Point.CastOrCreate(box.Min);
      Basics.Geom.Point max = Basics.Geom.Point.CastOrCreate(box.Max);
      if (min.Dist2(max) < 2000)
      {
        Basics.Geom.Point m = 0.5 * (min + max);
        Point2D d = new Point2D(25, 25);
        box = new Box(m - d, m + d);
      }
      _context.SetExtent(box);
      _context.Refresh();

      foreach (var data in _context.Data.GetAllData())
      {
        if (!(data is TableMapData tMap))
        { continue; }
        if (tMap.Data.Path.Equals(_fullPath, StringComparison.InvariantCultureIgnoreCase))
        {
          _data = tMap;
        }
      }
      if (_data == null)
      {
        _data = (TableMapData)GraphicMapData.FromFile(_ocdPath);
        _context.Data.Subparts.Add(_data);
        _context.Refresh();
      }

      grdJoinOptions.AutoGenerateColumns = false;
      grdJoinOptions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
        col.HeaderText = "Text";
        col.DataPropertyName = "Text";
        grdJoinOptions.Columns.Add(col);
      }
      grdJoinOptions.DataSource = GetJoinOptions(contours);
      btnJoin.Enabled = grdJoinOptions.Rows.Count > 0;

      _context.Draw();
      Redraw();
    }

    private List<JoinOption> GetJoinOptions(List<Contour> contours)
    {
      List<Contour> joins = new List<Contour>();
      foreach (var c in contours)
      {
        if (c == null || c.Id < 0)
        { continue; }
        joins.Add(c);
      }
      DbBaseCommand cmd = _data.Data.GetSelectCmd(null, "*");
      IList<DataColumn> keys = _data.Data.FullSchema.PrimaryKey;
      if (keys == null || keys.Count != 1)
      { return new List<JoinOption>(); }

      DataColumn key = keys[0];
      StringBuilder sb = new StringBuilder();
      foreach (var join in joins)
      {
        if (sb.Length > 0)
        { sb.Append(","); }
        sb.AppendFormat("{0}", join.Id);
      }
      cmd.CommandText = string.Format("{0} WHERE {1} IN ({2})", cmd.CommandText, key.ColumnName, sb);
      DbBaseAdapter adapter = cmd.Connection.CreateAdapter();
      adapter.SelectCommand = cmd;

      DataTable tbl = new DataTable();
      adapter.Fill(tbl);
      if (tbl.Rows.Count != 2)
      { return new List<JoinOption>(); }

      Polyline l0 = (Polyline)Ocad.Data.OcadConnection.FieldShape.GetValue(tbl.Rows[0]);
      Polyline l1 = (Polyline)Ocad.Data.OcadConnection.FieldShape.GetValue(tbl.Rows[1]);
      IEnumerable<ParamGeometryRelation> rels = GeometryOperator.CreateRelations(l0, l1);

      List<JoinOption> options = new List<JoinOption>();
      if (rels == null)
      { 
        return options; 
      }
      foreach (var rel in rels)
      {
        JoinOption option = new JoinOption(this);
        option.Row0 = tbl.Rows[0];
        option.Row1 = tbl.Rows[1];
        option.Relation = rel;

        options.Add(option);
      }
      return options;
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void BtnZoom_Click(object sender, EventArgs e)
    {
      ZoomTo();
    }

    private void ZoomTo()
    {
      Box box = null;
      foreach (var row in grdContours.SelectedRows.Enum())
      {
        if (!(row.DataBoundItem is Contour c))
        { continue; }

        if (box == null)
        { box = new Box(c.Polyline.Extent); }
        else
        { box.Include(c.Polyline.Extent); }
      }
      _context.SetExtent(box);
      _context.Draw();

      Redraw();
    }

    private void BtnRedraw_Click(object sender, EventArgs e)
    {
      Redraw();
    }

    private void Redraw()
    {
      IDrawable draw = _context.Maps[0];

      foreach (var row in grdContours.Rows.Enum())
      {
        if (!(row.DataBoundItem is Contour c))
        { continue; }

        SymbolPart lineSymbol = new SymbolPartLine();
        Color clr;
        if (row.Selected)
        { clr = Color.FromArgb(0, 0, 255); }
        else
        { clr = Color.FromArgb(255, 0, 0); }
        lineSymbol.Color = clr;
        draw.DrawLine(c.Polyline.Project(draw.Projection),
                      lineSymbol);
      }

      foreach (var row in grdJoinOptions.Rows.Enum())
      {
        if (!(row.DataBoundItem is JoinOption c))
        { continue; }

        Color clr;
        if (row.Selected)
        { clr = Color.FromArgb(0, 0, 255); }
        else
        { clr = Color.FromArgb(255, 0, 0); }

        IGeometry geom = c.Geometry;
        if (geom is IPoint p)
        {
          SymbolPartPoint pointSymbol = new SymbolPartPoint();
          pointSymbol.SymbolLine = Symbol.SquareLine();
          pointSymbol.Color = clr;
          draw.DrawLine(pointSymbol.PointLine(p, draw), pointSymbol);
        }
      }

      draw.Flush();
    }

    private void BtnJoin_Click(object sender, EventArgs e)
    {
      JoinOption j = null;
      foreach (var row in grdJoinOptions.SelectedRows.Enum())
      {
        j = row.DataBoundItem as JoinOption;
        break;
      }
      if (j != null)
      { j.Join(); }
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
      try
      {
        _data.Data.SaveEdits();
      }
      catch (Exception exp)
      { MessageBox.Show(Basics.Utils.GetMsg(exp)); }
    }

    private class JoinOption
    {
      private readonly WdgAdapt _parent;
      public ParamGeometryRelation Relation;
      public DataRow Row0;
      public DataRow Row1;
      public JoinOption(WdgAdapt parent)
      {
        _parent = parent;
      }

      public string Text
      {
        get
        {
          string txt = string.Format(
            "{0}-{1}",
            Row0[Ocad.Data.OcadConnection.FieldId],
            Row1[Ocad.Data.OcadConnection.FieldId]);
          return txt;
        }
      }

      public IGeometry Geometry
      {
        get { return Relation.Intersection; }
      }

      public void Join()
      {
        Polyline l0 = (Polyline)Ocad.Data.OcadConnection.FieldShape.GetValue(Row0);
        Polyline l1 = (Polyline)Ocad.Data.OcadConnection.FieldShape.GetValue(Row1);

        ParamGeometryRelation[] rels = new ParamGeometryRelation[] { Relation };

        IList<Polyline> parts0 = l0.Split(rels);
        Polyline part0 = (parts0[0].Length() > parts0[1].Length()) ? parts0[0] : parts0[1].Invert();
        IList<Polyline> parts1 = l1.Split(rels);
        Polyline part1 = (parts1[0].Length() > parts1[1].Length()) ? parts1[0].Invert() : parts1[1];
        foreach (var seg in part1.EnumSegments())
        { part0.Add(seg); }
        Ocad.Data.OcadConnection.FieldShape.SetValue(Row0, part0);

        _parent._data.Data.Update(Row0);
        _parent._data.Data.Delete(Row1);

      }
    }
  }
}