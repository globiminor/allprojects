using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Basics.Geom;
using OCourse.Ext;
using OCourse.Route;
using OCourse.Tracking;
using Control = Ocad.Control;

namespace OCourse.Gui
{
  public partial class WdgTrack : Form
  {
    private WdgOCourse _parent;
    private IList<Control> _course;
    public WdgTrack()
    {
      InitializeComponent();
    }

    public void SetData(string courseFile, WdgOCourse parent)
    {
      txtCourse.Text = courseFile;
      _parent = parent;

      throw new NotImplementedException();
      //WdgOCourseBck.SetCourseList(courseFile, lstCourse);
    }

    private void btnPath_Click(object sender, EventArgs e)
    {
      if (dlgOpen.ShowDialog(this) != DialogResult.OK)
      { return; }
      string path = dlgOpen.FileName;
      txtPath.Text = path;

      IList<TrackPoint> points;

      points = GpxIO.ReadData(path);
      if (points == null)
      { points = TcxReader.ReadData(path); }

      if (points == null)
      {
        MessageBox.Show(this,
          "Unknown format of " + path, "Unknown format");
        return;
      }

      // Project
      Basics.Geom.Projection.Utm prj = new Basics.Geom.Projection.Utm();
      List<IPoint> prjPoints = new List<IPoint>(points.Count);
      double? offset = null;
      foreach (TrackPoint trackPoint in points)
      {
        if (offset == null)
        {
          offset = Basics.Utils.Round(trackPoint.Long, 12);
        }


        Point2D p = new Point2D(trackPoint.Long - offset.Value, trackPoint.Lat);
        IPoint pp = prj.Gg2Prj(p);
        prjPoints.Add(pp);
      }
      // Transform
      foreach (Control section in _course)
      {
      }
    }

    private void btnResult_Click(object sender, EventArgs e)
    {
      dlgOpen.Filter = "*.csv|*.csv";
      if (dlgOpen.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      { return; }

      txtResult.Text = dlgOpen.FileName;
    }
    private void btnNormalized_Click(object sender, EventArgs e)
    {
      dlgSave.Filter = "*.csv|*.csv";
      if (dlgSave.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      { return; }

      txtNormalized.Text = dlgSave.FileName;
    }


    private void btnCreateNormalized_Click(object sender, EventArgs e)
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        RouteCalculator calc = null;
        if (chkGps.Checked)
        { calc = _parent.Vm.RouteCalculator; }
        if (btnRearrange.Checked)
        {
          PermutationUtils.NormalizeResult(txtCourse.Text, txtResult.Text, null,
                                           txtNormalized.Text, chkChoice.Checked, calc);
        }
        else
        {
          PermutationUtils.CompleteResult(txtCourse.Text, txtResult.Text, txtNormalized.Text);
        }
      }
      finally
      { Cursor = Cursors.Default; }

    }

    private void btnRg_Click(object sender, EventArgs e)
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        string r = txtResult.Text;
        string result = Path.Combine(Path.GetDirectoryName(r), Path.GetFileNameWithoutExtension(r)) + "_.csv";
        using (TextWriter writer = new StreamWriter(result))
        using (TextReader reader = new StreamReader(txtResult.Text))
        {
          for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
          {
            string[] parts = line.Split(';');

            StringBuilder rgLine = new StringBuilder();
            for (int i = 0; i < 3; i++)
            { rgLine.Append(";"); }
            rgLine.AppendFormat("{0};", parts[5]); // Name
            rgLine.AppendFormat("{0};", parts[6]); // Vorname
            for (int i = 5; i < 9; i++)
            { rgLine.Append(";"); }
            rgLine.AppendFormat("{0};", parts[11]); // StartTime
            rgLine.AppendFormat(";");
            rgLine.AppendFormat("{0};", parts[13]); // TotalTime
            rgLine.AppendFormat("{0};", parts[14]); // Wertung
            for (int i = 13; i < 15; i++)
            { rgLine.Append(";"); }
            rgLine.AppendFormat("{0};", parts[19] + " " + parts[20]); // Club
            for (int i = 16; i < 18; i++)
            { rgLine.Append(";"); }
            rgLine.AppendFormat("{0};", parts[25].ToLower()); // Category
            for (int i = 19; i < 39; i++)
            { rgLine.Append(";"); }
            rgLine.AppendFormat("{0};", parts[25].ToLower()); // Course
            rgLine.AppendFormat("{0};", parts[54]); // Distance
            rgLine.AppendFormat("{0};", parts[55]); // Climb
            rgLine.AppendFormat("{0};", parts[56]); // # controls
            rgLine.AppendFormat("{0};", parts[57]); // rang
            rgLine.AppendFormat("{0};", parts[58]); // start punch
            for (int i = 45; i < 46; i++)
            { rgLine.Append(";"); }
            for (int i = 60; i < parts.Length; i++)
            { rgLine.AppendFormat("{0};", parts[i]); }

            writer.WriteLine(rgLine.ToString());
          }
          reader.Close();
          writer.Close();
        }
      }
      finally
      { Cursor = Cursors.Default; }
    }

  }
}