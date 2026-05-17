using System.Text.Json;
using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Service;

public class PrintingService : IPrintingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string FilesProp(List<PostFile> files) =>
        files.Count == 0 ? "[]" : $"[{string.Join(", ", files.Select(f => f.Name))}]";

    public void PrintMessage(string message, string outputFormat)
    {
        if (outputFormat.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine(message);
    }

    public void PrintPost(Post post, string outputFormat)
    {
        if (outputFormat.Equals("application/json", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(JsonSerializer.Serialize(post, JsonOptions));
            return;
        }

        Console.WriteLine($"  Id:                    {post.Id}");
        Console.WriteLine($"  Title:                 {post.Title}");
        Console.WriteLine($"  Content:               {post.Content}");
        Console.WriteLine($"  Content Format:        {post.ContentFormat}");
        Console.WriteLine($"  Status:                {post.Status}");
        Console.WriteLine($"  Is Public:             {post.IsPublic}");
        Console.WriteLine($"  Encrypted:             {post.Encrypted}");
        Console.WriteLine($"  Tier Names:            {string.Join(", ", post.TierNames)}");
        Console.WriteLine($"  Collection Names:      {string.Join(", ", post.CollectionNames)}");
        Console.WriteLine($"  Tags:                  {string.Join(", ", post.Tags)}");
        Console.WriteLine($"  Files:                 {FilesProp(post.Files)}");
        Console.WriteLine($"  Photo Attachments:     {string.Join(", ", post.PhotoAttachmentFileNames)}");
        Console.WriteLine($"  Attachments:           {string.Join(", ", post.AttachmentFileNames)}");
        Console.WriteLine($"  Publish Date (UTC):    {post.PublishDateUtc ?? "-"}");
        Console.WriteLine($"  Patreon Post Id:       {post.PatreonPostId}");
        Console.WriteLine($"  Created At:            {post.CreatedAt}");
        Console.WriteLine($"  Updated At:            {post.UpdatedAt}");
        Console.WriteLine($"  Expires At:            {post.ExpiresAt}");
        Console.WriteLine($"  Files Expire At:       {post.FilesExpireAt}");
        Console.WriteLine($"  Patreon Updated At:    {post.PatreonUpdatedAt ?? "-"}");
    }

    public void PrintPosts(List<Post> posts, string outputFormat)
    {
        if (outputFormat.Equals("application/json", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(JsonSerializer.Serialize(posts, JsonOptions));
            return;
        }

        foreach (var post in posts)
        {
            Console.WriteLine($"  Id:                    {post.Id}");
            Console.WriteLine($"  Title:                 {post.Title}");
            var contentPreview = post.Content.Length > 80 ? post.Content[..80] + "\u2026" : post.Content;
            Console.WriteLine($"  Content:               {contentPreview}");
            Console.WriteLine($"  Content Format:        {post.ContentFormat}");
            Console.WriteLine($"  Status:                {post.Status}");
            Console.WriteLine($"  Is Public:             {post.IsPublic}");
            Console.WriteLine($"  Encrypted:             {post.Encrypted}");
            Console.WriteLine($"  Tier Names:            {string.Join(", ", post.TierNames)}");
            Console.WriteLine($"  Collection Names:      {string.Join(", ", post.CollectionNames)}");
            Console.WriteLine($"  Tags:                  {string.Join(", ", post.Tags)}");
            Console.WriteLine($"  Files:                 {FilesProp(post.Files)}");
            Console.WriteLine($"  Photo Attachments:     {string.Join(", ", post.PhotoAttachmentFileNames)}");
            Console.WriteLine($"  Attachments:           {string.Join(", ", post.AttachmentFileNames)}");
            Console.WriteLine($"  Publish Date (UTC):    {post.PublishDateUtc ?? "-"}");
            Console.WriteLine($"  Patreon Post Id:       {post.PatreonPostId}");
            Console.WriteLine($"  Created At:            {post.CreatedAt}");
            Console.WriteLine($"  Updated At:            {post.UpdatedAt}");
            Console.WriteLine($"  Expires At:            {post.ExpiresAt}");
            Console.WriteLine($"  Files Expire At:       {post.FilesExpireAt}");
            Console.WriteLine($"  Patreon Updated At:    {post.PatreonUpdatedAt ?? "-"}");
            Console.WriteLine();
        }
    }
}
