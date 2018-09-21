using System;
using System.Collections.Generic;
using System.Net.Http;
using CamundaClient.Dto;
using CamundaClient.Worker;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace notification.Tasks
{
    [ExternalTaskTopic("brp")]
    public class Brp: IExternalTaskAdapter
    {
        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine("Starting Process brp");
            HubConnection connection;
            Console.WriteLine("--- External Task Variables ---");

            var s = JsonConvert.SerializeObject(externalTask);
            Console.WriteLine(s);
            foreach (var item in externalTask.Variables)
            {
                Console.WriteLine(item.Key.ToString().PadRight(18) + ": " + item.Value.Value.ToString() );
            }            

            HttpClient client = new HttpClient();
            var bsn=externalTask.Variables["bsn"].Value;
            Console.WriteLine($"bsn: ${bsn}");
            var result = client.GetStringAsync($"http://localhost:5080/bg/RaadpleegIngeschrevenPersoonNAW?burgerservicenummer={bsn}").Result;
            Program.Db.StringSet($"redis_get!brp-{bsn}", result,new TimeSpan(0,1,0));

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/eventhub")
                .Build();

            connection.StartAsync();
            connection.InvokeAsync("PublishMessage","brp", $"redis_get!brp-{bsn}", "", s);
            connection.DisposeAsync();
        }
    }
}