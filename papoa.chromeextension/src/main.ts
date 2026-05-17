import { PostGetResultDTO } from "./dto/v1/post-dto.js";
import { BrowserService } from "./service/browser-service.js";
import { PostService } from "./service/post-service.js";
import { PatreonService } from "./service/patreon-service.js";
import {
  AppConfig,
  ConfigService,
  DEFAULT_BASE_URL,
  DEFAULT_POLL_INTERVAL_MINUTES,
} from "./service/config-service.js";
const configService = new ConfigService();
// --- Config view ---

function renderConfigView(root: HTMLElement, currentConfig?: AppConfig): void {
  root.innerHTML = `
    <div class="p-3">
      <h5 class="mb-3">Setup</h5>
      <div class="mb-3">
        <label for="api-key-input" class="form-label">
          API Key <span class="text-danger">*</span>
        </label>
        <input
          type="password"
          class="form-control"
          id="api-key-input"
          placeholder="Enter your API key"
          value="${currentConfig?.apiKey || ""}"
        />
      </div>
      <div class="mb-3">
        <label for="base-url-input" class="form-label">Base URL</label>
        <input
          type="text"
          class="form-control"
          id="base-url-input"
          placeholder="${DEFAULT_BASE_URL}"
          value="${currentConfig?.baseUrl || DEFAULT_BASE_URL}"
        />
        <div class="form-text">Leave blank to use the default.</div>
      </div>
      <div class="form-check mb-3">
        <input
          class="form-check-input"
          type="checkbox"
          value=""
          id="background-fetch-input"
          ${currentConfig?.backgroundFetchEnabled ? "checked" : ""}
        />
        <label class="form-check-label" for="background-fetch-input">
          Background fetch (automatically fetches posts in the background and starts a new tab process)
        </label>
      </div>
      <div class="mb-3">
        <label for="poll-interval-input" class="form-label">Background fetch interval (minutes)</label>
        <input
          type="number"
          class="form-control"
          id="poll-interval-input"
          min="1"
          value="${currentConfig?.pollIntervalMinutes ?? DEFAULT_POLL_INTERVAL_MINUTES}"
        />
        <div class="form-text">
         Reducing this interval causes more frequent requests. This consumes your daily API limit faster and may cause rate limiting.
        </div>
      </div>
      <div class="form-check mb-3">
        <input
          class="form-check-input"
          type="checkbox"
          value=""
          id="auto-submit-input"
          ${currentConfig?.autoSubmitEnabled ? "checked" : ""}
        />
        <label class="form-check-label" for="auto-submit-input">
          Auto-submit (automatically publishes the post to Patreon instead of just preparing a draft)
        </label>
      </div>
      <div class="mb-3">
        <label for="password-input" class="form-label">Decryption Password</label>
        <input
          type="password"
          class="form-control"
          id="password-input"
          placeholder="Enter decryption password (optional)"
          value="${currentConfig?.password || ""}"
        />
        <div class="form-text">Required to decrypt posts that have encrypted content.</div>
      </div>
      <div id="config-error" class="alert alert-danger d-none" role="alert"></div>
      <button id="save-config-btn" class="btn btn-primary w-100">Save &amp; Connect</button>
    </div>
  `;

  document
    .getElementById("save-config-btn")!
    .addEventListener("click", async () => {
      const apiKey = (
        document.getElementById("api-key-input") as HTMLInputElement
      ).value.trim();
      const baseUrl =
        (
          document.getElementById("base-url-input") as HTMLInputElement
        ).value.trim() || DEFAULT_BASE_URL;
      const backgroundFetchEnabled = (
        document.getElementById("background-fetch-input") as HTMLInputElement
      ).checked;
      const autoSubmitEnabled = (
        document.getElementById("auto-submit-input") as HTMLInputElement
      ).checked;
      const pollIntervalMinutes = Math.max(
        1,
        Number(
          (document.getElementById("poll-interval-input") as HTMLInputElement)
            .value,
        ) || DEFAULT_POLL_INTERVAL_MINUTES,
      );
      const password = (
        document.getElementById("password-input") as HTMLInputElement
      ).value;
      const errorDiv = document.getElementById("config-error")!;

      if (!apiKey) {
        errorDiv.textContent = "API key is required.";
        errorDiv.classList.remove("d-none");
        return;
      }

      await configService.saveConfig({
        baseUrl,
        apiKey,
        backgroundFetchEnabled,
        autoSubmitEnabled,
        pollIntervalMinutes,
        password,
      });
      renderMainView(root, baseUrl, apiKey, autoSubmitEnabled, password);
    });
}

