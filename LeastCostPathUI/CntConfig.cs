using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Grid;
using Grid.Lcp;

namespace LeastCostPathUI
{
  public partial class CntConfig : UserControl
  {
    private class StepImage
    {
      public Image Image;
      public Steps Step;
      public StepImage(Steps step, Image img)
      {
        Step = step;
        Image = img;
      }
    }

    private const string _dhmExt = "*.asc";
    private const string _veloExt = "*.tif";
    public CntConfig()
    {
      InitializeComponent();

      txtHeight.Text = _dhmExt;
      txtVelo.Text = _veloExt;
      txtResol.Text = string.Format("{0:N1}", 4.0);
      lstStep.Items.AddRange(new object[]{
        new StepImage(Steps.Step16, Images.Step16),
        new StepImage(Steps.Step8, Images.Step8),
        new StepImage(Steps.Step4, Images.Step4),
        });
      lstStep.SelectedIndex = 0;

      SetCostProviderType(new VelocityCostProvider());
    }

    public ICostProvider CostProvider
    {
      get { return _costProvider; }
    }
    private void SetCostProviderType(ICostProvider costProvider)
    {
      _costProvider = costProvider;
      Type type = _costProvider.GetType();
      txtCost.Text = type.Name;
      ttp.SetToolTip(txtCost, type.AssemblyQualifiedName);
    }

    public string HeightName
    { get { return txtHeight.Text; } }
    public DoubleGrid HeightGrid
    {
      get
      {
        if (_grdHeight == null)
        {
          if (File.Exists(txtHeight.Text))
          {
            _grdHeight = DoubleGrid.FromAsciiFile(txtHeight.Text, 0, 0.01, typeof(double));
          }
        }
        return _grdHeight;
      }
    }

    public string VelocityName
    { get { return txtVelo.Text; } }

    public double Resolution
    {
      get { return double.Parse(txtResol.Text); }
      set { txtResol.Text = string.Format("{0:N1}", value); }
    }

    public Steps Step
    {
      get
      {
        StepImage step = (StepImage)lstStep.SelectedItem;
        return step.Step;
      }
    }
    private DoubleGrid _grdHeight;

    private ICostProvider _costProvider;

    private void txtResol_Validating(object sender, CancelEventArgs e)
    {
      try
      {
        double d = Convert.ToDouble(txtResol.Text);
        if (d < 0)
        { e.Cancel = true; }
      }
      catch
      { e.Cancel = true; }
    }

    private void lstStep_DrawItem(object sender, DrawItemEventArgs e)
    {
      StepImage step = (StepImage)lstStep.Items[e.Index];
      e.Graphics.DrawImage(step.Image, 4, 2 + e.Bounds.Y);
    }

    private void btnStepCost_Click(object sender, EventArgs e)
    {
      WdgCustom wdg = new WdgCustom();
      if (wdg.ShowDialog(this) != DialogResult.OK)
      { return; }

      Type type = wdg.CostProviderType;
      ICostProvider p = (ICostProvider)Activator.CreateInstance(type, true);
      //      SetStepCost((StepCostHandler<double>)Delegate.CreateDelegate(typeof(StepCostHandler<double>), type));
      SetCostProviderType(p);
    }

    private void btnHeight_Click(object sender, EventArgs e)
    {
      try
      {
        dlgOpen.Filter = "height grid files|*.asc;*.agr;*.grd;*.txt|*.asc|*.asc|*.agr|*.agr|*.grd|*.grd|All Files|*.*";
        if (dlgOpen.ShowDialog() == DialogResult.OK)
        {
          txtHeight.Text = dlgOpen.FileName;
        }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void btnVelo_Click(object sender, EventArgs e)
    {
      try
      {
        string velocityName;
        using (OpenFileDialog dlg = new OpenFileDialog())
        {
          dlg.InitialDirectory = Path.GetDirectoryName(txtVelo.Text);
          dlg.Filter = _costProvider.FileFilter;
          if (dlg.ShowDialog() != DialogResult.OK)
          { return; }
          velocityName = dlg.FileName;
        }

        if (velocityName != null)
        {
          txtVelo.Text = velocityName;
        }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void txtHeight_TextChanged(object sender, EventArgs e)
    {
      _grdHeight = null;
    }

    private void txtVelo_TextChanged(object sender, EventArgs e)
    {
    }

    public void SetConfig(string dir, Config config)
    {
      if (config == null) return;
      bool setResol = false;
      if (!File.Exists(txtHeight.Text) && !string.IsNullOrEmpty(config.Dhm))
      {
        txtHeight.Text = Config.GetFullPath(dir, config.Dhm);
        setResol = true;
      }
      if (!File.Exists(txtVelo.Text) && !string.IsNullOrEmpty(config.Velo))
      {
        txtVelo.Text = Config.GetFullPath(dir, config.Velo);
        setResol = true;
      }
      if (setResol && config.Resolution > 0)
      { Resolution = config.Resolution; }
    }
  }
}
