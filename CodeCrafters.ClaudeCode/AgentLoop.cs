using System.Text.Json;
using CodeCrafters.ClaudeCode.Tools;
using OpenAI.Chat;

namespace CodeCrafters.ClaudeCode;

public static class AgentLoop
{
    public static void Execute(ChatClient client, string prompt)
    {
        List<ChatMessage> messages = [new UserChatMessage(prompt)];
        Console.Error.WriteLine($"User: {prompt}");
        
        while (true)
        {
            Console.Error.WriteLine();
            ChatCompletion response = client.CompleteChat(
                [..messages],
                new ChatCompletionOptions
                {
                    Tools =
                    {
                        ToolManager.currentTimeTool,
                        ToolManager.readFileTool,
                        ToolManager.writeFileTool,
                    },
                }
            );
            messages.Add(new AssistantChatMessage(response));

            if (response.ToolCalls is { Count: > 0 })
            {
                foreach (var toolCall in response.ToolCalls)
                {
                    var result = ExecuteTool(toolCall);
                    messages.Add(new ToolChatMessage(toolCall.Id, result.Content));
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

    private static ToolResult ExecuteTool(ChatToolCall toolCall)
    {
        switch (toolCall.FunctionName)
        {
            case "get_current_time":
            {
                return DateTime.Now.ToString("O");
            }
            
            case "read_file":
            {
                var args = JsonSerializer.Deserialize<ReadFileArguments>(toolCall.FunctionArguments);
                if (args is null) return ToolResult.Error("Invalid tool arguments.");
                
                if (!File.Exists(args.file_path)) return ToolResult.Error($"File {args.file_path} does not exist.");
                
                Console.Error.WriteLine($"Reading file '{args.file_path}' ...");
                var content = File.ReadAllText(args.file_path);
                
                Console.Error.WriteLine("\"\"\"");
                Console.Error.WriteLine(content);
                Console.Error.WriteLine("\"\"\"");
                
                return content;
            }
            
            case "write_file":
            {
                var args = JsonSerializer.Deserialize<WriteFileArguments>(toolCall.FunctionArguments);
                if (args is null) return ToolResult.Error("Invalid tool arguments.");
                
                Console.Error.WriteLine($"Writing file '{args.file_path}' ...");
                File.WriteAllText(args.file_path, args.file_content);
                
                Console.Error.WriteLine("\"\"\"");
                Console.Error.WriteLine(args.file_content);
                Console.Error.WriteLine("\"\"\"");
                
                return "Success";
            }

            default: throw new Exception("Unknown function: " + toolCall.FunctionName);
        }
    }
    
    public record struct ToolResult(bool Success, string Content) 
    {
        public static ToolResult Ok(string content) => new(true, content);
        public static ToolResult Error(string message) => new(false, message);
        
        public static implicit operator ToolResult(string content) => Ok(content);
        
        public string ToMessage() => JsonSerializer.Serialize(this);
    }

    public sealed record ReadFileArguments(string file_path);
    public sealed record WriteFileArguments(string file_path, string file_content);
}
