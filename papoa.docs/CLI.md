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

### Commands

#### `post create`

Creates a new post and uploads any attached files.

```
papoa post create --title <title> [options]
```

| Option                    | Required | Description                                           |
| ------------------------- | -------- | ----------------------------------------------------- |
| `--title <value>`         | Yes      | Title of the post                                     |
| `--text <value>`          | No       | Body text of the post                                 |
| `--text-file <path>`      | No       | Read body text from a file (overrides `--text`)       |
| `--text-format <value>`   | No       | `text/plain` (default) or `text/markdown`             |
| `--file <path>`           | No       | File to attach (repeat for multiple files)            |
| `--password <value>`      | No       | Encrypt attached files with AES-256-CBC before upload |
| `--output-format <value>` | No       | Output format: `text/plain` (default)                 |

> When `--password` is supplied, files are encrypted client-side with AES-256-CBC before leaving your
> machine. The Papoa service stores only the ciphertext and cannot read your file contents. It is **highly** recommended to use `--password` when using the CLI. It protects you against potential leaks of your valuable files.

**Example**

```bash
papoa post create --title "My Post" --text-file post.md --text-format text/markdown --file image.png --password "my-secret"
```

---

#### `post update`

Updates an existing post by ID.

```
papoa post update --id <id> --title <title> [options]
```

| Option                    | Required | Description                                           |
| ------------------------- | -------- | ----------------------------------------------------- |
| `--id <value>`            | Yes      | ID of the post to update                              |
| `--title <value>`         | Yes      | New title                                             |
| `--text <value>`          | No       | New body text                                         |
| `--text-file <path>`      | No       | Read body text from a file (overrides `--text`)       |
| `--text-format <value>`   | No       | `text/plain` (default) or `text/markdown`             |
| `--file <path>`           | No       | File to attach (repeat for multiple files)            |
| `--password <value>`      | No       | Encrypt attached files with AES-256-CBC before upload |
| `--output-format <value>` | No       | Output format: `text/plain` (default)                 |

> When `--password` is supplied, files are encrypted with AES-256-CBC before upload. See the note under `post create` for details.

**Example**

```bash
papoa post update --id abc123 --title "Updated Title" --text "New content" --password "my-secret"
```

---

#### `post delete`

Deletes a post by ID.

```
papoa post delete --id <id> [options]
```

| Option                    | Required | Description                           |
| ------------------------- | -------- | ------------------------------------- |
| `--id <value>`            | Yes      | ID of the post to delete              |
| `--output-format <value>` | No       | Output format: `text/plain` (default) |

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

| Option                    | Required | Description                           |
| ------------------------- | -------- | ------------------------------------- |
| `--id <value>`            | No       | ID of a specific post to retrieve     |
| `--output-format <value>` | No       | Output format: `text/plain` (default) |

**Examples**

```bash
# List all posts
papoa post list

# Get a specific post
papoa post list --id abc123
```
