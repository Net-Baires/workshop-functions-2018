### Listado de imagenes para colocar.
http trigger configuration portal http://prntscr.com/j6xcuc

error configuration trigger http://prntscr.com/j6xd2x

on-newpayment http://prntscr.com/j6xddv

vs install
http://prntscr.com/j6yk7u
functions and webjobs tools http://prntscr.com/j6yklx


### codes

http trigger
```C#
using System.Net;

public class NewPayment{
    public string Email {get;set;}
    public string Name {get;set;}
    public string LastName {get;set;}
    public bool Valid {get;set;}
    public string Transaction {get;set;}
}

public static async Task<HttpResponseMessage> Run(
    HttpRequestMessage req, 
    TraceWriter log,
    IAsyncCollector<NewPayment> queue
    )
{
    var item =  await req.Content.ReadAsAsync<NewPayment>();
    await queue.AddAsync(item);

    return req.CreateResponse(HttpStatusCode.OK, "Hello ");
}

```
```json
{
  "bindings": [
    {
      "authLevel": "function",
      "name": "req",
      "type": "httpTrigger",
      "direction": "in",
      "methods": [
        "get",
        "post"
      ]
    },
    {
      "name": "$return",
      "type": "http",
      "direction": "out"
    },
    {
      "type": "queue",
      "name": "queue",
      "queueName": "payments",
      "connection": "AzureWebJobsDashboard",
      "direction": "out"
    }
  ],
  "disabled": false
}
```

on-error
```
using System;

public class NewPayment{
    public string Email {get;set;}
    public string Name {get;set;}
    public string LastName {get;set;}
    public bool Valid {get;set;}
    public string Transaction {get;set;}
}


public static void Run(NewPayment errorPayment, TraceWriter log)
{
    log.Info($"Processing error payment for : {errorPayment.Email}");
}

```
```json
{
  "bindings": [
    {
      "name": "errorPayment",
      "type": "queueTrigger",
      "direction": "in",
      "queueName": "payments-poison",
      "connection": "AzureWebJobsDashboard"
    }
  ],
  "disabled": false
}
```

on-newpayment
```
using System;

public class NewPayment{
    public string Email {get;set;}
    public string Name {get;set;}
    public string LastName {get;set;}
    public bool Valid {get;set;}
    public string Transaction {get;set;}
}


public static async Task Run(NewPayment item, TraceWriter log, IAsyncCollector<NewPayment> collection)
{
    if(!item.Valid){
        throw new Exception("payment rejected. Move to poison");
    }
    await collection.AddAsync(item);
    log.Info($"C# Queue trigger function processed: {item.Name}");
}
```

```json
{
  "bindings": [
    {
      "name": "item",
      "type": "queueTrigger",
      "direction": "in",
      "queueName": "payments",
      "connection": "AzureWebJobsDashboard"
    },
    {
      "type": "documentDB",
      "name": "collection",
      "databaseName": "workshop",
      "collectionName": "payments",
      "createIfNotExists": true,
      "connection": "supermanv2db_DOCUMENTDB",
      "direction": "out"
    }
  ],
  "disabled": false
}
```

# workshop-functions

## Setup
Para el lab se requiere una cuenta de azure y visual studio community edition.

## Introducción

TODO INTRO

TODO Functions Image

## Paso 1 crear Azure functions app

- Abrir portal.azure.com
- Click en nuevo
- Escribir Function App
- 