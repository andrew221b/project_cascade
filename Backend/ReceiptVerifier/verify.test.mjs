import { test } from 'node:test';
import assert from 'node:assert/strict';
import { accountBinding, authorizeService, validateServiceConfig, verifyPurchase, verifyServicePurchase } from './verify.mjs';
const base = { bundleId: 'com.vickand.cascade', productId: 'cascade.coins_240', transactionId: 'paid-1', store: 'AppleAppStore', jws: 'signed-evidence' };
const apple = { ...base, quantity: 1, type: 'Consumable', inAppOwnershipType: 'PURCHASED' };
const adapters = (purchase = apple) => ({ bundleId: base.bundleId, apple: async () => purchase, google: async () => ({ purchaseState: 0, orderId: base.transactionId, quantity: 1 }) });
test('returns only the exact verified product and transaction', async () => assert.deepEqual(await verifyPurchase(base, adapters()), { valid: true, productId: base.productId, transactionId: base.transactionId }));
test('rejects untrusted bundle, unknown SKU and store', async () => {
  for (const change of [{ bundleId: 'other' }, { productId: 'fake' }, { store: 'FakeStore' }, { transactionId: '' }]) assert.equal((await verifyPurchase({ ...base, ...change }, adapters())).valid, false);
});
test('rejects revoked, shared, mismatched and multi-quantity Apple evidence', async () => {
  for (const change of [{ revocationDate: 1 }, { inAppOwnershipType: 'FAMILY_SHARED' }, { bundleId: 'other' }, { productId: 'cascade.coins_480' }, { transactionId: 'another' }, { quantity: 2 }, { type: 'Non-Consumable' }]) assert.equal((await verifyPurchase(base, adapters({ ...apple, ...change }))).valid, false);
});
const googleProof = () => ({ ...base, store: 'GooglePlay', receipt: JSON.stringify({ Store: 'GooglePlay', Payload: JSON.stringify({ json: JSON.stringify({ packageName: base.bundleId, productId: base.productId, purchaseToken: 'token', purchaseState: 0 }) }) }) });
test('Google publisher API, rather than submitted state, decides eligibility', async () => {
  assert.equal((await verifyPurchase(googleProof(), adapters())).valid, true);
  for (const change of [{ purchaseState: 1 }, { purchaseState: 2 }, { orderId: 'other' }, { quantity: 2 }]) {
    const a = adapters(); a.google = async () => ({ purchaseState: 0, orderId: base.transactionId, quantity: 1, ...change });
    assert.equal((await verifyPurchase(googleProof(), a)).valid, false);
  }
});
test('store outage propagates so the pending purchase can retry', async () => {
  const a = adapters(); a.apple = async () => { throw new Error('offline'); }; await assert.rejects(verifyPurchase(base, a));
});
test('subscription grants only the live store expiry, never a lifetime flag', async () => {
  const proof = { ...base, productId: 'cascade.no_ads_month' };
  const expiry = Math.floor(Date.now() / 1000) + 3600;
  const a = { ...adapters({ ...apple, productId: proof.productId, type: 'Auto-Renewable Subscription' }), appleSubscription: async () => expiry };
  assert.deepEqual(await verifyPurchase(proof, a), { valid: true, productId: proof.productId, transactionId: proof.transactionId, expiresUnix: expiry });
  a.appleSubscription = async () => 0;
  assert.equal((await verifyPurchase(proof, a)).expiresUnix, 0);
  a.apple = async () => ({ ...apple, productId: proof.productId, type: 'Non-Consumable' });
  assert.equal((await verifyPurchase(proof, a)).valid, false);
});
test('Google subscription status and account binding control entitlement', async () => {
  const proof = { ...googleProof(), productId: 'cascade.no_ads_year' };
  const envelope = JSON.parse(proof.receipt), payload = JSON.parse(envelope.Payload), inner = JSON.parse(payload.json);
  inner.productId = proof.productId; payload.json = JSON.stringify(inner); envelope.Payload = JSON.stringify(payload); proof.receipt = JSON.stringify(envelope);
  const binding = { googleAccountId: 'bound-player' };
  const expiryTime = new Date(Date.now() + 3600000).toISOString();
  const data = { packageName: proof.bundleId, latestOrderId: proof.transactionId, subscriptionState: 'SUBSCRIPTION_STATE_ACTIVE', externalAccountIdentifiers: { obfuscatedExternalAccountId: binding.googleAccountId }, lineItems: [{ productId: proof.productId, expiryTime }] };
  const a = { ...adapters(), purchaseIdentity: binding, googleSubscription: async () => data };
  assert.ok((await verifyPurchase(proof, a)).expiresUnix > Date.now() / 1000);
  a.googleSubscription = async () => ({ ...data, subscriptionState: 'SUBSCRIPTION_STATE_ON_HOLD' });
  assert.equal((await verifyPurchase(proof, a)).expiresUnix, 0);
  a.googleSubscription = async () => ({ ...data, externalAccountIdentifiers: { obfuscatedExternalAccountId: 'other' } });
  assert.equal((await verifyPurchase(proof, a)).valid, false);
});

