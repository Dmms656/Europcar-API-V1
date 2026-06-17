import { chromium } from 'playwright';
import { writeFileSync } from 'node:fs';

const url = process.argv[2] ?? 'http://localhost:3456';
const errors = [];

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', (e) => errors.push(`PAGE: ${e.message}\n${e.stack ?? ''}`));
page.on('console', (m) => {
  if (m.type() === 'error') errors.push(`CONSOLE: ${m.text()}`);
});
page.on('requestfailed', (r) => {
  errors.push(`REQ_FAIL: ${r.url()} ${r.failure()?.errorText ?? ''}`);
});

await page.goto(url, { waitUntil: 'networkidle', timeout: 90000 });
await page.waitForTimeout(5000);

const rootLen = await page.evaluate(() => document.getElementById('root')?.innerHTML?.trim().length ?? 0);
const scriptSrc = await page.evaluate(() => {
  const s = document.querySelector('script[src*="entry-"]');
  return s?.getAttribute('src') ?? '';
});

const out = { url, scriptSrc, rootLen, errors };
writeFileSync('web-check.json', JSON.stringify(out, null, 2));
console.log(JSON.stringify(out, null, 2));
await browser.close();
