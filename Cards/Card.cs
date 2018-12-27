
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

    private static readonly Suite _unknownSuite = new Suite("U", "Undefined");
    private static readonly Height _unknownHeight = new Height(-1, "U");

    internal static Card CreateEmpty()
    {
      return new Card(_unknownSuite, _unknownHeight);
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
      if (Suite == null)
      { return "??"; }
      return string.Format("{0}{1}", Suite.Code, Height.Code);
    }

    public void Replace(Card newValues)
    {
      Suite = newValues.Suite;
      Height = newValues.Height;
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
      set
      {
        if (value.Length < 2)
        { throw new InvalidOperationException(value); }
        string suiteCode = value[0].ToString();
        string heightCode = value.Substring(1);

        foreach (var s in Suite.Suites)
        {
          if (s.Code == suiteCode)
          {
            Suite = s;
          }
        }
        foreach (var h in Height.Heights)
        {
          if (h.Code == heightCode)
          {
            Height = h;
          }
        }
      }
    }

    public class CardComparer : IComparer<Card>
    {
      public int Compare(Card x, Card y)
      { return Card.Compare(x, y); }
    }

    public static int Compare(Card x, Card y)
    {
      int d = x.Suite.Code.CompareTo(y.Suite.Code);
      if (d != 0)
      { return d; }

      d = x.Height.H.CompareTo(y.Height.H);
      return d;
    }
    public bool EqualsCard(Card other)
    {
      if (other == null)
      { return false; }

      return Compare(this, other) == 0;
    }

    public static void EmptyCards(List<Card> cards)
    {
      foreach (var card in cards)
      {
        card.Suite = _unknownSuite;
        card.Height = _unknownHeight;
      }
    }
    public static List<Card> Shuffle(List<Card> cards, Random r = null)
    {
      if (r == null)
      { r = new Random((int)DateTime.Now.Ticks); }

      List<int> randoms = new List<int>(cards.Count);
      Dictionary<int, Card> randomCards = new Dictionary<int, Card>(cards.Count);
      foreach (var card in cards)
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
      foreach (var idx in randoms)
      {
        shuffled.Add(randomCards[idx]);
      }
      return shuffled;
    }
  }
}
