
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cards
{
    [DataContract]
    public class Suite
    {
        [Obsolete("used for serializing")]
        public Suite()
        {            
        }

        public Suite(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static IEnumerable<Suite> Suites
        {
            get
            {
                yield return new Suite("H", "Herz");
                yield return new Suite("E", "Ecken");
                yield return new Suite("K", "Kreuz");
                yield return new Suite("S", "Schaufeln");
            }
        }

        [DataMember]
        public string Code { get; private set; }
        [DataMember]
        public string Name { get; private set; }

        public List<Card> CreateCards()
        {
            return CreateCards(Height.Heights);
        }

        public List<Card> CreateCards(IEnumerable<Height> heights)
        {
            List<Card> cards = new List<Card>();
            foreach (var height in heights)
            {
                cards.Add(new Card(this, height));
            }
            return cards;
        }
    }
}
