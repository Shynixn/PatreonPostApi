using System.CommandLine;
using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Command;

public class PostCreateCommand(IPostService postService, IFileUploadService fileUploadService, IPrintingService printingService)
{
    /// <summary>
    /// Builds the <c>post create</c> sub-command.
    /// </summary>
    public System.CommandLine.Command Build()
    {
        var command = new System.CommandLine.Command("create", "Create a new post");

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
        var publishDateUtcOption = new Option<string?>("--publish-date-utc") { Required = false };
        var tagsOption = new Option<List<string>>("--tag") { Required = false };
        var ttlDaysOption = new Option<int?>("--ttl-days") { Required = false };
        var addFilesOption = new Option<List<string>>("--add-file") { Required = false };
        var passwordOption = new Option<string?>("--password") { Required = false };
        var outputFormatOption = new Option<string>("--output-format")
        {
            Required = false,
            DefaultValueFactory = _ => "text/plain",
        };
        outputFormatOption.AcceptOnlyFromAmong("text/plain");

        command.Add(titleOption);
        command.Add(contentOption);
        command.Add(contentFormatOption);
        command.Add(contentFileOption);
        command.Add(isPublicOption);
        command.Add(tierNamesOption);
        command.Add(collectionNamesOption);
        command.Add(publishDateUtcOption);
        command.Add(tagsOption);
        command.Add(ttlDaysOption);
        command.Add(addFilesOption);
        command.Add(passwordOption);
        command.Add(outputFormatOption);

        command.SetAction(async parseResult =>
        {
            var title = parseResult.GetValue(titleOption)!;
            var content = parseResult.GetValue(contentOption) ?? string.Empty;
            var contentFormat = parseResult.GetValue(contentFormatOption)!;
            var isPublic = parseResult.GetValue(isPublicOption);
            var tierNames = parseResult.GetValue(tierNamesOption) ?? [];
            var collectionNames = parseResult.GetValue(collectionNamesOption) ?? [];
            var publishDateUtc = parseResult.GetValue(publishDateUtcOption);
            var tags = parseResult.GetValue(tagsOption) ?? [];
            var ttlDays = parseResult.GetValue(ttlDaysOption);
            var outputFormat = parseResult.GetValue(outputFormatOption)!;
            var addFiles = parseResult.GetValue(addFilesOption) ?? [];
            var password = parseResult.GetValue(passwordOption);
            var contentFile = parseResult.GetValue(contentFileOption);

            if (!string.IsNullOrEmpty(contentFile))
            {
                content = await File.ReadAllTextAsync(contentFile);
            }

            var request = new PostCreateRequest
            {
                Title = title,
                Content = content,
                ContentFormat = contentFormat,
                IsPublic = isPublic,
                TierNames = tierNames.Count > 0 ? tierNames : null,
                CollectionNames = collectionNames.Count > 0 ? collectionNames : null,
                PublishDateUtc = publishDateUtc,
                Tags = tags.Count > 0 ? tags : null,
                TtlDays = ttlDays,
                Encrypted = !string.IsNullOrEmpty(password),
                AddFiles = addFiles.Select(e => new PostFile { Name = Path.GetFileName(e), Size = new FileInfo(e).Length }).ToList(),
            };

            var createResult = await postService.CreatePostAsync(request);

            for (var i = 0; i < createResult.UploadUrls.Count; i++)
            {
                var uploadSession = createResult.UploadUrls[i];
                var filePath = addFiles[i];

                if (outputFormat.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Uploading {filePath}...");
                }

                await fileUploadService.UploadFileAsync(uploadSession, filePath, password);

                if (outputFormat.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Uploaded {filePath}.");
                }
            }

            if (outputFormat.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                var post = createResult.Post;
                Console.WriteLine("Post Created");
                Console.WriteLine($"  Id:               {post.Id}");
                Console.WriteLine($"  Title:            {printingService.StringProp(post.Title, post.Pending?.Title)}");
                Console.WriteLine($"  Content:          {printingService.StringProp(post.Content, post.Pending?.Content)}");
                Console.WriteLine($"  Is Public:        {post.IsPublic}");
                Console.WriteLine($"  Tier Names:       {string.Join(", ", post.TierNames)}");
                Console.WriteLine($"  Collection Names: {string.Join(", ", post.CollectionNames)}");
                Console.WriteLine($"  Tags:             {string.Join(", ", post.Tags)}");
                Console.WriteLine($"  Files:            {printingService.FilesProp(post.Files, post.Pending?.AddFiles, post.Pending?.RemoveFiles)}");
                Console.WriteLine($"  Created At:       {post.CreatedAt}");
            }
        });

        return command;
    }
}
