using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Basics.Data;
using Basics.Geom;

namespace Dhm
{
  public partial class WdgMain : Form
  {
    private class StopException : Exception
    { }

    private TMap.IContext _context;
    private readonly Dictionary<int, ContourType> _ocadSymbols;
    private readonly List<int> _ocadFallDirSymbols;
    private Info _infoRow;

    private string _origFile;

    private bool _pause = false;
    private bool _break = true;

    private ContourSorter _calc;

    public WdgMain()
    {
      InitializeComponent();

      _ocadSymbols = new Dictionary<int, ContourType>();
      _ocadSymbols.Add(101000, new ContourType(2, 0));
      _ocadSymbols.Add(102000, new ContourType(10, 0));
      _ocadSymbols.Add(103000, new ContourType(2, 1));

      _ocadSymbols.Add(1010, new ContourType(2, 0));
      _ocadSymbols.Add(1020, new ContourType(10, 0));
      _ocadSymbols.Add(1030, new ContourType(2, 1));

      _ocadFallDirSymbols = new List<int>();
      _ocadFallDirSymbols.Add(104000);
      _ocadFallDirSymbols.Add(1040);
    }

    public TMap.IContext TMapContext
    {
      get { return _context; }
      set
      {
        _context = value;
        UpdateAppearance();
      }
    }

    private void ProgressOcad(ContourSorter.ProgressEventArgs args)
    {
      if (args.Progress > 0)
      {
        return;
      }
      Process ocdProc = StartOcad();


      Macro.Macro m = new Macro.Macro();
      OcadInitFile(m, ocdProc, txtOcad.Text);

      if (args.Progress == ContourSorter.Progress.Intersection)
      {
        OcadSetZoom32(m, ocdProc);
        OcadCenter(m, args.Contour.Polyline.Extent);
        MessageBox.Show(this, "Intersection");
      }
      else if (args.Progress == ContourSorter.Progress.ErrorFallDir)
      {
        OcadSetZoom32(m, ocdProc);
        OcadCenter(m, args.Contour.Polyline.Extent);
        MessageBox.Show("FallDir");
      }
      else
      {
        OcadSetZoom32(m, ocdProc);
        OcadCenter(m, args.Contour.Polyline.Extent);
        DialogResult res = MessageBox.Show(@"
Unknown error
Continue?",
          "Error", MessageBoxButtons.YesNo);
        if (res == DialogResult.No)
        { EndThread(); }


        //throw new NotImplementedException("Unhandled progress " + args.Progress);
      }
    }

    private void OcadCenter(Macro.Macro m, IBox extent)
    {
      List<byte> code = new List<byte>();
      IBox b = extent;
      Point2D p = new Point2D((b.Min.X + b.Max.X) / 2.0, (b.Min.Y + b.Max.Y) / 2.0);
      code.Clear();
      code.Add(Macro.Macro.VK_CONTROL);
      m.SendCommand('g', code);

      Clipboard.SetText(p.X.ToString("f0"));
      code.Clear();
      code.Add(Macro.Macro.VK_CONTROL);
      m.SendCommand('v', code);

      code.Clear();
      m.SendKey('\t');
      m.WaitForInputIdle();
      m.WaitIdle();

      Clipboard.SetText(p.Y.ToString("f0"));
      code.Clear();
      code.Add(Macro.Macro.VK_CONTROL);
      m.SendCommand('v', code);

      code.Clear();

      code.Add(Macro.Macro.VK_RETURN);
      m.SendCode(code);

      m.WaitForInputIdle();
      m.WaitIdle();
    }

    private void OcadSetZoom32(Macro.Macro m, Process ocdProc)
    {
      List<byte> code = new List<byte>();

      m.SetForegroundProcess(ocdProc);
      m.WaitForInputIdle();
      m.WaitIdle();
      code.Clear();
      code.Add(Macro.Macro.VK_ALT);
      m.SendCommand('n', code); // Ansicht
      code.Clear();
      m.WaitForInputIdle();
      m.WaitIdle();
      m.SendCommand('m', code); // Zoom
      m.WaitForInputIdle();
      m.WaitIdle();
      m.SendCommand('3', code); // 32
      m.WaitForInputIdle();
      m.WaitIdle();
      m.ForegroundClick(150, 150);

      m.WaitForInputIdle();
      m.WaitIdle();
    }

