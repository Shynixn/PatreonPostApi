export const DEFAULT_BASE_URL = "https://api.papoa.shynixn.com";
export const DEFAULT_POLL_INTERVAL_MINUTES = 10;

export type AppConfig = {
  baseUrl: string;
  apiKey: string;
  backgroundFetchEnabled: boolean;
  autoSubmitEnabled: boolean;
  pollIntervalMinutes: number;
  password: string;
};

const STORAGE_KEYS: (keyof AppConfig)[] = [
  "baseUrl",
  "apiKey",
  "backgroundFetchEnabled",
  "autoSubmitEnabled",
  "pollIntervalMinutes",
  "password",
];

export class ConfigService {
  async getConfig(): Promise<AppConfig | null> {
    const result = await chrome.storage.local.get(STORAGE_KEYS);

    if (!result.apiKey) {
      return null;
    }

    return {
      baseUrl: result.baseUrl || DEFAULT_BASE_URL,
      apiKey: result.apiKey,
      backgroundFetchEnabled: Boolean(result.backgroundFetchEnabled),
      autoSubmitEnabled: Boolean(result.autoSubmitEnabled),
      pollIntervalMinutes:
        Number(result.pollIntervalMinutes) || DEFAULT_POLL_INTERVAL_MINUTES,
      password: result.password || "",
    };
  }

  async saveConfig(config: AppConfig): Promise<void> {
    return new Promise((resolve) => {
      chrome.storage.local.set(config, resolve);
    });
  }
}
