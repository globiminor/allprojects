using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace LeastCostPathUI
{
  public partial class WdgAnalyzeImg : Form
  {
    private AnalyzeImg _img;
    private List<Panel> _colors;

    public WdgAnalyzeImg()
    {
      InitializeComponent();
    }

    [STAThread]
    public static void Main(string[] arg)
    {
      Application.ThreadException -= Application_ThreadException;
      Application.ThreadException += Application_ThreadException;
      Application.Run(new WdgAnalyzeImg());
    }

    private static void Application_ThreadException(object sender,
      System.Threading.ThreadExceptionEventArgs e)
    {
      Exception exp = e.Exception;
      string msg = Basics.Utils.GetMsg(exp);
      MessageBox.Show(msg);
    }

    private void BtnOpen_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      if (dlg.ShowDialog(this) != DialogResult.OK)
      { return; }
      txtFile.Text = dlg.FileName;
      _img = null;
      if (_colors != null)
      {
        foreach (Panel color in _colors)
        {
          Controls.Remove(color);
        }
      }
      _colors = null;
    }

    private void BtnAnalyze_Click(object sender, EventArgs e)
    {
      if (_img == null)
      {
        _img = new AnalyzeImg(txtFile.Text);
      }
      IList<Color> colors;
      if (_colors == null || _colors.Count == 0)
      {
        colors = new Color[] { Color.White };
      }
      else
      {
        colors = new List<Color>(_colors.Count);
        foreach (Panel co in _colors)
        {
          colors.Add(co.ForeColor);
        }
      }

      Color f = _img.GetFarestColor(colors);

      AddColor(f);
    }

    private void AddColor(Color c)
    {
      if (_colors == null)
      { _colors = new List<Panel>(); }
      Panel p = new Panel();
      p.BorderStyle = BorderStyle.FixedSingle;
      p.ForeColor = c;
      p.BackColor = c;
      p.Width = 16;
      p.Height = 16;
      p.Top = btnAnalyze.Bottom + 8;
      p.Left = btnAnalyze.Left + _colors.Count * (p.Width + 4);

      Controls.Add(p);
      _colors.Add(p);
      Refresh();
    }
  }
}