# AirtableIntegration

This Azure function integrates the [Airtable .NET API](https://github.com/ngocnicholas/airtable.net) with [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/). Every six hours, all records are fetched from an Airtable and written to a JSON file in your Azure blob storage.


## Configuration

Copy your local.settings.sample.json file and save as a local.settings.json file. This is where environment variables are stored. You will add AppKey, BaseId, TableName from Airtable, and StorageConnectionString, BlobContainerName from your Azure Blob. 

## Todo
- Add Azure keyvault functionality for storign environment variables.
- Fetch more than 100 records at a time. 
- Detailed error handling / notifications.
