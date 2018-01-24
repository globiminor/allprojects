using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using OCourse.Ext;
using System;

namespace OCourse.Gui
{
  public partial class WdgPermutations : Form
  {
    private WdgOCourse _parent;
    public WdgPermutations()
    {
      InitializeComponent();
    }

    public static readonly string RawSubdir = @"temp\Roh";
    public static readonly string PartSubdir = @"temp\Rein";
    public static readonly string CatSubdir = @"temp\Kategorien";

    public void SetData(string courseFile, WdgOCourse parent)
    {
      txtCourse.Text = courseFile;
      _parent = parent;

      //WdgOCourseBck.SetCourseList(courseFile, lstCourse);
      throw new NotImplementedException();

      string courseDir = Path.GetDirectoryName(courseFile);
      {
        string rawDir = Path.Combine(courseDir, RawSubdir);
        if (Directory.Exists(rawDir))
        { txtRawDir.Text = rawDir; }
        else
        { txtRawDir.Text = ""; }
      }
      {
        string varDir = Path.Combine(courseDir, PartSubdir);
        if (Directory.Exists(varDir))
        { txtClean.Text = varDir; }
        else
        { txtClean.Text = ""; }
      }
      {
        string comb = Combine(courseFile, "_comb_");
        txtCleanTemplates.Text = comb;
      }
      {
        string varDir = Path.Combine(courseDir, CatSubdir);
        if (Directory.Exists(varDir))
        { txtCategory.Text = varDir; }
        else
        { txtCategory.Text = ""; }
      }

      SetRawParent();
    }

    private void SetRawParent()
    {
      string exportKey;
      if (string.IsNullOrEmpty(lstCourse.Text) == false)
      {
        exportKey = lstCourse.Text;
      }
      else
      {
        exportKey = "_All_";
      }

      txtRawParent.Text = Combine(txtCourse.Text, exportKey);
    }
    private static string Combine(string name, string ext)
    {
      string comb = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(name)),
        Path.GetFileNameWithoutExtension(name) +
        "." + ext + Path.GetExtension(name));

      return comb;
    }

    private void CreateRaw()
    {
      IList<string> courseNames = GetCourseNames();
      if (courseNames.Count > 1)
      {
        _parent.Vm.RouteCalculator.CalcEvent(txtCourse.Text, _parent.Vm.LcpConfig.Resolution);
      }
      PermutationUtils.CreateCourseVariations(txtCourse.Text, txtRawParent.Text,
        courseNames, _parent.Vm.RouteCalculator, _parent.Vm.LcpConfig.Resolution);
    }

    private IList<string> GetCourseNames()
    {
      IList<string> courseNames;
      if (string.IsNullOrEmpty(lstCourse.Text) == false)
      {
        courseNames = new string[] { lstCourse.Text };
      }
      else
      {
        courseNames = Utils.GetCourseList(txtCourse.Text);
      }
      return courseNames;
    }

    private void BtnCreateRaw_Click(object sender, System.EventArgs e)
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        CreateRaw();
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnCreateClean_Click(object sender, System.EventArgs e)
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        IList<string> courseNames = GetCourseNames();
        PermutationUtils.AdaptCourseFiles(txtCourse.Text,
          txtRawDir.Text, txtClean.Text, courseNames, chkNr.Checked, chkCodes.Checked, txtCleanTemplates.Text);
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnCreateCat_Click(object sender, System.EventArgs e)
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        IList<string> courseNames = GetCourseNames();
        int runnersConst = -1;
        if (chkConstant.Checked)
        {
          if (int.TryParse(txtRunners.Text, out runnersConst) == false)
          {
            MessageBox.Show("Invalid # of runners " + txtRunners.Text);
          }

        }
        PermutationUtils.CreateCategories(txtCourse.Text,
          txtClean.Text, txtCategory.Text, courseNames, runnersConst);
      }
      finally
      { Cursor = Cursors.Default; }

    }

    private void LstCourse_TextChanged(object sender, System.EventArgs e)
    {
      SetRawParent();
    }

    private void BtnBahnexport_Click(object sender, System.EventArgs e)
    {
      if (dlgSave.ShowDialog(this) != DialogResult.OK)
      { return; }

      IList<string> courseNames = GetCourseNames();
      int runnersConst = -1;
      if (chkConstant.Checked)
      {
        if (int.TryParse(txtRunners.Text, out runnersConst) == false)
        {
          MessageBox.Show("Invalid # of runners " + txtRunners.Text);
        }
      }

      try
      {
        Cursor = Cursors.WaitCursor;
        PermutationUtils.ExportV8(txtCourse.Text, courseNames, dlgSave.FileName, runnersConst);
      }
      finally
      { Cursor = Cursors.Default; }


    }
  }
}