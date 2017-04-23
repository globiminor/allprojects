
using Android.Hardware;
using System.Collections.Generic;

namespace OMapScratch.ViewModels
{
  public interface ICompassContext
  {
    SensorManager GetSensorManager();
  }
  public interface ICompassView
  {
    void SetAngle(float? angle);
  }


  public class CompassVm : Java.Lang.Object, ISensorEventListener
  {
    private readonly ICompassContext _context;

    private SensorManager _sensorMgr;
    private Sensor _compass;
    private float? _lastAngle;

    public CompassVm(ICompassContext context)
    {
      _context = context;
    }

    public ICompassView View { get; set; }
    public float? LastAngle { get { return _lastAngle; } }

    private SensorManager InitSensorManager(out Sensor compass)
    {
      SensorManager sensorMgr = _context.GetSensorManager();
      compass = sensorMgr.GetDefaultSensor(SensorType.Orientation);
      return sensorMgr;
    }

    public void StartCompass()
    {
      StopCompass();
      _lastAngle = null;
      _sensorMgr = _sensorMgr ?? InitSensorManager(out _compass);
      _sensorMgr?.RegisterListener(this, _compass, SensorDelay.Fastest);
      Settings.UseCompass = true;
    }
    public void StopCompass()
    {
      _lastAngle = null;
      View?.SetAngle(_lastAngle);
      _sensorMgr = _sensorMgr ?? InitSensorManager(out _compass);
      _sensorMgr.UnregisterListener(this);
    }

    public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
    { }

    public void OnSensorChanged(SensorEvent e)
    {
      IList<float> values = e.Values;
      float? angle = null;
      if (values?.Count > 0)
      { angle = values[0]; }
      _lastAngle = angle;

      View?.SetAngle(angle);
    }
  }
}