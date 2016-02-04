using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Consoluno.Client.ViewModel
{
    public class UserViewModel :ViewModelBase
    {
        public SolidColorBrush Color { get; set; }
        public String UserName { get; set; }
        public RelayCommand SayUnoCommand { get; private set; }
        public int CardCount { get; set; }

        public string DisplayName
        {
            get { return String.Format("{0} ({1})", UserName, CardCount); }
        }

        public UserViewModel(string username, bool active, int cardCount, Action<string> sayUnoAction)
        {
            UserName = username;
            this.Color = (active) ? Brushes.White : Brushes.Gray;
            this.CardCount = cardCount;
            SayUnoCommand = new RelayCommand(() =>
            {
                if (sayUnoAction != null)
                    sayUnoAction(UserName);
            }, ()=>CardCount==1);
        }
    }
}
