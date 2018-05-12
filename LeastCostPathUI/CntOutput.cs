using System;
using System.ComponentModel;
using System.Windows.Forms;
using Common.Gui;
using Grid;
using Basics.Geom;
using BackgroundWorker = Common.Gui.BackgroundWorker;
using System.Threading;
using Grid.Lcp;

namespace LeastCostPathUI
{
  public partial class CntOutput : UserControl
  {
    private LeastCostGrid<TvmCell> _costPath;
    private bool _cancelled;
    private bool _disposed;

    public CntOutput()
    {
      InitializeComponent();
      cntRoute.CheckCostImage(true);

      UpdateExtentEnabled();
    }

    public void SetExtent(IBox box)
    {
      txtXMin.Text = box.Min.X.ToString("N1");
      txtYMin.Text = box.Min.Y.ToString("N1");
      txtXMax.Text = box.Max.X.ToString("N1");
      txtYMax.Text = box.Max.Y.ToString("N1");
    }

    public void SetStart(IPoint start)
    {
      cntFrom.SetPoint(start);
    }

    public void SetEnd(IPoint end)
    {
      cntTo.SetPoint(end);
    }

    public void SetMainPath(string mainPath)
    {
      cntFrom.SetMainPath(mainPath);
      cntTo.SetMainPath(mainPath);
      cntRoute.SetMainPath(mainPath);
    }

    public void SetNames(string from, string to)
    {
      cntFrom.SetName(from + "_");
      cntTo.SetName("_" + to);

      cntRoute.SetName(from + "_" + to);
      txtRouteShp.Text = from + "_" + to + "r.shp";
      txtRoute.Text = from + "_" + to + "r.tif";
    }

    private IDoubleGrid _grdHeight;
    private string _grdVelo;

    private IGrid<double> _fromCost;
    private IGrid<int> _fromDir;

    private IGrid<double> _toCost;
    private IGrid<int> _toDir;

    private Point2D _from;
    private Point2D _to;

    public void Calc(IDirCostProvider<TvmCell> provider,
      IDoubleGrid grHeight, string grVelo, double resolution, Steps step, ThreadStart resetDlg)
    {
      _cancelled = false;

      double dX0 = Convert.ToDouble(txtXMin.Text);
      double dY0 = Convert.ToDouble(txtYMin.Text);
      double dX1 = Convert.ToDouble(txtXMax.Text);
      double dY1 = Convert.ToDouble(txtYMax.Text);
      double dResol = resolution;
      if (dY0 > dY1)
      {
        double t;

        t = dX0;
        dX0 = dX1;
        dX1 = t;
        txtXMin.Text = dX0.ToString();
        txtXMax.Text = dX1.ToString();
      }
      if (dY0 > dY1)
      {
        double t;

        t = dX0;
        dX0 = dX1;
        dX1 = t;
        txtXMin.Text = dX0.ToString();
        txtXMax.Text = dX1.ToString();
      }

      Box box = chkAuto.Checked
        ? null
        : new Box(new Point2D(dX0, dY0), new Point2D(dX1, dY1));

      IDirCostProvider<TvmCell> costProvider = provider;
      LeastCostGrid<TvmCell> costPath = new LeastCostGrid<TvmCell>(costProvider, dResol, box, step);

      if (!costPath.EqualSettings(_costPath) ||
        grVelo != _grdVelo || grHeight != _grdHeight)
      {
        _fromCost = null;
        _fromDir = null;
        _toCost = null;
        _toDir = null;

        _costPath = costPath;

        _grdVelo = grVelo;
        _grdHeight = grHeight;
      }

      Point2D from = new Point2D(cntFrom.X, cntFrom.Y);
      Point2D to = new Point2D(cntTo.X, cntTo.Y);
      double lengthFact = 1.2;
      if (double.TryParse(txtSlower.Text, out double lengthPct))
      {
        lengthFact = 1 + lengthPct / 100.0;
      }
      double offset = 0.05;
      if (double.TryParse(txtMinOffset.Text, out double offsetPct))
      {
        offset = offsetPct / 100.0;
      }

      CalcCostWorker worker = new CalcCostWorker(this, costPath, grHeight, from, to, _costPath.Steps,
        chkAuto.Checked, chkRoute.Checked || chkRouteShp.Checked, lengthFact, offset, resetDlg);

      Enable(false);
      BackgroundWorker bgWorker = new BackgroundWorker(this, worker);
      bgWorker.DoWork();
    }

