using System;

namespace Asvz.Sola
{
  public class SolaStrecke : Strecke, IComparable<SolaStrecke>
  {
    private int _nummer;
    private readonly string _vorlage;

    public SolaStrecke(int nummer, string vorlage)
    {
      _nummer = nummer;
      _vorlage = vorlage;
    }

    public int Nummer
    {
      get { return _nummer; }
    }
    public string Vorlage
    {
      get { return _vorlage; }
    }

    public override string Name(Categorie cat)
    {
      return "Strecke " + _nummer;
    }

    public SolaCategorie GetCategorie(Kategorie kat)
    {
      foreach (var c in Categories)
      {
        SolaCategorie cat = (SolaCategorie)c;
        if ((cat.Typ & kat) == kat)
        {
          return cat;
        }
      }
      return null;
    }

    public int CompareTo(SolaStrecke other)
    {
      return this._nummer - other._nummer;
    }
  }
}