// --- Post card ---

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleString();
}

function createPostCard(
  post: PostGetResultDTO,
  onPostToPatreon: (post: PostGetResultDTO) => Promise<void>,
): HTMLElement {
  const card = document.createElement("div");
  card.className = "card job-card";

  const cardBody = document.createElement("div");
  cardBody.className = "card-body";

  const title = document.createElement("h5");
  title.className = "card-title";
  title.textContent = post.title;

  const text = document.createElement("p");
  text.className = "card-text";
  text.textContent = post.content;

  const createdDate = document.createElement("p");
  createdDate.className = "text-muted small";
  createdDate.innerHTML = `<strong>Created:</strong> ${formatDate(post.createdAt)}`;

  cardBody.appendChild(title);
  cardBody.appendChild(text);
  cardBody.appendChild(createdDate);

  if (post.files.length > 0) {
    const filesDiv = document.createElement("div");
    filesDiv.className = "job-files";
    filesDiv.innerHTML = `<strong>Files:</strong> ${post.files.map((f: { name: string }) => f.name).join(", ")}`;
    cardBody.appendChild(filesDiv);
  }

  const statusText = document.createElement("div");
  statusText.className = "small mt-2 d-none";

  const platformButtons = document.createElement("div");
  platformButtons.className = "platform-buttons mt-3";

  const postButton = document.createElement("button");
  postButton.className = "btn btn-outline-primary btn-sm";
  postButton.textContent = "Post to Patreon";

  postButton.addEventListener("click", async () => {
    postButton.disabled = true;
    postButton.textContent = "Posting...";
    statusText.className = "small mt-2 text-muted";
    statusText.textContent = "Opening Patreon and preparing your draft...";

    try {
      console.log("Posting to Patreon:" + JSON.stringify(post));
      await onPostToPatreon(post);
      statusText.className = "small mt-2 text-success";
      statusText.textContent = "Posted and confirmed.";
      postButton.textContent = "Post to Patreon";
    } catch (error) {
      console.error("Error posting to Patreon:", error);
      statusText.className = "small mt-2 text-danger";
      statusText.textContent = `Failed to post:` + error;
      postButton.textContent = "Post to Patreon";
    } finally {
      postButton.disabled = false;
    }
  });

  platformButtons.appendChild(postButton);
  platformButtons.appendChild(statusText);
  cardBody.appendChild(platformButtons);

  card.appendChild(cardBody);
  return card;
}

// --- Main view ---

async function loadPosts(
  postService: PostService,
  patreonService: PatreonService,
  autoSubmitEnabled: boolean,
  root: HTMLElement,
): Promise<void> {
  const loadingDiv = document.getElementById("loading");
  const errorDiv = document.getElementById("error");
  const jobsContainer = document.getElementById("jobs-container");

  if (loadingDiv) loadingDiv.classList.remove("d-none");
  if (errorDiv) errorDiv.classList.add("d-none");
  if (jobsContainer) jobsContainer.classList.add("d-none");

  try {
    const posts = await postService.fetchPendingPosts();

    if (loadingDiv) loadingDiv.classList.add("d-none");
    if (jobsContainer) {
      jobsContainer.classList.remove("d-none");
      jobsContainer.innerHTML = "";

      if (posts.length === 0) {
        const emptyMessage = document.createElement("div");
        emptyMessage.className = "alert alert-info";
        emptyMessage.textContent = "No pending posts found.";
        jobsContainer.appendChild(emptyMessage);
      } else {
        posts.forEach((post) => {
          jobsContainer.appendChild(
            createPostCard(post, async (selected) => {
              await patreonService.postToPatreon(
                selected,
                undefined,
                autoSubmitEnabled,
              );
              await loadPosts(
                postService,
                patreonService,
                autoSubmitEnabled,
                root,
              );
            }),
          );
        });
      }
    }
  } catch (error) {
    if (loadingDiv) loadingDiv.classList.add("d-none");
    if (errorDiv) {
      errorDiv.textContent = `Failed to load posts: ${
        error instanceof Error ? error.message : "Unknown error"
      }`;
      errorDiv.classList.remove("d-none");
    }
  }
}

