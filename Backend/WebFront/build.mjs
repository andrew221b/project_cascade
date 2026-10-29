import fs from 'node:fs';import path from 'node:path';import crypto from 'node:crypto';import {fileURLToPath} from 'node:url';
const root=path.dirname(fileURLToPath(import.meta.url)),website=path.resolve(root,'../../Website'),dist=path.join(website,'dist'),assets={};
const files=[['/privacy/','privacy/index.html'],['/terms/','terms/index.html'],['/data-request/','data-request/index.html'],['/delete-account/','delete-account/index.html'],['/privacy.txt','privacy.txt'],['/terms.txt','terms.txt'],['/assets/deletion.js','assets/deletion.js']];
function add(route,body,type){assets[route]={type,body,sha256:crypto.createHash('sha256').update(body).digest('hex')};}
for(const[route,file]of files){
 let body=fs.readFileSync(path.join(dist,file),'utf8');
 if(route==='/delete-account/')body=body.replace('</head>','<link rel="stylesheet" href="/assets/deletion.css"></head>');
 const type=file.endsWith('.html')?'text/html; charset=utf-8':file.endsWith('.js')?'text/javascript; charset=utf-8':'text/plain; charset=utf-8';add(route,body,type);
 if(route.endsWith('/'))assets[route.slice(0,-1)]={redirect:route};
}
assets['/delete-account']=assets['/delete-account/'];
add('/assets/deletion.css',fs.readFileSync(path.join(website,'deletion.css'),'utf8'),'text/css; charset=utf-8');
const data='export const assets='+JSON.stringify(assets)+';\n';fs.writeFileSync(path.join(root,'assets.mjs'),data);
const source=fs.readFileSync(path.join(root,'worker.mjs'),'utf8').replace("import {assets} from './assets.mjs';",data.replace('export const','const'));
fs.writeFileSync(path.join(root,'worker.production.mjs'),source);
console.log(JSON.stringify({routes:Object.keys(assets),bytes:Buffer.byteLength(source),sha256:crypto.createHash('sha256').update(source).digest('hex')}));
