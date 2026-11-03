using System.Text.Json;
using CodeCrafters.ClaudeCode.Tools;
using OpenAI.Chat;

namespace CodeCrafters.ClaudeCode;

public class AgentLoop(ChatClient client, ToolSet toolSet)
{
    public void Execute(string prompt)
    {
        List<ChatMessage> messages = [new UserChatMessage(prompt)];
        ChatCompletionOptions chatCompletionOptions = new();

        foreach (var i in toolSet.AvailableTools) chatCompletionOptions.Tools.Add(i);
        
        Console.Error.WriteLine($"User: {prompt}");
        
        while (true)
        {
            Console.Error.WriteLine();
            ChatCompletion response = client.CompleteChat(
                [..messages],
                chatCompletionOptions
            );
            messages.Add(new AssistantChatMessage(response));

            // If there is any message
            if (response.Content is { Count: > 0 })
                foreach (var content in response.Content)
                {
                    if (string.IsNullOrWhiteSpace(content.Text)) continue;
                    Console.WriteLine(content.Text);
                }
            
            // If there is any tool call
            if (response.ToolCalls is { Count: > 0 })
                foreach (var toolCall in response.ToolCalls)
                {
                    var toolName = toolCall.FunctionName;
                    var result = toolSet.TryExecute(toolName, toolCall.FunctionArguments);
                    messages.Add(new ToolChatMessage(toolCall.Id, result.Content));
                }
            
            if (response.FinishReason == ChatFinishReason.Stop) break;
            if (response.FinishReason == ChatFinishReason.ToolCalls) continue;

            throw new ArgumentOutOfRangeException(response.FinishReason.ToString());
        }
    }
}
