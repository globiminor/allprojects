using System;
using System.Collections.Generic;
using System.Text;

namespace Asvz.Sola
{
  [Flags]
  public enum Kategorie
  {
    Default = 1,
    Damen = 2,
    Herren = 4,
    Senioren = 8,
    Alle = 15
  }

  public class SolaCategorie : Categorie
  {
    private Kategorie _typ;
    private string _stufe;
    private double _offsetStart;
    private double _offsetEnd;

    public SolaCategorie(Kategorie typ, double? distance, double offsetStart, double offsetEnd, string stufe)
      : base(distance)
    {
      _typ = typ;
      _stufe = stufe;

      _offsetStart = offsetStart;
      _offsetEnd = offsetEnd;
    }

    public string Stufe
    {
      get { return _stufe; }
    }

    public Kategorie Typ
    {
      get { return _typ; }
    }

    public override double OffsetStart
    {
      get { return _offsetStart; }
    }

    public override double OffsetEnd
    {
      get { return _offsetEnd; } 
    }

    public override string Name
    {
      get { return _typ.ToString(); }
    }

    public override string KmlStyle
    {
      get
      {
        if ((Typ & Kategorie.Default) == Kategorie.Default)
        { return Stufe; }
        else
        { return "special"; }
      }
    }
  }
}
