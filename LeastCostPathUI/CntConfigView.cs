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
using LeastCostPathUI.Properties;

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
      SetText();
    }
    private void SetText()
    {
      optHeightVelo.Text = Msg.HeightVeloModel;
      optMultiLayer.Text = Msg.MultiLeyelModel;
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
        cntConfigSimple.ConfigVm = value;

        bool notBound = _bindingSource.DataSource == null;

        _bindingSource.DataSource = _vm;


        if (value == null)
        { return; }

        List<StepImage> steps = new List<StepImage>();
        foreach (var step in _vm.StepsModes)
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
            HeaderText = Msg.HeightModel,
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
            HeaderText = Msg.VeloModel,
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
            HeaderText = Msg.Resolution,
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

    private void BtnAddLevel_Click(object sender, EventArgs e)
    {
      ConfigVm detail = _vm.AddLevel();
      EditDetail(detail);
    }

    private void EditDetail(ConfigVm detail)
    {
      Form wdgEdit = new Form();
      wdgEdit.Controls.Add(new CntConfig());
      wdgEdit.Show();
    }
  }
}
