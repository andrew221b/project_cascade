# Menu return paths

Back returns to the page that opened the current page. The map's bag opens
Trophies; its Back returns to the bag, then the bag's Back returns to the map.
Opening Shop explicitly from a trophy returns to that same trophy and preserves
the bag's map origin. Owning or selecting a trophy does not open Shop.

A visit from Shop to the bag, tools, Trophies, or Workshop saves Shop's category,
selected item, and original entry page. Returning restores that visit. A link
to an already open parent resumes it, preventing Shop/bag and Shop/tools loops.
Coins belongs to the current Shop visit. Parcels opened in the journal or
Trophies return to their entry page; completion parcels retain the map flow.

These are presentation changes. Prices, inventory rules, save schemas, content
IDs, campaign attempts, and descent stages are unchanged. The bag uses the
existing localized Trophies label consistently.

Focused PlayMode coverage was added to NavigationResumeTests for map → bag →
Trophies → Back, explicit trophy → Shop → Back, buying a tool then visiting the
bag/tools, and Home → Shop → Coins/bag → Back. Each route also checks the pinned
attempt, stage, and prior clears. Execution results are recorded in TASKS.md;
these checks do not establish whether the game feels enjoyable.
