using CodeCrafters.ClaudeCode.Models;

namespace CodeCrafters.ClaudeCode;

public record Args(
    string? prompt,
    ModelDefinition? model
);

public static class ArgsParser
{
    public static Args? Consume(string[] args)
    {
        string? prompt = null;
        var model = "claude";

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-p":
                case "--prompt":
                    if (++i >= args.Length) throw new ArgumentException("Missing prompt value");
                    prompt = args[i];
                break;

                case "-m":
                case "--model":
                    if (++i >= args.Length) throw new ArgumentException("Missing model value");
                    model = args[i];
                break;

                case "-h":
                case "--help":
                    Console.WriteLine(
                        """
                        Usage: program [options]

                        Options:
                          -p, --prompt <prompt>   Prompt to send
                          -m, --model <model>     Model mnemonic or identifier
                          -h, --help              Show this help
                        """);

                    return null;

                default:
                    throw new ArgumentException(
                        $"Unknown argument: {args[i]}"
                    );
            }
        }

        return new Args(
            prompt: prompt,
            model: ModelRegistry.Resolve(model)
        );
    }
}
