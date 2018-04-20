# workshop-functions

## Requisitos
Para el lab se requiere una cuenta de Azure y VisualStudio Community Edition.
Ademas se van a utilizar los siguientes servicios
- Cosmos DB
- Azure Storage
- Azure Cli
- Azure Remote Cli
- Sendgrid
- Slack

## Instalación

Para instalar el cli podemos descargarlo del siguiente [link](https://docs.microsoft.com/es-es/cli/azure/install-azure-cli?view=azure-cli-latest). Vamos a encontrar instrucciones para Windows, Mac y Linux.


## Introducción

TODO INTRO que vamos a contruir

## Paso 1 crear Azure Function y Storage Account
Vamos a crear nuestra azure function desde el portal. Podemos seguir paso a paso el siguiente [articulo](https://docs.microsoft.com/es-es/azure/azure-functions/functions-create-first-azure-function) o sino podemos usar el cli de Azure o de nuestra computadora para ejecutar este comando.


```shell
az group create --name workshop --location eastus

```

```shell
az storage account create --name wkstoragerand123 --location eastus --resource-group workshop2 --sku Standard_LRS
```

```shell
az functionapp create --resource-group workshop2 --consumption-plan-location eastus --name function1234112018 --storage-account  wkstoragerand123 
```

> Azure requiere tener nombres unidos por recurso agrupados por zonas. Si al crear el recurso da algún error cambien el name por otro

Para mas información en las docs oficiales se explican mas detalles [link](https://docs.microsoft.com/es-es/azure/azure-functions/functions-create-first-azure-function-azure-cli)

### Primera parte. Conociendo la interfaz Web y los bindings mas comunes

### Ejercicio 1
Vamos a crear una nueva function que responda a un pedido http e imprima un Hello World

### Ejercicio 2
Modificar la function anterior para leer datos desde un POST y retornar 200.

### Ejercicio 3
Almacenar el contenido del post en una mensaje que se va a transmitir por una cola para un procesamiento en diferido.

### Ejercicio 4
Recibir el mensaje de la cola y almacenarlo en Cosmos DB


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