
using Basics.Forms;
using Basics.Geom;
using Grid.Lcp;
using Ocad;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TMap;

namespace OCourse.Gui.Plugins
{
  public class OCourseCmd : ICommand
  {
    private WdgOCourse _wdg;

    public OCourseCmd()
    {
      // "AssemblyResolve" is active only here
      // --> load all potentially needed libraries here
      _wdg = new WdgOCourse();
      Programm.Init();
    }
    #region ICommand Members


    public void Execute(IContext context)
    {
      if (_wdg == null)
      {
        _wdg = new WdgOCourse();
        Programm.Init();
      }
      _wdg.TMapContext = context;
      if (context is IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
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
        SymbolPartLine symbol = new SymbolPartLine() { Color = System.Drawing.Color.Red };

        //drawable.BeginDraw();
        drawable.BeginDraw(symbol, null);
        drawable.DrawArea(null, null);
        Draw(drawable, symbol, _wdg.Vm.SelectedCost);
        drawable.EndDraw(symbol);
        //drawable.EndDraw();
        //drawable.Flush();
      }
      private void Draw(IDrawable drawable, SymbolPart symbol, Route.CostBase cost)
      {
        if (cost is Route.CostSectionlist sections)
        {
          Draw(drawable, symbol, sections);
        }
        else if (cost is Route.CostFromTo fromTo)
        {
          Draw(drawable, symbol, fromTo);
        }
      }

      private void Draw(IDrawable drawable, SymbolPart symbol, Route.CostFromTo fromTo)
      {
        Polyline l = Polyline.Create(new[] { fromTo.Start, fromTo.End });
        drawable.DrawLine(l.Project(drawable.Projection), symbol);

        if (fromTo.Route != null)
        {
          SymbolPartLine routeSymbol = new SymbolPartLine { Color = System.Drawing.Color.Blue };
          drawable.DrawLine(fromTo.Route.Project(drawable.Projection), routeSymbol);
        }
      }

      private void Draw(IDrawable drawable, SymbolPart symbol, Route.CostSectionlist sections)
      {
        SectionCollection course = new SectionCollection();
        foreach (var ctr in sections.Sections.Controls)
        {
          course.AddLast(ctr);
        }
        Draw(drawable, symbol, course);
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
            IList<Ocad.Control> preControls = _wdg.Vm.Course.PreviousControls(section);
            IList<Ocad.Control> nextControls = _wdg.Vm.Course.NextControls(section);

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

              {
                if (preControls != null && preControls.Count == 1 &&
                  branch.First != null && branch.First.Value is Ocad.Control to)
                {
                  Polyline p = GetPolyline((Ocad.Control)branch.First.Value, to);
                  drawable.DrawLine(p.Project(drawable.Projection), symbol);
                }
                Draw(drawable, symbol, branch);
              }
              {
                if (nextControls != null && nextControls.Count == 1 &&
                  branch.Last != null && branch.Last.Value is Ocad.Control to)
                {
                  Polyline p = GetPolyline(nextControls[0], to);
                  drawable.DrawLine(p.Project(drawable.Projection), symbol);
                }
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

            IList<Ocad.Control> preControls = _wdg.Vm.Course.PreviousControls(section);
            IList<Ocad.Control> nextControls = _wdg.Vm.Course.NextControls(section);
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
              Ocad.Control c1 = preControls[0];
              string comb = "";
              foreach (var cntr in branch.GetCombination(comb))
              {
                Ocad.Control c0 = c1;
                c1 = cntr;
                p = GetPolyline(c0, c1);

                drawable.DrawLine(p.Project(drawable.Projection), symbol);
              }

              p = GetPolyline(c1, nextControls[i1]);
              drawable.DrawLine(p.Project(drawable.Projection), symbol);

              if (preControls.Count > 1 && i0 >= 0)
              { i0++; }
              if (nextControls.Count > 1)
              { i1++; }
            }
            #endregion
          }
          else if (section is Ocad.Control to)
          {
            if (node.Previous != null)
            {
              ISection prev = node.Previous.Value;
              if (prev is Ocad.Control from)
              {
                Polyline p = GetPolyline(from, to);

                drawable.DrawLine(p.Project(drawable.Projection), symbol);
              }
            }
          }
        }
      }

      private Polyline GetPolyline(Ocad.Control from, Ocad.Control to)
      {
        Polyline p = new Polyline();
        AddPoint(p, from, false);
        AddPoint(p, to, true);
        return p;
      }
      private IPoint AddPoint(Polyline line, Ocad.Control c, bool start)
      {
        if (c?.Element?.Geometry == null)
        { return null; }

        IPoint p = Utils.GetBorderPoint(c.Element.Geometry, atEnd: !start);
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

      LcpDetailShowing -= LcpDetailShowingTMap;
      LcpDetailShowing += LcpDetailShowingTMap;
      return 1;
    }


    private static void InitTMap(WdgOCourse wdg)
    {
      if (_initTMap != 1)
      { Console.WriteLine("Why am I here"); }

      wdg.Vm.ShowProgress += wdg.ShowInTMap;
      wdg.Vm.DrawingCourse += wdg.DrawCourseInTMap;
    }

    private static void LcpDetailShowingTMap(LeastCostPathUI.WdgOutput wdg)
    {
      if (!(wdg.Owner is WdgOCourse owner))
      { return; }
      IContext context = owner._context;
      if (context == null)
      { return; }

      GridMapData grid = null;
      wdg.CntOutput.StatusChanged += (s, args) => 
      {
        LeastCostPathUI.LeastCostPathCmd.ShowInTMap(context, s as LeastCostGrid, ref grid, args);
      };
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