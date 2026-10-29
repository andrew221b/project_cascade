CREATE TABLE `server_credentials` (
	`id` text PRIMARY KEY NOT NULL,
	`sealed` text NOT NULL,
	`signer` text NOT NULL,
	`created` integer NOT NULL
);
