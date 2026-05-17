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
        outputFormatOption.AcceptOnlyFromAmong("text/plain");

        command.Add(idOption);
        command.Add(outputFormatOption);

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetValue(idOption);
            var outputFormat = parseResult.GetValue(outputFormatOption)!;

            var posts = await postService.ListPostsAsync(id);

            if (outputFormat.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var post in posts)
                {
                    Console.WriteLine($"Id:               {post.Id}");
                    Console.WriteLine($"Title:            {post.Title}");
                    Console.WriteLine($"Content:          {post.Content}");
                    Console.WriteLine($"Status:           {post.Status}");
                    Console.WriteLine($"Is Public:        {post.IsPublic}");
                    Console.WriteLine($"Tier Names:       {string.Join(", ", post.TierNames)}");
                    Console.WriteLine($"Collection Names: {string.Join(", ", post.CollectionNames)}");
                    Console.WriteLine($"Tags:             {string.Join(", ", post.Tags)}");
                    Console.WriteLine($"Files:            {printingService.FilesProp(post.Files)}");
                    Console.WriteLine($"Created At:       {post.CreatedAt}");
                    Console.WriteLine();
                }
            }
        });

        return command;
    }
}
