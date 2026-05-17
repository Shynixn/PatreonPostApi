using System.CommandLine;
using Papoa.Contract;

namespace Papoa.Command;

public class PostListCommand(IPostService postService, IPrintingService printingService)
{
    /// <summary>
    /// Builds the <c>post list</c> sub-command.
    /// </summary>
    public System.CommandLine.Command Build()
    {
        var command = new System.CommandLine.Command("list", "List posts");

        var idOption = new Option<string?>("--id") { Required = false };
        var outputFormatOption = new Option<string>("--output-format")
        {
            Required = false,
            DefaultValueFactory = _ => "text/plain",
        };
        outputFormatOption.AcceptOnlyFromAmong("text/plain", "application/json");

        command.Add(idOption);
        command.Add(outputFormatOption);

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetValue(idOption);
            var outputFormat = parseResult.GetValue(outputFormatOption)!;

            var posts = await postService.ListPostsAsync(id);

            printingService.PrintPosts(posts, outputFormat);
        });

        return command;
    }
}
