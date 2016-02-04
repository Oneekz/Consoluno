using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Consoluno.Common;
using GalaSoft.MvvmLight;

namespace Consoluno.Client.ViewModel
{
    public class CardViewModel : ViewModelBase
    {
        private readonly Card card;
        private bool hidden = false;

        public CardViewModel(Card card)
        {
            this.card = card;
        }

        public string ColorName
        {
            get { return card.Color.ToString(); }
        }

        public SolidColorBrush DisplayColor
        {
            get
            {
                if (Hidden)
                    return Brushes.White;
                switch (card.Color)
                {
                    case CardColor.Blue:
                        return Brushes.LightSkyBlue;
                    case CardColor.Red:
                        return Brushes.LightCoral;
                    case CardColor.Green:
                        return Brushes.LightGreen;
                    case CardColor.Yellow:
                        return Brushes.Yellow;
                    default:
                        return Brushes.White;
                }
            }
        }

        public string DisplayText
        {
            get
            {
                var res = String.Empty;
                switch (card.Type)
                {
                    case CardType.Reverse:
                        res = "reverse";
                        break;
                    case CardType.Skip:
                        res = "skip";
                        break;
                    case CardType.Draw2:
                        res = "draw+2";
                        break;
                    case CardType.Draw4:
                        res = "draw+4";
                        break;
                    case CardType.Wild:
                        res = "wild";
                        break;
                    default:
                        res = card.Type.ToString().Last().ToString();
                        break;
                }
                if (!hidden) return res;
                return String.Format("[{0} {1}]", res, Color.ToString().ToLower()[0]);
            }
        }

        public CardType Type
        {
            get { return card.Type; }
        }

        public CardColor Color
        {
            get { return card.Color; }
        }

        public Card CardModel
        {
            get { return card; }
        }
        public static CardType ParseCardType(string type)
        {
            type = type.Trim().ToLower();
            if (type.Length == 1)
            {
                var c = type[0];
                if (c >= '0' && c <= '9')
                {
                    CardType result;
                    if (Enum.TryParse("Number" + c, false, out result))
                    {
                        return result;
                    }
                }
            }
            if (type == "reverse" || type == "r" || type == "re")
                return CardType.Reverse;
            if (type == "skip" || type == "s" || type == "sk")
                return CardType.Skip;
            if (type == "+2" || type == "dr+2" || type == "draw+2")
                return CardType.Draw2;
            if (type == "+4" || type == "dr+4" || type == "draw+4")
                return CardType.Draw4;
            if (type == "wild" || type == "w" || type == "wi")
                return CardType.Wild;
            return CardType.None;
        }

        public static CardColor ParseCardColor(string color)
        {
            color = color.Trim().ToLower();
            if (color == "red" || color == "r" || color == "re")
                return CardColor.Red;
            if (color == "blue" || color == "bl" || color == "b")
                return CardColor.Blue;
            if (color == "green" || color == "gr" || color == "g")
                return CardColor.Green;
            if (color == "yellow" || color == "yw" || color == "y")
                return CardColor.Yellow;
            return CardColor.None;
        }

        public bool Hidden
        {
            get { return hidden; }
            set
            {
                hidden = value;
                RaisePropertyChanged(() => DisplayColor);
                RaisePropertyChanged(() => DisplayText);
            }
        }
    }
}
