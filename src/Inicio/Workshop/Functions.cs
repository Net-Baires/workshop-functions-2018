using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;

namespace Workshop
{

    public static class Functions
    {
        [FunctionName("saludar")]
        public static HttpResponseMessage EnviarSaludo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req,
            TraceWriter log)
        {
            return req.CreateResponse("Bienvenidos al azure bootcamp");
        }
    }
}
