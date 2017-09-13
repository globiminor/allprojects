
using Android.Graphics;
using Android.Views;

namespace OMapScratch.Views
{
  class MotionListener : Java.Lang.Object, View.IOnTouchListener
  {
    private class Touch
    {
      public float X { get; set; }
      public float Y { get; set; }
      public long Time { get; set; }
      public MotionEventActions Action { get; set; }
      public float Precision { get; set; }

      public static Touch Create(MotionEvent e, int pointId)
      {
        float x = e.RawX;
        float y = e.RawY;
        if (e.PointerCount > pointId)
        {
          MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
          e.GetPointerCoords(pointId, coords);
          x = coords.X;
          y = coords.Y;
        }
        Touch t = new Touch
        {
          X = x,
          Y = y,
          Time = e.EventTime,
          Action = e.Action,
          Precision = (float)System.Math.Sqrt(e.XPrecision * e.XPrecision + e.YPrecision * e.YPrecision)
        };
        return t;
      }
    }

    private readonly MainActivity _parent;
    public MotionListener(MainActivity parent)
    {
      _parent = parent;
    }

    private Touch _t1Down;
    private Touch _t2Down;
    private Touch _t1Up;
    private Touch _t2Up;

    private long _downTime;
    private bool _longTimeAction = false;

    private void TouchReset()
    {
      _t1Down = null;
      _t2Down = null;
      _t1Up = null;
      _t2Up = null;

      _longTimeAction = false;
      _parent.MapView.ShowDetail = null;
    }

    private float GetPrec()
    {
      using (Rect rect = new Rect())
      { return GetPrec(rect); }
    }
    private float GetPrec(Rect rect)
    {
      MapView mapView = _parent.MapView;
      mapView.GetGlobalVisibleRect(rect);
      float prec = mapView.PrecisionMm * Utils.GetMmPixel(mapView); // System.Math.Min(rect.Height(), rect.Width()) / mapView.Precision;
      return prec;
    }

    private bool HandleAction()
    {
      MapView mapView = _parent.MapView;
      if (_t1Down == null)
      {
        TouchReset();
        return false;
      }
      if (_t1Up == null)
      {
        return false;
      }
      if ((_t2Down == null) != (_t2Up == null))
      {
        if (_t2Up != null)
        { TouchReset(); }
        return false;
      }

      if (_longTimeAction)
      {
        TouchReset();
        IMapView mv = _parent.MapView;
        mv.SetNextPointAction(null);
        return false;
      }

      using (Rect rect = new Rect())
      {
        mapView.GetGlobalVisibleRect(rect);
        if (_t2Down == null)
        {
          float dx = _t1Up.X - _t1Down.X;
          float dy = _t1Up.Y - _t1Down.Y;
          float prec = GetPrec(rect);

          if (mapView.IsTouchHandled())
          { }
          else if (System.Math.Abs(dx) <= prec && System.Math.Abs(dy) <= prec)
          {
            float x = (_t1Down.X + _t1Up.X) / 2.0f;
            float y = (_t1Down.Y + _t1Up.Y) / 2.0f;

            _parent.BtnCurrentMode.MapClicked(x, y);
          }
          else
          { mapView.Translate(dx, dy); }
          mapView.PostInvalidate();

          TouchReset();
          return false;
        }
        {
          float dxDown = _t2Down.X - _t1Down.X;
          float dyDown = _t2Down.Y - _t1Down.Y;
          float lDown = (float)System.Math.Sqrt(dxDown * dxDown + dyDown * dyDown);

          float dxUp = _t2Up.X - _t1Up.X;
          float dyUp = _t2Up.Y - _t1Up.Y;
          float lUp = (float)System.Math.Sqrt(dxUp * dxUp + dyUp * dyUp);
          float scale = lUp / lDown;

          float centerX = (_t1Down.X + _t1Up.X + _t2Down.X + _t2Up.X) / 4 - rect.Left;
          float centerY = (_t1Down.Y + _t1Up.Y + _t2Down.Y + _t2Up.Y) / 4 - rect.Top;

          mapView.Scale(scale, centerX, centerY);
        }
      }

      TouchReset();
      return false;
    }

    bool View.IOnTouchListener.OnTouch(View v, MotionEvent e)
    {
      MapView mapView = _parent.MapView;

      if (e.Action == MotionEventActions.Down)
      {
        _downTime = Java.Lang.JavaSystem.CurrentTimeMillis();
        if (_t1Down == null) _t1Down = Touch.Create(e, 0);
        mapView.OnTouch(_t1Down.X, _t1Down.Y, _t1Down.Action);
        return HandleAction();
      }
      if (e.Action == MotionEventActions.Move)
      {

        Touch move = Touch.Create(e, 0);
        mapView.OnTouch(move.X, move.Y, move.Action);

        TryLongClick(move);

        return false;
      }
      if (e.Action == MotionEventActions.Up)
      {
        if (_t1Up == null) _t1Up = Touch.Create(e, 0);
        else if (_t2Up == null) _t2Up = Touch.Create(e, 0);
        HandleAction();
        TouchReset();
      }
      if (e.Action == MotionEventActions.Pointer2Down)
      {
        _t2Down = Touch.Create(e, 1);
        return HandleAction();
      }
      if (e.Action == MotionEventActions.Pointer2Up)
      {
        _t2Up = Touch.Create(e, 1);
        return HandleAction();
      }
      if (e.Action == MotionEventActions.Pointer1Up)
      {
        _t1Up = Touch.Create(e, 0);
        return HandleAction();
      }
      if (e.Action == MotionEventActions.Pointer1Down)
      {
        _t1Down = Touch.Create(e, 0);
        return HandleAction();
      }

      return false;
    }

    bool TryLongClick(Touch move)
    {
      if (_t1Down == null)
      { return false; }
      if (_t2Down != null)
      { return false; }

      IEditAction editAction = _parent.MapView.NextPointAction as IEditAction;

      if (editAction == null && !(_parent.BtnCurrentMode.CurrentMode is SymbolButton))
      { return false; }

      float x;
      float y;
      if (_longTimeAction)
      {
        _parent.MapVm.Undo();
        x = move.X;
        y = move.Y;
      }
      else
      {
        if (!_parent.DrawOnlyMode)
        {
          float dx = (move.X - _t1Down.X);
          float dy = (move.Y - _t1Down.Y);
          float prec = GetPrec();

          if (System.Math.Abs(dx) > prec || System.Math.Abs(dy) > prec)
          { return false; }

          long dt = Java.Lang.JavaSystem.CurrentTimeMillis() - _downTime;
          if (dt < _parent.MapView.LongClickTime)
          { return false; }
        }

        x = (_t1Down.X + move.X) / 2.0f;
        y = (_t1Down.Y + move.Y) / 2.0f;
      }

      if (editAction != null)
      {
        float[] inverted = { x, y };
        _parent.MapView.InversElemMatrix.MapPoints(inverted);
        Pnt pnt = new Pnt(inverted[0], inverted[1]);

        editAction.Action(pnt);
      }
      else
      { _parent.BtnCurrentMode.MapClicked(x, y); }

      _longTimeAction = true;

      if (editAction?.ShowDetail ?? true)
      { _parent.MapView.ShowDetail = new Pnt(x, y); }

      _parent.MapView.PostInvalidate();

      return false;
    }
  }


}