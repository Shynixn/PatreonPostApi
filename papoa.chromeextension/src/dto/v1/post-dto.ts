export interface PostFileDTO {
  name: string;
  size?: number;
  url?: string;
}

export interface PostGetResultDTO {
  id: string;
  title: string;
  content: string;
  contentFormat: string;
  files: PostFileDTO[];
  encrypted: boolean;
  isPublic: boolean;
  tierNames: string[];
  collectionNames: string[];
  publishDateUtc?: string;
  tags: string[];
  imageVideoAudioFileNames: string[];
  attachmentFileNames: string[];
  pending?: {
    title: string;
    content: string;
    contentFormat: string;
    isPublic: boolean;
    tierNames: string[];
    collectionNames: string[];
    publishDateUtc?: string;
    tags: string[];
    addFiles: PostFileDTO[];
    removeFiles: PostFileDTO[];
    imageVideoAudioFileNames: string[];
    attachmentFileNames: string[];
  };
  patreonPostId: string;
  patreonUpdatedAt: string;
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
  content: string;
  contentFormat?: string;
  isPublic: boolean;
  tierNames?: string[];
  collectionNames?: string[];
  publishDateUtc?: string;
  tags?: string[];
  ttlDays?: number;
  addFiles?: PostFileDTO[];
  encrypted?: boolean;
  imageVideoAudioFileNames?: string[];
  attachmentFileNames?: string[];
}

export interface PostCreateResultDTO {
  post: PostGetResultDTO;
  uploadUrls: PostUploadSessionDTO[];
}

export interface PostUpdateRequestDTO {
  title: string;
  content: string;
  contentFormat?: string;
  isPublic: boolean;
  tierNames?: string[];
  collectionNames?: string[];
  publishDateUtc?: string;
  tags?: string[];
  patreonPostId?: string;
  addFiles?: PostFileDTO[];
  removeFiles?: PostFileDTO[];
  imageVideoAudioFileNames?: string[];
  attachmentFileNames?: string[];
}

export interface PostUpdateResultDTO {
  post: PostGetResultDTO;
  uploadUrls: PostUploadSessionDTO[];
}
