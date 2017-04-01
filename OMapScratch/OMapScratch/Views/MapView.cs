
using Android.Graphics;
using Android.Widget;
using System.Collections.Generic;
using System;

namespace OMapScratch.Views
{
  public class MapView : ImageView
  {
    private readonly MainActivity _context;
    private Matrix _elemMatrix;
    private float[] _elemMatrixValues;
    private Matrix _inversElemMatrix;
    private Matrix _initMatrix;

    private LinearLayout _contextMenu;
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

    public float Precision { get { return 100; } }

    public MapView(MainActivity context)
      : base(context)
    {
      _context = context;

      SetScaleType(ScaleType.Matrix);
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

    private float[] ElemMatrixValues
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

    public void InitContextMenus(float x, float y)
    {
      Rect rect = new Rect();
      GetGlobalVisibleRect(rect);
      float prec = System.Math.Min(rect.Width(), rect.Height()) / Precision;

      float[] at = { x, y };
      InversElemMatrix.MapPoints(at);
      float[] max = { x + prec, y + prec };
      InversElemMatrix.MapPoints(max);
      float search = System.Math.Max(System.Math.Abs(max[0] - at[0]), System.Math.Abs(max[1] - at[1]));

      IList<ContextAction> actions = _context.MapVm.GetContextActions(at[0], at[1], search);
      if (actions.Count <= 0)
      {
        ContextMenu.Visibility = Android.Views.ViewStates.Invisible;
        ContextMenu.RemoveAllViews();
        ContextMenu.PostInvalidate();
        PostInvalidate();
        return;
      }

      ContextMenu.RemoveAllViews();
      foreach (ContextAction action in actions)
      {
        ActionButton actionButton = new ActionButton(_context, action);
        actionButton.Text = action.Name;
        actionButton.Click += (s, e) =>
        {
          action.Action();
          ContextMenu.Visibility = Android.Views.ViewStates.Invisible;
          PostInvalidate();
        };
        ContextMenu.AddView(actionButton);
      }

      ContextMenu.SetX(x);
      ContextMenu.SetY(y);
      ContextMenu.Visibility = Android.Views.ViewStates.Visible;
      ContextMenu.PostInvalidate();
      PostInvalidate();
    }

    private class ActionButton : Button
    {
      public ContextAction Action { get; }
      public ActionButton(MainActivity context, ContextAction action)
        : base(context)
      { Action = action; }
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

    protected override void OnDraw(Canvas canvas)
    {
      base.OnDraw(canvas);
      DrawElems(canvas, _context.MapVm.Elems);
    }

    public static Color DefaultColor { get { return Color.Red; } }

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

        //canvas.Translate(matrix[2] * symbolScale, matrix[5] * symbolScale);
        //canvas.Scale(matrix[0] / symbolScale, matrix[0] / symbolScale);
        foreach (Elem elem in elems)
        {
          p.Color = elem.Color?.Color ?? DefaultColor;
          elem.Geometry.Draw(canvas, elem.Symbol, matrix, symbolScale, p);
        }
      }
      finally
      { canvas.Restore(); }

      if (ContextMenu.Visibility == Android.Views.ViewStates.Visible)
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

        for (int iChild = 0; iChild < ContextMenu.ChildCount; iChild++)
        {
          ActionButton btn = ContextMenu.GetChildAt(iChild) as ActionButton;
          if (btn == null)
          { continue; }
          Pnt pnt = btn.Action.Position;
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
}