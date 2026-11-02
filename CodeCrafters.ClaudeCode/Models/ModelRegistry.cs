namespace CodeCrafters.ClaudeCode.Models;

public record ModelDefinition(string Alias, string Model, ModelProvider Provider );

public static class ModelRegistry
{
    private static readonly Dictionary<string, ModelDefinition> Models = new(StringComparer.OrdinalIgnoreCase)
    {
        ["claude"]   = new ModelDefinition("claude", "anthropic/claude-haiku-4.5", ModelProvider.OpenRouter),
        ["nemotron"] = new ModelDefinition("nemotron", "nvidia/nemotron-3-ultra-550b-a55b:free", ModelProvider.OpenRouter),
        ["gemma"]    = new ModelDefinition("gemma", "google/gemma-4-26b-a4b-it:free", ModelProvider.OpenRouter),
        ["free"]     = new ModelDefinition("free", "openrouter/free", ModelProvider.OpenRouter),
    };

    public static ModelDefinition? Resolve(string alias) => Models.GetValueOrDefault(alias);
}
