using System.Text.Json;

namespace CodeCrafters.ClaudeCode.Tools;

public record struct ToolResult(bool Success, string Content) 
{
    public static ToolResult Ok(string content) => new(true, content);
    public static ToolResult Error(string message) => new(false, message);
        
    public static implicit operator ToolResult(string content) => Ok(content);
        
    public string ToMessage() => JsonSerializer.Serialize(this);
}