    private class CalcCostWorker : IWorker
    {
      private readonly CntOutput _parent;
      private readonly LeastCostGrid<TvmCell> _costPath;
      private readonly IDoubleGrid _grHeight;
      private readonly Point2D _from;
      private readonly Point2D _to;
      private readonly Steps _step;
      private readonly bool _calcRoute;
      private readonly double _lengthFact;
      private readonly double _offset;
      private readonly bool _autoExtent;
      private readonly ThreadStart _resetDlg;

      private DoubleGrid _sum;
      private DoubleGrid _route;
      private RouteTable _routes;

      public CalcCostWorker(CntOutput parent, LeastCostGrid<TvmCell> costPath,
        IDoubleGrid grHeight, Point2D from, Point2D to, Steps step, bool autoExtent,
        bool calcRoute, double lengthFact, double offset, ThreadStart resetDlg)
      {
        _parent = parent;
        _costPath = costPath;
        _grHeight = grHeight;
        _from = from;
        _to = to;
        _step = step;
        _autoExtent = autoExtent;
        _calcRoute = calcRoute;
        _lengthFact = lengthFact;
        _offset = offset;
        _resetDlg = resetDlg;
      }

      public bool Start()
      {
        _costPath.Status -= _parent.CostPath_Status;
        _costPath.Status += _parent.CostPath_Status;

        if (_parent._cancelled)
        {
          return false;
        }

        Steps fromStep = _step;
        Steps toStep = _step;

        _parent.SetStepLabel(this, "FROM:");
        if (_parent._from == null || _from.Dist2(_parent._from) != 0 || _parent._fromCost == null)
        {
          IGrid<double> cost;
          IGrid<int> dir;
          if (_autoExtent)
          {
            _costPath.CalcCost(_from, new[] { _to }, out IGrid<double> allCost, out IGrid<int> allDir, 
              invers: false, stopFactor: _lengthFact);
            cost = BaseGrid.TryReduce(allCost);
            dir = BaseGrid.TryReduce(allDir);
            LeastCostGrid.SetUndefinedCells(cost, dir, double.NaN);
          }
          else
          {
            _costPath.CalcCost(_from, out cost, out dir, false);
          }
          _parent._fromCost = cost;
          _parent._fromDir = dir;
          _parent._from = _from;
        }

        if (_parent._cancelled)
        {
          return false;
        }

        _parent.SetStepLabel(this, "TO:");
        if (_parent._to == null || _to.Dist2(_parent._to) != 0 || _parent._toCost == null)
        {
          IGrid<double> cost;
          IGrid<int> dir;
          if (_autoExtent)
          {
            _costPath.CalcCost(_to, new[] { _from }, out IGrid<double> allCost, out IGrid<int> allDir,
              invers: true, stopFactor: _lengthFact);
            cost = BaseGrid.TryReduce(allCost);
            dir = BaseGrid.TryReduce(allDir);
            LeastCostGrid.SetUndefinedCells(cost, dir, double.NaN);
          }
          else
          {
            _costPath.CalcCost(_to, out cost, out dir, true);
          }

          _parent._toCost = cost;
          _parent._toDir = dir;
          _parent._to = _to;
        }

        //Steps.ReverseEngineer(_parent._toDir, _parent._toCost);

        _parent.SetStepLabel(this, "SUM:");
        _parent.CostPath_Status(this, new StatusEventArgs(null, null, 0, 0, 0, 0));
        _sum = DoubleGrid.Sum(_parent._fromCost, _parent._toCost);
        _route = _sum - _sum.Min();

        if (_parent._cancelled)
        {
          return false;
        }

        if (_calcRoute)
        {
          _parent.SetStepLabel(this, "ROUTES:");
          _parent.CostPath_Status(this, new StatusEventArgs(null, null, 0, 0, 0, 0));

          //CreateRouteImage(sum, cntRoute.FullName(txtRoute.Text));
          _routes = LeastCostGrid.CalcBestRoutes(_sum, _parent._fromCost, _parent._fromDir, fromStep,
            _parent._toCost, _parent._toDir, toStep, _lengthFact, _offset, _parent.CostPath_Status);
        }
        _parent.SetStepLabel(this, "EXPORT:");

        return true;
      }

