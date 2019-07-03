using Android.Graphics;
using Android.OS;
using Basics;
using Java.IO;

namespace OMapScratch
{
  public partial interface ISegment
  {
    void Init(Path path);
    void AppendTo(Path path);
  }
  public interface ILocationAction
  {
    string WaitDescription { get; }
    string SetDescription { get; }
    void Action(Android.Locations.Location loc);
  }

  public partial interface IMapView
  {
    void SetNextLocationAction(ILocationAction action);
  }

  public partial interface IDrawable
  {
  }

  public partial class MapVm
  {
    public event System.ComponentModel.CancelEventHandler ImageChanging;
    public event System.EventHandler ImageChanged;
    private Android.Locations.Location _currentWorldLocation;

    public Bitmap CurrentImage
    { get { return _map.CurrentImage; } }

    public Android.Locations.Location CurrentWorldLocation
    { get { return _currentWorldLocation; } }
    public void RefreshImage()
    {
      if (EventUtils.Cancel(this, ImageChanging))
      { return; }

      ImageChanged?.Invoke(this, null);
    }

    public System.Collections.Generic.IList<string> GetRecents()
    {
      XmlRecents recents = GetRecents(out string path, create: false);
      return recents?.Recents;
    }

    private XmlRecents GetRecents(out string recentPath, bool create)
    {
      File store = Environment.ExternalStorageDirectory;
      string path = store.AbsolutePath;
      string home = System.IO.Path.Combine(path, ".oscratch");
      if (!System.IO.Directory.Exists(home))
      {
        if (!create)
        {
          recentPath = null;
          return null;
        }
        System.IO.Directory.CreateDirectory(home);
      }

      recentPath = System.IO.Path.Combine(home, "recent.xml");
      XmlRecents recentList;
      if (System.IO.File.Exists(recentPath))
      {
        using (System.IO.TextReader r = new System.IO.StreamReader(recentPath))
        { Serializer.TryDeserialize(out recentList, r); }
      }
      else
      { return null; }

      return recentList;
    }

    internal string SaveImg(Bitmap currentScratch, double[] worldMatrix)
    {
      return _map.SaveImg(currentScratch, worldMatrix);
    }

    public void SetRecent(string configPath)
    {
      XmlRecents recentList = GetRecents(out string recentPath, create: true)
        ?? new XmlRecents { Recents = new System.Collections.Generic.List<string>() }; ;

      recentList.Recents.Insert(0, configPath);
      for (int iPos = recentList.Recents.Count - 1; iPos > 0; iPos--)
      {
        if (recentList.Recents[iPos].Equals(configPath, System.StringComparison.InvariantCultureIgnoreCase))
        { recentList.Recents.RemoveAt(iPos); }
      }
      while (recentList.Recents.Count > 5)
      { recentList.Recents.RemoveAt(recentList.Recents.Count - 1); }

      using (System.IO.TextWriter w = new System.IO.StreamWriter(recentPath))
      { Serializer.Serialize(recentList, w); }
    }

    public void SetCurrentLocation(Android.Locations.Location location)
    {
      _currentWorldLocation = location;
      if (location == null)
      {
        _currentLocalLocation = null;
        return;
      }
      SetCurrentLocation(location.Latitude, location.Longitude, location.Altitude, location.Accuracy);
    }

    public void SynchLocation(Pnt mapCoord, Android.Locations.Location location)
    {
      _map.SynchLocation(mapCoord, location.Latitude, location.Longitude, location.Altitude, location.Accuracy);
    }

    public void LoadLocalImage(int imageIndex, Matrix inversElemMatrix, int width, int height)
    {
      if (_map.Images?.Count <= imageIndex)
      { return; }

      GeoImageViews geoImages = _map.Images[imageIndex];
      LoadLocalImage(geoImages.DefaultView, inversElemMatrix, width, height);
    }

    private MatrixPrj GetWorldPrj(Matrix inversElemMatrix)
    {
      if (inversElemMatrix == null)
      { return null; }

      float[] m = new float[9];
      inversElemMatrix.GetValues(m);
      double[] o = GetOffset();
      MatrixPrj worldPrj = new MatrixPrj(new double[] { m[0], m[1], -m[3], -m[4], o[0] + m[2], o[1] - m[5] });
      return worldPrj;
    }

    public bool CheckUpdateNeeded(Matrix inversElemMatrix, int width, int height)
    {
      return CheckUpdateNeeded(_map.CurrentGeoImage, inversElemMatrix, width, height);
    }
    public bool CheckUpdateNeeded(IGeoImage geoImage, Matrix inversElemMatrix, int width, int height)
    {
      if (geoImage == null)
      { return true; }
      if (inversElemMatrix == null)
      { return true; }

      return CheckUpdateImage(geoImage, ()=> GetWorldPrj(inversElemMatrix), width, height);
    }
    public bool CheckUpdateImage(IGeoImage geoImg, System.Func<MatrixPrj> getWorldPrj, int width, int height)
    {
      return geoImg.CheckUpdateImage(getWorldPrj, width, height);
    }

