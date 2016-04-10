using System;
using System.Windows.Forms;
using Basics.Geom;
using Grid;
using Grid.Lcp;

namespace LeastCostPathUI
{
  public partial class WdgOutput : Form
  {
    private ICostProvider _costProvider;
    private DoubleGrid _height;
    private string _velo;
    private double _resol;
    private Steps _step;

    public WdgOutput()
    {
      InitializeComponent();
    }

    public void Init(ICostProvider costProvider, DoubleGrid height, string velo,
      double resolution, Steps step)
    {
      _costProvider = costProvider;
      _height = height;
      _velo = velo;
      _resol = resolution;
      _step = step;
    }

    public void SetExtent(IBox box)
    {
      cntOutput.SetExtent(box);
    }
    public void SetStart(IPoint start)
    {
      cntOutput.SetStart(start);
    }
    public void SetEnd(IPoint end)
    {
      cntOutput.SetEnd(end);
    }

    public void SetNames(string from, string to)
    {
      cntOutput.SetNames(from, to);
    }

    public void SetMainPath(string mainPath)
    {
      cntOutput.SetMainPath(mainPath);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      Hide();
      Close();
    }

    private bool _calc;
    private void btnOK_Click(object sender, EventArgs e)
    {
      if (_calc == false)
      {
        btnOK.Text = "Cancel";
        _calc = true;
        cntOutput.Calc(_costProvider, _height, _velo, _resol, _step, ResetCalc);
      }
      else
      {
        cntOutput.Cancel();
        _calc = false;
        btnOK.Text = "OK";
      }
    }
    private void ResetCalc()
    {
      _calc = false;
      btnOK.Text = "OK";
    }
  }
}