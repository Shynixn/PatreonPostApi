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

## Install and Create Your First Post

### Step 1 — Get Your API Key

Visit [papoa.shynixn.com](https://papoa.shynixn.com/) and agree to the terms of service. Your free-tier API key will be shown after acceptance.

For higher limits, subscribe to a membership at [patreon.com/c/shynixn/membership](https://www.patreon.com/c/shynixn/membership) and your key will be upgraded automatically.

| Tier      | API Requests / Day | Upload Limit   | Posts       |
| --------- | ------------------ | -------------- | ----------- |
| Free      | 300                | 10 MB / year   | 2 / year    |
| Basic     | 300                | 100 MB / month | 31 / month  |
| Elite     | 500                | 300 MB / month | 200 / month |
| Legendary | 500                | 600 MB / month | 500 / month |

### Step 2 — Download the CLI

Download the latest release for your platform from [GitHub Releases](https://github.com/Shynixn/PatreonPostApi/releases/latest). For the full CLI reference, see the [CLI documentation](papoa.docs/CLI.md).

### Step 3 — Configure the CLI

Set the `PAPOA_API_KEY` environment variable to your API key.

**Linux / macOS**

```bash
export PAPOA_API_KEY="your-api-key"
```

**Windows (PowerShell)**

```powershell
$env:PAPOA_API_KEY = "your-api-key"
```

### Step 4 — Install the Chrome Extension

The CLI queues posts for the Chrome extension to publish on your behalf. [Follow the Chrome Extension Installation Guide](papoa.docs/ChromeExtension.md) to download, load, and configure the extension.

### Step 5 — Create Your First Post

Create a file called `post.md` for the body of your post:

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

Then run:

```bash
papoa post create --title "My First Post" --text-file post.md --text-format text/markdown
```

Your post will appear in the extension's queue. Open your browser — the extension will detect it automatically and publish the post to your Patreon account.