    public void LoadLocalImage(IGeoImage geoImg, Matrix inversElemMatrix, int width, int height)
    {
      if (geoImg == null)
      { return; }

      MatrixPrj worldPrj = GetWorldPrj(inversElemMatrix);
      LoadImage(geoImg, worldPrj, width, height);
    }

    private void LoadImage(IGeoImage geoImg, MatrixPrj worldPrj, int width, int height)
    {
      if (geoImg == null)
      { return; }

      if (EventUtils.Cancel(this, ImageChanging))
      { return; }

      _map.LoadImage(geoImg, worldPrj, width, height);

      ImageChanged?.Invoke(this, null);
    }
  }

  public partial class Map
  {
    private Bitmap _currentImage;

    public Bitmap CurrentImage { get { return _currentImage; } }

    public Matrix ImageMatrix { get; set; }

    public void LoadImage(IGeoImage geoImg, MatrixPrj worldPrj, int width, int height)
    {
      Bitmap img = geoImg.LoadImage(worldPrj, width, height);

      _currentImage?.Dispose();
      _currentImage = img;
      _currentGeoImage = geoImg;
    }

    public string SaveImg(Bitmap currentScratch, double[] worldMatrix)
    {
      if (currentScratch == null)
      { return null; }

      string imgPath = GetLocalPath(_config?.Data?.ScratchImg ?? DefaultScratchImg);
      if (imgPath == null)
      { return null; }

      using (var stream = new System.IO.FileStream(imgPath, System.IO.FileMode.Create))
      {
        currentScratch.Compress(Bitmap.CompressFormat.Jpeg, 95, stream);
        stream.Close();
      }

      string worldPath = GeoImage.GetWorldPath(imgPath);
      using (System.IO.TextWriter w = new System.IO.StreamWriter(worldPath))
      {
        for (int i = 0; i < 6; i++)
        { w.WriteLine(worldMatrix[i]); }
      }
      return imgPath;
    }

    public static bool Deserialize<T>(string path, out T obj)
    {
      if (!System.IO.File.Exists(path))
      {
        obj = default(T);
        return false;
      }

      using (System.IO.TextReader r = new System.IO.StreamReader(path))
      { Serializer.Deserialize(out obj, r); }

      return true;
    }

    public System.Collections.Generic.List<ColorRef> GetDefaultColors()
    {
      return new System.Collections.Generic.List<ColorRef>
      {
        new ColorRef { Id = "Bl", Color = Color.Black },
        new ColorRef { Id = "Gy", Color = Color.Gray },
        new ColorRef { Id = "Bw", Color = Color.Brown },
        new ColorRef { Id = "Y", Color = Color.Yellow },
        new ColorRef { Id = "G", Color = Color.Green },
        new ColorRef { Id = "K", Color = Color.Khaki },
        new ColorRef { Id = "R", Color = Color.Red },
        new ColorRef { Id = "B", Color = Color.Blue } };
    }
  }

  public partial class XmlColor
  {
    private void GetEnvColor(ColorRef color)
    {
      color.Color = new Color { A = byte.MaxValue, R = Red, G = Green, B = Blue };
    }
    private void SetEnvColor(ColorRef color)
    {
      Red = color.Color.R;
      Green = color.Color.G;
      Blue = color.Color.B;
    }
  }

  public partial class ColorRef
  {
    public Color Color { get; set; }
  }

  public partial class Lin
  {
    void ISegment.Init(Path path)
    {
      path.MoveTo(From.X, From.Y);
    }

    void ISegment.AppendTo(Path path)
    {
      path.LineTo(To.X, To.Y);
    }
  }

  partial class Circle
  {
    void ISegment.Init(Path path)
    { }
    void ISegment.AppendTo(Path path)
    {
      Pnt lt = new Pnt(Center.X - Radius, Center.Y - Radius);
      Pnt rb = new Pnt(Center.X + Radius, Center.Y + Radius);
      RectF r = new RectF
      {
        Left = lt.X,
        Right = rb.X,
        Top = lt.Y,
        Bottom = rb.Y,
      };
      path.AddArc(r, 90 - Azi / (float)System.Math.PI * 180, (float)(-Ang / System.Math.PI) * 180);
    }
  }

  public partial class Bezier
  {
    void ISegment.Init(Path path)
    {
      path.MoveTo(From.X, From.Y);
    }
    void ISegment.AppendTo(Path path)
    {
      path.CubicTo(I0.X, I0.Y, I1.X, I1.Y, To.X, To.Y);
    }
  }

  partial class MatrixPrj
  {
    public MatrixPrj(Matrix matrix)
      : this(GetValues(matrix))
    { }

    public static float[] GetValues(Matrix matrix)
    {
      float[] values = new float[9];
      matrix.GetValues(values);
      return values;
    }

    public MatrixPrj(float[] matrix)
    {
      float[] m = matrix;
      _matrix = new double[] { m[0], m[1], m[3], m[4], m[2], m[5] };
    }

  }
}