    private static void OcadInitFile(Macro.Macro m, Process ocdProc, string fileName)
    {
      List<byte> code = new List<byte>();
      m.SetForegroundProcess(ocdProc);
      m.WaitIdle();
      m.WaitForInputIdle();
      string windowName = m.ForeGroundWindowName();
      if (!windowName.Contains(fileName))
      {
        {
          m.SetForegroundProcess(ocdProc);
          m.WaitIdle();
          m.WaitForInputIdle();
          m.WaitIdle();
          code.Add(Macro.Macro.VK_CONTROL);
          m.SendCommand('o', code);
          code.Clear();
        }
        {
          //m.SetForegroundProcess(ocdProc);
          m.WaitForInputIdle();
          m.WaitIdle();
          code.Add(Macro.Macro.VK_ALT);
          m.SendCommand('n', code);
          code.Clear();
        }
        {
          // m.SetForegroundProcess(ocdProc);
          m.WaitForInputIdle();
          m.WaitIdle();
          Clipboard.Clear();
          Clipboard.SetText(fileName);
          code.Add(Macro.Macro.VK_CONTROL);
          m.SendCommand('v', code);
          code.Clear();
        }
        {
          // m.SetForegroundProcess(ocdProc);
          m.WaitForInputIdle();
          m.WaitIdle();
          code.Add(Macro.Macro.VK_ALT);
          m.SendCommand('f', code);
          code.Clear();
          m.WaitForInputIdle();
          m.WaitIdle();
        }
      }
    }

    private Process StartOcad()
    {
      Microsoft.Win32.RegistryKey ocdKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(".ocd");
      string ocdProg = ocdKey.GetValue(null).ToString();
      ocdKey.Close();
      Microsoft.Win32.RegistryKey ocdProgKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ocdProg);
      Microsoft.Win32.RegistryKey commandKey = ocdProgKey.OpenSubKey(@"shell\open\command");
      string programPath = commandKey.GetValue(null).ToString().Split('"')[1];
      string programName = System.IO.Path.GetFileName(programPath);

      IList<Process> procs = Process.GetProcesses();
      Process ocdProc = null;
      foreach (Process proc in procs)
      {
        if (proc.MainWindowHandle.ToInt32() == 0)
        { continue; }

        try
        {
          if (proc.MainModule.FileName == programPath)
          {
            ocdProc = proc;
            break;
          }
        }
        catch (Win32Exception) { }
      }
      if (ocdProc == null)
      {
        ocdProc = Process.Start(programPath, txtOcad.Text);
        ocdProc.WaitForExit(2000);
      }
      return ocdProc;
    }

    private delegate void EmptyDlg();
    private delegate void ProgressDlg(ContourSorter.ProgressEventArgs args);
    private ProgressDlg _progressDlg;
    private void DrawMesh()
    {
      DrawMesh(chkLines.Checked, chkTris.Checked);
      btnPause_Click(this, null);
    }
    private void Adapt(ContourSorter.ProgressEventArgs args)
    {
      if (chkOCAD.Checked)
      {
        ProgressOcad(args);
        _waitHandle.Set();
      }

      if (_context != null && chkShow.Checked)
      {
        WdgAdapt wdg = new WdgAdapt(txtOcad.Text, args, _context);
        wdg.Disposed += wdg_Closed;
        Enabled = false;

        wdg.Show(this);
      }
    }

    void wdg_Closed(object sender, EventArgs e)
    {
      Enabled = true;
      WdgAdapt wdg = sender as WdgAdapt;
      if (wdg != null && wdg.DialogResult != DialogResult.OK)
      {
        EndThread();
        UpdateAppearance();
      }
      _waitHandle.Set();
    }
    internal Delegate GetAction(ContourSorter.ProgressEventArgs args)
    {
      _progressDlg = null;
      ProgressDlg dlg;
      if (args == null)
      {
        EmptyDlg invoke = DrawMesh;
        Invoke(invoke);
        dlg = null;
      }
      else if (chkIgnoreProgress.Checked)
      {
        dlg = null;
      }
      else if (args.Progress == ContourSorter.Progress.Intersection)
      {
        _progressDlg = Adapt;
        dlg = _progressDlg;
      }
      else if (_context != null)
      {
        ProgressDlg invoke = DrawLine;
        Invoke(invoke, args);
        dlg = null;
      }
      else
      { dlg = null; }

      if (_pause)
      {
        dlg = Pause;
      }
      return dlg;
    }

