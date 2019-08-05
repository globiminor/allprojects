using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OMapScratch
{
  public partial interface IGeoImage
  {
    double[] GetWorldMatrix();
    bool CheckUpdateImage(Func<MatrixPrj> getWorldMatrix, float width, float height);
  }

  public partial class GeoImageComb : IGeoImage
  {
    private readonly List<GeoImageView> _views;
    private readonly GeoImage _baseImage;

    private double[] _currentWorldMatrix;
    private IBox _currentImagePartExtent;

    public GeoImageComb(GeoImage baseImage)
    {
      _views = new List<GeoImageView>();
      _baseImage = baseImage;
    }

    public void Add(GeoImageView geoImage)
    {
      _views.Add(geoImage);
    }

    public bool CheckUpdateImage(Func<MatrixPrj> getWorldPrj, float width, float height)
    {
      return _baseImage.LoadNeeded(getWorldPrj, width, height, _currentImagePartExtent);
    }

    public IReadOnlyList<GeoImageView> Views => _views;

    public double[] GetWorldMatrix()
    {
      _currentWorldMatrix = _currentWorldMatrix ?? _baseImage.DefaultWorldMatrix;
      return _currentWorldMatrix;
    }
  }

  public partial class GeoImageView : IGeoImage
  {
    public GeoImage BaseImage { get; }
    private double[] _currentWorldMatrix;
    private float _transparency;
    private float[] _colorTransform;
    private IBox _currentImagePartExtent;

    public GeoImageView(GeoImage baseImage)
    {
      BaseImage = baseImage;
    }

    public double[] GetWorldMatrix()
    {
      _currentWorldMatrix = _currentWorldMatrix ?? BaseImage.DefaultWorldMatrix;
      return _currentWorldMatrix;
    }

    public float Transparency
    {
      get => _transparency;
      set
      {
        _transparency = Math.Max(0, Math.Min(1, value));
        _colorTransform = null;
      }
    }

    private int _gray;
    public int Gray
    {
      get => _gray;
      set
      {
        _gray = value;
        _colorTransform = null;
      }
    }

    private int _colorRot;
    public int ColorRotation
    {
      get => _colorRot;
      set
      {
        _colorRot = value;
        _colorTransform = null;
      }
    }


    public string ColorTransformText
    {
      get => string.Concat(_colorTransform?.Select(x => $"{x},")).Trim(',');
      set
      {
        _colorTransform = null;
        if (string.IsNullOrWhiteSpace(value))
          return;
        IList<string> parts = value.Split(',');
        if (parts.Count == 0)
          return;
        if (parts.Count == 1)
        {
          _transparency = float.Parse(parts[0]);
          return;
        }
        if (parts.Count != 20)
        {
          return;
        }
        List<float> colorTransform = new List<float>();
        foreach (var part in parts)
        {
          colorTransform.Add(float.Parse(part));
        }
        _colorTransform = colorTransform.ToArray();
      }
    }

    [Obsolete("implement correctly")]
    public bool CheckUpdateImage(Func<MatrixPrj> getWorldPrj, float width, float height)
    {
      return BaseImage.LoadNeeded(getWorldPrj, width, height, _currentImagePartExtent);
    }

  }

  public interface IGeoImagesContainer
  {
    void Invalidate();
  }
  public partial class GeoImageVm : Basics.ViewModels.BaseVm
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

    private int _gray;
    public int Gray
    {
      get => _gray;
      set
      {
        if (SetStruct(ref _gray, value))
        {
          Changed(null);
          Container?.Invalidate();
        }
      }
    }
    public bool GrayEnabled => ColorRotation <= 0;

    private int _colorRot;
    public int ColorRotation
    {
      get => _colorRot;
      set
      {
        if (SetStruct(ref _colorRot, value))
        {
          Changed(null);
          Container?.Invalidate();
        }
      }
    }
    public bool ColorRotationEnabled => Gray <= 0;

    public GeoImageComb Save()
    {
      if (_container == null || _combinations == null)
      { return null; }

      GeoImageComb comb = new GeoImageComb(_baseImage);
      GeoImageView baseImg = new GeoImageView(_baseImage);
      baseImg.Transparency = 1 - Opacity / 100.0f;
      baseImg.Gray = Gray;
      baseImg.ColorRotation = ColorRotation;
      comb.Add(baseImg);
      foreach (var img in _combinations)
      {
        if (!img.Visible || img.Opacity <= 0)
        { continue; }

        GeoImageView part = new GeoImageView(img.BaseImage);
        part.Transparency = 1 - img.Opacity / 100.0f;
        part.Gray = img.Gray;
        part.ColorRotation = img.ColorRotation;
        comb.Add(part);
      }
      _container.Replace(_editView, comb);
      return comb;
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

    public bool LoadNeeded(Func<MatrixPrj> getTargetPrj, float width, float height, IBox loadedExtent)
    {
      if (loadedExtent == null)
      { return true; }

      IBox imgExt = ImageExtent;
      double x0 = Math.Floor(imgExt.Min.X);
      double x1 = Math.Ceiling(imgExt.Max.X);
      double y0 = Math.Floor(imgExt.Min.Y);
      double y1 = Math.Ceiling(imgExt.Max.Y);

      int nx = (int)(x1 - x0);
      int ny = (int)(y1 - y0);

      if (nx * ny < MaxDim * MaxDim)
      { return false; } // All loaded

      MatrixPrj targetPrj = getTargetPrj();
      Pnt targetMax = new Pnt(width, height);

      double[] m = targetPrj.Matrix;
      double[] localMatrix = new double[] { m[0], -m[1], -m[2], m[3], 0, 0 };

      double[] p = DefaultWorldMatrix;
      double[] localPartMatrix = new double[] { p[0], -p[1], -p[2], p[3], p[4] - m[4], p[5] - m[5] };
      MatrixPrj localTargetPrj = new MatrixPrj(localMatrix);

      MatrixPrj partPrj = new MatrixPrj(localPartMatrix);
      MatrixPrj invPartPrj = partPrj.GetInverse();

      Curve targetBoxInPart = MatrixPrj.GetLocalBox(invPartPrj, targetMax, localTargetPrj);
      IBox extent = targetBoxInPart.Extent;
      double rX0 = Math.Max(x0, extent.Min.X);
      double rY0 = Math.Max(y0, extent.Min.Y);
      double rX1 = Math.Min(x1, extent.Max.X);
      double rY1 = Math.Min(y1, extent.Max.Y);
      if (rX0 < loadedExtent.Min.X || rY0 < loadedExtent.Min.Y
        || rX1 > loadedExtent.Max.X || rY1 > loadedExtent.Max.Y)
      {
        return true;
      }

      // handle resampled images
      {
        IBox l = loadedExtent;
        double dxLoaded = l.Max.X - l.Min.X;
        double dyLoaded = l.Max.Y - l.Min.Y;

        if (dxLoaded <= MaxDim && dyLoaded <= MaxDim)
        {
          return false;
        }

        IBox e = extent;
        double dxNeeded = e.Max.X - e.Min.X;
        double dyNeeded = e.Max.Y - e.Min.Y;
        if (dxNeeded <= MaxDim && dyNeeded <= MaxDim)
        {
          return true;
        }
      }

      return false;
    }

    [Obsolete("refactor")]
    public MatrixPrj GetImagePrj(MatrixPrj worldPrj, int width, int height, out int nx, out int ny, out IBox imgPartExtent)
    {
      IBox imgExt = ImageExtent;
      int x0 = (int)Math.Floor(imgExt.Min.X);
      int x1 = (int)Math.Ceiling(imgExt.Max.X);
      int y0 = (int)Math.Floor(imgExt.Min.Y);
      int y1 = (int)Math.Ceiling(imgExt.Max.Y);

      nx = x1 - x0;
      ny = y1 - y0;

      MatrixPrj imagePrj;
      int maxDim2 = MaxDim * MaxDim;
      double[] m = DefaultWorldMatrix;
      double[] orig;
      double[] imageMat;
      MatrixPrj prj = new MatrixPrj(m);

      if (nx * ny <= maxDim2)
      {
        orig = prj.Project(x0, y0);
        imageMat = new double[] { m[0], m[1], m[2], m[3], orig[0], orig[1] };
        imagePrj = new MatrixPrj(imageMat);

        imgPartExtent = imgExt;
        return imagePrj;
      }

      int resample = 1;

      if (worldPrj != null)
      {
        double[] ul = worldPrj.Project(0, 0);
        double[] center = worldPrj.Project(width / 2, height / 2);

        var invPrj = prj.GetInverse();
        double[] imageCenter = invPrj.Project(center);
        double[] imageUl = invPrj.Project(ul);
        double dx_2 = imageUl[0] - imageCenter[0];
        double dy_2 = imageUl[1] - imageCenter[1];
        double dd_2 = (dx_2 * dx_2 + dy_2 * dy_2);
        while (dd_2 > maxDim2 / 4 && nx * ny > maxDim2)
        {
          nx = (nx + 1) / 2;
          ny = (ny + 1) / 2;
          dd_2 = dd_2 / 4;
          resample *= 2;
        }
        if (nx * ny <= maxDim2)
        {
          orig = prj.Project(x0, y0);
          imageMat = new double[] { m[0] * resample, m[1] * resample, m[2] * resample, m[3] * resample, orig[0], orig[1] };
          imagePrj = new MatrixPrj(imageMat);

          imgPartExtent = imgExt;
          return imagePrj;
        }


        if (imageCenter[0] + MaxDim * resample / 2 > x1)
        { x0 = Math.Max(x0, x1 - MaxDim * resample); }
        else
        { x0 = Math.Max(x0, (int)imageCenter[0] - MaxDim * resample / 2); }

        if (imageCenter[1] + MaxDim * resample / 2 > y1)
        { y0 = Math.Max(y0, y1 - MaxDim * resample); }
        else
        { y0 = Math.Max(y0, (int)imageCenter[1] - MaxDim * resample / 2); }
      }

      orig = prj.Project(x0, y0);
      imageMat = new double[] { m[0] * resample, m[1] * resample, m[2] * resample, m[3] * resample, orig[0], orig[1] };
      imagePrj = new MatrixPrj(imageMat);

      nx = Math.Min(nx, MaxDim);
      ny = Math.Min(ny, MaxDim);

      imgPartExtent = new Box(new Pnt(x0, y0), new Pnt(x0 + nx, y0 + ny));

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
      Dictionary<string, GeoImageViews> baseImages = new Dictionary<string, GeoImageViews>();
      foreach (var xml in xmlImages)
      {
        GeoImage img = Create(xml);
        img.ConfigDir = configDir;
        GeoImageViews views = new GeoImageViews(img);
        images.Add(views);
        baseImages[img.Name] = views;
      }
      foreach (var xml in xmlImages)
      {
        if (xml.Kombinations == null)
        { continue; }
        GeoImageViews views = baseImages[xml.Name];
        if (views.BaseImage.ImagePath != xml.Path)
        { continue; }
        foreach (var xmlKomb in xml.Kombinations)
        {
          if (xmlKomb.Parts == null)
          { continue; }
          GeoImageComb view = new GeoImageComb(views.BaseImage);
          foreach (var xmlPart in xmlKomb.Parts)
          {
            if (!baseImages.TryGetValue(xmlPart.Name, out GeoImageViews baseImage))
            {
              view = null;
              break;
            }
            view.Add(new GeoImageView(baseImage.BaseImage) { ColorTransformText = xmlPart.ColorTransform });
          }
          if (view == null)
          { continue; }

          views.Replace(null, view);
        }
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
