using OpenAI.Chat;

namespace CodeCrafters.ClaudeCode.Tools;

public static class ToolManager
{
    public static readonly ChatTool currentTimeTool = ChatTool.CreateFunctionTool(
        functionName: "get_current_time",
        functionDescription: "Returns the current system date and time",
        functionParameters: BinaryData.FromString(
            """
            {
              "type": "object",
              "properties": {},
              "required": []
            }
            """)
    );
    public static readonly ChatTool readFileTool = ChatTool.CreateFunctionTool(
        functionName: "read_file",
        functionDescription: "Read and return the contents of a file",
        functionParameters: BinaryData.FromString(
            """
            {
              "type": "object",
              "properties": {
                "file_path": {
                    "type": "string",
                    "description": "The path to the file to read"
                }
              },
              "required": ["file_path"]
            }
            """)
    );
    
    public static readonly ChatTool writeFileTool = ChatTool.CreateFunctionTool(
        functionName: "write_file",
        functionDescription: "Write the contents of a file",
        functionParameters: BinaryData.FromString(
            """
            {
              "type": "object",
              "properties": {
                "file_path": {
                    "type": "string",
                    "description": "The path to write into"
                },
                "file_content": {
                "type": "string",
                "description": "The file content"
            }
              },
              "required": [
                "file_path",
                "file_content"
              ]
            }
            """)
    );
}
