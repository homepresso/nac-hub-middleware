using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Google.Cloud.Firestore.V1;

namespace Nintex.Team7
{
    public static class ReadFireStore
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

            // Initialize Firestore database.
            return FirestoreDb.Create("nacstatushub", builder.Build()); // Replace with your project ID.
        }

        [FunctionName("ReadFirestoreData")]
        public static async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Function, "get", Route = "collections/{collection}/documents/{document}")] HttpRequest req,
                string collection,
                string document,
                ILogger log)
        {
            if (string.IsNullOrWhiteSpace(collection) || string.IsNullOrWhiteSpace(document))
            {
                return new BadRequestObjectResult("Both 'Collection' and 'Document' parameters are required.");
            }

            FirestoreDb db = await Initialize();

            // Access Firestore collections and documents
            DocumentReference docRef = db.Collection(collection).Document(document); // Replace with collection name and document ID.

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();

                if (data.ContainsKey("Start Date") && data["Start Date"] is Timestamp startTimestamp)
                {
                    data["Start Date"] = startTimestamp.ToDateTimeOffset().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                }
                if (data.ContainsKey("Due Date") && data["Due Date"] is Timestamp dueTimestamp)
                {
                    data["Due Date"] = dueTimestamp.ToDateTimeOffset().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                }
                string jsonData = JsonConvert.SerializeObject(data);
                return new OkObjectResult(jsonData);
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}