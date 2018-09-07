# Notification task

The following external task sends topic messages to the SignalR Server.

## BPMN Example:
![notification-process](./doc/notification-process.png)

```
dotnet run ./notification/src/notification
```

### Start the process called: Notification from camunda

![select-process](./doc/start-process-select.png)

### Enter topic data for the notification

![start-process](./doc/start-process.png)

### Dashboard will show the number of notifications

![notification-dashboard](./doc/notification-dashboard.png)
