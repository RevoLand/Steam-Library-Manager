import { blake3, createBLAKE3 } from 'hash-wasm';
import fs from 'node:fs';

export async function hashText(text: string): Promise<string> {
  return blake3(text);
}

export async function hashFile(filePath: string): Promise<string> {
  const hasher = await createBLAKE3();

  return new Promise((resolve, reject) => {
    const stream = fs.createReadStream(filePath);

    stream.on('data', (chunk) => hasher.update(chunk));
    stream.on('end', () => resolve(hasher.digest()));
    stream.on('error', reject);
  });
}
