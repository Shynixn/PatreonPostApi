using System.CommandLine;
using Papoa.Command;
using Papoa.Service;

namespace Papoa;

class Program
{
    /// <summary>
    /// Entry point. Configures services and dispatches CLI commands.
    /// </summary>
    static async Task<int> Main(string[] args)
    {
        // Environment Variables
        var baseUrl = Environment.GetEnvironmentVariable("PAPOA_BASE_URL")
            ?? "https://api.papoa.shynixn.com";
        var apiKey = Environment.GetEnvironmentVariable("PAPOA_API_KEY") ?? "";

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
}
