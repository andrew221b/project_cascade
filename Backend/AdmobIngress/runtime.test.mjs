import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { Miniflare, createFetchMock } = require(process.env.CASCADE_MINIFLARE_PATH ?? '../AccountCloud/node_modules/miniflare');

test('actual Workers runtime exchanges a service token and returns verified Unity readiness', async () => {
 const mock = createFetchMock();
 mock.disableNetConnect();
 mock.get('https://services.api.unity.com').intercept({
  method: 'POST', path: '/auth/v1/token-exchange?projectId=a1b8a760-5a0c-4c2d-8ae7-54c1b411259a&environmentId=2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7'
 }).reply(201, JSON.stringify({accessToken: 'fake-service-jwt'}), {headers: {'content-type': 'application/json'}});
 mock.get('https://cloud-code.services.api.unity.com').intercept({
  method: 'POST', path: '/v1/projects/a1b8a760-5a0c-4c2d-8ae7-54c1b411259a/modules/CascadeAccount/BackendStatus'
 }).reply(200, JSON.stringify({output: {Version:'20261028.2',Ready:true,Healthy:true,MaintenanceFresh:true}}), {headers: {'content-type': 'application/json'}});
 const worker = new Miniflare({
  modules: true, script: (await readFile(new URL('./worker.production.mjs', import.meta.url), 'utf8')).replace('export function createHandler', 'function createHandler'),
  compatibilityDate: '2026-05-15', compatibilityFlags: ['nodejs_compat'],
  bindings: {UNITY_SERVICE_KEY_ID: 'fake-id', UNITY_SERVICE_SECRET: 'fake-secret'}, fetchMock: mock
 });
 try {
  const response = await worker.dispatchFetch('https://vickand.com/api/flying-ladders/health');
  assert.equal(response.status, 200);
  assert.deepEqual(await response.json(), {service:'flying-ladders-ingress',backend:'UnityCloudCode',storage:'UnityCloudSave',version:'20261028.2',ready:true,healthy:true,maintenanceFresh:true});
  mock.assertNoPendingInterceptors();
 } finally { await worker.dispose(); await mock.close(); }
});
