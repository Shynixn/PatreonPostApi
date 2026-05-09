using System.CommandLine;
using Papoa.Contract;

namespace Papoa.Command;

public class PostCommand(IPostService postService, IFileUploadService fileUploadService, IPrintingService printingService)
{
    /// <summary>
    /// Builds the top-level <c>post</c> command with all sub-commands.
    /// </summary>
    public System.CommandLine.Command Build()
    {
        var command = new System.CommandLine.Command("post", "Manage posts");
        command.Add(new PostCreateCommand(postService, fileUploadService, printingService).Build());
        command.Add(new PostUpdateCommand(postService, fileUploadService, printingService).Build());
        command.Add(new PostDeleteCommand(postService).Build());
        command.Add(new PostListCommand(postService, printingService).Build());
        return command;
    }
}
