import { chromium } from 'playwright'
import { mkdir } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const rootDir = path.resolve(__dirname, '..')
const outputFile = path.join(rootDir, 'public', 'images', 'docs-homepage.png')
const url = process.env.DOCS_URL ?? 'http://127.0.0.1:4173/uptime-kuma-operator/'

await mkdir(path.dirname(outputFile), { recursive: true })

const browser = await chromium.launch({ headless: true })
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 1600 } })
  await page.goto(url, { waitUntil: 'networkidle' })
  await page.screenshot({ path: outputFile, fullPage: true })
  console.log(outputFile)
} finally {
  await browser.close()
}
