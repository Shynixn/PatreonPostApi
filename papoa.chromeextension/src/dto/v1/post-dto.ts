export interface PostCreateRequestDTO {
  title: string;
  content?: string;
  contentFormat?: string;
  isPublic?: boolean;
  tierNames?: string[];
  collectionNames?: string[];
  publishDateUtc?: string;
  tags?: string[];
  ttlDays?: number;
  encrypted?: boolean;
  files?: PostFileDTO[];
  photoAttachmentFileNames?: string[];
  attachmentFileNames?: string[];
}

export interface PostCreateResultDTO {
  post: PostGetResultDTO;
  uploadUrls: PostUploadSessionDTO[];
}

export interface PostUpdateRequestDTO {
  title: string;
  content?: string;
  status?: "pending" | "published";
  contentFormat?: string;
  isPublic?: boolean;
  tierNames?: string[];
  collectionNames?: string[];
  tags?: string[];
  patreonPostId?: string;
}

export interface PostUpdateResultDTO {
  post: PostGetResultDTO;
}

export interface PostGetResultDTO {
  id: string;
  title: string;
  content: string;
  status: "pending" | "published";
  contentFormat: string;
  encrypted: boolean;
  isPublic: boolean;
  tierNames: string[];
  collectionNames: string[];
  publishDateUtc?: string;
  tags: string[];
  files: PostFileDTO[];
  photoAttachmentFileNames: string[];
  attachmentFileNames?: string[];
  patreonPostId: string;
  patreonUpdatedAt: string;
  updatedAt: string;
  createdAt: string;
  expiresAt: string;
  filesExpireAt: string;
}

export interface PostFileDTO {
  name: string;
  size?: number;
  url?: string;
}

export interface PostUploadSessionDTO {
  name: string;
  url: string;
  fields: Record<string, string>;
}
