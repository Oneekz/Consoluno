﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Consoluno.Bot.UnoServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="UnoServiceReference.IUnoService")]
    public interface IUnoService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/RegisterUser", ReplyAction="http://tempuri.org/IUnoService/RegisterUserResponse")]
        Consoluno.Common.ServiceAnswer<System.Guid> RegisterUser(string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/RegisterUser", ReplyAction="http://tempuri.org/IUnoService/RegisterUserResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<System.Guid>> RegisterUserAsync(string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/GetRegisteredUsers", ReplyAction="http://tempuri.org/IUnoService/GetRegisteredUsersResponse")]
        Consoluno.Common.Pair<bool, string, int>[] GetRegisteredUsers();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/GetRegisteredUsers", ReplyAction="http://tempuri.org/IUnoService/GetRegisteredUsersResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.Pair<bool, string, int>[]> GetRegisteredUsersAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/StartGame", ReplyAction="http://tempuri.org/IUnoService/StartGameResponse")]
        Consoluno.Common.ServiceAnswer<bool> StartGame(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/StartGame", ReplyAction="http://tempuri.org/IUnoService/StartGameResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> StartGameAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/GetCommandsToDo", ReplyAction="http://tempuri.org/IUnoService/GetCommandsToDoResponse")]
        Consoluno.Common.ServiceAnswer<Consoluno.Common.NewsItem[]> GetCommandsToDo(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/GetCommandsToDo", ReplyAction="http://tempuri.org/IUnoService/GetCommandsToDoResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<Consoluno.Common.NewsItem[]>> GetCommandsToDoAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/ViewMyCards", ReplyAction="http://tempuri.org/IUnoService/ViewMyCardsResponse")]
        Consoluno.Common.ServiceAnswer<Consoluno.Common.Card[]> ViewMyCards(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/ViewMyCards", ReplyAction="http://tempuri.org/IUnoService/ViewMyCardsResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<Consoluno.Common.Card[]>> ViewMyCardsAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/TakeCard", ReplyAction="http://tempuri.org/IUnoService/TakeCardResponse")]
        Consoluno.Common.ServiceAnswer<bool> TakeCard(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/TakeCard", ReplyAction="http://tempuri.org/IUnoService/TakeCardResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> TakeCardAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/PutCard", ReplyAction="http://tempuri.org/IUnoService/PutCardResponse")]
        Consoluno.Common.ServiceAnswer<bool> PutCard(System.Guid token, Consoluno.Common.Card card);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/PutCard", ReplyAction="http://tempuri.org/IUnoService/PutCardResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> PutCardAsync(System.Guid token, Consoluno.Common.Card card);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/GameIsRunning", ReplyAction="http://tempuri.org/IUnoService/GameIsRunningResponse")]
        Consoluno.Service.GameState GameIsRunning();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/GameIsRunning", ReplyAction="http://tempuri.org/IUnoService/GameIsRunningResponse")]
        System.Threading.Tasks.Task<Consoluno.Service.GameState> GameIsRunningAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/CheckTokenValidness", ReplyAction="http://tempuri.org/IUnoService/CheckTokenValidnessResponse")]
        string CheckTokenValidness(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/CheckTokenValidness", ReplyAction="http://tempuri.org/IUnoService/CheckTokenValidnessResponse")]
        System.Threading.Tasks.Task<string> CheckTokenValidnessAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/WriteMessage", ReplyAction="http://tempuri.org/IUnoService/WriteMessageResponse")]
        Consoluno.Common.ServiceAnswer<bool> WriteMessage(System.Guid token, string message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/WriteMessage", ReplyAction="http://tempuri.org/IUnoService/WriteMessageResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> WriteMessageAsync(System.Guid token, string message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/VoteForStart", ReplyAction="http://tempuri.org/IUnoService/VoteForStartResponse")]
        Consoluno.Common.ServiceAnswer<bool> VoteForStart(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/VoteForStart", ReplyAction="http://tempuri.org/IUnoService/VoteForStartResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> VoteForStartAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/SayUnoForMyself", ReplyAction="http://tempuri.org/IUnoService/SayUnoForMyselfResponse")]
        Consoluno.Common.ServiceAnswer<bool> SayUnoForMyself(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/SayUnoForMyself", ReplyAction="http://tempuri.org/IUnoService/SayUnoForMyselfResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> SayUnoForMyselfAsync(System.Guid token);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/SayUno", ReplyAction="http://tempuri.org/IUnoService/SayUnoResponse")]
        Consoluno.Common.ServiceAnswer<bool> SayUno(System.Guid token, string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/SayUno", ReplyAction="http://tempuri.org/IUnoService/SayUnoResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> SayUnoAsync(System.Guid token, string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/AddBot", ReplyAction="http://tempuri.org/IUnoService/AddBotResponse")]
        Consoluno.Common.ServiceAnswer<bool> AddBot(System.Guid token, string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUnoService/AddBot", ReplyAction="http://tempuri.org/IUnoService/AddBotResponse")]
        System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> AddBotAsync(System.Guid token, string username);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IUnoServiceChannel : Consoluno.Bot.UnoServiceReference.IUnoService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class UnoServiceClient : System.ServiceModel.ClientBase<Consoluno.Bot.UnoServiceReference.IUnoService>, Consoluno.Bot.UnoServiceReference.IUnoService {
        
        public UnoServiceClient() {
        }
        
        public UnoServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public UnoServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public UnoServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public UnoServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Consoluno.Common.ServiceAnswer<System.Guid> RegisterUser(string userName) {
            return base.Channel.RegisterUser(userName);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<System.Guid>> RegisterUserAsync(string userName) {
            return base.Channel.RegisterUserAsync(userName);
        }
        
        public Consoluno.Common.Pair<bool, string, int>[] GetRegisteredUsers() {
            return base.Channel.GetRegisteredUsers();
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.Pair<bool, string, int>[]> GetRegisteredUsersAsync() {
            return base.Channel.GetRegisteredUsersAsync();
        }
        
        public Consoluno.Common.ServiceAnswer<bool> StartGame(System.Guid token) {
            return base.Channel.StartGame(token);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> StartGameAsync(System.Guid token) {
            return base.Channel.StartGameAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<Consoluno.Common.NewsItem[]> GetCommandsToDo(System.Guid token) {
            return base.Channel.GetCommandsToDo(token);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<Consoluno.Common.NewsItem[]>> GetCommandsToDoAsync(System.Guid token) {
            return base.Channel.GetCommandsToDoAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<Consoluno.Common.Card[]> ViewMyCards(System.Guid token) {
            return base.Channel.ViewMyCards(token);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<Consoluno.Common.Card[]>> ViewMyCardsAsync(System.Guid token) {
            return base.Channel.ViewMyCardsAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> TakeCard(System.Guid token) {
            return base.Channel.TakeCard(token);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> TakeCardAsync(System.Guid token) {
            return base.Channel.TakeCardAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> PutCard(System.Guid token, Consoluno.Common.Card card) {
            return base.Channel.PutCard(token, card);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> PutCardAsync(System.Guid token, Consoluno.Common.Card card) {
            return base.Channel.PutCardAsync(token, card);
        }
        
        public Consoluno.Service.GameState GameIsRunning() {
            return base.Channel.GameIsRunning();
        }
        
        public System.Threading.Tasks.Task<Consoluno.Service.GameState> GameIsRunningAsync() {
            return base.Channel.GameIsRunningAsync();
        }
        
        public string CheckTokenValidness(System.Guid token) {
            return base.Channel.CheckTokenValidness(token);
        }
        
        public System.Threading.Tasks.Task<string> CheckTokenValidnessAsync(System.Guid token) {
            return base.Channel.CheckTokenValidnessAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> WriteMessage(System.Guid token, string message) {
            return base.Channel.WriteMessage(token, message);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> WriteMessageAsync(System.Guid token, string message) {
            return base.Channel.WriteMessageAsync(token, message);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> VoteForStart(System.Guid token) {
            return base.Channel.VoteForStart(token);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> VoteForStartAsync(System.Guid token) {
            return base.Channel.VoteForStartAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> SayUnoForMyself(System.Guid token) {
            return base.Channel.SayUnoForMyself(token);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> SayUnoForMyselfAsync(System.Guid token) {
            return base.Channel.SayUnoForMyselfAsync(token);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> SayUno(System.Guid token, string username) {
            return base.Channel.SayUno(token, username);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> SayUnoAsync(System.Guid token, string username) {
            return base.Channel.SayUnoAsync(token, username);
        }
        
        public Consoluno.Common.ServiceAnswer<bool> AddBot(System.Guid token, string username) {
            return base.Channel.AddBot(token, username);
        }
        
        public System.Threading.Tasks.Task<Consoluno.Common.ServiceAnswer<bool>> AddBotAsync(System.Guid token, string username) {
            return base.Channel.AddBotAsync(token, username);
        }
    }
}
