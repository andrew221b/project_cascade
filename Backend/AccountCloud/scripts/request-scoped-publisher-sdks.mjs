import { createRequire, builtinModules } from 'node:module';
import { fileURLToPath } from 'node:url';
const require = createRequire(import.meta.url);
// Wrangler is already a pinned build dependency and pins this esbuild version.
const { build } = createRequire(require.resolve('wrangler/package.json'))('esbuild');
const root = fileURLToPath(new URL('../', import.meta.url));
const builtinNames = new Set(builtinModules.map(n => n.replace(/^node:/, '')));
const virtualId = '\0cascade:request-scoped-publisher-sdks';
let prepared;
async function prepare() {
  const packages = [['Apple', '@apple/app-store-server-library'], ['Google', 'google-auth-library']];
  const bundles = [];
  for (const [name, id] of packages) {
    const plugins=name==='Apple'?[{name:'cascade-apple-native-fetch',setup(build){build.onResolve({filter:/^node-fetch$/},()=>({path:fileURLToPath(new URL('./worker-fetch.mjs',import.meta.url))}));}}]:[];
    const result = await build({stdin:{contents:`module.exports = require(${JSON.stringify(id)});`,resolveDir:root,sourcefile:`cascade-${name.toLowerCase()}-sdk.cjs`},plugins,bundle:true,platform:'node',format:'cjs',target:'es2022',write:false,metafile:true,legalComments:'inline',logLevel:'silent'});
    const imports = Object.values(result.metafile.outputs).flatMap(o => o.imports).filter(i => i.external).map(i => i.path).filter(id => id !== 'encoding'); // node-fetch optionally probes this absent legacy charset converter; publisher JSON is UTF-8.
    for (const id of imports) if (!builtinNames.has(id.replace(/^node:/,''))) throw new Error(`Unbundled publisher SDK dependency: ${id}`);
    bundles.push({name,code:result.outputFiles[0].text,imports});
  }
  const ids = [...new Set(bundles.flatMap(b => b.imports))];
  const imports = ids.map((id,i) => `import * as builtin${i} from ${JSON.stringify('node:'+id.replace(/^node:/,''))};`).join('\n');
  const map = ids.map((id,i) => `${JSON.stringify(id)}:builtin${i}.default??builtin${i}`).join(',');
  return imports + `\nconst builtins={${map}};\nfunction require(id){if(Object.hasOwn(builtins,id))return builtins[id];throw new Error('Unsupported SDK builtin');}\n` + bundles.map(b => `let cached${b.name};\nexport function load${b.name}Sdk(){if(!cached${b.name}){const module={exports:{}};const exports=module.exports;const __filename='/cascade/${b.name.toLowerCase()}-sdk.cjs';const __dirname='/cascade';\n${b.code}\ncached${b.name}=module.exports;}return cached${b.name};}\n`).join('\n');
}
export function requestScopedPublisherSdks() {
  return {name:'cascade-request-scoped-publisher-sdks',enforce:'pre',resolveId(source,importer){if(source==='./sdk.mjs' && /\/backend\/(publishers|unity-gateway)\.mjs$/.test(importer?.replace(/\\/g,'/')??''))return virtualId;},load(id){if(id===virtualId)return prepared??=prepare();}};
}
