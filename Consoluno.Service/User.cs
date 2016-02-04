using System;
using System.Collections.Generic;
using System.Linq;
using Consoluno.Common;

namespace Consoluno.Service
{
    public class User
    {
        private List<NewsItem> commandList = new List<NewsItem>();

        public string UserName { get; private set; }
        public Guid Token { get; private set; }
        public List<Card> Cards { get; set; }
        public int OrderId { get; set; }
        public bool Ready { get; set; }
        public bool UnoSaid { get; set; }
        public DateTime LastAccessTime { get; set; }

        public List<NewsItem> CommandList
        {
            get { return commandList; }
            set { commandList = value; }
        }
        

        public User(string username, int orderId)
        {
            this.UserName = username;
            this.Token = Guid.NewGuid();
            this.Cards = new List<Card>(20);
            this.OrderId = orderId;
        }

        public override string ToString()
        {
            return String.Format("({0}){1}, toke={2}", OrderId, UserName, Token);
        }

        public int CardsWeight
        {
            get
            {
                return Cards.Select(c => c.Weight).Sum();
            }
        }
    }
}
