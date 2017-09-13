
using Android.Locations;
using System.Collections.Generic;
using System.Linq;

namespace OMapScratch.ViewModels
{
  public interface ILocationView
  {
    void ShowText(string text);
    void SetLocation(Location loc);
    bool HasGlobalLocation();
  }
  public interface ILocationContext
  {
    LocationManager GetLocationManager();
  }

  public class LocationVm : Java.Lang.Object, ILocationListener
  {
    private readonly ILocationContext _context;
    private LocationManager _locMgr;
    private string _locProvider;

    public LocationVm(ILocationContext context)
    {
      _context = context;
    }

    public ILocationView View { get; set; }
    public ILocationAction NextLocationAction { get; set; }

    private LocationManager InitLocationManager(out string locationProvider)
    {
      LocationManager locMgr = _context.GetLocationManager();

      if (locMgr.IsProviderEnabled(LocationManager.GpsProvider))
      {
        locationProvider = LocationManager.GpsProvider;
      }
      else
      {
        Criteria criteria = new Criteria { Accuracy = Accuracy.Fine };
        IList<string> locProviders = locMgr.GetProviders(criteria, enabledOnly: true);
        locationProvider = locProviders?.FirstOrDefault();
      }
      return locMgr;
    }

    public void StartLocation(bool forceInit)
    {
      _locMgr = _locMgr ?? InitLocationManager(out _locProvider);

      if ((View?.HasGlobalLocation() ?? false) || forceInit)
      { _locMgr?.RequestLocationUpdates(_locProvider, 1000, 0, this); }
      else
      { _locMgr?.RemoveUpdates(this); }
      if (NextLocationAction != null)
      { View?.ShowText(NextLocationAction.WaitDescription); }
      View?.SetLocation(null);
    }

    public void PauseLocation()
    {
      _locMgr?.RemoveUpdates(this);
      View?.SetLocation(null);
    }

    public void OnLocationChanged(Location location)
    {
      ILocationAction locationAction = NextLocationAction;
      NextLocationAction = null;
      if (locationAction != null)
      {
        locationAction.Action(location);
        View?.ShowText(locationAction.SetDescription);
      }

      View?.SetLocation(location);
      //MapView.TextInfo.Text = $"{location.Latitude:N6}  {location.Longitude:N6}";
      //MapView.TextInfo.Visibility = ViewStates.Visible;
      //MapView.TextInfo.PostInvalidate();
    }

    public void OnProviderDisabled(string provider)
    { }

    public void OnProviderEnabled(string provider)
    { }

    public void OnStatusChanged(string provider, Availability status, Android.OS.Bundle extras)
    { }

  }
}