export interface PostFileDTO {
  name: string;
  url?: string;
}

export interface PostPendingDTO {
  title: string;
  text: string;
  textFormat: string;
  addFiles: PostFileDTO[];
  removeFiles: PostFileDTO[];
}

export interface PostGetResultDTO {
  id: string;
  title: string;
  text: string;
  textFormat: string;
  files: PostFileDTO[];
  encrypted: boolean;
  pending?: PostPendingDTO;
  updatedAt: string;
  createdAt: string;
}

export interface PostUploadSessionDTO {
  name: string;
  url: string;
  fields: Record<string, string>;
}

export interface PostCreateRequestDTO {
  title: string;
  text: string;
  textFormat?: string;
  addFiles?: PostFileDTO[];
  encrypted?: boolean;
}

export interface PostCreateResultDTO {
  post: PostGetResultDTO;
  uploadUrls: PostUploadSessionDTO[];
}

export interface PostUpdateRequestDTO {
  title: string;
  text: string;
  textFormat?: string;
  patreonPostId?: string;
  addFiles?: PostFileDTO[];
  removeFiles?: PostFileDTO[];
}

export interface PostUpdateResultDTO {
  post: PostGetResultDTO;
  uploadUrls: PostUploadSessionDTO[];
}
