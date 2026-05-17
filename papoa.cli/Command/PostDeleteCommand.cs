using System.CommandLine;
using Papoa.Contract;

namespace Papoa.Command;

public class PostDeleteCommand(IPostService postService, IPrintingService printingService)
{
    /// <summary>
    /// Builds the <c>post delete</c> sub-command.
    /// </summary>
    public System.CommandLine.Command Build()
    {
        var command = new System.CommandLine.Command("delete", "Delete a post");

        var idOption = new Option<string>("--id") { Required = true };
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
            var id = parseResult.GetValue(idOption)!;
            var outputFormat = parseResult.GetValue(outputFormatOption)!;

            await postService.DeletePostAsync(id);

            printingService.PrintMessage($"Post {id} deleted.", outputFormat);
        });

        return command;
    }
}
