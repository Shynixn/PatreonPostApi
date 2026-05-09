<h1><img src="papoa.resources/icon.png" alt="Papoa icon" width="56" /> PatreonPostApi (Unofficial) - Papoa</h1>

PatreonPost API (Papoa) allows you to programmatically prepare and publish posts on patreon.com. This project is not affiliated with Patreon.

## Background

I distribute several programs on [patreon.com/Shynixn](https://patreon.com/Shynixn) and use Patreon to post regular updates to my supporters. With multiple programs each needing their own updates every month, creating all those posts by hand is tedious and error-prone — copy-pasting content, attaching files, double-checking formatting, repeat.

So I built Papoa to automate it. What makes this particularly frustrating is that Patreon, a market leader in creator monetisation, still does not offer a public API for creating or publishing posts in 2026. For a platform of that size and maturity, that is honestly embarrassing. Papoa works around that gap.

## How It Works

The official Patreon API does **not provide POST actions** for creating or publishing posts. On top of that, browser automation approaches based on **Selenium**, **headless browsers**, or other **fake-browser techniques** are typically **blocked by Cloudflare**. In practice, that means the usual **server-side automation routes are not viable**.

The remaining workable approach is similar to how **AI agents** interact with websites: a **browser-resident extension** operating inside a **real user session**. Papoa follows that model.

This API works by accepting your **post metadata and files**, storing that data in a **repository**, and then letting a **Chrome extension** poll for pending work. When the extension detects a job, it opens Patreon in your browser session and executes the required steps on your behalf using the browser's normal **autofill** and page interaction mechanisms.

To use this API, you need two parts:

1. An **integration** that sends your **post metadata and files** to this API. You can use the provided CLI or build your own
2. A **web browser** where you are already **logged in to your Patreon account**, with the **Chrome extension installed** and allowed to control the browser when patreon.com is open.

The **Chrome extension is fully public** in this repository, so you can inspect and verify exactly what it does before using it.

You do **not send Patreon credentials** to this service. Authentication stays **inside your own browser session**, and the extension operates **locally** in that session. From Patreon's point of view, this behaves like a browser automation or text autofill helper acting inside a **real, logged-in browser**, rather than a third-party service impersonating your account.

## CLI Usage

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

**post.md**

```markdown
# April Update — New Chapter & Behind-the-Scenes

Hey everyone! 👋

This month's update is finally here, and I'm excited to share what I've been working on.

## What's New

- Finished the first draft of Chapter 12 — it's the longest one yet
- Added 3 new behind-the-scenes photos from last week's shoot
- Early access to the next episode is attached below

## A Note from Me

Thank you so much for your continued support — it genuinely makes this possible.
If you have feedback or requests for next month, drop them in the comments!

— Alex
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
