using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirtableApiClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AirtableIntegration
{
    public static class Function1
    {
        [FunctionName("AirtableFetch")]
        // Run every 15 seconds
        public static async Task RunAsync([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string baseId = config["BaseId"];
            string appKey = config["AppKey"];
            string tableName = config["TableName"];

            string offset = null;
            string errorMessage = null;
            var records = new List<AirtableRecord>();

            using (AirtableBase airtableBase = new AirtableBase(appKey, baseId))
            {
                do
                {
                    Task<AirtableListRecordsResponse> task = airtableBase.ListRecords(
                           tableName,
                           offset,
                           null,
                           null,
                           100000,
                           100,
                           null,
                           null);

                    AirtableListRecordsResponse response = await task;

                    if (response.Success)
                    {
                        records.AddRange(response.Records.ToList());
                        offset = response.Offset;
                    }
                    else if (response.AirtableApiError is AirtableApiException)
                    {
                        errorMessage = response.AirtableApiError.ErrorMessage;
                        break;
                    }
                    else
                    {
                        errorMessage = "Unknown error";
                        break;
                    }
                } while (offset != null);
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Error reporting
            }
            else
            {
                // Do something with the retrieved 'records' and the 'offset'
                // for the next page of the record list.
            }
        }
    }
}
