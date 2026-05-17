using Papoa.Contract;
using Papoa.Entity;
using Spectre.Console;

namespace Papoa.Command;

public class InteractiveMode(
    IPostService postService,
    IFileUploadService fileUploadService,
    IPrintingService printingService)
{
    public async Task RunAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Papoa").Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Main Menu[/]")
                    .HighlightStyle("cyan1")
                    .AddChoices("Posts", "Exit"));

            if (choice == "Exit")
            {
                AnsiConsole.MarkupLine("[grey]Goodbye![/]");
                break;
            }

            await HandlePostsMenuAsync();
        }
    }

    private async Task HandlePostsMenuAsync()
    {
        while (true)
        {
            AnsiConsole.WriteLine();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Posts[/]")
                    .HighlightStyle("cyan1")
                    .AddChoices("List", "Create", "Update", "Delete", "← Back"));

            switch (choice)
            {
                case "List": await ListPostsAsync(); break;
                case "Create": await CreatePostAsync(); break;
                case "Update": await UpdatePostAsync(); break;
                case "Delete": await DeletePostAsync(); break;
                case "← Back": return;
            }
        }
    }

    // ─── List ────────────────────────────────────────────────────────────────

    private async Task ListPostsAsync()
    {
        List<Post> posts = [];
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Loading posts...", async _ =>
                {
                    posts = await postService.ListPostsAsync();
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            Pause();
            return;
        }

        if (posts.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No posts found.[/]");
            Pause();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Id[/]"))
            .AddColumn(new TableColumn("[bold]Title[/]"))
            .AddColumn(new TableColumn("[bold]Status[/]"))
            .AddColumn(new TableColumn("[bold]Content[/]"))
            .AddColumn(new TableColumn("[bold]Public[/]"))
            .AddColumn(new TableColumn("[bold]Tiers[/]"))
            .AddColumn(new TableColumn("[bold]Files[/]"))
            .AddColumn(new TableColumn("[bold]Created At[/]"))
            .AddColumn(new TableColumn("[bold]Patreon Updated At[/]"));

        foreach (var post in posts)
        {
            table.AddRow(
                new Text(post.Id),
                new Text(post.Title),
                new Markup(post.Status == "published" ? "[green]published[/]" : "[yellow]pending[/]"),
                new Text(Truncate(post.Content, 50)),
                new Text(post.IsPublic ? "Yes" : "No"),
                new Text(string.Join(", ", post.TierNames)),
                new Text(printingService.FilesProp(post.Files)),
                new Text(post.CreatedAt),
                new Text(post.PatreonUpdatedAt ?? "-"));
        }

        AnsiConsole.Write(table);
        Pause();
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    private async Task CreatePostAsync()
    {
        AnsiConsole.MarkupLine("[bold]Create Post[/]");
        AnsiConsole.WriteLine();

        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("Title:"));

        var contentInputMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Post content:")
                .HighlightStyle("cyan1")
                .AddChoices("None", "Inline text", "Text file"));

        string content = string.Empty;
        string contentFormat = "text/plain";

        if (contentInputMode == "Inline text")
        {
            content = AnsiConsole.Prompt(
                new TextPrompt<string>("Content [grey](optional)[/]:")
                    .AllowEmpty());

            contentFormat = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Content format:")
                    .AddChoices("text/plain", "text/markdown"));
        }
        else if (contentInputMode == "Text file")
        {
            var contentFiles = BrowseForFiles("Select post content file:");
            if (contentFiles.Count > 0)
            {
                try
                {
                    content = await File.ReadAllTextAsync(contentFiles[0]);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Could not read file: {Markup.Escape(ex.Message)}[/]");
                    Pause();
                    return;
                }
            }

            contentFormat = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Content format:")
                    .AddChoices("text/plain", "text/markdown"));
        }

        var isPublic = AnsiConsole.Confirm("Is the post public (available to everyone)?\n[grey]No = restricted to paying Patreons only[/]", defaultValue: false);

        List<string> tierNames = [];
        if (!isPublic)
        {
            tierNames = PromptForStringList("Tier names [grey](which tiers can access this post)[/]");
        }

        var collectionNames = PromptForStringList("Collection names [grey](collections this post belongs to)[/]");

        var addFiles = BrowseForFiles("Add files to attach:");

        string? password = null;
        if (AnsiConsole.Confirm("Encrypt with password?", defaultValue: false))
        {
            password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());
        }

        // Pre-encrypt so the size sent to the API matches the uploaded payload.
        var preparedFiles = new List<(string Path, byte[]? Bytes, long Size)>();
        foreach (var path in addFiles)
        {
            if (password is not null)
            {
                var raw = await File.ReadAllBytesAsync(path);
                var enc = fileUploadService.Encrypt(raw, password);
                preparedFiles.Add((path, enc, enc.Length));
            }
            else
            {
                preparedFiles.Add((path, null, new FileInfo(path).Length));
            }
        }

        var request = new PostCreateRequest
        {
            Title = title,
            Content = content,
            ContentFormat = contentFormat,
            IsPublic = isPublic,
            TierNames = tierNames.Count > 0 ? tierNames : null,
            CollectionNames = collectionNames.Count > 0 ? collectionNames : null,
            Encrypted = password != null,
            Files = preparedFiles.Select(f => new PostFile { Name = Path.GetFileName(f.Path), Size = f.Size }).ToList(),
            AttachmentFileNames = addFiles.Count > 0 ? addFiles.Select(f => Path.GetFileName(f)).ToList() : null,
        };

        PostCreateResult? result = null;
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Creating post...", async _ =>
                {
                    result = await postService.CreatePostAsync(request);
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            Pause();
            return;
        }

        if (result != null)
        {
            await UploadFilesAsync(result.UploadUrls, preparedFiles);
            var post = result.Post;
            AnsiConsole.MarkupLine("[green]Post created![/]");
            AnsiConsole.MarkupLine($"  [bold]Id:[/]       {Markup.Escape(post.Id)}");
            AnsiConsole.MarkupLine($"  [bold]Title:[/]    {Markup.Escape(post.Title)}");
            AnsiConsole.MarkupLine($"  [bold]Public:[/]   {(post.IsPublic ? "Yes" : "No")}");
            if (post.TierNames.Count > 0)
                AnsiConsole.MarkupLine($"  [bold]Tiers:[/]    {Markup.Escape(string.Join(", ", post.TierNames))}");
            if (post.CollectionNames.Count > 0)
                AnsiConsole.MarkupLine($"  [bold]Collections:[/] {Markup.Escape(string.Join(", ", post.CollectionNames))}");
        }

        Pause();
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    private async Task UpdatePostAsync()
    {
        var post = await SelectPostAsync("Select a post to update:");
        if (post == null) return;

        AnsiConsole.MarkupLine($"[bold]Updating:[/] {Markup.Escape(post.Title)}");
        AnsiConsole.WriteLine();

        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("Title:")
                .DefaultValue(post.Title));

        var contentInputMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Post content:")
                .HighlightStyle("cyan1")
                .AddChoices("Keep existing", "Inline text", "Text file"));

        string content = post.Content;
        string contentFormat = post.ContentFormat;

        if (contentInputMode == "Inline text")
        {
            content = AnsiConsole.Prompt(
                new TextPrompt<string>("Content:")
                    .AllowEmpty()
                    .DefaultValue(post.Content));

            contentFormat = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Content format:")
                    .AddChoices("text/plain", "text/markdown"));
        }
        else if (contentInputMode == "Text file")
        {
            var contentFiles = BrowseForFiles("Select post content file:");
            if (contentFiles.Count > 0)
            {
                try
                {
                    content = await File.ReadAllTextAsync(contentFiles[0]);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Could not read file: {Markup.Escape(ex.Message)}[/]");
                    Pause();
                    return;
                }
            }

            contentFormat = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Content format:")
                    .AddChoices("text/plain", "text/markdown"));
        }

        var isPublic = AnsiConsole.Confirm("Is the post public (available to everyone)?\n[grey]No = restricted to paying Patreons only[/]", defaultValue: post.IsPublic);

        List<string> tierNames = [];
        if (!isPublic)
        {
            AnsiConsole.MarkupLine($"[grey]Current tier names: {(post.TierNames.Count > 0 ? string.Join(", ", post.TierNames) : "none")}[/]");
            tierNames = PromptForStringList("Tier names [grey](which tiers can access this post)[/]");
        }

        AnsiConsole.MarkupLine($"[grey]Current collection names: {(post.CollectionNames.Count > 0 ? string.Join(", ", post.CollectionNames) : "none")}[/]");
        var collectionNames = PromptForStringList("Collection names [grey](collections this post belongs to)[/]");

        string? status = null;
        if (AnsiConsole.Confirm("Mark as published (confirm post)?", defaultValue: false))
        {
            status = "published";
        }

        var request = new PostUpdateRequest
        {
            Id = post.Id,
            Title = title,
            Content = content,
            ContentFormat = contentFormat,
            IsPublic = isPublic,
            TierNames = tierNames.Count > 0 ? tierNames : null,
            CollectionNames = collectionNames.Count > 0 ? collectionNames : null,
            Status = status,
        };

        PostUpdateResult? result = null;
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Updating post...", async _ =>
                {
                    result = await postService.UpdatePostAsync(request);
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            Pause();
            return;
        }

        if (result != null)
        {
            AnsiConsole.MarkupLine("[green]Post updated![/]");
        }

        Pause();
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    private async Task DeletePostAsync()
    {
        var post = await SelectPostAsync("Select a post to delete:");
        if (post == null) return;

        if (!AnsiConsole.Confirm($"[red]Delete[/] \"{Markup.Escape(post.Title)}\"?", defaultValue: false))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            Pause();
            return;
        }

        try
        {
            await AnsiConsole.Status()
                .StartAsync("Deleting post...", async _ =>
                {
                    await postService.DeletePostAsync(post.Id);
                });
            AnsiConsole.MarkupLine("[green]Post deleted.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
        }

        Pause();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<Post?> SelectPostAsync(string prompt)
    {
        List<Post> posts = [];
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Loading posts...", async _ =>
                {
                    posts = await postService.ListPostsAsync();
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            Pause();
            return null;
        }

        if (posts.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No posts found.[/]");
            Pause();
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<Post>()
                .Title(prompt)
                .HighlightStyle("cyan1")
                .UseConverter(p => $"{p.Id}  {p.Title}")
                .AddChoices(posts));
    }

    /// <summary>
    /// Text-input loop for entering a list of strings. Enter a blank line to finish.
    /// </summary>
    private static List<string> PromptForStringList(string title)
    {
        var items = new List<string>();
        AnsiConsole.MarkupLine($"[bold]{title}[/] [grey](enter value, leave blank to finish)[/]");

        while (true)
        {
            var value = AnsiConsole.Prompt(
                new TextPrompt<string>("Value [grey](blank = done)[/]:")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(value))
                break;

            var trimmed = value.Trim();
            if (items.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(trimmed)} already in the list.[/]");
            }
            else
            {
                items.Add(trimmed);
                AnsiConsole.MarkupLine($"[green]Added:[/] {Markup.Escape(trimmed)}");
            }
        }

        return items;
    }

    /// <summary>
    /// Arrow-key file browser. Navigates directories; selecting a file adds it to the list.
    /// </summary>
    private static List<string> BrowseForFiles(string title)
    {
        const string DoneLabel = "✓  Done";
        const string DirPrefix = "📁 ";
        const string FilePrefix = "📄 ";
        const string ParentLabel = "📁 ..";

        var selected = new List<string>();
        var currentDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!Directory.Exists(currentDir))
            currentDir = Directory.GetCurrentDirectory();

        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/] [grey](arrows to navigate, enter to open/select, choose ✓ Done when finished)[/]");

        while (true)
        {
            var selectedLabel = selected.Count == 0
                ? "[grey]none[/]"
                : string.Join(", ", selected.Select(f => $"[cyan1]{Markup.Escape(Path.GetFileName(f))}[/]"));

            AnsiConsole.MarkupLine($"[grey]Current:[/] [yellow]{Markup.Escape(currentDir)}[/]");
            AnsiConsole.MarkupLine($"[grey]Selected:[/] {selectedLabel}");

            var choices = new List<string> { DoneLabel };
            if (Directory.GetParent(currentDir) != null)
                choices.Add(ParentLabel);

            try
            {
                choices.AddRange(Directory.GetDirectories(currentDir)
                    .OrderBy(d => d)
                    .Select(d => DirPrefix + Path.GetFileName(d)));
                choices.AddRange(Directory.GetFiles(currentDir)
                    .OrderBy(f => f)
                    .Select(f => FilePrefix + Path.GetFileName(f)));
            }
            catch (UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLine("[red]Access denied.[/]");
                currentDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
                continue;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(18)
                    .HighlightStyle("cyan1")
                    .AddChoices(choices));

            if (choice == DoneLabel)
                break;

            if (choice == ParentLabel)
            {
                currentDir = Directory.GetParent(currentDir)!.FullName;
                continue;
            }

            if (choice.StartsWith(DirPrefix))
            {
                currentDir = Path.Combine(currentDir, choice[DirPrefix.Length..]);
                continue;
            }

            // File selected
            var fullPath = Path.Combine(currentDir, choice[FilePrefix.Length..]);
            if (selected.Contains(fullPath))
            {
                AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(Path.GetFileName(fullPath))} is already added.[/]");
            }
            else
            {
                selected.Add(fullPath);
                AnsiConsole.MarkupLine($"[green]Added:[/] {Markup.Escape(Path.GetFileName(fullPath))}");
            }

            if (!AnsiConsole.Confirm("Add another file?", defaultValue: false))
                break;
        }

        return selected;
    }

    private async Task UploadFilesAsync(
        List<PostUploadSession> uploadUrls,
        List<(string Path, byte[]? Bytes, long Size)> preparedFiles)
    {
        for (var i = 0; i < uploadUrls.Count; i++)
        {
            var session = uploadUrls[i];
            var (filePath, encryptedBytes, _) = preparedFiles[i];
            await AnsiConsole.Status()
                .StartAsync($"Uploading [cyan1]{Markup.Escape(Path.GetFileName(filePath))}[/]...", async _ =>
                {
                    if (encryptedBytes is not null)
                        await fileUploadService.UploadBytesAsync(session, encryptedBytes, Path.GetFileName(filePath));
                    else
                        await fileUploadService.UploadFileAsync(session, filePath, null);
                });
            AnsiConsole.MarkupLine($"  Uploaded [cyan1]{Markup.Escape(Path.GetFileName(filePath))}[/].");
        }
    }

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : string.Concat(s.AsSpan(0, max), "…");
}
