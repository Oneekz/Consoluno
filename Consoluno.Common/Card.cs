using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Consoluno.Common
{
    [DataContract]
    public class Card
    {
        private CardType type;
        private CardColor color = CardColor.White;

        public Card(CardType type)
        {
            this.Type = type;
        }

        public Card(CardType type, CardColor color) : this(type)
        {
            this.Color = color;
        }

        [DataMember]
        public CardType Type
        {
            get { return type; }
            set { type = value; }
        }

        [DataMember]
        public CardColor Color
        {
            get { return color; }
            set { color = value; }
        }

        [DataMember]
        public string ZeroCardUserName { get; set; }

        public bool IsUnicolored
        {
            get { return this.Type == CardType.Wild || this.Type == CardType.Draw4; }
        }

        public bool IsDraw
        {
            get { return this.Type == CardType.Draw2 || this.Type == CardType.Draw4; }
        }

        public bool IsNumeric { get
        {
            return this.Type == CardType.Number0 || this.Type == CardType.Number1 || this.Type == CardType.Number2 ||
                   this.Type == CardType.Number3 || this.Type == CardType.Number4 || this.Type == CardType.Number5 ||
                   this.Type == CardType.Number6 || this.Type == CardType.Number7 || this.Type == CardType.Number8 ||
                   this.Type == CardType.Number9;
            /* return this.Type != CardType.Draw2 || this.Type != CardType.Draw4 || this.Type != CardType.Reverse ||
                   this.Type != CardType.Skip || this.Type != CardType.Wild;*/
        } }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var card = obj as Card;
            if (card == null)
                return false;
            if (card.Type != this.Type)
                return false;
            //compare non-colored cards
            if (this.IsUnicolored)
            {
                return true; 
            }
            return card.Color == this.Color;
        }

        public bool CanBeBefore(Card upper)
        {
            if (upper.IsUnicolored)
                return true;
            if (upper.Type == this.Type)
                return true;
            if (upper.Color == this.Color)
                return true;
            return false;
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", this.Type, this.Color);
        }

        public int Weight
        {
            get
            {
                if (this.IsUnicolored)
                    return 50;
                if (!this.IsNumeric)
                    return 20;
                return Int16.Parse(type.ToString().Last().ToString());
            }
        }
    }
}
