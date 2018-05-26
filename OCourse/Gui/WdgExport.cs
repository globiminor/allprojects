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

    private void BtnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnExport_Click(object sender, EventArgs e)
    {
      dlgSave.Filter = "*.ocd|*.ocd";
      if (dlgSave.ShowDialog(this) != DialogResult.OK)
      { return; }

      txtExport.Text = dlgSave.FileName;
    }

    private void BtnTemplate_Click(object sender, EventArgs e)
    {
      if (dlgOpen.ShowDialog(this) != DialogResult.OK)
      { return; }

      txtTemplate.Text = dlgOpen.FileName;
    }
  }
}