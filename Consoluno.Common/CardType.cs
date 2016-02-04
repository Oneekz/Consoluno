using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consoluno.Common
{
    public enum CardType
    {
        None, Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9, Reverse, Skip, Draw2, Draw4, Wild
    }

    public static class ExtensionClass
    {
        public static string toString(this CardType type)
        {
            switch (type)
            {
                case CardType.Reverse:
                    return "reverse";
                case CardType.Skip:
                    return "skip";
                case CardType.Draw2:
                    return "draw +2";
                case CardType.Draw4:
                    return "draw +4";
                case CardType.Wild:
                    return "wild";
                default:
                    return type.ToString().Last().ToString();
            }
        }
    }
}
