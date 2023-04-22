using Basics.Geom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
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
      private CntMap _map;
      public _Projection(CntMap map)
      {
        _map = map;
      }

      public IPoint Project(IPoint point)
      {
        if (_map._extent != null)
        {
          return new Point2D((point.X - _map._extent.Min.X) * _map._dF,
                             _map.Height - (point.Y - _map._extent.Min.Y) * _map._dF);
        }
        else
        {
          return new Point2D(point.X * _map._dF, _map.Height - point.Y * _map._dF);
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
    private readonly IProjection _prj;

    public ToolHandler ToolEnd;
    public ToolHandler ToolMove;
    // designer variables
    private PictureBox _pnlMap;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private Container _components = null;

    public CntMap()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      _dataImages = new SortedList<int, Bitmap>();
      _dataGraphics = new Dictionary<int, Graphics>();
      _backImage = new Bitmap(_pnlMap.Width, _pnlMap.Height, _pnlMap.CreateGraphics());
      _pnlMap.BackgroundImage = _backImage;

      //_image = new Bitmap(pnlMap.Width, pnlMap.Height, pnlMap.CreateGraphics());
      //_background = Graphics.FromImage(_image);
      //pnlMap.BackgroundImage = _image;

      _pnlMap.MouseWheel += PnlMap_MouseWheel;
      _pnlMap.MouseLeave += PnlMap_MouseLeave;

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
        if (_components != null)
        {
          _components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    private void DisposeDatagraphics()
    {
      foreach (var pair in _dataGraphics)
      { pair.Value.Dispose(); }
      _dataGraphics.Clear();

      foreach (var pair in _dataImages)
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
      this._pnlMap = new System.Windows.Forms.PictureBox();
      this.SuspendLayout();
      // 
      // pnlMap
      // 
      this._pnlMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._pnlMap.BackColor = System.Drawing.SystemColors.Window;
      this._pnlMap.Location = new System.Drawing.Point(-1, -1);
      this._pnlMap.Name = "pnlMap";
      this._pnlMap.Size = new System.Drawing.Size(336, 232);
      this._pnlMap.TabIndex = 0;
      this._pnlMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PnlMap_MouseDown);
      this._pnlMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PnlMap_MouseMove);
      this._pnlMap.Resize += new System.EventHandler(this.PnlMap_Resize);
      this._pnlMap.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlMap_Paint);
      this._pnlMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PnlMap_MouseUp);
      // 
      // WdgMap
      // 
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this._pnlMap);
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
      if (_dataGraphics.TryGetValue(level, out Graphics back) == false)
      {
        Bitmap image = new Bitmap(_pnlMap.Width, _pnlMap.Height, _pnlMap.CreateGraphics());
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
        _foreground = _pnlMap.CreateGraphics();
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

    public void BeginDraw(ISymbolPart symbolPart, System.Data.DataRow properties)
    {
      if (!_symbolPens.TryGetValue(symbolPart, out Pen pen))
      {
        pen = new Pen(symbolPart.Color);
        _symbolPens.Add(symbolPart, pen);
      }
      pen.Color = symbolPart.Color;
      if (symbolPart is ILineWidthPart lineWidth)
      {
        double width = lineWidth.LineWidth;
        if (symbolPart is IScaleablePart scalable && scalable.Scale)
        {
          Polyline wLine = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(0, width) });
          wLine = wLine.Project(_prj);
          width = wLine.Length();
        }
        pen.Width = (float)width;
      }

      if (!_symbolBrushes.TryGetValue(symbolPart, out Brush b))
      {
        b = new SolidBrush(symbolPart.Color);
        _symbolBrushes.Add(symbolPart, b);
      }
      if (b is SolidBrush sb)
      { sb.Color = symbolPart.Color; }

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
      _pnlMap.Invalidate();
      Application.DoEvents();
    }

    public void DrawLine(Polyline line, ISymbolPart symbolPart)
    {
      if (_symbolPens.TryGetValue(symbolPart, out Pen pen) == false)
      {
        BeginDraw(symbolPart, null);
        pen = _symbolPens[symbolPart];
      }
      Basics.Forms.DrawUtils.DrawLine(Graphics, line, pen);
    }

    public void DrawArea(Surface area, ISymbolPart symbolPart)
    {
      if (area == null && symbolPart == null)
      {
        Graphics.Clear(Color.Transparent);
        return;
      }
      if (_symbolBrushes.TryGetValue(symbolPart, out Brush brush) == false)
      {
        BeginDraw(symbolPart, null);
        brush = _symbolBrushes[symbolPart];
      }
      Basics.Forms.DrawUtils.DrawArea(Graphics, area, brush);
    }


    public void Draw(MapData data)
    {
      if (data == null && _dataImages?.Count > _drawLevel)
      {
        using (Graphics g = Graphics.FromImage(_dataImages[_drawLevel]))
        { g.Clear(Color.Transparent); }
        return;
      }

      try
      {
        data.Draw(this);
      }
      finally
      { SetDrawLevel(0); }
    }
    public void DrawRaster(GridMapData grid)
    {
      if (!(_dataImages?.Count > _drawLevel))
      { return; }

      Bitmap bmp = _dataImages[_drawLevel];

      BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, Width, Height),
        ImageLockMode.ReadWrite, bmp.PixelFormat);

      try
      {
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
      }
      finally
      {
        // To commit the changes, unlock the portion of the bitmap.
        bmp.UnlockBits(bmpData);
      }
      if (grid.Transparency == 0)
      {
        Graphics.DrawImageUnscaled(bmp, 0, 0);
      }
      else
      {
        //create a color matrix object  
        ColorMatrix matrix = new ColorMatrix();

        //set the opacity  
        matrix.Matrix33 = 1 - (float)grid.Transparency;

        //create image attributes  
        ImageAttributes attributes = new ImageAttributes();

        //set the color(opacity) of the image  
        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

        //now draw the image  
        Graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
      }
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
      foreach (var pen in _symbolPens.Values)
      { pen.Dispose(); }
      _symbolPens.Clear();
      foreach (var brush in _symbolBrushes.Values)
      { brush.Dispose(); }
      _symbolBrushes.Clear();

      Flush();
      DisposeDatagraphics();
      _preImage = (Bitmap)_backImage.Clone();
    }

    public void EndDraw()
    {
      _pnlMap.Invalidate();
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
      _screenPointLast = _pnlMap.PointToScreen(_screenPointLast);

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
      System.Drawing.Point end = _pnlMap.PointToScreen(
        new System.Drawing.Point((int)args.End.X, (int)args.End.Y));
      end = PointToClient(end);
      _pnlMap.Top = (int)(end.Y - args.Start.Y);
      _pnlMap.Left = (int)(end.X - args.Start.X);
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

    private void PnlMap_Resize(object sender, EventArgs e)
    {
      try
      {
        Form wdg = (Form)TopLevelControl;
        if (wdg == null || wdg.WindowState == FormWindowState.Minimized)
        { return; }
        if (_pnlMap.Height <= 0 || _pnlMap.Width <= 0)
        { return; }

        DisposeDatagraphics();
        _backImage = new Bitmap(_pnlMap.Width, _pnlMap.Height, _pnlMap.CreateGraphics());
        _pnlMap.BackgroundImage = _backImage;
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void PnlMap_MouseDown(object sender, MouseEventArgs e)
    {
      try
      {
        _pointDown.X = e.X;
        _pointDown.Y = e.Y;
        _screenPointDown = _pnlMap.PointToScreen(_pointDown);
        _screenPointLast = _screenPointDown;
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    public event MouseEventHandler MouseMoveMap;
    private void PnlMap_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        Point2D p0 = InvProject(e.X, e.Y);
        Basics.Logger.Verbose(() => $"{p0.X:N0} {p0.Y:N0}", "mySwitch");
        _pnlMap.Focus();
        ToolMove?.Invoke(this, new ToolArgs(p0, (int)e.Button,
          new Point2D(_pointDown.X, _pointDown.Y),
          new Point2D(e.X, e.Y), false));
        MouseMoveMap?.Invoke(this, e);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void PnlMap_MouseUp(object sender, MouseEventArgs e)
    {
      try
      {
        DrawSelExtent();
        System.Drawing.Point end = new System.Drawing.Point(e.X + _pnlMap.Left, e.Y + _pnlMap.Top);
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
        _pnlMap.Left = 0;
        _pnlMap.Top = 0;
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
    }

    private void PnlMap_MouseLeave(object sender, EventArgs e)
    {
      _pnlMap.Parent.Focus();
    }

    private void PnlMap_MouseWheel(object sender, MouseEventArgs e)
    {
      try
      {
        if (!(TopLevelControl is WdgMain main))
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

    private void PnlMap_Paint(object sender, PaintEventArgs e)
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
    private readonly bool _isPoint;

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
