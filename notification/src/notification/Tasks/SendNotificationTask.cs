using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;
using Microsoft.AspNetCore.SignalR.Client;

namespace notification.Tasks
{
    [ExternalTaskTopic("send-notification")]
    public class NotificationTask : IExternalTaskAdapter
    {
        HubConnection _connection;

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine("--- External Task Variables ---");
            foreach (var item in externalTask.Variables)
            {
                Console.WriteLine(item.Key.ToString().PadRight(18) + ": " + item.Value.Value.ToString() );

            }
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/eventhub")
                .Build();

            _connection.InvokeAsync("Subscribe", "topic");
        }
    }
}