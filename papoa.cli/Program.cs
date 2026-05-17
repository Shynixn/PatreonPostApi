using System.CommandLine;
using System.Text.Json;
using Papoa.Command;
using Papoa.Service;
using Spectre.Console;

namespace Papoa;

class Program
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create),
        "papoa");

    private static readonly string ConfigFile = Path.Combine(ConfigDirectory, "config.json");

    /// <summary>
    /// Entry point. Configures services and dispatches CLI commands.
    /// </summary>
    static async Task<int> Main(string[] args)
    {
        // Environment Variables
        var baseUrl = Environment.GetEnvironmentVariable("PAPOA_BASE_URL")
            ?? "https://api.papoa.shynixn.com";

        // Resolve API key: env var → persisted config → interactive prompt (interactive mode only)
        var apiKey = Environment.GetEnvironmentVariable("PAPOA_API_KEY") ?? "";
        if (string.IsNullOrEmpty(apiKey))
            apiKey = ReadApiKeyFromConfig();
        if (string.IsNullOrEmpty(apiKey) && args.Length == 0)
            apiKey = await PromptAndSaveApiKeyAsync();

        // HTTP Clients
        using var apiHttpClient = new HttpClient();
        apiHttpClient.BaseAddress = new Uri(baseUrl);
        if (!string.IsNullOrEmpty(apiKey))
        {
            apiHttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }
        using var fileUploadHttpClient = new HttpClient();

        // Implementation
        var postService = new PostService(apiHttpClient);
        var fileUploadService = new FileUploadService(fileUploadHttpClient);
        var printingService = new PrintingService();
        var postCommand = new PostCommand(postService, fileUploadService, printingService);

        // Interactive GUI when launched without arguments (e.g. double-click)
        if (args.Length == 0)
        {
            var interactive = new InteractiveMode(postService, fileUploadService, printingService);
            await interactive.RunAsync();
            return 0;
        }

        // CLI
        var rootCommand = new RootCommand("Papoa CLI");
        rootCommand.Add(postCommand.Build());
        var parseResult = rootCommand.Parse(args);
        await parseResult.InvokeAsync();
        if (parseResult.Errors.Count > 0)
        {
            Console.Error.WriteLine("Errors:");
            foreach (var error in parseResult.Errors)
            {
                Console.Error.WriteLine($"- {error.Message}");
            }
            return 1;
        }
        return 0;
    }

    private static string ReadApiKeyFromConfig()
    {
        if (!File.Exists(ConfigFile))
            return "";

        try
        {
            var json = File.ReadAllText(ConfigFile);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("apiKey", out var prop))
                return prop.GetString() ?? "";
        }
        catch { /* corrupt config — ignore */ }

        return "";
    }

    private static async Task<string> PromptAndSaveApiKeyAsync()
    {
        AnsiConsole.MarkupLine("[yellow]No API key found.[/] Set the [bold]PAPOA_API_KEY[/] environment variable, or enter one now.");
        var key = AnsiConsole.Prompt(
            new TextPrompt<string>("API key:")
                .Secret()
                .AllowEmpty());

        if (string.IsNullOrWhiteSpace(key))
            return "";

        if (AnsiConsole.Confirm("Save this key for future sessions?", defaultValue: true))
        {
            try
            {
                Directory.CreateDirectory(ConfigDirectory);
                var json = JsonSerializer.Serialize(new { apiKey = key });
                await File.WriteAllTextAsync(ConfigFile, json);
                AnsiConsole.MarkupLine($"[grey]Saved to {Markup.Escape(ConfigFile)}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Could not save config: {Markup.Escape(ex.Message)}[/]");
            }
        }

        return key;
    }
}
