using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Basics.Geom;
using Grid;
using Grid.Lcp;
using System.IO;

namespace LeastCostPathUI
{
  public partial class CntOutGrid : UserControl
  {
    private bool _inSynchron = false;
    private string _mainPath = "";

    public CntOutGrid()
    {
      InitializeComponent();
    }

    public bool ShowCoordinates
    {
      get
      { return grpCoord.Visible; }
      set
      { grpCoord.Visible = value; }
    }

    public void SetPoint(IPoint pnt)
    {
      X = pnt.X;
      Y = pnt.Y;
    }

    public void SetName(string name)
    {
      txtCost.Text = name + "c.grd";
    }

    public void SetMainPath(string mainPath)
    {
      _mainPath = mainPath;
    }
    public double X
    {
      get
      { return Convert.ToDouble(txtX.Text); }
      set
      { txtX.Text = value.ToString(); }
    }
    public double Y
    {
      get
      { return Convert.ToDouble(txtY.Text); }
      set
      { txtY.Text = value.ToString(); }
    }

    public string CostGrid
    {
      get
      {
        if (chkCost.Checked == false) return null;
        return FullName(txtCost.Text);
      }
    }
    public string CostImage
    {
      get
      {
        if (chkCostImg.Checked == false) return null;
        return FullName(txtCostImg.Text);
      }
    }
    public string DirGrid
    {
      get
      {
        if (chkDir.Checked == false) return null;
        return FullName(txtDir.Text);
      }
    }
    public string DirImage
    {
      get
      {
        if (chkDirImg.Checked == false) return null;
        return FullName(txtDirImg.Text);
      }
    }

    public void CheckCost(bool check)
    {
      chkCost.Checked = check;
    }
    public void CheckCostImage(bool check)
    {
      chkCostImg.Checked = check;
    }
    public void CheckDir(bool check)
    {
      chkDir.Checked = check;
    }
    public void CheckDirImage(bool check)
    {
      chkDirImg.Checked = check;
    }

    public void Export(ILeastCostGridData lcg)
    {
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Grid.Common.InitColors(r, g, b);

      if (lcg.CostGrid != null)
      {
        if (CostGrid != null)
        { DoubleGrid.Save(lcg.CostGrid, CostGrid); }
        if (CostImage != null)
        { ImageGrid.GridToImage(lcg.CostGrid.ToInt().Mod(256), CostImage, r, g, b); }
      }
      if (lcg.DirGrid != null)
      {
        if (DirGrid != null)
        { IntGrid.Save(lcg.DirGrid, DirGrid); }
        if (DirImage != null)
        {
          // make sure that the start point cell returns a valid value for the step angle array
          IGrid<int> angleGrid = lcg.Steps[lcg.DirGrid.Add(-1).Abs().Mod(lcg.Steps.Count)].Div(Math.PI).Add(1.0).Mult(128).ToInt();
          ImageGrid.GridToImage(angleGrid, DirImage, r, g, b);
        }
      }
    }

    public void Export(IGrid<double> grid)
    {
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Grid.Common.InitColors(r, g, b);

      if (grid != null)
      {
        if (CostGrid != null)
        { DoubleGrid.Save(grid, CostGrid); }
        if (CostImage != null)
        { ImageGrid.GridToImage(grid.ToInt().Mod(256), CostImage, r, g, b); }
      }
    }


    public string FullName(string name)
    {
      if (name.IndexOf(Path.PathSeparator) < 0)
      {
        string full = Path.Combine(_mainPath, name);
        return full;
      }
      else
      {
        return name;
      }
    }

    public void Synchrone(TextBox orig)
    {
      if (chkSynchron.Checked == false)
      { return; }
      if (_inSynchron)
      { return; }

      try
      {
        _inSynchron = true;
        string sStamm = Path.GetFileNameWithoutExtension(orig.Text);
        if (sStamm.Length > 1)
        { sStamm = sStamm.Substring(0, sStamm.Length - 1); }
        txtCost.Text = sStamm + "c.grd";
        txtCostImg.Text = sStamm + "c.tif";
        txtDir.Text = sStamm + "d.grd";
        txtDirImg.Text = sStamm + "d.tif";
      }
      finally
      { _inSynchron = false; }
    }
    #region events
    private void BtnCost_Click(object sender, EventArgs e)
    {
      try
      {
        dlgSave.Filter = "cost grid files (*c.grd)|*c.grd";
        if (dlgSave.ShowDialog() == DialogResult.OK)
        {
          txtCost.Text = dlgSave.FileName;
          Synchrone(txtCost);
        }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void BtnCostImg_Click(object sender, EventArgs e)
    {
      try
      {
        dlgSave.Filter = "cost image files (*c.tif)|*c.tif";
        if (dlgSave.ShowDialog() == DialogResult.OK)
        {
          txtCostImg.Text = dlgSave.FileName;
          Synchrone(txtCostImg);
        }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void BtnDir_Click(object sender, EventArgs e)
    {
      try
      {
        dlgSave.Filter = "direction grid files (*d.grd)|*d.grd";
        if (dlgSave.ShowDialog() == DialogResult.OK)
        {
          txtDir.Text = dlgSave.FileName;
          Synchrone(txtDir);
        }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void BtnDirImg_Click(object sender, EventArgs e)
    {
      try
      {
        dlgSave.Filter = "direction image files (*d.tif)|*d.tif";
        if (dlgSave.ShowDialog() == DialogResult.OK)
        {
          txtDirImg.Text = dlgSave.FileName;
          Synchrone(txtDirImg);
        }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void TxtCost_TextChanged(object sender, EventArgs e)
    {
      try
      {
        Synchrone(txtCost);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void TxtCostImg_TextChanged(object sender, EventArgs e)
    {
      try
      {
        Synchrone(txtCostImg);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void TxtDir_TextChanged(object sender, EventArgs e)
    {
      try
      {
        Synchrone(txtDir);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void TxtDirImg_TextChanged(object sender, EventArgs e)
    {
      try
      {
        Synchrone(txtDirImg);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }
    #endregion

  }
}
