
using Android.Graphics;
using Android.Widget;
using OMapScratch.ViewModels;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class MapView : ImageView, IMapView
  {
    private class OnGlobalLayoutListener : Java.Lang.Object, Android.Views.ViewTreeObserver.IOnGlobalLayoutListener
    {
      private readonly MapView _parent;
      public OnGlobalLayoutListener(MapView parent)
      {
        _parent = parent;
      }
      public void OnGlobalLayout()
      {
        //_parent.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

        int width = _parent.ContextMenu.MeasuredWidth;
        int height = _parent.ContextMenu.MeasuredHeight;

        float x = _parent.ContextMenu.GetX();
        if (width + x > _parent.Width)
        {
          _parent.ContextMenu.SetX(x - width);
        }
        float y = _parent.ContextMenu.GetY();
        if (height + y > _parent.Height)
        {
          _parent.ContextMenu.SetY(_parent.Height - height);
        }

      }
    }

    private class ActionButton : Button
    {
      public ContextAction Action { get; }
      public ActionButton(MainActivity context, ContextAction action)
        : base(context)
      { Action = action; }
    }

    private readonly MainActivity _context;
    private Matrix _elemMatrix;
    private float[] _elemMatrixValues;
    private Matrix _inversElemMatrix;
    private Matrix _initMatrix;

    private Pnt _editPnt;
    private IPointAction _nextPointAction;

    private LinearLayout _contextMenu;
    private bool _keepTextOnDraw;

    public MapView(MainActivity context)
      : base(context)
    {
      _context = context;

      SetScaleType(ScaleType.Matrix);
    }

    internal ConstrView ConstrView { get; set; }
    public IPointAction NextPointAction { get { return _nextPointAction; } }
    public float Precision { get { return 100; } }

    internal LinearLayout ContextMenu
    {
      get { return _contextMenu; }
      set
      {
        _contextMenu = value;
        Android.Views.ViewTreeObserver vto = _contextMenu.ViewTreeObserver;
        vto.AddOnGlobalLayoutListener(new OnGlobalLayoutListener(this));
      }
    }
    internal TextView TextInfo
    { get; set; }

    public void ResetContextMenu()
    {
      _editPnt = null;
      _nextPointAction = null;
      TextInfo.Visibility = Android.Views.ViewStates.Invisible;
      TextInfo.PostInvalidate();
    }

    void IMapView.SetNextPointAction(IPointAction actionWithNextPoint)
    {
      _nextPointAction = actionWithNextPoint;
      ShowText(actionWithNextPoint.Description);
    }

    private void ShowText(string text, bool success = true)
    {
      if (string.IsNullOrWhiteSpace(text))
      {
        TextInfo.Visibility = Android.Views.ViewStates.Invisible;
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
        SymbolButton btn = mode as SymbolButton;
        if (btn != null)
        {
          string message;
          bool success = symbolAction.Action(btn.Symbol, btn.Color, out message);
          ShowText(message, success);
          if (success)
          { PostInvalidate(); }
        }
        else
        { ShowText("No valid symbol button", false); }
      });
    }

    public void Scale(float f)
    {
      Rect rect = new Rect();
      GetGlobalVisibleRect(rect);
      Scale(f, rect.Width() / 2, rect.Height() / 2);
    }

    public void ResetMap()
    {
      _currentWorldMatrix = null;
      _lastWorldMatrix = null;
      _mapOffset = null;
      ResetElemMatrix();
    }

    private void ResetElemMatrix()
    {
      _elemMatrix = null;
      _elemMatrixValues = null;
      _inversElemMatrix = null;
    }

    private Matrix ElemMatrix
    {
      get
      {
        if (_elemMatrix == null)
        {
          Matrix elemMatrix = new Matrix(ImageMatrix);

          float[] imgMat = CurrentWorldMatrix;
          float[] mapOffset = null;
          if (imgMat.Length > 5)
          {
            mapOffset = MapOffset;
          }
          if (mapOffset?.Length > 1)
          {
            float[] last = new float[] { 0, 0 };
            elemMatrix.MapPoints(last);

            float[] current = new float[] {
              (mapOffset[0] - imgMat[4]) / imgMat[0],
              (mapOffset[1] - imgMat[5]) / imgMat[0]
            };
            elemMatrix.MapPoints(current);


            float scale = 1.0f / imgMat[0];
            float dx = current[0] - last[0];
            float dy = current[1] - last[1];
            elemMatrix.PostTranslate(dx, dy);
            if (scale != 1)
            { Scale(elemMatrix, scale, current[0], current[1]); }
          }
          _elemMatrix = elemMatrix;
        }
        return _elemMatrix;
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
    private Matrix InversElemMatrix
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

    private float[] MapOffset
    {
      get { return _mapOffset ?? (_mapOffset = _context.MapVm.GetOffset() ?? new float[] { }); }
    }
    private float[] _currentWorldMatrix;
    private float[] CurrentWorldMatrix
    {
      get { return _currentWorldMatrix ?? (_currentWorldMatrix = _context.MapVm.GetCurrentWorldMatrix() ?? new float[] { }); }
    }
    private float[] _lastWorldMatrix;
    private float[] _mapOffset;

    public void PrepareUpdateMapImage()
    {
      _lastWorldMatrix = CurrentWorldMatrix;
      _currentWorldMatrix = null;
    }

    public void UpdateMapImage()
    {
      ResetElemMatrix();

      SetScaleType(ScaleType.Matrix);
      SetImageBitmap(_context.MapVm.CurrentImage);

      if (_lastWorldMatrix != null && _lastWorldMatrix.Length > 0)
      {
        float[] newMat = CurrentWorldMatrix;
        if (newMat.Length <= 5 ||
          newMat[0] != _lastWorldMatrix[0] ||
          newMat[4] != _lastWorldMatrix[4] ||
          newMat[5] != _lastWorldMatrix[5])
        {
          float[] last = new float[] { 0, 0 };
          ImageMatrix.MapPoints(last);

          float[] current = new float[] {
            (newMat[4] - _lastWorldMatrix[4]) / _lastWorldMatrix[0],
            (newMat[5] - _lastWorldMatrix[5]) / _lastWorldMatrix[0]
          };
          ImageMatrix.MapPoints(current);

          float scale = newMat[0] / _lastWorldMatrix[0];
          float dx = current[0] - last[0];
          float dy = current[1] - last[1];
          ImageMatrix.PostTranslate(dx, dy);
          if (scale != 1)
          { Scale(scale, current[0], current[1]); }
          return;
        }
        if (_initMatrix != null)
        { return; }
      }

      if (_initMatrix == null)
      {
        Matrix m = new Matrix();
        m.PostTranslate(10, 10);
        _initMatrix = m;
      }
      ImageMatrix = _initMatrix;
    }

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
      float prec = System.Math.Min(rect.Width(), rect.Height()) / Precision;

      float[] max = { x + prec, y + prec };
      InversElemMatrix.MapPoints(max);
      float search = System.Math.Max(System.Math.Abs(max[0] - at[0]), System.Math.Abs(max[1] - at[1]));

      IList<ContextActions> allActions = _context.MapVm.GetContextActions(this, at[0], at[1], search);

      ContextMenu.RemoveAllViews();
      ResetContextMenu();
      bool first = true;
      foreach (ContextActions objActions in allActions)
      {
        AddMenues(objActions, first);
        first = false;
      }
      AddMenues(ConstrView.ViewModel.GetConstrActions(new Pnt(at[0], at[1])), first);

      ContextMenu.SetX(x);
      ContextMenu.SetY(y);
      PositionMenu();

      ContextMenu.Visibility = Android.Views.ViewStates.Visible;
      ContextMenu.PostInvalidate();
      PostInvalidate();
    }

    private void AddMenues(ContextActions objActions, bool first)
    {
      LinearLayout objMenus = new LinearLayout(_context);
      objMenus.Orientation = Orientation.Vertical;

      Button actionsButton = new Button(_context);
      actionsButton.Text = objActions.Name;
      actionsButton.Click += (s, e) =>
      {
        ToggleMenu(ContextMenu, objMenus);
      };

      foreach (ContextAction action in objActions.Actions)
      {
        ActionButton actionButton = new ActionButton(_context, action);
        actionButton.Text = action.Name;
        actionButton.Click += (s, e) =>
        {
          action.Action();
          ContextMenu.Visibility = Android.Views.ViewStates.Invisible;
          PostInvalidate();
        };
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(
            Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.WrapContent);
          lprams.Height = (int)(5 * Utils.GetMmPixel(actionButton));
          actionButton.LayoutParameters = lprams;
        }
        actionButton.SetPadding(0, 0, 0, 0);
        actionButton.SetTextSize(Android.Util.ComplexUnitType.Mm, 2);
        objMenus.AddView(actionButton);
      }
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
          Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.WrapContent);
        lprams.Height = (int)(6 * mm);
        actionsButton.LayoutParameters = lprams;
        actionsButton.SetPadding((int)mm, 0, (int)mm, 0);
      }
      actionsButton.SetTextSize(Android.Util.ComplexUnitType.Mm, 3.5f);

      ContextMenu.AddView(actionsButton);
      ContextMenu.AddView(objMenus);
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

    private void ToggleMenu(LinearLayout contextMenu, LinearLayout objMenus)
    {
      _editPnt = null;
      int nViews = contextMenu.ChildCount;
      for (int iView = 0; iView < nViews; iView++)
      {
        LinearLayout view = contextMenu.GetChildAt(iView) as LinearLayout;
        if (view == null)
        { continue; }
        if (view == objMenus)
        {
          if (view.Visibility == Android.Views.ViewStates.Visible)
          { view.Visibility = Android.Views.ViewStates.Gone; }
          else
          {
            int nMenus = objMenus.ChildCount;
            for (int iMenu = 0; iMenu < nMenus; iMenu++)
            {
              ActionButton btn = objMenus.GetChildAt(iMenu) as ActionButton;
              if (btn != null)
              {
                _editPnt = btn.Action.Position;
                break;
              }
            }
            view.Visibility = Android.Views.ViewStates.Visible;
          }
          view.PostInvalidate();
        }
        else if (view.Visibility != Android.Views.ViewStates.Gone)
        {
          view.Visibility = Android.Views.ViewStates.Gone;
          view.PostInvalidate();
        }
      }

      PositionMenu();
      contextMenu.PostInvalidate();
      PostInvalidate();
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
      Matrix inverted = new Matrix();
      matrix.Invert(inverted);
      inverted.MapPoints(pre);

      matrix.PostScale(scale, scale);
      float[] post = new float[] { pre[0], pre[1] };
      matrix.MapPoints(post);

      matrix.PostTranslate(centerX - post[0], centerY - post[1]);
      return matrix;
    }

    public static Color DefaultColor { get { return Color.Red; } }

    protected override void OnDraw(Canvas canvas)
    {
      base.OnDraw(canvas);

      if (!_keepTextOnDraw)
      { ShowText(null); }

      DrawElems(canvas, _context.MapVm.Elems);

      ConstrView.PostInvalidate();
    }

    private void DrawElems(Canvas canvas, IEnumerable<Elem> elems)
    {
      if (elems == null)
      { return; }

      Paint p = new Paint();
      canvas.Save();
      try
      {
        float[] matrix = ElemMatrixValues;

        float symbolScale = _context.MapVm.SymbolScale;

        foreach (Elem elem in elems)
        {
          p.Color = elem.Color?.Color ?? DefaultColor;
          elem.Geometry.Draw(canvas, elem.Symbol, matrix, symbolScale, p);
        }
      }
      finally
      { canvas.Restore(); }

      if (ContextMenu.Visibility == Android.Views.ViewStates.Visible &&
        _editPnt != null)
      {
        Paint wp = new Paint();
        wp.Color = Color.White;
        wp.StrokeWidth = 3;
        wp.SetStyle(Paint.Style.Stroke);
        Paint bp = new Paint();
        bp.Color = Color.Black;
        bp.StrokeWidth = 1;
        bp.SetStyle(Paint.Style.Stroke);

        Matrix mat = ElemMatrix;

        Pnt pnt = _editPnt;
        float[] draw = new float[] { pnt.X, pnt.Y };
        mat.MapPoints(draw);

        int x0 = (int)draw[0];
        int y0 = (int)draw[1];

        float size = 10;
        Path path = new Path();
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
}