
namespace Asvz.SolaDuo
{
  class DuoCategorie : Categorie
  {
    public DuoCategorie(Data data)
    {
      Data = data;
    }
    public override string Name
    {
      get { return Kategorie.Strecke.ToString(); }
    }

    public override string KmlStyle
    {
      get { return Name; }
    }
  }
}
