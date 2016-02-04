using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Consoluno.Common
{
    [DataContract]
    public class ServiceAnswer<T>
    {
        [DataMember]
        public T Value { get; set; }
        [DataMember]
        public string Message { get; set; }

        public ServiceAnswer(T value, string message)
        {
            this.Value = value;
            this.Message = message;
        }

        public ServiceAnswer(T value) : this(value, String.Empty) { }
    }

    public static class ServiceAnswer
    {
        public static ServiceAnswer<T> Create<T>(T value, string message)
        {
            return new ServiceAnswer<T>(value, message);
        }

        public static ServiceAnswer<T> Create<T>(T value)
        {
            return new ServiceAnswer<T>(value);
        }
    }
}
