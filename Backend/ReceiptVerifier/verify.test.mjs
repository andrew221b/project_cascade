import { test } from 'node:test';
import assert from 'node:assert/strict';
import { verifyPurchase } from './verify.mjs';
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
