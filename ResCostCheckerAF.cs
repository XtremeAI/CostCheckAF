using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http;

namespace IFX.ResCost
{
    public static class ResCostCheckerAF
    {
        [FunctionName("ResCostCheckerAF")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            const string armUri = "https://management.azure.com/";
            const string subscriptionId = "c31a9f6e-5df8-4846-924c-334eb25e79c5";
            const string requestParam = "providers/Microsoft.Consumption/usageDetails?api-version=2018-06-30";


            string rg = req.Query["rg"];
            string tag = req.Query["tag"];
            
            if (rg == null && tag == null) 
            {
                return new BadRequestObjectResult("Please pass a resource group 'rg' or tag 'tag' on the query string");
            }

            string filter = "&$filter=";
            
            if (rg != null) filter += $"properties/resourceGroup eq {rg}";
            if (rg != null && tag != null) filter += " and ";
            if (tag != null) filter += $"tags eq {tag}";

            var uri = $"{armUri}subscriptions/{subscriptionId}/{requestParam}{filter}";

            var consumptionCostSummary = 0.0;

            try
            {
                using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler(),log)))
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(armUri).ConfigureAwait(false);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                    using (var response = await client.GetAsync(uri))
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            log.LogInformation($"Status:  {response.StatusCode}");
                            log.LogInformation($"Content: {responseString}");
                        }
                        
                        response.EnsureSuccessStatusCode();
                        var data = await response.Content.ReadAsAsync<dynamic>();

                        foreach(dynamic val in data.value) 
                        {
                            consumptionCostSummary += (double) val.properties.pretaxCost;
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, $"Error: {ex.Message}");
                return new BadRequestObjectResult("Something went wrong! Please check the log.");
            }

            return (ActionResult)new OkObjectResult($"Total consumed cost of the selected resources is: {consumptionCostSummary} EUR");
        }

    }
}
