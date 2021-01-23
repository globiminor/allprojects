using Basics.Forms;
using OCourse.ViewModels;
using System;
using System.Windows.Forms;

namespace OCourse.Gui
{
  public partial class WdgVeloModel : Form
  {
    private VeloModelVm _vm;
    private readonly BindingSource _bindingSource;

    public WdgVeloModel()
    {
      InitializeComponent();
      _bindingSource = new BindingSource(components);
    }

    public VeloModelVm Vm
    {
      get { return _vm; }
      set
      {
        _vm = value;
        bool notBound = _bindingSource.DataSource == null;

        _bindingSource.DataSource = _vm;
        if (notBound)
        {
          InitGrdSymbol();

          this.Bind(x => x.SettingsPath, _bindingSource, nameof(_vm.SettingsPath),
            true, DataSourceUpdateMode.Never);

          txtMap.Bind(x => x.Text, _bindingSource, nameof(_vm.MapName),
            true, DataSourceUpdateMode.OnValidation);

          txtDefaultVelo.Bind(x => x.Text, _bindingSource, nameof(_vm.DefaultVelocity),
            true, DataSourceUpdateMode.OnPropertyChanged);
          txtDefaultPntSize.Bind(x => x.Text, _bindingSource, nameof(_vm.DefaultPntSize),
            true, DataSourceUpdateMode.OnPropertyChanged);

          grdSymbols.Bind(x => x.DataSource, _bindingSource, nameof(_vm.Symbols),
            true, DataSourceUpdateMode.Never);

          this.Bind(x => SettingSaveEnabled, _bindingSource, nameof(_vm.CanSave),
            false, DataSourceUpdateMode.Never);
        }
      }
    }

    [System.ComponentModel.Browsable(false)]
    public bool SettingSaveEnabled
    {
      get { return mniSave.Enabled; }
      set { mniSave.Enabled = value; }
    }
    [System.ComponentModel.Browsable(false)]
    public string SettingsPath
    {
      get { return null; }
      set { Text = $"Velocity Model {value}"; }
    }


    private void MniOpen_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      using (OpenFileDialog dlgOpen = new OpenFileDialog())
      {
        dlgOpen.Filter = "*.xml | *.xml";
        if (dlgOpen.ShowDialog() != DialogResult.OK)
        { return; }

        _vm.LoadSettings(dlgOpen.FileName);
      }
    }

    private void InitGrdSymbol()
    {
      grdSymbols.SuspendLayout();
      grdSymbols.Columns.Clear();
      grdSymbols.AutoGenerateColumns = false;
      grdSymbols.RowHeadersVisible = false;

      VeloModelVm.SymbolVm s = null;

      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Code),
          HeaderText = "Symbol",
          ReadOnly = true,
          Width = 60
        };
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Description),
          HeaderText = "Description",
          ReadOnly = true,
          Width = 250
        };

        FilterHeaderCell.CreateCore(col);
        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Velocity),
          HeaderText = "Velocity",
          Width = 50
        };
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        col.DefaultCellStyle.Format = "N3";
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Size),
          HeaderText = "Size [m]",
          Width = 50
        };
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        col.DefaultCellStyle.Format = "N1";
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Teleport),
          HeaderText = "Teleport Velocity",
          Width = 50
        };
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        col.DefaultCellStyle.Format = "N2";
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Priority),
          HeaderText = "Priority",
          Width = 30
        };
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        col.DefaultCellStyle.Format = "N0";
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.NObjects),
          HeaderText = "# Objects",
          Width = 50,
          ReadOnly = true
        };
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        col.DefaultCellStyle.Format = "N0";
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Type),
          HeaderText = "Symbol.Type",
          Width = 50,
          ReadOnly = true
        };
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }
      {
        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
        {
          DataPropertyName = nameof(s.Status),
          HeaderText = "Symb.Status",
          Width = 50,
          ReadOnly = true
        };
        FilterHeaderCell.CreateCore(col);

        grdSymbols.Columns.Add(col);
      }

    }
    private void Btnmap_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      using (OpenFileDialog dlgOpen = new OpenFileDialog())
      {
        dlgOpen.Filter = "*.ocd | *.ocd";
        if (dlgOpen.ShowDialog() != DialogResult.OK)
        { return; }

        _vm.SetMap(dlgOpen.FileName);
      }
    }

    private void MniSave_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(_vm?.SettingsPath))
      { return; }

      _vm.Save(_vm.SettingsPath);
    }

    private void MniSaveAs_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      using (SaveFileDialog dlgSave = new SaveFileDialog())
      {
        dlgSave.Filter = "*.xml | *.xml";
        if (dlgSave.ShowDialog() != DialogResult.OK)
        { return; }

        _vm.Save(dlgSave.FileName);
      }
    }
  }
}
