using System;
using System.Collections.Generic;
using System.Threading;
using CamundaClient;
using System;
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
            Console.WriteLine( logo + "\n\n" + "Starting redis client.\n\n");

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();
           
            Console.WriteLine("Deploying models and start External Task Workers.\n\nPRESS Ctrl-C TO STOP WORKERS.\n\n");

            CamundaEngineClient camunda = new CamundaEngineClient();            
            camunda.Startup(); // Deploys all models to Camunda and Start all found ExternalTask-Workers

            if (args.Length>0 && args[0]=="-t"){

                HttpClient client = new HttpClient();

                while (true)
                {
                    var bsn=new Random().Next(1234567890);
                    Console.WriteLine($"bsn: ${bsn}");
                    var result = client.GetStringAsync($"http://localhost:5080/bg/RaadpleegIngeschrevenPersoonNAW?burgerservicenummer={bsn}").Result;
                    db.StringSet($"redis_get!brp-{bsn}", result,new TimeSpan(0,1,0));

                    camunda.BpmnWorkflowService.StartProcessInstance("notification", new Dictionary<string, object>()
                    {
                        {"topicid", "topic" },
                        {"notificationmessage", $"redis_get!brp-{bsn}"}
                    });
                    Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                }
            }
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
