using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

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
            var chatDetails = "{ \"messages\": " + requestBody + ",\"model\":\"gpt-3.5-turbo\",\"functions\":[{\"name\":\"Leave_Request\",\"parameters\":{\"type\":\"object\",\"properties\":{\"startData\":{\"type\":\"object\",\"properties\":{\"se_enddate\":{\"type\":\"string\",\"format\":\"date-time\"},\"se_startdate\":{\"type\":\"string\",\"format\":\"date-time\"},\"se_email\":{\"type\":\"string\",\"format\":\"email\"}},\"required\":[\"se_enddate\",\"se_startdate\",\"se_email\"]}},\"required\":[\"startData\"]}},{\"name\":\"Add_To_Timesheet\",\"parameters\":{\"type\":\"object\",\"properties\":{\"startData\":{\"type\":\"object\",\"properties\":{\"se_endtime\":{\"type\":\"string\",\"format\":\"date-time\"},\"se_starttime\":{\"type\":\"string\",\"format\":\"date-time\"},\"se_date\":{\"type\":\"string\",\"format\":\"date\"},\"se_email\":{\"type\":\"string\",\"format\":\"email\"},\"se_numberofhours\":{\"type\":\"number\"}},\"required\":[\"se_endtime\",\"se_starttime\",\"se_email\",\"se_numberofhours\"]}},\"required\":[\"startData\"]}},{\"name\":\"Time_Sheet_Balance\",\"parameters\":{\"type\":\"object\",\"properties\":{\"startData\":{\"type\":\"object\",\"properties\":{\"se_email\":{\"type\":\"string\",\"format\":\"email\"}},\"required\":[\"se_email\"]}},\"required\":[\"startData\"]}},{\"name\":\"Leave_PTO_Balance\",\"parameters\":{\"type\":\"object\",\"properties\":{\"startData\":{\"type\":\"object\",\"properties\":{\"se_email\":{\"type\":\"string\",\"format\":\"email\"}},\"required\":[\"se_email\"]}},\"required\":[\"startData\"]}}],\"function_call\":\"auto\"}"; string jsonString = string.Empty;
            var result = string.Empty;
            using (HttpClient openAiClient = new HttpClient())
            {
                openAiClient.DefaultRequestHeaders.Authorization
                         = new AuthenticationHeaderValue("Bearer", "");
                var content = new StringContent(chatDetails, Encoding.UTF8, "application/json");
                var response = await openAiClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    jsonString = await response.Content.ReadAsStringAsync();
                    var workFlowType = GetValueFromJson(jsonString, "name");
                    var startData = await GetStartDataFromJson(jsonString);
                    // Add the document ID to the JSON string.
                    result = jsonString.Replace("\"usage\":", "\"documentId\": \"" + startData.Item2 + "\", \"usage\": ");
                    var nacContent = new StringContent(startData.Item1, Encoding.UTF8, "application/json");
                    HttpResponseMessage nacResponse = null;
                    if (workFlowType == "Leave_Request")
                    {
                        using (HttpClient nacClient = new HttpClient())
                        {
                            nacResponse = await nacClient.PostAsync("https://wenning.workflowcloud.com/api/v1/workflow/published/0f181a93-5fca-4214-aa78-305529574b6d/instances?token=AxJOaxRkhMck4zoaeLSVTLNkb2Q1KRVBEErMCoFWtnp4iab355JW0RsKD4t7LkTh0wvly9", nacContent);
                        }
                    }
                    else if (workFlowType == "Time_Sheet_Balance")
                    {
                        using (HttpClient nacClient = new HttpClient())
                        {
                            nacResponse = await nacClient.PostAsync("https://wenning.workflowcloud.com/api/v1/workflow/published/1a9c0692-598a-4dae-9491-cd86b1b6d3cf/instances?token=F0huoUfiFa4qgjS1RmnLMxT9Oq7yQRd17h0TsW81x87NLrwU5E9wCsStssOOsbqDGW9U85", nacContent);
                        }
                    }
                    else if (workFlowType == "Add_To_Timesheet")
                    {
                        using (HttpClient nacClient = new HttpClient())
                        {
                            nacResponse = await nacClient.PostAsync("https://wenning.workflowcloud.com/api/v1/workflow/published/bea92f56-5777-4e25-8405-d6be020d966a/instances?token=F0wMxkU0gTnniR5JhfF9iHFxFGVAvoR4NH4ZmAW7Pi2mOb9lj2DCFwJNN1FyAucxduhOSr", nacContent);
                        }
                    }
                    else if (workFlowType == "Leave_PTO_Balance")
                    {
                        using (HttpClient nacClient = new HttpClient())
                        {
                            nacResponse = await nacClient.PostAsync("https://wenning.workflowcloud.com/api/v1/workflow/published/b66bd70f-58bc-4eec-9567-5b5bcfd52d4d/instances?token=9GFz9paNn8fBwuIfm3FBboY9LUvRgis9x9YX214XL8v7KDNzg1TKLXdPkikwfsJp9R2wa5", nacContent);
                        }
                    }

                    if (nacResponse.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        var workFlowId = await nacResponse.Content.ReadAsStringAsync();
                        result = $"{{\"workflowType\": \"{workFlowType}\", \"documentId\": \"{startData.Item2}\", \"workflowId\": \"{workFlowId}\"}}";
                    }
                    else
                    {
                        result = "Error";
                    }
                }
                else
                {
                    result = "Error";
                }
            }

            return new OkObjectResult(result);
        }

        private static async Task<Tuple<string, string>> GetStartDataFromJson(string jsonString)
        {
            // Create a JObject object from the JSON string.
            var dynamicObject = JsonConvert.DeserializeObject<dynamic>(jsonString)!;

            // Get the JSON object with the specified name.
            string startData = dynamicObject["choices"][0]["message"]["function_call"]["arguments"].Value;
            string documentId = string.Empty;
            // call CreateDocument function to get DocumentID.
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent("{\"PlaceHolder\":\"Sent From Azure\"}", Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://nachub7.azurewebsites.net/api/collections/WorkflowStatus", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var fireBaseObj = JsonConvert.DeserializeObject<dynamic>(result)!;

                    documentId = fireBaseObj["documentID"].Value;
                    startData = startData.Replace("\n  }\n}", ", \"se_documentid\": \"" + documentId + "\"\n  }\n}");
                }
            }

            return new Tuple<string, string>(startData, documentId);
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