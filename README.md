# Introduction

Example C# Console App to call an API endpoint asynchronously and write to an Azure Storage Account and/or Azure Cosmos Database.

This can be ran locally using [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio) for emulating Azure Storage and the [Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21) for emulating Cosmos locally.

After installing Azurite, you will need to run the following command:

```bash
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

There are two applications in the solution.

**FunctionApp1** - This app the generates `Person` objects based on the passed in `page` and `pageSize`. This is used to generate dummy data.

**ConsoleApp** - This app connects to the API, by default the Function App, and requests Person objects to then be written to Azure Storage and/or Azure Cosmos DB.

The `appsettings.json` file should look like:

```json
{
  "URL": "http://localhost:7071/api/Function1",
  "BATCH_COUNT": 5,
  "BATCH_REQUESTS_PER_BATCH": 5,
  "QUERY_PAGE": 1,
  "QUERY_PAGESIZE": 20,
  "COSMOS_CONNECTION": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
  "COSMOS_DATABASE": "storage",
  "COSMOS_CONTAINER": "files",
  "STORAGE_CONNECTION": "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;",
  "STORAGE_CONTAINER": "files",
  "WRITE_TO_BLOB": false,
  "WRITE_TO_COSMOS": false
}
```

**URL** - The endpoint to call.

**BATCH_COUNT** - How many batches to process, for example, setting the value to `10` will execute 10 batches.

**BATCH_REQUESTS_PER_BATCH** - The number of requests to include in each batch.

**QUERY_PAGE** - What `page` or `offset` to start at.

**COSMOS_CONNECTION** - The Cosmos DB connection string, defaults to the Cosmos DB Emulator.

**COSMOS_DATABASE** - The Cosmos DB database.

**COSMOS_CONTAINER** - The Cosmos DB container where data will be written.

**STORAGE_CONNECTION** - The Azure Storage connection string, defaults to the Azurite emulator.

**STORAGE_CONTAINER** - The Azure Storage container where files will be written.

**WRITE_TO_BLOB** -  Determines whether records will be written to Azure Storage as a file, defaults to `false`.

**WRITE_TO_COSMOS** - Determines whether records will be written to Cosmos, defaults to `false`.

