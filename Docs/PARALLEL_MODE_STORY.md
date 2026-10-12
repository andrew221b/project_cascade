# Parallel skies: narrative memory across modes

Shapes, Words and Mixed remain independent puzzle journeys, inventories/receipts/rewards and attempts unchanged. Characters share narrative memory: automatic story gates consult the active and archived journey story receipts. Existing saves require no migration and no schema changes.

On an actual mode switch after some story has been heard, show a short destination-specific parallel-sky conversation. A first visit and the first return have different lines. Receipts 9000–9002 (arrival) and 9010–9012 (return) are new, disjoint narrative-only IDs in the existing validated range. They grant no rewards. Subsequent tab switches do not repeat these conversations. The very first mode selection without story experience still gets the normal introduction. Switching and save failure handling remain atomic; failed saves do not show an arrival.

EN/RU/UK source and shipped localization tables contain the new copy. No build/install requested.
