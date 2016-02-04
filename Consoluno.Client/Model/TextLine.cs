using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace Consoluno.Client.Model
{
    public class TextLine : ViewModelBase
    {
        private bool hidden = false;
        private SolidColorBrush color;

        public SolidColorBrush Color
        {
            get
            {
                if (!Hidden)
                    return color;
                return Brushes.White;
            }
            set { color = value; }
        }

        public String Text { get; set; }

        public bool Hidden
        {
            get { return hidden; }
            set
            {
                hidden = value;
                RaisePropertyChanged(() => Color);
            }
        }

        public TextLine(string text)
        {
            this.Text = text;
            this.Color = Brushes.White;
        }

        public TextLine(string text, SolidColorBrush color)
        {
            this.Text = text;
            this.Color = color;
        }

        public static TextLine Create(string text)
        {
            return new TextLine(text);
        }

        public static TextLine Create(string text, SolidColorBrush color)
        {
            return new TextLine(text, color);
        }

    }
}
