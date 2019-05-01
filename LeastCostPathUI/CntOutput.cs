using Basics.Geom;
using Common.Gui;
using Grid;
using Grid.Lcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using BackgroundWorker = Common.Gui.BackgroundWorker;

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

    private LeastCostData _fromResult;

    private LeastCostData _toResult;

    private Point2D _from;
    private Point2D _to;

    public void Calc(IDirCostModel<TvmCell> provider,
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

      IDirCostModel<TvmCell> costProvider = provider;
      LeastCostGrid<TvmCell> costPath = new LeastCostGrid<TvmCell>(costProvider, dResol, box, step);

      if (!costPath.EqualSettings(_costPath) ||
        grVelo != _grdVelo || grHeight != _grdHeight)
      {
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
      private Basics.Views.BindingListView<RouteRecord> _routes;

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

        _parent.SetStepLabel(this, "FROM:");
        if (_parent._from == null || _from.Dist2(_parent._from) != 0 || _parent._fromResult == null)
        {
          LeastCostData result;
          if (_autoExtent)
          {
            LeastCostData allResult = _costPath.CalcCost(_from, new[] { _to }, stopFactor: _lengthFact);

            result = allResult.GetReduced();
          }
          else
          {
            result = _costPath.CalcCost(_from);
          }
          _parent._fromResult = result;
          _parent._from = _from;
        }

        if (_parent._cancelled)
        {
          return false;
        }

        _parent.SetStepLabel(this, "TO:");
        if (_parent._to == null || _to.Dist2(_parent._to) != 0 || _parent._toResult == null)
        {
          LeastCostData result;
          if (_autoExtent)
          {
            LeastCostData allResult = _costPath.CalcCost(_to, new[] { _from }, invers: true, stopFactor: _lengthFact);
            result = allResult.GetReduced();
          }
          else
          {
            result = _costPath.CalcCost(_to, invers: true);
          }

          _parent._toResult = result;
          _parent._to = _to;
        }

        //Steps.ReverseEngineer(_parent._toDir, _parent._toCost);

        _parent.SetStepLabel(this, "SUM:");
        _parent.CostPath_Status(this, new StatusEventArgs(null, null, 0, 0, 0, 0));
        _sum = DoubleGrid.Sum(_parent._fromResult.CostGrid, _parent._toResult.CostGrid);
        double min = _parent._fromResult.CostGrid.Value(_to.X, _to.Y);
        _route = _sum - min;

        if (_parent._cancelled)
        {
          return false;
        }

        double limit = 0.9 * min;
        double replace = 2 * min;
        DoubleGrid sum = DoubleGrid.Filter(_sum, (ix,iy) =>
        {
          double v = _sum[ix, iy];
          return v < limit ? replace : v;
        });

        if (_calcRoute)
        {
          _parent.SetStepLabel(this, "ROUTES:");
          _parent.CostPath_Status(this, new StatusEventArgs(null, null, 0, 0, 0, 0));

          //CreateRouteImage(sum, cntRoute.FullName(txtRoute.Text));
          List<RouteRecord> routes = LeastCostGrid.CalcBestRoutes(sum, _parent._fromResult,
            _parent._toResult, _lengthFact, _offset, _parent.CostPath_Status);

          _routes = new Basics.Views.BindingListView<RouteRecord>();
          foreach (RouteRecord route in routes)
          { _routes.Add(route); }
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

        _parent.cntFrom.Export(_parent._fromResult);
        _parent.cntTo.Export(_parent._toResult);
        _parent.cntRoute.Export(_route);

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

    private void ChkAuto_CheckedChanged(object sender, EventArgs e)
    {
      UpdateExtentEnabled();
    }
  }
}
