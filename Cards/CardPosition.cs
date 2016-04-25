using System.Runtime.Serialization;

namespace Cards
{
  [DataContract]
  public class CardPosition
  {
    public Card Card { get; set; }
    [DataMember]
    public string CardCode
    {
      get { return Card.Code; }
      set { Card = new Card(value); }
    }

    [DataMember]
    public double Left { get; set; }
    [DataMember]
    public double Top { get; set; }
    [DataMember]
    public bool Visible { get; set; }

    public bool EqualsCardAndPos(CardPosition other)
    {
      if (other == null)
      { return false; }

      if (Visible != other.Visible)
      { return false; }

      if (Left != other.Left)
      { return false; }

      if (Top != other.Top)
      { return false; }

      if (!Card.EqualsCard(other.Card))
      { return false; }

      return true;
    }
  }
}
