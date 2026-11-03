namespace CodeCrafters.ClaudeCode.Core;

public class UnknownToolException(string toolName) : Exception($"Unknown tool '{toolName}'.")
{
    
}
