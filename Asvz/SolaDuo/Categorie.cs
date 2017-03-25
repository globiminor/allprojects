using System.Collections.Generic;
using Basics.Geom;

namespace Asvz
{
  public abstract class Categorie
  {
    private class StreckeTeil
    {
      private readonly Polyline _line;
      private readonly bool _onDtm;

      public StreckeTeil(Polyline line, bool onDtm)
      {
        _line = line;
        _onDtm = onDtm;
      }

      public Polyline Line
      {
        get { return _line; }
      }
      public bool OnDtm
      {
        get { return _onDtm; }
      }

      public Polyline Profil(Grid.IDoubleGrid dhm)
      {
        Polyline profil;
        if (_onDtm)
        {
          profil = Grid.DoubleGrid.Profil(dhm, _line);
        }
        else
        {
          IPoint p;
          p = _line.Points.First.Value;
          double h0 = dhm.Value(p.X, p.Y);
          p = _line.Points.Last.Value;
          double h1 = dhm.Value(p.X, p.Y);
          profil = Polyline.Create(new[] {
                new Point2D(0, h0),
                new Point2D(_line.Length(), h1)
          });
        }
        return profil;

      }
    }
    private double? _userLength;

    private Polyline _s;
    private List<StreckeTeil> _streckeTeilList;
    private Data _data;
    private double? _geomXyLength;

    private Polyline _profil;
    private Polyline _profilNormed;
    private double _steigung = -1;

    protected Categorie(double? userLength)
    {
      _userLength = userLength;
    }

    public abstract string Name { get; }
    public abstract string KmlStyle { get; }

    protected Categorie()
    {
      _userLength = null;
    }

    public double? UserLength
    {
      get { return _userLength; }
      set
      {
        _userLength = value;
        _profilNormed = null;
      }
    }
    public double DispLength
    {
      get { return _userLength ?? GeomXyLength; }
    }

    public Polyline Strecke
    {
      get
      {
        if (_s == null)
        {
          if (_streckeTeilList != null && _streckeTeilList.Count > 0)
          {
            _s = _streckeTeilList[0].Line.Clone();
            int n = _streckeTeilList.Count;
            for (int i = 1; i < n; i++)
            {
              foreach (Curve c in _streckeTeilList[i].Line.Segments)
              { _s.Add(c.Clone()); }
            }
          }

        }
        return _s;
      }
    }

    public Data Data
    {
      get { return _data; }
      protected set { _data = value; }
    }

    public void AddGeometry(Polyline teil, bool onDtm)
    {
      if (_streckeTeilList == null)
      { _streckeTeilList = new List<StreckeTeil>(); }

      _streckeTeilList.Add(new StreckeTeil(teil, onDtm));

      _s = null;
      _profil = null;
      _profilNormed = null;
    }

    public void SetGeometry(Polyline strecke, Data data)
    {
      _streckeTeilList = new List<StreckeTeil>();
      _streckeTeilList.Add(new StreckeTeil(strecke, true));
      _data = data;

      _s = null;
      _profil = null;
      _profilNormed = null;
    }

    private double GeomXyLength
    {
      get { return _geomXyLength ?? (_geomXyLength = Strecke.Project(Geometry.ToXY).Length()).Value; }
    }

    public double Faktor()
    {
      if (_userLength == null)
      { return 1; }

      return GeomXyLength / _userLength.Value;
    }

    public Polyline Profil
    {
      get
      {
        if (_profil == null)
        {
          foreach (StreckeTeil teil in _streckeTeilList)
          {
            Polyline teilProfil = teil.Profil(_data.Dhm);

            if (_profil == null)
            {
              _profil = teilProfil;
            }
            else
            {
              double x0 = _profil.Points.Last.Value.X;

              foreach (IPoint p in teilProfil.Points)
              {
                if (p.X > 0)
                { _profil.Add(new Point2D(x0 + p.X, p.Y)); }
              }
            }
          }
        }
        return _profil;
      }
    }
    public Polyline ProfilNormed
    {
      get
      {
        if (_profilNormed == null)
        {
          double faktor = Faktor();
          _profilNormed = new Polyline();
          foreach (IPoint p in Profil.Points)
          {
            _profilNormed.Add(new Point2D(p.X / faktor, p.Y));
          }
        }
        return _profilNormed;
      }
    }
    public double Steigung
    {
      get
      {
        if (_steigung < 0)
        {
          _steigung = 0;
          foreach (Curve seg in Profil.Segments)
          {
            double dH = seg.End.Y - seg.Start.Y;
            if (dH > 0)
            { _steigung += dH; }
          }
        }
        return _steigung;
      }
    }
    public double SteigungRound(double round)
    {
      return Basics.Utils.Round(Steigung, round);
    }

    public virtual double OffsetStart { get { return 0; } }
    public virtual double OffsetEnd { get { return 0; } }

    public double[] GetLineParams(double distance)
    {
      Polyline line = Strecke;

      double lengthGeom = GeomXyLength;
      double lengthMeas = DispLength;
      double lengthDisp = OffsetStart + lengthMeas + OffsetEnd;

      double f = lengthGeom / lengthDisp;

      double normedDist = (distance + OffsetStart) * f;
      double[] param = line.ParamAt(normedDist);

      return param;
    }
  }
}
