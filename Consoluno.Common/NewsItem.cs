using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Consoluno.Common
{
    [DataContract]
    public class NewsItem
    {
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Command { get; set; }
        [DataMember]
        public bool IsCommand { get; set; }
        [DataMember]
        public Card Card { get; set; }
        

        public NewsItem(string message)
        {
            this.Message = message;
        }

        public NewsItem()
        {
            
        }

        public static NewsItem CreateCmd(string command)
        {
            return new NewsItem {IsCommand = true, Command = command};
        }


        public static NewsItem CreateCmd(string command,string message, params object[] args)
        {
            return new NewsItem(String.Format(message, args)) { IsCommand = true, Command = command };
        }

        public static NewsItem CreateMsg(string message, params object[] args)
        {
            return new NewsItem { IsCommand = false, Message = String.Format(message, args) };
        }

        public static NewsItem CreateCmd(string command, string message, Card card)
        {
            return new NewsItem(message) { IsCommand = true, Command = command, Card = card};
        }
    }
}
