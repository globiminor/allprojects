
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace A2048.Views
{
  public interface IMainView
  {
    void ShowText(string text, bool success = true);
  }

  public class MainView : View, IMainView
  {
    private MainActivity _activity;

    public MainView(MainActivity activity)
      : base(activity)
    {
      _activity = activity;
    }

    protected override void OnDraw(Canvas canvas)
    {
      base.OnDraw(canvas);

      if (!_keepTextOnDraw)
      { ShowText(null); }

      Paint p = new Paint();
      canvas.Save();
      try
      {
        int n = _activity.Grid.Size;
        float cellSize = GetCellSize();
        for (int iRow = 0; iRow < n; iRow++)
        {
          for (int iCol = 0; iCol < n; iCol++)
          {
            DrawCell(canvas, iRow, iCol, cellSize);
          }
        }
      }
      finally
      { canvas.Restore(); }
    }

    private float GetCellSize()
    {
      return System.Math.Min(MeasuredWidth, MeasuredHeight) / _activity.Grid.Size;
    }

    private void DrawCell(Canvas canvas, int iRow, int iCol, float size)
    {
      Paint p = new Paint();

      RectF rect = new RectF(iCol * size, iRow * size, (iCol + 1) * size, (iRow + 1) * size);

      int value = _activity.Grid.GetValue(iRow, iCol);
      p.Color = GetColor(value);
      p.SetStyle(Paint.Style.Fill);
      canvas.DrawRect(rect, p);

      p.SetStyle(Paint.Style.Stroke);

      if (value > 1)
      {
        p.Color = Color.Black;
        p.TextSize = size / 4;
        p.TextAlign = Paint.Align.Center;
        canvas.DrawText(value.ToString(), (iCol + 0.5f) * size, (iRow + 0.5f) * size, p);
      }

      p.Color = Color.DarkGray;
      p.StrokeWidth = 0.5f * Utils.GetMmPixel(this);
      p.SetStyle(Paint.Style.Stroke);

      canvas.DrawRect(rect, p);
    }

    private Color GetColor(int value)
    {
      if (value < 2)
      { return new Color(192, 192, 192, 255); }
      if (value == 2)
      { return Color.White; }
      if (value == 4)
      { return new Color(234, 234, 234, 255); }
      if (value == 8)
      { return new Color(255, 224, 192, 255); }
      if (value == 16)
      { return new Color(255, 192, 128, 255); }
      if (value == 32)
      { return new Color(255, 160, 64, 255); }
      if (value == 64)
      { return new Color(255, 128, 0, 255); }
      if (value == 128)
      { return new Color(255, 255, 234, 255); }
      if (value == 256)
      { return new Color(255, 255, 192, 255); }
      if (value == 512)
      { return new Color(255, 255, 128, 255); }
      if (value == 1024)
      { return new Color(255, 255, 0, 255); }
      if (value == 2048)
      { return new Color(234, 255, 234, 255); }
      if (value == 4096)
      { return new Color(192, 255, 192, 255); }
      if (value == 8192)
      { return new Color(128, 255, 128, 255); }

      return new Color(255, 0, 0, 255);
    }

    private bool _keepTextOnDraw;
    internal TextView TextInfo
    { get; set; }

    void IMainView.ShowText(string text, bool success)
    {
      ShowText(text, success);
      _keepTextOnDraw = true;
    }

    public float Precision { get { return 1000.0f; } }

    public void ShowText(string text, bool success = true)
    {
      if (string.IsNullOrWhiteSpace(text))
      {
        TextInfo.Visibility = ViewStates.Gone;
      }
      else
      {
        TextInfo.Text = text;
        TextInfo.SetTextColor(success ? Color.Black : Color.Red);
        TextInfo.Visibility = ViewStates.Visible;
        _keepTextOnDraw = success;
      }
      TextInfo.PostInvalidate();
    }

    private float[] _down;
    public bool OnTouch(float x, float y, MotionEventActions action)
    {
      if (action == MotionEventActions.Down)
      {
        _down = new[] { x, y };
      }
      _keepTextOnDraw = false;
      return true;
    }

    public bool IsTouchHandled()
    { return false; }

    public bool Translate(float dx, float dy)
    {
      float aX = System.Math.Abs(dx);
      float aY = System.Math.Abs(dy);
      if (System.Math.Max(aX, aY) < GetCellSize() / 2)
      { return false; }

      float f = (aX > aY) ? aY / aX : aX / aY;
      if (f > 0.5)
      { return false; }
      if (aX > aY)
      {
        if (dx > 0) _activity.Grid.MoveRight();
        else _activity.Grid.MoveLeft();
      }
      else
      {
        if (dy > 0) _activity.Grid.MoveDown();
        else _activity.Grid.MoveUp();
      }
      PostInvalidate();

      return true;
    }
    public void TouchReset()
    {
      _down = null;
    }
  }
}