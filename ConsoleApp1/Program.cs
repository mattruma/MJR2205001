using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace ConsoleApp1
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static HttpClient HttpClient = new HttpClient();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Loading configuration settings from appsettings.json

            Program.Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Outputting configuration settings for informational purposes

            var URL = Program.Configuration["URL"];

            Console.WriteLine($"URL                         : {URL}");

            var BATCH_COUNT = int.Parse(Configuration["BATCH_COUNT"]);
            var BATCH_REQUESTS_PER_BATCH = int.Parse(Configuration["BATCH_REQUESTS_PER_BATCH"]);
            var QUERY_PAGE = int.Parse(Configuration["QUERY_PAGE"]);
            var QUERY_PAGESIZE = int.Parse(Configuration["QUERY_PAGESIZE"]);

            Console.WriteLine($"BATCH_COUNT                 : {BATCH_COUNT}");
            Console.WriteLine($"BATCH_REQUESTS_PER_BATCH    : {BATCH_REQUESTS_PER_BATCH}");
            Console.WriteLine($"QUERY_PAGE                  : {QUERY_PAGE}");
            Console.WriteLine($"QUERY_PAGESIZE              : {QUERY_PAGESIZE}");
            Console.WriteLine();

            // Build out the batch of tasks to execute at a single time

            var currentPage = QUERY_PAGE;

            for (var i = 0; i < BATCH_COUNT; i++)
            {
                Console.WriteLine($"[BATCH {i + 1}] - Preparing batch...");

                var tasks =
                    new List<Task>();

                for (var j = 0; j < BATCH_REQUESTS_PER_BATCH; j++)
                {
                    tasks.Add(
                        Program.SendRequestAsync(i + 1, j + 1, $"{URL}?page={currentPage}&pageSize={QUERY_PAGESIZE}"));

                    currentPage++;
                }

                Task.WaitAll(tasks.ToArray());

                Console.WriteLine($"[BATCH {i + 1}] - Batch sent!");
            };

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        static async Task SendRequestAsync(
            int batch,
            int request,
            string endpoint)
        {
            Console.WriteLine($"[BATCH {batch}] [REQUEST {request}] - Calling '{endpoint}'...");

            var response =
                await Program.HttpClient.GetAsync(endpoint);

            Console.WriteLine($"[BATCH {batch}] [REQUEST {request}] - Received status code {response.StatusCode}.");

            if (response.StatusCode != HttpStatusCode.OK) return;

            var people = JsonConvert.DeserializeObject<IEnumerable<Person>>(await response.Content.ReadAsStringAsync());

            if (people == null) return;

            foreach (var person in people)
            {
                var encodedDataAsBytes = Convert.FromBase64String(person.Bio);

                person.Bio = ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);

                Console.WriteLine(JsonConvert.SerializeObject(person, Formatting.Indented));
            }

            // Write to Blob Storage

            if (bool.Parse(Program.Configuration["WRITE_TO_BLOB"]))
            {
                var blobServiceClient =
                    new BlobServiceClient(Program.Configuration["STORAGE_CONNECTION"]);

                var blobContainers = blobServiceClient.GetBlobContainers();

                if (blobContainers.All(x => x.Name != Program.Configuration["STORAGE_CONTAINER"]))
                {
                    await blobServiceClient.CreateBlobContainerAsync(Program.Configuration["STORAGE_CONTAINER"]);
                }

                var blobContainerClient =
                    blobServiceClient.GetBlobContainerClient(Program.Configuration["STORAGE_CONTAINER"]);

                foreach (var person in people)
                {
                    var blobClient = blobContainerClient.GetBlobClient($"{person.Name}.json");

                    var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(person, Formatting.Indented));
                    var memoryStream = new MemoryStream(byteArray);

                    await blobClient.UploadAsync(memoryStream, true);
                }
            }

            // Write to Cosmos

            if (bool.Parse(Program.Configuration["WRITE_TO_COSMOS"]))
            {
                var cosmosClientOptions =
                    new CosmosClientOptions { AllowBulkExecution = true };

                var cosmosClient =
                    new CosmosClient(Program.Configuration["COSMOS_CONNECTION"], cosmosClientOptions);

                await cosmosClient.CreateDatabaseIfNotExistsAsync(Program.Configuration["COSMOS_DATABASE"]);

                var cosmosDatabase =
                    cosmosClient.GetDatabase(Program.Configuration["COSMOS_DATABASE"]);

                await cosmosDatabase.CreateContainerIfNotExistsAsync(
                    id: Program.Configuration["COSMOS_CONTAINER"],
                    partitionKeyPath: "/object");

                var cosmosContainer =
                    cosmosDatabase.GetContainer(Program.Configuration["COSMOS_CONTAINER"]);

                var tasks = new List<Task>();

                foreach (var person in people)
                {
                    tasks.Add(cosmosContainer.UpsertItemAsync(person, new PartitionKey("Person")));
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}