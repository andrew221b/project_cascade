# Google server setup — current result, 20 October 2026

The Worker SDK/global initialization fix is published. The previous source-token publication block is resolved. The old 503 result is superseded: a real message published through `projects/tracker-project-497108/topics/flying-ladders-rtdn` reached the authenticated endpoint at 14:00:27.435Z with HTTP 200. Unsigned delivery is rejected with 401. Evidence: `Reports/CloudFinishOct20/google-publish-test.json`, `deployment-verification.json`, `verification.md`.

The dedicated topic grants Google Play only topic-level Pub/Sub Publisher. The authenticated `flying-ladders-rtdn-push` subscription uses the exact service-account identity and endpoint audience, seven-day retention/retries and no automatic idle expiration. Existing topics and RevenueCat subscriptions for other applications were preserved. Original setup evidence: `Reports/GoogleRtdnOct20/verification.md`.

The IAM introspection API remains disabled. This does not invalidate successful topic/subscription creation or actual signed delivery. No additional IAM roles are currently requested for Google Pub/Sub.

This was a TEST message, not a real purchase, renewal or refund. Google Play Console's RTDN topic setting and a real store transaction are still to be verified. Update 21 October: the existing Google JSON is now installed in the backend AES-GCM sealed credential record, loaded as GOOGLE_CREDENTIALS_JSON at runtime. Signed direct HTTPS import completed without printing private values; the import authorizer is removed and endpoint closed (404). The old rejected stdout method is no longer the current blocker. Hosted publisher API authentication and a real Play transaction remain unverified. Never put private credentials in source or the deployment archive.

Design note: keep package `com.vickand.flyingladders`, catalog/prices, account contracts and other applications unchanged. Exact audience/email/signature verification remains mandatory. A notification only triggers revalidation; delivery alone does not grant an entitlement. No screenshot, store build upload, submission or advertising change was made.

Next connection actions are in `Docs/CLOUD_REMAINING_MANUAL_OCT20_RU.md`. Current source commit and honest activation flags are recorded in `Configs/account-backend.json`.