    private ContourSorter.ProgressEventArgs _pauseArgs;
    private void Pause(ContourSorter.ProgressEventArgs args)
    {
      _pauseArgs = args;
    }

    private void DrawLine(ContourSorter.ProgressEventArgs args)
    {
      TMap.SymbolPart lineSymbol = new TMap.SymbolPartLine(null);
      if (args.Progress == ContourSorter.Progress.FallDirAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(255, 0, 0);
      }
      else if (args.Progress == ContourSorter.Progress.HillAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(255, 96, 96);
      }
      else if (args.Progress == ContourSorter.Progress.ParallelAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(0, 0, 255);
      }
      else if (args.Progress == ContourSorter.Progress.HeightAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(0, 255, 0);
      }
      else if (args.Progress == ContourSorter.Progress.Error)
      {
        lineSymbol.LineColor = Color.FromArgb(255, 0, 255);
      }
      else
      {
        DialogResult res = MessageBox.Show(string.Format(@"
Unhandled progress {0}
Continue?", args.Progress), "Error", MessageBoxButtons.YesNo);

        if (res == DialogResult.No)
        {
          EndThread();
          return;
        }
      }
      TMap.IDrawable draw = _context.Maps[0];
      draw.DrawLine(args.Contour.Polyline.Project(draw.Projection),
                    lineSymbol);
      draw.Flush();
    }

    private void ProgressMap(ContourSorter.ProgressEventArgs args)
    {
      if (args == null)
      {
        DrawMesh(chkLines.Checked, chkTris.Checked);
        btnPause_Click(this, null);
        return;
      }

      if (chkIgnoreProgress.Checked)
      { return; }

      if (args.Progress == ContourSorter.Progress.Intersection)
      {
        WdgAdapt wdg = new WdgAdapt(txtOcad.Text, args, _context);
        if (wdg.ShowDialog(this) == DialogResult.OK)
        { return; }
      }


      TMap.SymbolPart lineSymbol = new TMap.SymbolPartLine(null);
      if (args.Progress == ContourSorter.Progress.FallDirAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(255, 0, 0);
      }
      else if (args.Progress == ContourSorter.Progress.HillAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(255, 96, 96);
      }
      else if (args.Progress == ContourSorter.Progress.ParallelAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(0, 0, 255);
      }
      else if (args.Progress == ContourSorter.Progress.HeightAssigned)
      {
        lineSymbol.LineColor = Color.FromArgb(0, 255, 0);
      }
      else if (args.Progress == ContourSorter.Progress.Error)
      {
        lineSymbol.LineColor = Color.FromArgb(255, 0, 255);
      }
      else
      {
        DialogResult res = MessageBox.Show(string.Format(@"
Unhandled progress {0}
Continue?", args.Progress), "Error", MessageBoxButtons.YesNo);

        if (res == DialogResult.No)
        {
          EndThread();
          return;
        }
      }
      TMap.IDrawable draw = _context.Maps[0];
      draw.DrawLine(args.Contour.Polyline.Project(draw.Projection),
                    lineSymbol);
      draw.Flush();
    }

    private void DrawMesh(bool drawLines, bool drawTris)
    {
      if (_calc == null || _calc.Mesh == null)
      { return; }


      TMap.IDrawable draw = _context.Maps[0];
      draw.BeginDraw();
      if (drawTris)
      {
        TMap.SymbolPart areaSymbol = new TMap.SymbolPartArea(null);
        areaSymbol.LineColor = Color.FromArgb(192, 192, 192);

        foreach (Mesh.Tri tri in _calc.Mesh.Tris(draw.Extent))
        {
          Area poly = new Area(tri.Border);
          draw.DrawArea(poly.Project(draw.Projection),
                        areaSymbol);

          if (_break)
          {
            throw new StopException();
          }
        }
        draw.Flush();
      }

      if (drawLines)
      {
        TMap.SymbolPart lineSymbol = new TMap.SymbolPartLine(null);
        lineSymbol.LineColor = Color.FromArgb(128, 128, 128);
        TMap.SymbolPart tagSymbol = new TMap.SymbolPartLine(null);
        tagSymbol.LineColor = Color.FromArgb(255, 128, 128);

        foreach (Mesh.MeshLine line in _calc.Mesh.Lines(draw.Extent))
        {
          Polyline poly = Polyline.Create(new[] { line.Start, line.End });

          TMap.SymbolPart drawSymbol;
          bool isReverse;
          if (line.GetTag(out isReverse) != null)
          { drawSymbol = tagSymbol; }
          else
          { drawSymbol = lineSymbol; }
          draw.DrawLine(poly.Project(draw.Projection),
                        drawSymbol);

          if (_break)
          {
            throw new StopException();
          }
        }
        draw.Flush();
      }
      draw.EndDraw();
    }

    private void UpdateAppearance()
    {
      grpMap.Visible = (_context != null);

      bool running = IsRunning();
      btnPause.Enabled = running;

      if (btnPause.Enabled && _pause)
      { btnPause.Text = "Continue"; }
      else
      { btnPause.Text = "Pause"; }

      btnDlgOpen.Enabled = !running;
      btnOK.Enabled = !running;
      txtTolerance.Enabled = !running;
      txtOcad.Enabled = !running;

      if (running)
      { Cursor = Cursors.WaitCursor; }
      else
      { Cursor = Cursors.Default; }
    }

    private bool IsRunning()
    {
      bool running;
      if (_calcThread == null)
      {
        running = false;
      }
      else
      {
        Thread.Sleep(100);
        if (_calcThread.ThreadState == System.Threading.ThreadState.Stopped)
        {
          _calcThread = null;
          running = false;
        }
        else
        {
          running = true;
        }
      }
      return running;
    }

    private void btnDlgOpen_Click(object sender, EventArgs e)
    {
      dlgOpen.Filter = "*.ocd | *.ocd";
      dlgOpen.Title = "Open OCAD File";

      if (dlgOpen.ShowDialog() == DialogResult.OK)
      {
        txtOcad.Text = dlgOpen.FileName;
      }
    }

    private AutoResetEvent _waitHandle;
    private void btnOK_Click(object sender, EventArgs e)
    {
      string ocdFile = txtOcad.Text;
      if (string.IsNullOrEmpty(ocdFile))
      { return; }
      double tolerance = double.Parse(txtTolerance.Text);

      _pause = false;
      _break = false;

      Cursor = Cursors.WaitCursor;
      EmptyDlg endDlg = UpdateAppearance;
      _waitHandle = new AutoResetEvent(false);
      CreateDhm worker = new CreateDhm(ocdFile, tolerance,
        _ocadSymbols, _ocadFallDirSymbols, this, endDlg, _waitHandle);

      _calcThread = new Thread(worker.Create);
      _calcThread.Start();

      UpdateAppearance();
    }

    private Thread _calcThread;
    private void EndThread()
    {
      if (_calcThread == null)
      { return; }
      if (_calcThread.ThreadState != System.Threading.ThreadState.Stopped)
      { _calcThread.Abort(); }
      _calcThread = null;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      EndThread();
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }


    private void btnCancel_Click(object sender, EventArgs e)
    {
      bool running = IsRunning();
      if (!running == false)
      {
        Close();
      }
      else
      {
        _break = true;
        _pause = false;
        EndThread();
        UpdateAppearance();
      }
    }

    private void LoadSettings(string name)
    {
      _ocadSymbols.Clear();
      _ocadFallDirSymbols.Clear();

      Config cnf = new Config();
      System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(Config));
      using (System.IO.TextReader r = new System.IO.StreamReader(name))
      {
        cnf = (Config)reader.Deserialize(r);
      }

      foreach (ContourSetting row in cnf.Contour)
      {
        _ocadSymbols.Add(row.Symbol, new ContourType(row.Intervall, row.Offset));
      }

      foreach (FallDirSetting row in cnf.FallDir)
      {
        _ocadFallDirSymbols.Add(row.Symbol);
      }

      _infoRow = cnf.Info;
    }

    private TMap.GroupMapData _mapGrp;
    private TMap.TableMapData _mapMesh;
    private TMap.TableMapData _mapContours;

    private void btnPause_Click(object sender, EventArgs e)
    {
      _pause = !_pause;
      if (_pause && sender != null && _context != null && _calc != null)
      {
        UpdateContext();
      }

      UpdateAppearance();
      if (!_pause && _progressDlg != null && _pauseArgs != null)
      {
        ProgressDlg dlg = _progressDlg;
        ContourSorter.ProgressEventArgs args = _pauseArgs;
        _progressDlg = null;
        _pauseArgs = null;
        dlg(args);
      }
      else if (_waitHandle != null)
      {
        _waitHandle.Set();
      }
    }

    private void UpdateContext()
    {
      TMap.GroupMapData mapGrp = null;
      TMap.TableMapData mapMesh = null;
      TMap.TableMapData mapContours = null;

      foreach (TMap.MapData mapData in _context.Data.GetAllData())
      {
        if (mapData == _mapGrp)
        { mapGrp = _mapGrp; }
        if (mapData == _mapMesh)
        { mapMesh = _mapMesh; }
        if (mapData == _mapContours)
        { mapContours = _mapContours; }
      }
      if (mapGrp == null && (mapMesh == null || mapContours == null))
      {
        _mapGrp = new TMap.GroupMapData("Contours");
        _context.Data.Subparts.Add(_mapGrp);
      }

      if (mapMesh != null)
      {
        SimpleConnection<Mesh.MeshLine> conn = (SimpleConnection<Mesh.MeshLine>)_mapMesh.Data.Connection;
        MeshSimpleData data = (MeshSimpleData)conn.Data;
        data.Mesh = _calc.Mesh;
      }
      else
      {
        SimpleConnection<Mesh.MeshLine> conn = new SimpleConnection<Mesh.MeshLine>(new MeshSimpleData(_calc.Mesh));
        _mapMesh = new TMap.TableMapData("Mesh", new TData.TTable("Mesh", conn));
        _mapGrp.Subparts.Add(_mapMesh);
      }

      if (mapContours != null)
      {
        SimpleConnection<Contour> conn = (SimpleConnection<Contour>)_mapContours.Data.Connection;
        ContourSimpleData data = (ContourSimpleData)conn.Data;
        data.Contours = _calc.Contours;
      }
      else
      {
        // TData.TTable tbl = new TData.TTable("Contours", new ContourConnection(_calc.Contours));
        TData.TTable tbl = new TData.TTable("Contours", new SimpleConnection<Contour>(new ContourSimpleData(_calc.Contours)));
        DataRow templateRow = tbl.FullSchema.NewRow();
        _mapContours = new TMap.TableMapData("Contours", tbl);
        _mapContours.Symbolisation.SymbolList.Table.Clear();
        {
          TMap.SymbolPartLine main = new TMap.SymbolPartLine(templateRow);
          main.LineColor = Color.Red;
          TMap.SymbolPartLinePoint dir = new TMap.SymbolPartLinePoint(TMap.SymbolPartLinePoint.LinePointType.DashPoint, templateRow);
          dir.LineColor = Color.Red;
          dir.Point = new TMap.SymbolPartPoint(templateRow);
          dir.Point.DirectPoints = true;
          dir.Point.LineColor = Color.Red;
          dir.Point.Scale = true;
          dir.Point.SymbolLine = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(0, -2) });

          TMap.Symbol sym = new TMap.Symbol(main);
          sym.Add(dir);
          _mapContours.Symbolisation.Add(sym, "Orientation = 1");
        }
        {
          TMap.SymbolPartLine main = new TMap.SymbolPartLine(templateRow);
          main.LineColor = Color.Blue;
          TMap.SymbolPartLinePoint dir = new TMap.SymbolPartLinePoint(TMap.SymbolPartLinePoint.LinePointType.DashPoint, templateRow);
          dir.LineColor = Color.Blue;
          dir.Point = new TMap.SymbolPartPoint(templateRow);
          dir.Point.DirectPoints = true;
          dir.Point.LineColor = Color.Blue;
          dir.Point.Scale = true;
          dir.Point.SymbolLine = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(0, 2) });

          TMap.Symbol sym = new TMap.Symbol(main);
          sym.Add(dir);
          _mapContours.Symbolisation.Add(sym, "Orientation = 2");
        }
        {
          TMap.SymbolPartLine main = new TMap.SymbolPartLine(templateRow);
          main.LineColor = Color.Violet;
          _mapContours.Symbolisation.Add(new TMap.Symbol(main), "true");
        }
        _mapGrp.Subparts.Add(_mapContours);
      }

