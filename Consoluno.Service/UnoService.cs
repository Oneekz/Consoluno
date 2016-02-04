using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Consoluno.Common;
using System.Threading.Tasks;

namespace Consoluno.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class UnoService : IUnoService
    {
        #region Fields

        private Dictionary<Guid, User> users = new Dictionary<Guid, User>();
        private GameState gameState = GameState.Stopped; //false: no game, true: is playing, null: waiting for vote
        private Queue<Card> deck = new Queue<Card>(108);
        private Direction currentDirection = Direction.Forward;
        private int currentTurnPlayerId = 0;
        private Card lastCard;
        private object sync = new object();
        private List<AppDomain> BotsDomains = new List<AppDomain>();
        private Pair<int, CardType> drawsPending = null;

        #endregion

        #region OperationBehaviors

        [OperationBehavior]
        public ServiceAnswer<Guid> RegisterUser(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
                return ServiceAnswer.Create(Guid.Empty, "Username is empty");
            userName = userName.Trim();
            if (Users.Any(u => u.Value.UserName.ToLower() == userName.ToLower()))
                return ServiceAnswer.Create(Guid.Empty, "Username is already taken. Please provide another one");
            if (userName.Length > 15)
                return ServiceAnswer.Create(Guid.Empty, "Username is too long");
            if (!Regex.IsMatch(userName, @"^[a-zA-Z0-9]+$"))
                return ServiceAnswer.Create(Guid.Empty, "Username contains invalid character(s)");
            if (gameState == GameState.Started)
                return ServiceAnswer.Create(Guid.Empty, "Game is already running");
            if (!CheckUsernameValidness(userName))
                return ServiceAnswer.Create(Guid.Empty, "Username cannot be same as color codes");

            var user = new User(userName, Users.Count) { LastAccessTime = DateTime.Now };
            Users.Add(user.Token, user);
            var cmd = NewsItem.CreateCmd(NewsCommands.ReadUsers, "New user has been registered: {0}", userName);
            AddReturnCommand(cmd);
            Console.WriteLine(cmd.Message);
            return ServiceAnswer.Create(user.Token, "OK");
        }

        [OperationBehavior]
        public List<Pair<bool, string, int>> GetRegisteredUsers()
        {
            if (currentDirection == Direction.Forward)
                return Users.Select(u => Pair.Create(u.Value.OrderId == CurrentTurnPlayerId, u.Value.UserName, u.Value.Cards.Count)).ToList();
            return Users.Select(u => Pair.Create(u.Value.OrderId == CurrentTurnPlayerId, u.Value.UserName, u.Value.Cards.Count)).Reverse().ToList();
        }

        [OperationBehavior]
        public ServiceAnswer<bool> StartGame(Guid token)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (Users.Any(u => u.Value.Ready))
                return ServiceAnswer.Create(false, "Waiting for all user to be ready");
            if (gameState == GameState.Stopped)
            {
                if (Users.Count < 2)
                    return ServiceAnswer.Create(false, "At lease two user needed to start a game");
                gameState = GameState.Starting;
                Users[token].Ready = true;
                Users[token].CommandList.Add(NewsItem.CreateMsg("{0}: Let's start a game", Users[token].UserName));
                AddReturnCommand(NewsItem.CreateCmd(NewsCommands.VoteForStart, "{0}: Let's start a game. Write 'ready' to agree", Users[token].UserName), token);

                return ServiceAnswer.Create(true, "The game was promted to be started");
            }
            return ServiceAnswer.Create(false, "The game is already running");
        }

        [OperationBehavior]
        public ServiceAnswer<List<NewsItem>> GetCommandsToDo(Guid token)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create<List<NewsItem>>(null, "Token is not valid");
            try
            {
                CheckIfOtherUsersOnline(token);
                var list = Users[token].CommandList.Select(d => d).ToList();
                Users[token].CommandList.Clear();
                Users[token].LastAccessTime = DateTime.Now;
                return ServiceAnswer.Create(list, "OK");

            }
            catch (Exception ex)
            {
                return ServiceAnswer.Create<List<NewsItem>>(null, ex.Message);
            }
        }

        [OperationBehavior]
        public ServiceAnswer<List<Card>> ViewMyCards(Guid token)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create<List<Card>>(null, "Token is not valid");
            return ServiceAnswer.Create(Users[token].Cards.OrderBy(c => c.Color).ThenBy(t => t.Type).ToList(), "OK");
        }

        [OperationBehavior]
        public ServiceAnswer<bool> TakeCard(Guid token)
        {
            if (gameState != GameState.Started)
                return ServiceAnswer.Create(false, "Game is not running");
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (CurrentTurnPlayerId != Users[token].OrderId)
                return ServiceAnswer.Create(false, "It is not your turn");

            var cardCount = 1;
            if (drawsPending != null)
            {
                cardCount = ((drawsPending.Item2 == CardType.Draw4) ? 4 : 2) * drawsPending.Item1;
                drawsPending = null;
                AddReturnCommand(NewsItem.CreateMsg("{0} has taken {1} cards", Users[token].UserName, cardCount));
            }
            else
                AddReturnCommand(NewsItem.CreateMsg("{0} has taken a card", Users[token].UserName));

            UserAddCard(token, GetCardsFromDeck(cardCount));

            IncreaseOrderId();
            Users[token].CommandList.Add(NewsItem.CreateCmd(NewsCommands.ReadCards));
            
            return ServiceAnswer.Create(true, "OK");
        }

        [OperationBehavior]
        public ServiceAnswer<bool> PutCard(Guid token, Card card)
        {
            if (gameState != GameState.Started)
                return ServiceAnswer.Create(false, "Game is not running");
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (currentTurnPlayerId != Users[token].OrderId)
                return ServiceAnswer.Create(false, "It is not your turn. Please, be patient");
            if (!Users[token].Cards.Any(d => d.Equals(card)))
                return ServiceAnswer.Create(false, "You don't have such card");
            if (!LastCard.CanBeBefore(card))
                return ServiceAnswer.Create(false, "Wrong card placed");
            if (card.Color == CardColor.White)
                return ServiceAnswer.Create(false, "Please choose color of the card");
            if (card.Type == CardType.Number0 && (String.IsNullOrWhiteSpace(card.ZeroCardUserName) || Users.All(u => u.Value.UserName != card.ZeroCardUserName)))
                return ServiceAnswer.Create(false, "There is no such user to take card from");

            if (drawsPending != null)
            {
                if (card.Type != drawsPending.Item2)
                    return ServiceAnswer.Create(false, "You have to put card of the same type or just take one card");
            }

            PlaceCardAndNotify(card, users[token].UserName);

            UserCardRemove(token, card);

            if (!Users[token].Cards.Any())
            {
                GameOver(token);
            }

            if (LastCard.Type == CardType.Reverse)
            {
                currentDirection = (currentDirection == Direction.Backward) ? Direction.Forward : Direction.Backward;
            }
            else
                if (LastCard.IsDraw)
                {
                    var nextUserGuid = GetNextUser();
                    if (Users[nextUserGuid].Cards.All(c => c.Type != LastCard.Type))
                    {
                        if (LastCard.Type == CardType.Draw2)
                        {
                            if (drawsPending == null)
                            {
                                UserAddCard(nextUserGuid, GetCardsFromDeck(2));
                                AddReturnCommand(NewsItem.CreateMsg("{0} has taken two cards", Users[nextUserGuid].UserName));
                            }
                            else
                            {
                                int count = 2 * drawsPending.Item1;
                                UserAddCard(nextUserGuid, GetCardsFromDeck(count));
                                AddReturnCommand(NewsItem.CreateMsg("{0} has taken {1} cards", Users[nextUserGuid].UserName, count));
                                drawsPending = null;
                            }
                        }
                        else if (LastCard.Type == CardType.Draw4)
                        {
                            if (drawsPending == null)
                            {
                                UserAddCard(nextUserGuid, GetCardsFromDeck(4));
                                AddReturnCommand(NewsItem.CreateMsg("{0} has taken four cards", Users[nextUserGuid].UserName));
                            }
                            else
                            {
                                int count = 4 * drawsPending.Item1;
                                UserAddCard(nextUserGuid, GetCardsFromDeck(count));
                                AddReturnCommand(NewsItem.CreateMsg("{0} has taken {1} cards", Users[nextUserGuid].UserName, count));
                                drawsPending = null;
                            }
                        }
                        Users[nextUserGuid].CommandList.Add(NewsItem.CreateCmd(NewsCommands.ReadCards));
                    }
                    else
                    {
                        Users[nextUserGuid].CommandList.Add(NewsItem.CreateCmd(NewsCommands.DrawMayBeRedirected, "You may redirect drawing card by placing same type card"));
                        if (drawsPending == null)
                        {
                            drawsPending = new Pair<int, CardType>(1, LastCard.Type);
                        }
                        else
                            drawsPending.Item1++;
                        AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
                        return ServiceAnswer.Create(true, "OK");
                    }
                }
                else
                    if (LastCard.Type == CardType.Number0)
                    {
                        ZeroCardPlaced(token, LastCard);
                    }

            IncreaseOrderId(LastCard.Type == CardType.Skip ? 2 : 1);

            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
            return ServiceAnswer.Create(true, "OK");
        }

        [OperationBehavior]
        public GameState GameIsRunning()
        {
            return gameState;
        }

        [OperationBehavior]
        public string CheckTokenValidness(Guid token)
        {
            if (Users.ContainsKey(token))
                return Users[token].UserName;
            return null;
        }

        [OperationBehavior]
        public ServiceAnswer<bool> WriteMessage(Guid token, string message)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (String.IsNullOrWhiteSpace(message))
                return ServiceAnswer.Create(false, "Message is empty");
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.Message, "{0}: {1}", Users[token].UserName, message));
            return ServiceAnswer.Create(true, "OK");
        }

        [OperationBehavior]
        public ServiceAnswer<bool> VoteForStart(Guid token)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (gameState == GameState.Started)
                return ServiceAnswer.Create(false, "Game is already running");
            if (gameState == GameState.Stopped)
                return ServiceAnswer.Create(false, "Prompt to start a game before");
            if (Users[token].Ready)
                return ServiceAnswer.Create(false, "You have already voted for game to start");

            Users[token].Ready = true;
            if (Users.All(u => u.Value.Ready))
            {
                StartGameProcess();
            }
            else
            {
                AddReturnCommand(NewsItem.CreateMsg("{0} is ready to start", Users[token].UserName));
            }
            return ServiceAnswer.Create(true, "OK");
        }

        [OperationBehavior]
        public ServiceAnswer<bool> SayUnoForMyself(Guid token)
        {
            if (gameState != GameState.Started)
                return ServiceAnswer.Create(false, "Game is not running");
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            //якщо твій хід то має бути карт 1 або 2
            //якщо не твій хід, то має бути тільки одна карта
            if (Users[token].OrderId != CurrentTurnPlayerId) //not your turn
            {
                if (Users[token].Cards.Count != 1)
                    return ServiceAnswer.Create(false, "It is not your turn");
            }
            else
                if (Users[token].Cards.Count > 2) // your turn
                    return ServiceAnswer.Create(false, "You need to have one or two cards to say uno");
            Users[token].UnoSaid = true;
            AddReturnCommand(NewsItem.CreateMsg("{0}: UNO", Users[token].UserName));
            return ServiceAnswer.Create(true, "OK");
        }

        [OperationBehavior]
        public ServiceAnswer<bool> SayUno(Guid token, string username)
        {
            if (gameState != GameState.Started)
                return ServiceAnswer.Create(false, "Game is not running");
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            var user = Users.FirstOrDefault(u => u.Value.UserName == username).Value;
            if (user == null)
                return ServiceAnswer.Create(false, "Username not found");
            if (user.Cards.Count == 1 && !user.UnoSaid)
            {
                UserAddCard(user.Token, GetCardsFromDeck(2));
                AddReturnCommand(NewsItem.CreateMsg("{0} said UNO to {1}", Users[token].UserName, user.UserName));
                AddReturnCommand(NewsItem.CreateMsg("{0} takes two cards", user.UserName));
            }
            if (user.Cards.Count == 1 && user.UnoSaid)
                return ServiceAnswer.Create(false, "User have already said 'uno'");
            return ServiceAnswer.Create(false, "User has more than one card");
        }

        [OperationBehavior]
        public ServiceAnswer<bool> AddBot(Guid token, string username)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (gameState == GameState.Started)
                return ServiceAnswer.Create(false, "Game is already running");
            try
            {
                AppDomain domain = AppDomain.CreateDomain("botDomain" + BotsDomains.Count);
                domain.CreateInstance("Consoluno.Bot", "Consoluno.Bot.Bot", true, BindingFlags.CreateInstance, null,
                    new object[] { username }, null, null);
                BotsDomains.Add(domain);
                return ServiceAnswer.Create(true, "OK");
            }
            catch (Exception ex)
            {
                return ServiceAnswer.Create(false, ex.Message);
            }
        }

        [OperationBehavior]
        public ServiceAnswer<bool> ShuffleUser(Guid token)
        {
            if (!ValidateToken(token))
                return ServiceAnswer.Create(false, "Token is not valid");
            if (gameState == GameState.Started)
                return ServiceAnswer.Create(false, "Game is already running");
            var tempUsers = Users.Select(u => u.Value).ToList();
            tempUsers.Shuffle();
            Users.Clear();
            for (var i = 0; i < tempUsers.Count; i++)
            {
                tempUsers[i].OrderId = i;
                Users.Add(tempUsers[i].Token, tempUsers[i]);
            }

            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
            return ServiceAnswer.Create(true, "OK");
        }

        #endregion

        #region Private Methods

        private Guid GetNextUser()
        {
            IncreaseOrderId();
            return Users.First(d => d.Value.OrderId == currentTurnPlayerId).Key;
        }

        private void IncreaseOrderId(int by = 1)
        {
            if (currentDirection == Direction.Forward)
            {
                CurrentTurnPlayerId += by;
            }
            else
            {
                CurrentTurnPlayerId -= by;
            }
        }

        private bool ValidateToken(Guid token)
        {
            return Users.Any(u => u.Value.Token == token);
        }

        private void AddReturnCommand(NewsItem commad)
        {
            foreach (var u in Users)
            {
                u.Value.CommandList.Add(commad);
            }
        }

        private void AddReturnCommand(NewsItem commad, Guid expectUser)
        {
            foreach (var u in Users)
            {
                if (u.Key == expectUser)
                    continue;
                u.Value.CommandList.Add(commad);
            }
        }

        private void FillDeck()
        {
            var cards = new List<Card>(108);
            var colors = new List<CardColor> { CardColor.Blue, CardColor.Green, CardColor.Red, CardColor.Yellow };
            foreach (var color in colors)
            {
                for (var i = 0; i < 10; i++)
                {
                    var type = (CardType)Enum.Parse(typeof(CardType), "Number" + i);
                    cards.Add(new Card(type, color));
                    if (i != 0)
                        cards.Add(new Card(type, color));
                }
                var specTypes = new List<CardType> { CardType.Draw2, CardType.Reverse, CardType.Skip};
                cards.AddRange(specTypes.Select(specType => new Card(specType, color)));
            }
            for (var i = 0; i < 5; i++)
            {
                cards.Add(new Card(CardType.Draw4));
                cards.Add(new Card(CardType.Wild));
            }
            cards.Shuffle();
            cards.Shuffle();
            if (deck == null) deck = new Queue<Card>(108);
            cards.ForEach(c => deck.Enqueue(c));
        }

        private List<Card> GetCardsFromDeck(int count)
        {
            if (deck.Count < count)
                FillDeck();
            var list = new List<Card>(4);
            for (var i = 0; i < count; i++)
                list.Add(deck.Dequeue());
            return list;
        }

        private void StartGameProcess()
        {
            for (var i = 0; i < 6; i++)
                foreach (var user in Users)
                {
                    user.Value.Cards.AddRange(GetCardsFromDeck(1));
                }
            gameState = GameState.Started;
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.GameHasStarted, "Game has been started"));
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadCards));
            Card card;
            do
            {
                card = GetCardsFromDeck(1)[0];
            } while (!card.IsNumeric);
            PlaceCardAndNotify(card, "System");
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
            CurrentTurnPlayerId = 0;
        }

        private void GameOver(Guid token)
        {
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.GameOver, "1. {0} wins the game. Congratulatings!",
                Users[token].UserName));
            var otherUsers = Users.Where(u => u.Key != token).OrderBy(w => w.Value.CardsWeight).Select(p => p.Value).ToList();
            for (var i = 0; i < otherUsers.Count; i++)
                AddReturnCommand(NewsItem.CreateMsg("{0}. {1} has scored {2}.", i + 2, otherUsers[i].UserName, otherUsers[i].CardsWeight));

            gameState = GameState.Stopped;
            Task.Run(() =>
            {
                foreach (var botsDomain in BotsDomains)
                {
                    try
                    {
                        AppDomain.Unload(botsDomain);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                Thread.Sleep(2000);
                Users.Clear();
            });
        }


        private void UserAddCard(Guid token, List<Card> cards)
        {
            Users[token].Cards.AddRange(cards);
            Users[token].UnoSaid = false;
            Users[token].CommandList.Add(NewsItem.CreateCmd(NewsCommands.ReadCards));
            if (cards.Count() > 1)
                Users[token].CommandList.Add(NewsItem.CreateCmd(NewsCommands.MoreTheOneCardTaken, cards.Count().ToString()));
        }

        private void UserCardRemove(Guid token, Card card)
        {
            Users[token].Cards.Remove(card);
        }

        private void ZeroCardPlaced(Guid token, Card card)
        {
            var messses = new List<string>();
            if (Users.Count == 2)
            {
                var temp = Users.ElementAt(0).Value.Cards;
                Users.ElementAt(0).Value.Cards = Users.ElementAt(1).Value.Cards;
                Users.ElementAt(1).Value.Cards = temp;
                messses.Add(Users.ElementAt(0).Key == token
                    ? String.Format("{0} has taken cards from {1}", Users.ElementAt(0).Value.UserName, Users.ElementAt(1).Value.UserName)
                    : String.Format("{0} has taken cards from {1}", Users.ElementAt(1).Value.UserName, Users.ElementAt(0).Value.UserName));
            }
            else
            {
                var tokenCardsTakedFrom = Users.First(u => u.Value.UserName == card.ZeroCardUserName).Key;
                if (Users.Count == 3)
                {
                    var thirdToken = Users.First(u => u.Key != token && u.Key != tokenCardsTakedFrom).Key;
                    var cardToTake = Users[tokenCardsTakedFrom].Cards;
                    Users[tokenCardsTakedFrom].Cards = Users[thirdToken].Cards;
                    Users[thirdToken].Cards = Users[token].Cards;
                    Users[token].Cards = cardToTake;

                    messses.Add(String.Format("{0} has taken cards from {1}", Users[token].UserName, Users[tokenCardsTakedFrom].UserName));
                    messses.Add(String.Format("  {0} has received cards from {1}", Users[tokenCardsTakedFrom].UserName, Users[thirdToken].UserName));
                    messses.Add(String.Format("  {0} has received cards from {1}", Users[thirdToken].UserName, Users[token].UserName));
                }
                else if (Users.Count >= 4)
                {
                    var cards = Users[tokenCardsTakedFrom].Cards;
                    Users[tokenCardsTakedFrom].Cards = Users[token].Cards;
                    Users[token].Cards = cards;

                    messses.Add(String.Format("{0} has taken cards from {1}", Users[token].UserName, Users[tokenCardsTakedFrom].UserName));
                    messses.Add(String.Format("  {1} has received cards from {0}", Users[token].UserName, Users[tokenCardsTakedFrom].UserName));

                    var anotherUsers = Users.Where(u => u.Key != token && u.Key != tokenCardsTakedFrom).Select(d => d.Value).ToList();
                    anotherUsers = (currentDirection == Direction.Forward ? anotherUsers.OrderBy(u => u.OrderId)
                        : anotherUsers.OrderByDescending(u => u.OrderId)).ToList();

                    cards = anotherUsers[0].Cards;
                    for (var i = 1; i < anotherUsers.Count; i++)
                    {
                        anotherUsers[i - 1].Cards = anotherUsers[i].Cards;
                        messses.Add(String.Format("  {0} has received cards from {1}", anotherUsers[i - 1].UserName, anotherUsers[i].UserName));
                    }
                    anotherUsers.Last().Cards = cards;
                    messses.Add(String.Format("  {0} has received cards from {1}", anotherUsers.Last().UserName, anotherUsers[0].UserName));
                }
            }
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadCards));
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
            foreach (var mes in messses)
            {
                AddReturnCommand(NewsItem.CreateMsg(mes));
            }
            //AddReturnCommand(NewsItem.CreateMsg("{0} has taken cards from {1}", Users[token].UserName, card.ZeroCardUserName));
        }


        private void CheckIfOtherUsersOnline(Guid token)
        {
            Users.Where(u => u.Key != token).ToList().ForEach(r =>
            {
                if ((DateTime.Now - r.Value.LastAccessTime).TotalSeconds > 10)
                {
                    AddReturnCommand(NewsItem.CreateMsg("{0} has left a game", r.Value.UserName));
                    Users.Remove(r.Key);
                    AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
                }
            });
        }

        private bool CheckUsernameValidness(string name)
        {
            return !(new[] {"red", "r", "blue", "b", "bl", "yellow", "y", "green", "g", "gr"}).Contains(name);
        }

        #endregion

        #region Properties

        public int CurrentTurnPlayerId
        {
            get { return currentTurnPlayerId; }
            set
            {
                if (value == Users.Count)
                    currentTurnPlayerId = 0;
                else if (value > Users.Count)
                    currentTurnPlayerId = 1;
                else if (value == -1)
                    currentTurnPlayerId = Users.Count - 1;
                else if (value == -2)
                    currentTurnPlayerId = Users.Count - 2;
                else currentTurnPlayerId = value;

                AddReturnCommand(NewsItem.CreateCmd(NewsCommands.ReadUsers));
            }
        }

        public Dictionary<Guid, User> Users
        {
            get { lock (sync) { return users; } }
            set { lock (sync) { users = value; } }
        }

        public Card LastCard
        {
            get { return lastCard; }

        }

        private void PlaceCardAndNotify(Card card, string userName)
        {
            lastCard = card;
            AddReturnCommand(NewsItem.CreateCmd(NewsCommands.CardPlaced, userName, LastCard));
        }

        #endregion
    }
}
