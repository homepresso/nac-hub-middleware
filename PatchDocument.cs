using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nintex.Team7
{
    public static class PatchFirestoreDocumentFunction
    {
        [FunctionName("PatchFirestoreDocument")]
            public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "collections/{collection}/documents/{documentId}")] HttpRequest req,
            string collection,
            string documentId,
            ILogger log)
        {

            if (string.IsNullOrWhiteSpace(collection) || string.IsNullOrWhiteSpace(documentId))
            {
                return new BadRequestObjectResult("Both 'collection' and 'documentId' parameters are required.");
            }

            // Read the JSON data from the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var patchData = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);

            if (patchData == null || patchData.Count == 0)
            {
                return new BadRequestObjectResult("JSON data with fields to update is required in the request body.");
            }
            // Initialize Firebase Admin SDK
            InitializeFirebaseAdminSDK.Initialize();

            // Create a Firestore client
            FirestoreDb db = FirestoreDb.Create("nacstatushub");

            // Reference to the document to be patched
            DocumentReference docRef = db.Collection(collection).Document(documentId);

            // Perform a partial update (patch) of the document
            await docRef.UpdateAsync(patchData);

            return new OkObjectResult("Document patched successfully.");
        }

        private static string FormatTimestamp(Timestamp timestamp)
        {
            DateTimeOffset dateTimeOffset = timestamp.ToDateTimeOffset();
            string formattedTimestamp = dateTimeOffset.ToString("MMMM dd, yyyy 'at' h:mm:ss tt 'UTC-7'");
            return formattedTimestamp;
        }
    }
}