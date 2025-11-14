using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        dynamic deserialize = JsonConvert.DeserializeObject(input.Body);

        string payload = JsonConvert.SerializeObject(new { text = $"Issue Created: {deserialize.issue.html_url}" });

        var client = new HttpClient();

        var environmentVariable = Environment.GetEnvironmentVariable("SLACK_URL");
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = client.PostAsync(
            environmentVariable,
            content
        ).Result;

        var proxyResponse = new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = response.Content.ReadAsStringAsync().Result,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        return proxyResponse;
    }
}
