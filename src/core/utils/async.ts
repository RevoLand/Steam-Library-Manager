export async function waitWhile(condition: () => boolean, interval = 200): Promise<void> {
  while (condition()) {
    await new Promise((resolve) => setTimeout(resolve, interval));
  }
}
