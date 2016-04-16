using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Basics.Geom;
using TMap;

namespace TMapWin
{
  public delegate void ToolHandler(object sender, ToolArgs e);
  public enum MouseDownStyle { None, Line, Box };
  /// <summary>
  /// Summary description for WdgMap.
  /// </summary>
  public class CntMap : UserControl, ILevelDrawable
  {
    #region nested classes
    private class _Projection : IProjection
    {
      #region IProjection Members
      private CntMap mMap;
      public _Projection(CntMap map)
      {
        mMap = map;
      }

      public IPoint Project(IPoint point)
      {
        if (mMap._extent != null)
        {
          return new Point2D((point.X - mMap._extent.Min.X) * mMap._dF,
                             mMap.Height - (point.Y - mMap._extent.Min.Y) * mMap._dF);
        }
        else
        {
          return new Point2D(point.X * mMap._dF, mMap.Height - point.Y * mMap._dF);
        }
      }

      #endregion
    }
    #endregion

    // member variables
    private double _dF = 1;
    private Box _extent = null;

    private int _drawLevel = 0;
    private Dictionary<int, Graphics> _dataGraphics;
    private SortedList<int, Bitmap> _dataImages;
    private Bitmap _preImage;
    private Bitmap _backImage;
    private Graphics _foreground;
    private Graphics _graph;

    private Dictionary<ISymbolPart, Pen> _symbolPens = new Dictionary<ISymbolPart, Pen>();
    private Dictionary<ISymbolPart, Brush> _symbolBrushes = new Dictionary<ISymbolPart, Brush>();

    private System.Drawing.Point _screenPointDown;
    private System.Drawing.Point _screenPointLast;
    private System.Drawing.Point _pointDown = new System.Drawing.Point(0, 0);
    private MouseDownStyle _downStyle;
    private IProjection _prj;

    public ToolHandler ToolEnd;
    public ToolHandler ToolMove;
    // designer variables
    private PictureBox pnlMap;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public CntMap()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      _dataImages = new SortedList<int, Bitmap>();
      _dataGraphics = new Dictionary<int, Graphics>();
      _backImage = new Bitmap(pnlMap.Width, pnlMap.Height, pnlMap.CreateGraphics());
      pnlMap.BackgroundImage = _backImage;

      //_image = new Bitmap(pnlMap.Width, pnlMap.Height, pnlMap.CreateGraphics());
      //_background = Graphics.FromImage(_image);
      //pnlMap.BackgroundImage = _image;

      pnlMap.MouseWheel += pnlMap_MouseWheel;
      pnlMap.MouseLeave += pnlMap_MouseLeave;

