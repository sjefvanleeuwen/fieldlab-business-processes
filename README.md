# fieldlab-business-processes
[![Build Status](https://travis-ci.org/wigo4it/fieldlab-business-processes.svg?branch=master)](https://travis-ci.org/wigo4it/fieldlab-business-processes)  
Business Processes within the fieldlab. The workings descibed are experimental.

## Folder Structures

Each business process will have its own directory, containing a ./doc folder for process documentation (preferably in markup format), and ./src folder for process execution.

## Processes

### Topic Notifications
Sends topic messages to the SignalR Server.
documentation [here](./notification/README.md)

### Request City Pass
Citizen process for requesting a city pass [here](./city-pass/doc/readme.md)

## Common process sequences

### (view)data Segregation from bpmn processes

The lab contains common process sequences that offer decoupling process specific data from (view) data that is used for the user interface. The following example combines the BPMN process with memory caching in Redis and live signalling using a websockets based event hub called SignalR. SignaR pushes data to a portal user interface.

![data-segregation-from-process](./common/sequences/data-segregation-from-process.png)

1. A camunda **bpmn task** caches (view)data in the redis memory cluster with a certain key/value pair.
2. A generic camunda task called: Notify then notifies the SignalR Event hub with a certain topic and message.
3. The event hub discovers the message contains a value that starts with: redis_get! and knows to lookup the key value stored under (1)
4. The event hub enriches the original topic/message payload with the view data stored in redis and pushes it to the portal UI.

The view can now be rendered in the portal by binding to the Key/Value payload that originally came from redis cache.

Viewed from a BPMN modelling perspective:  

![data-segregation-from-process-bpmn](./common/sequences/data-segregation-from-process-bpmn.png)
