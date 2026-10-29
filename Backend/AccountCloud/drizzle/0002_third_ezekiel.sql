ALTER TABLE `deletions` ADD `legacy_ids` text DEFAULT '[]' NOT NULL;--> statement-breakpoint
ALTER TABLE `subscriptions` ADD `superseded_by` text;