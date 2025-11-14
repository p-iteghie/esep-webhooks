using Amazon.Lambda.Core;
using System.Text;
using Newtonsoft.Json;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        context.Logger.LogInformation($"FunctionHandler received: {input.Body}");

        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.Body);

        var message = new
        {
            text = $"Issue Created: {json.issue.html_url}"
        };
        string payload = JsonConvert.SerializeObject(message);

        var client = new HttpClient();
        
        var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = client.Send(webRequest);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "Message sent to Slack",
            Headers = new Dictionary<string, string> { 
                { "Content-Type", "application/json" } 
            }
        };
    }
}
