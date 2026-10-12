import fs from 'node:fs';
import path from 'node:path';
import assert from 'node:assert/strict';
import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const dist=path.join(root,'dist');
const pages=['index.html','privacy/index.html','terms/index.html','support/index.html','data-request/index.html','404.html'];
const titles=new Set();let links=0;
for(const file of pages){
 const body=fs.readFileSync(path.join(dist,file),'utf8');
 assert.equal((body.match(/<h1\b/g)||[]).length,1,`${file} must have one H1`);
 const title=body.match(/<title>(.*?)<\/title>/s)?.[1];assert(title&& !titles.has(title),'Titles must be unique');titles.add(title);
 assert(body.includes('<meta name="description"'),'Missing description');assert(body.includes('<link rel="canonical"'),'Missing canonical');
 assert(body.includes('<html lang="en">'),'Missing document language');
 for(const [,schema]of body.matchAll(/<script type="application\/ld\+json">(.*?)<\/script>/gs))assert(JSON.parse(schema));
 for(const [image]of body.matchAll(/<img\b[^>]+>/g)){assert(/\balt="/.test(image),'Image without alt');assert(/\bwidth="/.test(image)&&/\bheight="/.test(image),'Image dimensions missing');}
 for(const [,href]of body.matchAll(/(?:href|src)="([^"]+)"/g)){
  if(!href.startsWith('/')&&!href.startsWith('#'))continue;
  let [base,anchor]=href.split('#');base=base.split('?')[0];
  let target=base?path.join(dist,base.endsWith('/')?base.slice(1)+'index.html':base.slice(1)):path.join(dist,file);
  assert(fs.existsSync(target),`${file}: broken ${href}`);
  if(anchor){const targetBody=fs.readFileSync(target,'utf8');assert(targetBody.includes(`id="${anchor}"`),`${file}: broken fragment ${href}`);}
  links++;
 }
 assert(!body.includes('★★★★★'),'No invented reviews');
}
const c=JSON.parse(fs.readFileSync(path.join(root,'content/site-config.json'),'utf8'));
if(!c.documentsApproved){for(const file of ['privacy/index.html','terms/index.html'])assert(fs.readFileSync(path.join(dist,file),'utf8').includes('Private review draft'));assert(fs.readFileSync(path.join(dist,'robots.txt'),'utf8').includes('Disallow: /'));}
for(const name of ['privacy','terms']){const text=fs.readFileSync(path.join(dist,name+'.txt'),'utf8');assert(text.length>8000,'Document incomplete');const embedded=path.join(root,'../CascadeUnity/Assets/_Cascade/Content/Resources/CascadeLegal',name+'.txt');if(fs.existsSync(embedded))assert.equal(text,fs.readFileSync(embedded,'utf8'),'Offline and web documents differ');}
assert(!/googletagmanager\.com|mc\.yandex\.ru/.test(fs.readFileSync(path.join(dist,'index.html'),'utf8')),'No unconditional analytics requests');
const critical=['display.ttf','body.ttf','hero-island.webp','mika.webp','cloud.webp','birds.webp','insects.webp','far-islands.webp','site.css','site.js','icon.webp'];
const bytes=critical.reduce((n,f)=>n+fs.statSync(path.join(dist,'assets',f)).size,0);
assert(bytes<2300000,'Critical scene payload exceeds budget');
console.log(`PASS: ${pages.length} pages, ${links} internal links/assets/fragments, JSON-LD, document parity, draft indexing and ${Math.round(bytes/1024)} KiB scene budget.`);