const serviceToken = 'test-only-service-token-32-characters';
const authorization = `Bearer ${serviceToken}`;
const context = { playerId: 'ugs-player-1', projectId: 'ugs-project-1', environmentId: 'ugs-development-1' };
const serviceAdapters = (purchase = apple) => ({ ...adapters({ ...purchase, appAccountToken: accountBinding(context.projectId, context.environmentId, context.playerId).appleToken }), serviceToken, expectedProjectId: context.projectId, expectedEnvironmentId: context.environmentId, google: async () => ({ purchaseState: 0, orderId: base.transactionId, quantity: 1, obfuscatedExternalAccountId: accountBinding(context.projectId, context.environmentId, context.playerId).googleAccountId }) });
const serviceProof = () => ({ ...base, ...context });

test('service authorization rejects missing, malformed and wrong tokens before contacting stores', async () => {
  let calls = 0; const a = serviceAdapters(); a.apple = async () => { calls++; return apple; };
  for (const header of [undefined, '', 'Basic token', 'Bearer wrong', `Bearer ${serviceToken.slice(0, -1)}X`, `${authorization}, ${authorization}`, [authorization]]) {
    assert.equal(authorizeService(header, serviceToken), false);
    assert.deepEqual(await verifyServicePurchase(serviceProof(), header, a), { status: 401, body: { Valid: false } });
  }
  assert.equal(calls, 0);
});

test('service configuration cannot silently omit a secret, project or environment', () => {
  for (const change of [{ serviceToken: '' }, { serviceToken: 'short' }, { expectedProjectId: '' }, { expectedEnvironmentId: '' }]) {
    assert.throws(() => validateServiceConfig({ ...serviceAdapters(), ...change }));
  }
});

test('authorized calls cannot cross project or environment boundaries', async () => {
  let calls = 0; const a = serviceAdapters(); a.apple = async () => { calls++; return apple; };
  for (const change of [{ projectId: 'another-project' }, { environmentId: 'production' }, { projectId: undefined }, { environmentId: undefined }]) {
    assert.deepEqual(await verifyServicePurchase({ ...serviceProof(), ...change }, authorization, a), { status: 403, body: { Valid: false } });
  }
  assert.equal(calls, 0);
});

test('caller identity is required, bounded and cannot be manufactured with verdict fields', async () => {
  let calls = 0; const a = serviceAdapters(); a.apple = async () => { calls++; return apple; };
  for (const playerId of [undefined, '', ' ', 'player\nother', 'player/other', 'x'.repeat(129), { PlayerId: context.playerId }]) {
    assert.deepEqual(await verifyServicePurchase({ ...serviceProof(), playerId, Valid: true, PlayerBindingVerified: true }, authorization, a), { status: 400, body: { Valid: false } });
  }
  assert.equal(calls, 0);
  const forged = { ...serviceProof(), PlayerId: 'forged-player', ProjectId: 'forged-project', Valid: true, CatalogVerified: true };
  const result = await verifyServicePurchase(forged, authorization, serviceAdapters());
  assert.equal(result.body.PlayerId, context.playerId); assert.equal(result.body.ProjectId, context.projectId);
});

