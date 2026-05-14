# Papoa REST API

Personal reference for the REST API consumed by the Papoa CLI.

## Base URL & Authentication

| Item        | Value                           |
| ----------- | ------------------------------- |
| Default URL | `https://api.papoa.shynixn.com` |
| Auth header | `x-api-key: <your-api-key>`     |

All requests must include the `x-api-key` header. Every response body follows the envelope:

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

On failure `success` is `false`, `data` is `null`, and `error` contains a message string.

---

## Data Types

### `PostFile`

```json
{
  "name": "image.png",
  "size": 204800,
  "url": "https://..."
}
```

| Field  | Type    | Description        |
| ------ | ------- | ------------------ |
| `name` | string  | Filename           |
| `size` | integer | File size in bytes |

---

### `PostUploadSession`

Returned after creating or updating a post when files need to be uploaded.

```json
{
  "name": "image.png",
  "url": "https://s3.example.com/upload",
  "fields": {
    "key": "posts/abc123/image.png",
    "Content-Type": "image/png"
  }
}
```

Upload by sending a `multipart/form-data` `POST` to `url` including all `fields` as form fields, with the file appended last under the key `file`.

---

### `PostGetResult`

```json
{
  "id": "abc123",
  "title": "My Post",
  "content": "Hello world",
  "contentFormat": "text/plain",
  "files": [],
  "encrypted": false,
  "isPublic": true,
  "tierNames": [],
  "collectionNames": ["Dev Log"],
  "publishDateUtc": null,
  "tags": ["release"],
  "pending": null,
  "patreonPostId": "12345678",
  "patreonUpdatedAt": "2026-05-14T10:00:00Z",
  "updatedAt": "2026-05-14T10:00:00Z",
  "createdAt": "2026-05-14T09:00:00Z"
}
```

| Field              | Type           | Description                                         |
| ------------------ | -------------- | --------------------------------------------------- |
| `id`               | string         | Internal post ID                                    |
| `title`            | string         | Post title                                          |
| `content`          | string         | Post body                                           |
| `contentFormat`    | string         | `text/plain` or `text/markdown`                     |
| `files`            | PostFile[]     | Currently attached files                            |
| `encrypted`        | boolean        | Whether attached files are encrypted                |
| `isPublic`         | boolean        | Public or patreon-restricted                        |
| `tierNames`        | string[]       | Tiers that can access the post (when not public)    |
| `collectionNames`  | string[]       | Collections this post belongs to                    |
| `publishDateUtc`   | string \| null | Scheduled publish date (ISO 8601 UTC) _(optional)_  |
| `tags`             | string[]       | Tags                                                |
| `pending`          | object \| null | Pending changes not yet applied by the extension    |
| `patreonPostId`    | string         | Linked Patreon post ID                              |
| `patreonUpdatedAt` | string         | When the extension last synced this post to Patreon |
| `updatedAt`        | string         | Last modified timestamp                             |
| `createdAt`        | string         | Creation timestamp                                  |

The `pending` object has the same shape as the post fields and represents changes queued for the Chrome extension to apply.

---

## Endpoints

### `GET /api/v1/post`

Returns all posts.

**Response** `200 OK`

```json
{
  "success": true,
  "data": [
    /* PostGetResult[] */
  ]
}
```

---

### `GET /api/v1/post/{id}`

Returns a single post by ID.

**Response** `200 OK`

```json
{
  "success": true,
  "data": {
    /* PostGetResult */
  }
}
```

---

### `POST /api/v1/post`

Creates a new post.

**Request body**

```json
{
  "title": "My Post",
  "content": "Hello world",
  "contentFormat": "text/plain",
  "isPublic": false,
  "tierNames": ["Early Access"],
  "collectionNames": ["Dev Log"],
  "publishDateUtc": null,
  "tags": ["release"],
  "ttlDays": null,
  "encrypted": false,
  "addFiles": [{ "name": "image.png" }]
}
```

