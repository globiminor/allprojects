
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cards
{
    [DataContract]
    public class Height
    {
        [Obsolete("used for serializing")]
        public Height()
        {
        }

        public Height(int h, string code)
        {
            H = h;
            Code = code;
        }

        public static IEnumerable<Height> Heights
        {
            get
            {
                yield return new Height(1, "A");
                yield return new Height(2, "2");
                yield return new Height(3, "3");
                yield return new Height(4, "4");
                yield return new Height(5, "5");
                yield return new Height(6, "6");
                yield return new Height(7, "7");
                yield return new Height(8, "8");
                yield return new Height(9, "9");
                yield return new Height(10, "10");
                yield return new Height(11, "B");
                yield return new Height(12, "Q");
                yield return new Height(13, "K");
            }
        }

        [DataMember]
        public int H { get; private set; }
        [DataMember]
        public string Code { get; private set; }
    }
}