function renderMainView(
  root: HTMLElement,
  baseUrl: string,
  apiKey: string,
  autoSubmitEnabled: boolean,
  password: string = "",
): void {
  const browserService = new BrowserService();
  const postService = new PostService(baseUrl, apiKey);
  const patreonService = new PatreonService(
    browserService,
    postService,
    password,
  );

  root.innerHTML = `
    <div class="p-3">
      <div class="header-container">
        <h4 class="mb-0">Papoa - Pending Posts</h4>
        <div class="d-flex gap-2 align-items-center">
          <button id="reload-btn" class="reload-btn" title="Reload posts">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
              <path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2v1z"/>
              <path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466z"/>
            </svg>
          </button>
          <button id="settings-btn" class="reload-btn" title="Settings">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
              <path d="M8 4.754a3.246 3.246 0 1 0 0 6.492 3.246 3.246 0 0 0 0-6.492zM5.754 8a2.246 2.246 0 1 1 4.492 0 2.246 2.246 0 0 1-4.492 0z"/>
              <path d="M9.796 1.343c-.527-1.79-3.065-1.79-3.592 0l-.094.319a.873.873 0 0 1-1.255.52l-.292-.16c-1.64-.892-3.433.902-2.54 2.541l.159.292a.873.873 0 0 1-.52 1.255l-.319.094c-1.79.527-1.79 3.065 0 3.592l.319.094a.873.873 0 0 1 .52 1.255l-.16.292c-.892 1.64.901 3.434 2.541 2.54l.292-.159a.873.873 0 0 1 1.255.52l.094.319c.527 1.79 3.065 1.79 3.592 0l.094-.319a.873.873 0 0 1 1.255-.52l.292.16c1.64.893 3.434-.902 2.54-2.541l-.159-.292a.873.873 0 0 1 .52-1.255l.319-.094c1.79-.527 1.79-3.065 0-3.592l-.319-.094a.873.873 0 0 1-.52-1.255l.16-.292c.893-1.64-.902-3.433-2.541-2.54l-.292.159a.873.873 0 0 1-1.255-.52l-.094-.319z"/>
            </svg>
          </button>
        </div>
      </div>
      <div id="loading" class="loading">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-2">Loading posts...</p>
      </div>
      <div id="error" class="alert alert-danger d-none" role="alert"></div>
      <div id="jobs-container" class="d-none"></div>
    </div>
  `;

  document
    .getElementById("reload-btn")!
    .addEventListener("click", () =>
      loadPosts(postService, patreonService, autoSubmitEnabled, root),
    );
  document
    .getElementById("settings-btn")!
    .addEventListener("click", async () => {
      const config = await configService.getConfig();
      renderConfigView(
        root,
        config ?? {
          baseUrl,
          apiKey,
          backgroundFetchEnabled: false,
          autoSubmitEnabled: false,
          pollIntervalMinutes: DEFAULT_POLL_INTERVAL_MINUTES,
          password,
        },
      );
    });

  loadPosts(postService, patreonService, autoSubmitEnabled, root);
}

// --- Entry point ---

document.addEventListener("DOMContentLoaded", async () => {
  const root = document.querySelector<HTMLElement>(".container-fluid")!;
  const config = await configService.getConfig();

  if (config) {
    renderMainView(
      root,
      config.baseUrl,
      config.apiKey,
      config.autoSubmitEnabled,
      config.password,
    );
  } else {
    renderConfigView(root, {
      baseUrl: DEFAULT_BASE_URL,
      apiKey: "",
      backgroundFetchEnabled: false,
      autoSubmitEnabled: false,
      pollIntervalMinutes: DEFAULT_POLL_INTERVAL_MINUTES,
      password: "",
    });
  }
});