| Field             | Type       | Required | Description                                        |
| ----------------- | ---------- | -------- | -------------------------------------------------- |
| `title`           | string     | Yes      | Post title                                         |
| `content`         | string     | Yes      | Post body                                          |
| `contentFormat`   | string     | No       | `text/plain` (default) or `text/markdown`          |
| `isPublic`        | boolean    | Yes      | Public or patreon-restricted                       |
| `tierNames`       | string[]   | No       | Tiers allowed to access (when `isPublic` is false) |
| `collectionNames` | string[]   | No       | Collections this post belongs to                   |
| `publishDateUtc`  | string     | No       | Scheduled publish date (ISO 8601 UTC)              |
| `tags`            | string[]   | No       | Tags                                               |
| `ttlDays`         | integer    | No       | Days until the post is automatically deleted       |
| `encrypted`       | boolean    | No       | Whether attached files are encrypted client-side   |
| `addFiles`        | PostFile[] | No       | Files to attach (only `name` is required here)     |

**Response** `200 OK`

```json
{
  "success": true,
  "data": {
    "post": {
      /* PostGetResult */
    },
    "uploadUrls": [
      /* PostUploadSession[] */
    ]
  }
}
```

`uploadUrls` contains one entry per file in `addFiles`. Files must be uploaded to their respective presigned URLs immediately after this call.

---

### `PUT /api/v1/post/{id}`

Updates an existing post.

**Request body**

```json
{
  "title": "Updated Title",
  "content": "New content",
  "contentFormat": "text/plain",
  "isPublic": true,
  "tierNames": [],
  "collectionNames": ["Dev Log"],
  "publishDateUtc": null,
  "tags": ["v2"],
  "patreonPostId": null,
  "addFiles": [],
  "removeFiles": [{ "name": "old-image.png" }]
}
```

| Field             | Type       | Required | Description                                        |
| ----------------- | ---------- | -------- | -------------------------------------------------- |
| `title`           | string     | Yes      | New title                                          |
| `content`         | string     | Yes      | New body                                           |
| `contentFormat`   | string     | No       | `text/plain` (default) or `text/markdown`          |
| `isPublic`        | boolean    | Yes      | Public or patreon-restricted                       |
| `tierNames`       | string[]   | No       | Tiers allowed to access (when `isPublic` is false) |
| `collectionNames` | string[]   | No       | Collections this post belongs to                   |
| `publishDateUtc`  | string     | No       | Scheduled publish date (ISO 8601 UTC)              |
| `tags`            | string[]   | No       | Tags                                               |
| `patreonPostId`   | string     | No       | Override the linked Patreon post ID                |
| `addFiles`        | PostFile[] | No       | Files to attach (only `name` is required here)     |
| `removeFiles`     | PostFile[] | No       | Files to remove (only `name` is required here)     |

**Response** `200 OK`

```json
{
  "success": true,
  "data": {
    "post": {
      /* PostGetResult */
    },
    "uploadUrls": [
      /* PostUploadSession[] */
    ]
  }
}
```

---

### `DELETE /api/v1/post/{id}`

Deletes a post by ID.

**Response** `200 OK`

```json
{
  "success": true,
  "data": null
}
```

---

## File Upload Flow

After a `POST /api/v1/post` or `PUT /api/v1/post/{id}` call that includes files, the response contains `uploadUrls`. For each entry, upload the file with a `multipart/form-data` POST:

```
POST <uploadUrls[i].url>
Content-Type: multipart/form-data

<all fields from uploadUrls[i].fields as form parts>
file=<binary file content>
```

The `file` part must come **last**. The presigned URLs are typically short-lived.

### Encryption

When `encrypted: true` is set on the post, the CLI encrypts each file client-side **before** uploading. The wire format is:

```
salt (16 bytes) | IV (16 bytes) | AES-256-CBC ciphertext (PKCS7 padded)
```

Key derivation uses **PBKDF2-SHA256** with 600 000 iterations and a 32-byte output key.
