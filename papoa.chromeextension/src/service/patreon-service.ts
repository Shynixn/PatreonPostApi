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
    await this.browserService.delay(1000);

    await this.browserService.writeElementByAttribute(
      "placeholder",
      "Title",
      pending.title,
      targetTabId,
    );
    await this.browserService.writeElementByAttribute(
      "class",
      "remirror-editor-wrapper",
      pending.textFormat === "text/markdown"
        ? `${await this.browserService.markdown2Html(pending.text)}`
        : "<p>" + pending.text.replace(/\n/g, "</p><p>") + "</p>",
      targetTabId,
    );

    await this.browserService.clickOnElementByAttribute(
      "aria-label",
      "Select tiers",
      targetTabId,
    );
    await this.browserService.delay(1000);
    await this.browserService.clickOnElementByAttribute(
      "id",
      "Basic_17654380",
      targetTabId,
    );
    await this.browserService.delay(1000);

    await this.browserService.clickOnElementByAttribute(
      "aria-label",
      "Icon indicating the dropdown can be expanded to display a creator's collections",
      targetTabId,
    );
    await this.browserService.delay(1000);
    await this.browserService.clickOnElementByAttribute(
      "aria-label",
      "BlockBall",
      targetTabId,
    );
    await this.browserService.delay(1000);

    // Retrieve current opened url and extract the Patreon post ID
    const currentUrl = await this.browserService.getTabUrl(targetTabId);
    console.log("Current tab URL:", currentUrl);
    const patreonPostIdMatch = currentUrl.match(/\/posts\/(\d+)/);
    const patreonPostId = patreonPostIdMatch?.[1];

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

    if (autoSubmit) {
      await this.browserService.delay(1000);
      this.browserService.clickOnElementByAttribute(
        "data-tag",
        "make-a-post-action-publish",
        targetTabId,
      );
      await this.browserService.delay(10000);
    }

    await this.postService.confirmPost(post, patreonPostId ?? undefined);
  }
}
