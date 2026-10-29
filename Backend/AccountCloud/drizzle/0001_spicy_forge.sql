CREATE INDEX `rewards_player` ON `rewards` (`player`);--> statement-breakpoint
CREATE UNIQUE INDEX `one_confirmed_parcel_reward` ON `rewards` (`player`,`kind`,`subject`) WHERE "rewards"."confirmed"=1 AND "rewards"."kind" IN ('parcel_open','parcel_bonus');--> statement-breakpoint
CREATE INDEX `subscriptions_player` ON `subscriptions` (`player`);