using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Basics;
using System.Collections.Generic;
using Grid.Lcp;
using GuiUtils;
using Basics.Forms;
using System.ComponentModel;

namespace LeastCostPathUI
{
  public partial class CntConfigView : UserControl
  {
    private class StepImage
    {
      public Image Image { get; set; }
      public Steps Step { get; set; }
      public StepImage(Steps step, Image img)
      {
        Step = step;
        Image = img;
      }
    }

    private readonly BindingSource _bindingSource;
    private ConfigVm _vm;

    public CntConfigView()
    {
      InitializeComponent();

      _bindingSource = new BindingSource(components);
    }

    public static Image GetImage(Steps steps)
    {
      Image img = null;
      if (steps.Count == Steps.Step16.Count) { img = Images.Step16; }
      else if (steps.Count == Steps.Step8.Count) { img = Images.Step8; }
      else if (steps.Count == Steps.Step4.Count) { img = Images.Step4; }

      return img;
    }

    public ConfigVm ConfigVm
    {
      get { return _vm; }
      set
      {
        _vm = value;
        bool notBound = _bindingSource.DataSource == null;

        _bindingSource.DataSource = _vm;

        if (value == null)
        { return; }

        List<StepImage> steps = new List<StepImage>();
        foreach (Steps step in _vm.StepsModes)
        {
          steps.Add(new StepImage(step, GetImage(step)));
        }
        grdLevels.DataSource = _vm.ModelLevels;

        if (notBound)
        {
          InitGridLayout();

          optHeightVelo.Bind(x => x.Checked, _bindingSource, nameof(_vm.TerrainVeloModel),
            false, DataSourceUpdateMode.OnPropertyChanged);
          pnlTerrainVelo.Bind(x => x.Visible, _bindingSource, nameof(_vm.TerrainVeloModel),
            false, DataSourceUpdateMode.Never);

          optMultiLayer.Bind(x => x.Checked, _bindingSource, nameof(_vm.MultiLayerModel),
            false, DataSourceUpdateMode.OnPropertyChanged);
          pnlMulti.Bind(x => x.Visible, _bindingSource, nameof(_vm.MultiLayerModel),
            false, DataSourceUpdateMode.Never);

          txtHeight.Bind(x => x.Text, _bindingSource, nameof(_vm.HeightPath),
            true, DataSourceUpdateMode.OnPropertyChanged);

          txtVelo.Bind(x => x.Text, _bindingSource, nameof(_vm.VeloPath),
            true, DataSourceUpdateMode.OnPropertyChanged);

          txtResol.Bind(x => x.Text, _bindingSource, nameof(_vm.Resolution),
            true, DataSourceUpdateMode.OnPropertyChanged).FormatString = "N1";

          txtCost.Bind(x => x.Text, _bindingSource, nameof(_vm.CostTypeName),
            true, DataSourceUpdateMode.Never);

          _lstStep.DataSource = steps;
          _lstStep.ValueMember = nameof(StepImage.Step);
          _lstStep.DisplayMember = nameof(StepImage.Image);
          _lstStep.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.StepsMode),
            true, DataSourceUpdateMode.OnPropertyChanged);
        }
      }
    }

    private void InitGridLayout()
    {
      try
      {
        grdLevels.SuspendLayout();
        grdLevels.Columns.Clear();
        grdLevels.AutoGenerateColumns = false;
        grdLevels.RowHeadersVisible = true;
        grdLevels.RowHeadersWidth = 20;

        grdLevels.AllowUserToAddRows = false;

        ConfigVm c = null;

        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.HeightPath),
            HeaderText = "Terrain",
            ReadOnly = true,
            Width = 80
          };
          FilterHeaderCell.CreateCore(col);

          grdLevels.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.VeloPath),
            HeaderText = "Velocity",
            ReadOnly = true,
            Width = 80
          };
          FilterHeaderCell.CreateCore(col);

          grdLevels.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.TeleportPath),
            HeaderText = "Teleports",
            ReadOnly = true,
            Width = 80
          };
          FilterHeaderCell.CreateCore(col);

          grdLevels.Columns.Add(col);
        }
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
          {
            DataPropertyName = nameof(c.Resolution),
            HeaderText = "Resol.",
            ReadOnly = true,
            Width = 40
          };
          col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          col.DefaultCellStyle.Format = "N1";
          FilterHeaderCell.CreateCore(col);

          grdLevels.Columns.Add(col);
        }
        {
          DataGridViewImageColumn col = new DataGridViewImageColumn
          {
            CellTemplate = new StepImageCell(),
            DataPropertyName = nameof(c.StepsMode),
            HeaderText = "Steps",
            ReadOnly = true,
            Width = 40,
          };
          FilterHeaderCell.CreateCore(col);

          grdLevels.Columns.Add(col);
        }
      }
      finally
      {
        grdLevels.ResumeLayout();
      }
    }
    private class StepImageCell : DataGridViewImageCell
    {
      protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
      {
        if (value is Steps steps)
        { value = GetImage(steps); }
        return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
      }
    }

    private void LstStep_DrawItem(object sender, DrawItemEventArgs e)
    {
      if (_lstStep.DataSource is IList<StepImage> steps && e.Index >= 0 && e.Index < steps.Count)
      {
        StepImage step = steps[e.Index];
        e.Graphics.DrawImage(step.Image, 4, 2 + e.Bounds.Y);
      }
    }

    private void BtnStepCost_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      WdgCustom wdg = new WdgCustom();
      if (wdg.ShowDialog(this) != DialogResult.OK)
      { return; }

      _vm.TvmCalc = wdg.TvmCalc;
    }

    private void BtnHeight_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }
      try
      {
        dlgOpen.Filter = "height grid files|*.asc;*.agr;*.grd;*.txt|*.asc|*.asc|*.agr|*.agr|*.grd|*.grd|All Files|*.*";
        if (dlgOpen.ShowDialog() == DialogResult.OK)
        { _vm.HeightPath = dlgOpen.FileName; }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void BtnVelo_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }
      try
      {
        string velocityName;
        using (OpenFileDialog dlg = new OpenFileDialog())
        {
          dlg.InitialDirectory = Path.GetDirectoryName(_vm.VeloPath);
          dlg.Filter = _vm.VeloFileFilter;
          if (dlg.ShowDialog() != DialogResult.OK)
          { return; }
          velocityName = dlg.FileName;
        }

        if (velocityName != null)
        { _vm.VeloPath = velocityName; }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void BtnAddLevel_Click(object sender, EventArgs e)
    {
      _vm.AddLevel();
    }

    private void btnAddLevel_Click(object sender, EventArgs e)
    {

    }
  }
}
