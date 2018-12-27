
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Basics.Geom;
using Ocad;
using OCourse.Gui;
using TMap;
using Grid.Lcp;
using Control = Ocad.Control;
using Basics.Forms;

namespace OCourse
{
  public class OCourseCmd : ICommand
  {
    private WdgOCourse _wdg;

    #region ICommand Members

    public void Execute(IContext context)
    {
      if (_wdg == null)
      {
        _wdg = new WdgOCourse();
        Common.Init();
      }
      _wdg.TMapContext = context;
      _wdg.Show();
    }

    #endregion
  }
}

namespace OCourse.Gui
{
  partial class WdgOCourse
  {
    private class CourseMapData : MapData
    {
      readonly WdgOCourse _wdg;
      public CourseMapData(WdgOCourse wdg)
      {
        _wdg = wdg;
      }

      public override IBox Extent
      {
        get { return null; } // TODO
      }

      public override void Draw(IDrawable drawable)
      {
        SymbolPartLine symbol = new SymbolPartLine(null) { LineColor = System.Drawing.Color.Red };

        //drawable.BeginDraw();
        drawable.BeginDraw(symbol);
        Draw(drawable, symbol, _wdg.Vm.Course);
        drawable.EndDraw(symbol);
        //drawable.EndDraw();
        //drawable.Flush();
      }
      private void Draw(IDrawable drawable, SymbolPart symbol,
        SectionCollection sections)
      {
        for (LinkedListNode<ISection> node = sections.First; node != null; node = node.Next)
        {
          ISection section = node.Value;

          if (section is Variation)
          {
            #region Variation
            IList<Control> preControls = _wdg.Vm.Course.PreviousControls(section);
            IList<Control> nextControls = _wdg.Vm.Course.NextControls(section);

            DataGridView dgv = _wdg.dgvPermut;
            List<int> legs = new List<int>();
            if (dgv.Columns.Count > 0 && dgv.Columns[0].Tag == null)
            {
              for (int i = 0; i < dgv.Rows.Count; i++)
              {
                if (dgv.Rows[i].Cells[0].Selected)
                { legs.Add(i + 1); }
              }
            }

            foreach (Variation.Branch branch in ((Variation)section).Branches)
            {
              if (legs.Count > 0)
              {
                bool notSelected = true;
                foreach (var leg in branch.Legs)
                {
                  if (legs.Contains(leg))
                  {
                    notSelected = false;
                    break;
                  }
                }
                if (notSelected)
                { continue; }
              }

              if (preControls != null && preControls.Count == 1 &&
                branch.First != null && branch.First.Value is Control)
              {
                Polyline p = new Polyline();
                AddPoint(p, preControls[0], true);
                AddPoint(p, (Control)branch.First.Value, false);

                drawable.DrawLine(p.Project(drawable.Projection), symbol);
              }
              Draw(drawable, symbol, branch);

              if (nextControls != null && nextControls.Count == 1 &&
                branch.Last != null && branch.Last.Value is Control)
              {
                Polyline p = new Polyline();
                AddPoint(p, (Control)branch.Last.Value, true);
                AddPoint(p, nextControls[0], false);

                drawable.DrawLine(p.Project(drawable.Projection), symbol);
              }
            }
            #endregion Variation
          }

          else if (section is Fork)
          {
            #region fork

            DataGridView dgv = _wdg.dgvPermut;
            List<int> forks = new List<int>();
            foreach (var col in dgv.Columns.Enum())
            {
              if (col.Tag == section)
              {
                int idx = col.Index;
                for (int fork = 0; fork < dgv.Rows.Count; fork++)
                {
                  DataGridViewCell cell = dgv.Rows[fork].Cells[idx];
                  if (cell.Selected && (string)cell.Value != "-")
                  {
                    forks.Add(((string)cell.Value)[0] - 'A');
                  }
                }
                break;
              }
            }

            IList<Control> preControls = _wdg.Vm.Course.PreviousControls(section);
            IList<Control> nextControls = _wdg.Vm.Course.NextControls(section);
            int i0 = 0;
            int i1 = 0;
            Fork sFork = (Fork)section;
            int nFork = sFork.Branches.Count;
            for (int iFork = 0; iFork < nFork; iFork++)
            {
              if (forks.Count > 0 && forks.Contains(iFork) == false)
              {
                continue;
              }
              Fork.Branch branch = (Fork.Branch)sFork.Branches[iFork];
              Polyline p;
              Control c1 = preControls[0];
              string comb = "";
              foreach (var cntr in branch.GetCombination(comb))
              {
                Control c0 = c1;
                c1 = cntr;
                p = new Polyline();
                AddPoint(p, c1, true);
                AddPoint(p, c0, false);

                drawable.DrawLine(p.Project(drawable.Projection), symbol);
              }

              p = new Polyline();
              AddPoint(p, c1, false);
              AddPoint(p, nextControls[i1], true);

              drawable.DrawLine(p.Project(drawable.Projection), symbol);

              if (preControls.Count > 1 && i0 >= 0)
              { i0++; }
              if (nextControls.Count > 1)
              { i1++; }
            }
            #endregion
          }
          else if (section is Control)
          {
            if (node.Previous != null)
            {
              ISection prev = node.Previous.Value;
              if (prev is Control)
              {
                Polyline p = new Polyline();
                AddPoint(p, (Control)section, true);
                AddPoint(p, (Control)prev, false);

                drawable.DrawLine(p.Project(drawable.Projection), symbol);
              }
            }
          }
        }
      }

