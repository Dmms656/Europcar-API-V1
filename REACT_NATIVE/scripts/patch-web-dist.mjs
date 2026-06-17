/**
 * Render y otros hosts estáticos ignoran carpetas que empiezan con "_"
 * (p. ej. _expo). Renombra a expo-static y actualiza referencias.
 */
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const dist = path.resolve(__dirname, '..', 'dist');
const FROM = '/_expo/';
const TO = '/expo-static/';

if (!fs.existsSync(dist)) {
  console.error('[patch-web-dist] No existe dist/. Ejecuta expo export primero.');
  process.exit(1);
}

const legacy = path.join(dist, '_expo');
const target = path.join(dist, 'expo-static');

if (fs.existsSync(legacy)) {
  if (fs.existsSync(target)) fs.rmSync(target, { recursive: true, force: true });
  fs.cpSync(legacy, target, { recursive: true });
  fs.rmSync(legacy, { recursive: true, force: true });
  console.log('[patch-web-dist] _expo → expo-static');
}

// GitHub Pages / Jekyll
fs.writeFileSync(path.join(dist, '.nojekyll'), '');

const EXT = new Set(['.html', '.js', '.json', '.css']);

function patchFile(filePath) {
  const raw = fs.readFileSync(filePath, 'utf8');
  if (!raw.includes(FROM)) return;
  const next = raw.replaceAll(FROM, TO);
  if (next !== raw) fs.writeFileSync(filePath, next);
}

function walk(dir) {
  for (const name of fs.readdirSync(dir)) {
    const p = path.join(dir, name);
    const st = fs.statSync(p);
    if (st.isDirectory()) walk(p);
    else if (EXT.has(path.extname(name))) patchFile(p);
  }
}

walk(dist);

const bundle = path.join(target, 'static', 'js', 'web');
if (!fs.existsSync(bundle)) {
  console.error('[patch-web-dist] No se encontró el bundle JS en expo-static/static/js/web');
  process.exit(1);
}

console.log('[patch-web-dist] Listo:', fs.readdirSync(bundle).join(', '));
