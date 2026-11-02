using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using CodeCrafters.ClaudeCode;
using CodeCrafters.ClaudeCode.Models;
using CodeCrafters.ClaudeCode.Tools;

var arguments = ArgsParser.Consume(args);
if (arguments == null) Environment.Exit(1);

var prompt = arguments.prompt;
var model = arguments.model;

if (string.IsNullOrEmpty(prompt)) throw new Exception("Prompt must not be empty");
if (model == null) throw new Exception("Model not found");

var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1";

if (string.IsNullOrEmpty(apiKey))
    throw new Exception("OPENROUTER_API_KEY is not set");

var client = ModelManager.GetChatClient(model);

AgentLoop.Execute(client, prompt);
