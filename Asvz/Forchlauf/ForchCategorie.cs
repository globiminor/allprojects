
namespace Asvz.Forchlauf
{
  public class ForchCategorie : Categorie
  {
    private Kategorie _cat;
    public ForchCategorie(Kategorie kategorie)
    {
      _cat = kategorie;
    }

    public Kategorie Kategorie
    {
      get { return _cat; }
    }
    public override string KmlStyle
    {
      get { return _cat.ToString(); }
    }
    public override string Name
    {
      get { return _cat.ToString(); }
    }
  }
}
