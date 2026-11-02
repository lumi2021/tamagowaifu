using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

namespace CodeCrafters.ClaudeCode.Models;

public static class ModelManager
{
    
    public static ChatClient GetChatClient(ModelDefinition model)
    {
        var (apiKey, endpoint) = model.Provider switch
        {
            ModelProvider.OpenRouter => (
                Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
                Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1"
            ),

            ModelProvider.Nvidia => (
                Environment.GetEnvironmentVariable("NVIDIA_API_KEY") ??  Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
                "https://integrate.api.nvidia.com/v1"
            ),

            _ => throw new NotSupportedException($"Provider '{model.Provider}' is not supported."),
        };

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException($"API key for {model.Provider} is not configured.");

        return new ChatClient(
            model: model.Model,
            credential: new ApiKeyCredential(apiKey),
            options: new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
        );
    }
}
