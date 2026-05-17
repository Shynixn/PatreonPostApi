using System.CommandLine;
using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Command;

public class PostUpdateCommand(IPostService postService, IPrintingService printingService)
{
    /// <summary>
    /// Builds the <c>post update</c> sub-command.
    /// </summary>
    public System.CommandLine.Command Build()
    {
        var command = new System.CommandLine.Command("update", "Update an existing post");

        var idOption = new Option<string>("--id") { Required = true };
        var titleOption = new Option<string>("--title") { Required = true };
        var contentOption = new Option<string>("--content") { Required = false };
        var contentFormatOption = new Option<string>("--content-format")
        {
            Required = false,
            DefaultValueFactory = _ => "text/plain",
        };
        contentFormatOption.AcceptOnlyFromAmong("text/plain", "text/markdown");
        var contentFileOption = new Option<string>("--content-file") { Required = false };
        var isPublicOption = new Option<bool>("--is-public") { Required = false, DefaultValueFactory = _ => false };
        var tierNamesOption = new Option<List<string>>("--tier-name") { Required = false };
        var collectionNamesOption = new Option<List<string>>("--collection-name") { Required = false };
        var tagsOption = new Option<List<string>>("--tag") { Required = false };
        var statusOption = new Option<string?>("--status") { Required = false };
        statusOption.AcceptOnlyFromAmong("pending", "published");
        var patreonPostIdOption = new Option<string?>("--patreon-post-id") { Required = false };
        var outputFormatOption = new Option<string>("--output-format")
        {
            Required = false,
            DefaultValueFactory = _ => "text/plain",
        };
        outputFormatOption.AcceptOnlyFromAmong("text/plain", "application/json");

        command.Add(idOption);
        command.Add(titleOption);
        command.Add(contentOption);
        command.Add(contentFormatOption);
        command.Add(contentFileOption);
        command.Add(isPublicOption);
        command.Add(tierNamesOption);
        command.Add(collectionNamesOption);
        command.Add(tagsOption);
        command.Add(statusOption);
        command.Add(patreonPostIdOption);
        command.Add(outputFormatOption);

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetValue(idOption)!;
            var title = parseResult.GetValue(titleOption)!;
            var content = parseResult.GetValue(contentOption);
            var contentFormat = parseResult.GetValue(contentFormatOption)!;
            var isPublic = parseResult.GetValue(isPublicOption);
            var tierNames = parseResult.GetValue(tierNamesOption) ?? [];
            var collectionNames = parseResult.GetValue(collectionNamesOption) ?? [];
            var tags = parseResult.GetValue(tagsOption) ?? [];
            var status = parseResult.GetValue(statusOption);
            var patreonPostId = parseResult.GetValue(patreonPostIdOption);
            var outputFormat = parseResult.GetValue(outputFormatOption)!;
            var contentFile = parseResult.GetValue(contentFileOption);

            if (!string.IsNullOrEmpty(contentFile))
            {
                content = await File.ReadAllTextAsync(contentFile);
            }

            var request = new PostUpdateRequest
            {
                Id = id,
                Title = title,
                Content = content,
                ContentFormat = contentFormat,
                IsPublic = isPublic,
                TierNames = tierNames.Count > 0 ? tierNames : null,
                CollectionNames = collectionNames.Count > 0 ? collectionNames : null,
                Tags = tags.Count > 0 ? tags : null,
                Status = status,
                PatreonPostId = patreonPostId,
            };

            var updateResult = await postService.UpdatePostAsync(request);

            printingService.PrintMessage("Post Updated", outputFormat);
            printingService.PrintPost(updateResult.Post, outputFormat);
        });

        return command;
    }
}

