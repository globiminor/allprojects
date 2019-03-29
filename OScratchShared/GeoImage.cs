using System;
using System.Collections.Generic;
using System.IO;

namespace OMapScratch
{
  public partial interface IGeoImage
  {
    double[] GetWorldMatrix();
  }

  public partial class GeoImageComb : IGeoImage
  {
    private readonly List<GeoImageView> _views;
    private readonly GeoImage _baseImage;
    private double[] _worldMatrix;

    public GeoImageComb(GeoImage baseImage)
    {
      _views = new List<GeoImageView>();
      _baseImage = baseImage;
    }

    public void Add(GeoImageView geoImage)
    {
      _views.Add(geoImage);
    }
    public double[] GetWorldMatrix()
    {
      _worldMatrix = _worldMatrix ?? _baseImage.DefaultWorldMatrix;
      return _worldMatrix;
    }
  }

  public partial class GeoImageView : IGeoImage
  {
    public GeoImage BaseImage { get; }
    private double[] _worldMatrix;
    private double _transparency;

    public GeoImageView(GeoImage baseImage)
    {
      BaseImage = baseImage;
    }

    public double[] GetWorldMatrix()
    {
      _worldMatrix = _worldMatrix ?? BaseImage.DefaultWorldMatrix;
      return _worldMatrix;
    }

    public double Transparency
    {
      get => _transparency;
      set { _transparency = Math.Max(0, Math.Min(1, value)); }
    }
  }

  public interface IGeoImagesContainer
  {
    void Invalidate();
  }
  public partial class GeoImageVm : Views.BaseVm
  {
    private readonly GeoImage _baseImage;
    private int _opacity;
    private bool _visible;

    public GeoImageVm(GeoImageComb editView, GeoImageViews container, IReadOnlyList<GeoImageVm> combinations)
      : this(container.BaseImage)
    {
      Name = container.BaseImage.Name;
      _editView = editView;
      _container = container;
      _combinations = combinations;
    }

    public GeoImageVm(GeoImage baseImage)
    {
      _baseImage = baseImage;
    }

    public IGeoImagesContainer Container { get; set; }

    public MatrixPrj Projection { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public string Name { get; set; }

    private readonly GeoImageComb _editView;
    private readonly GeoImageViews _container;
    private readonly IReadOnlyList<GeoImageVm> _combinations;

    public GeoImage BaseImage => _baseImage;

    public bool VisibleEnabled { get; set; }

    public bool Visible
    {
      get => _visible;
      set
      {
        if (SetStruct(ref _visible, value))
        { Container?.Invalidate(); }
      }
    }
    public int Opacity
    {
      get => _opacity;
      set
      {
        if (SetStruct(ref _opacity, value))
        { Container?.Invalidate(); }
      }
    }

    public void Save()
    {
      if (_container == null || _combinations == null)
      { return; }

      GeoImageComb comb = new GeoImageComb(_baseImage);
      GeoImageView baseImg = new GeoImageView(_baseImage);
      baseImg.Transparency = 1 - Opacity / 100.0;
      comb.Add(baseImg);
      foreach (var img in _combinations)
      {
        if (!img.Visible || img.Opacity <= 0)
        { continue; }

        GeoImageView part = new GeoImageView(img.BaseImage);
        part.Transparency = 1 - img.Opacity / 100.0;
        comb.Add(part);
      }
      _container.Replace(_editView, comb);
    }
  }
  public class GeoImageViews
  {
    private readonly GeoImage _baseImage;
    private readonly List<IGeoImage> _views;

    public GeoImageViews(GeoImage baseImage)
    {
      _baseImage = baseImage;
      _views = new List<IGeoImage>();
      _views.Add(new GeoImageView(baseImage));
    }
    public IGeoImage DefaultView => _views[0];
    public GeoImage BaseImage => _baseImage;
    public IReadOnlyList<IGeoImage> Views => _views;

    [Obsolete("implement")]
    public void Replace(IGeoImage old, IGeoImage geoImage)
    {
      if (geoImage == null)
      {
        if (old != null)
        { _views.Remove(old); }
        return;
      }
      if (old == null)
      {
        _views.Add(geoImage);
        return;
      }
      int pos = _views.IndexOf(old);
      if (pos < 0)
      {
        _views.Add(geoImage);
        return;
      }
      _views.RemoveAt(pos);
      _views.Insert(pos, geoImage);
      // TODO: old.Replace(geoImageView);
    }
  }

  public partial class GeoImage
  {
    public class Part
    {
      public string Path { get; internal set; }
      public int Height { get; set; }
      public int Width { get; set; }

      private double[] _worldMatrix;
      public double[] WorldMatrix => _worldMatrix ?? (_worldMatrix = GetWorldMatrix(Path));
    }

    private static int MaxDim = 3000;

    private IBox _imageExtent;
    public IBox ImageExtent
    {
      get
      {
        if (_imageExtent == null)
        {
          double[] d = DefaultWorldMatrix;
          double[] localDefaultMatrix = new double[] { d[0], d[1], d[2], d[3], 0, 0 };
          MatrixPrj inversePrj = new MatrixPrj(localDefaultMatrix).GetInverse();

          Box extent = null;
          foreach (Part part in Parts)
          {
            double[] m = part.WorldMatrix;
            double[] localMatrix = new double[] { d[0], d[1], d[2], d[3], m[4] - d[4], m[5] - d[5] };
            MatrixPrj prj = new MatrixPrj(localMatrix);
            Curve partBoxInDefault = MatrixPrj.GetLocalBox(inversePrj, new Pnt(part.Width, part.Height), prj);

            if (extent == null)
            {
              extent = new Box(partBoxInDefault.Extent.Min.Clone(), partBoxInDefault.Extent.Max.Clone());
            }
            else
            {
              extent.Include(partBoxInDefault.Extent);
            }
          }
          _imageExtent = extent;
        }
        return _imageExtent;
      }
    }


