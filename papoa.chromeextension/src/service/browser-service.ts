// @ts-ignore
import { marked } from "../lib/marked.js";

export class BrowserService {
  private async resolveTabId(tabId?: number): Promise<number> {
    if (tabId) return tabId;
    const tabs = await chrome.tabs.query({ active: true, currentWindow: true });
    if (!tabs[0]?.id) throw new Error("No active tab found");
    return tabs[0].id;
  }

  /**
   * Simulates typing or setting HTML into the body of an element found by attribute name and value.
   * @param attributeName The name of the attribute to match.
   * @param attributeValue The value of the attribute to match.
   * @param text The text or HTML to insert into the element's body.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async writeElementByAttribute(
    attributeName: string,
    attributeValue: string,
    text: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (attrName: string, attrValue: string, txt: string) => {
        let el = document.querySelector(`[${attrName}='${attrValue}']`) as any;
        if (!el) {
          el = document.querySelector(`[${attrName}="${attrValue}"]`) as any;
        }
        if (!el) {
          console.warn(
            `Element with attribute ${attrName}='${attrValue}' not found.`,
          );
          return;
        }
        el.focus && el.focus();
        // Check if the text contains HTML tags (indicating it's HTML content)
        const isHtml = /<[a-z][\s\S]*>/i.test(txt);
        if (isHtml) {
          el.innerHTML = txt;
        } else {
          const doc = el.ownerDocument;
          if (doc) {
            doc.execCommand("selectAll", false);
            doc.execCommand("delete", false);
            doc.execCommand("insertText", false, txt);
          } else {
            console.warn(
              "Element has no ownerDocument, cannot insert text properly.",
            );
            el.textContent = txt;
          }
        }
      },
      args: [attributeName, attributeValue, text],
    });
  }

  /**
   * Clicks the first element with the given attribute name and value in the active tab.
   * @param attributeName The name of the attribute to match.
   * @param attributeValue The value of the attribute to match.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async clickElementByAttribute(
    attributeName: string,
    attributeValue: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (attrName: string, attrValue: string) => {
        let el;
        try {
          el = document.querySelector(`[${attrName}='${attrValue}']`) as any;
        } catch {}
        if (!el) {
          el = document.querySelector(`[${attrName}="${attrValue}"]`) as any;
        }
        if (el) {
          console.log(el);
          el.click();
        } else {
          console.warn(
            `Element with attribute [${attrName}="${attrValue}"] not found.`,
          );
        }
      },
      args: [attributeName, attributeValue],
    });
  }

  /**
   * Clicks the first element with the given class name in the active tab.
   * @param className The class name of the element to click.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async clickElementByClassName(
    className: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (cls: string) => {
        const el = document.getElementsByClassName(cls)[0] as HTMLElement;
        if (el) {
          el.click();
        } else {
          console.warn(`Element with class '${cls}' not found.`);
        }
      },
      args: [className],
    });
  }

  /**
   * Clicks the element with the given ID in the active tab.
   * @param elementId The ID of the element to click.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async clickElementById(elementId: string, tabId?: number): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (id: string) => {
        const el = document.getElementById(id) as HTMLElement;
        if (el) {
          console.warn(`Clicked '${el}' not found.`);
          el.click();
        } else {
          console.warn(`Element with ID '${id}' not found.`);
        }
      },
      args: [elementId],
    });
  }

  async markdown2Html(markdown: string): Promise<string> {
    const html = marked.parse(markdown).replace(/\n/g, "");
    return html;
  }

  /**
   * Sets the value of an input, select, or textarea element found by ID.
   * Fires input and change events so framework listeners are notified.
   * @param elementId The ID of the element.
   * @param value The value to set.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async setValueById(
    elementId: string,
    value: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (id: string, val: string) => {
        const el = document.getElementById(id) as
          | HTMLInputElement
          | HTMLSelectElement
          | HTMLTextAreaElement
          | null;
        if (!el) {
          console.warn(`Element with ID '${id}' not found.`);
          return;
        }
        el.value = val;
        el.dispatchEvent(new Event("input", { bubbles: true }));
        el.dispatchEvent(new Event("change", { bubbles: true }));
      },
      args: [elementId, value],
    });
  }

  /**
   * Simulates typing or setting HTML into the body of an element found by ID.
   * @param elementId The ID of the element.
   * @param text The text or HTML to insert into the element's body.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async writeElementById(
    elementId: string,
    text: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (id: string, txt: string) => {
        const el = document.getElementById(id) as any;
        if (!el) {
          console.warn(`Element with ID '${id}' not found.`);
          return;
        }
        el.focus && el.focus();
        const isHtml = /<[a-z][\s\S]*>/i.test(txt);
        if (isHtml) {
          el.innerHTML = txt;
        } else {
          const doc = el.ownerDocument;
          if (doc) {
            const selection = doc.getSelection && doc.getSelection();
            if (selection && el.firstChild) {
              const range = doc.createRange();
              range.selectNodeContents(el);
              selection.removeAllRanges();
              selection.addRange(range);
              doc.execCommand && doc.execCommand("delete", false);
            } else {
              el.innerHTML = "";
            }
            for (let i = 0; i < txt.length; i++) {
              const char = txt[i];
              doc.execCommand && doc.execCommand("insertText", false, char);
            }
          } else {
            el.textContent = txt;
          }
        }
      },
      args: [elementId, text],
    });
  }

  /**
   * Simulates typing text into the body of an iframe by its index on the page.
   * @param iframeIndex The index of the iframe in document.querySelectorAll('iframe').
   * @param text The text to type into the iframe's body.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async writeIFrameByIndex(
    iframeIndex: number,
    text: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (idx: number, txt: string) => {
        const iframes = document.querySelectorAll<HTMLIFrameElement>("iframe");
        const iframe = iframes[idx];
        if (!iframe) {
          console.warn(`No iframe found at index ${idx}`);
          return;
        }
        try {
          const win = iframe.contentWindow;
          const doc = win?.document;
          const body = doc?.body;
          if (!body) {
            console.warn("No body found in iframe");
            return;
          }
          // Focus the body
          body.focus();
          // Clear the body text before setting HTML
          if (doc) {
            // Check if the text contains HTML tags (indicating it's HTML content)
            const isHtml = /<[a-z][\s\S]*>/i.test(txt);
            if (isHtml) {
              // If it's HTML, set it directly as innerHTML
              body.innerHTML = txt;
            } else {
              // If it's plain text, use the original character-by-character insertion
              // Try to select all and delete
              const selection = doc.getSelection && doc.getSelection();
              if (selection && body.firstChild) {
                const range = doc.createRange();
                range.selectNodeContents(body);
                selection.removeAllRanges();
                selection.addRange(range);
                doc.execCommand("delete", false);
              } else {
                body.innerHTML = "";
              }
              // Use execCommand to insert each character
              for (let i = 0; i < txt.length; i++) {
                const char = txt[i];
                doc?.execCommand("insertText", false, char);
              }
            }
          }
        } catch (e) {
          console.warn("Error writing to iframe:", e);
        }
      },
      args: [iframeIndex, text],
    });
  }

  /**
   * Delays execution for a specified number of milliseconds.
   * @param ms Number of milliseconds to wait.
   */
  async delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  async getTabUrl(tabId?: number): Promise<string> {
    const targetTabId = await this.resolveTabId(tabId);
    const tab = await chrome.tabs.get(targetTabId);
    return tab.url ?? "";
  }

