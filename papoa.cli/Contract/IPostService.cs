using Papoa.Entity;

namespace Papoa.Contract;

public interface IPostService
{
    /// <summary>
    /// Creates a new post and returns the result, including any upload sessions.
    /// </summary>
    Task<PostCreateResult> CreatePostAsync(PostCreateRequest request);

    /// <summary>
    /// Updates an existing post and returns the result, including any upload sessions.
    /// </summary>
    Task<PostUpdateResult> UpdatePostAsync(PostUpdateRequest request);

    /// <summary>
    /// Deletes the post with the given ID.
    /// </summary>
    Task DeletePostAsync(string id);

    /// <summary>
    /// Returns all posts, or a single post if <paramref name="id"/> is provided.
    /// </summary>
    Task<List<Post>> ListPostsAsync(string? id = null);
}
