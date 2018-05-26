using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Dhm
{
  public partial class WdgSetGridHeight : Form
  {
    private SetGridHeight _setGridHeight;
    private BindingSource _bindingSource;
    public WdgSetGridHeight()
    {
      InitializeComponent();
    }

    public void SetData(SetGridHeight g)
    {
      components = new Container();
      _bindingSource = new BindingSource(components);
      _bindingSource.DataSource = g;
      _setGridHeight = g;

      txtGrid.DataBindings.Add(new Binding("Text", _bindingSource, "Breite", true,
        DataSourceUpdateMode.OnPropertyChanged));

      lstGrids.DataSource = g.Grids;
      lstGrids.DisplayMember = "Name";
      lstGrids.ValueMember = "Grid";
      lstGrids.DataBindings.Add(new Binding("SelectedValue", _bindingSource, "Grid", false,
        DataSourceUpdateMode.OnPropertyChanged));
    }

    private void BtnExport_Click(object sender, EventArgs e)
    {
      if (_setGridHeight == null) { return; }

      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter = "*.asc | *.asc";
      if (dlg.ShowDialog(this) != DialogResult.OK)
      { return; }

      string file = dlg.FileName;

      Grid.DoubleGrid.SaveASCII(_setGridHeight.Grid, file, "N2");
    }

    public void SetInfo(string info)
    {
      lblPos.Text = info;
    }
  }
}