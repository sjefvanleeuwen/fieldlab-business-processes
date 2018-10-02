using System;
using System.Collections.Generic;
using System.Threading;
using CamundaClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR.Client;

namespace notification {

    public class User
    {
        public string ConnectionId {get;set;}
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }


    public  class Program
    {
        public static IDatabase Db;

private static string logo = @"
 __      __.___  ________________     _____ .______________ 
/  \    /  |   |/  _____/\_____  \   /  |  ||   \__    ___/ 
\   \/\/   |   /   \  ___ /   |   \ /   |  ||   | |    |    
 \        /|   \    \_\  /    |    /    ^   |   | |    |    
  \__/\  / |___|\______  \_______  \____   ||___| |____|    
       \/              \/        \/     |__|                
       
notification camunda processes";

        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
        
        public static HubConnection connection;

        private static void Main(string[] args)
        {
            var auth = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/authenticationhub")
                .Build();

            auth.StartAsync();
            var s = auth.InvokeAsync<User>("Authenticate","serviceaccount","test").Result;

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/eventhub",options => {
                     options.AccessTokenProvider = () => Task.FromResult(s.Token);
                })
                .Build();
            
            connection.StartAsync().GetAwaiter();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( logo + "\n\n" + "Starting redis client.\n\n");

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            Db = redis.GetDatabase();
           
            Console.WriteLine("Deploying models and start External Task Workers.\n\nPRESS Ctrl-C TO STOP WORKERS.\n\n");

            CamundaEngineClient camunda = new CamundaEngineClient();            
            camunda.Startup(); // Deploys all models to Camunda and Start all found ExternalTask-Workers
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
            _closing.WaitOne();
            camunda.Shutdown(); // Stop Task Workers
            connection.DisposeAsync();
        }

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit");
            _closing.Set();
        }

        private static void writeTasksToConsole(CamundaEngineClient camunda)
        {
            var tasks = camunda.HumanTaskService.LoadTasks();
            foreach (var task in tasks)
            {
                Console.WriteLine(task.Name);
            }
        }
    }
}
