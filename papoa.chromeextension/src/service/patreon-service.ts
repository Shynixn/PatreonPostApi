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

  async resetPatreonPost(
    patreonUrl: string,
    post: PostGetResultDTO,
    targetTabId: number,
  ): Promise<void> {
    // Reset Collections
    await this.browserService.clickElementByAttribute(
      "aria-label",
      "Icon indicating the dropdown can be expanded to display a creator's collections",
      targetTabId,
    );
    await this.browserService.delay(1000);
    await this.browserService.uncheckAll(targetTabId);
    await this.browserService.delay(1000);
    await this.browserService.clickElementByAttribute(
      "aria-label",
      "Icon indicating the dropdown can be expanded to display a creator's collections",
      targetTabId,
    );

    // Reset Tiers
    await this.browserService.delay(1000);
    this.browserService.clickElementByAttribute("value", "paid", targetTabId);
    await this.browserService.delay(1000);
    await this.browserService.clickElementByAttribute(
      "aria-label",
      "Select tiers",
      targetTabId,
    );
    await this.browserService.delay(1000);
    await this.browserService.uncheckAll(targetTabId);
    await this.browserService.delay(1000);
    this.browserService.clickElementByAttribute(
      "aria-label",
      "Free access",
      targetTabId,
    );
    await this.browserService.delay(1000);
    await this.browserService.clearTags(targetTabId);
  }

  async postToPatreon(
    post: PostGetResultDTO,
    tabId?: number,
    autoSubmit?: boolean,
  ): Promise<void> {
    if (post.status !== "pending")
      throw new Error("Post is not in pending status.");

    let patreonUrl = this.patreonUrl;
    let isEditingExistingPost = false;
    if (post.patreonPostId != null && post.patreonPostId !== "") {
      patreonUrl = `https://www.patreon.com/posts/${post.patreonPostId}/edit`;
      isEditingExistingPost = true;
    }

    const targetTabId = tabId
      ? await this.browserService.navigateTabTo(tabId, patreonUrl)
      : await this.browserService.navigateActiveTabTo(patreonUrl);
    await this.browserService.delay(5000);

    if (isEditingExistingPost) {
      // If editing an existing post, we need to reset the form first to ensure the pending changes are correctly applied
      await this.resetPatreonPost(patreonUrl, post, targetTabId);
    }

    // ################# START #####################

    // Title
    await this.browserService.delay(1000);
    await this.browserService.clickElementByAttribute(
      "aria-label",
      "Title",
      targetTabId,
    );
    await this.browserService.writeElementByAttribute(
      "placeholder",
      "Title",
      post.title,
      targetTabId,
    );

    // Content
    await this.browserService.delay(1000);
    await this.browserService.writeElementByAttribute(
      "aria-label",
      "Text input field for post content",
      post.contentFormat === "text/markdown"
        ? `${await this.browserService.markdown2Html(post.content)}`
        : "<p>" + post.content.replace(/\n/g, "</p><p>") + "</p>",
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
      if (post.tierNames.length > 0) {
        await this.browserService.delay(1000);
        await this.browserService.clickElementByAttribute(
          "aria-label",
          "Select tiers",
          targetTabId,
        );

        await this.browserService.delay(1000);
        await this.browserService.clickElementByAttribute(
          "aria-label",
          "Select all tiers",
          targetTabId,
        );

        for (const tierName of post.tierNames) {
          await this.browserService.delay(1000);
          await this.browserService.clickElementByAttribute(
            "aria-label",
            tierName,
            targetTabId,
          );
        }
      }
    }

    // Select Collections
    if (post.collectionNames.length > 0) {
      await this.browserService.delay(1000);
      await this.browserService.clickElementByAttribute(
        "aria-label",
        "Icon indicating the dropdown can be expanded to display a creator's collections",
        targetTabId,
      );
      await this.browserService.delay(1000);
      for (const collectionName of post.collectionNames) {
        await this.browserService.delay(1000);
        await this.browserService.clickElementByAttribute(
          "aria-label",
          collectionName,
          targetTabId,
        );
      }
      await this.browserService.delay(1000);
      await this.browserService.clickElementByAttribute(
        "aria-label",
        "Icon indicating the dropdown can be expanded to display a creator's collections",
        targetTabId,
      );
    }

    // Publish Date
    if (post.publishDateUtc != null && !isEditingExistingPost) {
      await this.browserService.delay(1000);
      await this.browserService.clickElementById(
        "scheduled-for-toggle",
        targetTabId,
      );
      // Date
      await this.browserService.delay(1000);
      const date = new Date(post.publishDateUtc);
      const month = String(date.getUTCMonth() + 1).padStart(2, "0");
      const day = String(date.getUTCDate()).padStart(2, "0");
      const year = date.getUTCFullYear();
      await this.browserService.setValueById(
        "date",
        `${year}-${month}-${day}`,
        targetTabId,
      );
      // Time
      await this.browserService.delay(1000);
      const hours = String(date.getUTCHours()).padStart(2, "0");
      const minutes = String(date.getUTCMinutes()).padStart(2, "0");
      await this.browserService.setValueById(
        ":r2n:",
        `${hours}:${minutes}`,
        targetTabId,
      );
    }

    // Post tags
    if (post.tags != null && post.tags.length > 0) {
      for (const tag of post.tags) {
        await this.browserService.delay(1000);
        await this.browserService.writeElementByAttribute(
          "data-tag",
          "tags-auto-complete",
          tag,
          targetTabId,
        );
        await this.browserService.delay(1000);
        await this.browserService.pressKey("Enter", targetTabId);
      }
    }

    // File Uploads
    if (!isEditingExistingPost) {
      const postWithUrls = await this.postService.getPostWithDownloadUrls(
        post.id,
      );
      const filesToUpload = postWithUrls.files ?? [];
      const attachmentFiles: Array<{ content: ArrayBuffer; name: string }> = [];
      const photoFiles: Array<{ content: ArrayBuffer; name: string }> = [];
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

          if ((post.attachmentFileNames ?? []).includes(file.name)) {
            attachmentFiles.push({ content: fileContent, name: file.name });
          }
          if (post.photoAttachmentFileNames.includes(file.name)) {
            photoFiles.push({ content: fileContent, name: file.name });
          }
        } catch (error) {
          if (post.encrypted) {
            throw new Error(
              `Error processing file ${file.name}: ${error}. Make sure the password is correct to decrypt the file.`,
            );
          } else {
            throw new Error(`Error processing file ${file.name}: ${error}. `);
          }
        }
      }

      if (attachmentFiles.length > 0) {
        await this.browserService.uploadFileById(
          "add-attachments-button",
          attachmentFiles,
          "application/octet-stream",
          true,
          targetTabId,
        );
      }
      if (photoFiles.length > 0) {
        await this.browserService.clickButtonByInnerHtml("browse", targetTabId);
        await this.browserService.delay(1000);
        await this.browserService.uploadFileById(
          "photosInput",
          photoFiles,
          "image/png",
          false,
          targetTabId,
        );
      }
    }

    // Submit the post and confirm in Papoa
    if (autoSubmit) {
      await this.browserService.delay(1000);
      if (isEditingExistingPost) {
        await this.browserService.clickElementByAttribute(
          "data-tag",
          "make-a-post-action-save_without_notifying",
          targetTabId,
        );
      } else if (post.publishDateUtc != null) {
        await this.browserService.clickElementByAttribute(
          "data-tag",
          "make-a-post-action-schedule_post",
          targetTabId,
        );
      } else {
        await this.browserService.clickElementByAttribute(
          "data-tag",
          "make-a-post-action-publish",
          targetTabId,
        );
      }
      await this.browserService.delay(10000);
    }

    // Retrieve current opened url and extract the Patreon post ID
    const currentUrl = await this.browserService.getTabUrl(targetTabId);
    const patreonPostIdMatch = currentUrl.match(/\/posts\/(\d+)/);
    const patreonPostId = patreonPostIdMatch?.[1];
    // Confirm posts
    await this.postService.confirmPost(post, patreonPostId ?? undefined);
  }
}
