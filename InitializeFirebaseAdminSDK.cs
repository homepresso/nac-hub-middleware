using System;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Google.Cloud.Firestore.V1;

public static class InitializeFirebaseAdminSDK
{
    public static void Initialize()
    {
        // Replace 'YOUR_JSON_KEY_FILE_PATH.json' with the actual path to your JSON key file.
        string jsonKeyFilePath = "/Users/raycabral/Desktop/Hackathon DotNetFunctions/nac-hub-middleware/nacstatushub-firebase-adminsdk-cqqpu-0a9771e250.json";

        // Set the GOOGLE_APPLICATION_CREDENTIALS environment variable.
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonKeyFilePath);

        // Initialize Firestore database.
        FirestoreDb db = FirestoreDb.Create("nacstatushub"); // Replace with your project ID.
        
        // You can now use 'db' to interact with Firestore.
    }
}
