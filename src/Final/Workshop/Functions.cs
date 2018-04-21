using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Workshop
{
    public class NewPayment
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Email { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public bool Valid { get; set; }
        public string Transaction { get; set; }
    }

    public static class Functions
    {
        [FunctionName("Payments")]
        public static async Task<HttpResponseMessage> RecievePayment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req,
            TraceWriter log,
            [Queue("payments")] IAsyncCollector<NewPayment> collector)
        {
            var payment = await req.Content.ReadAsAsync<NewPayment>();
            await collector.AddAsync(payment);
            return req.CreateResponse();
        }

        [FunctionName("on-newpayment")]
        public static async Task OnNewPayment(
            [QueueTrigger("payments")] NewPayment newPayment,
            [DocumentDB(databaseName: "%CosmosDB%", collectionName: "%CosmosCollection%", ConnectionStringSetting = "CosmosConnection", CreateIfNotExists = true)] IAsyncCollector<NewPayment> collector
            )
        {
            if (!newPayment.Valid) throw new ArgumentException("Payment is not valid");
            await collector.AddAsync(newPayment);

        }

        [FunctionName("on-error")]
        public static void OnError(
            [QueueTrigger("payments-poison")] NewPayment errorPayment,
            TraceWriter log
            )
        {
            //TODO llamar a slack
            log.Warning($"Contact {errorPayment.Email} because there were an error on the payment with id {errorPayment.Id}");
        }

        [FunctionName("generate-ticket")]
        public static async Task GenerateTicket(
            [CosmosDBTrigger(databaseName: "%CosmosDB%", collectionName: "%CosmosCollection%", ConnectionStringSetting = "CosmosConnection", CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> documents,
            TraceWriter log,
            Binder binder)
        {
            log.Info($"Prepare to process {documents.Count} payments");


            foreach (var p in documents)
            {
                var attributes = new Attribute[]
                {
                    new BlobAttribute($"tickets/{p.Id}.data",FileAccess.Write)
                };

                using (var writer = await binder.BindAsync<TextWriter>(attributes))
                {
                    var builder = new StringBuilder();
                    builder.AppendLine($"Ticket:{ p.Id}");
                    builder.AppendLine($"email:{ p.GetPropertyValue<string>("email")}");
                    await writer.WriteAsync(builder.ToString());

                }
            }
        }

        [FunctionName("Send-ticket")]
        public static async Task SendTicket(
            [BlobTrigger("tickets/{blobname}.{blobextension}")] ICloudBlob ticket,
            [DocumentDB(databaseName: "%CosmosDB%", collectionName: "%CosmosCollection%", ConnectionStringSetting = "CosmosConnection", Id = "{blobname}")] NewPayment payment,
            TraceWriter log)
        {
            log.Info($"procesing file tickets/{payment.Id}.data");
            log.Info($"Prepare to send email to {payment.Email}");
        }


    }
}
