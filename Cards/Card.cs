
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cards
{
  [DataContract]
  public class Card
  {
    [Obsolete("used for serializing")]
    public Card()
    {
    }

    internal Card(string code)
    {
      Code = code;
    }

    public Card(Suite suite, Height height)
    {
      Suite = suite;
      Height = height;
    }

    public override string ToString()
    {
      return string.Format("{0}{1}", Suite.Code, Height.Code);
    }

    public int GetCardCode()
    {
      return Suite.Code.GetHashCode() ^ Height.Code.GetHashCode();
    }

    public Suite Suite { get; private set; }
    public Height Height { get; private set; }

    [DataMember]
    internal string Code
    {
      get { return ToString(); }
      set {
        if (value.Length < 2)
        { throw new InvalidOperationException(value); }
        string suiteCode = value[0].ToString();
        string heightCode = value.Substring(1);

        foreach (Suite s in Suite.Suites)
        {
          if (s.Code == suiteCode)
          {
            Suite = s;
          }
        }
        foreach (Height h in Height.Heights)
        {
          if (h.Code == heightCode)
          {
            Height = h;
          }
        }
      }
    }

    public bool EqualsCard(Card other)
    {
      if (other == null)
      { return false; }

      if (Suite.Code != other.Suite.Code)
      { return false; }

      if (Height.H != other.Height.H)
      { return false; }

      return true;
    }


    public static List<Card> Shuffle(List<Card> cards, Random r = null)
    {
      if (r == null) 
      { r = new Random((int)DateTime.Now.Ticks); }

      List<int> randoms = new List<int>(cards.Count);
      Dictionary<int, Card> randomCards = new Dictionary<int, Card>(cards.Count);
      foreach (Card card in cards)
      {
        int idx;
        do
        {
          idx = r.Next();
        }
        while (randomCards.ContainsKey(idx));
        randomCards.Add(idx, card);
        randoms.Add(idx);
      }
      randoms.Sort();
      List<Card> shuffled = new List<Card>(cards.Count);
      foreach (int idx in randoms)
      {
        shuffled.Add(randomCards[idx]);
      }
      return shuffled;
    }
  }
}
