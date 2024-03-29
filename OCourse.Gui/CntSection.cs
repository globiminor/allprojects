﻿using Basics.Forms;
using Ocad;
using OCourse.Ext;
using OCourse.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Control = Ocad.Control;

namespace OCourse.Gui
{
  internal delegate void SectionEventHandler(object sender, IList<SectionList> list);

  internal partial class CntSection : UserControl
  {
    public event SectionEventHandler PartChanged;

    private readonly BindingSource _bindingSource;


    private static CLabel _startControl;
    private static CLabel _endControl;

    private Dictionary<NextControl, NextControlList> _nextDict;
    private Dictionary<NextControl, ControlLabel> _lblDict;

    private OCourseVm _vm;

    public CntSection()
    {
      InitializeComponent();
      Height = 1;
      Width = 1;

      SetStyle(ControlStyles.UserPaint, true);
      base.BackColor = System.Drawing.Color.FromArgb(0, 255, 255, 255);

      SetStyle(ControlStyles.SupportsTransparentBackColor, true);

      _bindingSource = new BindingSource(components);
    }

    public OCourseVm Vm
    {
      get { return _vm; }
      set
      {
        _vm = value;
        bool notBound = _bindingSource.DataSource == null;
        _bindingSource.DataSource = value;
        if (_vm == null)
        { return; }

        if (notBound)
        {
          this.Bind(x => x.Course, _bindingSource, nameof(_vm.Course),
            true, DataSourceUpdateMode.Never);

          this.Bind(x => x.BoldCombination, _bindingSource, nameof(_vm.SelectedCombination),
            true, DataSourceUpdateMode.Never);
        }
      }
    }

    private Course _course;
    public Course Course
    {
      get { return _course; }
      set
      {
        if (_course == value)
        { return; }

        _course = value;
        SetCourse(_course);
      }
    }

    private SectionList _boldCombination;
    public SectionList BoldCombination
    {
      get { return _boldCombination; }
      set
      {
        if (_boldCombination == value)
        { return; }

        _boldCombination = value;
        ShowCombination(_boldCombination);
      }


    }
    public Control StartControl
    {
      get { return _startControl?.Control; }
    }
    public Control EndControl
    {
      get { return _endControl?.Control; }
    }


    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);

      DrawNew(e.Graphics);

