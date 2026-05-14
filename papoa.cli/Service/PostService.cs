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

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}",
                null,
                response.StatusCode);
        }
    }

    /// <summary>
    /// Creates a new post via POST /api/v1/post and returns the create result.
    /// </summary>
    public async Task<PostCreateResult> CreatePostAsync(PostCreateRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/post", request, JsonOptions);
        await EnsureSuccessAsync(response);

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
        await EnsureSuccessAsync(response);

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
        await EnsureSuccessAsync(response);
    }

    /// <summary>
    /// Lists all posts via GET /api/v1/post, or fetches a single post if <paramref name="id"/> is provided.
    /// </summary>
    public async Task<List<Post>> ListPostsAsync(string? id = null)
    {
        if (id is not null)
        {
            var response = await httpClient.GetAsync($"/api/v1/post/{id}");
            await EnsureSuccessAsync(response);

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
            await EnsureSuccessAsync(response);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<Post>>>(JsonOptions);

            if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
            {
                throw new InvalidOperationException(apiResponse?.Error ?? "Unknown API error");
            }

            return apiResponse.Data;
        }
    }
}
