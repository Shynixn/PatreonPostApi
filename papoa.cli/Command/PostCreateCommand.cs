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
        var textOption = new Option<string>("--text") { Required = false };
        var textFormatOption = new Option<string>("--text-format")
        {
            Required = false,
            DefaultValueFactory = _ => "text/plain",
        };
        textFormatOption.AcceptOnlyFromAmong("text/plain", "text/markdown");
        var textFileOption = new Option<string>("--text-file") { Required = false };
        var addFilesOption = new Option<List<string>>("--add-file") { Required = false };
        var passwordOption = new Option<string?>("--password") { Required = false };
        var outputFormatOption = new Option<string>("--output-format")
        {
            Required = false,
            DefaultValueFactory = _ => "text/plain",
        };
        outputFormatOption.AcceptOnlyFromAmong("text/plain");

        command.Add(titleOption);
        command.Add(textOption);
        command.Add(textFormatOption);
        command.Add(textFileOption);
        command.Add(addFilesOption);
        command.Add(passwordOption);
        command.Add(outputFormatOption);

        command.SetAction(async parseResult =>
        {
            var title = parseResult.GetValue(titleOption)!;
            var text = parseResult.GetValue(textOption) ?? string.Empty;
            var textFormat = parseResult.GetValue(textFormatOption)!;
            var outputFormat = parseResult.GetValue(outputFormatOption)!;
            var addFiles = parseResult.GetValue(addFilesOption) ?? [];
            var password = parseResult.GetValue(passwordOption);
            var textFile = parseResult.GetValue(textFileOption);

            if (!string.IsNullOrEmpty(textFile))
            {
                text = await File.ReadAllTextAsync(textFile);
            }

            var request = new PostCreateRequest
            {
                Title = title,
                Text = text,
                TextFormat = textFormat,
                Encrypted = !string.IsNullOrEmpty(password),
                AddFiles = addFiles.Select(e => new PostFile { Name = Path.GetFileName(e) }).ToList(),
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
                Console.WriteLine($"  Id:         {post.Id}");
                Console.WriteLine($"  Title:      {printingService.StringProp(post.Title, post.Pending?.Title)}");
                Console.WriteLine($"  Text:       {printingService.StringProp(post.Text, post.Pending?.Text)}");
                Console.WriteLine($"  Files:      {printingService.FilesProp(post.Files, post.Pending?.AddFiles, post.Pending?.RemoveFiles)}");
                Console.WriteLine($"  Created At: {post.CreatedAt}");
            }
        });

        return command;
    }
}
