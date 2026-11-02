using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using CodeCrafters.ClaudeCode;
using CodeCrafters.ClaudeCode.Models;
using CodeCrafters.ClaudeCode.Tools;

var arguments = ArgsParser.Consume(args);
if (arguments == null) Environment.Exit(1);

var prompt = arguments.prompt;
var model = arguments.model;

if (string.IsNullOrEmpty(prompt)) throw new Exception("Prompt must not be empty");
if (model == null) throw new Exception("Model not found");

var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1";

if (string.IsNullOrEmpty(apiKey))
    throw new Exception("OPENROUTER_API_KEY is not set");

var client = ModelManager.GetChatClient(model);

//Console.WriteLine($"User: {prompt}");
ChatCompletion response = client.CompleteChat(
    [new UserChatMessage(prompt)],
    new ChatCompletionOptions
    {
        Tools = {
            ToolManager.currentTimeTool,
            ToolManager.readFileTool,
        },
    }
);

if (response.ToolCalls is { Count: > 0 })
{
    foreach (var toolCall in response.ToolCalls)
    {
        switch (toolCall.FunctionName)
        {
            case "read_file":
            {
                var tool_arguments = JsonSerializer.Deserialize<ReadFileArguments>(toolCall.FunctionArguments);
                if (tool_arguments is null) throw new Exception("Invalid tool arguments.");
                var content = File.ReadAllText(tool_arguments.file_path);
                
                Console.Write(content);
            } break;
            
            default:
                throw new Exception("Unknown function: " + toolCall.FunctionName);
        }
    }
}
else if (response.Content is { Count: > 0 })
{
    Console.Write(response.Content[0].Text);    
}
else throw new Exception("No choices in response");

//Console.WriteLine("[Finished]");
