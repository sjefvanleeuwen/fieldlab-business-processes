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
            Console.WriteLine($"bsn: {bsn}");
            var result = client.GetStringAsync($"http://wigo4it-k8s.464b0cafc5de44b9bc20.westeurope.aksapp.io:5080/bg/RaadpleegIngeschrevenPersoonNAW?burgerservicenummer={bsn}").Result;
            
            Console.WriteLine($"saving data to redis store key: {externalTask.Variables["datakey"].Value.ToString()}");
            
            Program.Db.StringSet(externalTask.Variables["datakey"].Value.ToString(), result,new TimeSpan(0,1,0));
        }
    }
}