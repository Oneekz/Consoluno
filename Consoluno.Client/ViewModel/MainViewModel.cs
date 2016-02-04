using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Consoluno.Client.Properties;
using Consoluno.Client.UnoServiceReference;
using Consoluno.Common;
using Consoluno.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Threading.Tasks;
using System.Windows.Media;
using Consoluno.Client.Helpers;
using Consoluno.Client.Model;
using GalaSoft.MvvmLight.Threading;
using System.Windows;

namespace Consoluno.Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private IUnoService service;
        private ObservableCollection<CardViewModel> currentCards = new ObservableCollection<CardViewModel>();
        private ObservableCollection<TextLine> lines = new ObservableCollection<TextLine>();
        private Guid token = Guid.Empty;
        private string commandString;
        private object sync = new object();
        private List<UserViewModel> userList;
        private string windowTitle = "ConsolUno";
        private string userName = String.Empty;
        private Card lastCard;
        private Brush inputLineBackground = Brushes.Black;
        private bool hiddenMode;
        private GameState gameIsRunnning = GameState.Stopped;
        private string lastCommand = String.Empty;
        private FlashWindowHelper flasher;
        private bool myTurn;

        #endregion

        #region Ctor

        public MainViewModel()
        {
            this.Service = new UnoServiceClient();
            PushCommand = new RelayCommand(OnPushCommand);
            LastCommadCommand = new RelayCommand(OnLastCommadCommand);
            CheckConnection();
            flasher = new FlashWindowHelper(Application.Current);
        }


        #endregion

        #region WriteMethods

        private void WriteInfo(string text, params object[] args)
        {
            WriteMessage(Brushes.White, text, args);
        }

        private void WriteError(string text, params object[] args)
        {
            WriteMessage(Brushes.Red, text, args);
        }

        private void WriteWarning(string text, params object[] args)
        {
            WriteMessage(Brushes.Orange, text, args);
        }

        private void WriteMessage(SolidColorBrush color, string text, params object[] args)
        {
            var tl = TextLine.Create(String.Format(text, args), color);
            tl.Hidden = hiddenMode;
            DispatcherHelper.CheckBeginInvokeOnUI(() => OutputData.Add(tl));
        }

        #endregion

        #region Output commands

        private void OnPushCommand()
        {
            if (String.IsNullOrWhiteSpace(CommandString))
                return;
            lastCommand = CommandString;
            var spaceIndex = CommandString.IndexOf(' ');
            string cmd;
            string args = String.Empty;
            if (spaceIndex >= 0)
            {
                cmd = CommandString.Substring(0, spaceIndex).ToLower();
                args = CommandString.Substring(spaceIndex + 1);
            }
            else
            {
                cmd = CommandString.ToLower();
                args = String.Empty;
            }
            switch (cmd)
            {
                case "register":
                case "reg":
                    RegisterUser(args);
                    break;
                case "start":
                case "begin":
                    StartGame();
                    break;
                case "put":
                    PutCard(args);
                    break;
                case "take":
                case "t":
                    TakeCard();
                    break;
                case "help":
                    WriteInfo("Availble commands: register, start, put, take, message, ready, hidden, uno");
                    break;
                case "message":
                case "mes":
                    SendMessage(args);
                    break;
                case "hidden":
                case "hid":
                case "hide":
                case "hi":
                case "stealth":
                    ChangeHiddenMode();
                    break;
                case "ready":
                case "vote":
                    VoteForStart();
                    break;
                case "uno":
                    SayUno(args);
                    break;
                case "bot":
                    AddBot(args);
                    break;
                case "shuffle":
                    ShuffleUsers(args);
                    break;
                default:
                    PutCard(CommandString);
                    break;
            }
            CommandString = String.Empty;
        }

        private void ShuffleUsers(string args)
        {
            Task.Run(async () =>
            {
                var res = await Service.ShuffleUserAsync(token);
                if(!res.Value)
                    WriteInfo(res.Message);
            });
        }

        private void AddBot(string args)
        {
            Task.Run(async () =>
            {
                var res = await Service.AddBotAsync(token, args);

            });
        }

        private void SayUno(string args)
        {
            if (String.IsNullOrWhiteSpace(args))
                SayUnoForMyself();
            else
                SayUnoFor(args);
        }

        private void SayUnoFor(string args)
        {
            Task.Run(async () =>
            {
                var res = await Service.SayUnoAsync(Token, args);
                if (!res.Value)
                    WriteInfo(res.Message);
            });
        }

        private void SayUnoForMyself()
        {
            Task.Run(async () =>
            {
                var res = await Service.SayUnoForMyselfAsync(Token);
                if (!res.Value)
                    WriteInfo(res.Message);
            });
        }

        private void VoteForStart()
        {
            Task.Run(async () =>
            {
                var res = await Service.VoteForStartAsync(Token);
                if (!res.Value)
                    WriteInfo(res.Message);
            });
        }

        private void ChangeHiddenMode()
        {
            hiddenMode = !hiddenMode;

            foreach (var textLine in OutputData)
            {
                textLine.Hidden = hiddenMode;
            }

            foreach (var card in CurrentCards)
            {
                card.Hidden = hiddenMode;
            }

        }

        private void SendMessage(string args)
        {
            Task.Run(async () =>
            {
                var res = await Service.WriteMessageAsync(Token, args);
                if (!res.Value)
                    WriteInfo(res.Message);
            });
        }

        private void RegisterUser(string username)
        {
            if (!Token.Equals(Guid.Empty))
            {
                WriteWarning("You are already registered. You cannot do it again");
                return;
            }
            Task.Run(async () =>
            {
                try
                {
                    var ans = await Service.RegisterUserAsync(username);
                    if (ans.Value.Equals(Guid.Empty))
                        WriteWarning(ans.Message);
                    else
                    {
                        InitializeMonitoring(username, ans.Value);
                    }
                }
                catch (Exception ex)
                {
                    WriteError(ex.Message);
                }
            });

        }

        private void InitializeMonitoring(string username, Guid token)
        {
            Token = token;
            StartMonitoring();
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                WindowTitle = String.Format("ConsolUno: {0}", username);
                this.userName = username;
                CurrentCards.Clear();
            });
        }

        private void StartGame()
        {
            Task.Run(async () =>
            {
                try
                {
                    var res = await Service.StartGameAsync(Token);
                    if (!res.Value)
                        WriteInfo(res.Message);

                }
                catch (Exception ex)
                {
                    WriteError(ex.Message);
                }
            });
        }

        private void PutCard(string s)
        {
            var parts = s.Split(' ');
            if (parts.Length == 1)
                PutCardProvidedOnlyType(s);
            else if (parts.Length == 2)
                PutCardProvidedTypeAndColor(parts[0], parts[1]);
            else if (parts.Length == 3)
                PutCardProvided3Params(parts[0], parts[1], parts[2]);
        }


        private void TakeCard()
        {
            Task.Run(async () =>
            {
                var card = await Service.TakeCardAsync(Token);
                if (!card.Value)
                    WriteInfo(card.Message);
            });
        }

        #endregion

        #region Input commands

        private void StartMonitoring()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!Token.Equals(Guid.Empty))
                {
                    var commands = await Service.GetCommandsToDoAsync(Token);
                    if (commands.Value != null && commands.Value.Length > 0)
                    {
                        foreach (var newsItem in commands.Value)
                        {
                            ProcessNewsItem(newsItem);
                        }
                    }
                    await Task.Delay(500);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void ProcessNewsItem(NewsItem newsItem)
        {
            if (!newsItem.IsCommand)
                WriteInfo(newsItem.Message);
            else
            {
                switch (newsItem.Command)
                {
                    case "ReadUsers":
                        ReadUsers();
                        break;
                    case "ReadCards":
                        ReadCards();
                        break;
                    case "GameOver":
                        OverGame(newsItem);
                        break;
                    case "CardPlaced":
                        ShowCardPlaced(newsItem);
                        break;
                    case "Message":
                        WriteMessage(Brushes.Gray, newsItem.Message);
                        break;
                    case "VoteForStart":
                        WriteInfo(newsItem.Message);
                        break;
                    case "GameHasStarted":
                        gameIsRunnning = GameState.Started;
                        WriteInfo(newsItem.Message);
                        break;
                    case "DrawMayBeRedirected":
                        WriteInfo(newsItem.Message);
                        break;

                }
            }
        }

        private void ShowCardPlaced(NewsItem newsItem)
        {
            var cardVm = new CardViewModel(newsItem.Card);
            WriteMessage(cardVm.DisplayColor, "{0}: {1}", newsItem.Message, cardVm.DisplayText);
            lastCard = newsItem.Card;
        }

        private void ReadUsers()
        {
            Task.Run(async () =>
            {
                var users = await Service.GetRegisteredUsersAsync();

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    UserList = users.Select(u => new UserViewModel(u.Item2, u.Item1, u.Item3, SayUnoFor)).ToList();
                    MyTurn = (users.First(u => u.Item2 == userName).Item1 && gameIsRunnning == GameState.Started);
                    InputLineBackground = MyTurn ? Brushes.Gray : Brushes.Black;

                });
            });
        }


        private void ReadCards()
        {
            Task.Run(async () =>
            {
                var cards = (await Service.ViewMyCardsAsync(Token));
                if (cards.Value != null)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        CurrentCards.Clear();
                        cards.Value.ToList().ForEach(c =>
                        {
                            var cvm = new CardViewModel(c) { Hidden = hiddenMode };
                            CurrentCards.Add(cvm);
                        });
                    });

                }
                else WriteWarning(cards.Message);
            });


        }

        private void OverGame(NewsItem newsItem)
        {
            Token = Guid.Empty;
            gameIsRunnning = GameState.Stopped;
            WriteInfo(newsItem.Message);
        }

        #endregion

        #region Private method-helpers

        private void CheckConnection()
        {
            Task.Run(async () =>
            {
                try
                {
                    gameIsRunnning = await Service.GameIsRunningAsync();
                    if (gameIsRunnning == GameState.Started)
                        WriteInfo("Connection established. Game is currently running");
                    else if (gameIsRunnning == GameState.Stopped)
                        WriteInfo("Connection established. Please register to game");
                    else
                        WriteInfo("Connection established. Game is waitng for all user readiness");
#if (!DEBUG)
                    //Temporary turned of for testing by multiple clients
                    if (!Settings.Default.LastGuid.Equals(Guid.Empty))
                    {
                        var username = await Service.CheckTokenValidnessAsync(Settings.Default.LastGuid);
                        if (!String.IsNullOrWhiteSpace(username))
                        {
                            InitializeMonitoring(username, Settings.Default.LastGuid);
                            RestoreSession();
                        }
                    }
#endif
                }
                catch
                {
                    WriteError("Could not connect to the server. Please validate hostname is the config file");
                }
            });
        }


        private void RestoreSession()
        {
            ReadUsers();
            ReadCards();
            WriteInfo("Connection was restored");
        }


        private void SendCard(CardViewModel card)
        {
            Task.Run(async () =>
            {
                var res = await Service.PutCardAsync(Token, card.CardModel);
                if (!res.Value)
                    WriteWarning(res.Message);
                else
                {
                    var same = CurrentCards.Where(d => CardAreSame(d.CardModel, card.CardModel));
                    DispatcherHelper.CheckBeginInvokeOnUI(() => CurrentCards.Remove(same.First()));
                }
            });
        }

        private bool CardAreSame(Card card1, Card card2)
        {
            if (card1.Type == card2.Type && card1.Color == card2.Color)
                return true;
            if (card1.IsUnicolored)
            {
                if (card1.Type == card2.Type)
                    return true;
            }
            return false;
        }

        private void PutCardProvidedOnlyType(string s)
        {
            var type = CardViewModel.ParseCardType(s);
            if (type != CardType.None)
            {
                var cards = CurrentCards.Where(c => c.Type == type).ToList();
                if (cards.Count == 1)
                {
                    SendCard(cards.First());
                }
                else if (!cards.Any())
                    WriteInfo("You don't have such card");
                else
                {
                    if (type == CardType.Draw4 || type == CardType.Wild)
                        WriteInfo("Plase specify new color for card");
                    else
                        PutCardProvidedTypeAndColor(s, lastCard.Color.ToString());
                   }
            }
            else WriteInfo("Entered wrong card type");
        }

        private void PutCardProvidedTypeAndColor(string typeStr, string colorStr)
        {
            var type = CardViewModel.ParseCardType(typeStr);
            var color = CardViewModel.ParseCardColor(colorStr);
            if (type == CardType.None)
            {
                WriteInfo("Entered wrong card type");
                return;
            }
            if (color == CardColor.None)
            {
                WriteInfo("Entered wrong card color");
                return;
            }
            var cards = CurrentCards.Where(c => c.Type == type && c.Color == color).ToList();
            if (cards.Any())
                SendCard(cards.First());
            else if (type == CardType.Draw4 || type == CardType.Wild)
            {
                cards = CurrentCards.Where(c => c.Type == type).ToList();
                if (cards.Any())
                {
                    cards.First().CardModel.Color = color;
                    SendCard(cards.First());
                }
            }
            else
                WriteInfo("Wrong command");

        }

        private void PutCardProvided3Params(string typeStr, string colorStr, string username)
        {
            var type = CardViewModel.ParseCardType(typeStr);
            if (type != CardType.Number0) WriteInfo("Wrong command");

            var color = CardViewModel.ParseCardColor(colorStr);
            var cards = CurrentCards.Where(c => c.Type == type && c.Color == color).ToList();
            if (cards.Any())
            {
                var card = cards.First();
                card.CardModel.ZeroCardUserName = username;
                SendCard(card);
            }
            else WriteInfo("You don't have such card");
        }

        private void OnLastCommadCommand()
        {
            CommandString = lastCommand;
        }

        #endregion

        #region Public Properties

        public ObservableCollection<CardViewModel> CurrentCards
        {
            get { return currentCards; }
            set { currentCards = value; }
        }

        public ObservableCollection<TextLine> OutputData
        {
            get { return lines; }
            set
            {
                lines = value;
                RaisePropertyChanged();
            }
        }

        public string CommandString
        {
            get { return commandString; }
            set
            {
                Set(ref commandString, value);
            }
        }

        public RelayCommand PushCommand { get; private set; }

        public RelayCommand LastCommadCommand { get; private set; }

        public IUnoService Service
        {
            get { lock (sync) { return service; } }
            set { lock (sync) { service = value; } }
        }

        public List<UserViewModel> UserList
        {
            get { return userList; }
            set
            {
                Set(ref userList, value);
            }
        }

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                Set(ref windowTitle, value);
            }
        }

        public Brush InputLineBackground
        {
            get { return inputLineBackground; }
            set
            {
                Set(ref inputLineBackground, value);
            }
        }

        public Guid Token
        {
            get { return token; }
            set
            {
                token = value;
                Settings.Default.LastGuid = token;
                Settings.Default.Save();
            }
        }

        public bool MyTurn
        {
            get { return myTurn; }
            set
            {
                if (myTurn != value)
                {
                    myTurn = value;
                    if (myTurn)
                        flasher.FlashApplicationWindow();
                }
            }
        }

        #endregion
    }
}