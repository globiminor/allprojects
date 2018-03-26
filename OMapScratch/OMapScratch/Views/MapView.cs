
using Android.Graphics;
using Android.Widget;
using OMapScratch.ViewModels;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class MapView : ImageView, IMapView, ILocationView, ICompassView
  {
    private class ElemButton : Button
    {
      private readonly Elem _elem;
      public ElemButton(MainActivity context, Elem elem)
        : base(context)
      {
        _elem = elem;
        Gravity = Android.Views.GravityFlags.Left;
        //        SetPadding(10, 0, 0, 0);
      }
      protected override void OnDraw(Canvas canvas)
      {
        base.OnDraw(canvas);

        if (_elem != null)
        {
          canvas.Save();
          try
          {
            canvas.Translate(Width - 1.2f * Height, 0);
            Utils.DrawSymbol(canvas, _elem.Symbol, _elem.Color?.Color ?? DefaultColor, Height, Height, 1);
          }
          finally
          { canvas.Restore(); }
        }
      }
    }
    private readonly MainActivity _context;
    private Matrix _elemMatrix;
    private float[] _elemMatrixValues;
    private Matrix _inversElemMatrix;
    private Box _maxExtent;

    private Pnt _editPnt;
    private IPointAction _nextPointAction;

    private ContextMenuView _contextMenu;
    private bool _keepTextOnDraw;
    private float? _elemTextSize;

    private static Matrix _staticElemMatrix;

    public MapView(MainActivity context)
      : base(context)
    {
      _context = context;

      SetScaleType(ScaleType.Matrix);
      PrecisionMm = 2.0f;
      LongClickTime = 500;
    }

    public new MainActivity Context { get { return (MainActivity)base.Context; } }

    internal ConstrView ConstrView { get; set; }
    public IPointAction NextPointAction { get { return _nextPointAction; } }
    public float PrecisionMm { get; set; }
    public long LongClickTime { get; set; }

    public MapVm MapVm
    { get { return _context.MapVm; } }

    bool ILocationView.HasGlobalLocation()
    { return MapVm.HasGlobalLocation(); }
    void ILocationView.SetLocation(Android.Locations.Location loc)
    {
      MapVm.SetCurrentLocation(loc);
      ConstrView.PostInvalidate();
    }
    void ILocationView.ShowText(string text)
    { ShowText(text); }

    void ICompassView.SetAngle(float? angle)
    {
      MapVm.SetCurrentOrientation(angle);
      ConstrView.PostInvalidate();
    }
    void IMapView.StartCompass(bool hide)
    {
      _context.CompassVm.StartCompass();
      HideCompass = hide;
    }
    internal ContextMenuView ContextMenu
    {
      get { return _contextMenu; }
      set { _contextMenu = value; }
    }
    internal TextView TextInfo
    { get; set; }

    internal bool HideCompass { get; set; }

    public Pnt ShowDetail { get; set; }

    public void ResetContextMenu(bool clearMenu = false)
    {
      _editPnt = null;
      _nextPointAction = null;
      TextInfo.Visibility = Android.Views.ViewStates.Gone;
      TextInfo.PostInvalidate();

      if (clearMenu && ContextMenu.Visibility == Android.Views.ViewStates.Visible)
      {
        ContextMenu.RemoveAllViews();
        ContextMenu.Visibility = Android.Views.ViewStates.Invisible;
        ContextMenu.PostInvalidate();
      }
    }

    public Pnt GetCurrentMapLocation()
    {
      Pnt localLoc = _context.MapVm.GetCurrentLocalLocation();
      if (localLoc == null)
      { return null; }

      float[] coords = { localLoc.X, localLoc.Y };
      ElemMatrix.MapPoints(coords);

      return new Pnt(coords[0], coords[1]);
    }

    void IMapView.SetNextPointAction(IPointAction actionWithNextPoint)
    {
      _nextPointAction = actionWithNextPoint;
      ShowText(actionWithNextPoint?.Description);
    }

    void IMapView.SetNextLocationAction(ILocationAction action)
    {
      Android.Locations.Location loc = _context.MapVm.CurrentWorldLocation;
      if (loc != null)
      {
        action.Action(loc);
        return;
      }
      _context.LocationVm.NextLocationAction = action;
      _context.LocationVm.StartLocation(forceInit: true);
    }

    void IMapView.ShowText(string text, bool success)
    {
      ShowText(text, success);
      _keepTextOnDraw = true;
    }
    public void ShowText(string text, bool success = true)
    {
      if (string.IsNullOrWhiteSpace(text))
      {
        TextInfo.Visibility = Android.Views.ViewStates.Gone;
      }
      else
      {
        TextInfo.Text = text;
        TextInfo.SetTextColor(success ? Color.Black : Color.Red);
        TextInfo.Visibility = Android.Views.ViewStates.Visible;
        _keepTextOnDraw = success;
      }
      TextInfo.PostInvalidate();
    }

    void IMapView.SetGetSymbolAction(ISymbolAction symbolAction)
    {
      ShowText(symbolAction.Description);

      _context.ShowSymbols((mode) =>
      {
        if (mode is SymbolButton btn)
        {
          bool success = symbolAction.Action(btn.Symbol, btn.Color, out string message);
          ShowText(message, success);
          if (success)
          { PostInvalidate(); }
        }
        else
        { ShowText("No valid symbol button", false); }
      });
    }

    void IMapView.SetGetColorAction(IColorAction colorAction)
    {
      ShowText(colorAction.Description);

      _context.ShowColors((clr) => colorAction.Action(clr));
    }

    public float ElemTextSize
    { get { return _elemTextSize ?? (_elemTextSize = MapVm.ElemTextSize).Value; } }

    public void Scale(float f)
    {
      Rect rect = new Rect();
      GetGlobalVisibleRect(rect);
      Scale(f, rect.Width() / 2, rect.Height() / 2);
    }

    public void ResetMap()
    {
      ResetImage();
      ResetElemMatrix();
    }

    private void ResetImage()
    {
      _imageWorldMatrix = null;
      _lastWorldMatrix = null;
      _mapOffset = null;

      _imgMapMat?.Dispose();
      _imgMapMat = null;

      _invImgMapMat?.Dispose();
      _invImgMapMat = null;

      _elemTextSize = null;
    }

    private void ResetElemMatrix()
    {
      _elemMatrix?.Dispose();
      _elemMatrix = null;

      _elemMatrixValues = null;

      _inversElemMatrix?.Dispose();
      _inversElemMatrix = null;

      _maxExtent = null;
    }

    public Box MaxExtent
    {
      get
      {
        if (_maxExtent == null)
        {
          float[] p00 = { 0, 0,
            Width, 0,
            0,Height,
            Width, Height };
          _maxExtent = GetMaxExtent(p00, InversElemMatrix);
        }
        return _maxExtent;
      }
    }

    private Box GetMaxExtent(float[] pts, Matrix inversElemMatrix)
    {
      inversElemMatrix.MapPoints(pts);
      float minX = float.MaxValue;
      float minY = float.MaxValue;
      float maxX = float.MinValue;
      float maxY = float.MinValue;
      for (int i = 0; i < pts.Length; i += 2)
      {
        minX = System.Math.Min(pts[i], minX);
        maxX = System.Math.Max(pts[i], maxX);

        minY = System.Math.Min(pts[i + 1], minY);
        maxY = System.Math.Max(pts[i + 1], maxY);
      }

      float maxSymbolSize = MapVm.MaxSymbolSize;
      Pnt min = new Pnt(minX - maxSymbolSize, minY - maxSymbolSize);
      Pnt max = new Pnt(maxX + maxSymbolSize, maxY + maxSymbolSize);

      Box maxExtent = new Box(min, max);
      return maxExtent;
    }

    public Matrix ElemMatrix
    {
      get
      {
        if (_elemMatrix == null)
        {
          _elemMatrix = GetElementMatrix(ImageMatrix, ImageMapMatrix);
          _staticElemMatrix?.Dispose();
          _staticElemMatrix = new Matrix(_elemMatrix);
        }
        return _elemMatrix;
      }
    }
    public void SetElemMatrix(Matrix elem, bool postInvalidate = true)
    {
      ResetElemMatrix();
      _elemMatrix?.Dispose();
      _elemMatrix = elem;
      ImageMatrix = GetImageMatrix(elem);

      if (postInvalidate)
      { PostInvalidate(); }
    }

    private Matrix GetImageMatrix(Matrix elemMatrix)
    {
      float d = 1000;
      float[] pts = new float[]
      {
        0,0,
        d, 0,
        0, d
      };
      //      invCwMat.MapPoints(pts);

      using (Matrix invElemMat = new Matrix())
      {
        elemMatrix.Invert(invElemMat);
        invElemMat.MapPoints(pts);
      }

      InverseImageMapMatrix.MapPoints(pts);
      //      elemMatrix.MapPoints(pts);

      float xx = (pts[2] - pts[0]) / d;
      float yx = (pts[3] - pts[1]) / d;
      float xy = (pts[4] - pts[0]) / d;
      float yy = (pts[5] - pts[1]) / d;

      using (Matrix invImgMat = new Matrix())
      {
        invImgMat.SetValues(new float[] { xx, xy, pts[0], yx, yy, pts[1], 0, 0, 1 });

        Matrix imgMat = new Matrix();
        invImgMat.Invert(imgMat);

        return imgMat;
      }
    }

    private static Matrix GetElementMatrix(Matrix imageMatrix, Matrix imageMapMatrix)
    {
      float d = 1000;
      float[] pts = new float[]
      {
        0, 0,
        d, 0,
        0, d
      };
      using (Matrix invImgMat = new Matrix())
      {
        imageMatrix.Invert(invImgMat);
        invImgMat.MapPoints(pts);
      }
      imageMapMatrix.MapPoints(pts);

      float xx = (pts[2] - pts[0]) / d;
      float yx = (pts[3] - pts[1]) / d;
      float xy = (pts[4] - pts[0]) / d;
      float yy = (pts[5] - pts[1]) / d;

      using (Matrix invElemMat = new Matrix())
      {
        invElemMat.SetValues(new float[] { xx, xy, pts[0], yx, yy, pts[1], 0, 0, 1 });

        Matrix elemMat = new Matrix();
        invElemMat.Invert(elemMat);

        return elemMat;
      }
    }

    public float[] ElemMatrixValues
    {
      get
      {
        if (_elemMatrixValues == null)
        {
          float[] matrix = new float[9];
          ElemMatrix.GetValues(matrix);
          _elemMatrixValues = matrix;
        }
        return _elemMatrixValues;
      }
    }
    public Matrix InversElemMatrix
    {
      get
      {
        if (_inversElemMatrix == null)
        {
          Matrix inverse = new Matrix();
          ElemMatrix.Invert(inverse);

          _inversElemMatrix = inverse;
        }
        return _inversElemMatrix;
      }
    }

    private double[] MapOffset
    {
      get { return _mapOffset ?? (_mapOffset = _context.MapVm.GetOffset() ?? new double[] { }); }
    }
    private double[] _imageWorldMatrix;
    private double[] ImageWorldMatrix
    {
      get { return _imageWorldMatrix ?? (_imageWorldMatrix = _context.MapVm.GetCurrentWorldMatrix() ?? new double[] { }); }
    }
    private double[] _lastWorldMatrix;
    private double[] _mapOffset;

    private Matrix _imgMapMat;
    public Matrix ImageMapMatrix
    {
      get { return _imgMapMat ?? (_imgMapMat = GetImageMapMatrix(ImageWorldMatrix, MapOffset)); }
    }

    private static Matrix GetImageMapMatrix(double[] imageWorldMatrix, double[] mapOffset)
    {
      if (!(imageWorldMatrix?.Length > 5))
      { return new Matrix(); }

      double[] c = imageWorldMatrix;
      double[] o = mapOffset;

      float[] vals = new float[]
      {
        (float)c[0], (float)c[1], (float)(c[4] - o[0]),
        -(float)c[2], -(float)c[3], -(float)(c[5] - o[1]),
        0, 0, 1
      };
      Matrix imgMapMat = new Matrix();
      imgMapMat.SetValues(vals);
      return imgMapMat;
    }

    private Matrix _invImgMapMat;
    public Matrix InverseImageMapMatrix
    {
      get
      {
        if (_invImgMapMat == null)
        {
          Matrix inverse = new Matrix();
          ImageMapMatrix.Invert(inverse);
          _invImgMapMat = inverse;
        }
        return _invImgMapMat;
      }
    }


    public void PrepareUpdateMapImage()
    {
      _lastWorldMatrix = ImageWorldMatrix;
      _imageWorldMatrix = null;
    }

    public void UpdateMapImage()
    {
      ResetImage();

      SetScaleType(ScaleType.Matrix);
      SetImageBitmap(_context.MapVm.CurrentImage);

      Matrix imgMat;
      if (_elemMatrix != null)
      {
        imgMat = GetImageMatrix(_elemMatrix);
      }
      else
      {
        Matrix m = new Matrix();

        m.PostTranslate(10, 10);
        imgMat = m;
      }
      ImageMatrix = imgMat;
    }

    //private int ImageMatrix;

    public void AddPoint(Symbol symbol, ColorRef color, float x, float y)
    {
      float[] inverted = { x, y };
      InversElemMatrix.MapPoints(inverted);

      _context.MapVm.AddPoint(inverted[0], inverted[1], symbol, color);
    }

    public bool OnTouch(float x, float y, Android.Views.MotionEventActions action)
    {
      ITouchAction touchAction = _nextPointAction as ITouchAction;
      if (touchAction == null)
      { return false; }
      if (action == Android.Views.MotionEventActions.Down)
      {
        float[] at = { x, y };
        InversElemMatrix.MapPoints(at);
        return touchAction.OnDown(at);
      }
      else if (action == Android.Views.MotionEventActions.Move)
      {
        float[] at = { x, y };
        InversElemMatrix.MapPoints(at);
        return touchAction.OnMove(at);
      }

      return false;
    }

    public bool IsTouchHandled(bool checkOnly = false)
    {
      ITouchAction touchAction = _nextPointAction as ITouchAction;
      if (touchAction == null)
      { return false; }

      if (checkOnly)
      { return true; }

      if (!touchAction.TryHandle(reInit: true))
      { return false; }

      ResetContextMenu();
      return true;
    }

    public void InitContextMenus(float x, float y)
    {
      float[] at = { x, y };
      InversElemMatrix.MapPoints(at);

      if (_nextPointAction != null)
      {
        IPointAction action = _nextPointAction;
        ResetContextMenu();

        action.Action(new Pnt(at[0], at[1]));

        ContextMenu.Visibility = Android.Views.ViewStates.Invisible;
        ContextMenu.RemoveAllViews();
        ContextMenu.PostInvalidate();
        PostInvalidate();
        return;
      }

      Rect rect = new Rect();
      GetGlobalVisibleRect(rect);
      float prec = PrecisionMm * Utils.GetMmPixel(this); // System.Math.Min(rect.Width(), rect.Height()) / Precision;

      float[] max = { x + prec, y + prec };
      InversElemMatrix.MapPoints(max);
      float search = System.Math.Max(System.Math.Abs(max[0] - at[0]), System.Math.Abs(max[1] - at[1]));

      IList<ContextActions> allActions = _context.MapVm.GetContextActions(this, at[0], at[1], search);

      ContextMenu.RemoveAllViews();
      ResetContextMenu();
      bool first = true;
      foreach (ContextActions objActions in allActions)
      {
        LinearLayout objMenus = AddMenues(objActions, first);
        AddConstrMenus(objMenus, ConstrView.ViewModel.GetConstrActions(objActions.Position));
        first = false;
      }
      {
        Pnt pos = new Pnt(at[0], at[1]);
        List<ContextAction> actions = ConstrView.ViewModel.GetConstrActions(pos);
        ContextActions constr = new ContextActions("Constr.", null, pos, actions);
        AddMenues(constr, first);
      }

      ContextMenu.SetX(x);
      ContextMenu.SetY(y);
      PositionMenu();

      ContextMenu.Visibility = Android.Views.ViewStates.Visible;
      ContextMenu.PostInvalidate();
      PostInvalidate();
    }

    private void AddConstrMenus(LinearLayout menues, IList<ContextAction> constrActions)
    {
      ActionButton btnConstr = new ActionButton(_context, null) { Text = "constrs..." };
      btnConstr.Visibility = Android.Views.ViewStates.Visible;
      ActionButton btnOthers = new ActionButton(_context, null) { Text = "others..." };
      btnOthers.Visibility = Android.Views.ViewStates.Gone;

      menues.AddView(btnConstr);
      menues.AddView(btnOthers);

      btnConstr.Click += (s, e) => Utils.Try(() =>
      {
        ToggleVisible(menues);
        menues.PostInvalidate();
      });

      btnOthers.Click += (s, e) => Utils.Try(() =>
      {
        ToggleVisible(menues);
        menues.PostInvalidate();
      });

      AddMenues(menues, constrActions, visibility: Android.Views.ViewStates.Gone);
    }

    private void ToggleVisible(LinearLayout menues)
    {
      for (int i = 0; i < menues.ChildCount; i++)
      {
        Android.Views.View child = menues.GetChildAt(i);
        child.Visibility = (child.Visibility == Android.Views.ViewStates.Visible) ? Android.Views.ViewStates.Gone : Android.Views.ViewStates.Visible;
      }
    }

    private void AddMenues(LinearLayout menues, IList<ContextAction> actions, Android.Views.ViewStates visibility = Android.Views.ViewStates.Visible)
    {
      foreach (ContextAction action in actions)
      {
        ActionButton actionButton = new ActionButton(_context, action)
        { Text = action.Name };
        actionButton.Click += (s, e) => Utils.Try(() =>
        {
          action.Execute();
          ContextMenu.Visibility = Android.Views.ViewStates.Invisible;
          PostInvalidate();
        });
        actionButton.Visibility = visibility;
        menues.AddView(actionButton);
      }
    }
    private LinearLayout AddMenues(ContextActions objActions, bool first)
    {
      LinearLayout objMenus = new LinearLayout(_context)
      { Orientation = Orientation.Vertical };

      ElemButton actionsButton = new ElemButton(_context, objActions.Elem);
      actionsButton.SetAllCaps(false);
      actionsButton.Text = objActions.Name;
      actionsButton.Click += (s, e) => Utils.Try(() =>
      {
        _editPnt = ContextMenu.ToggleMenu(objMenus);
        PositionMenu();
        ContextMenu.PostInvalidate();
        PostInvalidate();
      });

      AddMenues(objMenus, objActions.Actions);
      if (first)
      {
        objMenus.Visibility = Android.Views.ViewStates.Visible;
        _editPnt = objActions.Position;
      }
      else
      { objMenus.Visibility = Android.Views.ViewStates.Gone; }

      {
        float mm = Utils.GetMmPixel(actionsButton);

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(
          Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.WrapContent)
        { Height = (int)(6 * mm) };
        actionsButton.LayoutParameters = lprams;
        actionsButton.SetPadding((int)mm, 0, (int)mm, 0);
      }
      actionsButton.SetTextSize(Android.Util.ComplexUnitType.Mm, 3.5f);

      ContextMenu.AddView(actionsButton);
      ContextMenu.AddView(objMenus);

      return objMenus;
    }

    private void PositionMenu()
    {
      if (_editPnt == null)
      { return; }

      Matrix mat = ElemMatrix;

      float[] draw = new float[] { _editPnt.X, _editPnt.Y };
      mat.MapPoints(draw);

      ContextMenu.SetX(draw[0]);
      ContextMenu.SetY(draw[1]);
    }

    public void Translate(float dx, float dy)
    {
      ResetElemMatrix();
      ImageMatrix.PostTranslate(dx, dy);

      PostInvalidate();
    }
    public void Scale(float scale, float centerX, float centerY)
    {
      ResetElemMatrix();

      ImageMatrix = Scale(new Matrix(ImageMatrix), scale, centerX, centerY);

      PostInvalidate();
    }

    private static Matrix Scale(Matrix matrix, float scale, float centerX, float centerY)
    {
      float[] pre = new float[] { centerX, centerY };

      using (Matrix inverted = new Matrix())
      {
        matrix.Invert(inverted);
        inverted.MapPoints(pre);
      }

      matrix.PostScale(scale, scale);
      float[] post = new float[] { pre[0], pre[1] };
      matrix.MapPoints(post);

      matrix.PostTranslate(centerX - post[0], centerY - post[1]);
      return matrix;
    }

    public void Rotate(double angle, float centerX, float centerY)
    {
      ResetElemMatrix();

      ImageMatrix = Rotate(new Matrix(ImageMatrix), angle, centerX, centerY);

      PostInvalidate();
    }


    private static Matrix Rotate(Matrix matrix, double angle, float centerX, float centerY)
    {
      float[] pre = new float[] { centerX, centerY };

      using (Matrix inverted = new Matrix())
      {
        matrix.Invert(inverted);
        inverted.MapPoints(pre);
      }

      matrix.PostRotate((float)(angle * 180 / System.Math.PI));
      float[] post = new float[] { pre[0], pre[1] };
      matrix.MapPoints(post);

      matrix.PostTranslate(centerX - post[0], centerY - post[1]);
      return matrix;
    }


    public static Color DefaultColor { get { return Color.Red; } }

    protected override void OnDraw(Canvas canvas)
    {
      DrawRegion(canvas);
      ConstrView.PostInvalidate();
    }

    public string ProcessScratchImg(System.Func<Bitmap, double[], string> handleBitmap)
    {
      Bitmap current = _context?.MapVm?.CurrentImage;
      if (current == null)
      { return null; }

      float symScale = _context.MapVm.SymbolScale;
      float imgResol = (float)ImageWorldMatrix[0];
      float f = imgResol > symScale ? imgResol / symScale : 1;

      int w = (int)(current.Width * f);
      int h = (int)(current.Height * f);

      double[] worldMatrix = new double[6];
      worldMatrix[0] = ImageWorldMatrix[0] / f;
      worldMatrix[1] = ImageWorldMatrix[1] / f;
      worldMatrix[2] = ImageWorldMatrix[1] / f;
      worldMatrix[3] = ImageWorldMatrix[3] / f;
      worldMatrix[4] = ImageWorldMatrix[4];
      worldMatrix[5] = ImageWorldMatrix[5];

      using (Bitmap bitmap = Bitmap.CreateBitmap(w, h, current.GetConfig()))
      {
        using (Canvas canvas = new Canvas(bitmap))
        {
          canvas.DrawColor(Color.White);
          float x0 = (float)(ImageWorldMatrix[4] - MapOffset[0]);
          float y0 = (float)(ImageWorldMatrix[5] - MapOffset[1]);
          float dx = (float)ImageWorldMatrix[0];

          Box maxExtent = new Box(
            new Pnt(x0, y0 - h * dx),
            new Pnt(x0 + w * dx, y0));

          maxExtent = MaxExtent;

          Matrix imageMatrix = new Matrix();
          Matrix elemMatrix = GetElementMatrix(imageMatrix, GetImageMapMatrix(worldMatrix, MapOffset));
          float[] elemMatrixValues = new float[9];
          elemMatrix.GetValues(elemMatrixValues);
          Matrix inverseElemMatrix = new Matrix();
          elemMatrix.Invert(inverseElemMatrix);
          float[] pts = { 0, 0,
            bitmap.Width, 0,
            0, bitmap.Height,
            bitmap.Width, bitmap.Height };
          maxExtent = GetMaxExtent(pts, inverseElemMatrix);

          DrawElems(canvas, _context.MapVm.Elems, maxExtent, elemMatrixValues, null);
        }

        string savePath = handleBitmap(bitmap, worldMatrix);
        return savePath;
      }
    }
    private void DrawRegion(Canvas canvas)
    {
      base.OnDraw(canvas);

      if (!_keepTextOnDraw)
      { ShowText(null); }

      DrawElems(canvas, _context.MapVm.Elems, MaxExtent, ElemMatrixValues, null);
      DrawConstr(canvas);

      DrawDetail(canvas);
    }

    private void DrawElems(Canvas canvas, IEnumerable<Elem> elems, Box maxExtent, 
      float[] elemMatrixValues, float[] dd)
    {
      if (elems == null)
      { return; }

      using (Paint p = new Paint())
      {
        p.TextSize = ElemTextSize;
        canvas.Save();
        try
        {
          float[] matrix = elemMatrixValues;

          if (dd != null)
          {
            matrix = (float[])matrix.Clone();
            matrix[2] -= dd[0];
            matrix[5] -= dd[1];
          }

          float symbolScale = _context.MapVm.SymbolScale;
          MatrixProps matrixProps = new MatrixProps(matrix);

          foreach (Elem elem in elems)
          {
            if (!maxExtent.Intersects(elem.Geometry.Extent))
            { continue; }

            p.Color = elem.Color?.Color ?? DefaultColor;
            elem.Geometry.Draw(canvas, elem.Symbol, matrixProps, symbolScale, p);
          }
        }
        finally
        { canvas.Restore(); }
      }
    }

    private void DrawConstr(Canvas canvas)
    {
      if (ContextMenu.Visibility != Android.Views.ViewStates.Visible ||
        _editPnt == null)
      { return; }

      using (Paint wp = new Paint())
      using (Paint bp = new Paint())
      {
        wp.Color = Color.White;
        wp.StrokeWidth = 3 * ConstrView.ConstrLineWidth;
        wp.SetStyle(Paint.Style.Stroke);

        bp.Color = ConstrView.ConstrColor;
        bp.StrokeWidth = ConstrView.ConstrLineWidth;
        bp.SetStyle(Paint.Style.Stroke);

        Matrix mat = ElemMatrix;

        Pnt pnt = _editPnt;
        float[] draw = new float[] { pnt.X, pnt.Y };
        mat.MapPoints(draw);

        int x0 = (int)draw[0];
        int y0 = (int)draw[1];

        float size = 10;
        using (Path path = new Path())
        {
          path.MoveTo(draw[0] - size, draw[1] - size);
          path.LineTo(draw[0] - size, draw[1] + size);
          path.LineTo(draw[0] + size, draw[1] + size);
          path.LineTo(draw[0] + size, draw[1] - size);
          path.LineTo(draw[0] - size, draw[1] - size);

          canvas.DrawPath(path, wp);
          canvas.DrawPath(path, bp);
        }
      }
    }

    private int _detailSize = 300;

    public Rect GetDetailRect()
    {
      int det = _detailSize;
      int rx, ry;
      if (_context?.DetailUpperRight ?? true)
      {
        int w = Width;
        rx = w - det;
        ry = 0;
      }
      else
      {
        int h = Height;
        rx = 0;
        ry = h - det;
      }
      return new Rect(rx, ry, rx + det, ry + det);
    }

    public float[] GetDetailOffset(Rect rect)
    {
      int det_2 = _detailSize / 2;
      float f = 1; // vals[0];
      float[] dd = { (ShowDetail.X - (rect.Left + det_2)) * f, (ShowDetail.Y - (rect.Top + det_2)) * f };
      return dd;
    }

    public Box GetDetailExtent()
    {
      int det_2 = _detailSize / 2;

      float[] pts =
      {
        ShowDetail.X - det_2, ShowDetail.Y - det_2,
        ShowDetail.X - det_2, ShowDetail.Y + det_2,
        ShowDetail.X + det_2, ShowDetail.Y - det_2,
        ShowDetail.X + det_2, ShowDetail.Y + det_2
      };

      Box maxExtent = GetMaxExtent(pts, InversElemMatrix);

      return maxExtent;
    }

    private void DrawDetail(Canvas canvas)
    {
      if (ShowDetail == null)
      { return; }

      Bitmap current = _context?.MapVm?.CurrentImage;
      if (current == null)
      { return; }

      using (Rect dest = GetDetailRect())
      {
        try
        {
          canvas.Save();
          canvas.ClipRect(dest);

          Box maxExtent = GetDetailExtent();

          using (Matrix local = new Matrix(ImageMatrix))
          {
            local.PostTranslate(dest.ExactCenterX() - ShowDetail.X, dest.ExactCenterY() - ShowDetail.Y);
            canvas.DrawBitmap(_context.MapVm.CurrentImage, local, null);
          }

          float[] dd = GetDetailOffset(dest);
          DrawElems(canvas, _context.MapVm.Elems, maxExtent, ElemMatrixValues, dd);
          DrawConstr(canvas);
        }
        finally
        { canvas.Restore(); }
      }
    }
  }
}