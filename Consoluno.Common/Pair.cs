using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Consoluno.Common
{
    [DataContract]
    public class Pair<T1, T2>
    {
        [DataMember]
        public T1 Item1 { get; set; }
        [DataMember]
        public T2 Item2 { get; set; }

        public Pair(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }

    [DataContract]
    public class Pair<T1, T2, T3>
    {
        [DataMember]
        public T1 Item1 { get; set; }
        [DataMember]
        public T2 Item2 { get; set; }
        [DataMember]
        public T3 Item3 { get; set; }

        public Pair(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }


    public static class Pair
    {
        public static Pair<T1, T2> Create<T1,T2>(T1 item1, T2 item2)
        {
            return new Pair<T1, T2>(item1, item2);
        }
        public static Pair<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Pair<T1, T2, T3>(item1, item2, item3);
        }
    }


}
