using System.Text.Json;
using CodeCrafters.ClaudeCode.Core;
using OpenAI.Chat;

namespace CodeCrafters.ClaudeCode.Tools;

public class ToolSet
{
    private List<ChatTool> _chatTools = [];
    private Dictionary<string, ExecuteToolDelegate> _callables = [];

    public ToolSet(params ToolDescriptor[] tools)
    {
        foreach (var tool in tools)
        {
            _chatTools.Add(ToChatTool(tool));
            _callables.Add(tool.Name, tool.ExecuteToolCallback);
            
            _chatTools.TrimExcess();
            _callables.TrimExcess();
        }
    }
    
    public IList<ChatTool> AvailableTools => _chatTools;

    public ToolResult TryExecute(string toolName, BinaryData args)
    {
        return !_callables.TryGetValue(toolName, out var tool)
            ? throw new UnknownToolException(toolName)
            : tool.Invoke(args);
    }
    
    private static ChatTool ToChatTool(ToolDescriptor descriptor)
    {
        var properties = descriptor.Parameters
            .ToDictionary(
                parameter => parameter.Name,
                parameter => new
                {
                    type        = GetJsonSchemaType(parameter.Type),
                    description = parameter.Description,
                }
            );

        var required = descriptor.Parameters
            .Where(parameter => parameter.required)
            .Select(parameter => parameter.Name)
            .ToArray();

        var schema = new
        {
            type = "object",
            properties,
            required,
            additionalProperties = false
        };

        var parameters = BinaryData.FromBytes(
            JsonSerializer.SerializeToUtf8Bytes(schema)
        );

        return ChatTool.CreateFunctionTool(
            functionName: descriptor.Name,
            functionDescription: descriptor.Description,
            functionParameters: parameters
        );
    }
    private static string GetJsonSchemaType(ToolParameterType type)
    {
        return type switch
        {
            ToolParameterType.String  => "string",
            ToolParameterType.Number  => "number",
            ToolParameterType.Integer => "integer",
            ToolParameterType.Boolean => "boolean",
            ToolParameterType.Object  => "object",
            ToolParameterType.Array   => "array",

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported tool parameter type."),
        };
    }
}
