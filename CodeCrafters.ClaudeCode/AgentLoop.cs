using System.Text.Json;
using CodeCrafters.ClaudeCode.Tools;
using OpenAI.Chat;

namespace CodeCrafters.ClaudeCode;

public static class AgentLoop
{
    public static void Execute(ChatClient client, string prompt)
    {
        List<ChatMessage> messages = [];
        messages.Add(new UserChatMessage(prompt));

        while (true)
        {
            ChatCompletion response = client.CompleteChat(
                [..messages],
                new ChatCompletionOptions
                {
                    Tools =
                    {
                        ToolManager.currentTimeTool,
                        ToolManager.readFileTool,
                    },
                }
            );

            if (response.ToolCalls is { Count: > 0 })
            {
                foreach (var toolCall in response.ToolCalls)
                {
                    var result = ExecuteTool(toolCall);
                    messages.Add(new ToolChatMessage(toolCall.Id, result));
                }
            }
            else if (response.Content is { Count: > 0 })
            {
                Console.Write(response.Content[0].Text);
                break;
            }
            else throw new Exception("No choices in response");
        }
    }

    private static string ExecuteTool(ChatToolCall toolCall)
    {
        switch (toolCall.FunctionName)
        {
            case "get_current_time":
            {
                return DateTime.Now.ToString("O");
            }
            
            case "read_file":
            {
                var tool_arguments = JsonSerializer.Deserialize<ReadFileArguments>(toolCall.FunctionArguments);
                if (tool_arguments is null) throw new Exception("Invalid tool arguments.");
                var content = File.ReadAllText(tool_arguments.file_path);
                return content;
            }

            default: throw new Exception("Unknown function: " + toolCall.FunctionName);
        }
    }
    
    public sealed class ReadFileArguments
    {
        public required string file_path { get; set; }
    }
}
