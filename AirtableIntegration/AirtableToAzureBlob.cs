using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirtableApiClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace AirtableIntegration
{
    public static class AirtableToAzureBlob
    {
        [FunctionName("AirtableFetch")]
        // Run every 6 hours
        public static async Task RunAsync([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
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
            string storageConnectionString = config["StorageConnectionString"];
            string blobContainerName = config["BlobContainerName"];
            string fileName = "tableData.json";
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
                        await WriteToBlobStorage(storageConnectionString, blobContainerName, fileName, records);
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
        private static async Task WriteToBlobStorage(string storageConnectionString, string blobContainerName, string fileName, object records)
        {

            try
            {
                var serializedData = JsonConvert.SerializeObject(records);
                if (!string.IsNullOrWhiteSpace(serializedData))
                {
                    if (serializedData.Length != 0)
                    {
                        if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount cloudStorageAccount))
                        {
                            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                            var cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainerName);
                            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                            await cloudBlockBlob.UploadTextAsync(serializedData);
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
