using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Basics;
using System.Collections.Generic;
using Grid.Lcp;
using GuiUtils;

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
          Image add = null;
          if (step.Count == Steps.Step16.Count) { add = Images.Step16; }
          else if (step.Count == Steps.Step8.Count) { add = Images.Step8; }
          else if (step.Count == Steps.Step4.Count) { add = Images.Step4; }

          steps.Add(new StepImage(step, add));
        }

        if (notBound)
        {
          txtHeight.Bind(x => x.Text, _bindingSource, nameof(_vm.HeightPath),
            true, DataSourceUpdateMode.OnPropertyChanged);

          txtVelo.Bind(x => x.Text, _bindingSource, nameof(_vm.VeloPath),
            true, DataSourceUpdateMode.OnPropertyChanged);

          txtResol.Bind(x => x.Text, _bindingSource, nameof(_vm.Resolution),
            true, DataSourceUpdateMode.OnPropertyChanged).FormatString = "N1";

          txtCost.Bind(x => x.Text, _bindingSource, nameof(_vm.CostProviderName),
            true, DataSourceUpdateMode.Never);

          _lstStep.DataSource = steps;
          _lstStep.ValueMember = nameof(StepImage.Step);
          _lstStep.DisplayMember = nameof(StepImage.Image);
          _lstStep.Bind(x => x.SelectedValue, _bindingSource, nameof(_vm.StepsMode),
            true, DataSourceUpdateMode.OnPropertyChanged);
        }
      }
    }

    private void lstStep_DrawItem(object sender, DrawItemEventArgs e)
    {
      IList<StepImage> steps = _lstStep.DataSource as IList<StepImage>;
      if (steps != null && e.Index >= 0 && e.Index < steps.Count)
      {
        StepImage step = steps[e.Index];
        e.Graphics.DrawImage(step.Image, 4, 2 + e.Bounds.Y);
      }
    }

    private void btnStepCost_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      WdgCustom wdg = new WdgCustom();
      if (wdg.ShowDialog(this) != DialogResult.OK)
      { return; }

      Type type = wdg.CostProviderType;
      ICostProvider p = (ICostProvider)Activator.CreateInstance(type, true);
      //      SetStepCost((StepCostHandler<double>)Delegate.CreateDelegate(typeof(StepCostHandler<double>), type));
      _vm.SetCostProviderType(p);
    }

    private void btnHeight_Click(object sender, EventArgs e)
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

    private void btnVelo_Click(object sender, EventArgs e)
    {
      if (_vm == null || _vm.CostProvider == null)
      { return; }
      try
      {
        string velocityName;
        using (OpenFileDialog dlg = new OpenFileDialog())
        {
          dlg.InitialDirectory = Path.GetDirectoryName(_vm.VeloPath);
          dlg.Filter = _vm.CostProvider.FileFilter;
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
