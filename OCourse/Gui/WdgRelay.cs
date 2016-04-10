using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using OCourse.Ext;
using OCourse.Route;

namespace OCourse
{
  public partial class WdgRelay : Form
  {
    private List<CostFromTo> _routeCosts;
    public WdgRelay()
    {
      InitializeComponent();
    }

    internal void Init(string courseOcd, List<CostFromTo> costInfos)
    {
      txtCourseOcd.Text = courseOcd;

      string dir = Path.GetDirectoryName(courseOcd);
      string file = Path.GetFileNameWithoutExtension(courseOcd);
      string root = Path.Combine(dir, file);

      txtCourseXml.Text = root + ".Courses.xml";

      txtMapFolder.Text = Path.Combine(dir, "Bahnen");
      txtGrafics.Text = root + "_grafics.ocd";

      _routeCosts = costInfos;
    }

    private void btnCourseXml_Click(object sender, EventArgs e)
    {
      string filter = dlgOpen.Filter;
      dlgOpen.Filter = "*.xml | *.xml";
      dlgOpen.FileName = txtCourseXml.Text;
      DialogResult res = dlgOpen.ShowDialog();
      dlgOpen.Filter = filter;
      if (res != DialogResult.OK)
      { return; }

      txtCourseXml.Text = dlgOpen.FileName;
    }

    private void btnVerify_Click(object sender, EventArgs e)
    {
      RelayAdapt adapt = new RelayAdapt(txtCourseOcd.Text, txtCourseXml.Text);
      adapt.VerifyFromTo();
    }

    private void btnEstimate_Click(object sender, EventArgs e)
    {
      RelayAdapt adapt = new RelayAdapt(txtCourseOcd.Text, txtCourseXml.Text);
      adapt.EstimateFromTo();
    }

    private void btnCombinations_Click(object sender, EventArgs e)
    {
      string sTxt = Path.GetDirectoryName(txtCourseOcd.Text)
        + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(txtCourseOcd.Text)
        + ".Courses.txt";
      string filter = dlgSave.Filter;
      dlgSave.Filter = "*.txt | *.txt";
      dlgSave.FileName = sTxt;
      DialogResult res = dlgSave.ShowDialog();
      dlgSave.Filter = filter;
      if (res != DialogResult.OK)
      { return; }

      sTxt = dlgSave.FileName;

      RelayAdapt adapt = new RelayAdapt(txtCourseOcd.Text);

      adapt.ExportCoursesTxt(sTxt, _routeCosts);
    }

    private void btnCreateCsv_Click(object sender, EventArgs e)
    {
      string csvExport = Path.GetDirectoryName(txtCourseXml.Text)
        + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(txtCourseXml.Text)
        + ".csv";

      RelayAdapt adapt = new RelayAdapt(txtCourseOcd.Text, txtCourseXml.Text);

      adapt.ExportCoursesCsv(csvExport);
    }

    private void btnExportTxtV8_Click(object sender, EventArgs e)
    {
      string txtExport = Path.GetDirectoryName(txtCourseXml.Text)
        + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(txtCourseXml.Text)
        + "V8.txt";

      RelayAdapt adapt = new RelayAdapt(txtCourseOcd.Text, txtCourseXml.Text);

      string dummyPrefix = null;
      if (chkDummyPrefix.Checked)
      { dummyPrefix = txtDummyPrefix.Text; }
      adapt.ExportCoursesTxtV8(txtExport, dummyPrefix);
    }

    private void btnAdaptMaps_Click(object sender, EventArgs e)
    {
      string txtTemplate = txtGrafics.Text;
      string txtBahn = txtMapFolder.Text;


      RelayAdapt adapt = new RelayAdapt(txtCourseOcd.Text, txtCourseXml.Text);

      IList<string> bahnen = Directory.GetFiles(txtBahn, "*.ocd");


      adapt.AdaptMaps(bahnen, txtTemplate, "__", _routeCosts);

    }

    private void btnGrafics_Click(object sender, EventArgs e)
    {
      string filter = dlgOpen.Filter;
      dlgOpen.Filter = "*.ocd | *.ocd";
      dlgOpen.FileName = txtGrafics.Text;
      DialogResult res = dlgOpen.ShowDialog();
      dlgOpen.Filter = filter;
      if (res != DialogResult.OK)
      { return; }

      txtGrafics.Text = dlgOpen.FileName;

    }

    private void bntMapFolder_Click(object sender, EventArgs e)
    {
      dlgFolder.SelectedPath = Path.GetDirectoryName(txtMapFolder.Text);

      if (dlgFolder.ShowDialog() != DialogResult.OK)
      { return; }

      txtMapFolder.Text = dlgFolder.SelectedPath;
    }
  }
}