test('backend returns the exact publisher-verified PascalCase Cloud Code contract', async () => {
  assert.deepEqual(await verifyServicePurchase(serviceProof(), authorization, serviceAdapters()), { status: 200, body: {
    Valid: true, PlayerBindingVerified: true, CatalogVerified: true,
    PlayerId: context.playerId, ProjectId: context.projectId, EnvironmentId: context.environmentId,
    ProductId: base.productId, TransactionId: base.transactionId, Platform: base.store, BundleId: base.bundleId
  } });
  const proof = { ...googleProof(), ...context };
  const google = await verifyServicePurchase(proof, authorization, serviceAdapters());
  assert.equal(google.body.Valid, true); assert.equal(google.body.Platform, 'GooglePlay');
});

test('no authenticated binding verdict is emitted until the publisher accepts the purchase', async () => {
  assert.deepEqual(await verifyServicePurchase(serviceProof(), authorization, serviceAdapters({ ...apple, revocationDate: 1 })), { status: 200, body: { Valid: false } });
  const a = serviceAdapters(); a.apple = async () => { throw new Error('store unavailable'); };
  await assert.rejects(verifyServicePurchase(serviceProof(), authorization, a));
});

test('signed Apple and publisher Google evidence must belong to the authenticated player', async () => {
  const expected = accountBinding(context.projectId, context.environmentId, context.playerId);
  const options = serviceAdapters();
  options.apple = async () => ({ ...apple, appAccountToken: accountBinding(context.projectId, context.environmentId, 'another-player').appleToken });
  assert.equal((await verifyServicePurchase(serviceProof(), authorization, options)).body.Valid, false);
  options.apple = async () => ({ ...apple });
  assert.equal((await verifyServicePurchase(serviceProof(), authorization, options)).body.Valid, false);
  options.apple = async () => ({ ...apple, appAccountToken: expected.appleToken });
  assert.equal((await verifyServicePurchase(serviceProof(), authorization, options)).body.Valid, true);
  const google = { ...googleProof(), ...context };
  options.google = async () => ({ purchaseState: 0, orderId: base.transactionId, quantity: 1, obfuscatedExternalAccountId: 'another-player' });
  assert.equal((await verifyServicePurchase(google, authorization, options)).body.Valid, false);
  options.google = async () => ({ purchaseState: 0, orderId: base.transactionId, quantity: 1, obfuscatedExternalAccountId: expected.googleAccountId });
  assert.equal((await verifyServicePurchase(google, authorization, options)).body.Valid, true);
});

// Each new tier must use the same publisher evidence and account binding as legacy tiers.
test('all four new coin tiers accept matching publisher evidence and reject a different tier', async () => {
  for (const productId of ['cascade.coins_600', 'cascade.coins_720', 'cascade.coins_960', 'cascade.coins_1080']) {
    const proof = { ...base, productId };
    assert.equal((await verifyPurchase(proof, adapters({ ...apple, productId }))).valid, true);
    assert.equal((await verifyPurchase(proof, adapters(apple))).valid, false);
    const google = googleProof(); google.productId = productId;
    const envelope = JSON.parse(google.receipt); const payload = JSON.parse(envelope.Payload);
    const receipt = JSON.parse(payload.json); receipt.productId = productId;
    payload.json = JSON.stringify(receipt); envelope.Payload = JSON.stringify(payload); google.receipt = JSON.stringify(envelope);
    assert.equal((await verifyPurchase(google, adapters())).valid, true);
    receipt.productId = 'cascade.coins_240'; payload.json = JSON.stringify(receipt); envelope.Payload = JSON.stringify(payload); google.receipt = JSON.stringify(envelope);
    assert.equal((await verifyPurchase(google, adapters())).valid, false);
  }
});
