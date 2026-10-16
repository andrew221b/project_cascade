# CASCADE story revision — 7 October 2026

This revision expands all nine world introductions from three to four short comic pages. World receipts remain `6000–6008`; island openings, island payoffs, level asides and checkpoints keep their existing receipt formulas. No persistence identifiers, puzzle rules, packages or save schemas change. The original three pages retain their atlas positions. The fourth page uses the separate fourth-scene atlas supplied by the presentation work; the content model gains no frame field.

Mika's journey begins with a concrete promise: carry the surviving light to Gran at the Great Beacon. Helping each island makes that road usable and gives Mika people to rely on. She first asks someone else to test a bridge, then takes responsibility for checking whether a repair actually helps its residents. She learns to make room for fear, to ask for company, and finally to share a keeper's work with Gran. The homecoming resolves that promise. Star Observatory remains a voluntary epilogue after a rest, where Mika offers another traveller the help she received.

| World | Resident and personal stake | Movement within the chapter |
| --- | --- | --- |
| Wind Gardens | Bram knows the bridges by their boards and joints; their damage feels like his own failed work. His bread jokes soften his worry. | He tests one repaired crossing instead of waiting for every bridge to be perfect. Mika asks for help without hiding her fear. |
| Rain Mill | Pip talks quickly and works too quickly. His little brother's empty bucket gives the repair a specific purpose. | He checks the far end of the water route, rather than mistaking a turning wheel for a finished job. |
| Copper Orchard | Rue knows roots, nests and seasons. She planted her first tree when she was small; its seeds matter as much as this year's fruit. | She opens the orchard without sacrificing tomorrow's growth; Mika carries a seed to Gran. |
| Cloud Post | Sora can deliver everyone else's words, but keeps postponing an apology to an old friend. | She writes the difficult first sentence and sends her own letter with the repaired post route. |
| Glass Lagoon | Neri keeps the boat and night route her mother taught her to use. She understands Mika's fear of deep water. | They restore a reachable landing first. Courage means asking someone to hold the boat. |
| Moss Archive | Toma counts pages and worries about the last map made by his teacher. | The original stays dry while a travelling copy gains useful notes. Preservation becomes sharing. |
| Snow Spire | Lumi remembers the mountain, but a guide's memory cannot uncover buried markers. | She accepts warmth and company; Mika can propose waiting instead of asking a guide to know everything. |
| Great Beacon | Gran is an experienced keeper with tired knees, a stubborn window lamp and soup to watch. | She lets Mika carry the lantern while she holds the rail. Mika names the people who made the homecoming possible. |
| Star Observatory | Aster records small details, worries over one wrong bearing and has stopped trusting an unfinished chart. | He marks what he can verify and leaves a second observer's stool ready. Mika becomes a companion rather than an all-knowing rescuer. |

Island openings name the specific obstacle or promise. Their payoffs show its effect on someone: a bucket filled, a letter sent, a nest found, a map annotated. Checkpoints and level asides develop the resident's worry or relationship with Mika; they avoid repeating puzzle instructions. Existing strong images, jokes and brief exchanges are retained.

New source keys and EN/RU/UK translations are supplied in `/private/tmp/cascade-story-copy.tsv`, with columns `key`, `en`, `ru`, `uk` and no header. Existing localization IDs remain owned by the table importer. Source-level checks cover page/line counts, array lengths and unchanged receipt formulas. Root owns localization import, native comic transitions, artwork correspondence and small-screen visual checks; this content pass does not run a phone build.

## Content validation

The three pure content files compile with the project's Unity-bundled C# compiler. A standalone source probe confirms four pages and two spoken lines per page in worlds 0–8, world receipts `6000–6008`, all 45 island opening/payoff pairs, 540 visit positions, nine existing level asides and 180 existing checkpoint positions. Introduction visits still have three lines; returning visits have two authored lines instead of the repeated generic instruction. No receipt selector or formula changed.

The handoff contains 115 unique source keys, each with nonempty EN/RU/UK text. Every new source string and translation is at most 90 characters. Resident, introduction, reply, goal, island, checkpoint and aside arrays retain their required lengths. Whitespace checks pass.

The existing frequency of story delivery is retained: one level aside per world and the existing checkpoint selector. This revision improves their spoken content without creating additional persistence receipts. Native layout, imported localization tables, new fourth-scene artwork and transitions are validated by root separately. This source pass has not run Unity PlayMode, a phone build or an external language review.

## Integrated result

The final integrated set has117 new EN/RU/UK keys, including two additional Mika clarifications. Pip's returning visit no longer claims an already repaired wheel is untouched, and the water checkpoint does not announce a delivery before its pipe is open. Five existing RU/UK keys now consistently address Mika as female, with published English keys and localization IDs retained. Native Unity content4/4 and comic flow4/4 passed;48 isolated captures cover every comic page and each resident's paper dialogue, including small RU/UK Large Text. Detailed evidence and limits are in STORY_PRESENTATION_OCT07.md. Original established identities are retained: Pip is the stocky green frog and Rue the older russet fox; four old illustration cells and the corresponding fourth scenes are bound from corrected sibling atlases to match their portraits.
