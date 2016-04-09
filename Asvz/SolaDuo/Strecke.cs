using System.Collections.Generic;

namespace Asvz
{
  public class Strecke
  {
    private List<Categorie> _categories;

    public Strecke()
    {
      _categories = new List<Categorie>();
    }

    public IList<Categorie> Categories
    {
      get { return _categories; }
    }

    public virtual string Name(Categorie cat)
    {
      return cat.Name;
    }
  }
}
