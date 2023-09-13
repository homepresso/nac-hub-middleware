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
using System.Net.Http;
using Google.Cloud.Firestore.V1;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Nintex.Team7
{
    public static class ChatComplete
    {
        [FunctionName("ChatComplete")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ChatComplete")] HttpRequest req,
            ILogger log)
        {

            // Read the JSON data from the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var chatDetails = "{ \"messages\": " + requestBody + ",\"model\":\"gpt-3.5-turbo\",\"functions\":[{\"name\":\"Leave_Request\",\"parameters\":{\"type\":\"object\",\"properties\":{\"startData\":{\"type\":\"object\",\"properties\":{\"se_enddate\":{\"type\":\"string\",\"format\":\"date-time\"},\"se_startdate\":{\"type\":\"string\",\"format\":\"date-time\"},\"se_email\":{\"type\":\"string\",\"format\":\"email\"}},\"required\":[\"se_enddate\",\"se_startdate\",\"se_email\"]}},\"required\":[\"startData\"]}},{\"name\":\"Time_Sheet_Balance\",\"parameters\":{\"type\":\"object\",\"properties\":{\"startData\":{\"type\":\"object\",\"properties\":{\"se_email\":{\"type\":\"string\",\"format\":\"email\"}},\"required\":[\"se_email\"]}},\"required\":[\"startData\"]}}],\"function_call\":\"auto\"}"; string jsonString = string.Empty;
            var result = string.Empty;
            using (HttpClient openAiClient = new HttpClient())
            {
                openAiClient.DefaultRequestHeaders.Authorization
                         = new AuthenticationHeaderValue("Bearer", "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");
                var content = new StringContent(chatDetails, Encoding.UTF8, "application/json");
                var response = await openAiClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    jsonString = await response.Content.ReadAsStringAsync();
                    var workFlowType = GetValueFromJson(jsonString, "name");
                    if (workFlowType == "Leave_Request")
                    {
                        result = jsonString;
                        var startData = GetStartDataFromJson(jsonString, "startData");
                        using (HttpClient nacClient = new HttpClient())
                        {
                            var nacContent = new StringContent(startData, Encoding.UTF8, "application/json");
                            var nacResponse = await nacClient.PostAsync("https://wenning.workflowcloud.com/api/v1/workflow/published/0f181a93-5fca-4214-aa78-305529574b6d/instances?token=AxJOaxRkhMck4zoaeLSVTLNkb2Q1KRVBEErMCoFWtnp4iab355JW0RsKD4t7LkTh0wvly9", content);

                        }
                    }
                    else if (workFlowType == "Time_Sheet_Balance")
                    {
                        result = "Time Sheet Balance";
                    }
                }
                else
                {
                    result = "Error";
                }
            }

            return new OkObjectResult(result);
        }

        private static string GetStartDataFromJson(string jsonString, string key)
        {
            // Create a JObject object from the JSON string.
            var dynamicObject = JsonConvert.DeserializeObject<dynamic>(jsonString)!;

            // Get the JSON object with the specified name.
            var choices = dynamicObject["choices"];
            var choice1 = choices[0];
            var msg = choice1["message"];
            var func = msg["function_call"];
            var args = func["arguments"];
       
            return args.Value;
        }


        private static string GetValueFromJson(string jsonString, string key)
        {
            var regex = new Regex("\"" + key + "\": \"(.*?)\"", RegexOptions.IgnoreCase);

            // Match the regex against the string.
            var match = regex.Match(jsonString);
            return match.Groups[1].Value;
        }
    }
}