      if (mapContours == null || mapMesh == null)
      {
        _context.Refresh();
      }
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
      dlgOpen.Filter = "*.xml | *.xml";
      dlgOpen.Title = "Open Settings";

      if (dlgOpen.ShowDialog() == DialogResult.OK)
      {
        LoadSettings(dlgOpen.FileName);
      }

    }

    private void btnDrawMesh_Click(object sender, EventArgs e)
    {
      DrawMesh(chkLines.Checked, chkTris.Checked);
    }

    private void WdgMain_Load(object sender, EventArgs e)
    {
      UpdateAppearance();
      TopMost = chkToplevel.Checked;
    }
    private void WdgMain_Closing(object sender, CancelEventArgs e)
    {
      EndThread();
    }

    private void WdgMain_Closed(object sender, EventArgs e)
    {
      EndThread();
    }

    private void chkToplevel_CheckedChanged(object sender, EventArgs e)
    {
      TopMost = chkToplevel.Checked;
    }


  }
  internal class CreateDhm
  {
    private readonly string _ocadFile;
    private readonly double _tolerance;
    private readonly Dictionary<int, ContourType> _ocadSymbols;
    private readonly List<int> _ocadFallDirSymbols;
    private readonly WdgMain _wdg;
    private readonly Delegate _endDlg;

    private readonly AutoResetEvent _waitHandle;

    private ContourSorter _calc;

    public CreateDhm(string ocadFile, double tolerance,
      Dictionary<int, ContourType> ocadSymbols,
      List<int> ocadFallDirSymbols,
      WdgMain wdg, Delegate endDlg,
      AutoResetEvent waitHandle)
    {
      _ocadFile = ocadFile;
      _tolerance = tolerance;
      _ocadSymbols = ocadSymbols;
      _ocadFallDirSymbols = ocadFallDirSymbols;
      _wdg = wdg;
      _endDlg = endDlg;
      _waitHandle = waitHandle;
    }

    public void Create()
    {
      try
      { CreateCore(); }
      finally
      { _wdg.Invoke(_endDlg); }
    }
    public void CreateCore()
    {
      string inFile = _ocadFile;
      double tolerance = _tolerance;

      string exportFile = System.IO.Path.GetFileNameWithoutExtension(inFile) + "_e";
      exportFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(inFile), exportFile);

      List<Contour> contours;
      List<FallDir> fallDirs;
      using (Ocad.OcadReader data = Ocad.OcadReader.Open(inFile))
      {
        GetRaw(data, tolerance, out contours, out fallDirs);
        data.Close();
      }

      _calc = new ContourSorter();
      _calc.ProgressChanged += calc_ProgressChanged;
      try
      {
        ContourSorter.Progress progress = _calc.Execute(contours, fallDirs);

        if (progress < ContourSorter.Progress.LoopUpChecked)
        { ExportOcd(exportFile + ".ocd", new List<Contour>(_calc.Contours)); }
        else if (progress == ContourSorter.Progress.LoopUpChecked)
        { ExportOcd(exportFile + ".ocd", _calc.LeftSideAssignedContours(_calc.Contours)); }
        else if (progress == ContourSorter.Progress.LoopDownChecked)
        { ExportOcd(exportFile + ".ocd", _calc.LoopContours(_calc.Contours)); }
        else if (progress == ContourSorter.Progress.HeightAssigned)
        {
          ExportOcd(exportFile + ".ocd", _calc.HeightAssignedContours(_calc.Contours));
          ExportShp(exportFile + ".shp", _calc.HeightAssignedContours(_calc.Contours), null);
        }
      }
      finally
      {
        _calc.ProgressChanged -= calc_ProgressChanged;
        //_calc = null;
      }
    }
    private void GetRaw(Ocad.OcadReader reader, double tolerance,
      out List<Contour> contours, out List<FallDir> fallDirs)
    {
      List<Ocad.ElementIndex> indexList = GetIndices(reader);
      GetElements(reader, indexList, tolerance, out contours, out fallDirs);
    }

    private List<Ocad.ElementIndex> GetIndices(Ocad.OcadReader reader)
    {
      List<Ocad.ElementIndex> idxList = new List<Ocad.ElementIndex>();
      int i = 0;

      for (Ocad.ElementIndex idx = reader.ReadIndex(i); idx != null; idx = reader.ReadIndex(i))
      {
        if (_ocadSymbols.ContainsKey(idx.Symbol))
        {
          idxList.Add(idx);
        }
        else if (_ocadFallDirSymbols.Contains(idx.Symbol))
        {
          idxList.Add(idx);
        }

        i++;
      }
      return idxList;
    }

    private void GetElements(Ocad.OcadReader reader, List<Ocad.ElementIndex> idxList,
      double tolerance, out List<Contour> contours, out List<FallDir> fallDirs)
    {
      contours = new List<Contour>();
      fallDirs = new List<FallDir>();
      foreach (Ocad.Element elem in reader.Elements(true, idxList))
      {
        ContourType type;
        if (_ocadSymbols.TryGetValue(elem.Symbol, out type))
        {
          Polyline line = (Polyline)elem.Geometry;
          contours.Add(new Contour(elem.Index, line.Linearize(tolerance), type));
        }
        else if (_ocadFallDirSymbols.Contains(elem.Symbol))
        {
          fallDirs.Add(new FallDir((Point2D)elem.Geometry, elem.Angle));
        }
        else
        {
          throw new InvalidOperationException("unexpected Symbol " + elem.Symbol);
        }
      }
    }

    private void ExportOcd(string file, List<Contour> contours)
    {
      Ocad.Ocad9Writer writer = null;
      try
      {
        System.IO.File.Copy(_ocadFile, file, true);
        writer = Ocad.Ocad9Writer.AppendTo(file);
        writer.DeleteElements(null);

        foreach (Contour contour in contours)
        {
          Polyline line = contour.Polyline;
          if (contour.Orientation == Orientation.RightSideDown)
          { line = line.Invert(); }
          Ocad.Element element = new Ocad.ElementV9(true);
          element.Geometry = line;
          element.Type = Ocad.GeomType.line;
          element.Symbol = Symbol(contour.Type);
          writer.Append(element);
        }
      }
      finally
      {
        if (writer != null)
        { writer.Close(); }
      }
    }

    private void ExportShp(string file, List<Contour> contours, Info settings)
    {
      DataTable schema = new DataTable();
      schema.Columns.Add("Shape", typeof(IGeometry));
      schema.Columns.Add(new DBase.DBaseColumn("Symbol", DBase.ColumnType.Int, 8, 0));
      schema.Columns.Add(new DBase.DBaseColumn("Height", DBase.ColumnType.Double, 10, 2));
      Shape.ShapeWriter writer = null;

      int h0 = int.MaxValue;
      int h1 = int.MinValue;
      foreach (Contour contour in contours)
      {
        if (contour.HeightIndex != null && contour.HeightIndex < h0)
        { h0 = contour.HeightIndex.Value; }
        if (contour.HeightIndex != null && contour.HeightIndex > h1)
        { h1 = contour.HeightIndex.Value; }
      }

      double aequi = 2.5;
      double hMin = 100.0;

      if (settings != null)
      {
        if (settings.Aequidistance > 0)
        {
          int interval = 2;
          if (settings.Intervall > 0)
          {
            interval = settings.Intervall;
          }
          aequi = settings.Aequidistance / interval;
        }
        if (settings.HMin > 0)
        {
          hMin = settings.HMin;
        }
        else if (settings.HMax > 0)
        {
          hMin = settings.HMax - (h1 - h0) * aequi;
        }

      }

      try
      {
        writer = new Shape.ShapeWriter(file, Shape.ShapeType.LineZ, schema);
        foreach (Contour contour in contours)
        {
          Polyline line = contour.Polyline;
          if (contour.Orientation == Orientation.LeftSideDown)
          { line = line.Invert(); }

          double h = 0;
          if (contour.HeightIndex != null)
          {
            h = (contour.HeightIndex.Value - h0) * aequi + hMin;
          }

          Polyline lineZ = new Polyline();
          foreach (IPoint p in line.Points)
          {
            lineZ.Add(new Point3D(p.X, p.Y, h));
          }
          writer.Write(lineZ,
            new object[] { Symbol(contour.Type), h });
        }
      }
      finally
      {
        if (writer != null)
        { writer.Close(); }
      }
    }
    private int Symbol(ContourType type)
    {
      foreach (KeyValuePair<int, ContourType> pair in _ocadSymbols)
      {
        if (pair.Value == type)
        { return pair.Key; }
      }
      return -1;
    }

    void calc_ProgressChanged(object sender, ContourSorter.ProgressEventArgs args)
    {
      Delegate dlg = _wdg.GetAction(args);
      if (dlg != null)
      {
        _wdg.Invoke(dlg, args);
        _waitHandle.WaitOne();
      }
    }
  }
}