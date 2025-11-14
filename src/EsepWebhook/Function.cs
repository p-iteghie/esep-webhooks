using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public async Task<string> FunctionHandler(Stream input, ILambdaContext context)
    {
        string body;
        using (var reader = new StreamReader(input))
        {
            body = await reader.ReadToEndAsync();
        }
        
        context.Logger.LogLine($"Raw input: {body}");
        
        try
        {
            // First, try to parse as API Gateway format
            var apiGatewayPayload = JObject.Parse(body);
            
            // Check if it's wrapped in API Gateway format (has "body" property)
            if (apiGatewayPayload["body"] != null)
            {
                // Extract the actual GitHub payload from the "body" field
                var githubPayloadString = apiGatewayPayload["body"].ToString();
                context.Logger.LogLine($"GitHub payload: {githubPayloadString}");
                
                var githubPayload = JObject.Parse(githubPayloadString);
                var issueUrl = githubPayload["issue"]?["html_url"]?.ToString();
                
                if (string.IsNullOrEmpty(issueUrl))
                {
                    context.Logger.LogLine("No issue URL found in payload");
                    return "No issue URL found";
                }

                // Send to Slack
                var message = new { text = $"Issue Created: {issueUrl}" };
                string payload = JsonConvert.SerializeObject(message);

                using var client = new HttpClient();
                var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(slackUrl, content);
                
                context.Logger.LogLine($"Slack response: {response.StatusCode}");

                return "Message sent to Slack";
            }
            else
            {
                // It's already the GitHub payload directly
                var issueUrl = apiGatewayPayload["issue"]?["html_url"]?.ToString();
                
                if (string.IsNullOrEmpty(issueUrl))
                {
                    context.Logger.LogLine("No issue URL found in payload");
                    return "No issue URL found";
                }

                // Send to Slack
                var message = new { text = $"Issue Created: {issueUrl}" };
                string payload = JsonConvert.SerializeObject(message);

                using var client = new HttpClient();
                var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(slackUrl, content);
                
                context.Logger.LogLine($"Slack response: {response.StatusCode}");

                return "Message sent to Slack";
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error: {ex.Message}");
            throw;
        }
    }
}