    public string ConfigDir { get; set; }
    public string ImagePath { get; private set; }
    public string Name { get; set; }

    private List<Part> _parts;
    public IReadOnlyList<Part> Parts
    {
      get { return _parts ?? (_parts = new List<Part>(EnumParts())); }
    }

    private IEnumerable<Part> EnumParts()
    {
      foreach (string imagePath in EnumImagePathes())
      {
        yield return InitPart(imagePath);
      }
    }

    private double[] _defaultWorldMatrix;
    public double[] DefaultWorldMatrix => _defaultWorldMatrix ?? (_defaultWorldMatrix = GetDefaultWorldMatrix());
    private double[] GetDefaultWorldMatrix()
    {
      foreach (var imagePath in EnumImagePathes())
      {
        double[] mat = GetWorldMatrix(imagePath);
        if (mat != null)
        { return mat; }
      }
      return null;
    }

    private IEnumerable<string> EnumImagePathes()
    {
      foreach (string path in Directory.EnumerateFiles(ConfigDir ?? string.Empty, ImagePath, SearchOption.TopDirectoryOnly))
      {
        yield return path;
      }
    }

    [Obsolete("refactor")]
    public MatrixPrj GetImagePrj(MatrixPrj worldPrj, int width, int height, out int nx, out int ny)
    {
      IBox imgExt = ImageExtent;
      int x0 = (int)Math.Floor(imgExt.Min.X);
      int x1 = (int)Math.Ceiling(imgExt.Max.X);
      int y0 = (int)Math.Floor(imgExt.Min.Y);
      int y1 = (int)Math.Ceiling(imgExt.Max.Y);

      nx = x1 - x0;
      ny = y1 - y0;

      MatrixPrj imagePrj;
      if (nx * ny < MaxDim * MaxDim)
      {
        double[] m = DefaultWorldMatrix;
        MatrixPrj prj = new MatrixPrj(m);
        double[] orig = prj.Project(x0, y0);
        double[] imageMat = new double[] { m[0], m[1], m[2], m[3], orig[0], orig[1] };
        imagePrj = new MatrixPrj(imageMat);
      }
      else
      {

        // TODO : adapt nx, ny
        double[] m = DefaultWorldMatrix;
        MatrixPrj prj = new MatrixPrj(m);

        if (worldPrj != null)
        {
          double[] center = worldPrj.Project(width / 2, height / 2);
          double[] imageCenter = prj.GetInverse().Project(center);

          if (imageCenter[0] + MaxDim / 2 > x1)
          { x0 = Math.Max(x0, x1 - MaxDim); }
          else
          { x0 = Math.Max(x0, (int)imageCenter[0] - MaxDim / 2); }

          if (imageCenter[1] + MaxDim / 2 > y1)
          { y0 = Math.Max(y0, y1 - MaxDim); }
          else
          { y0 = Math.Max(y0, (int)imageCenter[1] - MaxDim / 2); }
        }

        double[] orig = prj.Project(x0, y0);
        double[] imageMat = new double[] { m[0], m[1], m[2], m[3], orig[0], orig[1] };
        imagePrj = new MatrixPrj(imageMat);

        nx = Math.Min(nx, MaxDim);
        ny = Math.Min(ny, MaxDim);
      }
      return imagePrj;
    }

    public static GeoImage Create(XmlImage xmlImage)
    {
      GeoImage image = new GeoImage();
      image.ImagePath = xmlImage.Path;
      image.Name = xmlImage.Name;
      return image;
    }

    public static List<GeoImageViews> CreateImages(IList<XmlImage> xmlImages, string configDir)
    {
      if (xmlImages == null)
      { return null; }
      List<GeoImageViews> images = new List<GeoImageViews>();
      foreach (XmlImage xml in xmlImages)
      {
        GeoImage img = Create(xml);
        img.ConfigDir = configDir;
        images.Add(new GeoImageViews(img));
      }
      return images;
    }
    public static double[] GetWorldMatrix(string imagePath)
    {
      string worldFile = GetWorldPath(imagePath);

      if (worldFile == null || !File.Exists(worldFile))
      { return null; }

      using (TextReader r = new StreamReader(worldFile))
      {
        if (!double.TryParse(r.ReadLine(), out double x00)) return null;
        if (!double.TryParse(r.ReadLine(), out double x01)) return null;
        if (!double.TryParse(r.ReadLine(), out double x10)) return null;
        if (!double.TryParse(r.ReadLine(), out double x11)) return null;
        if (!double.TryParse(r.ReadLine(), out double dx)) return null;
        if (!double.TryParse(r.ReadLine(), out double dy)) return null;

        return new double[] { x00, x01, x10, x11, dx, dy };
      }
    }

    public static string GetWorldPath(string imagePath)
    {
      if (string.IsNullOrEmpty(imagePath))
      { return null; }

      string ext = Path.GetExtension(imagePath);
      if (string.IsNullOrEmpty(ext) || ext.Length < 3)
      { return null; }

      string worldExt = $"{ext.Substring(0, 2)}{ext[ext.Length - 1]}w";
      string worldFile = Path.ChangeExtension(imagePath, worldExt);
      return worldFile;
    }

  }
}
