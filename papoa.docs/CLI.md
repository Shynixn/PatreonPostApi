# Papoa CLI

## CLI (GUI)

Executing the `papoa` executable opens an interactive GUI to manage your posts. Navigate using the arrow keys on your keyboard.

---

## CLI (Automation)

For automation purposes, the CLI offers the following non-interactive commands.

### Configuration

| Environment Variable | Required | Description                        | Default                         |
| -------------------- | -------- | ---------------------------------- | ------------------------------- |
| `PAPOA_BASE_URL`     | No       | Base URL of the Papoa API          | `https://api.papoa.shynixn.com` |
| `PAPOA_API_KEY`      | Yes      | API key sent as `x-api-key` header | —                               |

### Output Format

All commands accept an `--output-format` option that controls how results are written to stdout.

| Value              | Behaviour                                                                                     |
| ------------------ | --------------------------------------------------------------------------------------------- |
| `text/plain`       | Human-readable, labelled fields. Status messages (e.g. upload progress) are also printed.     |
| `application/json` | Prettified JSON object (or array for `post list`). No status messages — only the JSON output. |

The `application/json` format is intended for scripting and piping into tools like `jq`.

---

### Commands

#### `post create`

Creates a new post and uploads any attached files.

```
papoa post create --title <title> [options]
```

| Option                                 | Required | Description                                                    |
| -------------------------------------- | -------- | -------------------------------------------------------------- |
| `--title <value>`                      | Yes      | Title of the post                                              |
| `--content <value>`                    | No       | Body content of the post                                       |
| `--content-file <path>`                | No       | Read body content from a file (overrides `--content`)          |
| `--content-format <value>`             | No       | `text/plain` (default) or `text/markdown`                      |
| `--is-public`                          | No       | Make the post publicly available (default: false)              |
| `--tier-name <value>`                  | No       | Tier that can access the post (repeat for multiple)            |
| `--collection-name <value>`            | No       | Collection this post belongs to (repeat for multiple)          |
| `--publish-date-utc <value>`           | No       | Scheduled publish date in UTC (ISO 8601)                       |
| `--tag <value>`                        | No       | Tag to attach to the post (repeat for multiple)                |
| `--ttl-days <value>`                   | No       | Time-to-live in days before metadata expires (1–90)            |
| `--file <path>`                        | No       | File to attach (repeat for multiple files)                     |
| `--photo-attachment-file-name <value>` | No       | Filename to mark as a photo attachment (repeat for multiple)   |
| `--attachment-file-name <value>`       | No       | Filename to mark as a generic attachment (repeat for multiple) |
| `--password <value>`                   | No       | Encrypt attached files with AES-256-CBC before upload          |
| `--output-format <value>`              | No       | Output format: `text/plain` (default) or `application/json`    |

> When `--password` is supplied, files are encrypted client-side with AES-256-CBC before leaving your
> machine. The Papoa service stores only the ciphertext and cannot read your file contents. It is **highly** recommended to use `--password` when using the CLI. It protects you against potential leaks of your valuable files.

**Example**

```bash
papoa post create --title "My Post" --content-file post.md --content-format text/markdown \
  --is-public --tag release --collection-name "Dev Log" --file image.png \
  --photo-attachment-file-name "image.png" --password "my-secret"
```

---

#### `post update`

Updates an existing post's metadata. File attachments cannot be changed after creation.

Send `--status published` to confirm that the Chrome extension has successfully published the post to Patreon.

```
papoa post update --id <id> [options]
```

| Option                      | Required | Description                                                 |
| --------------------------- | -------- | ----------------------------------------------------------- |
| `--id <value>`              | Yes      | ID of the post to update                                    |
| `--title <value>`           | No       | New title                                                   |
| `--content <value>`         | No       | New body content                                            |
| `--content-file <path>`     | No       | Read body content from a file (overrides `--content`)       |
| `--content-format <value>`  | No       | `text/plain` (default) or `text/markdown`                   |
| `--is-public`               | No       | Make the post publicly available (default: false)           |
| `--tier-name <value>`       | No       | Tier that can access the post (repeat for multiple)         |
| `--collection-name <value>` | No       | Collection this post belongs to (repeat for multiple)       |
| `--tag <value>`             | No       | Tag to attach to the post (repeat for multiple)             |
| `--status <value>`          | No       | Set post status: `pending` or `published`                   |
| `--patreon-post-id <value>` | No       | Override the linked Patreon post ID                         |
| `--output-format <value>`   | No       | Output format: `text/plain` (default) or `application/json` |

> Update only changes metadata — visibility, tiers, tags, content. Files attached at creation time cannot be added or removed.

**Example**

```bash
papoa post update --id abc123 --title "Updated Title" --content "New content" \
  --tag v2 --status published
```

---

#### `post delete`

Deletes a post by ID.

```
papoa post delete --id <id> [options]
```

| Option                    | Required | Description                                                 |
| ------------------------- | -------- | ----------------------------------------------------------- |
| `--id <value>`            | Yes      | ID of the post to delete                                    |
| `--output-format <value>` | No       | Output format: `text/plain` (default) or `application/json` |

**Example**

```bash
papoa post delete --id abc123
```

---

#### `post list`

Lists all posts, or retrieves a single post if `--id` is provided.

```
papoa post list [options]
```

| Option                    | Required | Description                                                 |
| ------------------------- | -------- | ----------------------------------------------------------- |
| `--id <value>`            | No       | ID of a specific post to retrieve                           |
| `--output-format <value>` | No       | Output format: `text/plain` (default) or `application/json` |

**Examples**

```bash
# List all posts
papoa post list

# Get a specific post
papoa post list --id abc123
```
