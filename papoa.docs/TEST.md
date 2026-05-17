# Papoa CLI — Test Scenarios

All commands assume the following environment is set:

```bash
export PAPOA_API_KEY="<valid-api-key>"
export PAPOA_BASE_URL="https://api.papoa.shynixn.com"  # optional, this is the default
```

---

## 1. `post create`

### TC-CREATE-01 — Minimal required options (title only)

```bash
papoa post create --title "Hello World"
```

**Expected:** Post created with the given title, empty content, not public, no tiers/collections/tags/files. Output prints post details.

---

### TC-CREATE-02 — Inline content with plain text format (default)

```bash
papoa post create --title "Plain Post" --content "This is plain text." --content-format text/plain
```

**Expected:** Post created with `text/plain` body. No error raised.

---

### TC-CREATE-03 — Content from file with markdown format

```bash
papoa post create --title "Markdown Post" --content-file ./papoa.resources/post.md --content-format text/markdown
```

**Expected:** File `./papoa.resources/post.md` is read and used as body. `--content` is ignored if both are supplied (file takes precedence). Post created successfully.

---

### TC-CREATE-04 — `--content-file` overrides `--content` when both are given

```bash
papoa post create --title "Override Test" --content "Inline content" --content-file ./papoa.resources/post.md
```

**Expected:** The file content replaces the inline `--content` value. No error raised.

---

### TC-CREATE-05 — Public post

```bash
papoa post create --title "Public Post" --is-public
```

**Expected:** `Is Public: True` in output.

---

### TC-CREATE-06 — Post with multiple tiers

```bash
papoa post create --title "Tiered Post" --tier-name "The Gold One" --tier-name "Platinum Tier"
```

**Expected:** Post created with `TierNames = ["Gold", "Silver"]`.

---

### TC-CREATE-07 — Post with multiple collections

```bash
papoa post create --title "Collection Post" --collection-name "Dev Log" --collection-name "Demo 1"
```

**Expected:** Post created with `CollectionNames = ["Dev Log", "News"]`.

---

### TC-CREATE-08 — Post with multiple tags

```bash
papoa post create --title "Tagged Post" --tag release --tag v1 --tag featured
```

**Expected:** Post created with `Tags = ["release", "v1", "featured"]`.

---

### TC-CREATE-09 — Scheduled post with publish date

```bash
papoa post create --title "Scheduled Post" --publish-date-utc "2026-12-31T00:00:00Z"
```

**Expected:** Post scheduled. Output reflects pending publish date.

---

### TC-CREATE-10 — Post with TTL

```bash
papoa post create --title "Expiring Post" --ttl-days 7
```

**Expected:** Post created with a 7-day time-to-live.

---

### TC-CREATE-11 — Attach a single file without encryption

```bash
papoa post create --title "File Post" \
  --file ./papoa.resources/icon.png \
  --photo-attachment-file-name "icon.png"
```

**Expected:** File is uploaded unencrypted. Output shows `Uploading ./papoa.resources/icon.png...` then `Uploaded ./papoa.resources/icon.png.`. Post `Files` field reflects the attachment.

---

### TC-CREATE-12 — Attach multiple files without encryption

```bash
papoa post create --title "Multi File Post" \
  --file ./papoa.resources/icon.png \
  --file ./papoa.resources/post.md \
  --file ./papoa.resources/papoa-chan.png \
  --photo-attachment-file-name "icon.png" \
  --photo-attachment-file-name "papoa-chan.png" \
  --attachment-file-name "post.md" \
  --attachment-file-name "papoa-chan.png"
```

**Expected:** Both files uploaded sequentially. Both appear in the `Files` output field.

---

### TC-CREATE-13 — Attach a file with encryption (password)

```bash
papoa post create --title "Encrypted Post" \
  --file ./papoa.resources/post.md \
  --attachment-file-name "post.md" \
  --password "1234"
```

**Expected:** File is encrypted client-side with AES-256-CBC before upload. The API receives only ciphertext. Upload progress is printed. Post shows the attachment.

