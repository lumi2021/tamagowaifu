using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CodeCrafters.ClaudeCode.Tools;

public delegate ToolResult ExecuteToolDelegate(BinaryData arguments);

public record ToolDescriptor(
    string Name,
    string Description,
    ExecuteToolDelegate ExecuteToolCallback,
    params ToolParameter[] Parameters
);
public record ToolParameter(ToolParameterType Type, string Name, string Description, bool required);

public enum ToolParameterType
{
    String,
    Number,
    Integer,
    Boolean,
    Object,
    Array,
}

public static class ToolRegistry
{
    public static readonly ToolDescriptor GetCurrentTime = new (
        "get_current_time", "Returns the current system date and time",
        ExecuteGetCurrentTime
    );

    public static readonly ToolDescriptor ListDirectory = new (
        "list_directory", "Lists the contents of a directory",
        ExecuteListDirectory,
        new ToolParameter(ToolParameterType.String, "dir_path", "The path to the desired directory", true)
    );
    
    public static readonly ToolDescriptor ReadFile = new (
        "read_file", "Read and return the contents of a file",
        ExecuteReadFile,
        new ToolParameter(ToolParameterType.String, "file_path", "The path to the desired file", true)
    );
    
    public static readonly ToolDescriptor WriteFile = new (
        "write_file", "Write the contents of a file",
        ExecuteWriteFile,
        new ToolParameter(ToolParameterType.String, "file_path", "The path to write into", true),
        new ToolParameter(ToolParameterType.String, "file_content", "The file content", true)
    );
    
    public static readonly ToolDescriptor RunBashCmd = new (
        "run_bash_cmd", "Execute a shell command",
        ExecuteRunBashCmd,
        new ToolParameter(ToolParameterType.String, "command", "The command to execute", true)
    );
    
    private static ToolResult ExecuteGetCurrentTime(BinaryData arguments) => DateTime.Now.ToString("O");
    
    public sealed record ListDirectoryArgument(string dir_path);
    private static ToolResult ExecuteListDirectory(BinaryData arguments)
    {
        var args = JsonSerializer.Deserialize<ListDirectoryArgument>(arguments);
        if (args is null) return ToolResult.Error("Invalid tool arguments.");
        
        if (!Directory.Exists(args.dir_path)) return ToolResult.Error($"Directory '{args.dir_path}' does not exist.");
        
        var content = new DirectoryInfo(args.dir_path).EnumerateFileSystemInfos();
        var sb = new StringBuilder();

        var entries = content.Select(fsinfo => fsinfo switch
            {
                { LinkTarget: not null } => (object)new
                {
                    name   = fsinfo.Name,
                    kind   = "symbolic_link",
                    target = fsinfo.LinkTarget,
                },
                
                DirectoryInfo => new
                {
                    name   = fsinfo.Name,
                    kind   = "directory",
                },
                
                FileInfo file => new
                {
                    name   = fsinfo.Name,
                    kind   = "file",
                    size   = (file.Length / 1024) + "KiB",
                },
                
                _ => throw new ArgumentOutOfRangeException(nameof(fsinfo), fsinfo.GetType().Name),
            }
        ).ToArray();

        var result = JsonSerializer.Serialize(entries);
        return result;
    }

    
    public sealed record ReadFileArguments(string file_path);
    private static ToolResult ExecuteReadFile(BinaryData arguments)
    {
        var args = JsonSerializer.Deserialize<ReadFileArguments>(arguments);
        if (args is null) return ToolResult.Error("Invalid tool arguments.");
                
        if (!File.Exists(args.file_path)) return ToolResult.Error($"File '{args.file_path}' does not exist.");
        
        var content = File.ReadAllText(args.file_path);
        return content;
    }
    
    public sealed record WriteFileArguments(string file_path, string file_content);
    private static ToolResult ExecuteWriteFile(BinaryData arguments)
    {
        var args = JsonSerializer.Deserialize<WriteFileArguments>(arguments);
        if (args is null) return ToolResult.Error("Invalid tool arguments.");
        
        var directory = Path.GetDirectoryName(args.file_path);
        if (string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory!);
        
        File.WriteAllText(args.file_path, args.file_content);
        return "Success";
    }
    
    public sealed record RunBashCmdArguments(string command);
    public sealed record RunBashCmdReturn(int status_code, string stdout, string stderr);
    private static ToolResult ExecuteRunBashCmd(BinaryData arguments)
    {
        var args = JsonSerializer.Deserialize<RunBashCmdArguments>(arguments);
        if (args is null) return ToolResult.Error("Invalid tool arguments.");

        var parsedCommand = ParseCommand(args.command);
        
        var startInfo = new ProcessStartInfo
        {
            FileName               = parsedCommand[0],
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
        };
        for (var i = 1; i < parsedCommand.Length; i++) startInfo.ArgumentList.Add(parsedCommand[i]);
        
        var process = new Process { StartInfo = startInfo };
        process.Start();
        
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        process.WaitForExit();
        var statusCode = process.ExitCode;
        
        var result = new RunBashCmdReturn(statusCode, stdout, stderr);
        return JsonSerializer.Serialize(result);
    }
    
    private static string[] ParseCommand(string command)
    {
        var arguments = new List<string>();
        var current = new StringBuilder();

        var quote = '\0';
        var escaped = false;

        foreach (var c in command)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\' && quote != '\'')
            {
                escaped = true;
                continue;
            }

            if (quote != '\0')
            {
                if (c == quote) quote = '\0';
                else current.Append(c);
                continue;
            }

            if (c is '"' or '\'')
            {
                quote = c;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    arguments.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (escaped || quote != '\0') throw new FormatException("Invalid command syntax.");
        if (current.Length > 0) arguments.Add(current.ToString());

        return [.. arguments];
    }
}
