const SALT_SIZE = 16;
const IV_SIZE = 16;
const PBKDF2_ITERATIONS = 600_000;

export class CryptoService {
  /**
   * Decrypts binary data encrypted by CbcEncryptingStream.
   * Wire format: salt(16) | IV(16) | AES-256-CBC/PKCS7 ciphertext.
   * Key is derived via PBKDF2-SHA256 with 600 000 iterations.
   */
  async decryptBytes(
    encrypted: ArrayBuffer,
    password: string,
  ): Promise<ArrayBuffer> {
    const encryptedBytes = new Uint8Array(encrypted);
    const salt = encryptedBytes.slice(0, SALT_SIZE);
    const iv = encryptedBytes.slice(SALT_SIZE, SALT_SIZE + IV_SIZE);
    const ciphertext = encryptedBytes.slice(SALT_SIZE + IV_SIZE);

    const keyMaterial = await crypto.subtle.importKey(
      "raw",
      new TextEncoder().encode(password),
      "PBKDF2",
      false,
      ["deriveKey"],
    );

    const key = await crypto.subtle.deriveKey(
      {
        name: "PBKDF2",
        salt,
        iterations: PBKDF2_ITERATIONS,
        hash: "SHA-256",
      },
      keyMaterial,
      { name: "AES-CBC", length: 256 },
      false,
      ["decrypt"],
    );

    return crypto.subtle.decrypt({ name: "AES-CBC", iv }, key, ciphertext);
  }

  /**
   * Decrypts a base64-encoded string encrypted by CbcEncryptingStream.
   * Wire format: salt(16) | IV(16) | AES-256-CBC/PKCS7 ciphertext.
   * Key is derived via PBKDF2-SHA256 with 600 000 iterations.
   */
  async decrypt(encryptedBase64: string, password: string): Promise<string> {
    const encryptedBytes = base64ToBytes(encryptedBase64);

    const salt = encryptedBytes.slice(0, SALT_SIZE);
    const iv = encryptedBytes.slice(SALT_SIZE, SALT_SIZE + IV_SIZE);
    const ciphertext = encryptedBytes.slice(SALT_SIZE + IV_SIZE);

    const keyMaterial = await crypto.subtle.importKey(
      "raw",
      new TextEncoder().encode(password),
      "PBKDF2",
      false,
      ["deriveKey"],
    );

    const key = await crypto.subtle.deriveKey(
      {
        name: "PBKDF2",
        salt,
        iterations: PBKDF2_ITERATIONS,
        hash: "SHA-256",
      },
      keyMaterial,
      { name: "AES-CBC", length: 256 },
      false,
      ["decrypt"],
    );

    const decrypted = await crypto.subtle.decrypt(
      { name: "AES-CBC", iv },
      key,
      ciphertext,
    );

    return new TextDecoder().decode(decrypted);
  }
}

function base64ToBytes(base64: string): Uint8Array {
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
}
