using System.IO;
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

namespace Nintex.Team7
{
public static class ReadFireStore
{
    public static void InitializeFirebaseAdminMethod()
    {
        // Call the initialization method from your custom class.
        InitializeFirebaseAdminSDK.Initialize();
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

        InitializeFirebaseAdminMethod();
        // Create a Firestore client
        FirestoreDb db = FirestoreDb.Create("nacstatushub");

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