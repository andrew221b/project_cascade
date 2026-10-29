CREATE TABLE `accounts` (
	`player` text PRIMARY KEY NOT NULL,
	`version` integer NOT NULL,
	`payload` text NOT NULL
);
--> statement-breakpoint
CREATE TABLE `delete_codes` (
	`hash` text PRIMARY KEY NOT NULL,
	`player` text NOT NULL,
	`expires` integer NOT NULL
);
--> statement-breakpoint
CREATE TABLE `deletions` (
	`owner` text PRIMARY KEY NOT NULL,
	`player` text NOT NULL,
	`created` integer NOT NULL,
	`done` integer DEFAULT 0 NOT NULL
);
--> statement-breakpoint
CREATE TABLE `inbox` (
	`id` text PRIMARY KEY NOT NULL,
	`store` text NOT NULL,
	`payload` text NOT NULL,
	`created` integer NOT NULL,
	`done` integer DEFAULT 0 NOT NULL
);
--> statement-breakpoint
CREATE TABLE `owners` (
	`key` text PRIMARY KEY NOT NULL,
	`owner` text NOT NULL,
	`product` text NOT NULL,
	`bundle` text NOT NULL
);
--> statement-breakpoint
CREATE TABLE `rewards` (
	`id` text PRIMARY KEY NOT NULL,
	`player` text NOT NULL,
	`kind` text NOT NULL,
	`subject` text NOT NULL,
	`unit` text NOT NULL,
	`created` integer NOT NULL,
	`expires` integer NOT NULL,
	`transaction` text,
	`confirmed` integer DEFAULT 0 NOT NULL
);
--> statement-breakpoint
CREATE UNIQUE INDEX `rewards_transaction_unique` ON `rewards` (`transaction`);--> statement-breakpoint
CREATE TABLE `subscriptions` (
	`chain` text PRIMARY KEY NOT NULL,
	`player` text NOT NULL,
	`store` text NOT NULL,
	`product` text NOT NULL,
	`proof` text NOT NULL,
	`expires` integer NOT NULL,
	`observed` integer NOT NULL,
	`generation` integer DEFAULT 0 NOT NULL
);
--> statement-breakpoint
CREATE TABLE `tombstones` (
	`owner` text PRIMARY KEY NOT NULL,
	`created` integer NOT NULL
);
