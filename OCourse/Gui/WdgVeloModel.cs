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
          this.Bind(x => x.Text, _bindingSource, nameof(_vm.Title),
            true, DataSourceUpdateMode.Never);

          txtMap.Bind(x => x.Text, _bindingSource, nameof(_vm.MapName),
            true, DataSourceUpdateMode.OnValidation);

        }
      }
    }

    private void MniOpen_Click(object sender, EventArgs e)
    {
      if (_vm == null)
      { return; }

      OpenFileDialog dlgOpen = new OpenFileDialog();
      dlgOpen.Filter = "*.xml | *.xml";
      if (dlgOpen.ShowDialog() != DialogResult.OK)
      { return; }

      _vm.LoadSettings(dlgOpen.FileName);
    }
  }
}
