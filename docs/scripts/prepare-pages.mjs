import { cp, mkdir, readdir } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const docsRoot = path.resolve(__dirname, '..')
const distRoot = path.join(docsRoot, '.vitepress', 'dist')

await mkdir(distRoot, { recursive: true })

const staticFiles = ['index.yaml']
for (const fileName of staticFiles) {
  const source = path.join(docsRoot, fileName)
  const destination = path.join(distRoot, fileName)
  await cp(source, destination)
}

for (const fileName of await readdir(docsRoot)) {
  if (!fileName.endsWith('.tgz')) continue
  const source = path.join(docsRoot, fileName)
  const destination = path.join(distRoot, fileName)
  await cp(source, destination)
}

console.log(`Prepared Pages output in ${distRoot}`)