      e.Graphics.Flush();
    }

    private void DrawNew(Graphics graphics)
    {

      if (_nextDict == null || _nextDict.Count == 0)
      { return; }

      Pen f = new Pen(System.Drawing.Color.Red, 1);
      Pen b = new Pen(System.Drawing.Color.Blue, 1);

      foreach (var pair in _nextDict)
      {
        NextControl fc = pair.Key;

        // TODO
        if (!_lblDict.ContainsKey(fc))
        { continue; }

        ControlLabel fg = _lblDict[fc];
        Label fl = fg.Lbl;
        foreach (var nextControl in pair.Value.List)
        {
          NextControl tc = nextControl;
          if (!_lblDict.TryGetValue(tc, out ControlLabel tg))
          { continue; }

          Label tl = tg.Lbl;

          if (fg.Pos < tg.Pos)
          {
            graphics.DrawLines(f, new Point[]
                                    {
                                      new Point(fl.Left + fl.Width / 2, fl.Bottom),
                                      new Point(fl.Left + fl.Width / 2, tl.Top - 10),
                                      new Point(tl.Left + tl.Width / 2, tl.Top)
                                    });
          }
          else
          {
            Label ttl = tg.RecLbl;
            if (ttl == null)
            {
              continue;
            }
            graphics.DrawLines(b, new Point[]
                                    {
                                      new Point(fl.Left + fl.Width / 2, fl.Bottom),
                                      new Point(fl.Left + fl.Width / 2, ttl.Top - 10),
                                      new Point(ttl.Left + tl.Width / 2, ttl.Top)
                                    });

            graphics.DrawLines(b, new Point[]
                                    {
                                      new Point(ttl.Left + ttl.Width / 2, ttl.Bottom),
                                      new Point(ttl.Left + ttl.Width / 2,ttl.Bottom + 4),
                                      new Point(3, ttl.Bottom + 4),
                                      new Point(3, tl.Bottom),
                                      new Point(tl.Left - tl.Height / 2, tl.Bottom),
                                      new Point(tl.Left, tl.Top + tl.Height / 2),
                                    });
          }
        }
      }
    }

    void Lbl_MouseDown(object sender, MouseEventArgs e)
    {
      CLabel lbl = (CLabel)sender;
      MouseEventArgs f = new MouseEventArgs(e.Button, e.Clicks,
        e.X + lbl.Left, e.Y + lbl.Top, e.Delta);

      _startControl = null;
      _endControl = null;

      Clear();

      lbl.SetStartSelection();
      SectionMouseDown(f);

      _startControl = lbl;
    }

    private void Clear()
    {
      foreach (var l in Controls)
      {
        if (!(l is CLabel ll))
        { continue; }
        ll.Clear();
      }
      if (_startControl != null) _startControl.SetBeginEnd();
      if (_endControl != null) _endControl.SetBeginEnd();
    }

    void Lbl_MouseMove(object sender, MouseEventArgs e)
    {
      Label lbl = (Label)sender;
      MouseEventArgs f = new MouseEventArgs(e.Button, e.Clicks,
        e.X + lbl.Left, e.Y + lbl.Top, e.Delta);
      SectionMouseMove(f);
    }
    void Lbl_MouseUp(object sender, MouseEventArgs e)
    {
      CLabel lbl = (CLabel)sender;
      MouseEventArgs f = new MouseEventArgs(e.Button, e.Clicks,
        e.X + lbl.Left, e.Y + lbl.Top, e.Delta);

      List<SectionList> section = GetSection(f);
      OnPartChanged(section);
    }

    private Point _screenPointDown;
    private Point _screenPointLast;

    private void SectionMouseDown(MouseEventArgs e)
    {
      _startControl = null;
      _endControl = null;

      _screenPointDown = Point.Empty;
      _screenPointLast = Point.Empty;
      if (e.Button == MouseButtons.Left)
      {
        _screenPointDown = PointToScreen(e.Location);
      }
    }
    private void SectionMouseMove(MouseEventArgs e)
    {
      if (_startControl != null &&
        e.Button == MouseButtons.Left && _screenPointDown != Point.Empty)
      {
        if (_screenPointLast != Point.Empty)
        {
          ControlPaint.DrawReversibleLine(
            _screenPointDown, _screenPointLast, System.Drawing.Color.Black);
        }
        _screenPointLast = PointToScreen(e.Location);
        ControlPaint.DrawReversibleLine(
          _screenPointDown, _screenPointLast, System.Drawing.Color.Black);
      }
      else
      {
        _screenPointLast = Point.Empty;
      }
    }
    private List<SectionList> GetSection(MouseEventArgs e)
    {
      if (_startControl == null)
      { return null; }

      if (_startControl != null &&
        e.Button == MouseButtons.Left && _screenPointDown != Point.Empty)
      {
        _screenPointLast = PointToScreen(e.Location);
        ControlPaint.DrawReversibleLine(
          _screenPointDown, _screenPointLast, System.Drawing.Color.Black);
      }
      _screenPointLast = Point.Empty;

      if (!(GetChildAtPoint(e.Location) is CLabel end))
      {
        _startControl.Clear();
        _startControl = null;
        return null;
      }
      _endControl = end;

      _startControl.SetBeginEnd();
      _endControl.SetBeginEnd();

      foreach (var list in _nextDict.Values)
      { list.AssureCodes(); }

      Dictionary<NextControl, NextControlList> nextDict = Clone(_nextDict);
      List<SectionList> permuts = new List<SectionList>();
      SectionList permut = new SectionList(new NextControl(_startControl.Control));
      NextControl startNext = new NextControl(_startControl.Control);
      NextControl endNext = new NextControl(_endControl.Control);
      GetPermuts(permut, nextDict, startNext, endNext, permuts);
      if (permuts.Count == 0)
      {
        permut = new SectionList(new NextControl(_startControl.Control));
        permut.Add(new NextControl(_endControl.Control));
        permuts.Add(permut);
      }

      return permuts;
    }

    private void GetPermuts(SectionList permut, Dictionary<NextControl, NextControlList> nextDict,
      NextControl start, NextControl end, List<SectionList> permuts)
    {
      NextControl.EqualControlComparer cmp = new NextControl.EqualControlComparer();
      do
      {
        if (!nextDict.TryGetValue(start, out NextControlList list))
        { return; }
        if (list.Count == 0)
        {
          return;
        }
        if (list.Count == 1)
        {
          start = list.List[0];
          list.List.Clear();
          permut.Add(start);
          if (cmp.Equals(start, end))
          {
            permuts.Add(permut);
            return;
          }
        }
        else
        {
          for (int i = 0; i < list.Count; i++)
          {
            Dictionary<NextControl, NextControlList> cloneDict = Clone(nextDict);
            SectionList clonePermut = permut.Clone();
            NextControlList cloneList = cloneDict[start];
            NextControl cloneInfo = cloneList.List[i];
            cloneList.List.RemoveAt(i);
            clonePermut.Add(cloneInfo);
            if (cmp.Equals(cloneInfo, end))
            {
              permuts.Add(clonePermut);
            }
            else
            {
              GetPermuts(clonePermut, cloneDict, cloneInfo, end, permuts);
            }
          }
          return;
        }
      } while (true);
    }

    private static Dictionary<NextControl, NextControlList> Clone(Dictionary<NextControl, NextControlList> nextDict)
    {
      Dictionary<NextControl, NextControlList> clone = new Dictionary<NextControl, NextControlList>(new NextControl.EqualControlComparer());
      foreach (var pair in nextDict)
      {
        NextControl from = pair.Key;
        pair.Value.AssureCodes();

        NextControlList cloneList = new NextControlList();
        cloneList.AssureCodes();
        Dictionary<NextControl, NextControl> existing = new Dictionary<NextControl, NextControl>(new NextControl.EqualControlComparer());
        foreach (var section in pair.Value.List)
        {
          if (existing.ContainsKey(section))
          { continue; }
          existing.Add(section, section);

          SimpleSection cloneSection = new SimpleSection(from.Control, section.Control);
          cloneSection.SetWhere(section.Where);
          NextControl cloneNext = cloneList.Add(cloneSection);
          cloneNext.Code = section.Code;
        }

        clone.Add(pair.Key, cloneList);
      }
      return clone;
    }

    protected void OnPartChanged(IList<SectionList> part)
    {
      PartChanged?.Invoke(this, part);
    }

    private Dictionary<NextControl, NextControlList> AssembleNextSections(SectionCollection course)
    {
      SectionCollection clone = course.Clone();

      SectionsBuilder secBuilder = new SectionsBuilder();
      secBuilder.AddingBranch += SecBuilder_AddingBranch;
      List<SimpleSection> simples = secBuilder.GetSections(clone);

      Dictionary<string, Control> uniqueDict = new Dictionary<string, Control>();
      Dictionary<Control, Control> oldNew = new Dictionary<Control, Control>();
      foreach (var section in simples)
      {
        AddControl(section.From, section.Where, uniqueDict, oldNew);
        AddControl(section.To, section.Where, uniqueDict, oldNew);
      }

      List<SimpleSection> sections = new List<SimpleSection>(simples.Count);
      foreach (var section in simples)
      {
        Control from = oldNew[section.From];
        Control to = oldNew[section.To];
        SimpleSection newSec = new SimpleSection(from, to);
        newSec.SetWhere(section.Where);
        sections.Add(newSec);
      }

      Dictionary<NextControl, NextControlList> vars = new Dictionary<NextControl, NextControlList>(
        new NextControl.EqualControlComparer());
      if (sections.Count == 0)
      { return vars; }

      Dictionary<Control, NextControl> whereControls = new Dictionary<Control, NextControl>();
      foreach (var section in sections)
      {
        if (whereControls.TryGetValue(section.From, out NextControl from) == false)
        {
          from = new NextControl(section.From);
          whereControls.Add(from.Control, from);
        }
        if (vars.TryGetValue(from, out NextControlList nextList) == false)
        {
          nextList = new NextControlList();
          vars.Add(from, nextList);
        }
        nextList.Add(section);
      }
      return vars;
    }

    private void AddControl(Control ctr, string where, Dictionary<string, Control> uniqueDict, Dictionary<Control, Control> oldNew)
    {
      string name = string.Format("{0};{1}", where, ctr.Name);
      Control uniqueCtr = ctr;

      if (ctr.Code == ControlCode.TextBlock || ctr.Code == ControlCode.MapChange)
      { }
      else if ((ctr.Text == null || !ctr.Text.StartsWith("where ", StringComparison.InvariantCultureIgnoreCase))
          && uniqueDict.TryGetValue(name, out uniqueCtr) == false)
      {
        uniqueCtr = ctr;
        uniqueDict.Add(name, uniqueCtr);
      }

      oldNew[ctr] = uniqueCtr;
    }

    IList<SectionsBuilder.IDisplayControl> SecBuilder_AddingBranch(int leg, ISection section, IList<SectionsBuilder.IDisplayControl> fromControls,
      List<SimpleSection> sections, string where)
    {
      Variation.Branch branch = (Variation.Branch)section;
      string legs = branch.GetLegsListToString();
      Control ctr = new Control(legs, ControlCode.TextBlock);
      ctr.Text = string.Format("where {0}", where);

      List<SectionsBuilder.IDisplayControl> result = new List<SectionsBuilder.IDisplayControl>();
      foreach (var fromControl in fromControls)
      {
        sections.Add(new SimpleSection(fromControl.Control, ctr));
        result.Add(new FromControl(fromControl.Control, ctr));
      }
      return result;
    }
    private class FromControl : SectionsBuilder.IDisplayControl
    {
      public FromControl(Control control, Control display)
      {
        Control = control;
        Display = display;
      }
      public Control Control { get; }
      public Control Display { get; }
    }

    public void SetCourse(Course course)
    {
      Controls.Clear();
      if (course == null)
      {
        if (_nextDict != null)
        { _nextDict.Clear(); }

        Width = 20;
        Height = 20;

        Invalidate();
        return;
      }

      SectionCollection list = new SectionCollection();
      Course clone = course.Clone();
      list.AddLast(clone);
      Control startWrapper = VariationBuilder.GetWrapperControl("++", list);
      list.AddFirst(startWrapper);
      Control endWrapper = VariationBuilder.GetWrapperControl("--", list);
      list.AddLast(endWrapper);

      Dictionary<NextControl, NextControlList> next = AssembleNextSections(list);
      startWrapper = GetControl(next, startWrapper.Name);
      endWrapper = GetControl(next, endWrapper.Name);

      NextControl start = null;
      foreach (var key in next.Keys)
      {
        start = key;
        break;
      }
      if (start == null)
      { return; }

      Dictionary<NextControl, ControlLabel> ctrlLblDict =
        new Dictionary<NextControl, ControlLabel>(next.Count,
        new NextControl.EqualControlComparer());
      BuildTree(next, ctrlLblDict, new List<NextControl>(),
         start, 0, new List<Control> { startWrapper, endWrapper });
      // Compensate for startWrapper:
      foreach (var value in ctrlLblDict.Values)
      { value.ReducePos(); }

      _nextDict = next;
      _lblDict = ctrlLblDict;
      Dictionary<int, List<int>> rowOcc = new Dictionary<int, List<int>>();

      int h0 = 10;
      int maxPos = ctrlLblDict.Count;
      for (int iPos = 0; iPos < maxPos; iPos++)
      {
        int jCol = -1;
        int dh = 0;
        if (!rowOcc.TryGetValue(iPos, out List<int> occupied))
        { occupied = null; }

        foreach (var ctrLbl in ctrlLblDict.Values)
        {
          if (ctrLbl.Pos == iPos)
          {
            jCol++;
            while (occupied != null && occupied.Contains(jCol))
            { jCol++; }

            CLabel lbl = new CLabel(this, ctrLbl.Control);
            lbl.Text = ctrLbl.Control.Name;
            if (ctrLbl.Control.Code == ControlCode.MapChange)
            {
              lbl.Text = "\u2341>\u2342";
            }
            lbl.AutoSize = true;
            lbl.Top = h0;

            dh = Math.Max(dh, lbl.Height);
            Controls.Add(lbl);
            lbl.Left = 30 + jCol * 30 - lbl.Width / 2;

            ctrLbl.Lbl = lbl;

            if (next.TryGetValue(new NextControl(ctrLbl.Control), out NextControlList nextList))
            {
              foreach (var info in nextList.List)
              {
                if (!ctrlLblDict.TryGetValue(info, out ControlLabel nextLabel))
                { continue; }

                int nextPos;
                if (nextLabel.Pos > iPos)
                { nextPos = nextLabel.Pos; }
                else
                { 
                  nextPos = nextLabel.RecPos;
                  if (nextPos >= maxPos)
                  { maxPos = nextPos + 1; }
                }
                for (int k = iPos + 1; k < nextPos; k++)
                {
                  if (!rowOcc.TryGetValue(k, out List<int> occ))
                  {
                    occ = new List<int>();
                    rowOcc.Add(k, occ);
                  }
                  occ.Add(jCol);
                }
              }
            }
          }
          if (ctrLbl.RecPos == iPos)
          {
            jCol++;
            while (occupied != null && occupied.Contains(jCol))
            { jCol++; }

            CLabel lbl = new CLabel(this, ctrLbl.Control, true);
            lbl.Top = h0;

            dh = Math.Max(dh, lbl.Height);
            Controls.Add(lbl);

            lbl.SetColumn(jCol);
            ctrLbl.RecLbl = lbl;

          }
        }
        h0 = h0 + dh;
      }
      // NextList = next;
      Width = 20;
      Height = 20;
      foreach (var o in Controls)
      {
        System.Windows.Forms.Control control = (System.Windows.Forms.Control)o;
        Width = Math.Max(Width, control.Right + 10);
        Height = Math.Max(Height, control.Bottom + 10);
      }
      Invalidate();
    }

    private Control GetControl(Dictionary<NextControl, NextControlList> nextDict, string name)
    {
      foreach (var pair in nextDict)
      {
        if (pair.Key.Control.Name == name)
        { return pair.Key.Control; }

        foreach (var next in pair.Value.List)
        {
          if (next.Control.Name == name)
          { return next.Control; }
        }
      }
      return null;
    }

    private void BuildTree(Dictionary<NextControl, NextControlList> next,
      Dictionary<NextControl, ControlLabel> dict,
      IList<NextControl> preList,
      NextControl c, int pos0, IList<Control> wrapperControls)
    {
      int pos = pos0;

      if (!dict.TryGetValue(c, out ControlLabel controlLabel))
      {
        if (!wrapperControls.Contains(c.Control))
        {
          controlLabel = new ControlLabel();
          controlLabel.Control = c.Control;
          controlLabel.Pos = pos;
          dict.Add(c, controlLabel);
        }
      }
      else
      {
        NextControl.EqualControlComparer cmp = new NextControl.EqualControlComparer();

        bool containes = false;
        foreach (var candidate in preList)
        {
          if (cmp.Equals(candidate, c))
          {
            containes = true;
            break;
          }
        }
        if (containes)
        {
          controlLabel.RecPos = Math.Max(pos, controlLabel.RecPos);
        }
        else
        {
          SetNewPosition(c, pos, next, dict);
        }
        return;
      }
      List<NextControl> nextPres = new List<NextControl>(preList.Count + 1);
      nextPres.AddRange(preList);
      nextPres.Add(c);
      if (!next.TryGetValue(c, out NextControlList list))
      { return; }
      foreach (var info in list.List)
      {
        BuildTree(next, dict, nextPres, info, pos + 1, wrapperControls);
      }
    }

    private void SetNewPosition(NextControl c, int pos,
      Dictionary<NextControl, NextControlList> next,
      Dictionary<NextControl, ControlLabel> dict)
    {
      ControlLabel controlLabel = dict[c];
      int origPos = controlLabel.Pos;
      if (origPos >= pos)
      { return; }

      controlLabel.Pos = pos;
      if (!next.TryGetValue(c, out NextControlList list))
      { return; }
      foreach (var info in list.List)
      {
        if (!dict.TryGetValue(info, out ControlLabel n))
        { continue; }

        if (n.RecPos > 0)
        {
          n.RecPos = Math.Max(n.RecPos, pos + 1);
        }
        else if (n.Pos > origPos)
        {
          SetNewPosition(info, pos + 1, next, dict);
        }
      }
    }

    private class CLabel : Label
    {
      private readonly CntSection _parent;
      public Control Control;
      private readonly bool _isReverseControl;
      private int _iCol;
      Timer _t;

      public CLabel(CntSection parent, Control ctr)
        : this(parent, ctr, false)
      { }

      public CLabel(CntSection parent, Control ctr, bool isReverseControl)
      {
        MouseDown += parent.Lbl_MouseDown;
        MouseMove += parent.Lbl_MouseMove;
        MouseUp += parent.Lbl_MouseUp;

        _parent = parent;
        _isReverseControl = isReverseControl;

        Control = ctr;
        base.Text = Control.Name;
        base.AutoSize = true;
        SetFont(false);

        if (ctr.Code == ControlCode.TextBlock)
        {
          if (ctr.Text != null &&
            ctr.Text.StartsWith("where ", StringComparison.InvariantCultureIgnoreCase))
          {
            parent.ttp.SetToolTip(this, ctr.Text);
          }

          _parent.ttp.AutoPopDelay = int.MaxValue;

          _t = new Timer();
          _t.Interval = _parent.ttp.ReshowDelay;
          _t.Tick += T_Tick;
          MouseMove += Tb_MouseMove;
        }
      }

      public int ColumnIndex => _iCol;
      public void SetColumn(int iCol)
      {
        Left = 30 + iCol * 30 - Width / 2;
        _iCol = iCol;
      }

      private void SetFont(bool bold)
      {
        FontStyle baseStyle;
        if (bold)
        { baseStyle = FontStyle.Bold; }
        else
        { baseStyle = FontStyle.Regular; }
        if (_isReverseControl)
        {
          Font = new Font(Font, baseStyle | FontStyle.Italic);
          ForeColor = System.Drawing.Color.Blue;
        }
        else if (Control.Code == ControlCode.TextBlock)
        {
          Font = new Font(Font, baseStyle | FontStyle.Regular);
          base.ForeColor = System.Drawing.Color.Blue;
        }
        else if (Control.Code == ControlCode.MapChange)
        {
          Font = new Font(Font, baseStyle | FontStyle.Regular);
          base.ForeColor = System.Drawing.Color.Blue;
        }
        else
        {
          Font = new Font(Font, baseStyle);
          base.ForeColor = System.Drawing.Color.Black;
        }
      }

      void T_Tick(object sender, EventArgs e)
      {
        _t.Stop();
        MouseMove += Tb_MouseMove;
      }

      void Tb_MouseMove(object sender, MouseEventArgs e)
      {
        MouseMove -= Tb_MouseMove;
        _parent.ttp.SetToolTip((sender as CLabel), Control.Text);
        _t.Start();
      }

      public void Clear()
      {
        BackColor = Parent.BackColor;
        SetFont(false);
      }

      public void SetStartSelection()
      {
        BackColor = System.Drawing.Color.LightGreen;
      }

      public void SetBeginEnd()
      {
        BackColor = System.Drawing.Color.LightBlue;
      }

      public void SetOnCombination()
      {
        SetFont(true);
      }
    }
    private class ControlLabel
    {
      public CLabel Lbl;
      public Control Control;
      public int Pos;
      public int RecPos = -1;
      public CLabel RecLbl;
      public override string ToString()
      {
        return string.Format("{0},{1}, {2}", Pos, RecPos, Control);
      }

      public void ReducePos()
      {
        Pos--;
        if (RecPos > 0)
        { RecPos--; }
      }
    }

    public void ShowCombination(SectionList combination)
    {
      Clear();
      if (combination == null || _lblDict == null)
      {
        return;
      }
      foreach (var key in combination.NextControls)
      {
        if (_lblDict.TryGetValue(key, out ControlLabel lbl))
        {
          lbl.Lbl.SetOnCombination();
        }
        else
        {
          foreach (var pair in _lblDict)
          {
            if (pair.Key.Control.Name == key.Control.Name)
            {
              if (pair.Key.Where == key.Where)
              {
                pair.Value.Lbl.SetOnCombination();
                break;
              }
              else
              { }
            }
          }
        }
      }
    }
  }
}
