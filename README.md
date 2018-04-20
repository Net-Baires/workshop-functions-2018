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

> Azure requiere tener nombres únicos para los recursos. Si al crear el recurso da algún error cambien el name por otro

Para mas información en las docs oficiales se explican mas detalles [link](https://docs.microsoft.com/es-es/azure/azure-functions/functions-create-first-azure-function-azure-cli)

### Primera parte. Conociendo la interfaz Web y los bindings mas comunes

### Ejercicio 1

En el siguiente ejercicio vamos a crear la primer Function y la vamos a ir modificando para agregarle distintos inputs y outputs.

- Crear una function que retorne como respuesta `hello World` y que acepte GET y POST
- Cambiar la configuración para que solo acepte POST
- Agregar la clase NewPayment al código.
- Leer el contenido del POST y convertir el contenido a la clase NewPayment.
- Retornar el email como respuesta
- Cambiar la configuración para agregar como output una cola de mensajes. El nombre de la cola es `payments`
- Almacenar el contenido del POST en la cola de mensajes

```C#
public class NewPayment{
    public string Email {get;set;}
    public string Name {get;set;}
    public string LastName {get;set;}
    public bool Valid {get;set;}
    public string Transaction {get;set;}
}
```
Todo poner manana.
[image](http://prntscr.com/j6xcuc)

### Crear recurso de CosmosDB
Para terminar exitosamente el ejercicio 2. El wizard del portal de Functions nos va a pedir una conexión con CosmosDB para lo cual es recomendable hacerla previamente. Podemos seguir este tutoríal gráfico
[tutorial](https://docs.microsoft.com/es-es/azure/cosmos-db/tutorial-develop-sql-api-dotnet) o ejecutar el comando por medio del cli..

```shell
az cosmosdb create  --name cosmoswk2018 --kind GlobalDocumentDB --resource-group workshop2  --max-interval 10 --max-staleness-prefix 200 
```


### Ejercicio 2
Como paso 2 vamos a crear otra Function que se inicia cuando hay un nuevo mensaje en la cola `payments` y se va a conectar con CosmosDB para almacenar el mensaje.

- Crear una nueva function que utilice la cola de mensajes como trigger.
- Mostrar por el log el email y el nombre que llega por el mensaje.
- Agregar un nuevo output para almacenar el contenido del mensaje en una colección de CosmosDB
- Almacenar el mensaje en la colección de CosmosBD

### Ejercicio 3
Como ultimo paso si el pago no es valido hay que lanzar una excepción sino hay que almacenarlo en CosmosDB. Los mensajes en la cola `payments` o cualquier cola de Azure Queue Storage por default se reintentan hasta 5 veces. Si la 5ta vez falla ese mensaje no se procesa mas y se mueve a la cola denominada poison o en nuestro caso `payments-poison`. Por ultimo en el portal veremos como procesar los mensajes inválidos para lanzar alguna alarma o en nuestro caso un mensaje por slack.

- Modificar la function del paso 2 para lanzar una excepción en caso de que el payment no sea valido. Si el pago es valido guardarlo en CosmosDB
- Crear una nueva function para procesar los mensajes inválidos y asociar el trigger a la cola payments-poison y el output a slack.

### Segunda parte. 

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
