# Account maintenance — design note, 21 October 2026

Add a single Cloud Code Maintenance endpoint to the existing module. It accepts only a non-player execution context provided by Unity (project/environment/service token required), takes no selectable player/path, and uses the existing Secret Manager URL/token and protected backend maintenance endpoint. The server receives fixed service identity maintenance plus the actual project/environment; players are rejected before any backend call. Errors propagate so Triggers can retry. No purchase/reward authority or account schema changes.

The dedicated production schedule emits com.unity.services.scheduler.cascade-account-maintenance.v1 every minute, bound only to CascadeAccount/Maintenance. Deploy these two files without --reconcile so unrelated jobs survive. Do not enable the schedule until the existing production connection token has been verified. A successful deployment/list is not proof of an actual run; require a successful hosted maintenance request or equivalent execution evidence.

Config: Configs/CloudServices/account-maintenance.sched and account-maintenance.tr. These are application backend jobs, not Codex reminders or chat automations. No native game UI or asset changes.
