import { BrowserService } from "./service/browser-service.js";
import { PostService } from "./service/post-service.js";
import { PatreonService } from "./service/patreon-service.js";
import {
  DEFAULT_POLL_INTERVAL_MINUTES,
  ConfigService,
} from "./service/config-service.js";

const configService = new ConfigService();
const AUTONOMOUS_ALARM = "papoa-autonomous-poll";

async function syncAutonomousAlarm(
  enabled: boolean,
  intervalMinutes: number = DEFAULT_POLL_INTERVAL_MINUTES,
): Promise<void> {
  if (enabled) {
    await chrome.alarms.create(AUTONOMOUS_ALARM, {
      delayInMinutes: intervalMinutes,
      periodInMinutes: intervalMinutes,
    });
    return;
  }

  await chrome.alarms.clear(AUTONOMOUS_ALARM);
}

async function processOnePostInAutonomousMode(): Promise<void> {
  const config = await configService.getConfig();
  if (!config || !config.backgroundFetchEnabled) {
    return;
  }

  const postService = new PostService(config.baseUrl, config.apiKey);
  const posts = await postService.fetchPendingPosts();
  if (posts.length === 0) {
    return;
  }

  const nextPost = posts[0];
  if (!nextPost?.id) {
    console.warn("Skipping autonomous post because the post id is missing.");
    return;
  }

  const browserService = new BrowserService();
  const patreonService = new PatreonService(
    browserService,
    postService,
    config.password ?? "",
  );

  const tabId = await browserService.openNewWindowTo(
    "https://www.patreon.com/",
  );
  try {
    await patreonService.postToPatreon(
      nextPost,
      tabId,
      config.autoSubmitEnabled,
    );
  } finally {
    if (config.autoSubmitEnabled) {
      try {
        await chrome.tabs.remove(tabId);
      } catch (error) {
        console.warn(`Failed to close autonomous tab ${tabId}:`, error);
      }
    }
  }
}

chrome.runtime.onInstalled.addListener(async () => {
  const config = await configService.getConfig();
  await syncAutonomousAlarm(
    Boolean(config?.backgroundFetchEnabled),
    config?.pollIntervalMinutes,
  );
});

chrome.runtime.onStartup.addListener(async () => {
  const config = await configService.getConfig();
  await syncAutonomousAlarm(
    Boolean(config?.backgroundFetchEnabled),
    config?.pollIntervalMinutes,
  );
});

chrome.storage.onChanged.addListener(async (changes, areaName) => {
  if (areaName !== "local") {
    return;
  }

  if (
    Object.prototype.hasOwnProperty.call(changes, "backgroundFetchEnabled") ||
    Object.prototype.hasOwnProperty.call(changes, "pollIntervalMinutes")
  ) {
    const config = await configService.getConfig();
    await syncAutonomousAlarm(
      Boolean(config?.backgroundFetchEnabled),
      config?.pollIntervalMinutes,
    );
  }
});

chrome.alarms.onAlarm.addListener(async (alarm) => {
  if (alarm.name !== AUTONOMOUS_ALARM) {
    return;
  }

  try {
    await processOnePostInAutonomousMode();
  } catch (error) {
    console.error("Autonomous mode failed to process post:", error);
  }
});
