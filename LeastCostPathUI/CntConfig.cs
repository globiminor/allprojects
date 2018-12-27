using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Grid.Lcp;
using GuiUtils;
using LeastCostPathUI.Properties;

namespace LeastCostPathUI
{
  public partial class CntConfig : UserControl
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

    public CntConfig()
    {
      InitializeComponent();

      _bindingSource = new BindingSource(components);

      Msg.Culture = System.Globalization.CultureInfo.CreateSpecificCulture("de-CH");

      SetText();
    }

    private void SetText()
    {
      lblHeight.Text = Msg.HeightModel;
      lblVelo.Text = Msg.VeloModel;
      lblResol.Text = Msg.Resolution;
      lblCost.Text = Msg.CostModel;
      lblSteps.Text = Msg.Steps;
      lblVelotyp.Text = Msg.VeloType;
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
        foreach (var step in _vm.StepsModes)
        {
          steps.Add(new StepImage(step, GetImage(step)));
        }

        if (notBound)
        {
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
  }
}
