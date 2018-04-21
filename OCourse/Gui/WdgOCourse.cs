using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using LeastCostPathUI;
using Basics.Geom;
using Ocad;
using OCourse.Commands;
using OCourse.Ext;
using OCourse.Route;
using OCourse.ViewModels;
using Basics.Views;
using Basics;
using GuiUtils;
using Basics.Forms;

namespace OCourse.Gui
{
  public partial class WdgOCourse : Form
  {
    private delegate void InitHandler(WdgOCourse wdg);

    private static event InitHandler Init;

    private List<CostSectionlist> _fullInfo;

    List<ICost> _selectedRoute;
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
        if (_working == value)
        { return; }

        _working = value;
        SetLayout(_working);
      }
    }

    private DataView _permutations;
    public DataView Permutations
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

          DataTable permutTbl = _permutations.Table;
          {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
            {
              DataPropertyName = OCourseVm.StartNrName,
              HeaderText = "StartNr"
            };
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Width = 50;
            dgvPermut.Columns.Add(col);
          }
          {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
            {
              DataPropertyName = OCourseVm.IndexName,
              HeaderText = "Index",
              Width = 50
            };
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPermut.Columns.Add(col);
          }

          int nPartName = OCourseVm.PartName.Length;
          foreach (DataColumn column in permutTbl.Columns)
          {
            if (!column.ColumnName.StartsWith(OCourseVm.PartName))
            {
              continue;
            }
            DataGridViewColumn col = new DataGridViewTextBoxColumn
            {
              DataPropertyName = column.ColumnName,
              HeaderText = string.Format("{0} {1}", "Part", column.ColumnName.Substring(nPartName))
            };
            dgvPermut.Columns.Add(col);
          }
        }
        finally
        {
          dgvPermut.ResumeLayout();
          _suspend = origSuspend;
        }
      }
    }

    private void SetLayout(bool calculating)
    {
      foreach (System.Windows.Forms.Control ctr in Controls)
      {
        ctr.Enabled = !calculating;
      }
      btnCancel.Enabled = calculating;
      btnCancel.Visible = calculating;
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

      IInfo info = item as IInfo;
      if (info == null)
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
        Setup setup;
        using (OcadReader reader = OcadReader.Open(_vm.CourseFile))
        {
          setup = reader.ReadSetup();
        }
        info = _vm.CalcCourse(list, setup);
      }
      else
      {
        info = _fullInfo;
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
      _vm.SetSelectionedComb(selected);

      btnRefreshSection.Enabled = false;
      foreach (DataGridViewRow row in dgvInfo.SelectedRows)
      {
        if (row.DataBoundItem is CostFromTo)
        {
          btnRefreshSection.Enabled = true;
          break;
        }
      }
    }

    private void DgvVars_SelectionChanged(object sender, EventArgs e)
    {
      if (_suspend)
      { return; }
      Vm.DrawCourse();
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
      if (cell == null)
      { return null; }
      DataView v = cell.DataGridView.DataSource as DataView;
      if (v == null)
      { return null; }
      if (cell.RowIndex > v.Count || cell.RowIndex < 0)
      { return null; }
      DataRowView vRow = cell.DataGridView.Rows[cell.RowIndex].DataBoundItem as DataRowView;
      if (vRow == null)
      { return null; }
      IList<SectionList> parts = vRow[0] as IList<SectionList>;
      if (parts == null)
      { return null; }
      int idx = cell.ColumnIndex - 2; // startNrCol und indexCol
      if (idx < 0)
      { return null; }
      if (parts.Count <= idx)
      { return null; }

      SectionList part = parts[idx];
      return part;
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
      foreach (DataGridViewRow gRow in dgvInfo.Rows)
      {
        CostSectionlist comb = gRow.DataBoundItem as CostSectionlist;

        if (comb == null)
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

      IPoint point;

      point = cStart.GetPoint();
      IPoint start = point.Project(setup.Map2Prj);
      point = cEnd.GetPoint();
      IPoint end = point.Project(setup.Map2Prj);

      double l = Math.Sqrt(PointOperator.Dist2(start, end)) / 2.0;
      Box box = _vm.RouteCalculator.GetBox(start, end, l);

      wdg.Init(_vm.LcpConfig.CostProvider, _vm.RouteCalculator.HeightGrid,
        _vm.RouteCalculator.VeloGrid, _vm.LcpConfig.Resolution, _vm.LcpConfig.StepsMode);
      wdg.SetExtent(box);
      wdg.SetStart(start);
      wdg.SetEnd(end);
      wdg.SetNames(cStart.Name, cEnd.Name);
      wdg.SetMainPath(Path.GetDirectoryName(dlgOpen.FileName));

      wdg.ShowDialog(this);
    }

    private void BtnRelay_Click(object sender, EventArgs e)
    {
      WdgRelay wdg = new WdgRelay();
      List<CostFromTo> infos = null;
      if (!_vm.IsRouteCalculatorNull)
      { infos = new List<CostFromTo>(_vm.RouteCalculator.RouteCostDict.Keys); }
      wdg.Init(_vm.CourseFile, infos);
      wdg.ShowDialog();
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
      {
        WdgTrack wdg = new WdgTrack();

        wdg.SetData(_vm.CourseFile, this);
        wdg.ShowDialog();
      }

      if (dgvInfo.CurrentRow == null)
      { return "No Course selected"; }
      if (dgvInfo.CurrentRow.DataBoundItem == null)
      { return "No Course selected"; }

      ICost row = (ICost)dgvInfo.CurrentRow.DataBoundItem;
      string comb = row.Name;
      if (comb.StartsWith(OCourseVm.MeanPrefix))
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

      foreach (Ocad.Control control in controls)
      {
        if (control.Element == null)
        { continue; }
        IPoint p = control.Element.Geometry as IPoint;
        if (p == null)
        { continue; }

        control.Element.Geometry = control.Element.Geometry.Project(setup.Map2Prj);
        prjControls.Add(control);
      }
      {
        WdgTrack wdg = new WdgTrack();

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
      IEnumerable<CostSectionlist> selectedCombs = CostSectionlist.GetUniqueCombs(GetSeletedCombs());

      WdgExport wdg = new WdgExport { TemplateFile = _vm.CourseFile };
      if (wdg.ShowDialog(this) != DialogResult.OK)
      { return; }

      string courseName = null;
      if (_vm.Course != null)
      { courseName = PermutationUtils.GetCoreCourseName(_vm.Course.Name); }
      using (CmdCourseTransfer cmd = new CmdCourseTransfer(wdg.ExportFile, wdg.TemplateFile, _vm.CourseFile))
      {
        cmd.Export(courseName, selectedCombs, courseName);
      }
    }

    private void BtnExportCsv_Click(object sender, EventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog
      { Filter = "*.txt|*.txt" };
      if (dlg.ShowDialog() != DialogResult.OK)
      { return; }

      string fileName = dlg.FileName;
      IEnumerable<CostSectionlist> selectedCombs = CostSectionlist.GetUniqueCombs(GetSeletedCombs());
      List<Course> courses = new List<Course>();
      using (TextWriter w = new StreamWriter(fileName))
      {
        foreach (ICost comb in selectedCombs)
        {
          List<CourseXmlDocument.ControlDist> controlDists = new List<CourseXmlDocument.ControlDist>();
          //foreach (Ocad.Control c in row.Combination.Controls)
          //{ course.AddLast(c); }
          //courses.Add(course);

          string line = CourseXmlDocument.GetLineV8("Staffel", comb.Name, comb.DirectKm * 1000, comb.Climb, controlDists);
          w.WriteLine(line);
        }

        w.WriteLine();

        OCourseVm temp = new OCourseVm();
        temp.CourseFile = _vm.CourseFile;

        List<string> lines = new List<string>();
        foreach (ICost comb in selectedCombs)
        {
          string courseName = comb.Name.Trim();
          if (!temp.CourseNames.Contains(courseName))
          {
            continue;
          }
          temp.CourseName = courseName;

          List<string> catNames = new List<string>(temp.CategoryNames);
          if (catNames.Count > 1)
          { catNames.RemoveAt(0); } // remove course name
          foreach (string cat in catNames)
          {
            List<CourseXmlDocument.ControlDist> controlDists = new List<CourseXmlDocument.ControlDist>();

            double climb = comb.Climb;
            climb = (int)(climb / 5) * 5;
            string line = $"{cat}; {temp.Course.Count - 1}; {comb.DirectKm:N1}; {climb:N0}; {comb.DirectLKm:N1}; {comb.OptimalKm:N1}; {comb.OptimalLKm:N1}";
            lines.Add(line);
          }
        }
        lines.Sort();
        foreach (string line in lines)
        {
          w.WriteLine(line);
        }

        w.Close();
      }
    }

    private IEnumerable<ICost> GetSeletedCombs()
    {
      foreach (DataGridViewRow row in dgvInfo.Rows)
      {
        if (!row.Selected)
        { continue; }
        ICost comb = row.DataBoundItem as ICost;
        if (comb == null)
        { continue; }

        yield return comb;
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

      foreach (DataGridViewRow row in dgvInfo.SelectedRows)
      {
        if (row.DataBoundItem is CostFromTo cost)
        {
          _vm.RouteCalculator.RouteCostDict.Remove(cost);
        }
      }
    }
  }
}