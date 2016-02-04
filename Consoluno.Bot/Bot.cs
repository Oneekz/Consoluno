using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consoluno.Bot.UnoServiceReference;
using Consoluno.Common;
using NewsItem = Consoluno.Common.NewsItem;

namespace Consoluno.Bot
{
    public class Bot
    {
        private readonly string username;
        private readonly Random rand;
        private readonly IUnoService service;
        private object sync = new object();
        private Guid token = Guid.Empty;
        private bool gameIsRunnning = false;
        private List<Pair<bool, string, int>> users = new List<Pair<bool, string, int>>(5);
        private Card[] cards = new Card[0];
        private Card lastCard = null;
        private bool drawMayBeRedirected = false;

        public Bot(string username)
        {
            this.username = username;
            rand = new Random(DateTime.Now.Millisecond);
            var binding = new BasicHttpBinding();
            var address = new EndpointAddress(Properties.Settings.Default.ServiceConnectionUri);
            service = new UnoServiceClient(binding, address); //TODO pass service uri
            Task.Run(() => Activate());
            //Activate();
        }

        public void Activate()
        {
            var ans = Service.RegisterUser(username);
            if (ans.Value.Equals(Guid.Empty))
                return;// ServiceAnswer.Create(false, ans.Message);
            token = ans.Value;
            InitializeMonitoring();
            //return ServiceAnswer.Create(true, "OK");
        }

        private void InitializeMonitoring()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!token.Equals(Guid.Empty))
                {
                    var commands = await Service.GetCommandsToDoAsync(token);
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

        private void ProcessNewsItem(Common.NewsItem newsItem)
        {
            if (newsItem.IsCommand)
            {
                /* Console.ForegroundColor = ConsoleColor.DarkYellow;
                 Console.WriteLine(newsItem.Command);*/
                switch (newsItem.Command)
                {
                    case "ReadUsers":
                        ReadUsers();
                        break;
                    case "ReadCards":
                        ReadCards();
                        break;
                    case "GameOver":
                        OverGame();
                        break;
                    case "CardPlaced":
                        ShowCardPlaced(newsItem);
                        break;
                    case "VoteForStart":
                        VoteForStart();
                        break;
                    case "GameHasStarted":
                        gameIsRunnning = true;
                        break;
                    case "DrawMayBeRedirected":
                        drawMayBeRedirected = true;
                        break;
                    case "MoreTheOneCardTaken":
                        MoreTheOneCardTaken(newsItem);
                        break;
                }
            }
        }

        private void MoreTheOneCardTaken(NewsItem newsItem)
        {
            int count;
            if (Int32.TryParse(newsItem.Message, out count))
            {
                SendMessage(count == 2 ? "Не надо так" : "Нормально же общались");
            }
        }

        private void VoteForStart()
        {
            Service.VoteForStart(token);
        }

        private void ShowCardPlaced(NewsItem newsItem)
        {
            if (newsItem.Card != null)
                lastCard = newsItem.Card;
        }

        private void OverGame()
        {
            token = Guid.Empty;
        }

        private void ReadCards()
        {
            var cardsGetted = Service.ViewMyCards(token);
            if (cardsGetted.Value != null)
            {
                cards = cardsGetted.Value;
                if (cards.Length == 1)
                    SometimesSayUno();
            }
        }

        private void ReadUsers()
        {
            users = Service.GetRegisteredUsers().ToList();
            var usersToSayUno = users.Where(u => u.Item3 == 1).ToList();
            if (usersToSayUno.Any())
            {
                usersToSayUno.ToList().ForEach(u => SometimesSayUno(u.Item2));
            }
            if (users.First(u => u.Item2 == username).Item1 && gameIsRunnning)
            {
                PlaceCard();
            }
        }

        private void PlaceCard()
        {
            try
            {
                Thread.Sleep(rand.Next(1000, 3000));
                var variants = cards.Where(c => lastCard.CanBeBefore(c)).ToList();

                if (!variants.Any())
                {
                    TakeCard();
                    return;
                }
                if (variants.Count == 1)
                    PutCard(variants.First());
                if (variants.Any(c => c.Type == CardType.Number0))
                {
                    var lessCardsCount = users.Where(u => u.Item2 != username).Select(c => c.Item3).Min();
                    if (cards.Length - lessCardsCount > 3)
                    {
                        var victim = users.FirstOrDefault(u => u.Item3 == lessCardsCount);
                        if (victim != null)
                        {
                            var card = variants.First(t => t.Type == CardType.Number0);
                            card.ZeroCardUserName = victim.Item2;
                            PutCard(card);
                            return;
                        }
                    }
                }
                if (drawMayBeRedirected)
                {
                    PutCard(variants.First(c => c.Type == lastCard.Type));
                    drawMayBeRedirected = false;
                    return;
                }
                if (rand.Next(3) == 0)
                    PutCard(variants[rand.Next(variants.Count)]);
                else
                {
                    variants = variants.OrderByDescending(v => v.Weight).ToList();
                    var withSameWeight = variants.Where(v => v.Weight == variants[0].Weight).ToList();
                    PutCard(withSameWeight[rand.Next(withSameWeight.Count)]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TakeCard();
            }
        }


        private void TakeCard()
        {
            Service.TakeCard(token);
        }

        private void SometimesSayUno()
        {
            if (rand.Next(3) != 0)
                Service.SayUnoForMyself(token);
        }

        private void SometimesSayUno(string userName)
        {
            if (rand.Next(3) == 0) // 33% that it will say uno
                Service.SayUno(token, userName);
        }

        private void PutCard(Card card)
        {
            if (card.Type == CardType.Number0)
                card.ZeroCardUserName = UserWithLessCards();
            if (card.IsUnicolored)
                card.Color = MostSpreadColor();
            if (cards.Length == 2)
                SometimesSayUno();
            var res = Service.PutCard(token, card);
            ReadCards();
            if (!res.Value)
                TakeCard();
        }

        private CardColor MostSpreadColor()
        {
            try
            {
                var selectedCard = cards.Where(c => !c.IsUnicolored).ToList(); // filter unicolored
                if (!selectedCard.Any())
                    return PureRandom();
                var groups = selectedCard.GroupBy(c => c.Color).ToList();
                if (groups.Count() == 1)
                    return groups[0].Key;
                var firstColor = groups.First();
                var secondColor = groups.ElementAt(1);
                if (firstColor.Select(c => c.Weight).Sum() > secondColor.Select(c => c.Weight).Sum())
                    return firstColor.Key;
                return secondColor.Key;
            }
            catch
            {
                return PureRandom();
            }
        }

        private CardColor PureRandom()
        {
            var colors = new[] { CardColor.Blue, CardColor.Green, CardColor.Red, CardColor.Yellow };
            return colors[rand.Next(colors.Length)];
        }

        private string UserWithLessCards()
        {
            return users.Where(u => u.Item2 != username).OrderBy(c => c.Item3).First().Item2;
        }

        private void SendMessage(string mes)
        {
            Service.WriteMessageAsync(token, mes);
        }

        public IUnoService Service
        {
            get { lock (sync) { return service; } }
        }
    }
}