---

### TC-CREATE-14 — Full combination

```bash
papoa post create \
  --title "Full Post" \
  --content-file ./papoa.resources/post.md \
  --content-format text/markdown \
  --is-public \
  --tier-name "Gold" \
  --collection-name "Dev Log" \
  --tag release \
  --tag v2 \
  --ttl-days 30 \
  --publish-date-utc "2026-06-01T12:00:00Z" \
  --file ./papoa.resources/icon.png \
  --photo-attachment-file-name "icon.png" \
  --password "strongpass"
```

**Expected:** Post created with all fields populated. File encrypted and uploaded. Output printed.

---

### TC-CREATE-15 — Invalid `--content-format` value

```bash
papoa post create --title "Bad Format" --content-format text/html
```

**Expected:** CLI rejects the value with an error. Allowed values are `text/plain` and `text/markdown`.

---

### TC-CREATE-16 — Missing required `--title`

```bash
papoa post create --content "No title here"
```

**Expected:** CLI reports that `--title` is required. Exit with non-zero code.

---

### TC-CREATE-17 — `--content-file` path does not exist

```bash
papoa post create --title "Missing File" --content-file /nonexistent/post.md
```

**Expected:** Runtime error when the file cannot be read. Clear error message.

---

---

## 2. `post update`

Update only changes metadata (title, content, visibility, tiers, collections, tags). File attachments cannot be modified after creation.

### TC-UPDATE-01 — Minimal required options (id and title)

```bash
papoa post update --id "abc123" --title "Updated Title"
```

**Expected:** Post metadata updated. Output shows new title and other preserved fields.

---

### TC-UPDATE-02 — Update content inline

```bash
papoa post update --id "abc123" --title "Updated Title" --content "New body content."
```

**Expected:** Post body replaced with the new inline content.

---

### TC-UPDATE-03 — Update content from file

```bash
papoa post update --id "abc123" --title "Updated Title" --content-file updated.md --content-format text/markdown
```

**Expected:** File content replaces the body. Markdown format applied.

---

### TC-UPDATE-04 — Change visibility to public

```bash
papoa post update --id "abc123" --title "Now Public" --is-public
```

**Expected:** `Is Public: True` in output.

---

### TC-UPDATE-05 — Update tier assignments

```bash
papoa post update --id "abc123" --title "The Gold One" --tier-name "Platinum Tier"
```

**Expected:** Tiers replaced with `["Platinum Tier"]`.

---

### TC-UPDATE-06 — Update collections

```bash
papoa post update --id "abc123" --title "New Collections" \
  --collection-name "Announcements" --collection-name "Changelog"
```

**Expected:** Collections updated.

---

### TC-UPDATE-07 — Update tags

```bash
papoa post update --id "abc123" --title "New Tags" --tag updated --tag v3
```

**Expected:** Tags updated.

---

### TC-UPDATE-08 — Mark post as published

```bash
papoa post update --id "abc123" --title "My Post" --status published
```

**Expected:** Post status set to `published`. Output shows `Status: published`.

---

### TC-UPDATE-09 — Set Patreon post ID

```bash
papoa post update --id "abc123" --title "With Patreon ID" --patreon-post-id "12345678"
```

**Expected:** `Patreon Updated At` field populated in output.

---

### TC-UPDATE-10 — Full combination

```bash
papoa post update --id "abc123" \
  --title "Complete Update" \
  --content-file updated.md \
  --content-format text/markdown \
  --is-public \
  --tier-name "Gold" \
  --collection-name "Dev Log" \
  --tag v3 \
  --status published \
  --patreon-post-id "12345678"
```

**Expected:** All metadata fields updated. Status set to published.

---

### TC-UPDATE-11 — Missing required `--id`

```bash
papoa post update --title "No ID"
```

**Expected:** CLI reports `--id` is required. Non-zero exit code.

---

### TC-UPDATE-12 — Missing required `--title`

```bash
papoa post update --id "abc123"
```

