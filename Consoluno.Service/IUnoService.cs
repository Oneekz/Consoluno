using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Consoluno.Common;

namespace Consoluno.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUnoService" in both code and config file together.
    [ServiceContract]
    public interface IUnoService
    {
        [OperationContract]
        ServiceAnswer<Guid> RegisterUser(string userName);

        [OperationContract]
        List<Pair<bool, string, int>> GetRegisteredUsers();

        [OperationContract]
        ServiceAnswer<bool> StartGame(Guid token);

        [OperationContract]
        ServiceAnswer<List<NewsItem>> GetCommandsToDo(Guid token);

        [OperationContract]
        ServiceAnswer<List<Card>> ViewMyCards(Guid token);

        [OperationContract]
        ServiceAnswer<bool> TakeCard(Guid token);

        [OperationContract]
        ServiceAnswer<bool> PutCard(Guid token, Card card);

        [OperationContract]
        GameState GameIsRunning();

        [OperationContract]
        string CheckTokenValidness(Guid token);

        [OperationContract]
        ServiceAnswer<bool> WriteMessage(Guid token, string message);

        [OperationContract]
        ServiceAnswer<bool> VoteForStart(Guid token);

        [OperationContract]
        ServiceAnswer<bool> SayUnoForMyself(Guid token);

        [OperationContract]
        ServiceAnswer<bool> SayUno(Guid token, string username);

        [OperationContract]
        ServiceAnswer<bool> AddBot(Guid token, string username);

        [OperationContract]
        ServiceAnswer<bool> ShuffleUser(Guid token);
    }
}
