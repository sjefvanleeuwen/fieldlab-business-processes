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

namespace notification {
    class Program
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
        private static void Main(string[] args)
        {
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
