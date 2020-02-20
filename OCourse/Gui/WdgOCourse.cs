using Basics;
using Basics.Forms;
using Basics.Geom;
using Basics.Views;
using LeastCostPathUI;
using Ocad;
using OCourse.Commands;
using OCourse.Ext;
using OCourse.Route;
using OCourse.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OCourse.Gui
{
  public partial class WdgOCourse : Form
  {
    private static event Action<WdgOCourse> Init;
    private static event Action<WdgOutput> LcpDetailShowing;

    private readonly List<ICost> _selectedRoute;
    private bool _suspend;

    private readonly BindingSource _bindingSource;

    public WdgOCourse()
    {
      InitializeComponent();

      _bindingSource = new BindingSource(components);

      _selectedRoute = new List<ICost>();

      _cntSection.PartChanged += CntSection_PartChanged;
    }

    private OCourseVm _vm;
    public OCourseVm Vm
    {
      get { return _vm; }
      set
      {
        _vm = value;
        _cntConfig.ConfigVm = _vm.LcpConfig;
        _cntSection.Vm = _vm;

        bool notBound = _bindingSource.DataSource == null;

        _bindingSource.DataSource = _vm;
        if (notBound)
        {
          this.Bind(x => x.Text, _bindingSource, nameof(_vm.Title),
            true, DataSourceUpdateMode.Never);

          _txtCourse.Bind(x => x.Text, _bindingSource, nameof(_vm.CourseFile),
            true, DataSourceUpdateMode.OnValidation);

          _txtMin.Bind(x => x.Text, _bindingSource, nameof(_vm.StartNrMin),
            true, DataSourceUpdateMode.OnPropertyChanged);
          _txtMax.Bind(x => x.Text, _bindingSource, nameof(_vm.StartNrMax),
            true, DataSourceUpdateMode.OnPropertyChanged);

          txtEstimate.Bind(x => x.Text, _bindingSource, nameof(_vm.PermutEstimate),
            true, DataSourceUpdateMode.Never);

          _lblProgress.Bind(x => x.Text, _bindingSource, nameof(_vm.Progress),
            true, DataSourceUpdateMode.Never);

          this.Bind(x => x.Working, _bindingSource, nameof(_vm.Working),
            false, DataSourceUpdateMode.Never);

          this.Bind(x => x.Permutations, _bindingSource, nameof(_vm.Permutations),
            true, DataSourceUpdateMode.Never);

          this.Bind(x => SettingSaveEnabled, _bindingSource, nameof(_vm.CanSave),
            false, DataSourceUpdateMode.Never);

          dgvInfo.Bind(x => x.DataSource, _bindingSource, nameof(_vm.Info),
              true, DataSourceUpdateMode.Never);

          mnuVarExport.Bind(x => x.Enabled, _bindingSource, nameof(_vm.CanVariationExport),
            false, DataSourceUpdateMode.Never);

          mnuPermExport.Bind(x => x.Enabled, _bindingSource, nameof(_vm.CanPermutationExport),
            false, DataSourceUpdateMode.Never);

          {
            _lstCourses.DataSource = _vm.CourseNames;
            _lstCourses.Bind(x => x.SelectedItem, _bindingSource, nameof(_vm.CourseName),
              true, DataSourceUpdateMode.Never);
            _lstCourses.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.CourseName),
              true, DataSourceUpdateMode.OnPropertyChanged);
          }

          {
            _lstCats.DataSource = _vm.CategoryNames;
            _lstCats.Bind(x => x.SelectedItem, _bindingSource, nameof(_vm.CategoryName),
              true, DataSourceUpdateMode.Never);
            _lstCats.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.CategoryName),
              true, DataSourceUpdateMode.OnPropertyChanged);
          }

          {
            EnumText<VelocityType> t = null;
            _lstVelo.DataSource = _vm.VelocityTypes;
            _lstVelo.ValueMember = nameof(t.Id);
            _lstVelo.DisplayMember = nameof(t.Text);
            _lstVelo.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.VelocityType),
              true, DataSourceUpdateMode.OnPropertyChanged);
          }
          {
            EnumText<VarBuilderType> t = null;
            _lstVarBuilders.DataSource = _vm.VarBuilderTypes;
            _lstVarBuilders.ValueMember = nameof(t.Id);
            _lstVarBuilders.DisplayMember = nameof(t.Text);
            _lstVarBuilders.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.VarBuilderType),
              true, DataSourceUpdateMode.OnPropertyChanged);
          }
          {
            EnumText<DisplayType> t = null;
            _lstDisplayType.DataSource = _vm.DisplayTypes;
            _lstDisplayType.ValueMember = nameof(t.Id);
            _lstDisplayType.DisplayMember = nameof(t.Text);
            _lstDisplayType.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.DisplayType),
              true, DataSourceUpdateMode.OnPropertyChanged);
          }
        }
      }
    }

    [System.ComponentModel.Browsable(false)]
    public bool SettingSaveEnabled
    {
      get { return mniSave.Enabled; }
      set { mniSave.Enabled = value; }
    }

    private bool _working;
    public bool Working
    {
      get { return _working; }
      set
      {
        if (_working && value)
        {
          if ((dgvInfo.FirstDisplayedCell?.RowIndex ?? short.MaxValue)
            + dgvInfo.DisplayedRowCount(includePartialRow: false) < dgvInfo.RowCount)
          {
            dgvInfo.FirstDisplayedCell = dgvInfo.Rows[dgvInfo.RowCount - dgvInfo.DisplayedRowCount(includePartialRow: false)].Cells[0];
          }
        }
        if (_working == value)
        { return; }

        _working = value;

        if (!_working && dgvInfo.Rows.Count > 0)
        {
          dgvInfo.FirstDisplayedCell = dgvInfo.Rows[0].Cells[0];
        }
        SetLayout(_working);
      }
    }

    private PermutationVms _permutations;
    public PermutationVms Permutations
    {
      get { return _permutations; }
      set
      {
        _permutations = value;

        bool origSuspend = _suspend;
        try
        {
          _suspend = true;
          dgvPermut.SuspendLayout();
          dgvPermut.Columns.Clear();

          dgvPermut.DataSource = _permutations;
          if (_permutations == null)
          { return; }

          {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
            {
              DataPropertyName = nameof(PermutationVm.StartNr),
              HeaderText = "StartNr"
            };
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Width = 50;
            FilterHeaderCell.CreateCore(col);
            dgvPermut.Columns.Add(col);
          }
          {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
            {
              DataPropertyName = nameof(PermutationVm.Index),
              HeaderText = "Index",
              Width = 50
            };
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            FilterHeaderCell.CreateCore(col);
            dgvPermut.Columns.Add(col);
          }

          if (_permutations.Count > 0)
          {
            int iPart = 0;
            foreach (var part in _permutations[0].Parts)
            {
              DataGridViewColumn col = new DataGridViewTextBoxColumn
              {
                DataPropertyName = PermutationVms.GetPartPropertyName(iPart),
                HeaderText = $"Part {iPart + 1}"
              };
              FilterHeaderCell.CreateCore(col);
              dgvPermut.Columns.Add(col);
              iPart++;
            }
          }
        }
        finally
        {
          dgvPermut.ResumeLayout();
          _suspend = origSuspend;
        }
      }
    }

    private System.Windows.Forms.Control _focused;
    private void SetLayout(bool calculating)
    {
      if (calculating)
      {
        _focused = Basics.Forms.Utils.FindFocusedControl(this);
      }

      foreach (var ctr in Controls)
      {
        ((System.Windows.Forms.Control)ctr).Enabled = !calculating;
      }
      btnCancel.Enabled = calculating;
      btnCancel.Visible = calculating;

      if (!calculating && _focused != null)
      {
        _focused.Focus();
      }
    }

    private void BtnCourse_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      dlgOpen.FileName = _txtCourse.Text;
      dlgOpen.Filter = "*.ocd|*.ocd";
      if (dlgOpen.ShowDialog(this) != DialogResult.OK)
      { return; }

      _vm.CourseFile = dlgOpen.FileName;
    }

    private void BtnBackCalc_Click(object sender, EventArgs e)
    {
      _vm?.CalcEventInit();
    }

    private void InitGridLayout()
    {
      try
      {
        _suspend = true;
        dgvInfo.SuspendLayout();
        dgvInfo.Columns.Clear();
        dgvInfo.AutoGenerateColumns = false;
        dgvInfo.RowHeadersVisible = false;

        ICost c = null;

        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.Name),
            HeaderText = "Name",
            ReadOnly = true,
            Width = 60
          };
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.DirectLKm),
            HeaderText = "DirectLKm",
            ReadOnly = true,
            Width = 50
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N2";
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.DirectKm),
            HeaderText = "DirectKm",
            ReadOnly = true,
            Width = 50
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N2";
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.Climb),
            HeaderText = "Climb",
            ReadOnly = true,
            Width = 40
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N0";
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.OptimalLKm),
            HeaderText = "OptimalLKm",
            ReadOnly = true,
            Width = 50
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N2";
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.OptimalKm),
            HeaderText = "OptimalKm",
            ReadOnly = true,
            Width = 50
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N2";
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.OptimalCost),
            HeaderText = "OptimalCost",
            ReadOnly = true,
            Width = 50
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N0";
          FilterHeaderCell.CreateCore(col);

          dgvInfo.Columns.Add(col);
        }

        dgvInfo.CellToolTipTextNeeded += DgvInfo_CellToolTipTextNeeded;

        dgvPermut.CellToolTipTextNeeded += DgvPermut_CellToolTipTextNeeded;
      }
      finally
      {
        dgvInfo.ResumeLayout();
        _suspend = false;
      }

    }

    void DgvInfo_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.ToolTipText))
      { return; }
      if (e.ColumnIndex < 0 && e.ColumnIndex >= dgvInfo.Columns.Count)
      { return; }
      if (e.RowIndex < 0 || e.RowIndex >= dgvInfo.Rows.Count)
      { return; }

      DataGridViewColumn col = dgvInfo.Columns[e.ColumnIndex];
      ICost item = dgvInfo.Rows[e.RowIndex].DataBoundItem as ICost;
      if (col.DataPropertyName != nameof(item.Name))
      { return; }

      if (!(item is IInfo info))
      { return; }

      string toolTip = info.GetInfo();
      if (toolTip == null)
      { return; }

      e.ToolTipText = toolTip;

      return;
    }

    private void CntSection_PartChanged(object sender, IList<SectionList> list)
    {
      List<CostSectionlist> info;
      if (list != null)
      {
        info = _vm.CalcCourse(list);
      }
      else
      {
        info = null;
      }
      _selectedRoute.Clear();
      Vm.SetInfo(_selectedRoute, info);

      if (_cntSection.StartControl != null && _cntSection.EndControl != null)
      {
        btnCalcSelection.Enabled = true;
        btnCalcSelection.Text = string.Format("Calc {0}-{1}",
                                              _cntSection.StartControl.Name,
                                              _cntSection.EndControl.Name);
      }
      else
      {
        btnCalcSelection.Enabled = false;
      }
    }

    private void ChkOnTop_CheckedChanged(object sender, EventArgs e)
    {
      TopMost = chkOnTop.Checked;
    }

    private void BtnExport_Click(object sender, EventArgs e)
    {
      dlgSave.Filter = "*.shp | *.shp";
      if (_vm.PathesFile != null && File.Exists(_vm.PathesFile))
      { dlgSave.FileName = _vm.PathesFile; }

      if (dlgSave.ShowDialog(this) != DialogResult.OK)
      { return; }

      string pathesFile = dlgSave.FileName;
      using (CmdShapeExport cmd = new CmdShapeExport(_vm))
      {
        cmd.Export(pathesFile);
        _vm.PathesFile = pathesFile;
      }
    }

    private void BtnImport_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      dlgOpen.Filter = "*.shp | *.shp";
      if (dlgOpen.ShowDialog() != DialogResult.OK)
      { return; }

      _vm.ImportRoutes(dlgOpen.FileName);
    }

    private void DgvInfo_CurrentCellChanged(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      object selected = null;
      if (dgvInfo.CurrentRow != null)
      {
        selected = dgvInfo.CurrentRow.DataBoundItem;
      }
      Vm.SetSelectionedComb(selected);

      btnRefreshSection.Enabled = false;
      if ((_vm.Info?.Count ?? 0) >= dgvInfo.RowCount)
      {
        foreach (var o in dgvInfo.SelectedRows)
        {
          DataGridViewRow row = (DataGridViewRow)o;
          if (row.DataBoundItem is CostFromTo)
          {
            btnRefreshSection.Enabled = true;
            break;
          }
        }
      }
      Vm.DrawCourse();
    }

    private void DgvPermut_SelectionChanged(object sender, EventArgs e)
    {
      if (_suspend)
      { return; }
      ShowPart(dgvPermut.CurrentCell);
    }

    private void ShowPart(DataGridViewCell cell)
    {
      SectionList part = GetPart(cell);
      if (part == null)
      { return; }
      TrySelect(part);
      _cntSection.ShowCombination(part);
    }

    private SectionList GetPart(DataGridViewCell cell)
    {
      return cell?.Value as SectionList;
    }

    void DgvPermut_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
      if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvPermut.Columns.Count)
      { return; }
      if (e.RowIndex < 0 || e.RowIndex >= dgvPermut.Rows.Count)
      { return; }

      SectionList part = GetPart(dgvPermut.Rows[e.RowIndex].Cells[e.ColumnIndex]);
      if (part == null)
      { return; }

      e.ToolTipText = part.ToString();
    }

    private bool TrySelect(SectionList part)
    {
      SectionList.EqualControlNamesComparer cmp = new SectionList.EqualControlNamesComparer();
      foreach (var o in dgvInfo.Rows)
      {
        DataGridViewRow gRow = (DataGridViewRow)o;
        if (!(gRow.DataBoundItem is CostSectionlist comb))
        { continue; }


        if (string.IsNullOrWhiteSpace(comb.Name))
        { continue; }

        SectionList sections = comb.Sections;
        if (!cmp.Equals(sections, part))
        { continue; }

        dgvInfo.ClearSelection();
        gRow.Selected = true;
        dgvInfo.FirstDisplayedScrollingRowIndex = gRow.Index;
        return true;
      }
      return false;
    }

    private void BtnCalcSelection_Click(object sender, EventArgs e)
    {
      WdgOutput wdg = new WdgOutput();
      Ocad.Control cStart = _cntSection.StartControl;
      Ocad.Control cEnd = _cntSection.EndControl;
      wdg.Text = string.Format("Calculate {0} - {1}", cStart.Name, cEnd.Name);

      Setup setup;
      using (OcadReader reader = OcadReader.Open(_vm.CourseFile))
      { setup = reader.ReadSetup(); }

      //IPoint point;
      //point = cStart.GetPoint();
      //IPoint start = PointOp.Project(point, setup.Map2Prj);
      //point = cEnd.GetPoint();
      //IPoint end = PointOp.Project(point, setup.Map2Prj);

      IPoint start = cStart.GetPoint();
      IPoint end = cEnd.GetPoint();

      double l = Math.Sqrt(PointOp.Dist2(start, end)) / 2.0;
      Box box = _vm.RouteCalculator.GetBox(start, end, l);

      wdg.Init((b) => _vm.RouteCalculator.GetLcpModel(_vm.LcpConfig.Resolution, _vm.LcpConfig.StepsMode, b), _vm.RouteCalculator.HeightGrid,
        _vm.RouteCalculator.VeloPath, _vm.LcpConfig.Resolution, _vm.LcpConfig.StepsMode);
      wdg.SetExtent(box);
      wdg.SetStart(start);
      wdg.SetEnd(end);
      wdg.SetNames(cStart.Name, cEnd.Name);
      wdg.SetMainPath(Path.GetDirectoryName(dlgOpen.FileName));

      wdg.Owner = this;
      LcpDetailShowing?.Invoke(wdg);

      wdg.ShowDialog(this);
    }

    private void BtnRelay_Click(object sender, EventArgs e)
    {
      using (WdgRelay wdg = new WdgRelay())
      {
        List<CostFromTo> infos = null;
        if (!_vm.IsRouteCalculatorNull)
        { infos = new List<CostFromTo>(_vm.RouteCalculator.RouteCostDict.Keys); }
        wdg.Init(_vm.CourseFile, infos);
        wdg.ShowDialog();
      }
    }

    private WdgPermutations _wdgVariations;
    private WdgPermutations WdgVariations
    {
      get
      {
        if (_wdgVariations == null || _wdgVariations.IsDisposed)
        { _wdgVariations = new WdgPermutations(); }
        return _wdgVariations;
      }
    }
    private void BtnPermutations_Click(object sender, EventArgs e)
    {
      WdgPermutations wdg = WdgVariations;

      wdg.SetData(_vm.CourseFile, this);

      if (wdg.Visible == false)
      { wdg.Show(this); }
    }

    private void BtnTrack_Click(object sender, EventArgs e)
    {
      string error = TryRunTrack();
      if (error != null)
      {
        MessageBox.Show(error);
      }
    }

    private string TryRunTrack()
    {
      using (WdgTrack wdg = new WdgTrack())
      {
        wdg.SetData(_vm.CourseFile, this);
        wdg.ShowDialog();
      }

      if (dgvInfo.CurrentRow == null)
      { return "No Course selected"; }
      if (dgvInfo.CurrentRow.DataBoundItem == null)
      { return "No Course selected"; }

      ICost row = (ICost)dgvInfo.CurrentRow.DataBoundItem;
      string comb = row.Name;
      if (comb.StartsWith(OCourseVm._meanPrefix))
      { return string.Format("Invalid Course {0} selected", comb); }

      Setup setup;
      Course course;
      using (OcadReader reader = OcadReader.Open(_vm.CourseFile))
      {
        setup = reader.ReadSetup();
        course = reader.ReadCourse(_vm.CourseName);
      }
      List<Ocad.Control> controls = course.GetCombination(comb);
      List<Ocad.Control> prjControls = new List<Ocad.Control>(controls.Count);

      foreach (var control in controls)
      {
        if (control.Element == null)
        { continue; }
        if (!(control.Element.Geometry is GeoElement.Point p))
        { continue; }

        control.Element.Geometry = control.Element.Geometry;
        prjControls.Add(control);
      }
      using (WdgTrack wdg = new WdgTrack())
      {
        wdg.SetData(_vm.CourseFile, this);
        wdg.ShowDialog();
      }

      return null;
    }

    private void BtnCreateScripts_Click(object sender, EventArgs e)
    {
      dlgSave.Filter = "*.bat | *.bat";
      if (dlgSave.ShowDialog(this) != DialogResult.OK)
      { return; }

      _vm.InitHeightVelo();
      _vm.SetCourseList();

      _vm.RouteCalculator.ScriptEvent(_vm.CourseFile, _vm.LcpConfig.HeightPath,
              _vm.LcpConfig.VeloPath, _vm.LcpConfig.Resolution, dlgSave.FileName);
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      _vm.CancelCalc();
    }

    private void BtnExportCourses_Click(object sender, EventArgs e)
    {
      using (WdgExport wdg = new WdgExport { TemplateFile = _vm.CourseFile })
      {
        if (wdg.ShowDialog(this) != DialogResult.OK)
        { return; }
        IEnumerable<ICost> selectedCosts = DataGridViewUtils.GetSelectedItems(dgvInfo).Cast<ICost>();
        IEnumerable<CostSectionlist> selectedCombs = CostSectionlist.GetUniqueCombs(selectedCosts);
        string courseName = null;
        if (_vm.Course != null)
        { courseName = PermutationUtils.GetCoreCourseName(_vm.Course.Name); }
        using (CmdCourseTransfer cmd = new CmdCourseTransfer(wdg.ExportFile, wdg.TemplateFile, _vm.CourseFile))
        {
          cmd.Export(selectedCombs.Select(comb => GetCourse(courseName, comb, wdg.SplitCourses)), courseName);
        }
      }
    }

    private Course GetCourse(string prefix, CostSectionlist comb, bool split)
    {
      Course course = comb.Sections.ToSimpleCourse();

      if (split)
      {
        VariationBuilder.Split(course);
      }

      string name;
      if (string.IsNullOrEmpty(comb.Name))
      { name = prefix; }
      else
      { name = $"{prefix}.{comb.Name}"; }
      course.Name = name;
      course.Climb = Basics.Utils.Round(comb.Climb, 5);

      return course;
    }

    private void BtnExportCsv_Click(object sender, EventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog
      { Filter = "*.txt|*.txt" };
      if (dlg.ShowDialog() != DialogResult.OK)
      { return; }

      string fileName = dlg.FileName;
      IEnumerable<ICost> selectedCosts = DataGridViewUtils.GetSelectedItems(dgvInfo).Cast<ICost>();
      IEnumerable<CostSectionlist> selectedCombs = CostSectionlist.GetUniqueCombs(selectedCosts);
      List<Course> courses = new List<Course>();
      using (TextWriter w = new StreamWriter(fileName))
      {
        foreach (var comb in selectedCombs)
        {
          ICost cost = comb;
          List<CourseXmlDocument.ControlDist> controlDists = new List<CourseXmlDocument.ControlDist>();
          //foreach (var c in row.Combination.Controls)
          //{ course.AddLast(c); }
          //courses.Add(course);

          string line = CourseXmlDocument.GetLineV8("Staffel", comb.Name, cost.DirectKm * 1000, comb.Climb, controlDists);
          w.WriteLine(line);
        }

        w.WriteLine();

        OCourseVm temp = new OCourseVm();
        temp.CourseFile = _vm.CourseFile;

        List<string> lines = new List<string>();
        foreach (var comb in selectedCombs)
        {
          ICost cost = comb;
          string courseName = comb.Name.Trim();
          if (!temp.CourseNames.Contains(courseName))
          {
            continue;
          }
          temp.CourseName = courseName;

          List<string> catNames = new List<string>(temp.CategoryNames);
          if (catNames.Count > 1)
          { catNames.RemoveAt(0); } // remove course name
          foreach (var cat in catNames)
          {
            List<CourseXmlDocument.ControlDist> controlDists = new List<CourseXmlDocument.ControlDist>();

            double climb = comb.Climb;
            climb = (int)(climb / 5) * 5;
            string line = $"{cat}; {temp.Course.Count - 1}; {cost.DirectKm:N1}; {climb:N0}; {cost.DirectLKm:N1}; {cost.OptimalKm:N1}; {cost.OptimalLKm:N1}";
            lines.Add(line);
          }
        }
        lines.Sort();
        foreach (var line in lines)
        {
          w.WriteLine(line);
        }

        w.Close();
      }
    }

    private void BtnCalcPermut_Click(object sender, EventArgs e)
    {
      try
      {
        dgvPermut.SuspendLayout();
        dgvPermut.Columns.Clear();
        dgvPermut.AutoGenerateColumns = false;
        dgvPermut.RowHeadersVisible = true;

        _vm.PermutationsInit();
      }
      finally
      { dgvPermut.ResumeLayout(); }
    }

    private void BtnExportPermutOcad_Click(object sender, EventArgs e)
    {
      using (WdgExport wdg = new WdgExport { TemplateFile = _vm.CourseFile })
      {
        if (wdg.ShowDialog(this) != DialogResult.OK)
        { return; }

        string courseName = null;
        if (_vm.Course != null)
        { courseName = PermutationUtils.GetCoreCourseName(_vm.Course.Name); }

        IEnumerable<PermutationVm> selectedPermuts = DataGridViewUtils.GetSelectedItems(dgvPermut).Cast<PermutationVm>();
        IEnumerable<CostSectionlist> selectedCombs =
          CostSectionlist.GetCostSectionLists(selectedPermuts, _vm.RouteCalculator, _vm.LcpConfig.Resolution);
        using (CmdCourseTransfer cmd = new CmdCourseTransfer(wdg.ExportFile, wdg.TemplateFile, _vm.CourseFile))
        {
          cmd.Export(selectedCombs.Select(comb => GetCourse(courseName, comb, wdg.SplitCourses)), courseName);
        }
      }
    }

    private void BtnExportPermutCsv_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      string exportFile;
      using (SaveFileDialog dlg = new SaveFileDialog())
      {
        dlg.Title = "Append to File";
        dlg.OverwritePrompt = false;
        if (dlg.ShowDialog(this) != DialogResult.OK)
        { return; }

        exportFile = dlg.FileName;
      }

      _vm.PermutationsExport(DataGridViewUtils.GetSelectedItems(dgvPermut).Cast<PermutationVm>(), exportFile);
    }


    private void WdgOCourse_Load(object sender, EventArgs e)
    {
      if (_vm == null)
      { Vm = new OCourseVm(); }

      Init?.Invoke(this);
      InitGridLayout();
    }

    private void MniOpen_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      dlgOpen.Filter = "*.xml | *.xml";
      if (dlgOpen.ShowDialog() != DialogResult.OK)
      { return; }

      _vm.LoadSettings(dlgOpen.FileName);
    }

    private void MniSave_Click(object sender, EventArgs e)
    {
      _vm?.SaveSettings();
    }

    private void MniSaveAs_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      dlgSave.Filter = "*.xml | *.xml";
      if (dlgSave.ShowDialog() != DialogResult.OK)
      { return; }

      _vm.SaveSettings(dlgSave.FileName);
    }

    private void BtnRefreshSection_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      foreach (var o in dgvInfo.SelectedRows)
      {
        DataGridViewRow row = (DataGridViewRow)o;
        if (row.DataBoundItem is CostFromTo cost)
        {
          _vm.RouteCalculator.RouteCostDict.Remove(cost);
        }
      }
    }
  }
}