      public void Finish()
      {
        if (_parent.chkRoute.Checked)
        {
          IO.CreateRouteImage(_routes, _sum.Extent, _parent.cntRoute.FullName(_parent.txtRoute.Text));
        }
        if (_parent.chkRouteShp.Checked)
        {
          IO.CreateRouteShapes(_routes, _sum, _parent._grdHeight, _parent.cntRoute.FullName(_parent.txtRouteShp.Text));
        }

        if (_parent._cancelled) { return; }

        _parent.lblStep.Text = "EXPORT:";
        _parent.CostPath_Status(this, new StatusEventArgs(null, null, 0, 0, 0, 0));

        _parent.cntFrom.Export(_parent._fromCost, _parent._fromDir, _parent._costPath.Steps);
        _parent.cntTo.Export(_parent._toCost, _parent._toDir, _parent._costPath.Steps);
        _parent.cntRoute.Export(_route, null, null);

        _parent.lblStep.Text = "< >";
        _parent.CostPath_Status(this, new StatusEventArgs(null, null, 0, 0, 0, 0));
      }

      public void Finally()
      {
        _costPath.Status -= _parent.CostPath_Status;
        _parent.Enable(true);
        _resetDlg();
      }
    }

    private void SetStepLabel(object sender, string title)
    {
      if (InvokeRequired)
      {
        EventDlg<string> dlg = SetStepLabel;
        Invoke(dlg, sender, title);
        return;
      }
      lblStep.Text = title;
    }
    public void Cancel()
    {
      _cancelled = true;
      lblProgress.Text = "CANCELLED";
    }

    private void Enable(bool enable)
    {
      if (_disposed == false)
      {
        grpExtent.Enabled = enable;
        grpFrom.Enabled = enable;
        grpTo.Enabled = enable;
        grpRoute.Enabled = enable;
      }
    }

    private void UpdateExtentEnabled()
    {
      pnlExtent.Enabled = !chkAuto.Checked;
    }

    void CostPath_Status(object sender, StatusEventArgs args)
    {
      if (InvokeRequired)
      {
        EventDlg<StatusEventArgs> dlg = CostPath_Status;
        Invoke(dlg, sender, args);
        return;
      }
      if (_cancelled)
      {
        args.Cancel = true;
        return;
      }
      if (args.CurrentStep % 1000 == 0)
      {
        if (TopLevelControl.Visible == false)
        {
          _cancelled = true;
        }
        else
        {
          if (args.TotalStep > 0)
          {
            string max = chkAuto.Checked ? "(max)" : string.Empty;
            lblProgress.Text = $"Step {args.CurrentStep:N0} of {max}{args.TotalStep:N0}";
          }
          else
          {
            lblProgress.Text = string.Format("...");
          }
        }
      }
      if (_cancelled)
      {
        args.Cancel = true;
        return;
      }
    }

    private void TxtDouble_Validating(object sender, CancelEventArgs e)
    {
      try
      {
        TextBox txt = (TextBox)sender;
        if (double.TryParse(txt.Text, out double d) == false)
        { e.Cancel = true; }
      }
      catch
      { e.Cancel = true; }
    }

    private void BtnRoute_Click(object sender, EventArgs e)
    {
      cntRoute.dlgSave.Filter = "route image files (*r.tif)|*r.tif";
      if (cntRoute.dlgSave.ShowDialog() == DialogResult.OK)
      {
        txtRoute.Text = cntRoute.dlgSave.FileName;
        cntRoute.Synchrone(txtRoute);
      }
    }

    private void BtnRouteShp_Click(object sender, EventArgs e)
    {
      cntRoute.dlgSave.Filter = "route shape files (*r.shp)|*r.shp";
      if (cntRoute.dlgSave.ShowDialog() == DialogResult.OK)
      {
        txtRouteShp.Text = cntRoute.dlgSave.FileName;
        cntRoute.Synchrone(txtRouteShp);
      }
    }

    private void chkAuto_CheckedChanged(object sender, EventArgs e)
    {
      UpdateExtentEnabled();
    }
  }
}
