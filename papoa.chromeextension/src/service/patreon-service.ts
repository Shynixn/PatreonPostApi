import { PostGetResultDTO } from "../dto/v1/post-dto.js";
import { BrowserService } from "./browser-service.js";
import { CryptoService } from "./crypto-service.js";
import { PostService } from "./post-service.js";

export class PatreonService {
  patreonUrl = "https://www.patreon.com/posts/new";
  private cryptoService = new CryptoService();

  constructor(
    private browserService: BrowserService,
    private postService: PostService,
    private password: string = "",
  ) {}

  async postToPatreon(
    post: PostGetResultDTO,
    tabId?: number,
    autoSubmit?: boolean,
  ): Promise<void> {
    const pending = post.pending;
    if (!pending) throw new Error("Post has no pending content to publish.");

    const targetTabId = tabId
      ? await this.browserService.navigateTabTo(tabId, this.patreonUrl)
      : await this.browserService.navigateActiveTabTo(this.patreonUrl);

    // Title
    await this.browserService.delay(5000);
    await this.browserService.writeElementByAttribute(
      "placeholder",
      "Title",
      pending.title,
      targetTabId,
    );

    // Content
    await this.browserService.delay(1000);
    await this.browserService.writeElementByAttribute(
      "class",
      "remirror-editor-wrapper",
      pending.contentFormat === "text/markdown"
        ? `${await this.browserService.markdown2Html(pending.content)}`
        : "<p>" + pending.content.replace(/\n/g, "</p><p>") + "</p>",
      targetTabId,
    );

    // Paid or Public
    await this.browserService.delay(1000);
    if (post.isPublic) {
      this.browserService.clickElementByAttribute(
        "aria-label",
        "Free access",
        targetTabId,
      );
    } else {
      this.browserService.clickElementByAttribute("value", "paid", targetTabId);

      // Select Specific Tiers
      await this.browserService.delay(1000);
      await this.browserService.clickElementByAttribute(
        "aria-label",
        "Select tiers",
        targetTabId,
      );

      for (const tierName of pending.tierNames) {
        await this.browserService.delay(1000);
        await this.browserService.clickElementByAttribute(
          "aria-label",
          tierName,
          targetTabId,
        );
      }
    }

    // Select Collections
    await this.browserService.delay(1000);
    await this.browserService.clickElementByAttribute(
      "aria-label",
      "Icon indicating the dropdown can be expanded to display a creator's collections",
      targetTabId,
    );
    for (const collectionName of pending.collectionNames) {
      await this.browserService.delay(1000);
      await this.browserService.clickElementByAttribute(
        "aria-label",
        collectionName,
        targetTabId,
      );
    }

    // Publish Date
    if (post.publishDateUtc != null) {
      await this.browserService.delay(1000);
      await this.browserService.clickElementById(
        "scheduled-for-toggle",
        targetTabId,
      );
      await this.browserService.delay(1000);
      // Date
      await this.browserService.clickElementById("date", targetTabId);
      const date = new Date(post.publishDateUtc);
      const month = String(date.getUTCMonth() + 1).padStart(2, "0");
      const day = String(date.getUTCDate()).padStart(2, "0");
      const year = date.getUTCFullYear();
      await this.browserService.delay(100);
      await this.browserService.writeElementById(
        "date",
        `${month}`,
        targetTabId,
      );
      await this.browserService.delay(100);
      await this.browserService.writeElementById("date", `${day}`, targetTabId);
      await this.browserService.delay(100);
      await this.browserService.writeElementById(
        "date",
        `${year}`,
        targetTabId,
      );
      // Time
      await this.browserService.clickElementById(":r2h:", targetTabId);
      const hours = String(date.getUTCHours()).padStart(2, "0");
      const minutes = String(date.getUTCMinutes()).padStart(2, "0");
      await this.browserService.delay(100);
      await this.browserService.writeElementById(
        ":r2h:",
        `${hours}`,
        targetTabId,
      );
      await this.browserService.delay(100);
      await this.browserService.writeElementById(
        ":r2h:",
        `${minutes}`,
        targetTabId,
      );
    }

    // Post tags
    if (post.tags != null) {
      await this.browserService.delay(1000);
      await this.browserService.clickElementByAttribute(
        "data-tag",
        "tags-auto-complete",
        targetTabId,
      );

      for (const tag of post.tags) {
        await this.browserService.delay(500);
        await this.browserService.writeElementByAttribute(
          "data-tag",
          "tags-auto-complete",
          tag,
          targetTabId,
        );
        await this.browserService.delay(500);
        await this.browserService.pressEnter(targetTabId);
      }
    }

    // File Uploads
    const postWithUrls = await this.postService.getPostWithDownloadUrls(
      post.id,
    );
    const filesToUpload = postWithUrls.pending?.addFiles ?? [];
    for (const file of filesToUpload) {
      if (!file.url) {
        console.warn(`Skipping file ${file.name}: no download URL.`);
        continue;
      }
      try {
        const response = await fetch(file.url);
        if (!response.ok) {
          console.warn(
            `Skipping file ${file.name}: fetch failed (${response.status}).`,
          );
          continue;
        }
        let fileContent = await response.arrayBuffer();
        if (post.encrypted && this.password) {
          fileContent = await this.cryptoService.decryptBytes(
            fileContent,
            this.password,
          );
        }
        await this.browserService.uploadFile(
          "add-attachments-button",
          fileContent,
          file.name,
          "application/octet-stream",
          true,
          targetTabId,
        );
      } catch {
        console.warn(`Skipping file ${file.name}: error during upload.`);
      }
    }

    // Submit the post and confirm in Papoa
    if (autoSubmit) {
      await this.browserService.delay(1000);
      await this.browserService.clickElementByAttribute(
        "data-tag",
        "make-a-post-action-publish",
        targetTabId,
      );
      await this.browserService.clickElementByAttribute(
        "data-tag",
        "make-a-post-action-schedule_post",
        targetTabId,
      );
      await this.browserService.delay(10000);
    }

    // Retrieve current opened url and extract the Patreon post ID
    const currentUrl = await this.browserService.getTabUrl(targetTabId);
    console.log("Current tab URL:", currentUrl);
    const patreonPostIdMatch = currentUrl.match(/\/posts\/(\d+)/);
    const patreonPostId = patreonPostIdMatch?.[1];
    // Confirm posts
    await this.postService.confirmPost(post, patreonPostId ?? undefined);
  }
}
