using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Google.Cloud.Firestore.V1;

namespace Nintex.Team7
{
    public static class CreateFirestoreDocument
    {
        public static async Task<FirestoreDb> Initialize()
        {
            string jsonKeyFilePath = "https://raysxtensionblobs.blob.core.windows.net/newcontainer/nacstatushub-firebase-adminsdk-cqqpu-0a9771e250.json";
            var jsonString = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                jsonString = await client.GetStringAsync(jsonKeyFilePath);
            }

            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };

            // Set the GOOGLE_APPLICATION_CREDENTIALS environment variable.
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonKeyFilePath);

            // Initialize Firestore database.
            return FirestoreDb.Create("nacstatushub", builder.Build()); // Replace with your project ID.

            // You can now use 'db' to interact with Firestore.
        }

        [FunctionName("CreateFirestoreDocument")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "collections/{collection}")] HttpRequest req,
            string collection,
            ILogger log)
        {
            var db = await Initialize();
            try
            {
                // Read the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult("Request body is empty.");
                }

                // Deserialize the JSON data to a dictionary
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);

                // Check if the collection parameter is valid
                if (string.IsNullOrWhiteSpace(collection))
                {
                    return new BadRequestObjectResult("Collection parameter is required.");
                }

                // Create a reference to the specified collection and add a new document
                CollectionReference collectionRef = db.Collection(collection);
                DocumentReference newDocRef = await collectionRef.AddAsync(data);

                return new OkObjectResult(new { DocumentID = newDocRef.Id });

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error creating Firestore document.");
                return new StatusCodeResult(500);
            }
        }
    }
}