      _prj = new _Projection(this);
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      DisposeDatagraphics();

      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    private void DisposeDatagraphics()
    {
      foreach (KeyValuePair<int, Graphics> pair in _dataGraphics)
      { pair.Value.Dispose(); }
      _dataGraphics.Clear();

      foreach (KeyValuePair<int, Bitmap> pair in _dataImages)
      { { pair.Value.Dispose(); } }
      _dataImages.Clear();

      if (_preImage != null)
      {
        _preImage.Dispose();
        _preImage = null;
      }
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.pnlMap = new System.Windows.Forms.PictureBox();
      this.SuspendLayout();
      // 
      // pnlMap
      // 
      this.pnlMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlMap.BackColor = System.Drawing.SystemColors.Window;
      this.pnlMap.Location = new System.Drawing.Point(-1, -1);
      this.pnlMap.Name = "pnlMap";
      this.pnlMap.Size = new System.Drawing.Size(336, 232);
      this.pnlMap.TabIndex = 0;
      this.pnlMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlMap_MouseDown);
      this.pnlMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMap_MouseMove);
      this.pnlMap.Resize += new System.EventHandler(this.pnlMap_Resize);
      this.pnlMap.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMap_Paint);
      this.pnlMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlMap_MouseUp);
      // 
      // WdgMap
      // 
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.pnlMap);
      this.Name = "WdgMap";
      this.Size = new System.Drawing.Size(334, 230);
      this.ResumeLayout(false);

    }

    #endregion

    public int DrawLevels
    {
      get { return 2; }
    }
    private void SetDrawLevel(int level)
    {
      if (level < 0)
      {
        _graph = ForeGroundGraphics();
      }
      else
      {
        _graph = BackGroundGraphics(level);
      }
      _drawLevel = level;
    }

    private Graphics BackGroundGraphics(int level)
    {
      Graphics back;
      if (_dataGraphics.TryGetValue(level, out back) == false)
      {
        Bitmap image = new Bitmap(pnlMap.Width, pnlMap.Height, pnlMap.CreateGraphics());
        back = Graphics.FromImage(image);

        _dataImages.Add(level, image);
        _dataGraphics.Add(level, back);
      }
      return back;
    }

    private Graphics ForeGroundGraphics()
    {
      if (_foreground == null)
      {
        _foreground = pnlMap.CreateGraphics();
      }
      return _foreground;
    }

    private Graphics Graphics
    {
      get
      {
        SetDrawLevel(_drawLevel);
        return _graph;
      }
    }

    #region ITMapDrawable Members

    private bool _breakDraw;
    public bool BreakDraw
    {
      get { return _breakDraw; }
      set { _breakDraw = value; }
    }

    public void BeginDraw()
    {
      DisposeDatagraphics();
      Flush();
      _lastFlush = DateTime.Now;
    }
    public void BeginDraw(MapData data)
    {
    }

    public void BeginDraw(ISymbolPart symbolPart)
    {
      if (_symbolPens.ContainsKey(symbolPart) == false)
      {
        Pen pen = new Pen(symbolPart.LineColor);
        ILineWidthPart lineWidth = symbolPart as ILineWidthPart;
        if (lineWidth != null)
        {
          double width = lineWidth.LineWidth;
          IScaleablePart scalable = symbolPart as IScaleablePart;
          if (scalable != null && scalable.Scale)
          {
            Polyline wLine = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(0, width) });
            wLine = wLine.Project(_prj);
            width = wLine.Length();
          }
          pen.Width = (float)width;
        }
        _symbolPens.Add(symbolPart, pen);
      }
      if (_symbolBrushes.ContainsKey(symbolPart) == false)
      {
        Brush brush = new SolidBrush(symbolPart.LineColor);
        _symbolBrushes.Add(symbolPart, brush);
      }
      SetDrawLevel(symbolPart.DrawLevel);
    }

    public void Flush()
    {
      using (Graphics g = Graphics.FromImage(_backImage))
      {
        g.Clear(Color.White);
        if (_preImage != null)
        { g.DrawImageUnscaled(_preImage, 0, 0); }
        for (int i = _dataImages.Count - 1; i >= 0; i--)
        {
          Bitmap image = _dataImages[_dataImages.Keys[i]];
          g.DrawImageUnscaled(image, 0, 0);
        }
        g.Flush();
      }
      pnlMap.Invalidate();
      Application.DoEvents();
    }

    public void DrawLine(Polyline line, ISymbolPart symbolPart)
    {
      Pen pen;
      if (_symbolPens.TryGetValue(symbolPart, out pen) == false)
      {
        BeginDraw(symbolPart);
        pen = _symbolPens[symbolPart];
      }
      Div.TMapGraphics.DrawLine(Graphics, line, pen);
    }

    public void DrawArea(Area area, ISymbolPart symbolPart)
    {
      Brush brush;
      if (_symbolBrushes.TryGetValue(symbolPart, out brush) == false)
      {
        BeginDraw(symbolPart);
        brush = _symbolBrushes[symbolPart];
      }
      Div.TMapGraphics.DrawArea(Graphics, area, brush);
    }


    public void Draw(MapData data)
    {
      try
      {
        data.Draw(this);
      }
      finally
      { SetDrawLevel(0); }
    }
    public void DrawRaster(GridMapData grid)
    {
      Bitmap bmp = _backImage;

      BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, Width, Height),
        ImageLockMode.ReadWrite, bmp.PixelFormat);

      // Write to the temporary buffer that is provided by LockBits.
      // Copy the pixels from the source image in this loop.
      // Because you want an index, convert RGB to the appropriate
      // palette index here.
      IntPtr pPixel = bmpData.Scan0;

      unsafe
      {
        // Get the pointer to the image bits.
        // This is the unsafe operation.
        int* pBits;
        if (bmpData.Stride > 0)
        {
          pBits = (int*)pPixel.ToPointer();
        }
        else
        {
          // If the Stide is negative, Scan0 points to the last
          // scanline in the buffer. To normalize the loop, obtain
          // a pointer to the front of the buffer that is located
          // (Height-1) scanlines previous.
          pBits = (int*)pPixel.ToPointer() + bmpData.Stride * (Height - 1);
        }
        uint stride = (uint)Math.Abs(bmpData.Stride) / 4;

        for (int row = 0; row < Height; row++)
        {
          int* pos0 = pBits + row * stride;
          for (int col = 0; col < Width; col++)
          {
            // The destination pixel.
            // The pointer to the color index byte of the
            // destination; this real pointer causes this
            // code to be considered unsafe.
            int* pPix = pos0 + col;

            Point2D pos = InvProject(col, row);
            Color? c = grid.Color(pos.X, pos.Y);
            if (c != null)
            {
              *pPix = ((Color)c).ToArgb();
            }
          }
        }
      } // end unsafe

      // To commit the changes, unlock the portion of the bitmap.
      bmp.UnlockBits(bmpData);
      Graphics.DrawImageUnscaled(bmp, 0, 0);
    }

    public IProjection Projection
    {
      [DebuggerStepThrough]
      get
      { return _prj; }
    }

    public Box Extent
    {
      get { return _extent; }
    }

    public void SetExtent(Basics.Geom.Point point, double scaleFactor)
    {
      if (_extent == null)
      { return; }

      Basics.Geom.Point max = Basics.Geom.Point.CastOrCreate(_extent.Max);
      Basics.Geom.Point d = (0.5 / scaleFactor) * (max - _extent.Min);
      if (point == null)
      { point = 0.5 * (_extent.Min + max); }
      SetExtent(new Box(point - d, point + d));
    }
    public void SetExtent(IBox proposedExtent)
    {
      if (proposedExtent == null)
      { return; }

      double fx, fy, f;
      double dx = proposedExtent.Max.X - proposedExtent.Min.X;
      double dy = proposedExtent.Max.Y - proposedExtent.Min.Y;

      if (_extent == null)
      {
        _extent = new Box(Basics.Geom.Point.Create(proposedExtent.Min),
          Basics.Geom.Point.Create(proposedExtent.Max));
      }

      if (dx != 0.0)
      { fx = Width / dx; }
      else if (dy != 0.0)
      { fx = Height / dy + 1.0; }
      else
      { fx = 1; }
      if (dy != 0.0)
      { fy = Height / dy; }
      else
      { fy = fx + 1; }

      if (fx < fy)
      {
        f = fx;
        _extent.Min.X = proposedExtent.Min.X;
        _extent.Min.Y = proposedExtent.Min.Y - ((Height - fx * dy) / 2.0) / fx;
        if (dx == 0.0)
        { _extent.Min.X = proposedExtent.Min.X - Width / (2.0 * fx); }
      }
      else
      {
        f = fy;
        _extent.Min.X = proposedExtent.Min.X - ((Width - fy * dx) / 2.0) / fy;
        _extent.Min.Y = proposedExtent.Min.Y;
      }
      _extent.Max.X = _extent.Min.X + Width / f;
      _extent.Max.Y = _extent.Min.Y + Height / f;

      _dF = f;
    }

    DateTime _lastFlush = DateTime.Now;
    public void EndDraw(ISymbolPart symbolPart)
    {
      //Pen pen;
      //if (_symbolPens.TryGetValue(symbolPart, out pen))
      //{
      //  _symbolPens.Remove(symbolPart);
      //  pen.Dispose();
      //}

      DateTime t1 = DateTime.Now;
      TimeSpan dt = t1 - _lastFlush;
      if (dt.TotalMilliseconds > 1000)
      {
        Flush();
        _lastFlush = t1;
      }
    }

    public void EndDraw(MapData data)
    {
      foreach (Pen pen in _symbolPens.Values)
      { pen.Dispose(); }
      _symbolPens.Clear();
      foreach (Brush brush in _symbolBrushes.Values)
      { brush.Dispose(); }
      _symbolBrushes.Clear();

      Flush();
      DisposeDatagraphics();
      _preImage = (Bitmap)_backImage.Clone();
    }

    public void EndDraw()
    {
      pnlMap.Invalidate();
      Flush();
    }

    #endregion

    public ToolHandler ToolDownHandler(MouseDownStyle style)
    {
      _downStyle = style;
      return ToolMoveDownStyle;
    }
    private void ToolMoveDownStyle(object sender, ToolArgs args)
    {
      if (args.Mouse == 0)
      { return; }

      DrawSelExtent();

      _screenPointLast.X = (int)args.End.X;
      _screenPointLast.Y = (int)args.End.Y;
      _screenPointLast = pnlMap.PointToScreen(_screenPointLast);

      DrawSelExtent();
    }

    public ToolHandler ToolMovePanelHandler()
    {
      return ToolMovePanel;
    }
    private void ToolMovePanel(object sender, ToolArgs args)
    {
      if (args.Mouse == 0)
      { return; }
      System.Drawing.Point end = pnlMap.PointToScreen(
        new System.Drawing.Point((int)args.End.X, (int)args.End.Y));
      end = PointToClient(end);
      pnlMap.Top = (int)(end.Y - args.Start.Y);
      pnlMap.Left = (int)(end.X - args.Start.X);
      //Console.WriteLine(args.Start + " " + args.End + " " + pnlMap.Left + ";" + pnlMap.Top);
    }

    public Point2D InvProject(double x, double y)
    {
      if (_extent != null)
      {
        return new Point2D(_extent.Min.X + x / _dF,
          _extent.Min.Y - (y - Height) / _dF);
      }
      else
      {
        return new Point2D(x / _dF, -(y - Height) / _dF);
      }
    }

    private void DrawSelExtent()
    {
      if (_downStyle == MouseDownStyle.Box)
      {
        ControlPaint.DrawReversibleFrame(new
          Rectangle(_screenPointDown.X, _screenPointDown.Y,
          _screenPointLast.X - _screenPointDown.X,
          _screenPointLast.Y - _screenPointDown.Y),
          Color.Black, FrameStyle.Dashed);
      }
      else if (_downStyle == MouseDownStyle.Line)
      {
        ControlPaint.DrawReversibleLine(
          _screenPointDown, _screenPointLast, Color.Black);
      }
    }
    #region events

    private void pnlMap_Resize(object sender, EventArgs e)
    {
      try
      {
        Form wdg = (Form)TopLevelControl;
        if (wdg == null || wdg.WindowState == FormWindowState.Minimized)
        { return; }
        if (pnlMap.Height <= 0 || pnlMap.Width <= 0)
        { return; }

        DisposeDatagraphics();
        _backImage = new Bitmap(pnlMap.Width, pnlMap.Height, pnlMap.CreateGraphics());
        pnlMap.BackgroundImage = _backImage;
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void pnlMap_MouseDown(object sender, MouseEventArgs e)
    {
      try
      {
        _pointDown.X = e.X;
        _pointDown.Y = e.Y;
        _screenPointDown = pnlMap.PointToScreen(_pointDown);
        _screenPointLast = _screenPointDown;
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    public event MouseEventHandler MouseMoveMap;
    private void pnlMap_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        Point2D p0 = InvProject(e.X, e.Y);
        Console.WriteLine("{0:N0} {1:N0}", p0.X, p0.Y);
        pnlMap.Focus();
        if (ToolMove != null)
        {
          ToolMove(this, new ToolArgs(p0, (int)e.Button,
            new Point2D(_pointDown.X, _pointDown.Y),
            new Point2D(e.X, e.Y), false));
        }
        if (MouseMoveMap != null)
        { MouseMoveMap(this, e); }
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void pnlMap_MouseUp(object sender, MouseEventArgs e)
    {
      try
      {
        DrawSelExtent();
        System.Drawing.Point end = new System.Drawing.Point(e.X + pnlMap.Left, e.Y + pnlMap.Top);
        if (ToolEnd != null)
        {
          Point2D pm = InvProject(end.X, end.Y);
          if (Math.Abs(_pointDown.X - end.X) < 3 && Math.Abs(_pointDown.Y - end.Y) < 3)
          {
            Point2D p0 = InvProject(_pointDown.X - 2, _pointDown.Y - 2);
            Point2D p1 = InvProject(_pointDown.X + 2, _pointDown.Y + 2);
            ToolEnd(this, new ToolArgs(pm, (int)e.Button, p0, p1, true));
          }
          else
          {
            ToolEnd(this, new ToolArgs(pm, (int)e.Button,
              InvProject(_pointDown.X, _pointDown.Y), pm, false));
          }
        }
        pnlMap.Left = 0;
        pnlMap.Top = 0;
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void pnlMap_MouseLeave(object sender, EventArgs e)
    {
      pnlMap.Parent.Focus();
    }

    private void pnlMap_MouseWheel(object sender, MouseEventArgs e)
    {
      try
      {
        WdgMain main = TopLevelControl as WdgMain;
        if (main == null)
        {
          return;
        }

        double dScale = Math.Pow(1.25, -e.Delta / 120);
        Box newExtent = Extent.Clone();
        Basics.Geom.Point p0 = Point2D.CastOrCreate(newExtent.Min);
        Basics.Geom.Point p1 = Point2D.CastOrCreate(newExtent.Max);
        Basics.Geom.Point p = InvProject(e.X, e.Y);
        p0 = dScale * (p0 - p) + p;
        p1 = dScale * (p1 - p) + p;
        SetExtent(new Box(p0, p1));

        main.Redraw();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }

    }


    #endregion

    private void pnlMap_Paint(object sender, PaintEventArgs e)
    {
      //System.Diagnostics.Debug.WriteLine(_graphics == e.Graphics);
      //e.Graphics.DrawLine(Pens.Red, 10, 10, 50, 60);
    }
  }
  #region auxilliary classes
  public class ToolArgs
  {
    private readonly Point2D _pos;
    private readonly int _mouse;
    private readonly Basics.Geom.Point _start;
    private readonly Basics.Geom.Point _end;
    private Basics.Geom.Point _middle = null;
    private bool _isPoint;

    public ToolArgs(Point2D pos, int mouse, Point2D start, Point2D end, bool isPoint)
    {
      _pos = pos;
      _mouse = mouse;
      _start = start;
      _end = end;
      _isPoint = isPoint;
    }
    public Point2D Position { get { return _pos; } }
    public int Mouse { get { return _mouse; } }
    public Basics.Geom.Point Start { get { return _start; } }
    public Basics.Geom.Point End { get { return _end; } }
    public Basics.Geom.Point Middle
    {
      get
      {
        if (_middle == null)
        { _middle = 0.5 * (_start + _end); }
        return _middle;
      }
    }
    public bool IsPoint { get { return _isPoint; } }
  }
  #endregion auxilliary classes
}