  /**
   * Sets the value of an input or textarea found by name attribute.
   * @param name The name attribute of the element.
   * @param text The value to set.
   * @param tabId Optional tab ID to target, defaults to active tab.
   * @param index Optional index when multiple elements share the same name (default: 0).
   */
  async writeElementByName(
    name: string,
    text: string,
    tabId?: number,
    index: number = 0,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (inputName: string, value: string, idx: number) => {
        // Try input first, then textarea, using index
        let el = document.querySelectorAll<HTMLInputElement>(
          `input[name='${inputName}']`,
        )[idx] as any;
        if (!el) {
          el = document.querySelectorAll<HTMLTextAreaElement>(
            `textarea[name='${inputName}']`,
          )[idx] as HTMLTextAreaElement | undefined;
        }
        if (el) {
          el.value = value;
          el.dispatchEvent(new Event("input", { bubbles: true }));
          el.dispatchEvent(new Event("change", { bubbles: true }));
        } else {
          console.warn(
            `Input or textarea with name '${inputName}' and index ${idx} not found.`,
          );
        }
      },
      args: [name, text, index],
    });
  }

  /**
   * Navigates the active tab to the given URL and waits until the page is fully loaded.
   * @param url The URL to navigate to.
   */
  async navigateActiveTabTo(url: string): Promise<number> {
    const tabs = await chrome.tabs.query({ active: true, currentWindow: true });
    if (!tabs[0]?.id) throw new Error("No active tab found");
    const tabId = tabs[0].id;
    await chrome.tabs.update(tabId, { url });
    // Wait for the tab to finish loading
    await new Promise<void>((resolve) => {
      function listener(
        updatedTabId: number,
        changeInfo: chrome.tabs.TabChangeInfo,
      ) {
        if (updatedTabId === tabId && changeInfo.status === "complete") {
          chrome.tabs.onUpdated.removeListener(listener);
          resolve();
        }
      }
      chrome.tabs.onUpdated.addListener(listener);
    });
    return tabId;
  }

  /**
   * Navigates a specific tab to a URL and waits until it is fully loaded.
   * @param tabId The tab to navigate.
   * @param url The destination URL.
   */
  async navigateTabTo(tabId: number, url: string): Promise<number> {
    await chrome.tabs.update(tabId, { url });
    await new Promise<void>((resolve) => {
      function listener(
        updatedTabId: number,
        changeInfo: chrome.tabs.TabChangeInfo,
      ) {
        if (updatedTabId === tabId && changeInfo.status === "complete") {
          chrome.tabs.onUpdated.removeListener(listener);
          resolve();
        }
      }
      chrome.tabs.onUpdated.addListener(listener);
    });
    return tabId;
  }

  /**
   * Opens a new focused window with one tab at the given URL and waits for that tab to finish loading.
   * @param url URL to open.
   */
  async openNewWindowTo(url: string): Promise<number> {
    const createdWindow = await chrome.windows.create({ url, focused: true });
    const tabId = createdWindow.tabs?.[0]?.id;
    if (!tabId) {
      throw new Error("Failed to open Patreon window.");
    }
    await new Promise<void>((resolve) => {
      function listener(
        updatedTabId: number,
        changeInfo: chrome.tabs.TabChangeInfo,
      ) {
        if (updatedTabId === tabId && changeInfo.status === "complete") {
          chrome.tabs.onUpdated.removeListener(listener);
          resolve();
        }
      }
      chrome.tabs.onUpdated.addListener(listener);
    });
    return tabId;
  }

  /**
   * Clicks on an element in the active tab using the provided XPath.
   * @param xPath The XPath of the element to click.
   * @param tabId The tab ID to execute the click in (optional, defaults to active tab)
   */
  async clickElementByXPath(xPath: string, tabId?: number): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (xpath: string) => {
        const result = document.evaluate(
          xpath,
          document,
          null,
          XPathResult.FIRST_ORDERED_NODE_TYPE,
          null,
        );
        const element = result.singleNodeValue as HTMLElement;
        if (element) {
          element.click();
        } else {
          console.warn(`Element not found for XPath: ${xpath}`);
        }
      },
      args: [xPath],
    });
  }

  /**
   * Dispatches a key press on the currently focused element in the tab.
   * @param key The key value to dispatch (e.g. "Enter", "Escape", "Tab").
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async pressKey(key: string, tabId?: number): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (k: string) => {
        const el = (document.activeElement ?? document.body) as HTMLElement;
        const init: KeyboardEventInit = {
          key: k,
          bubbles: true,
          cancelable: true,
        };
        el.dispatchEvent(new KeyboardEvent("keydown", init));
        el.dispatchEvent(new KeyboardEvent("keypress", init));
        el.dispatchEvent(new KeyboardEvent("keyup", init));
      },
      args: [key],
    });
  }

  /**
   * Unchecks all checkboxes on the page.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async uncheckAll(tabId?: number): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: () => {
        const checkboxes = document.querySelectorAll<HTMLInputElement>(
          'input[type="checkbox"]',
        );
        checkboxes.forEach((checkbox) => {
          checkbox.checked = false;
          checkbox.dispatchEvent(new Event("change", { bubbles: true }));
        });
      },
      args: [],
    });
  }

  /**
   * Removes all tags by clicking their close buttons, skipping the first 2.
   * Queries all elements with data-tag="IconClose", skips the first 2 entries,
   * and clicks all remaining ones.
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async clearTags(tabId?: number): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: async () => {
        const closeButtons = document.querySelectorAll<HTMLElement>(
          '[data-tag="IconClose"]',
        );
        const buttons = Array.from(closeButtons).slice(2);
        for (const btn of buttons) {
          btn.parentElement!.click();
          await new Promise((resolve) => setTimeout(resolve, 500));
        }
      },
      args: [],
    });
  }

  async uploadFile(
    elementId: string,
    fileContent: ArrayBuffer | Uint8Array,
    fileName: string,
    mimeType: string,
    useChildSelect: boolean = false,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    // Convert fileContent to base64 for transfer
    const base64 = btoa(String.fromCharCode(...new Uint8Array(fileContent)));
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (
        id: string,
        b64: string,
        fname: string,
        mtype: string,
        useChild: boolean,
      ) => {
        let el = document.getElementById(id) as HTMLInputElement;
        if (useChild) {
          el = Array.from(el.children).filter(
            (c) => c.tagName === "INPUT",
          )[0] as HTMLInputElement;
        }
        if (!el || el.type !== "file") {
          console.warn(`Element with ID '${id}' is not a file input.`);
          return;
        }
        // Convert base64 to Blob
        const byteString = atob(b64);
        const ab = new ArrayBuffer(byteString.length);
        const ia = new Uint8Array(ab);
        for (let i = 0; i < byteString.length; i++) {
          ia[i] = byteString.charCodeAt(i);
        }
        const file = new File([ab], fname, { type: mtype });
        // Create a DataTransfer to set files property
        const dt = new DataTransfer();
        dt.items.add(file);
        el.files = dt.files;
        el.dispatchEvent(new Event("input", { bubbles: true }));
        el.dispatchEvent(new Event("change", { bubbles: true }));
      },
      args: [
        String(elementId),
        String(base64),
        String(fileName),
        String(mimeType),
        Boolean(useChildSelect),
      ],
    });
  }

  /**
   * Clicks the first <button> element whose trimmed innerHTML matches the given string.
   * @param innerHtml The exact inner HTML to match (compared after trimming).
   * @param tabId Optional tab ID to target, defaults to active tab.
   */
  async clickButtonByInnerHtml(
    innerHtml: string,
    tabId?: number,
  ): Promise<void> {
    const targetTabId = await this.resolveTabId(tabId);
    await chrome.scripting.executeScript({
      target: { tabId: targetTabId },
      func: (html: string) => {
        const buttons = Array.from(
          document.querySelectorAll<HTMLButtonElement>("button"),
        );
        const button = buttons.find((b) => b.innerHTML.trim() === html.trim());
        if (button) {
          button.click();
        } else {
          console.warn(`Button with innerHTML '${html}' not found.`);
        }
      },
      args: [innerHtml],
    });
  }
}
