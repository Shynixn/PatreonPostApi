import { PostGetResultDTO, PostUpdateRequestDTO } from "../dto/v1/post-dto.js";

export class PostService {
  constructor(
    private readonly baseUrl: string,
    private readonly apiKey: string,
  ) {}

  private get headers(): Record<string, string> {
    return { "Content-Type": "application/json", "x-api-key": this.apiKey };
  }

  async fetchPendingPosts(): Promise<PostGetResultDTO[]> {
    const response = await fetch(`${this.baseUrl}/api/v1/post`, {
      method: "GET",
      headers: this.headers,
    });
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    const body = await response.json();
    const posts: PostGetResultDTO[] = Array.isArray(body?.data)
      ? body.data
      : [];
    return posts
      .filter((p) => p.pending != null)
      .sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      );
  }

  async getPostWithDownloadUrls(postId: string): Promise<PostGetResultDTO> {
    const response = await fetch(
      `${this.baseUrl}/api/v1/post/${encodeURIComponent(postId)}?includeDownloads=true`,
      {
        method: "GET",
        headers: this.headers,
      },
    );
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    const body = await response.json();
    return body?.data as PostGetResultDTO;
  }

  async confirmPost(
    post: PostGetResultDTO,
    patreonPostId?: string,
  ): Promise<void> {
    const pending = post.pending;
    if (!pending) return;
    const body: PostUpdateRequestDTO = {
      title: pending.title,
      content: pending.content,
      contentFormat: pending.contentFormat,
      isPublic: pending.isPublic,
      tierNames: pending.tierNames,
      collectionNames: pending.collectionNames,
      publishDateUtc: pending.publishDateUtc,
      tags: pending.tags,
      patreonPostId,
    };
    const response = await fetch(
      `${this.baseUrl}/api/v1/post/${encodeURIComponent(post.id)}?confirm=true`,
      {
        method: "PUT",
        headers: this.headers,
        body: JSON.stringify(body),
      },
    );
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
  }

  async deletePost(postId: string): Promise<void> {
    const response = await fetch(
      `${this.baseUrl}/api/v1/post/${encodeURIComponent(postId)}`,
      {
        method: "DELETE",
        headers: this.headers,
      },
    );
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
  }
}