      private IPoint AddPoint(Polyline line, Control c, bool start)
      {
        if (c == null)
        { return null; }
        IGeometry g = c.Element.Geometry;
        IPoint p;
        if (g is IPoint)
        { p = (IPoint)g; }
        else if (g is Polyline l)
        {
          if (start)
          { p = l.Points.First.Value; }
          else
          { p = l.Points.Last.Value; }
        }
        else
        { throw new InvalidOperationException("Unhandle control geometry " + g.GetType()); }

        p = p.Project(_wdg.Vm.Setup.Map2Prj);
        line.Add(p);

        return p;
      }
    }

    private GridMapData _thisGridData;
    private IContext _context;

    private static int _initTMap = InitTMapStatic();
    private static int InitTMapStatic()
    {
      Init -= InitTMap;
      Init += InitTMap;
      return 1;
    }


    private static void InitTMap(WdgOCourse wdg)
    {
      if (_initTMap != 1)
      { Console.WriteLine("Why am I here"); }

      wdg.Vm.ShowProgress += wdg.ShowInTMap;
      wdg.Vm.DrawingCourse += wdg.DrawCourseInTMap;
    }

    public IContext TMapContext
    {
      get { return _context; }
      set { _context = value; }
    }

    private void DrawCourseInTMap()
    {
      if (_context == null)
      { return; }
      _context.Maps[0].Draw(CourseData);
    }
    private MapData CourseData
    {
      get { return new CourseMapData(this); }
    }

    private void ShowInTMap(LeastCostGrid path, StatusEventArgs args)
    {
      if (_context == null)
      { return; }

      if (_thisGridData == null)
      {
        _thisGridData = GridMapData.FromData(args.DirGrid);
        LookupSymbolisation sym = new LookupSymbolisation();

        sym.Colors.Clear();

        sym.Colors.Add(null);
        for (int iCol = 0; iCol < path.Steps.Count; iCol++)
        {
          double angle = path.Steps.GetStep(iCol).Angle;

          int r = ColorVal(angle, 0);
          int g = ColorVal(angle, Math.PI * 2.0 / 3.0);
          int b = ColorVal(angle, -Math.PI * 2.0 / 3.0);
          double max = Math.Max(r, Math.Max(g, b));
          r = (int)(255 * r / max);
          g = (int)(255 * g / max);
          b = (int)(255 * b / max);
          sym.Colors.Add(System.Drawing.Color.FromArgb(r, g, b));
        }
        sym.Colors.Add(null);

        _thisGridData.Symbolisation = sym;
      }
      else
      {
        if (_thisGridData.Data.BaseData != args.DirGrid)
        {
          _thisGridData.Data.BaseData = args.DirGrid;
        }
      }
      _context.Maps[0].Draw(_thisGridData);
    }

    private static int ColorVal(double angle, double zero)
    {
      angle = angle - zero;
      if (angle > Math.PI)
      { angle -= Math.PI * 2; }
      if (angle < -Math.PI)
      { angle += Math.PI * 2; }
      double c = (1.0 - Math.Abs(angle) / (2.0 * Math.PI / 3.0));
      if (c < 0)
      { c = 0; }
      c = (1 - (1 - c) * (1 - c));
      return (int)(255 * c);
    }

  }
}