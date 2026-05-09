using System.Net.Http.Json;
using System.Text.Json;
using Papoa.Contract;
using Papoa.Entity;

namespace Papoa.Service;

public class PostService(HttpClient httpClient) : IPostService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Creates a new post via POST /api/v1/post and returns the create result.
    /// </summary>
    public async Task<PostCreateResult> CreatePostAsync(PostCreateRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/post", request, JsonOptions);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PostCreateResult>>(JsonOptions);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
        {
            throw new InvalidOperationException(apiResponse?.Error ?? "Unknown API error");
        }

        return apiResponse.Data;
    }

    /// <summary>
    /// Updates an existing post via PUT /api/v1/post/{id} and returns the update result.
    /// </summary>
    public async Task<PostUpdateResult> UpdatePostAsync(PostUpdateRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"/api/v1/post/{request.Id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PostUpdateResult>>(JsonOptions);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
        {
            throw new InvalidOperationException(apiResponse?.Error ?? "Unknown API error");
        }

        return apiResponse.Data;
    }

    /// <summary>
    /// Deletes a post via DELETE /api/v1/post/{id}.
    /// </summary>
    public async Task DeletePostAsync(string id)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/post/{id}");
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Lists all posts via GET /api/v1/post, or fetches a single post if <paramref name="id"/> is provided.
    /// </summary>
    public async Task<List<Post>> ListPostsAsync(string? id = null)
    {
        if (id is not null)
        {
            var response = await httpClient.GetAsync($"/api/v1/post/{id}");
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Post>>(JsonOptions);

            if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
            {
                throw new InvalidOperationException(apiResponse?.Error ?? "Unknown API error");
            }

            return [apiResponse.Data];
        }
        else
        {
            var response = await httpClient.GetAsync("/api/v1/post");
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<Post>>>(JsonOptions);

            if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
            {
                throw new InvalidOperationException(apiResponse?.Error ?? "Unknown API error");
            }

            return apiResponse.Data;
        }
    }
}
