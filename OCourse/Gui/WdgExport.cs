using System;
using System.Windows.Forms;

namespace OCourse.Gui
{
  public partial class WdgExport : Form
  {
    public WdgExport()
    {
      InitializeComponent();
    }
    public string ExportFile
    {
      get { return txtExport.Text; }
      set { txtExport.Text = value; }
    }
    public string TemplateFile
    {
      get { return txtTemplate.Text; }
      set { txtTemplate.Text = value; }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      DialogResult = System.Windows.Forms.DialogResult.OK;
      Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = System.Windows.Forms.DialogResult.Cancel;
      Close();
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
      dlgSave.Filter = "*.ocd|*.ocd";
      if (dlgSave.ShowDialog(this) != DialogResult.OK)
      { return; }

      txtExport.Text = dlgSave.FileName;
    }

    private void btnTemplate_Click(object sender, EventArgs e)
    {
      if (dlgOpen.ShowDialog(this) != DialogResult.OK)
      { return; }

      txtTemplate.Text = dlgOpen.FileName;
    }
  }
}