**Expected:** CLI reports `--title` is required. Non-zero exit code.

---

### TC-UPDATE-13 — Non-existent post ID

```bash
papoa post update --id "does-not-exist" --title "Ghost Post"
```

**Expected:** API returns an error (e.g., 404). CLI surfaces the error message.

---

### TC-UPDATE-14 — Invalid `--content-format`

```bash
papoa post update --id "abc123" --title "Bad Format" --content-format application/json
```

**Expected:** CLI rejects the value. Allowed: `text/plain`, `text/markdown`.

---

### TC-UPDATE-15 — Invalid `--status`

```bash
papoa post update --id "abc123" --title "Bad Status" --status draft
```

**Expected:** CLI rejects the value. Allowed: `pending`, `published`.

---

---

## 3. `post delete`

### TC-DELETE-01 — Delete by ID

```bash
papoa post delete --id "abc123"
```

**Expected:** Post deleted. Output: `Post abc123 deleted.`

---

### TC-DELETE-02 — Missing required `--id`

```bash
papoa post delete
```

**Expected:** CLI reports `--id` is required. Non-zero exit code.

---

### TC-DELETE-03 — Non-existent post ID

```bash
papoa post delete --id "does-not-exist"
```

**Expected:** API returns an error (e.g., 404). CLI surfaces the error message. Post not deleted.

---

### TC-DELETE-04 — Delete with explicit default output format

```bash
papoa post delete --id "abc123" --output-format text/plain
```

**Expected:** Same output as TC-DELETE-01. `text/plain` is the only accepted value.

---

### TC-DELETE-05 — Invalid `--output-format`

```bash
papoa post delete --id "abc123" --output-format application/json
```

**Expected:** CLI rejects the value. Only `text/plain` is allowed.

---

---

## 4. `post list`

### TC-LIST-01 — List all posts

```bash
papoa post list
```

**Expected:** All posts printed, one block per post showing Id, Title, Content, Is Public, Tier Names, Collection Names, Tags, Files, Created At. Blocks separated by a blank line.

---

### TC-LIST-02 — Fetch a single post by ID

```bash
papoa post list --id "abc123"
```

**Expected:** Only the post matching the given ID is printed.

---

### TC-LIST-03 — No posts exist

```bash
papoa post list
```

**Expected:** No output (empty list). Exit code 0.

---

### TC-LIST-04 — Post with pending status shows status column

```bash
papoa post list --id "abc123"
```

**Expected:** `Status: pending` shown in output for a post that has not been confirmed yet.

---

### TC-LIST-05 — Published post shows published status

```bash
papoa post list --id "abc123"
```

**Expected:** `Status: published` shown in output for a post that has been confirmed.

---

### TC-LIST-06 — Invalid `--output-format`

```bash
papoa post list --output-format text/html
```

**Expected:** CLI rejects the value. Only `text/plain` is accepted.

---

---

## 5. Authentication & Configuration

### TC-AUTH-01 — Missing API key

Unset `PAPOA_API_KEY` and ensure no saved config exists.

```bash
unset PAPOA_API_KEY
papoa post list
```

**Expected:** Request sent without `x-api-key` header. API returns 401/403. CLI surfaces the error.

---

### TC-AUTH-02 — Invalid API key

```bash
PAPOA_API_KEY="invalid-key" papoa post list
```

**Expected:** API returns 401/403. CLI surfaces the error.

---

### TC-AUTH-03 — Custom base URL via environment variable

```bash
PAPOA_BASE_URL="https://staging.api.papoa.shynixn.com" PAPOA_API_KEY="<key>" papoa post list
```

**Expected:** Requests sent to the staging base URL.

---

### TC-AUTH-04 — API key from saved config (no env var)

Pre-condition: run interactive mode once and save an API key to `~/.config/papoa/config.json` (or equivalent `AppData` path).

```bash
unset PAPOA_API_KEY
papoa post list
```

**Expected:** CLI reads the key from the config file and authenticates successfully.
